﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Flagship.Config;
using Flagship.Enums;
using Flagship.Hit;
using Flagship.Logger;
using Flagship.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Flagship.Api
{
    internal class TrackingManager : ITrackingManager
    {
        public const string PROCESS_LOOKUP_HIT = "LOOKUP HIT";
        public const string HIT_DATA_LOADED = "Hits data has been loaded from database: {0}";

        private ConcurrentDictionary<string, HitAbstract> _hitsPoolQueue;
        private ConcurrentDictionary<string, Activate> _activatePoolQueue;
        protected virtual BatchingCachingStrategyAbstract Strategy { get; set; }
        public FlagshipConfig Config { get; set; }
        public HttpClient HttpClient { get; set; }
        public ConcurrentDictionary<string, HitAbstract> HitsPoolQueue
        {
            get => _hitsPoolQueue;
        }
        public ConcurrentDictionary<string, Activate> ActivatePoolQueue
        {
            get => _activatePoolQueue;
        }
        public TroubleshootingData TroubleshootingData
        {
            set { Strategy.TroubleshootingData = value; }
            get { return Strategy.TroubleshootingData; }
        }

        public string FlagshipInstanceId { get; set; }

        protected Timer _timer;
        protected bool _isPolling;

        public TrackingManager(
            FlagshipConfig config,
            HttpClient httpClient,
            string flagshipInstanceId = null
        )
        {
            FlagshipInstanceId = flagshipInstanceId;
            Config = config;
            HttpClient = httpClient;
            _hitsPoolQueue = new ConcurrentDictionary<string, HitAbstract>();
            _activatePoolQueue = new ConcurrentDictionary<string, Activate>();

            Strategy = InitStrategy();
            _ = LookupHitsAsync();
        }

        protected BatchingCachingStrategyAbstract InitStrategy()
        {
            BatchingCachingStrategyAbstract strategy;
            switch (Config.TrackingManagerConfig.CacheStrategy)
            {
                case CacheStrategy.PERIODIC_CACHING:
                    strategy = new BatchingPeriodicCachingStrategy(
                        Config,
                        HttpClient,
                        ref _hitsPoolQueue,
                        ref _activatePoolQueue
                    );
                    break;
                case CacheStrategy.NO_BATCHING:
                    strategy = new NoBatchingContinuousCachingStrategy(
                        Config,
                        HttpClient,
                        ref _hitsPoolQueue,
                        ref _activatePoolQueue
                    );
                    break;
                case CacheStrategy.CONTINUOUS_CACHING:
                default:
                    strategy = new BatchingContinuousCachingStrategy(
                        Config,
                        HttpClient,
                        ref _hitsPoolQueue,
                        ref _activatePoolQueue
                    );
                    break;
            }

            strategy.FlagshipInstanceId = FlagshipInstanceId;
            return strategy;
        }

        public virtual async Task Add(HitAbstract hit)
        {
            await Strategy.Add(hit).ConfigureAwait(false);
        }

        public virtual async Task ActivateFlag(Activate hit)
        {
            await Strategy.ActivateFlag(hit).ConfigureAwait(false);
        }

        public virtual async Task SendBatch(
            CacheTriggeredBy batchTriggeredBy = CacheTriggeredBy.BatchLength
        )
        {
            await Strategy.SendBatch(batchTriggeredBy).ConfigureAwait(false);
            await Strategy.SendTroubleshootingQueue().ConfigureAwait(false);
            await Strategy.SendUsageHitQueue().ConfigureAwait(false);
        }

        public void StartBatchingLoop()
        {
            var batchIntervals = Config.TrackingManagerConfig.BatchIntervals;
            Log.LogInfo(Config, "Batching Loop have been started", "startBatchingLoop");

            _timer?.Dispose();

            _timer = new Timer(
                async (e) =>
                {
                    await BatchingLoop().ConfigureAwait(false);
                },
                null,
                batchIntervals,
                batchIntervals
            );
        }

        public virtual async Task BatchingLoop()
        {
            if (_isPolling)
            {
                return;
            }

            _isPolling = true;
            await SendBatch(CacheTriggeredBy.Timer).ConfigureAwait(false);
            _isPolling = false;
        }

        public void StopBatchingLoop()
        {
            _timer?.Dispose();
            _isPolling = false;
            Log.LogInfo(Config, "Batching Loop have been stopped", "stopBatchingLoop");
        }

        protected bool CheckHitTime(DateTime time) =>
            (DateTime.Now - time).TotalSeconds <= Constants.DEFAULT_HIT_CACHE_TIME;

        protected virtual bool ChecKLookupHitData1(JToken item)
        {
            return item != null
                && item["version"].ToObject<int>() == 1
                && item["data"].ToObject<HitCacheData>() != null
                && item["data"]["type"] != null;
        }

        protected HitAbstract GetHitFromContent(JObject content)
        {
            HitAbstract hit = null;
            switch (content["type"].ToObject<HitType>())
            {
                case HitType.EVENT:
                    hit = content.ToObject<Event>();
                    break;
                case HitType.ITEM:
                    hit = content.ToObject<Item>();
                    break;
                case HitType.PAGEVIEW:
                    hit = content.ToObject<Page>();
                    break;
                case HitType.SCREENVIEW:
                    hit = content.ToObject<Screen>();
                    break;
                case HitType.TRANSACTION:
                    hit = content.ToObject<Transaction>();
                    break;
                case HitType.ACTIVATE:
                    hit = content.ToObject<Activate>();
                    break;
                case HitType.SEGMENT:
                    hit = content.ToObject<Segment>();
                    break;
            }
            hit.Config = Config;
            return hit;
        }

        public async Task LookupHitsAsync()
        {
            try
            {
                var hitCacheInstance = Config.HitCacheImplementation;
                if (hitCacheInstance == null || Config.DisableCache)
                {
                    return;
                }

                var hitsCache = await hitCacheInstance.LookupHits().ConfigureAwait(false);

                if (hitsCache == null)
                {
                    return;
                }

                Log.LogInfo(
                    Config,
                    string.Format(HIT_DATA_LOADED, JsonConvert.SerializeObject(hitsCache)),
                    PROCESS_LOOKUP_HIT
                );

                var wrongHitKeys = new List<string>();

                var jsonSerializer = new JsonSerializer
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                };

                foreach (var item in hitsCache)
                {
                    if (
                        !ChecKLookupHitData1(item.Value)
                        || !CheckHitTime(item.Value["data"]["time"].Value<DateTime>())
                    )
                    {
                        wrongHitKeys.Add(item.Key);
                        continue;
                    }

                    var hitCache = item.Value.ToObject<HitCacheDTOV1>();

                    var hit = GetHitFromContent((JObject)hitCache.Data.Content);
                    if (hit is Activate activate)
                    {
                        ActivatePoolQueue[activate.Key] = activate;
                    }
                    else
                    {
                        HitsPoolQueue.TryAdd(hit.Key, hit);
                    }
                }

                if (wrongHitKeys.Any())
                {
                    await Strategy.FlushHitsAsync(wrongHitKeys.ToArray()).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                Log.LogError(Config, ex.Message, PROCESS_LOOKUP_HIT);
            }
        }

        public virtual async Task SendTroubleshootingHit(Troubleshooting hit)
        {
            await Strategy.SendTroubleshootingHit(hit).ConfigureAwait(false);
        }

        public virtual async Task SendUsageHit(UsageHit hit)
        {
            await Strategy.SendUsageHit(hit).ConfigureAwait(false);
        }

        public virtual void AddTroubleshootingHit(Troubleshooting hit)
        {
            Strategy.AddTroubleshootingHit(hit);
        }
    }
}

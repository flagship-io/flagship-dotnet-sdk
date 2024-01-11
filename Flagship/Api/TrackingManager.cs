using Flagship.Config;
using Flagship.Enums;
using Flagship.FsVisitor;
using Flagship.Hit;
using Flagship.Logger;
using Flagship.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace Flagship.Api
{

    internal class TrackingManager : ITrackingManager
    {
        public const string PROCESS_LOOKUP_HIT = "LOOKUP HIT";
        public const string HIT_DATA_LOADED = "Hits data has been loaded from database: {0}";

        private Dictionary<string, HitAbstract> _hitsPoolQueue;
        private Dictionary<string, Activate> _activatePoolQueue;
        protected virtual BatchingCachingStrategyAbstract Strategy { get; set; }
        public FlagshipConfig Config { get; set; }
        public HttpClient HttpClient { get; set; }
        public Dictionary<string, HitAbstract> HitsPoolQueue { get => _hitsPoolQueue; }
        public Dictionary<string, Activate> ActivatePoolQueue { get => _activatePoolQueue; }
        public TroubleshootingData TroubleshootingData { set { Strategy.TroubleshootingData = value; } }

        public string FlagshipInstanceId { get; set; }

        protected Timer _timer;
        protected bool _isPolling;


        public TrackingManager(FlagshipConfig config, HttpClient httpClient)
        {
            Config = config;
            HttpClient = httpClient;
            _hitsPoolQueue = new Dictionary<string, HitAbstract>();
            _activatePoolQueue = new Dictionary<string, Activate>();
            Strategy = InitStrategy();
            _ = LookupHitsAsync();
        }

        protected BatchingCachingStrategyAbstract InitStrategy()
        {
            BatchingCachingStrategyAbstract strategy;
            switch (Config.TrackingManagerConfig.CacheStrategy)
            {
                case CacheStrategy.PERIODIC_CACHING:
                    strategy = new BatchingPeriodicCachingStrategy(Config, HttpClient, ref _hitsPoolQueue, ref _activatePoolQueue);
                    break;
                case CacheStrategy.NO_BATCHING:
                    strategy = new NoBatchingContinuousCachingStrategy(Config, HttpClient, ref _hitsPoolQueue, ref _activatePoolQueue);
                    break;
                case CacheStrategy.CONTINUOUS_CACHING:
                default:
                    strategy = new BatchingContinuousCachingStrategy(Config, HttpClient, ref _hitsPoolQueue, ref _activatePoolQueue);
                    break;
            }

            strategy.FlagshipInstanceId = FlagshipInstanceId;
            return strategy;
        }

        public virtual async Task Add(HitAbstract hit)
        {
            await Strategy.Add(hit);
        }

        public virtual async Task ActivateFlag(Activate hit)
        {
            await Strategy.ActivateFlag(hit);
        }

        public virtual async Task SendBatch(CacheTriggeredBy batchTriggeredBy = CacheTriggeredBy.BatchLength)
        {
            await Strategy.SendBatch(batchTriggeredBy);
            await Strategy.SendTroubleshootingQueue();
            await Strategy.SendAnalyticQueue();
        }

        public void StartBatchingLoop()
        {
            var batchIntervals = Config.TrackingManagerConfig.BatchIntervals;
            Log.LogInfo(Config, "Batching Loop have been started", "startBatchingLoop");

            if (_timer != null)
            {
                _timer.Dispose();
            }

            _timer = new Timer(async (e) =>
            {
                await BatchingLoop();
            }, null, batchIntervals, batchIntervals);
        }

        public virtual async Task BatchingLoop()
        {
            if (_isPolling)
            {
                return;
            }

            _isPolling = true;
            await SendBatch(CacheTriggeredBy.Timer);
            _isPolling = false;
        }

        public void StopBatchingLoop()
        {

            if (_timer != null)
            {
                _timer.Dispose();
            }
            _isPolling = false;
            Log.LogInfo(Config, "Batching Loop have been stopped", "stopBatchingLoop");
        }

        protected bool CheckHitTime(DateTime time) => (DateTime.Now - time).TotalSeconds <= Constants.DEFAULT_HIT_CACHE_TIME;

        protected virtual bool ChecKLookupHitData1(JToken item)
        {
            return item != null && item["version"].ToObject<int>() == 1 && item["data"].ToObject<HitCacheData>() != null && item["data"]["type"] != null;
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

                var hitsCache = await hitCacheInstance.LookupHits();

                if (hitsCache == null)
                {
                    return;
                }

                Log.LogInfo(Config, string.Format(HIT_DATA_LOADED, JsonConvert.SerializeObject(hitsCache)), PROCESS_LOOKUP_HIT);

                var wrongHitKeys = new List<string>();

                var jsonSerializer = new JsonSerializer
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                };

                foreach (var item in hitsCache)
                {
                    if (!ChecKLookupHitData1(item.Value) || !CheckHitTime(item.Value["data"]["time"].Value<DateTime>()))
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
                        HitsPoolQueue[hit.Key]= hit;
                    }

                }

                if (wrongHitKeys.Any())
                {
                    await Strategy.FlushHitsAsync(wrongHitKeys.ToArray());
                }
            }
            catch (Exception ex)
            {
                Log.LogError(Config, ex.Message, PROCESS_LOOKUP_HIT);
            }
        }

        public async Task SendTroubleshootingHit(Troubleshooting hit)
        {
            await Strategy.SendTroubleshootingHit(hit);
        }

        public async Task SendAnalyticHit(Analytic hit)
        {
            await Strategy.SendAnalyticHit(hit);
        }

        public void AddTroubleshootingHit(Troubleshooting hit)
        {
            Strategy.AddTroubleshootingHit(hit);
        }
    }
}

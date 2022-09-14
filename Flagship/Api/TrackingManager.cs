using Flagship.Config;
using Flagship.Enums;
using Flagship.FsVisitor;
using Flagship.Hit;
using Flagship.Logger;
using Flagship.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
        private BatchingCachingStrategyAbstract _strategy;
        public FlagshipConfig Config { get; set; }
        public HttpClient HttpClient { get; set; }
        public Dictionary<string, HitAbstract> HitsPoolQueue { get => _hitsPoolQueue; }
        protected Timer _timer;
        protected bool _isPolling;


        public TrackingManager(FlagshipConfig config, HttpClient httpClient)
        {
            Config = config;
            HttpClient = httpClient;
            _hitsPoolQueue = new Dictionary<string, HitAbstract>();
            _strategy = InitStrategy();
            _ = LookupHitsAsync();
        }

        protected BatchingCachingStrategyAbstract InitStrategy()
        {
            BatchingCachingStrategyAbstract strategy;
            switch (Config.TrackingMangerConfig.BatchStrategy)
            {
                case BatchStrategy.PERIODIC_CACHING:
                    strategy = new BatchingContinuousCachingStrategy(Config, HttpClient, ref _hitsPoolQueue);
                    break;
                case BatchStrategy.NO_BATCHING:
                    strategy = new BatchingContinuousCachingStrategy(Config, HttpClient, ref _hitsPoolQueue);
                    break;
                default:
                    strategy = new BatchingContinuousCachingStrategy(Config, HttpClient, ref _hitsPoolQueue);
                    break;
            }
            return strategy;
        }

        public void StartBatchingLoop()
        {
            var batchIntervals = Config.TrackingMangerConfig.BatchIntervals;
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

        public async Task BatchingLoop()
        {
            if (_isPolling)
            {
                return;
            }

            _isPolling = true;
            await _strategy.SendBatch();
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

        public Task Add(HitAbstract hit)
        {
            throw new NotImplementedException();
        }

        protected bool CheckHitTime(DateTime time) => (DateTime.Now - time).TotalSeconds <= Constants.DEFAULT_HIT_CACHE_TIME;

        protected virtual bool ChecKLookupHitData1(JToken item)
        {
            return item != null && item["Version"].ToObject<int>() == 1 && item["Data"] != null && item["Data"]["Type"] != null;
        }

        protected HitAbstract GetHitFromContent(JObject content)
        {
            HitAbstract hit = null;
            switch (content["Type"].ToObject<HitType>())
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
                case HitType.CONTEXT:
                    hit = content.ToObject<Segment>();
                    break;
            }

            return hit;
        }
        public async Task LookupHitsAsync()
        {
            try
            {
                var hitCacheInstance = Config?.HitCacheImplementation;
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

                foreach (var item in hitsCache)
                {
                    if (!ChecKLookupHitData1(item.Value) || !CheckHitTime(item.Value["Data"]["Time"].Value<DateTime>()))
                    {
                        wrongHitKeys.Add(item.Key);
                        continue;
                    }

                    var hitCache = item.Value.ToObject<HitCacheDTOV1>();

                    var hit = GetHitFromContent((JObject)hitCache.Data.Content);
                    HitsPoolQueue.Add(hit.Key, hit);
                }

                if (wrongHitKeys.Any())
                {
                    await _strategy.FlushHitsAsync(wrongHitKeys.ToArray());
                }
            }
            catch (Exception ex)
            {
                Log.LogError(Config, ex.Message, PROCESS_LOOKUP_HIT);
            }
            

        }
    }
}

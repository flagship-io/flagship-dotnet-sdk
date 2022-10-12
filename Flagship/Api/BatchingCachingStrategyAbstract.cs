using Flagship.Config;
using Flagship.Hit;
using Flagship.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Flagship.Api
{
    internal abstract class BatchingCachingStrategyAbstract : ITrackingManagerCommon
    {
        public const string PROCESS_CACHE_HIT = "CACHE HIT";
        public const string HIT_DATA_CACHED = "Hit data has been saved into database : {0}";
        public const string PROCESS_FLUSH_HIT = "FLUSH HIT";
        public const string HIT_DATA_FLUSHED = "The following hit keys have been flushed from database : {0}";
        public const string ADD_HIT = "ADD HIT";
        public const string HIT_ADDED_IN_QUEUE = "The hit has been added to the pool queue : {0}";
        public const string BATCH_SENT_SUCCESS = "Batch hit has been sent : {0}";
        public const string SEND_BATCH = "SEND BATCH";
        public const string SEND_ACTIVATE = "SEND ACTIVATE";
        public const string SEND_SEGMENT_HIT = "SEND SEGMENT HIT";
        public const string HIT_SENT_SUCCESS = "hit has been sent : {0}";
        public const string URL_ACTIVATE = "activate";
        public const string URL_EVENT = "events";

        public FlagshipConfig Config { get ; set ; }
        public HttpClient HttpClient { get; set; }

        public Dictionary<string,HitAbstract> HitsPoolQueue { get; set; }
        public Dictionary<string, Activate> ActivatePoolQueue { get; set; }


        public BatchingCachingStrategyAbstract(FlagshipConfig config, HttpClient httpClient, ref Dictionary<string, HitAbstract> hitsPoolQueue, ref Dictionary<string, Activate> activatePoolQueue)
        {
            Config = config;
            HttpClient = httpClient;
            HitsPoolQueue = hitsPoolQueue;
            ActivatePoolQueue = activatePoolQueue;
        }

        abstract public Task Add(HitAbstract hit);

        abstract public Task ActivateFlag(Activate hit);

        abstract protected Task SendActivate(ICollection<Activate> activateHitsPool, Activate currentActivate);

        abstract public Task SendBatch();  

        abstract public Task NotConsent(string visitorId);

        public async Task CacheHitAsync(Dictionary<string, HitAbstract> hits)
        {
            try
            {
                var hitCacheInstance = Config?.HitCacheImplementation;
                if (hitCacheInstance == null || Config.DisableCache)
                {
                    return;
                }

                var data = new JObject();
                foreach (var keyValue in hits)
                {
                    var hitData = new HitCacheDTOV1
                    {
                        Version = 1,
                        Data = new HitCacheData
                        {
                            AnonymousId = keyValue.Value.AnonymousId,
                            VisitorId = keyValue.Value.VisitorId,
                            Type = keyValue.Value.Type,
                            Content = keyValue.Value,
                            Time = DateTime.Now
                        }
                    };

                    data[keyValue.Key] = JObject.FromObject(hitData);
                }

                await hitCacheInstance.CacheHit(data);
                Logger.Log.LogInfo(Config, string.Format(HIT_DATA_CACHED, JsonConvert.SerializeObject(data)), PROCESS_CACHE_HIT);
            }
            catch (Exception ex)
            {
                Logger.Log.LogError(Config, ex.Message, PROCESS_CACHE_HIT);
            }
        }

        public async Task FlushHitsAsync(string[] hitKeys)
        {
            try
            {
                var hitCacheInstance = Config?.HitCacheImplementation;
                if (hitCacheInstance == null || Config.DisableCache)
                {
                    return;
                }
                await hitCacheInstance.FlushHits(hitKeys);
                Logger.Log.LogInfo(Config, string.Format(HIT_DATA_FLUSHED, JsonConvert.SerializeObject(hitKeys)), PROCESS_FLUSH_HIT);
            }
            catch (Exception ex)
            {
                Logger.Log.LogError(Config, ex.Message, PROCESS_FLUSH_HIT);
            }
        }
    }
}

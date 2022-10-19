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
        static public string PROCESS_CACHE_HIT = "CACHE HIT";
        static public string HIT_DATA_CACHED = "Hit data has been saved into database : {0}";
        static public string PROCESS_FLUSH_HIT = "FLUSH HIT";
        static public string HIT_DATA_FLUSHED = "The following hit keys have been flushed from database : {0}";
        static public string ADD_HIT = "ADD HIT";
        static public string HIT_ADDED_IN_QUEUE = "The hit has been added to the pool queue : {0}";
        static public string BATCH_SENT_SUCCESS = "Batch hit has been sent : {0}";
        static public string SEND_BATCH = "SEND BATCH";
        static public string SEND_HIT = "SEND HIT";
        static public string SEND_ACTIVATE = "SEND ACTIVATE";
        static public string SEND_SEGMENT_HIT = "SEND SEGMENT HIT";
        static public string HIT_SENT_SUCCESS = "hit has been sent : {0}";
        static public string URL_ACTIVATE = "activate";
        static public string URL_EVENT = "events";

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

        public virtual async Task CacheHitAsync(Dictionary<string, Activate> activatesHits)
        {
            var hit = new Dictionary<string, HitAbstract>();
            foreach (var item in activatesHits)
            {
                hit[item.Key] = item.Value;
            }
            await CacheHitAsync(hit);
        }

        public virtual async Task CacheHitAsync(Dictionary<string, HitAbstract> hits)
        {
            try
            {
                var hitCacheInstance = Config.HitCacheImplementation;
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

        public virtual async Task FlushHitsAsync(string[] hitKeys)
        {
            try
            {
                var hitCacheInstance = Config.HitCacheImplementation;
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

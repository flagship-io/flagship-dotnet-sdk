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
        public const string FS_CONSENT = "fs_consent";
        public const string ADD_HIT = "ADD HIT";
        public const string HIT_ADDED_IN_QUEUE = "The hit has been added to the pool queue : {0}";
        public FlagshipConfig Config { get ; set ; }
        public HttpClient HttpClient { get; set; }

        public Dictionary<string,HitAbstract> HitsPoolQueue { get; set; } 

        public BatchingCachingStrategyAbstract(FlagshipConfig config, HttpClient httpClient, ref Dictionary<string, HitAbstract> hitsPoolQueue)
        {
            Config = config;
            HttpClient = httpClient;
            HitsPoolQueue = hitsPoolQueue;
        }

        abstract public Task Add(HitAbstract hit); 
         
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

using Flagship.Config;
using Flagship.Enums;
using Flagship.Hit;
using Flagship.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
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
        static public string FLUSH_ALL_HITS = "All hits have been flushed from database";
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
        static public string STATUS_CODE = "StatusCode:";
        static public  string REASON_PHRASE = "ReasonPhrase";
        static public string RESPONSE = "response";
        static public string ITEM_DURATION = "duration";
        static public string ITEM_BATCH_TRIGGERED_BY = "batchTriggeredBy";

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

        virtual public async  Task ActivateFlag(Activate hit)
        {

            var hitKey = $"{hit.VisitorId}:{Guid.NewGuid()}";
            hit.Key = hitKey;
            var activateHitPool = new List<Activate>();
            if (ActivatePoolQueue.Any())
            {
                activateHitPool = ActivatePoolQueue.Values.ToList();
                var keys = activateHitPool.Select(x => x.Key);
                foreach (var item in keys)
                {
                    ActivatePoolQueue.Remove(item);
                }
            }

            await SendActivate(activateHitPool, hit, CacheTriggeredBy.ActivateLength);
        }

        abstract protected Task SendActivate(ICollection<Activate> activateHitsPool, Activate currentActivate, CacheTriggeredBy batchTriggeredBy);
        virtual public async Task SendBatch(CacheTriggeredBy batchTriggeredBy = CacheTriggeredBy.BatchLength)
        {
            if (ActivatePoolQueue.Any())
            {
                var activateHits = ActivatePoolQueue.Values.ToList();
                var keys = activateHits.Select(x => x.Key);
                foreach (var item in keys)
                {
                    ActivatePoolQueue.Remove(item);
                }
                await SendActivate(activateHits, null, batchTriggeredBy);
            }
            var batch = new Batch()
            {
                Config = Config
            };

            var hitKeysToRemove = new List<string>();

            foreach (var item in HitsPoolQueue)
            {
                if ((DateTime.Now - item.Value.CreatedAt).TotalMilliseconds >= Constants.DEFAULT_HIT_CACHE_TIME)
                {
                    hitKeysToRemove.Add(item.Key);
                    continue;
                }

                var batchSize = JsonConvert.SerializeObject(batch).Length;
                if (batchSize > Constants.BATCH_MAX_SIZE)
                {
                    break;
                }
                batch.Hits.Add(item.Value);
                hitKeysToRemove.Add(item.Key);
            }

            foreach (var key in hitKeysToRemove)
            {
                HitsPoolQueue.Remove(key);
            }

            if (!batch.Hits.Any())
            {
                return;
            }

            var requestBody = batch.ToApiKeys();
            var now = DateTime.Now;

            try
            {
                var requestMessage = new HttpRequestMessage(HttpMethod.Post, Constants.HIT_EVENT_URL);

                requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.HEADER_APPLICATION_JSON));

                var postDatajson = JsonConvert.SerializeObject(requestBody);

                var stringContent = new StringContent(postDatajson, Encoding.UTF8, Constants.HEADER_APPLICATION_JSON);

                requestMessage.Content = stringContent;

                var response = await HttpClient.SendAsync(requestMessage);

                if (response.StatusCode >= System.Net.HttpStatusCode.Ambiguous)
                {
                    var message = new Dictionary<string, object>()
                    {
                        {STATUS_CODE, response.StatusCode},
                        {REASON_PHRASE, response.ReasonPhrase },
                        {RESPONSE, await response.Content.ReadAsStringAsync() }
                    };

                    throw new Exception(JsonConvert.SerializeObject(message));
                }

                Logger.Log.LogDebug(Config, string.Format(BATCH_SENT_SUCCESS, JsonConvert.SerializeObject(new
                {
                    url = Constants.HIT_EVENT_URL,
                    body = requestBody,
                    duration = (DateTime.Now - now).TotalMilliseconds,
                    batchTriggeredBy = $"{batchTriggeredBy}"
                })), SEND_BATCH);

                await FlushHitsAsync(hitKeysToRemove.ToArray());
            }
            catch (Exception ex)
            {
                foreach (var item in batch.Hits)
                {
                    HitsPoolQueue[item.Key] = item;
                }
                Logger.Log.LogError(Config, Utils.Utils.ErrorFormat(ex.Message, new
                {
                    url = Constants.HIT_EVENT_URL,
                    body = requestBody,
                    duration = (DateTime.Now - now).TotalMilliseconds,
                    batchTriggeredBy = $"{batchTriggeredBy}"
                }), SEND_BATCH);
            }
        }

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
                var jsonSerializer = new JsonSerializer
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                };
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

                    data[keyValue.Key] = JObject.FromObject(hitData, jsonSerializer);
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

        public virtual async Task FlushAllHitsAsync()
        {
            try
            {
                var hitCacheInstance = Config.HitCacheImplementation;
                if (hitCacheInstance == null || Config.DisableCache)
                {
                    return;
                }
                await hitCacheInstance.FlushAllHits();
                Logger.Log.LogInfo(Config, FLUSH_ALL_HITS, PROCESS_FLUSH_HIT);
            }
            catch (Exception ex)
            {
                Logger.Log.LogError(Config, ex.Message, PROCESS_FLUSH_HIT);
            }
        }
    }
}

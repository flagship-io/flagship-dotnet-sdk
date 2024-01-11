using Flagship.Config;
using Flagship.Enums;
using Flagship.FsFlag;
using Flagship.FsVisitor;
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
        static public string ADD_TROUBELSHOOTING_HIT = "ADD TROUBLESHOOTING HIT";
        static public string ADD_ANALYTIC_HIT = "ADD ANALYTIC HIT"; 
        static public string HIT_ADDED_IN_QUEUE = "The hit has been added to the pool queue : {0}";
        static public string HIT_TROUBLESHOOTING_ADDED_IN_QUEUE = "The hit troubleshooting has been added to the pool queue : {0}";
        static public string HIT_ANALYTIC_ADDED_IN_QUEUE = "The hit analytic has been added to the pool queue : {0}";
        static public string BATCH_SENT_SUCCESS = "Batch hit has been sent : {0}";
        static public string SEND_BATCH = "SEND BATCH";
        static public string SEND_HIT = "SEND HIT";
        static public string SEND_ACTIVATE = "SEND ACTIVATE";
        static public string SEND_TROUBLESHOOTING = "SEND TROUBLESHOOTING";
        static public string SEND_ANALYTIC = "SEND ANALYTIC";
        static public string ON_VISITOR_EXPOSED = "ON_VISITOR_EXPOSED";
        static public string SEND_SEGMENT_HIT = "SEND SEGMENT HIT";
        static public string HIT_SENT_SUCCESS = "hit has been sent : {0}";
        static public string TROUBLESHOOTING_SENT_SUCCESS = "Troubleshooting hit has been sent : {0}";
        static public string ANALYTIC_SENT_SUCCESS = "Analytic hit has been sent : {0}";
        static public string URL_ACTIVATE = "activate";
        static public string URL_EVENT = "events";
        static public string STATUS_CODE = "StatusCode:";
        static public string REASON_PHRASE = "ReasonPhrase";
        static public string RESPONSE = "response";
        static public string ITEM_DURATION = "duration";
        static public string ITEM_BATCH_TRIGGERED_BY = "batchTriggeredBy";

        public FlagshipConfig Config { get ; set ; }
        public HttpClient HttpClient { get; set; }

        public Dictionary<string,HitAbstract> HitsPoolQueue { get; set; }
        public Dictionary<string, Activate> ActivatePoolQueue { get; set; }
        public Dictionary<string, Troubleshooting> TroubleshootingQueue { get; set; } 
        public Dictionary<string, Analytic> AnalyticQueue { get; set; }
        bool _isAnalyticQueueSending;

        public TroubleshootingData TroubleshootingData { get; set; }

        bool _isTroubleshootingQueueSending;

        public string FlagshipInstanceId { get; set; }

        public BatchingCachingStrategyAbstract(FlagshipConfig config, HttpClient httpClient, ref Dictionary<string, HitAbstract> hitsPoolQueue, ref Dictionary<string, Activate> activatePoolQueue)
        {
            Config = config;
            HttpClient = httpClient;
            HitsPoolQueue = hitsPoolQueue;
            ActivatePoolQueue = activatePoolQueue;
            TroubleshootingQueue = new Dictionary<string, Troubleshooting>();
            AnalyticQueue = new Dictionary<string, Analytic>();
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

        protected void OnVisitorExposed(Activate activate)
        {
            var fromFlag = new ExposedFlag(activate.FlagKey, activate.FlagValue, activate.FlagDefaultValue, activate.FlagMetadata);
            var exposedVisitor = new ExposedVisitor(activate.VisitorId, activate.AnonymousId, activate.VisitorContext);
            try
            {
                Config.InvokeOnVisitorExposed(exposedVisitor, fromFlag);
            }
            catch (Exception ex)
            {
                Logger.Log.LogError(Config, Utils.Utils.ErrorFormat(ex.Message, new
                {
                    fromFlag,
                    exposedVisitor
                }), ON_VISITOR_EXPOSED);
            }
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

                var troubleshooting = new Troubleshooting()
                {
                    Label= DiagnosticLabel.SEND_BATCH_HIT_ROUTE_RESPONSE_ERROR,
                    LogLevel= LogLevel.ERROR,
                    VisitorId = FlagshipInstanceId,
                    FlagshipInstanceId = FlagshipInstanceId,
                    Traffic = 0,
                    Config = Config,
                    HttpRequestUrl = Constants.HIT_EVENT_URL,
                    HttpsRequestBody = requestBody,
                    HttpResponseBody = ex.Message,
                    HttpResponseMethod = "POST",
                    HttpResponseTime = (int?)(DateTime.Now - now).TotalMilliseconds,
                    BatchTriggeredBy = batchTriggeredBy
                };

                _ = SendTroubleshootingHit(troubleshooting);
            }
        }

        public virtual async Task NotConsent(string visitorId)
        {
            var hitKeys = HitsPoolQueue.Where(x => !(x.Value is Event eventHit && eventHit.Action == Constants.FS_CONSENT) &&
            (x.Value.VisitorId == visitorId || x.Value.AnonymousId == visitorId)).Select(x => x.Key).ToArray();

            var activateKeys = ActivatePoolQueue.Where(x => x.Value.VisitorId == visitorId || x.Value.AnonymousId == visitorId).Select(x => x.Key).ToArray();

            foreach (var item in hitKeys)
            {
                HitsPoolQueue.Remove(item);
            }

            foreach (var item in activateKeys)
            {
                ActivatePoolQueue.Remove(item);
            }

            var keysToFlush = new List<string>(hitKeys);

            keysToFlush.AddRange(activateKeys);

            if (!keysToFlush.Any())
            {
                return;
            }
            await FlushHitsAsync(keysToFlush.ToArray());
        }

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

        public virtual bool IsTroubleshootingActivated()
        {
            if (TroubleshootingData == null)
            {
                return false;
            }

            var now = DateTime.Now.ToUniversalTime();

            var isStarted = now >= TroubleshootingData.StartDate;
            if (!isStarted)
            {
                return false;
            }

            var isFinished = now > TroubleshootingData.EndDate;

            if (isFinished)
            {
                return true;
            }

            return true;
        }

        public void AddTroubleshootingHit(Troubleshooting hit)
        {
            if (string.IsNullOrWhiteSpace(hit.Key))
            {
                hit.Key = $"{hit.VisitorId}:{Guid.NewGuid()}";
            }

            TroubleshootingQueue[hit.Key] = hit;

            Logger.Log.LogDebug(Config, string.Format(HIT_TROUBLESHOOTING_ADDED_IN_QUEUE, JsonConvert.SerializeObject(hit.ToApiKeys())), ADD_TROUBELSHOOTING_HIT);
        }

        public async virtual Task SendTroubleshootingHit(Troubleshooting hit)
        {
            if (!IsTroubleshootingActivated())
            {
                return;
            }

            if (TroubleshootingData.Traffic < hit.Traffic )
            {
                return;
            }

            var url = Constants.TROUBLESHOOTING_HIT_URL;
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);

            requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.HEADER_APPLICATION_JSON));

            var requestBody = hit.ToApiKeys();
            var now = DateTime.Now;

            try
            {
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

                Logger.Log.LogDebug(Config, string.Format(TROUBLESHOOTING_SENT_SUCCESS, JsonConvert.SerializeObject(new
                {
                    url,
                    headers = new Dictionary<string, string>(),
                    body = requestBody,
                    duration = (DateTime.Now - now).TotalMilliseconds
                })), SEND_TROUBLESHOOTING);

                if (!string.IsNullOrWhiteSpace(hit.Key))
                {
                    TroubleshootingQueue.Remove(hit.Key);
                }
            }
            catch (Exception ex)
            {
                if (IsTroubleshootingActivated())
                {
                    AddTroubleshootingHit(hit); ;
                }

                Logger.Log.LogError(Config, Utils.Utils.ErrorFormat(ex.Message, new
                {
                    url,
                    headers = new Dictionary<string, string>(),
                    body = requestBody,
                    duration = (DateTime.Now - now).TotalMilliseconds,
                }), SEND_TROUBLESHOOTING);
            }
        }

        public async  Task SendTroubleshootingQueue()
        {
            if (!IsTroubleshootingActivated() || _isTroubleshootingQueueSending || TroubleshootingQueue.Count == 0)
            {
                return; 
            }

            _isTroubleshootingQueueSending = true;

            foreach (var item in TroubleshootingQueue)
            {
                await SendTroubleshootingHit(item.Value);
            }

            _isTroubleshootingQueueSending = false;
        }

        #region Analytic

        public void AddAnalyticHit(Analytic hit) 
        {
            if (!IsTroubleshootingActivated())
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(hit.Key))
            {
                hit.Key = $"{hit.VisitorId}:{Guid.NewGuid()}";
            }

            AnalyticQueue[hit.Key] = hit;

            Logger.Log.LogDebug(Config, string.Format(HIT_ANALYTIC_ADDED_IN_QUEUE, JsonConvert.SerializeObject(hit.ToApiKeys())), ADD_ANALYTIC_HIT);
        }
        public async virtual Task SendAnalyticHit(Analytic hit)
        {
            var url = Constants.ANALYTICS_HIT_URL;
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);

            requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.HEADER_APPLICATION_JSON));

            var requestBody = hit.ToApiKeys();
            var now = DateTime.Now;

            try
            {
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

                Logger.Log.LogDebug(Config, string.Format(ANALYTIC_SENT_SUCCESS, JsonConvert.SerializeObject(new
                {
                    url,
                    headers = new Dictionary<string, string>(),
                    body = requestBody,
                    duration = (DateTime.Now - now).TotalMilliseconds
                })), SEND_ANALYTIC);

                if (!string.IsNullOrWhiteSpace(hit.Key))
                {
                    AnalyticQueue.Remove(hit.Key);
                }
            }
            catch (Exception ex)
            {
                AddAnalyticHit(hit);

                Logger.Log.LogError(Config, Utils.Utils.ErrorFormat(ex.Message, new
                {
                    url,
                    headers = new Dictionary<string, string>(),
                    body = requestBody,
                    duration = (DateTime.Now - now).TotalMilliseconds,
                }), SEND_ANALYTIC);
            }
        }

        public async Task SendAnalyticQueue() 
        {
            if (_isAnalyticQueueSending || AnalyticQueue.Count == 0)
            {
                return;
            }

            _isAnalyticQueueSending = true;

            foreach (var item in AnalyticQueue)
            {
                await SendAnalyticHit(item.Value);
            }

            _isAnalyticQueueSending = false;
        }
        #endregion
    }
}

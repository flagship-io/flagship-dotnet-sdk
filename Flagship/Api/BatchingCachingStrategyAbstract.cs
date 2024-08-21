using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Flagship.Config;
using Flagship.Enums;
using Flagship.FsFlag;
using Flagship.FsVisitor;
using Flagship.Hit;
using Flagship.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Flagship.Api
{
    internal abstract class BatchingCachingStrategyAbstract : ITrackingManagerCommon
    {
        public static string PROCESS_CACHE_HIT = "CACHE HIT";
        public static string HIT_DATA_CACHED = "Hit data has been saved into database : {0}";
        public static string PROCESS_FLUSH_HIT = "FLUSH HIT";
        public static string HIT_DATA_FLUSHED =
            "The following hit keys have been flushed from database : {0}";
        public static string FLUSH_ALL_HITS = "All hits have been flushed from database";
        public static string ADD_HIT = "ADD HIT";
        public static string ADD_TROUBELSHOOTING_HIT = "ADD TROUBLESHOOTING HIT";
        public static string ADD_ANALYTIC_HIT = "ADD ANALYTIC HIT";
        public static string HIT_ADDED_IN_QUEUE = "The hit has been added to the pool queue : {0}";
        public static string HIT_TROUBLESHOOTING_ADDED_IN_QUEUE =
            "The hit troubleshooting has been added to the pool queue : {0}";
        public static string HIT_ANALYTIC_ADDED_IN_QUEUE =
            "The hit analytic has been added to the pool queue : {0}";
        public static string BATCH_SENT_SUCCESS = "Batch hit has been sent : {0}";
        public static string SEND_BATCH = "SEND BATCH";
        public static string SEND_HIT = "SEND HIT";
        public static string SEND_ACTIVATE = "SEND ACTIVATE";
        public static string SEND_TROUBLESHOOTING = "SEND TROUBLESHOOTING";
        public static string SEND_TROUBLESHOOTING_QUEUE = "SEND TROUBLESHOOTING QUEUE";
        public static string SEND_USAGE_HIT = "SEND USAGE HIT";
        public static string SEND_USAGE_HIT_QUEUE = "SEND USAGE HIT QUEUE";
        public static string ON_VISITOR_EXPOSED = "ON_VISITOR_EXPOSED";
        public static string SEND_SEGMENT_HIT = "SEND SEGMENT HIT";
        public static string HIT_SENT_SUCCESS = "hit has been sent : {0}";
        public static string TROUBLESHOOTING_SENT_SUCCESS =
            "Troubleshooting hit has been sent : {0}";
        public static string USAGE_SENT_SUCCESS = "Usage hit has been sent : {0}";
        public static string URL_ACTIVATE = "activate";
        public static string URL_EVENT = "events";
        public static string STATUS_CODE = "StatusCode:";
        public static string REASON_PHRASE = "ReasonPhrase";
        public static string RESPONSE = "response";
        public static string ITEM_DURATION = "duration";
        public static string ITEM_BATCH_TRIGGERED_BY = "batchTriggeredBy";

        public FlagshipConfig Config { get; set; }
        public HttpClient HttpClient { get; set; }

        public ConcurrentDictionary<string, HitAbstract> HitsPoolQueue { get; set; }
        public ConcurrentDictionary<string, Activate> ActivatePoolQueue { get; set; }
        public ConcurrentDictionary<string, Troubleshooting> TroubleshootingQueue { get; set; }
        public ConcurrentDictionary<string, UsageHit> UsageHitQueue { get; set; }
        bool _isAnalyticQueueSending;

        public TroubleshootingData TroubleshootingData { get; set; }

        bool _isTroubleshootingQueueSending;

        public string FlagshipInstanceId { get; set; }

        public BatchingCachingStrategyAbstract(
            FlagshipConfig config,
            HttpClient httpClient,
            ref ConcurrentDictionary<string, HitAbstract> hitsPoolQueue,
            ref ConcurrentDictionary<string, Activate> activatePoolQueue
        )
        {
            Config = config;
            HttpClient = httpClient;
            HitsPoolQueue = hitsPoolQueue;
            ActivatePoolQueue = activatePoolQueue;
            TroubleshootingQueue = new ConcurrentDictionary<string, Troubleshooting>();
            UsageHitQueue = new ConcurrentDictionary<string, UsageHit>();
        }

        public abstract Task Add(HitAbstract hit);

        public virtual async Task ActivateFlag(Activate hit)
        {
            var hitKey = string.Format("{0}:{1}", hit.VisitorId, Guid.NewGuid());
            hit.Key = hitKey;
            var activateHitPool = new List<Activate>();
            lock (ActivatePoolQueue)
            {
                activateHitPool = ActivatePoolQueue
                    .Values.Take(Constants.BATCH_ACTIVATE_HIT_COUNT_LIMIT)
                    .ToList();
                var keys = activateHitPool.Select(x => x.Key).ToList();
                foreach (var item in keys)
                {
                    ActivatePoolQueue.TryRemove(item, out _);
                }
            }

            await SendActivate(activateHitPool, hit, CacheTriggeredBy.ActivateLength)
                .ConfigureAwait(false);
        }

        protected void OnVisitorExposed(Activate activate)
        {
            var fromFlag = new ExposedFlag(
                activate.FlagKey,
                activate.FlagValue,
                activate.FlagDefaultValue,
                activate.FlagMetadata
            );
            var exposedVisitor = new ExposedVisitor(
                activate.VisitorId,
                activate.AnonymousId,
                activate.VisitorContext
            );
            try
            {
                Config.InvokeOnVisitorExposed(exposedVisitor, fromFlag);
            }
            catch (Exception ex)
            {
                Logger.Log.LogError(
                    Config,
                    Utils.Helper.ErrorFormat(ex.Message, new { fromFlag, exposedVisitor }),
                    ON_VISITOR_EXPOSED
                );
            }
        }

        protected abstract Task SendActivate(
            ICollection<Activate> activateHitsPool,
            Activate currentActivate,
            CacheTriggeredBy batchTriggeredBy
        );

        public virtual async Task SendBatch(
            CacheTriggeredBy batchTriggeredBy = CacheTriggeredBy.BatchLength
        )
        {
            List<Activate> activateHits = new List<Activate>();
            try
            {
                lock (ActivatePoolQueue)
                {
                    activateHits = ActivatePoolQueue
                        .ToDictionary(entry => entry.Key, entry => entry.Value)
                        .Values.ToList();
                    var keys = activateHits.Select(x => x.Key);
                    foreach (var item in keys)
                    {
                        ActivatePoolQueue.TryRemove(item, out _);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log.LogError(
                    Config,
                    Utils.Helper.ErrorFormat(
                        ex.Message,
                        new
                        {
                            errorStackTrace = ex.StackTrace,
                            batchTriggeredBy = $"{batchTriggeredBy}",
                        }
                    ),
                    SEND_BATCH
                );

                var troubleshooting = new Troubleshooting()
                {
                    Label = DiagnosticLabel.ERROR_CATCHED,
                    LogLevel = LogLevel.ERROR,
                    VisitorId = FlagshipInstanceId,
                    FlagshipInstanceId = FlagshipInstanceId,
                    Traffic = 0,
                    Config = Config,
                    ErrorMessage = ex.Message,
                    ErrorStackTrace = ex.StackTrace,
                    BatchTriggeredBy = batchTriggeredBy,
                };

                _ = SendTroubleshootingHit(troubleshooting);
            }

            if (activateHits.Count > 0)
            {
                await SendActivate(activateHits, null, batchTriggeredBy).ConfigureAwait(false);
            }

            var batch = new Batch() { Config = Config };

            var hitKeysToRemove = new List<string>();
            try
            {
                lock (HitsPoolQueue)
                {
                    var HitsPoolQueueClone = HitsPoolQueue.ToDictionary(
                        entry => entry.Key,
                        entry => entry.Value
                    );
                    foreach (var item in HitsPoolQueueClone)
                    {
                        if (
                            (DateTime.Now - item.Value.CreatedAt).TotalMilliseconds
                            >= Constants.DEFAULT_HIT_CACHE_TIME
                        )
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
                        HitsPoolQueue.TryRemove(key, out _);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log.LogError(
                    Config,
                    Utils.Helper.ErrorFormat(
                        ex.Message,
                        new
                        {
                            errorStackTrace = ex.StackTrace,
                            batchTriggeredBy = $"{batchTriggeredBy}",
                        }
                    ),
                    SEND_BATCH
                );

                var troubleshooting = new Troubleshooting()
                {
                    Label = DiagnosticLabel.ERROR_CATCHED,
                    LogLevel = LogLevel.ERROR,
                    VisitorId = FlagshipInstanceId,
                    FlagshipInstanceId = FlagshipInstanceId,
                    Traffic = 0,
                    Config = Config,
                    ErrorMessage = ex.Message,
                    ErrorStackTrace = ex.StackTrace,
                    BatchTriggeredBy = batchTriggeredBy,
                };

                _ = SendTroubleshootingHit(troubleshooting);
            }

            if (!batch.Hits.Any())
            {
                await FlushHitsAsync(hitKeysToRemove.ToArray()).ConfigureAwait(false);
                return;
            }

            var requestBody = batch.ToApiKeys();
            var now = DateTime.Now;

            try
            {
                var requestMessage = new HttpRequestMessage(
                    HttpMethod.Post,
                    Constants.HIT_EVENT_URL
                );

                requestMessage.Headers.Accept.Add(
                    new MediaTypeWithQualityHeaderValue(Constants.HEADER_APPLICATION_JSON)
                );

                var postDataJson = JsonConvert.SerializeObject(requestBody);

                var stringContent = new StringContent(
                    postDataJson,
                    Encoding.UTF8,
                    Constants.HEADER_APPLICATION_JSON
                );

                requestMessage.Content = stringContent;

                var response = await HttpClient.SendAsync(requestMessage).ConfigureAwait(false);

                if (response.StatusCode >= System.Net.HttpStatusCode.Ambiguous)
                {
                    var message = new Dictionary<string, object>()
                    {
                        { STATUS_CODE, response.StatusCode },
                        { REASON_PHRASE, response.ReasonPhrase },
                        {
                            RESPONSE,
                            await response.Content.ReadAsStringAsync().ConfigureAwait(false)
                        },
                    };

                    throw new Exception(JsonConvert.SerializeObject(message));
                }

                Logger.Log.LogDebug(
                    Config,
                    string.Format(
                        BATCH_SENT_SUCCESS,
                        JsonConvert.SerializeObject(
                            new
                            {
                                url = Constants.HIT_EVENT_URL,
                                body = requestBody,
                                duration = (DateTime.Now - now).TotalMilliseconds,
                                batchTriggeredBy = $"{batchTriggeredBy}",
                            }
                        )
                    ),
                    SEND_BATCH
                );

                await FlushHitsAsync(hitKeysToRemove.ToArray()).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                foreach (var item in batch.Hits)
                {
                    HitsPoolQueue.TryAdd(item.Key, item);
                }

                Logger.Log.LogError(
                    Config,
                    Utils.Helper.ErrorFormat(
                        ex.Message,
                        new
                        {
                            url = Constants.HIT_EVENT_URL,
                            body = requestBody,
                            duration = (DateTime.Now - now).TotalMilliseconds,
                            batchTriggeredBy = $"{batchTriggeredBy}",
                        }
                    ),
                    SEND_BATCH
                );

                var troubleshooting = new Troubleshooting()
                {
                    Label = DiagnosticLabel.SEND_BATCH_HIT_ROUTE_RESPONSE_ERROR,
                    LogLevel = LogLevel.ERROR,
                    VisitorId = FlagshipInstanceId,
                    FlagshipInstanceId = FlagshipInstanceId,
                    Traffic = 0,
                    Config = Config,
                    HttpRequestUrl = Constants.HIT_EVENT_URL,
                    HttpsRequestBody = requestBody,
                    HttpResponseBody = ex.Message,
                    HttpResponseMethod = "POST",
                    HttpResponseTime = (int?)(DateTime.Now - now).TotalMilliseconds,
                    BatchTriggeredBy = batchTriggeredBy,
                };

                _ = SendTroubleshootingHit(troubleshooting);
            }
        }

        public virtual async Task NotConsent(string visitorId)
        {
            var hitKeysToRemove = new List<string>();
            var activateKeysToRemove = new List<string>();

            lock (HitsPoolQueue)
            {
                hitKeysToRemove.AddRange(
                    HitsPoolQueue
                        .Where(x =>
                            !(x.Value is Event eventHit && eventHit.Action == Constants.FS_CONSENT)
                            && (x.Value.VisitorId == visitorId || x.Value.AnonymousId == visitorId)
                        )
                        .Select(x => x.Key)
                );

                foreach (var item in hitKeysToRemove)
                {
                    HitsPoolQueue.TryRemove(item, out _);
                }
            }

            lock (ActivatePoolQueue)
            {
                activateKeysToRemove.AddRange(
                    ActivatePoolQueue
                        .Where(x =>
                            x.Value.VisitorId == visitorId || x.Value.AnonymousId == visitorId
                        )
                        .Select(x => x.Key)
                );

                foreach (var item in activateKeysToRemove)
                {
                    ActivatePoolQueue.TryRemove(item, out _);
                }
            }

            var keysToFlush = hitKeysToRemove.Concat(activateKeysToRemove).ToArray();

            if (keysToFlush.Length > 0)
            {
                await FlushHitsAsync(keysToFlush).ConfigureAwait(false);
            }
        }

        public virtual async Task CacheHitAsync(
            ConcurrentDictionary<string, Activate> activatesHits
        )
        {
            var hit = new ConcurrentDictionary<string, HitAbstract>();
            foreach (var item in activatesHits)
            {
                hit.TryAdd(item.Key, item.Value);
            }
            await CacheHitAsync(hit).ConfigureAwait(false);
        }

        public virtual async Task CacheHitAsync(ConcurrentDictionary<string, HitAbstract> hits)
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
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
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
                            Time = DateTime.Now,
                        },
                    };

                    data[keyValue.Key] = JObject.FromObject(hitData, jsonSerializer);
                }

                await hitCacheInstance.CacheHit(data).ConfigureAwait(false);

                Logger.Log.LogInfo(
                    Config,
                    string.Format(HIT_DATA_CACHED, JsonConvert.SerializeObject(data)),
                    PROCESS_CACHE_HIT
                );
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
                await hitCacheInstance.FlushHits(hitKeys).ConfigureAwait(false);
                Logger.Log.LogInfo(
                    Config,
                    string.Format(HIT_DATA_FLUSHED, JsonConvert.SerializeObject(hitKeys)),
                    PROCESS_FLUSH_HIT
                );
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
                await hitCacheInstance.FlushAllHits().ConfigureAwait(false);
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
                return false;
            }

            return true;
        }

        public virtual void AddTroubleshootingHit(Troubleshooting hit)
        {
            if (string.IsNullOrWhiteSpace(hit.Key))
            {
                hit.Key = $"{hit.VisitorId}:{Guid.NewGuid()}";
            }

            TroubleshootingQueue.TryAdd(hit.Key, hit);

            Logger.Log.LogDebug(
                Config,
                string.Format(
                    HIT_TROUBLESHOOTING_ADDED_IN_QUEUE,
                    JsonConvert.SerializeObject(hit.ToApiKeys())
                ),
                ADD_TROUBELSHOOTING_HIT
            );
        }

        public virtual async Task SendTroubleshootingHit(Troubleshooting hit)
        {
            if (!IsTroubleshootingActivated())
            {
                return;
            }

            if (TroubleshootingData.Traffic < hit.Traffic)
            {
                return;
            }

            var url = Constants.TROUBLESHOOTING_HIT_URL;
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);

            requestMessage.Headers.Accept.Add(
                new MediaTypeWithQualityHeaderValue(Constants.HEADER_APPLICATION_JSON)
            );

            var requestBody = hit.ToApiKeys();
            var now = DateTime.Now;

            try
            {
                var postDataJson = JsonConvert.SerializeObject(requestBody);

                var stringContent = new StringContent(
                    postDataJson,
                    Encoding.UTF8,
                    Constants.HEADER_APPLICATION_JSON
                );

                requestMessage.Content = stringContent;

                var response = await HttpClient.SendAsync(requestMessage).ConfigureAwait(false);

                if (response.StatusCode >= System.Net.HttpStatusCode.Ambiguous)
                {
                    var message = new Dictionary<string, object>()
                    {
                        { STATUS_CODE, response.StatusCode },
                        { REASON_PHRASE, response.ReasonPhrase },
                        {
                            RESPONSE,
                            await response.Content.ReadAsStringAsync().ConfigureAwait(false)
                        },
                    };

                    throw new Exception(JsonConvert.SerializeObject(message));
                }

                Logger.Log.LogDebug(
                    Config,
                    string.Format(
                        TROUBLESHOOTING_SENT_SUCCESS,
                        JsonConvert.SerializeObject(
                            new
                            {
                                url,
                                headers = new Dictionary<string, string>(),
                                body = requestBody,
                                duration = (DateTime.Now - now).TotalMilliseconds,
                            }
                        )
                    ),
                    SEND_TROUBLESHOOTING
                );

                if (!string.IsNullOrWhiteSpace(hit.Key))
                {
                    TroubleshootingQueue.TryRemove(hit.Key, out _);
                }
            }
            catch (Exception ex)
            {
                if (IsTroubleshootingActivated())
                {
                    AddTroubleshootingHit(hit);
                    ;
                }

                Logger.Log.LogError(
                    Config,
                    Utils.Helper.ErrorFormat(
                        ex.Message,
                        new
                        {
                            url,
                            headers = new Dictionary<string, string>(),
                            body = requestBody,
                            duration = (DateTime.Now - now).TotalMilliseconds,
                        }
                    ),
                    SEND_TROUBLESHOOTING
                );
            }
        }

        public virtual async Task SendTroubleshootingQueue()
        {
            var troubleshootingQueueClone = new Dictionary<string, Troubleshooting>();
            try
            {
                lock (TroubleshootingQueue)
                {
                    troubleshootingQueueClone = TroubleshootingQueue.ToDictionary(
                        entry => entry.Key,
                        entry => entry.Value
                    );
                }
            }
            catch (Exception ex)
            {
                Logger.Log.LogError(
                    Config,
                    Utils.Helper.ErrorFormat(ex.Message, new { errorStackTrace = ex.StackTrace }),
                    SEND_TROUBLESHOOTING_QUEUE
                );

                var troubleshooting = new Troubleshooting()
                {
                    Label = DiagnosticLabel.ERROR_CATCHED,
                    LogLevel = LogLevel.ERROR,
                    VisitorId = FlagshipInstanceId,
                    FlagshipInstanceId = FlagshipInstanceId,
                    Traffic = 0,
                    Config = Config,
                    ErrorMessage = ex.Message,
                    ErrorStackTrace = ex.StackTrace,
                };

                _ = SendTroubleshootingHit(troubleshooting);
            }

            if (
                !IsTroubleshootingActivated()
                || _isTroubleshootingQueueSending
                || troubleshootingQueueClone.Count == 0
            )
            {
                return;
            }

            _isTroubleshootingQueueSending = true;

            foreach (var item in troubleshootingQueueClone)
            {
                await SendTroubleshootingHit(item.Value).ConfigureAwait(false);
            }

            _isTroubleshootingQueueSending = false;
        }

        #region Analytic

        public virtual void AddUsageHit(UsageHit hit)
        {
            if (string.IsNullOrWhiteSpace(hit.Key))
            {
                hit.Key = $"{hit.VisitorId}:{Guid.NewGuid()}";
            }

            UsageHitQueue.TryAdd(hit.Key, hit);

            Logger.Log.LogDebug(
                Config,
                string.Format(
                    HIT_ANALYTIC_ADDED_IN_QUEUE,
                    JsonConvert.SerializeObject(hit.ToApiKeys())
                ),
                ADD_ANALYTIC_HIT
            );
        }

        public virtual async Task SendUsageHit(UsageHit hit)
        {
            var url = Constants.USAGE_HIT_URL;
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);

            requestMessage.Headers.Accept.Add(
                new MediaTypeWithQualityHeaderValue(Constants.HEADER_APPLICATION_JSON)
            );

            var requestBody = hit.ToApiKeys();
            var now = DateTime.Now;

            try
            {
                var postDataJson = JsonConvert.SerializeObject(requestBody);

                var stringContent = new StringContent(
                    postDataJson,
                    Encoding.UTF8,
                    Constants.HEADER_APPLICATION_JSON
                );

                requestMessage.Content = stringContent;

                var response = await HttpClient.SendAsync(requestMessage).ConfigureAwait(false);

                if (response.StatusCode >= System.Net.HttpStatusCode.Ambiguous)
                {
                    var message = new Dictionary<string, object>()
                    {
                        { STATUS_CODE, response.StatusCode },
                        { REASON_PHRASE, response.ReasonPhrase },
                        {
                            RESPONSE,
                            await response.Content.ReadAsStringAsync().ConfigureAwait(false)
                        },
                    };

                    throw new Exception(JsonConvert.SerializeObject(message));
                }

                Logger.Log.LogDebug(
                    Config,
                    string.Format(
                        USAGE_SENT_SUCCESS,
                        JsonConvert.SerializeObject(
                            new
                            {
                                url,
                                headers = new Dictionary<string, string>(),
                                body = requestBody,
                                duration = (DateTime.Now - now).TotalMilliseconds,
                            }
                        )
                    ),
                    SEND_USAGE_HIT
                );

                if (!string.IsNullOrWhiteSpace(hit.Key))
                {
                    UsageHitQueue.TryRemove(hit.Key, out _);
                }
            }
            catch (Exception ex)
            {
                AddUsageHit(hit);

                Logger.Log.LogError(
                    Config,
                    Utils.Helper.ErrorFormat(
                        ex.Message,
                        new
                        {
                            url,
                            headers = new Dictionary<string, string>(),
                            body = requestBody,
                            duration = (DateTime.Now - now).TotalMilliseconds,
                        }
                    ),
                    SEND_USAGE_HIT
                );
            }
        }

        public virtual async Task SendUsageHitQueue()
        {
            var usageHitQueue = new Dictionary<string, UsageHit>();

            try
            {
                lock (UsageHitQueue)
                {
                    usageHitQueue = UsageHitQueue.ToDictionary(
                        entry => entry.Key,
                        entry => entry.Value
                    );
                }
            }
            catch (Exception ex)
            {
                Logger.Log.LogError(
                    Config,
                    Utils.Helper.ErrorFormat(ex.Message, new { errorStackTrace = ex.StackTrace }),
                    SEND_USAGE_HIT_QUEUE
                );

                var troubleshooting = new Troubleshooting()
                {
                    Label = DiagnosticLabel.ERROR_CATCHED,
                    LogLevel = LogLevel.ERROR,
                    VisitorId = FlagshipInstanceId,
                    FlagshipInstanceId = FlagshipInstanceId,
                    Traffic = 0,
                    Config = Config,
                    ErrorMessage = ex.Message,
                    ErrorStackTrace = ex.StackTrace,
                };

                _ = SendTroubleshootingHit(troubleshooting);
            }

            if (_isAnalyticQueueSending || usageHitQueue.Count == 0)
            {
                return;
            }

            _isAnalyticQueueSending = true;

            foreach (var item in usageHitQueue)
            {
                await SendUsageHit(item.Value).ConfigureAwait(false);
            }

            _isAnalyticQueueSending = false;
        }
        #endregion
    }
}

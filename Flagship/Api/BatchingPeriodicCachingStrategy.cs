using Flagship.Config;
using Flagship.Enums;
using Flagship.Hit;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Flagship.Api
{
    internal class BatchingPeriodicCachingStrategy : BatchingCachingStrategyAbstract
    {
        protected bool _isBatchSending;

        public BatchingPeriodicCachingStrategy(FlagshipConfig config, HttpClient httpClient, ref ConcurrentDictionary<string, HitAbstract> hitsPoolQueue, ref ConcurrentDictionary<string, Activate> activatePoolQueue) : base(config, httpClient, ref hitsPoolQueue, ref activatePoolQueue)
        {
            _isBatchSending = false;
        }

        public async override Task Add(HitAbstract hit)
        {

            var hitKey = $"{hit.VisitorId}:{Guid.NewGuid()}";
            hit.Key = hitKey;
            HitsPoolQueue.TryAdd(hitKey, hit);
            if (hit is Event eventHit && eventHit.Action == Constants.FS_CONSENT && eventHit.Label == $"{Constants.SDK_LANGUAGE}:{false}")
            {
                await NotConsent(hit.VisitorId).ConfigureAwait(false);
            }
            Logger.Log.LogDebug(Config, string.Format(HIT_ADDED_IN_QUEUE, JsonConvert.SerializeObject(hit.ToApiKeys())), ADD_HIT);

            lock (HitsPoolQueue)
            {
                if (HitsPoolQueue.Count >= Config.TrackingManagerConfig.PoolMaxSize)
                {
                    _ = SendBatch(CacheTriggeredBy.BatchLength);
                }
            }

        }

        protected async override Task SendActivate(ICollection<Activate> activateHitsPool, Activate currentActivate, CacheTriggeredBy batchTriggeredBy)
        {
            var url = Constants.BASE_API_URL + URL_ACTIVATE;
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);

            requestMessage.Headers.Add(Constants.HEADER_X_API_KEY, Config.ApiKey);
            requestMessage.Headers.Add(Constants.HEADER_X_SDK_CLIENT, Constants.SDK_LANGUAGE);
            requestMessage.Headers.Add(Constants.HEADER_X_SDK_VERSION, Constants.SDK_VERSION);
            requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.HEADER_APPLICATION_JSON));

            var activateBatch = new ActivateBatch(activateHitsPool, Config);

            if (currentActivate != null)
            {
                activateBatch.Hits.Add(currentActivate);
            }

            var requestBody = activateBatch.ToApiKeys();
            var now = DateTime.Now;

            try
            {
                var postDatajson = JsonConvert.SerializeObject(requestBody);

                var stringContent = new StringContent(postDatajson, Encoding.UTF8, Constants.HEADER_APPLICATION_JSON);

                requestMessage.Content = stringContent;

                var response = await HttpClient.SendAsync(requestMessage).ConfigureAwait(false);

                if (response.StatusCode >= System.Net.HttpStatusCode.Ambiguous)
                {
                    var message = new Dictionary<string, object>()
                    {
                        {STATUS_CODE, response.StatusCode},
                        {REASON_PHRASE, response.ReasonPhrase },
                        {RESPONSE, await response.Content.ReadAsStringAsync().ConfigureAwait(false) }
                    };

                    throw new Exception(JsonConvert.SerializeObject(message));
                }

                foreach (var item in activateBatch.Hits)
                {
                    OnVisitorExposed(item);
                }

                Logger.Log.LogDebug(Config, string.Format(HIT_SENT_SUCCESS, JsonConvert.SerializeObject(new
                {
                    url,
                    headers = new Dictionary<string, string>
                        {
                            {Constants.HEADER_X_API_KEY, Config.ApiKey},
                            {Constants.HEADER_X_SDK_CLIENT, Constants.SDK_LANGUAGE },
                            {Constants.HEADER_X_SDK_VERSION, Constants.SDK_VERSION }
                        },
                    body = requestBody,
                    duration = (DateTime.Now - now).TotalMilliseconds,
                    batchTriggeredBy = $"{batchTriggeredBy}"
                })), SEND_ACTIVATE);
            }
            catch (Exception ex)
            {
                foreach (var item in activateBatch.Hits)
                {
                    ActivatePoolQueue.TryAdd(item.Key, item);
                }

                Logger.Log.LogError(Config, Utils.Utils.ErrorFormat(ex.Message, new
                {
                    url,
                    headers = new Dictionary<string, string>
                        {
                            {Constants.HEADER_X_API_KEY, Config.ApiKey},
                            {Constants.HEADER_X_SDK_CLIENT, Constants.SDK_LANGUAGE },
                            {Constants.HEADER_X_SDK_VERSION, Constants.SDK_VERSION }
                        },
                    body = requestBody,
                    duration = (DateTime.Now - now).TotalMilliseconds,
                    batchTriggeredBy = $"{batchTriggeredBy}"
                }), SEND_ACTIVATE);


                var troubleshooting = new Troubleshooting()
                {
                    Label = DiagnosticLabel.SEND_ACTIVATE_HIT_ROUTE_ERROR,
                    LogLevel = LogLevel.ERROR,
                    VisitorId = FlagshipInstanceId,
                    FlagshipInstanceId = FlagshipInstanceId,
                    Traffic = 0,
                    Config = Config,
                    HttpRequestUrl = url,
                    HttpsRequestBody = requestBody,
                    HttpResponseBody = ex.Message,
                    HttpResponseMethod = "POST",
                    HttpResponseTime = (int?)(DateTime.Now - now).TotalMilliseconds,
                    BatchTriggeredBy = batchTriggeredBy
                };

                _ = SendTroubleshootingHit(troubleshooting);
            }
        }

        public async override Task SendBatch(CacheTriggeredBy batchTriggeredBy = CacheTriggeredBy.BatchLength)
        {
            var hasActivateHit = false;
            _isBatchSending = true;

            List<Activate> activateHits = new List<Activate>();

            try
            {
                lock (ActivatePoolQueue)
                {
                    activateHits = ActivatePoolQueue.ToDictionary(entry => entry.Key, entry => entry.Value).Values.ToList();
                    var keys = activateHits.Select(x => x.Key);
                    foreach (var item in keys)
                    {
                        ActivatePoolQueue.TryRemove(item, out _);
                    }
                }
            }
            catch (Exception ex)
            {

                Logger.Log.LogError(Config, Utils.Utils.ErrorFormat(ex.Message, new
                {
                    errorStackTrace = ex.StackTrace,
                    batchTriggeredBy = $"{batchTriggeredBy}"
                }), SEND_BATCH);

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
                    BatchTriggeredBy = batchTriggeredBy
                };

                _ = SendTroubleshootingHit(troubleshooting);
            }


            if (activateHits.Count > 0)
            {
                await SendActivate(activateHits, null, batchTriggeredBy).ConfigureAwait(false);
                hasActivateHit = true;
            }

            var batch = new Batch()
            {
                Config = Config
            };

            var hitKeysToRemove = new List<string>();

            try
            {
                lock (HitsPoolQueue)
                {
                    var HitsPoolQueueClone = HitsPoolQueue.ToDictionary(entry => entry.Key, entry => entry.Value);

                    foreach (var item in HitsPoolQueueClone)
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
                        HitsPoolQueue.TryRemove(key, out _);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log.LogError(Config, Utils.Utils.ErrorFormat(ex.Message, new
                {
                    errorStackTrace = ex.StackTrace,
                    batchTriggeredBy = $"{batchTriggeredBy}"
                }), SEND_BATCH);

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
                    BatchTriggeredBy = batchTriggeredBy
                };

                _ = SendTroubleshootingHit(troubleshooting);
            }


            if (!batch.Hits.Any())
            {
                if (hasActivateHit)
                {
                    await CacheHitAsync(ActivatePoolQueue).ConfigureAwait(false);
                }
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


                var response = await HttpClient.SendAsync(requestMessage).ConfigureAwait(false);

                if (response.StatusCode >= System.Net.HttpStatusCode.Ambiguous)
                {
                    var message = new Dictionary<string, object>()
                    {
                        {STATUS_CODE, response.StatusCode},
                        {REASON_PHRASE, response.ReasonPhrase },
                        {RESPONSE, await response.Content.ReadAsStringAsync().ConfigureAwait(false) }
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
            }
            catch (Exception ex)
            {
                foreach (var item in batch.Hits)
                {
                    HitsPoolQueue.TryAdd(item.Key, item);
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
                    BatchTriggeredBy = batchTriggeredBy
                };

                _ = SendTroubleshootingHit(troubleshooting);
            }

            var mergedQueue = new ConcurrentDictionary<string, HitAbstract>(HitsPoolQueue);
            foreach (var item in ActivatePoolQueue)
            {
                mergedQueue.TryAdd(item.Key, item.Value);
            }

            await FlushAllHitsAsync().ConfigureAwait(false);
            await CacheHitAsync(mergedQueue).ConfigureAwait(false);
            _isBatchSending = false;
        }
    }
}

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
    internal class NoBatchingContinuousCachingStrategy : BatchingCachingStrategyAbstract
    {

        readonly ConcurrentDictionary<string, string> _cacheHitKeys;
        public NoBatchingContinuousCachingStrategy(FlagshipConfig config, HttpClient httpClient, ref ConcurrentDictionary<string, HitAbstract> hitsPoolQueue, ref ConcurrentDictionary<string, Activate> activatePoolQueue) : base(config, httpClient, ref hitsPoolQueue, ref activatePoolQueue)
        {
            _cacheHitKeys = new ConcurrentDictionary<string, string>();
        }

        public override async Task Add(HitAbstract hit)
        {
            var hitKey = $"{hit.VisitorId}:{Guid.NewGuid()}";
            hit.Key = hitKey;

            if (hit is Event eventHit && eventHit.Action == Constants.FS_CONSENT && eventHit.Label == $"{Constants.SDK_LANGUAGE}:{false}")
            {
                await NotConsent(hit.VisitorId).ConfigureAwait(false);
            }

            await SendHit(hit).ConfigureAwait(false);
        }

        public async Task SendHit(HitAbstract hit)
        {

            var requestBody = hit.ToApiKeys();
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

                Logger.Log.LogDebug(Config, string.Format(HIT_SENT_SUCCESS, JsonConvert.SerializeObject(new
                {
                    url = Constants.HIT_EVENT_URL,
                    body = requestBody,
                    duration = (DateTime.Now - now).TotalMilliseconds,
                    batchTriggeredBy = $"{CacheTriggeredBy.DirectHit}"
                })), SEND_HIT);
            }
            catch (Exception ex)
            {
                if (!(hit is Event eventHit && eventHit.Action == Constants.FS_CONSENT))
                {
                    _cacheHitKeys.TryAdd(hit.Key, hit.VisitorId);
                }
                var hitDictionary = new ConcurrentDictionary<string, HitAbstract>();
                hitDictionary.TryAdd(hit.Key, hit);
                await CacheHitAsync(hitDictionary).ConfigureAwait(false);

                Logger.Log.LogError(Config, Utils.Utils.ErrorFormat(ex.Message, new
                {
                    url = Constants.HIT_EVENT_URL,
                    body = requestBody,
                    duration = (DateTime.Now - now).TotalMilliseconds,
                    batchTriggeredBy = $"{CacheTriggeredBy.DirectHit}"
                }), SEND_HIT);

                var troubleshooting = new Troubleshooting()
                {
                    Label = DiagnosticLabel.SEND_HIT_ROUTE_ERROR,
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
                    BatchTriggeredBy = CacheTriggeredBy.DirectHit
                };

                _ = SendTroubleshootingHit(troubleshooting);
            }
        }

        public async override Task NotConsent(string visitorId)
        {
            string[] hitKeysToRemove;
            string[] activateKeysToRemove;

            lock (HitsPoolQueue)
            {
                hitKeysToRemove = HitsPoolQueue.Where(x => !(x.Value is Event eventHit && eventHit.Action == Constants.FS_CONSENT) &&
                (x.Value.VisitorId == visitorId || x.Value.AnonymousId == visitorId)).Select(x => x.Key).ToArray();

                foreach (var item in hitKeysToRemove)
                {
                    HitsPoolQueue.TryRemove(item, out _);
                }
            }


            lock (ActivatePoolQueue)
            {
                activateKeysToRemove = ActivatePoolQueue.Where(x => (x.Value.VisitorId == visitorId || x.Value.AnonymousId == visitorId)).Select(x => x.Key).ToArray();


                foreach (var item in activateKeysToRemove)
                {
                    ActivatePoolQueue.TryRemove(item, out _);
                }
            }

            string[] visitorCacheKeys;
            lock (_cacheHitKeys)
            {
                visitorCacheKeys = _cacheHitKeys.Where(x => x.Value == visitorId).Select(x => x.Key).ToArray();
                foreach (var item in visitorCacheKeys)
                {
                    _cacheHitKeys.TryRemove(item, out _);
                }
            }

            var mergedKeys = new List<string>(hitKeysToRemove);
            mergedKeys.AddRange(visitorCacheKeys);
            mergedKeys.AddRange(activateKeysToRemove);

            if (!mergedKeys.Any())
            {
                return;
            }

            await FlushHitsAsync(mergedKeys.ToArray()).ConfigureAwait(false);
        }

        public async override Task ActivateFlag(Activate hit)
        {

            var hitKey = $"{hit.VisitorId}:{Guid.NewGuid()}";
            hit.Key = hitKey;
            var activateHitPool = new List<Activate>();

            await SendActivate(activateHitPool, hit, CacheTriggeredBy.ActivateLength).ConfigureAwait(false);
        }

        protected async override Task SendActivate(ICollection<Activate> activateHitsPool, Activate currentActivate, CacheTriggeredBy batchTriggeredBy)
        {
            var url = Constants.BASE_API_URL + URL_ACTIVATE;
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);

            requestMessage.Headers.Add(Constants.HEADER_X_API_KEY, Config.ApiKey);
            requestMessage.Headers.Add(Constants.HEADER_X_SDK_CLIENT, Constants.SDK_LANGUAGE);
            requestMessage.Headers.Add(Constants.HEADER_X_SDK_VERSION, Constants.SDK_VERSION);
            requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.HEADER_APPLICATION_JSON));

            var activateBatch = new ActivateBatch(activateHitsPool.ToList(), Config);

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

                var hitKeysToRemove = activateHitsPool.Select(x => x.Key).ToArray();
                if (hitKeysToRemove.Any())
                {
                    await FlushHitsAsync(hitKeysToRemove).ConfigureAwait(false);
                }

                foreach (var item in activateBatch.Hits)
                {
                    OnVisitorExposed(item);
                }

                requestBody[ITEM_DURATION] = (DateTime.Now - now).TotalMilliseconds;
                requestBody[ITEM_BATCH_TRIGGERED_BY] = $"{batchTriggeredBy}";
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
                })), SEND_ACTIVATE); ;
            }
            catch (Exception ex)
            {
                foreach (var item in activateBatch.Hits)
                {
                    _cacheHitKeys.TryAdd(item.Key, item.Key);
                }

                if (currentActivate != null)
                {
                    var hitDictionary = new ConcurrentDictionary<string, Activate>();
                    hitDictionary.TryAdd(currentActivate.Key, currentActivate);
                    await CacheHitAsync(hitDictionary).ConfigureAwait(false);
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
    }
}

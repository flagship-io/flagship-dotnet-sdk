using Flagship.Config;
using Flagship.Enums;
using Flagship.Hit;
using Newtonsoft.Json;
using System;
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

        readonly Dictionary<string, string> _cacheHitKeys;
        public NoBatchingContinuousCachingStrategy(FlagshipConfig config, HttpClient httpClient, ref Dictionary<string, HitAbstract> hitsPoolQueue, ref Dictionary<string, Activate> activatePoolQueue) : base(config, httpClient, ref hitsPoolQueue, ref activatePoolQueue)
        {
            _cacheHitKeys = new Dictionary<string, string>();
        }

        public override async Task Add(HitAbstract hit)
        {
            var hitKey = $"{hit.VisitorId}:{Guid.NewGuid()}";
            hit.Key = hitKey;

            if (hit is Event eventHit && eventHit.Action == Constants.FS_CONSENT && eventHit.Label == $"{Constants.SDK_LANGUAGE}:{false}")
            {
                await NotConsent(hit.VisitorId);
            }

            await SendHit(hit);
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
                    _cacheHitKeys[hit.Key] = hit.Key;
                }

                await CacheHitAsync(new Dictionary<string, HitAbstract>() { { hit.Key, hit } });

                Logger.Log.LogError(Config, Utils.Utils.ErrorFormat(ex.Message, new
                {
                    url = Constants.HIT_EVENT_URL,
                    body = requestBody,
                    duration = (DateTime.Now - now).TotalMilliseconds,
                    batchTriggeredBy = $"{CacheTriggeredBy.DirectHit}"
                }), SEND_BATCH);
            }
        }

        public async override Task NotConsent(string visitorId)
        {
            var keysHit = HitsPoolQueue.Where(x => !(x.Value is Event eventHit && eventHit.Action == Constants.FS_CONSENT) &&
            (x.Value.VisitorId==visitorId || x.Value.AnonymousId == visitorId)).Select(x => x.Key).ToArray();

            var keysActivate = ActivatePoolQueue.Where(x => (x.Value.VisitorId == visitorId || x.Value.AnonymousId == visitorId)).Select(x => x.Key).ToArray();

            foreach (var item in keysHit)
            {
                HitsPoolQueue.Remove(item);
            }

            foreach (var item in keysActivate)
            {
                ActivatePoolQueue.Remove(item);
            }

            var mergedKeys = new List<string>(keysHit);
            mergedKeys.AddRange(_cacheHitKeys.Keys);
            mergedKeys.AddRange(keysActivate);

            if (!mergedKeys.Any())
            {
                return;
            }
            
            await FlushHitsAsync(mergedKeys.ToArray());
        }

        public async override Task ActivateFlag(Activate hit)
        {

            var hitKey = $"{hit.VisitorId}:{Guid.NewGuid()}";
            hit.Key = hitKey;
            var activateHitPool = new List<Activate>();
          
            await SendActivate(activateHitPool, hit, CacheTriggeredBy.ActivateLength);
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

                var hitKeysToRemove = activateHitsPool.Select(x => x.Key).ToArray();
                if (hitKeysToRemove.Any())
                {
                    await FlushHitsAsync(hitKeysToRemove);
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
                    _cacheHitKeys[item.Key] = item.Key;
                }

                if (currentActivate != null)
                {
                    await CacheHitAsync(new Dictionary<string, HitAbstract>() { { currentActivate.Key, currentActivate } });
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
            }
        }
    }
}

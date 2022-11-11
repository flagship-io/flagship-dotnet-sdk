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

                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    var message = new Dictionary<string, object>()
                    {
                        {"StatusCode:", response.StatusCode},
                        {"ReasonPhrase", response.ReasonPhrase },
                        {"response", await response.Content.ReadAsStringAsync() }
                    };

                    throw new Exception(JsonConvert.SerializeObject(message));
                }

                requestBody["duration"] = (DateTime.Now - now).TotalMilliseconds;
                requestBody["batchTriggeredBy"] = $"{CacheTriggeredBy.DirectHit}";
                Logger.Log.LogDebug(Config, string.Format(HIT_SENT_SUCCESS, JsonConvert.SerializeObject(requestBody)), SEND_HIT);
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

            var keysActivate = ActivatePoolQueue.Where(x => x.Value.VisitorId == visitorId).Select(x => x.Key).ToArray();

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
          
            await SendActivate(activateHitPool, hit);
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

                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    var message = new Dictionary<string, object>()
                    {
                        {"StatusCode:", response.StatusCode},
                        {"ReasonPhrase", response.ReasonPhrase },
                        {"response", await response.Content.ReadAsStringAsync() }
                    };

                    throw new Exception(JsonConvert.SerializeObject(message));
                }

                var hitKeysToRemove = activateHitsPool.Select(x => x.Key).ToArray();
                if (hitKeysToRemove.Any())
                {
                    await FlushHitsAsync(hitKeysToRemove);
                }

                requestBody["duration"] = (DateTime.Now - now).TotalMilliseconds;
                requestBody["batchTriggeredBy"] = $"{batchTriggeredBy}";
                Logger.Log.LogDebug(Config, string.Format(HIT_SENT_SUCCESS, postDatajson), SEND_ACTIVATE);
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


        public async override Task SendBatch(CacheTriggeredBy batchTriggeredBy = CacheTriggeredBy.BatchLength)
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

                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    var message = new Dictionary<string, object>()
                    {
                        {"StatusCode:", response.StatusCode},
                        {"ReasonPhrase", response.ReasonPhrase },
                        {"response", await response.Content.ReadAsStringAsync() }
                    };

                    throw new Exception(JsonConvert.SerializeObject(message));
                }

                requestBody["duration"] = (DateTime.Now - now).TotalMilliseconds;
                requestBody["batchTriggeredBy"] = $"{batchTriggeredBy}";

                Logger.Log.LogDebug(Config, string.Format(BATCH_SENT_SUCCESS, JsonConvert.SerializeObject(requestBody)), SEND_BATCH);

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
                    body = requestBody
                }), SEND_BATCH);
            }
        }
    }
}

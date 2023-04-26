using Flagship.Config;
using Flagship.Enums;
using Flagship.Hit;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Flagship.Api
{
    internal class BatchingContinuousCachingStrategy : BatchingCachingStrategyAbstract
    {
        public BatchingContinuousCachingStrategy(FlagshipConfig config, HttpClient httpClient, ref Dictionary<string, HitAbstract> hitsPoolQueue, ref Dictionary<string, Activate> activatePoolQueue) : base(config, httpClient, ref hitsPoolQueue, ref activatePoolQueue)
        {
        }

        public async override Task Add(HitAbstract hit)
        {

            var hitKey = $"{hit.VisitorId}:{Guid.NewGuid()}";
            hit.Key = hitKey;

            HitsPoolQueue[hitKey] = hit;

            await CacheHitAsync(new Dictionary<string, HitAbstract>() { { hitKey, hit } });

            if (hit is Event eventHit && eventHit.Action == Constants.FS_CONSENT && eventHit.Label == $"{Constants.SDK_LANGUAGE}:{false}")
            {
                await NotConsent(hit.VisitorId);
            }
            Logger.Log.LogDebug(Config, string.Format(HIT_ADDED_IN_QUEUE, JsonConvert.SerializeObject(hit.ToApiKeys())), ADD_HIT);

            if (HitsPoolQueue.Count>= Config.TrackingMangerConfig.PoolMaxSize)
            {
                _ = SendBatch(CacheTriggeredBy.BatchLength);
            }
        }

        public override async Task NotConsent(string visitorId)
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


        override protected async Task SendActivate(ICollection<Activate> activateHitsPool, Activate currentActivate, CacheTriggeredBy batchTriggeredBy)
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
                    ActivatePoolQueue[item.Key] = item;
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

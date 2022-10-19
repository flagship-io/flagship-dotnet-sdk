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
    internal class BatchingPeriodicCachingStrategy : BatchingCachingStrategyAbstract
    {
        public BatchingPeriodicCachingStrategy(FlagshipConfig config, HttpClient httpClient, ref Dictionary<string, HitAbstract> hitsPoolQueue, ref Dictionary<string, Activate> activatePoolQueue) : base(config, httpClient, ref hitsPoolQueue, ref activatePoolQueue)
        {
        }

        public async override Task Add(HitAbstract hit)
        {
            var hitKey = $"{hit.VisitorId}:{Guid.NewGuid()}";
            hit.Key = hitKey;
            HitsPoolQueue[hitKey] = hit;
            if (hit is Event eventHit && eventHit.Action == Constants.FS_CONSENT && eventHit.Label == $"{Constants.SDK_LANGUAGE}:{false}")
            {
                await NotConsent(hit.VisitorId);
            }
            Logger.Log.LogDebug(Config, string.Format(HIT_ADDED_IN_QUEUE, JsonConvert.SerializeObject(hit.ToApiKeys())), ADD_HIT);
        }

        public async override Task NotConsent(string visitorId)
        {
            var keys = HitsPoolQueue.Where(x => !(x.Value is Event eventHit && eventHit.Action == Constants.FS_CONSENT) &&
            x.Value.VisitorId == visitorId).Select(x => x.Key).ToList();

            foreach (var item in keys)
            {
                HitsPoolQueue.Remove(item);
            }
            if (!keys.Any())
            {
                return;
            }
            await CacheHitAsync(HitsPoolQueue);
        }


        public async override Task ActivateFlag(Activate hit)
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

            await SendActivate(activateHitPool, hit);

        }

        protected async override Task SendActivate(ICollection<Activate> activateHitsPool, Activate currentActivate)
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

                Logger.Log.LogDebug(Config, string.Format(HIT_SENT_SUCCESS, postDatajson), SEND_ACTIVATE);
            }
            catch (Exception ex)
            {
                foreach (var item in activateBatch.Hits)
                {
                    ActivatePoolQueue[item.Key] = item;
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
                    body = requestBody
                }), SEND_ACTIVATE);
            }
        }


        public async override Task SendBatch()
        {
            var hasActivateHit = false;

            if (ActivatePoolQueue.Any())
            {
                var activateHits = ActivatePoolQueue.Values.ToList();
                var keys = activateHits.Select(x => x.Key);
                foreach (var item in keys)
                {
                    ActivatePoolQueue.Remove(item);
                }
                await SendActivate(activateHits, null);
                hasActivateHit = true;
            }

            var batch = new Batch()
            {
                Config = Config
            };

            var count = 0;


            var hitKeysToRemove = new List<string>();

            foreach (var item in HitsPoolQueue)
            {
                count++;
                var batchSize = JsonConvert.SerializeObject(batch).Length;
                if (batchSize > Constants.BATCH_MAX_SIZE || count > Config.TrackingMangerConfig.BatchLength)
                {
                    break;
                }
                batch.Hits.Add(item.Value);
                hitKeysToRemove.Add(item.Key);
            }


            if (!batch.Hits.Any())
            {
                if (hasActivateHit)
                {
                    await CacheHitAsync(ActivatePoolQueue);
                }
                return;
            }

            foreach (var key in hitKeysToRemove)
            {
                HitsPoolQueue.Remove(key);
            }

            var requestBody = batch.ToApiKeys();

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

                Logger.Log.LogDebug(Config, string.Format(BATCH_SENT_SUCCESS, JsonConvert.SerializeObject(requestBody)), SEND_BATCH);
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

            var mergedQueue = new Dictionary<string, HitAbstract>(HitsPoolQueue);
            foreach (var item in ActivatePoolQueue)
            {
                mergedQueue[item.Key]= item.Value;
            }

            await CacheHitAsync(mergedQueue);
        }
    }
}

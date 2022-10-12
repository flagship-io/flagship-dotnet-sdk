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
using System.Threading.Tasks;

namespace Flagship.Api
{
    internal class BatchingContinuousCachingStrategy : BatchingCachingStrategyAbstract
    {
        public BatchingContinuousCachingStrategy(FlagshipConfig config, HttpClient httpClient, ref Dictionary<string, HitAbstract> hitsPoolQueue, ref Dictionary<string, Activate> activatePoolQueue) : base(config, httpClient, ref hitsPoolQueue, ref activatePoolQueue)
        {
        }

        public override  Task Add(HitAbstract hit)
        {
            return Task.Factory.StartNew(async () =>
            {
                var hitKey = $"{hit.VisitorId}:{Guid.NewGuid()}";
                hit.Key = hitKey;
                await AddHitWithKey(hitKey, hit);
                if (hit is Event eventHit && eventHit.Action == Constants.FS_CONSENT && eventHit.Label == $"{Constants.SDK_LANGUAGE}:false")
                {
                    await NotConsent(hit.VisitorId);
                }
                Logger.Log.LogDebug(Config, string.Format(HIT_ADDED_IN_QUEUE, JsonConvert.SerializeObject(hit.ToApiKeys())), ADD_HIT);
            });
            
        }

        protected async Task AddHitWithKey(string key, HitAbstract hit)
        {
            HitsPoolQueue[key] = hit;
            await CacheHitAsync(new Dictionary<string, HitAbstract>() { { key, hit } });
        }

        public override async Task NotConsent(string visitorId)
        {
            var keys = HitsPoolQueue.Where(x => !(x.Value is Event eventHit && eventHit.Action == Constants.FS_CONSENT) && x.Key.Contains(visitorId)).Select(x => x.Key);

            foreach (var item in keys)
            {
                HitsPoolQueue.Remove(item);
            }
            if (keys.Any())
            {
                return;
            }
            await FlushHitsAsync(keys.ToArray());
        }

        public override async Task SendBatch()
        {
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

                await HttpClient.SendAsync(requestMessage);

                Logger.Log.LogDebug(Config, string.Format(BATCH_SENT_SUCCESS, JsonConvert.SerializeObject(requestBody)), SEND_BATCH);

                await FlushHitsAsync(hitKeysToRemove.ToArray());
            }
            catch (Exception ex)
            {
                foreach (var item in batch.Hits)
                {
                    await AddHitWithKey(item.Key, item);
                }
                Logger.Log.LogError(Config, Utils.Utils.ErrorFormat(ex.Message, new
                {
                    url = Constants.HIT_EVENT_URL,
                    body = requestBody
                }), SEND_BATCH);
            }
        }

        public override Task ActivateFlag(Activate hit)
        {
            return Task.Factory.StartNew(async () =>
            {
                var hitKey = $"{hit.VisitorId}:{Guid.NewGuid()}";
                hit.Key = hitKey;
                var activateHitPool = new List<Activate>();
                if (ActivatePoolQueue.Count > 0)
                {
                    activateHitPool = ActivatePoolQueue.Values.ToList();
                    var keys = activateHitPool.Select(x => x.Key);
                    foreach (var item in keys)
                    {
                        ActivatePoolQueue.Remove(item); 
                    }
                }

                await SendActivate(activateHitPool, hit);
            });
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

                await HttpClient.SendAsync(requestMessage);

                var hitKeysToRemove = activateHitsPool.Select(x => x.Key).ToArray();
                if (hitKeysToRemove.Length > 0)
                {
                    await FlushHitsAsync(hitKeysToRemove);
                }

                Logger.Log.LogDebug(Config, string.Format(HIT_SENT_SUCCESS, postDatajson), SEND_ACTIVATE);
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
                    body = requestBody
                }), SEND_ACTIVATE);
            }
        }

    }

}

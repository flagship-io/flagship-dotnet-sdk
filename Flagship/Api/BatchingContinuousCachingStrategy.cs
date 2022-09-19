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
        public BatchingContinuousCachingStrategy(FlagshipConfig config, HttpClient httpClient, ref Dictionary<string, HitAbstract> hitsPoolQueue) : base(config, httpClient, ref hitsPoolQueue)
        {
        }

        public override async Task Add(HitAbstract hit)
        {
            var hitKey = $"{hit.VisitorId}:{Guid.NewGuid()}";
            hit.Key = hitKey;
            await AddHitWithKey(hitKey, hit);
            if (hit is Event eventHit && eventHit.Action == Constants.FS_CONSENT && eventHit.Label == $"{Constants.SDK_LANGUAGE}:false")
            {
                await NotConsent(hit.VisitorId);
            }
            Logger.Log.LogDebug(Config, string.Format(HIT_ADDED_IN_QUEUE, JsonConvert.SerializeObject(hit.ToApiKeys())), ADD_HIT);
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

        public override async Task SendActivateAndSegmentHit(IEnumerable<HitAbstract> hits)
        {
            var requestMessage = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
            };

            var sdkVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            requestMessage.Headers.Add(Constants.HEADER_X_API_KEY, Config.ApiKey);
            requestMessage.Headers.Add(Constants.HEADER_X_SDK_CLIENT, Constants.SDK_LANGUAGE);
            requestMessage.Headers.Add(Constants.HEADER_X_SDK_VERSION, sdkVersion);
            requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.HEADER_APPLICATION_JSON));

            var hitKeys = new List<string>();

            foreach (var hit in hits)
            {
                HitsPoolQueue.Remove(hit.Key);
                var requestBody = hit.ToApiKeys();
                var isActivateHit = hit.Type == HitType.ACTIVATE;
                var url = Constants.BASE_API_URL;
                url += isActivateHit ? "activate" : $"{Config.EnvId}/events";
                requestMessage.RequestUri = new Uri(url, UriKind.RelativeOrAbsolute);
                var tag = isActivateHit ? SEND_ACTIVATE : SEND_SEGMENT_HIT;

                try
                {
                    var postDatajson = JsonConvert.SerializeObject(requestBody);

                    var stringContent = new StringContent(postDatajson, Encoding.UTF8, Constants.HEADER_APPLICATION_JSON);

                    requestMessage.Content = stringContent;

                    await HttpClient.SendAsync(requestMessage);

                    hitKeys.Add(hit.Key);

                    Logger.Log.LogDebug(Config, string.Format(HIT_SENT_SUCCESS, JsonConvert.SerializeObject(requestBody)), tag);
                }
                catch (Exception ex)
                {
                    await AddHitWithKey(hit.Key, hit);
                    Logger.Log.LogError(Config, Utils.Utils.ErrorFormat(ex.Message, new
                    {
                        url,
                        headers = new Dictionary<string, string>
                        {
                            {Constants.HEADER_X_API_KEY, Config.ApiKey},
                            {Constants.HEADER_X_SDK_CLIENT, Constants.SDK_LANGUAGE },
                            {Constants.HEADER_X_SDK_VERSION, sdkVersion }
                        },
                        body = requestBody
                    }), tag);
                }
            }

            if (hitKeys.Any())
            {
                await FlushHitsAsync(hitKeys.ToArray());
            }
        }

        public override async Task SendBatch()
        {
            var batch = new Batch()
            {
                Config = Config
            };

            var count = 0;

            var activateAndSegmentHits = new List<HitAbstract>();

            var hitKeysToRemove = new List<string>();

            foreach (var item in HitsPoolQueue)
            {
                if (item.Value.Type == HitType.ACTIVATE || item.Value.Type == HitType.CONTEXT)
                {
                    activateAndSegmentHits.Add(item.Value);
                    continue;
                }
                count++;
                var batchSize = JsonConvert.SerializeObject(batch).Length;
                if (batchSize > Constants.BATCH_MAX_SIZE || count > Config.TrackingMangerConfig.BatchLength)
                {
                    break;
                }
                batch.Hits.Add(item.Value);
                hitKeysToRemove.Add(item.Key);
            }

            await SendActivateAndSegmentHit(activateAndSegmentHits);

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
    }
}

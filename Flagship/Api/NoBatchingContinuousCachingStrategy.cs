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
    internal class NoBatchingContinuousCachingStrategy : BatchingCachingStrategyAbstract
    {

        readonly Dictionary<string, string> _cacheHitKeys;
        public NoBatchingContinuousCachingStrategy(FlagshipConfig config, HttpClient httpClient, ref Dictionary<string, HitAbstract> hitsPoolQueue) : base(config, httpClient, ref hitsPoolQueue)
        {
            _cacheHitKeys = new Dictionary<string, string>();
        }

        public override async Task Add(HitAbstract hit)
        {
            var hitKey = $"{hit.VisitorId}:{Guid.NewGuid()}";
            hit.Key = hitKey;

            if (hit is Event eventHit && eventHit.Action == Constants.FS_CONSENT && eventHit.Label == $"{Constants.SDK_LANGUAGE}:false")
            {
                await NotConsent(hit.VisitorId);
            }

            await CacheHitAsync(new Dictionary<string, HitAbstract>() { { hitKey, hit } });

            if (hit.Type == HitType.ACTIVATE || hit.Type == HitType.CONTEXT)
            {
                await SendActivateAndSegmentHit(hit);
                return;
            }

            await SendHit(hit);
        }

        public async Task SendHit(HitAbstract hit)
        {

            var requestBody = hit.ToApiKeys();

            try
            {
                var requestMessage = new HttpRequestMessage(HttpMethod.Post, Constants.HIT_EVENT_URL);

                requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.HEADER_APPLICATION_JSON));

                var postDatajson = JsonConvert.SerializeObject(requestBody);

                var stringContent = new StringContent(postDatajson, Encoding.UTF8, Constants.HEADER_APPLICATION_JSON);

                requestMessage.Content = stringContent;

                await HttpClient.SendAsync(requestMessage);

                Logger.Log.LogDebug(Config, string.Format(HIT_SENT_SUCCESS, JsonConvert.SerializeObject(requestBody)), SEND_BATCH);

                await FlushHitsAsync(new string[] { hit.Key });
            }
            catch (Exception ex)
            {
                if (!(hit is Event eventHit && eventHit.Action == Constants.FS_CONSENT))
                {
                    _cacheHitKeys[hit.Key] = hit.Key;
                }
                Logger.Log.LogError(Config, Utils.Utils.ErrorFormat(ex.Message, new
                {
                    url = Constants.HIT_EVENT_URL,
                    body = requestBody
                }), SEND_BATCH);
            }
        }

        public async Task SendActivateAndSegmentHit(HitAbstract hit)
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

            var requestBody = hit.ToApiKeys();
            var isActivateHit = hit.Type == HitType.ACTIVATE;
            var url = Constants.BASE_API_URL;
            url += isActivateHit ? URL_ACTIVATE : $"{Config.EnvId}/{URL_EVENT}";
            requestMessage.RequestUri = new Uri(url, UriKind.RelativeOrAbsolute);
            var tag = isActivateHit ? SEND_ACTIVATE : SEND_SEGMENT_HIT;

            try
            {
                var postDatajson = JsonConvert.SerializeObject(requestBody);

                var stringContent = new StringContent(postDatajson, Encoding.UTF8, Constants.HEADER_APPLICATION_JSON);

                requestMessage.Content = stringContent;

                await HttpClient.SendAsync(requestMessage);

                await FlushHitsAsync(new string[] { hit.Key });

                Logger.Log.LogDebug(Config, string.Format(HIT_SENT_SUCCESS, JsonConvert.SerializeObject(requestBody)), tag);
            }
            catch (Exception ex)
            {
                _cacheHitKeys[hit.Key] = hit.Key;
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

        public async override Task NotConsent(string visitorId)
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
            var mergedKeys = new List<string>(keys);
            mergedKeys.AddRange(_cacheHitKeys.Keys);

            await FlushHitsAsync(mergedKeys.ToArray());
        }

        public async override Task SendActivateAndSegmentHits(IEnumerable<HitAbstract> hits)
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
                url += isActivateHit ? URL_ACTIVATE : $"{Config.EnvId}/{URL_EVENT}"; ;
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
                    HitsPoolQueue[hit.Key] = hit;
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

        public async override Task SendBatch()
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

            await SendActivateAndSegmentHits(activateAndSegmentHits);

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

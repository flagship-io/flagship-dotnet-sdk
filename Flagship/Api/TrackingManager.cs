using Flagship.Config;
using Flagship.Enums;
using Flagship.FsVisitor;
using Flagship.Hit;
using Flagship.Logger;
using Flagship.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;


namespace Flagship.Api
{

    internal class TrackingManager : ITrackingManager
    {
        public FlagshipConfig Config { get; set; }
        public HttpClient HttpClient { get; set; }

        public TrackingManager(FlagshipConfig config, HttpClient httpClient)
        {
            Config = config;
            HttpClient = httpClient;
        }

        public async Task SendActive(VisitorDelegateAbstract visitor, FlagDTO flag)
        {

            try
            {
                var url = $"{Constants.BASE_API_URL}activate";
                var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);

                requestMessage.Headers.Add(Constants.HEADER_X_API_KEY, Config.ApiKey);
                requestMessage.Headers.Add(Constants.HEADER_X_SDK_CLIENT, Constants.SDK_LANGUAGE);
                requestMessage.Headers.Add(Constants.HEADER_X_SDK_VERSION, Constants.SDK_VERSION);
                requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.HEADER_APPLICATION_JSON));


                var postData = new Dictionary<string, object>
                {
                    [Constants.VISITOR_ID_API_ITEM] = visitor.VisitorId,
                    [Constants.VARIATION_ID_API_ITEM] = flag.VariationId,
                    [Constants.VARIATION_GROUP_ID_API_ITEM] = flag.VariationGroupId,
                    [Constants.CUSTOMER_ENV_ID_API_ITEM] = Config.EnvId
                };

                if (!string.IsNullOrWhiteSpace(visitor.AnonymousId) && !string.IsNullOrWhiteSpace(visitor.VisitorId))
                {
                    postData[Constants.VISITOR_ID_API_ITEM] = visitor.VisitorId;
                    postData[Constants.ANONYMOUS_ID] = visitor.AnonymousId;
                }
                else
                {
                    postData[Constants.VISITOR_ID_API_ITEM] = visitor.AnonymousId ?? visitor.VisitorId;
                    postData[Constants.ANONYMOUS_ID] = null;
                }

                var postDatajson = JsonConvert.SerializeObject(postData);

                var stringContent = new StringContent(postDatajson, Encoding.UTF8, Constants.HEADER_APPLICATION_JSON);

                requestMessage.Content = stringContent;

                var response = await HttpClient.SendAsync(requestMessage);
            }
            catch (Exception ex)
            {
                Log.LogError(Config, ex.Message, "SendActive");
            }

        }

        public async Task SendHit(HitAbstract hit)
        {
            try
            {
                var requestMessage = new HttpRequestMessage(HttpMethod.Post, Constants.HIT_API_URL);
                var postDatajson = JsonConvert.SerializeObject(hit.ToApiKeys());

                var stringContent = new StringContent(postDatajson, Encoding.UTF8, Constants.HEADER_APPLICATION_JSON);

                requestMessage.Content = stringContent;

                var response = await HttpClient.SendAsync(requestMessage);
            }
            catch (Exception ex)
            {
                Log.LogError(Config, ex.Message, "SendHit");
            }


        }
    }
}

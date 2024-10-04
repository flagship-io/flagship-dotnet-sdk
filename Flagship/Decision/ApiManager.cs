using Flagship.Api;
using Flagship.Config;
using Flagship.Enums;
using Flagship.FsVisitor;
using Flagship.Hit;
using Flagship.Logger;
using Flagship.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;


namespace Flagship.Decision
{
    internal class ApiManager : DecisionManager
    {
        public ApiManager(FlagshipConfig config, HttpClient httpClient) : base(config, httpClient)
        {
        }

        public async override Task<ICollection<Campaign>> GetCampaigns(VisitorDelegateAbstract visitor)
        {

            var postData = new Dictionary<string, object>
            {
                ["visitorId"] = visitor.VisitorId,
                ["anonymousId"] = visitor.AnonymousId,
                ["trigger_hit"] = false,
                ["context"] = visitor.Context,
                ["visitor_consent"] = visitor.HasConsented
            };

            var now = DateTime.Now;

                var url = $"{Constants.BASE_API_URL}{Config.EnvId}/campaigns?exposeAllKeys=true&extras[]=accountSettings";
            try
            {
                var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);

                requestMessage.Headers.Add(Constants.HEADER_X_API_KEY, Config.ApiKey);
                requestMessage.Headers.Add(Constants.HEADER_X_SDK_CLIENT, Constants.SDK_LANGUAGE);
                requestMessage.Headers.Add(Constants.HEADER_X_SDK_VERSION, Constants.SDK_VERSION);
                requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.HEADER_APPLICATION_JSON));
                
                var postDatajson = JsonConvert.SerializeObject(postData);

                var stringContent = new StringContent(postDatajson, Encoding.UTF8, "application/json");

                requestMessage.Content = stringContent;

                var response = await HttpClient.SendAsync(requestMessage).ConfigureAwait(false);

                if (response.StatusCode!= System.Net.HttpStatusCode.OK)
                {
                    throw new Exception(response.ReasonPhrase);
                }   

                string responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                var decisionResponse = JsonConvert.DeserializeObject<DecisionResponse>(responseBody, new IsoDateTimeConverter { DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.fffZ" });

                
                IsPanic = decisionResponse.Panic;

                TroubleshootingData = decisionResponse?.Extras?.AccountSettings?.Troubleshooting;

                return decisionResponse.Campaigns;
            }
            catch (Exception ex)
            {
                Log.LogError(Config, ex.Message, "GetCampaigns");

                var troubleshooting = new Troubleshooting()
                {
                    Label = DiagnosticLabel.GET_CAMPAIGNS_ROUTE_RESPONSE_ERROR,
                    LogLevel = LogLevel.ERROR,
                    VisitorId = visitor.VisitorId,
                    AnonymousId = visitor.AnonymousId,
                    FlagshipInstanceId = visitor.SdkInitialData?.InstanceId,
                    Traffic = 0,
                    Config = Config,
                    HttpRequestUrl = url,
                    HttpsRequestBody = postData,
                    HttpResponseBody = ex.Message,
                    HttpResponseMethod = "POST",
                    HttpResponseTime = (int?)(DateTime.Now - now).TotalMilliseconds
                };

                TrackingManager?.AddTroubleshootingHit(troubleshooting);

                return new Collection<Campaign>();
            }
        }
    }
}

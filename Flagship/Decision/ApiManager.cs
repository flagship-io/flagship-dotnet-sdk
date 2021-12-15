using Flagship.Config;
using Flagship.Enums;
using Flagship.FsVisitor;
using Flagship.Model;
using Newtonsoft.Json;
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
            
            try
            {
                
                var url = $"{Constants.BASE_API_URL}{Config.EnvId}/campaigns?exposeAllKeys=true";
                if (!visitor.HasConsented)
                {
                    url += $"&{Constants.SEND_CONTEXT_EVENT}=false";
                }

                var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);

                requestMessage.Headers.Add(Constants.HEADER_X_API_KEY, Config.ApiKey);
                requestMessage.Headers.Add(Constants.HEADER_X_SDK_CLIENT, Constants.SDK_LANGUAGE);
                requestMessage.Headers.Add(Constants.HEADER_X_SDK_VERSION, Constants.SDK_VERSION);
                requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.HEADER_APPLICATION_JSON));
                
               
                var postData = new Dictionary<string, object>
                {
                    ["visitorId"] = visitor.VisitorId,
                    ["anonymousId"] = visitor.AnonymousId,
                    ["trigger_hit"] = false,
                    ["context"] = visitor.Context
                };

                var postDatajson = JsonConvert.SerializeObject(postData);

                var stringContent = new StringContent(postDatajson, Encoding.UTF8, "application/json");

                requestMessage.Content = stringContent;

                var response = await HttpClient.SendAsync(requestMessage);

                if (response.StatusCode!= System.Net.HttpStatusCode.OK)
                {
                    throw new Exception(response.ReasonPhrase);
                }   

                string responseBody = await response.Content.ReadAsStringAsync();

                var decisionResponse = JsonConvert.DeserializeObject<DecisionResponse>(responseBody);

                IsPanic = decisionResponse.Panic;

                return decisionResponse.Campaigns;

            }
            catch (Exception ex)
            {
                Utils.Log.LogError(Config, ex.Message, "GetCampaigns");
                return new Collection<Campaign>();
            }
        }
    }
}

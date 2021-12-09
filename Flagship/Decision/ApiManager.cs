using Flagship.Config;
using Flagship.Enum;
using Flagship.FsVisitor;
using Flagship.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Flagship.Decision
{
    public class ApiManager : DecisionManager
    {
        public ApiManager(HttpClient httpClient, FlagshipConfig config) : base(httpClient, config)
        {
        }

        public async override Task<ICollection<Campaign>> GetCampaigns(VisitorDelegateAbstract visitor)
        {
            try
            {
                HttpClient.DefaultRequestHeaders.Clear();
                HttpClient.DefaultRequestHeaders.Add(Constants.HEADER_X_API_KEY, Config.ApiKey);
                HttpClient.DefaultRequestHeaders.Add(Constants.HEADER_X_SDK_CLIENT, Constants.SDK_LANGUAGE);
                HttpClient.DefaultRequestHeaders.Add(Constants.HEADER_X_SDK_VERSION, Constants.SDK_VERSION);
                HttpClient.DefaultRequestHeaders.Add(Constants.HEADER_CONTENT_TYPE, Constants.HEADER_APPLICATION_JSON);

                var postData = new Dictionary<string, object>
                {
                    ["visitorId"] = visitor.VisitorId,
                    ["anonymousId"] = visitor.AnonymousId,
                    ["trigger_hit"] = false,
                    ["context"] = visitor.Context
                };


                var postDatajson = JsonConvert.SerializeObject(postData);

                var stringContent = new StringContent(postDatajson, Encoding.UTF8, "application/json");

                var url = $"{Constants.BASE_API_URL}{Config.EnvId}/campaigns?exposeAllKeys=true";

                if (!visitor.HasConsented)
                {
                    url += $"&{Constants.SEND_CONTEXT_EVENT}=false"; 
                }

                var response = await HttpClient.PostAsync(url, stringContent);

                string responseBody = await response.Content.ReadAsStringAsync();

                var decisionResponse = JsonConvert.DeserializeObject<DecisionResponse>(responseBody);

                IsPanic = decisionResponse.Panic;

                return decisionResponse.Campaigns;

            }
            catch (Exception ex)
            {
                Utils.Utils.LogError(Config, ex.Message, "GetCampaigns");
                return new Collection<Campaign>();
            }
        }
    }
}

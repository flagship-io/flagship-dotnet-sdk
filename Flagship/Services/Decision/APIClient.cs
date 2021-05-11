using Flagship.Model;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Flagship.Services.Decision
{
    public sealed class APIDecisionManager : IDecisionManager, IDisposable
    {
        private const string DecisionApiUrl = "https://decision.flagship.io";
        private readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);

        private readonly string environmentId;
        private readonly HttpClient httpClient;

        public APIDecisionManager(FlagshipContext flagshipContext)
        {
            httpClient = new HttpClient
            {
                Timeout = flagshipContext.Options?.Timeout ?? DefaultTimeout
            };
            httpClient.DefaultRequestHeaders.Add("x-api-key", flagshipContext.ApiKey);
            environmentId = flagshipContext.EnvironmentId;
        }

        public async Task<DecisionResponse> GetResponse(DecisionRequest request)
        {
            var json = JsonConvert.SerializeObject(request);
            using (var stringContent = new StringContent(json, Encoding.UTF8, "application/json"))
            {
                HttpResponseMessage response = await httpClient.PostAsync($"{DecisionApiUrl}/v2/{environmentId}/campaigns?exposeAllKeys=true", stringContent);
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<DecisionResponse>(responseBody);
            }
        }

        public void Dispose()
        {
            if (httpClient != null)
            {
                httpClient.Dispose();
            };
        }
    }
}

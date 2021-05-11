using Flagship.Model;
using Flagship.Model.Bucketing;
using Flagship.Services.Bucketing;
using Flagship.Services.Logger;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using Flagship.Services.HitSender;
using Flagship.Model.Logs;

namespace Flagship.Services.Decision
{
    public class BucketingClient : IDecisionManager, IDisposable
    {
        private const string BucketingApiUrl = "https://cdn.flagship.io";
        private readonly TimeSpan DefaultPollingInterval = TimeSpan.FromMinutes(1);

        private readonly string environmentId;
        private readonly HttpClient httpClient;
        private Configuration configuration;
        private readonly ILogger logger;
        private readonly ISender sender;
        private readonly TargetingMatch targetingMatch;
        private readonly VariationAllocation variationAllocation;
        private readonly Timer loadTimer;

        public BucketingClient(FlagshipContext flagshipContext)
        {
            environmentId = flagshipContext.EnvironmentId;
            logger = flagshipContext.Logger;
            sender = flagshipContext.Sender;
            httpClient = new HttpClient();
            targetingMatch = new TargetingMatch(logger);
            variationAllocation = new VariationAllocation(logger);

            var interval = flagshipContext.Options?.BucketingPollingInterval ?? DefaultPollingInterval;
            loadTimer = new Timer(async (object state) => await LoadConfig(), this, interval, interval);
            LoadConfig();
        }

        public async Task LoadConfig()
        {
            try
            {
                HttpResponseMessage response = await httpClient.GetAsync($"{BucketingApiUrl}/{environmentId}/bucketing.json");
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();

                configuration = JsonConvert.DeserializeObject<Configuration>(responseBody);

                logger.Log(LogLevel.INFO, LogCode.BUCKETING_LOAD_SUCCESS);
            }
            catch (HttpRequestException e)
            {
                logger.Log(LogLevel.ERROR, LogCode.BUCKETING_LOADING_ERROR, new { message = e.Message });
            }
            catch (Exception e)
            {
                logger.Log(LogLevel.ERROR, LogCode.BUCKETING_LOADING_ERROR, new { message = e.Message });
            }
        }

        public async Task<DecisionResponse> GetResponse(DecisionRequest request)
        {
            var response = new DecisionResponse()
            {
                VisitorID = request.VisitorId,
                Campaigns = new HashSet<Model.Campaign>()
            };

            if (configuration == null)
            {
                logger.Log(LogLevel.WARN, LogCode.BUCKETING_NOT_LOADED);
                return response;
            }

            response.Panic = configuration.Panic;
            if (configuration.Panic)
            {
                logger.Log(LogLevel.WARN, LogCode.BUCKETING_PANIC_MODE_ENABLED);
                return response;
            }

            sender.SendEvent(new EventRequest(this.environmentId, request.VisitorId, EventType.CONTEXT, request.Context)).ContinueWith((Task t) =>
            {
                logger.Log(LogLevel.DEBUG, LogCode.CONTEXT_SENT_TRACKING, new { request.Context });
            });

            foreach (var campaign in configuration.Campaigns ?? new List<Model.Bucketing.Campaign>().AsEnumerable())
            {
                foreach (var vg in campaign.VariationGroups)
                {
                    var match = vg.Targeting.TargetingGroups.Any(tg => tg.Targetings.All(t =>
                    {
                        switch (t.Key)
                        {
                            case "fs_users":
                                return targetingMatch.Match(request.VisitorId, t.Operator, t.Value);
                            case "fs_all_users":
                                return true;
                            default:
                                if (request.Context.ContainsKey(t.Key))
                                {
                                    return targetingMatch.Match(request.Context[t.Key], t.Operator, t.Value);
                                }
                                return false;
                        }
                    }));

                    if (match)
                    {
                        var variation = variationAllocation.GetVariation(vg, request.VisitorId);
                        if (variation != null)
                        {
                            response.Campaigns.Add(new Model.Campaign()
                            {
                                Id = campaign.Id,
                                Variation = new Model.Variation()
                                {
                                    Id = variation.Id,
                                    Reference = variation.Reference,
                                    Modifications = variation.Modifications
                                },
                                VariationGroupId = vg.Id
                            });
                        }
                    }
                }
            }
            return response;
        }

        public void Dispose()
        {
            if (httpClient != null)
            {
                httpClient.Dispose();
            };

            if (loadTimer != null)
            {
                loadTimer.Dispose();
            }
            GC.SuppressFinalize(this);
        }
    }
}

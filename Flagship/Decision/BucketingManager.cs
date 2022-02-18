using Flagship.Config;
using Flagship.Delegate;
using Flagship.Enums;
using Flagship.FsVisitor;
using Flagship.Model;
using Flagship.Model.Bucketing;
using Murmur;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Flagship.Decision
{
    internal class BucketingManager : DecisionManager
    {
        new public event StatusChangeDelegate StatusChange;

        protected Murmur32 _murmur32;
        protected bool _isPolling;
        protected string _lastModified;
        private BucketingDTO bucketingContent;
        protected bool _isFirstPooling;
        new public BucketingConfig Config { get; set; }
        protected BucketingDTO BucketingContent { get => bucketingContent; set => bucketingContent = value; }

        protected Timer _timer;

        public BucketingManager(BucketingConfig config, HttpClient httpClient, Murmur32 murmur32) : base(config, httpClient)
        {
            _murmur32 = murmur32;
            _isPolling = false;
            _lastModified = null;
            _isFirstPooling = true;
            Config = config;
        }

        public async Task StartPolling()
        {

            var pollingInterval = Config.PollingInterval.Value;
            Utils.Log.LogInfo(Config, "Bucketing polling starts", "StartPolling");
            await Polling();
            if (pollingInterval.TotalMilliseconds == 0)
            {
                return;
            }

            if (_timer != null)
            {
                _timer.Dispose();
            }

            _timer = new Timer(async (e) =>
            {
                await Polling();
            }, null, pollingInterval, pollingInterval);


        }

        public async Task Polling()
        {
            try
            {
                if (_isPolling)
                {
                    return;
                }
                _isPolling = true;

                if (_isFirstPooling)
                {
                    StatusChange?.Invoke(FlagshipStatus.POLLING);
                }

                var url = string.Format(Constants.BUCKETING_API_URL, Config.EnvId);


                var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);

                requestMessage.Headers.Add(Constants.HEADER_X_API_KEY, Config.ApiKey);
                requestMessage.Headers.Add(Constants.HEADER_X_SDK_CLIENT, Constants.SDK_LANGUAGE);
                requestMessage.Headers.Add(Constants.HEADER_X_SDK_VERSION, Constants.SDK_VERSION);
                requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.HEADER_APPLICATION_JSON));


                if (_lastModified != null)
                {
                    requestMessage.Headers.Add(HttpRequestHeader.IfModifiedSince.ToString(), _lastModified);
                }

                var response = await HttpClient.SendAsync(requestMessage);


                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    BucketingContent = JsonConvert.DeserializeObject<BucketingDTO>(responseBody);
                }

                if (response.Headers.TryGetValues(HttpResponseHeader.LastModified.ToString(), out IEnumerable<string> lastModified))
                {
                    _lastModified = lastModified.First();
                };

                if (_isFirstPooling)
                {
                    _isFirstPooling = false;
                    StatusChange?.Invoke(FlagshipStatus.READY);
                }

                _isPolling = false;
            }
            catch (Exception ex)
            {
                _isPolling = false;

                if (_isFirstPooling)
                {
                    StatusChange?.Invoke(FlagshipStatus.NOT_INITIALIZED);
                }
                Utils.Log.LogError(Config, ex.Message, "Polling");
            }
        }

        public void StopPolling()
        {
            if (_timer != null)
            {
                _timer.Dispose();
            }
            _isPolling = false;
            Utils.Log.LogInfo(Config, "Bucketing polling stopped", "StopPolling");
        }

        virtual public async Task SendContext(VisitorDelegateAbstract visitor)
        {
            try
            {
                var url = string.Format(Constants.BUCKETING_API_CONTEXT_URL, Config.EnvId);

                var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);

                requestMessage.Headers.Add(Constants.HEADER_X_API_KEY, Config.ApiKey);
                requestMessage.Headers.Add(Constants.HEADER_X_SDK_CLIENT, Constants.SDK_LANGUAGE);
                requestMessage.Headers.Add(Constants.HEADER_X_SDK_VERSION, Constants.SDK_VERSION);
                requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.HEADER_APPLICATION_JSON));

                var postData = new Dictionary<string, object>
                {
                    ["visitorId"] = visitor.VisitorId,
                    ["type"] = "CONTEXT",
                    ["data"] = visitor.Context
                };

                var postDatajson = JsonConvert.SerializeObject(postData);

                var stringContent = new StringContent(postDatajson, Encoding.UTF8, "application/json");

                requestMessage.Content = stringContent;

                var response = await HttpClient.SendAsync(requestMessage);

                if (response.StatusCode >= HttpStatusCode.BadRequest)
                {
                    throw new Exception(response.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {

                Utils.Log.LogError(Config, ex.Message, "SendContext");
            }
        }
        public override Task<ICollection<Model.Campaign>> GetCampaigns(VisitorDelegateAbstract visitor)
        {
            _ = SendContext(visitor);

            return Task.Factory.StartNew(() =>
            {
                ICollection<Model.Campaign> campaigns = new Collection<Model.Campaign>();

                if (BucketingContent == null)
                {
                    return campaigns;
                }


                if (BucketingContent.Panic.HasValue && BucketingContent.Panic.Value)
                {
                    IsPanic = true;
                    return campaigns;
                }

                IsPanic = false;

                foreach (var item in BucketingContent.Campaigns)
                {
                    var campaign = GetVisitorCampaigns(item.VariationGroups, visitor, item.Id, item.Type);
                    if (campaign != null)
                    {
                        campaigns.Add(campaign);
                    }
                }

                return campaigns;
            });

        }

        protected Model.Campaign GetVisitorCampaigns(IEnumerable<VariationGroup> variationGroups, VisitorDelegateAbstract visitor, string campaignId, string campaignType)
        {
            foreach (var item in variationGroups)
            {
                var check = IsMatchedTargeting(item, visitor);
                if (check)
                {
                    var variation = GetVariation(item, visitor);
                    if (variation == null)
                    {
                        return null;
                    }
                    return new Model.Campaign
                    {
                        Id = campaignId,
                        Variation = variation,
                        VariationGroupId = item.Id,
                        Type = campaignType
                    };
                }
            }
            return null;
        }

        protected Model.Variation GetVariation(VariationGroup variationGroup, VisitorDelegateAbstract visitor)
        {
            if (variationGroup == null)
            {
                return null;
            }
            var hashBytes = _murmur32.ComputeHash(Encoding.UTF8.GetBytes(variationGroup.Id + visitor.VisitorId));
            var hash = BitConverter.ToUInt32(hashBytes, 0);
            var hashAllocation = hash % 100;
            var totalAllocation = 0;

            if (variationGroup.Variations == null)
            {
                return null;
            }

            foreach (var item in variationGroup.Variations)
            {
                if (visitor.VisitorCache?.Version == 1)
                {
                    var visitorCache = (VisitorCacheDTOV1)visitor.VisitorCache.Data;
                    var cacheVariation = visitorCache.Data.Campaigns.FirstOrDefault(x => x.VariationGroupId == variationGroup.Id);
                    if (cacheVariation != null)
                    {
                        var newVariation = variationGroup.Variations.FirstOrDefault(x => x.Id == cacheVariation.VariationId);
                        if (newVariation == null)
                        {
                            continue;
                        }

                        return new Model.Variation
                        {
                            Id = newVariation.Id,
                            Modifications = newVariation.Modifications,
                            Reference = newVariation.Reference,
                        };
                    }
                }

                totalAllocation += item.Allocation;
                if (hashAllocation <= totalAllocation)
                {
                    return new Model.Variation
                    {
                        Id = item.Id,
                        Modifications = item.Modifications,
                        Reference = item.Reference,
                    };
                }
            }
            return null;
        }

        protected bool IsMatchedTargeting(VariationGroup variationGroup, VisitorDelegateAbstract visitor)
        {
            bool check = false;

            if (variationGroup == null || variationGroup.Targeting == null || variationGroup.Targeting.TargetingGroups == null)
            {
                return check;
            }

            foreach (var item in variationGroup.Targeting.TargetingGroups)
            {
                check = CheckAndTargeting(item.Targetings, visitor);
                if (check)
                {
                    break;
                }
            }
            return check;
        }

        protected bool CheckAndTargeting(IEnumerable<Targeting> targetings, VisitorDelegateAbstract visitor)
        {
            object contextValue;
            var check = false;

            foreach (var item in targetings)
            {
                if (item.Key == "fs_all_users")
                {
                    check = true;
                    continue;
                }
                if (item.Key == "fs_users")
                {
                    contextValue = visitor.VisitorId;
                }
                else
                {
                    if (!(visitor.Context.ContainsKey(item.Key)))
                    {
                        check = false;
                        break;
                    }
                    contextValue = visitor.Context[item.Key];
                }

                check = TestOperator(item.Operator, contextValue, item.Value);
                if (!check)
                {
                    break;
                }
            }
            return check;
        }

        protected bool TestOperator(TargetingOperator operatorName, object contextValue, object targetingValue)
        {
            bool check = false;
            try
            {

                if (targetingValue is JArray targetingValueArray)
                {
                    return TestListOperator(operatorName, contextValue, targetingValueArray);
                }

                switch (operatorName)
                {
                    case TargetingOperator.EQUALS:
                        check = contextValue.Equals(targetingValue);
                        break;
                    case TargetingOperator.NOT_EQUALS:
                        check = !contextValue.Equals(targetingValue);
                        break;
                    case TargetingOperator.CONTAINS:
                        check = contextValue.ToString().Contains(targetingValue.ToString());
                        break;
                    case TargetingOperator.NOT_CONTAINS:
                        check = !contextValue.ToString().Contains(targetingValue.ToString());
                        break;
                    case TargetingOperator.GREATER_THAN:
                        check = MatchOperator(operatorName, contextValue, targetingValue);
                        break;
                    case TargetingOperator.LOWER_THAN:
                        check = MatchOperator(operatorName, contextValue, targetingValue);
                        break;
                    case TargetingOperator.GREATER_THAN_OR_EQUALS:
                        check = MatchOperator(operatorName, contextValue, targetingValue);
                        break;
                    case TargetingOperator.LOWER_THAN_OR_EQUALS:
                        check = MatchOperator(operatorName, contextValue, targetingValue);
                        break;
                    case TargetingOperator.STARTS_WITH:
                        check = contextValue.ToString().StartsWith(targetingValue.ToString());
                        break;
                    case TargetingOperator.ENDS_WITH:
                        check = contextValue.ToString().EndsWith(targetingValue.ToString());
                        break;
                    default:
                        check = false;
                        break;
                }
            }
            catch (Exception ex)
            {
                Utils.Log.LogError(Config, ex.Message, "TestOperator");
            }


            return check;
        }

        protected bool TestListOperatorLoop(TargetingOperator operatorName, object contextValue, JArray targetingValue, bool initialCheck)
        {
            var check = initialCheck;
            foreach (var item in targetingValue)
            {
                if (item.Type.ToString() == "Double")
                {
                    check = TestOperator(operatorName, contextValue, item.Value<double>());
                }
                else
                {
                    check = TestOperator(operatorName, contextValue, item.Value<string>());
                }

                if (check != initialCheck)
                {
                    break;
                }
            }
            return check;
        }

        protected bool TestListOperator(TargetingOperator operatorName, object contextValue, JArray targetingValue)
        {
            if (operatorName == TargetingOperator.NOT_EQUALS || operatorName == TargetingOperator.NOT_CONTAINS)
            {
                return TestListOperatorLoop(operatorName, contextValue, targetingValue, true);
            }
            return TestListOperatorLoop(operatorName, contextValue, targetingValue, false);
        }

        protected bool MatchOperator(TargetingOperator operatorName, object contextValue, object targetingValue)
        {
            bool check = false;
            if (targetingValue is string value && contextValue is string contextString)
            {
                return MatchOperator(operatorName, contextString, value);

            }
            if ((targetingValue is int || targetingValue is long || targetingValue is double)
                && (contextValue is int || contextValue is long || contextValue is double))
            {
                return MatchOperator(operatorName, Convert.ToDouble(contextValue), Convert.ToDouble(targetingValue));
            }

            return check;
        }

        protected bool MatchOperator(TargetingOperator operatorName, string contextValue, string targetingValue)
        {
            bool check;
            switch (operatorName)
            {
                case TargetingOperator.GREATER_THAN:
                    check = contextValue.CompareTo(targetingValue) > 0;
                    break;
                case TargetingOperator.LOWER_THAN:
                    check = contextValue.CompareTo(targetingValue) < 0;
                    break;
                case TargetingOperator.GREATER_THAN_OR_EQUALS:
                    check = contextValue.CompareTo(targetingValue) >= 0;
                    break;
                case TargetingOperator.LOWER_THAN_OR_EQUALS:
                    check = contextValue.CompareTo(targetingValue) <= 0;
                    break;
                default:
                    check = false;
                    break;
            }
            return check;
        }

        protected bool MatchOperator(TargetingOperator operatorName, double contextValue, double targetingValue)
        {
            bool check;
            switch (operatorName)
            {
                case TargetingOperator.GREATER_THAN:
                    check = contextValue.CompareTo(targetingValue) > 0;
                    break;
                case TargetingOperator.LOWER_THAN:
                    check = contextValue.CompareTo(targetingValue) < 0;
                    break;
                case TargetingOperator.GREATER_THAN_OR_EQUALS:
                    check = contextValue.CompareTo(targetingValue) >= 0;
                    break;
                case TargetingOperator.LOWER_THAN_OR_EQUALS:
                    check = contextValue.CompareTo(targetingValue) <= 0;
                    break;
                default:
                    check = false;
                    break;
            }
            return check;
        }
    }
}

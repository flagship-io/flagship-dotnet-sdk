﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Flagship.Config;
using Flagship.Enums;
using Flagship.FsVisitor;
using Flagship.Hit;
using Flagship.Logger;
using Flagship.Model;
using Flagship.Model.Bucketing;
using Murmur;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Flagship.Decision
{
    internal class BucketingManager : DecisionManager
    {
        protected Murmur32 _murmur32;
        protected bool _isPolling;
        protected DateTimeOffset? _lastModified;
        private BucketingDTO bucketingContent;
        protected bool _isFirstPooling;
        public new BucketingConfig Config { get; set; }
        protected BucketingDTO BucketingContent
        {
            get => bucketingContent;
            set => bucketingContent = value;
        }

        protected Timer _timer;

        public const string FETCH_THIRD_PARTY_SUCCESS =
            "THIRD_PARTY_SEGMENT has been fetched : {0}";
        public const string GET_THIRD_PARTY_SEGMENT = "GetThirdPartySegments";

        public BucketingManager(BucketingConfig config, HttpClient httpClient, Murmur32 murmur32)
            : base(config, httpClient)
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
            Log.LogInfo(Config, "Bucketing polling starts", "StartPolling");
            await Polling().ConfigureAwait(false);
            if (pollingInterval.TotalMilliseconds == 0)
            {
                return;
            }

            _timer?.Dispose();

            _timer = new Timer(
                async (e) =>
                {
                    await Polling().ConfigureAwait(false);
                },
                null,
                pollingInterval,
                pollingInterval
            );
        }

        public async Task Polling()
        {
            var now = DateTime.Now;
            var url = string.Format(Constants.BUCKETING_API_URL, Config.EnvId);
            try
            {
                if (_isPolling)
                {
                    return;
                }
                _isPolling = true;

                if (_isFirstPooling)
                {
                    UpdateStatus(FSSdkStatus.SDK_INITIALIZING);
                }

                var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);

                requestMessage.Headers.Add(Constants.HEADER_X_API_KEY, Config.ApiKey);
                requestMessage.Headers.Add(Constants.HEADER_X_SDK_CLIENT, Constants.SDK_LANGUAGE);
                requestMessage.Headers.Add(Constants.HEADER_X_SDK_VERSION, Constants.SDK_VERSION);
                requestMessage.Headers.Accept.Add(
                    new MediaTypeWithQualityHeaderValue(Constants.HEADER_APPLICATION_JSON)
                );

                if (_lastModified != null)
                {
                    requestMessage.Headers.IfModifiedSince = _lastModified;
                }

                var response = await HttpClient.SendAsync(requestMessage).ConfigureAwait(false);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string responseBody = await response
                        .Content.ReadAsStringAsync()
                        .ConfigureAwait(false);
                    BucketingContent = JsonConvert.DeserializeObject<BucketingDTO>(responseBody);
                    LastBucketingTimestamp = DateTime
                        .Now.ToUniversalTime()
                        .ToString(Constants.FORMAT_UTC);

                    var troubleshootingHit = new Troubleshooting()
                    {
                        Label = DiagnosticLabel.SDK_BUCKETING_FILE,
                        LogLevel = LogLevel.INFO,
                        VisitorId = FlagshipInstanceId,
                        FlagshipInstanceId = FlagshipInstanceId,
                        Traffic = 0,
                        Config = Config,
                        HttpResponseTime = (DateTime.Now - now).Milliseconds,
                        HttpRequestHeaders = new Dictionary<string, object>()
                        {
                            [Constants.HEADER_X_API_KEY] = Config.ApiKey,
                            [Constants.HEADER_X_SDK_CLIENT] = Constants.SDK_LANGUAGE,
                            [Constants.HEADER_X_SDK_VERSION] = Constants.SDK_VERSION,
                        },
                        HttpRequestMethod = "POST",
                        HttpRequestUrl = url,
                        HttpResponseBody = BucketingContent,
                        HttpResponseCode = (int?)response.StatusCode,
                    };

                    _lastModified = response.Content.Headers.LastModified;

                    TrackingManager.AddTroubleshootingHit(troubleshootingHit);
                }

                if (_isFirstPooling)
                {
                    _isFirstPooling = false;
                    UpdateStatus(FSSdkStatus.SDK_INITIALIZED);
                }

                _isPolling = false;
            }
            catch (Exception ex)
            {
                _isPolling = false;

                if (_isFirstPooling)
                {
                    UpdateStatus(FSSdkStatus.SDK_NOT_INITIALIZED);
                }
                Log.LogError(Config, ex.Message, "Polling");

                var troubleshootingHit = new Troubleshooting()
                {
                    Label = DiagnosticLabel.SDK_BUCKETING_FILE,
                    LogLevel = LogLevel.INFO,
                    VisitorId = FlagshipInstanceId,
                    FlagshipInstanceId = FlagshipInstanceId,
                    Traffic = 0,
                    Config = Config,
                    HttpResponseTime = (DateTime.Now - now).Milliseconds,
                    HttpRequestHeaders = new Dictionary<string, object>()
                    {
                        [Constants.HEADER_X_API_KEY] = Config.ApiKey,
                        [Constants.HEADER_X_SDK_CLIENT] = Constants.SDK_LANGUAGE,
                        [Constants.HEADER_X_SDK_VERSION] = Constants.SDK_VERSION,
                    },
                    HttpRequestMethod = "POST",
                    HttpRequestUrl = url,
                    HttpResponseBody = ex.Message,
                };

                TrackingManager?.AddTroubleshootingHit(troubleshootingHit);
            }
        }

        public void StopPolling()
        {
            _timer?.Dispose();
            _isPolling = false;
            Log.LogInfo(Config, "Bucketing polling stopped", "StopPolling");
        }

        public virtual async Task SendContextAsync(VisitorDelegateAbstract visitor)
        {
            try
            {
                if (
                    !visitor.HasConsented
                    || visitor.Context.Count <= Constants.NB_MIN_CONTEXT_KEYS
                    || !visitor.HasContextBeenUpdated
                )
                {
                    return;
                }

                visitor.HasContextBeenUpdated = false;

                var segment = new Segment(visitor.Context);
                await visitor.SendHit(segment).ConfigureAwait(false);

                var troubleshootingHit = new Troubleshooting()
                {
                    Label = DiagnosticLabel.VISITOR_SEND_HIT,
                    LogLevel = LogLevel.INFO,
                    Traffic = visitor.Traffic,
                    VisitorSessionId = visitor.SessionId,
                    FlagshipInstanceId = visitor.SdkInitialData.InstanceId,
                    AnonymousId = visitor.AnonymousId,
                    VisitorId = visitor.VisitorId,
                    Config = Config,
                    HitContent = segment.ToApiKeys(),
                };

                visitor.SegmentHitTroubleshooting = troubleshootingHit;
            }
            catch (Exception ex)
            {
                Log.LogError(Config, ex.Message, "SendContext");
            }
        }

        protected async Task<Dictionary<string, object>> GetThirdPartySegmentsAsync(
            string visitorId
        )
        {
            var url = string.Format(Constants.THIRD_PARTY_SEGMENT_URL, Config.EnvId, visitorId);
            var contexts = new Dictionary<string, object>();
            try
            {
                using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, url))
                {
                    using (var response = await HttpClient.SendAsync(requestMessage))
                    {
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            string responseBody = await response
                                .Content.ReadAsStringAsync()
                                .ConfigureAwait(false);
                            var thirdPartySegments = JsonConvert.DeserializeObject<
                                List<ThirdPartySegmentDTO>
                            >(responseBody);
                            if (thirdPartySegments != null && thirdPartySegments.Count > 0)
                            {
                                foreach (var segment in thirdPartySegments)
                                {
                                    var key = segment.Partner + "::" + segment.Segment;
                                    contexts[key] = segment.Value;
                                }
                                Log.LogDebug(
                                    Config,
                                    string.Format(
                                        FETCH_THIRD_PARTY_SUCCESS,
                                        JsonConvert.SerializeObject(contexts)
                                    ),
                                    GET_THIRD_PARTY_SEGMENT
                                );
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.LogError(Config, ex.Message, GET_THIRD_PARTY_SEGMENT);
            }
            return contexts;
        }

        public override async Task<ICollection<Model.Campaign>> GetCampaigns(
            VisitorDelegateAbstract visitor
        )
        {
            ICollection<Model.Campaign> campaigns = new Collection<Model.Campaign>();

            if (BucketingContent == null)
            {
                return campaigns;
            }

            if (BucketingContent.AccountSettings?.Troubleshooting != null)
            {
                TroubleshootingData = BucketingContent.AccountSettings.Troubleshooting;
            }

            if (BucketingContent.Panic.GetValueOrDefault())
            {
                IsPanic = true;
                return campaigns;
            }

            IsPanic = false;

            if (Config.FetchThirdPartyData)
            {
                var thirdPartySegments = await GetThirdPartySegmentsAsync(visitor.VisitorId);
                foreach (var item in thirdPartySegments)
                {
                    visitor.Context[item.Key] = item.Value;
                }
            }

            await SendContextAsync(visitor);

            foreach (var item in BucketingContent.Campaigns)
            {
                var campaign = GetMatchingVisitorVariationGroup(
                    item.VariationGroups,
                    visitor,
                    item.Id,
                    item.Type
                );
                if (campaign != null)
                {
                    campaign.Name = item.Name;
                    campaigns.Add(campaign);
                }
            }

            return campaigns;
        }

        protected Model.Campaign GetMatchingVisitorVariationGroup(
            IEnumerable<VariationGroup> variationGroups,
            VisitorDelegateAbstract visitor,
            string campaignId,
            string campaignType
        )
        {
            var matchingGroup = variationGroups.FirstOrDefault(item =>
                IsMatchedTargeting(item, visitor)
            );
            if (matchingGroup != null)
            {
                var variation = GetVariation(matchingGroup, visitor);
                if (variation != null)
                {
                    return new Model.Campaign
                    {
                        Id = campaignId,
                        Variation = variation,
                        VariationGroupId = matchingGroup.Id,
                        VariationGroupName = matchingGroup.Name,
                        Type = campaignType,
                    };
                }
            }
            return null;
        }

        protected Model.Variation GetVariation(
            VariationGroup variationGroup,
            VisitorDelegateAbstract visitor
        )
        {
            if (variationGroup?.Variations == null)
            {
                return null;
            }

            var hashBytes = _murmur32.ComputeHash(
                Encoding.UTF8.GetBytes(variationGroup.Id + visitor.VisitorId)
            );
            var hash = BitConverter.ToUInt32(hashBytes, 0);
            var hashAllocation = hash % 100;
            var totalAllocation = 0;

            foreach (var item in variationGroup.Variations)
            {
                if (visitor.VisitorCache?.Version == 1)
                {
                    var visitorCache = (VisitorCacheDTOV1)visitor.VisitorCache.Data;
                    var variationHistory = visitorCache?.Data?.AssignmentsHistory;

                    var cacheVariationId =
                        variationHistory != null && variationHistory.ContainsKey(variationGroup.Id)
                            ? variationHistory[variationGroup.Id]
                            : null;

                    if (cacheVariationId != null)
                    {
                        var newVariation = variationGroup.Variations.FirstOrDefault(x =>
                            x.Id == cacheVariationId
                        );
                        if (newVariation == null)
                        {
                            continue;
                        }

                        return new Model.Variation
                        {
                            Id = newVariation.Id,
                            Modifications = newVariation.Modifications,
                            Reference = newVariation.Reference,
                            Name = newVariation.Name,
                        };
                    }
                }

                if (item.Allocation == 0)
                {
                    continue;
                }

                totalAllocation += item.Allocation;
                if (hashAllocation < totalAllocation)
                {
                    return new Model.Variation
                    {
                        Id = item.Id,
                        Modifications = item.Modifications,
                        Reference = item.Reference,
                        Name = item.Name,
                    };
                }
            }
            return null;
        }

        protected bool IsMatchedTargeting(
            VariationGroup variationGroup,
            VisitorDelegateAbstract visitor
        )
        {
            bool check = false;

            if (variationGroup?.Targeting?.TargetingGroups == null)
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

        protected bool CheckAndTargeting(
            IEnumerable<Targeting> targetings,
            VisitorDelegateAbstract visitor
        )
        {
            if (targetings == null || targetings.Count() == 0)
            {
                return false;
            }
            foreach (var item in targetings)
            {
                if (item.Key == "fs_all_users")
                {
                    return true;
                }

                object contextValue;

                switch (item.Operator)
                {
                    case TargetingOperator.EXISTS:
                        if (visitor.Context.ContainsKey(item.Key))
                        {
                            continue;
                        }
                        return false;

                    case TargetingOperator.NOT_EXISTS:
                        if (!visitor.Context.ContainsKey(item.Key))
                        {
                            continue;
                        }
                        return false;
                    default:
                        if (item.Key == "fs_users")
                        {
                            contextValue = visitor.VisitorId;
                        }
                        else
                        {
                            if (!visitor.Context.ContainsKey(item.Key))
                            {
                                return false;
                            }
                            contextValue = visitor.Context[item.Key];
                        }

                        var check = TestOperator(item.Operator, contextValue, item.Value);
                        if (!check)
                        {
                            return false;
                        }
                        break;
                }
            }
            return true;
        }

        protected bool TestOperator(
            TargetingOperator operatorName,
            object contextValue,
            object targetingValue
        )
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
                }
            }
            catch (Exception ex)
            {
                Log.LogError(Config, ex.Message, "TestOperator");
            }

            return check;
        }

        protected bool TestListOperatorLoop(
            TargetingOperator operatorName,
            object contextValue,
            JArray targetingValue,
            bool initialCheck
        )
        {
            var check = initialCheck;
            foreach (var item in targetingValue)
            {
                if (item.Type.ToString() == "Double" || item.Type.ToString() == "Float")
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

        protected bool TestListOperator(
            TargetingOperator operatorName,
            object contextValue,
            JArray targetingValue
        )
        {
            if (
                operatorName == TargetingOperator.NOT_EQUALS
                || operatorName == TargetingOperator.NOT_CONTAINS
            )
            {
                return TestListOperatorLoop(operatorName, contextValue, targetingValue, true);
            }
            return TestListOperatorLoop(operatorName, contextValue, targetingValue, false);
        }

        protected bool MatchOperator(
            TargetingOperator operatorName,
            object contextValue,
            object targetingValue
        )
        {
            bool check = false;
            if (targetingValue is string value && contextValue is string contextString)
            {
                return MatchOperator(operatorName, contextString, value);
            }
            if (
                (targetingValue is int || targetingValue is long || targetingValue is double)
                && (contextValue is int || contextValue is long || contextValue is double)
            )
            {
                return MatchOperator(
                    operatorName,
                    Convert.ToDouble(contextValue),
                    Convert.ToDouble(targetingValue)
                );
            }

            return check;
        }

        protected bool MatchOperator(
            TargetingOperator operatorName,
            string contextValue,
            string targetingValue
        )
        {
            bool check = false;
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
            }
            return check;
        }

        protected bool MatchOperator(
            TargetingOperator operatorName,
            double contextValue,
            double targetingValue
        )
        {
            bool check = false;
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
            }
            return check;
        }
    }
}

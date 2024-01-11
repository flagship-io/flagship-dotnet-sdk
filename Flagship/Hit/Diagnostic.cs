using System;
using System.Collections.Generic;
using Flagship.Enums;
using Flagship.Model;
using Newtonsoft.Json;

namespace Flagship.Hit
{
    internal class Diagnostic : HitAbstract
    {
        public string Version { get; set; }
        public LogLevel LogLevel { get; set; }
        public string Timestamp { get; set; }
        public string TimeZone { get; set; }
        public DiagnosticLabel Label { get; set; }
        public string LastInitializationTimestamp { get; set; }
        public string LastBucketingTimestamp { get; set; }
        public uint Traffic { get; set; }
        public string FlagshipInstanceId { get; set; }

        public string StackType { get; set; }
        public string StackName { get; set; }
        public string StackVersion { get; set; }
        public string StackOriginName { get; set; }
        public string StackOriginVersion { get; set; }

        public FlagshipStatus? SdkStatus { get; set; }
        public DecisionMode? SdkConfigMode { get; set; }
        public TimeSpan? SdkConfigTimeout { get; set; }
        public TimeSpan? SdkConfigPollingInterval { get; set; }
        public Model.Bucketing.BucketingDTO SdkBucketingFile { get; set; }
        public CacheStrategy? SdkConfigTrackingManagerConfigStrategy { get; set; }
        public TimeSpan? SdkConfigTrackingManagerConfigBatchIntervals { get; set; }
        public int? SdkConfigTrackingManagerConfigPoolMaxSize { get; set; }
        public bool? SdkConfigUsingCustomHitCache { get; set; }
        public bool? SdkConfigUsingCustomVisitorCache { get; set; }
        public bool? SdkConfigUsingOnVisitorExposed { get; set; }
        public bool? SdkConfigDisableCache { get; set; }

        public string HttpRequestUrl { get; set; }
        public string HttpRequestMethod { get; set; }
        public IDictionary<string, object> HttpRequestHeaders { get; set; }
        public object HttpsRequestBody { get; set; }

        public string HttpResponseUrl { get; set; }
        public string HttpResponseMethod { get; set; }
        public IDictionary<string, object> HttpResponseHeaders { get; set; }
        public int? HttpResponseCode { get; set; }
        public object HttpResponseBody { get; set; }
        public int? HttpResponseTime { get; set; }

        public string VisitorStatus { get; set; }
        public InstanceType? VisitorInstanceType { get; set; }
        public IDictionary<string, object> VisitorContext { get; set; }
        public bool? VisitorConsent { get; set; }
        public IDictionary<string, object> VisitorAssignmentHistory { get; set; }
        public ICollection<FlagDTO> VisitorFlags { get; set; }
        public ICollection<Campaign> VisitorCampaigns { get; set; }
        public bool? VisitorIsAuthenticated { get; set; }
        public string VisitorSessionId { get; set; }

        public string ContextKey { get; set; }
        public object ContextValue { get; set; }

        public string FlagKey { get; set; }
        public object FlagValue { get; set; }
        public object FlagDefaultValue { get; set; }
        public bool? VisitorExposed { get; set; }

        public string FlagMetadataCampaignId { get; set; }
        public string FlagMetadataCampaignName { get; set; }
        public string FlagMetadataVariationGroupId { get; set; }
        public string FlagMetadataVariationGroupName { get; set; }
        public string FlagMetadataVariationId { get; set; }
        public string FlagMetadataVariationName { get; set; }
        public string FlagMetadataCampaignSlug { get; set; }
        public string FlagMetadataCampaignType { get; set; }
        public bool? FlagMetadataCampaignIsReference { get; set; }

        public IDictionary<string, object> HitContent { get; set; }
        public CacheTriggeredBy? BatchTriggeredBy { get; set; }


        public Diagnostic(HitType type) : base(type)
        {
            Version = "1";
            Timestamp = new DateTime().ToUniversalTime().ToString("u:");
            TimeZone = TimeZoneInfo.Local.StandardName;
            StackType = "SDK";
            StackName = Constants.SDK_LANGUAGE;
            StackVersion = Constants.SDK_VERSION;
        }

        internal override string GetErrorMessage()
        {
            throw new NotImplementedException();
        }

        internal override IDictionary<string, object> ToApiKeys()
        {
            var apiKeys = new Dictionary<string, object>()
            {
                [Constants.VISITOR_ID_API_ITEM] = VisitorId,
                [Constants.DS_API_ITEM] = DS,
                [Constants.CUSTOMER_ENV_ID_API_ITEM] = Config?.EnvId,
                [Constants.T_API_ITEM] = $"{Type}"
            };

            var customVariable = new Dictionary<string, string>()
            {
                ["version"] = Version,
                ["logLevel"] = $"{LogLevel}",
                ["timestamp"] = Timestamp,
                ["timeZone"] = TimeZone,
                ["label"] = $"{Label}",
                ["stack.type"] = StackType,
                ["stack.name"] = StackName,
                ["stack.version"] = StackVersion
            };

            if (!string.IsNullOrWhiteSpace(FlagshipInstanceId))
            {
                customVariable["flagshipInstanceId"] = FlagshipInstanceId;
            }

            if (!string.IsNullOrWhiteSpace(LastInitializationTimestamp))
            {
                customVariable["lastInitializationTimestamp"] = LastInitializationTimestamp;
            }

            if (!string.IsNullOrWhiteSpace(LastBucketingTimestamp))
            {
                customVariable["lastBucketingTimestamp"] = LastBucketingTimestamp;
            }

            if (!string.IsNullOrWhiteSpace(Config?.EnvId))
            {
                customVariable["envId"] = Config?.EnvId;
            }

            if (SdkBucketingFile!=null)
            {
                customVariable["sdkBucketingFile"] = JsonConvert.SerializeObject(SdkBucketingFile);
            }

            if (!string.IsNullOrWhiteSpace(StackOriginName))
            {
                customVariable["stack.origin.name"] = StackOriginName;
            }

            if (!string.IsNullOrWhiteSpace(StackOriginVersion))
            {
                customVariable["stack.origin.version"] = StackOriginVersion;
            }

            if (SdkStatus != null)
            {
                customVariable["sdk.status"] = $"{SdkStatus}";
            }

            if (SdkConfigMode != null)

            {
                customVariable["sdk.config.mode"] = $"{SdkConfigMode}";
            }

            if (SdkConfigTimeout != null)
            {
                customVariable["sdk.config.timeout"] = SdkConfigTimeout.GetValueOrDefault().ToString();
            }

            if (SdkConfigPollingInterval != null)
            {
                customVariable["sdk.config.pollingTime"] = SdkConfigPollingInterval.GetValueOrDefault().ToString();
            }

            if (SdkConfigTrackingManagerConfigStrategy != null)
            {
                customVariable["sdk.config.trackingManager.strategy"] = $"{SdkConfigTrackingManagerConfigStrategy}";
            }

            if (SdkConfigTrackingManagerConfigBatchIntervals != null)
            {
                customVariable["sdk.config.trackingManager.batchIntervals"] = SdkConfigTrackingManagerConfigBatchIntervals.GetValueOrDefault().ToString();
            }

            if (SdkConfigTrackingManagerConfigPoolMaxSize != null)
            {
                customVariable["sdk.config.trackingManager.poolMaxSize"] = SdkConfigTrackingManagerConfigPoolMaxSize.GetValueOrDefault().ToString();
            }

            if (SdkConfigUsingCustomHitCache != null)
            {
                customVariable["sdk.config.usingCustomHitCache"] = SdkConfigUsingCustomHitCache.GetValueOrDefault().ToString();
            }

            if (SdkConfigUsingCustomVisitorCache != null)
            {
                customVariable["sdk.config.usingCustomVisitorCache"] = SdkConfigUsingCustomVisitorCache.GetValueOrDefault().ToString();
            }

            if (SdkConfigUsingOnVisitorExposed != null)
            {
                customVariable["sdk.config.usingOnVisitorExposed"] = SdkConfigUsingOnVisitorExposed.GetValueOrDefault().ToString();
            }

            if (SdkConfigDisableCache != null)
            {
                customVariable["sdk.config.disableCache"] = SdkConfigDisableCache.GetValueOrDefault().ToString();
            }

            if (HttpRequestUrl != null)
            {
                customVariable["http.request.url"] = HttpRequestUrl;
            }

            if (HttpRequestMethod != null)
            {
                customVariable["http.request.method"] = HttpRequestMethod;
            }

            if (HttpRequestHeaders != null)
            {
                customVariable["http.request.headers"] = JsonConvert.SerializeObject(HttpRequestHeaders);
            }

            if (HttpsRequestBody != null)
            {
                customVariable["http.request.body"] = JsonConvert.SerializeObject(HttpsRequestBody);
            }

            if (HttpResponseUrl != null)
            {
                customVariable["http.response.url"] = HttpResponseUrl;
            }

            if (HttpResponseMethod != null)
            {
                customVariable["http.response.method"] = HttpResponseMethod;
            }

            if (HttpResponseHeaders != null)
            {
                customVariable["http.response.headers"] = JsonConvert.SerializeObject(HttpResponseHeaders);
            }

            if (HttpResponseCode != null)
            {
                customVariable["http.response.code"] = HttpResponseCode.GetValueOrDefault().ToString();
            }

            if (HttpResponseBody != null)
            {
                customVariable["http.response.body"] = JsonConvert.SerializeObject(HttpResponseBody);
            }

            if (HttpResponseTime != null)
            {
                customVariable["http.response.time"] = HttpResponseTime.GetValueOrDefault().ToString();
            }

            if (VisitorId!=null)
            {
                customVariable["visitor.visitorId"] = VisitorId;
            }

            if (AnonymousId != null)
            {
                customVariable["visitor.anonymousId"] = AnonymousId;
            }

            if (VisitorSessionId != null)
            {
                customVariable["visitor.sessionId"] = VisitorSessionId;
            }

            if (VisitorStatus != null)
            {
                customVariable["visitor.status"] = VisitorStatus;
            }

            if (VisitorInstanceType != null)
            {
                customVariable["visitor.instanceType"] = $"{VisitorInstanceType}";
            }

            if (VisitorContext != null)
            {
                foreach (var item in VisitorContext)
                {
                    customVariable[$"visitor.context.{item.Key}"] = item.Value.ToString();
                }
            }

            if (VisitorConsent != null)
            {
                customVariable["visitor.consent"] = VisitorConsent.GetValueOrDefault().ToString();
            }

            if (VisitorAssignmentHistory != null)
            {
                foreach (var item in VisitorAssignmentHistory)
                {
                    customVariable[$"visitor.assignments.[{item.Key}]"] = item.Value.ToString();
                }
            }

            if (VisitorFlags != null)
            {
                foreach (var flagDto in VisitorFlags)
                {
                    var flagKey = flagDto.Key;
                    var commonKey = $"visitor.flags.[{flagKey}]";
                    var customVariableKeyMetadata = $"{commonKey}.metadata";
                    customVariable[$"{commonKey}.key"] = flagKey;
                    customVariable[$"{commonKey}.value"] = JsonConvert.SerializeObject(flagDto.Value);
                    customVariable[$"{customVariableKeyMetadata}.campaignId"] = flagDto.CampaignId;
                    customVariable[$"{customVariableKeyMetadata}.campaignName"] = flagDto.CampaignName;
                    customVariable[$"{customVariableKeyMetadata}.campaignType"] = flagDto.CampaignType;
                    customVariable[$"{customVariableKeyMetadata}.variationGroupId"] = flagDto.VariationGroupId;
                    customVariable[$"{customVariableKeyMetadata}.variationGroupName"] = flagDto.VariationGroupName;
                    customVariable[$"{customVariableKeyMetadata}.variationId"] = flagDto.VariationId;
                    customVariable[$"{customVariableKeyMetadata}.variationName"] = flagDto.VariationName;
                    customVariable[$"{customVariableKeyMetadata}.slug"] = flagDto.Slug;
                    customVariable[$"{customVariableKeyMetadata}.isReference"] = flagDto.IsReference.ToString();

                }
            }

            if (VisitorIsAuthenticated != null)
            {
                customVariable["visitor.isAuthenticated"] = VisitorIsAuthenticated.GetValueOrDefault().ToString();
            }

            if (VisitorCampaigns != null)
            {
                customVariable["visitor.campaigns"] = JsonConvert.SerializeObject(VisitorCampaigns);
            }

            if (!string.IsNullOrWhiteSpace(ContextKey))
            {
                customVariable["contextKey"] = ContextKey;
            }

            if (ContextValue != null)
            {
                customVariable["contextValue"] = ContextValue.ToString();
            }

            if (!string.IsNullOrWhiteSpace(FlagKey))
            {
                customVariable["flag.key"] = FlagKey;
            }

            if (FlagValue != null)
            {
                customVariable["flag.value"] = JsonConvert.SerializeObject(FlagValue);
            }

            if (FlagDefaultValue != null)
            {
                customVariable["flag.default"] = JsonConvert.SerializeObject(FlagDefaultValue);
            }

            if (!string.IsNullOrWhiteSpace(FlagMetadataCampaignId))
            {
                customVariable["flag.metadata.campaignId"] = FlagMetadataCampaignId;
            }

            if (!string.IsNullOrWhiteSpace(FlagMetadataCampaignName))
            {
                customVariable["flag.metadata.campaignName"] = FlagMetadataCampaignName;
            }

            if (!string.IsNullOrWhiteSpace(FlagMetadataVariationGroupId))
            {
                customVariable["flag.metadata.variationGroupId"] = FlagMetadataVariationGroupId;
            }

            if (!string.IsNullOrWhiteSpace(FlagMetadataVariationGroupName))
            {
                customVariable["flag.metadata.variationGroupName"] = FlagMetadataVariationGroupName;
            }

            if (!string.IsNullOrWhiteSpace(FlagMetadataVariationId))
            {
                customVariable["flag.metadata.variationId"] = FlagMetadataVariationId;
            }

            if (!string.IsNullOrWhiteSpace(FlagMetadataVariationName))
            {
                customVariable["flag.metadata.variationName"] = FlagMetadataVariationName;
            }

            if (!string.IsNullOrWhiteSpace(FlagMetadataCampaignSlug))
            {
                customVariable["flag.metadata.campaignSlug"] = FlagMetadataCampaignSlug;
            }

            if (!string.IsNullOrWhiteSpace(FlagMetadataCampaignType))
            {
                customVariable["flag.metadata.campaignType"] = FlagMetadataCampaignType;
            }

            if (FlagMetadataCampaignIsReference != null)
            {
                customVariable["flag.metadata.isReference"] = FlagMetadataCampaignIsReference.GetValueOrDefault().ToString();
            }

            if (HitContent != null)
            {
                foreach (var item in HitContent)
                {
                    customVariable[item.Key] = item.Value is string value ? value : JsonConvert.SerializeObject(item.Value);
                }
            }

            if (BatchTriggeredBy != null)
            {
                customVariable["batchTriggeredBy"] = $"{BatchTriggeredBy}";
            }

            apiKeys["cv"] = customVariable;

            return apiKeys;
        }
    }
}


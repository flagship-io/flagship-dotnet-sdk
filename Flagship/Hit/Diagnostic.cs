using System;
using System.Collections.Generic;
using Flagship.Enums;
using Flagship.Model;
using Newtonsoft.Json;

namespace Flagship.Hit
{
    internal class Diagnostic : HitAbstract
    {
        public const string TROUBLESHOOTING_VERSION = "version";
        public const string LOG_LEVEL = "logLevel";
        public const string TIMESTAMP = "timestamp";
        public const string TIME_ZONE = "timeZone";
        public const string LABEL = "label";
        public const string STACK_TYPE = "stack.type";
        public const string STACK_NAME = "stack.name";
        public const string STACK_VERSION = "stack.version";
        public const string FLAGSHIP_INSTANCE_ID = "flagshipInstanceId";
        public const string LAST_INITIALIZATION_TIMESTAMP = "lastInitializationTimestamp";
        public const string LAST_BUCKETING_TIMESTAMP = "lastBucketingTimestamp";
        public const string ENV_ID = "envId";
        public const string SDK_BUCKETING_FILE = "sdkBucketingFile";
        public const string SDK_STATUS = "sdk.status";
        public const string SDK_CONFIG_MODE = "sdk.config.mode";
        public const string SDK_CONFIG_TIMEOUT = "sdk.config.timeout";
        public const string SDK_CONFIG_POLLING_TIME = "sdk.config.pollingTime";
        public const string SDK_CONFIG_TRACKING_MANAGER_STRATEGY = "sdk.config.trackingManager.strategy";
        public const string SDK_CONFIG_TRACKING_MANAGER_BATCH_INTERVALS = "sdk.config.trackingManager.batchIntervals";
        public const string SDK_CONFIG_TRACKING_MANAGER_POOL_MAX_SIZE = "sdk.config.trackingManager.poolMaxSize";
        public const string SDK_CONFIG_USING_CUSTOM_HIT_CACHE = "sdk.config.usingCustomHitCache";
        public const string SDK_CONFIG_USING_CUSTOM_VISITOR_CACHE = "sdk.config.usingCustomVisitorCache";
        public const string SDK_CONFIG_USIGN_ON_VISITOR_EXPOSED = "sdk.config.usingOnVisitorExposed";
        public const string SDK_CONFIG_DISABLE_CACHE = "sdk.config.disableCache";
        public const string METHOD = "method";
        public const string HTTP = "http";
        public const string REQUEST = "request";
        public const string HEADERS = "headers";
        public const string BODY = "body";
        public const string RESPONSE = "response";
        public const string URL = "url"; 
        public const string CODE = "code";
        public const string TIME = "time";

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
            return null;
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
                [TROUBLESHOOTING_VERSION] = Version,
                [LOG_LEVEL] = $"{LogLevel}",
                [TIMESTAMP] = Timestamp,
                [TIME_ZONE] = TimeZone,
                [LABEL] = $"{Label}",
                [STACK_TYPE] = StackType,
                [STACK_NAME] = StackName,
                [STACK_VERSION] = StackVersion
            };

            if (!string.IsNullOrWhiteSpace(FlagshipInstanceId))
            {
                customVariable[FLAGSHIP_INSTANCE_ID] = FlagshipInstanceId;
            }

            if (!string.IsNullOrWhiteSpace(LastInitializationTimestamp))
            {
                customVariable[LAST_INITIALIZATION_TIMESTAMP] = LastInitializationTimestamp;
            }

            if (!string.IsNullOrWhiteSpace(LastBucketingTimestamp))
            {
                customVariable[LAST_BUCKETING_TIMESTAMP] = LastBucketingTimestamp;
            }

            if (!string.IsNullOrWhiteSpace(Config?.EnvId))
            {
                customVariable[ENV_ID] = Config?.EnvId;
            }

            if (SdkBucketingFile!=null)
            {
                customVariable[SDK_BUCKETING_FILE] = JsonConvert.SerializeObject(SdkBucketingFile);
            }

            if (SdkStatus != null)
            {
                customVariable[SDK_STATUS] = $"{SdkStatus}";
            }

            if (SdkConfigMode != null)

            {
                customVariable[SDK_CONFIG_MODE] = $"{SdkConfigMode}";
            }

            if (SdkConfigTimeout != null)
            {
                customVariable[SDK_CONFIG_TIMEOUT] = SdkConfigTimeout.GetValueOrDefault().ToString();
            }

            if (SdkConfigPollingInterval != null)
            {
                customVariable[SDK_CONFIG_POLLING_TIME] = SdkConfigPollingInterval.GetValueOrDefault().ToString();
            }

            if (SdkConfigTrackingManagerConfigStrategy != null)
            {
                customVariable[SDK_CONFIG_TRACKING_MANAGER_STRATEGY] = $"{SdkConfigTrackingManagerConfigStrategy}";
            }

            if (SdkConfigTrackingManagerConfigBatchIntervals != null)
            {
                customVariable[SDK_CONFIG_TRACKING_MANAGER_BATCH_INTERVALS] = SdkConfigTrackingManagerConfigBatchIntervals.GetValueOrDefault().ToString();
            }

            if (SdkConfigTrackingManagerConfigPoolMaxSize != null)
            {
                customVariable[SDK_CONFIG_TRACKING_MANAGER_POOL_MAX_SIZE] = SdkConfigTrackingManagerConfigPoolMaxSize.GetValueOrDefault().ToString();
            }

            if (SdkConfigUsingCustomHitCache != null)
            {
                customVariable[SDK_CONFIG_USING_CUSTOM_HIT_CACHE] = SdkConfigUsingCustomHitCache.GetValueOrDefault().ToString();
            }

            if (SdkConfigUsingCustomVisitorCache != null)
            {
                customVariable[SDK_CONFIG_USING_CUSTOM_VISITOR_CACHE] = SdkConfigUsingCustomVisitorCache.GetValueOrDefault().ToString();
            }

            if (SdkConfigUsingOnVisitorExposed != null)
            {
                customVariable[SDK_CONFIG_USIGN_ON_VISITOR_EXPOSED] = SdkConfigUsingOnVisitorExposed.GetValueOrDefault().ToString();
            }

            if (SdkConfigDisableCache != null)
            {
                customVariable[SDK_CONFIG_DISABLE_CACHE] = SdkConfigDisableCache.GetValueOrDefault().ToString();
            }

            if (HttpRequestUrl != null)
            {
                customVariable[$"{HTTP}.{REQUEST}.{URL}"] = HttpRequestUrl;
            }

            if (HttpRequestMethod != null)
            {
                customVariable[$"{HTTP}.{REQUEST}.{METHOD}"] = HttpRequestMethod;
            }

            if (HttpRequestHeaders != null)
            {
                customVariable[$"{HTTP}.{REQUEST}.{HEADERS}"] = JsonConvert.SerializeObject(HttpRequestHeaders);
            }

            if (HttpsRequestBody != null)
            {
                customVariable[$"{HTTP}.{REQUEST}.{BODY}"] = JsonConvert.SerializeObject(HttpsRequestBody);
            }

            if (HttpResponseUrl != null)
            {
                customVariable[$"{HTTP}.{RESPONSE}.{URL}"] = HttpResponseUrl;
            }

            if (HttpResponseMethod != null)
            {
                customVariable[$"{HTTP}.{RESPONSE}.{METHOD}"] = HttpResponseMethod;
            }

            if (HttpResponseHeaders != null)
            {
                customVariable[$"{HTTP}.{RESPONSE}.{HEADERS}"] = JsonConvert.SerializeObject(HttpResponseHeaders);
            }

            if (HttpResponseCode != null)
            {
                customVariable[$"{HTTP}.{RESPONSE}.{CODE}"] = HttpResponseCode.GetValueOrDefault().ToString();
            }

            if (HttpResponseBody != null)
            {
                customVariable[$"{HTTP}.{RESPONSE}.{BODY}"] = JsonConvert.SerializeObject(HttpResponseBody);
            }

            if (HttpResponseTime != null)
            {
                customVariable[$"{HTTP}.{RESPONSE}.{TIME}"] = HttpResponseTime.GetValueOrDefault().ToString();
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


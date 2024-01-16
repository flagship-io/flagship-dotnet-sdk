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
        public const string VISITOR = "visitor";
        public const string VISITOR_ID = "visitorId";
        public const string ANONYMOUS_ID = "anonymousId";
        public const string SESSION_ID = "sessionId";
        public const string INSTANCE_TYPE = "instanceType";
        public const string CONTEXT = "context";
        public const string CONSENT = "consent";
        public const string ASSIGNMENTS = "assignments";
        public const string FLAGS = "flags";
        public const string FLAG = "flag";
        public const string METADATA = "metadata";
        public const string CAMPAIGN_ID = "campaignId";
        public const string CAMPAIGN_NAME = "campaignName";
        public const string CAMAPAIGN_TYPE = "campaignType";
        public const string VARIATION_GROUP_ID = "variationGroupId";
        public const string VARIATION_GROUP_NAME = "variationGroupName";
        public const string VARIATION_ID = "variationId";
        public const string VARIATION_NAME = "variationName";
        public const string SLUG = "slug";
        public const string IS_REFERENCE = "isReference";
        public const string CAMPAIGN_SLUG = "campaignSlug";
        public const string DEFAULT = "default";
        public const string VALUE = "value";
        public const string KEY = "key";
        public const string CONTEXT_VALUE = "contextValue";
        public const string CONTEXT_KEY = "contextKey";
        public const string CAMPAIGNS = "campaigns";
        public const string IS_AUTHENTICATED = "isAuthenticated";
        public const string BATCH_TRIGGERED_BY = "batchTriggeredBy";

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
                customVariable[$"{VISITOR}.{VISITOR_ID}"] = VisitorId;
            }

            if (AnonymousId != null)
            {
                customVariable[$"{VISITOR}.{ANONYMOUS_ID}"] = AnonymousId;
            }

            if (VisitorSessionId != null)
            {
                customVariable[$"{VISITOR}.{SESSION_ID}"] = VisitorSessionId;
            }

            if (VisitorInstanceType != null)
            {
                customVariable[$"{VISITOR}.{INSTANCE_TYPE}"] = $"{VisitorInstanceType}";
            }

            if (VisitorContext != null)
            {
                foreach (var item in VisitorContext)
                {
                    customVariable[$"{VISITOR}.{CONTEXT}.{item.Key}"] = item.Value.ToString();
                }
            }

            if (VisitorConsent != null)
            {
                customVariable[$"{VISITOR}.{CONSENT}"] = VisitorConsent.GetValueOrDefault().ToString();
            }

            if (VisitorAssignmentHistory != null)
            {
                foreach (var item in VisitorAssignmentHistory)
                {
                    customVariable[$"{VISITOR}.{ASSIGNMENTS}.[{item.Key}]"] = item.Value.ToString();
                }
            }

            if (VisitorFlags != null)
            {
                foreach (var flagDto in VisitorFlags)
                {
                    var flagKey = flagDto.Key;
                    var commonKey = $"{VISITOR}.{FLAGS}.[{flagKey}]";
                    var customVariableKeyMetadata = $"{commonKey}.{METADATA}";
                    customVariable[$"{commonKey}.{KEY}"] = $"{flagKey}";
                    customVariable[$"{commonKey}.{VALUE}"] = flagDto.Value is string value ? value : JsonConvert.SerializeObject(flagDto.Value);
                    customVariable[$"{customVariableKeyMetadata}.{CAMPAIGN_ID}"] = $"{flagDto.CampaignId}";
                    customVariable[$"{customVariableKeyMetadata}.{CAMPAIGN_NAME}"] = $"{flagDto.CampaignName}";
                    customVariable[$"{customVariableKeyMetadata}.{CAMAPAIGN_TYPE}"] = $"{flagDto.CampaignType}";
                    customVariable[$"{customVariableKeyMetadata}.{VARIATION_GROUP_ID}"] = $"{flagDto.VariationGroupId}";
                    customVariable[$"{customVariableKeyMetadata}.{VARIATION_GROUP_NAME}"] = $"{flagDto.VariationGroupName}";  
                    customVariable[$"{customVariableKeyMetadata}.{VARIATION_ID}"] = $"{flagDto.VariationId}";
                    customVariable[$"{customVariableKeyMetadata}.{VARIATION_NAME}"] = $"{flagDto.VariationName}";
                    customVariable[$"{customVariableKeyMetadata}.{SLUG}"] = $"{flagDto.Slug}";
                    customVariable[$"{customVariableKeyMetadata}.{IS_REFERENCE}"] = flagDto.IsReference.ToString();

                }
            }

            if (VisitorIsAuthenticated != null)
            {
                customVariable[$"{VISITOR}.{IS_AUTHENTICATED}"] = VisitorIsAuthenticated.GetValueOrDefault().ToString();
            }

            if (VisitorCampaigns != null)
            {
                customVariable[$"{VISITOR}.{CAMPAIGNS}"] = JsonConvert.SerializeObject(VisitorCampaigns);
            }

            if (!string.IsNullOrWhiteSpace(ContextKey))
            {
                customVariable[CONTEXT_KEY] = ContextKey;
            }

            if (ContextValue != null)
            {
                customVariable[CONTEXT_VALUE] = ContextValue.ToString();
            }

            if (!string.IsNullOrWhiteSpace(FlagKey))
            {
                customVariable[$"{FLAG}.{KEY}"] = FlagKey;
            }

            if (FlagValue != null)
            {
                customVariable[$"{FLAG}.{VALUE}"] = FlagValue is string value ? value : JsonConvert.SerializeObject(FlagValue);
            }

            if (FlagDefaultValue != null)
            {
                customVariable[$"{FLAG}.{DEFAULT}"] = FlagDefaultValue is string value ? value : JsonConvert.SerializeObject(FlagDefaultValue);
            }

            if (!string.IsNullOrWhiteSpace(FlagMetadataCampaignId))
            {
                customVariable[$"{FLAG}.{METADATA}.{CAMPAIGN_ID}"] = FlagMetadataCampaignId;
            }

            if (!string.IsNullOrWhiteSpace(FlagMetadataCampaignName))
            {
                customVariable[$"{FLAG}.{METADATA}.{CAMPAIGN_NAME}"] = FlagMetadataCampaignName;
            }

            if (!string.IsNullOrWhiteSpace(FlagMetadataVariationGroupId))
            {
                customVariable[$"{FLAG}.{METADATA}.{VARIATION_GROUP_ID}"] = FlagMetadataVariationGroupId;
            }

            if (!string.IsNullOrWhiteSpace(FlagMetadataVariationGroupName))
            {
                customVariable[$"{FLAG}.{METADATA}.{VARIATION_GROUP_NAME}"] = FlagMetadataVariationGroupName;
            }

            if (!string.IsNullOrWhiteSpace(FlagMetadataVariationId))
            {
                customVariable[$"{FLAG}.{METADATA}.{VARIATION_ID}"] = FlagMetadataVariationId;
            }

            if (!string.IsNullOrWhiteSpace(FlagMetadataVariationName))
            {
                customVariable[$"{FLAG}.{METADATA}.{VARIATION_NAME}"] = FlagMetadataVariationName;
            }

            if (!string.IsNullOrWhiteSpace(FlagMetadataCampaignSlug))
            {
                customVariable[$"{FLAG}.{METADATA}.{CAMPAIGN_SLUG}"] = FlagMetadataCampaignSlug;
            }

            if (!string.IsNullOrWhiteSpace(FlagMetadataCampaignType))
            {
                customVariable[$"{FLAG}.{METADATA}.{CAMAPAIGN_TYPE}"] = FlagMetadataCampaignType;
            }

            if (FlagMetadataCampaignIsReference != null)
            {
                customVariable[$"{FLAG}.{METADATA}.{IS_REFERENCE}"] = FlagMetadataCampaignIsReference.GetValueOrDefault().ToString();
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
                customVariable[BATCH_TRIGGERED_BY] = $"{BatchTriggeredBy}";
            }

            apiKeys["cv"] = customVariable;

            return apiKeys;
        }
    }
}


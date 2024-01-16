using Microsoft.VisualStudio.TestTools.UnitTesting;
using Flagship.Hit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flagship.Enums;
using System.Reflection.Emit;
using Flagship.Model.Bucketing;
using Flagship.Model;
using System.Diagnostics;
using Newtonsoft.Json;
using static System.Net.WebRequestMethods;
using System.Security.Policy;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Contexts;
using Newtonsoft.Json.Linq;
using Moq;

namespace Flagship.Hit.Tests
{
    [TestClass()]
    public class DiagnosticTests
    {
        [TestMethod()]
        public void DiagnosticTest()
        {
            var config = new Config.DecisionApiConfig()
            {
                EnvId = "envID",
                ApiKey = "apiKey"
            };
            var timestamp = new DateTime().ToUniversalTime().ToString("u:");
            var visitorId = "visitorId";
            var anonymousId = "anonymousId";
            var label = DiagnosticLabel.VISITOR_AUTHENTICATE;
            var logLevel = LogLevel.INFO;
            var lastInitializationTimestamp = DateTime.Now.ToString();
            var lastBucketingTimestamp = DateTime.Now.ToString();
            uint traffic = 50;
            var flagshipInstanceId = "FlagshipInstanceId";
            var sdkStatus = FlagshipStatus.READY;
            var sdkConfigMode = DecisionMode.DECISION_API;
            var sdkConfigTimeout = TimeSpan.FromSeconds(10);
            var sdkConfigPollingInterval = TimeSpan.FromSeconds(11);
            var sdkBucketingFile = new BucketingDTO();
            var sdkConfigTrackingManagerConfigStrategy = CacheStrategy.CONTINUOUS_CACHING;
            var sdkConfigTrackingManagerConfigBatchIntervals = TimeSpan.FromSeconds(12);
            var sdkConfigTrackingManagerConfigPoolMaxSize = 10;
            var sdkConfigUsingCustomHitCache = true;
            var sdkConfigUsingCustomVisitorCache = false;
            var sdkConfigUsingOnVisitorExposed = true;
            var sdkConfigDisableCache = false;
            var httpRequestUrl = "https://test.com";
            var httpRequestMethod = "POST";
            var httpRequestHeaders = new Dictionary<string, object>()
            {
                ["key"] = "value"
            };
            var httpRequestBody = new Dictionary<string, object>()
            {
                ["key-body"] = "value"
            };
            var httpResponseUrl = "https://test.com";
            var httpResponseMethod = "POST";
            var httpResponseHeaders = new Dictionary<string, object>()
            {
                ["key-response-header"] = "value"
            };
            var httpResponseCode = 200;
            var httpResponseBody = new Dictionary<string, object>()
            {
                ["key-response-body"] = "value"
            };
            var httpResponseTime = 30;
            var visitorInstanceType = InstanceType.NEW_INSTANCE;
            var VisitorContext = new Dictionary<string, object>()
            {
                ["key-context"] = "value"
            };
            var visitorConsent = true;
            var visitorAssignmentHistory = new Dictionary<string, object>()
            {
                ["campaignId"] = "variationId"
            };

            var flagDto = new FlagDTO()
            {
                Key = "key",
                Value = "value",
                CampaignId = "campaignId"
            };
            var visitorFlags = new List<FlagDTO>()
            {
                flagDto
            };
            var visitorCampaigns = new List<Model.Campaign>();
            var visitorIsAuthenticated = true;
            var visitorSessionId = "VisitorSessionId";
            var contextKey = "ContextKey";
            var contextValue = "ContextValue";
            var flagKey = "FlagKey";
            var flagValue = "FlagValue";
            var flagDefaultValue = "FlagDefaultValue";
            var visitorExposed = true;
            var flagMetadataCampaignId = "FlagMetadataCampaignId";
            var flagMetadataCampaignName = "FlagMetadataCampaignName";
            var flagMetadataVariationGroupId = "FlagMetadataVariationGroupId";
            var flagMetadataVariationGroupName = "FlagMetadataVariationGroupName";
            var flagMetadataVariationId = "FlagMetadataVariationId";
            var flagMetadataVariationName = "FlagMetadataVariationName";
            var flagMetadataCampaignSlug = "FlagMetadataCampaignSlug";
            var flagMetadataCampaignType = "FlagMetadataCampaignType";
            var flagMetadataCampaignIsReference = true;
            var hitContent = new Page("https://test.com").ToApiKeys();
            var batchTriggeredBy = CacheTriggeredBy.BatchLength;

            var diagnosticMock = new Mock<Diagnostic>(HitType.TROUBLESHOOTING) { CallBase = true };

            var currentTime = DateTime.Now;
            diagnosticMock.SetupGet(x => x.CurrentDateTime).Returns(currentTime);

            var diagnostic = diagnosticMock.Object;

            diagnostic.VisitorId = visitorId;
            diagnostic.AnonymousId = anonymousId;
            diagnostic.Config = config;
            diagnostic.Label = label;
            diagnostic.LogLevel = logLevel;
            diagnostic.LastInitializationTimestamp = lastInitializationTimestamp;
            diagnostic.LastBucketingTimestamp = lastBucketingTimestamp;
            diagnostic.Timestamp = timestamp;
            diagnostic.Traffic = traffic;
            diagnostic.FlagshipInstanceId = flagshipInstanceId;
            diagnostic.SdkStatus = sdkStatus;
            diagnostic.SdkConfigMode = sdkConfigMode;
            diagnostic.SdkConfigTimeout = sdkConfigTimeout;
            diagnostic.SdkConfigPollingInterval = sdkConfigPollingInterval;
            diagnostic.SdkBucketingFile = sdkBucketingFile;
            diagnostic.SdkConfigTrackingManagerConfigStrategy = sdkConfigTrackingManagerConfigStrategy;
            diagnostic.SdkConfigTrackingManagerConfigBatchIntervals = sdkConfigTrackingManagerConfigBatchIntervals;
            diagnostic.SdkConfigTrackingManagerConfigPoolMaxSize = sdkConfigTrackingManagerConfigPoolMaxSize;
            diagnostic.SdkConfigUsingCustomHitCache = sdkConfigUsingCustomHitCache;
            diagnostic.SdkConfigUsingCustomVisitorCache = sdkConfigUsingCustomVisitorCache;
            diagnostic.SdkConfigUsingOnVisitorExposed = sdkConfigUsingOnVisitorExposed;
            diagnostic.SdkConfigDisableCache = sdkConfigDisableCache;
            diagnostic.HttpRequestUrl = httpRequestUrl;
            diagnostic.HttpRequestMethod = httpRequestMethod;
            diagnostic.HttpRequestHeaders = httpRequestHeaders;
            diagnostic.HttpsRequestBody = httpRequestBody;
            diagnostic.HttpResponseUrl = httpResponseUrl;
            diagnostic.HttpResponseMethod = httpResponseMethod;
            diagnostic.HttpResponseHeaders = httpResponseHeaders;
            diagnostic.HttpResponseCode = httpResponseCode;
            diagnostic.HttpResponseBody = httpResponseBody;
            diagnostic.HttpResponseTime = httpResponseTime;
            diagnostic.VisitorInstanceType = visitorInstanceType;
            diagnostic.VisitorContext = VisitorContext;
            diagnostic.VisitorConsent = visitorConsent;
            diagnostic.VisitorAssignmentHistory = visitorAssignmentHistory;
            diagnostic.VisitorFlags = visitorFlags;
            diagnostic.VisitorCampaigns = visitorCampaigns;
            diagnostic.VisitorIsAuthenticated = visitorIsAuthenticated;
            diagnostic.VisitorSessionId = visitorSessionId;
            diagnostic.ContextKey = contextKey;
            diagnostic.ContextValue = contextValue;
            diagnostic.FlagKey = flagKey;
            diagnostic.FlagValue = flagValue;
            diagnostic.FlagDefaultValue = flagDefaultValue;
            diagnostic.VisitorExposed = visitorExposed;
            diagnostic.FlagMetadataCampaignId = flagMetadataCampaignId;
            diagnostic.FlagMetadataCampaignName = flagMetadataCampaignName;
            diagnostic.FlagMetadataVariationGroupId = flagMetadataVariationGroupId;
            diagnostic.FlagMetadataVariationGroupName = flagMetadataVariationGroupName;
            diagnostic.FlagMetadataVariationId = flagMetadataVariationId;
            diagnostic.FlagMetadataVariationName = flagMetadataVariationName;
            diagnostic.FlagMetadataCampaignSlug = flagMetadataCampaignSlug;
            diagnostic.FlagMetadataCampaignType = flagMetadataCampaignType;
            diagnostic.FlagMetadataCampaignIsReference = flagMetadataCampaignIsReference;
            diagnostic.HitContent = hitContent;
            diagnostic.BatchTriggeredBy = batchTriggeredBy;
      

            var keys = Newtonsoft.Json.JsonConvert.SerializeObject(diagnostic.ToApiKeys());

            var commonKey = $"{Diagnostic.VISITOR}.{Diagnostic.FLAGS}.[{flagDto.Key}]";
            var customVariableKeyMetadata = $"{commonKey}.{Diagnostic.METADATA}";


            var customVariation = new Dictionary<string, string>()
            {
                [Diagnostic.TROUBLESHOOTING_VERSION] = "1",
                [Diagnostic.LOG_LEVEL] = $"{logLevel}",
                [Diagnostic.TIMESTAMP] = timestamp,
                [Diagnostic.TIME_ZONE] = TimeZoneInfo.Local.StandardName,
                [Diagnostic.LABEL] = $"{label}",
                [Diagnostic.STACK_TYPE] = "SDK",
                [Diagnostic.STACK_NAME] = Constants.SDK_LANGUAGE,
                [Diagnostic.STACK_VERSION] = Constants.SDK_VERSION,
                [Diagnostic.FLAGSHIP_INSTANCE_ID] = flagshipInstanceId,
                [Diagnostic.LAST_INITIALIZATION_TIMESTAMP] = lastInitializationTimestamp,
                [Diagnostic.LAST_BUCKETING_TIMESTAMP] = lastBucketingTimestamp,
                [Diagnostic.ENV_ID] = config.EnvId,
                [Diagnostic.SDK_BUCKETING_FILE] = JsonConvert.SerializeObject(sdkBucketingFile),
                [Diagnostic.SDK_STATUS] = $"{sdkStatus}",
                [Diagnostic.SDK_CONFIG_MODE] = $"{sdkConfigMode}",
                [Diagnostic.SDK_CONFIG_TIMEOUT] = sdkConfigTimeout.ToString(),
                [Diagnostic.SDK_CONFIG_POLLING_TIME] = sdkConfigPollingInterval.ToString(),
                [Diagnostic.SDK_CONFIG_TRACKING_MANAGER_STRATEGY] = $"{sdkConfigTrackingManagerConfigStrategy}",
                [Diagnostic.SDK_CONFIG_TRACKING_MANAGER_BATCH_INTERVALS] = sdkConfigTrackingManagerConfigBatchIntervals.ToString(),
                [Diagnostic.SDK_CONFIG_TRACKING_MANAGER_POOL_MAX_SIZE] = sdkConfigTrackingManagerConfigPoolMaxSize.ToString(),
                [Diagnostic.SDK_CONFIG_USING_CUSTOM_HIT_CACHE] = sdkConfigUsingCustomHitCache.ToString(),
                [Diagnostic.SDK_CONFIG_USING_CUSTOM_VISITOR_CACHE] = sdkConfigUsingCustomVisitorCache.ToString(),
                [Diagnostic.SDK_CONFIG_USIGN_ON_VISITOR_EXPOSED] = sdkConfigUsingOnVisitorExposed.ToString(),
                [Diagnostic.SDK_CONFIG_DISABLE_CACHE] = sdkConfigDisableCache.ToString(),
                [$"{Diagnostic.HTTP}.{Diagnostic.REQUEST}.{Diagnostic.URL}"] = httpRequestUrl,
                [$"{Diagnostic.HTTP}.{Diagnostic.REQUEST}.{Diagnostic.METHOD}"] = httpRequestMethod,
                [$"{Diagnostic.HTTP}.{Diagnostic.REQUEST}.{Diagnostic.HEADERS}"] = JsonConvert.SerializeObject(httpRequestHeaders),
                [$"{Diagnostic.HTTP}.{Diagnostic.REQUEST}.{Diagnostic.BODY}"] = JsonConvert.SerializeObject(httpRequestBody),
                [$"{Diagnostic.HTTP}.{Diagnostic.RESPONSE}.{Diagnostic.URL}"] = httpResponseUrl,
                [$"{Diagnostic.HTTP}.{Diagnostic.RESPONSE}.{Diagnostic.METHOD}"] = httpResponseMethod,
                [$"{Diagnostic.HTTP}.{Diagnostic.RESPONSE}.{Diagnostic.HEADERS}"] = JsonConvert.SerializeObject(httpResponseHeaders),
                [$"{Diagnostic.HTTP}.{Diagnostic.RESPONSE}.{Diagnostic.CODE}"] = httpResponseCode.ToString(),
                [$"{Diagnostic.HTTP}.{Diagnostic.RESPONSE}.{Diagnostic.BODY}"] = JsonConvert.SerializeObject(httpResponseBody),
                [$"{Diagnostic.HTTP}.{Diagnostic.RESPONSE}.{Diagnostic.TIME}"] = httpResponseTime.ToString(),
                [$"{Diagnostic.VISITOR}.{Diagnostic.VISITOR_ID}"] = visitorId,
                [$"{Diagnostic.VISITOR}.{Diagnostic.ANONYMOUS_ID}"] = anonymousId,
                [$"{Diagnostic.VISITOR}.{Diagnostic.SESSION_ID}"] = visitorSessionId,
                [$"{Diagnostic.VISITOR}.{Diagnostic.INSTANCE_TYPE}"] = $"{visitorInstanceType}",
                [$"{Diagnostic.VISITOR}.{Diagnostic.CONTEXT}.key-context"] = "value",
                [$"{Diagnostic.VISITOR}.{Diagnostic.CONSENT}"] = visitorConsent.ToString(),
                [$"{Diagnostic.VISITOR}.{Diagnostic.ASSIGNMENTS}.[campaignId]"] = "variationId",
                [$"{commonKey}.{Diagnostic.KEY}"] = $"{flagDto.Key}",
                [$"{commonKey}.{Diagnostic.VALUE}"] = flagDto.Value is string flagDtoValue ? flagDtoValue : JsonConvert.SerializeObject(flagDto.Value),
                [$"{customVariableKeyMetadata}.{Diagnostic.CAMPAIGN_ID}"] = $"{flagDto.CampaignId}",
                [$"{customVariableKeyMetadata}.{Diagnostic.CAMPAIGN_NAME}"] = $"{flagDto.CampaignName}",
                [$"{customVariableKeyMetadata}.{Diagnostic.CAMAPAIGN_TYPE}"] = $"{flagDto.CampaignType}",
                [$"{customVariableKeyMetadata}.{Diagnostic.VARIATION_GROUP_ID}"] = $"{flagDto.VariationGroupId}",
                [$"{customVariableKeyMetadata}.{Diagnostic.VARIATION_GROUP_NAME}"] = $"{flagDto.VariationGroupName}",
                [$"{customVariableKeyMetadata}.{Diagnostic.VARIATION_ID}"] = $"{flagDto.VariationId}",
                [$"{customVariableKeyMetadata}.{Diagnostic.VARIATION_NAME}"] = $"{flagDto.VariationName}",
                [$"{customVariableKeyMetadata}.{Diagnostic.SLUG}"] = $"{flagDto.Slug}",
                [$"{customVariableKeyMetadata}.{Diagnostic.IS_REFERENCE}"] = flagDto.IsReference.ToString(),
                [$"{Diagnostic.VISITOR}.{Diagnostic.IS_AUTHENTICATED}"] = visitorIsAuthenticated.ToString(),
                [$"{Diagnostic.VISITOR}.{Diagnostic.CAMPAIGNS}"] = JsonConvert.SerializeObject(visitorCampaigns),
                [Diagnostic.CONTEXT_KEY] = contextKey,
                [Diagnostic.CONTEXT_VALUE] = contextValue,
                [$"{Diagnostic.FLAG}.{Diagnostic.KEY}"] = flagKey,
                [$"{Diagnostic.FLAG}.{Diagnostic.VALUE}"] = flagValue is string value ? value : JsonConvert.SerializeObject(flagValue),
                [$"{Diagnostic.FLAG}.{Diagnostic.DEFAULT}"] = flagDefaultValue is string flagDefaultValueString ? flagDefaultValueString : JsonConvert.SerializeObject(flagDefaultValue),
                [$"{Diagnostic.FLAG}.{Diagnostic.METADATA}.{Diagnostic.CAMPAIGN_ID}"] = flagMetadataCampaignId,
                [$"{Diagnostic.FLAG}.{Diagnostic.METADATA}.{Diagnostic.CAMPAIGN_NAME}"] = flagMetadataCampaignName,
                [$"{Diagnostic.FLAG}.{Diagnostic.METADATA}.{Diagnostic.VARIATION_GROUP_ID}"] = flagMetadataVariationGroupId,
                [$"{Diagnostic.FLAG}.{Diagnostic.METADATA}.{Diagnostic.VARIATION_GROUP_NAME}"] = flagMetadataVariationGroupName,
                [$"{Diagnostic.FLAG}.{Diagnostic.METADATA}.{Diagnostic.VARIATION_ID}"] = flagMetadataVariationId,
                [$"{Diagnostic.FLAG}.{Diagnostic.METADATA}.{Diagnostic.VARIATION_NAME}"] = flagMetadataVariationName,
                [$"{Diagnostic.FLAG}.{Diagnostic.METADATA}.{Diagnostic.CAMPAIGN_SLUG}"] = flagMetadataCampaignSlug,
                [$"{Diagnostic.FLAG}.{Diagnostic.METADATA}.{Diagnostic.CAMAPAIGN_TYPE}"] = flagMetadataCampaignType,
                [$"{Diagnostic.FLAG}.{Diagnostic.METADATA}.{Diagnostic.IS_REFERENCE}"] = flagMetadataCampaignIsReference.ToString()
            };


            foreach ( var hitKey in hitContent)
            {
                customVariation[hitKey.Key] = hitKey.Value is string hitValue ? hitValue : JsonConvert.SerializeObject(hitKey.Value);
            }

            customVariation[Diagnostic.BATCH_TRIGGERED_BY] = $"{batchTriggeredBy}";

            var apiKeys = new Dictionary<string, object>()
            {
                [Constants.VISITOR_ID_API_ITEM] = visitorId,
                [Constants.DS_API_ITEM] = "APP",
                [Constants.CUSTOMER_ENV_ID_API_ITEM] = config.EnvId,
                [Constants.T_API_ITEM] = $"{HitType.TROUBLESHOOTING}",
                ["cv"] = customVariation
            };

            var apiKeysJson = Newtonsoft.Json.JsonConvert.SerializeObject(apiKeys);

            Assert.AreEqual(apiKeysJson, keys);
        }
    }
}
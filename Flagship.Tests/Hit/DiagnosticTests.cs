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

            var visitorId = "visitorId";
            var anonymousId = "anonymousId";
            var label = DiagnosticLabel.VISITOR_AUTHENTICATE;
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
            var httpsRequestBody = new Dictionary<string, object>()
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
            var visitorFlags = new List<FlagDTO>()
            {
                new FlagDTO()
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
            var hitContent = new Page("https://test.com");
            var batchTriggeredBy = CacheTriggeredBy.BatchLength;

            var diagnostic = new Diagnostic(HitType.TROUBLESHOOTING)
            {
                VisitorId= visitorId,
                AnonymousId = anonymousId,
                Config = config,
                Label = label,
                LastInitializationTimestamp = lastInitializationTimestamp,
                LastBucketingTimestamp = lastBucketingTimestamp,
                Traffic = traffic,
                FlagshipInstanceId = flagshipInstanceId,
                SdkStatus = sdkStatus,
                SdkConfigMode = sdkConfigMode,
                SdkConfigTimeout = sdkConfigTimeout,
                SdkConfigPollingInterval = sdkConfigPollingInterval,
                SdkBucketingFile = sdkBucketingFile,
                SdkConfigTrackingManagerConfigStrategy = sdkConfigTrackingManagerConfigStrategy,
                SdkConfigTrackingManagerConfigBatchIntervals = sdkConfigTrackingManagerConfigBatchIntervals,
                SdkConfigTrackingManagerConfigPoolMaxSize = sdkConfigTrackingManagerConfigPoolMaxSize,
                SdkConfigUsingCustomHitCache = sdkConfigUsingCustomHitCache,
                SdkConfigUsingCustomVisitorCache = sdkConfigUsingCustomVisitorCache,
                SdkConfigUsingOnVisitorExposed = sdkConfigUsingOnVisitorExposed,
                SdkConfigDisableCache = sdkConfigDisableCache,
                HttpRequestUrl = httpRequestUrl,
                HttpRequestMethod = httpRequestMethod,
                HttpRequestHeaders = httpRequestHeaders,
                HttpsRequestBody = httpsRequestBody,
                HttpResponseUrl = httpResponseUrl,
                HttpResponseMethod = httpResponseMethod,
                HttpResponseHeaders = httpResponseHeaders,
                HttpResponseCode = httpResponseCode,
                HttpResponseBody = httpResponseBody,
                HttpResponseTime = httpResponseTime,
                VisitorInstanceType = visitorInstanceType,
                VisitorContext = VisitorContext,
                VisitorConsent = visitorConsent,
                VisitorAssignmentHistory = visitorAssignmentHistory,
                VisitorFlags = visitorFlags,
                VisitorCampaigns = visitorCampaigns,
                VisitorIsAuthenticated = visitorIsAuthenticated,
                VisitorSessionId = visitorSessionId,
                ContextKey = contextKey,
                ContextValue = contextValue,
                FlagKey = flagKey,
                FlagValue = flagValue,
                FlagDefaultValue = flagDefaultValue,
                VisitorExposed = visitorExposed,
                FlagMetadataCampaignId = flagMetadataCampaignId,
                FlagMetadataCampaignName = flagMetadataCampaignName,
                FlagMetadataVariationGroupId = flagMetadataVariationGroupId,
                FlagMetadataVariationGroupName= flagMetadataVariationGroupName,
                FlagMetadataVariationId = flagMetadataVariationId,
                FlagMetadataVariationName = flagMetadataVariationName,
                FlagMetadataCampaignSlug = flagMetadataCampaignSlug,
                FlagMetadataCampaignType = flagMetadataCampaignType,
                FlagMetadataCampaignIsReference = flagMetadataCampaignIsReference,
                HitContent = hitContent.ToApiKeys(),
                BatchTriggeredBy = batchTriggeredBy
            };

            var keys = Newtonsoft.Json.JsonConvert.SerializeObject(diagnostic.ToApiKeys());

            var apiKeys = new Dictionary<string, string>();
            {

            }

            var apiKeysJson = Newtonsoft.Json.JsonConvert.SerializeObject(apiKeys);

            Assert.AreEqual(apiKeysJson, keys);
        }
    }
}
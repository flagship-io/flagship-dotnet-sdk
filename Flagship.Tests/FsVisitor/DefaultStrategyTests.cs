using Microsoft.VisualStudio.TestTools.UnitTesting;
using Flagship.FsVisitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Flagship.Enums;
using Newtonsoft.Json.Linq;
using Flagship.Tests.Data;
using Newtonsoft.Json;
using Flagship.Logger;
using Flagship.Cache;
using Flagship.Model;
using System.Collections.ObjectModel;
using Flagship.Hit;
using Flagship.Config;

namespace Flagship.FsVisitor.Tests
{
    [TestClass()]
    public class DefaultStrategyTests
    {
        private Mock<IFsLogManager> fsLogManagerMock;
        private VisitorDelegate visitorDelegate;
        private Mock<Flagship.Decision.DecisionManager> decisionManagerMock;
        private Mock<Flagship.Api.ITrackingManager> trackingManagerMock;
        private DecisionApiConfig config;
        public DefaultStrategyTests()
        {
            fsLogManagerMock = new Mock<IFsLogManager>();
            config = new Flagship.Config.DecisionApiConfig()
            {
                EnvId = "envID",
                LogManager = fsLogManagerMock.Object,
                DisableDeveloperUsageTracking = true,
                TrackingManagerConfig = new TrackingManagerConfig()
            };

            trackingManagerMock = new Mock<Flagship.Api.ITrackingManager>();
            var trackingManager = trackingManagerMock.Object;

            decisionManagerMock = new Mock<Flagship.Decision.DecisionManager>(new object[] { null, null });
            
            var decisionManager = decisionManagerMock.Object;
            decisionManager.TrackingManager = trackingManager;

            var configManager = new Flagship.Config.ConfigManager(config, decisionManager, trackingManagerMock.Object);

            var context = new Dictionary<string, object>()
            {
                ["key0"] = 1,
            };

            visitorDelegate = new Flagship.FsVisitor.VisitorDelegate("visitorId", false, context, false, configManager);

        }

        [TestMethod()]
        public void DefaultStrategyTest()
        {

        }


        [TestMethod()]
        public void UpdateContexTest()
        {

            var defaultStrategy = new DefaultStrategy(visitorDelegate);

            var newContext = new Dictionary<string, object>()
            {
                ["key1"] = "value1",
                ["key2"] = "value2"
            };

            defaultStrategy.UpdateContext(newContext);

            Assert.AreEqual(visitorDelegate.Context.Count, 6);
            Assert.AreEqual(visitorDelegate.FetchFlagsStatus.Status, FSFetchStatus.FETCH_REQUIRED);
            Assert.AreEqual(visitorDelegate.FetchFlagsStatus.Reason,  FSFetchReasons.UPDATE_CONTEXT);

            var newContext2 = new Dictionary<string, object>()
            {
                ["key3"] = 5,
                ["key4"] = 1
            };

            defaultStrategy.UpdateContext(newContext2);

            Assert.AreEqual(visitorDelegate.Context.Count, 8);

            var newContext3 = new Dictionary<string, object>()
            {
                ["key5"] = true,
                ["key6"] = false
            };

            defaultStrategy.UpdateContext(newContext3);

            Assert.AreEqual(visitorDelegate.Context.Count, 10);

            defaultStrategy.UpdateContext("key1", "value3");
            Assert.AreEqual(visitorDelegate.Context["key1"], "value3");

            defaultStrategy.UpdateContext("key3", 10);
            Assert.AreEqual(10d, visitorDelegate.Context["key3"]);

            defaultStrategy.UpdateContext("key5", false);
            Assert.AreEqual(visitorDelegate.Context["key5"], false);

            defaultStrategy.UpdateContext(null);

            var newContext4 = new Dictionary<string, object>()
            {
                ["key6"] = new object(),
            };

            defaultStrategy.UpdateContext(newContext4);
            Assert.AreEqual(visitorDelegate.Context.Count, 10);
            fsLogManagerMock.Verify(x => x.Error(string.Format(Constants.CONTEXT_PARAM_ERROR, "key6"), "UpdateContex"), Times.Once());

            //Test clearContext

            defaultStrategy.ClearContext();
            Assert.AreEqual(visitorDelegate.Context.Count, 0);
        }

        [TestMethod()]
        public void PredefinedContextTest()
        {
            var defaultStrategy = new DefaultStrategy(visitorDelegate);

            var newContext = new Dictionary<string, object>()
            {
                ["key1"] = "value1",
                [PredefinedContext.LOCATION_CITY] = "London",
                [PredefinedContext.OS_VERSION_CODE] = 1,
                [PredefinedContext.APP_VERSION_CODE] = "1",
                [PredefinedContext.DEVICE_LOCALE] = Array.Empty<string>(),
                [PredefinedContext.DEVICE_MODEL] = null,
                [PredefinedContext.FLAGSHIP_CLIENT] = "custom client"
            };

            defaultStrategy.UpdateContext(newContext);

            Assert.AreEqual(visitorDelegate.Context.Count, 7);
            Assert.AreEqual(visitorDelegate.Context[PredefinedContext.LOCATION_CITY], "London");

            fsLogManagerMock.Verify(x => x.Error(string.Format(Constants.PREDEFINED_CONTEXT_TYPE_ERROR,
                PredefinedContext.APP_VERSION_CODE, "number"), "UpdateContext"), Times.Once());

            fsLogManagerMock.Verify(x => x.Error(string.Format(Constants.PREDEFINED_CONTEXT_TYPE_ERROR,
                PredefinedContext.DEVICE_LOCALE, "string"), "UpdateContext"), Times.Once());

            fsLogManagerMock.Verify(x => x.Error(string.Format(Constants.PREDEFINED_CONTEXT_TYPE_ERROR,
                PredefinedContext.DEVICE_MODEL, "string"), "UpdateContext"), Times.Once());
        }

        [TestMethod()]
        async public Task FetchFlagsFailedTest()
        {
            var errorMessage = "error";
            decisionManagerMock.Setup(x => x.GetCampaigns(visitorDelegate))
                .Throws(new Exception(errorMessage));

            var defaultStrategy = new DefaultStrategy(visitorDelegate);

            await defaultStrategy.FetchFlags().ConfigureAwait(false);

            fsLogManagerMock.Verify(x => x.Error(errorMessage, "FetchFlags"), Times.Once());
            Assert.AreEqual(visitorDelegate.Flags.Count, 0);
        }

        [TestMethod()]
        async public Task FetchFlagsTest()
        {
            decisionManagerMock.Setup(x => x.GetCampaigns(visitorDelegate))
                .Returns(Task.FromResult(CampaignsData.DecisionResponse().Campaigns));

            var defaultStrategy = new DefaultStrategy(visitorDelegate);

            await defaultStrategy.FetchFlags().ConfigureAwait(false);
            Assert.AreEqual(visitorDelegate.FetchFlagsStatus.Status, FSFetchStatus.FETCHED);
            Assert.AreEqual(visitorDelegate.FetchFlagsStatus.Reason, FSFetchReasons.NONE);

            Assert.AreEqual(visitorDelegate.Flags.Count, 6);
        }

        [TestMethod()]
        async public Task FetchFlagsPanicModeTest() 
        {
            decisionManagerMock.SetupGet(x=> x.IsPanic).Returns(true);
            decisionManagerMock.Setup(x => x.GetCampaigns(visitorDelegate))
                .Returns(Task.FromResult(CampaignsData.DecisionResponse().Campaigns));

            var defaultStrategy = new DefaultStrategy(visitorDelegate);

            await defaultStrategy.FetchFlags().ConfigureAwait(false);
            Assert.AreEqual(visitorDelegate.FetchFlagsStatus.Status, FSFetchStatus.PANIC);
            Assert.AreEqual(visitorDelegate.FetchFlagsStatus.Reason, FSFetchReasons.NONE);

            Assert.AreEqual(visitorDelegate.Flags.Count, 6);
        }

        [TestMethod()]
        async public Task FetchFlagsWithCacheTest()
        {
            ICollection<Campaign> campaigns = new Collection<Flagship.Model.Campaign>();
            decisionManagerMock.Setup(x => x.GetCampaigns(visitorDelegate))
                .Returns(Task.FromResult(campaigns));

            visitorDelegate.VisitorCache = new Model.VisitorCache
            {
                Version = 1,
                Data = new VisitorCacheDTOV1()
                {
                    Version = 1,
                    Data = new VisitorCacheData
                    {
                        VisitorId = "visitorID",
                        Consent = true,
                        Campaigns = new List<VisitorCacheCampaign>
                        {
                            new VisitorCacheCampaign
                            {
                                Activated = true,
                                CampaignId = "campaignID",
                                IsReference = true,
                                Type= ModificationType.FLAG,
                                VariationGroupId = "variationGbId",
                                VariationId = "variationID",
                                Flags = new Dictionary<string, object>
                                {
                                    ["key"] = "value"
                                }
                            }
                        }
                    }
                }
            };
            var defaultStrategy = new DefaultStrategy(visitorDelegate);

            await defaultStrategy.FetchFlags().ConfigureAwait(false);

            Assert.AreEqual(1, visitorDelegate.Flags.Count);
        }

        [TestMethod()]
        async public Task FetchFlagsWithCacheV2Test()
        {
            ICollection<Campaign> campaigns = new Collection<Flagship.Model.Campaign>();

            decisionManagerMock.Setup(x => x.GetCampaigns(visitorDelegate))
                .Returns(Task.FromResult(campaigns));

            visitorDelegate.VisitorCache = new Model.VisitorCache
            {
                Version = 2,
                Data = new VisitorCacheDTOV1()
                {
                    Version = 2,
                    Data = new VisitorCacheData
                    {
                    }
                }
            };
            var defaultStrategy = new DefaultStrategy(visitorDelegate);

            await defaultStrategy.FetchFlags().ConfigureAwait(false);

            Assert.AreEqual(visitorDelegate.Flags.Count, 0);

            visitorDelegate.VisitorCache = new Model.VisitorCache
            {
                Version = 1,
                Data = new VisitorCacheDTOV1()
                {
                    Version = 1
                }
            };
            defaultStrategy = new DefaultStrategy(visitorDelegate);

            await defaultStrategy.FetchFlags().ConfigureAwait(false);

            Assert.AreEqual(visitorDelegate.Flags.Count, 0);

            visitorDelegate.VisitorCache = null;

            defaultStrategy = new DefaultStrategy(visitorDelegate);

            await defaultStrategy.FetchFlags().ConfigureAwait(false);

            Assert.AreEqual(visitorDelegate.Flags.Count, 0);
        }

        [TestMethod()]
        public async Task VisitorExposedTest()
        {
            const string functionName = "VisitorExposed";
            var defaultStrategy = new DefaultStrategy(visitorDelegate);

            var key = "key1";
            await defaultStrategy.VisitorExposed(key, "defaultValue", null).ConfigureAwait(false);

            fsLogManagerMock.Verify(x => x.Error(string.Format(Constants.GET_FLAG_ERROR, key), functionName), Times.Once());

            var flagDto = CampaignsData.GetFlag()[0];
            await defaultStrategy.VisitorExposed(flagDto.Key, 1, flagDto).ConfigureAwait(false);

            fsLogManagerMock.Verify(x => x.Error(string.Format(Constants.USER_EXPOSED_CAST_ERROR, flagDto.Key), functionName), Times.Once());
            trackingManagerMock.Verify(x => x.SendTroubleshootingHit(It.Is<Troubleshooting>(y => y.Type == HitType.TROUBLESHOOTING)), Times.Exactly(2));

            await defaultStrategy.VisitorExposed(flagDto.Key, "defaultValueString", flagDto).ConfigureAwait(false);

            var activate = new Activate(flagDto.VariationGroupId, flagDto.VariationId);

            trackingManagerMock.Verify(x => x.ActivateFlag(It.Is<Activate>(
                y => y.VariationGroupId == flagDto.VariationGroupId && y.VariationId == flagDto.VariationId)), Times.Once());

            var flagDtoValueNull = CampaignsData.GetFlag()[1];

            await defaultStrategy.VisitorExposed(flagDtoValueNull.Key, "defaultValueString", flagDtoValueNull).ConfigureAwait(false);

            trackingManagerMock.Verify(x => x.ActivateFlag(It.Is<Activate>(
                y => y.VariationGroupId == flagDtoValueNull.VariationGroupId && y.VariationId == flagDtoValueNull.VariationId)
                ), Times.Once());

        }

        [TestMethod()]
        public void GetFlagValueWithFlagNullTest()
        {
            const string functionName = "getFlag.value";
            var defaultStrategy = new DefaultStrategy(visitorDelegate);
            var defaultValueString = "defaultValueString";
            var key = "key 1";
            var value = defaultStrategy.GetFlagValue(key, defaultValueString, null);
            Assert.AreEqual(defaultValueString, value);
            fsLogManagerMock.Verify(x => x.Info(string.Format(Constants.GET_FLAG_MISSING_ERROR, key), functionName), Times.Once());
            trackingManagerMock.Verify(x => x.SendTroubleshootingHit(It.Is<Troubleshooting>(y => y.Type == HitType.TROUBLESHOOTING)), Times.Once());
        }

        [TestMethod()]
        public void GetFlagValueWithValueNullTest()
        {
            var defaultStrategy = new DefaultStrategy(visitorDelegate);

            var defaultValueString = "defaultValueString";
            var flagDtoValueNull = CampaignsData.GetFlag()[1];

            var value2 = defaultStrategy.GetFlagValue(flagDtoValueNull.Key, defaultValueString, flagDtoValueNull);
            Assert.AreEqual(defaultValueString, value2);

            trackingManagerMock.Verify(x => x.ActivateFlag(It.Is<Activate>(
                y => y.VariationGroupId == flagDtoValueNull.VariationGroupId && y.VariationId == flagDtoValueNull.VariationId)), Times.Once());
        }

        [TestMethod()]
        public void GetFlagValueTypeDifferent()
        {
            const string functionName = "getFlag.value";
            var defaultStrategy = new DefaultStrategy(visitorDelegate);
            var flagDto = CampaignsData.GetFlag()[0];

            var value3 = defaultStrategy.GetFlagValue(flagDto.Key, 1, flagDto);
            Assert.AreEqual(1, value3);

            fsLogManagerMock.Verify(x => x.Info(string.Format(Constants.GET_FLAG_CAST_ERROR, flagDto.Key), functionName), Times.Once());
            var activate = new Activate(flagDto.VariationGroupId, flagDto.VariationId);
            trackingManagerMock.Verify(x => x.Add(activate), Times.Never());
            trackingManagerMock.Verify(x => x.SendTroubleshootingHit(It.Is<Troubleshooting>(y => y.Type == HitType.TROUBLESHOOTING)), Times.Once());
        }

        [TestMethod()]
        public void GetFlagValueWithUserExposedFalse()
        {
            var defaultStrategy = new DefaultStrategy(visitorDelegate);
            var flagDto = CampaignsData.GetFlag()[0];

            var value = defaultStrategy.GetFlagValue(flagDto.Key, "Default", flagDto, false);
            Assert.AreEqual(flagDto.Value, value);
            trackingManagerMock.Verify(x => x.ActivateFlag(It.Is<Activate>(
                y => y.VariationGroupId == flagDto.VariationGroupId && y.VariationId == flagDto.VariationId)), Times.Never());
        }

        [TestMethod()]
        public void GetFlagValue()
        {
            var defaultStrategy = new DefaultStrategy(visitorDelegate);
            var flagDto = CampaignsData.GetFlag()[0];

            var value = defaultStrategy.GetFlagValue(flagDto.Key, "Default", flagDto);
            Assert.AreEqual(flagDto.Value, value);
       
            trackingManagerMock.Verify(x => x.ActivateFlag(It.Is<Activate>(
                y=>y.VariationGroupId== flagDto.VariationGroupId && y.VariationId == flagDto.VariationId)
                ), Times.Once());
        }

        [TestMethod()]
        public void GetFlagMetadataFailedTest()
        {
            const string functionName = "flag.metadata";
            var defaultStrategy = new DefaultStrategy(visitorDelegate);

            var metadata = new FsFlag.FlagMetadata("CampaignId", "variationGroupId", "variationId", false, "", null, "CampaignName", "VariationGroupName", "VariationName");
            var resultatMetadata = defaultStrategy.GetFlagMetadata("key", null);

            Assert.AreEqual(JsonConvert.SerializeObject(FsFlag.FlagMetadata.EmptyMetadata()), JsonConvert.SerializeObject(resultatMetadata));
            fsLogManagerMock.Verify(x => x.Error(string.Format(Constants.GET_METADATA_CAST_ERROR, "key"), functionName), Times.Once());
            trackingManagerMock.Verify(x => x.SendTroubleshootingHit(It.Is<Troubleshooting>(y=> y.Type == HitType.TROUBLESHOOTING)), Times.Once());
        }

        [TestMethod()]
        public void GetFlagMetadataTest()
        {
            const string functionName = "flag.metadata";
            var defaultStrategy = new DefaultStrategy(visitorDelegate);

            var metadata = new FsFlag.FlagMetadata("CampaignId", "variationGroupId", "variationId", false, "", null, "CampaignName", "VariationGroupName", "VariationName");
            var resultatMetadata = defaultStrategy.GetFlagMetadata("key", null);

            Assert.AreEqual(JsonConvert.SerializeObject(metadata), JsonConvert.SerializeObject(resultatMetadata));
            fsLogManagerMock.Verify(x => x.Error(string.Format(Constants.GET_METADATA_CAST_ERROR, "key"), functionName), Times.Never());
        }

        [TestMethod()]
        public async Task SendHitWithHitNullTest()
        {
            const string functionName = "SendHit";
            var defaultStrategy = new DefaultStrategy(visitorDelegate);
            await defaultStrategy.SendHit(hit: null).ConfigureAwait(false);
            fsLogManagerMock.Verify(x => x.Error(Constants.HIT_NOT_NULL, functionName), Times.Once());
        }

        [TestMethod()]
        public async Task SendHitNotReadyTest()
        {
            const string functionName = "SendHit";
            var defaultStrategy = new DefaultStrategy(visitorDelegate);
            var hit = new Flagship.Hit.Screen(null);
            await defaultStrategy.SendHit(hit).ConfigureAwait(false);
            fsLogManagerMock.Verify(x => x.Error(hit.GetErrorMessage(), functionName), Times.Once());
        }

        [TestMethod()]
        public async Task SendHitFailedTest()
        {
            const string functionName = "SendHit";
            var errorMessage = "error hit";
            var defaultStrategy = new DefaultStrategy(visitorDelegate);
            var hit = new Flagship.Hit.Screen("HomeView");
            trackingManagerMock.Setup(x => x.Add(hit)).Throws(new Exception(errorMessage));

            await defaultStrategy.SendHit(hit).ConfigureAwait(false);
            trackingManagerMock.Verify(x => x.Add(hit), Times.Once());

            fsLogManagerMock.Verify(x => x.Error(errorMessage, functionName), Times.Once());
        }

        [TestMethod()]
        public async Task SendHitTest()
        {
            var defaultStrategy = new DefaultStrategy(visitorDelegate);
            var hit = new Screen("HomeView");
            await defaultStrategy.SendHit(hit).ConfigureAwait(false);
            trackingManagerMock.Verify(x => x.Add(hit), Times.Once());
            trackingManagerMock.Verify(x => x.SendTroubleshootingHit(It.Is<Troubleshooting>(y => y.Type == HitType.TROUBLESHOOTING)));
        }

        [TestMethod()]
        public async Task SendHitsTest()
        {
            var defaultStrategy = new DefaultStrategy(visitorDelegate);
            var screen = new Flagship.Hit.Screen("HomeView");
            var page = new Flagship.Hit.Page("HomePage");
            await defaultStrategy.SendHit(new List<Hit.HitAbstract>() { screen, page }).ConfigureAwait(false);
            trackingManagerMock.Verify(x => x.Add(screen), Times.Once());
            trackingManagerMock.Verify(x => x.Add(page), Times.Once());
        }

        [TestMethod()]
        public async Task SendConsentHitAsyncTest()
        {
            var defaultStrategy = new DefaultStrategy(visitorDelegate);

            await defaultStrategy.SendConsentHitAsync(true).ConfigureAwait(false);

            trackingManagerMock.Verify(x => x.Add(It.Is<Hit.Event>(
                item => item.Label == $"{Constants.SDK_LANGUAGE}:{true}" &&
                item.VisitorId == visitorDelegate.VisitorId &&
                item.DS == Constants.SDK_APP &&
                item.AnonymousId == visitorDelegate.AnonymousId
                )), Times.Once());
        }

        [TestMethod]
        public void ExperienceContinuityTest()
        {
            var visitorId = visitorDelegate.VisitorId;

            var defaultStrategy = new DefaultStrategy(visitorDelegate);

            var newVisitorId = "newVisitorId";

            defaultStrategy.Authenticate(newVisitorId);

            Assert.AreEqual(visitorId, visitorDelegate.AnonymousId);
            Assert.AreEqual(newVisitorId, visitorDelegate.VisitorId);
            Assert.AreEqual(visitorDelegate.FetchFlagsStatus.Status, FSFetchStatus.FETCH_REQUIRED);
            Assert.AreEqual(visitorDelegate.FetchFlagsStatus.Reason, FSFetchReasons.AUTHENTICATE);
            trackingManagerMock.Verify(x => x.SendTroubleshootingHit(It.Is<Troubleshooting>(y => y.Type == HitType.TROUBLESHOOTING)));

            defaultStrategy.Authenticate(null);

            Assert.AreEqual(visitorId, visitorDelegate.AnonymousId);
            Assert.AreEqual(newVisitorId, visitorDelegate.VisitorId);

            string methodName = "Authenticate";

            fsLogManagerMock.Verify(x => x.Error(string.Format(Constants.VISITOR_ID_ERROR, methodName), methodName), Times.Once());

            // Bucketing mode test
            var config = new Flagship.Config.BucketingConfig()
            {
                EnvId = "envID",
                LogManager = fsLogManagerMock.Object,
            };

            visitorDelegate.ConfigManager.Config = config;


            defaultStrategy.Authenticate("newVisitor2");

            Assert.AreEqual(visitorId, visitorDelegate.AnonymousId);
            Assert.AreEqual(newVisitorId, visitorDelegate.VisitorId);

            fsLogManagerMock.Verify(x => x.Error(string.Format(Constants.METHOD_DEACTIVATED_BUCKETING_ERROR, methodName), methodName), Times.Once());

            // Unauthenticate bucketing mode test

            defaultStrategy.Unauthenticate();


            Assert.AreEqual(visitorId, visitorDelegate.AnonymousId);
            Assert.AreEqual(newVisitorId, visitorDelegate.VisitorId);
            trackingManagerMock.Verify(x => x.SendTroubleshootingHit(It.Is<Troubleshooting>(y => y.Type == HitType.TROUBLESHOOTING)));

            methodName = "Unauthenticate";

            fsLogManagerMock.Verify(x => x.Error(string.Format(Constants.METHOD_DEACTIVATED_BUCKETING_ERROR, methodName), methodName), Times.Once());


            visitorDelegate.ConfigManager.Config = new Flagship.Config.DecisionApiConfig()
            {
                EnvId = "envID",
                LogManager = fsLogManagerMock.Object,
            };

            defaultStrategy.Unauthenticate();

            Assert.IsNull(visitorDelegate.AnonymousId);
            Assert.AreEqual(visitorId, visitorDelegate.VisitorId);

            Assert.AreEqual(visitorDelegate.FetchFlagsStatus.Status, FSFetchStatus.FETCH_REQUIRED);
            Assert.AreEqual(visitorDelegate.FetchFlagsStatus.Reason, FSFetchReasons.UNAUTHENTICATE);

            defaultStrategy.Unauthenticate();

            fsLogManagerMock.Verify(x => x.Error(string.Format(Constants.FLAGSHIP_VISITOR_NOT_AUTHENTICATE, methodName), methodName), Times.Once());

        }

        [TestMethod]
        public void LookupVisitorTest()
        {
            var visitorCache = new Mock<IVisitorCacheImplementation>();
            var visitorId = visitorDelegate.VisitorId;
            visitorDelegate.VisitorCache = null;
            visitorDelegate.Config.VisitorCacheImplementation = visitorCache.Object;

            var defaultStrategy = new DefaultStrategy(visitorDelegate);

            ICollection<Campaign> campaigns = new Collection<Campaign>()
            {
                new Campaign()
                {
                    Id = "id",
                    Variation = new Variation
                    {
                        Id = "varID",
                        Modifications = new Modifications
                        {
                            Type = ModificationType.FLAG,
                            Value = new Dictionary<string, object>
                            {
                                ["key"] = "value"
                            }
                        },
                        Reference = false
                    },
                    Type = "ab",
                    VariationGroupId = "varGroupId"
                }
            };

            var VisitorCacheCampaigns = new Collection<VisitorCacheCampaign>();

            foreach (var item in campaigns)
            {
                VisitorCacheCampaigns.Add(new VisitorCacheCampaign
                {
                    CampaignId = item.Id,
                    VariationGroupId = item.VariationGroupId,
                    VariationId = item.Variation.Id,
                    IsReference = item.Variation.Reference,
                    Type = item.Variation.Modifications.Type,
                    Activated = false,
                    Flags = item.Variation.Modifications.Value
                });
            }


            var failedData = new VisitorCacheDTOV1 
            {
                Version = 1,
                Data = new VisitorCacheData
                {
                    VisitorId = "any",
                    AnonymousId = visitorDelegate.AnonymousId,
                    Consent = visitorDelegate.HasConsented,
                    Context = visitorDelegate.Context,
                    Campaigns = VisitorCacheCampaigns
                }
            };

            visitorCache.Setup(x => x.LookupVisitor(visitorId)).Returns(Task.FromResult(JObject.FromObject(failedData)));

            defaultStrategy.LookupVisitor();

            fsLogManagerMock.Verify(x => x.Info(string.Format(StrategyAbstract.VISITOR_ID_MISMATCH_ERROR, "any", visitorId), "LookupVisitor"), Times.Once());

            Assert.IsNull(visitorDelegate.VisitorCache);

            var data = new VisitorCacheDTOV1
            {
                Version = 1,
                Data = new VisitorCacheData
                {
                    VisitorId = visitorDelegate.VisitorId,
                    AnonymousId = visitorDelegate.AnonymousId,
                    Consent = visitorDelegate.HasConsented,
                    Context = visitorDelegate.Context,
                    Campaigns = VisitorCacheCampaigns
                }
            };

            var dataJson = JObject.FromObject(data);

            visitorCache.Setup(x => x.LookupVisitor(visitorId)).Returns(Task.FromResult(dataJson));

            defaultStrategy.LookupVisitor();

            Assert.IsNotNull(visitorDelegate.VisitorCache);

            Assert.AreEqual(JsonConvert.SerializeObject(visitorDelegate.VisitorCache.Data), JsonConvert.SerializeObject(data));

            var error = new Exception("LookupVisitor error");

            visitorCache.Setup(x => x.LookupVisitor(visitorId)).Throws(error);

            defaultStrategy.LookupVisitor();

            fsLogManagerMock.Verify(x => x.Error(error.Message, "LookupVisitor"), Times.Once());

            visitorCache.Setup(x => x.LookupVisitor(visitorId)).Returns(Task.FromResult(new JObject()));

            defaultStrategy.LookupVisitor();

            fsLogManagerMock.Verify(x => x.Error(StrategyAbstract.LOOKUP_VISITOR_JSON_OBJECT_ERROR, "LookupVisitor"), Times.Once());

            visitorCache.Setup(x => x.LookupVisitor(visitorId)).Returns(Task.FromResult<JObject>(null));

            defaultStrategy.LookupVisitor();
        }

        [TestMethod]
        public void VisitorCacheTest()
        {
            ICollection<Campaign> campaigns = new Collection<Campaign>()
            {
                new Campaign()
                {
                    Id = "id",
                    Variation = new Variation
                    {
                        Id = "varID",
                        Modifications = new Modifications
                        {
                            Type = ModificationType.FLAG,
                            Value = new Dictionary<string, object>
                            {
                                ["key"] = "value"
                            }
                        },
                        Reference = false
                    },
                    Type = "ab",
                    VariationGroupId = "varGroupId"
                }
            };

            var VisitorCacheCampaigns = new Collection<VisitorCacheCampaign>();

            var variationHistory = new Dictionary<string, string>
            {
                ["varGrID"] = "varID"
            };

            foreach (var item in campaigns)
            {
                variationHistory[item.VariationGroupId] = item.Variation.Id;

                VisitorCacheCampaigns.Add(new VisitorCacheCampaign
                {
                    CampaignId = item.Id,
                    VariationGroupId = item.VariationGroupId,
                    VariationId = item.Variation.Id,
                    IsReference = item.Variation.Reference,
                    Type = item.Variation.Modifications.Type,
                    Activated = false,
                    Flags = item.Variation.Modifications.Value
                });
            }



            var data = new VisitorCacheDTOV1
            {
                Version = 1,
                Data = new VisitorCacheData
                {
                    VisitorId = visitorDelegate.VisitorId,
                    AnonymousId = visitorDelegate.AnonymousId,
                    Consent = visitorDelegate.HasConsented,
                    Context = visitorDelegate.Context,
                    Campaigns = VisitorCacheCampaigns,
                    AssignmentsHistory= variationHistory
                }
            };



            visitorDelegate.VisitorCache = new VisitorCache
            {
                Version = 1,
                Data = new VisitorCacheDTOV1
                {
                    Version = 1,
                    Data = new VisitorCacheData
                    {
                        VisitorId = visitorDelegate.VisitorId,
                        AnonymousId = visitorDelegate.AnonymousId,
                        Consent = visitorDelegate.HasConsented,
                        Context = visitorDelegate.Context,
                        AssignmentsHistory = new Dictionary<string, string>
                        {
                            ["varGrID"] = "varID"
                        }
                    }
                }
            };

            var dataJson = JObject.FromObject(data);

            var visitorCache = new Mock<IVisitorCacheImplementation>();
            var visitorId = visitorDelegate.VisitorId;
            visitorDelegate.Config.VisitorCacheImplementation = visitorCache.Object;

            visitorDelegate.Campaigns = campaigns;

            var defaultStrategy = new DefaultStrategy(visitorDelegate);

            defaultStrategy.CacheVisitorAsync();

            visitorCache.Verify(x => x.CacheVisitor(visitorId, It.Is<JObject>(y => y.ToString() == dataJson.ToString())), Times.Once);

            var error = new Exception("visitorCache error");

            visitorCache.Setup(x => x.CacheVisitor(It.IsAny<string>(), It.IsAny<JObject>())).Throws(error);

            defaultStrategy.CacheVisitorAsync();

            fsLogManagerMock.Verify(x => x.Error(error.Message, "CacheVisitor"), Times.Once());

            visitorDelegate.Config.VisitorCacheImplementation = null;

            defaultStrategy.CacheVisitorAsync();
        }

        [TestMethod]
        public void FlushVisitorAsyncTest()
        {
            var visitorCache = new Mock<IVisitorCacheImplementation>();
            var visitorId = visitorDelegate.VisitorId;
            visitorDelegate.Config.VisitorCacheImplementation = visitorCache.Object;

            var defaultStrategy = new DefaultStrategy(visitorDelegate);

            defaultStrategy.FlushVisitorAsync();

            visitorCache.Verify(x => x.FlushVisitor(visitorId), Times.Once());

            var error = new Exception("visitorCache error");

            visitorCache.Setup(x => x.FlushVisitor(It.IsAny<string>())).Throws(error);

            defaultStrategy.FlushVisitorAsync();

            fsLogManagerMock.Verify(x => x.Error(error.Message, "FlushVisitor"), Times.Once());

        }

    }
}
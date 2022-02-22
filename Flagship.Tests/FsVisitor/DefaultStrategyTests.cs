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
using Flagship.Cache;
using Flagship.Model;
using System.Collections.ObjectModel;
using Flagship.Hit;

namespace Flagship.FsVisitor.Tests
{
    [TestClass()]
    public class DefaultStrategyTests
    {
        private Mock<Flagship.Utils.IFsLogManager> fsLogManagerMock;
        private VisitorDelegate visitorDelegate;
        private Mock<Flagship.Decision.DecisionManager> decisionManagerMock;
        private Mock<Flagship.Api.ITrackingManager> trackingManagerMock;
        public DefaultStrategyTests()
        {
            fsLogManagerMock = new Mock<Flagship.Utils.IFsLogManager>();
            var config = new Flagship.Config.DecisionApiConfig()
            {
                EnvId = "envID",
                LogManager = fsLogManagerMock.Object,
            };
            trackingManagerMock = new Mock<Flagship.Api.ITrackingManager>();
            decisionManagerMock = new Mock<Flagship.Decision.DecisionManager>(new object[] { null, null });
            var configManager = new Flagship.Config.ConfigManager(config, decisionManagerMock.Object, trackingManagerMock.Object);

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

            Assert.AreEqual(visitorDelegate.Context.Count, 5);

            var newContext2 = new Dictionary<string, object>()
            {
                ["key3"] = 5,
                ["key4"] = 1
            };

            defaultStrategy.UpdateContext(newContext2);

            Assert.AreEqual(visitorDelegate.Context.Count, 7);

            var newContext3 = new Dictionary<string, object>()
            {
                ["key5"] = true,
                ["key6"] = false
            };

            defaultStrategy.UpdateContext(newContext3);

            Assert.AreEqual(visitorDelegate.Context.Count, 9);

            defaultStrategy.UpdateContext("key1", "value3");
            Assert.AreEqual(visitorDelegate.Context["key1"], "value3");

            defaultStrategy.UpdateContext("key3", 10);
            Assert.AreEqual(10d, visitorDelegate.Context["key3"]);

            defaultStrategy.UpdateContext("key5", false);
            Assert.AreEqual(visitorDelegate.Context["key5"], false);

            var newContext4 = new Dictionary<string, object>()
            {
                ["key6"] = new object(),
            };

            defaultStrategy.UpdateContext(newContext4);
            Assert.AreEqual(visitorDelegate.Context.Count, 9);
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
                [FsPredefinedContext.LOCATION_CITY] = "London",
                [FsPredefinedContext.OS_VERSION_CODE] = 1,
                [FsPredefinedContext.APP_VERSION_CODE] = "1",
                [FsPredefinedContext.DEVICE_LOCALE] = Array.Empty<string>(),
                [FsPredefinedContext.DEVICE_MODEL] = null,
            };

            defaultStrategy.UpdateContext(newContext);

            Assert.AreEqual(visitorDelegate.Context.Count, 6);
            Assert.AreEqual(visitorDelegate.Context[FsPredefinedContext.LOCATION_CITY], "London");

            fsLogManagerMock.Verify(x => x.Error(string.Format(Constants.PREDEFINED_CONTEXT_TYPE_ERROR,
                FsPredefinedContext.APP_VERSION_CODE, "number"), "UpdateContext"), Times.Once());

            fsLogManagerMock.Verify(x => x.Error(string.Format(Constants.PREDEFINED_CONTEXT_TYPE_ERROR,
                FsPredefinedContext.DEVICE_LOCALE, "string"), "UpdateContext"), Times.Once());

            fsLogManagerMock.Verify(x => x.Error(string.Format(Constants.PREDEFINED_CONTEXT_TYPE_ERROR,
                FsPredefinedContext.DEVICE_MODEL, "string"), "UpdateContext"), Times.Once());
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

            Assert.AreEqual(visitorDelegate.Flags.Count, 1);
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
        public async Task UserExposedTest()
        {
            const string functionName = "UserExposed";
            var defaultStrategy = new DefaultStrategy(visitorDelegate);

            var key = "key1";
            await defaultStrategy.UserExposed(key, "defaultValue", null).ConfigureAwait(false);

            fsLogManagerMock.Verify(x => x.Error(string.Format(Constants.GET_FLAG_ERROR, key), functionName), Times.Once());

            var flagDto = CampaignsData.GetFlag()[0];
            await defaultStrategy.UserExposed(flagDto.Key, 1, flagDto).ConfigureAwait(false);

            fsLogManagerMock.Verify(x => x.Error(string.Format(Constants.USER_EXPOSED_CAST_ERROR, flagDto.Key), functionName), Times.Once());

            await defaultStrategy.UserExposed(flagDto.Key, "defaultValueString", flagDto).ConfigureAwait(false);

            trackingManagerMock.Verify(x => x.SendActive(visitorDelegate, flagDto), Times.Once());

            var flagDtoValueNull = CampaignsData.GetFlag()[1];

            await defaultStrategy.UserExposed(flagDtoValueNull.Key, "defaultValueString", flagDtoValueNull).ConfigureAwait(false);

            trackingManagerMock.Verify(x => x.SendActive(visitorDelegate, flagDtoValueNull), Times.Once());

            var error = new Exception("userExposed error");

            trackingManagerMock.Setup(x => x.SendActive(visitorDelegate, flagDto)).Throws(error);
            await defaultStrategy.UserExposed(flagDto.Key, "defaultValueString", flagDto).ConfigureAwait(false);

            fsLogManagerMock.Verify(x => x.Error(error.Message, functionName), Times.Once());
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
        }

        [TestMethod()]
        public void GetFlagValueWithValueNullTest()
        {
            const string functionName = "getFlag.value";
            var defaultStrategy = new DefaultStrategy(visitorDelegate);

            var defaultValueString = "defaultValueString";
            var flagDtoValueNull = CampaignsData.GetFlag()[1];

            var value2 = defaultStrategy.GetFlagValue(flagDtoValueNull.Key, defaultValueString, flagDtoValueNull);
            Assert.AreEqual(defaultValueString, value2);

            fsLogManagerMock.Verify(x => x.Info(string.Format(Constants.GET_FLAG_CAST_ERROR, flagDtoValueNull.Key), functionName), Times.Once());
            trackingManagerMock.Verify(x => x.SendActive(visitorDelegate, flagDtoValueNull), Times.Once());
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
            trackingManagerMock.Verify(x => x.SendActive(visitorDelegate, flagDto), Times.Never());
        }

        [TestMethod()]
        public void GetFlagValueWithUserExposedFalse()
        {
            var defaultStrategy = new DefaultStrategy(visitorDelegate);
            var flagDto = CampaignsData.GetFlag()[0];

            var value = defaultStrategy.GetFlagValue(flagDto.Key, "Default", flagDto, false);
            Assert.AreEqual(flagDto.Value, value);
            trackingManagerMock.Verify(x => x.SendActive(visitorDelegate, flagDto), Times.Never());
        }

        [TestMethod()]
        public void GetFlagValue()
        {
            var defaultStrategy = new DefaultStrategy(visitorDelegate);
            var flagDto = CampaignsData.GetFlag()[0];

            var value = defaultStrategy.GetFlagValue(flagDto.Key, "Default", flagDto);
            Assert.AreEqual(flagDto.Value, value);
            trackingManagerMock.Verify(x => x.SendActive(visitorDelegate, flagDto), Times.Once());
        }

        [TestMethod()]
        public void GetFlagMetadataFailedTest()
        {
            const string functionName = "flag.metadata";
            var defaultStrategy = new DefaultStrategy(visitorDelegate);

            var metadata = new FsFlag.FlagMetadata("CampaignId", "variationGroupId", "variationId", false, "");
            var resultatMetadata = defaultStrategy.GetFlagMetadata(metadata, "key", false);

            Assert.AreEqual(JsonConvert.SerializeObject(FsFlag.FlagMetadata.EmptyMetadata()), JsonConvert.SerializeObject(resultatMetadata));
            fsLogManagerMock.Verify(x => x.Error(string.Format(Constants.GET_METADATA_CAST_ERROR, "key"), functionName), Times.Once());
        }

        [TestMethod()]
        public void GetFlagMetadataTest()
        {
            const string functionName = "flag.metadata";
            var defaultStrategy = new DefaultStrategy(visitorDelegate);

            var metadata = new FsFlag.FlagMetadata("CampaignId", "variationGroupId", "variationId", false, "");
            var resultatMetadata = defaultStrategy.GetFlagMetadata(metadata, "key", true);

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
            trackingManagerMock.Setup(x => x.SendHit(hit)).Throws(new Exception(errorMessage));

            await defaultStrategy.SendHit(hit).ConfigureAwait(false);
            trackingManagerMock.Verify(x => x.SendHit(hit), Times.Once());

            fsLogManagerMock.Verify(x => x.Error(errorMessage, functionName), Times.Once());
        }

        [TestMethod()]
        public async Task SendHitTest()
        {
            var defaultStrategy = new DefaultStrategy(visitorDelegate);
            var hit = new Flagship.Hit.Screen("HomeView");
            await defaultStrategy.SendHit(hit).ConfigureAwait(false);
            trackingManagerMock.Verify(x => x.SendHit(hit), Times.Once());
        }

        [TestMethod()]
        public async Task SendHitsTest()
        {
            var defaultStrategy = new DefaultStrategy(visitorDelegate);
            var screen = new Flagship.Hit.Screen("HomeView");
            var page = new Flagship.Hit.Page("HomePage");
            await defaultStrategy.SendHit(new List<Hit.HitAbstract>() { screen, page }).ConfigureAwait(false);
            trackingManagerMock.Verify(x => x.SendHit(screen), Times.Once());
            trackingManagerMock.Verify(x => x.SendHit(page), Times.Once());
        }

        [TestMethod()]
        public async Task SendConsentHitAsyncTest()
        {
            var defaultStrategy = new DefaultStrategy(visitorDelegate);

            await defaultStrategy.SendConsentHitAsync(true).ConfigureAwait(false);

            trackingManagerMock.Verify(x => x.SendHit(It.Is<Hit.Event>(
                item => item.Label == $"{Constants.SDK_LANGUAGE}:{true}" &&
                item.VisitorId == visitorDelegate.VisitorId &&
                item.DS == Constants.SDK_APP &&
                item.AnonymousId == visitorDelegate.AnonymousId
                )), Times.Once());
        }

        [TestMethod()]
        public async Task SendConsentHitAsyncFailedTest()
        {
            const string functionName = "SendConsentHit";
            var defaultStrategy = new DefaultStrategy(visitorDelegate);

            const string errorMessage = "error sendHit";

            trackingManagerMock.Setup(x => x.SendHit(It.IsAny<Hit.Event>())).Throws(new Exception(errorMessage));

            await defaultStrategy.SendConsentHitAsync(true).ConfigureAwait(false);

            fsLogManagerMock.Verify(x => x.Error(errorMessage, functionName), Times.Once());
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

            fsLogManagerMock.Verify(x => x.Error(VisitorStrategyAbstract.LOOKUP_VISITOR_JSON_OBJECT_ERROR, "LookupVisitor"), Times.Once());

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
                    VariationHistory= variationHistory
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
                        VariationHistory = new Dictionary<string, string>
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

        [TestMethod]
        public void CacheHitActivateTest()
        {
            var hitCache = new Mock<IHitCacheImplementation>();
            var visitorId = visitorDelegate.VisitorId;
            visitorDelegate.Config.HitCacheImplementation = hitCache.Object;

            var defaultStrategy = new DefaultStrategy(visitorDelegate);

            var flag = new FlagDTO
            {
                CampaignId = "campaignId",
                CampaignType = "ab",
                IsReference = true,
                Key = "key",
                Value = "value",
                VariationGroupId = "varGrID",
                VariationId = "varID"
            };

            var hitData = new HitCacheDTOV1
            {
                Version = 1,
                Data = new HitCacheData
                {
                    VisitorId = visitorDelegate.VisitorId,
                    AnonymousId = visitorDelegate.AnonymousId,
                    Type = HitCacheType.ACTIVATE,
                    Content = flag,
                    Time = DateTime.Now
                }
            };

            var hitDataJson = JObject.FromObject(hitData);

            defaultStrategy.CacheHit(flag);

            hitCache.Verify(x => x.CacheHit(visitorId, It.Is<JObject>(y =>
             y["Data"]["Type"].Value<int>() == ((int)HitCacheType.ACTIVATE) &&
             y["Data"]["Content"].Value<JObject>().ToString() == JObject.FromObject(flag).ToString()
            )), Times.Once());

            var error = new Exception("CacheHit error");

            hitCache.Setup(x => x.CacheHit(visitorId, It.IsAny<JObject>())).Throws(error);

            defaultStrategy.CacheHit(flag);

            fsLogManagerMock.Verify(x => x.Error(error.Message, "CacheHit"), Times.Once());
        }

        [TestMethod]
        public void CacheHitTest()
        {
            var hitCache = new Mock<IHitCacheImplementation>();
            var visitorId = visitorDelegate.VisitorId;
            visitorDelegate.Config.HitCacheImplementation = hitCache.Object;

            var defaultStrategy = new DefaultStrategy(visitorDelegate);

            var hit = new Screen("Home");

            var hitData = new HitCacheDTOV1
            {
                Version = 1,
                Data = new HitCacheData
                {
                    VisitorId = visitorDelegate.VisitorId,
                    AnonymousId = visitorDelegate.AnonymousId,
                    Type = HitCacheType.ACTIVATE,
                    Content = hit,
                    Time = DateTime.Now
                }
            };

            var hitDataJson = JObject.FromObject(hitData);

            defaultStrategy.CacheHit(hit);

            hitCache.Verify(x => x.CacheHit(visitorId, It.Is<JObject>(y =>
             y["Data"]["Type"].Value<int>() == ((int)HitCacheType.SCREENVIEW) &&
             y["Data"]["Content"].Value<JObject>().ToString() == JObject.FromObject(hit).ToString()
            )), Times.Once());

            var error = new Exception("CacheHit error");

            hitCache.Setup(x => x.CacheHit(visitorId, It.IsAny<JObject>())).Throws(error);

            defaultStrategy.CacheHit(hit);

            fsLogManagerMock.Verify(x => x.Error(error.Message, "CacheHit"), Times.Once());
        }

        [TestMethod]
        public void FlushHitsAsyncTest()
        {
            var hitCache = new Mock<IHitCacheImplementation>();
            var visitorId = visitorDelegate.VisitorId;
            visitorDelegate.Config.HitCacheImplementation = hitCache.Object;

            var defaultStrategy = new DefaultStrategy(visitorDelegate);

            defaultStrategy.FlushHitAsync();

            hitCache.Verify(x => x.FlushHits(visitorId), Times.Once());

            var error = new Exception("hitsCache error");

            hitCache.Setup(x => x.FlushHits(It.IsAny<string>())).Throws(error);

            defaultStrategy.FlushHitAsync();

            fsLogManagerMock.Verify(x => x.Error(error.Message, "FlushHits"), Times.Once());
        }

        [TestMethod]
        public void LookupHitsTest()
        {
            var hitCache = new Mock<IHitCacheImplementation>();
            var visitorId = visitorDelegate.VisitorId;
            visitorDelegate.Config.HitCacheImplementation = hitCache.Object;

            var defaultStrategy = new DefaultStrategy(visitorDelegate);

            var screenHit = new Screen(string.Concat(Enumerable.Repeat("Home", 2500)));

            var hitData = new HitCacheDTOV1
            {
                Version = 1,
                Data = new HitCacheData
                {
                    VisitorId = visitorDelegate.VisitorId,
                    AnonymousId = visitorDelegate.AnonymousId,
                    Type = HitCacheType.SCREENVIEW,
                    Content = screenHit,
                    Time = DateTime.Now
                }
            };

            var pageHit = new Page("Home");

            var hitData2 = new HitCacheDTOV1
            {
                Version = 1,
                Data = new HitCacheData
                {
                    VisitorId = visitorDelegate.VisitorId,
                    AnonymousId = visitorDelegate.AnonymousId,
                    Type = HitCacheType.PAGEVIEW,
                    Content = pageHit,
                    Time = DateTime.Now
                }
            };

            var batchHit = new Batch()
            {
                Hits = new Collection<HitAbstract>()
                {
                    pageHit,
                    screenHit
                },
            };

            var hitData3 = new HitCacheDTOV1
            {
                Version = 1,
                Data = new HitCacheData
                {
                    VisitorId = visitorDelegate.VisitorId,
                    AnonymousId = visitorDelegate.AnonymousId,
                    Type = HitCacheType.BATCH,
                    Content = batchHit,
                    Time = DateTime.Now
                }
            };

            var transactionHit = new Transaction("transactionID", "aff");

            var hitData4 = new HitCacheDTOV1
            {
                Version = 1,
                Data = new HitCacheData
                {
                    VisitorId = visitorDelegate.VisitorId,
                    AnonymousId = visitorDelegate.AnonymousId,
                    Type = HitCacheType.TRANSACTION,
                    Content = transactionHit,
                    Time = DateTime.Now
                }
            };

            var eventHit = new Event(EventCategory.USER_ENGAGEMENT, "aff");

            var hitData5 = new HitCacheDTOV1
            {
                Version = 1,
                Data = new HitCacheData
                {
                    VisitorId = visitorDelegate.VisitorId,
                    AnonymousId = visitorDelegate.AnonymousId,
                    Type = HitCacheType.EVENT,
                    Content = eventHit,
                    Time = DateTime.Now
                }
            };

            var itemHit = new Item("transID", "name", "code");

            var hitData6 = new HitCacheDTOV1
            {
                Version = 1,
                Data = new HitCacheData
                {
                    VisitorId = visitorDelegate.VisitorId,
                    AnonymousId = visitorDelegate.AnonymousId,
                    Type = HitCacheType.ITEM,
                    Content = itemHit,
                    Time = DateTime.Now
                }
            };

            var flag = new FlagDTO
            {
                CampaignId = "campaignCacheHitId",
                CampaignType = "ab",
                IsReference = true,
                Key = "key",
                Value = "value",
                VariationGroupId = "varGrID",
                VariationId = "varID"
            };

            var hitData7 = new HitCacheDTOV1
            {
                Version = 1,
                Data = new HitCacheData
                {
                    VisitorId = visitorDelegate.VisitorId,
                    AnonymousId = visitorDelegate.AnonymousId,
                    Type = HitCacheType.ACTIVATE,
                    Content = flag,
                    Time = DateTime.Now
                }
            };

            var array = new JArray()
            {
                JObject.FromObject(hitData),
                JObject.FromObject(hitData2),
                JObject.FromObject(hitData3),
                JObject.FromObject(hitData4),
                JObject.FromObject(hitData5),
                JObject.FromObject(hitData6),
                JObject.FromObject(hitData7),
            };

            hitCache.Setup(x => x.LookupHits(visitorId)).Returns(Task.FromResult(array));

            defaultStrategy.LookupHits();

            trackingManagerMock.Verify(x => x.SendActive(visitorDelegate, It.Is<FlagDTO>(
                y => y.CampaignId == flag.CampaignId &&
                y.CampaignType == flag.CampaignType &&
                y.IsReference == flag.IsReference &&
                y.Key == flag.Key &&
                y.Value == flag.Value &&
                y.VariationGroupId == flag.VariationGroupId &&
                y.VariationId == flag.VariationId)), Times.Once());

            Func<HitAbstract, bool> checkHit = (HitAbstract hitAbstract) =>
            {
                switch (hitAbstract.Type)
                {
                    case HitType.PAGEVIEW:
                        var page = (Page)hitAbstract;
                        return page.DocumentLocation == pageHit.DocumentLocation;
                    case HitType.SCREENVIEW:
                        var screen = (Screen)hitAbstract;
                        return screen.DocumentLocation == screenHit.DocumentLocation;
                    case HitType.TRANSACTION:
                        var transaction = (Transaction)hitAbstract;
                        return transaction.TransactionId == transactionHit.TransactionId && transaction.Affiliation == transactionHit.Affiliation;
                    case HitType.ITEM:
                        var item = (Item)hitAbstract;
                        return item.TransactionId == itemHit.TransactionId && item.Code == itemHit.Code && item.Name == itemHit.Name;
                    case HitType.EVENT:
                        var eventObject = (Event)hitAbstract;
                        return eventObject.Category == eventHit.Category && eventObject.Action == eventHit.Action;
                    case HitType.BATCH:
                        var batch = (Batch)hitAbstract;
                        return batch.Hits.Count == 2;
                    default:
                        break;
                }
                return false;
            };

            trackingManagerMock.Verify(x => x.SendHit(It.Is<Batch>(y => y.Hits.Any(checkHit))), Times.Exactly(3));
        }
    }
}
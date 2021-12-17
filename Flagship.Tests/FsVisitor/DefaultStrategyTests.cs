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

            var newContext = new Dictionary<string, string>()
            {
                ["key1"] = "value1",
                ["key2"] = "value2"
            };

            defaultStrategy.UpdateContex(newContext);

            Assert.AreEqual(visitorDelegate.Context.Count, 3);

            var newContext2 = new Dictionary<string, double>()
            {
                ["key3"] = 5,
                ["key4"] = 1
            };

            defaultStrategy.UpdateContex(newContext2);

            Assert.AreEqual(visitorDelegate.Context.Count, 5);

            var newContext3 = new Dictionary<string, bool>()
            {
                ["key5"] = true,
                ["key6"] = false
            };

            defaultStrategy.UpdateContex(newContext3);

            Assert.AreEqual(visitorDelegate.Context.Count, 7);

            defaultStrategy.UpdateContex("key1", "value3");
            Assert.AreEqual(visitorDelegate.Context["key1"], "value3");

            defaultStrategy.UpdateContex("key3", 10);
            Assert.AreEqual(10d, visitorDelegate.Context["key3"]);

            defaultStrategy.UpdateContex("key5", false);
            Assert.AreEqual(visitorDelegate.Context["key5"], false);

            var newContext4 = new Dictionary<string, object>()
            {
                ["key6"] = new object(),
            };

            defaultStrategy.UpdateContexCommon(newContext4);
            Assert.AreEqual(visitorDelegate.Context.Count, 7);
            fsLogManagerMock.Verify(x => x.Error(string.Format(Constants.CONTEXT_PARAM_ERROR, "key6"), "UpdateContex"), Times.Once());

            //Test clearContext

            defaultStrategy.ClearContext();
            Assert.AreEqual(visitorDelegate.Context.Count, 0);
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
            await defaultStrategy.SendHit(null).ConfigureAwait(false);
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
    }
}
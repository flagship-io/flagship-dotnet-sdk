using Microsoft.VisualStudio.TestTools.UnitTesting;
using Flagship.FsVisitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Flagship.Enums;
using Newtonsoft.Json;
using Flagship.Logger;
using Newtonsoft.Json.Linq;
using Flagship.Hit;

namespace Flagship.FsVisitor.Tests
{
    [TestClass()]
    public class NotReadyStrategyTests
    {
        private Mock<IFsLogManager> fsLogManagerMock;
        private VisitorDelegate visitorDelegate;
        private Mock<Flagship.Decision.DecisionManager> decisionManagerMock;
        private Mock<Flagship.Api.ITrackingManager> trackingManagerMock;
        private Flagship.Config.DecisionApiConfig config;
        public NotReadyStrategyTests()
        {
            fsLogManagerMock = new Mock<IFsLogManager>();
            config = new Flagship.Config.DecisionApiConfig()
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
        public void NotReadyStrategyTest()
        {
            var notReadyStategy = new NotReadyStrategy(visitorDelegate);

            var VisitorCacheImplementation = new Mock<Flagship.Cache.IVisitorCacheImplementation>();
            var HitCaheImplementation = new Mock<Cache.IHitCacheImplementation>();

            config.VisitorCacheImplementation = VisitorCacheImplementation.Object;
            config.HitCacheImplementation = HitCaheImplementation.Object;

            notReadyStategy.CacheVisitorAsync();
            notReadyStategy.LookupVisitor();

            VisitorCacheImplementation.Verify(x => x.CacheVisitor(It.IsAny<string>(), It.IsAny<JObject>()), Times.Never());
        }

        [TestMethod()]
        public async Task FetchFlagsTest()
        {
            var notReadyStategy = new NotReadyStrategy(visitorDelegate);
            await notReadyStategy.FetchFlags().ConfigureAwait(false);
            fsLogManagerMock.Verify(x => x.Error(string.Format(Constants.METHOD_DEACTIVATED_ERROR, "FetchFlags", FSSdkStatus.SDK_NOT_INITIALIZED), "FetchFlags"), Times.Once());
        }

        [TestMethod()]
        public async Task SendHitTest()
        {
            var notReadyStategy = new NotReadyStrategy(visitorDelegate);
            await notReadyStategy.SendHit(new Hit.Screen("Home")).ConfigureAwait(false);
            fsLogManagerMock.Verify(x => x.Error(string.Format(Constants.METHOD_DEACTIVATED_ERROR, "SendHit", FSSdkStatus.SDK_NOT_INITIALIZED), "SendHit"), Times.Once());
        }

        [TestMethod()]
        public void GetFlagValueTest()
        {
            var defaultValue = "defaultValue";
            var notReadyStategy = new NotReadyStrategy(visitorDelegate);
            var value = notReadyStategy.GetFlagValue("key", defaultValue, null);

            Assert.AreEqual(defaultValue, value);

            fsLogManagerMock.Verify(x => x.Error(string.Format(Constants.METHOD_DEACTIVATED_ERROR, "Flag.value", FSSdkStatus.SDK_NOT_INITIALIZED), "Flag.value"), Times.Once());
        }

        [TestMethod()]
        public async Task UserExposedTest()
        {
            var notReadyStategy = new NotReadyStrategy(visitorDelegate);
            await notReadyStategy.VisitorExposed("key","defaultValue", null).ConfigureAwait(false);
            fsLogManagerMock.Verify(x => x.Error(string.Format(Constants.METHOD_DEACTIVATED_ERROR, "VisitorExposed", FSSdkStatus.SDK_NOT_INITIALIZED), "VisitorExposed"), Times.Once());
        }

        [TestMethod()]
        public void GetFlagMetadataTest()
        {
            var notReadyStategy = new NotReadyStrategy(visitorDelegate);
            var value = notReadyStategy.GetFlagMetadata("key", null);

            Assert.AreEqual(JsonConvert.SerializeObject(FsFlag.FlagMetadata.EmptyMetadata()), JsonConvert.SerializeObject(value));

            fsLogManagerMock.Verify(x => x.Error(string.Format(Constants.METHOD_DEACTIVATED_ERROR, "flag.metadata", FSSdkStatus.SDK_NOT_INITIALIZED), "flag.metadata"), Times.Once());
        }

        [TestMethod()]
        public async Task SendTroubleshootingHitTest()
        {
            var fsLogManagerMock = new Mock<IFsLogManager>();
            var config = new Config.DecisionApiConfig()
            {
                EnvId = "envID",
                LogManager = fsLogManagerMock.Object,
                DisableDeveloperUsageTracking = true,
            };

            var trackingManagerMock = new Mock<Api.ITrackingManager>();
            var trackingManager = trackingManagerMock.Object;

            var decisionManagerMock = new Mock<Decision.DecisionManager>(new object[] { null, null });

            var decisionManager = decisionManagerMock.Object;
            decisionManager.TrackingManager = trackingManager;

            var configManager = new Config.ConfigManager(config, decisionManager, trackingManagerMock.Object);

            var context = new Dictionary<string, object>()
            {
                ["key"] = 1,
            };

            var visitorDelegate = new VisitorDelegate("visitorId", false, context, false, configManager);

            var strategy = new NotReadyStrategy(visitorDelegate);

            var troubleshootingHit = new Troubleshooting();

            //Test SendTroubleshootingHit

            await strategy.SendTroubleshootingHit(troubleshootingHit);

            trackingManagerMock.Verify(x => x.SendTroubleshootingHit(It.IsAny<Troubleshooting>()), Times.Never());

            //Test AddTroubleshootingHitTest

            strategy.AddTroubleshootingHit(troubleshootingHit);

            trackingManagerMock.Verify(x => x.AddTroubleshootingHit(It.IsAny<Troubleshooting>()), Times.Never());
        }
    }


}
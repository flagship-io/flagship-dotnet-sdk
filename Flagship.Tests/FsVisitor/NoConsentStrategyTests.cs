using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Flagship.Enums;
using Flagship.Logger;
using Newtonsoft.Json.Linq;
using Flagship.Model;
using Flagship.Hit;
using Flagship.Tests.Helpers;
using Flagship.Api;

namespace Flagship.FsVisitor.Tests
{
    [TestClass()]
    public class NoConsentStrategyTests
    {
        private Mock<IFsLogManager> fsLogManagerMock;
        private VisitorDelegate visitorDelegate;
        private Mock<Flagship.Decision.DecisionManager> decisionManagerMock;
        private Mock<Flagship.Api.ITrackingManager> trackingManagerMock;
        private Flagship.Config.DecisionApiConfig config;
        public NoConsentStrategyTests() 
        {
            fsLogManagerMock = new Mock<IFsLogManager>();
            config = new Flagship.Config.DecisionApiConfig()
            {
                EnvId = "envID",
                LogManager = fsLogManagerMock.Object,
            };
            trackingManagerMock = new Mock<Flagship.Api.ITrackingManager>();
            decisionManagerMock = new Mock<Flagship.Decision.DecisionManager>([null, null]);
            var configManager = new Flagship.Config.ConfigManager(config, decisionManagerMock.Object, trackingManagerMock.Object);

            var context = new Dictionary<string, object>()
            {
                ["key0"] = 1,
            };

            visitorDelegate = new FsVisitor.VisitorDelegate("visitorId", false, context, false, configManager);

        }
        [TestMethod()]
        public void NoConsentStrategyTest()
        {
            var noConsentStrategy = new NoConsentStrategy(visitorDelegate);

            var VisitorCacheImplementation = new Mock<Flagship.Cache.IVisitorCacheImplementation>();
            var HitCacheImplementation = new Mock<Cache.IHitCacheImplementation>();

            config.VisitorCacheImplementation = VisitorCacheImplementation.Object;
            config.HitCacheImplementation = HitCacheImplementation.Object;


            noConsentStrategy.CacheVisitorAsync();
            noConsentStrategy.LookupVisitor();

            VisitorCacheImplementation.Verify(x => x.CacheVisitor(It.IsAny<string>(), It.IsAny<JObject>()), Times.Never());

            var FetchVisitorCacheCampaigns = TestHelpers.GetPrivateMethod(noConsentStrategy, "FetchVisitorCacheCampaigns");

            var campaigns = (ICollection<Campaign>?)FetchVisitorCacheCampaigns?.Invoke(noConsentStrategy, [visitorDelegate]);

            Assert.AreEqual(campaigns?.Count, 0);

        }

        [TestMethod()]
        public async Task UserExposedTest()
        {
            var noConsentStategy = new NoConsentStrategy(visitorDelegate);
            var defaultValue = "default";
            await noConsentStategy.VisitorExposed("key", defaultValue, null).ConfigureAwait(false);
            fsLogManagerMock.Verify(x => x.Info(string.Format(Constants.METHOD_DEACTIVATED_CONSENT_ERROR, "VisitorExposed", visitorDelegate.VisitorId), "VisitorExposed"), Times.Once());
        }

        [TestMethod()]
        public async Task SendHitTest()
        {
            fsLogManagerMock = new Mock<IFsLogManager>();
            config = new Flagship.Config.DecisionApiConfig()
            {
                EnvId = "envID",
                LogManager = fsLogManagerMock.Object,
            };
            trackingManagerMock = new Mock<Flagship.Api.ITrackingManager>();
            decisionManagerMock = new Mock<Flagship.Decision.DecisionManager>([null, null]);
            var configManager = new Flagship.Config.ConfigManager(config, decisionManagerMock.Object, trackingManagerMock.Object);

            var context = new Dictionary<string, object>()
            {
                ["key0"] = 1,
            };
            var visitorDelegateMock = new Mock<VisitorDelegate>("visitorId", false, context, false, configManager, null) { CallBase = true };

            visitorDelegateMock.Setup(x => x.SetConsent(false));
            var visitorDelegate = visitorDelegateMock.Object;


            var noConsentStrategy = new NoConsentStrategy(visitorDelegate);

            await noConsentStrategy.SendHit(new Flagship.Hit.Screen("Home")).ConfigureAwait(false);

            fsLogManagerMock.Verify(x => x.Info(string.Format(Constants.METHOD_DEACTIVATED_CONSENT_ERROR, "SendHit", visitorDelegate.VisitorId), "SendHit"), Times.Once());
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

            var decisionManagerMock = new Mock<Decision.DecisionManager>([null, null]);

            var decisionManager = decisionManagerMock.Object;
            decisionManager.TrackingManager = trackingManager;

            var configManager = new Config.ConfigManager(config, decisionManager, trackingManagerMock.Object);

            var context = new Dictionary<string, object>()
            {
                ["key"] = 1,
            };

            var visitorDelegate = new VisitorDelegate("visitorId", false, context, false, configManager);

            var strategy = new NoConsentStrategy(visitorDelegate);

            var troubleshootingHit = new Troubleshooting();

            //Test SendTroubleshootingHit

            await strategy.SendTroubleshootingHit(troubleshootingHit);

            trackingManagerMock.Verify(x => x.SendTroubleshootingHit(It.IsAny<Troubleshooting>()), Times.Never());

            //Test AddTroubleshootingHitTest

            strategy.AddTroubleshootingHit(troubleshootingHit);

            trackingManagerMock.Verify(x => x.AddTroubleshootingHit(It.IsAny<Troubleshooting>()), Times.Never());
        }

        [TestMethod()]
        public void GetTroubleshootingData(){
            var config = new Config.DecisionApiConfig()
            {
                EnvId = "envID",
                LogManager = fsLogManagerMock.Object,
                DisableDeveloperUsageTracking = true,
                TrackingManagerConfig = new Config.TrackingManagerConfig()
            };

            var trackingManager = new TrackingManager(config, new HttpClient());

            var decisionManagerMock = new Mock<Decision.DecisionManager>([null, null]);

            var decisionManager = decisionManagerMock.Object;
            decisionManager.TrackingManager = trackingManager;

            var configManager = new Config.ConfigManager(config, decisionManager, trackingManager);

            var context = new Dictionary<string, object>()
            {
                ["key"] = 1,
            };

            var visitorDelegate = new VisitorDelegate("visitorId", false, context, false, configManager);

            var strategy = new NoConsentStrategy(visitorDelegate);

            trackingManager.TroubleshootingData = new TroubleshootingData();

            Assert.AreNotSame(trackingManager.TroubleshootingData, null);

            var troubleshootingHit = strategy.GetTroubleshootingData();

            Assert.AreEqual(troubleshootingHit, null);

            Assert.AreEqual(trackingManager.TroubleshootingData, null);
        }
    }
}
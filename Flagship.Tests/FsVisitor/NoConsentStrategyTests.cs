using Microsoft.VisualStudio.TestTools.UnitTesting;
using Flagship.FsVisitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Flagship.Enums;
using Flagship.Logger;
using Newtonsoft.Json.Linq;
using Flagship.Model;

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
            decisionManagerMock = new Mock<Flagship.Decision.DecisionManager>(new object[] { null, null });
            var configManager = new Flagship.Config.ConfigManager(config, decisionManagerMock.Object, trackingManagerMock.Object);

            var context = new Dictionary<string, object>()
            {
                ["key0"] = 1,
            };

            visitorDelegate = new Flagship.FsVisitor.VisitorDelegate("visitorId", false, context, false, configManager);

        }
        [TestMethod()]
        public void NoConsentStrategyTest()
        {
            var noConsentStategy = new NoConsentStrategy(visitorDelegate);

            var VisitorCacheImplementation = new Mock<Flagship.Cache.IVisitorCacheImplementation>();
            var HitCaheImplementation = new Mock<Cache.IHitCacheImplementation>();

            config.VisitorCacheImplementation = VisitorCacheImplementation.Object;
            config.HitCacheImplementation = HitCaheImplementation.Object;


            noConsentStategy.CacheVisitorAsync();
            noConsentStategy.LookupVisitor();

            VisitorCacheImplementation.Verify(x => x.CacheVisitor(It.IsAny<string>(), It.IsAny<JObject>()), Times.Never());


            var privateNoConsentStrategy = new PrivateObject(noConsentStategy);

            ICollection<Campaign> compaigns = (ICollection<Campaign>)privateNoConsentStrategy.Invoke("FetchVisitorCacheCampaigns", visitorDelegate);

            Assert.AreEqual(compaigns.Count, 0);

        }

        [TestMethod()]
        public async Task UserExposedTest()
        {
            var noConsentStategy = new NoConsentStrategy(visitorDelegate);
            var defaultValue = "default";
            await noConsentStategy.UserExposed("key", defaultValue, null).ConfigureAwait(false);
            fsLogManagerMock.Verify(x => x.Error(string.Format(Constants.METHOD_DEACTIVATED_CONSENT_ERROR, "UserExposed", visitorDelegate.VisitorId), "UserExposed"), Times.Once());
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
            decisionManagerMock = new Mock<Flagship.Decision.DecisionManager>(new object[] { null, null });
            var configManager = new Flagship.Config.ConfigManager(config, decisionManagerMock.Object, trackingManagerMock.Object);

            var context = new Dictionary<string, object>()
            {
                ["key0"] = 1,
            };
            var visitorDelegateMock = new Mock<VisitorDelegate>("visitorId", false, context, false, configManager, null) { CallBase = true };

            visitorDelegateMock.Setup(x => x.SetConsent(false));
            var visitorDelegate = visitorDelegateMock.Object;


            var noConsentStategy = new NoConsentStrategy(visitorDelegate);

            await noConsentStategy.SendHit(new Flagship.Hit.Screen("Home")).ConfigureAwait(false);

            fsLogManagerMock.Verify(x => x.Error(string.Format(Constants.METHOD_DEACTIVATED_CONSENT_ERROR, "SendHit", visitorDelegate.VisitorId), "SendHit"), Times.Once());
        }
    }
}
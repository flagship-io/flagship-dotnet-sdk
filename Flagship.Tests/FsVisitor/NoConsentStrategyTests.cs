using Microsoft.VisualStudio.TestTools.UnitTesting;
using Flagship.FsVisitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Flagship.Enums;

namespace Flagship.FsVisitor.Tests
{
    [TestClass()]
    public class NoConsentStrategyTests
    {
        private Mock<Flagship.Utils.IFsLogManager> fsLogManagerMock;
        private VisitorDelegate visitorDelegate;
        private Mock<Flagship.Decision.DecisionManager> decisionManagerMock;
        private Mock<Flagship.Api.ITrackingManager> trackingManagerMock;
        public NoConsentStrategyTests() 
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
        public void NoConsentStrategyTest()
        {

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
            var noConsentStategy = new NoConsentStrategy(visitorDelegate);

            await noConsentStategy.SendHit(new Flagship.Hit.Screen("Home")).ConfigureAwait(false);

            fsLogManagerMock.Verify(x => x.Error(string.Format(Constants.METHOD_DEACTIVATED_CONSENT_ERROR, "SendHit", visitorDelegate.VisitorId), "SendHit"), Times.Once());
        }
    }
}
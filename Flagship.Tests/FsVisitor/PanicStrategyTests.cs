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

namespace Flagship.FsVisitor.Tests
{
    [TestClass()]
    public class PanicStrategyTests
    {
        private Mock<Flagship.Utils.IFsLogManager> fsLogManagerMock;
        private VisitorDelegate visitorDelegate;
        private Mock<Flagship.Decision.DecisionManager> decisionManagerMock;
        private Mock<Flagship.Api.ITrackingManager> trackingManagerMock;
        public PanicStrategyTests() 
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
        public void PanicStrategyTest()
        {

        }

        [TestMethod()]
        public async Task SendConsentHitAsyncTest()
        {
            var panicStrategy = new PanicStrategy(visitorDelegate);
            await panicStrategy.SendConsentHitAsync(false).ConfigureAwait(false);
            fsLogManagerMock.Verify(x => x.Error(string.Format(Constants.METHOD_DEACTIVATED_ERROR, "SendConsentHitAsync", FlagshipStatus.READY_PANIC_ON), "SendConsentHitAsync"), Times.Once());
        }

        [TestMethod()]
        public void UpdateContexCommonTest()
        {
            var panicStrategy = new PanicStrategy(visitorDelegate);
            panicStrategy.UpdateContexCommon(new Dictionary<string, object>());
            fsLogManagerMock.Verify(x => x.Error(string.Format(Constants.METHOD_DEACTIVATED_ERROR, "UpdateContex", FlagshipStatus.READY_PANIC_ON), "UpdateContex"), Times.Once());
        }

        [TestMethod()]
        public void UpdateContexKeyValue()
        {
            var panicStrategy = new PanicStrategy(visitorDelegate);
            panicStrategy.UpdateContex("key","value");
            fsLogManagerMock.Verify(x => x.Error(string.Format(Constants.METHOD_DEACTIVATED_ERROR, "UpdateContex", FlagshipStatus.READY_PANIC_ON), "UpdateContex"), Times.Once());
        }

        [TestMethod()]
        public void ClearContextTest()
        {
            var panicStrategy = new PanicStrategy(visitorDelegate);
            panicStrategy.ClearContext();
            fsLogManagerMock.Verify(x => x.Error(string.Format(Constants.METHOD_DEACTIVATED_ERROR, "ClearContext", FlagshipStatus.READY_PANIC_ON), "ClearContext"), Times.Once());
        }

        [TestMethod()]
        public async Task SendHitTest()
        {
            var panicStrategy = new PanicStrategy(visitorDelegate);
            await panicStrategy.SendHit(new Flagship.Hit.Screen("")).ConfigureAwait(false);
            fsLogManagerMock.Verify(x => x.Error(string.Format(Constants.METHOD_DEACTIVATED_ERROR, "SendHit", FlagshipStatus.READY_PANIC_ON), "SendHit"), Times.Once());
        }

        [TestMethod()]
        public void GetFlagValueTest()
        {
            var panicStrategy = new PanicStrategy(visitorDelegate);
            var value = panicStrategy.GetFlagValue("key", "defaultValue", null);
            Assert.AreEqual(value, "defaultValue");
            fsLogManagerMock.Verify(x => x.Error(string.Format(Constants.METHOD_DEACTIVATED_ERROR, "Flag.value", FlagshipStatus.READY_PANIC_ON), "Flag.value"), Times.Once());
        }

        [TestMethod()]
        public async Task UserExposedTest()
        {
            var panicStrategy = new PanicStrategy(visitorDelegate);
            await panicStrategy.UserExposed("key", "defaultValue", null).ConfigureAwait(false);
            fsLogManagerMock.Verify(x => x.Error(string.Format(Constants.METHOD_DEACTIVATED_ERROR, "UserExposed", FlagshipStatus.READY_PANIC_ON), "UserExposed"), Times.Once());
        }

        [TestMethod()]
        public void GetFlagMetadataTest()
        {
            var panicStrategy = new PanicStrategy(visitorDelegate);
            var value = panicStrategy.GetFlagMetadata(null, "key", false);
            Assert.AreEqual(JsonConvert.SerializeObject(FsFlag.FlagMetadata.EmptyMetadata()), JsonConvert.SerializeObject(value));
            fsLogManagerMock.Verify(x => x.Error(string.Format(Constants.METHOD_DEACTIVATED_ERROR, "Flag.metadata", FlagshipStatus.READY_PANIC_ON), "Flag.metadata"), Times.Once());
        }
    }
}
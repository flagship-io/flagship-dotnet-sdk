﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
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
using Flagship.Model;
using Newtonsoft.Json.Linq;
using Flagship.Hit;

namespace Flagship.FsVisitor.Tests
{
    [TestClass()]
    public class PanicStrategyTests
    {
        private Mock<IFsLogManager> fsLogManagerMock;
        private VisitorDelegate visitorDelegate;
        private Mock<Flagship.Decision.DecisionManager> decisionManagerMock;
        private Mock<Flagship.Api.ITrackingManager> trackingManagerMock;
        private Flagship.Config.DecisionApiConfig config;
        public PanicStrategyTests() 
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
        public void PanicStrategyTest()
        {
            var panicStrategy = new PanicStrategy(visitorDelegate);

            var VisitorCacheImplementation = new Mock<Flagship.Cache.IVisitorCacheImplementation>();
            var HitCaheImplementation = new Mock<Cache.IHitCacheImplementation>();

            config.VisitorCacheImplementation = VisitorCacheImplementation.Object;
            config.HitCacheImplementation = HitCaheImplementation.Object;

            panicStrategy.CacheVisitorAsync();
            panicStrategy.LookupVisitor();

            VisitorCacheImplementation.Verify(x => x.CacheVisitor(It.IsAny<string>(), It.IsAny<JObject>()), Times.Never());

            var privateNoConsentStrategy = new PrivateObject(panicStrategy);

            ICollection<Campaign> compaigns = (ICollection<Campaign>)privateNoConsentStrategy.Invoke("FetchVisitorCacheCampaigns", visitorDelegate);

            Assert.AreEqual(compaigns.Count, 0);
        }

        [TestMethod()]
        public async Task SendConsentHitAsyncTest()
        {
            var panicStrategy = new PanicStrategy(visitorDelegate);
            await panicStrategy.SendConsentHitAsync(false).ConfigureAwait(false);
            fsLogManagerMock.Verify(x => x.Info(string.Format(Constants.METHOD_DEACTIVATED_ERROR, "SendConsentHitAsync", FlagshipStatus.READY_PANIC_ON), "SendConsentHitAsync"), Times.Once());
        }

        [TestMethod()]
        public void UpdateContextTest() 
        {
            var panicStrategy = new PanicStrategy(visitorDelegate);
            panicStrategy.UpdateContext(new Dictionary<string, object>());
            fsLogManagerMock.Verify(x => x.Info(string.Format(Constants.METHOD_DEACTIVATED_ERROR, "UpdateContex", FlagshipStatus.READY_PANIC_ON), "UpdateContex"), Times.Once());
        }

        [TestMethod()]
        public void UpdateContexKeyValue()
        {
            var panicStrategy = new PanicStrategy(visitorDelegate);
            panicStrategy.UpdateContext("key","value");
            fsLogManagerMock.Verify(x => x.Info(string.Format(Constants.METHOD_DEACTIVATED_ERROR, "UpdateContex", FlagshipStatus.READY_PANIC_ON), "UpdateContex"), Times.Once());
        }

        [TestMethod()]
        public void ClearContextTest()
        {
            var panicStrategy = new PanicStrategy(visitorDelegate);
            panicStrategy.ClearContext();
            fsLogManagerMock.Verify(x => x.Info(string.Format(Constants.METHOD_DEACTIVATED_ERROR, "ClearContext", FlagshipStatus.READY_PANIC_ON), "ClearContext"), Times.Once());
        }

        [TestMethod()]
        public async Task SendHitTest()
        {
            var panicStrategy = new PanicStrategy(visitorDelegate);
            await panicStrategy.SendHit(new Flagship.Hit.Screen("")).ConfigureAwait(false);
            fsLogManagerMock.Verify(x => x.Info(string.Format(Constants.METHOD_DEACTIVATED_ERROR, "SendHit", FlagshipStatus.READY_PANIC_ON), "SendHit"), Times.Once());
        }

        [TestMethod()]
        public void GetFlagValueTest()
        {
            var panicStrategy = new PanicStrategy(visitorDelegate);
            var value = panicStrategy.GetFlagValue("key", "defaultValue", null);
            Assert.AreEqual(value, "defaultValue");
            fsLogManagerMock.Verify(x => x.Info(string.Format(Constants.METHOD_DEACTIVATED_ERROR, "Flag.value", FlagshipStatus.READY_PANIC_ON), "Flag.value"), Times.Once());
        }

        [TestMethod()]
        public async Task UserExposedTest()
        {
            var panicStrategy = new PanicStrategy(visitorDelegate);
            await panicStrategy.VisitorExposed("key", "defaultValue", null).ConfigureAwait(false);
            fsLogManagerMock.Verify(x => x.Info(string.Format(Constants.METHOD_DEACTIVATED_ERROR, "VisitorExposed", FlagshipStatus.READY_PANIC_ON), "VisitorExposed"), Times.Once());
        }

        [TestMethod()]
        public void GetFlagMetadataTest()
        {
            var panicStrategy = new PanicStrategy(visitorDelegate);
            var value = panicStrategy.GetFlagMetadata(null, "key", false);
            Assert.AreEqual(JsonConvert.SerializeObject(FsFlag.FlagMetadata.EmptyMetadata()), JsonConvert.SerializeObject(value));
            fsLogManagerMock.Verify(x => x.Info(string.Format(Constants.METHOD_DEACTIVATED_ERROR, "Flag.metadata", FlagshipStatus.READY_PANIC_ON), "Flag.metadata"), Times.Once());
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

            var strategy = new PanicStrategy(visitorDelegate);

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
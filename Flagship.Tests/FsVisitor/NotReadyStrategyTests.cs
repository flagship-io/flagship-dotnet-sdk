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

namespace Flagship.FsVisitor.Tests
{
    [TestClass()]
    public class NotReadyStrategyTests
    {
        private Mock<Flagship.Utils.IFsLogManager> fsLogManagerMock;
        private VisitorDelegate visitorDelegate;
        private Mock<Flagship.Decision.DecisionManager> decisionManagerMock;
        private Mock<Flagship.Api.ITrackingManager> trackingManagerMock;
        public NotReadyStrategyTests()
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
        public void NotReadyStrategyTest()
        {

        }

        [TestMethod()]
        public async Task FetchFlagsTest()
        {
            var notReadyStategy = new NotReadyStrategy(visitorDelegate);
            await notReadyStategy.FetchFlags().ConfigureAwait(false);
            fsLogManagerMock.Verify(x => x.Error(string.Format(Constants.METHOD_DEACTIVATED_ERROR, "FetchFlags", FlagshipStatus.NOT_INITIALIZED), "FetchFlags"), Times.Once());
        }

        [TestMethod()]
        public async Task SendHitTest()
        {
            var notReadyStategy = new NotReadyStrategy(visitorDelegate);
            await notReadyStategy.SendHit(new Hit.Screen("Home")).ConfigureAwait(false);
            fsLogManagerMock.Verify(x => x.Error(string.Format(Constants.METHOD_DEACTIVATED_ERROR, "SendHit", FlagshipStatus.NOT_INITIALIZED), "SendHit"), Times.Once());
        }

        [TestMethod()]
        public void GetFlagValueTest()
        {
            var defaultValue = "defaultValue";
            var notReadyStategy = new NotReadyStrategy(visitorDelegate);
            var value = notReadyStategy.GetFlagValue("key", defaultValue, null);

            Assert.AreEqual(defaultValue, value);

            fsLogManagerMock.Verify(x => x.Error(string.Format(Constants.METHOD_DEACTIVATED_ERROR, "Flag.value", FlagshipStatus.NOT_INITIALIZED), "Flag.value"), Times.Once());
        }

        [TestMethod()]
        public async Task UserExposedTest()
        {
            var notReadyStategy = new NotReadyStrategy(visitorDelegate);
            await notReadyStategy.UserExposed("key","defaultValue", null).ConfigureAwait(false);
            fsLogManagerMock.Verify(x => x.Error(string.Format(Constants.METHOD_DEACTIVATED_ERROR, "UserExposed", FlagshipStatus.NOT_INITIALIZED), "UserExposed"), Times.Once());
        }

        [TestMethod()]
        public void GetFlagMetadataTest()
        {
            var notReadyStategy = new NotReadyStrategy(visitorDelegate);
            var value = notReadyStategy.GetFlagMetadata(null,"key", false);

            Assert.AreEqual(JsonConvert.SerializeObject(FsFlag.FlagMetadata.EmptyMetadata()), JsonConvert.SerializeObject(value));

            fsLogManagerMock.Verify(x => x.Error(string.Format(Constants.METHOD_DEACTIVATED_ERROR, "flag.metadata", FlagshipStatus.NOT_INITIALIZED), "flag.metadata"), Times.Once());
        }
    }
}
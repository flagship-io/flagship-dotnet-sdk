using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Flagship.Logger;
using Flagship.Hit;
using Flagship.Model;
using Flagship.Api;
using Flagship.Config;
using Murmur;

namespace Flagship.FsVisitor.Tests
{
    [TestClass()]
    public class VisitorStrategyAbstractTests
    {
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

            var strategy = new DefaultStrategy(visitorDelegate);

            var troubleshootingHit = new Troubleshooting();

            //Test SendTroubleshootingHit

            await strategy.SendTroubleshootingHit(troubleshootingHit);

            trackingManagerMock.Verify(x => x.SendTroubleshootingHit(troubleshootingHit), Times.Once());

            //Test AddTroubleshootingHitTest

            strategy.AddTroubleshootingHit(troubleshootingHit);

            trackingManagerMock.Verify(x => x.AddTroubleshootingHit(troubleshootingHit), Times.Once());
        }


        [TestMethod()]
        public void GetTroubleshootingDataTest()
        {
            var fsLogManagerMock = new Mock<IFsLogManager>();
            var config = new DecisionApiConfig()
            {
                EnvId = "envID",
                LogManager = fsLogManagerMock.Object,
                DisableDeveloperUsageTracking = true,
                TrackingManagerConfig = new TrackingManagerConfig()
            };

            var trackingManager = new TrackingManager(config, new System.Net.Http.HttpClient());

            var decisionManagerMock = new Mock<Decision.DecisionManager>(new object[] { null, null });

            var decisionManager = decisionManagerMock.Object;
            decisionManager.TrackingManager = trackingManager;

            var troubleshootingData = new TroubleshootingData();

            decisionManager.TroubleshootingData = troubleshootingData;

            var configManager = new ConfigManager(config, decisionManager, trackingManager);

            var context = new Dictionary<string, object>()
            {
                ["key"] = 1,
            };

            var visitorDelegate = new VisitorDelegate("visitorId", false, context, false, configManager);

            var strategy = new DefaultStrategy(visitorDelegate);

            var check = strategy.GetTroubleshootingData();

            Assert.AreEqual(troubleshootingData, check);
            Assert.AreEqual(troubleshootingData, trackingManager.TroubleshootingData);
        }

        [TestMethod()]
        public async Task SendFetchFlagsTroubleshootingHitTest()
        {
            var fsLogManagerMock = new Mock<IFsLogManager>();
            var config = new DecisionApiConfig()
            {
                EnvId = "envID",
                LogManager = fsLogManagerMock.Object,
                DisableDeveloperUsageTracking = true,
                TrackingManagerConfig = new TrackingManagerConfig()
            };

            var trackingManagerMock = new Mock<Api.ITrackingManager>();
            var trackingManager = trackingManagerMock.Object;

            var decisionManagerMock = new Mock<Decision.DecisionManager>(new object[] { null, null });

            var decisionManager = decisionManagerMock.Object;
            decisionManager.TrackingManager = trackingManager;

            var troubleshootingData = new TroubleshootingData();

            decisionManager.TroubleshootingData = troubleshootingData;

            var configManager = new ConfigManager(config, decisionManager, trackingManagerMock.Object);

            var context = new Dictionary<string, object>()
            {
                ["key"] = 1,
            };

            var visitorDelegate = new VisitorDelegate("visitorId", false, context, false, configManager)
            {
                Flags = new List<FlagDTO>
            {
                new FlagDTO()
                {
                    Key= "key",
                    CampaignId = "campaignId",
                    CampaignName = "campaignName",
                    CampaignType = "ab",
                    IsReference = false,
                    Value = "value",
                    VariationGroupId = "variationGrId",
                    VariationGroupName = "variationGrName",
                    VariationId = "id",
                    VariationName= "name",
                }
            },

                ConsentHitTroubleshooting = new Troubleshooting(),
                SegmentHitTroubleshooting = new Troubleshooting()
            };

            var strategy = new DefaultStrategy(visitorDelegate)
            {
                Murmur32 = MurmurHash.Create32()
            };

            await strategy.SendFetchFlagsTroubleshootingHit([], DateTime.Now);

            trackingManagerMock.Verify(x => x.SendTroubleshootingHit(It.Is<Troubleshooting>(y => y.Label == Enums.DiagnosticLabel.VISITOR_FETCH_CAMPAIGNS)), Times.Once());
            trackingManagerMock.Verify(x => x.SendTroubleshootingHit(It.Is<Troubleshooting>(y => y.Label == Enums.DiagnosticLabel.VISITOR_SEND_HIT)), Times.Exactly(3));

            decisionManager.TroubleshootingData = null;

            await strategy.SendFetchFlagsTroubleshootingHit([], DateTime.Now);

            trackingManagerMock.Verify(x => x.SendTroubleshootingHit(It.Is<Troubleshooting>(y => y.Label == Enums.DiagnosticLabel.VISITOR_FETCH_CAMPAIGNS)), Times.Once());
            trackingManagerMock.Verify(x => x.SendTroubleshootingHit(It.Is<Troubleshooting>(y => y.Label == Enums.DiagnosticLabel.VISITOR_SEND_HIT)), Times.Exactly(3));
        }

        [TestMethod()]
        public async Task SendUsageHitTest()
        {
            var fsLogManagerMock = new Mock<IFsLogManager>();
            var config = new DecisionApiConfig()
            {
                EnvId = "envID",
                LogManager = fsLogManagerMock.Object,
                TrackingManagerConfig = new TrackingManagerConfig()
            };

            var trackingManagerMock = new Mock<Api.ITrackingManager>();
            var trackingManager = trackingManagerMock.Object;

            var decisionManagerMock = new Mock<Decision.DecisionManager>(new object[] { null, null });

            var decisionManager = decisionManagerMock.Object;
            decisionManager.TrackingManager = trackingManager;

            var troubleshootingData = new TroubleshootingData();

            decisionManager.TroubleshootingData = troubleshootingData;

            var configManager = new ConfigManager(config, decisionManager, trackingManagerMock.Object);

            var context = new Dictionary<string, object>()
            {
                ["key"] = 1,
            };

            var visitorDelegate = new VisitorDelegate("8b48f7c7-97ff-4002-a958-108f8aa8b58e", false, context, false, configManager)
            {
                ConsentHitTroubleshooting = new Troubleshooting(),
                SegmentHitTroubleshooting = new Troubleshooting()
            };

            var strategyMock = new Mock<DefaultStrategy>(visitorDelegate)
            {
                CallBase = true
            };

            var currentTime = new DateTime(2024,1,29);
            strategyMock.SetupGet(x => x.CurrentDateTime).Returns(currentTime);

            var strategy = strategyMock.Object;
            strategy.Murmur32 = MurmurHash.Create32();

            await strategy.SendUsageHitSdkConfig();

            config.DisableDeveloperUsageTracking = true;

            await strategy.SendUsageHitSdkConfig();

            trackingManagerMock.Verify(x => x.SendUsageHit(It.Is<UsageHit>(y => y.Label == Enums.DiagnosticLabel.SDK_CONFIG)), Times.Once());
        }
    }
}
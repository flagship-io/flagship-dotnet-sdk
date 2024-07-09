using Microsoft.VisualStudio.TestTools.UnitTesting;
using Flagship.FsFlag;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using Flagship.Config;
using Flagship.Model;
using Newtonsoft.Json;
using Flagship.Enums;
using Flagship.Api;

namespace Flagship.Tests.FsFlag
{
    [TestClass()]
    public class FlagTests
    {
        private FlagDTO GetFlag()
        {
            var flagDTO = new FlagDTO
            {
                Key = "key",
                Value = "value",
                VariationId = "variationID",
                CampaignId = "campaignID",
                VariationGroupId = "variationGroupID",
                IsReference = true,
                CampaignType = "ab",
                Slug = "slujg"
            };
            return flagDTO;
        }

        [TestMethod()]
        async public Task FlagTest()
        {
            var flagDTO = GetFlag();
            var config = new DecisionApiConfig()
            {
                EnvId = "envID"
            };

            var metadata = new FlagMetadata(flagDTO.CampaignId, flagDTO.VariationGroupId, flagDTO.VariationId, flagDTO.IsReference, flagDTO.CampaignType, flagDTO.Slug, flagDTO.CampaignName, flagDTO.VariationGroupName, flagDTO.VariationName);

            var trackingManagerMock = new Mock<ITrackingManager>();
            var decisionManagerMock = new Mock<Decision.IDecisionManager>();
            var configManager = new ConfigManager(config, decisionManagerMock.Object, trackingManagerMock.Object);

            var context = new Dictionary<string, object>();
            var visitorMock = new Mock<FsVisitor.VisitorDelegateAbstract>(["visitorId", false, context, false, configManager, null]);

            visitorMock.Setup(x => x.GetStrategy()).CallBase();

            var flags = new List<FlagDTO>
            {
                flagDTO
            };

            visitorMock.SetupGet(x => x.Flags).Returns(flags);

            var defaultValue = "defaultString";
            var flag = new Flag(flagDTO.Key, visitorMock.Object);

            visitorMock.Setup(x => x.GetFlagValue(flagDTO.Key, defaultValue, flagDTO, true)).Returns((string)flagDTO.Value);
            visitorMock.Setup(x => x.VisitorExposed(flagDTO.Key, defaultValue, flagDTO, true)).Returns(Task.CompletedTask);
            visitorMock.Setup(x => x.GetFlagMetadata(flagDTO.Key, It.IsAny<FlagDTO>())).Returns(metadata);
            visitorMock.SetupGet(x => x.FetchFlagsStatus).Returns(new FetchFlagsStatus()
            {
                Status = FSFetchStatus.FETCHED,
                Reason = FSFetchReasons.NONE
            });

            var value = flag.GetValue(defaultValue);

            await flag.VisitorExposed().ConfigureAwait(false);
            var resultMeta = flag.Metadata;

            Assert.AreEqual(flagDTO.Value, value);
            Assert.IsTrue(flag.Exists);
            Assert.AreEqual(metadata, resultMeta);
            Assert.AreEqual(FSFlagStatus.FETCHED, flag.Status);

            visitorMock.SetupGet(x => x.FetchFlagsStatus).Returns(new FetchFlagsStatus()
            {
                Status = FSFetchStatus.FETCH_REQUIRED,
                Reason = FSFetchReasons.UPDATE_CONTEXT
            });

            Assert.AreEqual(FSFlagStatus.FETCH_REQUIRED, flag.Status);

            visitorMock.Verify(x => x.VisitorExposed<object>(flagDTO.Key, defaultValue, flagDTO, true), Times.Once());
            visitorMock.Verify(x => x.GetFlagValue(flagDTO.Key, defaultValue, flagDTO, true), Times.Once());
            visitorMock.Verify(x => x.GetFlagMetadata(flagDTO.Key, flagDTO), Times.Once());
        }

        [TestMethod()]
        async public Task FlagNotExistTest()
        {
            var flagDTO = GetFlag();
            var config = new DecisionApiConfig()
            {
                EnvId = "envID"
            };

            var trackingManagerMock = new Mock<ITrackingManager>();
            var decisionManagerMock = new Mock<Decision.IDecisionManager>();
            var configManager = new ConfigManager(config, decisionManagerMock.Object, trackingManagerMock.Object);

            var context = new Dictionary<string, object>();
            var visitorMock = new Mock<FsVisitor.VisitorDelegateAbstract>(new object[] { "visitorId", false, context, false, configManager, null });

            visitorMock.Setup(x => x.GetStrategy()).CallBase();

            var flags = new List<FlagDTO>
            {
                flagDTO
            };

            visitorMock.SetupGet(x => x.Flags).Returns(flags);


            visitorMock.SetupGet(x => x.Flags).Returns([]);

            var keyNotExists = "keyNotExists";
            var defaultValue = "defaultString";
            var flag = new Flag(keyNotExists, visitorMock.Object);

            visitorMock.Setup(x => x.GetFlagValue(keyNotExists, defaultValue, null, true)).Returns(defaultValue);
            visitorMock.Setup(x => x.VisitorExposed(keyNotExists, defaultValue, null, true)).Returns(Task.CompletedTask);
            visitorMock.Setup(x => x.GetFlagMetadata(keyNotExists, null)).Returns(FlagMetadata.EmptyMetadata());

            var value = flag.GetValue(defaultValue);

            visitorMock.Verify(x => x.VisitorExposed<object>(keyNotExists, defaultValue, null, true), Times.Never());

            await flag.VisitorExposed().ConfigureAwait(false);
            var resultMeta = flag.Metadata;

            Assert.AreEqual(defaultValue, value);
            Assert.IsFalse(flag.Exists);
            Assert.AreEqual(FlagMetadata.EmptyMetadata().ToJson(), resultMeta.ToJson());


            visitorMock.Verify(x => x.GetFlagValue(keyNotExists, defaultValue, null, true), Times.Once());
            visitorMock.Verify(x => x.GetFlagMetadata(It.IsAny<string>(), null), Times.Once());
            visitorMock.Verify(x => x.VisitorExposed<object>(keyNotExists, defaultValue, null, true), Times.Once());
            
            visitorMock.SetupGet(x => x.FetchFlagsStatus).Returns(new FetchFlagsStatus()
            {
                Status = FSFetchStatus.FETCH_REQUIRED,
                Reason = FSFetchReasons.UPDATE_CONTEXT
            });

            Assert.AreEqual(FSFlagStatus.NOT_FOUND, flag.Status);
        }



        [TestMethod()]
        public void FlagNullTest()
        {
            var config = new DecisionApiConfig()
            {
                EnvId = "envID"
            };
            var trackingManagerMock = new Mock<ITrackingManager>();
            var decisionManagerMock = new Mock<Decision.IDecisionManager>();
            var configManager = new ConfigManager(config, decisionManagerMock.Object, trackingManagerMock.Object);

            var context = new Dictionary<string, object>();
            var visitorMock = new Mock<FsVisitor.VisitorDelegateAbstract>(["visitorId", false, context, false, configManager, null]);

            visitorMock.Setup(x => x.GetStrategy()).CallBase();

            var flag = new Flag("key", visitorMock.Object);

            var metadata = new FlagMetadata("", "", "", false, "", null, "", "", "");

            var resultMeta = flag.Metadata;

            Assert.IsFalse(flag.Exists);
            Assert.AreEqual(JsonConvert.SerializeObject(metadata), JsonConvert.SerializeObject(resultMeta));

            visitorMock.Verify(x => x.GetFlagMetadata(It.IsAny<string>(), null), Times.Never());
        }

        [TestMethod()]
        public void FlagWithNullVisitorTest()
        {
            var flag = new Flag("key", null);

            var resultMeta = flag.Metadata;

            Assert.IsFalse(flag.Exists);
            Assert.AreEqual(JsonConvert.SerializeObject(FlagMetadata.EmptyMetadata()), JsonConvert.SerializeObject(resultMeta));

            var value = flag.GetValue("defaultValue");

            Assert.AreEqual("defaultValue", value);

            flag.VisitorExposed().Wait();

            Assert.AreEqual(FSFlagStatus.NOT_FOUND, flag.Status);
        }

        [TestMethod()]
        public void FlagStatusTest()
        {
            var flagDTO = GetFlag();
            var config = new DecisionApiConfig()
            {
                EnvId = "envID"
            };

            var trackingManagerMock = new Mock<ITrackingManager>();
            var decisionManagerMock = new Mock<Decision.IDecisionManager>();
            var configManager = new ConfigManager(config, decisionManagerMock.Object, trackingManagerMock.Object);

            var context = new Dictionary<string, object>();
            var visitorMock = new Mock<FsVisitor.VisitorDelegateAbstract>(["visitorId", false, context, false, configManager, null]);

            visitorMock.Setup(x => x.GetStrategy()).CallBase();

            var flags = new List<FlagDTO>
            {
                flagDTO
            };

            visitorMock.SetupGet(x => x.Flags).Returns(flags);

            var flag = new Flag(flagDTO.Key, visitorMock.Object);

            visitorMock.SetupGet(x => x.FetchFlagsStatus).Returns(new FetchFlagsStatus()
            {
                Status = FSFetchStatus.PANIC,
                Reason = FSFetchReasons.NONE
            });

            Assert.AreEqual(FSFlagStatus.PANIC, flag.Status);

            visitorMock.SetupGet(x => x.FetchFlagsStatus).Returns(new FetchFlagsStatus()
            {
                Status = FSFetchStatus.FETCHING,
                Reason = FSFetchReasons.NONE
            });

            Assert.AreEqual(FSFlagStatus.FETCHED, flag.Status);

            visitorMock.SetupGet(x => x.FetchFlagsStatus).Returns(new FetchFlagsStatus()
            {
                Status = FSFetchStatus.FETCH_REQUIRED,
                Reason = FSFetchReasons.UPDATE_CONTEXT
            });

            Assert.AreEqual(FSFlagStatus.FETCH_REQUIRED, flag.Status);

        }
    }
}
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

namespace Flagship.FsFlag.Tests
{
    [TestClass()]
    public class FlagTests
    {
        private FlagDTO GetFlag()
        {
            var flagDTO = new Flagship.Model.FlagDTO
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

            var trackingManagerMock = new Mock<Api.ITrackingManager>();
            var decisionManagerMock = new Mock<Decision.IDecisionManager>();
            var configManager = new ConfigManager(config, decisionManagerMock.Object, trackingManagerMock.Object);

            var context = new Dictionary<string, object>();
            var visitorMock = new Mock<FsVisitor.VisitorDelegateAbstract>(new object[] { "visitorId", false, context, false, configManager, null });

            visitorMock.Setup(x=> x.GetStrategy()).CallBase();

            var flags = new List<FlagDTO>
            {
                flagDTO
            };

            visitorMock.SetupGet(x => x.Flags).Returns(flags);

            var defaultValue = "defaultString";
            var flag = new Flag<string>(flagDTO.Key, visitorMock.Object, defaultValue);

            visitorMock.Setup(x => x.GetFlagValue(flagDTO.Key, defaultValue, flagDTO, true)).Returns((string)flagDTO.Value);
            visitorMock.Setup(x => x.VisitorExposed(flagDTO.Key, defaultValue, flagDTO)).Returns(Task.CompletedTask);
            visitorMock.Setup(x=> x.GetFlagMetadata(It.IsAny<IFlagMetadata>(), flagDTO.Key, true)).Returns(metadata);
            visitorMock.Object.FetchFlagsStatus = new FetchFlagsStatus()
            {
                Status = FSFetchStatus.FETCHED,
                Reason = FSFetchReasons.NONE
            };

            var value = flag.GetValue();

            await flag.VisitorExposed().ConfigureAwait(false);
            var resultMeta = flag.Metadata;

            Assert.AreEqual(flagDTO.Value, value);
            Assert.IsTrue(flag.Exists);
            Assert.AreEqual(metadata, resultMeta);
            Assert.AreEqual(FSFlagStatus.FETCHED, flag.Status);


            visitorMock.Object.FetchFlagsStatus = new FetchFlagsStatus()
            {
                Status = FSFetchStatus.FETCH_REQUIRED,
                Reason = FSFetchReasons.UPDATE_CONTEXT
            };

            Assert.AreEqual(FSFlagStatus.FETCH_REQUIRED, flag.Status);

            visitorMock.Verify(x => x.VisitorExposed<object>(flagDTO.Key, defaultValue, flagDTO), Times.Once());
            visitorMock.Verify(x => x.GetFlagValue(flagDTO.Key, defaultValue, flagDTO, true), Times.Once());
            visitorMock.Verify(x => x.GetFlagMetadata(It.Is<IFlagMetadata>(item=>
            item.CampaignId == metadata.CampaignId &&
            item.IsReference == metadata.IsReference && 
            item.VariationGroupId == metadata.VariationGroupId &&
            item.VariationId == metadata.VariationId &&
            item.CampaignType == metadata.CampaignType &&
            item.Slug == metadata.Slug
            
            ), flagDTO.Key, true), Times.Once());

            visitorMock.SetupGet(x => x.Flags).Returns((ICollection<FlagDTO>) null);
            var keyNotExists = "keyNotExists";
             defaultValue = "defaultString";
            flag = new Flag<string>(keyNotExists, visitorMock.Object, defaultValue);

            visitorMock.Setup(x => x.GetFlagValue(keyNotExists, defaultValue, null, true)).Returns(defaultValue);
            visitorMock.Setup(x => x.VisitorExposed(keyNotExists, defaultValue, null)).Returns(Task.CompletedTask);
            visitorMock.Setup(x => x.GetFlagMetadata(It.IsAny<IFlagMetadata>(), keyNotExists, true)).Returns(metadata);

            value = flag.GetValue();

            await flag.VisitorExposed().ConfigureAwait(false);
            resultMeta = flag.Metadata;

            Assert.AreEqual(defaultValue, value);
            Assert.IsFalse(flag.Exists);
            Assert.AreEqual(FlagMetadata.EmptyMetadata().ToJson(), resultMeta.ToJson());

            visitorMock.Verify(x => x.VisitorExposed<object>(keyNotExists, defaultValue, null), Times.Once());
            visitorMock.Verify(x => x.GetFlagValue(keyNotExists, defaultValue, null, true), Times.Once());
            visitorMock.Verify(x => x.GetFlagMetadata(It.IsAny<IFlagMetadata>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Once());

            Assert.AreEqual(FSFlagStatus.NOT_FOUND, flag.Status);
        }

        [TestMethod()]
        public void FlagNullTest()
        {
            var config = new DecisionApiConfig()
            {
                EnvId = "envID"
            };
            var trackingManagerMock = new Mock<Api.ITrackingManager>();
            var decisionManagerMock = new Mock<Decision.IDecisionManager>();
            var configManager = new ConfigManager(config, decisionManagerMock.Object, trackingManagerMock.Object);

            var context = new Dictionary<string, object>();
            var visitorMock = new Mock<FsVisitor.VisitorDelegateAbstract>(new object[] { "visitorId", false, context, false, configManager, null });

            visitorMock.Setup(x=> x.GetStrategy()).CallBase();

            var defaultValue = "defaultString";
            var flag = new Flag<string>("key", visitorMock.Object, defaultValue);

            var metadata = new FlagMetadata("", "", "", false, "", null, "", "", "");

            var resultMeta = flag.Metadata;

            Assert.IsFalse(flag.Exists);
            Assert.AreEqual(JsonConvert.SerializeObject(metadata), JsonConvert.SerializeObject(resultMeta));

            visitorMock.Verify(x => x.GetFlagMetadata(It.IsAny<IFlagMetadata>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never());
        }
    }
}
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

            var metadata = new FlagMetadata(flagDTO.CampaignId, flagDTO.VariationGroupId, flagDTO.VariationId, flagDTO.IsReference, "");

            var trackingManagerMock = new Mock<Api.ITrackingManager>();
            var decisionManagerMock = new Mock<Decision.IDecisionManager>();
            var configManager = new ConfigManager(config, decisionManagerMock.Object, trackingManagerMock.Object);

            var context = new Dictionary<string, object>();
            var visitorMock = new Mock<FsVisitor.VisitorDelegateAbstract>(new object[] { "visitorId", false, context, false, configManager });

            visitorMock.Protected().Setup("GetStrategy").CallBase();

            var defaultValue = "defaultString";
            var flag = new Flag<string>(flagDTO.Key, visitorMock.Object, flagDTO, defaultValue);

            visitorMock.Setup(x => x.GetFlagValue(flagDTO.Key, defaultValue, flagDTO, true)).Returns((string)flagDTO.Value);
            visitorMock.Setup(x => x.UserExposed(flagDTO.Key, defaultValue, flagDTO)).Returns(Task.CompletedTask);
            visitorMock.Setup(x=> x.GetFlagMetadata(It.IsAny<IFlagMetadata>(), flagDTO.Key, true)).Returns(metadata);

            var value = flag.Value();

            await flag.UserExposed().ConfigureAwait(false);
            var resultMeta = flag.Metadata;

            Assert.AreEqual(flagDTO.Value, value);
            Assert.IsTrue(flag.Exist);
            Assert.AreEqual(metadata, resultMeta);

            visitorMock.Verify(x => x.UserExposed<object>(flagDTO.Key, defaultValue, flagDTO), Times.Once());
            visitorMock.Verify(x => x.GetFlagValue(flagDTO.Key, defaultValue, flagDTO, true), Times.Once());
            visitorMock.Verify(x => x.GetFlagMetadata(It.Is<IFlagMetadata>(item=>
            item.CampaignId == metadata.CampaignId &&
            item.IsReference == metadata.IsReference && 
            item.VariationGroupId == metadata.VariationGroupId &&
            item.VariationId == metadata.VariationId &&
            item.CampaignType == metadata.CampaignType 
            
            ), flagDTO.Key, true), Times.Once());
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
            var visitorMock = new Mock<FsVisitor.VisitorDelegateAbstract>(new object[] { "visitorId", false, context, false, configManager });

            visitorMock.Protected().Setup("GetStrategy").CallBase();

            var defaultValue = "defaultString";
            var flag = new Flag<string>("key", visitorMock.Object, null, defaultValue);

            var metadata = new FlagMetadata("", "", "", false, "");

            var resultMeta = flag.Metadata;

            Assert.IsFalse(flag.Exist);
            Assert.AreEqual(JsonConvert.SerializeObject(metadata), JsonConvert.SerializeObject(resultMeta));

            visitorMock.Verify(x => x.GetFlagMetadata(It.IsAny<IFlagMetadata>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never());
        }
    }
}
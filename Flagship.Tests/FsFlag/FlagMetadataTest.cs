using Flagship.FsFlag;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flagship.Tests.FsFlag
{
    [TestClass()]
    public class FlagMetadataTest
    {
        [TestMethod()]
        public void ToJson()
        {
            var metadata = new FlagMetadata("CampaignId", "VariationGroupId", "VariationId", true, "", null, "CampaignName", "VariaitonGroupName", "VariationName");

            var metadataJson = new Dictionary<string, object>()
            {
                ["campaignId"] = "CampaignId",
                ["variationGroupId"] = "VariationGroupId",
                ["variationId"] = "VariationId",
                ["isReference"] = true,
                ["campaignType"] = "",
                ["slug"] = null,
                ["campaignName"] = "CampaignName",
                ["variationGroupName"] = "VariaitonGroupName",
                ["variationName"] = "VariationName",
            };
            Assert.AreEqual(metadata.ToJson(), JsonConvert.SerializeObject(metadataJson));
        }

        [TestMethod()]
        public void EmptyMetadata()
        {
            Assert.AreEqual(JsonConvert.SerializeObject(FlagMetadata.EmptyMetadata()), 
                JsonConvert.SerializeObject(new FlagMetadata("","","", false,"", null, "", "", "")));
        }
    }
}

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flagship.Model.Bucketing.Tests
{
    [TestClass]
    public class CampaignTests
    {
        [TestMethod] public void CampaignTest()
        {
            var id = Guid.NewGuid().ToString();
            var slug = "slug";
            var type = "ab";
            var variableGroups = new List<VariationGroup>();

            var campaign = new Campaign
            {
                Id = id,
                Slug = slug,
                Type = type,
                VariationGroups = variableGroups,
            };

            Assert.AreEqual(id, campaign.Id);
            Assert.AreEqual(slug, campaign.Slug);
            Assert.AreEqual(type, campaign.Type);
            Assert.AreSame(variableGroups, campaign.VariationGroups);
        }
    }
}

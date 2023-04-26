﻿using Flagship.Model;
using Flagship.Model.Bucketing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flagship.Model.Tests
{
    [TestClass()]
    public class CampaignTests
    {
        [TestMethod()]
        public void CampaignTest()
        {
            var id = Guid.NewGuid().ToString();
            var slug = "slug";
            var type = "ab";
            var variable = new Variation();
            var variableGroupId = "variableGroupeId";

            var campaign = new Campaign
            {
                Id = id,
                Slug = slug,
                Type = type,
                Variation = variable,
                VariationGroupId = variableGroupId,
            };

            Assert.AreEqual(id, campaign.Id);
            Assert.AreEqual(slug, campaign.Slug);
            Assert.AreEqual(type, campaign.Type);
            Assert.AreSame(variable, campaign.Variation);
            Assert.AreEqual(variableGroupId, campaign.VariationGroupId);
        }
    }
}

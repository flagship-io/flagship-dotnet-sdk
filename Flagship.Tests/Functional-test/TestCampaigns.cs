﻿using Flagship.Config;
using Flagship.Main;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Flagship.Tests.Functional_test
{
    [TestClass]
    public class TestCampaigns
    {
        [TestMethod]
        public async Task TestAPIMode()
        {
            var envID = Environment.GetEnvironmentVariable("FS_ENV_ID");
            var apiKey = Environment.GetEnvironmentVariable("FS_API_KEY");

            Fs.Start(envID, apiKey);

            var context = new Dictionary<string, object>()
            {
                ["ci-test"] = true,
                ["test-ab"] = true
            };

            var visitor = Fs.NewVisitor("visitor-1", true).SetContext(context).Build();

            await visitor.FetchFlags();

            var defaultValue = "default-value";

            var flag = visitor.GetFlag("ci_flag_1");
            Assert.AreEqual(defaultValue, flag.GetValue(defaultValue));
            Assert.AreEqual("Test-campaign ab", flag.Metadata.CampaignName);


            visitor = Fs.NewVisitor("visitor-3", true).SetContext(context).Build();
            await visitor.FetchFlags();

            flag = visitor.GetFlag("ci_flag_1");
            Assert.AreEqual("flag-1-value-1", flag.GetValue(defaultValue));
            Assert.AreEqual("Test-campaign ab", flag.Metadata.CampaignName);

            visitor.UpdateContext("test-ab", false);
            await visitor.FetchFlags();

            flag = visitor.GetFlag("ci_flag_1");
            Assert.AreEqual(defaultValue, flag.GetValue(defaultValue));
            Assert.AreEqual("", flag.Metadata.CampaignName);
        }

        [TestMethod]
        public async Task TestBucketingMOde()
        {
            var envID = Environment.GetEnvironmentVariable("FS_ENV_ID");
            var apiKey = Environment.GetEnvironmentVariable("FS_API_KEY");

            Fs.Start(envID, apiKey, new BucketingConfig());

            await Task.Delay(1500);

            var context = new Dictionary<string, object>()
            {
                ["ci-test"] = true,
                ["test-ab"] = true
            };

            var visitor = Fs.NewVisitor("visitor-1", true).SetContext(context).Build();

            await visitor.FetchFlags();

            var defaultValue = "default-value";

            var flag = visitor.GetFlag("ci_flag_1");
            Assert.AreEqual(defaultValue, flag.GetValue(defaultValue));
            Assert.AreEqual("Test-campaign ab", flag.Metadata.CampaignName);


            visitor = Fs.NewVisitor("visitor-3", true).SetContext(context).Build();
            await visitor.FetchFlags();

            flag = visitor.GetFlag("ci_flag_1");
            Assert.AreEqual("flag-1-value-1", flag.GetValue(defaultValue));
            Assert.AreEqual("Test-campaign ab", flag.Metadata.CampaignName);

            visitor.UpdateContext("test-ab", false);
            await visitor.FetchFlags();

            flag = visitor.GetFlag("ci_flag_1");
            Assert.AreEqual(defaultValue, flag.GetValue(defaultValue));
            Assert.AreEqual("", flag.Metadata.CampaignName);
        }

    }
}

using Flagship.Config;
using Flagship.Main;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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

            var visitor = Fs.NewVisitor("visitor-1").WithContext(context).Build();

            await visitor.FetchFlags();

            var defaultValue = "default-value";

            var flag = visitor.GetFlag("ci_flag_1", defaultValue);
            Assert.AreEqual(defaultValue, flag.GetValue());
            Assert.AreEqual("Test-campaign ab", flag.Metadata.CampaignName);


            visitor = Fs.NewVisitor("visitor-3").WithContext(context).Build();
            await visitor.FetchFlags();

            flag = visitor.GetFlag("ci_flag_1", defaultValue);
            Assert.AreEqual("flag-1-value-1", flag.GetValue());
            Assert.AreEqual("Test-campaign ab", flag.Metadata.CampaignName);

            visitor.UpdateContext("test-ab", false);
            await visitor.FetchFlags();

            flag = visitor.GetFlag("ci_flag_1", defaultValue);
            Assert.AreEqual(defaultValue, flag.GetValue());
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

            var visitor = Fs.NewVisitor("visitor-1").WithContext(context).Build();

            await visitor.FetchFlags();

            var defaultValue = "default-value";

            var flag = visitor.GetFlag("ci_flag_1", defaultValue);
            Assert.AreEqual(defaultValue, flag.GetValue());
            Assert.AreEqual("Test-campaign ab", flag.Metadata.CampaignName);


            visitor = Fs.NewVisitor("visitor-3").WithContext(context).Build();
            await visitor.FetchFlags();

            flag = visitor.GetFlag("ci_flag_1", defaultValue);
            Assert.AreEqual("flag-1-value-1", flag.GetValue());
            Assert.AreEqual("Test-campaign ab", flag.Metadata.CampaignName);

            visitor.UpdateContext("test-ab", false);
            await visitor.FetchFlags();

            flag = visitor.GetFlag("ci_flag_1", defaultValue);
            Assert.AreEqual(defaultValue, flag.GetValue());
            Assert.AreEqual("", flag.Metadata.CampaignName);
        }

    }
}

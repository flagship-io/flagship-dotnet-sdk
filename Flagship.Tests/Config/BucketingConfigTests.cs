using Microsoft.VisualStudio.TestTools.UnitTesting;
using Flagship.Enums;

namespace Flagship.Config.Tests
{
    [TestClass()]
    public class BucketingConfigTests
    {
        [TestMethod()]
        public void BucketingConfigTest()
        {
            var config = new Flagship.Config.BucketingConfig
            {
                ApiKey = "apiKey",
                EnvId = "envId"
            };

            Assert.AreEqual(config.DecisionMode, Enums.DecisionMode.BUCKETING);
            Assert.AreEqual(config.PollingInterval, TimeSpan.FromMilliseconds(Constants.DEFAULT_POLLING_INTERVAL));
        }
    }
}
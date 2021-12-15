using Microsoft.VisualStudio.TestTools.UnitTesting;
using Flagship.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flagship.Config.Tests
{
    [TestClass()]
    public class DecisionApiConfigTests
    {
        [TestMethod()]
        public void DecisionApiConfigTest()
        {
            var config = new Flagship.Config.DecisionApiConfig
            {
                ApiKey = "apiKey",
                EnvId = "envId"
            };


            config.StatusChange += Config_StatusChange;

            Assert.AreEqual(config.ApiKey, "apiKey");
            Assert.AreEqual(config.EnvId, "envId");
            Assert.AreEqual(config.DecisionMode, Enums.DecisionMode.DECISION_API);
            Assert.AreEqual(config.Timeout, TimeSpan.FromMilliseconds(Flagship.Enums.Constants.REQUEST_TIME_OUT));
            Assert.AreEqual(config.LogLevel, Enums.LogLevel.ALL);

            config.SetStatus(Enums.FlagshipStatus.READY);
        }

        private void Config_StatusChange(Enums.FlagshipStatus status)
        {
            Assert.AreEqual(status, Enums.FlagshipStatus.READY);
        }
    }
}
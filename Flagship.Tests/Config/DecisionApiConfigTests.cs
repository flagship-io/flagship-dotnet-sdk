using Microsoft.VisualStudio.TestTools.UnitTesting;
using Flagship.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flagship.FsVisitor;
using Flagship.FsFlag;

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

            config.SetStatus(Enums.FSSdkStatus.SDK_NOT_INITIALIZED);

            config.OnSdkStatusChanged += Config_StatusChange;

            Assert.AreEqual(config.ApiKey, "apiKey");
            Assert.AreEqual(config.EnvId, "envId");
            Assert.AreEqual(config.DecisionMode, Enums.DecisionMode.DECISION_API);
            Assert.AreEqual(config.Timeout, TimeSpan.FromMilliseconds(Flagship.Enums.Constants.REQUEST_TIME_OUT));
            Assert.AreEqual(config.LogLevel, Enums.LogLevel.ALL);
            Assert.AreEqual(config.DisableCache, false);

            config.SetStatus(Enums.FSSdkStatus.SDK_INITIALIZED);

            config.DisableCache = true;
            Assert.AreEqual(config.DisableCache, true);

            var exposedVisitor = new ExposedVisitor("visitorId", "visitorContext", new Dictionary<string, object>());
            var exposedFlag = new ExposedFlag("key", "value", "defaultValue", null);

            void Config_VisitorExposed(IExposedVisitor visitor, IExposedFlag flag)
            {
                Assert.AreEqual(visitor, exposedVisitor);
                Assert.AreEqual(flag, exposedFlag);
            }

            config.OnVisitorExposed += Config_VisitorExposed;

            config.InvokeOnVisitorExposed(exposedVisitor, exposedFlag);
        }


        private void Config_StatusChange(Enums.FSSdkStatus status)
        {
            Assert.AreEqual(status, Enums.FSSdkStatus.SDK_INITIALIZED);
        }
    }
}
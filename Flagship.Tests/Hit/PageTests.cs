using Microsoft.VisualStudio.TestTools.UnitTesting;
using Flagship.Hit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flagship.Enums;

namespace Flagship.Hit.Tests
{
    [TestClass()]
    public class PageTests
    {
        [TestMethod()]
        public void PageTest()
        {
            var config = new Config.DecisionApiConfig()
            {
                EnvId = "envID",
                ApiKey = "apiKey"
            };

            var pageUrl = "http://localhost";
            var visitorId = "VisitorId";

            var page = new Hit.Page(pageUrl)
            {
                Config = config,
                VisitorId = visitorId,
                DS = Constants.SDK_APP
            };

            Assert.AreEqual(page.DocumentLocation, pageUrl);
            Assert.AreEqual(page.Config, config);   
            Assert.AreEqual(page.VisitorId, visitorId);

            var keys = Newtonsoft.Json.JsonConvert.SerializeObject(page.ToApiKeys());

            var apiKeys = new Dictionary<string, object>()
            {
                [Constants.VISITOR_ID_API_ITEM] = visitorId,
                [Constants.DS_API_ITEM] = Constants.SDK_APP,
                [Constants.CUSTOMER_ENV_ID_API_ITEM] = config.EnvId,
                [Constants.T_API_ITEM] = $"{Hit.HitType.PAGEVIEW}",
                [Constants.CUSTOMER_UID] = null,
                [Constants.DL_API_ITEM] = pageUrl,
            };

            var apiKeysJson = Newtonsoft.Json.JsonConvert.SerializeObject(apiKeys);

            Assert.AreEqual(apiKeysJson, keys);

            Assert.IsTrue(page.IsReady());

            Assert.AreEqual(page.GetErrorMessage(), Constants.HIT_PAGE_ERROR_MESSAGE);

            page = new Hit.Page(null);
            Assert.IsFalse(page.IsReady());
        }
    }
}
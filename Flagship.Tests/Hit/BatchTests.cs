using Flagship.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flagship.Hit.Tests
{
    [TestClass()]
    public class BatchTests
    {
        [TestMethod()]
        public void BatchTest()
        {
            var config = new Config.DecisionApiConfig()
            {
                EnvId = "envID",
                ApiKey = "apiKey"
            };

            var visitorId = "VisitorId";

            var batch = new Batch()
            {
                Config = config,
                VisitorId = visitorId,
                DS = Constants.SDK_APP
            };

            var hits = new List<HitAbstract>()
            {
                new Page("home")
            };
            batch.Hits = hits;

            var keys = Newtonsoft.Json.JsonConvert.SerializeObject(batch.ToApiKeys());

            Assert.AreEqual(batch.Hits, hits);

            var apiKeys = new Dictionary<string, object>()
            {
                [Constants.VISITOR_ID_API_ITEM] = visitorId,
                [Constants.DS_API_ITEM] = Constants.SDK_APP,
                [Constants.CUSTOMER_ENV_ID_API_ITEM] = config.EnvId,
                [Constants.T_API_ITEM] = $"{HitType.BATCH}",
                [Constants.CUSTOMER_UID] = null,
                ["h"] = new Collection<IDictionary<string, object>>(){
                    new Dictionary<string, object>()
                {
                    [Constants.T_API_ITEM] = $"{HitType.PAGEVIEW}",
                    [Constants.DL_API_ITEM] = "home"

                }
                }
            };

            var apiKeysJson = Newtonsoft.Json.JsonConvert.SerializeObject(apiKeys);

            Assert.AreEqual(apiKeysJson, keys);

            Assert.IsTrue(batch.IsReady());

            Assert.AreEqual(batch.GetErrorMessage(), Batch.ERROR_MESSAGE);

            batch = new Batch();
            Assert.IsFalse(batch.IsReady());

            hits = new List<HitAbstract>();
            batch.Hits = hits;
            Assert.IsFalse(batch.IsReady());

            hits = new List<HitAbstract>()
            {
                new Page(null)
            };
            batch.Hits = hits;

            Assert.IsFalse(batch.IsReady());
        }
    }
}

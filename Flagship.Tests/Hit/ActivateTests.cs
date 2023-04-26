using Microsoft.VisualStudio.TestTools.UnitTesting;
using Flagship.Hit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flagship.Enums;
using Flagship.Config;
using Newtonsoft.Json;

namespace Flagship.Hit.Tests
{
    [TestClass()]
    public class ActivateTests
    {
        [TestMethod()]
        public void ActivateTest()
        {
            var variationId = "varId";
            var variationGroupId = "varGroupId";
            var config = new DecisionApiConfig()
            {
                EnvId = "envId"
            };

            var activate = new Activate(variationGroupId, variationId)
            {
                Config = config
            };

            Assert.AreEqual(variationId, activate.VariationId);
            Assert.AreEqual(variationGroupId, activate.VariationGroupId);
            Assert.IsFalse(activate.IsReady());
            Assert.AreEqual(Activate.ERROR_MESSAGE, activate.GetErrorMessage());

            var visitorId = "visitorId";

            activate.VisitorId = visitorId;
            Assert.IsTrue(activate.IsReady());

            var apiKeys = new Dictionary<string, object>()
            {
                [Constants.VISITOR_ID_API_ITEM] = visitorId,
                [Constants.VARIATION_ID_API_ITEM] = variationId,
                [Constants.VARIATION_GROUP_ID_API_ITEM_ACTIVATE] = variationGroupId,
                [Constants.CUSTOMER_ENV_ID_API_ACTIVATE] = config.EnvId,
                [Constants.ANONYMOUS_ID] = null
            };

            Assert.AreEqual(JsonConvert.SerializeObject(apiKeys), JsonConvert.SerializeObject(activate.ToApiKeys()));

            var anonymousId = "anonymousId";
            activate.AnonymousId = anonymousId;

            apiKeys[Constants.ANONYMOUS_ID] = anonymousId;
            Assert.AreEqual(JsonConvert.SerializeObject(apiKeys), JsonConvert.SerializeObject(activate.ToApiKeys()));


        }
    }
}
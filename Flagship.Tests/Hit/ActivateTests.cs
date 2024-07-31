using Microsoft.VisualStudio.TestTools.UnitTesting;
using Flagship.Enums;
using Flagship.Config;
using Newtonsoft.Json;
using Moq;

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

            var activateMock = new Mock<Activate>(variationGroupId, variationId)
            {
                CallBase = true
            };

            var currentTime = DateTime.Now;
            activateMock.SetupGet(x => x.CurrentDateTime).Returns(currentTime);

            var activate = activateMock.Object;
            activate.Config = config;


            Assert.AreEqual(variationId, activate.VariationId);
            Assert.AreEqual(variationGroupId, activate.VariationGroupId);
            Assert.IsFalse(activate.IsReady());
            Assert.AreEqual(Activate.ERROR_MESSAGE, activate.GetErrorMessage());

            var visitorId = "visitorId";

            activate.VisitorId = visitorId;
            var check = activate.IsReady();
            Assert.IsTrue(check);

            var apiKeys = new Dictionary<string, object>()
            {
                [Constants.VISITOR_ID_API_ITEM] = visitorId,
                [Constants.VARIATION_ID_API_ITEM] = variationId,
                [Constants.VARIATION_GROUP_ID_API_ITEM_ACTIVATE] = variationGroupId,
                [Constants.CUSTOMER_ENV_ID_API_ACTIVATE] = config.EnvId,
                [Constants.ANONYMOUS_ID] = null,
                [Constants.QT_API_ITEM] = 0
            };

            Assert.AreEqual(JsonConvert.SerializeObject(apiKeys), JsonConvert.SerializeObject(activate.ToApiKeys()));

            var anonymousId = "anonymousId";
            activate.AnonymousId = anonymousId;

            apiKeys[Constants.ANONYMOUS_ID] = anonymousId;
            Assert.AreEqual(JsonConvert.SerializeObject(apiKeys), JsonConvert.SerializeObject(activate.ToApiKeys()));


        }
    }
}
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Flagship.Config;
using Flagship.Enums;
using Newtonsoft.Json;
using Moq;

namespace Flagship.Hit.Tests
{
    [TestClass()]
    public class SegmentTests
    {
        [TestMethod()]
        public void SegmentTest()
        {
            var context = new Dictionary<string, object>()
            {
                ["isVip"] = true,
                ["key2"] = 123,
                ["key3"] = null,
                ["key4"] = "value4"
            };
            var config = new DecisionApiConfig()
            {
                EnvId = "envId"
            };
            var visitorId = "visitorId";

            var segmentMock = new Mock<Segment>(context) { CallBase = true };
            var currentTime = DateTime.Now;
            segmentMock.SetupGet(x => x.CurrentDateTime).Returns(currentTime);

            var page = segmentMock.Object;

            var segment = segmentMock.Object;

            segment.Config = config;

            Assert.AreEqual(context, segment.Context);

            Assert.IsFalse(segment.IsReady());

            segment.VisitorId = visitorId;

            Assert.IsTrue(segment.IsReady());

            Assert.AreEqual(Segment.ERROR_MESSAGE, segment.GetErrorMessage());

            var apiKeys = new Dictionary<string, object>()
            {
                [Constants.VISITOR_ID_API_ITEM] = visitorId,
                [Constants.DS_API_ITEM] = Constants.SDK_APP,
                [Constants.CUSTOMER_ENV_ID_API_ITEM] = config.EnvId,
                [Constants.T_API_ITEM] = $"{Hit.HitType.SEGMENT}",
                [Constants.CUSTOMER_UID] = null,
                [Constants.QT_API_ITEM] = 0,
                [Segment.S_API_ITEM] = new Dictionary<string, string>()
                {
                    ["isVip"] = "True",
                    ["key2"] = "123",
                    ["key3"] = "null",
                    ["key4"] = "value4"
                }
            };

            Assert.AreEqual(JsonConvert.SerializeObject(apiKeys), JsonConvert.SerializeObject(segment.ToApiKeys()));

        }
    }
}
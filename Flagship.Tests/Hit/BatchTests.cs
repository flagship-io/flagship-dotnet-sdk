using Flagship.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.ObjectModel;

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

            var currentTime = DateTime.Now;
            var batchMock = new Mock<Batch>()
            {
                CallBase = true,
            };

            batchMock.SetupGet(x=> x.CurrentDateTime).Returns(currentTime);

            var batch = batchMock.Object;

            batch.DS = Constants.SDK_APP;
            batch.VisitorId = visitorId;
            batch.Config = config;
            batch.CreatedAt = currentTime;

            var pageMock = new Mock<Page>("home")
            {
                CallBase=true,
            };

            pageMock.SetupGet(x => x.CurrentDateTime).Returns(currentTime);
            var page = pageMock.Object;

            page.CreatedAt = currentTime;
            page.VisitorId = visitorId;
            page.Config = config;
            

            var hits = new List<HitAbstract>()
            {
                page
            };
            batch.Hits = hits;

            var keys = Newtonsoft.Json.JsonConvert.SerializeObject(batch.ToApiKeys());

            Assert.AreEqual(batch.Hits, hits);

            var apiKeys = new Dictionary<string, object>()
            {
                [Constants.DS_API_ITEM] = Constants.SDK_APP,
                [Constants.CUSTOMER_ENV_ID_API_ITEM] = config.EnvId,
                [Constants.T_API_ITEM] = $"{HitType.BATCH}",
                [Constants.QT_API_ITEM] = (currentTime - currentTime).Milliseconds,
                ["h"] = new Collection<IDictionary<string, object>>() { 
                    page.ToApiKeys()
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

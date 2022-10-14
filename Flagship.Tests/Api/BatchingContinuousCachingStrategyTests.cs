using Microsoft.VisualStudio.TestTools.UnitTesting;
using Flagship.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flagship.Config;
using Moq;
using Flagship.Logger;
using System.Net.Http;
using System.Threading;
using Moq.Protected;
using Flagship.Hit;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Flagship.Api.Tests
{
    [TestClass()]
    public class BatchingContinuousCachingStrategyTests
    {
        private Mock<IFsLogManager> fsLogManagerMock;
        private DecisionApiConfig config;


        public BatchingContinuousCachingStrategyTests()
        {
            
            
        }

        [TestMethod()]
        public async Task AddTest()
        {
            var httpClientMock = new Mock<HttpClient>();
            var fsLogManagerMock = new Mock<IFsLogManager>();
            var config = new DecisionApiConfig()
            {
                EnvId = "envID",
                LogManager = fsLogManagerMock.Object
            };

            var hitsPoolQueue = new Dictionary<string, HitAbstract>();
            var activatePoolQueue = new Dictionary<string, Activate>();

            var strategyMock = new Mock<BatchingContinuousCachingStrategy>(new object[] { config, httpClientMock.Object,  hitsPoolQueue, activatePoolQueue })
            {
                CallBase = true,
            };

            var strategy = strategyMock.Object;

            var page = new Page("http://localhost");

            await strategy.Add(page).ConfigureAwait(false);

            Assert.AreEqual(1, hitsPoolQueue.Count);
            Assert.AreSame(page, hitsPoolQueue.Values.First());

            strategyMock.Verify(x => x.CacheHitAsync(It.Is<Dictionary<string, HitAbstract>>(y => y.First().Value == page)), Times.Once());

            //fsLogManagerMock.Verify(x => x.Debug(string.Format(BatchingCachingStrategyAbstract.HIT_ADDED_IN_QUEUE, JsonConvert.SerializeObject(page.ToApiKeys())), 
            //    BatchingCachingStrategyAbstract.ADD_HIT), Times.Once());
        }

        [TestMethod()]
        public void NotConsentTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void SendBatchTest()
        {
            HttpResponseMessage httpResponse = new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent(null, Encoding.UTF8, "application/json")
            };


            Mock<HttpMessageHandler> mockHandler = new Mock<HttpMessageHandler>();

            mockHandler.Protected().Setup<Task<HttpResponseMessage>>(
                 "SendAsync",
                  ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Post),
                  ItExpr.IsAny<CancellationToken>()
                ).ReturnsAsync(httpResponse);

            var httpClient = new HttpClient(mockHandler.Object);
        }

        [TestMethod()]
        public void ActivateFlagTest()
        {
            Assert.Fail();
        }
    }
}
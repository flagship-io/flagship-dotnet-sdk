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
using Flagship.Enums;
using System.Net.Http.Headers;
using Microsoft.QualityTools.Testing.Fakes;

namespace Flagship.Api.Tests
{
    [TestClass()]
    public class BatchingContinuousCachingStrategyTests
    {
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

            var strategyMock = new Mock<BatchingContinuousCachingStrategy>(new object[] { config, httpClientMock.Object, hitsPoolQueue, activatePoolQueue })
            {
                CallBase = true,
            };

            var strategy = strategyMock.Object;

            var visitorId = "visitorId";

            var page = new Page("http://localhost")
            {
                VisitorId = visitorId
            };

            await strategy.Add(page).ConfigureAwait(false);

            Assert.AreEqual(1, hitsPoolQueue.Count);
            Assert.AreSame(page, hitsPoolQueue.Values.First());

            strategyMock.Verify(x => x.CacheHitAsync(It.Is<Dictionary<string, HitAbstract>>(y => y.ContainsValue(page))), Times.Once());

            var hitEvent = new Event(EventCategory.USER_ENGAGEMENT, Constants.FS_CONSENT)
            {
                Label = $"{Constants.SDK_LANGUAGE}:{true}",
                VisitorId = visitorId,
                DS = Constants.SDK_APP,
                Config = config,
                AnonymousId = null
            };

            await strategy.Add(hitEvent).ConfigureAwait(false);
            Assert.AreEqual(2, hitsPoolQueue.Count);

            strategyMock.Verify(x => x.CacheHitAsync(It.IsAny<Dictionary<string, HitAbstract>>()), Times.Exactly(2));
            strategyMock.Verify(x => x.CacheHitAsync(It.Is<Dictionary<string, HitAbstract>>(y => y.ContainsValue(hitEvent))), Times.Once());
            strategyMock.Verify(x => x.CacheHitAsync(It.Is<Dictionary<string, HitAbstract>>(y => y.ContainsValue(page))), Times.Once());
            strategyMock.Verify(x => x.FlushHitsAsync(It.IsAny<string[]>()), Times.Never());
            //fsLogManagerMock.Verify(x => x.Debug(string.Format(BatchingCachingStrategyAbstract.HIT_ADDED_IN_QUEUE, JsonConvert.SerializeObject(page.ToApiKeys())), 
            //    BatchingCachingStrategyAbstract.ADD_HIT), Times.Once());
        }

        [TestMethod()]
        public async Task NotConsentTest()
        {
            var httpClientMock = new Mock<HttpClient>();
            var fsLogManagerMock = new Mock<IFsLogManager>();
            var config = new DecisionApiConfig()
            {
                EnvId = "envID",
                LogManager = fsLogManagerMock.Object,
            };

            var hitsPoolQueue = new Dictionary<string, HitAbstract>();
            var activatePoolQueue = new Dictionary<string, Activate>();

            var strategyMock = new Mock<BatchingContinuousCachingStrategy>(new object[] { config, httpClientMock.Object, hitsPoolQueue, activatePoolQueue })
            {
                CallBase = true,
            };

            var strategy = strategyMock.Object;
            var visitorId = "visitorId";

            var page = new Page("http://localhost")
            {
                VisitorId = visitorId
            };

            await strategy.Add(page).ConfigureAwait(false);

            Assert.AreEqual(1, hitsPoolQueue.Count);

            var screen = new Screen("home")
            {
                VisitorId = "visitorId_2"
            };

            await strategy.Add(screen).ConfigureAwait(false);

            Assert.AreEqual(2, hitsPoolQueue.Count);

            var hitEvent = new Event(EventCategory.USER_ENGAGEMENT, Constants.FS_CONSENT)
            {
                Label = $"{Constants.SDK_LANGUAGE}:{false}",
                VisitorId = visitorId,
                DS = Constants.SDK_APP,
                Config = config,
                AnonymousId = null
            };

            await strategy.Add(hitEvent).ConfigureAwait(false);
            Assert.AreEqual(2, hitsPoolQueue.Count);

            strategyMock.Verify(x => x.CacheHitAsync(It.Is<Dictionary<string, HitAbstract>>(y => y.ContainsValue(hitEvent))), Times.Once());
            strategyMock.Verify(x => x.FlushHitsAsync(It.Is<string[]>(y => y.Any(z => z.Contains(visitorId)))), Times.Once());
        }

        [TestMethod()]
        public async Task SendBatchTest()
        {
            var fsLogManagerMock = new Mock<IFsLogManager>();
            var config = new DecisionApiConfig()
            {
                EnvId = "envID",
                LogManager = fsLogManagerMock.Object,
                TrackingMangerConfig = new TrackingManagerConfig()
            };

            HttpResponseMessage httpResponse = new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent("", Encoding.UTF8, "application/json")
            };


            Mock<HttpMessageHandler> mockHandler = new Mock<HttpMessageHandler>();

            var shimeContext = ShimsContext.Create();
            System.Fakes.ShimDateTime.NowGet = () => { return new DateTime(2022, 1, 1); };


            var batch = new Batch
            {
                Config = config
            };

            Func<HttpRequestMessage, bool> actionBatch1 = (HttpRequestMessage x) =>
            {

                var postDataString = JsonConvert.SerializeObject(batch.ToApiKeys());
                var headers = new HttpRequestMessage().Headers;
                headers.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.HEADER_APPLICATION_JSON));

                var result = x.Content.ReadAsStringAsync().Result;
                return result == postDataString && headers.ToString() == x.Headers.ToString() && x.Method == HttpMethod.Post;
            };

            mockHandler.Protected().Setup<Task<HttpResponseMessage>>(
                 "SendAsync",
                  ItExpr.Is<HttpRequestMessage>(req => actionBatch1(req)),
                  ItExpr.IsAny<CancellationToken>()
                ).ReturnsAsync(httpResponse).Verifiable();

            var httpClient = new HttpClient(mockHandler.Object);

            var hitsPoolQueue = new Dictionary<string, HitAbstract>();
            var activatePoolQueue = new Dictionary<string, Activate>();

            var strategyMock = new Mock<BatchingContinuousCachingStrategy>(new object[] { config, httpClient, hitsPoolQueue, activatePoolQueue })
            {
                CallBase = true,
            };

            var strategy = strategyMock.Object;

            var visitorId = "visitorId";


            for (int i = 0; i < 20; i++)
            {
                var screen = new Screen("home")
                {
                    VisitorId = visitorId,
                    Key = $"{visitorId}:{Guid.NewGuid()}"
                };

                hitsPoolQueue[screen.Key] = screen;
                batch.Hits.Add(screen);
            }

            var page = new Page("http://localhost")
            {
                VisitorId = visitorId,
                Key = $"{visitorId}:{Guid.NewGuid()}"
            };

            hitsPoolQueue[page.Key] = page;

            Assert.AreEqual(21, hitsPoolQueue.Count);

            await strategy.SendBatch().ConfigureAwait(false);

            Assert.AreEqual(1, hitsPoolQueue.Count);

            strategyMock.Verify(x => x.CacheHitAsync(It.IsAny<Dictionary<string, HitAbstract>>()), Times.Never());
            strategyMock.Verify(x => x.FlushHitsAsync(It.Is<string[]>(y => y.Any(z => z.Contains(visitorId)) && y.Length == 20)), Times.Once());


            batch = new Batch()
            {
                Config = config,
            };

            batch.Hits.Add(page);

            Func<HttpRequestMessage, bool> actionBatch2 = (HttpRequestMessage x) =>
            {

                var postDataString = JsonConvert.SerializeObject(batch.ToApiKeys());
                var headers = new HttpRequestMessage().Headers;
                headers.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.HEADER_APPLICATION_JSON));

                var result = x.Content.ReadAsStringAsync().Result;
                return result == postDataString && headers.ToString() == x.Headers.ToString() && x.Method == HttpMethod.Post;
            };

            mockHandler.Protected().Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => actionBatch2(req)),
                ItExpr.IsAny<CancellationToken>()
                ).ReturnsAsync(httpResponse).Verifiable();

            await strategy.SendBatch().ConfigureAwait(false);

            Assert.AreEqual(0, hitsPoolQueue.Count);

            strategyMock.Verify(x => x.CacheHitAsync(It.IsAny<Dictionary<string, HitAbstract>>()), Times.Never());
            strategyMock.Verify(x => x.FlushHitsAsync(It.Is<string[]>(y => y.Any(z => z.Contains(visitorId)) && y.Length == 1)), Times.Once());


            await strategy.SendBatch().ConfigureAwait(false);

            httpResponse.Dispose();
            shimeContext.Dispose();
        }

        [TestMethod()]
        public async Task SendBatchFailedTest()
        {
            var fsLogManagerMock = new Mock<IFsLogManager>();
            var config = new DecisionApiConfig()
            {
                EnvId = "envID",
                LogManager = fsLogManagerMock.Object,
                TrackingMangerConfig = new TrackingManagerConfig()
            };

            HttpResponseMessage httpResponse = new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.BadRequest,
                Content = new StringContent("", Encoding.UTF8, "application/json")
            };


            Mock<HttpMessageHandler> mockHandler = new Mock<HttpMessageHandler>();

            var shimeContext = ShimsContext.Create();
            System.Fakes.ShimDateTime.NowGet = () => { return new DateTime(2022, 1, 1); };


            var batch = new Batch
            {
                Config = config
            };

            mockHandler.Protected().Setup<Task<HttpResponseMessage>>(
                 "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>()
                ).ReturnsAsync(httpResponse).Verifiable();

            var httpClient = new HttpClient(mockHandler.Object);

            var hitsPoolQueue = new Dictionary<string, HitAbstract>();
            var activatePoolQueue = new Dictionary<string, Activate>();

            var strategyMock = new Mock<BatchingContinuousCachingStrategy>(new object[] { config, httpClient, hitsPoolQueue, activatePoolQueue })
            {
                CallBase = true,
            };

            var strategy = strategyMock.Object;

            var visitorId = "visitorId";


            for (int i = 0; i < 20; i++)
            {
                var screen = new Screen("home")
                {
                    VisitorId = visitorId,
                    Key = $"{visitorId}:{Guid.NewGuid()}"
                };

                hitsPoolQueue[screen.Key] = screen;
                batch.Hits.Add(screen);
            }

            var page = new Page("http://localhost")
            {
                VisitorId = visitorId,
                Key = $"{visitorId}:{Guid.NewGuid()}"
            };

            hitsPoolQueue[page.Key] = page;

            Assert.AreEqual(21, hitsPoolQueue.Count);

            await strategy.SendBatch().ConfigureAwait(false);

            Assert.AreEqual(21, hitsPoolQueue.Count);

            strategyMock.Verify(x => x.CacheHitAsync(It.IsAny<Dictionary<string, HitAbstract>>()), Times.Never());
            strategyMock.Verify(x => x.FlushHitsAsync(It.IsAny<string[]>()), Times.Never());

            httpResponse.Dispose();
            shimeContext.Dispose();
        }

        [TestMethod()]
        public void ActivateFlagTest()
        {
            Assert.Fail();
        }
    }
}
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
using Flagship.Model;

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
                LogManager = fsLogManagerMock.Object,
                TrackingMangerConfig = new TrackingManagerConfig()
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
                TrackingMangerConfig = new TrackingManagerConfig()
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

            var page2 = new Page("http://localhost2")
            {
                VisitorId = visitorId
            };

            await strategy.Add(page).ConfigureAwait(false);
            await strategy.Add(page2).ConfigureAwait(false);

            Assert.AreEqual(2, hitsPoolQueue.Count);
            Assert.AreEqual(0, activatePoolQueue.Count);

            var screen = new Screen("home")
            {
                VisitorId = "visitorId_2"
            };

            await strategy.Add(screen).ConfigureAwait(false);

            Assert.AreEqual(3, hitsPoolQueue.Count);
            Assert.AreEqual(0, activatePoolQueue.Count);

            var activate = new Activate("varGroupId", "varId")
            {
                VisitorId = visitorId,
                Key = visitorId + "key-activate"
            };

            var activateXp = new Activate("varGroupId", "varId")
            {
                VisitorId = Guid.NewGuid().ToString(),
                AnonymousId = visitorId,
                Key = visitorId + "key-activate-xp",
            };

            activatePoolQueue[activate.Key] = activate;
            activatePoolQueue[activateXp.Key] = activateXp;

            Assert.AreEqual(3, hitsPoolQueue.Count);
            Assert.AreEqual(2, activatePoolQueue.Count);

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
            Assert.AreEqual(0, activatePoolQueue.Count);
            strategyMock.Verify(x => x.CacheHitAsync(It.Is<Dictionary<string, HitAbstract>>(y => y.ContainsValue(hitEvent))), Times.Once());
            strategyMock.Verify(x => x.FlushHitsAsync(It.Is<string[]>(y => y.Any(z => z.Contains(visitorId)) && y.Length==4)), Times.Once());
        }

        [TestMethod()]
        public async Task NotConsentEmptyTest() 
        {
            var httpClientMock = new Mock<HttpClient>();
            var fsLogManagerMock = new Mock<IFsLogManager>();
            var config = new DecisionApiConfig()
            {
                EnvId = "envID",
                LogManager = fsLogManagerMock.Object,
                TrackingMangerConfig = new TrackingManagerConfig()
            };

            var hitsPoolQueue = new Dictionary<string, HitAbstract>();
            var activatePoolQueue = new Dictionary<string, Activate>();

            var strategyMock = new Mock<BatchingContinuousCachingStrategy>(new object[] { config, httpClientMock.Object, hitsPoolQueue, activatePoolQueue })
            {
                CallBase = true,
            };

            var strategy = strategyMock.Object;
            var visitorId = "visitorId";

            Assert.AreEqual(0, hitsPoolQueue.Count);

            var hitEvent = new Event(EventCategory.USER_ENGAGEMENT, Constants.FS_CONSENT)
            {
                Label = $"{Constants.SDK_LANGUAGE}:{false}",
                VisitorId = visitorId,
                DS = Constants.SDK_APP,
                Config = config,
                AnonymousId = null
            };

            await strategy.Add(hitEvent).ConfigureAwait(false);
            Assert.AreEqual(1, hitsPoolQueue.Count);

            strategyMock.Verify(x => x.CacheHitAsync(It.Is<Dictionary<string, HitAbstract>>(y => y.ContainsValue(hitEvent))), Times.Once());
            strategyMock.Verify(x => x.FlushHitsAsync(It.IsAny<string[]>()), Times.Never());
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
                return result == postDataString && headers.ToString() == x.Headers.ToString() && x.Method == HttpMethod.Post
                && x.RequestUri.ToString() == Constants.HIT_EVENT_URL;
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

            Assert.AreEqual(20, hitsPoolQueue.Count);

            await strategy.SendBatch().ConfigureAwait(false);

            Assert.AreEqual(0, hitsPoolQueue.Count);

            strategyMock.Verify(x => x.CacheHitAsync(It.IsAny<Dictionary<string, HitAbstract>>()), Times.Never());
            strategyMock.Verify(x => x.FlushHitsAsync(It.Is<string[]>(y => y.Any(z => z.Contains(visitorId)) && y.Length == 20)), Times.Once());

            httpResponse.Dispose();
            shimeContext.Dispose();
        }

        [TestMethod()]
        public async Task SendBatchMaxSizeTest() 
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
                return result == postDataString && headers.ToString() == x.Headers.ToString() && x.Method == HttpMethod.Post
                && x.RequestUri.ToString() == Constants.HIT_EVENT_URL;
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


            for (int i = 0; i < 125; i++)
            {
                var screen = new Screen(string.Join("", Enumerable.Repeat("home", 5000)))
                {
                    VisitorId = visitorId,
                    Key = $"{visitorId}:{Guid.NewGuid()}"
                };

                hitsPoolQueue[screen.Key] = screen;
                if (i>123)
                {
                    continue;
                }
                batch.Hits.Add(screen);
            }

            Assert.AreEqual(125, hitsPoolQueue.Count);

            await strategy.SendBatch().ConfigureAwait(false);

            Assert.AreEqual(1, hitsPoolQueue.Count);

            strategyMock.Verify(x => x.CacheHitAsync(It.IsAny<Dictionary<string, HitAbstract>>()), Times.Never());
            strategyMock.Verify(x => x.FlushHitsAsync(It.Is<string[]>(y => y.Any(z => z.Contains(visitorId)) && y.Length == 124)), Times.Once());

            httpResponse.Dispose();
            shimeContext.Dispose();
        }

        [TestMethod()]
        public async Task SendBatchHitExpiredTest() 
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
                return result == postDataString && headers.ToString() == x.Headers.ToString() && x.Method == HttpMethod.Post
                && x.RequestUri.ToString() == Constants.HIT_EVENT_URL;
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

            var screen = new Screen("home")
            {
                VisitorId = visitorId,
                Key = $"{visitorId}:{Guid.NewGuid()}",
                CreatedAt = new DateTime(2022, 1, 1),
            };

            batch.Hits.Add(screen);

            var screen2 = new Screen("home2")
            {
                VisitorId = visitorId,
                Key = $"{visitorId}:{Guid.NewGuid()}",
                CreatedAt = new DateTime(2021, 1, 1),
            };

            hitsPoolQueue[screen.Key] = screen;
            hitsPoolQueue[screen2.Key] = screen2;

            Assert.AreEqual(2, hitsPoolQueue.Count);

            await strategy.SendBatch().ConfigureAwait(false);

            Assert.AreEqual(0, hitsPoolQueue.Count);

            strategyMock.Verify(x => x.CacheHitAsync(It.IsAny<Dictionary<string, HitAbstract>>()), Times.Never());
            strategyMock.Verify(x => x.FlushHitsAsync(It.Is<string[]>(y => y.Any(z => z.Contains(visitorId)) && y.Length == 2)), Times.Once());

            httpResponse.Dispose();
            shimeContext.Dispose();
        }

        [TestMethod()]
        public async Task SendBatchWithPoolMaxSizeTest()
        { 
            var fsLogManagerMock = new Mock<IFsLogManager>();
            var config = new DecisionApiConfig()
            {
                EnvId = "envID",
                LogManager = fsLogManagerMock.Object,
                TrackingMangerConfig = new TrackingManagerConfig(CacheStrategy.CONTINUOUS_CACHING, 5)
            };

            HttpResponseMessage httpResponse = new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent("", Encoding.UTF8, "application/json")
            };


            Mock<HttpMessageHandler> mockHandler = new Mock<HttpMessageHandler>();

            var shimeContext = ShimsContext.Create();
            System.Fakes.ShimDateTime.NowGet = () => { return new DateTime(2022, 1, 1); };


            var httpClient = new HttpClient(mockHandler.Object);

            var hitsPoolQueue = new Dictionary<string, HitAbstract>();
            var activatePoolQueue = new Dictionary<string, Activate>();

            var strategyMock = new Mock<BatchingContinuousCachingStrategy>(new object[] { config, httpClient, hitsPoolQueue, activatePoolQueue })
            {
                CallBase = true,
            };

            strategyMock.Setup(x => x.SendBatch(CacheTriggeredBy.BatchLength)).Callback(() =>
            {
                hitsPoolQueue.Clear();
            }).Returns(Task.CompletedTask);

            var strategy = strategyMock.Object;

            var visitorId = "visitorId";


            for (int i = 0; i < 20; i++)
            {
                var screen = new Screen("home")
                {
                    VisitorId = visitorId
                };
                await strategy.Add(screen).ConfigureAwait(false);
            }

            strategyMock.Verify(x => x.SendBatch(CacheTriggeredBy.BatchLength), Times.Exactly(4));

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
        public async Task ActivateFlagTest()
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

            var visitorId = "visitorId";

            var activate = new Activate("variationGroupId", "variationId")
            {
                VisitorId = visitorId,
                Config = config
            };

            var activateList = new List<Activate>()
            {
                activate,
            };

            var batch = new ActivateBatch(activateList, config);

            Func<HttpRequestMessage, bool> actionBatch1 = (HttpRequestMessage x) =>
            {
                var postDataString = JsonConvert.SerializeObject(batch.ToApiKeys());

                var headers = new HttpRequestMessage().Headers;
                headers.Add(Constants.HEADER_X_API_KEY, config.ApiKey);
                headers.Add(Constants.HEADER_X_SDK_CLIENT, Constants.SDK_LANGUAGE);
                headers.Add(Constants.HEADER_X_SDK_VERSION, Constants.SDK_VERSION);
                headers.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.HEADER_APPLICATION_JSON));

                var url = Constants.BASE_API_URL + BatchingCachingStrategyAbstract.URL_ACTIVATE;

                var f = headers.ToString();

                var result = x.Content.ReadAsStringAsync().Result;
                return result == postDataString && headers.ToString() == x.Headers.ToString() && x.Method == HttpMethod.Post
                && x.RequestUri.ToString() == url;
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

            await strategy.ActivateFlag(activate).ConfigureAwait(false);

            Assert.AreEqual(0, hitsPoolQueue.Count);
            Assert.AreEqual(0, activatePoolQueue.Count);

            strategyMock.Verify(x => x.CacheHitAsync(It.IsAny<Dictionary<string, HitAbstract>>()), Times.Never());
            strategyMock.Verify(x => x.FlushHitsAsync(It.IsAny<string[]>()), Times.Never());

            httpResponse.Dispose();
        }

        [TestMethod()]
        public async Task ActivateMultipleFlagTest() 
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

            var visitorId = "visitorId";

            var activate = new Activate("variationGroupId", "variationId")
            {
                VisitorId = visitorId,
                Config = config
            };

            var activate2 = new Activate("variationGroupId-2", "variationId-2")
            {
                VisitorId = visitorId,
                Config = config,
                Key = $"{visitorId}:{Guid.NewGuid()}"
            };

            var activate3 = new Activate("variationGroupId-3", "variationId-3")
            {
                VisitorId = visitorId,
                Config = config,
                Key = $"{visitorId}:{Guid.NewGuid()}"
            };

            var activateList = new List<Activate>()
            {
                activate2,
                activate3,
                activate,
            };

            var batch = new ActivateBatch(activateList, config);

            Func<HttpRequestMessage, bool> actionBatch1 = (HttpRequestMessage x) =>
            {
                var postDataString = JsonConvert.SerializeObject(batch.ToApiKeys());

                var headers = new HttpRequestMessage().Headers;
                headers.Add(Constants.HEADER_X_API_KEY, config.ApiKey);
                headers.Add(Constants.HEADER_X_SDK_CLIENT, Constants.SDK_LANGUAGE);
                headers.Add(Constants.HEADER_X_SDK_VERSION, Constants.SDK_VERSION);
                headers.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.HEADER_APPLICATION_JSON));

                var url = Constants.BASE_API_URL + BatchingCachingStrategyAbstract.URL_ACTIVATE;

                var f = headers.ToString();

                var result = x.Content.ReadAsStringAsync().Result;
                return result == postDataString && headers.ToString() == x.Headers.ToString() && x.Method == HttpMethod.Post
                && x.RequestUri.ToString() == url;
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

            activatePoolQueue[activate2.Key] = activate2;
            activatePoolQueue[activate3.Key] = activate3;

            Assert.AreEqual(2, activatePoolQueue.Count);

            await strategy.ActivateFlag(activate).ConfigureAwait(false);

            Assert.AreEqual(0, hitsPoolQueue.Count);
            Assert.AreEqual(0, activatePoolQueue.Count);

            strategyMock.Verify(x => x.CacheHitAsync(It.IsAny<Dictionary<string, HitAbstract>>()), Times.Never());
            strategyMock.Verify(x => x.FlushHitsAsync(It.Is<string[]>(y=>y.Contains(activate2.Key)
            && y.Contains(activate3.Key) 
            && y.Length==2)), Times.Once());

            httpResponse.Dispose();
        }

        [TestMethod()]
        public async Task ActivateMultipleFlagOnBatchTest()
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

            var visitorId = "visitorId";

            var activate = new Activate("variationGroupId", "variationId")
            {
                VisitorId = visitorId,
                Config = config,
                Key = $"{visitorId}:{Guid.NewGuid()}"
            };

            var activate2 = new Activate("variationGroupId-2", "variationId-2")
            {
                VisitorId = visitorId,
                Config = config,
                Key = $"{visitorId}:{Guid.NewGuid()}"
            };

            var activate3 = new Activate("variationGroupId-3", "variationId-3")
            {
                VisitorId = visitorId,
                Config = config,
                Key = $"{visitorId}:{Guid.NewGuid()}"
            };

            var activateList = new List<Activate>()
            {
                activate,
                activate2,
                activate3
            };

            var batch = new ActivateBatch(activateList, config);

            Func<HttpRequestMessage, bool> actionBatch1 = (HttpRequestMessage x) =>
            {
                var postDataString = JsonConvert.SerializeObject(batch.ToApiKeys());

                var headers = new HttpRequestMessage().Headers;
                headers.Add(Constants.HEADER_X_API_KEY, config.ApiKey);
                headers.Add(Constants.HEADER_X_SDK_CLIENT, Constants.SDK_LANGUAGE);
                headers.Add(Constants.HEADER_X_SDK_VERSION, Constants.SDK_VERSION);
                headers.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.HEADER_APPLICATION_JSON));

                var url = Constants.BASE_API_URL + BatchingCachingStrategyAbstract.URL_ACTIVATE;

                var result = x.Content.ReadAsStringAsync().Result;
                return result == postDataString && headers.ToString() == x.Headers.ToString() && x.Method == HttpMethod.Post
                && x.RequestUri.ToString() == url;
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

            activatePoolQueue[activate.Key] = activate;
            activatePoolQueue[activate2.Key] = activate2;
            activatePoolQueue[activate3.Key] = activate3;

            Assert.AreEqual(3, activatePoolQueue.Count);

            await strategy.SendBatch().ConfigureAwait(false);

            Assert.AreEqual(0, hitsPoolQueue.Count);
            Assert.AreEqual(0, activatePoolQueue.Count);

            strategyMock.Verify(x => x.CacheHitAsync(It.IsAny<Dictionary<string, HitAbstract>>()), Times.Never());
            strategyMock.Verify(x => x.FlushHitsAsync(It.Is<string[]>(y => y.Contains(activate2.Key)
            && y.Contains(activate3.Key) && y.Contains(activate.Key)
            && y.Length == 3)), Times.Once());

            httpResponse.Dispose();
        }

        [TestMethod()]
        public async Task ActivateFlagFailedTest() 
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

            var visitorId = "visitorId";

            var activate = new Activate("variationGroupId", "variationId")
            {
                VisitorId = visitorId,
                Config = config
            };

            var activateList = new List<Activate>()
            {
                activate,
            };

            var batch = new ActivateBatch(activateList, config);

            Func<HttpRequestMessage, bool> actionBatch1 = (HttpRequestMessage x) =>
            {
                var postDataString = JsonConvert.SerializeObject(batch.ToApiKeys());

                var headers = new HttpRequestMessage().Headers;
                headers.Add(Constants.HEADER_X_API_KEY, config.ApiKey);
                headers.Add(Constants.HEADER_X_SDK_CLIENT, Constants.SDK_LANGUAGE);
                headers.Add(Constants.HEADER_X_SDK_VERSION, Constants.SDK_VERSION);
                headers.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.HEADER_APPLICATION_JSON));

                var url = Constants.BASE_API_URL + BatchingCachingStrategyAbstract.URL_ACTIVATE;

                var f = headers.ToString();

                var result = x.Content.ReadAsStringAsync().Result;
                return result == postDataString && headers.ToString() == x.Headers.ToString() && x.Method == HttpMethod.Post
                && x.RequestUri.ToString() == url;
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

            await strategy.ActivateFlag(activate).ConfigureAwait(false);

            Assert.AreEqual(0, hitsPoolQueue.Count);
            Assert.AreEqual(1, activatePoolQueue.Count);

            strategyMock.Verify(x => x.CacheHitAsync(It.Is<Dictionary<string, HitAbstract>>(y=> y.ContainsValue(activate))), Times.Once());
            strategyMock.Verify(x => x.FlushHitsAsync(It.IsAny<string[]>()), Times.Never());

            httpResponse.Dispose();
        }

        [TestMethod]
        public async Task CacheHitAsync() 
        {
            var httpClientMock = new Mock<HttpClient>();
            var fsLogManagerMock = new Mock<IFsLogManager>();
            var hitCacheImplementation = new Mock<Cache.IHitCacheImplementation>();

            var config = new DecisionApiConfig()
            {
                EnvId = "envID",
                LogManager = fsLogManagerMock.Object,
                HitCacheImplementation = hitCacheImplementation.Object
            };

            var hitsPoolQueue = new Dictionary<string, HitAbstract>();
            var activatePoolQueue = new Dictionary<string, Activate>();

            var strategyMock = new Mock<BatchingContinuousCachingStrategy>(new object[] { config, httpClientMock.Object, hitsPoolQueue, activatePoolQueue })
            {
                CallBase = true,
            };

            var strategy = strategyMock.Object;

            var visitorId = "visitorId";

            var shimeContext = ShimsContext.Create();
            System.Fakes.ShimDateTime.NowGet = () => { return new DateTime(2022, 1, 1); };

            var hits = new Dictionary<string, HitAbstract>() {
                {$"{visitorId}:{Guid.NewGuid()}", new Page("http://localhost") }
            };

            hitCacheImplementation.Setup(x => x.CacheHit(It.IsAny<JObject>())).Returns(Task.CompletedTask);

            var data = new JObject();
            foreach (var keyValue in hits)
            {
                var hitData = new HitCacheDTOV1
                {
                    Version = 1,
                    Data = new HitCacheData
                    {
                        AnonymousId = keyValue.Value.AnonymousId,
                        VisitorId = keyValue.Value.VisitorId,
                        Type = keyValue.Value.Type,
                        Content = keyValue.Value,
                        Time = DateTime.Now
                    }
                };

                data[keyValue.Key] = JObject.FromObject(hitData);
            }

            await strategy.CacheHitAsync(hits).ConfigureAwait(false);

            hitCacheImplementation.Verify(x=>x.CacheHit(It.Is<JObject>(y=>y.ToString()== data.ToString())), Times.Once());

            config.DisableCache = true;

            await strategy.CacheHitAsync(hits).ConfigureAwait(false);

            hitCacheImplementation.Verify(x => x.CacheHit(It.Is<JObject>(y => y.ToString() == data.ToString())), Times.Once());

            config.DisableCache = false;
            config.HitCacheImplementation = null;

            await strategy.CacheHitAsync(hits).ConfigureAwait(false);

            hitCacheImplementation.Verify(x => x.CacheHit(It.Is<JObject>(y => y.ToString() == data.ToString())), Times.Once());

            shimeContext.Dispose();
        }

        [TestMethod]
        public async Task CacheHitActivateAsync()
        { 
            var httpClientMock = new Mock<HttpClient>();
            var fsLogManagerMock = new Mock<IFsLogManager>();
            var hitCacheImplementation = new Mock<Cache.IHitCacheImplementation>();

            var config = new DecisionApiConfig()
            {
                EnvId = "envID",
                LogManager = fsLogManagerMock.Object,
                HitCacheImplementation = hitCacheImplementation.Object
            };

            var hitsPoolQueue = new Dictionary<string, HitAbstract>();
            var activatePoolQueue = new Dictionary<string, Activate>();

            var strategyMock = new Mock<BatchingContinuousCachingStrategy>(new object[] { config, httpClientMock.Object, hitsPoolQueue, activatePoolQueue })
            {
                CallBase = true,
            };

            var strategy = strategyMock.Object;

            var visitorId = "visitorId";

            var shimeContext = ShimsContext.Create();
            System.Fakes.ShimDateTime.NowGet = () => { return new DateTime(2022, 1, 1); };

            var hits = new Dictionary<string, Activate>() {
                {$"{visitorId}:{Guid.NewGuid()}", new Activate("variationGroupId", "variationId") }
            };

            hitCacheImplementation.Setup(x => x.CacheHit(It.IsAny<JObject>())).Returns(Task.CompletedTask);

            var data = new JObject();
            foreach (var keyValue in hits)
            {
                var hitData = new HitCacheDTOV1
                {
                    Version = 1,
                    Data = new HitCacheData
                    {
                        AnonymousId = keyValue.Value.AnonymousId,
                        VisitorId = keyValue.Value.VisitorId,
                        Type = keyValue.Value.Type,
                        Content = keyValue.Value,
                        Time = DateTime.Now
                    }
                };

                data[keyValue.Key] = JObject.FromObject(hitData);
            }

            await strategy.CacheHitAsync(hits).ConfigureAwait(false);

            hitCacheImplementation.Verify(x => x.CacheHit(It.Is<JObject>(y => y.ToString() == data.ToString())), Times.Once());

            shimeContext.Dispose();
        }

        [TestMethod]
        public async Task CacheHitFailedAsync()
        {
            var httpClientMock = new Mock<HttpClient>();
            var fsLogManagerMock = new Mock<IFsLogManager>();
            var hitCacheImplementation = new Mock<Cache.IHitCacheImplementation>();

            var config = new DecisionApiConfig()
            {
                EnvId = "envID",
                LogManager = fsLogManagerMock.Object,
                HitCacheImplementation = hitCacheImplementation.Object
            };

            var hitsPoolQueue = new Dictionary<string, HitAbstract>();
            var activatePoolQueue = new Dictionary<string, Activate>();

            var strategy = new BatchingContinuousCachingStrategy(config, httpClientMock.Object, ref hitsPoolQueue, ref activatePoolQueue);

            var visitorId = "visitorId";

            var shimeContext = ShimsContext.Create();
            System.Fakes.ShimDateTime.NowGet = () => { return new DateTime(2022, 1, 1); };

            var hits = new Dictionary<string, HitAbstract>() {
                {$"{visitorId}:{Guid.NewGuid()}", new Page("http://localhost") }
            };

            var exception = new Exception("Error");

            hitCacheImplementation.Setup(x => x.CacheHit(It.IsAny<JObject>())).ThrowsAsync(exception);

            var data = new JObject();
            foreach (var keyValue in hits)
            {
                var hitData = new HitCacheDTOV1
                {
                    Version = 1,
                    Data = new HitCacheData
                    {
                        AnonymousId = keyValue.Value.AnonymousId,
                        VisitorId = keyValue.Value.VisitorId,
                        Type = keyValue.Value.Type,
                        Content = keyValue.Value,
                        Time = DateTime.Now
                    }
                };

                data[keyValue.Key] = JObject.FromObject(hitData);
            }

            await strategy.CacheHitAsync(hits).ConfigureAwait(false);

            hitCacheImplementation.Verify(x => x.CacheHit(It.Is<JObject>(y => y.ToString() == data.ToString())), Times.Once());

            fsLogManagerMock.Verify(x => x.Error(exception.Message, BatchingCachingStrategyAbstract.PROCESS_CACHE_HIT), Times.Once());

            shimeContext.Dispose();
        }

        [TestMethod]
        public async Task FlushHitsAsync()
        {
            var httpClientMock = new Mock<HttpClient>();
            var fsLogManagerMock = new Mock<IFsLogManager>();
            var hitCacheImplementation = new Mock<Cache.IHitCacheImplementation>();

            var config = new DecisionApiConfig()
            {
                EnvId = "envID",
                LogManager = fsLogManagerMock.Object,
                HitCacheImplementation = hitCacheImplementation.Object
            };

            var visitorId = "visitorId";

            var hitKeys = new string[] { $"{visitorId}:{Guid.NewGuid()}", $"{visitorId}:{Guid.NewGuid()}", $"{visitorId}:{Guid.NewGuid()}" };

            hitCacheImplementation.Setup(x => x.FlushHits(hitKeys)).Returns(Task.CompletedTask);

            var hitsPoolQueue = new Dictionary<string, HitAbstract>();
            var activatePoolQueue = new Dictionary<string, Activate>();

            var strategy = new BatchingContinuousCachingStrategy(config, httpClientMock.Object, ref hitsPoolQueue, ref activatePoolQueue);
            
           
            await strategy.FlushHitsAsync(hitKeys).ConfigureAwait(false);

            hitCacheImplementation.Verify(x => x.FlushHits(hitKeys), Times.Once());

            config.DisableCache = true;

            await strategy.FlushHitsAsync(hitKeys).ConfigureAwait(false);

            hitCacheImplementation.Verify(x => x.FlushHits(hitKeys), Times.Once());

            config.DisableCache = false;
            config.HitCacheImplementation = null;

            await strategy.FlushHitsAsync(hitKeys).ConfigureAwait(false);

            hitCacheImplementation.Verify(x => x.FlushHits(hitKeys), Times.Once());
        }

        [TestMethod]
        public async Task FlushHitsFailedAsync()
        {
            var httpClientMock = new Mock<HttpClient>();
            var fsLogManagerMock = new Mock<IFsLogManager>();
            var hitCacheImplementation = new Mock<Cache.IHitCacheImplementation>();

            var config = new DecisionApiConfig()
            {
                EnvId = "envID",
                LogManager = fsLogManagerMock.Object, 
                HitCacheImplementation = hitCacheImplementation.Object
            };

            var visitorId = "visitorId";

            var hitKeys = new string[] { $"{visitorId}:{Guid.NewGuid()}", $"{visitorId}:{Guid.NewGuid()}", $"{visitorId}:{Guid.NewGuid()}" };

            var exception = new Exception("error");

            hitCacheImplementation.Setup(x => x.FlushHits(hitKeys)).ThrowsAsync(exception);

            var hitsPoolQueue = new Dictionary<string, HitAbstract>();
            var activatePoolQueue = new Dictionary<string, Activate>();

            var strategy = new BatchingContinuousCachingStrategy(config, httpClientMock.Object, ref hitsPoolQueue, ref activatePoolQueue);


            await strategy.FlushHitsAsync(hitKeys).ConfigureAwait(false);

            hitCacheImplementation.Verify(x => x.FlushHits(hitKeys), Times.Once());

            fsLogManagerMock.Verify(x => x.Error(exception.Message, BatchingCachingStrategyAbstract.PROCESS_FLUSH_HIT), Times.Once());


        }

        [TestMethod]
        public async Task FlushAllHitsAsync()
        { 
            var httpClientMock = new Mock<HttpClient>();
            var fsLogManagerMock = new Mock<IFsLogManager>();
            var hitCacheImplementation = new Mock<Cache.IHitCacheImplementation>();

            var config = new DecisionApiConfig()
            {
                EnvId = "envID",
                LogManager = fsLogManagerMock.Object,
                HitCacheImplementation = hitCacheImplementation.Object
            };


            hitCacheImplementation.Setup(x => x.FlushAllHits()).Returns(Task.CompletedTask);

            var hitsPoolQueue = new Dictionary<string, HitAbstract>();
            var activatePoolQueue = new Dictionary<string, Activate>();

            var strategy = new BatchingContinuousCachingStrategy(config, httpClientMock.Object, ref hitsPoolQueue, ref activatePoolQueue);

            await strategy.FlushAllHitsAsync().ConfigureAwait(false);

            hitCacheImplementation.Verify(x => x.FlushAllHits(), Times.Once());

            config.DisableCache = true;

            await strategy.FlushAllHitsAsync().ConfigureAwait(false);

            hitCacheImplementation.Verify(x => x.FlushAllHits(), Times.Once());

            config.DisableCache = false;
            config.HitCacheImplementation = null;

            await strategy.FlushAllHitsAsync().ConfigureAwait(false);

            hitCacheImplementation.Verify(x => x.FlushAllHits(), Times.Once());
        }

        [TestMethod]
        public async Task FlushAllHitsFailedAsync()
        {
            var httpClientMock = new Mock<HttpClient>();
            var fsLogManagerMock = new Mock<IFsLogManager>();
            var hitCacheImplementation = new Mock<Cache.IHitCacheImplementation>();

            var config = new DecisionApiConfig()
            {
                EnvId = "envID",
                LogManager = fsLogManagerMock.Object,
                HitCacheImplementation = hitCacheImplementation.Object
            };

           
            var exception = new Exception("error");

            hitCacheImplementation.Setup(x => x.FlushAllHits()).ThrowsAsync(exception);

            var hitsPoolQueue = new Dictionary<string, HitAbstract>();
            var activatePoolQueue = new Dictionary<string, Activate>();

            var strategy = new BatchingContinuousCachingStrategy(config, httpClientMock.Object, ref hitsPoolQueue, ref activatePoolQueue);


            await strategy.FlushAllHitsAsync().ConfigureAwait(false);

            hitCacheImplementation.Verify(x => x.FlushAllHits(), Times.Once());

            fsLogManagerMock.Verify(x => x.Error(exception.Message, BatchingCachingStrategyAbstract.PROCESS_FLUSH_HIT), Times.Once());


        }

    }
}
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
using Flagship.Model;
using Newtonsoft.Json.Serialization;
using Flagship.FsFlag;
using Flagship.FsVisitor;
using Microsoft.QualityTools.Testing.Fakes;
using System.Collections.Concurrent;

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
                TrackingManagerConfig = new TrackingManagerConfig()
            };

            var hitsPoolQueue = new ConcurrentDictionary<string, HitAbstract>();
            var activatePoolQueue = new ConcurrentDictionary<string, Activate>();

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
            Assert.IsTrue(hitsPoolQueue.Values.Any(v => v == page));

            strategyMock.Verify(x => x.CacheHitAsync(It.Is<ConcurrentDictionary<string, HitAbstract>>(y => y.Values.Any(v => v == page))), Times.Once());

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

            strategyMock.Verify(x => x.CacheHitAsync(It.IsAny<ConcurrentDictionary<string, HitAbstract>>()), Times.Exactly(2));
            strategyMock.Verify(x => x.CacheHitAsync(It.Is<ConcurrentDictionary<string, HitAbstract>>(y => y.Values.Any(v => v == hitEvent))), Times.Once());
            strategyMock.Verify(x => x.CacheHitAsync(It.Is<ConcurrentDictionary<string, HitAbstract>>(y => y.Values.Any(v => v == page))), Times.Once());
            strategyMock.Verify(x => x.FlushHitsAsync(It.IsAny<string[]>()), Times.Never());
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
                TrackingManagerConfig = new TrackingManagerConfig()
            };

            var hitsPoolQueue = new ConcurrentDictionary<string, HitAbstract>();
            var activatePoolQueue = new ConcurrentDictionary<string, Activate>();

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
            strategyMock.Verify(x => x.CacheHitAsync(It.Is<ConcurrentDictionary<string, HitAbstract>>(y => y.Values.Contains(hitEvent))), Times.Once());
            strategyMock.Verify(x => x.FlushHitsAsync(It.Is<string[]>(y => y.Any(z => z.Contains(visitorId)) && y.Length == 4)), Times.Once());
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
                TrackingManagerConfig = new TrackingManagerConfig()
            };

            var hitsPoolQueue = new ConcurrentDictionary<string, HitAbstract>();
            var activatePoolQueue = new ConcurrentDictionary<string, Activate>();

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

            strategyMock.Verify(x => x.CacheHitAsync(It.Is<ConcurrentDictionary<string, HitAbstract>>(y => y.Values.Contains(hitEvent))), Times.Once());
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
                TrackingManagerConfig = new TrackingManagerConfig()
            };

            HttpResponseMessage httpResponse = new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent("", Encoding.UTF8, "application/json")
            };


            Mock<HttpMessageHandler> mockHandler = new Mock<HttpMessageHandler>();




            var batch = new Batch
            {
                Config = config
            };

            Func<HttpRequestMessage, Task<bool>> actionBatch1 = async (HttpRequestMessage x) =>
            {

                var batchedApiKeys = JToken.FromObject(batch.ToApiKeys());
                var result = await x.Content.ReadAsStringAsync();
                var resultApiKeys = JToken.Parse(result);
                batchedApiKeys["qt"] = 0;
                resultApiKeys["qt"] = 0;
                var isEquals = JToken.DeepEquals(batchedApiKeys, resultApiKeys);
                var headers = new HttpRequestMessage().Headers;
                headers.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.HEADER_APPLICATION_JSON));

                return isEquals && headers.ToString() == x.Headers.ToString() && x.Method == HttpMethod.Post
                && x.RequestUri?.ToString() == Constants.HIT_EVENT_URL;
            };

            mockHandler.Protected().Setup<Task<HttpResponseMessage>>(
                 "SendAsync",
                  ItExpr.Is<HttpRequestMessage>(req => actionBatch1(req).Result),
                  ItExpr.IsAny<CancellationToken>()
                ).ReturnsAsync(httpResponse).Verifiable();

            var httpClient = new HttpClient(mockHandler.Object);

            var hitsPoolQueue = new ConcurrentDictionary<string, HitAbstract>();
            var activatePoolQueue = new ConcurrentDictionary<string, Activate>();

            var strategyMock = new Mock<BatchingContinuousCachingStrategy>(new object[] { config, httpClient, hitsPoolQueue, activatePoolQueue })
            {
                CallBase = true,
            };

            var strategy = strategyMock.Object;

            var visitorId = "visitorId";

            var now = DateTime.Now;
            var dateTimeProviderMock = new Mock<IDateTimeProvider>();
            dateTimeProviderMock.Setup(x => x.Now).Returns(now);

            for (int i = 0; i < 20; i++)
            {
                var screen = new Screen("home")
                {
                    VisitorId = visitorId,
                    Key = $"{visitorId}:{Guid.NewGuid()}",
                    CreatedAt = now,
                    DateTimeProvider = dateTimeProviderMock.Object
                };

                hitsPoolQueue.TryAdd(screen.Key, screen);
                batch.Hits.Add(screen);
            }

            Assert.AreEqual(20, hitsPoolQueue.Count);

            await strategy.SendBatch().ConfigureAwait(false);

            Assert.AreEqual(0, hitsPoolQueue.Count);

            strategyMock.Verify(x => x.CacheHitAsync(It.IsAny<ConcurrentDictionary<string, HitAbstract>>()), Times.Never());
            strategyMock.Verify(x => x.FlushHitsAsync(It.Is<string[]>(y => y.Any(z => z.Contains(visitorId)) && y.Length == 20)), Times.Once());

            httpResponse.Dispose();

        }

        [TestMethod()]
        public async Task SendBatchMaxSizeTest()
        {
            var fsLogManagerMock = new Mock<IFsLogManager>();
            var config = new DecisionApiConfig()
            {
                EnvId = "envID",
                LogManager = fsLogManagerMock.Object,
                TrackingManagerConfig = new TrackingManagerConfig()
            };

            HttpResponseMessage httpResponse = new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent("", Encoding.UTF8, "application/json")
            };


            Mock<HttpMessageHandler> mockHandler = new Mock<HttpMessageHandler>();




            var batch = new Batch
            {
                Config = config
            };

            Func<HttpRequestMessage, bool> actionBatch1 = (HttpRequestMessage x) =>
            {

                var batchedApiKeys = JToken.FromObject(batch.ToApiKeys());
                var result = x.Content?.ReadAsStringAsync().Result;
                var resultApiKeys = JToken.Parse(result ?? "");
                batchedApiKeys["qt"] = 0;
                resultApiKeys["qt"] = 0;
                var isEquals = JToken.DeepEquals(batchedApiKeys, resultApiKeys);

                var headers = new HttpRequestMessage().Headers;
                headers.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.HEADER_APPLICATION_JSON));

                return isEquals && headers.ToString() == x.Headers.ToString() && x.Method == HttpMethod.Post
                && x.RequestUri?.ToString() == Constants.HIT_EVENT_URL;
            };

            mockHandler.Protected().Setup<Task<HttpResponseMessage>>(
                 "SendAsync",
                  ItExpr.Is<HttpRequestMessage>(req => actionBatch1(req)),
                  ItExpr.IsAny<CancellationToken>()
                ).ReturnsAsync(httpResponse).Verifiable();

            var httpClient = new HttpClient(mockHandler.Object);

            var hitsPoolQueue = new ConcurrentDictionary<string, HitAbstract>();
            var activatePoolQueue = new ConcurrentDictionary<string, Activate>();

            var strategyMock = new Mock<BatchingContinuousCachingStrategy>(new object[] { config, httpClient, hitsPoolQueue, activatePoolQueue })
            {
                CallBase = true,
            };

            var strategy = strategyMock.Object;

            var visitorId = "visitorId";

            var now = DateTime.Now;
            var dateTimeProviderMock = new Mock<IDateTimeProvider>();
            dateTimeProviderMock.Setup(x => x.Now).Returns(now);


            for (int i = 0; i < 125; i++)
            {
                var screen = new Screen(string.Join("", Enumerable.Repeat("home", 5000)))
                {
                    VisitorId = visitorId,
                    Key = $"{visitorId}:{Guid.NewGuid()}",
                    CreatedAt = now,
                    DateTimeProvider = dateTimeProviderMock.Object
                };

                hitsPoolQueue.TryAdd(screen.Key, screen);

                if (i > 123)
                {
                    continue;
                }
                batch.Hits.Add(screen);
            }

            Assert.AreEqual(125, hitsPoolQueue.Count);

            await strategy.SendBatch().ConfigureAwait(false);

            Assert.AreEqual(1, hitsPoolQueue.Count);

            strategyMock.Verify(x => x.CacheHitAsync(It.IsAny<ConcurrentDictionary<string, HitAbstract>>()), Times.Never());
            strategyMock.Verify(x => x.FlushHitsAsync(It.Is<string[]>(y => y.Any(z => z.Contains(visitorId)) && y.Length == 124)), Times.Once());

            httpResponse.Dispose();

        }

        [TestMethod()]
        public async Task SendBatchHitExpiredTest()
        {
            var fsLogManagerMock = new Mock<IFsLogManager>();
            var config = new DecisionApiConfig()
            {
                EnvId = "envID",
                LogManager = fsLogManagerMock.Object,
                TrackingManagerConfig = new TrackingManagerConfig()
            };

            HttpResponseMessage httpResponse = new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent("", Encoding.UTF8, "application/json")
            };

            Mock<HttpMessageHandler> mockHandler = new();



            var batch = new Batch
            {
                Config = config
            };

            Func<HttpRequestMessage, bool> actionBatch1 = (HttpRequestMessage x) =>
            {

                var batchedApiKeys = JToken.FromObject(batch.ToApiKeys());
                var result = x.Content?.ReadAsStringAsync().Result;
                var resultApiKeys = JToken.Parse(result ?? "");
                batchedApiKeys["qt"] = 0;
                resultApiKeys["qt"] = 0;
                var isEquals = JToken.DeepEquals(batchedApiKeys, resultApiKeys);

                var headers = new HttpRequestMessage().Headers;
                headers.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.HEADER_APPLICATION_JSON));

                return isEquals && headers.ToString() == x.Headers.ToString() && x.Method == HttpMethod.Post
                && x.RequestUri?.ToString() == Constants.HIT_EVENT_URL;
            };

            mockHandler.Protected().Setup<Task<HttpResponseMessage>>(
                 "SendAsync",
                  ItExpr.Is<HttpRequestMessage>(req => actionBatch1(req)),
                  ItExpr.IsAny<CancellationToken>()
                ).ReturnsAsync(httpResponse).Verifiable();

            var httpClient = new HttpClient(mockHandler.Object);

            var hitsPoolQueue = new ConcurrentDictionary<string, HitAbstract>();
            var activatePoolQueue = new ConcurrentDictionary<string, Activate>();

            var strategyMock = new Mock<BatchingContinuousCachingStrategy>([config, httpClient, hitsPoolQueue, activatePoolQueue])
            {
                CallBase = true,
            };

            var strategy = strategyMock.Object;

            var now = DateTime.Now;
            var dateTimeProviderMock = new Mock<IDateTimeProvider>();
            dateTimeProviderMock.Setup(x => x.Now).Returns(now);

            var visitorId = "visitorId";

            var screen = new Screen("home")
            {
                VisitorId = visitorId,
                Key = $"{visitorId}:{Guid.NewGuid()}",
                CreatedAt = now.AddHours(-5),
                DateTimeProvider = dateTimeProviderMock.Object
            };

            batch.Hits.Add(screen);

            var screen2 = new Screen("home2")
            {
                VisitorId = visitorId,
                Key = $"{visitorId}:{Guid.NewGuid()}",
                CreatedAt = now.AddHours(-5),
                DateTimeProvider = dateTimeProviderMock.Object
            };

            hitsPoolQueue[screen.Key] = screen;
            hitsPoolQueue[screen2.Key] = screen2;

            Assert.AreEqual(2, hitsPoolQueue.Count);

            await strategy.SendBatch().ConfigureAwait(false);

            Assert.AreEqual(0, hitsPoolQueue.Count);

            strategyMock.Verify(x => x.CacheHitAsync(It.IsAny<ConcurrentDictionary<string, HitAbstract>>()), Times.Never());
            strategyMock.Verify(x => x.FlushHitsAsync(It.Is<string[]>(y => y.Any(z => z.Contains(visitorId)) && y.Length == 2)), Times.Once());

            httpResponse.Dispose();

        }

        [TestMethod()]
        public async Task SendBatchWithPoolMaxSizeTest()
        {
            var fsLogManagerMock = new Mock<IFsLogManager>();
            var config = new DecisionApiConfig()
            {
                EnvId = "envID",
                LogManager = fsLogManagerMock.Object,
                TrackingManagerConfig = new TrackingManagerConfig(CacheStrategy.CONTINUOUS_CACHING, 5)
            };

            HttpResponseMessage httpResponse = new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent("", Encoding.UTF8, "application/json")
            };


            Mock<HttpMessageHandler> mockHandler = new Mock<HttpMessageHandler>();




            var httpClient = new HttpClient(mockHandler.Object);

            var hitsPoolQueue = new ConcurrentDictionary<string, HitAbstract>();
            var activatePoolQueue = new ConcurrentDictionary<string, Activate>();

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

        }

        [TestMethod()]
        public async Task SendBatchFailedTest()
        {
            var fsLogManagerMock = new Mock<IFsLogManager>();
            var config = new DecisionApiConfig()
            {
                EnvId = "envID",
                LogManager = fsLogManagerMock.Object,
                TrackingManagerConfig = new TrackingManagerConfig()
            };

            HttpResponseMessage httpResponse = new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.BadRequest,
                Content = new StringContent("", Encoding.UTF8, "application/json")
            };


            Mock<HttpMessageHandler> mockHandler = new Mock<HttpMessageHandler>();




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

            var hitsPoolQueue = new ConcurrentDictionary<string, HitAbstract>();
            var activatePoolQueue = new ConcurrentDictionary<string, Activate>();

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

            strategyMock.Verify(x => x.CacheHitAsync(It.IsAny<ConcurrentDictionary<string, HitAbstract>>()), Times.Never());
            strategyMock.Verify(x => x.FlushHitsAsync(It.IsAny<string[]>()), Times.Never());
            strategyMock.Verify(x => x.SendTroubleshootingHit(It.Is<Troubleshooting>(item => item.Type == HitType.TROUBLESHOOTING)), Times.Once());

            httpResponse.Dispose();

        }

        [TestMethod()]
        public async Task ActivateFlagTest()
        {
            var fsLogManagerMock = new Mock<IFsLogManager>();
            var config = new DecisionApiConfig()
            {
                EnvId = "envID",
                LogManager = fsLogManagerMock.Object,
                TrackingManagerConfig = new TrackingManagerConfig()
            };



            HttpResponseMessage httpResponse = new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent("", Encoding.UTF8, "application/json")
            };

            Mock<HttpMessageHandler> mockHandler = new();

            var now = DateTime.Now;
            var dateTimeProviderMock = new Mock<IDateTimeProvider>();
            dateTimeProviderMock.Setup(x => x.Now).Returns(now);

            var visitorId = "visitorId";

            var activate = new Activate("variationGroupId", "variationId")
            {
                VisitorId = visitorId,
                Config = config,
                CreatedAt = now,
                DateTimeProvider = dateTimeProviderMock.Object
            };

            var activateList = new List<Activate>()
            {
                activate,
            };

            var batch = new ActivateBatch(activateList, config);

            Func<HttpRequestMessage, bool> actionBatch1 = (HttpRequestMessage x) =>
            {
                var batchedApiKeys = JToken.FromObject(batch.ToApiKeys());
                var result = x.Content?.ReadAsStringAsync().Result;
                var resultApiKeys = JToken.Parse(result ?? "");
                batchedApiKeys["qt"] = 0;
                resultApiKeys["qt"] = 0;
                var isEquals = JToken.DeepEquals(batchedApiKeys, resultApiKeys);

                var headers = new HttpRequestMessage().Headers;
                headers.Add(Constants.HEADER_X_API_KEY, config.ApiKey);
                headers.Add(Constants.HEADER_X_SDK_CLIENT, Constants.SDK_LANGUAGE);
                headers.Add(Constants.HEADER_X_SDK_VERSION, Constants.SDK_VERSION);
                headers.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.HEADER_APPLICATION_JSON));

                var url = Constants.BASE_API_URL + BatchingCachingStrategyAbstract.URL_ACTIVATE;

                var f = headers.ToString();

                return isEquals && headers.ToString() == x.Headers.ToString() && x.Method == HttpMethod.Post
                && x.RequestUri?.ToString() == url;
            };

            mockHandler.Protected().Setup<Task<HttpResponseMessage>>(
                 "SendAsync",
                  ItExpr.Is<HttpRequestMessage>(req => actionBatch1(req)),
                  ItExpr.IsAny<CancellationToken>()
                ).ReturnsAsync(httpResponse).Verifiable();

            var httpClient = new HttpClient(mockHandler.Object);

            var hitsPoolQueue = new ConcurrentDictionary<string, HitAbstract>();
            var activatePoolQueue = new ConcurrentDictionary<string, Activate>();

            var strategyMock = new Mock<BatchingContinuousCachingStrategy>(new object[] { config, httpClient, hitsPoolQueue, activatePoolQueue })
            {
                CallBase = true,
            };

            var strategy = strategyMock.Object;

            await strategy.ActivateFlag(activate).ConfigureAwait(false);

            Assert.AreEqual(0, hitsPoolQueue.Count);
            Assert.AreEqual(0, activatePoolQueue.Count);

            strategyMock.Verify(x => x.CacheHitAsync(It.IsAny<ConcurrentDictionary<string, HitAbstract>>()), Times.Never());
            strategyMock.Verify(x => x.FlushHitsAsync(It.IsAny<string[]>()), Times.Never());

            httpResponse.Dispose();
        }

        [TestMethod()]
        public async Task OnVisitorExposedTest()
        {
            var configMock = new Mock<DecisionApiConfig>();

            var config = configMock.Object;
            config.EnvId = "envId";
            config.TrackingManagerConfig = new TrackingManagerConfig();

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
                FlagKey = "flagKey",
                FlagValue = "value",
                FlagDefaultValue = "default Value"
            };
            var fromFlag = new ExposedFlag(activate.FlagKey, activate.FlagValue, activate.FlagDefaultValue, activate.FlagMetadata);
            var exposedVisitor = new ExposedVisitor(activate.VisitorId, activate.AnonymousId, activate.VisitorContext);

            mockHandler.Protected().Setup<Task<HttpResponseMessage>>(
                 "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>()
                ).ReturnsAsync(httpResponse).Verifiable();

            var httpClient = new HttpClient(mockHandler.Object);

            var hitsPoolQueue = new ConcurrentDictionary<string, HitAbstract>();
            var activatePoolQueue = new ConcurrentDictionary<string, Activate>();

            var strategyMock = new Mock<BatchingContinuousCachingStrategy>(new object[] { config, httpClient, hitsPoolQueue, activatePoolQueue })
            {
                CallBase = true,
            };

            var strategy = strategyMock.Object;

            await strategy.ActivateFlag(activate).ConfigureAwait(false);

            configMock.Verify(x => x.InvokeOnVisitorExposed(
                It.Is<ExposedVisitor>(y => JsonConvert.SerializeObject(y) == JsonConvert.SerializeObject(exposedVisitor)),
                It.Is<ExposedFlag>(y => JsonConvert.SerializeObject(y) == JsonConvert.SerializeObject(fromFlag))), Times.Once);

            configMock.Setup(x => x.InvokeOnVisitorExposed(
                It.Is<ExposedVisitor>(y => JsonConvert.SerializeObject(y) == JsonConvert.SerializeObject(exposedVisitor)),
                It.Is<ExposedFlag>(y => JsonConvert.SerializeObject(y) == JsonConvert.SerializeObject(fromFlag))))
                .Throws(new Exception());

            await strategy.ActivateFlag(activate).ConfigureAwait(false);

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
                TrackingManagerConfig = new TrackingManagerConfig()
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
                return result.Contains(activate.VariationId) && result.Contains(activate.VariationGroupId) &&
                result.Contains(activate2.VariationId) && result.Contains(activate2.VariationGroupId) &&
                result.Contains(activate3.VariationId) && result.Contains(activate3.VariationGroupId) &&
                headers.ToString() == x.Headers.ToString() && x.Method == HttpMethod.Post
                && x.RequestUri.ToString() == url;
            };

            mockHandler.Protected().Setup<Task<HttpResponseMessage>>(
                 "SendAsync",
                  ItExpr.Is<HttpRequestMessage>(req => actionBatch1(req)),
                  ItExpr.IsAny<CancellationToken>()
                ).ReturnsAsync(httpResponse).Verifiable();

            var httpClient = new HttpClient(mockHandler.Object);

            var hitsPoolQueue = new ConcurrentDictionary<string, HitAbstract>();
            var activatePoolQueue = new ConcurrentDictionary<string, Activate>();

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

            strategyMock.Verify(x => x.CacheHitAsync(It.IsAny<ConcurrentDictionary<string, HitAbstract>>()), Times.Never());
            strategyMock.Verify(x => x.FlushHitsAsync(It.Is<string[]>(y => y.Contains(activate2.Key)
            && y.Contains(activate3.Key)
            && y.Length == 2)), Times.Once());

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
                TrackingManagerConfig = new TrackingManagerConfig()
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
                return result.Contains(activate.VariationId) && result.Contains(activate.VariationGroupId) &&
                result.Contains(activate2.VariationId) && result.Contains(activate2.VariationGroupId) &&
                result.Contains(activate3.VariationId) && result.Contains(activate3.VariationGroupId) &&
                headers.ToString() == x.Headers.ToString() && x.Method == HttpMethod.Post
                && x.RequestUri.ToString() == url;
            };

            mockHandler.Protected().Setup<Task<HttpResponseMessage>>(
                 "SendAsync",
                  ItExpr.Is<HttpRequestMessage>(req => actionBatch1(req)),
                  ItExpr.IsAny<CancellationToken>()
                ).ReturnsAsync(httpResponse).Verifiable();

            var httpClient = new HttpClient(mockHandler.Object);

            var hitsPoolQueue = new ConcurrentDictionary<string, HitAbstract>();
            var activatePoolQueue = new ConcurrentDictionary<string, Activate>();

            var strategyMock = new Mock<BatchingContinuousCachingStrategy>(new object[] { config, httpClient, hitsPoolQueue, activatePoolQueue })
            {
                CallBase = true,
            };

            var strategy = strategyMock.Object;

            activatePoolQueue.TryAdd(activate.Key, activate);
            activatePoolQueue.TryAdd(activate2.Key, activate2);
            activatePoolQueue.TryAdd(activate3.Key, activate3);

            Assert.AreEqual(3, strategy.ActivatePoolQueue.Count);

            await strategy.SendBatch().ConfigureAwait(false);

            Assert.AreEqual(0, strategy.HitsPoolQueue.Count);
            Assert.AreEqual(0, strategy.ActivatePoolQueue.Count);

            strategyMock.Verify(x => x.CacheHitAsync(It.IsAny<ConcurrentDictionary<string, HitAbstract>>()), Times.Never());
            strategyMock.Verify(x => x.FlushHitsAsync(It.Is<string[]>(y => y.Contains(activate2.Key)
            && y.Contains(activate3.Key) && y.Contains(activate.Key)
            && y.Length == 3)), Times.Once());

            httpResponse.Dispose();
        }

        [TestMethod()]
        public async Task SendActivateBatchTest()
        {
            var fsLogManagerMock = new Mock<IFsLogManager>();
            var config = new DecisionApiConfig()
            {
                EnvId = "envID",
                LogManager = fsLogManagerMock.Object,
                TrackingManagerConfig = new TrackingManagerConfig()
            };

            HttpResponseMessage httpResponse = new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent("", Encoding.UTF8, "application/json")
            };

            Mock<HttpMessageHandler> mockHandler = new Mock<HttpMessageHandler>();

            var visitorId = "visitorId";

            mockHandler.Protected().Setup<Task<HttpResponseMessage>>(
                 "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>()
                ).ReturnsAsync(httpResponse).Verifiable();

            var httpClient = new HttpClient(mockHandler.Object);

            var hitsPoolQueue = new ConcurrentDictionary<string, HitAbstract>();
            var activatePoolQueue = new ConcurrentDictionary<string, Activate>();

            var strategyMock = new Mock<BatchingContinuousCachingStrategy>(new object[] { config, httpClient, hitsPoolQueue, activatePoolQueue })
            {
                CallBase = true,
            };

            var strategy = strategyMock.Object;

            for (int i = 0; i < 250; i++)
            {
                var activate = new Activate("variationGroupId" + i, "variationId" + i)
                {
                    VisitorId = visitorId,
                    Config = config,
                    Key = $"{visitorId}:{Guid.NewGuid()}"
                };
                activatePoolQueue.TryAdd(activate.Key, activate);
            }

            Assert.AreEqual(250, strategy.ActivatePoolQueue.Count);

            await strategy.SendBatch().ConfigureAwait(false);

            Assert.AreEqual(0, strategy.HitsPoolQueue.Count);
            Assert.AreEqual(0, strategy.ActivatePoolQueue.Count);

            mockHandler.Protected().Verify<Task<HttpResponseMessage>>(
                 "SendAsync",
                    Times.Exactly(3),
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>()
                );

            // strategyMock.Verify(x => x.CacheHitAsync(It.IsAny<ConcurrentDictionary<string, HitAbstract>>()), Times.Never());
            // strategyMock.Verify(x => x.FlushHitsAsync(It.Is<string[]>(y => y.Contains(activate2.Key)
            // && y.Contains(activate3.Key) && y.Contains(activate.Key)
            // && y.Length == 3)), Times.Once());

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
                TrackingManagerConfig = new TrackingManagerConfig()
            };

            HttpResponseMessage httpResponse = new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.BadRequest,
                Content = new StringContent("", Encoding.UTF8, "application/json")
            };

            Mock<HttpMessageHandler> mockHandler = new Mock<HttpMessageHandler>();

            var now = DateTime.Now;
            var dateTimeProviderMock = new Mock<IDateTimeProvider>();
            dateTimeProviderMock.Setup(x => x.Now).Returns(now);

            var visitorId = "visitorId";

            var activate = new Activate("variationGroupId", "variationId")
            {
                VisitorId = visitorId,
                Config = config,
                CreatedAt = now,
                DateTimeProvider = dateTimeProviderMock.Object
            };

            var activateList = new List<Activate>()
            {
                activate,
            };

            var batch = new ActivateBatch(activateList, config);

            Func<HttpRequestMessage, bool> actionBatch1 = (HttpRequestMessage x) =>
            {
                var batchedApiKeys = JToken.FromObject(batch.ToApiKeys());
                var result = x.Content?.ReadAsStringAsync().Result;
                var resultApiKeys = JToken.Parse(result ?? "");
                batchedApiKeys["qt"] = 0;
                resultApiKeys["qt"] = 0;
                var isEquals = JToken.DeepEquals(batchedApiKeys, resultApiKeys);

                var headers = new HttpRequestMessage().Headers;
                headers.Add(Constants.HEADER_X_API_KEY, config.ApiKey);
                headers.Add(Constants.HEADER_X_SDK_CLIENT, Constants.SDK_LANGUAGE);
                headers.Add(Constants.HEADER_X_SDK_VERSION, Constants.SDK_VERSION);
                headers.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.HEADER_APPLICATION_JSON));

                var url = Constants.BASE_API_URL + BatchingCachingStrategyAbstract.URL_ACTIVATE;

                var f = headers.ToString();

                return isEquals && headers.ToString() == x.Headers.ToString() && x.Method == HttpMethod.Post
                && x.RequestUri?.ToString() == url;
            };

            mockHandler.Protected().Setup<Task<HttpResponseMessage>>(
                 "SendAsync",
                  ItExpr.Is<HttpRequestMessage>(req => actionBatch1(req)),
                  ItExpr.IsAny<CancellationToken>()
                ).ReturnsAsync(httpResponse).Verifiable();

            var httpClient = new HttpClient(mockHandler.Object);

            var hitsPoolQueue = new ConcurrentDictionary<string, HitAbstract>();
            var activatePoolQueue = new ConcurrentDictionary<string, Activate>();

            var strategyMock = new Mock<BatchingContinuousCachingStrategy>([config, httpClient, hitsPoolQueue, activatePoolQueue])
            {
                CallBase = true,
            };

            var strategy = strategyMock.Object;

            await strategy.ActivateFlag(activate).ConfigureAwait(false);

            Assert.AreEqual(0, hitsPoolQueue.Count);
            Assert.AreEqual(1, activatePoolQueue.Count);

            strategyMock.Verify(x => x.CacheHitAsync(It.Is<ConcurrentDictionary<string, HitAbstract>>(y => y.Values.Contains(activate))), Times.Once());
            strategyMock.Verify(x => x.FlushHitsAsync(It.IsAny<string[]>()), Times.Never());
            strategyMock.Verify(x => x.SendTroubleshootingHit(It.Is<Troubleshooting>(item => item.Type == HitType.TROUBLESHOOTING)), Times.Once());

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

            var hitsPoolQueue = new ConcurrentDictionary<string, HitAbstract>();
            var activatePoolQueue = new ConcurrentDictionary<string, Activate>();

            var strategyMock = new Mock<BatchingContinuousCachingStrategy>([config, httpClientMock.Object, hitsPoolQueue, activatePoolQueue])
            {
                CallBase = true,
            };

            var strategy = strategyMock.Object;

            var visitorId = "visitorId";

            var now = DateTime.Now;
            var dateTimeProviderMock = new Mock<IDateTimeProvider>();
            dateTimeProviderMock.Setup(x => x.Now).Returns(now);

            var key = $"{visitorId}:{Guid.NewGuid()}";

            var hits = new ConcurrentDictionary<string, HitAbstract>()
            {
                [key] = new Page("http://localhost")
                {
                    VisitorId = visitorId,
                    CreatedAt = now,
                    DateTimeProvider = dateTimeProviderMock.Object
                }
            };

            hitCacheImplementation.Setup(x => x.CacheHit(It.IsAny<JObject>())).Returns(Task.CompletedTask);

            var data = new JObject();
            var jsonSerializer = new JsonSerializer
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
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

                data[keyValue.Key] = JObject.FromObject(hitData, jsonSerializer);
            }

            await strategy.CacheHitAsync(hits).ConfigureAwait(false);

            var checkDeepEquals = (JObject actualData, JObject expectedData) =>
            {
                actualData[key]["data"]["time"] = now;
                expectedData[key]["data"]["time"] = now;
                return JToken.DeepEquals(actualData, expectedData);
            };

            hitCacheImplementation.Verify(x => x.CacheHit(It.Is<JObject>((y) => checkDeepEquals(y, data))), Times.Once());

            config.DisableCache = true;

            await strategy.CacheHitAsync(hits).ConfigureAwait(false);

            hitCacheImplementation.Verify(x => x.CacheHit(It.Is<JObject>(y => checkDeepEquals(y, data))), Times.Once());

            config.DisableCache = false;
            config.HitCacheImplementation = null;

            await strategy.CacheHitAsync(hits).ConfigureAwait(false);

            hitCacheImplementation.Verify(x => x.CacheHit(It.Is<JObject>(y => y.ToString() == data.ToString())), Times.Once());


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

            var hitsPoolQueue = new ConcurrentDictionary<string, HitAbstract>();
            var activatePoolQueue = new ConcurrentDictionary<string, Activate>();

            var strategyMock = new Mock<BatchingContinuousCachingStrategy>(new object[] { config, httpClientMock.Object, hitsPoolQueue, activatePoolQueue })
            {
                CallBase = true,
            };

            var strategy = strategyMock.Object;

            var visitorId = "visitorId";

            var now = DateTime.Now;
            var dateTimeProviderMock = new Mock<IDateTimeProvider>();
            dateTimeProviderMock.Setup(x => x.Now).Returns(now);

            var key = $"{visitorId}:{Guid.NewGuid()}";

            var hits = new ConcurrentDictionary<string, Activate>();
            hits.TryAdd(key, new Activate("variationGroupId", "variationId")
            {
                VisitorId = visitorId,
                CreatedAt = now,
                DateTimeProvider = dateTimeProviderMock.Object
            });

            hitCacheImplementation.Setup(x => x.CacheHit(It.IsAny<JObject>())).Returns(Task.CompletedTask);

            var data = new JObject();
            var jsonSerializer = new JsonSerializer
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
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

                data[keyValue.Key] = JObject.FromObject(hitData, jsonSerializer);
            }

            await strategy.CacheHitAsync(hits).ConfigureAwait(false);

            var checkDeepEquals = (JObject actualData, JObject expectedData) =>
           {
               actualData[key]["data"]["time"] = now;
               expectedData[key]["data"]["time"] = now;
               return JToken.DeepEquals(actualData, expectedData);
           };

            hitCacheImplementation.Verify(x => x.CacheHit(It.Is<JObject>(y => checkDeepEquals(y, data))), Times.Once());


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

            var hitsPoolQueue = new ConcurrentDictionary<string, HitAbstract>();
            var activatePoolQueue = new ConcurrentDictionary<string, Activate>();

            var strategy = new BatchingContinuousCachingStrategy(config, httpClientMock.Object, ref hitsPoolQueue, ref activatePoolQueue);

            var visitorId = "visitorId";

            var now = DateTime.Now;
            var dateTimeProviderMock = new Mock<IDateTimeProvider>();
            dateTimeProviderMock.Setup(x => x.Now).Returns(now);

            var key = $"{visitorId}:{Guid.NewGuid()}";


            var hits = new ConcurrentDictionary<string, HitAbstract>()
            {
                [key] = new Page("http://localhost")
                {
                    VisitorId = visitorId,
                    CreatedAt = now,
                    DateTimeProvider = dateTimeProviderMock.Object
                }
            };

            var exception = new Exception("Error");

            hitCacheImplementation.Setup(x => x.CacheHit(It.IsAny<JObject>())).ThrowsAsync(exception);

            var data = new JObject();
            var jsonSerializer = new JsonSerializer
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
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

                data[keyValue.Key] = JObject.FromObject(hitData, jsonSerializer);
            }

            await strategy.CacheHitAsync(hits).ConfigureAwait(false);

            var checkDeepEquals = (JObject actualData, JObject expectedData) =>
            {
                actualData[key]["data"]["time"] = now;
                expectedData[key]["data"]["time"] = now;
                return JToken.DeepEquals(actualData, expectedData);
            };

            hitCacheImplementation.Verify(x => x.CacheHit(It.Is<JObject>(y => checkDeepEquals(y, data))), Times.Once());

            fsLogManagerMock.Verify(x => x.Error(exception.Message, BatchingCachingStrategyAbstract.PROCESS_CACHE_HIT), Times.Once());


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

            var hitsPoolQueue = new ConcurrentDictionary<string, HitAbstract>();
            var activatePoolQueue = new ConcurrentDictionary<string, Activate>();

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

            var hitsPoolQueue = new ConcurrentDictionary<string, HitAbstract>();
            var activatePoolQueue = new ConcurrentDictionary<string, Activate>();

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

            var hitsPoolQueue = new ConcurrentDictionary<string, HitAbstract>();
            var activatePoolQueue = new ConcurrentDictionary<string, Activate>();

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

            var hitsPoolQueue = new ConcurrentDictionary<string, HitAbstract>();
            var activatePoolQueue = new ConcurrentDictionary<string, Activate>();

            var strategy = new BatchingContinuousCachingStrategy(config, httpClientMock.Object, ref hitsPoolQueue, ref activatePoolQueue);


            await strategy.FlushAllHitsAsync().ConfigureAwait(false);

            hitCacheImplementation.Verify(x => x.FlushAllHits(), Times.Once());

            fsLogManagerMock.Verify(x => x.Error(exception.Message, BatchingCachingStrategyAbstract.PROCESS_FLUSH_HIT), Times.Once());


        }

        [TestMethod()]
        public void AddTroubleshootingHitTest()
        {
            var fsLogManagerMock = new Mock<IFsLogManager>();
            var config = new DecisionApiConfig()
            {
                EnvId = "envID",
                LogManager = fsLogManagerMock.Object,
                TrackingManagerConfig = new TrackingManagerConfig()
            };



            HttpResponseMessage httpResponse = new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent("", Encoding.UTF8, "application/json")
            };

            Mock<HttpMessageHandler> mockHandler = new Mock<HttpMessageHandler>();

            mockHandler.Protected().Setup<Task<HttpResponseMessage>>(
                 "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>()
                ).ReturnsAsync(httpResponse).Verifiable();

            var httpClient = new HttpClient(mockHandler.Object);

            var hitsPoolQueue = new ConcurrentDictionary<string, HitAbstract>();
            var activatePoolQueue = new ConcurrentDictionary<string, Activate>();

            var strategyMock = new Mock<BatchingContinuousCachingStrategy>(new object[] { config, httpClient, hitsPoolQueue, activatePoolQueue })
            {
                CallBase = true,
            };

            var strategy = strategyMock.Object;

            var troubleshooting = new Troubleshooting();

            strategy.AddTroubleshootingHit(troubleshooting);

            Assert.AreEqual(1, strategy.TroubleshootingQueue.Count);

            httpResponse.Dispose();
        }

        [TestMethod()]
        public async Task SendTroubleshootingHitTest()
        {
            var fsLogManagerMock = new Mock<IFsLogManager>();
            var config = new DecisionApiConfig()
            {
                EnvId = "envID",
                LogManager = fsLogManagerMock.Object,
                TrackingManagerConfig = new TrackingManagerConfig()
            };



            HttpResponseMessage httpResponse = new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent("", Encoding.UTF8, "application/json")
            };

            Mock<HttpMessageHandler> mockHandler = new Mock<HttpMessageHandler>();

            var hitKey = "hitKey";

            var troubleshooting = new Troubleshooting()
            {
                Key = hitKey,
                Config = config,
                Traffic = 50
            };

            Func<HttpRequestMessage, bool> actionBatch1 = (HttpRequestMessage x) =>
            {
                var postDataString = JsonConvert.SerializeObject(troubleshooting.ToApiKeys());

                var headers = new HttpRequestMessage().Headers;
                headers.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.HEADER_APPLICATION_JSON));

                var url = Constants.TROUBLESHOOTING_HIT_URL;

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

            var hitsPoolQueue = new ConcurrentDictionary<string, HitAbstract>();
            var activatePoolQueue = new ConcurrentDictionary<string, Activate>();

            var strategyMock = new Mock<BatchingContinuousCachingStrategy>(new object[] { config, httpClient, hitsPoolQueue, activatePoolQueue })
            {
                CallBase = true,
            };

            strategyMock.Setup(x => x.IsTroubleshootingActivated()).Returns(true);

            var strategy = strategyMock.Object;

            strategy.TroubleshootingQueue.TryAdd(hitKey, troubleshooting);

            Assert.AreEqual(1, strategy.TroubleshootingQueue.Count);

            strategy.TroubleshootingData = new TroubleshootingData()
            {
                Traffic = 40
            };

            strategyMock.Setup(x => x.IsTroubleshootingActivated()).Returns(false);

            await strategy.SendTroubleshootingHit(troubleshooting);

            Assert.AreEqual(1, strategy.TroubleshootingQueue.Count);

            strategyMock.Setup(x => x.IsTroubleshootingActivated()).Returns(true);

            await strategy.SendTroubleshootingHit(troubleshooting);

            Assert.AreEqual(1, strategy.TroubleshootingQueue.Count);

            strategy.TroubleshootingData.Traffic = 100;

            await strategy.SendTroubleshootingHit(troubleshooting);

            Assert.AreEqual(0, strategy.TroubleshootingQueue.Count);

            mockHandler.Protected().Verify<Task<HttpResponseMessage>>(
            "SendAsync", Times.Once(), new object[] {
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            });

            httpResponse.Dispose();
        }

        [TestMethod()]
        public async Task SendTroubleshootingHitFailedTest()
        {
            var fsLogManagerMock = new Mock<IFsLogManager>();
            var config = new DecisionApiConfig()
            {
                EnvId = "envID",
                LogManager = fsLogManagerMock.Object,
                TrackingManagerConfig = new TrackingManagerConfig()
            };



            HttpResponseMessage httpResponse = new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.BadRequest,
                Content = new StringContent("", Encoding.UTF8, "application/json")
            };

            Mock<HttpMessageHandler> mockHandler = new Mock<HttpMessageHandler>();

            var troubleshooting = new Troubleshooting()
            {
                Config = config,
                Traffic = 100
            };

            Func<HttpRequestMessage, bool> actionBatch1 = (HttpRequestMessage x) =>
            {
                var postDataString = JsonConvert.SerializeObject(troubleshooting.ToApiKeys());

                var headers = new HttpRequestMessage().Headers;
                headers.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.HEADER_APPLICATION_JSON));

                var url = Constants.TROUBLESHOOTING_HIT_URL;

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

            var hitsPoolQueue = new ConcurrentDictionary<string, HitAbstract>();
            var activatePoolQueue = new ConcurrentDictionary<string, Activate>();

            var strategyMock = new Mock<BatchingContinuousCachingStrategy>(new object[] { config, httpClient, hitsPoolQueue, activatePoolQueue })
            {
                CallBase = true,
            };

            strategyMock.Setup(x => x.IsTroubleshootingActivated()).Returns(true);

            var strategy = strategyMock.Object;

            strategy.TroubleshootingData = new TroubleshootingData()
            {
                Traffic = 100
            };

            await strategy.SendTroubleshootingHit(troubleshooting);

            Assert.AreEqual(1, strategy.TroubleshootingQueue.Count);

            mockHandler.Protected().Verify<Task<HttpResponseMessage>>(
            "SendAsync", Times.Once(), new object[] {
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            });

            httpResponse.Dispose();
        }


        [TestMethod()]
        public async Task SendTroubleshootingHitQueueTest()
        {
            var fsLogManagerMock = new Mock<IFsLogManager>();
            var config = new DecisionApiConfig()
            {
                EnvId = "envID",
                LogManager = fsLogManagerMock.Object,
                TrackingManagerConfig = new TrackingManagerConfig()
            };



            HttpResponseMessage httpResponse = new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent("", Encoding.UTF8, "application/json")
            };

            Mock<HttpMessageHandler> mockHandler = new Mock<HttpMessageHandler>();

            var hitKey = "hitKey";

            var troubleshooting = new Troubleshooting()
            {
                Key = hitKey,
                Config = config,
                Traffic = 50
            };

            mockHandler.Protected().Setup<Task<HttpResponseMessage>>(
                 "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>()
                ).ReturnsAsync(httpResponse).Verifiable();

            var httpClient = new HttpClient(mockHandler.Object);

            var hitsPoolQueue = new ConcurrentDictionary<string, HitAbstract>();
            var activatePoolQueue = new ConcurrentDictionary<string, Activate>();

            var strategyMock = new Mock<BatchingContinuousCachingStrategy>(new object[] { config, httpClient, hitsPoolQueue, activatePoolQueue })
            {
                CallBase = true,
            };

            strategyMock.Setup(x => x.IsTroubleshootingActivated()).Returns(false);

            strategyMock.Setup(x => x.SendTroubleshootingHit(troubleshooting)).Returns(Task.CompletedTask);

            var strategy = strategyMock.Object;

            await strategy.SendTroubleshootingQueue();

            strategyMock.Setup(x => x.IsTroubleshootingActivated()).Returns(true);

            strategy.TroubleshootingQueue.TryAdd(hitKey, troubleshooting);

            await strategy.SendTroubleshootingQueue();

            mockHandler.Protected().Verify<Task<HttpResponseMessage>>(
            "SendAsync", Times.Never(), new object[] {
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            });

            strategyMock.Verify(x => x.SendTroubleshootingHit(troubleshooting), Times.Once());

            httpResponse.Dispose();
        }

        [TestMethod()]
        public void AddUsageHitTest()
        {
            var fsLogManagerMock = new Mock<IFsLogManager>();
            var config = new DecisionApiConfig()
            {
                EnvId = "envID",
                LogManager = fsLogManagerMock.Object,
                TrackingManagerConfig = new TrackingManagerConfig()
            };



            HttpResponseMessage httpResponse = new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent("", Encoding.UTF8, "application/json")
            };

            Mock<HttpMessageHandler> mockHandler = new Mock<HttpMessageHandler>();

            mockHandler.Protected().Setup<Task<HttpResponseMessage>>(
                 "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>()
                ).ReturnsAsync(httpResponse).Verifiable();

            var httpClient = new HttpClient(mockHandler.Object);

            var hitsPoolQueue = new ConcurrentDictionary<string, HitAbstract>();
            var activatePoolQueue = new ConcurrentDictionary<string, Activate>();

            var strategyMock = new Mock<BatchingContinuousCachingStrategy>(new object[] { config, httpClient, hitsPoolQueue, activatePoolQueue })
            {
                CallBase = true,
            };

            var strategy = strategyMock.Object;

            var usageHit = new UsageHit();

            strategy.AddUsageHit(usageHit);

            Assert.AreEqual(1, strategy.UsageHitQueue.Count);

            httpResponse.Dispose();
        }

        [TestMethod()]
        public async Task SendUsageHitTest()
        {
            var fsLogManagerMock = new Mock<IFsLogManager>();
            var config = new DecisionApiConfig()
            {
                EnvId = "envID",
                LogManager = fsLogManagerMock.Object,
                TrackingManagerConfig = new TrackingManagerConfig()
            };



            HttpResponseMessage httpResponse = new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent("", Encoding.UTF8, "application/json")
            };

            Mock<HttpMessageHandler> mockHandler = new Mock<HttpMessageHandler>();

            var hitKey = "hitKey";

            var usageHit = new UsageHit()
            {
                Key = hitKey,
                Config = config
            };

            Func<HttpRequestMessage, bool> actionBatch1 = (HttpRequestMessage x) =>
            {
                var postDataString = JsonConvert.SerializeObject(usageHit.ToApiKeys());

                var headers = new HttpRequestMessage().Headers;
                headers.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.HEADER_APPLICATION_JSON));

                var url = Constants.USAGE_HIT_URL;

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

            var hitsPoolQueue = new ConcurrentDictionary<string, HitAbstract>();
            var activatePoolQueue = new ConcurrentDictionary<string, Activate>();

            var strategyMock = new Mock<BatchingContinuousCachingStrategy>(new object[] { config, httpClient, hitsPoolQueue, activatePoolQueue })
            {
                CallBase = true,
            };


            var strategy = strategyMock.Object;

            strategy.UsageHitQueue.TryAdd(hitKey, usageHit);

            Assert.AreEqual(1, strategy.UsageHitQueue.Count);

            strategy.TroubleshootingData = new TroubleshootingData()
            {
                Traffic = 40
            };

            await strategy.SendUsageHit(usageHit);

            Assert.AreEqual(0, strategy.UsageHitQueue.Count);

            mockHandler.Protected().Verify<Task<HttpResponseMessage>>(
            "SendAsync", Times.Once(), new object[] {
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            });

            httpResponse.Dispose();
        }

        [TestMethod()]
        public async Task SendUsageHitFailedTest()
        {
            var fsLogManagerMock = new Mock<IFsLogManager>();
            var config = new DecisionApiConfig()
            {
                EnvId = "envID",
                LogManager = fsLogManagerMock.Object,
                TrackingManagerConfig = new TrackingManagerConfig()
            };



            HttpResponseMessage httpResponse = new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.BadRequest,
                Content = new StringContent("", Encoding.UTF8, "application/json")
            };

            Mock<HttpMessageHandler> mockHandler = new Mock<HttpMessageHandler>();

            var usageHit = new UsageHit()
            {
                Config = config
            };

            Func<HttpRequestMessage, bool> actionBatch1 = (HttpRequestMessage x) =>
            {
                var postDataString = JsonConvert.SerializeObject(usageHit.ToApiKeys());

                var headers = new HttpRequestMessage().Headers;
                headers.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.HEADER_APPLICATION_JSON));

                var url = Constants.USAGE_HIT_URL;

                var result = x.Content.ReadAsStringAsync().Result;
                return result == postDataString && headers.ToString() == x.Headers.ToString() && x.Method == HttpMethod.Post
                && x.RequestUri.ToString() == url;
            };

            mockHandler.Protected().Setup<Task<HttpResponseMessage>>(
                 "SendAsync",
                  ItExpr.Is<HttpRequestMessage>(req => actionBatch1(req)),
                  ItExpr.IsAny<CancellationToken>()
                ).ReturnsAsync(httpResponse);

            var httpClient = new HttpClient(mockHandler.Object);

            var hitsPoolQueue = new ConcurrentDictionary<string, HitAbstract>();
            var activatePoolQueue = new ConcurrentDictionary<string, Activate>();

            var strategyMock = new Mock<BatchingContinuousCachingStrategy>(new object[] { config, httpClient, hitsPoolQueue, activatePoolQueue })
            {
                CallBase = true,
            };

            var strategy = strategyMock.Object;

            await strategy.SendUsageHit(usageHit);

            Assert.AreEqual(1, strategy.UsageHitQueue.Count);

            mockHandler.Protected().Verify<Task<HttpResponseMessage>>(
            "SendAsync", Times.Once(), new object[] {
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            });

            httpResponse.Dispose();
        }

        [TestMethod()]
        public async Task SendUsageHitQueueTest()
        {
            var fsLogManagerMock = new Mock<IFsLogManager>();
            var config = new DecisionApiConfig()
            {
                EnvId = "envID",
                LogManager = fsLogManagerMock.Object,
                TrackingManagerConfig = new TrackingManagerConfig()
            };



            HttpResponseMessage httpResponse = new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent("", Encoding.UTF8, "application/json")
            };

            Mock<HttpMessageHandler> mockHandler = new Mock<HttpMessageHandler>();

            var hitKey = "hitKey";

            var usageHit = new UsageHit()
            {
                Key = hitKey,
                Config = config
            };

            mockHandler.Protected().Setup<Task<HttpResponseMessage>>(
                 "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>()
                ).ReturnsAsync(httpResponse).Verifiable();

            var httpClient = new HttpClient(mockHandler.Object);

            var hitsPoolQueue = new ConcurrentDictionary<string, HitAbstract>();
            var activatePoolQueue = new ConcurrentDictionary<string, Activate>();

            var strategyMock = new Mock<BatchingContinuousCachingStrategy>(new object[] { config, httpClient, hitsPoolQueue, activatePoolQueue })
            {
                CallBase = true,
            };


            strategyMock.Setup(x => x.SendUsageHit(usageHit)).Returns(Task.CompletedTask);

            var strategy = strategyMock.Object;

            await strategy.SendUsageHitQueue();

            strategy.UsageHitQueue.TryAdd(hitKey, usageHit);

            await strategy.SendUsageHitQueue();

            mockHandler.Protected().Verify<Task<HttpResponseMessage>>(
            "SendAsync", Times.Never(), new object[] {
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            });

            strategyMock.Verify(x => x.SendUsageHit(usageHit), Times.Once());

            httpResponse.Dispose();
        }

        [TestMethod()]
        public void IsTroubleshootingActivatedTest()
        {
            var fsLogManagerMock = new Mock<IFsLogManager>();
            var config = new DecisionApiConfig()
            {
                EnvId = "envID",
                LogManager = fsLogManagerMock.Object,
                TrackingManagerConfig = new TrackingManagerConfig()
            };

            HttpResponseMessage httpResponse = new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent("", Encoding.UTF8, "application/json")
            };

            Mock<HttpMessageHandler> mockHandler = new Mock<HttpMessageHandler>();

            var hitKey = "hitKey";

            var usageHit = new UsageHit()
            {
                Key = hitKey,
                Config = config
            };

            mockHandler.Protected().Setup<Task<HttpResponseMessage>>(
                 "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>()
                ).ReturnsAsync(httpResponse).Verifiable();

            var httpClient = new HttpClient(mockHandler.Object);

            var hitsPoolQueue = new ConcurrentDictionary<string, HitAbstract>();
            var activatePoolQueue = new ConcurrentDictionary<string, Activate>();

            var strategyMock = new Mock<BatchingContinuousCachingStrategy>(new object[] { config, httpClient, hitsPoolQueue, activatePoolQueue })
            {
                CallBase = true,
            };

            var strategy = strategyMock.Object;

            var check = strategy.IsTroubleshootingActivated();

            Assert.IsFalse(check);

            strategy.TroubleshootingData = new TroubleshootingData
            {
                StartDate = DateTime.Now.ToUniversalTime().AddMinutes(5),
            };

            check = strategy.IsTroubleshootingActivated();

            Assert.IsFalse(check);

            strategy.TroubleshootingData = new TroubleshootingData
            {
                StartDate = DateTime.Now.ToUniversalTime().AddMinutes(-2),
                EndDate = DateTime.Now.ToUniversalTime().AddSeconds(-10),
            };

            check = strategy.IsTroubleshootingActivated();

            Assert.IsFalse(check);

            strategy.TroubleshootingData = new TroubleshootingData
            {
                StartDate = DateTime.Now.ToUniversalTime().AddMinutes(-2),
                EndDate = DateTime.Now.ToUniversalTime().AddMinutes(10),
            };

            check = strategy.IsTroubleshootingActivated();

            Assert.IsTrue(check);

            httpResponse.Dispose();
        }

    }
}
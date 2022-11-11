﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Flagship.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flagship.Hit;
using Moq;
using Flagship.Logger;
using Flagship.Config;
using System.Net.Http;
using Flagship.Enums;
using System.Net.Http.Headers;
using System.Threading;
using Moq.Protected;
using Newtonsoft.Json;
using Microsoft.QualityTools.Testing.Fakes;

namespace Flagship.Api.Tests
{
    [TestClass()]
    public class BatchingPeriodicCachingStrategyTests
    {
      

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

            var strategyMock = new Mock<BatchingPeriodicCachingStrategy>(new object[] { config, httpClientMock.Object, hitsPoolQueue, activatePoolQueue })
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
            Assert.IsTrue(hitsPoolQueue.ContainsValue(page));

            strategyMock.Verify(x => x.CacheHitAsync(It.IsAny<Dictionary<string, HitAbstract>>()), Times.Never());

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
            Assert.IsTrue(hitsPoolQueue.ContainsValue(hitEvent));

            strategyMock.Verify(x => x.CacheHitAsync(It.IsAny<Dictionary<string, HitAbstract>>()), Times.Never());
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

            var strategyMock = new Mock<BatchingPeriodicCachingStrategy>(new object[] { config, httpClientMock.Object, hitsPoolQueue, activatePoolQueue })
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

            strategyMock.Verify(x => x.CacheHitAsync(hitsPoolQueue), Times.Once());
            strategyMock.Verify(x => x.FlushHitsAsync(It.IsAny<string[]>()), Times.Never());
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
            };

            var hitsPoolQueue = new Dictionary<string, HitAbstract>();
            var activatePoolQueue = new Dictionary<string, Activate>();

            var strategyMock = new Mock<BatchingPeriodicCachingStrategy>(new object[] { config, httpClientMock.Object, hitsPoolQueue, activatePoolQueue })
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

            strategyMock.Verify(x => x.CacheHitAsync(It.IsAny<Dictionary<string, HitAbstract>>()), Times.Never());
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

            var strategyMock = new Mock<BatchingPeriodicCachingStrategy>(new object[] { config, httpClient, hitsPoolQueue, activatePoolQueue })
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

            strategyMock.Verify(x => x.CacheHitAsync(hitsPoolQueue), Times.Once());
            strategyMock.Verify(x => x.FlushHitsAsync(It.IsAny<string[]>()), Times.Never());


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

            strategyMock.Verify(x => x.CacheHitAsync(hitsPoolQueue), Times.Once());
            strategyMock.Verify(x => x.FlushHitsAsync(It.IsAny<string[]>()), Times.Never());


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

            var strategyMock = new Mock<BatchingPeriodicCachingStrategy>(new object[] { config, httpClient, hitsPoolQueue, activatePoolQueue })
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

            strategyMock.Verify(x => x.CacheHitAsync(hitsPoolQueue), Times.Once());
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

            var strategyMock = new Mock<BatchingPeriodicCachingStrategy>(new object[] { config, httpClient, hitsPoolQueue, activatePoolQueue })
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

            var strategyMock = new Mock<BatchingPeriodicCachingStrategy>(new object[] { config, httpClient, hitsPoolQueue, activatePoolQueue })
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
            strategyMock.Verify(x => x.FlushHitsAsync(It.IsAny<string[]>()), Times.Never());

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

            var strategyMock = new Mock<BatchingPeriodicCachingStrategy>(new object[] { config, httpClient, hitsPoolQueue, activatePoolQueue })
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

            strategyMock.Verify(x => x.CacheHitAsync(activatePoolQueue), Times.Once());
            strategyMock.Verify(x => x.FlushHitsAsync(It.IsAny<string[]>()), Times.Never());
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

            var strategyMock = new Mock<BatchingPeriodicCachingStrategy>(new object[] { config, httpClient, hitsPoolQueue, activatePoolQueue })
            {
                CallBase = true,
            };

            var strategy = strategyMock.Object;

            await strategy.ActivateFlag(activate).ConfigureAwait(false);

            Assert.AreEqual(0, hitsPoolQueue.Count);
            Assert.AreEqual(1, activatePoolQueue.Count);

            strategyMock.Verify(x => x.CacheHitAsync(It.IsAny<Dictionary<string, HitAbstract>>()), Times.Never());
            strategyMock.Verify(x => x.FlushHitsAsync(It.IsAny<string[]>()), Times.Never());

            httpResponse.Dispose();
        }

        [TestMethod()]
        public async Task MergedPoolQueue()
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

            mockHandler.Protected().Setup<Task<HttpResponseMessage>>(
                 "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>()
                ).ReturnsAsync(httpResponse).Verifiable();

            var httpClient = new HttpClient(mockHandler.Object);

            var hitsPoolQueue = new Dictionary<string, HitAbstract>();
            var activatePoolQueue = new Dictionary<string, Activate>();

            var strategyMock = new Mock<BatchingPeriodicCachingStrategy>(new object[] { config, httpClient, hitsPoolQueue, activatePoolQueue })
            {
                CallBase = true,
            };

            var shimeContext = ShimsContext.Create();
            System.Fakes.ShimDateTime.NowGet = () => { return new DateTime(2022, 1, 1); };


            var strategy = strategyMock.Object;

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

            activatePoolQueue[activate.Key] = activate;
            activatePoolQueue[activate2.Key] = activate2;
            activatePoolQueue[activate3.Key] = activate3;

            Assert.AreEqual(3, activatePoolQueue.Count);

            var page = new Page("http://localhost")
            {
                VisitorId = visitorId,
                Config = config,
                Key= $"{visitorId}:{Guid.NewGuid()}"
            };

            hitsPoolQueue[page.Key] = page;

            await strategy.SendBatch().ConfigureAwait(false);

            Assert.AreEqual(1, hitsPoolQueue.Count);
            Assert.AreEqual(3, activatePoolQueue.Count);

            var mergedQueue = new Dictionary<string, HitAbstract>(hitsPoolQueue);
            foreach (var item in activatePoolQueue)
            {
                mergedQueue.Add(item.Key, item.Value);
            }

            strategyMock.Verify(x => x.CacheHitAsync(It.Is<Dictionary<string, HitAbstract>>(y=>JsonConvert.SerializeObject(y)==JsonConvert.SerializeObject(mergedQueue))), Times.Once());
            strategyMock.Verify(x => x.FlushHitsAsync(It.IsAny<string[]>()), Times.Never());
            
            httpResponse.Dispose();
            shimeContext.Dispose();
        }
    }
}
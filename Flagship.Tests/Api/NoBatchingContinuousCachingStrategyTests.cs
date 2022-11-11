﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Flagship.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flagship.Logger;
using Moq;
using Flagship.Hit;
using System.Net.Http;
using System.Threading;
using Moq.Protected;
using Flagship.Enums;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using Microsoft.QualityTools.Testing.Fakes;
using Flagship.Config;

namespace Flagship.Api.Tests
{
    [TestClass()]
    public class NoBatchingContinuousCachingStrategyTests
    {

        [TestMethod()]
        public async Task AddTest()
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

            var visitorId = "visitorId";

            var eventHit = new Event( EventCategory.ACTION_TRACKING, "click")
            {
                VisitorId = visitorId,
                Config = config
            };

            Func<HttpRequestMessage, bool> actionBatch1 = (HttpRequestMessage x) =>
            {

                var postDataString = JsonConvert.SerializeObject(eventHit.ToApiKeys());
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

            var strategyMock = new Mock<NoBatchingContinuousCachingStrategy>(new object[] { config, httpClient, hitsPoolQueue, activatePoolQueue })
            {
                CallBase = true,
            };

            var strategy = strategyMock.Object;

            await strategy.Add(eventHit).ConfigureAwait(false);

            Assert.AreEqual(0, hitsPoolQueue.Count);
            Assert.AreEqual(0, activatePoolQueue.Count);

            strategyMock.Verify(x => x.CacheHitAsync(It.IsAny<Dictionary<string, HitAbstract>>()), Times.Never());
            strategyMock.Verify(x => x.FlushHitsAsync(It.IsAny<string[]>()), Times.Never());

            httpResponse.Dispose();
            shimeContext.Dispose();

        }

        [TestMethod()]
        public async Task AddFailedTest()
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

            var visitorId = "visitorId";

            var page = new Page("http://localhost")
            {
                VisitorId = visitorId,
                Config = config
            };

            Func<HttpRequestMessage, bool> actionBatch1 = (HttpRequestMessage x) =>
            {

                var postDataString = JsonConvert.SerializeObject(page.ToApiKeys());
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

            var strategyMock = new Mock<NoBatchingContinuousCachingStrategy>(new object[] { config, httpClient, hitsPoolQueue, activatePoolQueue })
            {
                CallBase = true,
            };

            var strategy = strategyMock.Object;

            await strategy.Add(page).ConfigureAwait(false);

            Assert.AreEqual(0, hitsPoolQueue.Count);
            Assert.AreEqual(0, activatePoolQueue.Count);

            strategyMock.Verify(x => x.CacheHitAsync(It.Is<Dictionary<string, HitAbstract>>(y=> y.ContainsValue(page))), Times.Once());
            strategyMock.Verify(x => x.FlushHitsAsync(It.IsAny<string[]>()), Times.Never());

            httpResponse.Dispose();
            shimeContext.Dispose();

        }

        [TestMethod()]
        public async Task NotConsentTest()
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

            var visitorId = "visitorId";

            var hitEvent = new Event(EventCategory.USER_ENGAGEMENT, Constants.FS_CONSENT)
            {
                Label = $"{Constants.SDK_LANGUAGE}:{false}",
                VisitorId = visitorId,
                DS = Constants.SDK_APP,
                Config = config,
                AnonymousId = null
            };

            Func<HttpRequestMessage, bool> actionBatch1 = (HttpRequestMessage x) =>
            {

                var postDataString = JsonConvert.SerializeObject(hitEvent.ToApiKeys());
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

            var eventHit = new Event(EventCategory.ACTION_TRACKING, "click")
            {
                VisitorId = visitorId,
                Config = config,
                Key= $"{visitorId}:{Guid.NewGuid()}"
            };

            var hitsPoolQueue = new Dictionary<string, HitAbstract>()
            {
                {eventHit.Key, eventHit }
            };

            var activate = new Activate("variationGroupId", "variationId")
            {
                VisitorId = visitorId,
                Config = config,
                Key = $"{visitorId}:{Guid.NewGuid()}"
            };

            var activatePoolQueue = new Dictionary<string, Activate>()
            {
                {activate.Key, activate}
            };

            var strategyMock = new Mock<NoBatchingContinuousCachingStrategy>(new object[] { config, httpClient, hitsPoolQueue, activatePoolQueue })
            {
                CallBase = true,
            };

            var strategy = strategyMock.Object;


            mockHandler.Protected().Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => actionBatch1(req)),
                ItExpr.IsAny<CancellationToken>()
                ).ThrowsAsync(new Exception()).Verifiable();


            var screenView = new Screen("home")
            {
                Config = config,
                VisitorId = visitorId,
                Key = $"{visitorId}:{Guid.NewGuid()}"
            };

            await strategy.Add(screenView).ConfigureAwait(false);

            mockHandler.Protected().Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => actionBatch1(req)),
                ItExpr.IsAny<CancellationToken>()
                ).ReturnsAsync(httpResponse).Verifiable();

            await strategy.Add(hitEvent).ConfigureAwait(false);

            strategyMock.Verify(x => x.CacheHitAsync(It.Is<Dictionary<string, HitAbstract>>(y=> y.ContainsValue(screenView))), Times.Once());
            strategyMock.Verify(x => x.FlushHitsAsync(new string[] { eventHit.Key, screenView.Key, activate.Key}), Times.Once());

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

            var shimeContext = ShimsContext.Create();
            System.Fakes.ShimDateTime.NowGet = () => { return new DateTime(2022, 1, 1); };

            var visitorId = "visitorId";

            var activate = new Activate("variationGroupId", "variationId")
            {
                VisitorId = visitorId,
                Config = config
            };

            var activateBatch = new ActivateBatch(new List<Activate>() { activate }, config);

            Func<HttpRequestMessage, bool> actionBatch1 = (HttpRequestMessage x) =>
            {
                var postDataString = JsonConvert.SerializeObject(activateBatch.ToApiKeys());
                var headers = new HttpRequestMessage().Headers;

                var url = Constants.BASE_API_URL + BatchingCachingStrategyAbstract.URL_ACTIVATE;

                headers.Add(Constants.HEADER_X_API_KEY, config.ApiKey);
                headers.Add(Constants.HEADER_X_SDK_CLIENT, Constants.SDK_LANGUAGE);
                headers.Add(Constants.HEADER_X_SDK_VERSION, Constants.SDK_VERSION);
                headers.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.HEADER_APPLICATION_JSON));

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

            var strategyMock = new Mock<NoBatchingContinuousCachingStrategy>(new object[] { config, httpClient, hitsPoolQueue, activatePoolQueue })
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
            shimeContext.Dispose();
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

            var shimeContext = ShimsContext.Create();
            System.Fakes.ShimDateTime.NowGet = () => { return new DateTime(2022, 1, 1); };

            var visitorId = "visitorId";

            var activate = new Activate("variationGroupId", "variationId")
            {
                VisitorId = visitorId,
                Config = config
            };

            var activateBatch = new ActivateBatch(new List<Activate>() { activate }, config);

            Func<HttpRequestMessage, bool> actionBatch1 = (HttpRequestMessage x) =>
            {
                var postDataString = JsonConvert.SerializeObject(activateBatch.ToApiKeys());
                var headers = new HttpRequestMessage().Headers;

                var url = Constants.BASE_API_URL + BatchingCachingStrategyAbstract.URL_ACTIVATE;

                headers.Add(Constants.HEADER_X_API_KEY, config.ApiKey);
                headers.Add(Constants.HEADER_X_SDK_CLIENT, Constants.SDK_LANGUAGE);
                headers.Add(Constants.HEADER_X_SDK_VERSION, Constants.SDK_VERSION);
                headers.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.HEADER_APPLICATION_JSON));

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

            var strategyMock = new Mock<NoBatchingContinuousCachingStrategy>(new object[] { config, httpClient, hitsPoolQueue, activatePoolQueue })
            {
                CallBase = true,
            };

            var strategy = strategyMock.Object;

            await strategy.ActivateFlag(activate).ConfigureAwait(false);

            Assert.AreEqual(0, hitsPoolQueue.Count);
            Assert.AreEqual(0, activatePoolQueue.Count);

            strategyMock.Verify(x => x.CacheHitAsync(It.Is<Dictionary<string, HitAbstract>>(y=> y.ContainsValue(activate))), Times.Once());
            strategyMock.Verify(x => x.FlushHitsAsync(It.IsAny<string[]>()), Times.Never());

            httpResponse.Dispose();
            shimeContext.Dispose();
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

            var strategyMock = new Mock<NoBatchingContinuousCachingStrategy>(new object[] { config, httpClient, hitsPoolQueue, activatePoolQueue })
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

            var strategyMock = new Mock<NoBatchingContinuousCachingStrategy>(new object[] { config, httpClient, hitsPoolQueue, activatePoolQueue })
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

            var strategyMock = new Mock<NoBatchingContinuousCachingStrategy>(new object[] { config, httpClient, hitsPoolQueue, activatePoolQueue })
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


    }
}
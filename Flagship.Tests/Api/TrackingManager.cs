using Flagship.Api;
using Flagship.Enums;
using Flagship.Hit;
using Flagship.Logger;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using RichardSzalay.MockHttp;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Moq.Protected;
using Newtonsoft.Json.Linq;
using Flagship.Model;

namespace Flagship.Tests.Api
{
    [TestClass]
    public class TrackingManager
    {
        [TestMethod]
        public void InitStrategy()
        {
            var httpClientMock = new Mock<HttpClient>();
            var fsLogManagerMock = new Mock<IFsLogManager>();

            var config = new Flagship.Config.DecisionApiConfig
            {
                LogManager = fsLogManagerMock.Object,
                TrackingMangerConfig = new Config.TrackingManagerConfig()
            };

            var trackingManager = new Flagship.Api.TrackingManager(config, httpClientMock.Object);

            var trackingManagerPrivate = new PrivateObject(trackingManager);

            object strategy = trackingManagerPrivate.Invoke("InitStrategy");

            Assert.IsInstanceOfType(strategy, typeof(BatchingContinuousCachingStrategy));

            config.TrackingMangerConfig = new Config.TrackingManagerConfig(CacheStrategy.PERIODIC_CACHING);

            strategy = trackingManagerPrivate.Invoke("InitStrategy");

            Assert.IsInstanceOfType(strategy, typeof(BatchingPeriodicCachingStrategy));

            config.TrackingMangerConfig = new Config.TrackingManagerConfig(CacheStrategy.NO_BATCHING);

            strategy = trackingManagerPrivate.Invoke("InitStrategy");

            Assert.IsInstanceOfType(strategy, typeof(NoBatchingContinuousCachingStrategy));

            config.TrackingMangerConfig = new Config.TrackingManagerConfig(CacheStrategy.CONTINUOUS_CACHING);

            strategy = trackingManagerPrivate.Invoke("InitStrategy");

            Assert.IsInstanceOfType(strategy, typeof(BatchingContinuousCachingStrategy));
        }



        [TestMethod]
        public async Task Add()
        {
            var httpClientMock = new Mock<HttpClient>();
            var fsLogManagerMock = new Mock<IFsLogManager>();

            var config = new Flagship.Config.DecisionApiConfig
            {
                LogManager = fsLogManagerMock.Object,
                TrackingMangerConfig = new Config.TrackingManagerConfig()
            };

            var trackingManagerMock = new Mock<Flagship.Api.TrackingManager>(config, httpClientMock.Object)
            {
                CallBase = true,
            };

            var trackingManager = trackingManagerMock.Object;


            var hitsPoolQueue = new Dictionary<string, HitAbstract>();
            var activatePoolQueue = new Dictionary<string, Activate>();

            var strategyMock = new Mock<BatchingCachingStrategyAbstract>(config, httpClientMock.Object, hitsPoolQueue, activatePoolQueue);

            trackingManagerMock.Protected().SetupGet<BatchingCachingStrategyAbstract>("Strategy").Returns(strategyMock.Object);

            var page = new Page("https://myurl.com");

            await trackingManager.Add(page).ConfigureAwait(false);

            strategyMock.Verify(x => x.Add(page), Times.Once());
        }

        [TestMethod]
        public async Task ActivateFlag()
        {
            var httpClientMock = new Mock<HttpClient>();
            var fsLogManagerMock = new Mock<IFsLogManager>();

            var config = new Flagship.Config.DecisionApiConfig
            {
                LogManager = fsLogManagerMock.Object,
                TrackingMangerConfig = new Config.TrackingManagerConfig()
            };

            var trackingManagerMock = new Mock<Flagship.Api.TrackingManager>(config, httpClientMock.Object)
            {
                CallBase = true,
            };

            var trackingManager = trackingManagerMock.Object;


            var hitsPoolQueue = new Dictionary<string, HitAbstract>();
            var activatePoolQueue = new Dictionary<string, Activate>();

            var strategyMock = new Mock<BatchingCachingStrategyAbstract>(config, httpClientMock.Object, hitsPoolQueue, activatePoolQueue);

            trackingManagerMock.Protected().SetupGet<BatchingCachingStrategyAbstract>("Strategy").Returns(strategyMock.Object);

            var activate = new Activate("variationGroupId", "variationId");

            await trackingManager.ActivateFlag(activate).ConfigureAwait(false);

            strategyMock.Verify(x => x.ActivateFlag(activate), Times.Once());
        }

        public async Task LookupHitsAsync()
        {
            var httpClientMock = new Mock<HttpClient>();
            var fsLogManagerMock = new Mock<IFsLogManager>();
            var hitCacheImplementation = new Mock<Cache.IHitCacheImplementation>();

            var config = new Flagship.Config.DecisionApiConfig
            {
                LogManager = fsLogManagerMock.Object,
                TrackingMangerConfig = new Config.TrackingManagerConfig(),
                HitCacheImplementation = hitCacheImplementation.Object,
            };

            var trackingManagerMock = new Mock<Flagship.Api.TrackingManager>(config, httpClientMock.Object)
            {
                CallBase = true,
            };

            var trackingManager = trackingManagerMock.Object;

            var visitorId = "visitorId";

            var screen = new Screen("home")
            {
                VisitorId = visitorId,
                Key = $"{visitorId}:{Guid.NewGuid()}",
                Config = config
            };

            var page = new Page("https://myurl.com")
            {
                VisitorId = visitorId,
                Key = $"{visitorId}:{Guid.NewGuid()}",
                Config = config
            };

            var Event = new Event(EventCategory.USER_ENGAGEMENT, "click")
            {
                VisitorId = visitorId,
                Key = $"{visitorId}:{Guid.NewGuid()}",
                Config = config
            };

            var transaction = new Transaction("transId", "Aff")
            {
                VisitorId = visitorId,
                Key = $"{visitorId}:{Guid.NewGuid()}",
                Config = config
            };

            var item = new Item("transId", "name", "code")
            {
                VisitorId = visitorId,
                Key = $"{visitorId}:{Guid.NewGuid()}",
                Config = config
            };

            var segment = new Segment(new Dictionary<string, object>() { {"key","value"} })
            {
                VisitorId = visitorId,
                Key = $"{visitorId}:{Guid.NewGuid()}",
                Config = config
            };

            var activate = new Activate("variationGrId", "varationId")
            {
                VisitorId = visitorId,
                Key = $"{visitorId}:{Guid.NewGuid()}",
                Config = config
            };

            var hits = new Dictionary<string, HitAbstract>()
            {
                { screen.Key, screen },
                {page.Key, page },
                {Event.Key, Event },
                {transaction.Key, transaction },
                { item.Key, item },
                {segment.Key, segment },
                { activate.Key, activate }
            };

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

            await trackingManager.LookupHitsAsync().ConfigureAwait(false);
        }
    }
}

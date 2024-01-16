using Flagship.Api;
using Flagship.Enums;
using Flagship.Hit;
using Flagship.Logger;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Flagship.Model;
using Newtonsoft.Json.Serialization;

namespace Flagship.Tests.Api
{
    [TestClass]
    public class TrackingManagerTest
    {
        [TestMethod]
        public void InitStrategy()
        {
            var httpClientMock = new Mock<HttpClient>();
            var fsLogManagerMock = new Mock<IFsLogManager>();

            var config = new Flagship.Config.DecisionApiConfig
            {
                LogManager = fsLogManagerMock.Object,
                TrackingManagerConfig = new Config.TrackingManagerConfig()
            };

            var trackingManager = new Flagship.Api.TrackingManager(config, httpClientMock.Object);

            var trackingManagerPrivate = new PrivateObject(trackingManager);

            object strategy = trackingManagerPrivate.Invoke("InitStrategy");

            Assert.IsInstanceOfType(strategy, typeof(BatchingPeriodicCachingStrategy));

            config.TrackingManagerConfig = new Config.TrackingManagerConfig(CacheStrategy.PERIODIC_CACHING);

            strategy = trackingManagerPrivate.Invoke("InitStrategy");

            Assert.IsInstanceOfType(strategy, typeof(BatchingPeriodicCachingStrategy));

            config.TrackingManagerConfig = new Config.TrackingManagerConfig(CacheStrategy.NO_BATCHING);

            strategy = trackingManagerPrivate.Invoke("InitStrategy");

            Assert.IsInstanceOfType(strategy, typeof(NoBatchingContinuousCachingStrategy));

            config.TrackingManagerConfig = new Config.TrackingManagerConfig(CacheStrategy.CONTINUOUS_CACHING);

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
                TrackingManagerConfig = new Config.TrackingManagerConfig()
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

            // Test SendTroubleshootingHit

            var troubleshooting = new Troubleshooting();

            await trackingManager.SendTroubleshootingHit(troubleshooting);

            strategyMock.Verify(x=> x.SendTroubleshootingHit(troubleshooting), Times.Once());

            //Test AddTroubleshootingHit

            trackingManager.AddTroubleshootingHit(troubleshooting);

            strategyMock.Verify(x => x.AddTroubleshootingHit(troubleshooting), Times.Once());

            //Test

            var usageHit = new UsageHit();

            await trackingManager.SendUsageHit(usageHit);

            strategyMock.Verify(x=> x.SendUsageHit(usageHit), Times.Once());    
        }

        [TestMethod]
        public async Task ActivateFlag()
        {
            var httpClientMock = new Mock<HttpClient>();
            var fsLogManagerMock = new Mock<IFsLogManager>();

            var config = new Flagship.Config.DecisionApiConfig
            {
                LogManager = fsLogManagerMock.Object,
                TrackingManagerConfig = new Config.TrackingManagerConfig()
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

        [TestMethod]
        public async Task SendBatchTest() 
        {
            var httpClientMock = new Mock<HttpClient>();
            var fsLogManagerMock = new Mock<IFsLogManager>();

            var config = new Flagship.Config.DecisionApiConfig
            {
                LogManager = fsLogManagerMock.Object,
                TrackingManagerConfig = new Config.TrackingManagerConfig()
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
            
            strategyMock.Setup(x => x.SendBatch(CacheTriggeredBy.BatchLength)).Returns(Task.CompletedTask);
            strategyMock.Setup(x => x.SendTroubleshootingQueue()).Returns(Task.CompletedTask);
            strategyMock.Setup(x => x.SendUsageHitQueue()).Returns(Task.CompletedTask);

            await trackingManager.SendBatch().ConfigureAwait(false);

            strategyMock.Verify(x => x.SendBatch(CacheTriggeredBy.BatchLength), Times.Once());
            strategyMock.Verify(x => x.SendTroubleshootingQueue(), Times.Once());
            strategyMock.Verify(x => x.SendUsageHitQueue(), Times.Once());
        }

        [TestMethod]
        public async Task StartBatchingLoopTest()
        {
            var httpClientMock = new Mock<HttpClient>();
            var fsLogManagerMock = new Mock<IFsLogManager>();
            var hitCacheImplementation = new Mock<Cache.IHitCacheImplementation>();

            var config = new Flagship.Config.DecisionApiConfig
            {
                LogManager = fsLogManagerMock.Object,
                TrackingManagerConfig = new Config.TrackingManagerConfig(CacheStrategy.CONTINUOUS_CACHING,
                5, TimeSpan.FromMilliseconds(500)),
            };

            var trackingManagerMock = new Mock<Flagship.Api.TrackingManager>(config, httpClientMock.Object)
            {
                CallBase = true,
            };

            var trackingManager = trackingManagerMock.Object;

            trackingManagerMock.Setup(x => x.BatchingLoop()).Returns(Task.CompletedTask).Verifiable();

            trackingManager.StartBatchingLoop();
            trackingManager.StartBatchingLoop();

            await Task.Delay(800).ConfigureAwait(false);

            trackingManager.StopBatchingLoop();

            trackingManagerMock.Verify(x => x.BatchingLoop(), Times.Once());
        }

        [TestMethod]
        public async Task BatchingLoopTest()
        {
            var httpClientMock = new Mock<HttpClient>();
            var fsLogManagerMock = new Mock<IFsLogManager>();
            var hitCacheImplementation = new Mock<Cache.IHitCacheImplementation>();

            var config = new Flagship.Config.DecisionApiConfig
            {
                LogManager = fsLogManagerMock.Object,
                TrackingManagerConfig = new Config.TrackingManagerConfig(),
            };

            var trackingManagerMock = new Mock<Flagship.Api.TrackingManager>(config, httpClientMock.Object)
            {
                CallBase = true,
            };

            var trackingManager = trackingManagerMock.Object;

            trackingManagerMock.Setup(x => x.SendBatch(CacheTriggeredBy.Timer)).Returns(Task.CompletedTask).Verifiable();

            await trackingManager.BatchingLoop().ConfigureAwait(false);

            trackingManagerMock.Verify(x => x.SendBatch(CacheTriggeredBy.Timer), Times.Once());

        }

        [TestMethod]
        public async Task BatchingLoopDuplicateTest()
        {
            var httpClientMock = new Mock<HttpClient>();
            var fsLogManagerMock = new Mock<IFsLogManager>();
            var hitCacheImplementation = new Mock<Cache.IHitCacheImplementation>();

            var config = new Flagship.Config.DecisionApiConfig
            {
                LogManager = fsLogManagerMock.Object,
                TrackingManagerConfig = new Config.TrackingManagerConfig(),
            };

            var trackingManagerMock = new Mock<Flagship.Api.TrackingManager>(config, httpClientMock.Object)
            {
                CallBase = true,
            };

            var trackingManager = trackingManagerMock.Object;

            trackingManagerMock.Setup(x => x.SendBatch(CacheTriggeredBy.Timer)).Returns(async () =>
            {
                await Task.Delay(200).ConfigureAwait(false);
            }) ;

            _ = trackingManager.BatchingLoop();

            await Task.Delay(100).ConfigureAwait(false);

            _ = trackingManager.BatchingLoop();

            await Task.Delay(500).ConfigureAwait(false);

            trackingManagerMock.Verify(x => x.SendBatch(CacheTriggeredBy.Timer), Times.Once());

        }
        [TestMethod]
        public async Task LookupHitsAsync()
        {
            var httpClientMock = new Mock<HttpClient>();
            var fsLogManagerMock = new Mock<IFsLogManager>();
            var hitCacheImplementation = new Mock<Cache.IHitCacheImplementation>();

            var config = new Flagship.Config.DecisionApiConfig
            {
                LogManager = fsLogManagerMock.Object,
                TrackingManagerConfig = new Config.TrackingManagerConfig(),
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

            hitCacheImplementation.Setup(x => x.LookupHits()).Returns(Task.FromResult(data));

            await trackingManager.LookupHitsAsync().ConfigureAwait(false);

            hitCacheImplementation.Verify(x => x.LookupHits(), Times.Exactly(2));

            Assert.AreEqual(6,trackingManager.HitsPoolQueue.Count);
            Assert.AreEqual(1,trackingManager.ActivatePoolQueue.Count);
        }

        [TestMethod]
        public async Task LookupHitsBadFormatAsync()
        {
            var httpClientMock = new Mock<HttpClient>();
            var fsLogManagerMock = new Mock<IFsLogManager>();
            var hitCacheImplementation = new Mock<Cache.IHitCacheImplementation>();

            var config = new Config.DecisionApiConfig
            {
                LogManager = fsLogManagerMock.Object,
                TrackingManagerConfig = new Config.TrackingManagerConfig(),
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

            var hits = new Dictionary<string, HitAbstract>()
            {
                { screen.Key, screen }
            };

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
                    Data = null
                };

                data[keyValue.Key] = JObject.FromObject(hitData, jsonSerializer);
            }

            hitCacheImplementation.Setup(x => x.LookupHits()).Returns(Task.FromResult(data));

            await trackingManager.LookupHitsAsync().ConfigureAwait(false);

            hitCacheImplementation.Verify(x => x.LookupHits(), Times.Exactly(2));

            Assert.AreEqual(0, trackingManager.HitsPoolQueue.Count);
            Assert.AreEqual(0, trackingManager.ActivatePoolQueue.Count);

            hitCacheImplementation.Verify(x => x.FlushHits(new string[] { screen.Key }), Times.Once());
        }

        [TestMethod]
        public async Task LookupHitsAsyncThrowErrorTest()
        { 
            var httpClientMock = new Mock<HttpClient>();
            var fsLogManagerMock = new Mock<IFsLogManager>();
            var hitCacheImplementation = new Mock<Cache.IHitCacheImplementation>();

            var config = new Config.DecisionApiConfig
            {
                LogManager = fsLogManagerMock.Object,
                TrackingManagerConfig = new Config.TrackingManagerConfig(),
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

            var hits = new Dictionary<string, HitAbstract>()
            {
                { screen.Key, screen }
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

            var error = new Exception("Error");

            hitCacheImplementation.Setup(x => x.LookupHits()).Throws(error);

            await trackingManager.LookupHitsAsync().ConfigureAwait(false);

            hitCacheImplementation.Verify(x => x.LookupHits(), Times.Exactly(2));

            Assert.AreEqual(0, trackingManager.HitsPoolQueue.Count);
            Assert.AreEqual(0, trackingManager.ActivatePoolQueue.Count);
            fsLogManagerMock.Verify(x => x.Error(error.Message, TrackingManager.PROCESS_LOOKUP_HIT), Times.Once());
        }
    }
}

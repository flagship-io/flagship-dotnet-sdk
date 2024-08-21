using Microsoft.VisualStudio.TestTools.UnitTesting;
using Flagship.Enums;

namespace Flagship.Config.Tests
{
    [TestClass()]
    public class TrackingManagerConfigTests
    {
        [TestMethod()]
        public void TrackingManagerConfigTest()
        {
            var trackingManagerConfig = new TrackingManagerConfig();

            Assert.AreEqual(Enums.CacheStrategy.PERIODIC_CACHING, trackingManagerConfig.CacheStrategy);
            Assert.AreEqual(Constants.DEFAULT_POOL_MAX_SIZE, trackingManagerConfig.PoolMaxSize);
            Assert.AreEqual(TimeSpan.FromMilliseconds(Constants.DEFAULT_BATCH_TIME_INTERVAL), trackingManagerConfig.BatchIntervals);

            trackingManagerConfig = new TrackingManagerConfig(CacheStrategy.NO_BATCHING);
            Assert.AreEqual(CacheStrategy.NO_BATCHING, trackingManagerConfig.CacheStrategy);
             
            trackingManagerConfig = new TrackingManagerConfig(CacheStrategy.CONTINUOUS_CACHING, 45);
            Assert.AreEqual(Enums.CacheStrategy.CONTINUOUS_CACHING, trackingManagerConfig.CacheStrategy);
            Assert.AreEqual(45L, trackingManagerConfig.PoolMaxSize);

        }
    }
}
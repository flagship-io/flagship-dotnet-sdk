using Microsoft.VisualStudio.TestTools.UnitTesting;
using Flagship.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

            Assert.AreEqual(Enums.CacheStrategy.CONTINUOUS_CACHING, trackingManagerConfig.CacheStrategy);
            Assert.AreEqual(Constants.DEFAULT_BATCH_LENGTH, trackingManagerConfig.PoolMaxSize);
            Assert.AreEqual(TimeSpan.FromMilliseconds(Constants.DEFAULT_BATCH_TIME_INTERVAL), trackingManagerConfig.BatchIntervals);

            trackingManagerConfig = new TrackingManagerConfig(CacheStrategy.NO_BATCHING);
            Assert.AreEqual(CacheStrategy.NO_BATCHING, trackingManagerConfig.CacheStrategy);
             
            trackingManagerConfig = new TrackingManagerConfig(CacheStrategy.PERIODIC_CACHING, 45);
            Assert.AreEqual(Enums.CacheStrategy.PERIODIC_CACHING, trackingManagerConfig.CacheStrategy);
            Assert.AreEqual(45L, trackingManagerConfig.PoolMaxSize);

        }
    }
}
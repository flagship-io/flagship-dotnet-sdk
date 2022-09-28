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

            Assert.AreEqual(Enums.BatchStrategy.CONTINUOUS_CACHING, trackingManagerConfig.BatchStrategy);
            Assert.AreEqual(Constants.DEFAULT_BATCH_LENGTH, trackingManagerConfig.BatchLength);
            Assert.AreEqual(TimeSpan.FromMilliseconds(Constants.DEFAULT_BATCH_TIME_INTERVAL), trackingManagerConfig.BatchIntervals);

            trackingManagerConfig = new TrackingManagerConfig(BatchStrategy.NO_BATCHING);
            Assert.AreEqual(BatchStrategy.NO_BATCHING, trackingManagerConfig.BatchStrategy);
             
            trackingManagerConfig = new TrackingManagerConfig(BatchStrategy.PERIODIC_CACHING, 45);
            Assert.AreEqual(Enums.BatchStrategy.PERIODIC_CACHING, trackingManagerConfig.BatchStrategy);
            Assert.AreEqual(45L, trackingManagerConfig.BatchLength);

        }
    }
}
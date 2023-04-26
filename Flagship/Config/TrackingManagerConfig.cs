using Flagship.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flagship.Config
{
    public class TrackingManagerConfig : ITrackingManagerConfig
    {

        public TimeSpan BatchIntervals { get; set; }
        public int PoolMaxSize { get; set; }
        public CacheStrategy CacheStrategy { get; set; }

        public TrackingManagerConfig(CacheStrategy cacheStrategy, int poolMaxSize, TimeSpan batchIntervals)
        {
            BatchIntervals = batchIntervals;
            PoolMaxSize = poolMaxSize;
            CacheStrategy = cacheStrategy;
        }

        public TrackingManagerConfig():this(CacheStrategy.PERIODIC_CACHING,
            Constants.DEFAULT_POOL_MAX_SIZE, TimeSpan.FromMilliseconds(Constants.DEFAULT_BATCH_TIME_INTERVAL))
        {
        }

        public TrackingManagerConfig(CacheStrategy batchStrategy):this(batchStrategy, Constants.DEFAULT_POOL_MAX_SIZE, TimeSpan.FromMilliseconds(Constants.DEFAULT_BATCH_TIME_INTERVAL))
        {
           
        }

        public TrackingManagerConfig(CacheStrategy batchStrategy, int poolMaxSize) : this(batchStrategy, poolMaxSize, TimeSpan.FromMilliseconds(Constants.DEFAULT_BATCH_TIME_INTERVAL))
        {

        }
    }
}

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
        public CacheStrategy CacheStrategy { get;}

        public TrackingManagerConfig(CacheStrategy batchStrategy, int batchLength, TimeSpan batchIntervals)
        {
            BatchIntervals = batchIntervals;
            PoolMaxSize = batchLength;
            CacheStrategy = batchStrategy;
        }

        public TrackingManagerConfig():this(CacheStrategy.CONTINUOUS_CACHING,
            Constants.DEFAULT_BATCH_LENGTH, TimeSpan.FromMilliseconds(Constants.DEFAULT_BATCH_TIME_INTERVAL))
        {
        }

        public TrackingManagerConfig(CacheStrategy batchStrategy):this(batchStrategy, Constants.DEFAULT_BATCH_LENGTH, TimeSpan.FromMilliseconds(Constants.DEFAULT_BATCH_TIME_INTERVAL))
        {
           
        }

        public TrackingManagerConfig(CacheStrategy batchStrategy, int batchLength) : this(batchStrategy, batchLength, TimeSpan.FromMilliseconds(Constants.DEFAULT_BATCH_TIME_INTERVAL))
        {

        }
    }
}

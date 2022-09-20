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
        public int BatchLength { get; set; }
        public BatchStrategy BatchStrategy { get; set; }

        public TrackingManagerConfig(BatchStrategy batchStrategy, int batchLength, TimeSpan batchIntervals)
        {
            BatchIntervals = batchIntervals;
            BatchLength = batchLength;
            BatchStrategy = batchStrategy;
        }

        public TrackingManagerConfig():this(BatchStrategy.CONTINUOUS_CACHING,
            Constants.DEFAULT_BATCH_LENGTH, TimeSpan.FromMilliseconds(Constants.DEFAULT_BATCH_TIME_INTERVAL))
        {
        }

        public TrackingManagerConfig(BatchStrategy batchStrategy):this(batchStrategy, Constants.DEFAULT_BATCH_LENGTH, TimeSpan.FromMilliseconds(Constants.DEFAULT_BATCH_TIME_INTERVAL))
        {
           
        }

        public TrackingManagerConfig(BatchStrategy batchStrategy, int batchLength) : this(batchStrategy, batchLength, TimeSpan.FromMilliseconds(Constants.DEFAULT_BATCH_TIME_INTERVAL))
        {

        }
    }
}

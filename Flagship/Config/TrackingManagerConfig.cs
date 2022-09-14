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

        public TrackingManagerConfig(TimeSpan? batchIntervals, BatchStrategy batchStrategy = BatchStrategy.CONTINUOUS_CACHING, int batchLength = Constants.DEFAULT_BATCH_LENGTH)
        {
            BatchIntervals = batchIntervals?? TimeSpan.FromMilliseconds(Constants.DEFAULT_BATCH_TIME_INTERVAL);
            BatchLength = batchLength;
            BatchStrategy = batchStrategy;
        }
    }
}

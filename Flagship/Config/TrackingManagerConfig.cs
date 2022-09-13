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
        public int BatchIntervals { get; set; }
        public int BatchLength { get; set; }
        public BatchStrategy BatchStrategy { get; set; }

        public TrackingManagerConfig(BatchStrategy batchStrategy = BatchStrategy.CONTINUOUS_CACHING, int batchIntervals = Constants.DEFAULT_BATCH_TIME_INTERVAL, int batchLength = Constants.DEFAULT_BATCH_LENGTH)
        {
            BatchIntervals = batchIntervals;
            BatchLength = batchLength;
            BatchStrategy = batchStrategy;
        }
    }
}

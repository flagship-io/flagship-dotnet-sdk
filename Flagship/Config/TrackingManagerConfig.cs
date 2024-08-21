using System;
using Flagship.Enums;

namespace Flagship.Config
{
    /// <summary>
    /// Represents the configuration for the tracking manager.
    /// </summary>
    public class TrackingManagerConfig : ITrackingManagerConfig
    {
        public TimeSpan BatchIntervals { get; set; }

        public int PoolMaxSize { get; set; }

        public CacheStrategy CacheStrategy { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TrackingManagerConfig"/> class with the specified cache strategy, pool max size, and batch intervals.
        /// </summary>
        /// <param name="cacheStrategy">The cache strategy for tracking data.</param>
        /// <param name="poolMaxSize">The maximum size of the tracking pool.</param>
        /// <param name="batchIntervals">The batch intervals for sending tracked data.</param>
        public TrackingManagerConfig(
            CacheStrategy cacheStrategy,
            int poolMaxSize,
            TimeSpan batchIntervals
        )
        {
            BatchIntervals = batchIntervals;
            PoolMaxSize = poolMaxSize;
            CacheStrategy = cacheStrategy;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TrackingManagerConfig"/> class with default values.
        /// </summary>
        public TrackingManagerConfig()
            : this(
                CacheStrategy.PERIODIC_CACHING,
                Constants.DEFAULT_POOL_MAX_SIZE,
                TimeSpan.FromMilliseconds(Constants.DEFAULT_BATCH_TIME_INTERVAL)
            ) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="TrackingManagerConfig"/> class with the specified batch strategy and default values for pool max size and batch intervals.
        /// </summary>
        /// <param name="batchStrategy">The batch strategy for sending tracked data.</param>
        public TrackingManagerConfig(CacheStrategy batchStrategy)
            : this(
                batchStrategy,
                Constants.DEFAULT_POOL_MAX_SIZE,
                TimeSpan.FromMilliseconds(Constants.DEFAULT_BATCH_TIME_INTERVAL)
            ) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="TrackingManagerConfig"/> class with the specified batch strategy, pool max size, and default batch intervals.
        /// </summary>
        /// <param name="batchStrategy">The batch strategy for sending tracked data.</param>
        /// <param name="poolMaxSize">The maximum size of the tracking pool.</param>
        public TrackingManagerConfig(CacheStrategy batchStrategy, int poolMaxSize)
            : this(
                batchStrategy,
                poolMaxSize,
                TimeSpan.FromMilliseconds(Constants.DEFAULT_BATCH_TIME_INTERVAL)
            ) { }
    }
}

using System;
using Flagship.Enums;

namespace Flagship.Config
{
    /// <summary>
    /// Represents the configuration of the tracking manager.
    /// </summary>
    public interface ITrackingManagerConfig
    {
        /// <summary>
        /// Gets or sets a regular interval of time to trigger batch processing
        /// </summary>
        TimeSpan BatchIntervals { get; set; }

        /// <summary>
        /// Define the maximum number of hits the pool must reach to automatically batch all hits in the pool and send them.
        /// <br/>
        /// **Note**: <br/>
        /// - Must be greater than 5 otherwise default value will be used <br/>
        /// - Having a large PoolMaxSize can lead to performance issues
        /// </summary>
        int PoolMaxSize { get; set; }

        /// <summary>
        /// Gets the strategy that will be used for hit caching.
        /// </summary>
        CacheStrategy CacheStrategy { get; }
    }
}

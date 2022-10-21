using Flagship.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flagship.Config
{
    public interface ITrackingManagerConfig
    {
        /// <summary>
        /// Define the time intervals the SDK will use to send tracking batches.
        /// </summary>
        TimeSpan BatchIntervals { get; set; }

        /// <summary>
        /// Define the maximum number of tracking hit that each batch can contain.
        /// </summary>
        int BatchLength { get; set; }
        BatchStrategy BatchStrategy { get; }

    }
}

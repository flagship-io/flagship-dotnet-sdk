using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flagship.Enums
{
    /// <summary>
    /// Represents the status of the flags in the Flagship SDK.
    /// </summary>
    public enum FSFlagStatus
    {
        /// <summary>
        /// The flags have been successfully fetched from the API or re-evaluated in bucketing mode.
        /// </summary>
        FETCHED = 0,

        /// <summary>
        /// The flags need to be re-fetched due to a change in context, or because the flags were loaded from cache or XPC.
        /// </summary>
        FETCH_REQUIRED = 1,

        /// <summary>
        /// The flag was not found or does not exist.
        /// </summary>
        NOT_FOUND = 2,

        /// <summary>
        /// The SDK is in PANIC mode: All features are disabled except for the one to fetch flags.
        /// </summary>
        PANIC = 3,
    }
}

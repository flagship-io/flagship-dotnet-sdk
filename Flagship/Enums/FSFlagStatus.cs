﻿namespace Flagship.Enums
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
        /// The flags are currently being fetched from the API or re-evaluated in bucketing mode.
        /// </summary>
        FETCHING = 1,

        /// <summary>
        /// The flags need to be re-fetched due to a change in context, or because the flags were loaded from cache or XPC.
        /// </summary>
        FETCH_REQUIRED = 2,

        /// <summary>
        /// The SDK is in PANIC mode: All features are disabled except for the one to fetch flags.
        /// </summary>
        PANIC = 3,

        /// <summary>
        /// The flags were not found.
        /// </summary>
        NOT_FOUND = 4,
    }
}

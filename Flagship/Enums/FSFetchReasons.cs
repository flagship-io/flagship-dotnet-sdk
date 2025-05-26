namespace Flagship.Enums
{
    /// <summary>
    /// Enum representing the reasons for fetching Flags.
    /// </summary>
    public enum FSFetchReasons
    {
        /// <summary>
        /// Indicates that a context has been updated or changed.
        /// </summary>
        VISITOR_CONTEXT_UPDATED = 0,

        /// <summary>
        /// Indicates that the XPC method 'authenticate' has been called.
        /// </summary>
        VISITOR_AUTHENTICATED = 1,

        /// <summary>
        /// Indicates that the XPC method 'unauthenticate' has been called.
        /// </summary>
        VISITOR_UNAUTHENTICATED = 2,

        /// <summary>
        /// Indicates that fetching flags has failed.
        /// </summary>
        FLAGS_FETCHING_ERROR = 3,

        /// <summary>
        /// Indicates that flags have been fetched from the cache.
        /// </summary>
        FLAGS_FETCHED_FROM_CACHE = 4,

        /// <summary>
        /// Indicates that the visitor has been created.
        /// </summary>
        FLAGS_NEVER_FETCHED = 5,

        /// <summary>
        /// Indicates that there is no specific reason for fetching flags.
        /// </summary>
        NONE = 6,
    }
}

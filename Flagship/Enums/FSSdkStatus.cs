namespace Flagship.Enums
{
    /// <summary>
    /// Enum representing the status of the Flagship SDK.
    /// </summary>
    public enum FSSdkStatus
    {
        /// <summary>
        /// It is the default initial status. This status remains until the sdk has been initialized successfully.
        /// </summary>
        SDK_NOT_INITIALIZED = 0,

        /// <summary>
        /// The SDK is currently initializing.
        /// </summary>
        SDK_INITIALIZING = 1,

        /// <summary>
        /// Flagship SDK is ready but is running in Panic mode: All features are disabled except the one which refresh this status.
        /// </summary>
        SDK_PANIC = 2,

        /// <summary>
        /// The Initialization is done, and Flagship SDK is ready to use.
        /// </summary>
        SDK_INITIALIZED = 3,
    }
}

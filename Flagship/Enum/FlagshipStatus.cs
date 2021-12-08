using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flagship.Enum
{
    public enum FlagshipStatus
    {
        /// <summary>
        ///  It is the default initial status. This status remains until the sdk has been initialized successfully.
        ///  Flagship SDK has not been started or initialized successfully.
        ///  @deprecated in v2, use FlagshipStatus::NOT_INITIALIZED instead of
        /// </summary>
        NOT_INITIALIZED = 0,

        ///<summary>
        ///Flagship SDK is starting.
        /// </summary>
        STARTING = 1,

        ///<summary>
        ///Flagship SDK has been started successfully but is still polling campaigns.
        /// </summary>
        POLLING = 2,

        /// <summary>
        /// Flagship SDK is ready but is running in Panic mode: All features are disabled except the one which refresh this status.
        /// </summary>
        READY_PANIC_ON = 3,

        /// <summary>
        /// Flagship SDK is ready to use.
        /// </summary>
        READY = 4,
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        UPDATE_CONTEXT = 0,

        /// <summary>
        /// Indicates that the XPC method 'authenticate' has been called.
        /// </summary>
        AUTHENTICATE = 1,

        /// <summary>
        /// Indicates that the XPC method 'unauthenticate' has been called.
        /// </summary>
        UNAUTHENTICATE = 2,

        /// <summary>
        /// Indicates that fetching flags has failed.
        /// </summary>
        FETCH_ERROR = 3,

        /// <summary>
        /// Indicates that flags have been fetched from the cache.
        /// </summary>
        READ_FROM_CACHE = 4,

        /// <summary>
        /// Indicates that the visitor has been created.
        /// </summary>
        VISITOR_CREATED = 5,

        /// <summary>
        /// Indicates that there is no specific reason for fetching flags.
        /// </summary>
        NONE = 6
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flagship.FsFlag
{
    /// <summary>
    /// An interface to get information about the flag that has been exposed.
    /// </summary>
    public interface IExposedFlag
    {
        /// <summary>
        /// Get the flag key
        /// </summary>
        string Key { get; }

        /// <summary>
        /// Get the flag value
        /// </summary>
        object Value { get; }

        /// <summary>
        /// Get the flag default value
        /// </summary>
        object DefaultValue { get; }

        /// <summary>
        /// Get the flag metadata
        /// </summary>
        IFlagMetadata Metadata { get; }

    }
}

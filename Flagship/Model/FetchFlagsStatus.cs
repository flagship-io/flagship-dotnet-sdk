using Flagship.Enums;

namespace Flagship.Model
{
    /// <summary>
    /// Represents the status of visitor fetch for flag data.
    /// </summary>
    internal class FlagsStatus : IFlagsStatus
    {
        /// <summary>
        /// The new status of the flags fetch.
        /// </summary>
        public FSFlagStatus Status { get; set; }

        /// <summary>
        /// The reason for the status change.
        /// </summary>
        public FSFetchReasons Reason { get; set; }
    }
}

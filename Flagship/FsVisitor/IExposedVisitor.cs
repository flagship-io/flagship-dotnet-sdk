using System.Collections.Generic;

namespace Flagship.FsVisitor
{
    /// <summary>
    /// An interface to get information about the visitor to whom the flag has been exposed
    /// </summary>
    public interface IExposedVisitor
    {
        /// <summary>
        /// The key of flag
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Visitor anonymous id
        /// </summary>
        string AnonymousId { get; }

        /// <summary>
        /// Visitor context
        /// </summary>
        IDictionary<string, object> Context { get; }
    }
}

using System.Collections.Generic;
using Flagship.Config;
using Flagship.Delegate;
using Flagship.FsFlag;
using Flagship.Model;

namespace Flagship.FsVisitor
{
    public interface IVisitor : IVisitorCore
    {
        string VisitorId { get; set; }

        string AnonymousId { get; }

        /// <summary>
        /// Return True if the visitor has consented for private data usage, otherwise return False.
        /// </summary>
        bool HasConsented { get; }

        /// <summary>
        /// Set if visitor has consented for protected data usage.
        /// </summary>
        /// <param name="hasConsented">Set visitor consent for private data usage. When false some features will be deactivated.</param>
        void SetConsent(bool hasConsented);
        FlagshipConfig Config { get; }

        IDictionary<string, object> Context { get; }

        /// <summary>
        /// Return a Flag object by its key. If no flag match the given key an empty flag will be returned. Call Exists to check if the flag has been found.
        /// </summary>
        /// <param name="key">key associated to the flag.</param>
        /// <param name="defaultValue">flag default value.</param>
        /// <returns></returns>
        IFlag GetFlag(string key);

        /// <summary>
        /// Returns a collection of all flags fetched for the visitor.
        /// </summary>
        /// <returns></returns>
        IFlagCollection GetFlags();

        /// <summary>
        /// The fetch status of the flags.
        /// </summary>
        IFlagsStatus FlagsStatus { get; }

        /// <summary>
        /// This event is triggered when the fetch flags status changes.
        /// </summary>
        event OnFlagStatusChangedDelegate OnFlagsStatusChanged;

        /// <summary>
        /// This event is triggered when the fetch flags status is set to FETCH_REQUIRED.
        /// </summary>
        event OnFlagStatusFetchRequiredDelegate OnFlagStatusFetchRequired;

        /// <summary>
        /// This event is triggered when the fetch flags status is set to FETCHED.
        /// </summary>
        event OnFlagStatusFetchedDelegate OnFlagStatusFetched;
    }
}

using Flagship.Config;
using Flagship.Delegate;
using Flagship.FsFlag;
using Flagship.Hit;
using Flagship.Model;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flagship.FsVisitor
{
    internal interface IVisitor : IVisitorCore
    {
        string VisitorId { get; set; }

        string AnonymousId { get; }

        ICollection<FlagDTO> Flags { get; }

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
        IFlag<string> GetFlag(string key, string defaultValue);

        /// <summary>
        /// Return a Flag object by its key. If no flag match the given key an empty flag will be returned. Call Exists to check if the flag has been found.
        /// </summary>
        /// <param name="key">key associated to the flag.</param>
        /// <param name="defaultValue">flag default value.</param>
        /// <returns></returns>
        IFlag<long> GetFlag(string key, long defaultValue);

        /// <summary>
        /// Return a Flag object by its key. If no flag match the given key an empty flag will be returned. Call Exists to check if the flag has been found.
        /// </summary>
        /// <param name="key">key associated to the flag.</param>
        /// <param name="defaultValue">flag default value.</param>
        /// <returns></returns>
        IFlag<bool> GetFlag(string key, bool defaultValue);

        /// <summary>
        /// Return a Flag object by its key. If no flag match the given key an empty flag will be returned. Call Exists to check if the flag has been found.
        /// </summary>
        /// <param name="key">key associated to the flag.</param>
        /// <param name="defaultValue">flag default value.</param>
        /// <returns></returns>
        IFlag<JObject> GetFlag(string key, JObject defaultValue);

        /// <summary>
        /// Return a Flag object by its key. If no flag match the given key an empty flag will be returned. Call Exists to check if the flag has been found.
        /// </summary>
        /// <param name="key">key associated to the flag.</param>
        /// <param name="defaultValue">flag default value.</param>
        /// <returns></returns>
        IFlag<JArray> GetFlag(string key, JArray defaultValue);

        /// <summary>
        /// The fetch status of the flags.
        /// </summary>
        IFetchFlagsStatus FetchFlagsStatus { get; }

        /// <summary>
        /// This event is triggered when the fetch flags status changes.
        /// </summary>
        event onFetchFlagsStatusChangedDelegate OnFetchFlagsStatusChanged;


    }
}

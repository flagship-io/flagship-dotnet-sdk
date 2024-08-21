using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Flagship.Cache
{
    public interface IVisitorCacheImplementation
    {
        /// <summary>
        ///
        /// </summary>
        TimeSpan? LookupTimeout { get; set; }

        /// <summary>
        /// This method is called when the SDK needs to cache visitor information in your database.
        /// </summary>
        /// <param name="visitorId">Visitor ID</param>
        /// <param name="data">Visitor data JSON</param>
        Task CacheVisitor(string visitorId, JObject data);

        /// <summary>
        /// This method is called when the SDK needs to get the visitor information corresponding to visitor ID from your database.
        /// </summary>
        /// <param name="visitorId">Visitor ID</param>
        /// <returns></returns>
        Task<JObject> LookupVisitor(string visitorId);

        /// <summary>
        /// This method is called when the SDK needs to erase the visitor information corresponding to visitor ID in your database.
        /// </summary>
        /// <param name="visitorId">Visitor ID</param>
        Task FlushVisitor(string visitorId);
    }
}

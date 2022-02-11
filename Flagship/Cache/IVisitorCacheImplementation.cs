using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        Task CacheVisitor(string visitorId, string data);

        /// <summary>
        /// This method is called when the SDK needs to get the visitor information corresponding to visitor ID from your database.
        /// </summary>
        /// <param name="visitorId">Visitor ID</param>
        /// <returns></returns>
        Task<string> LookupVisitor(string visitorId);

        /// <summary>
        /// This method is called when the SDK needs to erase the visitor information corresponding to visitor ID in your database.
        /// </summary>
        /// <param name="visitorId">Visitor ID</param>
        Task FlushVisitor(string visitorId);
    }
}

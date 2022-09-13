using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flagship.Cache 
{
    public interface IHitCacheImplementation
    {
        /// <summary>
        /// 
        /// </summary>
        TimeSpan? LookupTimeout { get; set; }
        /// <summary>
        /// This method will be called to cache visitor hits when a hit has failed to be sent if there is no internet, there has been a timeout or if the request responded with something > 2XX.
        /// </summary>
        /// <param name="visitorId">visitor ID</param>
        /// <param name="data"></param>
        Task CacheHit(JObject data);

        /// <summary>
        /// This method will be called to load hits corresponding to visitor ID from your database and trying to send them again in the background.
        /// Note: Hits older than 4H will be ignored
        /// </summary>
        /// <param name="visitorId">visitor ID</param>
        /// <returns></returns>
        Task<JObject> LookupHits();

        /// <summary>
        /// This method will be called to erase the visitor hits cache corresponding to visitor ID from your database.
        /// </summary>
        /// <param name="visitorId">visitor ID</param>
        Task FlushHits(string[] hitKeys); 
    }
}

using Flagship.Config;
using Flagship.Hit;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Flagship.Api
{
    internal class BatchingContinuousCachingStrategy : BatchingCachingStrategyAbstract
    {
        public BatchingContinuousCachingStrategy(FlagshipConfig config, HttpClient httpClient, ref Dictionary<string, HitAbstract> hitsPoolQueue) : base(config, httpClient, ref hitsPoolQueue)
        {
        }

        public override async Task Add(HitAbstract hit)
        {
            var hitKey = $"{hit.VisitorId}:{Guid.NewGuid()}";
            hit.Key = hitKey;
            await AddHitWithKey(hitKey, hit);
            var sdkName = Assembly.GetExecutingAssembly().FullName;
            if (hit is Event eventHit && eventHit.Action == FS_CONSENT && eventHit.Label == $"{sdkName}:false")
            {
                await NotConsent(hit.VisitorId);
            }
            Logger.Log.LogInfo(Config, string.Format(HIT_ADDED_IN_QUEUE, JsonConvert.SerializeObject(hit.ToApiKeys())), ADD_HIT);
        }

        protected async Task AddHitWithKey(string key, HitAbstract hit)
        {
            HitsPoolQueue[key]= hit;
            await CacheHitAsync(HitsPoolQueue);
        }

        public override Task NotConsent(string visitorId)
        {
            var keys = HitsPoolQueue.Where(x=> !(x.Value is Event eventHit && eventHit.Action == FS_CONSENT) && x.Key.Contains(visitorId));
            var keysToFlush = new List<string>();
            foreach (var item in keys)
            {

            }
        }

        public override Task SendBatch()
        {
            throw new NotImplementedException();
        }
    }
}

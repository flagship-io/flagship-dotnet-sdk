using Newtonsoft.Json.Linq;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestQA
{
    public class FsRedisHitCache : Flagship.Cache.IHitCacheImplementation
    {
        public TimeSpan? LookupTimeout { get; set; }
        private ConnectionMultiplexer redis;
        ConfigurationOptions ConfigurationOptions { get; set; }

        public FsRedisHitCache(ConfigurationOptions configurationOptions)
        {
            ConfigurationOptions = configurationOptions;
            redis = ConnectionMultiplexer.Connect(configurationOptions);
            LookupTimeout = TimeSpan.FromSeconds(10);
        }

        public async Task CacheHit(JObject data)
        {
            var db = redis.GetDatabase(ConfigurationOptions.DefaultDatabase??-1);

            foreach (var item in data)
            {
                await db.StringSetAsync(item.Key, item.Value.ToString());
            }
        }

        public async Task FlushAllHits()
        {
            var dbName = ConfigurationOptions.DefaultDatabase ?? -1;
            var db = redis.GetDatabase(dbName);
            var endPoint = redis.GetEndPoints().First();
            var server = redis.GetServer(endPoint);
            var keys = server.Keys(dbName);

            await db.KeyDeleteAsync(keys.ToArray());
        }

        public async Task FlushHits(string[] hitKeys)
        {
            var db = redis.GetDatabase(ConfigurationOptions.DefaultDatabase ?? -1);
            foreach (var item in hitKeys)
            {
                await db.KeyDeleteAsync(item);
            }
        }

        public async Task<JObject> LookupHits()
        {
            var dbName = ConfigurationOptions.DefaultDatabase ?? -1;
            var db = redis.GetDatabase(dbName);
            var endPoint = ConfigurationOptions.EndPoints.First();
            var server = redis.GetServer(endPoint);
            var keys = server.Keys(dbName);
            var jobject = new JObject();
            foreach (var key in keys)
            {
                var value = await db.StringGetAsync(key);
                jobject[key] = JToken.Parse(value);
            }
            return jobject;

        }
    }
}

using Newtonsoft.Json.Linq;
using StackExchange.Redis;

namespace Test_asp.net_core
{
    class RedisHitCache : Flagship.Cache.IHitCacheImplementation
    {
        const string FS_HIT_PREFIX = "FS_DEFAULT_HIT_CACHE";
        private ConnectionMultiplexer _redis;
        public RedisHitCache(string host)
        {
            _redis = ConnectionMultiplexer.Connect(new ConfigurationOptions
            {
                EndPoints = { host }
            });
        }
        public TimeSpan? LookupTimeout { get; set; } = TimeSpan.FromSeconds(3);

        public async Task CacheHit(JObject data)
        {
            var db = _redis.GetDatabase();
            var localDatabaseJson = await db.StringGetAsync(FS_HIT_PREFIX);
            JObject result = data;

            if (localDatabaseJson.HasValue)
            {
                result = JObject.Parse(localDatabaseJson.ToString());
                result.Merge(data);
            }
            await db.StringSetAsync(FS_HIT_PREFIX, result.ToString());
        }

        public async Task FlushAllHits()
        {
            var db = _redis.GetDatabase();
            await db.KeyDeleteAsync(FS_HIT_PREFIX);
        }

        public async Task FlushHits(string[] hitKeys)
        {
            var db = _redis.GetDatabase();
            var localDatabaseJson = await db.StringGetAsync(FS_HIT_PREFIX);
            var localDatabase = JObject.Parse(localDatabaseJson);
            foreach (var item in hitKeys)
            {
                localDatabase.Remove(item);
            }
            db.StringSet(FS_HIT_PREFIX, localDatabase.ToString());
        }

        public async Task<JObject> LookupHits()
        {
            var db = _redis.GetDatabase();
            var data = await db.StringGetAsync(FS_HIT_PREFIX);
            var result = data.HasValue ? JObject.Parse(data.ToString()) : null;
            return result;
        }
    }
}

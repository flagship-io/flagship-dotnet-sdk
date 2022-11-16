using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Flagship.Config;
using Flagship.Hit;
using Flagship.Main;
using Newtonsoft.Json.Linq;
using StackExchange.Redis;

namespace test_last_version
{
    class Program
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
            public TimeSpan? LookupTimeout { get ; set ; } = TimeSpan.FromSeconds(3);

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
                var result = data.HasValue? JObject.Parse(data.ToString()): null;
                return result;
            }
        }
        static async Task TestCache1()
        {
            var visitor = Fs.NewVisitor("visitor_5678")
                .WithContext(new Dictionary<string, object>
                {
                    ["plan"] = "premium"
                }).Build();


            await visitor.FetchFlags();

            var flag = visitor.GetFlag("js-qa-app", "default");

            Console.WriteLine("flagValue: {0}", flag.GetValue());

            await visitor.SendHit(new Screen("Screen 1"));

            Console.WriteLine("Go offline");
            Console.ReadKey();

            await visitor.SendHit(new Screen("Screen 2"));
            await visitor.SendHit(new Event(EventCategory.ACTION_TRACKING, "event 1"));

            await visitor.GetFlag("perso_value", 1).UserExposed();

            Console.WriteLine("Go online");
            Console.ReadKey();

            await visitor.FetchFlags();

            visitor = Fs.NewVisitor("visitor_5678")
                .WithContext(new Dictionary<string, object>
                {
                    ["plan"] = "premium"
                }).IsAuthenticated(true).HasConsented(true).Build();

            await visitor.FetchFlags();

            flag = visitor.GetFlag("js-qa-app", "default");

            Console.WriteLine("flagValue: {0}", flag.GetValue());

            Console.WriteLine("update context");
            Console.ReadKey();

            visitor.UpdateContext(new Dictionary<string, object>
            {
                ["plan"] = "enterprise"
            });

            await visitor.FetchFlags();

            Console.WriteLine("flagValue: {0}", flag.GetValue());

            Console.WriteLine("SetConsent false");
            Console.ReadKey();

            visitor.SetConsent(false);

            await visitor.SendHit(new Screen("Screen 3"));

            Console.WriteLine("SetConsent true");
            Console.ReadKey();
            visitor.SetConsent(true);
            await visitor.FetchFlags();

            Console.WriteLine("Go offline");
            Console.ReadKey();

            visitor.UpdateContext(new Dictionary<string, object>
            {
                ["plan"] = "enterprise"
            });

            await visitor.FetchFlags();

            flag = visitor.GetFlag("js-qa-app", "js-qa-app");

            Console.WriteLine("flagValue: {0}", flag.GetValue());

            Console.WriteLine("Go offline");
            Console.ReadKey();

            await visitor.SendHit(new Screen("Screen 4"));

            visitor.SetConsent(false);

            Console.WriteLine("Go online");
            Console.WriteLine("Enable panic mode");
            Console.ReadKey();

            await visitor.FetchFlags();

            visitor.SetConsent(true);

            await visitor.SendHit(new Event(EventCategory.USER_ENGAGEMENT, "Event 2"));

            await visitor.SendHit(new Transaction("#12345", "affiliation")
            {
                Taxes = 19.99,
                Currency = "USD",
                CouponCode = "code",
                ItemCount = 1,
                ShippingMethod = "road",
                ShippingCosts = 5,
                PaymentMethod = "credit_card",
                TotalRevenue = 199.99
            });

            await visitor.SendHit(new Item("#12345", "product", "sku123")
            {
                Price = 199.99,
                Quantity = 1,
                Category = "test",
            });

            await visitor.SendHit(new Event(EventCategory.ACTION_TRACKING, "click")
            {
                Label = "label",
                Value = 100,
            });

            await visitor.FetchFlags();

            Console.WriteLine("Disabled panic mode");
            Console.ReadKey();

            await visitor.FetchFlags();

            flag = visitor.GetFlag("js-qa-app", "js-qa-app");

            Console.WriteLine("flagValue: {0}", flag.GetValue());
        }

        static void sendHits(int index)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, "http://localhost:3000?visitorId=" +index);
            var http = new HttpClient();
            var now = DateTime.Now;
            http.SendAsync(requestMessage).ContinueWith((res) =>
            {
                  Console.WriteLine(index +":"+ res.Result.Content.ReadAsStringAsync().Result+", duration:"+ (DateTime.Now - now).TotalMilliseconds);
                  
            });
        }

        static void Main(string[] args)
        {
            Fs.Start("", "", new DecisionApiConfig()
            {
                TrackingMangerConfig = new TrackingManagerConfig(Flagship.Enums.CacheStrategy.PERIODIC_CACHING, 5)
            });

        }
    }
}

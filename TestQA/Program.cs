using Flagship.Config;
using Flagship.Hit;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestQA
{
    class Program
    {
        class HitCache : Flagship.Cache.IHitCacheImplementation
        {
            public TimeSpan? LookupTimeout { get; set; } = TimeSpan.FromSeconds(2);

            public Task CacheHit(string visitorId, JObject data)
            {
                return Task.Run(() =>
                {
                    var check = File.Exists("cacheHit");
                    var array = new JObject();
                    if (check)
                    {
                        array = JObject.Parse(File.ReadAllText("cacheHit"));
                        var ob = array.Value<JArray>(visitorId);
                        if (ob == null)
                        {
                            array[visitorId] = new JArray(data);
                        }
                        else
                        {
                            ob.Add(data);
                        }
                        
                    }
                    else
                    {
                        array[visitorId] = new JArray(data);
                    }
                    
                    File.WriteAllText("cacheHit", array.ToString());
                }); 
            }

            public Task FlushHits(string visitorId)
            {
                return Task.Run(() =>
                {
                    var check = File.Exists("cacheHit");
                    if (!check)
                    {
                        return;
                    }
                    var file = JObject.Parse(File.ReadAllText("cacheHit"));
                    file.Remove(visitorId);
                    File.WriteAllText("cacheHit", file.ToString());
                });
            }

            public Task<JArray> LookupHits(string visitorId)
            {
                return Task.Run(() =>
                {
                    var check = File.Exists("cacheHit");
                    if (!check)
                    {
                        return null;
                    }
                    var file = JObject.Parse(File.ReadAllText("cacheHit"));
                    var array = file.Value<JArray>(visitorId);
                    file.Remove(visitorId);
                    File.WriteAllText("cacheHit", file.ToString());
                    return array;
                });
               
            }
        }

        class VisitorCache : Flagship.Cache.IVisitorCacheImplementation
        {
            public TimeSpan? LookupTimeout { get ; set ; } = TimeSpan.FromSeconds(2);

            public Task CacheVisitor(string visitorId, JObject data)
            {
                return Task.Run(() =>
                {
                    var check = File.Exists("cacheVisitor");
                    var array = new JObject();
                    if (check)
                    {
                        array = JObject.Parse(File.ReadAllText("cacheVisitor"));
                    }
                    array[visitorId] = data;
                    File.WriteAllText("cacheVisitor", array.ToString());
                });
            }

            public Task FlushVisitor(string visitorId)
            {
                return Task.Run(() =>
                {
                    var check = File.Exists("cacheVisitor");
                    if (!check)
                    {
                        return;
                    }
                    var file = JObject.Parse(File.ReadAllText("cacheVisitor"));
                    file.Remove(visitorId);
                    File.WriteAllText("cacheVisitor", file.ToString());
                });
            }

            public Task<JObject> LookupVisitor(string visitorId)
            {
                return Task.Run(() =>
                {
                    var check = File.Exists("cacheVisitor");
                    if (!check)
                    {
                        return null;
                    }
                    var file = JObject.Parse(File.ReadAllText("cacheVisitor"));
                    var ob = file.Value<JObject>(visitorId);
                    file.Remove(visitorId);
                    File.WriteAllText("cacheVisitor", file.ToString());
                    return ob;
                });
            }
        }
        static async Task TestGetFlag1()
        {
            Console.WriteLine("Test getFlag 1");
            var visitor = Flagship.Main.Flagship.NewVisitor("visitor-A")
                .WithContext(new Dictionary<string, object> {
                    ["qa_getflag"] = true
                }).Build();

            await visitor.FetchFlags();

            var flag = visitor.GetFlag("qa_flag", "default");

            Console.WriteLine("flagValue {0}", flag.GetValue());

            Console.WriteLine("Flag exist: {0}", flag.Exist);

            Console.WriteLine("metaData : {0}", flag.Metadata.ToJson());
        }

        static async Task TestGetFlag2()
        {
            Console.WriteLine("\nTest getFlag wrong type");
            var visitor = Flagship.Main.Flagship.NewVisitor("visitor-A")
                .WithContext(new Dictionary<string, object>
                {
                    ["qa_getflag"] = true
                }).Build();

            await visitor.FetchFlags();

            var flag = visitor.GetFlag("qa_flag", 10);

            Console.WriteLine("flagValue {0}", flag.GetValue());

            Console.WriteLine("Flag exist: {0}", flag.Exist);

            Console.WriteLine("metaData : {0}", flag.Metadata.ToJson());
        }

        static async Task TestGetFlag3()
        {
            Console.WriteLine("\nTest getFlag wrong key");
            var visitor = Flagship.Main.Flagship.NewVisitor("visitor-A")
                .WithContext(new Dictionary<string, object>
                {
                    ["qa_getflag"] = true
                }).Build();

            await visitor.FetchFlags();

            var flag = visitor.GetFlag("wrong", 10);

            Console.WriteLine("flagValue {0}", flag.GetValue());

            Console.WriteLine("Flag exist: {0}", flag.Exist);

            Console.WriteLine("metaData : {0}", flag.Metadata.ToJson());
        }

        static async Task TestGetFlag4()
        {
            Console.WriteLine("\nTest getFlag Original");
            var visitor = Flagship.Main.Flagship.NewVisitor("visitor-F")
                .WithContext(new Dictionary<string, object>
                {
                    ["qa_getflag"] = true
                }).Build();

            await visitor.FetchFlags();

            var flag = visitor.GetFlag("qa_flag", 10);

            Console.WriteLine("flagValue 1 {0}", flag.GetValue());

            Console.WriteLine("flagValue 1 {0}", flag.GetValue(false));

            await flag.UserExposed();


            var flag2 = visitor.GetFlag("qa_flag", "default");

            Console.WriteLine("flagValue 2 {0}", flag2.GetValue());

            var flag3 = visitor.GetFlag("qa_flag", null as string);
            Console.WriteLine("flagValue 3 {0}", flag3.GetValue());

            Console.WriteLine("metaData 3 : {0}", flag3.Metadata.ToJson());
        }

        static async Task TestXp1()
        {
            Console.WriteLine("\nvisitor created\n");

            var visitor = Flagship.Main.Flagship.NewVisitor("alias")
                .Build();

            Console.WriteLine("visitor {0}", visitor.VisitorId);
            Console.WriteLine("anonyme {0}", visitor.AnonymousId);

            await visitor.FetchFlags();

            var flag = visitor.GetFlag(".net", "default");

            Console.WriteLine("flagValue {0}", flag.GetValue());

            await visitor.SendHit(new Screen("abtastylab"));

            Console.WriteLine("\nvisitor authenticated\n");

            visitor.Authenticate("alias_01");

            Console.WriteLine("visitor {0}", visitor.VisitorId);
            Console.WriteLine("anonyme {0}", visitor.AnonymousId);

            Console.WriteLine("flagValue {0}", flag.GetValue());

            await visitor.SendHit(new Screen("abtastylab"));

            Console.WriteLine("\nvisitor Unauthenticate\n");

            visitor.Unauthenticate();

            Console.WriteLine("visitor {0}", visitor.VisitorId);
            Console.WriteLine("anonyme {0}", visitor.AnonymousId);

            Console.WriteLine("flagValue {0}", flag.GetValue());

            await visitor.SendHit(new Screen("abtastylab"));
        }

        static void TestvisitorInstance()
        {
            Console.WriteLine("\nScenario 1\n");

            var visitor = Flagship.Main.Flagship.NewVisitor("visitor_1")
                .Build();

            Console.WriteLine("visitor {0}", Flagship.Main.Flagship.Visitor?.VisitorId);

            Console.WriteLine("\nScenario 2\n");

            visitor = Flagship.Main.Flagship.NewVisitor("visitor_1")
                .Build();

            visitor = Flagship.Main.Flagship.NewVisitor("visitor_2", Flagship.Enums.InstanceType.SINGLE_INSTANCE)
                .Build();

            Console.WriteLine("visitor {0}", Flagship.Main.Flagship.Visitor?.VisitorId);

            Console.WriteLine("\nScenario 3\n");

            visitor = Flagship.Main.Flagship.NewVisitor("visitor_1")
                .Build();

            visitor = Flagship.Main.Flagship.NewVisitor("visitor_2", Flagship.Enums.InstanceType.SINGLE_INSTANCE)
                .Build();

            visitor = Flagship.Main.Flagship.NewVisitor("visitor_3", Flagship.Enums.InstanceType.SINGLE_INSTANCE)
                .Build();

            Console.WriteLine("visitor {0}", Flagship.Main.Flagship.Visitor?.VisitorId);

            Console.WriteLine("\nScenario 4\n");

            var visitor_1 = Flagship.Main.Flagship.NewVisitor("visitor_1", Flagship.Enums.InstanceType.SINGLE_INSTANCE)
                .Build();

            visitor_1.UpdateContext(new Dictionary<string, object>
            {
                ["color"] = "blue"
            });

            Console.WriteLine("visitor {0}", Flagship.Main.Flagship.Visitor.Context["color"]);

            var visitor_2 = Flagship.Main.Flagship.NewVisitor("visitor_2", Flagship.Enums.InstanceType.SINGLE_INSTANCE)
                .Build();

            Console.WriteLine("visitor {0}", Flagship.Main.Flagship.Visitor.Context.ContainsKey("color"));

            Flagship.Main.Flagship.Visitor.UpdateContext(new Dictionary<string, object>
            {
                ["color"] = "red"
            });

            Console.WriteLine("visitor 1 {0}", visitor_1.Context["color"]);
            Console.WriteLine("visitor 2 {0}", visitor_2.Context["color"]);
            Console.WriteLine("visitor {0}", Flagship.Main.Flagship.Visitor.Context["color"]);
        }

        static async Task TestQA_Report1()
        {
            var visitor = Flagship.Main.Flagship.NewVisitor("visitor_a")
                .WithContext(new Dictionary<string, object>
            {
                ["qa_report"] = true,
                ["is_net"] = true
            }).Build();

            await visitor.FetchFlags();

            var flag = visitor.GetFlag("qa_report_var", "test");

            Console.WriteLine("flagValue 1: {0}", flag.GetValue());

            await visitor.SendHit(new Screen("I LOVE QA"));

            await flag.UserExposed();

            await visitor.SendHit(new Page("I LOVE QA"));

            await flag.UserExposed();

            await visitor.SendHit(new Event(EventCategory.ACTION_TRACKING, "KP2"));
        }

        static async Task TestQA_Report2()
        {
            var visitor = Flagship.Main.Flagship.NewVisitor("zZz_visitor_zZz")
                .WithContext(new Dictionary<string, object>
                {
                    ["qa_report"] = true,
                    ["is_net"] = true
                }).Build();

            await visitor.FetchFlags();

            var flag = visitor.GetFlag("qa_report_var", "test");

            Console.WriteLine("flagValue 2: {0}", flag.GetValue());

            await visitor.SendHit(new Screen("I LOVE QA"));

            await flag.UserExposed();

            await visitor.SendHit(new Page("I LOVE QA"));

            await flag.UserExposed();

            await visitor.SendHit(new Event(EventCategory.ACTION_TRACKING, "KP2"));
        }

        static async Task TestQA_Report3()
        {
            var visitor = Flagship.Main.Flagship.NewVisitor("visitor_0_0")
                .WithContext(new Dictionary<string, object>
                {
                    ["qa_report"] = true,
                    ["is_net"] = true
                }).Build();

            await visitor.FetchFlags();

            var flag = visitor.GetFlag("qa_report_var", "test");

            Console.WriteLine("flagValue 3: {0}", flag.GetValue(false));

            await visitor.SendHit(new Screen("I LOVE QA"));

            await visitor.SendHit(new Page("I LOVE QA"));

            await visitor.SendHit(new Event(EventCategory.ACTION_TRACKING, "KP2"));
        }

        static async Task TestQA_Report4()
        {
            var visitor = Flagship.Main.Flagship.NewVisitor("visitor_B_B")
                .WithContext(new Dictionary<string, object>
                {
                    ["qa_report"] = true,
                    ["is_net"] = true
                }).Build();

            await visitor.FetchFlags();

            var flag = visitor.GetFlag("qa_report_var", "test");

            Console.WriteLine("flagValue 4: {0}", flag.GetValue());
        }

        static async Task TestQA_Report5()
        {
            var visitor = Flagship.Main.Flagship.NewVisitor("visitor_1111")
                .WithContext(new Dictionary<string, object>
                {
                    ["qa_report"] = true,
                    ["is_net"] = true
                }).Build();

            await visitor.FetchFlags();

            var flag = visitor.GetFlag("qa_report_var", "test");

            Console.WriteLine("flagValue 5: {0}", flag.GetValue());

            visitor.SetConsent(false);

            await visitor.SendHit(new Screen("I LOVE QA"));

            await flag.UserExposed();

            await visitor.SendHit(new Page("I LOVE QA"));

            await flag.UserExposed();

            await visitor.SendHit(new Event(EventCategory.ACTION_TRACKING, "KP2"));
        }

        static async Task TestQA_Report6()
        {
            var visitor = Flagship.Main.Flagship.NewVisitor("visitor_22")
                .WithContext(new Dictionary<string, object>
                {
                    ["qa_report"] = false,
                    ["is_net"] = false
                }).Build();

            await visitor.FetchFlags();

            var flag = visitor.GetFlag("qa_report_var", "test");

            Console.WriteLine("flagValue 6: {0}", flag.GetValue());

            await visitor.SendHit(new Screen("I LOVE QA"));

            await flag.UserExposed();

            await visitor.SendHit(new Page("I LOVE QA"));

            await flag.UserExposed();

            await visitor.SendHit(new Event(EventCategory.ACTION_TRACKING, "KP2"));
        }

        static async Task TestCache1()
        {
            var visitor = Flagship.Main.Flagship.NewVisitor("visitor_5678")
                .WithContext(new Dictionary<string, object> { 
                ["plan"] = "premium"
            }).Build();

            
            await visitor.FetchFlags();

            var flag = visitor.GetFlag("myAwesomeFeature", 1);

            Console.WriteLine("flagValue: {0}", flag.GetValue());

            await visitor.SendHit(new Screen("Screen 1"));

            Console.WriteLine("Go offline");
            Console.ReadKey();

            await visitor.SendHit(new Screen("Screen 2"));
            await visitor.SendHit(new Event( EventCategory.ACTION_TRACKING,"event 1"));

            await visitor.GetFlag("perso_value", 1).UserExposed();

            Console.WriteLine("Go online");
            Console.ReadKey();

            await visitor.FetchFlags();

            visitor = Flagship.Main.Flagship.NewVisitor("visitor_5678")
                .WithContext(new Dictionary<string, object>
                {
                    ["plan"] = "premium"
                }).Build();

            await visitor.FetchFlags();

            flag = visitor.GetFlag("myAwesomeFeature", 1);

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

            flag = visitor.GetFlag("myAwesomeFeature", 1);

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

            await visitor.SendHit(new Event( EventCategory.USER_ENGAGEMENT,"Event 2"));
            await visitor.FetchFlags();

            Console.WriteLine("Disabled panic mode");
            Console.ReadKey();

            await visitor.FetchFlags();

            flag = visitor.GetFlag("myAwesomeFeature", 1);

            Console.WriteLine("flagValue: {0}", flag.GetValue());
        }

        static async Task TestRealloc()
        {
            var visitor = Flagship.Main.Flagship.NewVisitor("visitor_AAAA")
                .WithContext(new Dictionary<string, object>
                {
                    ["cacheEnabled"] = true
                }).Build();

            await visitor.FetchFlags();

            var flag = visitor.GetFlag("cache", 0);

            Console.WriteLine("flagValue 1: {0}", flag.GetValue());

            var flag2 = visitor.GetFlag("cache-2", 0);

            Console.WriteLine("flagValue 2: {0}", flag2.GetValue());

            Console.WriteLine("wait 1 minute");
            Console.ReadKey();

            await visitor.FetchFlags();

            flag = visitor.GetFlag("cache", 0);

            Console.WriteLine("flagValue 1: {0}", flag.GetValue());

            flag2 = visitor.GetFlag("cache-2", 0);

            Console.WriteLine("flagValue 2: {0}", flag2.GetValue());

            Console.WriteLine("new visitor ");
            Console.ReadKey();

            visitor = Flagship.Main.Flagship.NewVisitor("visitor_BBBB")
                .WithContext(new Dictionary<string, object>
                {
                    ["cacheEnabled"] = true
                }).Build();

            await visitor.FetchFlags();

            flag = visitor.GetFlag("cache", 0);

            Console.WriteLine("flagValue 1: {0}", flag.GetValue());

            flag2 = visitor.GetFlag("cache-2", 0);

            Console.WriteLine("flagValue 2: {0}", flag2.GetValue());
        }

        static async void lunchTest()
        {
            
        }
        static void Main(string[] args)
        {
            Flagship.Main.Flagship.Start("c1ndrd07m0300ro0jf20", "QzdTI1M9iqaIhnJ66a34C5xdzrrvzq6q8XSVOsS6", 
                new BucketingConfig() { 
                HitCacheImplementation= new HitCache(),
                VisitorCacheImplementation = new VisitorCache(),
                Timeout= TimeSpan.FromSeconds(10)
            });

            Console.ReadKey();
            TestRealloc().Wait();
            Console.ReadLine();
        }
    }
}

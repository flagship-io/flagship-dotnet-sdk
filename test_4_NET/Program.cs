using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flagship.Config;
using Flagship.Hit;
using Flagship.Main;

namespace test_4_NET
{
    class Program
    {
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


        static void Main(string[] args)
        {
            Fs.Start("env_id", "api_key",
                new BucketingConfig
                {
                    //LogManager = new sentryCustomLog(),
                    LogLevel = Flagship.Enums.LogLevel.ALL,
                    Timeout = TimeSpan.FromSeconds(10),
                    PollingInterval = TimeSpan.FromSeconds(2)

                });

            Console.ReadKey();
            TestCache1().Wait();
            Console.ReadLine();
        }
    }
}

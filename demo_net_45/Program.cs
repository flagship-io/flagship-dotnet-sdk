using Flagship.Config;
using Flagship.Hit;
using Flagship.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace demo_net_45
{
    class Program
    {
        static async Task TestCache1()
        {
            var visitor = Fs.NewVisitor("visitor_F")
                .WithContext(new Dictionary<string, object>
                {
                    ["plan"] = "premium"
                }).Build();


            await visitor.FetchFlags();

            var flag = visitor.GetFlag("js-qa-app", "default");

            Console.WriteLine("flagValue: {0}", flag.GetValue());

            await visitor.SendHit(new Screen("abtastylab"));
            Console.WriteLine("sent hit screen");

            await visitor.SendHit(new Page("abtastylab"));
            Console.WriteLine("sent hit Page");

            await visitor.SendHit(new Event(EventCategory.ACTION_TRACKING, "KPI2"));
            Console.WriteLine("sent hit Event");


            await visitor.SendHit(new Transaction("#12345", "KPI1")
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
            Console.WriteLine("sent hit Transaction");

            await visitor.SendHit(new Item("#12345", "product", "sku123")
            {
                Price = 199.99,
                Quantity = 1,
                Category = "test",
            });
        }

        static void Main(string[] args)
        {
            Fs.Start("c1ndrd07m0300ro0jf20", "QzdTI1M9iqaIhnJ66a34C5xdzrrvzq6q8XSVOsS6",
                new DecisionApiConfig
                {
                    //LogManager = new sentryCustomLog(),
                    LogLevel = Flagship.Enums.LogLevel.ALL,
                    Timeout = TimeSpan.FromSeconds(10),
                    //PollingInterval = TimeSpan.FromSeconds(2)

                });

            Console.ReadKey();
            TestCache1().Wait();
            Console.ReadLine();
        }
    }
}

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flagship.Hit.Tests
{
    [TestClass()]
    public class BatchConverterTests
    {
        [TestMethod()]
        public void BatchConverterTest()
        {
            var batchConverter = new BatchConverter();

            var hits = new Collection<HitAbstract>()
            {
                new Page("home"),
                new Screen("home"),
                new Transaction("transID","aff"),
                new Item("transID", "name", "code"),
                new Event(EventCategory.USER_ENGAGEMENT, "click")
            };

            var hitJson = JArray.FromObject(hits);

            batchConverter.ReadJson(hitJson.CreateReader(), null, null, new Newtonsoft.Json.JsonSerializer());

            Assert.AreEqual(hits.Count, batchConverter.Create(null).Count);
        }
    }
}

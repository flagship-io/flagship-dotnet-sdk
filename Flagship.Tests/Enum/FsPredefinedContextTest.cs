using Microsoft.VisualStudio.TestTools.UnitTesting;
using Flagship;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flagship.Enums
{
    [TestClass()]
    public class FsPredefinedContextTest
    {
        [TestMethod()]
        public void FlagshipContextTest()
        {

            var check = FsPredefinedContext.IsPredefinedContext(FsPredefinedContext.LOCATION_CITY);
            Assert.IsTrue(check);

            check = FsPredefinedContext.IsPredefinedContext("NotExists");
            Assert.IsFalse(check);

            var type = FsPredefinedContext.GetPredefinedType(FsPredefinedContext.LOCATION_CITY);
            Assert.AreEqual("string", type);

            type = FsPredefinedContext.GetPredefinedType("NotExists");
            Assert.IsNull(type);

            check = FsPredefinedContext.CheckType(FsPredefinedContext.LOCATION_CITY, "London");
            Assert.IsTrue(check);

            check = FsPredefinedContext.CheckType(FsPredefinedContext.LOCATION_CITY, 5);
            Assert.IsFalse(check);

            check = FsPredefinedContext.CheckType(FsPredefinedContext.APP_VERSION_CODE, 1);
            Assert.IsTrue(check);

            check = FsPredefinedContext.CheckType(FsPredefinedContext.APP_VERSION_CODE, "test");
            Assert.IsFalse(check);

            check = FsPredefinedContext.CheckType(FsPredefinedContext.APP_VERSION_CODE, null);
            Assert.IsFalse(check);

            check = FsPredefinedContext.CheckType("NotExists", "test");
            Assert.IsFalse(check);

        }
    }
}
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Flagship.Enums
{
    [TestClass()]
    public class FsPredefinedContextTest
    {
        [TestMethod()]
        public void FlagshipContextTest()
        {

            var check = PredefinedContext.IsPredefinedContext(PredefinedContext.LOCATION_CITY);
            Assert.IsTrue(check);

            check = PredefinedContext.IsPredefinedContext("NotExists");
            Assert.IsFalse(check);

            var type = PredefinedContext.GetPredefinedType(PredefinedContext.LOCATION_CITY);
            Assert.AreEqual("string", type);

            type = PredefinedContext.GetPredefinedType("NotExists");
            Assert.IsNull(type);

            check = PredefinedContext.CheckType(PredefinedContext.LOCATION_CITY, "London");
            Assert.IsTrue(check);

            check = PredefinedContext.CheckType(PredefinedContext.LOCATION_CITY, 5);
            Assert.IsFalse(check);

            check = PredefinedContext.CheckType(PredefinedContext.APP_VERSION_CODE, 1);
            Assert.IsTrue(check);

            check = PredefinedContext.CheckType(PredefinedContext.APP_VERSION_CODE, "test");
            Assert.IsFalse(check);

            check = PredefinedContext.CheckType(PredefinedContext.APP_VERSION_CODE, null);
            Assert.IsFalse(check);

            check = PredefinedContext.CheckType("NotExists", "test");
            Assert.IsFalse(check);

        }
    }
}
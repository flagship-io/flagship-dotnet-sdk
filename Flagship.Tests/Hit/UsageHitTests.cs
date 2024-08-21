using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Flagship.Hit.Tests
{
    [TestClass()]
    public class UsageHitTests
    {
        [TestMethod()]
        public void UsageHitTest()
        {
            var usageHit = new UsageHit();

            Assert.AreEqual(usageHit.Type, HitType.USAGE);
        }
    }
}
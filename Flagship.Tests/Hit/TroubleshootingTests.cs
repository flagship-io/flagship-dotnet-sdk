using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Flagship.Hit.Tests
{
    [TestClass()]
    public class TroubleshootingTests
    {
        [TestMethod()]
        public void TroubleshootingTest()
        {
            var troubleshooting = new Troubleshooting();

            Assert.AreEqual(troubleshooting.Type, HitType.TROUBLESHOOTING);
        }
    }
}
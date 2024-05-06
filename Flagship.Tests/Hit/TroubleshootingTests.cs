using Microsoft.VisualStudio.TestTools.UnitTesting;
using Flagship.Hit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
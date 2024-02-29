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
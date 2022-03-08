using Microsoft.VisualStudio.TestTools.UnitTesting;
using Flagship.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flagship.Utils.Tests
{
    [TestClass()]
    public class UtilsTests
    {
        [TestMethod()]
        public void TwoDigitTest()
        {
            var valueOut  = Utils.TwoDigit(1);
            Assert.AreEqual("01", valueOut);

             valueOut = Utils.TwoDigit(15);
            Assert.AreEqual("15", valueOut);
        }

        [TestMethod()]
        public void HasSameTypeTest()
        {
            Assert.IsTrue(Utils.HasSameType(null, null));
            Assert.IsTrue(Utils.HasSameType("a", "d"));
            Assert.IsTrue(Utils.HasSameType(1, 5));
            Assert.IsFalse(Utils.HasSameType(null, "a"));
            Assert.IsFalse(Utils.HasSameType(1, null));
            Assert.IsFalse(Utils.HasSameType(1, 1.0));
        }

    }
}
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
    }
}
﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
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
            var valueOut  = Helper.TwoDigit(1);
            Assert.AreEqual("01", valueOut);

             valueOut = Helper.TwoDigit(15);
            Assert.AreEqual("15", valueOut);
        }

        [TestMethod()]
        public void HasSameTypeTest()
        {
            Assert.IsTrue(Helper.HasSameType(null, null));
            Assert.IsTrue(Helper.HasSameType("a", "d"));
            Assert.IsTrue(Helper.HasSameType(1, 5));
            Assert.IsFalse(Helper.HasSameType(null, "a"));
            Assert.IsFalse(Helper.HasSameType(1, null));
            Assert.IsFalse(Helper.HasSameType(1, 1.0));
        }

    }
}
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

        [TestMethod]
        public void IsDeepEqual_BothNull_ReturnsTrue()
        {
            IDictionary<string, object> dict1 = null;
            IDictionary<string, object> dict2 = null;

            var result = Helper.IsDeepEqual(dict1, dict2);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsDeepEqual_OneNull_ReturnsFalse()
        {
            IDictionary<string, object> dict1 = new Dictionary<string, object> { { "key1", "value1" } };
            IDictionary<string, object> dict2 = null;

            var result = Helper.IsDeepEqual(dict1, dict2);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsDeepEqual_DifferentCounts_ReturnsFalse()
        {
            IDictionary<string, object> dict1 = new Dictionary<string, object> { { "key1", "value1" } };
            IDictionary<string, object> dict2 = new Dictionary<string, object> { { "key1", "value1" }, { "key2", "value2" } };

            var result = Helper.IsDeepEqual(dict1, dict2);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsDeepEqual_DifferentKeys_ReturnsFalse()
        {
            IDictionary<string, object> dict1 = new Dictionary<string, object> { { "key1", "value1" } };
            IDictionary<string, object> dict2 = new Dictionary<string, object> { { "key2", "value1" } };

            var result = Helper.IsDeepEqual(dict1, dict2);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsDeepEqual_DifferentValues_ReturnsFalse()
        {
            IDictionary<string, object> dict1 = new Dictionary<string, object> { { "key1", "value1" } };
            IDictionary<string, object> dict2 = new Dictionary<string, object> { { "key1", "value2" } };

            var result = Helper.IsDeepEqual(dict1, dict2);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsDeepEqual_IdenticalDictionaries_ReturnsTrue()
        {
            IDictionary<string, object> dict1 = new Dictionary<string, object> { { "key1", "value1" } };
            IDictionary<string, object> dict2 = new Dictionary<string, object> { { "key1", "value1" } };

            var result = Helper.IsDeepEqual(dict1, dict2);

            Assert.IsTrue(result);
        }

    }
}
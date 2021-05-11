using System;
using Flagship.Model.Bucketing;
using Flagship.Services.Bucketing;
using Flagship.Services.Logger;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Flagship.Tests.Bucketing
{
    [TestClass]
    public class TargetingMatch
    {
        [TestMethod]
        public void TestBool()
        {
            var logger = new DefaultLogger();
            var targeting = new Flagship.Services.Bucketing.TargetingMatch(logger);

            Assert.IsTrue(targeting.MatchBool(true, TargetingOperator.EQUALS, true));
            Assert.IsTrue(targeting.MatchBool(false, TargetingOperator.EQUALS, false));
            Assert.IsFalse(targeting.MatchBool(true, TargetingOperator.EQUALS, false));
            Assert.IsFalse(targeting.MatchBool(false, TargetingOperator.EQUALS, true));

            Assert.IsFalse(targeting.MatchBool(true, TargetingOperator.NOT_EQUALS, true));
            Assert.IsFalse(targeting.MatchBool(false, TargetingOperator.NOT_EQUALS, false));
            Assert.IsTrue(targeting.MatchBool(true, TargetingOperator.NOT_EQUALS, false));
            Assert.IsTrue(targeting.MatchBool(false, TargetingOperator.NOT_EQUALS, true));

            Assert.IsFalse(targeting.MatchBool(true, TargetingOperator.NOT_CONTAINS, true), "Unhandled targeting operator should not match");
        }

        [TestMethod]
        public void TestNumber()
        {
            var logger = new DefaultLogger();
            var targeting = new Flagship.Services.Bucketing.TargetingMatch(logger);

            Assert.IsTrue(targeting.MatchNumber(1, TargetingOperator.EQUALS, 1));
            Assert.IsFalse(targeting.MatchNumber(2, TargetingOperator.EQUALS, 3));

            Assert.IsFalse(targeting.MatchNumber(1, TargetingOperator.NOT_EQUALS, 1));
            Assert.IsTrue(targeting.MatchNumber(2, TargetingOperator.NOT_EQUALS, 3));

            Assert.IsTrue(targeting.MatchNumber(2, TargetingOperator.GREATER_THAN, 1));
            Assert.IsFalse(targeting.MatchNumber(1, TargetingOperator.GREATER_THAN, 1));
            Assert.IsFalse(targeting.MatchNumber(1, TargetingOperator.GREATER_THAN, 2));

            Assert.IsTrue(targeting.MatchNumber(2, TargetingOperator.GREATER_THAN_OR_EQUALS, 1));
            Assert.IsTrue(targeting.MatchNumber(1, TargetingOperator.GREATER_THAN_OR_EQUALS, 1));
            Assert.IsFalse(targeting.MatchNumber(1, TargetingOperator.GREATER_THAN_OR_EQUALS, 2));

            Assert.IsTrue(targeting.MatchNumber(1, TargetingOperator.LOWER_THAN, 2));
            Assert.IsFalse(targeting.MatchNumber(1, TargetingOperator.LOWER_THAN, 1));
            Assert.IsFalse(targeting.MatchNumber(2, TargetingOperator.LOWER_THAN, 1));

            Assert.IsTrue(targeting.MatchNumber(1, TargetingOperator.LOWER_THAN_OR_EQUALS, 2));
            Assert.IsTrue(targeting.MatchNumber(1, TargetingOperator.LOWER_THAN_OR_EQUALS, 1));
            Assert.IsFalse(targeting.MatchNumber(2, TargetingOperator.LOWER_THAN_OR_EQUALS, 1));

            Assert.IsFalse(targeting.MatchNumber(1, TargetingOperator.CONTAINS, 1), "Unhandled targeting operator should not match");
            Assert.IsFalse(targeting.MatchNumber(1, TargetingOperator.NULL, 1), "Unhandled targeting operator should not match");
        }

        [TestMethod]
        public void TestString()
        {
            var logger = new DefaultLogger();
            var targeting = new Flagship.Services.Bucketing.TargetingMatch(logger);

            Assert.IsTrue(targeting.MatchString("a", TargetingOperator.EQUALS, "a"));
            Assert.IsTrue(targeting.MatchString("a", TargetingOperator.EQUALS, "A"));
            Assert.IsTrue(targeting.MatchString("B", TargetingOperator.EQUALS, "b"));
            Assert.IsFalse(targeting.MatchString("a", TargetingOperator.EQUALS, "b"));

            Assert.IsFalse(targeting.MatchString("a", TargetingOperator.NOT_EQUALS, "a"));
            Assert.IsFalse(targeting.MatchString("a", TargetingOperator.NOT_EQUALS, "A"));
            Assert.IsFalse(targeting.MatchString("B", TargetingOperator.NOT_EQUALS, "b"));
            Assert.IsTrue(targeting.MatchString("a", TargetingOperator.NOT_EQUALS, "b"));

            Assert.IsTrue(targeting.MatchString("bac", TargetingOperator.CONTAINS, "a"));
            Assert.IsTrue(targeting.MatchString("bac", TargetingOperator.CONTAINS, "A"));
            Assert.IsTrue(targeting.MatchString("BAC", TargetingOperator.CONTAINS, "a"));
            Assert.IsFalse(targeting.MatchString("bac", TargetingOperator.CONTAINS, "d"));

            Assert.IsFalse(targeting.MatchString("bac", TargetingOperator.NOT_CONTAINS, "a"));
            Assert.IsFalse(targeting.MatchString("bac", TargetingOperator.NOT_CONTAINS, "A"));
            Assert.IsFalse(targeting.MatchString("BAC", TargetingOperator.NOT_CONTAINS, "a"));
            Assert.IsTrue(targeting.MatchString("bac", TargetingOperator.NOT_CONTAINS, "d"));

            Assert.IsTrue(targeting.MatchString("abc", TargetingOperator.STARTS_WITH, "a"));
            Assert.IsTrue(targeting.MatchString("abc", TargetingOperator.STARTS_WITH, "A"));
            Assert.IsTrue(targeting.MatchString("ABC", TargetingOperator.STARTS_WITH, "a"));
            Assert.IsFalse(targeting.MatchString("abc", TargetingOperator.STARTS_WITH, "d"));

            Assert.IsTrue(targeting.MatchString("cba", TargetingOperator.ENDS_WITH, "a"));
            Assert.IsTrue(targeting.MatchString("cba", TargetingOperator.ENDS_WITH, "A"));
            Assert.IsTrue(targeting.MatchString("cba", TargetingOperator.ENDS_WITH, "a"));
            Assert.IsFalse(targeting.MatchString("cba", TargetingOperator.ENDS_WITH, "d"));

            Assert.IsTrue(targeting.MatchString("abc", TargetingOperator.GREATER_THAN, "a"));
            Assert.IsTrue(targeting.MatchString("b", TargetingOperator.GREATER_THAN, "A"));
            Assert.IsTrue(targeting.MatchString("B", TargetingOperator.GREATER_THAN, "a"));
            Assert.IsFalse(targeting.MatchString("a", TargetingOperator.GREATER_THAN, "a"));
            Assert.IsFalse(targeting.MatchString("a", TargetingOperator.GREATER_THAN, "b"));

            Assert.IsTrue(targeting.MatchString("abc", TargetingOperator.GREATER_THAN_OR_EQUALS, "a"));
            Assert.IsTrue(targeting.MatchString("b", TargetingOperator.GREATER_THAN_OR_EQUALS, "A"));
            Assert.IsTrue(targeting.MatchString("B", TargetingOperator.GREATER_THAN_OR_EQUALS, "a"));
            Assert.IsTrue(targeting.MatchString("a", TargetingOperator.GREATER_THAN_OR_EQUALS, "a"));
            Assert.IsFalse(targeting.MatchString("a", TargetingOperator.GREATER_THAN_OR_EQUALS, "b"));

            Assert.IsTrue(targeting.MatchString("abc", TargetingOperator.LOWER_THAN, "b"));
            Assert.IsTrue(targeting.MatchString("a", TargetingOperator.LOWER_THAN, "B"));
            Assert.IsTrue(targeting.MatchString("A", TargetingOperator.LOWER_THAN, "b"));
            Assert.IsFalse(targeting.MatchString("a", TargetingOperator.LOWER_THAN, "a"));
            Assert.IsFalse(targeting.MatchString("b", TargetingOperator.LOWER_THAN, "a"));

            Assert.IsTrue(targeting.MatchString("abc", TargetingOperator.LOWER_THAN_OR_EQUALS, "b"));
            Assert.IsTrue(targeting.MatchString("a", TargetingOperator.LOWER_THAN_OR_EQUALS, "B"));
            Assert.IsTrue(targeting.MatchString("A", TargetingOperator.LOWER_THAN_OR_EQUALS, "b"));
            Assert.IsTrue(targeting.MatchString("a", TargetingOperator.LOWER_THAN_OR_EQUALS, "a"));
            Assert.IsFalse(targeting.MatchString("b", TargetingOperator.LOWER_THAN_OR_EQUALS, "a"));

            Assert.IsFalse(targeting.MatchString("a", TargetingOperator.NULL, "a"), "Unhandled targeting operator should not match");
        }
    }
}

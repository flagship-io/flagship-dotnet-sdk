using Flagship.Model.Hits;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Flagship.Tests.Model.Hits
{
    [TestClass]
    public class EventTest
    {
        [TestMethod]
        public void TestValidate()
        {
            var hit = new Event();
            var result = hit.Validate();

            Assert.IsFalse(result.Success);
            Assert.AreEqual(1, result.Errors.Length);

            hit.Action = "action";
            result = hit.Validate();

            Assert.IsTrue(result.Success);
            Assert.AreEqual(0, result.Errors.Length);
        }
    }
}

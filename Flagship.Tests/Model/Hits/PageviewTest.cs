using Flagship.Model.Hits;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Flagship.Tests.Model.Hits
{
    [TestClass]
    public class PageviewTest
    {
        [TestMethod]
        public void TestValidate()
        {
            var hit = new Pageview();
            var result = hit.Validate();

            Assert.IsFalse(result.Success);
            Assert.AreEqual(1, result.Errors.Length);

            hit.DocumentLocation = "http://test.com";
            result = hit.Validate();

            Assert.IsTrue(result.Success);
            Assert.AreEqual(0, result.Errors.Length);
        }
    }
}

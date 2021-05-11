using Flagship.Model.Hits;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Flagship.Tests.Model.Hits
{
    [TestClass]
    public class ScreenviewTest
    {
        [TestMethod]
        public void TestValidate()
        {
            var hit = new Screenview();
            var result = hit.Validate();

            Assert.IsFalse(result.Success);
            Assert.AreEqual(1, result.Errors.Length);

            hit.PageTitle = "title";
            hit.DocumentLocation = "screen";
            result = hit.Validate();

            Assert.IsTrue(result.Success);
            Assert.AreEqual(0, result.Errors.Length);
        }
    }
}

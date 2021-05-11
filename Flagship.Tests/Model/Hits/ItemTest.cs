using Flagship.Model.Hits;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Flagship.Tests.Model.Hits
{
    [TestClass]
    public class ItemTest
    {
        [TestMethod]
        public void TestValidate()
        {
            var hit = new Item();
            var result = hit.Validate();

            Assert.IsFalse(result.Success);
            Assert.AreEqual(3, result.Errors.Length);

            hit.TransactionId = "id";
            hit.Name = "name";
            hit.Code = "code";
            result = hit.Validate();

            Assert.IsTrue(result.Success);
            Assert.AreEqual(0, result.Errors.Length);
        }
    }
}

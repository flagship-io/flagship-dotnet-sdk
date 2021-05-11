using Flagship.Model.Hits;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Flagship.Tests.Model.Hits
{
    [TestClass]
    public class TransactionTest
    {
        [TestMethod]
        public void TestValidate()
        {
            var hit = new Transaction();
            var result = hit.Validate();

            Assert.IsFalse(result.Success);
            Assert.AreEqual(2, result.Errors.Length);

            hit.Id = "id";
            hit.Affiliation = "affiliation";
            result = hit.Validate();

            Assert.IsTrue(result.Success);
            Assert.AreEqual(0, result.Errors.Length);
        }
    }
}

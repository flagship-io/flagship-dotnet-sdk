using Microsoft.VisualStudio.TestTools.UnitTesting;
using Flagship.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flagship.Model.Tests
{
    [TestClass()]
    public class DecisionResponseTests
    {
        [TestMethod()]
        public void DecisionResponseTest()
        {
            var visotoID = "visitorId";

            var decisionResponse = new DecisionResponse
            {
                VisitorID = visotoID,
                Panic = false
            };

            Assert.IsFalse(decisionResponse.Panic);
            Assert.AreEqual(visotoID, decisionResponse.VisitorID);
            Assert.IsNotNull(decisionResponse.Campaigns);
        }
    }
}
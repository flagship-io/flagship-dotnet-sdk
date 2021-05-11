using System.Collections.Generic;
using System.Threading.Tasks;
using Flagship.Tests.Utils;
using Flagship.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Flagship.Tests
{
    [TestClass]
    public class ModificationTest
    {
        const string environmentId = "env";
        const string visitorId = "123";
        const string apiKey = "api-key";

        [TestMethod]
        public async Task TestModification()
        {
            var flagshipVisitor = CreateVisitor.Create(environmentId, apiKey, visitorId, new Dictionary<string, object>(), new DecisionResponse()
            {
                VisitorID = visitorId,
                Campaigns = new HashSet<Campaign>()
                {
                    new Campaign()
                    {
                        Id = "123",
                        VariationGroupId = "vg123",
                        Variation = new Variation()
                        {
                            Id = "345",
                            Modifications = new Modifications()
                            {
                                Type = ModificationType.JSON,
                                Value = new Dictionary<string, object>()
                                {
                                    {"bool", true },
                                    {"number", 23.5 },
                                    {"string", "mystring" }
                                }
                            }
                        }
                    }
                },
                Panic = false
            });

            await flagshipVisitor.SynchronizeModifications().ConfigureAwait(false);

            var testBool = flagshipVisitor.GetModification<bool>("bool");
            Assert.IsTrue(testBool);

            var testNumber = flagshipVisitor.GetModification<double>("number");
            Assert.AreEqual(23.5, testNumber);

            var testString = flagshipVisitor.GetModification<string>("string");
            Assert.AreEqual("mystring", testString);

            var testNotExists = flagshipVisitor.GetModification<string>("notexists");
            Assert.AreEqual(null, testNotExists);

            var testNotExistsDefault = flagshipVisitor.GetModification<string>("notexists", "youpi");
            Assert.AreEqual("youpi", testNotExistsDefault);
        }


        [TestMethod]
        public async Task TestModificationPanic()
        {
            var flagshipVisitor = CreateVisitor.Create(environmentId, apiKey, visitorId, new Dictionary<string, object>(), new DecisionResponse()
            {
                VisitorID = visitorId,
                Campaigns = new HashSet<Campaign>(),
                Panic = true
            });

            await flagshipVisitor.SynchronizeModifications().ConfigureAwait(true);

            var testBool = flagshipVisitor.GetModification<bool>("bool");
            Assert.IsFalse(testBool);

            var testNumber = flagshipVisitor.GetModification<double>("number");
            Assert.AreEqual(0, testNumber);

            var testString = flagshipVisitor.GetModification<string>("string");
            Assert.AreEqual(null, testString);

            var testNotExists = flagshipVisitor.GetModification<string>("notexists");
            Assert.AreEqual(null, testNotExists);

            var testNotExistsDefault = flagshipVisitor.GetModification<string>("notexists", "youpi");
            Assert.AreEqual("youpi", testNotExistsDefault);
        }
    }
}

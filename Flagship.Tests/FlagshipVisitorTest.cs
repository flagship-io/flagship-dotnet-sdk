using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Flagship.Tests.Utils;
using Flagship;
using Flagship.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Flagship.Model.Config;
using System.Net.Http;
using Flagship.Services.ExceptionHandler;
using Flagship.Services.HitSender;
using Flagship.Model.Hits;

namespace Flagship.Tests
{
    [TestClass]
    public class FlagshipVisitorTest
    {
        const string environmentId = "env";
        const string visitorId = "123";
        const string apiKey = "api-key";

        [TestMethod]
        public async Task UpdateContextTest()
        {
            var flagshipVisitor = CreateVisitor.Create(environmentId, apiKey, visitorId, new Dictionary<string, object>(), new DecisionResponse()
            {
                VisitorID = visitorId,
                Panic = false
            });

            flagshipVisitor.UpdateContext("test", "value");

            BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
        | BindingFlags.Static;
            FieldInfo field = typeof(FlagshipVisitor).GetField("visitor", bindFlags);
            var visitor = field.GetValue(flagshipVisitor) as Visitor;

            Assert.AreEqual("value", visitor.Context["test"]);


            flagshipVisitor.UpdateContext("test", 34);
            visitor = field.GetValue(flagshipVisitor) as Visitor;

            Assert.AreEqual(34, visitor.Context["test"]);

            flagshipVisitor.UpdateContext(new Dictionary<string, object>()
            {
                { "test", 34 }
            });
            visitor = field.GetValue(flagshipVisitor) as Visitor;

            Assert.AreEqual(34, visitor.Context["test"]);
        }

        [TestMethod]
        public async Task GetAllModificationsTest()
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

            await flagshipVisitor.SynchronizeModifications();

            var modif = flagshipVisitor.GetAllModifications();

            Assert.AreEqual(3, modif.Count);

            Assert.IsTrue((bool)modif["bool"].Value);
            Assert.AreEqual("123", modif["bool"].CampaignId);
        }

        [TestMethod]
        public async Task GetModificationInfoTest()
        {
            var campaign = new Campaign()
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
            };
            var flagshipVisitor = CreateVisitor.Create(environmentId, apiKey, visitorId, new Dictionary<string, object>(), new DecisionResponse()
            {
                VisitorID = visitorId,
                Campaigns = new HashSet<Campaign>()
                {
                    campaign
                },
                Panic = false
            });

            await flagshipVisitor.SynchronizeModifications();

            var modif = flagshipVisitor.GetModificationInfo("string");

            Assert.AreEqual(campaign.Id, modif.CampaignID);
            Assert.AreEqual(campaign.VariationGroupId, modif.VariationGroupID);
            Assert.AreEqual(campaign.Variation.Id, modif.VariationID);
            Assert.AreEqual(campaign.Variation.Reference, modif.IsReference);
        }

        [TestMethod]
        public async Task TestSendHit()
        {
            var httpHandler = new TestHttpHandler();
            var httpClient = new HttpClient(httpHandler);
            var flagshipVisitor = CreateVisitor.Create(environmentId, apiKey, visitorId, new Dictionary<string, object>(), new DecisionResponse()
            {
                VisitorID = visitorId,
                Panic = true
            }, httpClient);

            await flagshipVisitor.SynchronizeModifications().ConfigureAwait(true);
            await flagshipVisitor.SendHit(new Transaction
            {
                Id = "tid",
                Affiliation = "affiliation"
            }).ConfigureAwait(true);

            Assert.AreEqual("", httpHandler.Content);
            
            flagshipVisitor = CreateVisitor.Create(environmentId, apiKey, visitorId, new Dictionary<string, object>(), new DecisionResponse()
            {
                VisitorID = visitorId,
                Panic = false
            }, httpClient);

            await flagshipVisitor.SendHit(new Transaction
            {
                Id = "tid",
                Affiliation = "affiliation"
            }).ConfigureAwait(true);

            Assert.AreNotEqual("", httpHandler.Content);
        }
    }
}

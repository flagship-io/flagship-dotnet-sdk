using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Flagship.Api;
using Flagship.Config;
using Flagship.Decision;

namespace Flagship.FsVisitor.Tests
{
    [TestClass()]
    public class VisitorBuilderTests
    {
        private string visitorId = "visitorId";
        private VisitorBuilder Builder;
     
        [TestMethod()]
        public void BuildTest()
        {
            var config = new DecisionApiConfig();
            var decisionManager = new Mock<IDecisionManager>().Object;
            var trackingManager = new Mock<ITrackingManager>().Object;
            var configManager = new ConfigManager(config, decisionManager, trackingManager);
            
            Builder = VisitorBuilder.Builder(configManager, visitorId, true);

            var visitor = Builder.Build();

            Assert.AreEqual(visitor.VisitorId, visitorId);
            Assert.AreEqual(true, visitor.HasConsented);

            Assert.IsNull(Main.Fs.Visitor);

            var context = new Dictionary<string, object>()
            {
                ["key1"] = "value1",
            };

            var context2 = new Dictionary<string, object>()
            {
                ["key1"] = true,
                ["key3"] = new object()
            };

            Builder.SetContext(context2);
            visitor = Builder.Build();
            Assert.AreEqual(4, visitor.Context.Count);

            var context3 = new Dictionary<string, object>()
            {
                ["key1"] = 1,
            };

            Builder.SetContext(context3);
            visitor = Builder.Build();
            Assert.AreEqual(4, visitor.Context.Count);


            Builder = VisitorBuilder.Builder(configManager, visitorId, true);
            Builder.SetShouldSaveInstance(true);
            visitor = Builder.Build();
            Assert.AreEqual(visitor, Flagship.Main.Fs.Visitor);

            Builder = VisitorBuilder.Builder(configManager, visitorId, true);
            Builder.SetIsAuthenticated(true);
            visitor = Builder.Build();
            Assert.AreNotEqual(visitor.AnonymousId, null);
        }

     

    }
}
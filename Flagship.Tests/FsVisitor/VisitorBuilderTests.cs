using Microsoft.VisualStudio.TestTools.UnitTesting;
using Flagship.FsVisitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;

namespace Flagship.FsVisitor.Tests
{
    [TestClass()]
    public class VisitorBuilderTests
    {
        private string visitorId = "visitorId";
        private VisitorBuilder Builder;
        public VisitorBuilderTests()
        {
            var configManager = new Mock<Flagship.Config.IConfigManager>();
            Builder = VisitorBuilder.Builder(configManager.Object, visitorId);
        }
        [TestMethod()]
        public void BuildTest()
        { 
            var visitor = Builder.Build();

            Assert.AreEqual(visitor.VisitorId, visitorId);
            Assert.AreEqual(true, visitor.HasConsented);

            var context = new Dictionary<string, string>()
            {
                ["key1"] = "value1",
            };

            Builder.HasConsented(false).IsAuthenticated(false).Context(context);
            visitor = Builder.Build();

            Assert.AreEqual(1, visitor.Context.Count);
            Assert.IsFalse(visitor.HasConsented);

            var context2 = new Dictionary<string, bool>()
            {
                ["key1"] = true,
            };

            Builder.Context(context2);
            visitor = Builder.Build();
            Assert.AreEqual(1, visitor.Context.Count);

            var context3 = new Dictionary<string, double>()
            {
                ["key1"] = 1,
            };

            Builder.Context(context3);
            visitor = Builder.Build();
            Assert.AreEqual(1, visitor.Context.Count);
        }

    }
}
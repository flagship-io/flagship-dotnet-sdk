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
     
        [TestMethod()]
        public void BuildTest()
        {
            var configManager = new Mock<Flagship.Config.IConfigManager>();
            Builder = VisitorBuilder.Builder(configManager.Object, visitorId, Enums.InstanceType.NEW_INSTANCE);

            var visitor = Builder.Build();

            Assert.AreEqual(visitor.VisitorId, visitorId);
            Assert.AreEqual(true, visitor.HasConsented);
            Assert.IsNull(Flagship.Main.Fs.Visitor);

            var context = new Dictionary<string, object>()
            {
                ["key1"] = "value1",
            };

            Builder.HasConsented(false).IsAuthenticated(false).WithContext(context);
            visitor = Builder.Build();

            Assert.AreEqual(4, visitor.Context.Count);
            Assert.IsFalse(visitor.HasConsented);

            var context2 = new Dictionary<string, object>()
            {
                ["key1"] = true,
                ["key3"] = new object()
            };

            Builder.WithContext(context2);
            visitor = Builder.Build();
            Assert.AreEqual(4, visitor.Context.Count);

            var context3 = new Dictionary<string, object>()
            {
                ["key1"] = 1,
            };

            Builder.WithContext(context3);
            visitor = Builder.Build();
            Assert.AreEqual(4, visitor.Context.Count);


            Builder = VisitorBuilder.Builder(configManager.Object, visitorId, Enums.InstanceType.SINGLE_INSTANCE);
            visitor = Builder.Build();
            Assert.AreEqual(visitor, Flagship.Main.Fs.Visitor);
        }

    }
}
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Flagship.FsVisitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Flagship.Enums;

namespace Flagship.FsVisitor.Tests
{
    [TestClass()]
    public class DefaultStrategyTests
    {
        [TestMethod()]
        public void DefaultStrategyTest()
        {

        }

        [TestMethod()]
        public void UpdateContexTest()
        {
            var fsLogManagerMock = new Mock<Flagship.Utils.IFsLogManager>();
            var config = new Flagship.Config.DecisionApiConfig()
            {
                EnvId = "envID",
                LogManager = fsLogManagerMock.Object,
            };
            var trackingManagerMock = new Mock<Flagship.Api.ITrackingManager>();
            var decisionManagerMock = new Mock<Flagship.Decision.IDecisionManager>();
            var configManager = new Flagship.Config.ConfigManager(config, decisionManagerMock.Object, trackingManagerMock.Object);

            var context = new Dictionary<string, object>()
            {
                ["key0"]=1,
            };

            var visitorDelegate = new Flagship.FsVisitor.VisitorDelegate("visitorId", false, context, false, configManager);
            
            var defaultStrategy = new DefaultStrategy(visitorDelegate);

            var newContext = new Dictionary<string, string>()
            {
                ["key1"]="value1",
                ["key2"]="value2"
            };

            defaultStrategy.UpdateContex(newContext);

            Assert.AreEqual(visitorDelegate.Context.Count, 3);

            var newContext2 = new Dictionary<string, double>()
            {
                ["key3"] = 5,
                ["key4"] = 1
            };

            defaultStrategy.UpdateContex(newContext2);

            Assert.AreEqual(visitorDelegate.Context.Count, 5);

            var newContext3 = new Dictionary<string, bool>()
            {
                ["key5"] = true,
                ["key6"] = false
            };

            defaultStrategy.UpdateContex(newContext3);

            Assert.AreEqual(visitorDelegate.Context.Count, 7);

            defaultStrategy.UpdateContex("key1", "value3");
            Assert.AreEqual(visitorDelegate.Context["key1"], "value3");

            defaultStrategy.UpdateContex("key3", 10);
            Assert.AreEqual(10d, visitorDelegate.Context["key3"]);

            defaultStrategy.UpdateContex("key5", false);
            Assert.AreEqual(visitorDelegate.Context["key5"], false);

            var newContext4 = new Dictionary<string, object>()
            {
                ["key6"] = new object(),
            };

            defaultStrategy.UpdateContexCommon(newContext4);
            Assert.AreEqual(visitorDelegate.Context.Count, 7);
            fsLogManagerMock.Verify(x => x.Error(string.Format(Constants.CONTEXT_PARAM_ERROR, "key6"), "UpdateContex"), Times.Once());


            defaultStrategy.ClearContext();
            Assert.AreEqual(visitorDelegate.Context.Count, 0);
        }

        [TestMethod()]
        public void ClearContextTest()
        {

        }

        [TestMethod()]
        public void FetchFlagsTest()
        {

        }

        [TestMethod()]
        public void UserExposedTest()
        {

        }

        [TestMethod()]
        public void GetFlagValueTest()
        {

        }

        [TestMethod()]
        public void GetFlagMetadataTest()
        {

        }

        [TestMethod()]
        public void SendHitTest()
        {

        }
    }
}
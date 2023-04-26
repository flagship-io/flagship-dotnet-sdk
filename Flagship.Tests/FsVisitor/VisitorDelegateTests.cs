using Microsoft.VisualStudio.TestTools.UnitTesting;
using Flagship.FsVisitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using Flagship.Tests.Data;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Flagship.FsVisitor.Tests
{
    [TestClass()]
    public class VisitorDelegateTests
    {
        Mock<VisitorDelegate> visitorDelegateMock;
        string visitorId = "visitorId";
        Dictionary<string,object> context = new Dictionary<string, object>()
        {
            ["key"] = "value"
        };
        Mock<Flagship.Config.IConfigManager> configManager;
        Mock<VisitorStrategyAbstract> defaultStrategy;


        public VisitorDelegateTests()
        {
            var config = new Flagship.Config.DecisionApiConfig();
            configManager = new Mock<Flagship.Config.IConfigManager>();
            configManager.SetupGet(x=>x.Config).Returns(config);

            visitorDelegateMock = new Mock<VisitorDelegate>(new object[] { visitorId, false, context, false, configManager.Object });
            visitorDelegateMock.Setup(x=> x.GetStrategy()).CallBase();
            defaultStrategy = new Mock<VisitorStrategyAbstract>(visitorDelegateMock.Object);

            visitorDelegateMock.Setup<VisitorStrategyAbstract>(x=> x.GetStrategy()).Returns(defaultStrategy.Object);
            visitorDelegateMock.CallBase = true;
        }


        [TestMethod()]
        public void TestStrategy()
        {
            var flagship = new Mock<Flagship.Main.Fs>();
            var visitorDelegate = new VisitorDelegate(null, true, new Dictionary<string,object>(),false, configManager.Object);
            var privateVisitor = new PrivateObject(visitorDelegate);
            privateVisitor.Invoke("GetStrategy");
        }

        [TestMethod()]
        public void VisitorDelegateTest()
        {
            
            var visitor = new VisitorDelegate(visitorId, false, context, false, configManager.Object );

            Assert.IsNull(visitor.AnonymousId);
            Assert.AreEqual(visitorId, visitor.VisitorId);
            Assert.IsFalse(visitor.HasConsented);
            Assert.AreEqual(visitor.Context.Count, 4);
            Assert.AreEqual(visitor.Flags.Count, 0);

            visitor = new VisitorDelegate(null, false, context, false, configManager.Object);
            Assert.IsNotNull(visitor.VisitorId);
            Assert.AreEqual(visitor.VisitorId.Length, 19);

            visitor = new VisitorDelegate(visitorId, true, context, false, configManager.Object);
            Assert.IsNotNull(visitor.AnonymousId);
            Assert.AreEqual(visitorId, visitor.VisitorId);
            Assert.AreEqual(36,visitor.AnonymousId.Length);

        }

        [TestMethod()]
        public void VisitorDelegateTest1()
        {
            var context = new Dictionary<string, object>()
            {
                ["key"] = "value",
                ["key2"] = 4,
                ["key3"] = true
            };

            var visitor = new VisitorDelegate(visitorId, false, context, false, configManager.Object);

            Assert.AreEqual(visitor.AnonymousId, null);
            Assert.AreEqual(visitorId, visitor.VisitorId);
            Assert.IsFalse(visitor.HasConsented);
            Assert.AreEqual(visitor.Context.Count, 6);
            Assert.AreEqual(visitor.Flags.Count, 0);
        }

        [TestMethod()]
        public void ClearContextTest()
        {

            defaultStrategy.Setup(x=>x.ClearContext()).Verifiable();
            visitorDelegateMock.Object.ClearContext();
            defaultStrategy.Verify();
        }

        [TestMethod()]
        public async Task FetchFlagsTest()
        {
            defaultStrategy.Setup(x => x.FetchFlags()).Verifiable();
            await visitorDelegateMock.Object.FetchFlags().ConfigureAwait(false);
            defaultStrategy.Verify();
        }

        [TestMethod()]
        public void GetFlagTest()
        {
            visitorDelegateMock.SetupGet(x => x.Flags).Returns(CampaignsData.GetFlag());
            var flag = visitorDelegateMock.Object.GetFlag("key", "default");
            Assert.IsNotNull(flag);
            Assert.IsTrue(flag.Exists);
        }

        [TestMethod()]
        public void GetFlagTest1()
        {
            visitorDelegateMock.SetupGet(x => x.Flags).Returns(CampaignsData.GetFlag());
            var flag = visitorDelegateMock.Object.GetFlag("keyNotExist", false);
            Assert.IsNotNull(flag);
            Assert.IsFalse(flag.Exists);
        }

        [TestMethod()]
        public void GetFlagTest2()
        {
            visitorDelegateMock.SetupGet(x => x.Flags).Returns(CampaignsData.GetFlag());
            var flag = visitorDelegateMock.Object.GetFlag("keyNotExist", 32);
            Assert.IsNotNull(flag);
            Assert.IsFalse(flag.Exists);
        }

        [TestMethod()]
        public void GetFlagTest3()
        {
            visitorDelegateMock.SetupGet(x => x.Flags).Returns(CampaignsData.GetFlag());
            var flag = visitorDelegateMock.Object.GetFlag("keyNotExist", new JObject());
            Assert.IsNotNull(flag);
            Assert.IsFalse(flag.Exists);
        }

        [TestMethod()]
        public void GetFlagTest4()
        {
            visitorDelegateMock.SetupGet(x => x.Flags).Returns(CampaignsData.GetFlag());
            var flag = visitorDelegateMock.Object.GetFlag("keyNotExist", new JArray());
            Assert.IsNotNull(flag);
            Assert.IsFalse(flag.Exists);
        }

        [TestMethod()]
        public void GetFlagMetadataTest()
        {
            defaultStrategy.Setup(x => x.GetFlagMetadata(null, "key", true))
                .Returns(Flagship.FsFlag.FlagMetadata.EmptyMetadata())
                .Verifiable();
            var metadata = visitorDelegateMock.Object.GetFlagMetadata(null, "key", true);
            Assert.AreEqual(JsonConvert.SerializeObject(metadata), JsonConvert.SerializeObject(FsFlag.FlagMetadata.EmptyMetadata()));
            defaultStrategy.Verify();
        }

        [TestMethod()]
        public void GetFlagValueTest()
        {
            defaultStrategy.Setup(x => x.GetFlagValue("key", "defaultValue", null, true))
                .Returns("value")
                .Verifiable();

            var value = visitorDelegateMock.Object.GetFlagValue("key","defaultValue", null, true);
            defaultStrategy.Verify();
        }

        [TestMethod()]
        public void UserExposedTest()
        {
            defaultStrategy.Setup(x => x.UserExposed("key", "default", null))
                .Verifiable();
            visitorDelegateMock.Object.UserExposed("key", "default", null);
            defaultStrategy.Verify();
        }

        [TestMethod()]
        public async Task SendHitTest()
        {
            var screen = new Flagship.Hit.Screen("home");
            defaultStrategy.Setup(x => x.SendHit(screen))
               .Verifiable();
            await visitorDelegateMock.Object.SendHit(screen).ConfigureAwait(false);
            defaultStrategy.Verify();
        }

        [TestMethod()]
        public void UpdateContexTest()
        {
            var context = new Dictionary<string, object>();
            defaultStrategy.Setup(x => x.UpdateContext(context))
              .Verifiable();
            visitorDelegateMock.Object.UpdateContext(context);
            defaultStrategy.Verify();
        }

        [TestMethod()]
        public void UpdateContexTest1()
        {
            var context = new Dictionary<string, object>();
            defaultStrategy.Setup(x => x.UpdateContext(context))
              .Verifiable();
            visitorDelegateMock.Object.UpdateContext(context);
            defaultStrategy.Verify();
        }

        [TestMethod()]
        public void UpdateContexTest2()
        {
            var context = new Dictionary<string, object>();
            defaultStrategy.Setup(x => x.UpdateContext(context))
              .Verifiable();
            visitorDelegateMock.Object.UpdateContext(context);
            defaultStrategy.Verify();
        }

        [TestMethod()]
        public void UpdateContexTest3()
        {
            defaultStrategy.Setup(x => x.UpdateContext("key","string"))
              .Verifiable();
            visitorDelegateMock.Object.UpdateContext("key", "string");
            defaultStrategy.Verify();
        }

        [TestMethod()]
        public void UpdateContexTest4()
        {
            defaultStrategy.Setup(x => x.UpdateContext("key", 1))
             .Verifiable();
            visitorDelegateMock.Object.UpdateContext("key", 1);
            defaultStrategy.Verify();
        }

        [TestMethod()]
        public void UpdateContexTest5()
        {
            defaultStrategy.Setup(x => x.UpdateContext("key", true))
            .Verifiable();
            visitorDelegateMock.Object.UpdateContext("key", true);
            defaultStrategy.Verify();
        }

        [TestMethod()]
        public void UpdateContexCommonTest()
        {
            var context = new Dictionary<string, object>();
            defaultStrategy.Setup(x => x.UpdateContext(context))
            .Verifiable();
            visitorDelegateMock.Object.UpdateContext(context);
            defaultStrategy.Verify();
        }

        [TestMethod()]
        public void AuthenticateTest()
        {
            var visitorId = "newVisitorID";
            defaultStrategy.Setup(x => x.Authenticate(visitorId))
            .Verifiable();
            visitorDelegateMock.Object.Authenticate(visitorId);

            defaultStrategy.Verify(x=>x.Authenticate(visitorId),Times.Once());
        }

        [TestMethod()]
        public void UnauthenticateTest()
        {
            defaultStrategy.Setup(x => x.Unauthenticate())
            .Verifiable();
            visitorDelegateMock.Object.Unauthenticate();

            defaultStrategy.Verify(x => x.Unauthenticate(), Times.Once());
        }
    }
}
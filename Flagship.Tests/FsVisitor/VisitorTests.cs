using Microsoft.VisualStudio.TestTools.UnitTesting;
using Flagship.FsVisitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;

namespace Flagship.FsVisitor.Tests
{
    [TestClass()]
    public class VisitorTests
    {
        Mock<VisitorDelegateAbstract> visitorDelegateMock;
        Visitor Visitor;
        public VisitorTests()
        {
            var configManager = new Mock<Flagship.Config.IConfigManager>();
            visitorDelegateMock = new Mock<VisitorDelegateAbstract>(new object[] { "visitor_id", false, new Dictionary<string, object>(), false, configManager.Object });
            Visitor = new Visitor(visitorDelegateMock.Object);
        }

        [TestMethod()]
        public void PropertieTest()
        {
            var config = new Flagship.Config.DecisionApiConfig();
            visitorDelegateMock.SetupGet(x=>x.Config).Returns(config);
            Assert.AreEqual(config, Visitor.Config);

            var anonymousId = "anonymousId";
            visitorDelegateMock.SetupGet(x => x.AnonymousId).Returns(anonymousId);
            Assert.AreEqual(anonymousId, Visitor.AnonymousId);

            var context = new Dictionary<string, object>();
            visitorDelegateMock.SetupGet(x=>x.Context).Returns(context);
            Assert.AreEqual(context, Visitor.Context);

            var flags = new Collection<Flagship.Model.FlagDTO>();
            visitorDelegateMock.SetupGet(x=>x.Flags).Returns(flags);
            Assert.AreEqual(flags, Visitor.Flags);

            visitorDelegateMock.SetupGet(x=>x.HasConsented).Returns(true);
            Assert.AreEqual(true, Visitor.HasConsented);

            Visitor.VisitorId = "newvisitorId";

            Assert.AreEqual(visitorDelegateMock.Object.VisitorId, Visitor.VisitorId);
            
        }

        [TestMethod()]
        public void ClearContextTest()
        {
            visitorDelegateMock.Setup(x => x.ClearContext()).Verifiable();
            Visitor.ClearContext();
            visitorDelegateMock.Verify();
        }

        [TestMethod()]
        public async Task FetchFlagsTest()
        {
            visitorDelegateMock.Setup(x => x.FetchFlags()).Returns(Task.CompletedTask);
            await Visitor.FetchFlags().ConfigureAwait(false);
            visitorDelegateMock.Verify(x=>x.FetchFlags(), Times.Once());
        }

        [TestMethod()]
        public void SetConsentTest()
        {
            visitorDelegateMock.Setup(x => x.SetConsent(true)).Verifiable();
            Visitor.SetConsent(true);
            visitorDelegateMock.Verify(x=>x.SetConsent(true), Times.Once());
        }

        [TestMethod()]
        public void GetFlagTest()
        {
            var flag = new FsFlag.Flag<string>("key", visitorDelegateMock.Object, null, "string");
            visitorDelegateMock.Setup(x => x.GetFlag("key", "string")).Returns(flag);
            var resultFlag = Visitor.GetFlag("key", "string");
            Assert.AreEqual(flag, resultFlag);
            visitorDelegateMock.Verify(x => x.GetFlag("key", "string"), Times.Once());
        }

        [TestMethod()]
        public void GetFlagTest1()
        {
            var flag = new FsFlag.Flag<long>("key", visitorDelegateMock.Object, null, 2);
            visitorDelegateMock.Setup(x => x.GetFlag("key", 2)).Returns(flag);
            var resultFlag = Visitor.GetFlag("key", 2);
            Assert.AreEqual(flag, resultFlag);
            visitorDelegateMock.Verify(x => x.GetFlag("key", 2), Times.Once());
        }

        [TestMethod()]
        public void GetFlagTest2()
        {
            var flag = new FsFlag.Flag<bool>("key", visitorDelegateMock.Object, null, true);
            visitorDelegateMock.Setup(x => x.GetFlag("key", true)).Returns(flag);
            var resultFlag = Visitor.GetFlag("key", true);
            Assert.AreEqual(flag, resultFlag);
            visitorDelegateMock.Verify(x => x.GetFlag("key", true), Times.Once());
        }

        [TestMethod()]
        public void GetFlagTest3()
        {
            var defaultValue = new JArray(new object[] { "a", "b" });
            var flag = new FsFlag.Flag<JArray>("key", visitorDelegateMock.Object, null, defaultValue);
            visitorDelegateMock.Setup(x => x.GetFlag("key", defaultValue)).Returns(flag);
            var resultFlag = Visitor.GetFlag("key", defaultValue);
            Assert.AreEqual(flag, resultFlag);
            visitorDelegateMock.Verify(x => x.GetFlag("key", defaultValue), Times.Once());
        }

        [TestMethod()]
        public void GetFlagTest4()
        {
            var defaultValue = new JObject();
            var flag = new FsFlag.Flag<JObject>("key", visitorDelegateMock.Object, null, defaultValue);
            visitorDelegateMock.Setup(x => x.GetFlag("key", defaultValue)).Returns(flag);
            var resultFlag = Visitor.GetFlag("key", defaultValue);
            Assert.AreEqual(flag, resultFlag);
            visitorDelegateMock.Verify(x => x.GetFlag("key", defaultValue), Times.Once());
        }

        [TestMethod()]
        public async Task SendHitTest()
        {
            visitorDelegateMock.Setup(x => x.SendHit(It.IsAny<Flagship.Hit.Screen>())).Returns(Task.CompletedTask);
            var screen = new Hit.Screen("home");
            await Visitor.SendHit(screen).ConfigureAwait(false);
            visitorDelegateMock.Verify(x => x.SendHit(screen));
        }

        [TestMethod()]
        public void UpdateContexTest()
        {
            var newContext = new Dictionary<string, object>();
            visitorDelegateMock.Setup(x=>x.UpdateContext(newContext)).Verifiable();
            Visitor.UpdateContext(newContext);
            visitorDelegateMock.Verify();
        }

        [TestMethod()]
        public void UpdateContexTest1()
        {
            var newContext = new Dictionary<string, object>();
            visitorDelegateMock.Setup(x => x.UpdateContext(newContext)).Verifiable();
            Visitor.UpdateContext(newContext);
            visitorDelegateMock.Verify();
        }

        [TestMethod()]
        public void UpdateContexTest2()
        {
            var newContext = new Dictionary<string, object>();
            visitorDelegateMock.Setup(x => x.UpdateContext(newContext)).Verifiable();
            Visitor.UpdateContext(newContext);
            visitorDelegateMock.Verify();
        }

        [TestMethod()]
        public void UpdateContexTest3()
        {
            visitorDelegateMock.Setup(x => x.UpdateContext("key", "string")).Verifiable();
            Visitor.UpdateContext("key", "string");
            visitorDelegateMock.Verify();
        }

        [TestMethod()]
        public void UpdateContexTest4()
        {
            visitorDelegateMock.Setup(x => x.UpdateContext("key", true)).Verifiable();
            Visitor.UpdateContext("key", true);
            visitorDelegateMock.Verify();
        }

        [TestMethod()]
        public void UpdateContexTest5()
        {
            visitorDelegateMock.Setup(x => x.UpdateContext("key", 2)).Verifiable();
            Visitor.UpdateContext("key", 2);
            visitorDelegateMock.Verify();
        }

        [TestMethod()]
        public void AuthenticateTest()
        {
            var visitorId = "visitorID";
            visitorDelegateMock.Setup(x => x.Authenticate(visitorId)).Verifiable();
            Visitor.Authenticate(visitorId);
            visitorDelegateMock.Verify(x=>x.Authenticate(visitorId), Times.Once());
        }

        [TestMethod()]
        public void UnauthenticateTest()
        { 
            visitorDelegateMock.Setup(x => x.Unauthenticate()).Verifiable();
            Visitor.Unauthenticate();
            visitorDelegateMock.Verify(x => x.Unauthenticate(), Times.Once());
        }
    }
}
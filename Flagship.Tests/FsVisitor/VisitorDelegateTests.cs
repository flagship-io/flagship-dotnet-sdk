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
using Flagship.Logger;
using Flagship.Api;
using Flagship.Config;
using Flagship.Decision;
using Flagship.Hit;
using Flagship.Model;
using Flagship.Utils;
using Flagship.Tests.Helpers;
using Flagship.FsFlag;

namespace Flagship.FsVisitor.Tests
{
    [TestClass()]
    public class VisitorDelegateTests
    {
        Mock<VisitorDelegate> visitorDelegateMock;
        string visitorId = "visitorId";
        Dictionary<string, object> context = new Dictionary<string, object>()
        {
            ["key"] = "value"
        };
        Mock<ConfigManager> configManager;
        Mock<StrategyAbstract> defaultStrategy;
        Mock<ITrackingManager> trackingManagerMock = new Mock<ITrackingManager>();

        public VisitorDelegateTests()
        {
            var config = new DecisionApiConfig();
            var decisionManagerMock = new Mock<IDecisionManager>();

            configManager = new Mock<ConfigManager>(config, decisionManagerMock.Object, trackingManagerMock.Object)
            {
                CallBase = true
            };

            visitorDelegateMock = new Mock<VisitorDelegate>(new object[] { visitorId, false, context, false, configManager.Object, null });
            visitorDelegateMock.Setup(x => x.GetStrategy()).CallBase();
            defaultStrategy = new Mock<StrategyAbstract>(visitorDelegateMock.Object);

            visitorDelegateMock.Setup(x => x.GetStrategy()).Returns(defaultStrategy.Object);
            visitorDelegateMock.CallBase = true;
            visitorDelegateMock.SetupGet(x => x.Config).Returns(config);

        }


        [TestMethod()]
        public void TestStrategy()
        {
            var flagship = new Mock<Main.Fs>();
            var visitorDelegate = new VisitorDelegate(null, true, new Dictionary<string, object>(), false, configManager.Object, null);
            var getStrategy = TestHelpers.GetPrivateMethod(visitorDelegate, "GetStrategy");
            getStrategy?.Invoke(visitorDelegate, null);
        }

        [TestMethod()]
        public void VisitorDelegateTest()
        {

            var visitor = new VisitorDelegate(visitorId, false, context, false, configManager.Object, null);

            Assert.IsNull(visitor.AnonymousId);
            Assert.AreEqual(visitorId, visitor.VisitorId);
            Assert.IsFalse(visitor.HasConsented);
            Assert.AreEqual(visitor.Context.Count, 4);
            Assert.AreEqual(visitor.Flags.Count, 0);

            visitor = new VisitorDelegate(null, false, context, false, configManager.Object);
            Assert.IsNotNull(visitor.VisitorId);
            Assert.AreEqual(visitor.VisitorId.Length, 36);

            visitor = new VisitorDelegate(visitorId, true, context, false, configManager.Object);
            Assert.IsNotNull(visitor.AnonymousId);
            Assert.AreEqual(visitorId, visitor.VisitorId);
            Assert.AreEqual(36, visitor.AnonymousId.Length);
            Assert.AreEqual(Enums.FSFetchStatus.FETCH_REQUIRED, visitor.FetchFlagsStatus.Status);
            Assert.AreEqual(Enums.FSFetchReasons.VISITOR_CREATED, visitor.FetchFlagsStatus.Reason);
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

            defaultStrategy.Setup(x => x.ClearContext()).Verifiable();
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
            visitorDelegateMock.SetupGet(x => x.FetchFlagsStatus).Returns(new FetchFlagsStatus
            {
                Status = Enums.FSFetchStatus.FETCHED,
                Reason = Enums.FSFetchReasons.NONE
            });
            var flag = visitorDelegateMock.Object.GetFlag("key");
            Assert.IsNotNull(flag);
            Assert.IsTrue(flag.Exists);
        }

        [TestMethod()]
        public void GetFlagTest1()
        {
            visitorDelegateMock.SetupGet(x => x.Flags).Returns(CampaignsData.GetFlag());
            visitorDelegateMock.SetupGet(x => x.FetchFlagsStatus).Returns(new FetchFlagsStatus
            {
                Status = Enums.FSFetchStatus.FETCHED,
                Reason = Enums.FSFetchReasons.NONE
            });
            var flag = visitorDelegateMock.Object.GetFlag("keyNotExist");
            Assert.IsNotNull(flag);
            Assert.IsFalse(flag.Exists);
        }

        [TestMethod()]
        public void GetFlagTest2()
        {
            visitorDelegateMock.SetupGet(x => x.Flags).Returns(CampaignsData.GetFlag());
            visitorDelegateMock.SetupGet(x => x.FetchFlagsStatus).Returns(new FetchFlagsStatus
            {
                Status = Enums.FSFetchStatus.FETCHED,
                Reason = Enums.FSFetchReasons.NONE
            });
            var flag = visitorDelegateMock.Object.GetFlag("keyNotExist");
            Assert.IsNotNull(flag);
            Assert.IsFalse(flag.Exists);
        }

        [TestMethod()]
        public void GetFlagTest3()
        {
            visitorDelegateMock.SetupGet(x => x.Flags).Returns(CampaignsData.GetFlag());
            visitorDelegateMock.SetupGet(x => x.FetchFlagsStatus).Returns(new FetchFlagsStatus
            {
                Status = Enums.FSFetchStatus.FETCHED,
                Reason = Enums.FSFetchReasons.NONE
            });
            var flag = visitorDelegateMock.Object.GetFlag("keyNotExist");

            Assert.IsNotNull(flag);
            Assert.IsFalse(flag.Exists);
        }

        [TestMethod()]
        public void GetFlagTest4()
        {
            visitorDelegateMock.SetupGet(x => x.Flags).Returns(CampaignsData.GetFlag());
            visitorDelegateMock.SetupGet(x => x.FetchFlagsStatus).Returns(new FetchFlagsStatus
            {
                Status = Enums.FSFetchStatus.FETCHED,
                Reason = Enums.FSFetchReasons.NONE
            });
            var flag = visitorDelegateMock.Object.GetFlag("keyNotExist");

            Assert.IsNotNull(flag);
            Assert.IsFalse(flag.Exists);
        }

        [TestMethod()]
        public void GetFlagTest5()
        {
            Mock<IFsLogManager> fsLogManagerMock = new();
            visitorDelegateMock.Object.Config.LogManager = fsLogManagerMock.Object;
            visitorDelegateMock.SetupGet(x => x.Flags).Returns(CampaignsData.GetFlag());
            visitorDelegateMock.SetupGet(x => x.FetchFlagsStatus).Returns(new FetchFlagsStatus
            {
                Status = Enums.FSFetchStatus.FETCH_REQUIRED,
                Reason = Enums.FSFetchReasons.VISITOR_CREATED
            });
            var flag = visitorDelegateMock.Object.GetFlag("key");
            fsLogManagerMock.Verify(x => x.Warning(It.Is<string>(y => y.Contains("created")), "GET_FLAG"), Times.Once());

            visitorDelegateMock.SetupGet(x => x.FetchFlagsStatus).Returns(new FetchFlagsStatus
            {
                Status = Enums.FSFetchStatus.FETCH_REQUIRED,
                Reason = Enums.FSFetchReasons.UPDATE_CONTEXT
            });

            flag = visitorDelegateMock.Object.GetFlag("key");
            fsLogManagerMock.Verify(x => x.Warning(It.Is<string>(y => y.Contains("context")), "GET_FLAG"), Times.Once());

            visitorDelegateMock.SetupGet(x => x.FetchFlagsStatus).Returns(new FetchFlagsStatus
            {
                Status = Enums.FSFetchStatus.FETCH_REQUIRED,
                Reason = Enums.FSFetchReasons.AUTHENTICATE
            });

            flag = visitorDelegateMock.Object.GetFlag("key");

            fsLogManagerMock.Verify(x => x.Warning(It.Is<string>(y => y.Contains("authenticate")), "GET_FLAG"), Times.Once());

            visitorDelegateMock.SetupGet(x => x.FetchFlagsStatus).Returns(new FetchFlagsStatus
            {
                Status = Enums.FSFetchStatus.FETCH_REQUIRED,
                Reason = Enums.FSFetchReasons.UNAUTHENTICATE
            });

            flag = visitorDelegateMock.Object.GetFlag("key");

            fsLogManagerMock.Verify(x => x.Warning(It.Is<string>(y => y.Contains("unauthenticate")), "GET_FLAG"), Times.Once());

            visitorDelegateMock.SetupGet(x => x.FetchFlagsStatus).Returns(new FetchFlagsStatus
            {
                Status = Enums.FSFetchStatus.FETCH_REQUIRED,
                Reason = Enums.FSFetchReasons.FETCH_ERROR
            });

            flag = visitorDelegateMock.Object.GetFlag("key");

            fsLogManagerMock.Verify(x => x.Warning(It.Is<string>(y => y.Contains("error")), "GET_FLAG"), Times.Once());

            visitorDelegateMock.SetupGet(x => x.FetchFlagsStatus).Returns(new FetchFlagsStatus
            {
                Status = Enums.FSFetchStatus.FETCH_REQUIRED,
                Reason = Enums.FSFetchReasons.READ_FROM_CACHE
            });

            flag = visitorDelegateMock.Object.GetFlag("key");

            fsLogManagerMock.Verify(x => x.Warning(It.Is<string>(y => y.Contains("cache")), "GET_FLAG"), Times.Once());
        }

        [TestMethod()]
        public void GetFlagsTest()
        {
            var flagsCollection = new FlagCollection();
            visitorDelegateMock.SetupGet(x => x.Flags).Returns(CampaignsData.GetFlag());
            visitorDelegateMock.SetupGet(x => x.FetchFlagsStatus).Returns(new FetchFlagsStatus
            {
                Status = Enums.FSFetchStatus.FETCH_REQUIRED,
                Reason = Enums.FSFetchReasons.AUTHENTICATE
            });

            var flags = visitorDelegateMock.Object.GetFlags();
            Assert.IsNotNull(flags);
        }

        [TestMethod()]
        public void GetFlagMetadataTest()
        {
            defaultStrategy.Setup(x => x.GetFlagMetadata("key", null))
                .Returns(FlagMetadata.EmptyMetadata())
                .Verifiable();
            var metadata = visitorDelegateMock.Object.GetFlagMetadata("key", null);
            Assert.AreEqual(JsonConvert.SerializeObject(metadata), JsonConvert.SerializeObject(FlagMetadata.EmptyMetadata()));
            defaultStrategy.Verify();
        }

        [TestMethod()]
        public void GetFlagValueTest()
        {
            defaultStrategy.Setup(x => x.GetFlagValue("key", "defaultValue", null, true))
                .Returns("value")
                .Verifiable();

            var value = visitorDelegateMock.Object.GetFlagValue("key", "defaultValue", null, true);
            defaultStrategy.Verify();
        }

        [TestMethod()]
        public void UserExposedTest()
        {
            defaultStrategy.Setup(x => x.VisitorExposed("key", "default", null, true))
                .Verifiable();
            visitorDelegateMock.Object.VisitorExposed("key", "default", null, true);
            defaultStrategy.Verify();

            defaultStrategy.Verify(x => x.VisitorExposed("key", "default", null, true), Times.Once());
        }

        [TestMethod()]
        public async Task SendHitTest()
        {
            var screen = new Screen("home");
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
            defaultStrategy.Setup(x => x.UpdateContext("key", "string"))
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

            defaultStrategy.Verify(x => x.Authenticate(visitorId), Times.Once());
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
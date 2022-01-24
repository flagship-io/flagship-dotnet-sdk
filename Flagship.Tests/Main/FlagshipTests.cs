using Microsoft.VisualStudio.TestTools.UnitTesting;
using Flagship.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flagship.Utils;
using System.IO;
using Flagship.Enums;

namespace Flagship.Main.Tests
{
    [TestClass()]
    public class FlagshipTests
    {
        [TestMethod()]
        public void StartTest()
        {
            var stringWriter = new StringWriter();
            Console.SetOut(stringWriter);
            Flagship.Start(null, null);
            Assert.IsNotNull(Flagship.Config);
            Assert.IsInstanceOfType(Flagship.Config, typeof(Config.DecisionApiConfig));
            Assert.IsInstanceOfType(Flagship.Config.LogManager, typeof(FsLogManager));
            Assert.IsNull(Flagship.Config.EnvId);
            Assert.IsNull(Flagship.Config.ApiKey);

            Assert.AreEqual(FlagshipStatus.NOT_INITIALIZED, Flagship.Status);

            Assert.IsTrue(stringWriter.ToString().Contains(Constants.INITIALIZATION_PARAM_ERROR));
            stringWriter.Dispose();
        }

        [TestMethod()]
        public void Start2Test()
        {
            var stringWriter = new StringWriter();
            Console.SetOut(stringWriter);

            var envId = "envId";
            var apiKey = "apiKey";

            Flagship.Start(envId, apiKey);

            Assert.IsNotNull(Flagship.Config);
            Assert.IsInstanceOfType(Flagship.Config, typeof(Config.DecisionApiConfig));
            Assert.IsInstanceOfType(Flagship.Config.LogManager, typeof(FsLogManager));

            Assert.AreEqual(envId, Flagship.Config.EnvId);
            Assert.AreEqual(apiKey, Flagship.Config.ApiKey);

            Assert.AreEqual(FlagshipStatus.READY, Flagship.Status);

            envId = "new envId";
            apiKey = "new apiKey";

            Flagship.Start(envId, apiKey);

            Assert.IsNotNull(Flagship.Config);
            Assert.IsInstanceOfType(Flagship.Config, typeof(Config.DecisionApiConfig));
            Assert.IsInstanceOfType(Flagship.Config.LogManager, typeof(FsLogManager));

            Assert.AreEqual(envId, Flagship.Config.EnvId);
            Assert.AreEqual(apiKey, Flagship.Config.ApiKey);

            var config = new Config.DecisionApiConfig();

            Flagship.Start(envId, apiKey, config);

            Assert.IsNotNull(Flagship.Config);
            Assert.AreEqual(Flagship.Config, config);
            Assert.IsInstanceOfType(Flagship.Config.LogManager, typeof(FsLogManager));

            Assert.AreEqual(envId, Flagship.Config.EnvId);
            Assert.AreEqual(apiKey, Flagship.Config.ApiKey);

            Assert.IsTrue(stringWriter.ToString().Contains(string.Format(Constants.SDK_STARTED_INFO, Constants.SDK_VERSION)));
            stringWriter.Dispose();
        }

        [TestMethod()]
        public void Start3Test()
        {
            var stringWriter = new StringWriter();
            Console.SetOut(stringWriter);

            var envId = "envId";
            var apiKey = "apiKey";

            var config = new Config.BucketingConfig();

            Flagship.Start(envId, apiKey, config);

            Assert.IsNotNull(Flagship.Config);
            Assert.IsInstanceOfType(Flagship.Config, typeof(Config.BucketingConfig));
            Assert.IsInstanceOfType(Flagship.Config.LogManager, typeof(FsLogManager));

            Assert.AreEqual(envId, Flagship.Config.EnvId);
            Assert.AreEqual(apiKey, Flagship.Config.ApiKey);

            Assert.IsTrue(stringWriter.ToString().Contains(string.Format(Constants.SDK_STARTED_INFO, Constants.SDK_VERSION)));
            Assert.IsTrue(stringWriter.ToString().Contains("Bucketing polling starts"));

            stringWriter.Dispose();
        }

        [TestMethod()]
        public void NewVisitorTest()
        {
            var visitorBuilder = Flagship.NewVisitor();
            Assert.IsNull(visitorBuilder);

            Flagship.Start(null, null);

            visitorBuilder = Flagship.NewVisitor();
            Assert.IsNull(visitorBuilder);

            var envId = "envId";
            var apiKey = "apiKey";

            Flagship.Start(envId, apiKey);

            visitorBuilder = Flagship.NewVisitor();

            Assert.IsInstanceOfType(visitorBuilder, typeof(FsVisitor.VisitorBuilder));

            var visitorId = "visitorId";
            visitorBuilder = Flagship.NewVisitor(visitorId);
            Assert.IsInstanceOfType(visitorBuilder, typeof(FsVisitor.VisitorBuilder));
            Assert.AreEqual(visitorBuilder.Build().VisitorId, visitorId);

            visitorBuilder = Flagship.NewVisitor(visitorId, InstanceType.SINGLE_INSTANCE);
            Assert.IsInstanceOfType(visitorBuilder, typeof(FsVisitor.VisitorBuilder));
            Assert.AreEqual(visitorBuilder.Build().VisitorId, visitorId);

            visitorBuilder = Flagship.NewVisitor(InstanceType.SINGLE_INSTANCE);
            Assert.IsInstanceOfType(visitorBuilder, typeof(FsVisitor.VisitorBuilder));

        }
    }
}
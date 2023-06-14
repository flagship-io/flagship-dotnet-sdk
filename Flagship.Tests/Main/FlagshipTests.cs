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
using Flagship.Logger;

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
            Fs.Start(null, null);
            Assert.IsNotNull(Fs.Config);
            Assert.IsInstanceOfType(Fs.Config, typeof(Config.DecisionApiConfig));
            Assert.IsInstanceOfType(Fs.Config.LogManager, typeof(FsLogManager));
            Assert.IsNull(Fs.Config.EnvId);
            Assert.IsNull(Fs.Config.ApiKey);

            Assert.AreEqual(FlagshipStatus.NOT_INITIALIZED, Fs.Status);

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

            Fs.Start(envId, apiKey);

            Assert.IsNotNull(Fs.Config);
            Assert.IsInstanceOfType(Fs.Config, typeof(Config.DecisionApiConfig));
            Assert.IsInstanceOfType(Fs.Config.LogManager, typeof(FsLogManager));

            Assert.AreEqual(envId, Fs.Config.EnvId);
            Assert.AreEqual(apiKey, Fs.Config.ApiKey);

            Assert.AreEqual(FlagshipStatus.READY, Fs.Status);

            envId = "new envId";
            apiKey = "new apiKey";

            Fs.Start(envId, apiKey);

            Assert.IsNotNull(Fs.Config);
            Assert.IsInstanceOfType(Fs.Config, typeof(Config.DecisionApiConfig));
            Assert.IsInstanceOfType(Fs.Config.LogManager, typeof(FsLogManager));

            Assert.AreEqual(envId, Fs.Config.EnvId);
            Assert.AreEqual(apiKey, Fs.Config.ApiKey);

            var config = new Config.DecisionApiConfig();

            Fs.Start(envId, apiKey, config);

            Assert.IsNotNull(Fs.Config);
            Assert.AreEqual(Fs.Config, config);
            Assert.IsInstanceOfType(Fs.Config.LogManager, typeof(FsLogManager));

            Assert.AreEqual(envId, Fs.Config.EnvId);
            Assert.AreEqual(apiKey, Fs.Config.ApiKey);

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

            Fs.Start(envId, apiKey, config);

            Assert.IsNotNull(Fs.Config);
            Assert.IsInstanceOfType(Fs.Config, typeof(Config.BucketingConfig));
            Assert.IsInstanceOfType(Fs.Config.LogManager, typeof(FsLogManager));

            Assert.AreEqual(envId, Fs.Config.EnvId);
            Assert.AreEqual(apiKey, Fs.Config.ApiKey);

            Assert.IsTrue(stringWriter.ToString().Contains(string.Format(Constants.SDK_STARTED_INFO, Constants.SDK_VERSION)));
            Assert.IsTrue(stringWriter.ToString().Contains("Bucketing polling starts"));

            stringWriter.Dispose();
        }

        [TestMethod()]
        public void NewVisitorTest()
        {
            var visitorBuilder = Fs.NewVisitor();

            Fs.Start(null, null);

            visitorBuilder = Fs.NewVisitor();
            Assert.IsNull(visitorBuilder);

            var envId = "envId";
            var apiKey = "apiKey";

            Fs.Start(envId, apiKey);

            visitorBuilder = Fs.NewVisitor();

            Assert.IsInstanceOfType(visitorBuilder, typeof(FsVisitor.VisitorBuilder));

            var visitorId = "visitorId";
            visitorBuilder = Fs.NewVisitor(visitorId);
            Assert.IsInstanceOfType(visitorBuilder, typeof(FsVisitor.VisitorBuilder));
            Assert.AreEqual(visitorBuilder.Build().VisitorId, visitorId);

            visitorBuilder = Fs.NewVisitor(visitorId, InstanceType.SINGLE_INSTANCE);
            Assert.IsInstanceOfType(visitorBuilder, typeof(FsVisitor.VisitorBuilder));
            Assert.AreEqual(visitorBuilder.Build().VisitorId, visitorId);

            visitorBuilder = Fs.NewVisitor(InstanceType.SINGLE_INSTANCE);
            Assert.IsInstanceOfType(visitorBuilder, typeof(FsVisitor.VisitorBuilder));

        }
    }
}
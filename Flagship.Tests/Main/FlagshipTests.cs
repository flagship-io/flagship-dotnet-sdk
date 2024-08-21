using Microsoft.VisualStudio.TestTools.UnitTesting;
using Flagship.Enums;
using Flagship.Logger;
using Flagship.FsVisitor;

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

            Assert.AreEqual(FSSdkStatus.SDK_NOT_INITIALIZED, Fs.Status);

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

            Assert.AreEqual(FSSdkStatus.SDK_INITIALIZED, Fs.Status);

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

            Assert.IsTrue(stringWriter.ToString().Contains(string.Format(Constants.SDK_STARTED_INFO, Constants.SDK_VERSION, FSSdkStatus.SDK_INITIALIZED)));
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

            Assert.IsTrue(stringWriter.ToString().Contains(string.Format(Constants.SDK_STARTED_INFO, Constants.SDK_VERSION, FSSdkStatus.SDK_INITIALIZING)));
            Assert.IsTrue(stringWriter.ToString().Contains("Bucketing polling starts"));

            stringWriter.Dispose();
        }

        [TestMethod()]
        public void NewVisitorTest()
        {
            var visitorBuilder = Fs.NewVisitor("visitorId", true);

            Fs.Start(null, null);

            visitorBuilder = Fs.NewVisitor("visitorId", true);
            Assert.IsInstanceOfType<VisitorBuilder>(visitorBuilder);

            var envId = "envId";
            var apiKey = "apiKey";

            Fs.Start(envId, apiKey);

            visitorBuilder = Fs.NewVisitor("visitorId", true);

            Assert.IsInstanceOfType(visitorBuilder, typeof(FsVisitor.VisitorBuilder));

            var visitorId = "visitorId";
            visitorBuilder = Fs.NewVisitor(visitorId, true);
            Assert.IsInstanceOfType(visitorBuilder, typeof(FsVisitor.VisitorBuilder));

            var visitor = visitorBuilder.Build();
            Assert.AreEqual(visitor.VisitorId, visitorId);
            Assert.AreEqual(visitor.HasConsented, true);

        }
    }
}
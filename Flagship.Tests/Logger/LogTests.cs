using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Flagship.Enums;

namespace Flagship.Logger.Tests 
{
    [TestClass()]
    public class LogTests
    {
        [TestMethod()]
        public void LogErrorTest()
        {
            var fsLogManagerMock = new Mock<IFsLogManager>();
            var config = new Config.DecisionApiConfig()
            {
                LogManager = fsLogManagerMock.Object,
            };

            var message = "test log Error";
            var tag = "test";

            Log.LogError(config, message, tag);

            fsLogManagerMock.Verify(x=>x.Error(message, tag), Times.Once());

            config.LogLevel = LogLevel.CRITICAL;

            Log.LogError(config, message, tag);

            fsLogManagerMock.Verify(x => x.Error(message, tag), Times.Once());

            config.LogManager = null;

            Log.LogError(config, message, tag);

            fsLogManagerMock.Verify(x => x.Error(message, tag), Times.Once());
        }

        [TestMethod()]
        public void LogErrorFailedTest()
        {
            var fsLogManagerMock = new Mock<IFsLogManager>();
            var config = new Config.DecisionApiConfig()
            {
                LogManager = fsLogManagerMock.Object,
            };


            var message = "test log Error";
            var tag = "test";

            var error = new Exception("error");

            fsLogManagerMock.Setup(x=>x.Error(message,tag)).Throws(error);

            var stringWriter = new StringWriter();
            Console.SetOut(stringWriter);

            Log.LogError(config, message, tag);

            fsLogManagerMock.Verify(x => x.Error(message, tag), Times.Once());

            Assert.IsTrue(stringWriter.ToString().Contains(error.Message));

            stringWriter.Dispose();

        }

        [TestMethod()]
        public void LogInfoTest()
        {
            var fsLogManagerMock = new Mock<IFsLogManager>();
            var config = new Config.DecisionApiConfig()
            {
                LogManager = fsLogManagerMock.Object,
            };

            var message = "test log Info";
            var tag = "test";

            Log.LogInfo(config, message, tag);

            fsLogManagerMock.Verify(x => x.Info(message, tag), Times.Once());

            config.LogLevel = LogLevel.CRITICAL;

            Log.LogInfo(config, message, tag);

            fsLogManagerMock.Verify(x => x.Info(message, tag), Times.Once());

            config.LogManager = null;

            Log.LogInfo(config, message, tag);

            fsLogManagerMock.Verify(x => x.Info(message, tag), Times.Once());
        }

        [TestMethod()]
        public void LogInfoFailedTest()
        {
            var fsLogManagerMock = new Mock<IFsLogManager>();
            var config = new Config.DecisionApiConfig()
            {
                LogManager = fsLogManagerMock.Object,
            };


            var message = "test log info";
            var tag = "test";

            var error = new Exception("error");

            fsLogManagerMock.Setup(x => x.Info(message, tag)).Throws(error);

            var stringWriter = new StringWriter();
            Console.SetOut(stringWriter);

            Log.LogInfo(config, message, tag);

            fsLogManagerMock.Verify(x => x.Info(message, tag), Times.Once());

            Assert.IsTrue(stringWriter.ToString().Contains(error.Message));

            stringWriter.Dispose();

        }


        [TestMethod()]
        public void LogDebugTest()
        { 
            var fsLogManagerMock = new Mock<IFsLogManager>();
            var config = new Config.DecisionApiConfig()
            {
                LogManager = fsLogManagerMock.Object,
            };

            var message = "test debug Info";
            var tag = "test";

            Log.LogDebug(config, message, tag);

            fsLogManagerMock.Verify(x => x.Debug(message, tag), Times.Once());

            config.LogLevel = LogLevel.CRITICAL;

            Log.LogDebug(config, message, tag);

            fsLogManagerMock.Verify(x => x.Debug(message, tag), Times.Once());

            config.LogManager = null;

            Log.LogDebug(config, message, tag);

            fsLogManagerMock.Verify(x => x.Debug(message, tag), Times.Once());
        }

        [TestMethod()]
        public void LogDebugFailedTest()
        {
            var fsLogManagerMock = new Mock<IFsLogManager>();
            var config = new Config.DecisionApiConfig()
            {
                LogManager = fsLogManagerMock.Object,
            };

            var message = "test debug info";
            var tag = "test";

            var error = new Exception("error");

            fsLogManagerMock.Setup(x => x.Debug(message, tag)).Throws(error);

            var stringWriter = new StringWriter();
            Console.SetOut(stringWriter);

            Log.LogDebug(config, message, tag);

            fsLogManagerMock.Verify(x => x.Debug(message, tag), Times.Once());

            Assert.IsTrue(stringWriter.ToString().Contains(error.Message));

            stringWriter.Dispose();

        }
    }
}
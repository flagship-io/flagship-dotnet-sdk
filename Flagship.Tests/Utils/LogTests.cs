using Microsoft.VisualStudio.TestTools.UnitTesting;
using Flagship.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Moq;
using Flagship.Enums;

namespace Flagship.Utils.Tests
{
    [TestClass()]
    public class LogTests
    {
        [TestMethod()]
        public void LogErrorTest()
        {
            var fsLogManagerMock = new Mock<Flagship.Utils.IFsLogManager>();
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
            var fsLogManagerMock = new Mock<Flagship.Utils.IFsLogManager>();
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
            var fsLogManagerMock = new Mock<Flagship.Utils.IFsLogManager>();
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
            var fsLogManagerMock = new Mock<Flagship.Utils.IFsLogManager>();
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
    }
}
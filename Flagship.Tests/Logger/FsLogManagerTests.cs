using Microsoft.VisualStudio.TestTools.UnitTesting;
using Flagship.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Flagship.Enums;

namespace Flagship.Logger.Tests
{ 
    [TestClass()]
    public class FsLogManagerTests
    {
        [TestMethod()]
        public void AlertTest()
        {
            var logManager = new FsLogManager();
            var message = "test log";
            var tag = "test";

            var stringWriter = new StringWriter();
            Console.SetOut(stringWriter);

            logManager.Alert(message, tag);

            Assert.IsTrue(stringWriter.ToString().Contains($"[{Constants.FLAGSHIP_SDK}] [{LogLevel.ALERT}] [{tag}]: {message}"));

            stringWriter.Dispose();

        }

        [TestMethod()]
        public void CriticalTest()
        {
            var logManager = new FsLogManager();
            var message = "test log";
            var tag = "test";

            var stringWriter = new StringWriter();
            Console.SetOut(stringWriter);

            logManager.Critical(message, tag);

            Assert.IsTrue(stringWriter.ToString().Contains($"[{Constants.FLAGSHIP_SDK}] [{LogLevel.CRITICAL}] [{tag}]: {message}"));

            stringWriter.Dispose();
        }

        [TestMethod()]
        public void DebugTest()
        {
            var logManager = new FsLogManager();
            var message = "test log";
            var tag = "test";

            var stringWriter = new StringWriter();
            Console.SetOut(stringWriter);

            logManager.Debug(message, tag);

            Assert.IsTrue(stringWriter.ToString().Contains($"[{Constants.FLAGSHIP_SDK}] [{LogLevel.DEBUG}] [{tag}]: {message}"));

            stringWriter.Dispose();
        }

        [TestMethod()]
        public void EmergencyTest()
        {
            var logManager = new FsLogManager();
            var message = "test log";
            var tag = "test";

            var stringWriter = new StringWriter();
            Console.SetOut(stringWriter);

            logManager.Emergency(message, tag);

            Assert.IsTrue(stringWriter.ToString().Contains($"[{Constants.FLAGSHIP_SDK}] [{LogLevel.EMERGENCY}] [{tag}]: {message}"));

            stringWriter.Dispose();
        }

        [TestMethod()]
        public void ErrorTest()
        {
            var logManager = new FsLogManager();
            var message = "test log";
            var tag = "test";

            var stringWriter = new StringWriter();
            Console.SetOut(stringWriter);

            logManager.Error(message, tag);

            Assert.IsTrue(stringWriter.ToString().Contains($"[{Constants.FLAGSHIP_SDK}] [{LogLevel.ERROR}] [{tag}]: {message}"));

            stringWriter.Dispose();
        }

        [TestMethod()]
        public void InfoTest()
        {
            var logManager = new FsLogManager();
            var message = "test log";
            var tag = "test";

            var stringWriter = new StringWriter();
            Console.SetOut(stringWriter);

            logManager.Info(message, tag);

            Assert.IsTrue(stringWriter.ToString().Contains($"[{Constants.FLAGSHIP_SDK}] [{LogLevel.INFO}] [{tag}]: {message}"));

            stringWriter.Dispose();
        }

        [TestMethod()]
        public void NoticeTest()
        {
            var logManager = new FsLogManager();
            var message = "test log";
            var tag = "test";

            var stringWriter = new StringWriter();
            Console.SetOut(stringWriter);

            logManager.Notice(message, tag);

            Assert.IsTrue(stringWriter.ToString().Contains($"[{Constants.FLAGSHIP_SDK}] [{LogLevel.NOTICE}] [{tag}]: {message}"));

            stringWriter.Dispose();
        }

        [TestMethod()]
        public void WarningTest()
        {
            var logManager = new FsLogManager();
            var message = "test log";
            var tag = "test";

            var stringWriter = new StringWriter();
            Console.SetOut(stringWriter);

            logManager.Warning(message, tag);

            Assert.IsTrue(stringWriter.ToString().Contains($"[{Constants.FLAGSHIP_SDK}] [{LogLevel.WARNING}] [{tag}]: {message}"));

            stringWriter.Dispose();
        }

        [TestMethod()]
        public void LogTest()
        {
            var logManager = new FsLogManager();
            var message = "test log";
            var tag = "test";

            var stringWriter = new StringWriter();
            Console.SetOut(stringWriter);

            logManager.Log(LogLevel.ALERT, message, tag);

            Assert.IsTrue(stringWriter.ToString().Contains($"[{Constants.FLAGSHIP_SDK}] [{LogLevel.ALERT}] [{tag}]: {message}"));

            stringWriter.Dispose();
        }
    }
}
using Flagship.Model.Config;
using Flagship.Model.Decision;
using Flagship.Model.Logs;
using Flagship.Services.ExceptionHandler;
using Flagship.Services.Logger;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Flagship.Tests.Config
{
    [TestClass]
    public class FlagshipOptionsTests
    {
        class CustomErrorHandler : IExceptionHandler
        {
            public void Handle(Exception exception)
            {
                throw new NotImplementedException();
            }
        }

        class CustomLogger : ILogger
        {
            public void Log(LogLevel level, LogCode code, params object[] args)
            {
                throw new NotImplementedException();
            }
        }

        [TestMethod]
        public void TestOptionsBuilder()
        {
            var errorHandler = new CustomErrorHandler();
            var logger = new CustomLogger();
            var builder = new FlagshipOptions.Builder();
            var options = builder.WithAPIOptions(TimeSpan.FromSeconds(2))
                 .WithBucketingOptions(TimeSpan.FromSeconds(3))
                 .WithDecisionMode(Mode.Bucketing)
                 .WithErrorHandler(errorHandler)
                 .WithLogger(logger)
                 .Build();

            Assert.AreEqual(options.ExceptionHandler, errorHandler);
            Assert.AreEqual(options.Logger, logger);
            Assert.AreEqual(options.DecisionMode, Mode.Bucketing);
            Assert.AreEqual(options.Timeout.Value.TotalSeconds, 2);
            Assert.AreEqual(options.BucketingPollingInterval.Value.TotalSeconds, 3);
        }
    }
}

using Flagship.Model.Decision;
using Flagship.Services.ExceptionHandler;
using Flagship.Services.Logger;
using System;
using System.Collections.Generic;
using System.Text;

namespace Flagship.Model.Config
{
    public class FlagshipOptions
    {
        public IExceptionHandler ExceptionHandler { get; private set; }
        public ILogger Logger { get; private set; }
        public Mode DecisionMode { get; private set; }
        public TimeSpan? Timeout { get; private set; }
        public TimeSpan? BucketingPollingInterval { get; private set; }

        public class Builder
        {
            private readonly FlagshipOptions options;
            public Builder()
            {
                options = new FlagshipOptions();
            }

            public Builder WithErrorHandler(IExceptionHandler handler)
            {
                options.ExceptionHandler = handler;
                return this;
            }

            public Builder WithLogger(ILogger logger)
            {
                options.Logger = logger;
                return this;
            }

            public Builder WithDecisionMode(Mode mode)
            {
                options.DecisionMode = mode;
                return this;
            }

            public Builder WithAPIOptions(TimeSpan timeout)
            {
                options.Timeout = timeout;
                return this;
            }

            public Builder WithBucketingOptions(TimeSpan? pollingInterval = null)
            {
                options.BucketingPollingInterval = pollingInterval;
                return this;
            }

            public FlagshipOptions Build()
            {
                return options;
            }
        }
    }
}

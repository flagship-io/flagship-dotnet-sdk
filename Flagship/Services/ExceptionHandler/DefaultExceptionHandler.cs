using Flagship.Services.Logger;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Flagship.Services.ExceptionHandler
{
    public class DefaultExceptionHandler : IExceptionHandler
    {
        private readonly ILogger logger;
        private readonly bool shouldThrow;

        /// <summary>
        /// DefaultExceptionHandler constructor
        /// </summary>
        /// <param name="logger">The logger to use to log the error messages</param>
        /// <param name="shouldThrow">If true, will throw exceptions to calling code</param>
        public DefaultExceptionHandler(ILogger logger = null, bool shouldThrow = false)
        {
            this.logger = logger;
            this.shouldThrow = shouldThrow;
        }

        public void Handle(Exception exception)
        {
            if (logger != null)
                logger.Log(LogLevel.ERROR, Model.Logs.LogCode.EXCEPTION_OCCURED, new { message = exception.Message });

            if (shouldThrow)
                throw exception;
        }
    }
}

using Flagship.Model.Logs;
using System;
using System.Collections.Generic;
using System.Text;
using static Flagship.Model.Logs.LogCodes;

namespace Flagship.Services.Logger
{
    public enum LogLevel
    {
        DEBUG,
        INFO,
        WARN,
        ERROR,
    }

    public interface ILogger
    {
        void Log(LogLevel level, LogCode code, params object[] args);
    }
}

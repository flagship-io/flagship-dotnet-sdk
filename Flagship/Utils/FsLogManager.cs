using Flagship.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flagship.Utils
{
    internal class FsLogManager : IFsLogManager
    {
        public void Alert(string message, string tag)
        {
            Log(LogLevel.ALERT, message, tag);
        }

        public void Critical(string message, string tag)
        {
            Log(LogLevel.CRITICAL, message, tag);
        }

        public void Debug(string message, string tag)
        {
            Log(LogLevel.DEBUG, message, tag);
        }

        public void Emergency(string message, string tag)
        {
            Log(LogLevel.EMERGENCY, message, tag);
        }

        public void Error(string message, string tag)
        {
            Log(LogLevel.ERROR, message, tag);
        }

        public void Info(string message, string tag)
        {
            Log(LogLevel.INFO, message, tag);
        }

        public void Notice(string message, string tag)
        {
            Log(LogLevel.NOTICE, message, tag);
        }

        public void Warning(string message, string tag)
        {
            Log(LogLevel.WARNING, message, tag);
        }
        public void Log(LogLevel level, string message, string tag)
        {
            Console.WriteLine($"[{DateTime.Now:G}] [{Constants.FLAGSHIP_SDK}] [{level}] [{tag}]: {message}");
        }
    }
}

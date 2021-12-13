using Flagship.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flagship.Utils
{
    public interface IFsLogManager
    {
         void Emergency(string message, string tag);
         void Alert(string message, string tag);
         void Critical(string message, string tag);
         void Error(string message, string tag);
         void Warning(string message, string tag);
         void Notice(string message, string tag);
         void Info(string message, string tag);
         void Debug(string message, string tag);
         void Log(LogLevel level, string message, string tag);
    }
}

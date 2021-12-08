using Flagship.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flagship.Utils
{
    public interface IFsLogManager
    {
        public void Emergency(string message, string tag);
        public void Alert(string message, string tag);
        public void Critical(string message, string tag);
        public void Error(string message, string tag);
        public void Warning(string message, string tag);
        public void Notice(string message, string tag);
        public void Info(string message, string tag);
        public void Debug(string message, string tag);
        public void Log(LogLevel level, string message, string tag);
    }
}

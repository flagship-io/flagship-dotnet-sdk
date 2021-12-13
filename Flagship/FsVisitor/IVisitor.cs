using Flagship.Config;
using Flagship.FsFlag;
using Flagship.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flagship.FsVisitor
{
    internal interface IVisitor : IVisitorCore
    {
        string VisitorId { get; set; }

        string AnonymousId { get; }

        ICollection<FlagDTO> Flags { get; }

        bool HasConsented { get; }

        FlagshipConfig Config { get; }

        IDictionary<string, object> Context { get; }

        IFlag<T> GetFlag<T>(string key, T defaultValue);

    }
}

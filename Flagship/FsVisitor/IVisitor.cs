using Flagship.Config;
using Flagship.FsFlag;
using Flagship.Hit;
using Flagship.Model;
using Newtonsoft.Json.Linq;
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
        void SetConsent(bool hasConsented);
        FlagshipConfig Config { get; }

        IDictionary<string, object> Context { get; }

        IFlag<string> GetFlag(string key, string defaultValue);
        IFlag<double> GetFlag(string key, double defaultValue);
        IFlag<bool> GetFlag(string key, bool defaultValue);
        IFlag<JObject> GetFlag(string key, JObject defaultValue);
        IFlag<JArray> GetFlag(string key, JArray defaultValue);
    }
}

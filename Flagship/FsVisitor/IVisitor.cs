using Flagship.Config;
using Flagship.Flag;
using Flagship.Model;
using Flagship.Visitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flagship.FsVisitor
{
    public interface IVisitor : IVisitorCore
    { 
        public string VisitorId { get; set; }

        public string? AnonymousId { get; }

        public ICollection<FlagDTO> Flags { get; set; }

        public bool HasConsented { get; }

        public FlagshipConfig Config { get; set; }

        public IDictionary<string, object> Context { get; }

        public IFlag GetFlag<T>(string key, T defaultValue);
       
    }
}

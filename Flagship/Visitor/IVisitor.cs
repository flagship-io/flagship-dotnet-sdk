using Flagship.Config;
using Flagship.Flag;
using Flagship.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flagship.FsVisitor
{
    public interface IVisitor
    { 
        public string VisitorID { get; set; }

        public string? Anonymous { get; }

        public ICollection<FlagDTO> Flags { get; set; }

        public bool HasConsented { get; }

        public void SetConsent(bool hasConsented);

        public FlagshipConfig Config { get; set; }

        public IDictionary<string, object> Context { get; }

        public void UpdateContex(IDictionary<string, object> context);
        public void ClearContext();
        public IFlag GetFlag<T>(string key, T defaultValue);
        public Task FetchFlags();
    }
}

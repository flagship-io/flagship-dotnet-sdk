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
    public abstract class VisitorDelegateAbstract : IVisitor
    {
        private IDictionary<string, object> _context;
        private bool _hasConsented;
        protected string? _anonymousId;
        public string VisitorId { get; set; }
        public ICollection<FlagDTO> Flags { get; set; }
        public bool HasConsented => _hasConsented;
        public FlagshipConfig Config { get; set; }
        public IConfigManager ConfigManager { get; set; }
        public IDictionary<string, object> Context => _context;
        public string AnonymousId => _anonymousId ;

        public VisitorDelegateAbstract(string? visitorID, bool isAuthenticated, IDictionary<string, object> context, bool hasConsented, IConfigManager configManager)
        {
            ConfigManager = configManager;
            UpdateContex(context);
            SetConsent(hasConsented);
            VisitorId = visitorID; // TO DO create an visitorID if null is given


        }

        abstract public void ClearContext();

        abstract public Task FetchFlags();

        abstract public IFlag GetFlag<T>(string key, T defaultValue);

        abstract public void SetConsent(bool hasConsented);

        abstract public void UpdateContex(IDictionary<string, object> context);
    }
}

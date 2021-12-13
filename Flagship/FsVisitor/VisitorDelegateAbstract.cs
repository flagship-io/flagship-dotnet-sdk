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
    internal abstract class VisitorDelegateAbstract : IVisitor
    {
        private readonly IDictionary<string, object> _context;
        private bool _hasConsented;
        protected string _anonymousId;
        public string VisitorId { get; set; }
        public ICollection<FlagDTO> Flags { get; set; }
        public bool HasConsented => _hasConsented;
        public FlagshipConfig Config { get; set; }
        public IConfigManager ConfigManager { get; set; }
        public IDictionary<string, object> Context => _context;
        public string AnonymousId => _anonymousId ;

        public VisitorDelegateAbstract(string visitorID, bool isAuthenticated, IDictionary<string, object> context, bool hasConsented, IConfigManager configManager)
        {
            ConfigManager = configManager;
            _context = new Dictionary<string, object>();
            UpdateContex(context);
            SetConsent(hasConsented);
            VisitorId = visitorID; // TO DO create an visitorID if null is given


        }

        protected VisitorStrategyAbstract GetStrategy()
        {
            return new DefaultStrategy(this);
        }

        public void SetConsent(bool hasConsented)
        {
            _hasConsented = hasConsented;
            this.GetStrategy().SetConsent(hasConsented);
        }

        abstract public void ClearContext();

        abstract public Task FetchFlags();

        abstract public IFlag<T> GetFlag<T>(string key, T defaultValue);

        abstract public void UpdateContex(IDictionary<string, object> context);

        abstract public Task UserExposed<T>(string key, T defaultValue, FlagDTO flag);
        abstract public T GetFlagValue<T>(string key, T defaultValue, FlagDTO flag, bool userExposed);
        abstract public IFlagMetadata GetFlagMetadata(IFlagMetadata metadata, string key, bool hasSameType);
    }
}

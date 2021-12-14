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
    internal abstract class VisitorDelegateAbstract : IVisitor
    {
        private readonly IDictionary<string, object> _context;
        private bool _hasConsented;
        protected string _anonymousId;
        public string VisitorId { get; set; }
        public ICollection<FlagDTO> Flags { get; set; }
        public bool HasConsented => _hasConsented;
        public FlagshipConfig Config => ConfigManager.Config;
        public IConfigManager ConfigManager { get; set; }
        public IDictionary<string, object> Context => _context;
        public string AnonymousId => _anonymousId ;

        public VisitorDelegateAbstract(string visitorID, bool isAuthenticated, IDictionary<string, object> context, bool hasConsented, IConfigManager configManager)
        {
            ConfigManager = configManager;
            _context = new Dictionary<string, object>();
            UpdateContex(context);
            VisitorId = visitorID ?? CreateVisitorId();
            SetConsent(hasConsented);
        }

        protected string CreateVisitorId()
        {
            var date = DateTime.Now;
            return $"{date.Year}{date.Month}{date.Day}{date.Hour}{date.Minute}{date.Second}{new Random().Next(10000, 99999)}";
        }

        protected VisitorStrategyAbstract GetStrategy()
        {
            return new DefaultStrategy(this);
        }

        public void SetConsent(bool hasConsented)
        {
            _hasConsented = hasConsented;
            this.GetStrategy().SendConsentHit(hasConsented);
        }

        abstract public void ClearContext();

        abstract public Task FetchFlags();

        abstract public IFlag<string> GetFlag(string key, string defaultValue);
        abstract public IFlag<double> GetFlag(string key, double defaultValue); 
        abstract public IFlag<bool> GetFlag(string key, bool defaultValue);
        abstract public IFlag<JObject> GetFlag(string key, JObject defaultValue);
        abstract public IFlag<JArray> GetFlag(string key, JArray defaultValue);

        abstract public void UpdateContex(IDictionary<string, object> context);

        abstract public Task UserExposed<T>(string key, T defaultValue, FlagDTO flag);
        abstract public T GetFlagValue<T>(string key, T defaultValue, FlagDTO flag, bool userExposed);
        abstract public IFlagMetadata GetFlagMetadata(IFlagMetadata metadata, string key, bool hasSameType);
        abstract public Task SendHit(HitAbstract hit);
    }
}

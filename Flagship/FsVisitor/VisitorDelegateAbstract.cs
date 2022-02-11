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
using Flagship.Enums;

namespace Flagship.FsVisitor
{ 
    internal abstract class VisitorDelegateAbstract : IVisitor
    {
        private readonly IDictionary<string, object> _context;
        private bool _hasConsented;
        protected string _anonymousId;
        virtual public string VisitorId { get; set; }
        virtual public ICollection<FlagDTO> Flags { get; set; }

        virtual public ICollection<Campaign> Campaigns { get; set; } 
        virtual public bool HasConsented => _hasConsented;
        virtual public FlagshipConfig Config => ConfigManager.Config;
        virtual public IConfigManager ConfigManager { get; set; }
        virtual public IDictionary<string, object> Context => _context;
        virtual public string AnonymousId { get => _anonymousId; internal set { _anonymousId = value; } }
        virtual public VisitorCache VisitorCache { get; set; }

        public VisitorDelegateAbstract(string visitorID, bool isAuthenticated, IDictionary<string, object> context, bool hasConsented, IConfigManager configManager)
        {
            ConfigManager = configManager;
            _context = new Dictionary<string, object>();
            UpdateContext(context);
            Flags = new HashSet<FlagDTO>();
            VisitorId = visitorID ?? CreateVisitorId();
            SetConsent(hasConsented);
            LoadPredefinedContext();

            if (isAuthenticated && configManager.Config.DecisionMode== DecisionMode.DECISION_API)
            {
                _anonymousId = Guid.NewGuid().ToString();
            }

            GetStrategy().LookupVisitor();
        }
       
        protected string CreateVisitorId()
        {
            
            var date = DateTime.Now;
            return $"{date.Year}{Utils.Utils.TwoDigit(date.Month)}{Utils.Utils.TwoDigit(date.Day)}{Utils.Utils.TwoDigit(date.Hour)}{Utils.Utils.TwoDigit(date.Minute)}{Utils.Utils.TwoDigit(date.Second)}{new Random().Next(10000, 99999)}";
        }

        protected void LoadPredefinedContext()
        {
            _context[FsPredefinedContext.FLAGSHIP_CLIENT] = Constants.SDK_LANGUAGE;
            _context[FsPredefinedContext.FLAGSHIP_VERSION] = Constants.SDK_VERSION;
            //_context[FsPredefinedContext.FLAGSHIP_VISITOR] = VisitorId; 
        }

        virtual protected VisitorStrategyAbstract GetStrategy()
        {
            VisitorStrategyAbstract strategy;
            if (Flagship.Main.Flagship.Status == Enums.FlagshipStatus.NOT_INITIALIZED)
            {
                strategy = new NotReadyStrategy(this);
            }
            else if (Flagship.Main.Flagship.Status == Enums.FlagshipStatus.READY_PANIC_ON)
            {
                strategy =  new PanicStrategy(this);
            }
            else if (!HasConsented)
            {
                strategy =  new NoConsentStrategy(this);
            }
            else
            {
                strategy =  new DefaultStrategy(this);
            }
            return strategy;
        }

        virtual public void SetConsent(bool hasConsented)
        {
            _hasConsented = hasConsented;
            _ = GetStrategy().SendConsentHitAsync(hasConsented);
        }

        abstract public void ClearContext();

        abstract public Task FetchFlags();

        abstract public IFlag<string> GetFlag(string key, string defaultValue);
        abstract public IFlag<long> GetFlag(string key, long defaultValue);
        abstract public IFlag<bool> GetFlag(string key, bool defaultValue);
        abstract public IFlag<JObject> GetFlag(string key, JObject defaultValue);
        abstract public IFlag<JArray> GetFlag(string key, JArray defaultValue);

        abstract public Task UserExposed<T>(string key, T defaultValue, FlagDTO flag);
        abstract public T GetFlagValue<T>(string key, T defaultValue, FlagDTO flag, bool userExposed);
        abstract public IFlagMetadata GetFlagMetadata(IFlagMetadata metadata, string key, bool hasSameType);
        abstract public Task SendHit(HitAbstract hit); 

        abstract public void UpdateContext(IDictionary<string, object> context); 
        abstract public void UpdateContext(string key, string value);

        abstract public void UpdateContext(string key, double value);

        abstract public void UpdateContext(string key, bool value);

        virtual public void Authenticate(string visitorId)
        {
            GetStrategy().Authenticate(visitorId);
        }

        virtual public void Unauthenticate()
        {
            GetStrategy().Unauthenticate();
        }
    }
}

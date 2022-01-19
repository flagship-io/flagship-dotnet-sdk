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
        virtual public bool HasConsented => _hasConsented;
        virtual public FlagshipConfig Config => ConfigManager.Config;
        virtual public IConfigManager ConfigManager { get; set; }
        virtual public IDictionary<string, object> Context => _context;
        virtual public string AnonymousId => _anonymousId ;

        private VisitorStrategyAbstract _strategy;

        public VisitorDelegateAbstract(string visitorID, bool isAuthenticated, IDictionary<string, object> context, bool hasConsented, IConfigManager configManager)
        {
            ConfigManager = configManager;
            _context = new Dictionary<string, object>();
            UpdateContexCommon(context);
            Init(visitorID, isAuthenticated, hasConsented);
        }
        public VisitorDelegateAbstract(string visitorID, bool isAuthenticated, IDictionary<string, string> context, bool hasConsented, IConfigManager configManager)
        {
            ConfigManager = configManager;
            _context = new Dictionary<string, object>();
            UpdateContext(context);
            Init(visitorID, isAuthenticated, hasConsented);
        }
        public VisitorDelegateAbstract(string visitorID, bool isAuthenticated, IDictionary<string, double> context, bool hasConsented, IConfigManager configManager)
        {
            ConfigManager = configManager;
            _context = new Dictionary<string, object>();
            UpdateContext(context);
            Init(visitorID, isAuthenticated, hasConsented);
        }

        public VisitorDelegateAbstract(string visitorID, bool isAuthenticated, IDictionary<string, bool> context, bool hasConsented, IConfigManager configManager)
        {
            ConfigManager = configManager;
            _context = new Dictionary<string, object>();
            UpdateContext(context);
            Init(visitorID, isAuthenticated, hasConsented);
        }

        protected void Init(string visitorID, bool isAuthenticated,  bool hasConsented)
        {
            Flags = new HashSet<FlagDTO>();
            VisitorId = visitorID ?? CreateVisitorId();
            SetConsent(hasConsented);
            LoadPredefinedContext();
        }
       
        protected string CreateVisitorId()
        {
            
            var date = DateTime.Now;
            return $"{date.Year}{Utils.Utils.TwoDigit(date.Month)}{Utils.Utils.TwoDigit(date.Day)}{Utils.Utils.TwoDigit(date.Hour)}{Utils.Utils.TwoDigit(date.Minute)}{Utils.Utils.TwoDigit(date.Second)}{new Random().Next(10000, 99999)}";
        }

        protected void LoadPredefinedContext()
        {
            Context[FsPredefinedContext.FLAGSHIP_CLIENT] = Constants.SDK_LANGUAGE;
            Context[FsPredefinedContext.FLAGSHIP_VERSION] = Constants.SDK_VERSION;
            //Context[FsPredefinedContext.FLAGSHIP_VISITOR] = VisitorId; 
        }

        virtual protected VisitorStrategyAbstract GetStrategy()
        {
            if (Flagship.Main.Flagship.Status == Enums.FlagshipStatus.NOT_INITIALIZED)
            {
                _strategy = _strategy!=null &&  _strategy.GetType().Name == typeof(NotReadyStrategy).Name ? _strategy: new NotReadyStrategy(this);
            }
            else if (Flagship.Main.Flagship.Status == Enums.FlagshipStatus.READY_PANIC_ON)
            {
                _strategy = _strategy != null && _strategy.GetType().Name == typeof(PanicStrategy).Name ? _strategy : new PanicStrategy(this);
            }
            else if (!HasConsented)
            {
                _strategy = _strategy != null && _strategy.GetType().Name == typeof(NoConsentStrategy).Name ? _strategy : new NoConsentStrategy(this);
            }
            else
            {
                _strategy = _strategy != null && _strategy.GetType().Name == typeof(DefaultStrategy).Name ? _strategy : new DefaultStrategy(this);
            }
            return _strategy;
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

        abstract public void UpdateContexCommon(IDictionary<string, object> context);
        abstract public void UpdateContext(IDictionary<string, string> context);
        abstract public void UpdateContext(IDictionary<string, double> context);
        abstract public void UpdateContext(IDictionary<string, bool> context);
        abstract public void UpdateContext(string key, string value);

        abstract public void UpdateContext(string key, double value);

        abstract public void UpdateContext(string key, bool value);
    }
}

﻿using Flagship.Config;
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
using Flagship.Main;

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
        virtual public uint Traffic { get; set; }
        virtual public string SessionId { get; set; }
        virtual public SdkInitialData SdkInitialData { get; set; }
        public FlagSyncStatus FlagSyncStatus { get; set; }
        public static FlagshipStatus SDKStatus { get; set; }

        public Troubleshooting ConsentHitTroubleshooting { get; set; }

        public Troubleshooting SegmentHitTroubleshooting { get; set; }


        public VisitorDelegateAbstract(string visitorID, bool isAuthenticated, IDictionary<string, object> context, bool hasConsented, IConfigManager configManager, SdkInitialData sdkInitialData = null)
        {
            SdkInitialData = sdkInitialData;
            ConfigManager = configManager;
            if (isAuthenticated && configManager.Config.DecisionMode == DecisionMode.DECISION_API)
            {
                AnonymousId = Guid.NewGuid().ToString();
            }
            SessionId = Guid.NewGuid().ToString();
            _context = new Dictionary<string, object>();
            UpdateContext(context);
            Flags = new HashSet<FlagDTO>();
            VisitorId = visitorID ?? CreateVisitorId();
            SetConsent(hasConsented);
            LoadPredefinedContext();

            GetStrategy().LookupVisitor();
            this.FlagSyncStatus = FlagSyncStatus.CREATED;
        }
       
        protected string CreateVisitorId()
        {
            return Guid.NewGuid().ToString();
        }

        public FlagshipStatus GetSdkStatus()
        {
            return SDKStatus;
        }

        protected void LoadPredefinedContext()
        {
            _context[PredefinedContext.FLAGSHIP_CLIENT] = Constants.SDK_LANGUAGE;
            _context[PredefinedContext.FLAGSHIP_VERSION] = Constants.SDK_VERSION;
            _context[PredefinedContext.FLAGSHIP_VISITOR] = VisitorId;  
        }

        virtual public VisitorStrategyAbstract GetStrategy()
        {
            VisitorStrategyAbstract strategy;
            if (Fs.Status == FlagshipStatus.NOT_INITIALIZED)
            {
                strategy = new NotReadyStrategy(this);
            }
            else if (Fs.Status == FlagshipStatus.READY_PANIC_ON)
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
            strategy.Murmur32 = Murmur.MurmurHash.Create32();
            return strategy;
        }

        virtual public void SetConsent(bool hasConsented)
        {
            _hasConsented = hasConsented;
            GetStrategy().SendConsentHitAsync(hasConsented).Wait();
        }

        abstract public void ClearContext();

        abstract public Task FetchFlags();

        abstract public IFlag<string> GetFlag(string key, string defaultValue);
        abstract public IFlag<long> GetFlag(string key, long defaultValue);
        abstract public IFlag<bool> GetFlag(string key, bool defaultValue);
        abstract public IFlag<JObject> GetFlag(string key, JObject defaultValue);
        abstract public IFlag<JArray> GetFlag(string key, JArray defaultValue);

        abstract public Task VisitorExposed<T>(string key, T defaultValue, FlagDTO flag);
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

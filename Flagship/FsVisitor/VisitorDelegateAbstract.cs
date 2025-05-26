using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Flagship.Config;
using Flagship.Delegate;
using Flagship.Enums;
using Flagship.FsFlag;
using Flagship.Hit;
using Flagship.Main;
using Flagship.Model;

namespace Flagship.FsVisitor
{
    internal abstract class VisitorDelegateAbstract : IVisitor
    {
        private readonly IDictionary<string, object> _context;
        private bool _hasConsented;
        protected string _anonymousId;
        private IFlagsStatus flagsStatus;

        public event OnFlagStatusChangedDelegate OnFlagsStatusChanged;

        public event OnFlagStatusFetchRequiredDelegate OnFlagStatusFetchRequired;

        public event OnFlagStatusFetchedDelegate OnFlagStatusFetched;

        public virtual string VisitorId { get; set; }
        public virtual ICollection<FlagDTO> Flags { get; set; }
        public virtual ICollection<Campaign> Campaigns { get; set; }
        public virtual bool HasConsented => _hasConsented;
        public virtual FlagshipConfig Config => ConfigManager.Config;
        public virtual IConfigManager ConfigManager { get; set; }
        public virtual IDictionary<string, object> Context => _context;
        public virtual string AnonymousId
        {
            get => _anonymousId;
            internal set { _anonymousId = value; }
        }
        public virtual VisitorCache VisitorCache { get; set; }
        public virtual uint Traffic { get; set; }
        public virtual string SessionId { get; set; }
        public virtual SdkInitialData SdkInitialData { get; set; }
        public static FSSdkStatus SDKStatus { get; set; }
        public virtual IFlagsStatus FlagsStatus
        {
            get => flagsStatus;
            internal set
            {
                flagsStatus = value;
                OnFlagsStatusChanged?.Invoke(value);
                if (value.Status == FSFlagStatus.FETCH_REQUIRED)
                {
                    OnFlagStatusFetchRequired?.Invoke(value.Reason);
                }
                else if (value.Status == FSFlagStatus.FETCHED)
                {
                    OnFlagStatusFetched?.Invoke();
                }
            }
        }

        public Troubleshooting ConsentHitTroubleshooting { get; set; }

        public Troubleshooting SegmentHitTroubleshooting { get; set; }

        public bool HasContextBeenUpdated { get; set; }
        public ConcurrentDictionary<string, TimeSpan> DeDuplicationCache { get; set; }

        internal VisitorCacheStatus VisitorCacheStatus { get; set; }

        public VisitorDelegateAbstract(
            string visitorID,
            bool isAuthenticated,
            IDictionary<string, object> context,
            bool hasConsented,
            IConfigManager configManager,
            SdkInitialData sdkInitialData = null
        )
        {
            DeDuplicationCache = new ConcurrentDictionary<string, TimeSpan>();
            SdkInitialData = sdkInitialData;
            ConfigManager = configManager;
            if (isAuthenticated && configManager.Config.DecisionMode == DecisionMode.DECISION_API)
            {
                AnonymousId = Guid.NewGuid().ToString();
            }
            SessionId = Guid.NewGuid().ToString();
            _context = new Dictionary<string, object>();
            UpdateContext(context);
            HasContextBeenUpdated = true;
            Flags = new HashSet<FlagDTO>();
            VisitorId = visitorID ?? CreateVisitorId();
            SetConsent(hasConsented);
            LoadPredefinedContext();

            FlagsStatus = new FlagsStatus
            {
                Reason = FSFetchReasons.FLAGS_NEVER_FETCHED,
                Status = FSFlagStatus.FETCH_REQUIRED,
            };
        }

        protected string CreateVisitorId()
        {
            return Guid.NewGuid().ToString();
        }

        public FSSdkStatus GetSdkStatus()
        {
            return SDKStatus;
        }

        protected void LoadPredefinedContext()
        {
            _context[PredefinedContext.FLAGSHIP_CLIENT] = Constants.SDK_LANGUAGE;
            _context[PredefinedContext.FLAGSHIP_VERSION] = Constants.SDK_VERSION;
            _context[PredefinedContext.FLAGSHIP_VISITOR] = VisitorId;
        }

        public virtual StrategyAbstract GetStrategy()
        {
            StrategyAbstract strategy;
            if (Fs.Status == FSSdkStatus.SDK_NOT_INITIALIZED)
            {
                strategy = new NotReadyStrategy(this);
            }
            else if (Fs.Status == FSSdkStatus.SDK_PANIC)
            {
                strategy = new PanicStrategy(this);
            }
            else if (!HasConsented)
            {
                strategy = new NoConsentStrategy(this);
            }
            else
            {
                strategy = new DefaultStrategy(this);
            }
            strategy.Murmur32 = Murmur.MurmurHash.Create32();
            return strategy;
        }

        public virtual void SetConsent(bool hasConsented)
        {
            _hasConsented = hasConsented;
            GetStrategy().SendConsentHitAsync(hasConsented).Wait();
        }

        public abstract void ClearContext();

        public abstract Task FetchFlags();

        public abstract IFlag GetFlag(string key);

        public abstract IFlagCollection GetFlags();

        public abstract Task VisitorExposed<T>(
            string key,
            T defaultValue,
            FlagDTO flag,
            bool hasGetValueBeenCalled = false
        );
        public abstract T GetFlagValue<T>(
            string key,
            T defaultValue,
            FlagDTO flag,
            bool visitorExposed
        );
        public abstract IFlagMetadata GetFlagMetadata(string key, FlagDTO flag);
        public abstract Task SendHit(HitAbstract hit);

        public abstract void UpdateContext(IDictionary<string, object> context);
        public abstract void UpdateContext(string key, string value);

        public abstract void UpdateContext(string key, double value);

        public abstract void UpdateContext(string key, bool value);

        public virtual void Authenticate(string visitorId)
        {
            GetStrategy().Authenticate(visitorId);
        }

        public virtual void Unauthenticate()
        {
            GetStrategy().Unauthenticate();
        }
    }
}

using Flagship.Api;
using Flagship.Config;
using Flagship.Decision;
using Flagship.Enums;
using Flagship.FsFlag;
using Flagship.FsVisitor;
using Flagship.Hit;
using Flagship.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flagship.FsVisitor
{
    internal abstract class VisitorStrategyAbstract : IVisitorCore
    {
        protected VisitorDelegateAbstract Visitor { get; set; }

        protected FlagshipConfig Config => Visitor.Config;

        protected ITrackingManager TrackingManager => Visitor.ConfigManager.TrackingManager;

        protected IDecisionManager DecisionManager => Visitor.ConfigManager.DecisionManager;

        public VisitorStrategyAbstract(VisitorDelegateAbstract visitor)
        {
            Visitor = visitor;
        }

        virtual public async Task SendConsentHitAsync(bool hasConsented) 
        {
            const string method = "SendConsentHit";
            try
            {
                var hitEvent = new Event(EventCategory.USER_ENGAGEMENT, "fs_consent")
                {
                    Label = $"{Constants.SDK_LANGUAGE}:{hasConsented}",
                    VisitorId = Visitor.VisitorId,
                    DS = Constants.SDK_APP,
                    Config = Config,
                    AnonymousId = Visitor.AnonymousId
                };
               await TrackingManager.SendHit(hitEvent);
            }
            catch (Exception ex)
            {
                Utils.Log.LogError(Config, ex.Message, method);
            }
        }

        public async Task LookupVisitor()
        {
            try
            {
                var visitorCacheInstance = Config.VisitorCacheImplementation;
                if (Config.DisableCache && visitorCacheInstance == null)
                {
                    return;
                }

                var visitorCacheStringData = await visitorCacheInstance.LookupVisitor(Visitor.VisitorId);
                if (visitorCacheStringData == null)
                {
                    return;
                }

                var visitorCache = Newtonsoft.Json.JsonConvert.DeserializeObject<VisitorCacheDTO>(visitorCacheStringData);
            }
            catch (Exception ex)
            {
                Utils.Log.LogError(Config, ex.Message, "LookupVisitor");
            }
            

        }
        abstract public void ClearContext();

        abstract public Task FetchFlags();

        abstract public Task UserExposed<T>(string key, T defaultValue, FlagDTO flag); 
        abstract public T GetFlagValue<T>(string key, T defaultValue, FlagDTO flag, bool userExposed);
        abstract public IFlagMetadata GetFlagMetadata(IFlagMetadata metadata, string key, bool hasSameType);

        abstract public Task SendHit(HitAbstract hit);

        abstract public void UpdateContext(IDictionary<string, object> context);

        abstract public void UpdateContext(string key, string value);

        abstract public void UpdateContext(string key, double value);

        abstract public void UpdateContext(string key, bool value);

        abstract public void Authenticate(string visitorId);

        abstract public void Unauthenticate();
    }
}

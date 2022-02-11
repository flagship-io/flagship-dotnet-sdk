using Flagship.Api;
using Flagship.Config;
using Flagship.Decision;
using Flagship.Enums;
using Flagship.FsFlag;
using Flagship.FsVisitor;
using Flagship.Hit;
using Flagship.Model;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
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

        protected virtual void MigrateVisitorCacheData(string visitorDataString)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(visitorDataString))
                {
                    return;
                }
                var parseData = JObject.Parse(visitorDataString);
                if (!parseData.ContainsKey("Version"))
                {
                    throw new Exception("JSON DATA must fit the type VisitorCacheDTO, property version is required");
                }

                var version = parseData["Version"];
                if (version.ToString() == "1")
                {
                    var data = Newtonsoft.Json.JsonConvert.DeserializeObject<VisitorCacheDTOV1>(visitorDataString);
                    Visitor.VisitorCache = new VisitorCache
                    {
                        Version = 1,
                        Data = data
                    };
                }
            }
            catch (Exception ex)
            {
                Utils.Log.LogError(Config, ex.Message, "LookupVisitor");
            }
        }

        public virtual void LookupVisitor()
        {
            try
            {
                var visitorCacheInstance = Config.VisitorCacheImplementation;
                if (Config.DisableCache || visitorCacheInstance == null)
                {
                    return;
                }

                var timeout = visitorCacheInstance?.LookupTimeout;

                var cts = new CancellationTokenSource();

                cts.CancelAfter(timeout ?? TimeSpan.FromMilliseconds(1));
                
                var lookupTask = visitorCacheInstance.LookupVisitor(Visitor.VisitorId);
                lookupTask.Wait(cts.Token);

                var visitorCacheStringData = lookupTask.Result;

                MigrateVisitorCacheData(visitorCacheStringData);
            }
            catch (Exception ex)
            {
                Utils.Log.LogError(Config, ex.Message, "LookupVisitor");
            }
        }

        public virtual async void CacheVisitorAsync()
        {
            try
            {
                var visitorCacheInstance = Config.VisitorCacheImplementation;
                if (Config.DisableCache || visitorCacheInstance==null)
                {
                    return;
                }

                var Campaigns = new Collection<VisitorCacheCampaign>();

                foreach (var item in Visitor.Campaigns)
                {
                    Campaigns.Add(new VisitorCacheCampaign { 
                        CampaignId = item.Id,
                        VariationGroupId = item.VariationGroupId,
                        VariationId = item.Variation.Id,
                        IsReference = item.Variation.Reference,
                        Type = item.Variation.Modifications.Type,
                        Activated = false,
                        Flags = item.Variation.Modifications.Value
                    });
                }

                var data = new VisitorCacheDTOV1
                {
                    Version = 1,
                    Data = new VisitorCacheData
                    {
                        VisitorId = Visitor.VisitorId,
                        AnonymousId = Visitor.AnonymousId,
                        Consent = Visitor.HasConsented,
                        Context = Visitor.Context,
                        Campaigns = Campaigns
                    }
                };

                var dataString = Newtonsoft.Json.JsonConvert.SerializeObject(data);

                await visitorCacheInstance.CacheVisitor(Visitor.VisitorId, dataString);
            }
            catch (Exception ex)
            {
                Utils.Log.LogError(Config, ex.Message, "CacheVisitor");
            }
        }

        public virtual async void FlushVisitorAsync() 
        {
            try
            {
                var visitorCacheInstance = Config.VisitorCacheImplementation;
                if (Config.DisableCache && visitorCacheInstance == null)
                {
                    return;
                }
                await visitorCacheInstance.FlushVisitor(Visitor.VisitorId);
            }
            catch (Exception ex)
            {
                Utils.Log.LogError(Config, ex.Message, "FlushVisitor");
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

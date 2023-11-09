using Flagship.Api;
using Flagship.Config;
using Flagship.Decision;
using Flagship.Enums;
using Flagship.FsFlag;
using Flagship.FsVisitor;
using Flagship.Hit;
using Flagship.Model;
using Murmur;
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
        public const string LOOKUP_HITS_JSON_OBJECT_ERROR = "JSON DATA must fit the type HitCacheDTO";
        public const string LOOKUP_VISITOR_JSON_OBJECT_ERROR = "JSON DATA must fit the type VisitorCacheDTO, property version is required";
        public const string VISITOR_ID_MISMATCH_ERROR = "Visitor ID mismatch: {0} vs {1}";
        public const int HIT_BATCH_LENGTH = 2621440;
        protected VisitorDelegateAbstract Visitor { get; set; }

        protected FlagshipConfig Config => Visitor.Config;

        protected ITrackingManager TrackingManager => Visitor.ConfigManager.TrackingManager;

        protected IDecisionManager DecisionManager => Visitor.ConfigManager.DecisionManager;

        public Murmur32  Murmur32 { get; set; }

        public VisitorStrategyAbstract(VisitorDelegateAbstract visitor)
        {
            Visitor = visitor;
        }

        virtual public async Task SendConsentHitAsync(bool hasConsented)
        {
            if (!hasConsented)
            {
                Visitor.GetStrategy().FlushVisitorAsync();
            }

            var hitEvent = new Event(EventCategory.USER_ENGAGEMENT, Constants.FS_CONSENT)
            {
                Label = $"{Constants.SDK_LANGUAGE}:{hasConsented}",
                VisitorId = Visitor.VisitorId,
                DS = Constants.SDK_APP,
                Config = Config,
                AnonymousId = Visitor.AnonymousId
            };

            await TrackingManager.Add(hitEvent);

        }

        protected virtual void MigrateVisitorCacheData(JObject visitorData)
        {
            try
            {
                if (visitorData == null)
                {
                    return;
                }
                
                if (!visitorData.ContainsKey("Version"))
                {
                    throw new Exception(LOOKUP_VISITOR_JSON_OBJECT_ERROR);
                }

                var version = visitorData["Version"];
                if (version.ToString() == "1")
                {
                    var data = visitorData.ToObject<VisitorCacheDTOV1>();
                    if (data.Data.VisitorId != Visitor.VisitorId)
                    {
                        Logger.Log.LogInfo(Config, string.Format(VISITOR_ID_MISMATCH_ERROR, data.Data.VisitorId, Visitor.VisitorId), "LookupVisitor");
                        return;
                    }
                    Visitor.VisitorCache = new VisitorCache
                    {
                        Version = 1,
                        Data = data
                    };
                }
            }
            catch (Exception ex)
            {
                Logger.Log.LogError(Config, ex.Message, "LookupVisitor");
            }
        }

        public virtual void LookupVisitor()
        {
            try
            {
                var visitorCacheInstance = Config?.VisitorCacheImplementation;
                if (visitorCacheInstance == null || Config.DisableCache)
                {
                    return;
                }

                var timeout = visitorCacheInstance?.LookupTimeout;

                var cts = new CancellationTokenSource();

                cts.CancelAfter(timeout ?? TimeSpan.FromMilliseconds(10));

                var lookupTask = visitorCacheInstance.LookupVisitor(Visitor.VisitorId);
                lookupTask.Wait(cts.Token);

                var visitorCacheStringData = lookupTask.Result;

                MigrateVisitorCacheData(visitorCacheStringData);
            }
            catch (Exception ex)
            {
                Logger.Log.LogError(Config, ex.Message, "LookupVisitor");
            }
        }

        public virtual async void CacheVisitorAsync()
        {
            try
            {
                var visitorCacheInstance = Config?.VisitorCacheImplementation;
                if (visitorCacheInstance == null || Config.DisableCache)
                {
                    return;
                }

                var Campaigns = new Collection<VisitorCacheCampaign>();
                var assignmentsHistory = new Dictionary<string, string>();

                var visitorCacheData = (VisitorCacheDTOV1)Visitor.VisitorCache?.Data;
                if (visitorCacheData != null)
                {
                    foreach (var item in visitorCacheData.Data.AssignmentsHistory)
                    {
                        assignmentsHistory[item.Key] = item.Value;
                    }
                }

                foreach (var item in Visitor.Campaigns)
                {
                    assignmentsHistory[item.VariationGroupId] = item.Variation.Id;

                    Campaigns.Add(new VisitorCacheCampaign
                    {
                        CampaignId = item.Id,
                        VariationGroupId = item.VariationGroupId,
                        VariationId = item.Variation.Id,
                        IsReference = item.Variation.Reference,
                        Type = item.Variation.Modifications.Type,
                        Activated = false,
                        Flags = item.Variation.Modifications.Value,
                        Slug = item.Slug
                       
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
                        Campaigns = Campaigns,
                        AssignmentsHistory = assignmentsHistory,
                    }
                };

                var dataJson = JObject.FromObject(data);

                await visitorCacheInstance.CacheVisitor(Visitor.VisitorId, dataJson);

                Visitor.VisitorCache = new VisitorCache
                {
                    Version = 1,
                    Data = data
                };
            }
            catch (Exception ex)
            {
                Logger.Log.LogError(Config, ex.Message, "CacheVisitor");
            }
        }

        public virtual async void FlushVisitorAsync()
        {
            try
            {
                var visitorCacheInstance = Config?.VisitorCacheImplementation;
                if (visitorCacheInstance == null || Config.DisableCache)
                {
                    return;
                }
                await visitorCacheInstance.FlushVisitor(Visitor.VisitorId);
            }
            catch (Exception ex)
            {
                Logger.Log.LogError(Config, ex.Message, "FlushVisitor");
            }
        }

        abstract public void ClearContext();

        abstract public Task FetchFlags();

        abstract protected Task SendActivate(FlagDTO flag, object defaultValue = null);

        abstract public Task UserExposed<T>(string key, T defaultValue, FlagDTO flag);
        abstract public T GetFlagValue<T>(string key, T defaultValue, FlagDTO flag, bool userExposed);
        abstract public IFlagMetadata GetFlagMetadata(IFlagMetadata metadata, string key, bool hasSameType);

        abstract public Task SendHit(HitAbstract hit);

        abstract public Task SendHit(IEnumerable<HitAbstract> hit);

        abstract public void UpdateContext(IDictionary<string, object> context);

        abstract public void UpdateContext(string key, string value);

        abstract public void UpdateContext(string key, double value);

        abstract public void UpdateContext(string key, bool value);

        abstract public void Authenticate(string visitorId);

        abstract public void Unauthenticate();
    }
}

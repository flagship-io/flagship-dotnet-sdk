using Flagship.Config;
using Flagship.Enums;
using Flagship.FsFlag;
using Flagship.Hit;
using Flagship.Logger;
using Flagship.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Flagship.FsVisitor
{
    internal class DefaultStrategy : VisitorStrategyAbstract
    {
        public static string FETCH_FLAGS_STARTED = "visitor {0} fetchFlags process is started";
        public static string FETCH_CAMPAIGNS_SUCCESS = "Visitor {0}, anonymousId {1} with context {2} has just fetched campaigns {3} in {4} ms";
        public static string FETCH_CAMPAIGNS_FROM_CACHE = "Visitor {0}, anonymousId {1} with context {2} has just fetched campaigns from cache {3} in {4} ms";
        public static string FETCH_FLAGS_FROM_CAMPAIGNS = "Visitor {0}, anonymousId {1} with context {2} has just fetched flags {3} from Campaigns";
        public DefaultStrategy(VisitorDelegateAbstract visitor) : base(visitor)
        {
        }

        virtual protected void UpdateContexKeyValue(string key, object value)
        {
            if (PredefinedContext.IsPredefinedContext(key) && !PredefinedContext.CheckType(key, value))
            {
                Log.LogError(
                    Config,
                    string.Format(Constants.PREDEFINED_CONTEXT_TYPE_ERROR, key, PredefinedContext.GetPredefinedType(key)),
                    "UpdateContext");
                return;
            }

            if (!(value is string) && !(value is bool) && !(value is double) && !(value is long) && !(value is int))
            {
                Log.LogError(Config, string.Format(Constants.CONTEXT_PARAM_ERROR, key), "UpdateContex");
                return;
            }

            if (Regex.IsMatch(key, @"^fs_", RegexOptions.IgnoreCase))
            {
                return;
            }

            Visitor.Context[key] = value;
            this.Visitor.FlagSyncStatus = FlagSyncStatus.CONTEXT_UPDATED;
        }

        public override void UpdateContext(IDictionary<string, object> context)
        {
            if (context == null)
            {
                return;
            }
            foreach (var item in context)
            {
                UpdateContexKeyValue(item.Key, item.Value);
            }
        }

        public override void UpdateContext(string key, string value)
        {
            UpdateContexKeyValue(key, value);
        }

        public override void UpdateContext(string key, double value)
        {
            UpdateContexKeyValue(key, value);
        }

        public override void UpdateContext(string key, bool value)
        {
            UpdateContexKeyValue(key, value);
        }

        public override void ClearContext()
        {
            Visitor.Context.Clear();
        }

        protected virtual ICollection<Campaign> FetchVisitorCacheCampaigns(VisitorDelegateAbstract visitor)
        {
            var campaigns = new Collection<Campaign>();
            if (visitor.VisitorCache == null || visitor.VisitorCache.Data == null)
            {
                return campaigns;
            }
            if (visitor.VisitorCache.Version == 1)
            {
                var data = (VisitorCacheDTOV1)visitor.VisitorCache.Data;

                foreach (var item in data.Data.Campaigns)
                {
                    campaigns.Add(new Campaign
                    {
                        Id = item.CampaignId,
                        VariationGroupId = item.VariationGroupId,
                        Variation = new Variation
                        {
                            Id = item.VariationId,
                            Reference = item.IsReference ?? false,
                            Modifications = new Modifications
                            {
                                Type = item.Type,
                                Value = item.Flags
                            }
                        }
                    });
                }

                return campaigns;
            }

            return campaigns;
        }

        async public override Task FetchFlags()
        {
            const string FUNCTION_NAME = "FetchFlags";
            Log.LogDebug(Config, string.Format(FETCH_FLAGS_STARTED, Visitor.VisitorId), FUNCTION_NAME);
            ICollection<Campaign> campaigns = new List<Campaign>();
            var now = DateTime.Now;
            try
            {
                campaigns = await DecisionManager.GetCampaigns(Visitor);

                Log.LogDebug(Config, string.Format(FETCH_CAMPAIGNS_SUCCESS,
                    Visitor.VisitorId, Visitor.AnonymousId,
                    JsonConvert.SerializeObject(Visitor.Context),
                    JsonConvert.SerializeObject(campaigns), (DateTime.Now - now).Milliseconds), FUNCTION_NAME);
            }
            catch (Exception ex)
            {
                Log.LogError(Config, ex.Message, FUNCTION_NAME);
            }
            try
            {
                if (campaigns.Count == 0)
                {
                    campaigns = FetchVisitorCacheCampaigns(Visitor);
                    if (campaigns.Count > 0)
                    {
                        Log.LogDebug(Config, string.Format(FETCH_CAMPAIGNS_FROM_CACHE,
                    Visitor.VisitorId, Visitor.AnonymousId,
                    JsonConvert.SerializeObject(Visitor.Context),
                    JsonConvert.SerializeObject(campaigns), (DateTime.Now - now).Milliseconds), FUNCTION_NAME);
                    }
                }

                Visitor.Campaigns = campaigns;
                Visitor.Flags = await DecisionManager.GetFlags(campaigns);
                Visitor.GetStrategy().CacheVisitorAsync();

                Visitor.FlagSyncStatus = FlagSyncStatus.FLAGS_FETCHED;

                Log.LogDebug(Config, string.Format(FETCH_FLAGS_FROM_CAMPAIGNS,
                    Visitor.VisitorId, Visitor.AnonymousId,
                    JsonConvert.SerializeObject(Visitor.Context),
                    JsonConvert.SerializeObject(Visitor.Flags)), FUNCTION_NAME);

                if (DecisionManager?.TroubleshootingData != null)
                {
                    _ = SendFetchFlagsTroubleshootingHit(campaigns, now);
                    _ = SendConsentHitTroubleshooting();
                    _ = SendSegmentHitTroubleshooting();
                }

                _ = SendAnalyticHit();
             
            }
            catch (Exception ex)
            {
                Log.LogError(Config, ex.Message, "FetchFlags");

                var fetchFlagTroubleshootingHit = new Troubleshooting()
                {
                    Label = DiagnosticLabel.VISITOR_FETCH_CAMPAIGNS,
                    LogLevel = LogLevel.INFO,
                    VisitorId = Visitor.VisitorId,
                    AnonymousId = Visitor.AnonymousId,
                    VisitorSessionId = Visitor.SessionId,
                    FlagshipInstanceId = Visitor.SdkInitialData?.InstanceId,
                    Traffic = Visitor.Traffic,
                    Config = Config,
                    SdkStatus = Visitor.GetSdkStatus(),
                    VisitorContext = Visitor.Context,
                    VisitorCampaigns = campaigns,
                    VisitorConsent = Visitor.HasConsented,
                    VisitorIsAuthenticated = string.IsNullOrEmpty(Visitor.AnonymousId),
                    VisitorFlags = Visitor.Flags,
                    LastBucketingTimestamp = "",
                    LastInitializationTimestamp = Visitor.SdkInitialData?.LastInitializationTimestamp,
                    HttpResponseTime = (DateTime.Now - now).Milliseconds,

                    SdkConfigMode = Config.DecisionMode,
                    SdkConfigTimeout = Config.Timeout,
                    SdkConfigTrackingManagerConfigStrategy = Config.TrackingManagerConfig.CacheStrategy,
                    SdkConfigTrackingManagerConfigBatchIntervals = Config.TrackingManagerConfig.BatchIntervals,
                    SdkConfigTrackingManagerConfigPoolMaxSize = Config.TrackingManagerConfig.PoolMaxSize,
                    SdkConfigUsingCustomHitCache = Config.HitCacheImplementation != null,
                    SdkConfigUsingCustomVisitorCache = Config.VisitorCacheImplementation != null,
                    SdkConfigUsingOnVisitorExposed = Config.HasOnVisitorExposed(),
                    SdkConfigDisableCache = Config.DisableCache
                };

                if (Config is BucketingConfig bucketingConfig)
                {
                    fetchFlagTroubleshootingHit.SdkConfigPollingInterval = bucketingConfig.PollingInterval;
                }

                _ = TrackingManager.SendTroubleshootingHit(fetchFlagTroubleshootingHit);
            }
        }



        protected override async Task SendActivate(FlagDTO flag, object defaultValue = null)
        {
            var activate = new Activate(flag.VariationGroupId, flag.VariationId)
            {
                VisitorId = Visitor.VisitorId,
                AnonymousId = Visitor.AnonymousId,
                Config = Config,
                FlagKey = flag.Key,
                FlagValue = flag.Value,
                FlagDefaultValue = defaultValue,
                VisitorContext = Visitor.Context,
                FlagMetadata = new FlagMetadata(flag.CampaignId, flag.VariationGroupId, flag.VariationId, flag.IsReference, flag.CampaignType, flag.Slug, flag.CampaignName, flag.VariationGroupName, flag.VariationName)
            };

            await TrackingManager.ActivateFlag(activate);

            var activateTroubleshooting = new Troubleshooting()
            {
                Label = DiagnosticLabel.VISITOR_SEND_ACTIVATE,
                LogLevel = LogLevel.INFO,
                Traffic = Visitor.Traffic,
                VisitorSessionId = Visitor.SessionId,
                FlagshipInstanceId = Visitor.SdkInitialData?.InstanceId,
                AnonymousId = Visitor.AnonymousId,
                VisitorId = Visitor.VisitorId,
                Config = Config,
                HitContent = activate.ToApiKeys()
            };

            _= TrackingManager.SendTroubleshootingHit(activateTroubleshooting);
        }
        public override async Task UserExposed<T>(string key, T defaultValue, FlagDTO flag)
        {
            const string functionName = "UserExposed";
            if (flag == null)
            {
                Log.LogError(Config, string.Format(Constants.GET_FLAG_ERROR, key), functionName);

                var troubleshootingHit = new Troubleshooting()
                {
                    Label = DiagnosticLabel.VISITOR_EXPOSED_FLAG_NOT_FOUND,
                    LogLevel = LogLevel.INFO,
                    Traffic = Visitor.Traffic,
                    VisitorSessionId = Visitor.SessionId,
                    FlagshipInstanceId = Visitor.SdkInitialData?.InstanceId,
                    AnonymousId = Visitor.AnonymousId,
                    VisitorId = Visitor.VisitorId,
                    VisitorContext = Visitor.Context,
                    Config = Config,
                    FlagKey = key,
                    FlagDefaultValue = defaultValue,
                };

                _ = TrackingManager.SendTroubleshootingHit(troubleshootingHit);

                return;
            }
            if (flag.Value != null && defaultValue != null && !Utils.Utils.HasSameType(flag.Value, defaultValue))
            {
                Log.LogError(Config, string.Format(Constants.USER_EXPOSED_CAST_ERROR, key), functionName);
                var troubleshootingHit = new Troubleshooting()
                {
                    Label = DiagnosticLabel.VISITOR_EXPOSED_TYPE_WARNING,
                    LogLevel = LogLevel.INFO,
                    Traffic = Visitor.Traffic,
                    VisitorSessionId = Visitor.SessionId,
                    FlagshipInstanceId = Visitor.SdkInitialData?.InstanceId,
                    AnonymousId = Visitor.AnonymousId,
                    VisitorId = Visitor.VisitorId,
                    VisitorContext = Visitor.Context,
                    Config = Config,
                    FlagKey = key,
                    FlagDefaultValue = defaultValue,
                };

                _ = TrackingManager.SendTroubleshootingHit(troubleshootingHit);

                return;
            }

            await SendActivate(flag, defaultValue);
        }

        public override T GetFlagValue<T>(string key, T defaultValue, FlagDTO flag, bool userExposed = true)
        {
            const string functionName = "getFlag.value";

            if (flag == null)
            {
                Log.LogInfo(Config, string.Format(Constants.GET_FLAG_MISSING_ERROR, key), functionName);

                var troubleshootingHit = new Troubleshooting()
                {
                    Label = DiagnosticLabel.GET_FLAG_VALUE_FLAG_NOT_FOUND,
                    LogLevel = LogLevel.INFO,
                    Traffic = Visitor.Traffic,
                    VisitorSessionId = Visitor.SessionId,
                    FlagshipInstanceId = Visitor.SdkInitialData?.InstanceId,
                    AnonymousId = Visitor.AnonymousId,
                    VisitorId = Visitor.VisitorId,
                    VisitorContext = Visitor.Context,
                    Config = Config,
                    FlagKey = key,
                    FlagDefaultValue = defaultValue,
                    VisitorExposed = userExposed
                };

                _ = TrackingManager.SendTroubleshootingHit(troubleshootingHit);

                return defaultValue;
            }

            if (flag.Value == null)
            {
                if (userExposed)
                {
                    _ = UserExposed(key, defaultValue, flag);
                }
                return defaultValue;
            }

            if (defaultValue != null && !Utils.Utils.HasSameType(flag.Value, defaultValue))
            {
                Log.LogInfo(Config, string.Format(Constants.GET_FLAG_CAST_ERROR, key), functionName);

                var troubleshootingHit = new Troubleshooting()
                {
                    Label = DiagnosticLabel.GET_FLAG_VALUE_TYPE_WARNING,
                    LogLevel = LogLevel.INFO,
                    Traffic = Visitor.Traffic,
                    VisitorSessionId = Visitor.SessionId,
                    FlagshipInstanceId = Visitor.SdkInitialData?.InstanceId,
                    AnonymousId = Visitor.AnonymousId,
                    VisitorId = Visitor.VisitorId,
                    VisitorContext = Visitor.Context,
                    Config = Config,
                    FlagKey = key,
                    FlagDefaultValue = defaultValue,
                    VisitorExposed = userExposed
                };

                _ = TrackingManager.SendTroubleshootingHit(troubleshootingHit);

                return defaultValue;
            }

            if (userExposed)
            {
                _ = UserExposed(key, defaultValue, flag);
            }

            return (T)flag.Value;
        }

        public override IFlagMetadata GetFlagMetadata(IFlagMetadata metadata, string key, bool hasSameType)
        {
            const string functionName = "flag.metadata";
            if (!hasSameType && !string.IsNullOrWhiteSpace(metadata.CampaignId))
            {
                Log.LogError(Config, string.Format(Constants.GET_METADATA_CAST_ERROR, key), functionName);

                var troubleshootingHit = new Troubleshooting()
                {
                    Label = DiagnosticLabel.GET_FLAG_VALUE_TYPE_WARNING,
                    LogLevel = LogLevel.INFO,
                    Traffic = Visitor.Traffic,
                    VisitorSessionId = Visitor.SessionId,
                    FlagshipInstanceId = Visitor.SdkInitialData?.InstanceId,
                    AnonymousId = Visitor.AnonymousId,
                    VisitorId = Visitor.VisitorId,
                    VisitorContext = Visitor.Context,
                    FlagKey = key,
                    Config = Config,
                    FlagMetadataCampaignId = metadata.CampaignId,
                    FlagMetadataCampaignName = metadata.CampaignName,
                    FlagMetadataCampaignIsReference = metadata.IsReference,
                    FlagMetadataCampaignSlug = metadata.Slug,
                    FlagMetadataCampaignType = metadata.CampaignType,
                    FlagMetadataVariationGroupId = metadata.VariationGroupId,
                    FlagMetadataVariationGroupName = metadata.VariationGroupName,
                    FlagMetadataVariationId = metadata.VariationId,
                    FlagMetadataVariationName = metadata.VariationName
                };

                _ = TrackingManager.SendTroubleshootingHit(troubleshootingHit);

                return FlagMetadata.EmptyMetadata();
            }
            return metadata;
        }

        public override async Task SendHit(IEnumerable<HitAbstract> hits)
        {
            foreach (var item in hits)
            {
                await SendHit(item);
            }
        }
        public override async Task SendHit(HitAbstract hit)
        {
            const string functionName = "SendHit";
            try
            {
                if (hit == null)
                {
                    Log.LogError(Config, Constants.HIT_NOT_NULL, functionName);
                    return;
                }

                hit.VisitorId = Visitor.VisitorId;
                hit.DS = Constants.SDK_APP;
                hit.Config = Config;
                hit.AnonymousId = Visitor.AnonymousId;

                if (!hit.IsReady())
                {
                    Log.LogError(Config, hit.GetErrorMessage(), functionName);
                    return;
                }

                await TrackingManager.Add(hit);

                if (hit.Type == HitType.SEGMENT)
                {
                    return;
                }

                var troubleshootingHit = new Troubleshooting()
                {
                    Label = DiagnosticLabel.VISITOR_SEND_HIT,
                    LogLevel = LogLevel.INFO,
                    Traffic = Visitor.Traffic,
                    VisitorSessionId = Visitor.SessionId,
                    FlagshipInstanceId = Visitor.SdkInitialData?.InstanceId,
                    AnonymousId = Visitor.AnonymousId,
                    VisitorId = Visitor.VisitorId,
                    Config = Config,
                    HitContent = hit.ToApiKeys()
                };

                _ = TrackingManager.SendTroubleshootingHit(troubleshootingHit);
            }
            catch (Exception ex)
            {
                Log.LogError(Config, ex.Message, functionName);
            }
        }

        public override void Authenticate(string visitorId)
        {
            const string methodName = "Authenticate";
            if (Config.DecisionMode == DecisionMode.BUCKETING)
            {
                LogDeactivateOnBucketingMode(methodName);
                return;
            }

            if (string.IsNullOrWhiteSpace(visitorId))
            {
                Log.LogError(Config, string.Format(Constants.VISITOR_ID_ERROR, methodName), methodName);
                return;
            }

            Visitor.AnonymousId = Visitor.VisitorId;
            Visitor.VisitorId = visitorId;
            Visitor.FlagSyncStatus = FlagSyncStatus.AUTHENTICATED;

            var troubleshootingHit = new Troubleshooting()
            {
                Label = DiagnosticLabel.VISITOR_AUTHENTICATE,
                LogLevel = LogLevel.INFO,
                Traffic = Visitor.Traffic,
                VisitorSessionId = Visitor.SessionId,
                FlagshipInstanceId = Visitor.SdkInitialData?.InstanceId,
                AnonymousId = Visitor.AnonymousId,
                VisitorId = Visitor.VisitorId,
                VisitorContext = Visitor.Context,
                Config = Config,
            };

            _ = TrackingManager.SendTroubleshootingHit(troubleshootingHit);
        }

        public override void Unauthenticate()
        {
            const string methodName = "Unauthenticate";
            if (Config.DecisionMode == DecisionMode.BUCKETING)
            {
                LogDeactivateOnBucketingMode(methodName);
                return;
            }

            if (string.IsNullOrWhiteSpace(Visitor.AnonymousId))
            {
                Log.LogError(Config, string.Format(Constants.FLAGSHIP_VISITOR_NOT_AUTHENTICATE, methodName), methodName);
                return;
            }

            Visitor.VisitorId = Visitor.AnonymousId;
            Visitor.AnonymousId = null;
            Visitor.FlagSyncStatus = FlagSyncStatus.UNAUTHENTICATED;

            var troubleshootingHit = new Troubleshooting()
            {
                Label = DiagnosticLabel.VISITOR_UNAUTHENTICATE,
                LogLevel = LogLevel.INFO,
                Traffic = Visitor.Traffic,
                VisitorSessionId = Visitor.SessionId,
                FlagshipInstanceId = Visitor.SdkInitialData?.InstanceId,
                AnonymousId = Visitor.AnonymousId,
                VisitorId = Visitor.VisitorId,
                VisitorContext = Visitor.Context,
                Config = Config,
            };

            _ = TrackingManager.SendTroubleshootingHit(troubleshootingHit);

        }

        protected void LogDeactivateOnBucketingMode(string methodName)
        {
            Log.LogError(Config, string.Format(Constants.METHOD_DEACTIVATED_BUCKETING_ERROR, methodName), methodName);
        }
    }
}

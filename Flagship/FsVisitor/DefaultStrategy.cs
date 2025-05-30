﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Flagship.Config;
using Flagship.Enums;
using Flagship.FsFlag;
using Flagship.Hit;
using Flagship.Logger;
using Flagship.Model;
using Newtonsoft.Json;

namespace Flagship.FsVisitor
{
    internal class DefaultStrategy : StrategyAbstract
    {
        public static string FETCH_FLAGS_STARTED = "visitor {0} fetchFlags process is started";
        public static string FETCH_CAMPAIGNS_SUCCESS =
            "Visitor {0}, anonymousId {1} with context {2} has just fetched campaigns {3} in {4} ms";
        public static string FETCH_CAMPAIGNS_FROM_CACHE =
            "Visitor {0}, anonymousId {1} with context {2} has just fetched campaigns from cache {3} in {4} ms";
        public static string FETCH_FLAGS_FROM_CAMPAIGNS =
            "Visitor {0}, anonymousId {1} with context {2} has just fetched flags {3} from Campaigns";

        public DefaultStrategy(VisitorDelegateAbstract visitor)
            : base(visitor) { }

        protected virtual void UpdateContextKeyValue(string key, object value)
        {
            if (
                PredefinedContext.IsPredefinedContext(key)
                && !PredefinedContext.CheckType(key, value)
            )
            {
                Log.LogError(
                    Config,
                    string.Format(
                        Constants.PREDEFINED_CONTEXT_TYPE_ERROR,
                        key,
                        PredefinedContext.GetPredefinedType(key)
                    ),
                    "UpdateContext"
                );
                return;
            }

            if (
                !(value is string)
                && !(value is bool)
                && !(value is double)
                && !(value is long)
                && !(value is int)
            )
            {
                Log.LogError(
                    Config,
                    string.Format(Constants.CONTEXT_PARAM_ERROR, key),
                    "UpdateContex"
                );
                return;
            }

            if (
                key == PredefinedContext.FLAGSHIP_CLIENT
                || key == PredefinedContext.FLAGSHIP_VERSION
                || key == PredefinedContext.FLAGSHIP_VISITOR
            )
            {
                return;
            }

            Visitor.Context[key] = value;
        }

        protected void UpdateContextFetchFlagsStatus(
            IDictionary<string, object> oldContext,
            IDictionary<string, object> newContext
        )
        {
            if (Utils.Helper.IsDeepEqual(oldContext, newContext))
            {
                return;
            }

            Visitor.HasContextBeenUpdated = true;

            Visitor.FlagsStatus = new FlagsStatus
            {
                Reason = FSFetchReasons.VISITOR_CONTEXT_UPDATED,
                Status = FSFlagStatus.FETCH_REQUIRED,
            };
        }

        public override void UpdateContext(IDictionary<string, object> context)
        {
            if (context == null)
            {
                return;
            }
            Visitor.HasContextBeenUpdated = false;
            var oldContext = new Dictionary<string, object>(Visitor.Context);
            foreach (var item in context)
            {
                UpdateContextKeyValue(item.Key, item.Value);
            }
            var newContext = new Dictionary<string, object>(Visitor.Context);
            UpdateContextFetchFlagsStatus(oldContext, newContext);
        }

        public override void UpdateContext(string key, string value)
        {
            Visitor.HasContextBeenUpdated = false;
            var oldContext = new Dictionary<string, object>(Visitor.Context);
            UpdateContextKeyValue(key, value);
            var newContext = new Dictionary<string, object>(Visitor.Context);
            UpdateContextFetchFlagsStatus(oldContext, newContext);
        }

        public override void UpdateContext(string key, double value)
        {
            Visitor.HasContextBeenUpdated = false;
            var oldContext = new Dictionary<string, object>(Visitor.Context);
            UpdateContextKeyValue(key, value);
            var newContext = new Dictionary<string, object>(Visitor.Context);
            UpdateContextFetchFlagsStatus(oldContext, newContext);
        }

        public override void UpdateContext(string key, bool value)
        {
            Visitor.HasContextBeenUpdated = false;
            var oldContext = new Dictionary<string, object>(Visitor.Context);
            UpdateContextKeyValue(key, value);
            var newContext = new Dictionary<string, object>(Visitor.Context);
            UpdateContextFetchFlagsStatus(oldContext, newContext);
        }

        public override void ClearContext()
        {
            Visitor.HasContextBeenUpdated = false;
            var oldContext = new Dictionary<string, object>(Visitor.Context);
            Visitor.Context.Clear();
            var newContext = new Dictionary<string, object>(Visitor.Context);
            UpdateContextFetchFlagsStatus(oldContext, newContext);
        }

        protected virtual ICollection<Campaign> FetchVisitorCacheCampaigns(
            VisitorDelegateAbstract visitor
        )
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
                    campaigns.Add(
                        new Campaign
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
                                    Value = item.Flags,
                                },
                            },
                        }
                    );
                }

                return campaigns;
            }

            return campaigns;
        }

        protected async Task<ICollection<Campaign>> GetCampaignsFromDecisionManager(
            string functionName,
            DateTime now
        )
        {
            try
            {
                Visitor.FlagsStatus = new FlagsStatus
                {
                    Reason = FSFetchReasons.NONE,
                    Status = FSFlagStatus.FETCHING,
                };

                var campaigns = await DecisionManager.GetCampaigns(Visitor).ConfigureAwait(false);
                if (DecisionManager.IsPanic)
                {
                    Visitor.FlagsStatus = new FlagsStatus
                    {
                        Reason = FSFetchReasons.NONE,
                        Status = FSFlagStatus.PANIC,
                    };
                }

                Log.LogDebug(
                    Config,
                    string.Format(
                        FETCH_CAMPAIGNS_SUCCESS,
                        Visitor.VisitorId,
                        Visitor.AnonymousId,
                        JsonConvert.SerializeObject(Visitor.Context),
                        JsonConvert.SerializeObject(campaigns),
                        (DateTime.Now - now).Milliseconds
                    ),
                    functionName
                );

                return campaigns;
            }
            catch (Exception ex)
            {
                Visitor.FlagsStatus = new FlagsStatus
                {
                    Reason = FSFetchReasons.FLAGS_FETCHING_ERROR,
                    Status = FSFlagStatus.FETCH_REQUIRED,
                };
                Log.LogError(Config, ex.Message, functionName);
            }
            return null;
        }

        public override async Task FetchFlags()
        {
            const string FUNCTION_NAME = "FetchFlags";
            Log.LogDebug(
                Config,
                string.Format(FETCH_FLAGS_STARTED, Visitor.VisitorId),
                FUNCTION_NAME
            );
            ICollection<Campaign> campaigns = new List<Campaign>();
            var now = DateTime.Now;

            try
            {
                campaigns = await GetCampaignsFromDecisionManager(FUNCTION_NAME, now)
                    .ConfigureAwait(false);
                if (campaigns.Count == 0)
                {
                    campaigns = FetchVisitorCacheCampaigns(Visitor);
                    if (campaigns.Count > 0)
                    {
                        Log.LogDebug(
                            Config,
                            string.Format(
                                FETCH_CAMPAIGNS_FROM_CACHE,
                                Visitor.VisitorId,
                                Visitor.AnonymousId,
                                JsonConvert.SerializeObject(Visitor.Context),
                                JsonConvert.SerializeObject(campaigns),
                                (DateTime.Now - now).Milliseconds
                            ),
                            FUNCTION_NAME
                        );
                        Visitor.FlagsStatus = new FlagsStatus
                        {
                            Reason = FSFetchReasons.FLAGS_FETCHED_FROM_CACHE,
                            Status = FSFlagStatus.FETCH_REQUIRED,
                        };
                    }
                }

                Visitor.Campaigns = campaigns;
                Visitor.Flags = await DecisionManager.GetFlags(campaigns).ConfigureAwait(false);
                Visitor.GetStrategy().CacheVisitorAsync();

                if (Visitor.FlagsStatus.Status == FSFlagStatus.FETCHING)
                {
                    Visitor.FlagsStatus = new FlagsStatus
                    {
                        Reason = FSFetchReasons.NONE,
                        Status = FSFlagStatus.FETCHED,
                    };
                }

                Log.LogDebug(
                    Config,
                    string.Format(
                        FETCH_FLAGS_FROM_CAMPAIGNS,
                        Visitor.VisitorId,
                        Visitor.AnonymousId,
                        JsonConvert.SerializeObject(Visitor.Context),
                        JsonConvert.SerializeObject(Visitor.Flags)
                    ),
                    FUNCTION_NAME
                );

                _ = SendFetchFlagsTroubleshootingHit(campaigns, now);
                _ = SendUsageHitSdkConfig();
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
                    LastBucketingTimestamp = DecisionManager.LastBucketingTimestamp,
                    LastInitializationTimestamp = Visitor
                        .SdkInitialData
                        ?.LastInitializationTimestamp,
                    HttpResponseTime = (DateTime.Now - now).Milliseconds,

                    SdkConfigMode = Config.DecisionMode,
                    SdkConfigTimeout = Config.Timeout,
                    SdkConfigTrackingManagerConfigStrategy = Config
                        .TrackingManagerConfig
                        .CacheStrategy,
                    SdkConfigTrackingManagerConfigBatchIntervals = Config
                        .TrackingManagerConfig
                        .BatchIntervals,
                    SdkConfigTrackingManagerConfigPoolMaxSize = Config
                        .TrackingManagerConfig
                        .PoolMaxSize,
                    SdkConfigUsingCustomHitCache = Config.HitCacheImplementation != null,
                    SdkConfigUsingCustomVisitorCache = Config.VisitorCacheImplementation != null,
                    SdkConfigUsingOnVisitorExposed = Config.HasOnVisitorExposed(),
                    SdkConfigDisableCache = Config.DisableCache,
                };

                if (Config is BucketingConfig bucketingConfig)
                {
                    fetchFlagTroubleshootingHit.SdkConfigPollingInterval =
                        bucketingConfig.PollingInterval;
                }

                _ = SendTroubleshootingHit(fetchFlagTroubleshootingHit);
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
                FlagMetadata = new FlagMetadata(
                    flag.CampaignId,
                    flag.VariationGroupId,
                    flag.VariationId,
                    flag.IsReference,
                    flag.CampaignType,
                    flag.Slug,
                    flag.CampaignName,
                    flag.VariationGroupName,
                    flag.VariationName
                ),
            };

            await TrackingManager.ActivateFlag(activate).ConfigureAwait(false);

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
                HitContent = activate.ToApiKeys(),
            };

            _ = TrackingManager.SendTroubleshootingHit(activateTroubleshooting);
        }

        private void sendFlagTroubleshooting(
            DiagnosticLabel label,
            string key,
            object defaultValue,
            bool? visitorExposed
        )
        {
            var troubleshootingHit = new Troubleshooting()
            {
                Label = label,
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
                VisitorExposed = visitorExposed,
            };

            _ = SendTroubleshootingHit(troubleshootingHit);
        }

        public override async Task VisitorExposed<T>(
            string key,
            T defaultValue,
            FlagDTO flag,
            bool hasGetValueBeenCalled = false
        )
        {
            const string functionName = "VisitorExposed";
            if (flag == null)
            {
                Log.LogError(
                    Config,
                    string.Format(Constants.GET_FLAG_ERROR, Visitor.VisitorId, key),
                    functionName
                );
                sendFlagTroubleshooting(
                    DiagnosticLabel.VISITOR_EXPOSED_FLAG_NOT_FOUND,
                    key,
                    defaultValue,
                    null
                );
                return;
            }

            if (!hasGetValueBeenCalled)
            {
                Log.LogWarning(
                    Config,
                    string.Format(
                        Constants.VISITOR_EXPOSED_FLAG_VALUE_NOT_CALLED,
                        Visitor.VisitorId,
                        key
                    ),
                    functionName
                );
                sendFlagTroubleshooting(
                    DiagnosticLabel.FLAG_VALUE_NOT_CALLED,
                    key,
                    defaultValue,
                    null
                );
                return;
            }

            if (
                flag.Value != null
                && defaultValue != null
                && !Utils.Helper.HasSameType(flag.Value, defaultValue)
            )
            {
                Log.LogWarning(
                    Config,
                    string.Format(Constants.USER_EXPOSED_CAST_ERROR, Visitor.VisitorId, key),
                    functionName
                );
                sendFlagTroubleshooting(
                    DiagnosticLabel.VISITOR_EXPOSED_TYPE_WARNING,
                    key,
                    defaultValue,
                    null
                );
                return;
            }

            await SendActivate(flag, defaultValue).ConfigureAwait(false);
        }

        public override T GetFlagValue<T>(
            string key,
            T defaultValue,
            FlagDTO flag,
            bool visitorExposed = true
        )
        {
            const string functionName = "getFlag.value";

            if (flag == null)
            {
                Log.LogWarning(
                    Config,
                    string.Format(
                        Constants.GET_FLAG_MISSING_ERROR,
                        Visitor.VisitorId,
                        key,
                        defaultValue
                    ),
                    functionName
                );
                sendFlagTroubleshooting(
                    DiagnosticLabel.GET_FLAG_VALUE_FLAG_NOT_FOUND,
                    key,
                    defaultValue,
                    visitorExposed
                );

                return defaultValue;
            }

            if (visitorExposed)
            {
                _ = SendActivate(flag, defaultValue);
            }

            if (flag.Value == null)
            {
                return defaultValue;
            }

            if (defaultValue != null && !Utils.Helper.HasSameType(flag.Value, defaultValue))
            {
                Log.LogWarning(
                    Config,
                    string.Format(
                        Constants.GET_FLAG_CAST_ERROR,
                        Visitor.VisitorId,
                        key,
                        defaultValue
                    ),
                    functionName
                );
                sendFlagTroubleshooting(
                    DiagnosticLabel.GET_FLAG_VALUE_TYPE_WARNING,
                    key,
                    defaultValue,
                    visitorExposed
                );

                return defaultValue;
            }

            Log.LogDebug(
                Config,
                string.Format(Constants.GET_FLAG_VALUE, Visitor.VisitorId, key, flag.Value),
                functionName
            );

            return (T)flag.Value;
        }

        protected virtual void SendFlagMetadataTroubleshooting(string key)
        {
            var troubleshootingHit = new Troubleshooting()
            {
                Label = DiagnosticLabel.GET_FLAG_METADATA_FLAG_NOT_FOUND,
                LogLevel = LogLevel.INFO,
                Traffic = Visitor.Traffic,
                VisitorSessionId = Visitor.SessionId,
                FlagshipInstanceId = Visitor.SdkInitialData?.InstanceId,
                AnonymousId = Visitor.AnonymousId,
                VisitorId = Visitor.VisitorId,
                VisitorContext = Visitor.Context,
                FlagKey = key,
                Config = Config,
            };

            _ = SendTroubleshootingHit(troubleshootingHit);
        }

        public override IFlagMetadata GetFlagMetadata(string key, FlagDTO flag)
        {
            const string functionName = "flag.metadata";
            if (flag == null)
            {
                Log.LogWarning(
                    Config,
                    string.Format(Constants.GET_METADATA_NO_FLAG_FOUND, Visitor.VisitorId, key),
                    functionName
                );

                SendFlagMetadataTroubleshooting(key);

                return FlagMetadata.EmptyMetadata();
            }

            return new FlagMetadata(
                flag.CampaignId,
                flag.VariationGroupId,
                flag.VariationId,
                flag.IsReference,
                flag.CampaignType,
                flag.Slug,
                flag.CampaignName,
                flag.VariationGroupName,
                flag.VariationName
            );
        }

        public override async Task SendHit(IEnumerable<HitAbstract> hits)
        {
            foreach (var item in hits)
            {
                await SendHit(item).ConfigureAwait(false);
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

                await TrackingManager.Add(hit).ConfigureAwait(false);

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
                    HitContent = hit.ToApiKeys(),
                };

                _ = SendTroubleshootingHit(troubleshootingHit);
            }
            catch (Exception ex)
            {
                Log.LogError(Config, ex.Message, functionName);
            }
        }

        public override void Authenticate(string visitorId)
        {
            const string methodName = "Authenticate";
            var visitorCacheInstance = Config.VisitorCacheImplementation;
            if (Config.DecisionMode == DecisionMode.BUCKETING && visitorCacheInstance == null)
            {
                LogDeactivateOnBucketingMode(methodName);
                return;
            }

            if (string.IsNullOrWhiteSpace(visitorId))
            {
                Log.LogError(
                    Config,
                    string.Format(Constants.VISITOR_ID_ERROR, methodName),
                    methodName
                );
                return;
            }

            Visitor.AnonymousId = Visitor.VisitorId;
            Visitor.VisitorId = visitorId;

            Visitor.FlagsStatus = new FlagsStatus
            {
                Reason = FSFetchReasons.VISITOR_AUTHENTICATED,
                Status = FSFlagStatus.FETCH_REQUIRED,
            };

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

            _ = SendTroubleshootingHit(troubleshootingHit);
        }

        public override void Unauthenticate()
        {
            const string methodName = "Unauthenticate";
            var visitorCacheInstance = Config.VisitorCacheImplementation;
            if (Config.DecisionMode == DecisionMode.BUCKETING && visitorCacheInstance == null)
            {
                LogDeactivateOnBucketingMode(methodName);
                return;
            }

            if (string.IsNullOrWhiteSpace(Visitor.AnonymousId))
            {
                Log.LogError(
                    Config,
                    string.Format(Constants.FLAGSHIP_VISITOR_NOT_AUTHENTICATE, methodName),
                    methodName
                );
                return;
            }

            Visitor.VisitorId = Visitor.AnonymousId;
            Visitor.AnonymousId = null;

            Visitor.FlagsStatus = new FlagsStatus
            {
                Reason = FSFetchReasons.VISITOR_UNAUTHENTICATED,
                Status = FSFlagStatus.FETCH_REQUIRED,
            };

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

            _ = SendTroubleshootingHit(troubleshootingHit);
        }

        protected void LogDeactivateOnBucketingMode(string methodName)
        {
            Log.LogError(Config, Constants.XPC_BUCKETING_WARNING, methodName);
        }
    }
}

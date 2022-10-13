using Flagship.Enums;
using Flagship.FsFlag;
using Flagship.Hit;
using Flagship.Logger;
using Flagship.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Flagship.FsVisitor
{
    internal class DefaultStrategy : VisitorStrategyAbstract
    {
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
            if (visitor.VisitorCache == null || visitor.VisitorCache.Data ==null)
            {
                return campaigns;
            }
            if (visitor.VisitorCache.Version == 1)
            {
                  var data= (VisitorCacheDTOV1)visitor.VisitorCache.Data;
                visitor.UpdateContext(data.Data.Context?.ToDictionary(entry => entry.Key, entry => entry.Value));
                
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
            try
            {
                var campaigns = await DecisionManager.GetCampaigns(Visitor);
                if (campaigns.Count == 0)
                {
                    campaigns = FetchVisitorCacheCampaigns(Visitor);
                }
                Visitor.Campaigns = campaigns;
                Visitor.Flags = await DecisionManager.GetFlags(campaigns);
                Visitor.GetStrategy().CacheVisitorAsync();
            }
            catch (Exception ex)
            {
                Log.LogError(Config, ex.Message, "FetchFlags");
            }
        }

        protected override async Task SendActivate(FlagDTO flag)
        {
            try
            {
                var activate = new Activate(flag.VariationGroupId, flag.VariationId)
                {
                    Config=Config
                };
                await TrackingManager.ActivateFlag(activate);
            }
            catch (Exception ex)
            {
                Log.LogError(Config, ex.Message, "UserExposed");
            }
        }
        public override async Task UserExposed<T>(string key, T defaultValue, FlagDTO flag)
        {
            const string functionName = "UserExposed";
            if (flag == null)
            {
                Log.LogError(Config, string.Format(Constants.GET_FLAG_ERROR, key), functionName);
                return;
            }
            if (flag.Value != null && defaultValue!=null && !Utils.Utils.HasSameType(flag.Value, defaultValue))
            {
                Log.LogError(Config, string.Format(Constants.USER_EXPOSED_CAST_ERROR, key), functionName);
                return;
            }

            await SendActivate(flag);
        }

        public override T GetFlagValue<T>(string key, T defaultValue, FlagDTO flag, bool userExposed = true)
        {
            const string functionName = "getFlag.value";

            if (flag == null)
            {
                Log.LogInfo(Config, string.Format(Constants.GET_FLAG_MISSING_ERROR, key), functionName);
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
            }
            catch (Exception ex)
            {
                Log.LogError(Config, ex.Message, functionName);
            }
        }

        public override void Authenticate(string visitorId)
        {
            const string methodName = "Authenticate";
            if (Config.DecisionMode== DecisionMode.BUCKETING)
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

            Visitor.VisitorId= Visitor.AnonymousId;
            Visitor.AnonymousId = null;

        }

        protected void LogDeactivateOnBucketingMode(string methodName)
        {
            Log.LogError(Config, string.Format(Constants.METHOD_DEACTIVATED_BUCKETING_ERROR, methodName), methodName);
        }
    }
}

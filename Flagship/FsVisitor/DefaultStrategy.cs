using Flagship.Enums;
using Flagship.FsFlag;
using Flagship.Hit;
using Flagship.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
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
            if (FsPredefinedContext.IsPredefinedContext(key) && !FsPredefinedContext.CheckType(key, value))
            {
                Utils.Log.LogError(
                    Config,
                    string.Format(Constants.PREDEFINED_CONTEXT_TYPE_ERROR, key, FsPredefinedContext.GetPredefinedType(key)),
                    "UpdateContext");
                return;
            }

            if (!(value is string) && !(value is bool) && !(value is double) && !(value is long) && !(value is int))
            {
                Utils.Log.LogError(Config, string.Format(Constants.CONTEXT_PARAM_ERROR, key), "UpdateContex");
                return;
            }

            Visitor.Context[key] = value;
        }

        public override void UpdateContext(IDictionary<string, object> context)
        {
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
                visitor.UpdateContext(data.Data.Context);
                
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
                CacheVisitorAsync();
            }
            catch (Exception ex)
            {
                Utils.Log.LogError(Config, ex.Message, "FetchFlags");
            }
        }

        public override Task UserExposed<T>(string key, T defaultValue, FlagDTO flag)
        {
            const string functionName = "UserExposed";
            if (flag == null)
            {
                return Task.Factory.StartNew(() =>
                {
                    Utils.Log.LogError(Config, string.Format(Constants.GET_FLAG_ERROR, key), functionName);
                });
            }
            if (flag.Value != null && !Utils.Utils.HasSameType(flag.Value, defaultValue))
            {
                return Task.Factory.StartNew(() =>
                {
                    Utils.Log.LogError(Config, string.Format(Constants.USER_EXPOSED_CAST_ERROR, key), functionName);
                });
            }

            return TrackingManager.SendActive(Visitor, flag);
        }

        public override T GetFlagValue<T>(string key, T defaultValue, FlagDTO flag, bool userExposed = true)
        {
            const string functionName = "getFlag.value";

            if (flag == null)
            {
                Utils.Log.LogInfo(Config, string.Format(Constants.GET_FLAG_MISSING_ERROR, key), functionName);
                return defaultValue;
            }

            if (flag.Value == null && defaultValue != null)
            {
                if (userExposed)
                {
                    UserExposed(key, defaultValue, flag);
                }
                Utils.Log.LogInfo(Config, string.Format(Constants.GET_FLAG_CAST_ERROR, key), functionName);
                return defaultValue;
            }

            if (!Utils.Utils.HasSameType(flag.Value, defaultValue))
            {
                Utils.Log.LogInfo(Config, string.Format(Constants.GET_FLAG_CAST_ERROR, key), functionName);
                return defaultValue;
            }

            if (userExposed)
            {
                UserExposed(key, defaultValue, flag);
            }

            return (T)flag.Value;
        }

        public override IFlagMetadata GetFlagMetadata(IFlagMetadata metadata, string key, bool hasSameType)
        {
            const string functionName = "flag.metadata";
            if (!hasSameType && !string.IsNullOrWhiteSpace(metadata.CampaignId))
            {
                Utils.Log.LogError(Config, string.Format(Constants.GET_METADATA_CAST_ERROR, key), functionName);
                return FlagMetadata.EmptyMetadata();
            }
            return metadata;
        }

        public override Task SendHit(HitAbstract hit)
        {
            const string functionName = "SendHit";
            try
            {
                if (hit == null)
                {
                    return Task.Factory.StartNew(() =>
                    {
                        Utils.Log.LogError(Config, Constants.HIT_NOT_NULL, functionName);
                    });
                }

                hit.VisitorId = Visitor.VisitorId;
                hit.DS = Constants.SDK_APP;
                hit.Config = Config;
                hit.AnonymousId = Visitor.AnonymousId;

                if (!hit.IsReady())
                {
                    return Task.Factory.StartNew(() =>
                    {
                        Utils.Log.LogError(Config, hit.GetErrorMessage(), functionName);
                    });
                }

                return TrackingManager.SendHit(hit);
            }
            catch (Exception ex)
            {
                return Task.Factory.StartNew(() => { Utils.Log.LogError(Config, ex.Message, functionName); });
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
                Utils.Log.LogError(Config, string.Format(Constants.VISITOR_ID_ERROR, methodName), methodName);
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
                Utils.Log.LogError(Config, string.Format(Constants.FLAGSHIP_VISITOR_NOT_AUTHENTICATE, methodName), methodName);
                return;
            }

            Visitor.VisitorId= Visitor.AnonymousId;
            Visitor.AnonymousId = null;

        }

        protected void LogDeactivateOnBucketingMode(string methodName)
        {
            Utils.Log.LogError(Config, string.Format(Constants.METHOD_DEACTIVATED_BUCKETING_ERROR, methodName), methodName);
        }
    }
}

using Flagship.Enums;
using Flagship.FsFlag;
using Flagship.Hit;
using Flagship.Model;
using System;
using System.Collections.Generic;
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
            if (!(value is string) && !(value is bool) && !(value is double) && !(value is long) && !(value is int))
            {
                Utils.Log.LogError(Config, string.Format(Constants.CONTEXT_PARAM_ERROR, key), "UpdateContex");
                return;
            }

            Visitor.Context[key] = value;
        }

        public override void UpdateContexCommon(IDictionary<string, object> context)
        {
            foreach (var item in context)
            {
                UpdateContexKeyValue(item.Key, item.Value);
            }
        }

        public override void UpdateContex(IDictionary<string, string> context)
        {
            foreach (var item in context)
            {
                UpdateContexKeyValue(item.Key, item.Value);
            }
        }

        public override void UpdateContex(IDictionary<string, double> context)
        {
            foreach (var item in context)
            {
                UpdateContexKeyValue(item.Key, item.Value);
            }
        }

        public override void UpdateContex(IDictionary<string, bool> context)
        {
            foreach (var item in context)
            {
                UpdateContexKeyValue(item.Key, item.Value);
            }
        }

        public override void UpdateContex(string key, string value)
        {
            UpdateContexKeyValue(key, value);
        }

        public override void UpdateContex(string key, double value)
        {
            UpdateContexKeyValue(key, value);
        }

        public override void UpdateContex(string key, bool value)
        {
            UpdateContexKeyValue(key, value);
        }

        public override void ClearContext()
        {
            Visitor.Context.Clear();
        }

        async public override Task FetchFlags()
        {
            try
            {
                var campaigns = await DecisionManager.GetCampaigns(Visitor);
                Visitor.Flags = await DecisionManager.GetFlags(campaigns);
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
            if (flag.Value != null && flag.Value.GetType() != defaultValue.GetType())
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

            if (flag.Value == null)
            {
                if (userExposed)
                {
                    UserExposed(key, defaultValue, flag);
                }
                Utils.Log.LogInfo(Config, string.Format(Constants.GET_FLAG_CAST_ERROR, key), functionName);
                return defaultValue;
            }
            if (!flag.Value.GetType().Equals(defaultValue.GetType()))
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


    }
}

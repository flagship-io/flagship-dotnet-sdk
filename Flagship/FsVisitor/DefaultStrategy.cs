using Flagship.Enums;
using Flagship.FsFlag;
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

        protected void UpdateContexKeyValue(string key, object value)
        {
            if (!(value is string) && !(value is int) && !(value is double))
            {
                Utils.Log.LogError(Config, string.Format(Constants.CONTEXT_PARAM_ERROR, key), "UpdateContex");
                return;
            }

           Visitor.Context[key] = value;
        }

        public override void UpdateContex(IDictionary<string, object> context)
        {
            foreach (var item in context)
            {
                UpdateContexKeyValue(item.Key, item.Value);
            }
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
            const string functionName = "userExposed";
            if (flag == null)
            {
                Utils.Log.LogError(Config, string.Format(Constants.GET_FLAG_ERROR, key), functionName);
                return default;
            }
            if (flag.Value != null && flag.Value.GetType() != defaultValue.GetType())
            {
                Utils.Log.LogError(Config, string.Format(Constants.USER_EXPOSED_CAST_ERROR, key), functionName);
                return default;
            }

            return TrackingManager.SendActive(Visitor, flag);
        }

        public override T GetFlagValue<T>(string key, T defaultValue, FlagDTO flag, bool userExposed = true)
        {
            const string functionName = "getFlag value";
            try
            {
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
            catch (Exception ex)
            {
                Utils.Log.LogInfo(Config, ex.Message, functionName);
                return defaultValue;
            }
           
        }

        public override IFlagMetadata GetFlagMetadata(IFlagMetadata metadata, string key, bool hasSameType)
        {
            const string functionName = "flag.metadata";
            if (!hasSameType && string.IsNullOrWhiteSpace(metadata.CampaignId))
            {
                Utils.Log.LogError(Config, string.Format(Constants.GET_METADATA_CAST_ERROR, key), functionName);
                return FlagMetadata.EmptyMetadata();
            }
            return metadata;
        }
    }
}

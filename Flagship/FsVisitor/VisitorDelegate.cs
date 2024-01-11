using Flagship.Config;
using Flagship.Enums;
using Flagship.FsFlag;
using Flagship.Hit;
using Flagship.Logger;
using Flagship.Model;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flagship.FsVisitor
{
    internal class VisitorDelegate : VisitorDelegateAbstract
    {
        public VisitorDelegate(string visitorID, bool isAuthenticated, IDictionary<string, object> context, bool hasConsented, IConfigManager configManager, SdkInitialData sdkInitialData = null) : base(visitorID, isAuthenticated, context, hasConsented, configManager, sdkInitialData)
        {
        }

        public override void ClearContext()
        {
            GetStrategy().ClearContext();
            LoadPredefinedContext();
        }

        public override Task FetchFlags()
        {
            return this.GetStrategy().FetchFlags();
        }

        private IFlag<T> CreateFlag<T>(string key, T defaultValue)
        {
            if (FlagSyncStatus != FlagSyncStatus.FLAGS_FETCHED)
            {
                Log.LogWarning(Config, string.Format(FlagSyncStatusMessage(FlagSyncStatus), VisitorId, key), "GET_FLAG");
            }
            return new Flag<T>(key, this, defaultValue);
        }

        public override IFlag<string> GetFlag(string key, string defaultValue)
        {
            return CreateFlag(key, defaultValue);
        }

        public override IFlag<long> GetFlag(string key, long defaultValue)
        {
            return CreateFlag(key, defaultValue);
        }

        public override IFlag<bool> GetFlag(string key, bool defaultValue)
        {
            return CreateFlag(key, defaultValue);
        }

        public override IFlag<JObject> GetFlag(string key, JObject defaultValue)
        {
            return CreateFlag(key, defaultValue);
        }

        public override IFlag<JArray> GetFlag(string key, JArray defaultValue)
        {
            return CreateFlag(key, defaultValue);
        }


        public override IFlagMetadata GetFlagMetadata(IFlagMetadata metadata, string key, bool hasSameType)
        {
            return GetStrategy().GetFlagMetadata(metadata, key, hasSameType);
        }

        public override T GetFlagValue<T>(string key, T defaultValue, Model.FlagDTO flag, bool userExposed)
        {
            return GetStrategy().GetFlagValue(key, defaultValue, flag, userExposed);
        }

        public override Task VisitorExposed<T>(string key, T defaultValue, Model.FlagDTO flag)
        {
            return GetStrategy().UserExposed(key, defaultValue, flag);  
        }

        public override Task SendHit(HitAbstract hit)
        {
            return GetStrategy().SendHit(hit);
        }

        public override void UpdateContext(IDictionary<string, object> context)
        {
            GetStrategy().UpdateContext(context);
        }

        public override void UpdateContext(string key, string value)
        {
            GetStrategy().UpdateContext(key, value);
        }

        public override void UpdateContext(string key, double value)
        {
            GetStrategy().UpdateContext(key, value);
        }

        public override void UpdateContext(string key, bool value)
        {
            GetStrategy().UpdateContext(key, value);
        }

        protected string FlagSyncStatusMessage(FlagSyncStatus flagSyncStatus)
        {
            var message = "";
            var flagMessage = "without calling `fetchFlags` method afterwards, the value of the flag `{1}` may be outdated";
            switch (flagSyncStatus) {
                case FlagSyncStatus.CREATED:
                    message = $"Visitor `{{0}}` has been created {flagMessage}";
                    break;
                case FlagSyncStatus.CONTEXT_UPDATED:
                    message = $"Visitor context for visitor `{{0}}` has been updated {flagMessage}";
                    break;
                case FlagSyncStatus.AUTHENTICATED:
                    message = $"Visitor `{{0}}` has been authenticated {flagMessage}";
                    break;
                case FlagSyncStatus.UNAUTHENTICATED:
                    message = $"Visitor `{{0}}` has been unauthenticated {flagMessage}";
                    break;
            }
            return message;
        }
    }
}

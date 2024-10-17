using Flagship.Config;
using Flagship.Enums;
using Flagship.FsFlag;
using Flagship.Hit;
using Flagship.Logger;
using Flagship.Model;
using System.Collections.Generic;
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

        public override async Task FetchFlags()
        {
            await GetStrategy().LookupVisitor().ConfigureAwait(false);
            await GetStrategy().FetchFlags().ConfigureAwait(false);
        }

        public override IFlag GetFlag(string key)
        {
            if (FlagsStatus.Status != FSFlagStatus.FETCHED && FlagsStatus.Status != FSFlagStatus.PANIC
                && FlagsStatus.Status != FSFlagStatus.FETCHING)
            {
                Log.LogWarning(Config, string.Format(FlagSyncStatusMessage(FlagsStatus.Reason), VisitorId, key), "GET_FLAG");
            }
            return new Flag(key, this);
        }

        public override IFlagCollection GetFlags()
        {
            return new FlagCollection(this);
        }

        public override IFlagMetadata GetFlagMetadata(string key, FlagDTO flag)
        {
            return GetStrategy().GetFlagMetadata(key, flag);
        }

        public override T GetFlagValue<T>(string key, T defaultValue, FlagDTO flag, bool visitorExposed)
        {
            return GetStrategy().GetFlagValue(key, defaultValue, flag, visitorExposed);
        }

        public override Task VisitorExposed<T>(string key, T defaultValue, FlagDTO flag, bool hasGetValueBeenCalled = false)
        {
            return GetStrategy().VisitorExposed(key, defaultValue, flag, hasGetValueBeenCalled);
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

        protected string FlagSyncStatusMessage(FSFetchReasons reason)
        {
            var message = "";
            const string VISITOR_SYNC_FLAGS_MESSAGE = "without calling `fetchFlags` method afterwards. So, the value of the flag `{1}` might be outdated";
            switch (reason)
            {
                case FSFetchReasons.FLAGS_NEVER_FETCHED:
                    message = $"Visitor `{{0}}` has been created. So, the value of the flag `{{1}}` is the default value";
                    break;
                case FSFetchReasons.VISITOR_CONTEXT_UPDATED:
                    message = $"Visitor context for visitor `{{0}}` has been updated {VISITOR_SYNC_FLAGS_MESSAGE}";
                    break;
                case FSFetchReasons.VISITOR_AUTHENTICATED:
                    message = $"Visitor `{{0}}` has been authenticated {VISITOR_SYNC_FLAGS_MESSAGE}";
                    break;
                case FSFetchReasons.VISITOR_UNAUTHENTICATED:
                    message = $"Visitor `{{0}}` has been unauthenticated {VISITOR_SYNC_FLAGS_MESSAGE}";
                    break;
                case FSFetchReasons.FLAGS_FETCHING_ERROR:
                    message = "There was an error while fetching flags for visitor `{0}`. So the value of the flag `{1}` may be outdated";
                    break;
                case FSFetchReasons.FLAGS_FETCHED_FROM_CACHE:
                    message = "Flags for visitor `{0}` have been fetched from cache";
                    break;
            }
            return message;
        }
    }
}

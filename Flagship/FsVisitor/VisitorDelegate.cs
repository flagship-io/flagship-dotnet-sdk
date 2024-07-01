﻿using Flagship.Config;
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
            if (FetchFlagsStatus.Status != FSFetchStatus.FETCHED && FetchFlagsStatus.Status != FSFetchStatus.PANIC )
            {
                Log.LogWarning(Config, string.Format(FlagSyncStatusMessage(FetchFlagsStatus.Reason), VisitorId, key), "GET_FLAG");
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
                case FSFetchReasons.VISITOR_CREATED:
                    message = $"Visitor `{{0}}` has been created. So, the value of the flag `{{1}}` is the default value";
                    break;
                case FSFetchReasons.UPDATE_CONTEXT:
                    message = $"Visitor context for visitor `{{0}}` has been updated {VISITOR_SYNC_FLAGS_MESSAGE}";
                    break;
                case FSFetchReasons.AUTHENTICATE:
                    message = $"Visitor `{{0}}` has been authenticated {VISITOR_SYNC_FLAGS_MESSAGE}";
                    break;
                case FSFetchReasons.UNAUTHENTICATE:
                    message = $"Visitor `{{0}}` has been unauthenticated {VISITOR_SYNC_FLAGS_MESSAGE}";
                    break;
                case FSFetchReasons.FETCH_ERROR:
                    message = "There was an error while fetching flags for visitor `{0}`. So the value of the flag `{1}` may be outdated";
                    break;
                case FSFetchReasons.READ_FROM_CACHE:
                    message = "Flags for visitor `{0}` have been fetched from cache";
                    break;
                default:
                    break;
            }
            return message;
        }
    }
}

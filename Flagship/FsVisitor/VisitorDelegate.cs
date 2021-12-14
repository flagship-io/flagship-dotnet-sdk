using Flagship.Config;
using Flagship.FsFlag;
using Flagship.Hit;
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
        public VisitorDelegate(string visitorID, bool isAuthenticated, IDictionary<string, object> context, bool hasConsented, IConfigManager configManager) : base(visitorID, isAuthenticated, context, hasConsented, configManager)
        {
        }

        public override void ClearContext()
        {
            GetStrategy().ClearContext();
        }

        public override Task FetchFlags()
        {
            return this.GetStrategy().FetchFlags();
        }

        public override void UpdateContex(IDictionary<string, object> context)
        {
            GetStrategy().UpdateContex(context);
        }

        private IFlag<T> CreateFlag<T>(string key, T defaultValue)
        {
            var flagDTO = Flags.FirstOrDefault(x => x.Key == key);
            return new Flag<T>(key, this, flagDTO, defaultValue);
        }

        public override IFlag<string> GetFlag(string key, string defaultValue)
        {
            return CreateFlag(key, defaultValue);
        }

        public override IFlag<double> GetFlag(string key, double defaultValue)
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

        public override Task UserExposed<T>(string key, T defaultValue, Model.FlagDTO flag)
        {
            return GetStrategy().UserExposed(key, defaultValue, flag);  
        }

        public override Task SendHit(HitAbstract hit)
        {
            return GetStrategy().SendHit(hit);
        }

    }
}

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
    internal class PanicStrategy : DefaultStrategy
    {
        public PanicStrategy(VisitorDelegateAbstract visitor) : base(visitor)
        {
        }

        public override Task SendConsentHitAsync(bool hasConsented)
        {
            return Task.Factory.StartNew(() =>
            {
                Log("SendConsentHitAsync");
            });
        }

        public override void UpdateContext(IDictionary<string, object> context)
        {
            Log("UpdateContex");    
        }

        protected override void UpdateContexKeyValue(string key, object value)
        {
            Log("UpdateContex");
        }

        public override void ClearContext()
        {
            Log("ClearContext");
        }

        public override Task SendHit(HitAbstract hit)
        {
            return Task.Factory.StartNew(() =>
            {
                Log("SendHit");
            });
        }

        public override T GetFlagValue<T>(string key, T defaultValue, FlagDTO flag, bool userExposed = true)
        {
            Log("Flag.value");
            return defaultValue;
        }

        public override Task UserExposed<T>(string key, T defaultValue, FlagDTO flag)
        {
            return Task.Factory.StartNew(() =>
            {
                Log("UserExposed");
            });
        }

        public override IFlagMetadata GetFlagMetadata(IFlagMetadata metadata, string key, bool hasSameType)
        {
            Log("Flag.metadata");
            return FlagMetadata.EmptyMetadata();
        }

        private void Log(string methodName)
        {
            Logger.Log.LogError(Config, string.Format(Constants.METHOD_DEACTIVATED_ERROR, methodName, FlagshipStatus.READY_PANIC_ON), methodName);
        }
    }
}

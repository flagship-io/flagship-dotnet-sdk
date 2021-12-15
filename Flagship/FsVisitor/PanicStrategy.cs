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

        public override void SendConsentHitAsync(bool hasConsented)
        {
            Log("SendConsentHitAsync");
        }

        public override void UpdateContex(IDictionary<string, object> context)
        {
            Log("UpdateContex");
        }

        public override void ClearContext()
        {
            Log("ClearContext");
        }

        public override Task SendHit(HitAbstract hit)
        {
            Log("SendHit");
            return default;
        }

        public override T GetFlagValue<T>(string key, T defaultValue, FlagDTO flag, bool userExposed = true)
        {
            Log("Flag.value");
            return default;
        }

        public override Task UserExposed<T>(string key, T defaultValue, FlagDTO flag)
        {
            Log("UserExposed");
            return default;
        }

        public override IFlagMetadata GetFlagMetadata(IFlagMetadata metadata, string key, bool hasSameType)
        {
            Log("Flag.metadata");
            return FlagMetadata.EmptyMetadata();
        }

        private void Log(string methodName)
        {
            Utils.Log.LogError(Config, string.Format(Constants.METHOD_DEACTIVATED_ERROR, methodName, FlagshipStatus.READY_PANIC_ON), methodName);
        }
    }
}

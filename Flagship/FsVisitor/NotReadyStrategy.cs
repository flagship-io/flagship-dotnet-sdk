using Flagship.Enums;
using Flagship.FsFlag;
using Flagship.Hit;
using Flagship.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flagship.Logger;

namespace Flagship.FsVisitor
{
    internal class NotReadyStrategy : DefaultStrategy
    {
        public NotReadyStrategy(VisitorDelegateAbstract visitor) : base(visitor)
        {
        }

        public override void LookupVisitor()
        {
            //
        }

        public override void CacheVisitorAsync()
        {
            //
        }

        public override Task FetchFlags()
        {
            return Task.Factory.StartNew(() => { Log("FetchFlags"); });
        }

        public override Task SendHit(HitAbstract hit)
        {
            return Task.Factory.StartNew(() => { Log("SendHit"); });
        }

        public override T GetFlagValue<T>(string key, T defaultValue, FlagDTO flag, bool userExposed = true)
        {
            Log(FLAG_VALUE);
            return defaultValue;
        }

        public override Task VisitorExposed<T>(string key, T defaultValue, FlagDTO flag)
        {
            return Task.Factory.StartNew(() => { Log(FLAG_VISITOR_EXPOSED); });
        }

        public override IFlagMetadata GetFlagMetadata(IFlagMetadata metadata, string key, bool hasSameType)
        {
            Log(FLAG_METADATA);
            return FlagMetadata.EmptyMetadata();
        }

        private void Log(string methodName)
        {
            Logger.Log.LogError(Config, string.Format(Constants.METHOD_DEACTIVATED_ERROR, methodName, FlagshipStatus.NOT_INITIALIZED), methodName);
        }
    }
}

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

        public override void LookupHits()
        {
            //
        }

        public override void CacheHit(FlagDTO flagDTO)
        {
            //
        }

        public override void CacheHit(HitAbstract hit)
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
            Log("Flag.value");
            return defaultValue;
        }

        public override Task UserExposed<T>(string key, T defaultValue, FlagDTO flag)
        {
            return Task.Factory.StartNew(() => { Log("UserExposed"); });
        }

        public override IFlagMetadata GetFlagMetadata(IFlagMetadata metadata, string key, bool hasSameType)
        {
            Log("flag.metadata");
            return FlagMetadata.EmptyMetadata();
        }

        private void Log(string methodName)
        {
            Logger.Log.LogError(Config, string.Format(Constants.METHOD_DEACTIVATED_ERROR, methodName, FlagshipStatus.NOT_INITIALIZED), methodName);
        }
    }
}

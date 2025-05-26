using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Flagship.Enums;
using Flagship.FsFlag;
using Flagship.Hit;
using Flagship.Model;

namespace Flagship.FsVisitor
{
    internal class PanicStrategy : DefaultStrategy
    {
        public PanicStrategy(VisitorDelegateAbstract visitor)
            : base(visitor) { }

        public override Task LookupVisitor()
        {
            return Utils.Helper.VoidTask();
        }

        public override void CacheVisitorAsync()
        {
            //
        }

        protected override ICollection<Campaign> FetchVisitorCacheCampaigns(
            VisitorDelegateAbstract visitor
        )
        {
            return new Collection<Campaign>();
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

        protected override void UpdateContextKeyValue(string key, object value)
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

        public override T GetFlagValue<T>(
            string key,
            T defaultValue,
            FlagDTO flag,
            bool userExposed = true
        )
        {
            Log("Flag.value");
            return defaultValue;
        }

        public override Task VisitorExposed<T>(
            string key,
            T defaultValue,
            FlagDTO flag,
            bool hasGetValueBeenCalled = false
        )
        {
            return Task.Factory.StartNew(() =>
            {
                Log("VisitorExposed");
            });
        }

        public override IFlagMetadata GetFlagMetadata(string key, FlagDTO flag)
        {
            Log("Flag.metadata");
            return FlagMetadata.EmptyMetadata();
        }

        private void Log(string methodName)
        {
            Logger.Log.LogInfo(
                Config,
                string.Format(
                    Constants.METHOD_DEACTIVATED_ERROR,
                    methodName,
                    FSSdkStatus.SDK_PANIC
                ),
                methodName
            );
        }

        public override void AddTroubleshootingHit(Troubleshooting hit)
        {
            //
        }

        public override Task SendTroubleshootingHit(Troubleshooting hit)
        {
            return Utils.Helper.VoidTask();
        }

        public override TroubleshootingData GetTroubleshootingData()
        {
            TrackingManager.TroubleshootingData = null;
            return null;
        }
    }
}

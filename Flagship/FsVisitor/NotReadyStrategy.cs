using Flagship.Enums;
using Flagship.FsFlag;
using Flagship.Hit;
using Flagship.Model;
using System.Threading.Tasks;

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
            Log("Flag.value");
            return defaultValue;
        }

        public override Task VisitorExposed<T>(string key, T defaultValue, FlagDTO flag, bool hasGetValueBeenCalled = false)
        {
            return Task.Factory.StartNew(() => { Log("VisitorExposed"); });
        }

        public override IFlagMetadata GetFlagMetadata(string key, FlagDTO flag)
        {
            Log("flag.metadata");
            return FlagMetadata.EmptyMetadata();
        }

        private void Log(string methodName)
        {
            Logger.Log.LogError(Config, string.Format(Constants.METHOD_DEACTIVATED_ERROR, methodName, FSSdkStatus.SDK_NOT_INITIALIZED), methodName);
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

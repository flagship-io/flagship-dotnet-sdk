using Flagship.Enums;
using Flagship.Hit;
using Flagship.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flagship.FsVisitor
{
    internal class NoConsentStrategy : DefaultStrategy
    {
        public NoConsentStrategy(VisitorDelegateAbstract visitor) : base(visitor)
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

        protected override ICollection<Campaign> FetchVisitorCacheCampaigns(VisitorDelegateAbstract visitor)
        {
            return new Collection<Campaign>();
        }

        public override Task VisitorExposed<T>(string key, T defaultValue, FlagDTO flag, bool hasGetValueBeenCalled = false)
        {

            return Task.Factory.StartNew(() =>
            {
                Log("VisitorExposed");
            });
        }

        public override Task SendHit(HitAbstract hit)
        {

            return Task.Factory.StartNew(() =>
            {
                Log("SendHit");
            });
        }

        private void Log(string methodName)
        {
            Logger.Log.LogInfo(Config, string.Format(Constants.METHOD_DEACTIVATED_CONSENT_ERROR, methodName, Visitor.VisitorId), methodName);
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
            TrackingManager.TroubleshootingData =null;
            return null;
        }
    }
}

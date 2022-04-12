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

        protected override ICollection<Campaign> FetchVisitorCacheCampaigns(VisitorDelegateAbstract visitor)
        {
            return new Collection<Campaign>();
        }

        public override Task UserExposed<T>(string key, T defaultValue, FlagDTO flag)
        {

            return Task.Factory.StartNew(() =>
            {
                Log("UserExposed");
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
            Logger.Log.LogError(Config, string.Format(Constants.METHOD_DEACTIVATED_CONSENT_ERROR, methodName, Visitor.VisitorId), methodName);
        }
    }
}

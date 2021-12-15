using Flagship.Enums;
using Flagship.Hit;
using Flagship.Model;
using System;
using System.Collections.Generic;
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
            Utils.Log.LogError(Config, string.Format(Constants.METHOD_DEACTIVATED_CONSENT_ERROR, methodName, Visitor.VisitorId), methodName);
        }
    }
}

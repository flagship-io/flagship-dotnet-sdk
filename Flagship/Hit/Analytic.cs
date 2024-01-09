using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flagship.Hit
{
    internal class Analytic : Diagnostic
    {
        public Analytic() : base(HitType.USAGE)
        {
        }

        internal override IDictionary<string, object> ToApiKeys()
        {
            VisitorId = null;
            AnonymousId = null;
            return base.ToApiKeys();
        }
    }
}

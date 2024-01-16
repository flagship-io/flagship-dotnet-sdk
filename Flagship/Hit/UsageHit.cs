using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flagship.Hit
{
    internal class UsageHit : Diagnostic
    {
        public UsageHit() : base(HitType.USAGE)
        {
        }
    }
}

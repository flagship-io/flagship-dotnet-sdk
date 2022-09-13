using Flagship.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flagship.Api
{
    internal interface ITrackingManagerCommon
    {
        public FlagshipConfig Config { get; set; }

        public void Add(Hit.HitAbstract hit);
    }
}

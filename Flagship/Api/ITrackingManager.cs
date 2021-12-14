using Flagship.Config;
using Flagship.FsVisitor;
using Flagship.Hit;
using Flagship.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flagship.Api
{
    internal interface ITrackingManager
    {
        FlagshipConfig Config { get; set; }

        Task SendActive(VisitorDelegateAbstract visitor, FlagDTO flag);

        Task SendHit(HitAbstract hit);
    }
}

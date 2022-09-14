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
        FlagshipConfig Config { get; set; }

        Task Add(Hit.HitAbstract hit);
    } 
}

using Flagship.Config;
using Flagship.Enums;
using Flagship.Hit;
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

        Task Add(HitAbstract hit);

        Task ActivateFlag(Activate hit);

        Task SendBatch(CacheTriggeredBy batchTriggeredBy = CacheTriggeredBy.BatchLength);
    } 
}

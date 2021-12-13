using Flagship.Api;
using Flagship.Decision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flagship.Config
{
    interface IConfigManager
    {
        IDecisionManager DecisionManager { get; set; }

        FlagshipConfig Config { get; set; }

        ITrackingManager TrackingManager { get; set; }

    }
}

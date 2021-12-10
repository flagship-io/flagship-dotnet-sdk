using Flagship.Api;
using Flagship.Decision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flagship.Config
{
    public interface IConfigManager
    {
        public IDecisionManager DecisionManager { get; set; }

        public FlagshipConfig Config { get; set; }

        public ITrackingManager TrackingManager { get; set; }

    }
}

using Flagship.Api;
using Flagship.Decision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flagship.Config
{
    internal class ConfigManager : IConfigManager
    {
        public IDecisionManager DecisionManager { get; set; }
        public FlagshipConfig Config { get; set; }
        public ITrackingManager TrackingManager { get; set; }

        public ConfigManager(FlagshipConfig config, IDecisionManager decisionManager, ITrackingManager trackingManager)
        {
            DecisionManager = decisionManager;
            Config = config;
            TrackingManager = trackingManager;
        }

    }
}

using Flagship.Api;
using Flagship.Decision;

namespace Flagship.Config
{
    interface IConfigManager
    {
        IDecisionManager DecisionManager { get; set; }

        FlagshipConfig Config { get; set; }

        ITrackingManager TrackingManager { get; set; }
    }
}

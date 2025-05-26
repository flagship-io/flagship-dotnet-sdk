using System.Threading.Tasks;
using Flagship.Hit;
using Flagship.Model;

namespace Flagship.Api
{
    internal interface ITrackingManager : ITrackingManagerCommon
    {
        void StartBatchingLoop();

        void StopBatchingLoop();

        Task SendTroubleshootingHit(Troubleshooting hit);
        void AddTroubleshootingHit(Troubleshooting hit);
        Task SendUsageHit(UsageHit hit);

        TroubleshootingData TroubleshootingData { set; get; }
    }
}

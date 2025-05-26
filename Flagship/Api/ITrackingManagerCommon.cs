using System.Threading.Tasks;
using Flagship.Config;
using Flagship.Enums;
using Flagship.Hit;

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

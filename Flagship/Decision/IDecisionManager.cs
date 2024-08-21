using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Flagship.Api;
using Flagship.Delegate;
using Flagship.FsVisitor;
using Flagship.Model;

namespace Flagship.Decision
{
    internal interface IDecisionManager
    {
        event StatusChangedDelegate StatusChange;

        HttpClient HttpClient { get; set; }

        TroubleshootingData TroubleshootingData { get; set; }

        bool IsPanic { get; }

        Task<ICollection<FlagDTO>> GetFlags(ICollection<Campaign> campaigns);

        Task<ICollection<Campaign>> GetCampaigns(VisitorDelegateAbstract visitor);

        string LastBucketingTimestamp { get; set; }

        string FlagshipInstanceId { get; set; }

        ITrackingManager TrackingManager { get; set; }
    }
}

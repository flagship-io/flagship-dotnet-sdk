using Flagship.Config;
using Flagship.Delegate;
using Flagship.FsVisitor;
using Flagship.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Flagship.Decision
{
    internal interface IDecisionManager
    {
        event StatusChangeDelegate StatusChange;

        HttpClient HttpClient { get; set; }

        TroubleshootingData TroubleshootingData { get; set; }

        bool IsPanic { get; }

        Task<ICollection<FlagDTO>> GetFlags (ICollection<Campaign> campaigns);

        Task<ICollection<Campaign>> GetCampaigns(VisitorDelegateAbstract visitor);

        string LastBucketingTimestamp { get; set; }

    }
}

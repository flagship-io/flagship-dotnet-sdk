using Flagship.Config;
using Flagship.Delegate;
using Flagship.FsVisitor;
using Flagship.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flagship.Decision
{
    internal interface IDecisionManager
    {
        public event StatusChangeDelegate StatusChange;

        public bool IsPanic { get; }

        public Task<ICollection<FlagDTO>> GetFlags (ICollection<Campaign> campaigns);

        public Task<ICollection<Campaign>> GetCampaigns(VisitorDelegateAbstract visitor);

    }
}

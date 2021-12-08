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
    public abstract class DecisionManager : IDecisionManager
    {
        public event StatusChangeDelegate StatusChange;

        public FlagshipConfig Config { get; set; }
        public HttpClient HttpClient { get; set; }

        public DecisionManager(HttpClient httpClient, FlagshipConfig config)
        {
            HttpClient = httpClient;
            Config = config;    
        }

        public Task<ICollection<Campaign>> GetCampaigns(VisitorDelegateAbstract visitor)
        {
            throw new NotImplementedException();
        }

        public ICollection<FlagDTO> GetFlags(ICollection<Campaign> campaigns)
        {
            throw new NotImplementedException();
        }

        public bool IsPanic()
        {
            throw new NotImplementedException();
        }
    }
}

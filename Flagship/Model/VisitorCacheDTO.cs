using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flagship.Model
{
    internal class VisitorCacheCampaign
    {
        public string CampaignId { get; set; }

        public string VariationGroupId { get; set; }

        public string VariationId { get; set; }

        public bool? IsReference { get; set; }

        public string Type { get; set; }

        public bool? Activated { get; set; }

        public object Flags { get; set; }

    }
    internal class VisitorCacheData
    {
        public string VisitorId { get; set; } 

        public string AnonymousId { get; set; }

        public bool? Consent { get; set; }

        public Dictionary<string,object> Context { get; set; }

        public IEnumerable<VisitorCacheCampaign> Campaigns { get; set; }
    }
     
    internal class VisitorCacheDTO
    {
        public int Version { get; set; }

        public VisitorCacheData Data { get; set; }
    }
}

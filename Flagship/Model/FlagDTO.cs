using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flagship.Model
{
    public class FlagDTO
    {
        public string Key { get; set; }
        public string CampaignId { get; set; }

        public string VariationGroupId { get; set; }    

        public string VariationId { get; set; }

        public bool IsReference { get; set; }

        public IDictionary<string,object> Value { get; set; }

    }
}

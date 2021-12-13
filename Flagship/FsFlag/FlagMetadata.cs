using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flagship.FsFlag
{
    public class FlagMetadata : IFlagMetadata
    {
        public string CampaignId { get; set; }

        public string VariationGroupId { get; set; }

        public string VariationId { get; set; }

        public bool IsReference { get; set; }

        public string CampaignType { get; set; }

        internal FlagMetadata(string campaignId, string variationGroupId, string variationId, bool isReference, string campaignType)
        {
            CampaignId = campaignId;
            VariationGroupId = variationGroupId;
            VariationId = variationId;
            IsReference = isReference;
            CampaignType = campaignType;

        }

        public string ToJson()
        {
            try
            {
                return JsonConvert.SerializeObject(this);
            }
            catch (Exception)
            {
                return "{}";
            }
             
        }

        internal static IFlagMetadata EmptyMetadata()
        {
            return new FlagMetadata("", "", "", false, "");
        }

    }
}

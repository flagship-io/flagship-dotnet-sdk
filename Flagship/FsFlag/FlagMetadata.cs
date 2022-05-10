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
        [JsonProperty("campaignId")]
        public string CampaignId { get; set; }

        [JsonProperty("variationGroupId")]
        public string VariationGroupId { get; set; }

        [JsonProperty("variationId")]
        public string VariationId { get; set; }

        [JsonProperty("isReference")]
        public bool IsReference { get; set; }

        [JsonProperty("campaignType")]
        public string CampaignType { get; set; }

        [JsonProperty("slug")]
        public string Slug { get; set; }

        internal FlagMetadata(string campaignId, string variationGroupId, string variationId, bool isReference, string campaignType, string slug)
        {
            CampaignId = campaignId;
            VariationGroupId = variationGroupId;
            VariationId = variationId;
            IsReference = isReference;
            CampaignType = campaignType;
            Slug = slug;
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        internal static IFlagMetadata EmptyMetadata()
        {
            return new FlagMetadata("", "", "", false, "", null);
        }

    }
}

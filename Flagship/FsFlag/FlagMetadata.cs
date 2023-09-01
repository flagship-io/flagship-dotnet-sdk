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

        [JsonProperty("campaignName")]
        public string CampaignName { get; set; }

        [JsonProperty("variationGroupName")]
        public string VariationGroupName { get; set; }

        [JsonProperty("variationName")]
        public string VariationName { get; set; }

        internal FlagMetadata(string campaignId, string variationGroupId, string variationId, bool isReference, string campaignType, string slug, string campaignName, string variationGroupName, string variaitonName)
        {
            CampaignId = campaignId;
            VariationGroupId = variationGroupId;
            VariationId = variationId;
            IsReference = isReference;
            CampaignType = campaignType;
            Slug = slug;
            CampaignName = campaignName;
            VariationGroupName = variationGroupName;
            VariationName = variaitonName;
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        internal static IFlagMetadata EmptyMetadata()
        {
            return new FlagMetadata("", "", "", false, "", null, "", "", "");
        }

    }
}

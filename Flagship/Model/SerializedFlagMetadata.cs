using Newtonsoft.Json;

namespace Flagship.Model
{
    internal class SerializedFlagMetadata
    {
        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("campaignId")]
        public string CampaignId { get; set; }

        [JsonProperty("campaignName")]
        public string CampaignName { get; set; }

        [JsonProperty("variationGroupId")]
        public string VariationGroupId { get; set; }

        [JsonProperty("variationGroupName")]
        public string VariationGroupName { get; set; }

        [JsonProperty("variationId")]
        public string VariationId { get; set; }

        [JsonProperty("variationName")]
        public string VariationName { get; set; }

        [JsonProperty("isReference")]
        public bool? IsReference { get; set; }

        [JsonProperty("campaignType")]
        public string CampaignType { get; set; }

        [JsonProperty("slug")]
        public string Slug { get; set; }

        [JsonProperty("hex")]
        public string Hex { get; set; }
    }
}

using Newtonsoft.Json;

namespace Flagship.Model
{
    class ThirdPartySegmentDTO
    {
        [JsonProperty("visitor_id")]
        public string VisitorId { get; set; }

        [JsonProperty("segment")]
        public string Segment { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("expiration")]
        public int Expiration { get; set; }

        [JsonProperty("partner")]
        public string Partner { get; set; }
    }
}

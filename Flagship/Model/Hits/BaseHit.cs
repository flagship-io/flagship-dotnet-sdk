using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Flagship.Model.Hits
{
    public abstract class BaseHit
    {
        [JsonProperty("ds")]
        public static string DataSource { get { return "APP"; } }

        [JsonProperty("t")]
        [JsonConverter(typeof(StringEnumConverter))]
        public HitType Type { get; private set; }

        [JsonProperty("vid")]
        public string VisitorId { get; private set; }

        [JsonProperty("cid")]
        public string EnvironmentId { get; private set; }

        [JsonProperty("dl")]
        public string DocumentLocation { get; set; }

        [JsonProperty("pt")]
        public string PageTitle { get; set; }

        [JsonProperty("v")]
        public string ProtocolVersion { get; set; }

        [JsonProperty("uip")]
        public string UserIP { get; set; }

        [JsonProperty("dr")]
        public string DocumentReferrer { get; set; }

        [JsonProperty("vp")]
        public string ViewportSize { get; set; }

        [JsonProperty("sr")]
        public string ScreenResolution { get; set; }

        [JsonProperty("de")]
        public string DocumentEncoding { get; set; }

        [JsonProperty("sd")]
        public string ScreenColorDepth { get; set; }

        [JsonProperty("ul")]
        public string UserLanguage { get; set; }

        [JsonProperty("je")]
        public string JavaEnabled { get; set; }

        [JsonProperty("fl")]
        public string FlashVersion { get; set; }

        [JsonProperty("qt")]
        public string QueueTime { get; set; }

        [JsonProperty("cst")]
        public string CurrentSessionTimestamp { get; set; }

        [JsonProperty("sn")]
        public string SessionNumber { get; set; }

        public abstract ValidationResult Validate();

        public void SetBaseInfos(HitType type, string environmentId, string visitorId)
        {
            Type = type;
            VisitorId = visitorId;
            EnvironmentId = environmentId;
        }
    }
}

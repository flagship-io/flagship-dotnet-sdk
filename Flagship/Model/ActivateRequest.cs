using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Flagship.Model
{
    public class ActivateRequest
    {
        [JsonProperty("cid")]
        public string EnvironmentId { get; private set; }

        [JsonProperty("vid")]
        public string VisitorId { get; private set; }

        [JsonProperty("caid")]
        public string VariationGroupId { get; set; }

        [JsonProperty("vaid")]
        public string VariationId { get; set; }

        public ActivateRequest(string environmentId, string visitorId, string variationGroupId, string variationId)
        {
            this.EnvironmentId = environmentId;
            this.VisitorId = visitorId;
            this.VariationGroupId = variationGroupId;
            this.VariationId = variationId;
        }
    }
}

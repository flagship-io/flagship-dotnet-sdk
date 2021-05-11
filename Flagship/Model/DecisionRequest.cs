using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Flagship.Model
{
    public class DecisionRequest
    {
        [JsonProperty("visitor_id")]
        public string VisitorId { get; private set; }

        [JsonProperty("context")]
        public IDictionary<string, object> Context { get; private set; }

        [JsonProperty("decision_group")]
        public string DecisionGroup { get; private set; }

        public DecisionRequest(string visitorId, IDictionary<string, object> context, string decisionGroup = null)
        {
            this.VisitorId = visitorId;
            this.Context = context;
            this.DecisionGroup = decisionGroup;
        }
    }
}

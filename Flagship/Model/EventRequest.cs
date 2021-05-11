using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Text;

namespace Flagship.Model
{
    public enum EventType
    {
        CONTEXT = 1
    }

    public class EventRequest
    {
        [JsonProperty("client_id")]
        public string EnvironmentId { get; private set; }

        [JsonProperty("visitor_id")]
        public string VisitorId { get; private set; }

        [JsonProperty("type")]
        [JsonConverter(typeof(StringEnumConverter))]
        public EventType Type { get; private set; }

        [JsonProperty("data")]
        public IDictionary<string, object> Data { get; private set; }

        public EventRequest(string environmentId, string visitorId, EventType type, IDictionary<string, object> data)
        {
            EnvironmentId = environmentId;
            VisitorId = visitorId;
            Type = type;
            Data = data;
        }
    }
}

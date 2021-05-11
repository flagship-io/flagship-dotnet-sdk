using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace QAApp.Model
{
    public class Environment
    {
        [JsonPropertyName("environment_id")]
        [Required]
        public string Id { get; set; }

        [JsonPropertyName("api_key")]
        [Required]
        public string ApiKey { get; set; }

        [JsonPropertyName("bucketing")]
        public bool Bucketing { get; set; }

        [JsonPropertyName("timeout")]
        public int Timeout { get; set; }

        [JsonPropertyName("polling_interval")]
        public int PollingInterval { get; set; }
    }
}

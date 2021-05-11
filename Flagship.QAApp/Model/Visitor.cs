using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace QAApp.Model
{
    public class Visitor
    {
        [JsonPropertyName("visitor_id")]
        [Required]
        public string Id { get; set; }

        [JsonPropertyName("context")]
        [Required]
        public Dictionary<string, JsonElement> Context { get; set; }
    }
}

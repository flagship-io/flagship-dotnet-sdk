using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using System.Linq;

namespace Flagship.Model.Hits
{
    public class Event : BaseHit
    {
        [JsonProperty("ea")]
        public string Action { get; set; }

        [JsonProperty("ec")]
        [JsonConverter(typeof(StringEnumConverter))]
        public EventCategory Category { get; set; }

        [JsonProperty("el")]
        public string Label { get; set; }

        [JsonProperty("ev")]
        public int Value { get; set; }

        public override ValidationResult Validate()
        {
            var errors = new HashSet<string>();
            if (string.IsNullOrEmpty(Action))
            {
                errors.Add("Event Action should not be empty");
            }

            return new ValidationResult()
            {
                Success = errors.Count == 0,
                Errors = errors.ToArray()
            };
        }
    }
}

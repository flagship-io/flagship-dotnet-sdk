using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using System.Linq;

namespace Flagship.Model.Hits
{
    public class Pageview : BaseHit
    {
        [JsonProperty("dl")]
        public new string DocumentLocation { get; set; }

        public override ValidationResult Validate()
        {
            var errors = new HashSet<string>();
            if (string.IsNullOrEmpty(DocumentLocation))
            {
                errors.Add("Pageview DocumentLocation should not be empty");
            }

            return new ValidationResult()
            {
                Success = errors.Count == 0,
                Errors = errors.ToArray()
            };
        }
    }
}

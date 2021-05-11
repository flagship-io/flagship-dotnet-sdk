using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using System.Linq;

namespace Flagship.Model.Hits
{
    public class Screenview : BaseHit
    {
        [JsonProperty("pt")]
        public new string PageTitle { get; set; }

        public override ValidationResult Validate()
        {
            var errors = new HashSet<string>();
            if (string.IsNullOrEmpty(DocumentLocation))
            {
                errors.Add("Screenview DocumentLocation should not be empty");
            }

            return new ValidationResult()
            {
                Success = errors.Count == 0,
                Errors = errors.ToArray()
            };
        }
    }
}

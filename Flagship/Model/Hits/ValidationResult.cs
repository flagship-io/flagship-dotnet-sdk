using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Flagship.Model.Hits
{
    public class ValidationResult
    {
        public bool Success { get; set; }

        public string[] Errors { get; set; }
    }
}

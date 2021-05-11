using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using System.Linq;

namespace Flagship.Model.Hits
{
    public class Item : BaseHit
    {
        [JsonProperty("tid")]
        [JsonRequired]
        public string TransactionId { get; set; }

        [JsonProperty("in")]
        [JsonRequired]
        public string Name { get; set; }

        [JsonProperty("ip")]
        public decimal Price { get; set; }

        [JsonProperty("iq")]
        public int Quantity { get; set; }

        /// <summary>
        /// Item code (SKU for instance)
        /// </summary>
        [JsonProperty("ic")]
        public string Code { get; set; }

        /// <summary>
        /// Item category
        /// </summary>
        [JsonProperty("iv")]
        public string Category { get; set; }

        public override ValidationResult Validate()
        {
            var errors = new HashSet<string>();
            if (string.IsNullOrEmpty(Name))
            {
                errors.Add("Item Name should not be empty");
            }
            if (string.IsNullOrEmpty(Code))
            {
                errors.Add("Item Code should not be empty");
            }
            if (string.IsNullOrEmpty(TransactionId))
            {
                errors.Add("Item TransactionId should not be empty");
            }

            return new ValidationResult()
            {
                Success = errors.Count == 0,
                Errors = errors.ToArray()
            };
        }
    }
}

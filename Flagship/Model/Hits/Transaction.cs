using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Flagship.Model.Hits
{
    public class Transaction : BaseHit
    {
        [JsonProperty("tid")]
        [JsonRequired]
        public string Id { get; set; }

        [JsonProperty("ta")]
        [JsonRequired]
        public string Affiliation { get; set; }

        [JsonProperty("tr")]
        public decimal? Revenue { get; set; }

        [JsonProperty("ts")]
        public decimal? Shipping { get; set; }

        [JsonProperty("tt")]
        public decimal? Tax { get; set; }

        [JsonProperty("tc")]
        public string Currency { get; set; }

        [JsonProperty("tcc")]
        public string CouponCode { get; set; }

        [JsonProperty("pm")]
        public string PaymentMethod { get; set; }

        [JsonProperty("sm")]
        public string ShippingMethod { get; set; }

        [JsonProperty("icn")]
        public int? ItemCount { get; set; }

        public override ValidationResult Validate()
        {
            var errors = new HashSet<string>();
            if (string.IsNullOrEmpty(Id))
            {
                errors.Add("Transaction Id should not be empty");
            }
            if (string.IsNullOrEmpty(Affiliation))
            {
                errors.Add("Transaction Affiliation should not be empty");
            }

            return new ValidationResult()
            {
                Success = errors.Count == 0,
                Errors = errors.ToArray()
            };
        }
    }
}

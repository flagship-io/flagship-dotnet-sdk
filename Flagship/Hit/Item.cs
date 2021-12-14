using Flagship.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flagship.Hit
{
    public class Item : HitAbstract
    {
        public string TransactionId { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public double? Price { get; set; }
        public double? Quantity { get; set; }
        public string Category { get; set; }

        public Item(string transactionId, string name, string code ):base(HitType.ITEM)
        {
            TransactionId = transactionId;
            Name = name;
            Code = code;
        }


        internal override IDictionary<string, object> ToApiKeys()
        {
            var apiKeys = base.ToApiKeys();
            apiKeys[Constants.TID_API_ITEM] = TransactionId;
            apiKeys[Constants.IN_API_ITEM] = Name;
            apiKeys[Constants.ICN_API_ITEM]= Code;

            if (Price.HasValue)
            {
                apiKeys[Constants.IP_API_ITEM] = Price.Value;
            }

            if (Quantity.HasValue)
            {
                apiKeys[Constants.IQ_API_ITEM]= Quantity.Value;
            }

            if (Category!=null)
            {
                apiKeys[Constants.IV_API_ITEM] = Category;
            }

            return apiKeys;
        }

        internal override bool IsReady()
        {
            return base.IsReady() && 
                !string.IsNullOrWhiteSpace(TransactionId) && 
                !string.IsNullOrWhiteSpace(Name) && 
                !string.IsNullOrWhiteSpace(Code);
        }

        internal override string GetErrorMessage()
        {
            return Constants.HIT_ITEM_ERROR_MESSAGE;
        }

    }
}

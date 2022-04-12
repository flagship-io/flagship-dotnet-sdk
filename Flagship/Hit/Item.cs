using Flagship.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flagship.Hit
{
    /// <summary>
    /// This hit is used to link an item with a transaction. It must be sent after the corresponding transaction hit.
    /// </summary>
    public class Item : HitAbstract
    {
        /// <summary>
        /// Unique identifier for your transaction.
        /// </summary>
        public string TransactionId { get; set; }

        /// <summary>
        /// Name of your item.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Specifies the SKU or item code.
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Specifies the price for a single item/unit.
        /// </summary>
        public double? Price { get; set; }

        /// <summary>
        /// Specifies the number of items purchased.
        /// </summary>
        public double? Quantity { get; set; }

        /// <summary>
        /// Specifies the category that the item belongs to.
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// This hit is used to link an item with a transaction. It must be sent after the corresponding transaction hit.
        /// </summary>
        /// <param name="transactionId">Unique identifier for your transaction.</param>
        /// <param name="name">Name of your item.</param>
        /// <param name="code">Specifies the SKU or item code.</param>
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

        internal override bool IsReady(bool checkParent = true)
        {
            return (!checkParent || base.IsReady()) && 
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

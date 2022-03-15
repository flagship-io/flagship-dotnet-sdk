using Flagship.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flagship.Hit
{
    public class Transaction:HitAbstract
    {
        public string TransactionId { get; set; }
        public string Affiliation { get; set; }
        public double? Taxes { get; set; }
        public string Currency { get; set; }
        public string CouponCode { get; set; }
        public double? ItemCount { get; set; }
        public string ShippingMethod { get; set; }
        public string PaymentMethod { get; set; }
        public double? TotalRevenue { get; set; }
        public double? ShippingCosts { get; set; }

        public Transaction(string transactionId, string affiliation):base(HitType.TRANSACTION)
        {
            TransactionId = transactionId;
            Affiliation = affiliation;
        }

        internal override IDictionary<string, object> ToApiKeys()
        {
            var apiKeys = base.ToApiKeys();
            apiKeys[Constants.TID_API_ITEM] = TransactionId;
            apiKeys[Constants.TA_API_ITEM] = Affiliation;

            if (Taxes.HasValue)
            {
                apiKeys[Constants.TT_API_ITEM]= Taxes.Value;
            }

            if (Currency!=null)
            {
                apiKeys[Constants.TC_API_ITEM] = Currency;
            }

            if (CouponCode != null)
            {
                apiKeys[Constants.TCC_API_ITEM]= CouponCode;
            }

            if (ItemCount.HasValue)
            {
                apiKeys[Constants.ICN_API_ITEM] = ItemCount.Value;
            }

            if (ShippingMethod!=null)
            {
                apiKeys[Constants.SM_API_ITEM] = ShippingMethod;
            }

            if (PaymentMethod!=null)
            {
                apiKeys[Constants.PM_API_ITEM] = PaymentMethod;
            }

            if (TotalRevenue.HasValue)
            {
                apiKeys[Constants.TR_API_ITEM]= TotalRevenue.Value;
            }

            if (ShippingCosts.HasValue)
            {
                apiKeys[Constants.TS_API_ITEM]= ShippingCosts.Value;
            }

            return apiKeys;
        }

        internal override bool IsReady(bool checkParent = true)
        {
            return (!checkParent || base.IsReady()) && !string.IsNullOrWhiteSpace(TransactionId) && !string.IsNullOrWhiteSpace(Affiliation);
        }

        internal override string GetErrorMessage()
        {
            return Constants.HIT_TRANSACTION_ERROR_MESSAGE;
        }

    }
}

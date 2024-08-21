using System.Collections.Generic;
using Flagship.Enums;

namespace Flagship.Hit
{
    /// <summary>
    /// This hit should be sent when a user complete a Transaction.
    /// </summary>
    public class Transaction : HitAbstract
    {
        /// <summary>
        /// Unique identifier for your transaction.
        /// </summary>
        public string TransactionId { get; set; }

        /// <summary>
        /// The name of the KPI that you will have inside your reporting.
        /// </summary>
        public string Affiliation { get; set; }

        /// <summary>
        /// Specifies the total amount of taxes in your transaction.
        /// </summary>
        public double? Taxes { get; set; }

        /// <summary>
        /// Specifies the currency of your transaction. NOTE: This value should be a valid ISO 4217 currency code.
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// Specifies the coupon code used by the customer in your transaction.
        /// </summary>
        public string CouponCode { get; set; }

        /// <summary>
        /// Specifies the number of items in your transaction.
        /// </summary>
        public int? ItemCount { get; set; }

        /// <summary>
        /// The shipping method for your transaction.
        /// </summary>
        public string ShippingMethod { get; set; }

        /// <summary>
        /// Specifies the payment method used for your transaction.
        /// </summary>
        public string PaymentMethod { get; set; }

        /// <summary>
        /// Specifies the total revenue associated with the transaction. This value should include any shipping and/or tax amounts.
        /// </summary>
        public double? TotalRevenue { get; set; }

        /// <summary>
        /// The total shipping cost of your transaction.
        /// </summary>
        public double? ShippingCosts { get; set; }

        /// <summary>
        /// This hit should be sent when a user complete a Transaction.
        /// </summary>
        /// <param name="transactionId">Unique identifier for your transaction.</param>
        /// <param name="affiliation">The name of the KPI that you will have inside your reporting. </param>
        public Transaction(string transactionId, string affiliation)
            : base(HitType.TRANSACTION)
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
                apiKeys[Constants.TT_API_ITEM] = Taxes.Value;
            }

            if (Currency != null)
            {
                apiKeys[Constants.TC_API_ITEM] = Currency;
            }

            if (CouponCode != null)
            {
                apiKeys[Constants.TCC_API_ITEM] = CouponCode;
            }

            if (ItemCount.HasValue)
            {
                apiKeys[Constants.ICN_API_ITEM] = ItemCount.Value;
            }

            if (ShippingMethod != null)
            {
                apiKeys[Constants.SM_API_ITEM] = ShippingMethod;
            }

            if (PaymentMethod != null)
            {
                apiKeys[Constants.PM_API_ITEM] = PaymentMethod;
            }

            if (TotalRevenue.HasValue)
            {
                apiKeys[Constants.TR_API_ITEM] = TotalRevenue.Value;
            }

            if (ShippingCosts.HasValue)
            {
                apiKeys[Constants.TS_API_ITEM] = ShippingCosts.Value;
            }

            return apiKeys;
        }

        internal override bool IsReady(bool checkParent = true)
        {
            return (!checkParent || base.IsReady())
                && !string.IsNullOrWhiteSpace(TransactionId)
                && !string.IsNullOrWhiteSpace(Affiliation);
        }

        internal override string GetErrorMessage()
        {
            return Constants.HIT_TRANSACTION_ERROR_MESSAGE;
        }
    }
}

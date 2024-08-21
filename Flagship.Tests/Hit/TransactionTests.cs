using Microsoft.VisualStudio.TestTools.UnitTesting;
using Flagship.Enums;
using Moq;

namespace Flagship.Hit.Tests
{
    [TestClass()]
    public class TransactionTests
    {
        [TestMethod()]
        public void TransactionTest()
        {
            var config = new Config.DecisionApiConfig()
            {
                EnvId = "envID",
                ApiKey = "apiKey"
            };

            var transactionId = "transactionId";
            var affiliation = "affiliation";
            var taxes = 100f;
            var currency = "ItemCategory";
            var visitorId = "VisitorId";
            var couponCode = "couponCode";
            var itemCount = 5;
            var shippingMethod = "shippingMethod";
            var paymentMethod = "paymentMethod";
            var totalRevenue = 450f;
            var shippingCosts = 10f;

            var transactionMock = new Mock<Transaction>(transactionId, affiliation) { CallBase = true };

            var currentTime = DateTime.Now;
            transactionMock.SetupGet(x => x.CurrentDateTime).Returns(currentTime);

            var transaction = transactionMock.Object;

            transaction.Taxes = taxes;
            transaction.Currency = currency;
            transaction.CouponCode = couponCode;
            transaction.ItemCount = itemCount;
            transaction.ShippingMethod = shippingMethod;
            transaction.PaymentMethod = paymentMethod;
            transaction.TotalRevenue = totalRevenue;
            transaction.ShippingCosts = shippingCosts;
            transaction.Config = config;
            transaction.VisitorId = visitorId;
            transaction.DS = Constants.SDK_APP;
            transaction.CreatedAt = currentTime;



            Assert.AreEqual(transactionId, transaction.TransactionId);
            Assert.AreEqual(affiliation, transaction.Affiliation);
            Assert.AreEqual(taxes, transaction.Taxes);
            Assert.AreEqual(currency, transaction.Currency);
            Assert.AreEqual(couponCode, transaction.CouponCode);
            Assert.AreEqual(itemCount, transaction.ItemCount);
            Assert.AreEqual(shippingMethod, transaction.ShippingMethod);
            Assert.AreEqual(paymentMethod, transaction.PaymentMethod);
            Assert.AreEqual(totalRevenue, transaction.TotalRevenue);
            Assert.AreEqual(shippingCosts, transaction.ShippingCosts);


            var keys = Newtonsoft.Json.JsonConvert.SerializeObject(transaction.ToApiKeys());

            var apiKeys = new Dictionary<string, object>()
            {
                [Constants.VISITOR_ID_API_ITEM] = visitorId,
                [Constants.DS_API_ITEM] = Constants.SDK_APP,
                [Constants.CUSTOMER_ENV_ID_API_ITEM] = config.EnvId,
                [Constants.T_API_ITEM] = $"{Hit.HitType.TRANSACTION}",
                [Constants.CUSTOMER_UID] = null,
                [Constants.QT_API_ITEM] = 0,
                [Constants.TID_API_ITEM] = transactionId,
                [Constants.TA_API_ITEM] = affiliation,
                [Constants.TT_API_ITEM] = taxes,
                [Constants.TC_API_ITEM] = currency,
                [Constants.TCC_API_ITEM] = couponCode,
                [Constants.ICN_API_ITEM] = itemCount,
                [Constants.SM_API_ITEM] = shippingMethod,
                [Constants.PM_API_ITEM] = paymentMethod,
                [Constants.TR_API_ITEM] = totalRevenue,
                [Constants.TS_API_ITEM] = shippingCosts
            };

            var apiKeysJson = Newtonsoft.Json.JsonConvert.SerializeObject(apiKeys);

            Assert.AreEqual(apiKeysJson, keys);

            Assert.IsTrue(transaction.IsReady());

            Assert.AreEqual(transaction.GetErrorMessage(), Constants.HIT_TRANSACTION_ERROR_MESSAGE);

            transaction = new Hit.Transaction(null, null);
            Assert.IsFalse(transaction.IsReady());
        }
    }
}
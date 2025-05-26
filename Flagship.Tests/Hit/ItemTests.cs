using Microsoft.VisualStudio.TestTools.UnitTesting;
using Flagship.Enums;
using Moq;

namespace Flagship.Hit.Tests
{
    [TestClass()]
    public class ItemTests
    {
        [TestMethod()]
        public void ItemTest()
        {
            var config = new Config.DecisionApiConfig()
            {
                EnvId = "envID",
                ApiKey = "apiKey"
            };

            var transactionId = "transactionId";
            var code = "itemCode";
            var name = "itemName";
            var price = 100f;
            var quantity = 5;
            var category = "ItemCategory";
            var visitorId = "VisitorId";

            var itemMock = new Mock<Item>(transactionId, name, code) { CallBase = true };


            var currentTime = DateTime.Now;
            itemMock.SetupGet(x => x.CurrentDateTime).Returns(currentTime);

            var item = itemMock.Object;

            item.Price = price;
            item.Quantity = quantity;
            item.Category = category;
            item.Config = config;
            item.VisitorId = visitorId;
            item.DS = Constants.SDK_APP;
            item.CreatedAt = currentTime;
            

            Assert.AreEqual(transactionId, item.TransactionId);
            Assert.AreEqual(name, item.Name);
            Assert.AreEqual(code, item.Code);
            Assert.AreEqual(price, item.Price);
            Assert.AreEqual(quantity, item.Quantity);
            Assert.AreEqual(category, item.Category);

            var keys = Newtonsoft.Json.JsonConvert.SerializeObject(item.ToApiKeys());

            var apiKeys = new Dictionary<string, object>()
            {
                [Constants.VISITOR_ID_API_ITEM] = visitorId,
                [Constants.DS_API_ITEM] = Constants.SDK_APP,
                [Constants.CUSTOMER_ENV_ID_API_ITEM] = config.EnvId,
                [Constants.T_API_ITEM] = $"{Hit.HitType.ITEM}",
                [Constants.CUSTOMER_UID] = null,
                [Constants.QT_API_ITEM] = 0,
                [Constants.TID_API_ITEM] = transactionId,
                [Constants.IN_API_ITEM] = name,
                [Constants.IC_API_ITEM] = code,
                [Constants.IP_API_ITEM] = price,
                [Constants.IQ_API_ITEM] = quantity,
                [Constants.IV_API_ITEM] = category,
        };

            var apiKeysJson = Newtonsoft.Json.JsonConvert.SerializeObject(apiKeys);

            Assert.AreEqual(apiKeysJson, keys);

            Assert.IsTrue(item.IsReady());

            Assert.AreEqual(item.GetErrorMessage(), Constants.HIT_ITEM_ERROR_MESSAGE);

            item = new Hit.Item(null, null, null);

            Assert.IsFalse(item.IsReady());

        }
    }
}
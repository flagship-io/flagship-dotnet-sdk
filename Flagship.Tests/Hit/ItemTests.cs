﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Flagship.Hit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flagship.Enums;

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
            var quantity = 5f;
            var category = "ItemCategory";
            var visitorId = "VisitorId";

            var item = new Hit.Item(transactionId, name, code)
            {
                Price = price,
                Quantity = quantity,
                Category = category,
                Config = config,
                VisitorId = visitorId,
                DS = Constants.SDK_APP
            };

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
                [Constants.TID_API_ITEM] = transactionId,
                [Constants.IN_API_ITEM] = name,
                [Constants.ICN_API_ITEM] = code,
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
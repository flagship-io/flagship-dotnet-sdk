using Microsoft.VisualStudio.TestTools.UnitTesting;
using Flagship.Hit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flagship.Enums;
using Moq;

namespace Flagship.Hit.Tests
{
    [TestClass()]
    public class EventTests
    {
        [TestMethod()]
        public void EventTest()
        {
            var config = new Config.DecisionApiConfig()
            {
                EnvId = "envID",
                ApiKey ="apiKey"
            };
            var action = "click";
            var label = "lable";
            uint value = 12;
            var visitorId = "VisitorId";



            var hitEventMock = new Mock<Event>(EventCategory.USER_ENGAGEMENT, action)
            {
                CallBase = true
            };

            var currentTime = DateTime.Now;
            hitEventMock.SetupGet(x => x.CurrentDateTime).Returns(currentTime);

            var hitEvent = hitEventMock.Object;

            hitEvent.Label = label;
            hitEvent.Value = value;
            hitEvent.Config = config;
            hitEvent.VisitorId = visitorId;
            hitEvent.DS = Constants.SDK_APP;
            hitEvent.CreatedAt = currentTime;
            

            var keys = Newtonsoft.Json.JsonConvert.SerializeObject(hitEvent.ToApiKeys());

            var apiKeys = new Dictionary<string, object>()
            {
                [Constants.VISITOR_ID_API_ITEM] = visitorId,
                [Constants.DS_API_ITEM] = Constants.SDK_APP,
                [Constants.CUSTOMER_ENV_ID_API_ITEM] = config.EnvId,
                [Constants.T_API_ITEM] = $"{Hit.HitType.EVENT}",
                [Constants.CUSTOMER_UID] = null,
                [Constants.QT_API_ITEM] = 0,
                [Constants.EVENT_CATEGORY_API_ITEM] = $"{EventCategory.USER_ENGAGEMENT}",
                [Constants.EVENT_ACTION_API_ITEM] = action,
                [Constants.EVENT_LABEL_API_ITEM] = label,
                [Constants.EVENT_VALUE_API_ITEM] = value
            };

            var apiKeysJson = Newtonsoft.Json.JsonConvert.SerializeObject(apiKeys);

            Assert.AreEqual(EventCategory.USER_ENGAGEMENT, hitEvent.Category);
            Assert.AreEqual(action, hitEvent.Action);
            Assert.AreEqual(value, hitEvent.Value);
            Assert.AreEqual(label, hitEvent.Label);
            Assert.AreEqual(apiKeysJson, keys);

            Assert.IsTrue(hitEvent.IsReady());

            Assert.AreEqual(hitEvent.GetErrorMessage(), Constants.HIT_EVENT_ERROR_MESSSAGE);


            hitEvent = new Hit.Event(EventCategory.USER_ENGAGEMENT, null);
            Assert.IsFalse(hitEvent.IsReady());
        }
    }
}
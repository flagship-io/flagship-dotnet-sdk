﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Flagship.Enums;
using Moq;

namespace Flagship.Hit.Tests
{
    [TestClass()]
    public class ScreenTests
    {
        [TestMethod()]
        public void ScreenTest()
        {
            var config = new Config.DecisionApiConfig()
            {
                EnvId = "envID",
                ApiKey = "apiKey"
            };

            var viewName = "HomeView";
            var visitorId = "VisitorId";
            var anonymousId = "anonymousId";
            var userIp = "127.0.0.1";
            var screenResolution = "800X650";
            var locale = "en";
            var sessionNumber = "1";

            var screenMock = new Mock<Screen>(viewName)
            {
                CallBase = true
            };

            var currentTime = DateTime.Now;
            screenMock.SetupGet(x => x.CurrentDateTime).Returns(currentTime);

            var screen = screenMock.Object;

            screen.Config = config;
            screen.VisitorId = visitorId;
            screen.DS = Constants.SDK_APP;
            screen.AnonymousId = anonymousId;
            screen.UserIp = userIp;
            screen.ScreenResolution = screenResolution;
            screen.Locale = locale;
            screen.SessionNumber = sessionNumber;
            screen.CreatedAt = currentTime;
            

            Assert.AreEqual(screen.DocumentLocation, viewName);
            Assert.AreEqual(screen.Config, config);
            Assert.AreEqual(screen.VisitorId, visitorId);
            Assert.AreEqual(screen.AnonymousId, anonymousId);
            Assert.AreEqual(screen.UserIp, userIp);
            Assert.AreEqual(screen.ScreenResolution, screenResolution);
            Assert.AreEqual(screen.Locale, locale);
            Assert.AreEqual(screen.SessionNumber, sessionNumber);

            var keys = Newtonsoft.Json.JsonConvert.SerializeObject(screen.ToApiKeys());

            var apiKeys = new Dictionary<string, object>()
            {
                [Constants.VISITOR_ID_API_ITEM] = anonymousId,
                [Constants.DS_API_ITEM] = Constants.SDK_APP,
                [Constants.CUSTOMER_ENV_ID_API_ITEM] = config.EnvId,
                [Constants.T_API_ITEM] = $"{Hit.HitType.SCREENVIEW}",
                [Constants.CUSTOMER_UID] = visitorId,
                [Constants.QT_API_ITEM] = 0,
                [Constants.USER_IP_API_ITEM] = userIp,
                [Constants.SCREEN_RESOLUTION_API_ITEM] = screenResolution,
                [Constants.USER_LANGUAGE] = locale,
                [Constants.SESSION_NUMBER] = sessionNumber,
                [Constants.DL_API_ITEM] = viewName,
            };

            var apiKeysJson = Newtonsoft.Json.JsonConvert.SerializeObject(apiKeys);

            Assert.AreEqual(apiKeysJson, keys);

            Assert.IsTrue(screen.IsReady());

            Assert.AreEqual(screen.GetErrorMessage(), Constants.HIT_SCREEN_ERROR_MESSAGE);

            screen = new Hit.Screen(null)
            {
                Config = config,
            };
            Assert.IsFalse(screen.IsReady());


        }
    }
}
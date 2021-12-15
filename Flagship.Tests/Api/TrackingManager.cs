using Flagship.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using RichardSzalay.MockHttp;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Flagship.Tests.Api
{
    [TestClass]
    public class TrackingManager
    {
        [TestMethod]
        public async Task TestMethod1()
        {

            HttpResponseMessage httpResponse = new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent("", Encoding.UTF8, "application/json")
            };

            Mock<HttpMessageHandler> mockHandler = new Mock<HttpMessageHandler>();


            mockHandler.Protected().Setup<Task<HttpResponseMessage>>(
                 "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>()
                ).ReturnsAsync(httpResponse);

         

            var config = new Flagship.Config.DecisionApiConfig();
            var httpClient = new HttpClient(mockHandler.Object);
            var trackingManager = new Flagship.Api.TrackingManager(config, httpClient);

            var decisionManagerMock = new Mock<Flagship.Decision.IDecisionManager>();
            var configManager = new Flagship.Config.ConfigManager(config, decisionManagerMock.Object, trackingManager);

            var context = new Dictionary<string, object>();

            var visitorDelegate = new Flagship.FsVisitor.VisitorDelegate("visitorId", false, context, true, configManager);

            var flag = new Flagship.Model.FlagDTO();

            await trackingManager.SendActive(visitorDelegate, flag).ConfigureAwait(false);

            mockHandler.Protected().Verify("SendAsync", Times.Exactly(2), new object[] {  ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>() });

            httpResponse.Dispose();
            httpClient.Dispose();
        }
    }
}

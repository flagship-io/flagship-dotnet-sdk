using Flagship.Enums;
using Flagship.Logger;
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
        public async Task SendActive()
        {

            //HttpResponseMessage httpResponse = new HttpResponseMessage
            //{
            //    StatusCode = System.Net.HttpStatusCode.OK,
            //    Content = new StringContent("", Encoding.UTF8, "application/json")
            //};

            //Mock<HttpMessageHandler> mockHandler = new Mock<HttpMessageHandler>();

            //mockHandler.Protected().Setup<Task<HttpResponseMessage>>(
            //     "SendAsync",
            //      ItExpr.IsAny<HttpRequestMessage>(),
            //      ItExpr.IsAny<CancellationToken>()
            //    ).ReturnsAsync(httpResponse);


            //var fsLogManagerMock = new Mock<IFsLogManager>();

            //var config = new Flagship.Config.DecisionApiConfig
            //{
            //    ApiKey = "apiKey",
            //    EnvId = "envId",
            //    LogManager = fsLogManagerMock.Object,
            //};

            //var httpClient = new HttpClient(mockHandler.Object);
            //var trackingManager = new Flagship.Api.TrackingManager(config, httpClient);

            //var decisionManagerMock = new Mock<Flagship.Decision.IDecisionManager>();
            //var configManager = new Flagship.Config.ConfigManager(config, decisionManagerMock.Object, trackingManager);

            //var context = new Dictionary<string, object>();

            //var visitorDelegate = new Flagship.FsVisitor.VisitorDelegate("visitorId", false, context, true, configManager);

            //var flag = new Flagship.Model.FlagDTO()
            //{
            //    VariationGroupId = "varGroupID",
            //    VariationId = "varID",

            //};

            //var postData = new Dictionary<string, object>
            //{
            //    [Constants.VISITOR_ID_API_ITEM] = visitorDelegate.VisitorId,
            //    [Constants.VARIATION_ID_API_ITEM] = flag.VariationId,
            //    [Constants.VARIATION_GROUP_ID_API_ITEM] = flag.VariationGroupId,
            //    [Constants.CUSTOMER_ENV_ID_API_ITEM] = config.EnvId,
            //    [Constants.ANONYMOUS_ID] = null
            //};

            

            //Func<HttpRequestMessage, bool> action = (HttpRequestMessage x) => {

            //    var postDataString = JsonConvert.SerializeObject(postData);
            //    var headers = new HttpRequestMessage().Headers;
            //    headers.Add(Constants.HEADER_X_API_KEY, config.ApiKey);
            //    headers.Add(Constants.HEADER_X_SDK_CLIENT, Constants.SDK_LANGUAGE);
            //    headers.Add(Constants.HEADER_X_SDK_VERSION, Constants.SDK_VERSION);
            //    headers.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.HEADER_APPLICATION_JSON));

            //    var result = x.Content.ReadAsStringAsync().Result ;
            //    return result == postDataString && headers.ToString() == x.Headers.ToString() && x.Method == HttpMethod.Post;
            //};

            //var errorSendAsyn = new Exception("Error Send");

            //mockHandler.Protected().Setup<Task<HttpResponseMessage>>(
            //     "SendAsync",
            //      ItExpr.Is<HttpRequestMessage>(x => action(x)),
            //      ItExpr.IsAny<CancellationToken>()
            //    ).ReturnsAsync(httpResponse).Verifiable();

            //await trackingManager.SendActive(visitorDelegate, flag).ConfigureAwait(false);

            //// XC test

            //mockHandler.Protected().Setup<Task<HttpResponseMessage>>(
            //     "SendAsync",
            //      ItExpr.Is<HttpRequestMessage>(x => action(x)),
            //      ItExpr.IsAny<CancellationToken>()
            //    ).ReturnsAsync(httpResponse).Verifiable();


            //var newVisitorId = "newVisitorId";
            //visitorDelegate.Authenticate(newVisitorId);

            //postData[Constants.VISITOR_ID_API_ITEM] = visitorDelegate.VisitorId;
            //postData[Constants.ANONYMOUS_ID]= visitorDelegate.AnonymousId;


            //await trackingManager.SendActive(visitorDelegate, flag).ConfigureAwait(false);

            //mockHandler.Verify();

            //httpResponse.Dispose();
            //httpClient.Dispose();
        }

        [TestMethod]
        public async Task SendHit()
        {
            //HttpResponseMessage httpResponse = new HttpResponseMessage
            //{
            //    StatusCode = System.Net.HttpStatusCode.OK,
            //    Content = new StringContent("", Encoding.UTF8, "application/json")
            //};

            //Mock<HttpMessageHandler> mockHandler = new Mock<HttpMessageHandler>();

            //mockHandler.Protected().Setup<Task<HttpResponseMessage>>(
            //     "SendAsync",
            //      ItExpr.IsAny<HttpRequestMessage>(),
            //      ItExpr.IsAny<CancellationToken>()
            //    ).ReturnsAsync(httpResponse);


            //var fsLogManagerMock = new Mock<IFsLogManager>();

            //var config = new Flagship.Config.DecisionApiConfig
            //{
            //    LogManager = fsLogManagerMock.Object,
            //};

            //var httpClient = new HttpClient(mockHandler.Object);
            //var trackingManager = new Flagship.Api.TrackingManager(config, httpClient);


            //var hit = new Hit.Screen("Screen")
            //{
            //    Config = config,
            //};

            //await trackingManager.SendHit(hit).ConfigureAwait(false);

            //await trackingManager.SendHit(hit).ConfigureAwait(false);

            //mockHandler.Protected().Verify("SendAsync", Times.Exactly(2), new object[] { ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>() });

            //httpResponse.Dispose();
            //httpClient.Dispose();

        }
    }
}

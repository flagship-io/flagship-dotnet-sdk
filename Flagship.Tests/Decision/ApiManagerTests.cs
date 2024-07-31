using Microsoft.VisualStudio.TestTools.UnitTesting;
using Flagship.Decision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Threading;
using Moq.Protected;
using Moq;
using Flagship.Enums;
using System.Net.Http.Headers;
using System.Collections.ObjectModel;
using Flagship.Logger;

namespace Flagship.Decision.Tests
{
    [TestClass()]
    public class ApiManagerTests
    {
        public string GetCampaignUrl (string envId) 
        {
            return $"{Constants.BASE_API_URL}{envId}/campaigns?exposeAllKeys=true&extras[]=accountSettings";
        }
        private string GetCampaigns()
        {
            return @"{'visitorId':'anonymeId','campaigns':[{'id':'c3ev1afkprbg5u3burag','variation':{'id':'c3mrlpveoqt1lkm7tc00','modifications':{'type':'JSON','value':{'array':[3,3,3],'complex':{'carray':[{'cobject':3}]},'object':{'value':8552}}},'reference':false},'variationGroupId':'c3ev1afkprbg5u3burbg'},{'id':'c2nrh1hjg50l9thhu8bg','variation':{'id':'c2nrh1hjg50l9thhu8dg','modifications':{'type':'JSON','value':{'key':'value'}},'reference':false},'variationGroupId':'c2nrh1hjg50l9thhu8cg'},{'id':'c20j8bk3fk9hdphqtd1g','variation':{'id':'c20j8bk3fk9hdphqtd30','modifications':{'type':'HTML','value':{'my_html':'\u003cdiv\u003e\n \u003cp\u003eoriginal\u003c/ p\u003e\n\u003c/ div\u003e','my_text':null}},'reference':true},'variationGroupId':'c20j8bk3fk9hdphqtd2g'}]}";
        }
        [TestMethod()]
        public async Task GetCampaignsTest()
        {
            var config = new Flagship.Config.DecisionApiConfig()
            {
                EnvId = "envID"
            };
            HttpResponseMessage httpResponse = new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent(GetCampaigns(), Encoding.UTF8, "application/json")
            };

            var url = GetCampaignUrl(config.EnvId);

            Mock<HttpMessageHandler> mockHandler = new Mock<HttpMessageHandler>();

            mockHandler.Protected().Setup<Task<HttpResponseMessage>>(
                 "SendAsync",
                  ItExpr.Is<HttpRequestMessage>(req=> req.Method == HttpMethod.Post && req.RequestUri.ToString()==url ),
                  ItExpr.IsAny<CancellationToken>()
                ).ReturnsAsync(httpResponse);

            var httpClient = new HttpClient(mockHandler.Object);
            var trackingManagerMock = new Mock<Flagship.Api.ITrackingManager>();
            var decisionManagerMock = new Mock<Flagship.Decision.IDecisionManager>();
            var configManager = new Flagship.Config.ConfigManager(config, decisionManagerMock.Object, trackingManagerMock.Object);

            var context = new Dictionary<string, object>();

            var visitorDelegate = new Flagship.FsVisitor.VisitorDelegate("visitorId", false, context, false, configManager);

            var decisionManager = new Flagship.Decision.ApiManager(config, httpClient);

            decisionManager.StatusChange += DecisionManager_StatusChange1;

            Collection<Flagship.Model.Campaign> campaigns = (Collection<Model.Campaign>)await decisionManager.GetCampaigns(visitorDelegate).ConfigureAwait(false);

            Collection<Flagship.Model.FlagDTO> flags  = (Collection<Model.FlagDTO>)await decisionManager.GetFlags(campaigns).ConfigureAwait(false); 

            Assert.AreEqual(campaigns.Count, 3);
            Assert.AreEqual(campaigns[0].Id, "c3ev1afkprbg5u3burag");
            Assert.AreEqual(campaigns[2].Id, "c20j8bk3fk9hdphqtd1g");

            Assert.AreEqual(flags.Count, 6);
            Assert.AreEqual(flags[0].Key, "array");
            Assert.AreEqual(flags[5].Key, "my_text");

            httpClient.Dispose();
            httpResponse.Dispose();
        }

        private void DecisionManager_StatusChange1(FSSdkStatus status)
        {
            Assert.AreEqual(status, FSSdkStatus.SDK_INITIALIZED);
        }

        [TestMethod()]
        public async Task GetCampaignsPanicModeTest()
        {
            var config = new Flagship.Config.DecisionApiConfig()
            {
                EnvId = "envID"
            };
            HttpResponseMessage httpResponse = new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent("{'visitorId':'anonymeId','campaigns':[],'panic':true}", Encoding.UTF8, "application/json")
            };

            var url = GetCampaignUrl(config.EnvId);

            Mock<HttpMessageHandler> mockHandler = new Mock<HttpMessageHandler>();

            mockHandler.Protected().Setup<Task<HttpResponseMessage>>(
                 "SendAsync",
                  ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Post && req.RequestUri.ToString() == url),
                  ItExpr.IsAny<CancellationToken>()
                ).ReturnsAsync(httpResponse);

            var httpClient = new HttpClient(mockHandler.Object);
            var trackingManagerMock = new Mock<Flagship.Api.ITrackingManager>();
            var decisionManagerMock = new Mock<Flagship.Decision.IDecisionManager>();
            var configManager = new Flagship.Config.ConfigManager(config, decisionManagerMock.Object, trackingManagerMock.Object);

            var context = new Dictionary<string, object>();

            var visitorDelegate = new Flagship.FsVisitor.VisitorDelegate("visitorId", false, context, false, configManager);

            var decisionManager = new Flagship.Decision.ApiManager(config, httpClient);

            decisionManager.StatusChange += DecisionManager_StatusChange;

            var campaigns = await decisionManager.GetCampaigns(visitorDelegate).ConfigureAwait(false);

            Assert.AreEqual(campaigns.Count, 0);

            Assert.IsTrue(decisionManager.IsPanic);

            httpClient.Dispose();
            httpResponse.Dispose();
        }

        private void DecisionManager_StatusChange(FSSdkStatus status)
        {
            Assert.AreEqual(status, FSSdkStatus.SDK_PANIC);
        }

        [TestMethod()]
        public async Task GetCampaignsPanicMode2Test()
        {
            var config = new Flagship.Config.DecisionApiConfig()
            {
                EnvId = "envID"
            };
            HttpResponseMessage httpResponse = new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent("{'visitorId':'anonymeId','campaigns':[],'panic':true}", Encoding.UTF8, "application/json")
            };

            var url = GetCampaignUrl(config.EnvId);

            Mock<HttpMessageHandler> mockHandler = new Mock<HttpMessageHandler>();

            mockHandler.Protected().Setup<Task<HttpResponseMessage>>(
                 "SendAsync",
                  ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Post && req.RequestUri.ToString() == url),
                  ItExpr.IsAny<CancellationToken>()
                ).ReturnsAsync(httpResponse);

            var httpClient = new HttpClient(mockHandler.Object);
            var trackingManagerMock = new Mock<Flagship.Api.ITrackingManager>();
            var decisionManagerMock = new Mock<IDecisionManager>();
            var configManager = new Flagship.Config.ConfigManager(config, decisionManagerMock.Object, trackingManagerMock.Object);

            var context = new Dictionary<string, object>();

            var visitorDelegate = new Flagship.FsVisitor.VisitorDelegate("visitorId", false, context, false, configManager);

            var decisionManager = new Flagship.Decision.ApiManager(config, httpClient);


            var campaigns = await decisionManager.GetCampaigns(visitorDelegate).ConfigureAwait(false);

            Assert.AreEqual(campaigns.Count, 0);

            Assert.IsTrue(decisionManager.IsPanic);

            httpClient.Dispose();
            httpResponse.Dispose();
        }

        [TestMethod()]
        public async Task GetCampaignsTestFailTest()
        {
            var fsLogManagerMock = new Mock<IFsLogManager>();


            var config = new Flagship.Config.DecisionApiConfig()
            {
                EnvId = "envID",
                LogManager = fsLogManagerMock.Object,
            };

            var responseContent = "Error";

            HttpResponseMessage httpResponse = new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.BadRequest,
                Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
            };

            var url = GetCampaignUrl(config.EnvId);

            Mock<HttpMessageHandler> mockHandler = new Mock<HttpMessageHandler>();

            mockHandler.Protected().Setup<Task<HttpResponseMessage>>(
                 "SendAsync",
                  ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Post && req.RequestUri.ToString() == url),
                  ItExpr.IsAny<CancellationToken>()
                ).ReturnsAsync(httpResponse);

            var httpClient = new HttpClient(mockHandler.Object);
            var trackingManagerMock = new Mock<Flagship.Api.ITrackingManager>();
            var decisionManagerMock = new Mock<Flagship.Decision.IDecisionManager>();
            var configManager = new Flagship.Config.ConfigManager(config, decisionManagerMock.Object, trackingManagerMock.Object);

            var context = new Dictionary<string, object>();

            var visitorDelegate = new Flagship.FsVisitor.VisitorDelegate("visitorId", false, context, false, configManager);

            var decisionManager = new Flagship.Decision.ApiManager(config, httpClient);

            var campaigns = await decisionManager.GetCampaigns(visitorDelegate).ConfigureAwait(false);

            Assert.AreEqual(campaigns.Count, 0);

            fsLogManagerMock.Verify(x => x.Error("Bad Request", "GetCampaigns"), Times.Once());

            httpClient.Dispose();
            httpResponse.Dispose();
        }
    }
}
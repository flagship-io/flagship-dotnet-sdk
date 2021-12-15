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

namespace Flagship.Decision.Tests
{
    [TestClass()]
    public class ApiManagerTests
    {
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

            var url = $"{Constants.BASE_API_URL}{config.EnvId}/campaigns?exposeAllKeys=true&{Constants.SEND_CONTEXT_EVENT}=false";

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

            var campaigns = await decisionManager.GetCampaigns(visitorDelegate).ConfigureAwait(false);

            Assert.AreEqual(campaigns.Count, 3);

            httpClient.Dispose();
            httpResponse.Dispose();
        }
    }
}
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Flagship.Decision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Moq;
using System.Threading;
using Moq.Protected;
using Flagship.Enums;
using System.IO;
using System.Net.Http.Headers;
using System.Net;

namespace Flagship.Decision.Tests
{
    [TestClass()]
    public class BucketingManagerTests
    {
        public string GetBucketing()
        {
            return File.ReadAllText("bucketing.json");
        }
        [TestMethod()]
        public async Task BucketingManagerTest()
        {

            var config = new Config.BucketingConfig()
            {
                EnvId = "envID",
                ApiKey = "spi",
                PollingInterval = TimeSpan.FromSeconds(0),
            };

            

            HttpResponseMessage httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(GetBucketing(), Encoding.UTF8, "application/json"),
            };

            httpResponse.Headers.Add(HttpResponseHeader.LastModified.ToString(), "2022-01-20");

            var url = string.Format(Constants.BUCKETING_API_URL, config.EnvId);

            Mock<HttpMessageHandler> mockHandler = new Mock<HttpMessageHandler>();

            mockHandler.Protected().Setup<Task<HttpResponseMessage>>(
                 "SendAsync",
                  ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && req.RequestUri.ToString() == url),
                  ItExpr.IsAny<CancellationToken>()
                ).ReturnsAsync(httpResponse);

            var httpClient = new HttpClient(mockHandler.Object);

            var decisionManager = new BucketingManager(config, httpClient, null);

            await decisionManager.StartPolling().ConfigureAwait(false);

            httpResponse.Dispose();
            httpClient.Dispose();

        }

        [TestMethod()]
        public void StartPollingTest()
        {

        }

        [TestMethod()]
        public void PollingTest()
        {

        }

        [TestMethod()]
        public void StopPollingTest()
        {

        }

        [TestMethod()]
        public void SendContextTest()
        {

        }

        [TestMethod()]
        public void GetCampaignsTest()
        {

        }
    }
}
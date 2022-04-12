using Flagship.Model;
using Flagship.Model.Config;
using Flagship.Model.Hits;
using Flagship.Services.ExceptionHandler;
using Flagship.Services.HitSender;
using Flagship.Tests.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Flagship.Tests.Services.HitSender
{
    [TestClass]
    public class SenderTest
    {
        private readonly Sender sender;
        private readonly TestHttpHandler httpHandler;
        private const string env_id = "bk87t3jggr10c6l6sdog";

        public SenderTest()
        {
            httpHandler = new TestHttpHandler();
            var httpClient = new HttpClient(httpHandler);

            sender = new Sender(new FlagshipContext("env_id", "api_key", new FlagshipOptions.Builder()
                .WithErrorHandler(new DefaultExceptionHandler(null, true))
                .Build()));
            var senderClient = sender.GetType().GetField("httpClient", System.Reflection.BindingFlags.NonPublic
    | System.Reflection.BindingFlags.Instance);
            senderClient.SetValue(sender, httpClient);
        }

        [TestCleanup()]
        public void Cleanup()
        {
            httpHandler.ThrowError = false;
        }

        [TestMethod]
        public async Task TestSendHit()
        {
            var transaction = new Transaction()
            {
                Id = "tid",
                Affiliation = "affiliation",
                Revenue = 10
            };

            await sender.Send("vis_id", transaction).ConfigureAwait(true);
            Assert.AreEqual(
                $"{{\"tid\":\"{transaction.Id}\",\"ta\":\"{transaction.Affiliation}\",\"tr\":{transaction.Revenue.Value.ToString("F1", CultureInfo.InvariantCulture)},\"t\":\"TRANSACTION\",\"vid\":\"vis_id\",\"cid\":\"env_id\",\"ds\":\"APP\"}}",
                httpHandler.Content
            );
            Assert.AreEqual(HttpMethod.Post, httpHandler.Method);
            Assert.AreEqual("https://ariane.abtasty.com/", httpHandler.Url);

            httpHandler.ThrowError = true;
            await Assert.ThrowsExceptionAsync<HttpRequestException>(async () =>
            {
                await sender.Send("vis_id", transaction).ConfigureAwait(true);
            }).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task TestActivate()
        {
            //await sender.Activate(new ActivateRequest("env_id", "vis_id", "vg_id", "vid")).ConfigureAwait(true);
            //Assert.AreEqual(
            //    "{\"cid\":\"env_id\",\"vid\":\"vis_id\",\"caid\":\"vg_id\",\"vaid\":\"vid\"}",
            //    httpHandler.Content
            //);
            //Assert.AreEqual(HttpMethod.Post, httpHandler.Method);
            //Assert.AreEqual("https://decision.flagship.io/v2/activate", httpHandler.Url);


            //httpHandler.ThrowError = true;
            //await Assert.ThrowsExceptionAsync<HttpRequestException>(async () =>
            //{
            //    await sender.Activate(new ActivateRequest("env_id", "vis_id", "vg_id", "vid")).ConfigureAwait(true);
            //}).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task TestSendEvent()
        {
            await sender.SendEvent(new EventRequest(env_id, "vid", EventType.CONTEXT, new Dictionary<string, object>
            {
                { "test", "string" }
            })).ConfigureAwait(true);
            Assert.AreEqual(
                $"{{\"client_id\":\"{env_id}\",\"visitor_id\":\"vid\",\"type\":\"CONTEXT\",\"data\":{{\"test\":\"string\"}}}}",
                httpHandler.Content
            );
            Assert.AreEqual(HttpMethod.Post, httpHandler.Method);
            Assert.AreEqual($"https://decision.flagship.io/v2/{env_id}/events", httpHandler.Url);


            httpHandler.ThrowError = true;
            await Assert.ThrowsExceptionAsync<HttpRequestException>(async () =>
            {
                await sender.SendEvent(new EventRequest(env_id, "vid", EventType.CONTEXT, new Dictionary<string, object>
            {
                { "test", "string" }
            })).ConfigureAwait(true);
            }).ConfigureAwait(false);
        }
    }
}

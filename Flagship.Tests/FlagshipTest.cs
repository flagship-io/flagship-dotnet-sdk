using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Flagship.Model.Config;
using Flagship.Model.Decision;
using Flagship.Model.Hits;
using Flagship.Services.ExceptionHandler;
using Flagship.Services.Logger;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Flagship.Tests
{
    [TestClass]
    public class FlagshipTest
    {
        private const string env_id = "bk87t3jggr10c6l6sdog";
        private const string api_key = "api_key";

        [TestMethod]
        public async Task TestInitialization()
        {
            var flagship = FlagshipBuilder.Start(env_id, api_key);
            var visitor = flagship.NewVisitor("123", new Dictionary<string, object>()
            {
                { "CookieTest", true }
            });

            await visitor.SynchronizeModifications().ConfigureAwait(true);

            var test = visitor.GetModification<bool>("not_exist");

            Assert.IsFalse(test);
        }

        [TestMethod]
        public async Task TestBucketing()
        {
            var flagship = FlagshipBuilder.Start(
                env_id,
                api_key,
                new FlagshipOptions.Builder()
                    .WithDecisionMode(Mode.Bucketing)
                    .Build());

            Thread.Sleep(2000);

            var visitor = flagship.NewVisitor("123", new Dictionary<string, object>()
            {
                { "isBetaTester", "yes_it_is" }
            });

            await visitor.SynchronizeModifications().ConfigureAwait(true);

            var test = visitor.GetModification<bool>("drone");

            Assert.IsTrue(test);
        }

        [TestMethod]
        public async Task TestSendHit()
        {
            var flagship = FlagshipBuilder.Start(
                env_id,
                api_key,
                new FlagshipOptions.Builder()
                    .WithDecisionMode(Mode.Bucketing)
                    .WithErrorHandler(new DefaultExceptionHandler(new DefaultLogger(), true))
                    .Build());

            try
            {
                await flagship.SendHit("vis_id", HitType.TRANSACTION, new Event()
                {
                    Action = "action"
                });
                Assert.Fail();
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual("Hit is malformed", e.Message);
            }

            try
            {
                await flagship.SendHit("vis_id", HitType.EVENT, new Event()
                {
                    Action = "action"
                });
                await flagship.SendHit("vis_id", new Event()
                {
                    Action = "action"
                });
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }
    }
}

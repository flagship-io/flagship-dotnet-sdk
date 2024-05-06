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
using Flagship.Delegate;
using Flagship.Model.Bucketing;
using System.Collections.ObjectModel;
using Newtonsoft.Json.Linq;
using Flagship.Model;
using Flagship.Logger;
using Flagship.Hit;

namespace Flagship.Decision.Tests
{
    [TestClass()]
    public class BucketingManagerTests
    {
        public string GetBucketing()
        {
            return File.ReadAllText("bucketing.json");
        }

        public string GetBucketingRealloc()
        {
            return File.ReadAllText("bucketing_realloc.json");
        }

        public string GetBucketingRemovedVariation2() 
        {
            return File.ReadAllText("Bucketing_removed_variation.json");
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

            HttpResponseMessage httpResponsePanicMode = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{ \"panic\": true }", Encoding.UTF8, "application/json"),
            };

            httpResponse.Headers.Add(HttpResponseHeader.LastModified.ToString(), "2022-01-20");

            var url = string.Format(Constants.BUCKETING_API_URL, config.EnvId);

            Mock<HttpMessageHandler> mockHandler = new Mock<HttpMessageHandler>();

            mockHandler.Protected().SetupSequence<Task<HttpResponseMessage>>(
                 "SendAsync",
                  ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && req.RequestUri.ToString() == url),
                  ItExpr.IsAny<CancellationToken>()
                ).ReturnsAsync(httpResponsePanicMode).ReturnsAsync(httpResponse);

            var httpClient = new HttpClient(mockHandler.Object);

            var decisionManager = new BucketingManager(config, httpClient, Murmur.MurmurHash.Create32());

            var decisionManagerPrivate = new PrivateObject(decisionManager);

            var trackingManagerMock = new Mock<Flagship.Api.ITrackingManager>();
            var decisionManagerMock = new Mock<Flagship.Decision.IDecisionManager>();
            var configManager = new Flagship.Config.ConfigManager(config, decisionManagerMock.Object, trackingManagerMock.Object);

            var context = new Dictionary<string, object>()
            {
                ["age"] = 20
            };


            var visitorDelegate = new Flagship.FsVisitor.VisitorDelegate("visitor_1", false, context, false, configManager);

            // test GetCampaigns empty 

            var campaigns = await decisionManager.GetCampaigns(visitorDelegate).ConfigureAwait(false);
            Assert.AreEqual(0, campaigns.Count);

            // test GetCampaigns Panic mode

            await decisionManager.StartPolling().ConfigureAwait(false);
            campaigns = await decisionManager.GetCampaigns(visitorDelegate).ConfigureAwait(false);
            Assert.AreEqual(0, campaigns.Count);
            Assert.IsTrue(decisionManager.IsPanic);

            await decisionManager.StartPolling().ConfigureAwait(false);

            campaigns = await decisionManager.GetCampaigns(visitorDelegate).ConfigureAwait(false);
            var flags = await decisionManager.GetFlags(campaigns).ConfigureAwait(false);

            Assert.AreEqual(6, flags.Count);


            httpResponse.Dispose();
            httpClient.Dispose();
            httpResponsePanicMode.Dispose();

        }

        [TestMethod()]
        public void BucketingMethod()
        {
            var config = new Config.BucketingConfig()
            {
                EnvId = "envID",
                ApiKey = "spi",
                PollingInterval = TimeSpan.FromSeconds(0),
            };

            var httpClient = new HttpClient();

            var decisionManager = new BucketingManager(config, httpClient, Murmur.MurmurHash.Create32());

            var decisionManagerPrivate = new PrivateObject(decisionManager);

            var trackingManagerMock = new Mock<Flagship.Api.ITrackingManager>();
            var decisionManagerMock = new Mock<Flagship.Decision.IDecisionManager>();
            var configManager = new Flagship.Config.ConfigManager(config, decisionManagerMock.Object, trackingManagerMock.Object);

            var context = new Dictionary<string, object>()
            {
                ["age"] = 20
            };

            var visitorId = "123456";

            var visitorDelegate = new Flagship.FsVisitor.VisitorDelegate(visitorId, false, context, false, configManager);

            //test getVariation
            var variations = new Collection<Model.Bucketing.Variation>
            {
                new Model.Bucketing.Variation
                {
                    Id = "c20j8bk3fk9hdphqtd30",
                    Modifications = new Modifications{
                        Type= Model.ModificationType.HTML,
                        Value = new Dictionary<string, object>
                        {
                            ["my_html"] = "<div>\n  <p>Original</p>\n</div>"
                        }
                    },
                    Allocation = 34,
                    Reference = true
                },
                new Model.Bucketing.Variation
                {
                    Id = "c20j8bk3fk9hdphqtd3g",
                    Modifications = new Modifications{
                        Type= Model.ModificationType.HTML,
                        Value = new Dictionary<string, object>
                        {
                            ["my_html"] = "<div>\n  <p>variation 1</p>\n</div>"
                        }
                    },
                    Allocation = 33,
                },
                new Model.Bucketing.Variation
                {
                    Id = "c20j9lgbcahhf2mvhbf0",
                    Modifications = new Modifications{
                        Type= Model.ModificationType.HTML,
                        Value = new Dictionary<string, object>
                        {
                            ["my_html"] = "<div>\n  <p>variation 2</p>\n</div>"
                        }
                    },
                    Allocation = 33,
                }
            };

            var VariationGroup = new VariationGroup
            {
                Id = "9273BKSDJtoto",
                Variations = variations
            };
            var variationResult = (Model.Variation)decisionManagerPrivate.Invoke("GetVariation", new object[] { VariationGroup, visitorDelegate });

            Assert.IsNotNull(variationResult);

            Assert.AreEqual(variations[0].Id, variationResult.Id);

            // Test null variation

            variationResult = (Model.Variation)decisionManagerPrivate.Invoke("GetVariation", new object[] { null, visitorDelegate });

            Assert.IsNull(variationResult);

            VariationGroup = new VariationGroup
            {
                Id = "9273BKSDJtoto",
                Variations = null
            };
            variationResult = (Model.Variation)decisionManagerPrivate.Invoke("GetVariation", new object[] { VariationGroup, visitorDelegate });

            Assert.IsNull(variationResult);


            // test isMatchTargeting with empty VariationGroupDTO

            var IsMatchedTargeting = (bool)decisionManagerPrivate.Invoke("IsMatchedTargeting", new object[] { new VariationGroup(), visitorDelegate });

            Assert.IsFalse(IsMatchedTargeting);

            VariationGroup = new VariationGroup
            {
                Targeting = new TargetingContainer
                {
                    TargetingGroups = new List<TargetingGroup>
                    {
                        new TargetingGroup
                        {
                            Targetings = new Collection<Targeting>
                            {
                                new Targeting
                                {
                                    Key = "age",
                                    Operator = TargetingOperator.EQUALS,
                                    Value = 21
                                }
                            }
                        }
                    }
                }
            };

            IsMatchedTargeting = (bool)decisionManagerPrivate.Invoke("IsMatchedTargeting", new object[] { VariationGroup, visitorDelegate });

            Assert.IsFalse(IsMatchedTargeting);

            var TargetingGroups = VariationGroup.Targeting.TargetingGroups.Append(new TargetingGroup
            {
                Targetings = new Collection<Targeting>
                            {
                                new Targeting
                                {
                                    Key = "fs_all_users",
                                    Operator = TargetingOperator.EQUALS,
                                    Value = ""
                                }
                            }
            }).Append(new TargetingGroup
            {
                Targetings = new Collection<Targeting>
                            {
                                new Targeting
                                {
                                    Key = "fs_users",
                                    Operator = TargetingOperator.EQUALS,
                                    Value = visitorId
                                }
                            }
            });

            VariationGroup.Targeting.TargetingGroups = TargetingGroups;


            IsMatchedTargeting = (bool)decisionManagerPrivate.Invoke("IsMatchedTargeting", new object[] { VariationGroup, visitorDelegate });

            Assert.IsTrue(IsMatchedTargeting);

            //test CheckAndTargeting

            var CheckAndTargeting = (bool)decisionManagerPrivate.Invoke("CheckAndTargeting", new object[] { new Collection<Targeting>(), visitorDelegate });

            Assert.IsFalse(CheckAndTargeting);

            // test checkAndTargeting fs_all_users

            var targetings = new Collection<Targeting>
                            {
                                new Targeting
                                {
                                    Key = "fs_all_users",
                                    Operator = TargetingOperator.EQUALS,
                                    Value = ""
                                }
                            };

            CheckAndTargeting = (bool)decisionManagerPrivate.Invoke("CheckAndTargeting", new object[] { targetings, visitorDelegate });

            Assert.IsTrue(CheckAndTargeting);

            // test checkAndTargeting fs_users

            targetings = new Collection<Targeting>
                            {
                                new Targeting
                                {
                                    Key = "fs_users",
                                    Operator = TargetingOperator.STARTS_WITH,
                                    Value = "12"
                                },
                                new Targeting
                                {
                                    Key = "fs_users",
                                    Operator = TargetingOperator.ENDS_WITH,
                                    Value = "6"
                                }
                            };

            CheckAndTargeting = (bool)decisionManagerPrivate.Invoke("CheckAndTargeting", new object[] { targetings, visitorDelegate });

            Assert.IsTrue(CheckAndTargeting);

            // test checkAndTargeting fs_users targeting and

            targetings = new Collection<Targeting>
                            {
                                new Targeting
                                {
                                    Key = "fs_users",
                                    Operator = TargetingOperator.STARTS_WITH,
                                    Value = "2"
                                },
                                new Targeting
                                {
                                    Key = "fs_users",
                                    Operator = TargetingOperator.ENDS_WITH,
                                    Value = "6"
                                }
                            };

            CheckAndTargeting = (bool)decisionManagerPrivate.Invoke("CheckAndTargeting", new object[] { targetings, visitorDelegate });

            Assert.IsFalse(CheckAndTargeting);

            // test checkAndTargeting key not match any context

            targetings = new Collection<Targeting>
                            {
                                new Targeting
                                {
                                    Key = "anyKey",
                                    Operator = TargetingOperator.EQUALS,
                                    Value = "anyValue"
                                }
                            };

            CheckAndTargeting = (bool)decisionManagerPrivate.Invoke("CheckAndTargeting", new object[] { targetings, visitorDelegate });

            Assert.IsFalse(CheckAndTargeting);

            // test checkAndTargeting key match context

            targetings = new Collection<Targeting>
                            {
                                new Targeting
                                {
                                    Key = "age",
                                    Operator = TargetingOperator.EQUALS,
                                    Value = 20
                                }
                            };

            CheckAndTargeting = (bool)decisionManagerPrivate.Invoke("CheckAndTargeting", new object[] { targetings, visitorDelegate });

            Assert.IsTrue(CheckAndTargeting);

            // test testOperator EQUALS Test different values

            var testOperator = (bool)decisionManagerPrivate.Invoke("TestOperator", new object[] { TargetingOperator.EQUALS, 5, 6 });

            Assert.IsFalse(testOperator);

            // test testOperator EQUALS Test different type

            testOperator = (bool)decisionManagerPrivate.Invoke("TestOperator", new object[] { TargetingOperator.EQUALS, 5, "5" });

            Assert.IsFalse(testOperator);

            testOperator = (bool)decisionManagerPrivate.Invoke("TestOperator", new object[] { TargetingOperator.EQUALS, true, "5" });

            Assert.IsFalse(testOperator);

            testOperator = (bool)decisionManagerPrivate.Invoke("TestOperator", new object[] { TargetingOperator.EQUALS, true, false });

            Assert.IsFalse(testOperator);

            // test testOperator EQUALS Test same type and value

            testOperator = (bool)decisionManagerPrivate.Invoke("TestOperator", new object[] { TargetingOperator.EQUALS, true, true });

            Assert.IsTrue(testOperator);

            testOperator = (bool)decisionManagerPrivate.Invoke("TestOperator", new object[] { TargetingOperator.EQUALS, "abc", "abc" });

            Assert.IsTrue(testOperator);

            testOperator = (bool)decisionManagerPrivate.Invoke("TestOperator", new object[] { TargetingOperator.EQUALS, 1, 1 });

            Assert.IsTrue(testOperator);

            // test testOperator EQUALS Test contextValue EQUALS targetingValue list

            testOperator = (bool)decisionManagerPrivate.Invoke("TestOperator", new object[] { TargetingOperator.EQUALS, "a", new JArray { "a","b","c" } });

            Assert.IsTrue(testOperator);

            testOperator = (bool)decisionManagerPrivate.Invoke("TestOperator", new object[] { TargetingOperator.EQUALS, 2d, new JArray { 2d, 1d, 3d } });

            Assert.IsTrue(testOperator);

            testOperator = (bool)decisionManagerPrivate.Invoke("TestOperator", new object[] { TargetingOperator.EQUALS, "a", new JArray { "b", "c" } });

            Assert.IsFalse(testOperator);

            // test testOperator NOT_EQUALS Test different values


            testOperator = (bool)decisionManagerPrivate.Invoke("TestOperator", new object[] { TargetingOperator.NOT_EQUALS, 5, 6 });

            Assert.IsTrue(testOperator);

            // test testOperator NOT_EQUALS Test different type

            testOperator = (bool)decisionManagerPrivate.Invoke("TestOperator", new object[] { TargetingOperator.NOT_EQUALS, 5, '5' });

            Assert.IsTrue(testOperator);

            // test testOperator NOT_EQUALS Test same type and value

            testOperator = (bool)decisionManagerPrivate.Invoke("TestOperator", new object[] { TargetingOperator.NOT_EQUALS, 5, 5 });

            Assert.IsFalse(testOperator);

            // test testOperator NOT_EQUALS Test contextValue NOT_EQUALS targetingValue list


            testOperator = (bool)decisionManagerPrivate.Invoke("TestOperator", new object[] { TargetingOperator.NOT_EQUALS, "a", new JArray { "b", "c", "d" } });

            Assert.IsTrue(testOperator);

            // test testOperator NOT_EQUALS Test contextValue NOT_EQUALS targetingValue list

            testOperator = (bool)decisionManagerPrivate.Invoke("TestOperator", new object[] { TargetingOperator.NOT_EQUALS, "a", new JArray { "a", "b", "c" } });

            Assert.IsFalse(testOperator);


            // test testOperator CONTAINS Test contextValue not contains targetingValue

            testOperator = (bool)decisionManagerPrivate.Invoke("TestOperator", new object[] { TargetingOperator.CONTAINS, "a", "b" });

            Assert.IsFalse(testOperator);

            // test testOperator CONTAINS Test contextValue contains targetingValue

            testOperator = (bool)decisionManagerPrivate.Invoke("TestOperator", new object[] { TargetingOperator.CONTAINS, "abc", "b" });

            Assert.IsTrue(testOperator);

            testOperator = (bool)decisionManagerPrivate.Invoke("TestOperator", new object[] { TargetingOperator.CONTAINS, 123, 2 });

            Assert.IsTrue(testOperator);

            testOperator = (bool)decisionManagerPrivate.Invoke("TestOperator", new object[] { TargetingOperator.CONTAINS, 123, "2" });

            Assert.IsTrue(testOperator);

            // test testOperator CONTAINS Test contextValue contains targetingValue

            testOperator = (bool)decisionManagerPrivate.Invoke("TestOperator", new object[] { TargetingOperator.CONTAINS, "nopq_hij", new JArray() { "abc", "dfg", "hij", "klm" } });

            Assert.IsTrue(testOperator);

            // test testOperator CONTAINS Test contextValue CONTAINS targetingValue list

            testOperator = (bool)decisionManagerPrivate.Invoke("TestOperator", new object[] { TargetingOperator.CONTAINS, "abcd", new JArray() { "a", "b", "c" } });

            Assert.IsTrue(testOperator);

            // test testOperator CONTAINS Test contextValue not CONTAINS targetingValue list

            testOperator = (bool)decisionManagerPrivate.Invoke("TestOperator", new object[] { TargetingOperator.CONTAINS, "abcd", new JArray { "e", "f" } });

            Assert.IsFalse(testOperator);

            // test testOperator NOT_CONTAINS Test contextValue not contains targetingValue

            testOperator = (bool)decisionManagerPrivate.Invoke("TestOperator", new object[] { TargetingOperator.NOT_CONTAINS, "abc", "d" });

            Assert.IsTrue(testOperator);

            // test testOperator NOT_CONTAINS Test contextValue contains targetingValue

            testOperator = (bool)decisionManagerPrivate.Invoke("TestOperator", new object[] { TargetingOperator.NOT_CONTAINS, "abc", "b" });

            Assert.IsFalse(testOperator);

            // test testOperator NOT_CONTAINS Test contextValue NOT_CONTAINS targetingValue list

            testOperator = (bool)decisionManagerPrivate.Invoke("TestOperator", new object[] { TargetingOperator.NOT_CONTAINS, "abcd", new JArray { "e", "f" } });

            Assert.IsTrue(testOperator);

            // test testOperator NOT_CONTAINS Test contextValue not NOT_CONTAINS targetingValue list

            testOperator = (bool)decisionManagerPrivate.Invoke("TestOperator", new object[] { TargetingOperator.NOT_CONTAINS, "abcd", new JArray { "a", "e" } });

            Assert.IsFalse(testOperator);

            // test testOperator GREATER_THAN Test contextValue not GREATER_THAN targetingValue

            testOperator = (bool)decisionManagerPrivate.Invoke("TestOperator", new object[] { TargetingOperator.GREATER_THAN, 5, 6 });

            Assert.IsFalse(testOperator);

            // test testOperator GREATER_THAN Test contextValue not GREATER_THAN targetingValue

            testOperator = (bool)decisionManagerPrivate.Invoke("TestOperator", new object[] { TargetingOperator.GREATER_THAN, 5, 5 });

            Assert.IsFalse(testOperator);

            // test testOperator GREATER_THAN Test contextValue not GREATER_THAN targetingValue

            testOperator = (bool)decisionManagerPrivate.Invoke("TestOperator", new object[] { TargetingOperator.GREATER_THAN, "a", "b" });

            Assert.IsFalse(testOperator);

            // test testOperator GREATER_THAN Test contextValue not GREATER_THAN targetingValue


            testOperator = (bool)decisionManagerPrivate.Invoke("TestOperator", new object[] { TargetingOperator.GREATER_THAN, "abz", "bcg" });

            Assert.IsFalse(testOperator);

            // test testOperator GREATER_THAN Test contextValue GREATER_THAN targetingValue


            testOperator = (bool)decisionManagerPrivate.Invoke("TestOperator", new object[] { TargetingOperator.GREATER_THAN, 8, 5 });

            Assert.IsTrue(testOperator);


            // test testOperator GREATER_THAN Test contextValue GREATER_THAN targetingValue


            testOperator = (bool)decisionManagerPrivate.Invoke("TestOperator", new object[] { TargetingOperator.GREATER_THAN, "9dlk", "8" });

            Assert.IsTrue(testOperator);

            // test testOperator LOWER_THAN Test contextValue LOWER_THAN targetingValue

            testOperator = (bool)decisionManagerPrivate.Invoke("TestOperator", new object[] { TargetingOperator.LOWER_THAN, 5, 6 });

            Assert.IsTrue(testOperator);

            // test testOperator LOWER_THAN Test contextValue not GREATER_THAN targetingValue

            testOperator = (bool)decisionManagerPrivate.Invoke("TestOperator", new object[] { TargetingOperator.LOWER_THAN, 5, 5 });

            Assert.IsFalse(testOperator);

            // test testOperator LOWER_THAN Test contextValue LOWER_THAN targetingValue

            testOperator = (bool)decisionManagerPrivate.Invoke("TestOperator", new object[] { TargetingOperator.LOWER_THAN, "a", "b" });

            Assert.IsTrue(testOperator);

            // test testOperator LOWER_THAN Test contextValue LOWER_THAN targetingValue

            testOperator = (bool)decisionManagerPrivate.Invoke("TestOperator", new object[] { TargetingOperator.LOWER_THAN, "abz", "bcg" });

            Assert.IsTrue(testOperator);

            // test testOperator LOWER_THAN Test contextValue not LOWER_THAN targetingValue

            testOperator = (bool)decisionManagerPrivate.Invoke("TestOperator", new object[] { TargetingOperator.LOWER_THAN, 8, 2 });

            Assert.IsFalse(testOperator);

            // test testOperator GREATER_THAN_OR_EQUALS Test contextValue GREATER_THAN targetingValue

            testOperator = (bool)decisionManagerPrivate.Invoke("TestOperator", new object[] { TargetingOperator.GREATER_THAN_OR_EQUALS, 8, 2 });

            Assert.IsTrue(testOperator);

            // test testOperator GREATER_THAN_OR_EQUALS Test contextValue EQUALS targetingValue

            testOperator = (bool)decisionManagerPrivate.Invoke("TestOperator", new object[] { TargetingOperator.GREATER_THAN_OR_EQUALS, 8, 8 });

            Assert.IsTrue(testOperator);

            // test testOperator GREATER_THAN_OR_EQUALS Test contextValue LOWER_THAN targetingValue

            testOperator = (bool)decisionManagerPrivate.Invoke("TestOperator", new object[] { TargetingOperator.GREATER_THAN_OR_EQUALS, 7, 8 });

            Assert.IsFalse(testOperator);

            // test testOperator GREATER_THAN_OR_EQUALS Test contextValue LOWER_THAN targetingValue

            testOperator = (bool)decisionManagerPrivate.Invoke("TestOperator", new object[] { TargetingOperator.GREATER_THAN_OR_EQUALS, "a", "b" });

            Assert.IsFalse(testOperator);

            // test testOperator LOWER_THAN_OR_EQUALS Test contextValue GREATER_THAN targetingValue

            testOperator = (bool)decisionManagerPrivate.Invoke("TestOperator", new object[] { TargetingOperator.LOWER_THAN_OR_EQUALS, 8, 6 });

            Assert.IsFalse(testOperator);

            // test testOperator LOWER_THAN_OR_EQUALS Test contextValue EQUALS targetingValue

            testOperator = (bool)decisionManagerPrivate.Invoke("TestOperator", new object[] { TargetingOperator.LOWER_THAN_OR_EQUALS, 8, 8 });

            Assert.IsTrue(testOperator);

            // test testOperator LOWER_THAN_OR_EQUALS Test contextValue LOWER_THAN targetingValue

            testOperator = (bool)decisionManagerPrivate.Invoke("TestOperator", new object[] { TargetingOperator.LOWER_THAN_OR_EQUALS, 7, 8 });

            Assert.IsTrue(testOperator);

            // test testOperator LOWER_THAN_OR_EQUALS Test contextValue LOWER_THAN targetingValue

            testOperator = (bool)decisionManagerPrivate.Invoke("TestOperator", new object[] { TargetingOperator.LOWER_THAN_OR_EQUALS, "a", "b" });

            Assert.IsTrue(testOperator);

            //test testOperator STARTS_WITH Test contextValue STARTS_WITH targetingValue

            testOperator = (bool)decisionManagerPrivate.Invoke("TestOperator", new object[] { TargetingOperator.STARTS_WITH, "abcd", "ab" });

            Assert.IsTrue(testOperator);

            // test testOperator STARTS_WITH Test contextValue STARTS_WITH targetingValue

            testOperator = (bool)decisionManagerPrivate.Invoke("TestOperator", new object[] { TargetingOperator.STARTS_WITH, "abcd", "AB" });

            Assert.IsFalse(testOperator);

            // test testOperator STARTS_WITH Test contextValue STARTS_WITH targetingValue

            testOperator = (bool)decisionManagerPrivate.Invoke("TestOperator", new object[] { TargetingOperator.STARTS_WITH, "abcd", "ac" });

            Assert.IsFalse(testOperator);

            // test testOperator ENDS_WITH Test contextValue ENDS_WITH targetingValue

            testOperator = (bool)decisionManagerPrivate.Invoke("TestOperator", new object[] { TargetingOperator.ENDS_WITH, "abcd", "cd" });

            Assert.IsTrue(testOperator);

            // test testOperator ENDS_WITH Test contextValue ENDS_WITH targetingValue

            testOperator = (bool)decisionManagerPrivate.Invoke("TestOperator", new object[] { TargetingOperator.ENDS_WITH, "abcd", "CD" });

            Assert.IsFalse(testOperator);

            // test testOperator ENDS_WITH Test contextValue ENDS_WITH targetingValue

            testOperator = (bool)decisionManagerPrivate.Invoke("TestOperator", new object[] { TargetingOperator.ENDS_WITH, "abcd", "bd" });

            Assert.IsFalse(testOperator);

            httpClient.Dispose();
        }

        [TestMethod()]
        public async Task PollingTest()
        {
            var fsLogManagerMock = new Mock<IFsLogManager>();

            var config = new Config.BucketingConfig()
            {
                EnvId = "envID",
                ApiKey = "spi",
                PollingInterval = TimeSpan.FromSeconds(0),
                LogManager = fsLogManagerMock.Object,
            };

            HttpResponseMessage httpResponsePanicMode = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{ \"panic\": true }", Encoding.UTF8, "application/json"),
            };

            httpResponsePanicMode.Headers.Add(HttpResponseHeader.LastModified.ToString(), "2022-01-20");

            var url = string.Format(Constants.BUCKETING_API_URL, config.EnvId);

            Mock<HttpMessageHandler> mockHandler = new Mock<HttpMessageHandler>();

            var exception = new Exception("Test");

            mockHandler.Protected().SetupSequence<Task<HttpResponseMessage>>(
                 "SendAsync",
                  ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && req.RequestUri.ToString() == url),
                  ItExpr.IsAny<CancellationToken>()
                ).ReturnsAsync(httpResponsePanicMode).Throws(exception);

            var httpClient = new HttpClient(mockHandler.Object);

            var decisionManagerMock = new Mock<BucketingManager>(config, httpClient, null)
            {
                CallBase = true
            };

            decisionManagerMock.Setup(x => x.SendContextAsync(It.IsAny<FsVisitor.VisitorDelegateAbstract>())) ;

            var decisionManager = decisionManagerMock.Object;

            var countStatus = 0;

            void DecisionManager_StatusChange(FlagshipStatus status)
            {
                if (countStatus == 0)
                {
                    countStatus++;
                    Assert.AreEqual(FlagshipStatus.POLLING, status);
                }
                else
                {
                    Assert.AreEqual(FlagshipStatus.READY, status);
                }
            }

            decisionManager.StatusChange += DecisionManager_StatusChange;

            var trackingManagerMock = new Mock<Flagship.Api.ITrackingManager>();

            var trackingManager = trackingManagerMock.Object;

            decisionManager.TrackingManager = trackingManager;

            await Task.WhenAll(new Task[]
            {
                decisionManager.StartPolling(),
                decisionManager.StartPolling()
            }).ConfigureAwait(false);

            var configManager = new Flagship.Config.ConfigManager(config, decisionManager, trackingManager);

            var context = new Dictionary<string, object>()
            {
                ["age"] = 20
            };

            var visitorDelegate = new Flagship.FsVisitor.VisitorDelegate("visitor_1", false, context, false, configManager);

            var campaigns = await decisionManager.GetCampaigns(visitorDelegate).ConfigureAwait(false);

            Assert.IsTrue(decisionManager.IsPanic);

            await decisionManager.StartPolling().ConfigureAwait(false);

            fsLogManagerMock.Verify(x => x.Error(exception.Message, "Polling"), Times.Once());

            httpResponsePanicMode.Dispose();
            httpClient.Dispose();
        }


        [TestMethod()]
        public async Task StopPollingTest()
        {
            var fsLogManagerMock = new Mock<IFsLogManager>();

            var config = new Config.BucketingConfig()
            {
                EnvId = "envID",
                ApiKey = "spi",
                PollingInterval = TimeSpan.FromMilliseconds(500),
                LogManager = fsLogManagerMock.Object,
            };

            HttpResponseMessage httpResponsePanicMode = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{ \"panic\": true }", Encoding.UTF8, "application/json"),
            };

            httpResponsePanicMode.Headers.Add(HttpResponseHeader.LastModified.ToString(), "2022-01-20");

            var url = string.Format(Constants.BUCKETING_API_URL, config.EnvId);

            Mock<HttpMessageHandler> mockHandler = new Mock<HttpMessageHandler>();


            mockHandler.Protected().SetupSequence<Task<HttpResponseMessage>>(
                 "SendAsync",
                  ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && req.RequestUri.ToString() == url),
                  ItExpr.IsAny<CancellationToken>()
                ).ReturnsAsync(httpResponsePanicMode);

            var httpClient = new HttpClient(mockHandler.Object);

            var decisionManagerMock = new Mock<BucketingManager>(config, httpClient, null)
            {
                CallBase = true
            };

            decisionManagerMock.Setup(x => x.SendContextAsync(It.IsAny<FsVisitor.VisitorDelegateAbstract>()))
                ;

            var decisionManager = decisionManagerMock.Object;

            _ = decisionManager.StartPolling().ConfigureAwait(false);

            await Task.Delay(1000).ConfigureAwait(false);

            decisionManager.StopPolling();

            mockHandler.Protected().Verify(
                 "SendAsync",
                 Times.AtLeast(2),
                  ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && req.RequestUri.ToString() == url),
                  ItExpr.IsAny<CancellationToken>()
                );

            httpResponsePanicMode.Dispose();
            httpClient.Dispose();
        }

        [TestMethod()]
        public async Task SendContextTest()
        {
            var fsLogManagerMock = new Mock<IFsLogManager>();

            var config = new Config.BucketingConfig()
            {
                EnvId = "envID",
                ApiKey = "spi",
                PollingInterval = TimeSpan.FromSeconds(0),
                LogManager = fsLogManagerMock.Object,
            };

            var httpClient = new HttpClient();

            var decisionManagerMock = new BucketingManager(config, httpClient, null);

            var trackingManagerMock = new Mock<Api.ITrackingManager>();
            var trackingManager = trackingManagerMock.Object;

            decisionManagerMock.TrackingManager = trackingManager;

            var configManager = new Config.ConfigManager(config, decisionManagerMock, trackingManager);

            var context = new Dictionary<string, object>()
            {
                ["age"] = 20
            };

            var visitorDelegateMock = new Mock<FsVisitor.VisitorDelegate>("visitor_1", false, context, false, configManager, null)
            {
                CallBase = true
            };

            visitorDelegateMock.Setup(x => x.SendHit(It.Is<Segment>(y => y.Context["age"] == context["age"])));

            var visitorDelegate = visitorDelegateMock.Object;

            await decisionManagerMock.SendContextAsync(visitorDelegate);

            visitorDelegate.SetConsent(true);

            await decisionManagerMock.SendContextAsync(visitorDelegate);

            visitorDelegateMock.Verify(x => x.SendHit(It.Is<Segment>(y => y.Context["age"] == context["age"])), Times.Once());

            var exception = new Exception("sendHit error");

            visitorDelegateMock.Setup(x => x.SendHit(It.Is<Segment>(y => y.Context["age"] == context["age"]))).Throws(exception);

            await decisionManagerMock.SendContextAsync(visitorDelegateMock.Object);

            fsLogManagerMock.Verify(x => x.Error(exception.Message, "SendContext"), Times.Once());

            httpClient.Dispose();
        }

        [TestMethod()]
        public async Task GetCampaignsTest()
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
                Content = new StringContent(GetBucketingRealloc(), Encoding.UTF8, "application/json"),
            };

            HttpResponseMessage httpResponse2 = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(GetBucketingRemovedVariation2(), Encoding.UTF8, "application/json"),
            };

            httpResponse.Headers.Add(HttpResponseHeader.LastModified.ToString(), "2022-01-20");

            var url = string.Format(Constants.BUCKETING_API_URL, config.EnvId);

            Mock<HttpMessageHandler> mockHandler = new Mock<HttpMessageHandler>();

            mockHandler.Protected().SetupSequence<Task<HttpResponseMessage>>(
                 "SendAsync",
                  ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && req.RequestUri.ToString() == url),
                  ItExpr.IsAny<CancellationToken>()
                ).ReturnsAsync(httpResponse).ReturnsAsync(httpResponse).ReturnsAsync(httpResponse2);

            var httpClient = new HttpClient(mockHandler.Object);

            var murmurHash = Murmur.MurmurHash.Create32();
            var decisionManager = new BucketingManager(config, httpClient, murmurHash);

            var decisionManagerPrivate = new PrivateObject(decisionManager);

            var trackingManagerMock = new Mock<Flagship.Api.ITrackingManager>();
            var decisionManagerMock = new Mock<Flagship.Decision.IDecisionManager>();
            var configManager = new Flagship.Config.ConfigManager(config, decisionManagerMock.Object, trackingManagerMock.Object);

            var context = new Dictionary<string, object>()
            {
                ["age"] = 20
            };


            var visitorDelegate = new Flagship.FsVisitor.VisitorDelegate("visitor_1", false, context, false, configManager);


            await decisionManager.StartPolling().ConfigureAwait(false);

            var campaigns = await decisionManager.GetCampaigns(visitorDelegate).ConfigureAwait(false);
            var flags = await decisionManager.GetFlags(campaigns).ConfigureAwait(false);

            Assert.AreEqual(6, flags.Count);

            Assert.IsNotNull(flags.FirstOrDefault(x => x.VariationId == "c20j9lgbcahhf2mvhbf0"));

            visitorDelegate.VisitorCache = new VisitorCache
            {
                Version = 1,
                Data = new VisitorCacheDTOV1
                {
                    Version = 1,
                    Data = new VisitorCacheData
                    {
                        VisitorId = "visitor_1",
                        Consent = true,
                        AssignmentsHistory = new Dictionary<string, string>
                        {
                            ["c20j8bk3fk9hdphqtd2g"] = "c20j8bk3fk9hdphqtd3g"
                        }
                    }
                }
            };


            await decisionManager.StartPolling().ConfigureAwait(false);

            campaigns = await decisionManager.GetCampaigns(visitorDelegate).ConfigureAwait(false);
            flags = await decisionManager.GetFlags(campaigns).ConfigureAwait(false);

            Assert.AreEqual(6, flags.Count);

            // Test realloc
            Assert.IsNull(flags.FirstOrDefault(x => x.VariationId == "c20j9lgbcahhf2mvhbf0"));

            Assert.IsNotNull(flags.FirstOrDefault(x => x.VariationId == "c20j8bk3fk9hdphqtd3g"));



            await decisionManager.StartPolling().ConfigureAwait(false);

            campaigns = await decisionManager.GetCampaigns(visitorDelegate).ConfigureAwait(false);
            flags = await decisionManager.GetFlags(campaigns).ConfigureAwait(false);

            Assert.AreEqual(5, flags.Count);

            // Test realloc
            Assert.IsNull(flags.FirstOrDefault(x => x.VariationId == "c20j9lgbcahhf2mvhbf0"));

            Assert.IsNull(flags.FirstOrDefault(x => x.VariationId == "c20j8bk3fk9hdphqtd3g"));

            httpResponse.Dispose();
            httpClient.Dispose();
            httpResponse2.Dispose();
            murmurHash.Dispose();
        }
    }
}
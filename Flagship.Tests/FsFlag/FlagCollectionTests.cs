using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Flagship.FsFlag;
using Flagship.FsVisitor;
using Flagship.Logger;
using Flagship.Model;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Flagship.Api;
using Flagship.Config;
using Newtonsoft.Json.Linq;

namespace Flagship.Tests.FsFlag;

[TestClass]
public class FlagCollectionTests
{
    private Mock<VisitorDelegateAbstract> _mockVisitor;
    private ICollection<FlagDTO> _flagsDTO;

    private string key1 = "key1";
    private string key2 = "key2";

    private string value1 = "value1";

    private string value2 = "value2";


    [TestInitialize]
    public void Setup()
    {
        var config = new DecisionApiConfig()
        {
            EnvId = "envID"
        };
        var trackingManagerMock = new Mock<ITrackingManager>();
        var decisionManagerMock = new Mock<Decision.IDecisionManager>();
        var configManager = new ConfigManager(config, decisionManagerMock.Object, trackingManagerMock.Object);

        var context = new Dictionary<string, object>();
        _mockVisitor = new Mock<VisitorDelegateAbstract>(["visitorId", false, context, false, configManager, null]);

        _mockVisitor.Setup(x => x.GetStrategy()).CallBase();


        _flagsDTO =
        [
            new() { Key = "key1",
            CampaignId = "campaignId1",
            VariationGroupId = "variationGroupId1",
            VariationId = "variationId1",
            CampaignName = "campaignName1",
            VariationGroupName = "variationGroupName1",
            VariationName = "variationName1",
            Slug = "slug1",
            Value = "value1",
            CampaignType = "ab"
             },
            new() { Key = "key2", CampaignId = "campaignId2",
            VariationGroupId = "variationGroupId2",
            VariationId = "variationId2",
            CampaignName = "campaignName2", 
            VariationGroupName = "variationGroupName2",
            VariationName = "variationName2",
            Slug = "slug2",
            Value = "value2",
            CampaignType = "ab"
            }
        ];
        _mockVisitor.SetupGet(v => v.Flags).Returns(_flagsDTO);
    }

    [TestMethod]
    public void Constructor_WithNullVisitorAndFlags_InitializesEmpty()
    {
        var flagCollection = new FlagCollection();
        Assert.AreEqual(0, flagCollection.Size);
    }

    [TestMethod]
    public void Constructor_WithVisitorAndNoFlags_PopulatesFromVisitor()
    {
        _mockVisitor.Setup(v => v.Flags).Returns(_flagsDTO);
        var flagCollection = new FlagCollection(_mockVisitor.Object);
        Assert.AreEqual(_flagsDTO.Count, flagCollection.Size);
    }

    [TestMethod]
    public void Constructor_WithVisitorAndFlags_UsesProvidedFlags()
    {
        var flags = new Dictionary<string, IFlag>
        {
            { "key1", new Flag("key1", _mockVisitor.Object) },
        };
        var flagCollection = new FlagCollection(_mockVisitor.Object, flags);
        Assert.AreEqual(1, flagCollection.Size);
    }

    [TestMethod]
    public void Get_ExistingKey_ReturnsFlag()
    {

        var flagCollection = new FlagCollection(_mockVisitor.Object, null);
        var flag = flagCollection.Get("key1");
        Assert.IsNotNull(flag);
        Assert.IsTrue(flag.Exists);
    }

    [TestMethod]
    public void Get_NonExistingKey_ReturnsNewFlagAndLogsWarning()
    {
        var flagCollection = new FlagCollection(_mockVisitor.Object);
        var flag = flagCollection.Get("nonExistingKey");
        Assert.IsNotNull(flag);
        // Assuming Log.LogWarning is static and cannot be directly verified in this context.
    }

    [TestMethod]
    public void Has_Key_ReturnsCorrectBoolean()
    {
        _mockVisitor.Object.Flags = _flagsDTO;
        var flagCollection = new FlagCollection(_mockVisitor.Object, null);
        Assert.IsTrue(flagCollection.Has("key1"));
        Assert.IsFalse(flagCollection.Has("nonExistingKey"));
    }

    [TestMethod]
    public void Keys_ReturnsAllKeys()
    {
        _mockVisitor.Object.Flags = _flagsDTO;
        var flagCollection = new FlagCollection(_mockVisitor.Object, null);
        var keys = flagCollection.Keys();
        Assert.AreEqual(_flagsDTO.Count, keys.Count);
        Assert.IsTrue(keys.SetEquals(["key1", "key2"]));
    }

    [TestMethod]
    public void Filter_ReturnsFilteredCollection()
    {
        _mockVisitor.Object.Flags = _flagsDTO;
        var flagCollection = new FlagCollection(_mockVisitor.Object, null);
        var filtered = flagCollection.Filter((flag, key, collection) => key == "key1");
        Assert.AreEqual(1, filtered.Size);
    }

    [TestMethod]
    public async Task ExposeAllAsync_CallsVisitorExposedOnAllFlags()
    {
        var flag1 = _flagsDTO.First(x => x.Key == "key1");
        var flag2 = _flagsDTO.First(x => x.Key == "key2");
        _mockVisitor.Setup(v => v.VisitorExposed<object?>("key1", null, flag1, false)).Returns(Task.CompletedTask);
        _mockVisitor.Setup(v => v.VisitorExposed<object?>("key2", null, flag2, false)).Returns(Task.CompletedTask);

        var flagCollection = new FlagCollection(_mockVisitor.Object, null);
        await flagCollection.ExposeAllAsync();
        _mockVisitor.Verify(v => v.VisitorExposed<object?>("key1", null, flag1, false), Times.Once);
        _mockVisitor.Verify(v => v.VisitorExposed<object?>("key2", null, flag2, false), Times.Once);
    }

    [TestMethod]
    public void GetMetadata_ReturnsMetadataForAllFlags()
    {
        var flagCollection = new FlagCollection(_mockVisitor.Object, null);
        var metadata = flagCollection.GetMetadata();
        Assert.AreEqual(_flagsDTO.Count, metadata.Count);
    }

    [TestMethod]
    public void ToJson_SerializesMetadataCorrectly()
    {
        var flag1 = _flagsDTO.First(x => x.Key == "key1");
        _mockVisitor.Setup(x => x.GetFlagMetadata("key1", flag1)).Returns(new FlagMetadata(flag1.CampaignId, flag1.VariationGroupId, flag1.VariationId, true, flag1.CampaignName, flag1.VariationGroupName, flag1.VariationName, flag1.VariationGroupName, flag1.VariationName));
        var flag2 = _flagsDTO.First(x => x.Key == "key2");
        _mockVisitor.Setup(x => x.GetFlagMetadata("key2", flag2)).Returns(new FlagMetadata(flag2.CampaignId, flag2.VariationGroupId, flag2.VariationId, true, flag2.CampaignName, flag2.VariationGroupName, flag2.VariationName, flag2.VariationGroupName, flag2.VariationName));

        _mockVisitor.Setup(x=> x.GetFlagValue<object?>(key1, null, flag1, false)).Returns(flag1.Value);
        _mockVisitor.Setup(x => x.GetFlagValue<object?>(key2, null, flag2, false)).Returns(flag2.Value);


        var flagCollection = new FlagCollection(_mockVisitor.Object, null);
        var json = flagCollection.ToJson();

        Assert.IsNotNull(json);
        string jsonData = "[{\"key\":\"key1\",\"campaignId\":\"campaignId1\",\"campaignName\":\"variationName1\",\"variationGroupId\":\"variationGroupId1\",\"variationGroupName\":\"variationGroupName1\",\"variationId\":\"variationId1\",\"variationName\":\"variationName1\",\"isReference\":true,\"campaignType\":\"campaignName1\",\"slug\":\"variationGroupName1\",\"hex\":\"7b2276223a2276616c756531227d\"},{\"key\":\"key2\",\"campaignId\":\"campaignId2\",\"campaignName\":\"variationName2\",\"variationGroupId\":\"variationGroupId2\",\"variationGroupName\":\"variationGroupName2\",\"variationId\":\"variationId2\",\"variationName\":\"variationName2\",\"isReference\":true,\"campaignType\":\"campaignName2\",\"slug\":\"variationGroupName2\",\"hex\":\"7b2276223a2276616c756532227d\"}]";

        var resultJsonToken = JToken.Parse(json);
        var expectedJsonToken = JToken.Parse(jsonData);

        Assert.IsTrue(JToken.DeepEquals(expectedJsonToken, resultJsonToken));

        _mockVisitor.Verify(x => x.GetFlagMetadata("key1", flag1), Times.Once);
        _mockVisitor.Verify(x => x.GetFlagMetadata("key2", flag2), Times.Once);

        _mockVisitor.Verify(x => x.GetFlagValue<object?>(key1, null, flag1, false), Times.Once);
        _mockVisitor.Verify(x => x.GetFlagValue<object?>(key2, null, flag2, false), Times.Once);
    }
}

using Flagship.Model;
using System.Collections.Generic;

namespace Flagship
{
    public class Visitor
    {
        public readonly string Id;
        public IDictionary<string, object> Context { get; set; }
        public string DecisionGroup { get; set; }
        public readonly IDictionary<string, FlagInfo> FlagInfos;

        public bool IsPanic { get; set; }

        public Visitor(string id, IDictionary<string, object> context, string decisionGroup)
        {
            Id = id;
            Context = context;
            DecisionGroup = decisionGroup;
            FlagInfos = new Dictionary<string, FlagInfo>();
        }

        public void SetFlagInfos(DecisionResponse response)
        {
            IsPanic = response.Panic;
            foreach (var campaign in response.Campaigns)
            {
                foreach (var keyValue in campaign.Variation.Modifications.Value)
                {
                    if (!FlagInfos.ContainsKey(keyValue.Key))
                    {
                        FlagInfos.Add(keyValue.Key, new FlagInfo
                        {
                            CampaignId = campaign.Id,
                            VariationGroupId = campaign.VariationGroupId,
                            VariationId = campaign.Variation.Id,
                            Reference = campaign.Variation.Reference,
                            Value = keyValue.Value
                        });
                    }
                }
            }
        }
    }
}

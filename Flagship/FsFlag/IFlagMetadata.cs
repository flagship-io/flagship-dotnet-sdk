using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flagship.FsFlag
{
    public interface IFlagMetadata 
    {
        string CampaignId { get; }
        string CampaignName { get; } 
        string VariationGroupId { get; }
        string VariationGroupName { get; }
        string VariationName { get; }
        string VariationId { get; }
        bool IsReference { get; }
        string CampaignType { get; }
        string Slug { get; }
        string ToJson();
    }
}

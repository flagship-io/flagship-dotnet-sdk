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
        string VariationGroupId { get; }
        string VariationId { get; }
        bool IsReference { get; }
        string CampaignType { get; }
        string ToJson();
    }
}

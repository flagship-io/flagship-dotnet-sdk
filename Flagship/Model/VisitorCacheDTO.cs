using System.Collections.Generic;

namespace Flagship.Model
{
    internal class VisitorCacheCampaign
    {
        public string CampaignId { get; set; }

        public string VariationGroupId { get; set; }

        public string VariationId { get; set; }

        public bool? IsReference { get; set; }

        public ModificationType Type { get; set; }

        public string Slug { get; set; }

        public bool? Activated { get; set; }

        public IDictionary<string, object> Flags { get; set; }
    }

    internal class VisitorCacheData
    {
        public string VisitorId { get; set; }

        public string AnonymousId { get; set; }

        public bool? Consent { get; set; }

        public IDictionary<string, object> Context { get; set; }

        public ICollection<VisitorCacheCampaign> Campaigns { get; set; }

        public IDictionary<string, string> AssignmentsHistory { get; set; }
    }

    internal class VisitorCacheDTOV1
    {
        public int Version { get; set; }

        public VisitorCacheData Data { get; set; }
    }
}

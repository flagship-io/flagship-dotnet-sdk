namespace Flagship.Model
{
    public class FlagDTO
    {
        public string Key { get; set; }
        public string CampaignId { get; set; }

        public string CampaignName { get; set; }

        public string VariationGroupId { get; set; }

        public string VariationGroupName { get; set; }

        public string VariationId { get; set; }

        public string VariationName { get; set; }

        public bool IsReference { get; set; }

        public object Value { get; set; }

        public string CampaignType { get; set; }
        public string Slug { get; set; }
    }
}

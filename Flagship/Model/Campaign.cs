namespace Flagship.Model
{
    public class Campaign
    {
        /// <summary>
        /// The campaign ID
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The variation group ID (scenario)
        /// </summary>
        public string VariationGroupId { get; set; }

        /// <summary>
        /// The variation assigned for the visitor
        /// </summary>
        public Variation Variation { get; set; }
            
        public string CampaignType { get; set; }

    }
}
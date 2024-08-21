namespace Flagship.Model
{
    public class Campaign
    {
        /// <summary>
        /// The campaign ID
        /// </summary>
        public string Id { get; set; }

        public string Name { get; set; }

        public string Slug { get; set; }

        /// <summary>
        /// The variation group ID (scenario)
        /// </summary>
        public string VariationGroupId { get; set; }

        public string VariationGroupName { get; set; }

        /// <summary>
        /// The variation assigned for the visitor
        /// </summary>
        public Variation Variation { get; set; }

        public string Type { get; set; }
    }
}

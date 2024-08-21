using System.Collections.Generic;

namespace Flagship.Model.Bucketing
{
    public class VariationGroup
    {
        public string Id { get; set; }

        public string Name { get; set; }
        public TargetingContainer Targeting { get; set; }
        public IEnumerable<Variation> Variations { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Flagship.Model.Bucketing
{
    public class VariationGroup
    {
        public string Id { get; set; }
        public TargetingContainer Targeting { get; set; }
        public IEnumerable<Variation> Variations { get; set; }
    }
}

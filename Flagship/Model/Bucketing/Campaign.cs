﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Flagship.Model.Bucketing
{
    public class Campaign
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public string Slug { get; set; }
        public IEnumerable<VariationGroup> VariationGroups { get; set; }
    }
}

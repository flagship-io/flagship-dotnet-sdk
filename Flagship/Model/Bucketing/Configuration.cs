using System;
using System.Collections.Generic;
using System.Text;

namespace Flagship.Model.Bucketing
{
    public class Configuration
    {
        public bool Panic { get; set; }
        public IEnumerable<Campaign> Campaigns { get; set; }
    }
}

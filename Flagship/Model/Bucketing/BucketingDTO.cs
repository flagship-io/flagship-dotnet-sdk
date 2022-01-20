using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flagship.Model.Bucketing
{
    internal class BucketingDTO
    {
        public bool? Panic { get; set; }
        public IEnumerable<Campaign> Campaigns { get; set; }
    }
}

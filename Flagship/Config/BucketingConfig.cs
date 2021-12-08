using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flagship.Enum;

namespace Flagship.Config
{
    public class BucketingConfig : FlagshipConfig
    {
        public int? PollingInterval { get; set; }
        public BucketingConfig():base()
        {
            DecisionMode = DecisionMode.BUCKETING;
            if (!PollingInterval.HasValue)
            {
                PollingInterval = Constants.DEFAULT_POLLING_INTERVAL;
            }
        }
    }
}

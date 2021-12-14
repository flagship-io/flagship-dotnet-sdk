using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flagship.Enums;

namespace Flagship.Config
{
    public class BucketingConfig : FlagshipConfig
    {
        public TimeSpan? PollingInterval { get; set; }
        public BucketingConfig():base()
        {
            DecisionMode = DecisionMode.BUCKETING;
            if (!PollingInterval.HasValue)
            {
                PollingInterval = TimeSpan.FromMilliseconds(Constants.DEFAULT_POLLING_INTERVAL);
            }
        }
    }
}

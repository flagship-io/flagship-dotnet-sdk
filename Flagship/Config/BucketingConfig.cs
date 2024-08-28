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
        /// <summary>
        /// Specify delay between two bucketing polling when SDK is running on Bucketing mode.
        /// Note: If 0 is given, it should poll only once at start time.
        /// </summary>
        public TimeSpan? PollingInterval { get; set; }

        public bool FetchThirdPartyData { get; set; }
        public BucketingConfig() : base(DecisionMode.BUCKETING)
        {
            if (!PollingInterval.HasValue)
            {
                PollingInterval = TimeSpan.FromMilliseconds(Constants.DEFAULT_POLLING_INTERVAL);
            }
        }
    }
}

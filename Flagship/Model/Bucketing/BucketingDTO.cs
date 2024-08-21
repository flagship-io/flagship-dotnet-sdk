using System.Collections.Generic;

namespace Flagship.Model.Bucketing
{
   internal class AccountSettings
    {
        public bool? EnabledXPC { get; set; }
        public TroubleshootingData Troubleshooting { get; set; }
    }
    internal class BucketingDTO
    {
        public bool? Panic { get; set; }
        public IEnumerable<Campaign> Campaigns { get; set; }
        public AccountSettings  AccountSettings { get; set; }
    }
}

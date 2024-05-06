using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Flagship.Model
{
    public class Extras
    {
        public AccountSettings AccountSettings { get; set; }
    }

    public class AccountSettings
    {
        public TroubleshootingData Troubleshooting { get; set; }
    }

    public class DecisionResponse
    {
        /// <summary>
        /// The visitor ID
        /// </summary>
        public string VisitorID { get; set; }

        /// <summary>
        /// The visitor assigned campaigns
        /// </summary>
        public ICollection<Campaign> Campaigns { get; set; }

        /// <summary>
        /// Is the environment in panic mode
        /// </summary>
        public bool Panic { get; set; }

        public Extras Extras { get; set; }

        public DecisionResponse()
        {
            Campaigns = new Collection<Campaign>();
        }
    }
}

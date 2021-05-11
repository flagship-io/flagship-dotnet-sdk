using System;
using System.Collections.Generic;
using System.Text;

namespace Flagship.Model
{
    public class ModificationInfo
    {
        /// <summary>
        /// The campaign ID
        /// </summary>
        public string CampaignID { get; set; }

        /// <summary>
        /// The variation group ID
        /// </summary>
        public string VariationGroupID { get; set; }

        /// <summary>
        /// The variation ID
        /// </summary>
        public string VariationID { get; set; }

        /// <summary>
        /// Is the reference variation
        /// </summary>
        public bool IsReference { get; set; }
    }
}

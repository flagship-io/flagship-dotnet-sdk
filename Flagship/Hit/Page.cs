using Flagship.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flagship.Hit
{

    /// <summary>
    /// This hit should be sent each time a visitor arrives on a new page.
    /// </summary>
    public class Page:HitAbstract
    {
        /// <summary>
        /// Valid url
        /// </summary>
        public string DocumentLocation { get; set; }

        /// <summary>
        /// This hit should be sent each time a visitor arrives on a new page.
        /// </summary>
        /// <param name="documentLocation">Valid url</param>
        public Page(string documentLocation):base(HitType.PAGEVIEW)
        {
            DocumentLocation = documentLocation;
        }

        internal override IDictionary<string, object> ToApiKeys()
        {
            var apiKeys = base.ToApiKeys();
            apiKeys[Constants.DL_API_ITEM] = DocumentLocation;
            return apiKeys;
        }

        internal override bool IsReady(bool checkParent = true)
        {
            return (!checkParent || base.IsReady()) && !string.IsNullOrWhiteSpace(DocumentLocation);
        }

        internal override string GetErrorMessage()
        {
            return Constants.HIT_PAGE_ERROR_MESSAGE;
        }

    }
}

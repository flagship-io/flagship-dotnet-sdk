using Flagship.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flagship.Hit
{
    public class Page:HitAbstract
    {
        public string DocumentLocation { get; set; }

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

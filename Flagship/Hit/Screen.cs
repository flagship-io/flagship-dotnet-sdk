using Flagship.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flagship.Hit
{
    public class Screen:HitAbstract
    {
        public string DocumentLocation { get; set; }

        public Screen(string documentLocation):base(HitType.SCREENVIEW)
        {
            DocumentLocation = documentLocation;
        }

        internal override IDictionary<string, object> ToApiKeys()
        {
            var apiKeys= base.ToApiKeys();
            apiKeys[Constants.DL_API_ITEM] = DocumentLocation;
            return apiKeys;
        }

        internal override bool IsReady()
        {
            return base.IsReady() && !string.IsNullOrWhiteSpace(DocumentLocation);
        }

        internal override string GetErrorMessage()
        {
            return Constants.HIT_SCREEN_ERROR_MESSAGE;
        }
    }
}

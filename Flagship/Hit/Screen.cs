using System.Collections.Generic;
using Flagship.Enums;

namespace Flagship.Hit
{
    /// <summary>
    /// This hit should be sent each time a visitor arrives on an interface on client side.
    /// </summary>
    public class Screen : HitAbstract
    {
        /// <summary>
        /// Screen name
        /// </summary>
        public string DocumentLocation { get; set; }

        /// <summary>
        /// This hit should be sent each time a visitor arrives on an interface on client side.
        /// </summary>
        /// <param name="documentLocation">Screen name</param>
        public Screen(string documentLocation)
            : base(HitType.SCREENVIEW)
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
            return Constants.HIT_SCREEN_ERROR_MESSAGE;
        }
    }
}

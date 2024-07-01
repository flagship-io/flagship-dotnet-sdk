using Flagship.Enums;
using Flagship.FsFlag;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flagship.Hit
{
    internal class Activate : HitAbstract
    {
        public const string ERROR_MESSAGE = "variationGroupId and variationId are required";
        public string VariationGroupId { get; set; }
        public string VariationId { get; set; }

        public string FlagKey { get; set; } 

        public object FlagValue { get; set; }

        public object FlagDefaultValue { get; set; }

        public IFSFlagMetadata FlagMetadata { get; set; }

        public IDictionary<string, object> VisitorContext { get; set; } 

        public Activate(string variationGroupId, string variationId):base(HitType.ACTIVATE)
        {
            VariationGroupId = variationGroupId;
            VariationId = variationId;
        }

        internal override bool IsReady(bool checkParent = true)
        {
            return base.IsReady(checkParent) && !string.IsNullOrWhiteSpace(VariationGroupId) && !string.IsNullOrWhiteSpace(VariationId);
        }

        internal override IDictionary<string, object> ToApiKeys()
        {
            var apiKeys = new Dictionary<string, object>()
            {
                [Constants.VISITOR_ID_API_ITEM] = VisitorId,
                [Constants.VARIATION_ID_API_ITEM] = VariationId,
                [Constants.VARIATION_GROUP_ID_API_ITEM_ACTIVATE] = VariationGroupId,
                [Constants.CUSTOMER_ENV_ID_API_ACTIVATE] = Config.EnvId,
                [Constants.ANONYMOUS_ID] = null,
                [Constants.QT_API_ITEM] = (CurrentDateTime - CreatedAt).Milliseconds
            };

            if (!string.IsNullOrWhiteSpace(VisitorId) && !string.IsNullOrWhiteSpace(AnonymousId))
            {
                apiKeys[Constants.VISITOR_ID_API_ITEM] = VisitorId;
                apiKeys[Constants.ANONYMOUS_ID] = AnonymousId;
            }

            return apiKeys;
        }

        internal override string GetErrorMessage()
        {
            return ERROR_MESSAGE;
        }
    }
}

using Flagship.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flagship.Hit
{
    internal class Batch : HitAbstract
    {
        public const string ERROR_MESSAGE = "Please check required fields";

        [JsonConverter(typeof(BatchConverter))]
        public ICollection<HitAbstract> Hits { get; set; } 
        public Batch() : base(HitType.BATCH)
        {
        }

        internal override bool IsReady(bool checkParent = true)
        {
            return base.IsReady() && Hits!=null && Hits.Count> 0 && Hits.All(hit=> hit.IsReady(false));
        }

        internal override IDictionary<string, object> ToApiKeys()
        {
            var apiKeys = base.ToApiKeys();

            var apiKeysHits = new Collection<IDictionary<string, object>>();

            foreach (var hit in Hits)
            {
                var hitKeys = hit.ToApiKeys();

                hitKeys.Remove(Constants.VISITOR_ID_API_ITEM);
                hitKeys.Remove(Constants.CUSTOMER_ENV_ID_API_ITEM);
                hitKeys.Remove(Constants.USER_IP_API_ITEM);
                hitKeys.Remove(Constants.SCREEN_RESOLUTION_API_ITEM);
                hitKeys.Remove(Constants.USER_LANGUAGE);
                hitKeys.Remove(Constants.SESSION_NUMBER);
                hitKeys.Remove(Constants.CUSTOMER_UID);
                hitKeys.Remove(Constants.DS_API_ITEM);

                apiKeysHits.Add(hitKeys);
            }

            apiKeys["h"] = apiKeysHits;

            return apiKeys;
        }

        internal override string GetErrorMessage()
        {
            return ERROR_MESSAGE;
        }
    } 
}

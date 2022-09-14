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

            var apiKeys = new Dictionary<string, object>()
            {
                [Constants.DS_API_ITEM] = DS,
                [Constants.CUSTOMER_ENV_ID_API_ITEM] = Config?.EnvId,
                [Constants.T_API_ITEM] = $"{Type}",
                [Constants.QT_API_ITEM] = (DateTime.Now - CreatedAt).Milliseconds
            };

            var apiKeysHits = new Collection<IDictionary<string, object>>();

            foreach (var hit in Hits)
            {
                var hitKeys = hit.ToApiKeys();

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

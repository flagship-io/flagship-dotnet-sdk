using Flagship.Config;
using Flagship.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flagship.Hit
{
    internal class ActivateBatch
    {
        public ICollection<Activate> Hits { get; set; }

        public FlagshipConfig Config { get; set; }

        public ActivateBatch(ICollection<Activate> hits, FlagshipConfig config)
        {
            Hits = hits;
            Config = config;
        }

        public IDictionary<string, object> ToApiKeys()
        {
            return new Dictionary<string, object>()
            {
                {Constants.CUSTOMER_ENV_ID_API_ITEM, Config?.EnvId },
                {Constants.BATCH, Hits.Select(x=> x.ToApiKeys()) }
            };
        }

    }
}

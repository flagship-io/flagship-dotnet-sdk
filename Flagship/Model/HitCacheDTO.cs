using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flagship.Model
{

    internal enum HitCacheType
    {
        PAGEVIEW = Hits.HitType.PAGEVIEW,
        SCREENVIEW = Hit.HitType.SCREENVIEW,
        EVENT = Hit.HitType.EVENT,
        TRANSACTION = Hit.HitType.TRANSACTION,
        ITEM = Hit.HitType.ITEM,
        BATCH = 5,
        ACTIVATE = 6,
    }
    internal class HitCacheData
    {
        public string VisitorId { get; set; }

        public string AnonymousId { get; set; }

        public HitCacheType Type { get; set; }

        public DateTime Time { get; set; }

        public object Content { get; set; }
    }
    internal class HitCacheDTOV1
    {
        public int Version { get; set; }

        public HitCacheData Data { get; set; } 

    }
}

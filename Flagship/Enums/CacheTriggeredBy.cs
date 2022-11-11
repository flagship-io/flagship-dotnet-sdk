using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flagship.Enums
{
    internal enum CacheTriggeredBy
    {
        Timer,
        BatchLength,
        Flush,
        ActivateLength,
        DirectHit,
        DirectActivate
    }
}

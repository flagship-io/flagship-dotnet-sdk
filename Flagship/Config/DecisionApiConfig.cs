using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flagship.Config
{
    public class DecisionApiConfig : FlagshipConfig
    {
        public DecisionApiConfig() : base(Enums.DecisionMode.DECISION_API)
        {
        }
    }
}

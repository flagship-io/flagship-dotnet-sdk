using Flagship.Delegate;
using Flagship.Enum;
using Flagship.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flagship.Config
{
    public abstract class FlagshipConfig 
    {
        public string EnvId { get; internal set; }
        public string ApiKey { get; internal set; }

        public DecisionMode DecisionMode {  get; protected set; }

        public int? Timeout { get; set; }

        public LogLevel LogLevel { get; set; }
         
        public event StatusChangeDelegate StatusChange;

        public IFsLogManager LogManager { get; set; }

        public FlagshipConfig()
        {
            if (!Timeout.HasValue)
            {
                Timeout = Constants.REQUEST_TIME_OUT;
            }
        }

    }
}

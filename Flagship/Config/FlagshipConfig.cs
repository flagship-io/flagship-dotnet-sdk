using Flagship.Delegate;
using Flagship.Enums;
using Flagship.FsVisitor;
using Flagship.Hit;
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

        public TimeSpan? Timeout { get; set; }

        public LogLevel LogLevel { get; set; }
         
        public event StatusChangeDelegate StatusChange;

        internal void SetStatus(FlagshipStatus status)
        {
            StatusChange?.Invoke(status);
        }

        public IFsLogManager LogManager { get; set; }

        public IVisitorCacheImplementation VisitorCacheImplementation { get; set; }

        public IHitCacheImplementation HitCacheImplementation { get; set; }

        public bool DisableCache { get; set; } 
        
        public FlagshipConfig()
        {
            LogLevel = LogLevel.ALL;
            if (!Timeout.HasValue)
            {
                Timeout = TimeSpan.FromMilliseconds(Constants.REQUEST_TIME_OUT);
            }

        }

    }
}

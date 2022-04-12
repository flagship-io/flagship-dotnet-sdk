using Flagship.Cache;
using Flagship.Delegate;
using Flagship.Enums;
using Flagship.Logger;
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

        /// <summary>
        /// Specify timeout for api request.
        /// Note: timeout can't be lower than 0 second.
        /// </summary>
        public TimeSpan? Timeout { get; set; }

        /// <summary>
        /// Set the maximum log level to display
        /// </summary>
        public LogLevel LogLevel { get; set; }

        /// <summary>
        /// Define a callable in order to get callback when the SDK status has changed.
        /// </summary>
        public event StatusChangeDelegate StatusChanged;

        internal void SetStatus(FlagshipStatus status)
        {
            StatusChanged?.Invoke(status);
        }

        /// <summary>
        /// Specify a custom implementation of LogManager in order to receive logs from the SDK.
        /// Note: The object must fill
        /// </summary>
        public IFsLogManager LogManager { get; set; }

        /// <summary>
        /// Define an object that implement the interface IVisitorCacheImplementation, to handle the visitor cache.
        /// </summary>
        public IVisitorCacheImplementation VisitorCacheImplementation { get; set; }

        /// <summary>
        /// Define an object that implement the interface IHitCacheImplementation, to handle the visitor cache.
        /// </summary>
        public IHitCacheImplementation HitCacheImplementation { get; set; }

        /// <summary>
        /// If it's set to true, hit cache and visitor cache will be disabled otherwise will be enabled.
        /// </summary>
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

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
using Flagship.FsFlag;

namespace Flagship.Config
{
    public abstract class FlagshipConfig
    {
        private ITrackingManagerConfig trackingManagerConfig;

        public string EnvId { get; internal set; }
        public string ApiKey { get; internal set; }

        public DecisionMode DecisionMode { get; protected set; }

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
        /// 
        /// </summary>
        public event OnVisitorExposedDelegate OnVisitorExposed;

        virtual internal void InvokeOnVisitorExposed(IExposedVisitor exposedVisitor, IExposedFlag exposedFlag)
        {
            OnVisitorExposed?.Invoke(exposedVisitor, exposedFlag);
        }

        virtual internal bool HasOnVisitorExposed()
        {
            return OnVisitorExposed != null;
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

        /// <summary>
        /// Define options to configure hit batching
        /// </summary>
        /// 
        [Obsolete("Please use TrackingManagerConfig instead ")]
        public ITrackingManagerConfig TrackingMangerConfig { get => trackingManagerConfig; set => trackingManagerConfig = value; }

        /// <summary>
        /// Define options to configure hit batching
        /// </summary>
        public ITrackingManagerConfig TrackingManagerConfig { get => trackingManagerConfig; set => trackingManagerConfig = value; }

        /// <summary>
        /// The SDK will collect usage data to help us improve our product
        /// <br/> If set true no usage data will be collected
        /// </summary>
        public bool DisableDeveloperUsageTracking { get; set; }

        public FlagshipConfig(DecisionMode decisionMode = DecisionMode.DECISION_API)
        {
            DecisionMode = decisionMode;
            LogLevel = LogLevel.ALL;
            if (!Timeout.HasValue)
            {
                Timeout = TimeSpan.FromMilliseconds(Constants.REQUEST_TIME_OUT);
            }
            DisableDeveloperUsageTracking = false;
        }

    }
}

using System;
using Flagship.Enums;

namespace Flagship.Hit
{
    internal class Diagnostic : HitAbstract
    {
        public string Version { get; set; }

        public LogLevel LogLevel { get; set; }

        public string EnvId { get; set; }

        public string Timestamp { get; set; }

        public string TimeZone { get; set; }

        public string Label { get; set; }

        public string LastInitializationTimestamp { get; set; }

        public string LastBucketingTimestamp { get; set; }


        public string StackType { get; set; }
        public string StackName { get; set; }
        public string StackVersion { get; set; }
        public string StackOriginName { get; set; }
        public string StackOriginVersion { get; set; }

        public FlagshipStatus? SdkStatus { get; set; }
        public string SdkConfigMode { get; set; }
        public TimeSpan? SdkConfigTimeout { get; set; }
        public TimeSpan? SdkConfigPollingInterval { get; set; }
        public Model.Bucketing.BucketingDTO SdkConfigInitialBucketing { get; set; }
        public CacheStrategy SdkConfigTrackingManagerConfigStrategy { get; set; }
        public int? SdkConfigTrackingManagerConfigBatchIntervals { get; set; }
        public int? SdkConfigTrackingManagerConfigPoolMaxSize { get; set; }
        public bool? SdkConfigUsingCustomHitCache { get; set; }
        public bool? SdkConfigUsingCustomVisitorCache { get; set; }
        public bool? SdkConfigUsingOnVisitorExposed { get; set; }





        public Diagnostic(HitType type) : base(type)
        {
        }

        internal override string GetErrorMessage()
        {
            throw new NotImplementedException();
        }
    }
}


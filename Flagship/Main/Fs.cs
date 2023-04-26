using Flagship.Api;
using Flagship.Config;
using Flagship.Decision;
using Flagship.Delegate;
using Flagship.Enums;
using Flagship.FsVisitor;
using Flagship.Logger;
using Flagship.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("Flagship.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
namespace Flagship.Main
{
    public class Fs
    {
        private static Fs instance;

        private FlagshipStatus _status = FlagshipStatus.NOT_INITIALIZED;

        private FlagshipConfig _config;

        private IConfigManager _configManager;
        private Visitor _visitor;

        protected static Fs GetInstance()
        {
            if (instance == null)
            {
                instance = new Fs();
            }
            return instance;
        }

        private Fs()
        {

        }

        /// <summary>
        /// Return current status of Flagship SDK.
        /// </summary>
        public static FlagshipStatus Status => GetInstance()._status;

        /// <summary>
        /// Return the current config used by the SDK.
        /// </summary>
        public static FlagshipConfig Config => GetInstance()._config;

        private void SetStatus(FlagshipStatus status)
        {
            if (_status == status)
            {
                return;
            }

            if (status == FlagshipStatus.READY)
            {
                _configManager?.TrackingManager?.StartBatchingLoop();
            }
            else
            {
                _configManager?.TrackingManager?.StopBatchingLoop();
            }
            _status = status;
            _config.SetStatus(status);
        }

        private static bool IsReady()
        {
            return GetInstance()._status == FlagshipStatus.READY;
        }

        /// <summary>
        /// Will return any previous created visitor instance initialized with the SINGLE_INSTANCE
        /// </summary>
        public static FsVisitor.Visitor Visitor
        {
            get { return GetInstance()._visitor; }
            internal set { GetInstance()._visitor = value; }
        }

        /// <summary>
        /// Start the Flagship SDK.
        /// </summary>
        /// <param name="envId">Environment id provided by Flagship.</param>
        /// <param name="apiKey">Api authentication key provided by Flagship.</param>
        /// <param name="config">Custom flagship configuration.</param>
        public static void Start(string envId, string apiKey, FlagshipConfig config = null)
        {
#if NET45
ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls | SecurityProtocolType.Ssl3;
#elif NET40
ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
#endif

            if (config == null)
            {
                config = new DecisionApiConfig();
            }
            if (config.TrackingMangerConfig == null)
            {
                config.TrackingMangerConfig = new TrackingManagerConfig();
            }
            var fsInstance = GetInstance();

            fsInstance._config = config;

            if (config.LogManager == null)
            {
                config.LogManager = new FsLogManager();
            }

            if (string.IsNullOrWhiteSpace(envId) || string.IsNullOrWhiteSpace(apiKey))
            {
                fsInstance.SetStatus(FlagshipStatus.NOT_INITIALIZED);
                Log.LogError(config, Constants.INITIALIZATION_PARAM_ERROR, Constants.PROCESS_INITIALIZATION);
                return;
            }

            config.EnvId = envId;
            config.ApiKey = apiKey;

            fsInstance.SetStatus(FlagshipStatus.STARTING);
            var httpClient = new HttpClient()
            {
                Timeout = config.Timeout ?? TimeSpan.FromMilliseconds(Constants.REQUEST_TIME_OUT)
            };

            IDecisionManager decisionManager = fsInstance._configManager?.DecisionManager;

            if (decisionManager != null && decisionManager is BucketingManager bucketingManager)
            {
                bucketingManager.StopPolling();
            }

            if (config.DecisionMode == DecisionMode.BUCKETING)
            {
                decisionManager = new BucketingManager((BucketingConfig)config, httpClient, Murmur.MurmurHash.Create32());
                _ = ((BucketingManager)decisionManager).StartPolling();
            }
            else
            {
                decisionManager = new ApiManager(config, httpClient);
            }

            var trackingManager = new TrackingManager(config, httpClient);

            decisionManager.StatusChange += DecisionManager_StatusChange;

            if (fsInstance._configManager == null)
            {
                fsInstance._configManager = new ConfigManager(config, decisionManager, trackingManager);
            }
            else
            {
                fsInstance._configManager.DecisionManager = decisionManager;
                fsInstance._configManager.TrackingManager = trackingManager;
            }

            fsInstance.SetStatus(FlagshipStatus.READY);
            Log.LogInfo(config, string.Format(Constants.SDK_STARTED_INFO, Constants.SDK_VERSION),
                Constants.PROCESS_INITIALIZATION);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns> 
        public static async Task Close() 
        {
            await instance?._configManager?.TrackingManager?.SendBatch(CacheTriggeredBy.Flush);
        }
        private static void DecisionManager_StatusChange(FlagshipStatus status)
        {
            GetInstance().SetStatus(status);
        }

        /// <summary>
        /// Initialize the builder and Return a VisitorBuilder or null if the SDK hasn't started successfully.
        /// </summary>
        /// <param name="visitorId"></param>
        /// <returns>VisitorBuilder | null</returns>
        public static VisitorBuilder NewVisitor(string visitorId)
        {
            return NewVisitor(visitorId, InstanceType.NEW_INSTANCE);
        }

        /// <summary>
        /// Initialize the builder and Return a VisitorBuilder or null if the SDK hasn't started successfully.
        /// </summary>
        /// <returns></returns>
        public static VisitorBuilder NewVisitor()
        {
            return NewVisitor(null, InstanceType.NEW_INSTANCE);
        }

        /// <summary>
        /// Initialize the builder and Return a VisitorBuilder or null if the SDK hasn't started successfully.
        /// </summary>
        /// <param name="visitorId"></param>
        /// <param name="instanceType"></param>
        /// <returns></returns>
        public static VisitorBuilder NewVisitor(string visitorId, InstanceType instanceType)
        {
            if (!IsReady())
            {
                return null;
            }
            return VisitorBuilder.Builder(GetInstance()._configManager, visitorId, instanceType);
        }

        /// <summary>
        /// Initialize the builder and Return a VisitorBuilder or null if the SDK hasn't started successfully.
        /// </summary>
        /// <param name="instanceType"></param>
        /// <returns></returns>
        public static VisitorBuilder NewVisitor(InstanceType instanceType)
        {
            return NewVisitor(null, instanceType);
        }
    }
}

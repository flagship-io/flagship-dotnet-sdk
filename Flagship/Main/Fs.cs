using Flagship.Api;
using Flagship.Config;
using Flagship.Decision;
using Flagship.Delegate;
using Flagship.Enums;
using Flagship.FsVisitor;
using Flagship.Logger;
using Flagship.Model;
using Flagship.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("Flagship.Tests, PublicKey = 002400000480000094000000060200000024000052534131000400000100010041a296859040463dccfcb0b9fef7ff74f0db1a5c12034963bf51209203aabc1beb060f0a015bc7825756efb3bae9929f8ea40404a8aa9668f731718be0b547519260caf58fa7199108d431c9f084342b75883fe01a35809df34e87dd406c6e27c4ff4e63bbb3f1632ca3fbd4387ee821be9d56c4d54e56db8ec57418e91024cc")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2, PublicKey = 0024000004800000940000000602000000240000525341310004000001000100c547cac37abd99c8db225ef2f6c8a3602f3b3606cc9891605d02baa56104f4cfc0734aa39b93bf7852f7d9266654753cc297e7d2edfe0bac1cdcf9f717241550e0a7b191195b7667bb4f64bcb8e2121380fd1d9d46ad2d92d2d15605093924cceaf74c4861eff62abf69b9291ed0a340e113be11e6a7d3113e92484cf7045cc7")]
namespace Flagship.Main
{
    public class Fs
    {
        private static Fs instance;

        private FSSdkStatus _status = FSSdkStatus.SDK_NOT_INITIALIZED;

        private FlagshipConfig _config;

        private IConfigManager _configManager;
        private Visitor _visitor;
        private readonly SdkInitialData _sdkInitialData;

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
            _sdkInitialData = new SdkInitialData()
            {
                InstanceId = Guid.NewGuid().ToString()
            };
        }

        /// <summary>
        /// Return current status of Flagship SDK.
        /// </summary>
        public static FSSdkStatus Status => GetInstance()._status;

        /// <summary>
        /// Return the current config used by the SDK.
        /// </summary>
        public static FlagshipConfig Config => GetInstance()._config;

        private void SetStatus(FSSdkStatus status)
        {
            if (_status == status)
            {
                return;
            }

            if (status == FSSdkStatus.SDK_INITIALIZED)
            {
                _configManager?.TrackingManager?.StartBatchingLoop();
            }
            else
            {
                _configManager?.TrackingManager?.StopBatchingLoop();
            }
            _status = status;
            _config.SetStatus(status);
            VisitorDelegateAbstract.SDKStatus = Status;
        }

        private static bool IsReady()
        {
            return GetInstance()._status == FSSdkStatus.SDK_INITIALIZED;
        }

        /// <summary>
        /// Will return any previous created visitor instance initialized with the SINGLE_INSTANCE
        /// </summary>
        public static Visitor Visitor
        {
            get { return GetInstance()._visitor; }
            internal set { GetInstance()._visitor = value; }
        }

        private IDecisionManager BuildDecisionManager( FlagshipConfig config, HttpClient httpClient)
        {
            IDecisionManager decisionManager = this._configManager?.DecisionManager;

            if (decisionManager != null && decisionManager is BucketingManager bucketingManager)
            {
                bucketingManager.StopPolling();
            }

            if (config.DecisionMode == DecisionMode.BUCKETING)
            {
                decisionManager = new BucketingManager((BucketingConfig)config, httpClient, Murmur.MurmurHash.Create32());

                decisionManager.StatusChange += DecisionManager_StatusChange;
                _ = ((BucketingManager)decisionManager).StartPolling();
                return decisionManager;
            }
            decisionManager = new ApiManager(config, httpClient);
            decisionManager.StatusChange += DecisionManager_StatusChange;
            return decisionManager;
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
            if (config.TrackingManagerConfig == null)
            {
                config.TrackingManagerConfig = new TrackingManagerConfig();
            }
            var fsInstance = GetInstance();

            fsInstance._config = config;

            if (config.LogManager == null)
            {
                config.LogManager = new FsLogManager();
            }

            if (string.IsNullOrWhiteSpace(envId) || string.IsNullOrWhiteSpace(apiKey))
            {
                fsInstance.SetStatus(FSSdkStatus.SDK_NOT_INITIALIZED);
                Log.LogError(config, Constants.INITIALIZATION_PARAM_ERROR, Constants.PROCESS_INITIALIZATION);
                return;
            }

            config.EnvId = envId;
            config.ApiKey = apiKey;

            var httpClient = new HttpClient()
            {
                Timeout = config.Timeout ?? TimeSpan.FromMilliseconds(Constants.REQUEST_TIME_OUT)
            };

            var decisionManager = fsInstance.BuildDecisionManager(config, httpClient);

            var trackingManager = new TrackingManager(config, httpClient, instance._sdkInitialData.InstanceId);


            decisionManager.FlagshipInstanceId = instance._sdkInitialData.InstanceId;

            decisionManager.TrackingManager = trackingManager;


            if (fsInstance._configManager == null)
            {
                fsInstance._configManager = new ConfigManager(config, decisionManager, trackingManager);
            }
            else
            {
                fsInstance._configManager.DecisionManager = decisionManager;
                fsInstance._configManager.TrackingManager = trackingManager;
            }

            instance._sdkInitialData.LastInitializationTimestamp = DateTime.Now.ToUniversalTime().ToString(Constants.FORMAT_UTC);

            if (fsInstance._status != FSSdkStatus.SDK_INITIALIZING)
            {
                fsInstance.SetStatus(FSSdkStatus.SDK_INITIALIZED);
            }


            Log.LogInfo(config, string.Format(Constants.SDK_STARTED_INFO, Constants.SDK_VERSION, fsInstance._status), Constants.PROCESS_INITIALIZATION);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns> 
        public static async Task Close() 
        {
            await instance?._configManager?.TrackingManager?.SendBatch(CacheTriggeredBy.Flush);
        }
        private static void DecisionManager_StatusChange(FSSdkStatus status)
        {
            GetInstance().SetStatus(status);
        }

    
        /// <summary>
        /// Initialize the builder and Return a VisitorBuilder or null if the SDK hasn't started successfully.
        /// </summary>
        /// <param name="visitorId"></param>
        /// <param name="instanceType"></param>
        /// <returns></returns>
        public static VisitorBuilder NewVisitor(string visitorId, bool hasConsented)
        {
            if (!IsReady())
            {
                return null;
            }
            var instance = GetInstance();
            return VisitorBuilder.Builder(instance._configManager, visitorId, hasConsented, instance._sdkInitialData);
        }

   
    }
}

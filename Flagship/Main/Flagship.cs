using Flagship.Api;
using Flagship.Config;
using Flagship.Decision;
using Flagship.Delegate;
using Flagship.Enums;
using Flagship.FsVisitor;
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
    public class Flagship
    {
        private static Flagship instance;

        private FlagshipStatus _status = FlagshipStatus.NOT_INITIALIZED;

        private FlagshipConfig _config;

        private IConfigManager _configManager;

        protected static Flagship GetInstance()
        {
            if (instance == null)
            {
                instance = new Flagship();
            }
            return instance;
        }

        private Flagship()
        {

        }

        public static FlagshipStatus Status => GetInstance()._status;

        public static FlagshipConfig Config => GetInstance()._config;

        private void SetStatus(FlagshipStatus status)
        {
            _status = status;
            _config.SetStatus(status);
        }

        private static bool IsReady()
        {
            return GetInstance()._status == FlagshipStatus.READY;
        }

        public static void Start(string envId, string apiKey, FlagshipConfig config = null)
        {
#if NET40 || NETSTANDARD2_0
ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls;
#else
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
#endif

            if (config == null)
            {
                config = new DecisionApiConfig();
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
            var httpClient = new HttpClient();
            var decisionManager = new ApiManager(config, httpClient);
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

        private static void DecisionManager_StatusChange(FlagshipStatus status)
        {
            GetInstance().SetStatus(status);
        }

        /// <summary>
        /// Initialize the builder and Return a VisitorBuilder or null if the SDK hasn't started successfully.
        /// </summary>
        /// <param name="visitorId"></param>
        /// <returns>VisitorBuilder | null</returns>
        public static VisitorBuilder NewVisitor(string visitorId = null)
        {
            if (!IsReady())
            {
                return null;
            }
            return VisitorBuilder.Builder(GetInstance()._configManager, visitorId);
        }
    }
}

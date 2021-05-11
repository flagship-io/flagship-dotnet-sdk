using Flagship.Model.Config;
using Flagship.Model.Decision;
using Flagship.Services.Decision;
using Flagship.Services.ExceptionHandler;
using Flagship.Services.HitSender;
using Flagship.Services.Logger;

namespace Flagship
{
    public class FlagshipContext
    {
        public readonly string EnvironmentId;
        public readonly string ApiKey;
        public readonly FlagshipOptions Options;
        public readonly ILogger Logger;
        public readonly IExceptionHandler ExceptionHandler;
        public readonly IDecisionManager DecisionManager;
        public readonly ISender Sender;

        public FlagshipContext(string environmentId, string apiKey, FlagshipOptions options = null)
        {
            EnvironmentId = environmentId;
            ApiKey = apiKey;
            Options = options;
            Logger = options?.Logger ?? new DefaultLogger();
            ExceptionHandler = options?.ExceptionHandler ?? new DefaultExceptionHandler(Logger);
            Sender = new Sender(this);

            var decisionMode = options?.DecisionMode ?? Mode.API;

            if (decisionMode == Mode.Bucketing)
            {
                DecisionManager = new BucketingClient(this);
            }
            else
            {
                DecisionManager = new APIDecisionManager(this);
            }
        }
    }
}

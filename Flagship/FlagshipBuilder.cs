using Flagship.Model.Config;
using Flagship.Model.Decision;
using Flagship.Services;
using Flagship.Services.Decision;
using Flagship.Services.ExceptionHandler;
using Flagship.Services.HitSender;
using Flagship.Services.Logger;

namespace Flagship
{
    public static class FlagshipBuilder
    {
        public static IFlagshipClient Start(string environmentId, string apiKey, FlagshipOptions options = null)
        {
            var context = new FlagshipContext(environmentId, apiKey, options);
            var flagshipClient = new FlagshipClient(context);

            return flagshipClient;
        }
    }
}

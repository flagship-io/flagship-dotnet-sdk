using Flagship.Model.Logs;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Globalization;
using static Flagship.Model.Logs.LogCodes;

namespace Flagship.Services.Logger
{
    public class DefaultLogger : ILogger
    {
        public void Log(LogLevel level, LogCode code, params object[] args)
        {
            object toSerialize = args;
            if (args.Length == 1)
            {
                toSerialize = args[0];
            }

            var argsJson = JsonConvert.SerializeObject(toSerialize, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
            });

            string line = $"[{level}] : {code.GetDescription()}. Parameters: {argsJson}";
            Trace.WriteLine(line);
        }
}
}

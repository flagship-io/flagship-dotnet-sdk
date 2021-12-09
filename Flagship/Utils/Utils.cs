using Flagship.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flagship.Utils
{
    internal static class Utils
    {
        public static void LogError(FlagshipConfig config, string message, string tag)
        {
            try
            {
                if (config == null || config.LogManager == null ||  config.LogLevel< Enum.LogLevel.ERROR)
                {
                    return;
                }

                config.LogManager.Error(message, tag);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
        }

        public static void LogInfo(FlagshipConfig config, string message, string tag)
        {
            try
            {
                if (config == null || config.LogManager==null || config.LogLevel < Enum.LogLevel.INFO)
                {
                    return;
                }

                config.LogManager.Info(message, tag);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
        }
    }
}

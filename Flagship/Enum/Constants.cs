using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flagship.Enum
{
    internal static class Constants
    {
        public const int DEFAULT_POLLING_INTERVAL = 1;
        /// <summary>
        /// Default request timeout in second
        /// </summary>
        public const int REQUEST_TIME_OUT = 2;

        public const string BASE_API_URL = "https://decision.flagship.io/v2/";

        public const string SEND_CONTEXT_EVENT = "sendContextEvent";

        public const string HEADER_X_API_KEY = "x-api-key";
        public const string HEADER_CONTENT_TYPE = "Content-Type";
        public const string HEADER_X_SDK_CLIENT = "x-sdk-client";
        public const string HEADER_X_SDK_VERSION = "x-sdk-version";
        public const string HEADER_APPLICATION_JSON = "application/json";
        public const string SDK_LANGUAGE = ".NET";
        public const string SDK_VERSION = "v2";
    }
}

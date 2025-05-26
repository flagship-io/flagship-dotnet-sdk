using System.Collections.Generic;

namespace Flagship.Enums
{
    public static class PredefinedContext
    {
        /// <summary>
        /// Current device locale
        /// </summary>
        public const string DEVICE_LOCALE = "sdk_deviceLanguage";

        /// <summary>
        /// Current device type  tablet, pc, server, iot, other
        /// </summary>
        public const string DEVICE_TYPE = "sdk_deviceType";

        /// <summary>
        /// Current device model
        /// </summary>
        public const string DEVICE_MODEL = "sdk_deviceModel";

        /// <summary>
        /// Current visitor city
        /// </summary>
        public const string LOCATION_CITY = "sdk_city";

        /// <summary>
        /// Current visitor region
        /// </summary>
        public const string LOCATION_REGION = "sdk_region";

        /// <summary>
        /// Current visitor country
        /// </summary>
        public const string LOCATION_COUNTRY = "sdk_country";

        /// <summary>
        /// Current visitor latitude
        /// </summary>
        public const string LOCATION_LAT = "sdk_lat";

        /// <summary>
        /// Current visitor longitude
        /// </summary>
        public const string LOCATION_LONG = "sdk_long";

        /// <summary>
        /// Device public ip
        /// </summary>
        public const string IP = "sdk_ip";

        /// <summary>
        /// OS name
        /// </summary>
        public const string OS_NAME = "sdk_osName";

        /// <summary>
        /// OS version name
        /// </summary>
        public const string OS_VERSION_NAME = "sdk_osVersionName";

        /// <summary>
        /// OS version code
        /// </summary>
        public const string OS_VERSION_CODE = "sdk_osVersionCode";

        /// <summary>
        /// Carrier operator
        /// </summary>
        public const string CARRIER_NAME = "sdk_carrierName";

        /// <summary>
        /// Internet connection type : 4G, 5G, Fiber
        /// </summary>
        public const string INTERNET_CONNECTION = "sdk_internetConnection";

        /// <summary>
        /// Customer app version name
        /// </summary>
        public const string APP_VERSION_NAME = "sdk_versionName";

        /// <summary>
        /// Customer app version code
        /// </summary>
        public const string APP_VERSION_CODE = "sdk_versionCode";

        /// <summary>
        /// Current customer app interface name
        /// </summary>
        public const string INTERFACE_NAME = "sdk_interfaceName";

        /// <summary>
        /// Flagship SDK client name
        /// </summary>
        public const string FLAGSHIP_CLIENT = "fs_client";

        /// <summary>
        /// Flagship SDK version name
        /// </summary>
        public const string FLAGSHIP_VERSION = "fs_version";

        /// <summary>
        /// Current visitor id
        /// </summary>
        public const string FLAGSHIP_VISITOR = "fs_users";

        private static readonly IDictionary<string, string> FLAGSHIP_CONTEXT = new Dictionary<
            string,
            string
        >
        {
            [DEVICE_LOCALE] = "string",
            [DEVICE_TYPE] = "string",
            [DEVICE_MODEL] = "string",
            [LOCATION_CITY] = "string",
            [LOCATION_REGION] = "string",
            [LOCATION_COUNTRY] = "string",
            [LOCATION_LAT] = "number",
            [LOCATION_LONG] = "number",
            [IP] = "string",
            [OS_NAME] = "string",
            [OS_VERSION_NAME] = "string",
            [OS_VERSION_CODE] = "number",
            [CARRIER_NAME] = "string",
            [INTERNET_CONNECTION] = "string",
            [APP_VERSION_NAME] = "string",
            [APP_VERSION_CODE] = "number",
            [INTERFACE_NAME] = "string",
            [FLAGSHIP_CLIENT] = "string",
            [FLAGSHIP_VERSION] = "string",
            [FLAGSHIP_VISITOR] = "string",
        };

        internal static bool IsPredefinedContext(string predefinedContext)
        {
            return FLAGSHIP_CONTEXT.ContainsKey(predefinedContext);
        }

        internal static string GetPredefinedType(string predefinedContext)
        {
            return FLAGSHIP_CONTEXT.ContainsKey(predefinedContext)
                ? FLAGSHIP_CONTEXT[predefinedContext]
                : null;
        }

        internal static bool CheckType(string predefinedContext, object value)
        {
            if (value == null)
            {
                return false;
            }

            if (!IsPredefinedContext(predefinedContext))
            {
                return false;
            }
            ;

            var type = FLAGSHIP_CONTEXT[predefinedContext];

            bool check = false;

            switch (type)
            {
                case "string":
                    check = value is string;
                    break;
                case "number":
                    check = value is long || value is int || value is double;
                    break;
            }

            return check;
        }
    }
}

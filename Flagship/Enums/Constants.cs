using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flagship.Enums 
{
    internal static class Constants
    {
        public const int DEFAULT_POLLING_INTERVAL = 2000;

        public const int DEFAULT_HIT_CACHE_TIME = 14400000;

        public const int DEFAULT_BATCH_TIME_INTERVAL = 10000;

        public const int DEFAULT_POOL_MAX_SIZE = 100;

        public const int BATCH_MAX_SIZE = 2500000;

        /// <summary>
        /// Default request timeout in second
        /// </summary>
        public const int REQUEST_TIME_OUT = 2000;

        public const string BASE_API_URL = "https://staging-decision.flagship.io/v2/"; // "https://decision.flagship.io/v2/";

        public const string HIT_API_URL = "https://ariane.abtasty.com";

        public const string HIT_EVENT_URL = "https://events.flagship.io/"; 

        public const string BUCKETING_API_URL = "https://cdn-staging.flagship.io/{0}/bucketing.json";

        public const string BUCKETING_API_CONTEXT_URL = "https://decision.flagship.io/v2/{0}/events";

        public const string SEND_CONTEXT_EVENT = "sendContextEvent";

        public const string SDK_VERSION = "3.1.2";

        public const string FLAGSHIP_SDK = "Flagship SDK";

        public const string HEADER_X_API_KEY = "x-api-key";
        public const string HEADER_CONTENT_TYPE = "Content-Type";
        public const string HEADER_X_SDK_CLIENT = "x-sdk-client";
        public const string HEADER_X_SDK_VERSION = "x-sdk-version";
        public const string HEADER_APPLICATION_JSON = "application/json";
        public const string SDK_LANGUAGE = ".NET";

        public const string FS_CONSENT = "fs_consent";

        public const string CONTEXT_PARAM_ERROR = "params {0} must be a non null String, and 'value' must be one of the following types string, Number, Boolean";

        public const string INITIALIZATION_PARAM_ERROR = "Params 'envId' and 'apiKey' must not be null or empty.";

        public const string PROCESS_INITIALIZATION = "INITIALIZATION";

        public const string VISITOR_ID_ERROR = "visitorId must not be null or empty";

        public const string FLAGSHIP_VISITOR_NOT_AUTHENTICATE = "Visitor is not authenticated yet";

        public const string SDK_STARTED_INFO = "Flagship SDK (version: {0}) READY";

        public const string METHOD_DEACTIVATED_ERROR = "Method {0} is deactivated while SDK status is: {1}.";

        public const string GET_FLAG_MISSING_ERROR = "No Flag for key {0}. Default value is returned.";

        public const string GET_FLAG_CAST_ERROR = "Flag for key {0} has a different type. Default value is returned.";

        public const string GET_FLAG_ERROR = "No flag for key {0}, no activate will be sent";

        public const string USER_EXPOSED_CAST_ERROR = "Flag for key {0} has a different type with defaultValue, no activate will be sent";

        public const string GET_METADATA_CAST_ERROR = "Flag for key {0} has a different type with defaultValue, an empty metadata object is returned";

        public const string HIT_NOT_NULL = "A hit must not be null";

        public const string METHOD_DEACTIVATED_CONSENT_ERROR = "Method {0} is deactivated for visitor {1} : visitor did not consent.";

        public const string PREDEFINED_CONTEXT_TYPE_ERROR = "Predefined Context {0} must be type of {1}";

        public const string METHOD_DEACTIVATED_BUCKETING_ERROR = "Method {0} is deactivated on Bucketing mode.";

        public const string BATCH = "batch";
        public const string CUSTOMER_ENV_ID_API_ITEM = "cid";
        public const string CUSTOMER_UID = "cuid";
        public const string ANONYMOUS_ID = "aid";
        public const string VISITOR_ID_API_ITEM = "vid";
        public const string VARIATION_GROUP_ID_API_ITEM = "caid";
        public const string VARIATION_ID_API_ITEM = "vaid";
        public const string DS_API_ITEM = "ds";
        public const string T_API_ITEM = "t";
        public const string DL_API_ITEM = "dl";
        public const string SDK_APP = "APP";
        public const string TID_API_ITEM = "tid";
        public const string TA_API_ITEM = "ta";
        public const string TT_API_ITEM = "tt";
        public const string TC_API_ITEM = "tc";
        public const string TCC_API_ITEM = "tcc";
        public const string ICN_API_ITEM = "icn";
        public const string SM_API_ITEM = "sm";
        public const string PM_API_ITEM = "pm";
        public const string TR_API_ITEM = "tr";
        public const string TS_API_ITEM = "ts";
        public const string IN_API_ITEM = "in";
        public const string IC_API_ITEM = "ic";
        public const string IP_API_ITEM = "ip";
        public const string IQ_API_ITEM = "iq";
        public const string IV_API_ITEM = "iv";
        public const string EVENT_CATEGORY_API_ITEM = "ec";
        public const string EVENT_ACTION_API_ITEM = "ea";
        public const string EVENT_LABEL_API_ITEM = "el";
        public const string EVENT_VALUE_API_ITEM = "ev";
        public const string USER_IP_API_ITEM = "uip";
        public const string SCREEN_RESOLUTION_API_ITEM = "sr";
        public const string USER_LANGUAGE = "ul";
        public const string SESSION_NUMBER = "sn";
        public const string VARIATION_GROUP_ID_API_ITEM_ACTIVATE = "caid";
        public const string CUSTOMER_ENV_ID_API_ACTIVATE = "cid";
        public const string QT_API_ITEM = "qt";

        public const string HIT_EVENT_ERROR_MESSSAGE = "event category and event action are required";

        public const string HIT_ITEM_ERROR_MESSAGE = "Transaction Id, Item name and item code are required";

        public const string HIT_PAGE_ERROR_MESSAGE = "documentLocation url is required";

        public const string HIT_SCREEN_ERROR_MESSAGE = "documentLocation Screen name is required";

        public const string HIT_TRANSACTION_ERROR_MESSAGE = "Transaction Id and Transaction affiliation are required";


    }

}

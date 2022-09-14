using Flagship.Config;
using Flagship.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flagship.Hit
{
    
    public abstract class HitAbstract
    {

        [JsonProperty("Key")]
        internal string Key { get; set; }
        [JsonProperty("VisitorId")]
        internal string VisitorId { get; set; }
        internal FlagshipConfig Config { get; set; }

        [Newtonsoft.Json.JsonProperty("Type")]
        internal HitType Type { get; set; }

        [Newtonsoft.Json.JsonProperty("DS")]
        internal string DS { get; set; }

        [Newtonsoft.Json.JsonProperty("AnonymousId")]
        internal string AnonymousId { get; set; }

        /// <summary>
        /// User IP
        /// </summary>
        public string UserIp { get; set; }

        /// <summary>
        /// Screen resolution.
        /// </summary>
        public string ScreenResolution { get; set; }

        /// <summary>
        /// User language
        /// </summary>
        public string Locale { get; set; }

        /// <summary>
        /// Session number
        /// </summary>
        public string SessionNumber { get; set; }
        
        [JsonProperty("CreatedAt")]
        internal DateTime CreatedAt { get; set; }

        public HitAbstract(HitType type)
        {
            Type = type;
            CreatedAt = DateTime.Now;
        }

        internal virtual IDictionary<string, object> ToApiKeys()
        {

            var apiKeys = new Dictionary<string, object>()
            {
                [Constants.VISITOR_ID_API_ITEM] = AnonymousId ?? VisitorId,
                [Constants.DS_API_ITEM] = DS,
                [Constants.CUSTOMER_ENV_ID_API_ITEM] = Config?.EnvId,
                [Constants.T_API_ITEM] = $"{Type}",
                [Constants.CUSTOMER_UID] = null,
                [Constants.QT_API_ITEM] = (DateTime.Now - CreatedAt).Milliseconds
            };

            if (UserIp!=null)
            {
                apiKeys[Constants.USER_IP_API_ITEM] = UserIp;
            }

            if (ScreenResolution != null)
            {
                apiKeys[Constants.SCREEN_RESOLUTION_API_ITEM] = ScreenResolution;
            }

            if (Locale!=null)
            {
                apiKeys[Constants.USER_LANGUAGE] = Locale;
            }

            if (SessionNumber!=null)
            {
                apiKeys[Constants.SESSION_NUMBER] = SessionNumber;
            }

            if (!string.IsNullOrWhiteSpace(VisitorId) && !string.IsNullOrWhiteSpace(AnonymousId))
            {
                apiKeys[Constants.VISITOR_ID_API_ITEM] = AnonymousId;
                apiKeys[Constants.CUSTOMER_UID] = VisitorId;
            }

            return apiKeys;
        }

        internal virtual  bool IsReady(bool checkParent = true)
        {
            return !string.IsNullOrWhiteSpace(VisitorId) && 
                !string.IsNullOrWhiteSpace(DS) && 
                Config!=null && 
                !string.IsNullOrWhiteSpace(Config.EnvId);
        }
            
        abstract internal  string GetErrorMessage();
    }
}

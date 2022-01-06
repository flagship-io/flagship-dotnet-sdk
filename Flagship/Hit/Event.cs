using Flagship.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flagship.Hit
{
    public class Event : HitAbstract
    {
        public EventCategory Category { get; set; }
        public string Action { get; set; }
        public string Label { get; set; }
        public double? Value { get; set; }

        public Event( EventCategory category, string action):base(HitType.EVENT)
        {
            Category = category;
            Action = action;
        }

        internal override IDictionary<string, object> ToApiKeys()
        {
            var apiKeys = base.ToApiKeys();
            apiKeys[Constants.EVENT_CATEGORY_API_ITEM] = $"{Category}";
            apiKeys[Constants.EVENT_ACTION_API_ITEM] = Action;

            if (!string.IsNullOrWhiteSpace(Label))
            {
                apiKeys[Constants.EVENT_LABEL_API_ITEM] = Label;
            }

            if (Value.HasValue)
            {
                apiKeys[Constants.EVENT_VALUE_API_ITEM]= Value.Value;
            }

            return apiKeys;
        }

        internal override bool IsReady()
        {
            return base.IsReady() && !string.IsNullOrWhiteSpace(Action);
        }

        internal override string GetErrorMessage()
        {
            return Constants.HIT_EVENT_ERROR_MESSSAGE;
        }
    }
}

using Flagship.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flagship.Hit
{
    /// <summary>
    /// This hit can be used for any event (e.g. Add To Cart click, newsletter subscription).
    /// </summary>
    public class Event : HitAbstract
    {
        /// <summary>
        /// Specifies the category of your event.
        /// </summary>
        public EventCategory Category { get; set; }

        /// <summary>
        /// Event name that will also serve as the KPI that you will have inside your reporting. 
        /// </summary>
        public string Action { get; set; }

        /// <summary>
        /// Additional description of your event.
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Specifies the monetary value associated with an event (e.g. you earn 10 to 100 euros depending on the quality of lead generated). NOTE: this value must be non-negative.
        /// </summary>
        public uint? Value { get; set; }

        /// <summary>
        /// This hit can be used for any event (e.g. Add To Cart click, newsletter subscription).
        /// </summary>
        /// <param name="category">Specifies the category of your event.</param>
        /// <param name="action">Event name that will also serve as the KPI that you will have inside your reporting. </param>
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

        internal override bool IsReady(bool checkParent = true)
        {
            return (!checkParent || base.IsReady()) && !string.IsNullOrWhiteSpace(Action);
        }

        internal override string GetErrorMessage()
        {
            return Constants.HIT_EVENT_ERROR_MESSSAGE;
        }
    }
}

using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace Flagship.Model.Hits
{
    public enum EventCategory
    {
        [EnumMember(Value = "User Engagement")]
        UserEngagement,
        [EnumMember(Value = "Action Tracking")]
        ActionTracking
    }
}

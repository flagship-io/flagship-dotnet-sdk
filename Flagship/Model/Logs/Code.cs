using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Flagship.Model.Logs
{
    public enum LogCode
    {
        [Description("An exception occured")]
        EXCEPTION_OCCURED,

        [Description("Campaign ID has been activated with variation ID")]
        CAMPAIGN_ACTIVATED,

        [Description("Context value is not of type bool")]
        CONTEXT_VALUE_BOOL,

        [Description("Context value is not of type double")]
        CONTEXT_VALUE_DOUBLE,

        [Description("Visitor not tracked")]
        VISITOR_NOT_TRACKED,

        [Description("Missing context key")]
        TARGETING_MISSING_CONTEXT_KEY,

        [Description("Targeting type not handled")]
        TARGETING_TYPE_NOT_HANDLED,

        [Description("Targeting operator not handled")]
        TARGETING_OPERATOR_NOT_HANDLED,

        [Description("Bucketing configuration not loaded yet")]
        BUCKETING_NOT_LOADED,

        [Description("Context sent to event tracking")]
        CONTEXT_SENT_TRACKING,

        [Description("An error occured when loading configuration")]
        BUCKETING_LOADING_ERROR,

        [Description("Bucketing file has been loaded successfully")]
        BUCKETING_LOAD_SUCCESS,

        [Description("Panic mode enabled, returning default response")]
        BUCKETING_PANIC_MODE_ENABLED,

        [Description("Panic mode enabled, tracking is disabled")]
        PANIC_NO_TRACKING,

        [Description("Hit sent to data collect")]
        HIT_SENT
    }

    public static class LogCodes
    {
        public static string GetDescription<T>(this T e) where T : IConvertible
        {
            if (e is Enum)
            {
                Type type = e.GetType();
                Array values = Enum.GetValues(type);

                foreach (int val in values)
                {
                    if (val == e.ToInt32(CultureInfo.InvariantCulture))
                    {
#if NETSTANDARD1_6
                        var memInfo = type.GetTypeInfo().GetMember(type.GetTypeInfo().GetEnumName(val));
#else
                        var memInfo = type.GetMember(type.GetEnumName(val));

#endif
                        if (memInfo[0]
                            .GetCustomAttributes(typeof(DescriptionAttribute), false)
                            .FirstOrDefault() is DescriptionAttribute descriptionAttribute)
                        {
                            return descriptionAttribute.Description;
                        }
                    }
                }
            }

            return string.Empty;
        }
    }
}
using Flagship.Model.Bucketing;
using Flagship.Model.Logs;
using Flagship.Services.Logger;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Flagship.Services.Bucketing
{
    public class TargetingMatch
    {
        private readonly ILogger logger;
        public TargetingMatch(ILogger logger)
        {
            this.logger = logger;
        }

        public bool Match(object contextValue, TargetingOperator targetingOperator, object targetingValue)
        {
            switch (targetingValue)
            {
                case bool value:
                    if (contextValue is bool boolean)
                    {
                        return MatchBool(boolean, targetingOperator, value);
                    }
                    logger.Log(LogLevel.ERROR, LogCode.CONTEXT_VALUE_BOOL, new { contextValue });
                    return false;
                case double value:
                    try
                    {
                        var doubleContext = Convert.ToDouble(contextValue, CultureInfo.InvariantCulture);
                        return MatchNumber(doubleContext, targetingOperator, value);
                    }
                    catch (Exception e)
                    {
                        logger.Log(LogLevel.ERROR, LogCode.CONTEXT_VALUE_DOUBLE, new { contextValue });
                        return false;
                    }
                case string value:
                    return MatchString(contextValue as string, targetingOperator, value);
                case IEnumerable<object> value:
                    if (targetingOperator == TargetingOperator.EQUALS)
                    {
                        return value.Any(x => Match(contextValue, targetingOperator, x));
                    }
                    if (targetingOperator == TargetingOperator.NOT_EQUALS)
                    {
                        return !value.Any(x => Match(contextValue, targetingOperator, x));
                    }
                    return false;
                default:
                    logger.Log(LogLevel.ERROR, LogCode.TARGETING_TYPE_NOT_HANDLED, new { targetingType = targetingValue.GetType().Name });
                    return false;
            }
        }

        public bool MatchBool(bool contextValue, TargetingOperator targetingOperator, bool targetingValue)
        {
            switch (targetingOperator)
            {
                case TargetingOperator.EQUALS:
                    return contextValue.Equals(targetingValue);
                case TargetingOperator.NOT_EQUALS:
                    return !contextValue.Equals(targetingValue);
                default:
                    logger.Log(LogLevel.ERROR, LogCode.TARGETING_OPERATOR_NOT_HANDLED, new { targetingOperator });
                    return false;
            }
        }

        public bool MatchString(string contextValue, TargetingOperator targetingOperator, string targetingValue)
        {
            switch (targetingOperator)
            {
                case TargetingOperator.CONTAINS:
                    return contextValue.ToLowerInvariant().Contains(targetingValue.ToLowerInvariant());
                case TargetingOperator.ENDS_WITH:
                    return contextValue.EndsWith(targetingValue, StringComparison.OrdinalIgnoreCase);
                case TargetingOperator.EQUALS:
                    return contextValue.Equals(targetingValue, StringComparison.OrdinalIgnoreCase);
                case TargetingOperator.GREATER_THAN:
                    return string.Compare(contextValue, targetingValue, StringComparison.OrdinalIgnoreCase) > 0;
                case TargetingOperator.GREATER_THAN_OR_EQUALS:
                    return string.Compare(contextValue, targetingValue, StringComparison.OrdinalIgnoreCase) >= 0;
                case TargetingOperator.LOWER_THAN:
                    return string.Compare(contextValue, targetingValue, StringComparison.OrdinalIgnoreCase) < 0;
                case TargetingOperator.LOWER_THAN_OR_EQUALS:
                    return string.Compare(contextValue, targetingValue, StringComparison.OrdinalIgnoreCase) <= 0;
                case TargetingOperator.NOT_CONTAINS:
                    return !contextValue.ToLowerInvariant().Contains(targetingValue.ToLowerInvariant());
                case TargetingOperator.NOT_EQUALS:
                    return !contextValue.Equals(targetingValue, StringComparison.OrdinalIgnoreCase);
                case TargetingOperator.STARTS_WITH:
                    return contextValue.StartsWith(targetingValue, StringComparison.OrdinalIgnoreCase);
                default:
                    logger.Log(LogLevel.ERROR, LogCode.TARGETING_OPERATOR_NOT_HANDLED, new { targetingOperator });
                    return false;
            }
        }

        public bool MatchNumber(double contextValue, TargetingOperator targetingOperator, double targetingValue)
        {
            switch (targetingOperator)
            {
                case TargetingOperator.EQUALS:
                    return contextValue.Equals(targetingValue);
                case TargetingOperator.GREATER_THAN:
                    return contextValue > targetingValue;
                case TargetingOperator.GREATER_THAN_OR_EQUALS:
                    return contextValue >= targetingValue;
                case TargetingOperator.LOWER_THAN:
                    return contextValue < targetingValue;
                case TargetingOperator.LOWER_THAN_OR_EQUALS:
                    return contextValue <= targetingValue;
                case TargetingOperator.NOT_EQUALS:
                    return !contextValue.Equals(targetingValue);
                default:
                    logger.Log(LogLevel.ERROR, LogCode.TARGETING_OPERATOR_NOT_HANDLED, new { targetingOperator });
                    return false;
            }
        }
    }
}

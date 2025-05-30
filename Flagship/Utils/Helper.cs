﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Flagship.Utils
{
    internal static class Helper
    {
        public static string TwoDigit(int value)
        {
            var prefixValue = value < 10 ? "0" : null;
            return $"{prefixValue}{value}";
        }

        public static bool HasSameType(object params1, object params2)
        {
            if (params1 == null && params2 == null)
            {
                return true;
            }
            if (params1 == null || params2 == null)
            {
                return false;
            }
            return params1.GetType() == params2.GetType();
        }

        public static string ErrorFormat(string errorMessage, object errorData)
        {
            return JsonConvert.SerializeObject(
                new { ErrorMessage = errorMessage, ErrorData = errorData }
            );
        }

        public static Task VoidTask()
        {
            var tcs = new TaskCompletionSource<object>();
            tcs.SetResult(null);
            return tcs.Task;
        }

        public static string ValueToHex<T>(T value)
        {
            var jsonString = JsonConvert.SerializeObject(value);
            var sb = new StringBuilder();
            foreach (char c in jsonString)
            {
                sb.Append(Convert.ToInt32(c).ToString("x"));
            }
            return sb.ToString();
        }

        public static bool IsDeepEqual(
            IDictionary<string, object> dict1,
            IDictionary<string, object> dict2
        )
        {
            if (dict1 == null && dict2 == null)
            {
                return true;
            }
            if (dict1 == null || dict2 == null)
            {
                return false;
            }
            if (dict1.Count != dict2.Count)
            {
                return false;
            }
            foreach (var key in dict1.Keys)
            {
                if (!dict2.ContainsKey(key))
                {
                    return false;
                }
                if (!dict1[key].Equals(dict2[key]))
                {
                    return false;
                }
            }
            return true;
        }
    }
}

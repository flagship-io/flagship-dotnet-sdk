using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flagship.Utils
{
    internal static class Utils
    {
        public static string TwoDigit(int value)
        {
            var prefixValue = value < 10 ? "0" : null;
            return $"{prefixValue}{value}";
        }
    }
}

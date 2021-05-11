using System;
using System.Collections.Generic;
using System.Text;

namespace Flagship.Model.Bucketing
{
    public class Targeting
    {
        public TargetingOperator Operator { get; set; }
        public string Key { get; set; }
        public object Value { get; set; }
    }
}
 
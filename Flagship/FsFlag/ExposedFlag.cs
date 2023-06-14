using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flagship.FsFlag
{
    internal class ExposedFlag : IExposedFlag
    {
        public string Key { get; set; }

        public object Value { get; set; }

        public object DefaultValue { get; set; }

        public IFlagMetadata Metadata { get; set; }

        public ExposedFlag(string key, object value, object defaultValue, IFlagMetadata metadata)
        {
            Key = key;
            Value = value;
            DefaultValue = defaultValue;
            Metadata = metadata;
        }
    }
}

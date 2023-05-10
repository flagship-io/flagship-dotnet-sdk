using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flagship.FsFlag
{
    public interface IExposedFlag
    {
        string Key { get; }
        object Value { get; }
        object DefaultValue { get; }
        IFlagMetadata Metadata { get; }

    }
}

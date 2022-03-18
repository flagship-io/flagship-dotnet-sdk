using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flagship.FsFlag
{
    public interface IFlag<T> 
    {
        T GetValue(bool userExposed=true);

        bool Exists { get; }

        Task UserExposed();

        IFlagMetadata Metadata { get; }
    }
}

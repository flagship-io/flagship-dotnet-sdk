using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flagship.FsFlag
{
    public interface IFlag<T> 
    {
        T Value(bool userExposed=true);

        bool Exist { get; }

        Task UserExposed();

        IFlagMetadata Metadata { get; }
    }
}

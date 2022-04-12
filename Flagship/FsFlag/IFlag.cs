using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flagship.FsFlag
{
    public interface IFlag<T> 
    {
        /// <summary>
        /// Returns the value from the assigned campaign variation or the Flag default value if the Flag does not exist, or if types are different.
        /// </summary>
        /// <param name="userExposed">Tells Flagship the user have been exposed and have seen this flag. This will increment the visits for the current variation on your campaign reporting.
        /// If needed it is possible to set this param to false and call UserExposed() afterward when the user sees it.</param>
        /// <returns></returns>
        T GetValue(bool userExposed=true);

        /// <summary>
        /// Return true if a Flag exists in Flagship
        /// </summary>
        bool Exists { get; }

        /// <summary>
        /// Tells Flagship the user have been exposed and have seen this flag. This will increment the visits for the current variation on your campaign reporting. No user exposition will be sent if the Flag doesn't exist or if the default value type do not correspond to the Flag type in Flagship.
        /// </summary>
        /// <returns></returns>
        Task UserExposed();

        /// <summary>
        /// Return the campaign information metadata or an empty object if the Flag doesn't exist or if the default value type does not correspond to the Flag type in Flagship.
        /// </summary>
        IFlagMetadata Metadata { get; }
    }
}

using System.Threading.Tasks;
using Flagship.Enums;

namespace Flagship.FsFlag
{
    public interface IFlag
    {
        /// <summary>
        /// Return the Flag value or the default value if the Flag doesn't exist or if the default value type do not correspond to the Flag type in Flagship.
        /// </summary>
        /// <typeparam name="T">The type of the Flag value</typeparam>
        /// <param name="defaultValue">The default value to return if the Flag doesn't exist or if the default value type do not correspond to the Flag type in Flagship.</param>
        /// <param name="visitorExposed">If true, the visitor will be exposed to the Flag. If false, the visitor will not be exposed to the Flag.</param>
        T GetValue<T>(T defaultValue, bool visitorExposed = true);

        /// <summary>
        /// Return true if a Flag exists in Flagship
        /// </summary>
        bool Exists { get; }

        /// <summary>
        /// Tells Flagship the visitor have been exposed and have seen this flag. This will increment the visits for the current variation on your campaign reporting. No user exposition will be sent if the Flag doesn't exist or if the default value type do not correspond to the Flag type in Flagship.
        /// </summary>
        /// <returns></returns>
        Task VisitorExposed();

        /// <summary>
        /// Return the campaign information metadata or an empty object if the Flag doesn't exist or if the default value type does not correspond to the Flag type in Flagship.
        /// </summary>
        IFlagMetadata Metadata { get; }

        FSFlagStatus Status { get; }
    }
}

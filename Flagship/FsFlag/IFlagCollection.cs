using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Flagship.Enums;

namespace Flagship.FsFlag
{
    public interface IFlagCollection : IEnumerable<KeyValuePair<string, IFlag>>
    {
        /// <summary>
        /// Gets the number of flags in the collection.
        /// </summary>
        int Size { get; }

        /// <summary>
        /// Retrieves the flag associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the flag to retrieve.</param>
        /// <returns>The flag associated with the specified key, or null if the key is not found.</returns>
        IFlag Get(string key);

        /// <summary>
        /// Checks if the collection contains a flag with the specified key.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <returns>True if the collection contains a flag with the specified key, false otherwise.</returns>
        bool Has(string key);

        /// <summary>
        /// Gets the keys of all flags in the collection.
        /// </summary>
        /// <returns>A set of all keys in the collection.</returns>
        HashSet<string> Keys();

        /// <summary>
        /// Filters the collection based on a predicate function.
        /// </summary>
        /// <param name="predicate">The predicate function used to filter the collection.</param>
        /// <returns>A new IFSFlagCollection containing the flags that satisfy the predicate.</returns>
        IFlagCollection Filter(Func<IFlag, string, IFlagCollection, bool> predicate);

        /// <summary>
        /// Exposes all flags in the collection.
        /// </summary>
        /// <returns>A task that completes when all flags have been exposed.</returns>
        Task ExposeAllAsync();

        /// <summary>
        /// Retrieves the metadata for all flags in the collection.
        /// </summary>
        /// <returns>A dictionary containing the metadata for all flags in the collection.</returns>
        Dictionary<string, IFlagMetadata> GetMetadata();

        /// <summary>
        /// Serializes the metadata for all flags in the collection.
        /// </summary>
        /// <returns>An array of serialized flag metadata.</returns>
        string ToJson();


        /// <summary>
        /// Return flags status
        /// </summary>
        FSFlagStatus Status { get; }
    }
}
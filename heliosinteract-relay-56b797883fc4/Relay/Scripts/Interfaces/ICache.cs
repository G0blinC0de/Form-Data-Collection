namespace Helios.Relay
{
    using System.Collections.Generic;
    using LiteDB;

    public interface ICache
    {
        /// <summary>
        /// Inserts an entry into the cache.
        /// </summary>
        /// <typeparam name="T">The type of the entry to be insterted.</typeparam>
        /// <param name="cacheEntry">The entry to be inserted.</param>
        void Insert<T>(T entry) where T : CacheEntry;

        /// <summary>
        /// Removes the entry with the given id from the cache.
        /// </summary>
        /// <typeparam name="T">The type of the entry to be removed.</typeparam>
        /// <param name="entryId">The id of the entry to be removed.</param>
        /// <returns>Whether the operation was successful.</returns>
        bool Remove<T>(BsonValue entryId) where T : CacheEntry;

        /// <summary>
        /// Removes failed entries of the specified type from the cache.
        /// </summary>
        /// <typeparam name="T">The type of the entry to be removed.</typeparam>
        /// <param name="minimumNumberOfFailures">The minimum number of failures to qualify an entry for removal.</param>
        /// <returns>The number of entries that were removed.</returns>
        int RemoveFailedEntries<T>(int minimumNumberOfFailures) where T : CacheEntry;

        /// <summary>
        /// Updates an entry in the cache.
        /// </summary>
        /// <typeparam name="T">The type of the entry to be updated.</typeparam>
        /// <param name="cacheEntry">The entry to be updated.</param>
        /// <returns>Whether the operation was successful.</returns>
        bool Update<T>(T entry) where T : CacheEntry;

        /// <summary>
        /// Gets an entry of the specified type from the cache.
        /// </summary>
        /// <typeparam name="T">The type of the entry to be found.</typeparam>
        /// <returns>An entry, if one exists. If no entry exists, returns null.</returns>
        T GetEntry<T>() where T : CacheEntry;

        /// <summary>
        /// Gets all entries of the specified type from the cache.
        /// </summary>
        /// <typeparam name="T">The type of the entries to be returned.</typeparam>
        /// <returns>All entries of the specified type.</returns>
        IEnumerable<T> GetAllEntries<T>() where T : CacheEntry;

        /// <summary>
        /// Gets the number of entries of the specified type in the cache.
        /// </summary>
        /// <typeparam name="T">The type of the entries to be counted.</typeparam>
        /// <returns>The number of entries of the specified type.</returns>
        int Count<T>() where T : CacheEntry;
    }
}
namespace Helios.Relay
{
    using System.Collections.Generic;
    using LiteDB;
    using Microsoft.AspNetCore.Http;

    public interface IRelay
    {
        RelayService ServiceType { get; }

        /// <summary>
        /// Receives a post and returns whether it is valid. If it is, also outputs the CacheEntry.
        /// </summary>
        /// <param name="postData">The post data.</param>
        /// <param name="cacheEntry">The entry which is created during validation.</param>
        /// <returns>Whether the post was successfully validated and a status message.</returns>
        ValidationResult TryValidate(IPostData postData, out CacheEntry cacheEntry);

        /// <summary>
        /// Adds the validated CacheEntry to the cache.
        /// </summary>
        /// <param name="cacheEntry">The entry to add.</param>
        void Add(CacheEntry cacheEntry);

        /// <summary>
        /// Removes the entry with the given id from the cache.
        /// </summary>
        /// <param name="entryId">The id of the entry to be removed.</param>
        /// <returns>Whether the operation was successful.</returns>
        bool Remove(BsonValue entryId);

        /// <summary>
        /// Removes failed entries of this relay's type from the cache.
        /// </summary>
        /// <param name="minimumNumberOfFailures">The minimum number of failures to qualify an entry for removal.</param>
        /// <returns>The number of entries that were removed.</returns>
        int RemoveFailedEntries(int minimumNumberOfFailures);

        /// <summary>
        /// Gets all entries of this relay's type from the cache.
        /// </summary>
        /// <returns>All entries of the specified type.</returns>
        IEnumerable<CacheEntry> GetAllEntries();

        /// <summary>
        /// Log the status of all entries of this relay's type in the cache.
        /// </summary>
        /// <returns>A collection of log messages.</returns>
        IEnumerable<string> GetLogs();

        /// <summary>
        /// Starts the relay routine.
        /// </summary>
        void StartRelay();
    }
}
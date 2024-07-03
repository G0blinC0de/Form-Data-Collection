namespace Helios.Relay
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Encryption;
    using LiteDB;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Represents a collection of cache entries.
    /// </summary>
    public class Cache : ICache
    {
        private readonly LiteDatabase _database;

        public Cache(IConfiguration configuration)
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database");
            Directory.CreateDirectory(path);
            ConsoleLogger.WriteLine($"Database Path: {path}");
            _database = new LiteDatabase(string.Concat("Filename=",
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, StringEncryption.Decrypt(configuration.GetSection("Relay")["DatabaseConnectionString"]))));
        }

        ~Cache()
        {
            _database.Dispose();
        }

        public bool Remove<T>(BsonValue entryId) where T : CacheEntry
        {
            if (_database.GetCollection<T>(typeof(T).Name).Delete(entryId))
            {
                //ConsoleLogger.WriteLine($"Removing entry from collection {typeof(T).Name}.", entryId, typeof(T).Name);
                return true;
            }

            return false;
        }

        public int RemoveFailedEntries<T>(int minimumNumberOfFailures) where T : CacheEntry
        {
            var entries = GetAllEntries<T>();
            var entriesRemoved = 0;
            var cacheEntries = entries.ToList();
            foreach (var entry in cacheEntries)
            {
                if (entry.TotalFailedAttempts >= minimumNumberOfFailures)
                {
                    if (_database.GetCollection<T>(typeof(T).Name).Delete(entry.EntryId))
                    {
                        entriesRemoved++;
                        //ConsoleLogger.WriteLine($"Removing entry from collection {typeof(T).Name}.", entry.EntryId, typeof(T).Name);
                    }
                }
            }

            //ConsoleLogger.WriteLine($"Entries Removed: {entriesRemoved} Removed From Collection: {typeof(T).Name}.");
            return entriesRemoved;
        }

        public T GetEntry<T>() where T : CacheEntry
        {
            T findOne;
            try
            {
                if (_database.GetCollection<T>(typeof(T).Name).Count(x => !x.IsFrozen) <= 0)
                {
                    //ConsoleLogger.WriteLine($"{typeof(T).Name} Count was 0 when attempting to GetEntry, returning null.");
                    return null;
                }

                findOne = _database.GetCollection<T>(typeof(T).Name).FindOne(x => x.ReadyToSend && !x.IsFrozen);
                if (findOne != null)
                {
                    //ConsoleLogger.WriteLine("GetEntry returned an entry from the cache.", findOne.EntryId, typeof(T).Name);
                    return findOne;
                }
            }
            catch (Exception ex)
            {
                ConsoleLogger.WriteLine(ex);
                throw;
            }

            MarkAllEntriesForSend();
            findOne = _database.GetCollection<T>(typeof(T).Name).FindOne(x => x.ReadyToSend && !x.IsFrozen);
            //ConsoleLogger.WriteLine("GetEntry returned an entry from the cache after marking entries for send.", findOne.EntryId, typeof(T).Name);
            return findOne;

            void MarkAllEntriesForSend()
            {
                //ConsoleLogger.WriteLine($"{typeof(T).Name} No entries marked for send. Marking all entries for send.");
                try
                {
                    var allEntries = _database.GetCollection<T>(typeof(T).Name).FindAll();
                    foreach (var reachEntry in allEntries)
                    {
                        reachEntry.ReadyToSend = true;
                        _database.GetCollection<T>(typeof(T).Name).Update(reachEntry.EntryId, reachEntry);
                    }
                }
                catch (Exception ex)
                {
                    ConsoleLogger.WriteLine(ex);
                    throw;
                }
            }
        }

        public IEnumerable<T> GetAllEntries<T>() where T : CacheEntry
        {
            var allEntries = _database.GetCollection<T>(typeof(T).Name).FindAll();
            //ConsoleLogger.WriteLine($"{typeof(T).Name} GetAllEntries EntriesReturned: {allEntries.Count()}");
            return allEntries;
        }

        public int Count<T>() where T : CacheEntry
        {
            var count = _database.GetCollection<T>(typeof(T).Name).Count(x => !x.IsFrozen);
            //ConsoleLogger.WriteLine($"{typeof(T).Name} Count: {count}");
            return count;
        }

        public void Insert<T>(T cacheEntry) where T : CacheEntry
        {
            //ConsoleLogger.WriteLine($"Adding entry to collection '{typeof(T).Name}'.", cacheEntry.EntryId, typeof(T).Name);
            _database.GetCollection<T>(typeof(T).Name).Insert(cacheEntry.EntryId, cacheEntry);
        }

        public bool Update<T>(T cacheEntry) where T : CacheEntry
        {
            //ConsoleLogger.WriteLine($"Updating an entry in collection '{typeof(T).Name}'.", cacheEntry.EntryId, typeof(T).Name);
            return _database.GetCollection<T>(typeof(T).Name).Update(cacheEntry.EntryId, cacheEntry);
        }
    }
}
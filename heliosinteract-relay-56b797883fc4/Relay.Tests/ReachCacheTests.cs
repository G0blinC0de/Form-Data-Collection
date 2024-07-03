namespace Relay.Tests
{
    using LiteDB;
    using System.Threading.Tasks;
    using System;
    using System.Linq;
    using Helios.Relay;
    using NUnit.Framework;

    [TestFixture]
    public class ReachCacheTests
    {
        private readonly ICache reachCache = new Cache("TestDatabase", "TestCollection");

        [TearDown]
        public Task CleanUpAsync()
        {
            return ClearReachCacheAsync();
        }

        [TestCase]
        public void Insert_ValidEntry_DoesInsert()
        {
            reachCache.Insert(new ReachCacheEntry());
            Assert.AreEqual(1, reachCache.Count());
        }

        [TestCase]
        public void Insert_ManyEntries_DoesInsert()
        {
            for (var i = 0; i < 1000; i++)
            {
                reachCache.Insert(new ReachCacheEntry());
            }
            Assert.AreEqual(1000, reachCache.Count());
        }

        [TestCaseSource(typeof(ReachCacheTestsData), "InvalidEntryIds")]
        public void Remove_InvalidIds_DoesNotRemove(object id)
        {
            CreateEntryInReachCache();
            Assert.False(reachCache.Remove(new BsonValue(id)));
        }

        [TestCase]
        public void Remove_ExistentEntry_DoesRemove()
        {
            Assert.True(reachCache.Remove(CreateEntryInReachCache().EntryId));
        }

        [TestCase]
        public void Count_IsReturned_DoesGetCount()
        {
            Assert.True(reachCache.Count() >= 0);
        }

        [TestCase]
        public void Count_GreaterThanZero_IsGreaterThanZero()
        {
            CreateEntryInReachCache();
            Assert.True(reachCache.Count() > 0);
        }

        [TestCase]
        public void Count_ReturnsCorrectCount_CountIsTwo()
        {
            CreateEntryInReachCache();
            CreateEntryInReachCache();
            Assert.AreEqual(2, reachCache.Count());
        }

        [TestCase]
        public void Update_NonExistentEntry_DoesNotUpdate()
        {
            Assert.False(reachCache.Update(new ReachCacheEntry()));
        }

        [TestCase]
        public void Update_ExistentEntry_ShouldUpdate()
        {
            var testCacheEntry = CreateEntryInReachCache();
            reachCache.Update(testCacheEntry);
        }

        [TestCase]
        public void GetEntry_DatabaseIsEmpty_DoesNotGetEntry()
        {
            Assert.Null(reachCache.GetEntry());
        }

        [TestCase]
        public void GetEntry_DatabaseHasEntries_DoesGetEntry()
        {
            CreateEntryInReachCache();
            Assert.NotNull(reachCache.GetEntry());
        }

        [TestCase]
        public void GetEntry_TypeIsReachCacheEntry_IsCorrectType()
        {
            CreateEntryInReachCache();
            Assert.IsInstanceOf<ReachCacheEntry>(reachCache.GetEntry());
        }

        [TestCase]
        public void GetAllEntriesAsync_ReturnsCollection_DoesReturn()
        {
            Assert.NotNull(reachCache.GetAllEntriesAsync());
        }

        [TestCase]
        public async Task GetAllEntriesAsync_CollectionIsEmpty_IsEmpty()
        {
            var enumerable = await reachCache.GetAllEntriesAsync().ConfigureAwait(false);
            var count = enumerable.Count();
            Assert.AreEqual(0, count);
        }

        [TestCase]
        public async Task GetAllEntriesAsync_CollectionAllHasEntries_IsAllEntries()
        {
            for (var i = 0; i < 5; i++)
            {
                CreateEntryInReachCache();
            }
            var enumerable = await reachCache.GetAllEntriesAsync().ConfigureAwait(false);
            var count = enumerable.Count();
            Assert.AreEqual(5, count);
        }

        [TestCase]
        public async Task RemoveCountAsync_NoFailedEntries_RemovesNoEntries()
        {
            var entriesRemoved = await reachCache.RemoveFailedEntriesAsync(10);
            Assert.AreEqual(0, entriesRemoved);
        }

        [TestCase]
        public async Task RemoveCountAsync_HasFailedEntries_RemovesFailedEntries()
        {
            CreateEntryInReachCache();
            var failedEntryOne = CreateEntryInReachCache();
            var failedEntryTwo = CreateEntryInReachCache();
            failedEntryOne.TotalFailedAttempts = 10;
            failedEntryTwo.TotalFailedAttempts = 11;
            reachCache.Update(failedEntryOne);
            reachCache.Update(failedEntryTwo);
            var entriesRemoved = await reachCache.RemoveFailedEntriesAsync(10);
            Assert.True(entriesRemoved == 2);
        }

        [TestCase]
        public void OnEntryAdded_EntryAdded_DoesCallback()
        {
            reachCache.OnEntryAdded += delegate { Assert.NotNull(reachCache.GetEntry()); };
            reachCache.Insert(new ReachCacheEntry());
        }

        private ReachCacheEntry CreateEntryInReachCache()
        {
            var testCacheEntry = new ReachCacheEntry();
            reachCache.Insert(testCacheEntry);
            return testCacheEntry;
        }

        private Task ClearReachCacheAsync()
        {
            return reachCache.RemoveFailedEntriesAsync(0);
        }
    }
}
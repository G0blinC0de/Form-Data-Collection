namespace Relay.Tests
{
    public class ReachCacheTestsData
    {
        public static object[] InvalidEntryIds =
        {
            new object[] { 1.1 },
            new object[] { 0 },
            new object[] { -1000000000000 },
            new object[] { 1000000000000 },
            new object[] { "-%^&*$" },
            new object[] { "1234567890" }
        };
    }
}

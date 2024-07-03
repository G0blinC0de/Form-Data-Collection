namespace Helios.Relay.Reach
{
    public class ReachCacheEntryFactory : ICacheEntryFactory<ReachCacheEntry>
    {
        public ValidationResult CreateEntryFromPostData(IPostData postData, out ReachCacheEntry cacheEntry)
        {
            cacheEntry = null;

            var (checkPassed, checkResponse) = CheckHasData();
            if (!checkPassed) return new ValidationResult(false, checkResponse);

            (checkPassed, checkResponse) = CheckHasActivationId();
            if (!checkPassed) return new ValidationResult(false, checkResponse);

            EnsureExperienceType();
            return new ValidationResult(true, "Received valid ReachCacheEntry.");

            void EnsureExperienceType()
            {
                if (string.IsNullOrWhiteSpace(postData.Experience.type))
                {
                    ConsoleLogger.WriteLine("Setting experience type to 'Relay-default'. Was null or empty.");
                    postData.Experience.type = "Relay-default";
                }
            }

            (bool checkPasses, string response) CheckHasData()
            {
                return postData.Experience == null ? (false, "No Experience info received.") : (true, string.Empty);
            }

            (bool checkPasses, string response) CheckHasActivationId()
            {
                return string.IsNullOrEmpty(postData.Experience.activationId) ? (false, "No activation id sent with experience.") : (true, string.Empty);
            }
        }
    }
}

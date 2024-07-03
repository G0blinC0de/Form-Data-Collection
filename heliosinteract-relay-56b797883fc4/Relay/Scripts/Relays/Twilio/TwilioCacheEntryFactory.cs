using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Helios.Relay.Twilio
{
    public class TwilioCacheEntryFactory : ICacheEntryFactory<TwilioCacheEntry>
    {
        public ValidationResult CreateEntryFromPostData(IPostData postData, out TwilioCacheEntry cacheEntry)
        {
            cacheEntry = null;

            var (checkPassed, checkResponse) = CheckHasData();
            if (!checkPassed) return new ValidationResult(false, checkResponse);

            (checkPassed, checkResponse) = CheckHasActivationId();
            if (!checkPassed) return new ValidationResult(false, checkResponse);

            (checkPassed, checkResponse) = CheckHasKey();
            if (!checkPassed) return new ValidationResult(false, checkResponse);

            (checkPassed, checkResponse) = CheckHasPhone();
            if (!checkPassed) return new ValidationResult(false, checkResponse);

            var pendingPhoneList = SetupPhones();
            EnsureExperienceType();

            cacheEntry = new TwilioCacheEntry(postData, pendingPhoneList);
            return new ValidationResult(true, "Received valid TwilioCacheEntry.");

            void EnsureExperienceType()
            {
                if (string.IsNullOrWhiteSpace(postData.Experience.type))
                {
                    ConsoleLogger.WriteLine("Setting experience type to 'Relay-twilio-default'. Was null or empty.");
                    postData.Experience.type = "Relay-twilio-default";
                }
            }

            (bool checkPasses, string response) CheckHasKey()
            {
                return string.IsNullOrWhiteSpace(postData.Key)
                    ? (false, "No Reach key received with request.")
                    : (true, string.Empty);
            }

            (bool checkPasses, string response) CheckHasPhone()
            {
                return postData.Guest.phones.Length < 1 || string.IsNullOrWhiteSpace(postData.Guest.phones[0])
                    ? (false, "Invalid Twilio phone number received.")
                    : (true, string.Empty);
            }

            (bool checkPasses, string response) CheckHasData()
            {
                return postData.Experience == null ? (false, "No Experience info received.") : (true, string.Empty);
            }

            (bool checkPasses, string response) CheckHasActivationId()
            {
                return string.IsNullOrEmpty(postData.Experience.activationId) && string.IsNullOrWhiteSpace(postData.FileInfo.activationId) ?
                    (false, "No activation id sent with experience.") :
                    (true, string.Empty);
            }

            List<string> SetupPhones()
            {
                var pendingPhoneList = new List<string>();
                foreach (string phone in postData.Guest.phones)
                {
                    pendingPhoneList.Add(Regex.Replace(phone, "[^0-9]", ""));
                }
                return pendingPhoneList;
            }
        }
    }
}

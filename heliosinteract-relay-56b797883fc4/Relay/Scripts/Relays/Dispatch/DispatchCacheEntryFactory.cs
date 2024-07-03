namespace Helios.Relay.Dispatch
{
    public class DispatchCacheEntryFactory : ICacheEntryFactory<DispatchCacheEntry>
    {
        public ValidationResult CreateEntryFromPostData(IPostData postData, out DispatchCacheEntry cacheEntry)
        {
            cacheEntry = null;

            // Invalid if no guest data
            if (postData.Guest == null) return new ValidationResult(false, "No Guest info received.");

            // Invalid if no file
            if (postData.FileInfo == null) return new ValidationResult(false, "No file received");

            // Get contact info
            var name = postData.Guest.name;
            var email = postData.Guest.email;
            string phone = null;
            if (postData.Guest.phones?.Length > 0) phone = postData.Guest.phones[0];
            if (postData.Guest.phones?.Length == 0) postData.Guest.phones = null;

            // Invalid if no phone or email
            if (string.IsNullOrWhiteSpace(email) && string.IsNullOrWhiteSpace(phone)) return new ValidationResult(false, "No Guest email or phone received.");

            // Ensure supported file type
            switch (postData.FileInfo.mimeType)
            {
                case "image/png":
                case "image/jpeg":
                case "image/gif":
                case "video/mp4":
                    break;

                default:
                    return new ValidationResult(false, $"Unsupported file type: {postData.FileInfo.mimeType}");
            }

            // Create the cache entry
            cacheEntry = new DispatchCacheEntry(name, email, phone, postData.FileInfo);

            // Success
            return new ValidationResult(true, string.Empty);
        }
    }
}

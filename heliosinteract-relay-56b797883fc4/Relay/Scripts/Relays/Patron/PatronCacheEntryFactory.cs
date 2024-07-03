namespace Helios.Relay.Patron
{
    public class PatronCacheEntryFactory : ICacheEntryFactory<PatronCacheEntry>
    {
        public ValidationResult CreateEntryFromPostData(IPostData postData, out PatronCacheEntry cacheEntry)
        {
            cacheEntry = null;

            // Confirm Patron ID is present
            var guestId = (postData.Guest?.meta.TryGetValue("patronId", out var id) ?? false) ? (string)id : null;
            if (guestId == null) guestId = (postData.Experience?.meta.TryGetValue("patronId", out id) ?? false) ? (string)id : null;
            if (guestId == null) return new ValidationResult(false, "No Patron ID received.");

            // Validate post type
            PatronCacheEntry.PostType type;
            if (postData.FileInfo != null)
            {
                switch (postData.FileInfo.mimeType)
                {
                    case "image/jpeg":
                    case "image/png":
                    case "image/tiff":
                    case "image/gif":
                        type = PatronCacheEntry.PostType.Photo;
                        break;

                    case "video/mp4":
                        type = PatronCacheEntry.PostType.Video;
                        break;

                    default:
                        return new ValidationResult(false, $"Unsupported media type: {postData.FileInfo.mimeType}");
                }
            }
            else
            {
                return new ValidationResult(false, $"Post is a check-in or survey, which is not implemented.");
            }

            cacheEntry = new PatronCacheEntry(guestId, type, postData.FileInfo);
            return new ValidationResult(true, "Received valid PatronCacheEntry.");
        }
    }
}

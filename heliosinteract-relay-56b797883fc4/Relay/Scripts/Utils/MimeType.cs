namespace Helios.Relay
{
    using System.Collections.Generic;
    using System.IO;

    public static class MimeType
    {
        public static string GetMimeType(string filePathOrName)
        {
            if (string.IsNullOrEmpty(filePathOrName))
            {
                return null;
            }

            return SupportedMimes.TryGetValue(Path.GetExtension(filePathOrName).ToLower(), out var mime) ? mime : null;
        }

        public static string GetExtension(string mimeType)
        {
            if (string.IsNullOrEmpty(mimeType))
            {
                return null;
            }

            return SupportedMimes.TryGetValue(mimeType.ToLower(), out var extension) ? extension : null;
        }

        public static readonly Dictionary<string, string> SupportedMimes = new Dictionary<string, string>
        {
            {".png", "image/png"},
            {".jpg", "image/jpeg"},
            {".jpeg", "image/jpeg"},
            {".gif", "image/gif"},
            {".mp4", "video/mp4"},
            {"image/png", ".png"},
            {"image/jpeg", ".jpg"},
            {"image/gif", ".gif"},
            {"video/mp4", ".mp4"}
        };
    }
}
namespace Helios.Relay
{
    using System;
    using System.Text.RegularExpressions;

    public static class UniqueId
    {
        public static string Create()
        {
            return Regex.Replace(Convert.ToBase64String(Guid.NewGuid().ToByteArray()), "[/+=]", "");
        }
    }
}
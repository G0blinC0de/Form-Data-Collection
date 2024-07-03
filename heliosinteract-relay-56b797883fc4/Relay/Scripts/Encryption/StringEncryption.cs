namespace Helios.Encryption
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;

    public static class StringEncryption
    {
        private static readonly byte[] key = Convert.FromBase64String("lfuS/xg9+H/wCOnl1XetC+6AdAgf/rN+");

        private static readonly Random random = new Random();

        private static readonly RijndaelManaged rijndaelManaged = new RijndaelManaged();

        private static readonly UTF8Encoding encoder = new UTF8Encoding();

        public static string Encrypt(string unencrypted)
        {
            var vector = new byte[16];
            random.NextBytes(vector);
            var cryptogram = vector.Concat(Encrypt(encoder.GetBytes(unencrypted), vector));
            return Convert.ToBase64String(cryptogram.ToArray());
        }

        public static string Decrypt(string encrypted)
        {
            var cryptogram = Convert.FromBase64String(encrypted);
            if (cryptogram.Length < 17)
            {
                throw new ArgumentException($"Not a valid encrypted string{encrypted}");
            }

            var vector = cryptogram.Take(16).ToArray();
            var buffer = cryptogram.Skip(16).ToArray();
            return encoder.GetString(Decrypt(buffer, vector));
        }

        private static byte[] Encrypt(byte[] buffer, byte[] vector)
        {
            var encryptor = rijndaelManaged.CreateEncryptor(key, vector);
            return Transform(buffer, encryptor);
        }

        private static byte[] Decrypt(byte[] buffer, byte[] vector)
        {
            var decryptor = rijndaelManaged.CreateDecryptor(key, vector);
            return Transform(buffer, decryptor);
        }

        private static byte[] Transform(byte[] buffer, ICryptoTransform transform)
        {
            var stream = new MemoryStream();
            using (var cs = new CryptoStream(stream, transform, CryptoStreamMode.Write))
            {
                cs.Write(buffer, 0, buffer.Length);
            }

            return stream.ToArray();
        }
    }
}
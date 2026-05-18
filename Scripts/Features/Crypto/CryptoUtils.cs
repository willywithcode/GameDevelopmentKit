namespace GameFoundation.Scripts.Features.Crypto
{
    using System;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;

    public static class CryptoUtils
    {
        private static readonly byte[] KeyPart1 =
        {
            0x4A, 0x8F, 0xC2, 0x31, 0x7E, 0xB5, 0x69, 0xD4,
            0x23, 0xF8, 0xA1, 0x5C, 0x90, 0x3B, 0xE7, 0x62
        };

        private static readonly byte[] KeyPart2 =
        {
            0x1D, 0x86, 0x4F, 0xC3, 0xA9, 0x52, 0x7B, 0xE0,
            0x35, 0x9C, 0x6A, 0xD8, 0x14, 0xF7, 0x2B, 0x58
        };

        private static byte[] BuildKey()
        {
            var key = new byte[32];
            Array.Copy(KeyPart1, 0, key, 0, 16);
            Array.Copy(KeyPart2, 0, key, 16, 16);
            return key;
        }

        public static byte[] Encrypt(string plaintext)
        {
            using var aes = Aes.Create();
            aes.Key     = BuildKey();
            aes.Mode    = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.GenerateIV();

            using var ms = new MemoryStream();
            ms.Write(aes.IV, 0, aes.IV.Length);
            using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
            using (var sw = new StreamWriter(cs, new UTF8Encoding(false)))
            {
                sw.Write(plaintext);
            }

            return ms.ToArray();
        }

        public static string Decrypt(byte[] cipherBytes)
        {
            if (cipherBytes == null || cipherBytes.Length <= 16)
                throw new ArgumentException("Invalid encrypted data.");

            using var aes = Aes.Create();
            aes.Key     = BuildKey();
            aes.Mode    = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            var iv = new byte[16];
            Array.Copy(cipherBytes, 0, iv, 0, 16);
            aes.IV = iv;

            using var ms = new MemoryStream(cipherBytes, 16, cipherBytes.Length - 16);
            using var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read);
            using var sr = new StreamReader(cs, new UTF8Encoding(false));
            return sr.ReadToEnd();
        }
    }
}

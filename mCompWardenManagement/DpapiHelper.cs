using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace mCompWardenManagement
{
    public static class DpapiHelper
    {
        private static readonly byte[] Entropy = Encoding.UTF8.GetBytes("mCompWardenV3SecretEntropy");
        private static readonly byte[] AesKey = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes("mCompWardenSharedSecretKey2026!"));
        private static readonly byte[] AesIV = { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF, 0xfe, 0xdc, 0xba, 0x09, 0x87, 0x65, 0x43, 0x21 };

        public static string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText)) return "";
            try
            {
                using (var aes = Aes.Create())
                {
                    aes.Key = AesKey;
                    aes.IV = AesIV;
                    using (var ms = new MemoryStream())
                    {
                        using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                        {
                            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
                            cs.Write(plainBytes, 0, plainBytes.Length);
                            cs.FlushFinalBlock();
                        }
                        return "aes:" + Convert.ToBase64String(ms.ToArray());
                    }
                }
            }
            catch
            {
                byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
                byte[] cipherBytes = ProtectedData.Protect(plainBytes, Entropy, DataProtectionScope.LocalMachine);
                return Convert.ToBase64String(cipherBytes);
            }
        }

        public static string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText)) return "";

            if (cipherText.StartsWith("aes:", StringComparison.OrdinalIgnoreCase))
            {
                string base64 = cipherText.Substring(4);
                return DecryptAes(base64);
            }

            string aesDec = DecryptAes(cipherText);
            if (!string.IsNullOrEmpty(aesDec)) return aesDec;

            try
            {
                byte[] cipherBytes = Convert.FromBase64String(cipherText);
                byte[] plainBytes = ProtectedData.Unprotect(cipherBytes, Entropy, DataProtectionScope.LocalMachine);
                return Encoding.UTF8.GetString(plainBytes);
            }
            catch { }

            try
            {
                byte[] cipherBytes = Convert.FromBase64String(cipherText);
                byte[] plainBytes = ProtectedData.Unprotect(cipherBytes, null, DataProtectionScope.LocalMachine);
                return Encoding.UTF8.GetString(plainBytes);
            }
            catch { }

            return "";
        }

        private static string DecryptAes(string cipherBase64)
        {
            try
            {
                byte[] cipherBytes = Convert.FromBase64String(cipherBase64);
                using (var aes = Aes.Create())
                {
                    aes.Key = AesKey;
                    aes.IV = AesIV;
                    using (var ms = new MemoryStream())
                    {
                        using (var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(cipherBytes, 0, cipherBytes.Length);
                            cs.FlushFinalBlock();
                        }
                        return Encoding.UTF8.GetString(ms.ToArray());
                    }
                }
            }
            catch
            {
                return "";
            }
        }
    }
}

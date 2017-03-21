using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace EventSourcing.Poc.Processing.Commons {
    public static class CryptographyExtensions {
        public static string Encrypt(this string plainText, string key, string iv) {
            if (iv.Length != 16) {
                throw new ArgumentOutOfRangeException(nameof(iv));
            }
            if (key.Length != 32) {
                throw new ArgumentOutOfRangeException(nameof(key));
            }
            byte[] encrypted;
            using (var aes = Aes.Create()) {
 //               aes.BlockSize = 128;
 //               aes.KeySize = 256;
                aes.IV = Encoding.UTF8.GetBytes(iv);
                aes.Key = Encoding.UTF8.GetBytes(key);
 //               aes.Mode = CipherMode.CBC;
 //               aes.Padding = PaddingMode.PKCS7;
                using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV)) {
                    using (var msEncrypt = new MemoryStream()) {
                        using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write)) {
                            using (var swEncrypt = new StreamWriter(csEncrypt)) {
                                swEncrypt.Write(plainText);
                            }
                            encrypted = msEncrypt.ToArray();
                        }
                    }
                }
            }
            return Convert.ToBase64String(encrypted);
        }

        public static string Decrypt(this string cipherText, string key, string iv) {
            if (iv.Length != 16) {
                throw new ArgumentOutOfRangeException(nameof(iv));
            }
            if (key.Length != 32) {
                throw new ArgumentOutOfRangeException(nameof(key));
            }
            var aes = Aes.Create();
//            aes.BlockSize = 128;
//            aes.KeySize = 256;
            aes.IV = Encoding.UTF8.GetBytes(iv);
            aes.Key = Encoding.UTF8.GetBytes(key);
//            aes.Mode = CipherMode.CBC;
//            aes.Padding = PaddingMode.PKCS7;
            string plainText;
            using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV)) {
                using (var msDecrypt = new MemoryStream(Convert.FromBase64String(cipherText))) {
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read)) {
                        using (var srDecrypt = new StreamReader(csDecrypt)) {
                            plainText = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
            return plainText;
        }

        public static string GenerateString(int size) {
            var random = new Random((int) DateTime.Now.Ticks);
            var sb = new StringBuilder();
            for (var i = 0; i < size; i++) {
                sb.Append(Convert.ToChar(random.Next(28, 126)));
            }
            return sb.ToString();
        }
    }
}
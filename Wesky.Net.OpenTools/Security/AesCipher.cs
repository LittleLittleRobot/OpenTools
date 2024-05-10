using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Wesky.Net.OpenTools.Converter;

namespace Wesky.Net.OpenTools.Security
{
    internal class AesCipher
    {
        /// <summary>
        /// 使用AES加密算法加密文本。
        /// Encrypts the text using AES encryption algorithm.
        /// </summary>
        /// <param name="key">加密密钥，必须是32字符长。/ Encryption key, must be 32 characters long.</param>
        /// <param name="password">要加密的文本。/ The text to be encrypted.</param>
        /// <param name="iv">初始化向量，必须是16字符长。/ Initialization vector, must be 16 characters long.</param>
        /// <returns>加密后的十六进制字符串。/ Encrypted text in hexadecimal string format.</returns>
        public static string AesEncrypt(string key, string password, string iv)
        {
            if (key == null || key.Length < 32)
                throw new ArgumentException("Key must be at least 32 characters long.", nameof(key));
            if (iv == null || iv.Length < 16)
                throw new ArgumentException("IV must be at least 16 characters long.", nameof(iv));

            byte[] keyBytes = Encoding.UTF8.GetBytes(key.Substring(0, 32));
            byte[] ivBytes = Encoding.UTF8.GetBytes(iv.Substring(0, 16));

            using (AesCryptoServiceProvider aesAlg = new AesCryptoServiceProvider())
            {
                aesAlg.Key = keyBytes;
                aesAlg.IV = ivBytes;

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(password);
                        }
                        return ByteConvert.ByteArrayToHexString(msEncrypt.ToArray());
                    }
                }
            }
        }

        /// <summary>
        /// 使用AES解密算法解密文本。
        /// Decrypts the text using the AES decryption algorithm.
        /// </summary>
        /// <param name="key">解密密钥，必须是32字符长。/ Decryption key, must be 32 characters long.</param>
        /// <param name="encryptedText">要解密的文本，以十六进制字符串格式。/ The text to be decrypted, in hexadecimal string format.</param>
        /// <param name="iv">初始化向量，必须是16字符长。/ Initialization vector, must be 16 characters long.</param>
        /// <returns>解密后的字符串。/ Decrypted string.</returns>
        public static string AESDecrypt(string key, string encryptedText, string iv)
        {
            if (key == null || key.Length < 32)
                throw new ArgumentException("Key must be at least 32 characters long.", nameof(key));
            if (iv == null || iv.Length < 16)
                throw new ArgumentException("IV must be at least 16 characters long.", nameof(iv));

            byte[] inputBytes = ByteConvert.HexStringToByteArray(encryptedText);
            byte[] keyBytes = Encoding.UTF8.GetBytes(key.Substring(0, 32));
            byte[] ivBytes = Encoding.UTF8.GetBytes(iv.Substring(0, 16));

            using (AesCryptoServiceProvider aesAlg = new AesCryptoServiceProvider())
            {
                aesAlg.Key = keyBytes;
                aesAlg.IV = ivBytes;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                using (MemoryStream msDecrypt = new MemoryStream(inputBytes))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            return srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
        }

    }
}

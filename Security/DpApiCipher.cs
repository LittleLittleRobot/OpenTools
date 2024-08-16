/// <summary>
///********************************************
/// Author ： Wesky
/// CreateTime ： 2024/8/16 10:51:40
/// Description ： 微软的Data Protection API 进行加密解密
///********************************************
/// </summary>
using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
namespace Wesky.Net.OpenTools.Security
{
    public class DpApiCipher
    {

        // 可以调整这里的熵值以匹配原始加密过程中使用的熵
        private static readonly byte[] additionalEntropy = null;

        /// <summary>
        /// 加密数据
        /// </summary>
        /// <param name="dataToEncrypt"></param>
        /// <returns></returns>
        public static string EncryptData(string dataToEncrypt)
        {
            try
            {
                byte[] secret = Encoding.Unicode.GetBytes(dataToEncrypt);
                byte[] encryptedSecret = ProtectedData.Protect(secret, additionalEntropy, DataProtectionScope.LocalMachine);
                string res = string.Empty;
                foreach (byte b in encryptedSecret)
                {
                    res += b.ToString("X2");
                }
                return res;

            }
            catch (Exception ex)
            {
                Console.WriteLine("加密过程中出现异常: " + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// 解密数据
        /// </summary>
        /// <param name="dataToDecrypt"></param>
        /// <returns></returns>
        public static string DecryptData(string hexEncryptedData)
        {
            try
            {
                byte[] dataToDecrypt = ConvertHexStringToByteArray(hexEncryptedData);
                byte[] decryptedData = ProtectedData.Unprotect(dataToDecrypt, null, DataProtectionScope.LocalMachine);
                return Encoding.Default.GetString(decryptedData);
            }
            catch (Exception ex)
            {
                Console.WriteLine("解密过程中出现异常: " + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// 将十六进制字符串转换为字节数组
        /// </summary>
        /// <param name="hexString"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private static byte[] ConvertHexStringToByteArray(string hexString)
        {
            if (hexString.Length % 2 != 0)
            {
                throw new ArgumentException("输入必须为有效的十六进制字符串，其长度为偶数。");
            }

            byte[] bytes = new byte[hexString.Length / 2];
            for (int i = 0; i < hexString.Length; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hexString.Substring(i, 2), 16);
            }
            return bytes;
        }


    }
}

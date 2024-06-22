using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace Wesky.Net.OpenTools.Security
{
    public class HmacSHA256Cipher
    {
        /// <summary>
        /// 钉钉机器人签名计算加密
        /// </summary>
        /// <param name="secret">密钥</param>
        /// <param name="timestamp">时间戳</param>
        /// <returns></returns>
        public static string HmacSHA256ForDingRobot(string secret, long timestamp)
        {
            string encodedSign = string.Empty;
            string stringToSign = timestamp + "\n" + secret;
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret)))
            {
                byte[] signData = hmac.ComputeHash(Encoding.UTF8.GetBytes(stringToSign));
                string sign = Convert.ToBase64String(signData);
                encodedSign = HttpUtility.UrlEncode(sign);
            }
            return encodedSign;
        }
    }
}

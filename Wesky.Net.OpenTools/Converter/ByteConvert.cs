using System;
using System.Collections.Generic;
using System.Text;

namespace Wesky.Net.OpenTools.Converter
{
    /// <summary>
    /// Byte数据处理
    /// Byte Data Processing
    /// </summary>
    public class ByteConvert
    {
        /// <summary>
        /// 16进制字符串转byte[]数组
        /// Convert hexadecimal string to byte array
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static byte[] HexStringToByteArray(string str)
        {
            str = str.Replace(" ", "");
            byte[] buffer = new byte[str.Length / 2];
            for (int i = 0; i < str.Length; i += 2)
                buffer[i / 2] = (byte)Convert.ToByte(str.Substring(i, 2), 16);
            return buffer;
        }

        /// <summary>
        /// byte[]数组转16进制字符串
        /// Convert byte array to hexadecimal string
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string ByteArrayToHexString(byte[] data)
        {
            if (data == null || data.Length == 0)
            {
                return string.Empty;
            }
            StringBuilder sb = new StringBuilder(data.Length * 2); 
            foreach (byte b in data)
            {
                sb.AppendFormat("{0:X2}", b);
            }
            return sb.ToString();
        }
    }
}

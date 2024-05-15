using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Runtime.InteropServices;

namespace Wesky.Net.OpenTools.NetworkExtensions
{
    /// <summary>
    /// Ntp时间客户端。
    /// </summary>
    public class NtpClient
    {
        private const int NtpDataLength = 48; // NTP消息长度 | NTP message length

        /// <summary>
        /// 获取NTP服务器的时间。
        /// Retrieves the time from an NTP server.
        /// </summary>
        /// <param name="ntpServer">NTP服务器地址 | NTP server address</param>'
        /// <param name="ntpPort">NTP服务的端口 | NTP service port</param>
        /// <returns>服务器时间 | Server time</returns>
        public static DateTime GetNtpServerTime(string ntpServer,int ntpPort=123)
        {
            // 初始化NTP数据缓冲区
            // Initialize NTP data buffer
            byte[] ntpData = new byte[NtpDataLength];
            ntpData[0] = 0x1B; // NTP version number (3) and mode (3), client request

            var addresses = Dns.GetHostAddresses(ntpServer);
            IPEndPoint ipEndPoint = new IPEndPoint(addresses[0], ntpPort);

            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                socket.Connect(ipEndPoint);
                socket.Send(ntpData);
                socket.Receive(ntpData);
            }

            // 从字节40和44提取时间戳
            // Extract timestamp from bytes 40 and 44
            ulong intPart = BitConverter.ToUInt32(ntpData, 40);
            ulong fractPart = BitConverter.ToUInt32(ntpData, 44);

            // 转换字节序为小端格式
            // Convert byte order to little endian
            intPart = SwapEndianness(intPart);
            fractPart = SwapEndianness(fractPart);

            var milliseconds = (intPart * 1000) + ((fractPart * 1000) / 0x100000000L);

            // NTP时间是从1900年开始计算的，这里将其转换为UTC时间
            // NTP time starts from 1900, this converts it to UTC DateTime
            DateTime networkDateTime = new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds((long)milliseconds);

            return networkDateTime.ToLocalTime();
        }

        /// <summary>
        /// 转换大端和小端字节序。
        /// Converts big endian to little endian and vice versa.
        /// </summary>
        /// <param name="x">待转换的数值 | The number to convert</param>
        /// <returns>转换后的数值 | Converted number</returns>
        private static uint SwapEndianness(ulong x)
        {
            return (uint)(((x & 0x000000ff) << 24) +
                          ((x & 0x0000ff00) << 8) +
                          ((x & 0x00ff0000) >> 8) +
                          ((x & 0xff000000) >> 24));
        }

        /// <summary>
        /// 设置系统时间。
        /// Sets the system time.
        /// </summary>
        /// <param name="newTime">新的时间 | New time to set</param>
        public static void SetSystemTime(DateTime newTime)
        {
            SystemTime st = new SystemTime
            {
                wYear = (ushort)newTime.Year,
                wMonth = (ushort)newTime.Month,
                wDay = (ushort)newTime.Day,
                wHour = (ushort)newTime.Hour,
                wMinute = (ushort)newTime.Minute,
                wSecond = (ushort)newTime.Second,
                wMilliseconds = (ushort)newTime.Millisecond
            };

            SetLocalTime(ref st);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SystemTime
        {
            public ushort wYear;
            public ushort wMonth;
            public ushort wDayOfWeek;
            public ushort wDay;
            public ushort wHour;
            public ushort wMinute;
            public ushort wSecond;
            public ushort wMilliseconds;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetLocalTime(ref SystemTime time);
    }
}

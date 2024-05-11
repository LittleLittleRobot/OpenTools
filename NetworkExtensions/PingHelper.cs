using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using Wesky.Net.OpenTools.NetworkExtensions.ExtensionModel;

namespace Wesky.Net.OpenTools.NetworkExtensions
{
    public class PingHelper
    {
        /// <summary>
        /// 对指定主机执行 ping 操作并返回结果
        /// Ping the specified host and return the result
        /// </summary>
        /// <param name="host">需要被 ping 的主机或 IP 地址 The hostname or IP address to ping</param>
        /// <param name="timeout">ping 超时时间，以毫秒为单位 Timeout duration in milliseconds for ping</param>
        /// <returns>包含 ping 操作结果的 PingResultInfo 对象 A PingResultInfo object containing the result of the ping operation</returns>
        public static PingResultInfo PingHost(string host, int timeout)
        {
            try
            {
                // 解析域名获取 IP 地址
                // Resolve the domain name to get IP address
                IPAddress[] addresses = Dns.GetHostAddresses(host);
                if (addresses.Length == 0)
                {
                    return new PingResultInfo
                    {
                        Host = null,
                        Result = false,
                        Message = "No IP addresses resolved"
                    };
                }
                using (Ping pingSender = new Ping())
                {
                    PingOptions options = new PingOptions
                    {
                        // 设置防止数据包被分片
                        DontFragment = true // Prevent packet fragmentation
                    };

                    // 数据缓冲区，包含要发送的字符串数据
                    // Data buffer containing the string data to send
                    string data = "ABCDEFGHIJKLMNOPQRSTUVWXYZ012345";
                    byte[] buffer = Encoding.ASCII.GetBytes(data);

                    // 使用第一个解析的 IP 地址进行 ping 操作
                    // Use the first resolved IP address to perform the ping
                    IPAddress targetIP = addresses[0];

                    // 发送 ping 请求并获取回复
                    // Send the ping request and obtain the reply
                    PingReply reply = pingSender.Send(targetIP, timeout, buffer, options);

                    // 创建并返回包含 ping 操作结果的 PingResultInfo 对象
                    // Create and return a PingResultInfo object containing the ping result
                    return new PingResultInfo
                    {
                        Host = targetIP,
                        Result = reply.Status == IPStatus.Success,
                        Message = reply.Status == IPStatus.Success
                            ? $"Success: RoundTrip time={reply.RoundtripTime}ms; TTL={reply.Options.Ttl}; Data size={buffer.Length} bytes"
                            : $"Failed: Status={reply.Status}",
                        RoundTripTime = reply.Status == IPStatus.Success ? reply.RoundtripTime : -1,
                        Ttl = reply.Status == IPStatus.Success ? reply.Options.Ttl : -1,
                        DataSize = buffer.Length
                    };
                }
            }
            catch (Exception e)
            {
                // 捕获异常并返回错误信息
                // Catch any exceptions and return error information
                return new PingResultInfo
                {
                    Host = null,
                    Result = false,
                    Message = $"错误: {e.Message} Error: {e.Message}"
                };
            }
        }
    }
}

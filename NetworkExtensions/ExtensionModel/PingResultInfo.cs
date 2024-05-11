using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Wesky.Net.OpenTools.NetworkExtensions.ExtensionModel
{
    /// <summary>
    /// The result of a ping operation.
    ///  ping 操作的结果。
    /// </summary>
    public class PingResultInfo
    {
        /// <summary>
        /// Gets or sets the host address that was pinged.
        /// 获取或设置被 ping 的主机地址。
        /// </summary>
        public IPAddress Host { get; set; }

        /// <summary>
        /// Gets or sets the result of the ping operation (true if success, false otherwise).
        /// 获取或设置 ping 操作的结果（如果成功则为 true，否则为 false）。
        /// </summary>
        public bool Result { get; set; }

        /// <summary>
        /// Gets or sets a message describing the outcome of the ping operation.
        /// 获取或设置描述 ping 操作结果的消息。
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the round-trip time for the ping message, in milliseconds.
        /// 获取或设置 ping 消息的往返时间，单位为毫秒。
        /// </summary>
        public long RoundTripTime { get; set; }

        /// <summary>
        /// Gets or sets the Time-to-Live (TTL) value for the packet sent.
        /// 获取或设置发送的数据包的生存时间（TTL）值。
        /// </summary>
        public int Ttl { get; set; }

        /// <summary>
        /// Gets or sets the size of the data sent to the host, in bytes.
        /// 获取或设置发送到主机的数据大小，单位为字节。
        /// </summary>
        public int DataSize { get; set; }
    }

}

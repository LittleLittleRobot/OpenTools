using System.Net;
using System.Net.Sockets;

namespace Wesky.Net.OpenTools.Iot.Scanner.Models
{
    /// <summary>
    /// Represents the client configuration for a scanner.
    /// 表示扫描器的客户端配置。
    /// </summary>
    public class ReaderClientInfo
    {
        /// <summary>
        /// The IP address of the scanner.
        /// 扫描器的IP地址。
        /// </summary>
        public IPAddress Ip { get; set; }

        /// <summary>
        /// The port number for the scanner connection.
        /// 扫描器连接的端口号。
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Number of retry attempts if no code is scanned.
        /// 如果没有扫描到码的重试次数。
        /// </summary>
        public short Count { get; set; }

        /// <summary>
        /// The socket connection to the scanner.
        /// 扫描器的Socket连接。
        /// </summary>
        public Socket Client { get; set; }

        /// <summary>
        /// The identifier number of the scanner.
        /// 扫描器的编号。
        /// </summary>
        public ushort ReaderNo { get; set; }

        /// <summary>
        /// Timeout in milliseconds for sending requests.
        /// 发送请求的超时时间（毫秒）。
        /// </summary>
        public int SendTimeOut { get; set; } = 3000;

        /// <summary>
        /// Timeout in milliseconds for receiving responses.
        /// 接收响应的超时时间（毫秒）。
        /// </summary>
        public int ReceiveTimeOut { get; set; } = 3000;

        /// <summary>
        /// The brand of the scanner, such as Keyence, Cognex, OPT, etc.
        /// 扫描器的品牌，例如基恩士、康耐视、OPT等等。
        /// </summary>
        public string Brand { get; set; }

        /// <summary>
        /// Command to trigger the scan.
        /// 触发扫描的命令。
        /// </summary>
        public string Command { get; set; }

        /// <summary>
        /// Command to stop triggering the scanner (used by Keyence).
        /// 停止触发扫描器的命令（基恩士使用）。
        /// </summary>
        public string CloseCommand { get; set; }

        /// <summary>
        /// Start character for commands, if applicable (empty string if none).
        /// 命令的起始字符（如果有），没有则为空字符串。
        /// </summary>
        public string Start { get; set; } = string.Empty;

        /// <summary>
        /// End character for commands, such as '\r\n' for Keyence; empty if not used.
        /// 命令的结束字符，如基恩士使用的'\r\n'；如果不使用则为空字符串。
        /// </summary>
        public string End { get; set; } = string.Empty;
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using Wesky.Net.OpenTools.Iot.Scanner.Models;

namespace Wesky.Net.OpenTools.Iot.Scanner
{
    public interface ICodeReader
    {
        /// <summary>
        /// 触发扫码
        /// Trigger barcode scanning.
        /// </summary>
        /// <param name="clientInfo">客户端信息/Client information</param>
        /// <returns>扫码结果/Scanning result</returns>
        ReaderResultInfo ReaderRead(ref ReaderClientInfo clientInfo);
        /// <summary>
        /// 手动关闭扫码器
        /// Manually close the scanner.
        /// </summary>
        /// <param name="clientInfo">客户端信息/Client information</param>
        /// <returns>操作结果/Operation result</returns>
        ReaderResultInfo ReaderClose(ReaderClientInfo clientInfo);
        /// <summary>
        /// 建立与扫码器的连接
        /// Establish a connection with the scanner.
        /// </summary>
        /// <param name="clientInfo">客户端信息/Client information</param>
        /// <returns>连接结果/Connection result</returns>
        ReaderResultInfo ReaderConnection(ref ReaderClientInfo clientInfo);
    }
}

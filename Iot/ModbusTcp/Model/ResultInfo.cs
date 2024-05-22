using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace Wesky.Net.OpenTools.Iot.ModbusTcp.Model
{
    public class ResultInfo
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public Boolean IsSucceed { get; set; } = false;
        /// <summary>
        /// 错误编码
        /// </summary>
        public Int32 Code { get; set; } = 0;
        /// <summary>
        /// 异常内容
        /// </summary>
        public string Message { get; set; }
    }

    public class SocketConnectionResult : ResultInfo
    {
        public Socket Client { get; set; }
    }
}

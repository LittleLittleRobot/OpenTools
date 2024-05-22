using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Wesky.Net.OpenTools.Iot.ModbusTcp.Model
{
    public class ModbusConnectionInfo
    {
        public IPAddress Ip { get; set; }
        public int Port { get; set; }
        public byte Station { get; set; }
    }
}

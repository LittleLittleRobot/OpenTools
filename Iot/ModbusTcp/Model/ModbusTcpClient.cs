using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace Wesky.Net.OpenTools.Iot.ModbusTcp.Model
{
    public class ModbusTcpClient
    {
        public Socket Client { get; set; }
        public ushort MessageCode { get; set; }
        public ModbusConnectionInfo ConnectionInfo { get; set; }

        public int LockState { get; set; } = 0;
    }
}

using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;
using Wesky.Net.OpenTools.Iot.ModbusTcp.Model;

namespace Wesky.Net.OpenTools.Iot.ModbusTcp
{
    public class ModbusConnection
    {
        /// <summary>
        /// 短连接使用
        /// </summary>
        /// <param name="client"></param>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public static ResultInfo Connection(ref Socket client, IPAddress ip, int port)
        {
            ResultInfo result = new ResultInfo();
            try
            {
                client?.Close();
                client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                client.Connect(new IPEndPoint(ip, port));
                result.IsSucceed = true;
            }
            catch (Exception ex)
            {
                result.IsSucceed = false;
                result.Message = $"连接Modbus-TCP服务失败:{ex.Message}";
            }
            return result;
        }

        /// <summary>
        /// 长连接使用
        /// </summary>
        /// <param name="connectionInfo"></param>
        /// <param name="messageCode"></param>
        /// <returns></returns>
        public static ModbusResultInfo<ModbusTcpClient> Connection(ModbusConnectionInfo connectionInfo, ushort messageCode)
        {
            ModbusResultInfo<ModbusTcpClient> result = new ModbusResultInfo<ModbusTcpClient>();
            ModbusTcpClient modbusTcp = new ModbusTcpClient();
            modbusTcp.ConnectionInfo = connectionInfo;
            modbusTcp.MessageCode = messageCode;
            try
            {
                modbusTcp.Client?.Close();
                modbusTcp.Client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                modbusTcp.Client.Connect(new IPEndPoint(connectionInfo.Ip, connectionInfo.Port));

                result = ModbusResult.ReturnSucceed<ModbusTcpClient>(modbusTcp);
            }
            catch (Exception ex)
            {
                result = ModbusResult.ReturnFailed<ModbusTcpClient>($"连接Modbus-TCP服务失败:{ex.Message}", modbusTcp);
            }
            return result;
        }

        public static void DisConnection(ref Socket client)
        {
            client?.Close();
        }

        public static void DisConnection(ModbusTcpClient client)
        {
            client.Client?.Close();
        }
    }
}

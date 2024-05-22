using System;
using System.Collections.Generic;
using System.Text;
using Wesky.Net.OpenTools.Iot.ModbusTcp.Model;

namespace Wesky.Net.OpenTools.Iot.ModbusTcp
{
    public interface IModbusReadWriteService
    {
        ModbusResultInfo<bool[]> ReadBool(string address, ushort length, ModbusTcpClient client);
        ModbusResultInfo<short[]> ReadInt16(string address, ushort length, ModbusTcpClient client);
        ModbusResultInfo<int[]> ReadInt32(string address, ushort length, ModbusTcpClient client);
        ModbusResultInfo<float[]> ReadFloat(string address, ushort length, ModbusTcpClient client);
        ModbusResultInfo<string> ReadString(string address, ushort length, ModbusTcpClient client);
        ResultInfo WriteBool(string address, bool value, ModbusTcpClient client);
        ResultInfo WriteInt16(string address, short[] value, ModbusTcpClient client);
        ResultInfo WriteInt32(string address, int[] value, ModbusTcpClient client);
        ResultInfo WriteFloat(string address, float[] value, ModbusTcpClient client);
        ResultInfo WriteString(string address, string value, ModbusTcpClient client);
    }
}

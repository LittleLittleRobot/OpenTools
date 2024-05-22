using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using Wesky.Net.OpenTools.Iot.ModbusTcp.Model;

namespace Wesky.Net.OpenTools.Iot.ModbusTcp
{
    public class ModbusCoreService
    {
        /// <summary>
        /// 获取发送给Modbus的套接字byte数组数据
        /// </summary>
        /// <param name="modAddress"></param>
        /// <param name="station"></param>
        /// <param name="functionCode"></param>
        /// <param name="length"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public byte[] GetSendTcpBytes(ModbusAddress modAddress, byte station, byte functionCode, ushort length, object value = null)
        {
            byte code = modAddress.Function < 1 ? functionCode : (byte)modAddress.Function;
            byte[] headBuffer = null;
            switch (code)
            {
                case ModbusFunctionCode.WriteSingleCoil:  // 写单个线圈
                    headBuffer = new byte[6];
                    headBuffer[0] = modAddress.Station < 0 ? station : (byte)modAddress.Station;
                    headBuffer[1] = code;
                    headBuffer[2] = BitConverter.GetBytes(modAddress.Address)[1];
                    headBuffer[3] = BitConverter.GetBytes(modAddress.Address)[0];
                    headBuffer[4] = (byte)((bool)value ? 0xFF : 0x00);
                    headBuffer[5] = 0x00;
                    break;

                case ModbusFunctionCode.WriteMultiRegister:  // 写多个值(非线圈),一个值也用该方法写比较简单，不需要额外使用写单个值
                    byte[] values = (byte[])value;
                    headBuffer = new byte[7 + values.Length];
                    headBuffer[0] = modAddress.Station < 0 ? station : (byte)modAddress.Station;
                    headBuffer[1] = code;
                    headBuffer[2] = BitConverter.GetBytes(modAddress.Address)[1];
                    headBuffer[3] = BitConverter.GetBytes(modAddress.Address)[0];
                    headBuffer[4] = (byte)(values.Length / 2 / 256);
                    headBuffer[5] = (byte)(values.Length / 2 % 256);
                    headBuffer[6] = (byte)(values.Length);
                    values.CopyTo(headBuffer, 7);
                    break;

                default:  // 其他默认
                    headBuffer = new byte[6];
                    headBuffer[0] = modAddress.Station < 0 ? station : (byte)modAddress.Station;
                    headBuffer[1] = code;
                    headBuffer[2] = BitConverter.GetBytes(modAddress.Address)[1];
                    headBuffer[3] = BitConverter.GetBytes(modAddress.Address)[0];
                    headBuffer[4] = BitConverter.GetBytes(length)[1];
                    headBuffer[5] = BitConverter.GetBytes(length)[0];
                    break;
            }

            return GetDataBytes(modAddress.MessageCode, headBuffer);

        }

        /// <summary>
        /// 拼接套接字
        /// </summary>
        /// <param name="messageCode"></param>
        /// <param name="headBuffer"></param>
        /// <returns></returns>
        private byte[] GetDataBytes(ushort messageCode, byte[] headBuffer)
        {
            byte[] tcpBuffer = new byte[headBuffer.Length + 6];
            tcpBuffer[0] = BitConverter.GetBytes(messageCode)[1];
            tcpBuffer[1] = BitConverter.GetBytes(messageCode)[0];

            tcpBuffer[4] = BitConverter.GetBytes(headBuffer.Length)[1];
            tcpBuffer[5] = BitConverter.GetBytes(headBuffer.Length)[0];

            headBuffer.CopyTo(tcpBuffer, 6);

            return tcpBuffer;
        }

        private ResultInfo CompareBuffer(byte[] tcpBuffer, byte[] receiveBytes)
        {
            ResultInfo resultInfo = new ResultInfo();

            if ((tcpBuffer[7] + 0x80) == receiveBytes[7])
            {
                var str = ModbusFunctionCode.GetDescriptionByErrorCode(receiveBytes[8]);
                resultInfo.IsSucceed = false;
                resultInfo.Message = str;
            }
            else
            {
                resultInfo.IsSucceed = true;
            }

            return resultInfo;
        }

        public ModbusResultInfo<bool[]> ReadBool(string address, ushort length, ushort messageCode, ModbusConnectionInfo connectionInfo)
        {
            ModbusResultInfo<bool[]> result = new ModbusResultInfo<bool[]>();

            byte functionCode = ModbusFunctionCode.ReadCoil;
            ushort newLength = 0;

            ModbusAddress modAddress = new ModbusAddress(address, functionCode, connectionInfo);
            modAddress.MessageCode = messageCode;
            Socket client = null;

            var clientInfo = ModbusConnection.Connection(ref client, modAddress.ModbusConnectInfo.Ip, modAddress.ModbusConnectInfo.Port);
            if (!clientInfo.IsSucceed)
            {
                return ModbusResult.ReturnFailed<bool[]>(clientInfo.Message, new bool[0]);
            }

            bool[] readMultiResult = new bool[length];
            int resultIndex = 0;

            try
            {
                while (length > 0)
                {
                    if (length > 200)
                    {
                        length = (ushort)(length - 200);
                        newLength = 200;
                    }
                    else
                    {
                        newLength = length;
                        length = 0;
                    }
                    resultIndex += newLength;

                    byte[] tcpBuffer = GetSendTcpBytes(modAddress, modAddress.ModbusConnectInfo.Station, functionCode, newLength);

                    client.Send(tcpBuffer);

                    byte[] receiveBytes = new byte[newLength * 2 + 9];
                    int count = client.Receive(receiveBytes);

                    var resultInfo = CompareBuffer(tcpBuffer, receiveBytes);
                    if (!resultInfo.IsSucceed)
                    {
                        throw new Exception(resultInfo.Message);
                    }

                    List<byte> byteList = new List<byte>(receiveBytes);
                    byteList.RemoveRange(0, 9);

                    var readResult = DataConvert.ByteToBoolArray(byteList.ToArray(), newLength);

                    Buffer.BlockCopy(readResult, 0, readMultiResult, resultIndex - newLength, newLength);

                }
                result = ModbusResult.ReturnSucceed<bool[]>(readMultiResult);
            }
            catch (Exception ex)
            {
                result = ModbusResult.ReturnFailed<bool[]>($"读取数据失败:{ex.Message}", new bool[0]);
            }

            client?.Close();
            return result;
        }

        public ModbusResultInfo<short[]> ReadInt16(string address, ushort length, ushort messageCode, ModbusConnectionInfo connectionInfo)
        {
            ModbusResultInfo<short[]> result = new ModbusResultInfo<short[]>();

            byte functionCode = ModbusFunctionCode.ReadRegister;

            ModbusAddress modAddress = new ModbusAddress(address, functionCode, connectionInfo);
            modAddress.MessageCode = messageCode;

            Socket client = null;
            var clientInfo = ModbusConnection.Connection(ref client, modAddress.ModbusConnectInfo.Ip, modAddress.ModbusConnectInfo.Port);
            if (!clientInfo.IsSucceed)
            {
                return ModbusResult.ReturnFailed<short[]>(clientInfo.Message, new short[0]);
            }

            ushort newLength = 0;
            short[] readMultiResult = new short[length];
            int resultIndex = 0;

            try
            {
                while (length > 0)
                {
                    if (length > 124)
                    {
                        newLength = 124;
                        length = (ushort)(length - 124);
                    }
                    else
                    {
                        newLength = length;
                    }
                    resultIndex += newLength;

                    byte[] tcpBuffer = GetSendTcpBytes(modAddress, modAddress.ModbusConnectInfo.Station, functionCode, newLength);

                    client.Send(tcpBuffer);

                    byte[] receiveBytes = new byte[newLength * 2 + 9];
                    int count = client.Receive(receiveBytes);

                    var resultInfo = CompareBuffer(tcpBuffer, receiveBytes);
                    if (!resultInfo.IsSucceed)
                    {
                        throw new Exception(resultInfo.Message);
                    }

                    List<byte> byteList = new List<byte>(receiveBytes);
                    byteList.RemoveRange(0, 9);

                    var readResult = DataConvert.TransShort(byteList.ToArray(), 0, newLength);
                    Buffer.BlockCopy(readResult, 0, readMultiResult, resultIndex - newLength, newLength);

                }
                result = ModbusResult.ReturnSucceed<short[]>(readMultiResult);

            }
            catch (Exception ex)
            {
                return ModbusResult.ReturnFailed<short[]>(ex.Message, new short[0]);
            }
            client?.Close();

            return result;
        }

        public ModbusResultInfo<int[]> ReadInt32(string address, ushort length, ushort messageCode, ModbusConnectionInfo connectionInfo)
        {
            ModbusResultInfo<int[]> result = new ModbusResultInfo<int[]>();

            byte functionCode = ModbusFunctionCode.ReadRegister;

            ModbusAddress modAddress = new ModbusAddress(address, functionCode, connectionInfo);
            modAddress.MessageCode = messageCode;

            Socket client = null;
            var clientInfo = ModbusConnection.Connection(ref client, modAddress.ModbusConnectInfo.Ip, modAddress.ModbusConnectInfo.Port);
            if (!clientInfo.IsSucceed)
            {
                return ModbusResult.ReturnFailed<int[]>(clientInfo.Message, new int[0]);
            }

            ushort newLength = 0;
            int[] readMultiResult = new int[length];
            int resultIndex = 0;

            try
            {
                while (length > 0)
                {
                    if (length > 60)
                    {
                        newLength = 60;
                        length = (ushort)(length - 60);
                    }
                    else
                    {
                        newLength = length;
                    }
                    resultIndex += newLength;

                    byte[] tcpBuffer = GetSendTcpBytes(modAddress, modAddress.ModbusConnectInfo.Station, functionCode, (ushort)(newLength * 2));

                    client.Send(tcpBuffer);

                    byte[] receiveBytes = new byte[newLength * 4 + 9];
                    int count = client.Receive(receiveBytes);

                    var resultInfo = CompareBuffer(tcpBuffer, receiveBytes);
                    if (!resultInfo.IsSucceed)
                    {
                        throw new Exception(resultInfo.Message);
                    }

                    List<byte> byteList = new List<byte>(receiveBytes);
                    byteList.RemoveRange(0, 9);

                    var readResult = DataConvert.TransInt32(byteList.ToArray(), 0, newLength);
                    Buffer.BlockCopy(readResult, 0, readMultiResult, resultIndex - newLength, newLength);

                }
                result = ModbusResult.ReturnSucceed<int[]>(readMultiResult);

            }
            catch (Exception ex)
            {
                return ModbusResult.ReturnFailed<int[]>(ex.Message, new int[0]);
            }
            client?.Close();

            return result;
        }

        public ModbusResultInfo<float[]> ReadFloat(string address, ushort length, ushort messageCode, ModbusConnectionInfo connectionInfo)
        {
            ModbusResultInfo<float[]> result = new ModbusResultInfo<float[]>();

            byte functionCode = ModbusFunctionCode.ReadRegister;

            ModbusAddress modAddress = new ModbusAddress(address, functionCode, connectionInfo);
            modAddress.MessageCode = messageCode;

            Socket client = null;
            var clientInfo = ModbusConnection.Connection(ref client, modAddress.ModbusConnectInfo.Ip, modAddress.ModbusConnectInfo.Port);
            if (!clientInfo.IsSucceed)
            {
                return ModbusResult.ReturnFailed<float[]>(clientInfo.Message, new float[0]);
            }

            ushort newLength = 0;
            float[] readMultiResult = new float[length];
            int resultIndex = 0;

            try
            {
                while (length > 0)
                {
                    if (length > 60)
                    {
                        newLength = 60;
                        length = (ushort)(length - 60);
                    }
                    else
                    {
                        newLength = length;
                    }
                    resultIndex += newLength;

                    byte[] tcpBuffer = GetSendTcpBytes(modAddress, modAddress.ModbusConnectInfo.Station, functionCode, (ushort)(newLength * 2));

                    client.Send(tcpBuffer);

                    byte[] receiveBytes = new byte[newLength * 4 + 9];
                    int count = client.Receive(receiveBytes);

                    var resultInfo = CompareBuffer(tcpBuffer, receiveBytes);
                    if (!resultInfo.IsSucceed)
                    {
                        throw new Exception(resultInfo.Message);
                    }

                    List<byte> byteList = new List<byte>(receiveBytes);
                    byteList.RemoveRange(0, 9);

                    var readResult = DataConvert.TransSingle(byteList.ToArray(), 0, newLength);
                    Buffer.BlockCopy(readResult, 0, readMultiResult, resultIndex - newLength, newLength);

                }
                result = ModbusResult.ReturnSucceed<float[]>(readMultiResult);

            }
            catch (Exception ex)
            {
                return ModbusResult.ReturnFailed<float[]>(ex.Message, new float[0]);
            }
            client?.Close();

            return result;
        }

        public ModbusResultInfo<string> ReadString(string address, ushort length, ushort messageCode, ModbusConnectionInfo connectionInfo)
        {
            ModbusResultInfo<string> result = new ModbusResultInfo<string>();

            byte functionCode = ModbusFunctionCode.ReadRegister;

            ModbusAddress modAddress = new ModbusAddress(address, functionCode, connectionInfo);
            modAddress.MessageCode = messageCode;

            Socket client = null;
            var clientInfo = ModbusConnection.Connection(ref client, modAddress.ModbusConnectInfo.Ip, modAddress.ModbusConnectInfo.Port);
            if (!clientInfo.IsSucceed)
            {
                return ModbusResult.ReturnFailed<string>(clientInfo.Message, "");
            }

            try
            {
                byte[] tcpBuffer = GetSendTcpBytes(modAddress, modAddress.ModbusConnectInfo.Station, functionCode, length);

                client.Send(tcpBuffer);

                byte[] receiveBytes = new byte[length * 2 + 9];
                int count = client.Receive(receiveBytes);

                var resultInfo = CompareBuffer(tcpBuffer, receiveBytes);
                if (!resultInfo.IsSucceed)
                {
                    throw new Exception(resultInfo.Message);
                }

                List<byte> byteList = new List<byte>(receiveBytes);
                byteList.RemoveRange(0, 9);

                var values = DataConvert.TransString(byteList.ToArray(), 0, length, Encoding.ASCII);

                result = ModbusResult.ReturnSucceed<string>(values);

            }
            catch (Exception ex)
            {
                result = ModbusResult.ReturnFailed<string>(ex.Message, "");
            }

            client?.Close();

            return result;
        }

        public ResultInfo WriteBool(string address, bool value, ushort messageCode, ModbusConnectionInfo connectionInfo)
        {
            ResultInfo result = new ResultInfo();
            byte functionCode = ModbusFunctionCode.WriteSingleCoil;

            ModbusAddress modAddress = new ModbusAddress(address, functionCode, connectionInfo);
            modAddress.MessageCode = messageCode;
            byte[] tcpBuffer = GetSendTcpBytes(modAddress, modAddress.ModbusConnectInfo.Station, functionCode, 0, value);

            Socket client = null;

            var clientInfo = ModbusConnection.Connection(ref client, modAddress.ModbusConnectInfo.Ip, modAddress.ModbusConnectInfo.Port);
            if (!clientInfo.IsSucceed)
            {
                return clientInfo;
            }
            try
            {
                client.Send(tcpBuffer);

                byte[] receiveBytes = new byte[9];
                int count = client.Receive(receiveBytes);

                if ((tcpBuffer[7] + 0x80) == receiveBytes[7])
                {
                    var str = ModbusFunctionCode.GetDescriptionByErrorCode(receiveBytes[8]);
                    result.IsSucceed = false;
                    result.Message = str;
                }
                else
                {
                    result.IsSucceed = true;
                    result.Message = "Ok";
                }
            }
            catch (Exception ex)
            {
                result.IsSucceed = false;
                result.Message = ex.Message;
            }
            client?.Close();

            return result;
        }

        public ResultInfo WriteInt16(string address, short[] value, ushort messageCode, ModbusConnectionInfo connectionInfo)
        {
            ResultInfo result = new ResultInfo();
            byte functionCode = ModbusFunctionCode.WriteMultiRegister;

            ModbusAddress modAddress = new ModbusAddress(address, functionCode, connectionInfo);
            modAddress.MessageCode = messageCode;

            Socket client = null;

            var clientInfo = ModbusConnection.Connection(ref client, modAddress.ModbusConnectInfo.Ip, modAddress.ModbusConnectInfo.Port);
            if (!clientInfo.IsSucceed)
            {
                return clientInfo;
            }

            byte[] values = DataConvert.TransReverseByte(value);

            byte[] tcpBuffer = GetSendTcpBytes(modAddress, modAddress.ModbusConnectInfo.Station, functionCode, 0, values);

            try
            {
                client.Send(tcpBuffer);

                byte[] receiveBytes = new byte[9];
                int count = client.Receive(receiveBytes);

                if ((tcpBuffer[7] + 0x80) == receiveBytes[7])
                {
                    var str = ModbusFunctionCode.GetDescriptionByErrorCode(receiveBytes[8]);
                    result.IsSucceed = false;
                    result.Message = str;
                }
                else
                {
                    result.IsSucceed = true;
                    result.Message = "Ok";
                }
            }
            catch (Exception ex)
            {
                result.IsSucceed = false;
                result.Message = ex.Message;
            }

            client?.Close();
            return result;
        }

        public ResultInfo WriteInt32(string address, int[] value, ushort messageCode, ModbusConnectionInfo connectionInfo)
        {
            ResultInfo result = new ResultInfo();
            byte functionCode = ModbusFunctionCode.WriteMultiRegister;

            ModbusAddress modAddress = new ModbusAddress(address, functionCode, connectionInfo);
            modAddress.MessageCode = messageCode;

            Socket client = null;

            var clientInfo = ModbusConnection.Connection(ref client, modAddress.ModbusConnectInfo.Ip, modAddress.ModbusConnectInfo.Port);
            if (!clientInfo.IsSucceed)
            {
                return clientInfo;
            }

            byte[] values = DataConvert.TransReverseByte(value);

            byte[] tcpBuffer = GetSendTcpBytes(modAddress, modAddress.ModbusConnectInfo.Station, functionCode, 0, values);

            try
            {
                client.Send(tcpBuffer);

                byte[] receiveBytes = new byte[9];
                int count = client.Receive(receiveBytes);

                if ((tcpBuffer[7] + 0x80) == receiveBytes[7])
                {
                    var str = ModbusFunctionCode.GetDescriptionByErrorCode(receiveBytes[8]);
                    result.IsSucceed = false;
                    result.Message = str;
                }
                else
                {
                    result.IsSucceed = true;
                    result.Message = "Ok";
                }
            }
            catch (Exception ex)
            {
                result.IsSucceed = false;
                result.Message = ex.Message;
            }

            client?.Close();
            return result;
        }

        public ResultInfo WriteFloat(string address, float[] value, ushort messageCode, ModbusConnectionInfo connectionInfo)
        {
            ResultInfo result = new ResultInfo();
            byte functionCode = ModbusFunctionCode.WriteMultiRegister;

            ModbusAddress modAddress = new ModbusAddress(address, functionCode, connectionInfo);
            modAddress.MessageCode = messageCode;

            Socket client = null;

            var clientInfo = ModbusConnection.Connection(ref client, modAddress.ModbusConnectInfo.Ip, modAddress.ModbusConnectInfo.Port);
            if (!clientInfo.IsSucceed)
            {
                return clientInfo;
            }

            byte[] values = DataConvert.TransReverseByte(value);

            byte[] tcpBuffer = GetSendTcpBytes(modAddress, modAddress.ModbusConnectInfo.Station, functionCode, 0, values);

            try
            {
                client.Send(tcpBuffer);

                byte[] receiveBytes = new byte[9];
                int count = client.Receive(receiveBytes);

                if ((tcpBuffer[7] + 0x80) == receiveBytes[7])
                {
                    var str = ModbusFunctionCode.GetDescriptionByErrorCode(receiveBytes[8]);
                    result.IsSucceed = false;
                    result.Message = str;
                }
                else
                {
                    result.IsSucceed = true;
                    result.Message = "Ok";
                }
            }
            catch (Exception ex)
            {
                result.IsSucceed = false;
                result.Message = ex.Message;
            }

            client?.Close();
            return result;
        }

        public ResultInfo WriteString(string address, string value, ushort messageCode, ModbusConnectionInfo connectionInfo)
        {
            ResultInfo result = new ResultInfo();
            byte functionCode = ModbusFunctionCode.WriteMultiRegister;

            ModbusAddress modAddress = new ModbusAddress(address, functionCode, connectionInfo);
            modAddress.MessageCode = messageCode;

            Socket client = null;

            var clientInfo = ModbusConnection.Connection(ref client, modAddress.ModbusConnectInfo.Ip, modAddress.ModbusConnectInfo.Port);
            if (!clientInfo.IsSucceed)
            {
                return clientInfo;
            }

            byte[] tmp = DataConvert.TransByte(value, Encoding.ASCII);

            byte[] values = DataConvert.ArrayExpandToLengthEven<byte>(tmp);

            byte[] tcpBuffer = GetSendTcpBytes(modAddress, modAddress.ModbusConnectInfo.Station, functionCode, 0, values);

            try
            {
                client.Send(tcpBuffer);

                byte[] receiveBytes = new byte[9];
                int count = client.Receive(receiveBytes);

                if ((tcpBuffer[7] + 0x80) == receiveBytes[7])
                {
                    var str = ModbusFunctionCode.GetDescriptionByErrorCode(receiveBytes[8]);
                    result.IsSucceed = false;
                    result.Message = str;
                }
                else
                {
                    result.IsSucceed = true;
                    result.Message = "Ok";
                }
            }
            catch (Exception ex)
            {
                result.IsSucceed = false;
                result.Message = ex.Message;
            }

            client?.Close();
            return result;
        }
    }
}

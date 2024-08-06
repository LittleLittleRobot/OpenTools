using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;
using Wesky.Net.OpenTools.Iot.ModbusTcp.Model;

namespace Wesky.Net.OpenTools.Iot.ModbusTcp
{
    public class ModbusReadWriteService : IModbusReadWriteService
    {
        private byte[] GetSendTcpBytes(ModbusAddress modAddress, byte station, byte functionCode, ushort length, object value = null)
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

                case ModbusFunctionCode.WriteSingleRegister:
                    byte[] valueBytes = (byte[])value;
                    headBuffer = new byte[6];
                    headBuffer[0] = modAddress.Station < 0 ? station : (byte)modAddress.Station;
                    headBuffer[1] = code;
                    headBuffer[2] = BitConverter.GetBytes(modAddress.Address)[1];
                    headBuffer[3] = BitConverter.GetBytes(modAddress.Address)[0];
                    headBuffer[4] = valueBytes[0];
                    headBuffer[5] = valueBytes[1];
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

        private bool IsSocketConnected(Socket client)
        {
            bool blockingState = client.Blocking;
            try
            {
                byte[] tmp = new byte[1];

                client.Blocking = false;  // 1、 如果连接可以用，一切正常的情况下，此处会抛异常，异常代码是 10035  如果连接不可用，此处不会抛异常

                client.Send(tmp, 0, 0); // 2、连接不可用的情况下，进行发送数据，如果发送成功，说明连接还可以使用。不过基本上进入到这一步，连接都是被断开或者不可用的情况了。

                //  client.Receive(new byte[1]);

                return true;
            }
            catch (SocketException e)
            {
                if (e.NativeErrorCode.Equals(10035))  // 对应第一步，异常在这里，会返回true，说明连接可以继续用
                {
                    return true;
                }
                else
                {
                    return false;  // 对应第二步，说明连接已经被断开不可用
                }
            }
            finally
            {
                client.Blocking = blockingState;    // 恢复状态
            }
        }
        private static SocketConnectionResult Connection(IPAddress ip, int port)
        {
            SocketConnectionResult result = new SocketConnectionResult();
            try
            {
                result.Client?.Close();
                result.Client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                result.Client.Connect(new IPEndPoint(ip, port));
                result.IsSucceed = true;
            }
            catch (Exception ex)
            {
                result.Client = null;
                result.IsSucceed = false;
                result.Message = ex.Message;
            }
            return result;
        }
        public ModbusResultInfo<bool[]> ReadBool(string address, ushort length, ModbusTcpClient client)
        {
            ModbusResultInfo<bool[]> result = new ModbusResultInfo<bool[]>();

            byte functionCode = ModbusFunctionCode.ReadCoil;
            ushort newLength = 0;

            ModbusAddress modAddress = new ModbusAddress(address, functionCode);
            modAddress.ModbusConnectInfo = client.ConnectionInfo;
            modAddress.MessageCode = client.MessageCode;


            bool[] readMultiResult = new bool[length];
            int resultIndex = 0;
            ushort currentLength = length;
            try
            {
                List<byte> byteResult = new List<byte>();

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

                    try
                    {
                        client.Client.Send(tcpBuffer);
                    }
                    catch (Exception ex)
                    {
                        var socketResult = Connection(client.ConnectionInfo.Ip, client.ConnectionInfo.Port);
                        if (socketResult.IsSucceed)
                        {
                            client.Client = socketResult.Client;
                            client.Client.Send(tcpBuffer);
                        }
                        else
                        {
                            throw new Exception($"PLC服务器:{client.ConnectionInfo.Ip} 的连接已断开，并且尝试重新连接也失败。连接失败异常信息:{socketResult.Message}");
                        }
                    }

                    byte[] receiveBytes = new byte[newLength * 2 + 9];
                    int count = client.Client.Receive(receiveBytes);

                    var resultInfo = CompareBuffer(tcpBuffer, receiveBytes);
                    if (!resultInfo.IsSucceed)
                    {
                        throw new Exception(resultInfo.Message);
                    }

                    List<byte> byteList = new List<byte>(receiveBytes);
                    byteList.RemoveRange(0, 9);
                    byteResult.AddRange(byteList);
                    modAddress.Address += newLength;

                }
                result = ModbusResult.ReturnSucceed<bool[]>(DataConvert.ByteToBoolArray(byteResult.ToArray(), currentLength));
            }
            catch (Exception ex)
            {
                result = ModbusResult.ReturnFailed<bool[]>($"读取数据失败:{ex.Message}", new bool[0]);
            }

            return result;
        }

        public ModbusResultInfo<short[]> ReadInt16(string address, ushort length, ModbusTcpClient client)
        {
            ModbusResultInfo<short[]> result = new ModbusResultInfo<short[]>();

            byte functionCode = ModbusFunctionCode.ReadRegister;

            ModbusAddress modAddress = new ModbusAddress(address, functionCode);
            modAddress.ModbusConnectInfo = client.ConnectionInfo;
            modAddress.MessageCode = client.MessageCode;

            ushort newLength = 0;
            short[] readMultiResult = new short[length];
            ushort currentLength = length;
            int resultIndex = 0;

            try
            {
                List<byte> byteResult = new List<byte>();
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
                        length = 0;
                    }
                    resultIndex = newLength + resultIndex;


                    byte[] tcpBuffer = GetSendTcpBytes(modAddress, modAddress.ModbusConnectInfo.Station, functionCode, newLength);

                    try
                    {
                        client.Client.Send(tcpBuffer);
                    }
                    catch (Exception ex)
                    {
                        var socketResult = Connection(client.ConnectionInfo.Ip, client.ConnectionInfo.Port);
                        if (socketResult.IsSucceed)
                        {
                            client.Client = socketResult.Client;
                            client.Client.Send(tcpBuffer);
                        }
                        else
                        {
                            throw new Exception($"PLC服务器:{client.ConnectionInfo.Ip} 的连接已断开，并且尝试重新连接也失败。连接失败异常信息:{socketResult.Message}");
                        }
                    }

                    byte[] receiveBytes = new byte[newLength * 2 + 9];
                    int count = client.Client.Receive(receiveBytes);

                    var resultInfo = CompareBuffer(tcpBuffer, receiveBytes);
                    if (!resultInfo.IsSucceed)
                    {
                        throw new Exception(resultInfo.Message);
                    }

                    List<byte> byteList = new List<byte>(receiveBytes);
                    byteList.RemoveRange(0, 9);

                    byteResult.AddRange(byteList);

                    modAddress.Address += newLength;

                }
                result = ModbusResult.ReturnSucceed<short[]>(DataConvert.TransShort(byteResult.ToArray(), 0, currentLength));

            }
            catch (Exception ex)
            {
                return ModbusResult.ReturnFailed<short[]>(ex.Message, new short[0]);
            }

            return result;
        }

        public ModbusResultInfo<int[]> ReadInt32(string address, ushort length, ModbusTcpClient client)
        {
            ModbusResultInfo<int[]> result = new ModbusResultInfo<int[]>();

            byte functionCode = ModbusFunctionCode.ReadRegister;

            ModbusAddress modAddress = new ModbusAddress(address, functionCode);
            modAddress.ModbusConnectInfo = client.ConnectionInfo;
            modAddress.MessageCode = client.MessageCode;

            ushort newLength = 0;
            int[] readMultiResult = new int[length];
            int resultIndex = 0;
            ushort currentLength = length;
            try
            {
                List<byte> byteResult = new List<byte>();

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
                        length = 0;
                    }
                    resultIndex += newLength;

                    byte[] tcpBuffer = GetSendTcpBytes(modAddress, modAddress.ModbusConnectInfo.Station, functionCode, (ushort)(newLength * 2));

                    try
                    {
                        client.Client.Send(tcpBuffer);
                    }
                    catch (Exception ex)
                    {
                        var socketResult = Connection(client.ConnectionInfo.Ip, client.ConnectionInfo.Port);
                        if (socketResult.IsSucceed)
                        {
                            client.Client = socketResult.Client;
                            client.Client.Send(tcpBuffer);
                        }
                        else
                        {
                            throw new Exception($"PLC服务器:{client.ConnectionInfo.Ip} 的连接已断开，并且尝试重新连接也失败。连接失败异常信息:{socketResult.Message}");
                        }
                    }

                    byte[] receiveBytes = new byte[newLength * 4 + 9];
                    int count = client.Client.Receive(receiveBytes);

                    var resultInfo = CompareBuffer(tcpBuffer, receiveBytes);
                    if (!resultInfo.IsSucceed)
                    {
                        throw new Exception(resultInfo.Message);
                    }

                    List<byte> byteList = new List<byte>(receiveBytes);
                    byteList.RemoveRange(0, 9);
                    byteResult.AddRange(byteList);

                    modAddress.Address += newLength;

                }
                result = ModbusResult.ReturnSucceed<int[]>(DataConvert.TransInt32(byteResult.ToArray(), 0, currentLength));

            }
            catch (Exception ex)
            {
                return ModbusResult.ReturnFailed<int[]>(ex.Message, new int[0]);
            }

            return result;
        }

        public ModbusResultInfo<float[]> ReadFloat(string address, ushort length, ModbusTcpClient client)
        {
            ModbusResultInfo<float[]> result = new ModbusResultInfo<float[]>();

            byte functionCode = ModbusFunctionCode.ReadRegister;

            ModbusAddress modAddress = new ModbusAddress(address, functionCode);
            modAddress.ModbusConnectInfo = client.ConnectionInfo;
            modAddress.MessageCode = client.MessageCode;

            ushort newLength = 0;
            float[] readMultiResult = new float[length];
            int resultIndex = 0;
            ushort currentLength = length;
            try
            {
                List<byte> byteResult = new List<byte>();

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
                        length = 0;
                    }
                    resultIndex += newLength;

                    byte[] tcpBuffer = GetSendTcpBytes(modAddress, modAddress.ModbusConnectInfo.Station, functionCode, (ushort)(newLength * 2));

                    try
                    {
                        client.Client.Send(tcpBuffer);
                    }
                    catch (Exception ex)
                    {
                        var socketResult = Connection(client.ConnectionInfo.Ip, client.ConnectionInfo.Port);
                        if (socketResult.IsSucceed)
                        {
                            client.Client = socketResult.Client;
                            client.Client.Send(tcpBuffer);
                        }
                        else
                        {
                            throw new Exception($"PLC服务器:{client.ConnectionInfo.Ip} 的连接已断开，并且尝试重新连接也失败。连接失败异常信息:{socketResult.Message}");
                        }
                    }

                    byte[] receiveBytes = new byte[newLength * 4 + 9];
                    int count = client.Client.Receive(receiveBytes);

                    var resultInfo = CompareBuffer(tcpBuffer, receiveBytes);
                    if (!resultInfo.IsSucceed)
                    {
                        throw new Exception(resultInfo.Message);
                    }

                    List<byte> byteList = new List<byte>(receiveBytes);
                    byteList.RemoveRange(0, 9);

                    byteResult.AddRange(byteList);

                    modAddress.Address += newLength;

                }
                result = ModbusResult.ReturnSucceed<float[]>(DataConvert.TransSingle(byteResult.ToArray(), 0, currentLength));

            }
            catch (Exception ex)
            {
                return ModbusResult.ReturnFailed<float[]>(ex.Message, new float[0]);
            }

            return result;
        }

        public ModbusResultInfo<string> ReadString(string address, ushort length, ModbusTcpClient client)
        {
            ModbusResultInfo<string> result = new ModbusResultInfo<string>();

            byte functionCode = ModbusFunctionCode.ReadRegister;

            ModbusAddress modAddress = new ModbusAddress(address, functionCode);
            modAddress.ModbusConnectInfo = client.ConnectionInfo;
            modAddress.MessageCode = client.MessageCode;

            try
            {
                byte[] tcpBuffer = GetSendTcpBytes(modAddress, modAddress.ModbusConnectInfo.Station, functionCode, length);

                try
                {
                    client.Client.Send(tcpBuffer);
                }
                catch (Exception ex)
                {
                    var socketResult = Connection(client.ConnectionInfo.Ip, client.ConnectionInfo.Port);
                    if (socketResult.IsSucceed)
                    {
                        client.Client = socketResult.Client;
                        client.Client.Send(tcpBuffer);
                    }
                    else
                    {
                        throw new Exception($"PLC服务器:{client.ConnectionInfo.Ip} 的连接已断开，并且尝试重新连接也失败。连接失败异常信息:{socketResult.Message}");
                    }
                }

                byte[] receiveBytes = new byte[length * 2 + 9];
                int count = client.Client.Receive(receiveBytes);

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

            return result;
        }

        public ResultInfo WriteBool(string address, bool value, ModbusTcpClient client)
        {
            ResultInfo result = new ResultInfo();
            byte functionCode = ModbusFunctionCode.WriteSingleCoil;

            ModbusAddress modAddress = new ModbusAddress(address, functionCode);
            modAddress.ModbusConnectInfo = client.ConnectionInfo;
            modAddress.MessageCode = client.MessageCode;

            byte[] tcpBuffer = GetSendTcpBytes(modAddress, modAddress.ModbusConnectInfo.Station, functionCode, 0, value);

            try
            {
                try
                {
                    client.Client.Send(tcpBuffer);
                }
                catch (Exception ex)
                {
                    var socketResult = Connection(client.ConnectionInfo.Ip, client.ConnectionInfo.Port);
                    if (socketResult.IsSucceed)
                    {
                        client.Client = socketResult.Client;
                        client.Client.Send(tcpBuffer);
                    }
                    else
                    {
                        throw new Exception($"PLC服务器:{client.ConnectionInfo.Ip} 的连接已断开，并且尝试重新连接也失败。连接失败异常信息:{socketResult.Message}");
                    }
                }

                byte[] receiveBytes = new byte[12]; // 9 调整为至少12个字节以包括MBAP头
                int count = client.Client.Receive(receiveBytes);

                if (count < 8) // 检查是否至少接收了8个字节
                {
                    throw new Exception("接收到的数据不完整，操作失败。");
                }

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

            return result;
        }

        public ResultInfo WriteInt16(string address, short[] value, ModbusTcpClient client)
        {
            ResultInfo result = new ResultInfo();
            byte functionCode = ModbusFunctionCode.WriteMultiRegister;

            ModbusAddress modAddress = new ModbusAddress(address, functionCode);
            modAddress.ModbusConnectInfo = client.ConnectionInfo;
            modAddress.MessageCode = client.MessageCode;

            byte[] values = DataConvert.TransReverseByte(value);

            byte[] tcpBuffer = GetSendTcpBytes(modAddress, modAddress.ModbusConnectInfo.Station, functionCode, 0, values);

            try
            {
                try
                {
                    client.Client.Send(tcpBuffer);
                }
                catch (Exception ex)
                {
                    var socketResult = Connection(client.ConnectionInfo.Ip, client.ConnectionInfo.Port);
                    if (socketResult.IsSucceed)
                    {
                        client.Client = socketResult.Client;
                        client.Client.Send(tcpBuffer);
                    }
                    else
                    {
                        throw new Exception($"PLC服务器:{client.ConnectionInfo.Ip} 的连接已断开，并且尝试重新连接也失败。连接失败异常信息:{socketResult.Message}");
                    }
                }

                byte[] receiveBytes = new byte[12]; // 9 调整为至少12个字节以包括MBAP头
                int count = client.Client.Receive(receiveBytes);

                if (count < 8) // 检查是否至少接收了8个字节
                {
                    throw new Exception("接收到的数据不完整，操作失败。");
                }

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

            return result;
        }

        public ResultInfo WriteInt32(string address, int[] value, ModbusTcpClient client)
        {
            ResultInfo result = new ResultInfo();
            byte functionCode = ModbusFunctionCode.WriteMultiRegister;

            ModbusAddress modAddress = new ModbusAddress(address, functionCode);
            modAddress.ModbusConnectInfo = client.ConnectionInfo;
            modAddress.MessageCode = client.MessageCode;

            byte[] values = DataConvert.TransReverseByte(value);

            byte[] tcpBuffer = GetSendTcpBytes(modAddress, modAddress.ModbusConnectInfo.Station, functionCode, 0, values);

            try
            {
                try
                {
                    client.Client.Send(tcpBuffer);
                }
                catch (Exception ex)
                {
                    var socketResult = Connection(client.ConnectionInfo.Ip, client.ConnectionInfo.Port);
                    if (socketResult.IsSucceed)
                    {
                        client.Client = socketResult.Client;
                        client.Client.Send(tcpBuffer);
                    }
                    else
                    {
                        throw new Exception($"PLC服务器:{client.ConnectionInfo.Ip} 的连接已断开，并且尝试重新连接也失败。连接失败异常信息:{socketResult.Message}");
                    }
                }

                byte[] receiveBytes = new byte[12]; // 9 调整为至少12个字节以包括MBAP头
                int count = client.Client.Receive(receiveBytes);

                if (count < 8) // 检查是否至少接收了8个字节
                {
                    throw new Exception("接收到的数据不完整，操作失败。");
                }

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

            return result;
        }

        public ResultInfo WriteFloat(string address, float[] value, ModbusTcpClient client)
        {
            ResultInfo result = new ResultInfo();
            byte functionCode = ModbusFunctionCode.WriteMultiRegister;

            ModbusAddress modAddress = new ModbusAddress(address, functionCode);
            modAddress.ModbusConnectInfo = client.ConnectionInfo;
            modAddress.MessageCode = client.MessageCode;

            byte[] values = DataConvert.TransReverseByte(value);

            byte[] tcpBuffer = GetSendTcpBytes(modAddress, modAddress.ModbusConnectInfo.Station, functionCode, 0, values);

            try
            {
                try
                {
                    client.Client.Send(tcpBuffer);
                }
                catch (Exception ex)
                {
                    var socketResult = Connection(client.ConnectionInfo.Ip, client.ConnectionInfo.Port);
                    if (socketResult.IsSucceed)
                    {
                        client.Client = socketResult.Client;
                        client.Client.Send(tcpBuffer);
                    }
                    else
                    {
                        throw new Exception($"PLC服务器:{client.ConnectionInfo.Ip} 的连接已断开，并且尝试重新连接也失败。连接失败异常信息:{socketResult.Message}");
                    }
                }

                byte[] receiveBytes = new byte[12]; // 9 调整为至少12个字节以包括MBAP头
                int count = client.Client.Receive(receiveBytes);

                if (count < 8) // 检查是否至少接收了8个字节
                {
                    throw new Exception("接收到的数据不完整，操作失败。");
                }

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

            return result;
        }

        public ResultInfo WriteString(string address, string value, ModbusTcpClient client)
        {
            ResultInfo result = new ResultInfo();
            byte functionCode = ModbusFunctionCode.WriteMultiRegister;

            ModbusAddress modAddress = new ModbusAddress(address, functionCode);
            modAddress.ModbusConnectInfo = client.ConnectionInfo;
            modAddress.MessageCode = client.MessageCode;

            byte[] tmp = DataConvert.TransByte(value, Encoding.ASCII);

            byte[] values = DataConvert.ArrayExpandToLengthEven<byte>(tmp);

            byte[] tcpBuffer = GetSendTcpBytes(modAddress, modAddress.ModbusConnectInfo.Station, functionCode, 0, values);

            try
            {
                try
                {
                    client.Client.Send(tcpBuffer);
                }
                catch (Exception ex)
                {
                    var socketResult = Connection(client.ConnectionInfo.Ip, client.ConnectionInfo.Port);
                    if (socketResult.IsSucceed)
                    {
                        client.Client = socketResult.Client;
                        client.Client.Send(tcpBuffer);
                    }
                    else
                    {
                        throw new Exception($"PLC服务器:{client.ConnectionInfo.Ip} 的连接已断开，并且尝试重新连接也失败。连接失败异常信息:{socketResult.Message}");
                    }
                }

                byte[] receiveBytes = new byte[12]; // 9 调整为至少12个字节以包括MBAP头
                int count = client.Client.Receive(receiveBytes);

                if (count < 8) // 检查是否至少接收了8个字节
                {
                    throw new Exception("接收到的数据不完整，操作失败。");
                }

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

            return result;
        }

        public ModbusResultInfo<bool[]> ReadBool(string address, ushort length, ModbusTcpClient client, ushort messageCode)
        {
            ModbusResultInfo<bool[]> result = new ModbusResultInfo<bool[]>();

            byte functionCode = ModbusFunctionCode.ReadCoil;
            ushort newLength = 0;

            ModbusAddress modAddress = new ModbusAddress(address, functionCode);
            modAddress.ModbusConnectInfo = client.ConnectionInfo;
            modAddress.MessageCode = messageCode;


            bool[] readMultiResult = new bool[length];
            int resultIndex = 0;
            ushort currentLength = length;
            try
            {
                List<byte> byteResult = new List<byte>();

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

                    try
                    {
                        client.Client.Send(tcpBuffer);
                    }
                    catch (Exception ex)
                    {
                        var socketResult = Connection(client.ConnectionInfo.Ip, client.ConnectionInfo.Port);
                        if (socketResult.IsSucceed)
                        {
                            client.Client = socketResult.Client;
                            client.Client.Send(tcpBuffer);
                        }
                        else
                        {
                            throw new Exception($"PLC服务器:{client.ConnectionInfo.Ip} 的连接已断开，并且尝试重新连接也失败。连接失败异常信息:{socketResult.Message}");
                        }
                    }

                    byte[] receiveBytes = new byte[newLength * 2 + 9];
                    int count = client.Client.Receive(receiveBytes);

                    var resultInfo = CompareBuffer(tcpBuffer, receiveBytes);
                    if (!resultInfo.IsSucceed)
                    {
                        throw new Exception(resultInfo.Message);
                    }

                    List<byte> byteList = new List<byte>(receiveBytes);
                    byteList.RemoveRange(0, 9);
                    byteResult.AddRange(byteList);
                    modAddress.Address += newLength;

                }
                result = ModbusResult.ReturnSucceed<bool[]>(DataConvert.ByteToBoolArray(byteResult.ToArray(), currentLength));
            }
            catch (Exception ex)
            {
                result = ModbusResult.ReturnFailed<bool[]>($"读取数据失败:{ex.Message}", new bool[0]);
            }

            return result;
        }

        public ModbusResultInfo<short[]> ReadInt16(string address, ushort length, ModbusTcpClient client, ushort messageCode)
        {
            ModbusResultInfo<short[]> result = new ModbusResultInfo<short[]>();

            byte functionCode = ModbusFunctionCode.ReadRegister;

            ModbusAddress modAddress = new ModbusAddress(address, functionCode);
            modAddress.ModbusConnectInfo = client.ConnectionInfo;
            modAddress.MessageCode = messageCode;


            ushort newLength = 0;
            short[] readMultiResult = new short[length];
            ushort currentLength = length;
            int resultIndex = 0;

            try
            {
                List<byte> byteResult = new List<byte>();
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
                        length = 0;
                    }
                    resultIndex = newLength + resultIndex;


                    byte[] tcpBuffer = GetSendTcpBytes(modAddress, modAddress.ModbusConnectInfo.Station, functionCode, newLength);

                    try
                    {
                        client.Client.Send(tcpBuffer);
                    }
                    catch (Exception ex)
                    {
                        var socketResult = Connection(client.ConnectionInfo.Ip, client.ConnectionInfo.Port);
                        if (socketResult.IsSucceed)
                        {
                            client.Client = socketResult.Client;
                            client.Client.Send(tcpBuffer);
                        }
                        else
                        {
                            throw new Exception($"PLC服务器:{client.ConnectionInfo.Ip} 的连接已断开，并且尝试重新连接也失败。连接失败异常信息:{socketResult.Message}");
                        }
                    }

                    byte[] receiveBytes = new byte[newLength * 2 + 9];
                    int count = client.Client.Receive(receiveBytes);

                    var resultInfo = CompareBuffer(tcpBuffer, receiveBytes);
                    if (!resultInfo.IsSucceed)
                    {
                        throw new Exception(resultInfo.Message);
                    }

                    List<byte> byteList = new List<byte>(receiveBytes);
                    byteList.RemoveRange(0, 9);

                    byteResult.AddRange(byteList);

                    modAddress.Address += newLength;

                }
                result = ModbusResult.ReturnSucceed<short[]>(DataConvert.TransShort(byteResult.ToArray(), 0, currentLength));

            }
            catch (Exception ex)
            {
                return ModbusResult.ReturnFailed<short[]>(ex.Message, new short[0]);
            }

            return result;
        }

        public ModbusResultInfo<byte[]> ReadByte(string address, ushort length, ModbusTcpClient client, ushort messageCode)
        {
            ModbusResultInfo<byte[]> result = new ModbusResultInfo<byte[]>();

            byte functionCode = ModbusFunctionCode.ReadRegister;

            ModbusAddress modAddress = new ModbusAddress(address, functionCode);
            modAddress.ModbusConnectInfo = client.ConnectionInfo;
            modAddress.MessageCode = messageCode;

            ushort newLength = 0;
            short[] readMultiResult = new short[length];
            ushort currentLength = length;
            int resultIndex = 0;

            try
            {
                List<byte> byteResult = new List<byte>();
                while (length > 0)
                {
                    if (length > 200)
                    {
                        newLength = 200;
                        length = (ushort)(length - 200);

                    }
                    else
                    {
                        newLength = length;
                        length = 0;
                    }
                    resultIndex = newLength + resultIndex;
                    byte[] tcpBuffer = GetSendTcpBytes(modAddress, modAddress.ModbusConnectInfo.Station, functionCode, (ushort)(newLength / 2));
                    try
                    {
                        client.Client.Send(tcpBuffer);
                    }
                    catch (Exception ex)
                    {
                        var socketResult = Connection(client.ConnectionInfo.Ip, client.ConnectionInfo.Port);
                        if (socketResult.IsSucceed)
                        {
                            client.Client = socketResult.Client;
                            client.Client.Send(tcpBuffer);
                        }
                        else
                        {
                            throw new Exception($"PLC服务器:{client.ConnectionInfo.Ip} 的连接已断开，并且尝试重新连接也失败。连接失败异常信息:{socketResult.Message}");
                        }
                    }

                    byte[] receiveBytes = new byte[newLength + 9];
                    int count = client.Client.Receive(receiveBytes);

                    var resultInfo = CompareBuffer(tcpBuffer, receiveBytes);
                    if (!resultInfo.IsSucceed)
                    {
                        throw new Exception(resultInfo.Message);
                    }

                    List<byte> byteList = new List<byte>(receiveBytes);
                    byteList.RemoveRange(0, 9);

                    byteResult.AddRange(byteList);

                    modAddress.Address += newLength;
                }
                result = ModbusResult.ReturnSucceed<byte[]>(byteResult.ToArray());

                #region short 转换
                byte[] newBuffer = new byte[2];
                newBuffer[0] = result.Value[1];
                newBuffer[1] = result.Value[0];
                var resultShort = BitConverter.ToInt16(newBuffer, 0);
                #endregion

                #region int 转换
                byte[] intBuffer = new byte[4];
                intBuffer[0] = result.Value[3];
                intBuffer[1] = result.Value[2];
                intBuffer[2] = result.Value[1];
                intBuffer[3] = result.Value[0];
                var resultFirst = BitConverter.ToInt32(intBuffer, 0);
                #endregion

            }
            catch (Exception ex)
            {
                return ModbusResult.ReturnFailed<byte[]>(ex.Message, new byte[0]);
            }
            return result;
        }
        public ModbusResultInfo<int[]> ReadInt32(string address, ushort length, ModbusTcpClient client, ushort messageCode)
        {
            ModbusResultInfo<int[]> result = new ModbusResultInfo<int[]>();

            byte functionCode = ModbusFunctionCode.ReadRegister;

            ModbusAddress modAddress = new ModbusAddress(address, functionCode);
            modAddress.ModbusConnectInfo = client.ConnectionInfo;
            modAddress.MessageCode = messageCode;

            ushort newLength = 0;
            int[] readMultiResult = new int[length];
            int resultIndex = 0;
            ushort currentLength = length;
            try
            {
                List<byte> byteResult = new List<byte>();

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
                        length = 0;
                    }
                    resultIndex += newLength;

                    byte[] tcpBuffer = GetSendTcpBytes(modAddress, modAddress.ModbusConnectInfo.Station, functionCode, (ushort)(newLength * 2));

                    try
                    {
                        client.Client.Send(tcpBuffer);
                    }
                    catch (Exception ex)
                    {
                        var socketResult = Connection(client.ConnectionInfo.Ip, client.ConnectionInfo.Port);
                        if (socketResult.IsSucceed)
                        {
                            client.Client = socketResult.Client;
                            client.Client.Send(tcpBuffer);
                        }
                        else
                        {
                            throw new Exception($"PLC服务器:{client.ConnectionInfo.Ip} 的连接已断开，并且尝试重新连接也失败。连接失败异常信息:{socketResult.Message}");
                        }
                    }

                    byte[] receiveBytes = new byte[newLength * 4 + 9];
                    int count = client.Client.Receive(receiveBytes);

                    var resultInfo = CompareBuffer(tcpBuffer, receiveBytes);
                    if (!resultInfo.IsSucceed)
                    {
                        throw new Exception(resultInfo.Message);
                    }

                    List<byte> byteList = new List<byte>(receiveBytes);
                    byteList.RemoveRange(0, 9);
                    byteResult.AddRange(byteList);

                    modAddress.Address += newLength;
                }
                result = ModbusResult.ReturnSucceed<int[]>(DataConvert.TransInt32(byteResult.ToArray(), 0, currentLength));

            }
            catch (Exception ex)
            {
                return ModbusResult.ReturnFailed<int[]>(ex.Message, new int[0]);
            }

            return result;
        }

        public ModbusResultInfo<float[]> ReadFloat(string address, ushort length, ModbusTcpClient client, ushort messageCode)
        {
            ModbusResultInfo<float[]> result = new ModbusResultInfo<float[]>();

            byte functionCode = ModbusFunctionCode.ReadRegister;

            ModbusAddress modAddress = new ModbusAddress(address, functionCode);
            modAddress.ModbusConnectInfo = client.ConnectionInfo;
            modAddress.MessageCode = messageCode;

            ushort newLength = 0;
            float[] readMultiResult = new float[length];
            int resultIndex = 0;
            ushort currentLength = length;
            try
            {
                List<byte> byteResult = new List<byte>();

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
                        length = 0;
                    }
                    resultIndex += newLength;

                    byte[] tcpBuffer = GetSendTcpBytes(modAddress, modAddress.ModbusConnectInfo.Station, functionCode, (ushort)(newLength * 2));

                    try
                    {
                        client.Client.Send(tcpBuffer);
                    }
                    catch (Exception ex)
                    {
                        var socketResult = Connection(client.ConnectionInfo.Ip, client.ConnectionInfo.Port);
                        if (socketResult.IsSucceed)
                        {
                            client.Client = socketResult.Client;
                            client.Client.Send(tcpBuffer);
                        }
                        else
                        {
                            throw new Exception($"PLC服务器:{client.ConnectionInfo.Ip} 的连接已断开，并且尝试重新连接也失败。连接失败异常信息:{socketResult.Message}");
                        }
                    }

                    byte[] receiveBytes = new byte[newLength * 4 + 9];
                    int count = client.Client.Receive(receiveBytes);

                    var resultInfo = CompareBuffer(tcpBuffer, receiveBytes);
                    if (!resultInfo.IsSucceed)
                    {
                        throw new Exception(resultInfo.Message);
                    }

                    List<byte> byteList = new List<byte>(receiveBytes);
                    byteList.RemoveRange(0, 9);
                    byteResult.AddRange(byteList);
                    modAddress.Address += newLength;
                }
                result = ModbusResult.ReturnSucceed<float[]>(DataConvert.TransSingle(byteResult.ToArray(), 0, currentLength));

            }
            catch (Exception ex)
            {
                return ModbusResult.ReturnFailed<float[]>(ex.Message, new float[0]);
            }

            return result;
        }

        public ModbusResultInfo<string> ReadString(string address, ushort length, ModbusTcpClient client, ushort messageCode)
        {
            ModbusResultInfo<string> result = new ModbusResultInfo<string>();

            byte functionCode = ModbusFunctionCode.ReadRegister;

            ModbusAddress modAddress = new ModbusAddress(address, functionCode);
            modAddress.ModbusConnectInfo = client.ConnectionInfo;
            modAddress.MessageCode = messageCode;

            try
            {
                byte[] tcpBuffer = GetSendTcpBytes(modAddress, modAddress.ModbusConnectInfo.Station, functionCode, length);

                try
                {
                    client.Client.Send(tcpBuffer);
                }
                catch (Exception ex)
                {
                    var socketResult = Connection(client.ConnectionInfo.Ip, client.ConnectionInfo.Port);
                    if (socketResult.IsSucceed)
                    {
                        client.Client = socketResult.Client;
                        client.Client.Send(tcpBuffer);
                    }
                    else
                    {
                        throw new Exception($"PLC服务器:{client.ConnectionInfo.Ip} 的连接已断开，并且尝试重新连接也失败。连接失败异常信息:{socketResult.Message}");
                    }
                }

                byte[] receiveBytes = new byte[length * 2 + 9];
                int count = client.Client.Receive(receiveBytes);

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

            return result;
        }

        public ResultInfo WriteBool(string address, bool value, ModbusTcpClient client, ushort messageCode)
        {
            ResultInfo result = new ResultInfo();
            byte functionCode = ModbusFunctionCode.WriteSingleCoil;

            ModbusAddress modAddress = new ModbusAddress(address, functionCode);
            modAddress.ModbusConnectInfo = client.ConnectionInfo;
            modAddress.MessageCode = messageCode;

            byte[] tcpBuffer = GetSendTcpBytes(modAddress, modAddress.ModbusConnectInfo.Station, functionCode, 0, value);

            try
            {
                try
                {
                    client.Client.Send(tcpBuffer);
                }
                catch (Exception ex)
                {
                    var socketResult = Connection(client.ConnectionInfo.Ip, client.ConnectionInfo.Port);
                    if (socketResult.IsSucceed)
                    {
                        client.Client = socketResult.Client;
                        client.Client.Send(tcpBuffer);
                    }
                    else
                    {
                        throw new Exception($"PLC服务器:{client.ConnectionInfo.Ip} 的连接已断开，并且尝试重新连接也失败。连接失败异常信息:{socketResult.Message}");
                    }
                }

                byte[] receiveBytes = new byte[12]; // 9 调整为至少12个字节以包括MBAP头
                int count = client.Client.Receive(receiveBytes);

                if (count < 8) // 检查是否至少接收了8个字节
                {
                    throw new Exception("接收到的数据不完整，操作失败。");
                }

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

            return result;
        }

        public ResultInfo WriteInt16(string address, short[] value, ModbusTcpClient client, ushort messageCode)
        {
            ResultInfo result = new ResultInfo();
            byte functionCode = ModbusFunctionCode.WriteMultiRegister;

            ModbusAddress modAddress = new ModbusAddress(address, functionCode);
            modAddress.ModbusConnectInfo = client.ConnectionInfo;
            modAddress.MessageCode = messageCode;

            byte[] values = DataConvert.TransReverseByte(value);

            byte[] tcpBuffer = GetSendTcpBytes(modAddress, modAddress.ModbusConnectInfo.Station, functionCode, 0, values);

            try
            {
                try
                {
                    client.Client.Send(tcpBuffer);
                }
                catch (Exception ex)
                {
                    var socketResult = Connection(client.ConnectionInfo.Ip, client.ConnectionInfo.Port);
                    if (socketResult.IsSucceed)
                    {
                        client.Client = socketResult.Client;
                        client.Client.Send(tcpBuffer);
                    }
                    else
                    {
                        throw new Exception($"PLC服务器:{client.ConnectionInfo.Ip} 的连接已断开，并且尝试重新连接也失败。连接失败异常信息:{socketResult.Message}");
                    }
                }

                byte[] receiveBytes = new byte[12]; // 9 调整为至少12个字节以包括MBAP头
                int count = client.Client.Receive(receiveBytes);

                if (count < 8) // 检查是否至少接收了8个字节
                {
                    throw new Exception("接收到的数据不完整，操作失败。");
                }

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

            return result;
        }

        public ResultInfo WriteInt32(string address, int[] value, ModbusTcpClient client, ushort messageCode)
        {
            ResultInfo result = new ResultInfo();
            byte functionCode = ModbusFunctionCode.WriteMultiRegister;

            ModbusAddress modAddress = new ModbusAddress(address, functionCode);
            modAddress.ModbusConnectInfo = client.ConnectionInfo;
            modAddress.MessageCode = messageCode;

            byte[] values = DataConvert.TransReverseByte(value);

            byte[] tcpBuffer = GetSendTcpBytes(modAddress, modAddress.ModbusConnectInfo.Station, functionCode, 0, values);

            try
            {
                try
                {
                    client.Client.Send(tcpBuffer);
                }
                catch (Exception ex)
                {
                    var socketResult = Connection(client.ConnectionInfo.Ip, client.ConnectionInfo.Port);
                    if (socketResult.IsSucceed)
                    {
                        client.Client = socketResult.Client;
                        client.Client.Send(tcpBuffer);
                    }
                    else
                    {
                        throw new Exception($"PLC服务器:{client.ConnectionInfo.Ip} 的连接已断开，并且尝试重新连接也失败。连接失败异常信息:{socketResult.Message}");
                    }
                }

                byte[] receiveBytes = new byte[12]; // 9 调整为至少12个字节以包括MBAP头
                int count = client.Client.Receive(receiveBytes);

                if (count < 8) // 检查是否至少接收了8个字节
                {
                    throw new Exception("接收到的数据不完整，操作失败。");
                }

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

            return result;
        }

        public ResultInfo WriteFloat(string address, float[] value, ModbusTcpClient client, ushort messageCode)
        {
            ResultInfo result = new ResultInfo();
            byte functionCode = ModbusFunctionCode.WriteMultiRegister;

            ModbusAddress modAddress = new ModbusAddress(address, functionCode);
            modAddress.ModbusConnectInfo = client.ConnectionInfo;
            modAddress.MessageCode = messageCode;

            byte[] values = DataConvert.TransReverseByte(value);

            byte[] tcpBuffer = GetSendTcpBytes(modAddress, modAddress.ModbusConnectInfo.Station, functionCode, 0, values);

            try
            {
                try
                {
                    client.Client.Send(tcpBuffer);
                }
                catch (Exception ex)
                {
                    var socketResult = Connection(client.ConnectionInfo.Ip, client.ConnectionInfo.Port);
                    if (socketResult.IsSucceed)
                    {
                        client.Client = socketResult.Client;
                        client.Client.Send(tcpBuffer);
                    }
                    else
                    {
                        throw new Exception($"PLC服务器:{client.ConnectionInfo.Ip} 的连接已断开，并且尝试重新连接也失败。连接失败异常信息:{socketResult.Message}");
                    }
                }

                byte[] receiveBytes = new byte[12]; // 9 调整为至少12个字节以包括MBAP头
                int count = client.Client.Receive(receiveBytes);

                if (count < 8) // 检查是否至少接收了8个字节
                {
                    throw new Exception("接收到的数据不完整，操作失败。");
                }

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

            return result;
        }

        public ResultInfo WriteString(string address, string value, ModbusTcpClient client, ushort messageCode)
        {
            ResultInfo result = new ResultInfo();
            byte functionCode = ModbusFunctionCode.WriteMultiRegister;

            ModbusAddress modAddress = new ModbusAddress(address, functionCode);
            modAddress.ModbusConnectInfo = client.ConnectionInfo;
            modAddress.MessageCode = messageCode;
            byte[] tmp = DataConvert.TransByte(value, Encoding.ASCII);
            byte[] values = DataConvert.ArrayExpandToLengthEven<byte>(tmp);
            byte[] tcpBuffer = GetSendTcpBytes(modAddress, modAddress.ModbusConnectInfo.Station, functionCode, 0, values);
            try
            {
                try
                {
                    client.Client.Send(tcpBuffer);
                }
                catch (Exception ex)
                {
                    var socketResult = Connection(client.ConnectionInfo.Ip, client.ConnectionInfo.Port);
                    if (socketResult.IsSucceed)
                    {
                        client.Client = socketResult.Client;
                        client.Client.Send(tcpBuffer);
                    }
                    else
                    {
                        throw new Exception($"PLC服务器:{client.ConnectionInfo.Ip} 的连接已断开，并且尝试重新连接也失败。连接失败异常信息:{socketResult.Message}");
                    }
                }

                byte[] receiveBytes = new byte[12]; // 9 调整为至少12个字节以包括MBAP头
                int count = client.Client.Receive(receiveBytes);

                if (count < 8) // 检查是否至少接收了8个字节
                {
                    throw new Exception("接收到的数据不完整，操作失败。");
                }

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

            return result;
        }
    }
}


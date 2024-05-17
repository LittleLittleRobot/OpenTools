using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Net;
using System.Text;
using Wesky.Net.OpenTools.Iot.Scanner.Models;

namespace Wesky.Net.OpenTools.Iot.Scanner
{
    public class CodeReader:ICodeReader
    {
        private readonly object _lock = new object();

        /// <summary>
        /// 触发扫码
        /// Trigger barcode scanning.
        /// </summary>
        /// <param name="clientInfo">客户端信息/Client information</param>
        /// <returns>扫码结果/Scanning result</returns>
        public ReaderResultInfo ReaderRead(ref ReaderClientInfo clientInfo)
        {
            var result = new ReaderResultInfo();
            try
            {
                lock (_lock)
                {
                    var stopwatch = new Stopwatch();
                    stopwatch.Start();
                    result = ReadCommand(ref clientInfo);
                    stopwatch.Stop();

                    result.ElapsedMilliseconds = stopwatch.ElapsedMilliseconds;
                    result.ReaderNo = clientInfo.ReaderNo;
                }
            }
            catch (Exception ex)
            {
                result.IsSucceed = false;
                result.Value = ex.Message;
                result.ReaderNo = clientInfo.ReaderNo;
            }

            return result;
        }

        /// <summary>
        /// 手动关闭扫码器
        /// Manually close the scanner.
        /// </summary>
        /// <param name="clientInfo">客户端信息/Client information</param>
        /// <returns>操作结果/Operation result</returns>
        public ReaderResultInfo ReaderClose(ReaderClientInfo clientInfo)
        {
            var result = new ReaderResultInfo();
            try
            {
                result = CloseReader(clientInfo.Client, clientInfo.CloseCommand);
            }
            catch (Exception ex)
            {
                result.IsSucceed = false;
                result.Message = $"扫码器编号:{clientInfo.ReaderNo} 品牌:{clientInfo.Brand} IP:{clientInfo.Ip} 关闭失败:{ex.Message}";
                result.ReaderNo = clientInfo.ReaderNo;
            }
            return result;
        }

        /// <summary>
        /// 建立与扫码器的连接
        /// Establish a connection with the scanner.
        /// </summary>
        /// <param name="clientInfo">客户端信息/Client information</param>
        /// <returns>连接结果/Connection result</returns>
        public ReaderResultInfo ReaderConnection(ref ReaderClientInfo clientInfo)
        {
            var result = new ReaderResultInfo();
            try
            {
                clientInfo.Client?.Close();
                clientInfo.Client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                clientInfo.Client.SendTimeout = clientInfo.SendTimeOut;
                clientInfo.Client.ReceiveTimeout = clientInfo.ReceiveTimeOut;
                clientInfo.Client.Connect(new IPEndPoint(clientInfo.Ip, clientInfo.Port));
                result.IsSucceed = true;
            }
            catch (Exception ex)
            {
                result.IsSucceed = false;
                result.Message = $"扫码器编号:{clientInfo.ReaderNo} 品牌:{clientInfo.Brand} IP:{clientInfo.Ip} 连接失败:{ex.Message}";
                result.ReaderNo = clientInfo.ReaderNo;
            }
            return result;
        }

        /// <summary>
        /// 发送扫码指令并接收结果
        /// Send scan command and receive the result.
        /// </summary>
        /// <param name="clientInfo">客户端信息包含连接和指令数据/Client information including connection and command data</param>
        /// <returns>扫码结果/Scanning result</returns>
        private ReaderResultInfo ReadCommand(ref ReaderClientInfo clientInfo)
        {
            var result = new ReaderResultInfo();
            int count = clientInfo.Count;
            while (--count >= 0)
            {
                lock (_lock)
                {
                    // 尝试发送指令到扫码器
                    // Attempt to send a command to the scanner
                    if (!TrySendCommand(clientInfo, ref result))
                    {
                        return result;  // 如果发送失败且无法重连，直接返回结果
                    }

                    // 尝试接收扫码器返回的数据
                    // Attempt to receive data from the scanner
                    if (!TryReceiveData(clientInfo, ref result))
                    {
                        continue;  // 如果接收失败，继续尝试
                    }

                    return result;  
                }
            }
            return result; 
        }

        private bool TrySendCommand(ReaderClientInfo clientInfo, ref ReaderResultInfo result)
        {
            try
            {
                byte[] bytes = Encoding.ASCII.GetBytes(clientInfo.Command);
                clientInfo.Client.Send(bytes);
                return true;
            }
            catch (Exception e)
            {
                var res = ReaderConnection(ref clientInfo);
                result.IsSucceed = false;
                result.Message = $"发送失败:{e.Message}, 重试连接失败:{res.Message}";
                result.ReaderNo = clientInfo.ReaderNo;
                return !res.IsSucceed;  // 返回连接尝试的结果
            }
        }

        private bool TryReceiveData(ReaderClientInfo clientInfo, ref ReaderResultInfo result)
        {
            try
            {
                byte[] receiveBytes = new byte[1024];
                int recCount = clientInfo.Client.Receive(receiveBytes);
                if (recCount <= 0)
                {
                    throw new Exception("没有收到扫码器的返回值");
                }
                byte[] receiveData = new byte[recCount];
                Array.Copy(receiveBytes, receiveData, recCount);
                result.Value = Encoding.UTF8.GetString(receiveData);
                result.IsSucceed = true;
                result.Message = "OK";
                return true;
            }
            catch (Exception ee)
            {
                result.IsSucceed = false;
                result.Message = $"接收失败:{ee.Message}";
                return false;
            }
        }

        /// <summary>
        /// 关闭与扫码器的通信
        /// Close communication with the scanner.
        /// </summary>
        /// <param name="client">扫码器的 Socket 连接/Scanner's Socket connection</param>
        /// <param name="command">关闭指令/Close command</param>
        /// <returns>操作结果/Operation result</returns>
        private ReaderResultInfo CloseReader(Socket client, string command)
        {
            var result = new ReaderResultInfo();
            try
            {
                byte[] bytes = Encoding.ASCII.GetBytes(command);
                client.Send(bytes);
                result.IsSucceed = true;
            }
            catch (Exception e)
            {
                result.IsSucceed = false;
                result.Message = $"关闭通信失败:{e.Message}";
            }
            return result;
        }
    }

}

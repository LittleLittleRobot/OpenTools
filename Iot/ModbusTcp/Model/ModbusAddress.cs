using System;
using System.Collections.Generic;
using System.Text;

namespace Wesky.Net.OpenTools.Iot.ModbusTcp.Model
{

    public class ModbusAddress
    {
        public ModbusConnectionInfo ModbusConnectInfo { get; set; }
        public ModbusAddress()
        {
            Station = -1;
            Function = -1;
            Address = 0;
        }

        public ModbusAddress(string address, byte function, ModbusConnectionInfo connectionInfo)
        {
            ModbusConnectInfo = connectionInfo;
            Station = -1;
            Function = function;
            Address = 0;
            Parse(address);
        }

        /// <summary>
        /// 实例化一个默认的对象，使用默认的地址初始化
        /// </summary>
        /// <param name="address">传入的地址信息，支持富地址，例如s=2;x=3;100</param>
        public ModbusAddress(string address)
        {
            Station = -1;
            Function = -1;
            Address = 0;
            Parse(address);
        }

        /// <summary>
        /// 实例化一个默认的对象，使用默认的地址初始化
        /// </summary>
        /// <param name="address">传入的地址信息，支持富地址，例如s=2;x=3;100</param>
        /// <param name="function">默认的功能码信息</param>
        public ModbusAddress(string address, byte function)
        {
            Station = -1;
            Function = function;
            Address = 0;
            Parse(address);
        }

        /// <summary>
        /// 实例化一个默认的对象，使用默认的地址初始化
        /// </summary>
        /// <param name="station">站号信息</param>
        /// <param name="function">功能码信息</param>
        /// <param name="address">地址信息</param>
        public ModbusAddress(byte station, byte function, ushort address)
        {
            Station = -1;
            Function = function;
            Address = 0;
        }

        /// <summary>
        /// 起始地址
        /// </summary>
        public ushort Address { get; set; }
        /// <summary>
        /// Modbus站号
        /// </summary>
        public int Station { get; set; }
        /// <summary>
        /// 功能码
        /// </summary>
        public int Function { get; set; }
        /// <summary>
        /// 传输标识 每次通信需要自动+1
        /// </summary>
        public ushort MessageCode { get; set; }

        /// <summary>
        /// 解析字符串的地址
        /// </summary>
        /// <param name="address">地址信息</param>
        public virtual void Parse(string address)
        {
            if (address.IndexOf(';') < 0)
            {
                // 正常地址，功能码03
                Address = ushort.Parse(address);
            }
            else
            {
                // 带功能码的地址
                string[] list = address.Split(';');
                for (int i = 0; i < list.Length; i++)
                {
                    if (list[i][0] == 's' || list[i][0] == 'S')
                    {
                        // 站号信息
                        this.Station = byte.Parse(list[i].Substring(2));
                    }
                    else if (list[i][0] == 'x' || list[i][0] == 'X')
                    {
                        this.Function = byte.Parse(list[i].Substring(2));
                    }
                    else
                    {
                        this.Address = ushort.Parse(list[i]);
                    }
                }
            }
        }
        /// <summary>
        /// 返回表示当前对象的字符串
        /// </summary>
        /// <returns>字符串数据</returns>
        public override string ToString()
        {
            return Address.ToString();
        }

        public ModbusAddress AddressAdd(int value)
        {
            return new ModbusAddress()
            {
                Station = this.Station,
                Function = this.Function,
                Address = (ushort)(this.Address + value),
            };
        }
    }
}

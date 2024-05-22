using System;
using System.Collections.Generic;
using System.Text;

namespace Wesky.Net.OpenTools.Iot.ModbusTcp.Model
{
    public class ModbusFunctionCode
    {
        /// <summary>
        /// 读取线圈状态 寄存器PLC地址 00001 - 09999
        /// Read coil status. Register PLC address 00001 - 09999
        /// </summary>
        public const byte ReadCoil = 0x01;

        /// <summary>
        /// 读取可输入的离散量 寄存器PLC地址 10001 - 19999
        /// Read input discrete values. Register PLC address 10001 - 19999
        /// </summary>
        public const byte ReadInputDiscrete = 0x02;

        /// <summary>
        /// 读取保持寄存器 40001 - 49999
        /// Read holding registers. Address range 40001 - 49999
        /// </summary>
        public const byte ReadRegister = 0x03;

        /// <summary>
        /// 读取可输入寄存器 30001 - 39999
        /// Read input registers. Address range 30001 - 39999
        /// </summary>
        public const byte ReadInputRegister = 0x04;

        /// <summary>
        /// 写单个线圈 00001 - 09999
        /// Write a single coil. Address range 00001 - 09999
        /// </summary>
        public const byte WriteSingleCoil = 0x05;

        /// <summary>
        /// 写单个保持寄存器 40001 - 49999
        /// Write a single holding register. Address range 40001 - 49999
        /// </summary>
        public const byte WriteSingleRegister = 0x06;

        /// <summary>
        /// 写多个线圈 00001 - 09999
        /// Write multiple coils. Address range 00001 - 09999
        /// </summary>
        public const byte WriteMultiCoil = 0x0F;

        /// <summary>
        /// 写多个保持寄存器 40001 - 49999
        /// Write multiple holding registers. Address range 40001 - 49999
        /// </summary>
        public const byte WriteMultiRegister = 0x10;

        /// <summary>
        /// 查询从站状态信息（串口通信使用）
        /// Query slave status information (used for serial communication)
        /// </summary>
        public const byte SelectSlave = 0x11;

        /// <summary>
        /// 非法功能码
        /// Illegal function code
        /// </summary>
        public const byte FunctionCodeNotSupport = 0x01;

        /// <summary>
        /// 非法数据地址
        /// Illegal data address
        /// </summary>
        public const byte DataAddressNotSupport = 0x02;

        /// <summary>
        /// 非法数据值
        /// Illegal data value
        /// </summary>
        public const byte DataValueNotSupport = 0x03;

        /// <summary>
        /// 从站设备异常
        /// Slave device failure
        /// </summary>
        public const byte DeviceNotWork = 0x04;

        /// <summary>
        /// 请求已确认，但是需要更长时间进行处理请求
        /// Request acknowledged but requires longer processing time
        /// </summary>
        public const byte LongTimeResponse = 0x05;

        /// <summary>
        /// 设备繁忙
        /// Device busy
        /// </summary>
        public const byte DeviceBusy = 0x06;

        /// <summary>
        /// 奇偶性错误
        /// Parity error
        /// </summary>
        public const byte OddEvenError = 0x08;

        /// <summary>
        /// 网关错误
        /// Gateway error
        /// </summary>
        public const byte GatewayNotSupport = 0x0A;

        /// <summary>
        /// 网关设备响应失败
        /// Gateway device response timeout
        /// </summary>
        public const byte GatewayDeviceResponseTimeout = 0x0B;


        /// <summary>
        /// 根据错误码获取描述信息
        /// Get description by error code
        /// </summary>
        /// <param name="code">错误码</param>
        /// <returns>错误描述</returns>
        public static string GetDescriptionByErrorCode(byte code)
        {
            switch (code)
            {
                case FunctionCodeNotSupport:
                    return "非法功能码 - Illegal function code";
                case DataAddressNotSupport:
                    return "非法数据地址 - Illegal data address";
                case DataValueNotSupport:
                    return "非法数据值 - Illegal data value";
                case DeviceNotWork:
                    return "从站设备异常 - Slave device failure";
                case LongTimeResponse:
                    return "请求已确认，但是需要更长时间进行处理请求 - Request acknowledged but requires longer processing time";
                case DeviceBusy:
                    return "设备繁忙 - Device busy";
                case OddEvenError:
                    return "奇偶性错误 - Parity error";
                case GatewayNotSupport:
                    return "网关错误 - Gateway error";
                case GatewayDeviceResponseTimeout:
                    return "网关设备响应失败 - Gateway device response timeout";

                default:
                    return "未知错误 - Unknown error";
            }
        }


    }
}

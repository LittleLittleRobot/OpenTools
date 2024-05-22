using System;
using System.Collections.Generic;
using System.Text;

namespace Wesky.Net.OpenTools.Iot.ModbusTcp.Model
{
    public class ModbusResultInfo<T> : ResultInfo
    {
        public T Value { get; set; }

    }

    public class ModbusResult
    {
        /// <summary>
        /// 返回成功信息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static ModbusResultInfo<T> ReturnSucceed<T>(T value)
        {
            return new ModbusResultInfo<T>()
            {
                IsSucceed = true,
                Code = 0,
                Message = "Ok",
                Value = value
            };
        }

        /// <summary>
        /// 返回失败信息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static ModbusResultInfo<T> ReturnFailed<T>(string message, T value)
        {
            return new ModbusResultInfo<T>()
            {
                IsSucceed = false,
                Code = 0,
                Message = message,
                Value = value
            };
        }
    }
}

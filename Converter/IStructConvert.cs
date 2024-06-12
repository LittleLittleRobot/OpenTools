using System;
using System.Collections.Generic;
using System.Text;

namespace Wesky.Net.OpenTools.Converter
{
    /// <summary>
    /// IStructConvert 接口，提供结构体与字节数组之间的序列化和反序列化功能。
    /// IStructConvert interface, providing serialization and deserialization functionality between structures and byte arrays.
    /// </summary>
    public interface IStructConvert
    {
        /// <summary>
        /// 将字节数组反序列化为结构体。
        /// Deserializes a byte array into a structure.
        /// </summary>
        /// <typeparam name="T">结构体的类型。</typeparam>
        /// <param name="data">包含结构体数据的字节数组。</param>
        /// <returns>反序列化后的结构体实例。</returns>
        byte[] StructToBytes<T>(T structure) where T : struct;

        /// <summary>
        /// 将结构体实例转换为字节数组。
        /// Converts a structure instance into a byte array.
        /// </summary>
        /// <typeparam name="T">要转换的结构体类型，必须是值类型。</typeparam>
        /// <param name="structure">要转换的结构体实例。</param>
        /// <returns>表示结构体数据的字节数组。</returns>
        T BytesToStruct<T>(byte[] data) where T : struct;
    }
}

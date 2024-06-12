using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Wesky.Net.OpenTools.Converter
{
    /// <summary>
    /// 实现IStructConvert接口，提供结构体与字节数组间的基本转换功能。
    /// Implements the IStructConvert interface to provide conversion between structures and byte arrays.
    /// </summary>
    public class MarshalConvert : IStructConvert
    {
        /// <summary>
        /// 将字节数组转换为指定类型的结构体实例。
        /// Converts a byte array into an instance of the specified type of structure.
        /// </summary>
        /// <typeparam name="T">要转换的结构体类型，必须是值类型。</typeparam>
        /// <param name="data">包含结构体数据的字节数组。</param>
        /// <returns>转换后的结构体实例。</returns>
        public T BytesToStruct<T>(byte[] data) where T : struct
        {
            T structure;
            // 计算结构体类型T的内存大小
            // Calculate the memory size of the structure type T
            int size = Marshal.SizeOf(typeof(T));
            // 分配相应大小的内存缓冲区
            // Allocate a memory buffer of the appropriate size
            IntPtr buffer = Marshal.AllocHGlobal(size);
            try
            {
                // 将字节数组复制到分配的内存中
                // Copy the byte array to the allocated memory
                Marshal.Copy(data, 0, buffer, size);
                // 将内存缓冲区转换为指定的结构体
                // Convert the memory buffer to the specified structure
                structure = Marshal.PtrToStructure<T>(buffer);
            }
            finally
            {
                // 释放内存缓冲区
                // Free the memory buffer
                Marshal.FreeHGlobal(buffer);
            }
            return structure;
        }

        /// <summary>
        /// 将结构体实例转换为字节数组。
        /// Converts a structure instance into a byte array.
        /// </summary>
        /// <typeparam name="T">要转换的结构体类型，必须是值类型。</typeparam>
        /// <param name="structure">要转换的结构体实例。</param>
        /// <returns>表示结构体数据的字节数组。</returns>
        public byte[] StructToBytes<T>(T structure) where T : struct
        {
            // 计算结构体实例的内存大小
            // Calculate the memory size of the structure instance
            int size = Marshal.SizeOf(structure);
            byte[] array = new byte[size];
            // 分配相应大小的内存缓冲区
            // Allocate a memory buffer of the appropriate size
            IntPtr buffer = Marshal.AllocHGlobal(size);
            try
            {
                // 将结构体实例复制到内存缓冲区
                // Copy the structure instance to the memory buffer
                Marshal.StructureToPtr(structure, buffer, false);
                // 将内存缓冲区的数据复制到字节数组
                // Copy the data from the memory buffer to the byte array
                Marshal.Copy(buffer, array, 0, size);
            }
            finally
            {
                // 释放内存缓冲区
                // Free the memory buffer
                Marshal.FreeHGlobal(buffer);
            }
            return array;
        }
    }
}

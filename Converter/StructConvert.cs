using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Wesky.Net.OpenTools.Converter
{
    /// <summary>
    /// 实现 IStructConvert 接口，提供结构体与字节数组之间的序列化和反序列化功能。
    /// Implements the IStructConvert interface, providing serialization and deserialization functionality between structures and byte arrays.
    /// </summary>
    public class StructConvert:IStructConvert
    {
        /// <summary>
        /// 将结构体序列化为字节数组。
        /// Serializes a structure into a byte array.
        /// </summary>
        /// <typeparam name="T">结构体的类型。</typeparam>
        /// <param name="structure">要序列化的结构体实例。</param>
        /// <returns>表示结构体的字节数组。</returns>
        public byte[] StructToBytes<T>(T structure) where T : struct
        {
            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                SerializeObject(bw, structure);
                return ms.ToArray();
            }
        }

        /// <summary>
        /// 将字节数组反序列化为结构体。
        /// Deserializes a byte array into a structure.
        /// </summary>
        /// <typeparam name="T">结构体的类型。</typeparam>
        /// <param name="data">包含结构体数据的字节数组。</param>
        /// <returns>反序列化后的结构体实例。</returns>
        /// <exception cref="InvalidOperationException">当数据不完整时抛出，无法完成反序列化。</exception>
        public T BytesToStruct<T>(byte[] data) where T : struct
        {
            using (var ms = new MemoryStream(data))
            using (var br = new BinaryReader(ms))
            {
                try
                {
                    return (T)DeserializeObject(typeof(T), br);
                }
                catch (EndOfStreamException e)
                {
                    throw new InvalidOperationException("数据不完整，无法反序列化", e);
                }
            }
        }

        /// <summary>
        /// 将对象序列化为二进制数据。
        /// Serializes an object into binary data.
        /// </summary>
        /// <param name="bw">用于写入序列化数据的BinaryWriter实例。</param>
        /// <param name="obj">需要序列化的对象。</param>
        private void SerializeObject(BinaryWriter bw, object obj)
        {
            var fields = obj.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var field in fields)
            {
                var fieldValue = field.GetValue(obj);
                WriteField(bw, fieldValue, field.FieldType);
            }
        }

        /// <summary>
        /// 从二进制数据反序列化为对象实例。
        /// Deserializes binary data into an object instance.
        /// </summary>
        /// <param name="type">要反序列化的对象类型。</param>
        /// <param name="br">用于读取序列化数据的BinaryReader实例。</param>
        /// <returns>反序列化后的对象实例。</returns>
        private object DeserializeObject(Type type, BinaryReader br)
        {
            object obj = Activator.CreateInstance(type);
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var field in fields)
            {
                object fieldValue = ReadField(br, field.FieldType);
                field.SetValue(obj, fieldValue);
            }
            return obj;
        }

        /// <summary>
        /// 将字段值写入二进制写入器。
        /// Writes field values into a binary writer.
        /// </summary>
        /// <param name="bw">BinaryWriter实例。</param>
        /// <param name="fieldValue">字段的值。</param>
        /// <param name="fieldType">字段的类型。</param>
        private void WriteField(BinaryWriter bw, object fieldValue, Type fieldType)
        {
            if (fieldType == typeof(int))
            {
                bw.Write((int)fieldValue);
            }
            else if (fieldType == typeof(string))
            {
                byte[] strBytes = Encoding.UTF8.GetBytes((string)fieldValue);
                bw.Write(strBytes.Length);
                bw.Write(strBytes);
            }
            else if (fieldType == typeof(byte[]))
            {
                byte[] bytes = (byte[])fieldValue;
                bw.Write(bytes.Length);
                bw.Write(bytes);
            }
            else if (fieldType == typeof(char[]))
            {
                char[] chars = (char[])fieldValue;
                bw.Write(chars.Length);
                foreach (var c in chars)
                {
                    bw.Write(c);
                }
            }
            else if (fieldType.IsArray)
            {
                Array array = (Array)fieldValue;
                bw.Write(array.Length);
                foreach (var item in array)
                {
                    WriteField(bw, item, fieldType.GetElementType());
                }
            }
            else if (fieldType.IsValueType || fieldType.IsEnum)
            {
                SerializeObject(bw, fieldValue);
            }
            else
            {
                throw new NotImplementedException("不支持序列化此类型的属性: " + fieldType);
            }
        }

        /// <summary>
        /// 从二进制读取器中读取字段值。
        /// Reads field values from a binary reader.
        /// </summary>
        /// <param name="br">BinaryReader实例。</param>
        /// <param name="fieldType">字段的类型。</param>
        /// <returns>读取的字段值。</returns>
        private object ReadField(BinaryReader br, Type fieldType)
        {
            if (fieldType == typeof(int))
            {
                return br.ReadInt32();
            }
            else if (fieldType == typeof(string))
            {
                int length = br.ReadInt32();
                return Encoding.UTF8.GetString(br.ReadBytes(length));
            }
            else if (fieldType == typeof(byte[]))
            {
                int length = br.ReadInt32();
                return br.ReadBytes(length);
            }
            else if (fieldType == typeof(char[]))
            {
                int length = br.ReadInt32();
                char[] chars = new char[length];
                for (int i = 0; i < length; i++)
                {
                    chars[i] = br.ReadChar();
                }
                return chars;
            }
            else if (fieldType.IsArray)
            {
                int length = br.ReadInt32();
                Array array = Array.CreateInstance(fieldType.GetElementType(), length);
                for (int i = 0; i < length; i++)
                {
                    array.SetValue(ReadField(br, fieldType.GetElementType()), i);
                }
                return array;
            }
            else if (fieldType.IsValueType || fieldType.IsEnum)
            {
                return DeserializeObject(fieldType, br);
            }
            else
            {
                throw new NotImplementedException("不支持反序列化此类型的属性: " + fieldType);
            }
        }
    }
}

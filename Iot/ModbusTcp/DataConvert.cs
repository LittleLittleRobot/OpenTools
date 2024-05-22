using System;
using System.Collections.Generic;
using System.Text;

namespace Wesky.Net.OpenTools.Iot.ModbusTcp
{

    public class DataConvert
    {
        /// <summary>
        /// Converts a byte array to a boolean array representing each bit.
        /// 将字节数组转换为表示每个位的布尔数组。
        /// </summary>
        /// <param name="bytes">The byte array to convert. 输入的字节数组。</param>
        /// <param name="length">The number of bits to convert. 要转换的位数。</param>
        /// <returns>A boolean array or null if input is null. 布尔数组，如果输入为 null 则返回 null。</returns>
        public static bool[] ByteToBoolArray(byte[] InBytes, int length)
        {
            if (InBytes == null) return null;

            if (length > InBytes.Length * 8) length = InBytes.Length * 8;
            bool[] buffer = new bool[length];

            for (int i = 0; i < length; i++)
            {
                int index = i / 8;
                int offect = i % 8;

                byte temp = 0;
                switch (offect)
                {
                    case 0: temp = 0x01; break;
                    case 1: temp = 0x02; break;
                    case 2: temp = 0x04; break;
                    case 3: temp = 0x08; break;
                    case 4: temp = 0x10; break;
                    case 5: temp = 0x20; break;
                    case 6: temp = 0x40; break;
                    case 7: temp = 0x80; break;
                    default: break;
                }

                if ((InBytes[index] & temp) == temp)
                {
                    buffer[i] = true;
                }
            }

            return buffer;
        }

        public static bool[] ByteToBoolArray(byte[] InBytes)
        {
            if (InBytes == null) return null;

            return ByteToBoolArray(InBytes, InBytes.Length * 8);
        }

        #region Get Value From Bytes   获取数据

        /// <summary>
        /// 从缓存中提取出bool结果 
        /// Extracts a boolean result from the buffer
        /// </summary>
        /// <param name="buffer">缓存数据</param>
        /// <param name="index">位的索引</param>
        /// <returns>bool对象</returns>
        public static bool TransBool(byte[] buffer, int index)
        {
            return ((buffer[index] & 0x01) == 0x01);
        }

        /// <summary>
        /// 从缓存中提取byte结果
        /// Extracts a byte result from the buffer
        /// </summary>
        /// <param name="buffer">缓存数据</param>
        /// <param name="index">索引位置</param>
        /// <returns>byte对象</returns>
        public static byte TransByte(byte[] buffer, int index)
        {
            return buffer[index];
        }

        /// <summary>
        /// 从缓存中提取byte数组结果
        /// Extracts a byte array result from the buffer
        /// </summary>
        /// <param name="buffer">缓存数据</param>
        /// <param name="index">索引位置</param>
        /// <param name="length">读取的数组长度</param>
        /// <returns>byte数组对象</returns>
        public static byte[] TransByte(byte[] buffer, int index, int length)
        {
            byte[] tmp = new byte[length];
            Array.Copy(buffer, index, tmp, 0, length);
            return tmp;
        }

        /// <summary>
        /// Converts an array of short values to a byte array and reverses the byte order of each short.
        /// 将short值的数组转换为字节数组，并反转每个short的字节顺序。
        /// </summary>
        /// <param name="values">The array of short values to convert. 要转换的short值的数组。</param>
        /// <returns>A byte array with reversed byte order for each short, or null if input is null. 每个short字节顺序反转后的字节数组，如果输入为null则返回null。</returns>
        public static byte[] TransReverseByte(short[] values)
        {
            if (values == null) return null;

            byte[] buffer = new byte[values.Length * 2];
            for (int i = 0; i < values.Length; i++)
            {
                byte[] tmp = BitConverter.GetBytes(values[i]);
                Array.Reverse(tmp);
                tmp.CopyTo(buffer, 2 * i);
            }

            return buffer;
        }

        public static byte[] TransReverseByte(int[] values)
        {
            if (values == null) return null;

            byte[] buffer = new byte[values.Length * 4];
            for (int i = 0; i < values.Length; i++)
            {
                byte[] tmp = BitConverter.GetBytes(values[i]);
                //     Array.Reverse(tmp);
                ByteTransDataFormat4(tmp).CopyTo(buffer, 4 * i);
            }

            return buffer;
        }

        public static byte[] TransReverseByte(float[] values)
        {
            if (values == null) return null;

            byte[] buffer = new byte[values.Length * 4];
            for (int i = 0; i < values.Length; i++)
            {
                byte[] tmp = BitConverter.GetBytes(values[i]);
                ByteTransDataFormat4(tmp).CopyTo(buffer, 4 * i);
            }

            return buffer;
        }

        public static byte[] TransByte(string value, Encoding encoding)
        {
            if (value == null) return null;

            return encoding.GetBytes(value);
        }

        public static T[] ArrayExpandToLength<T>(T[] data, int length)
        {
            if (data == null) return new T[length];

            if (data.Length == length) return data;

            T[] buffer = new T[length];

            Array.Copy(data, buffer, Math.Min(data.Length, buffer.Length));

            return buffer;
        }

        public static T[] ArrayExpandToLengthEven<T>(T[] data)
        {
            if (data == null) return new T[0];

            if (data.Length % 2 == 1)
            {
                return ArrayExpandToLength(data, data.Length + 1);
            }
            else
            {
                return data;
            }
        }

        /// <summary>
        /// 从缓存中提取short结果
        /// </summary>
        /// <param name="buffer">缓存数据</param>
        /// <param name="index">索引位置</param>
        /// <returns>short对象</returns>
        public static short TransInt16(byte[] buffer, int index)
        {
            if (buffer == null || buffer.Length == 0)
            {
                return 0;
            }

            byte[] newBuffer = new byte[2];
            newBuffer[0] = buffer[index + 1];
            newBuffer[1] = buffer[index];

            return BitConverter.ToInt16(newBuffer, 0);

        }

        public static short TransShort(byte[] buffer, int index)
        {
            if (buffer == null || buffer.Length == 0)
            {
                return 0;
            }
            return BitConverter.ToInt16(buffer, index);
        }

        /// <summary>
        /// 从缓存中提取short数组结果
        /// </summary>
        /// <param name="buffer">缓存数据</param>
        /// <param name="index">索引位置</param>
        /// <param name="length">读取的数组长度</param>
        /// <returns>short数组对象</returns>
        public static short[] TransInt16(byte[] buffer, int index, int length)
        {
            short[] tmp = new short[length];
            for (int i = 0; i < length; i++)
            {
                tmp[i] = TransInt16(buffer, index + 2 * i);
            }
            return tmp;
        }

        public static short[] TransShort(byte[] buffer, int index, int length)
        {
            short[] tmp = new short[length];
            for (int i = 0; i < length; i++)
            {
                tmp[i] = TransInt16(buffer, index + 2 * i);
                if (i == (length - 1))
                {
                    break;
                }
            }
            return tmp;
        }


        /// <summary>
        /// 从缓存中提取ushort结果
        /// </summary>
        /// <param name="buffer">缓存数据</param>
        /// <param name="index">索引位置</param>
        /// <returns>ushort对象</returns>
        public static ushort TransUInt16(byte[] buffer, int index)
        {
            return BitConverter.ToUInt16(buffer, index);
        }

        /// <summary>
        /// 从缓存中提取ushort数组结果
        /// </summary>
        /// <param name="buffer">缓存数据</param>
        /// <param name="index">索引位置</param>
        /// <param name="length">读取的数组长度</param>
        /// <returns>ushort数组对象</returns>
        public static ushort[] TransUInt16(byte[] buffer, int index, int length)
        {
            ushort[] tmp = new ushort[length];
            for (int i = 0; i < length; i++)
            {
                tmp[i] = TransUInt16(buffer, index + 2 * i);
            }
            return tmp;
        }


        protected static byte[] ByteTransDataFormat4(byte[] value, int index = 0)
        {
            byte[] buffer = new byte[4];
            buffer[0] = value[index + 3];
            buffer[1] = value[index + 2];
            buffer[2] = value[index + 1];
            buffer[3] = value[index + 0];
            return buffer;
        }
        protected static byte[] ByteTransDataFormat8(byte[] value, int index = 0)
        {
            byte[] buffer = new byte[8];
            buffer[0] = value[index + 7];
            buffer[1] = value[index + 6];
            buffer[2] = value[index + 5];
            buffer[3] = value[index + 4];
            buffer[4] = value[index + 3];
            buffer[5] = value[index + 2];
            buffer[6] = value[index + 1];
            buffer[7] = value[index + 0];
            return buffer;
        }

        /// <summary>
        /// 从缓存中提取int结果
        /// </summary>
        /// <param name="buffer">缓存数据</param>
        /// <param name="index">索引位置</param>
        /// <returns>int对象</returns>
        public static int TransInt32(byte[] buffer, int index)
        {
            return BitConverter.ToInt32(ByteTransDataFormat4(buffer, index), 0);
        }

        /// <summary>
        /// 从缓存中提取int数组结果
        /// </summary>
        /// <param name="buffer">缓存数据</param>
        /// <param name="index">索引位置</param>
        /// <param name="length">读取的数组长度</param>
        /// <returns>int数组对象</returns>
        public static int[] TransInt32(byte[] buffer, int index, int length)
        {
            int[] tmp = new int[length];
            for (int i = 0; i < length; i++)
            {
                tmp[i] = TransInt32(buffer, index + 4 * i);
                if (i == (length - 1))
                {
                    break;
                }
            }
            return tmp;
        }



        /// <summary>
        /// 从缓存中提取uint结果
        /// </summary>
        /// <param name="buffer">缓存数据</param>
        /// <param name="index">索引位置</param>
        /// <returns>uint对象</returns>
        public static uint TransUInt32(byte[] buffer, int index)
        {
            return BitConverter.ToUInt32(ByteTransDataFormat4(buffer, index), 0);
        }

        /// <summary>
        /// 从缓存中提取uint数组结果
        /// </summary>
        /// <param name="buffer">缓存数据</param>
        /// <param name="index">索引位置</param>
        /// <param name="length">读取的数组长度</param>
        /// <returns>uint数组对象</returns>
        public static uint[] TransUInt32(byte[] buffer, int index, int length)
        {
            uint[] tmp = new uint[length];
            for (int i = 0; i < length; i++)
            {
                tmp[i] = TransUInt32(buffer, index + 4 * i);
            }
            return tmp;
        }

        /// <summary>
        /// 从缓存中提取long结果
        /// </summary>
        /// <param name="buffer">缓存数据</param>
        /// <param name="index">索引位置</param>
        /// <returns>long对象</returns>
        public static long TransInt64(byte[] buffer, int index)
        {
            return BitConverter.ToInt64(ByteTransDataFormat8(buffer, index), 0);
        }

        /// <summary>
        /// 从缓存中提取long数组结果
        /// </summary>
        /// <param name="buffer">缓存数据</param>
        /// <param name="index">索引位置</param>
        /// <param name="length">读取的数组长度</param>
        /// <returns>long数组对象</returns>
        public static long[] TransInt64(byte[] buffer, int index, int length)
        {
            long[] tmp = new long[length];
            for (int i = 0; i < length; i++)
            {
                tmp[i] = TransInt64(buffer, index + 8 * i);
            }
            return tmp;
        }


        /// <summary>
        /// 从缓存中提取ulong结果
        /// </summary>
        /// <param name="buffer">缓存数据</param>
        /// <param name="index">索引位置</param>
        /// <returns>ulong对象</returns>
        public static ulong TransUInt64(byte[] buffer, int index)
        {
            return BitConverter.ToUInt64(ByteTransDataFormat8(buffer, index), 0);
        }

        /// <summary>
        /// 从缓存中提取ulong数组结果
        /// </summary>
        /// <param name="buffer">缓存数据</param>
        /// <param name="index">索引位置</param>
        /// <param name="length">读取的数组长度</param>
        /// <returns>ulong数组对象</returns>
        public static ulong[] TransUInt64(byte[] buffer, int index, int length)
        {
            ulong[] tmp = new ulong[length];
            for (int i = 0; i < length; i++)
            {
                tmp[i] = TransUInt64(buffer, index + 8 * i);
            }
            return tmp;
        }

        /// <summary>
        /// 从缓存中提取float结果
        /// </summary>
        /// <param name="buffer">缓存对象</param>
        /// <param name="index">索引位置</param>
        /// <returns>float对象</returns>
        public static float TransSingle(byte[] buffer, int index)
        {
            return BitConverter.ToSingle(ByteTransDataFormat4(buffer, index), 0);
        }

        /// <summary>
        /// 从缓存中提取float数组结果
        /// </summary>
        /// <param name="buffer">缓存数据</param>
        /// <param name="index">索引位置</param>
        /// <param name="length">读取的数组长度</param>
        /// <returns>float数组对象</returns>
        public static float[] TransSingle(byte[] buffer, int index, int length)
        {
            float[] tmp = new float[length];
            for (int i = 0; i < length; i++)
            {
                tmp[i] = TransSingle(buffer, index + 4 * i);
                if (i == (length - 1))
                {
                    break;
                }
            }
            return tmp;
        }


        /// <summary>
        /// 从缓存中提取double结果
        /// </summary>
        /// <param name="buffer">缓存对象</param>
        /// <param name="index">索引位置</param>
        /// <returns>double对象</returns>
        public static double TransDouble(byte[] buffer, int index)
        {
            return BitConverter.ToDouble(ByteTransDataFormat8(buffer, index), 0);
        }

        /// <summary>
        /// 从缓存中提取double数组结果
        /// </summary>
        /// <param name="buffer">缓存对象</param>
        /// <param name="index">索引位置</param>
        /// <param name="length">读取的数组长度</param>
        /// <returns>double数组对象</returns>
        public static double[] TransDouble(byte[] buffer, int index, int length)
        {
            double[] tmp = new double[length];
            for (int i = 0; i < length; i++)
            {
                tmp[i] = TransDouble(buffer, index + 8 * i);
            }
            return tmp;
        }


        /// <summary>
        /// 从缓存中提取string结果，使用指定的编码
        /// </summary>
        /// <param name="buffer">缓存对象</param>
        /// <param name="index">索引位置</param>
        /// <param name="length">byte数组长度</param>
        /// <param name="encoding">字符串的编码</param>
        /// <returns>string对象</returns>
        public static string TransString(byte[] buffer, int index, int length, Encoding encoding)
        {
            byte[] tmp = TransByte(buffer, index, length);
            return encoding.GetString(tmp);
        }

        #endregion
    }
}

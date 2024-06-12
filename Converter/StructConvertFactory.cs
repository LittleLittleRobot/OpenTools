using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Wesky.Net.OpenTools.Converter
{
    /// <summary>
    /// 提供结构体转换器的工厂类。
    /// Provides a factory class for structure converters.
    /// </summary>
    public class StructConvertFactory
    {
        /// <summary>
        /// 根据结构体类型的复杂性选择合适的转换器。
        /// Selects an appropriate converter based on the complexity of the structure type.
        /// </summary>
        /// <typeparam name="T">要为其创建转换器的结构体类型。</typeparam>
        /// <returns>返回符合结构体类型特性的转换器实例。</returns>
        /// <remarks>
        /// 如果结构体包含复杂字段，则返回一个基于反射的转换器，否则返回一个基于内存操作的转换器。
        /// If the structure contains complex fields, a reflection-based converter is returned; otherwise, a memory operation-based converter is provided.
        /// </remarks>
        public static IStructConvert CreateConvertor<T>() where T : struct
        {
            // 判断结构体类型T是否包含复杂字段
            if (HasComplexFields(typeof(T)))
            {
                // 返回反射方式实现的结构体转换器
                return new StructConvert();
            }
            else
            {
                // 返回内存操作方式实现的结构体转换器
                return new MarshalConvert();
            }
        }

        /// <summary>
        /// 验证指定类型的字段是否包含复杂类型。
        /// Verifies whether the fields of the specified type contain complex types.
        /// </summary>
        /// <param name="type">要检查的类型。</param>
        /// <returns>如果包含复杂类型字段，则返回true；否则返回false。</returns>
        /// <remarks>
        /// 复杂类型包括数组、类以及非基本的值类型（如结构体），但不包括decimal。
        /// Complex types include arrays, classes, and non-primitive value types such as structures, but exclude decimal.
        /// </remarks>
        private static bool HasComplexFields(Type type)
        {
            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                if (field.FieldType.IsArray || field.FieldType.IsClass ||
                    (field.FieldType.IsValueType && !field.FieldType.IsPrimitive &&
                     field.FieldType != typeof(decimal)))
                {
                    return true;
                }
            }
            return false;
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;

namespace Wesky.Net.OpenTools.SystemExtensions.JsonExtensions
{
    /// <summary>
    /// 实体类JSON模式生成器
    /// Entity class JSON schema generator
    /// </summary>
    public static class JsonSchemaGenerator
    {
        // 根据类型生成JSON模式的字符串
        // Generate JSON schema string based on the type
        public static string CreateJsonSchema(Type type)
        {
            StringBuilder jsonBuilder = new StringBuilder();
            jsonBuilder.Append("{");

            PropertyInfo[] properties = type.GetProperties();
            for (int i = 0; i < properties.Length; i++)
            {
                PropertyInfo prop = properties[i];
                string jsonPropertyName = GetJsonPropertyName(prop);
                Type propType = prop.PropertyType;

                // 使用属性名作为JSON键
                // Use the property name as JSON key
                jsonBuilder.AppendFormat("\"{0}\": ", jsonPropertyName);

                // 处理基本类型、字符串和日期类型
                // Handle primitive types, string, and date types
                if (propType.IsPrimitive || propType == typeof(string) || propType == typeof(DateTime) ||
                    Nullable.GetUnderlyingType(propType) != null && Nullable.GetUnderlyingType(propType).IsPrimitive)
                {
                    jsonBuilder.Append($"\"{propType.Name}\"");
                }
                // 处理泛型列表
                // Handle generic list types
                else if (propType.IsGenericType && propType.GetGenericTypeDefinition() == typeof(List<>))
                {
                    Type itemType = propType.GetGenericArguments()[0];
                    jsonBuilder.Append("[");
                    jsonBuilder.Append(CreateJsonSchema(itemType));
                    jsonBuilder.Append("]");
                }
                // 递归处理其他类类型
                // Recursively handle other class types
                else if (propType.IsClass)
                {
                    jsonBuilder.Append(CreateJsonSchema(propType));
                }

                // 在属性之间添加逗号分隔符
                // Add comma separator between properties
                if (i < properties.Length - 1)
                {
                    jsonBuilder.Append(", ");
                }
            }

            jsonBuilder.Append("}");
            return jsonBuilder.ToString();
        }

        // 获取属性的显示名称，如果未设置则使用属性原名
        // Get the display name of the property, if not set, use the property's original name
        private static string GetJsonPropertyName(PropertyInfo prop)
        {
            // 查找并返回自定义JsonKey属性中指定的KeyName
            // Look for and return the KeyName specified in the custom JsonKey attribute
            var attribute = prop.GetCustomAttribute<OpenJsonAttribute>();
            if (attribute != null)
            {
                return attribute.KeyName;
            }
            else
            {
                return prop.Name;
            }
        }

    }

}

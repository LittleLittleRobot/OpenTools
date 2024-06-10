using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace Wesky.Net.OpenTools.HttpExtensions
{
    public interface IWebserviceHelper
    {
        /// <summary>
        /// 从XML中提取并转换指定节点的值到泛型指定的类型。
        /// Extracts and converts the value of a specified node in an XML to the type specified by the generic type parameter.
        /// </summary>
        /// <typeparam name="T">期望的返回类型，必须是可以从string转换的基本类型 / The expected return type, must be a primitive type that can be converted from string.</typeparam>
        /// <param name="xml">XML字符串 / XML string.</param>
        /// <param name="nodeName">节点名称，应包括命名空间前缀（如果有） / Node name, should include the namespace prefix if any.</param>
        /// <param name="ns">命名空间 / Namespace.</param>
        /// <returns>转换后的节点值 / Converted node value.</returns>
        T ExtractBasicValueFromXml<T>(string xml, string nodeName, XNamespace ns);
        /// <summary>
        /// 解析XML字符串并映射到指定的对象类型，支持嵌套对象和集合。
        /// Parses an XML string and maps it to a specified object type, supporting nested objects and collections.
        /// </summary>
        /// <typeparam name="T">需要映射的对象类型，必须有无参构造函数 / The type of object to map, must have a parameterless constructor.</typeparam>
        /// <param name="xml">包含对象数据的XML字符串 / The XML string containing the object data.</param>
        /// <param name="rootNode">根节点名称 / The name of the root node.</param>
        /// <param name="ns">XML命名空间 / The XML namespace.</param>
        /// <returns>映射好的对象实例 / The mapped object instance.</returns>
        /// <exception cref="InvalidOperationException">如果无法找到根节点或属性映射失败时抛出 / Thrown if the root node is not found or if property mapping fails.</exception>
        T ExtractCustomerValueFromXml<T>(string xml, string rootNode, XNamespace ns) where T : new();

        /// <summary>
        /// 调用Web服务
        /// Calls a web service.
        /// </summary>
        /// <param name="url">服务URL / Service URL</param>
        /// <param name="apiName">API名称 / API name</param>
        /// <param name="expireSecond">过期时间（秒）/ Expiration time in seconds</param>
        /// <param name="parameters">调用参数 / Invocation parameters</param>
        /// <returns>调用结果 / Invocation result</returns>
        OpenToolResult<string> CallWebservice(string url, string apiName, long expireSecond = 86400, params object[] parameters);
    }
}

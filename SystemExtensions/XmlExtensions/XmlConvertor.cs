using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Wesky.Net.OpenTools.SystemExtensions.XmlExtensions
{
    public class XmlConvertor
    {
        public static T DeserializeFromXml<T>(string xml)
        {
            using (StringReader stringReader = new StringReader(xml))
            {
                var serializer = new XmlSerializer(typeof(T));
                return (T)serializer.Deserialize(stringReader);
            }
        }

       
        public static string SerializeToXml(object param)
        {
            using (var stringwriter = new StringWriter())
            {
                var serializer = new XmlSerializer(param.GetType());
                serializer.Serialize(stringwriter, param);
                return stringwriter.ToString().Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>", "");
            }
        }

        /// <summary>
        /// 对象序列化为XML字符串
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string SerializeObjectToXml<T>(T obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj), "提供的对象不能为空。");
            }

            Type objectType = obj.GetType();
            XmlSerializer serializer = new XmlSerializer(objectType);
            XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
            namespaces.Add("", ""); // 移除所有命名空间

            XmlWriterSettings settings = new XmlWriterSettings
            {
                OmitXmlDeclaration = true, // 不包括XML声明
                Encoding = Encoding.UTF8, // 使用UTF-8编码
                Indent = true // 可选，美化输出
            };

            using (StringWriter textWriter = new StringWriter())
            {
                using (XmlWriter xmlWriter = XmlWriter.Create(textWriter, settings))
                {
                    serializer.Serialize(xmlWriter, obj, namespaces);
                }

                // 使用XDocument处理XML，根据根节点是否有子元素来决定返回内容
                var xDoc = XDocument.Parse(textWriter.ToString());
                XElement root = xDoc.Root;
                var elements = root.Elements();
          //      StringBuilder sb = new StringBuilder();
                if (elements.Any())
                {
                    // 如果有子元素，返回所有子元素的序列化字符串
                    return string.Join(Environment.NewLine, elements.Select(e => e.ToString()));
                }
                else
                {
                    return root.Value;
                }
            }
        }

        //public static string SerializeObjectToXml<T>(T obj)
        //{
        //    XmlSerializer serializer = new XmlSerializer(typeof(T));
        //    XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
        //    namespaces.Add("", ""); // 移除所有命名空间

        //    XmlWriterSettings settings = new XmlWriterSettings
        //    {
        //        OmitXmlDeclaration = true, // 不包括XML声明
        //        Encoding = Encoding.UTF8, // 使用UTF-8编码
        //        Indent = true // 可选，美化输出
        //    };

        //    using (StringWriter textWriter = new StringWriter())
        //    {
        //        using (XmlWriter xmlWriter = XmlWriter.Create(textWriter, settings))
        //        {
        //            serializer.Serialize(xmlWriter, obj, namespaces);
        //        }
        //        // 使用XDocument处理XML，移除根节点并保留所有子元素
        //        var xDoc = XDocument.Parse(textWriter.ToString());
        //        XElement root = xDoc.Root;
        //        var elements = root.Elements();
        //        StringBuilder sb = new StringBuilder();
        //        foreach (var elem in elements)
        //        {
        //            sb.AppendLine(elem.ToString()); // 将所有子元素添加到StringBuilder中
        //        }
        //        return sb.ToString();
        //    }
        //}

        public static string SerializeToXml<T>(T param)
        {
            using (var stringwriter = new StringWriter())
            {
                using (var xmlWriter = XmlWriter.Create(stringwriter))
                {
                    var serializer = new DataContractSerializer(typeof(T));
                    serializer.WriteObject(xmlWriter, param);
                    xmlWriter.Flush();
                    return stringwriter.ToString().Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>", "");
                }
            }
        }
    }
}

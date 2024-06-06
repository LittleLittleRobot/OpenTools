using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace Wesky.Net.OpenTools.SystemExtensions.XmlExtensions
{
    public class OpenXmlGenerator
    {
        /// <summary>
        /// 生成给定类型的所有属性的摘要信息列表，搜索所有相关XML文档。
        /// Generates a list of summary information for all properties of a given type, searching through all relevant XML documents.
        /// </summary>
        /// <param name="type">要分析的类型。The type to analyze.</param>
        /// <param name="parentPrefix">处理属性路径时用于嵌套属性的前缀。Prefix for nested properties to handle property paths correctly.</param>
        /// <returns>摘要信息实体列表。A list of summary information entities.</returns>
        public static List<DynamicSumaryInfo> GenerateEntitySummaries(Type type, string parentPrefix = "")
        {
            var summaryInfos = new List<DynamicSumaryInfo>();
            IEnumerable<string> xmlPaths = GetAllXmlDocumentationPaths();

            foreach (string xmlPath in xmlPaths)
            {
                if (File.Exists(xmlPath))
                {
                    XDocument xmlDoc = XDocument.Load(xmlPath);
                    XElement root = xmlDoc.Root;

                    summaryInfos.AddRange(ExtractSummaryInfo(type, root, parentPrefix));
                }
            }

            return summaryInfos;
        }

        /// <summary>
        /// 获取当前执行环境目录下所有XML文档的路径。
        /// Retrieves the paths to all XML documentation files in the current execution environment directory.
        /// </summary>
        /// <returns>所有XML文档文件的路径列表。A list of paths to all XML documentation files.</returns>
        private static IEnumerable<string> GetAllXmlDocumentationPaths()
        {
            string basePath = AppContext.BaseDirectory;
            return Directory.GetFiles(basePath, "*.xml", SearchOption.TopDirectoryOnly);
        }

        /// <summary>
        /// 从XML文档中提取指定类型的所有属性的摘要信息。
        /// Extracts summary information for all properties of a specified type from an XML document.
        /// </summary>
        /// <param name="type">属性所属的类型。The type to which the properties belong.</param>
        /// <param name="root">XML文档的根元素。The root element of the XML document.</param>
        /// <param name="parentPrefix">属性的前缀路径。The prefix path for properties.</param>
        /// <returns>摘要信息实体列表。A list of summary information entities.</returns>
        private static List<DynamicSumaryInfo> ExtractSummaryInfo(Type type, XElement root, string parentPrefix)
        {
            var infos = new List<DynamicSumaryInfo>();

            foreach (PropertyInfo property in type.GetProperties())
            {
                string fullPath = string.IsNullOrEmpty(parentPrefix) ? property.Name : $"{parentPrefix}.{property.Name}";
                string typeName = property.PropertyType.Name;

                if (property.PropertyType.IsClass && property.PropertyType != typeof(string))
                {
                    Type propertyType = property.PropertyType;
                    if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(List<>))
                    {
                        propertyType = propertyType.GetGenericArguments()[0];
                        typeName = $"List<{propertyType.Name}>";
                    }

                    infos.AddRange(GenerateEntitySummaries(propertyType, fullPath));
                }
                else
                {
                    string summary = GetPropertySummary(root, type, property);
                    infos.Add(new DynamicSumaryInfo
                    {
                        Name = fullPath,
                        TypeName = typeName,
                        Summary = summary ?? string.Empty
                    });
                }
            }

            return infos;
        }

        /// <summary>
        /// 从XML中获取指定属性的摘要信息。
        /// Retrieves the summary information for a specified property from XML.
        /// </summary>
        /// <param name="root">XML文档的根元素。The root element of the XML document.</param>
        /// <param name="type">属性所属的类型。The type to which the property belongs.</param>
        /// <param name="property">要获取摘要的属性。The property to get the summary for.</param>
        /// <returns>属性的摘要信息。The summary information of the property.</returns>
        private static string GetPropertySummary(XElement root, Type type, PropertyInfo property)
        {
            string memberName = $"P:{type.FullName}.{property.Name}";
            return root.Descendants("member")
                       .FirstOrDefault(node => node.Attribute("name")?.Value == memberName)
                       ?.Element("summary")?.Value?.Trim();
        }


    }
}

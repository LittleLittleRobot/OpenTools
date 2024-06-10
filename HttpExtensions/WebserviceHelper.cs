using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using Wesky.Net.OpenTools.SystemExtensions.XmlExtensions;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using System.Dynamic;
using System.Linq;
using System.Diagnostics;
using System.Collections;

namespace Wesky.Net.OpenTools.HttpExtensions
{
    public class WebserviceHelper:IWebserviceHelper
    {
        /// <summary>
        /// 从WSDL文档获取命名空间
        /// Retrieves the namespace from the WSDL document.
        /// </summary>
        /// <param name="wsdlUrl">WSDL文档的URL / URL of the WSDL document</param>
        /// <param name="operationName">操作名称 / Name of the operation</param>
        /// <returns>命名空间字符串 / Namespace string</returns>
        private string GetNamespaceFromWsdl(string wsdlUrl, string operationName)
        {
            XmlDocument wsdlDoc = new XmlDocument();
            wsdlDoc.Load(wsdlUrl); // 加载WSDL文档 / Load the WSDL document

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(wsdlDoc.NameTable);
            nsmgr.AddNamespace("wsdl", "http://schemas.xmlsoap.org/wsdl/");
            nsmgr.AddNamespace("soap", "http://schemas.xmlsoap.org/wsdl/soap/");

            // 构建XPath查询字符串 / Construct XPath query string
            string xpath = $"//wsdl:binding/wsdl:operation[@name='{operationName}']/soap:operation";
            XmlNode operationNode = wsdlDoc.SelectSingleNode(xpath, nsmgr);
            if (operationNode != null && operationNode.Attributes["soapAction"] != null)
            {
                string soapAction = operationNode.Attributes["soapAction"].Value;
                return soapAction.Substring(0, soapAction.LastIndexOf('/') + 1);
            }

            // 如果无法找到操作，则返回默认命名空间 / Return default namespace if operation is not found
            return "http://tempuri.org/";
        }

        /// <summary>
        /// 从WSDL文档获取参数名列表
        /// Retrieves a list of parameter names from the WSDL document.
        /// </summary>
        /// <param name="wsdlUrl">WSDL文档的URL / URL of the WSDL document</param>
        /// <param name="operationName">操作名称 / Name of the operation</param>
        /// <returns>参数名列表 / List of parameter names</returns>
        private List<string> GetParameterNamesFromWsdl(string wsdlUrl, string operationName)
        {
            var parameterNames = new List<string>();

            XmlDocument wsdlDoc = new XmlDocument();
            wsdlDoc.Load(wsdlUrl);  // 加载WSDL文档 / Load the WSDL document

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(wsdlDoc.NameTable);
            nsmgr.AddNamespace("wsdl", "http://schemas.xmlsoap.org/wsdl/");
            nsmgr.AddNamespace("s", "http://www.w3.org/2001/XMLSchema");

            // 构建XPath查询字符串 / Construct XPath query string
            string xpath = $"//wsdl:portType/wsdl:operation[@name='{operationName}']/wsdl:input";
            XmlNode inputNode = wsdlDoc.SelectSingleNode(xpath, nsmgr);
            if (inputNode != null)
            {
                string messageName = inputNode.Attributes["message"].Value.Split(':')[1]; // 解析消息名称
                XmlNode messageNode = wsdlDoc.SelectSingleNode($"//wsdl:message[@name='{messageName}']/wsdl:part", nsmgr);
                if (messageNode != null)
                {
                    string elementName = messageNode.Attributes["element"].Value.Split(':')[1]; // 解析元素名称
                    XmlNode elementNode = wsdlDoc.SelectSingleNode($"//s:schema/s:element[@name='{elementName}']", nsmgr);
                    if (elementNode != null)
                    {
                        XmlNode sequenceNode = elementNode.SelectSingleNode("s:complexType/s:sequence", nsmgr);
                        if (sequenceNode != null)
                        {
                            foreach (XmlNode param in sequenceNode.ChildNodes)
                            {
                                parameterNames.Add(param.Attributes["name"].Value);
                            }
                        }
                    }
                }
            }

            return parameterNames;
        }

        /// <summary>
        /// 构建SOAP消息信封
        /// Builds a SOAP message envelope.
        /// </summary>
        /// <param name="methodName">方法名称 / Method name</param>
        /// <param name="parameters">参数字典 / Dictionary of parameters</param>
        /// <param name="actionHeader">动作头部 / Action header</param>
        /// <returns>SOAP信封字符串 / SOAP envelope string</returns>
        private string BuildSoapEnvelope(string methodName, Dictionary<string, string> parameters, string actionHeader)
        {
            var sb = new StringBuilder();
            sb.Append(@"<?xml version=""1.0"" encoding=""utf-8""?>");
            sb.Append(@"<soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">");
            sb.Append(@"<soap:Body>");
            sb.Append($"<{methodName} xmlns=\"{actionHeader}\">");

            // 遍历字典并添加XML元素
            foreach (var param in parameters)
            {
                sb.Append($"<{param.Key}>{param.Value}</{param.Key}>");
            }

            sb.Append($"</{methodName}>");
            sb.Append(@"</soap:Body>");
            sb.Append(@"</soap:Envelope>");

            return sb.ToString();
        }

        /// <summary>
        /// 调用Web服务
        /// Invokes a web service.
        /// </summary>
        /// <param name="url">服务URL / Service URL</param>
        /// <param name="methodName">方法名称 / Method name</param>
        /// <param name="parameters">参数字典 / Dictionary of parameters</param>
        /// <param name="actionHeader">命名空间，默认为http://tempuri.org/ / Action header(Namespace), defaults to http://tempuri.org/</param>
        /// <returns>服务调用结果 / Service invocation result</returns>
        private string InvokeService(string url, string methodName, Dictionary<string, string> parameters, string actionHeader = "http://tempuri.org/")
        {
            var soapEnvelope = BuildSoapEnvelope(methodName, parameters, actionHeader);
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.Headers.Add("SOAPAction", $"{actionHeader}{methodName}");
            webRequest.ContentType = "text/xml;charset=\"utf-8\"";
            webRequest.Accept = "text/xml";
            webRequest.Method = "POST";

            using (Stream stream = webRequest.GetRequestStream())
            {
                byte[] postBytes = Encoding.UTF8.GetBytes(soapEnvelope);
                stream.Write(postBytes, 0, postBytes.Length);
            }

            try
            {
                using (WebResponse response = webRequest.GetResponse())
                {
                    using (StreamReader rd = new StreamReader(response.GetResponseStream()))
                    {
                        string soapResult = rd.ReadToEnd();
                        return soapResult;
                    }
                }
            }
            catch (WebException e)
            {
                using (var stream = e.Response.GetResponseStream())
                using (var reader = new StreamReader(stream))
                {
                    string errorResponse = reader.ReadToEnd();
                    return $"Error: {errorResponse}";
                }
            }
        }

        /// <summary>
        /// 检查Web服务缓存是否过期，并在必要时更新
        /// Checks if the web service cache is expired and updates it if necessary.
        /// </summary>
        /// <param name="url">服务的URL / The URL of the service</param>
        /// <param name="apiName">API名称 / API name</param>
        /// <param name="expireSecond">过期时间（秒）/ Expiration time in seconds</param>
        private void CheckExpireTime(string url, string apiName, long expireSecond = 86400)
        {
            var webServiceInfo = OpenWebserviceInfo.OpenWebservice.FirstOrDefault(x => x.WebserviceUrl == url && x.OperationName == apiName);

            if (webServiceInfo != null)
            {
                if (webServiceInfo.ResetTime.AddSeconds(expireSecond) < DateTime.Now)
                {
                    try
                    {
                        // 重置时间并重新加载Web服务的文档
                        webServiceInfo.ResetTime = DateTime.Now;
                        webServiceInfo.ParameterNames = GetParameterNamesFromWsdl(url, apiName);
                        webServiceInfo.Namespace = GetNamespaceFromWsdl(url, apiName);
                        webServiceInfo.ExpireSeconds = expireSecond;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"加载wsdl服务文档失败。Failed to reload WSDL document: {ex.Message}");
                    }
                }
            }
            else
            {
                try
                {
                    // 创建新的缓存条目
                    webServiceInfo = new OpenWebserviceDocCache
                    {
                        WebserviceUrl = url,
                        OperationName = apiName,
                        Namespace = GetNamespaceFromWsdl(url, apiName),
                        ParameterNames = GetParameterNamesFromWsdl(url, apiName),
                        ExpireSeconds = expireSecond,
                        ResetTime = DateTime.Now
                    };
                    OpenWebserviceInfo.OpenWebservice.Add(webServiceInfo);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"加载wsdl服务文档失败。Failed to load WSDL document: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 调用Web服务
        /// Calls a web service.
        /// </summary>
        /// <param name="url">服务URL / Service URL</param>
        /// <param name="apiName">API名称 / API name</param>
        /// <param name="expireSecond">过期时间（秒）/ Expiration time in seconds</param>
        /// <param name="parameters">调用参数 / Invocation parameters</param>
        /// <returns>调用结果 / Invocation result</returns>
        public OpenToolResult<string> CallWebservice(string url, string apiName,long expireSecond = 86400,params object[] parameters)
        {
            OpenToolResult<string> result = new HttpExtensions.OpenToolResult<string>();

            CheckExpireTime(url, apiName, expireSecond);

            var wsInfo = OpenWebserviceInfo.OpenWebservice.FirstOrDefault(x => x.WebserviceUrl == url && x.OperationName == apiName);

            if (wsInfo == null)
            {
                result.IsSuccess = false;
                result.Message = "本地无法加载远程webservice服务。Cannot load the remote webservice locally.";
                return result;
            }

            if ((parameters == null && wsInfo.ParameterNames.Count > 0) || (parameters.Length != wsInfo.ParameterNames.Count))
            {
                result.IsSuccess = false;
                result.Message = $"远程服务接口参数个数和你传入的参数个数不匹配。远程服务参数个数:{wsInfo.ParameterNames.Count}, 本地传入参数个数： {parameters?.Length ?? 0}。Parameter count mismatch: remote service has {wsInfo.ParameterNames.Count}, provided {parameters?.Length ?? 0}.";
                return result;
            }

            //if (wsInfo.ParameterNames == null || wsInfo.ParameterNames.Count==0)
            //{
            //    if(parameters!=null && parameters.Length > 0)
            //    {
            //        result.IsSuccess = false;
            //        result.Message = $"远程服务接口参数个数和你传入的参数个数不匹配,远程参数0个，你传入参数{parameters.Length}个";
            //        return result;
            //    }
            //}

            //if (parameters == null && wsInfo.ParameterNames.Count>0)
            //{
            //    result.IsSuccess = false;
            //    result.Message = $"远程服务接口参数个数和你传入的参数个数不匹配；远程参数{wsInfo.ParameterNames.Count}个，你传入参数0个";
            //    return result;
            //}

            //if (parameters.Length != wsInfo.ParameterNames.Count)
            //{
            //    result.IsSuccess = false;
            //    result.Message = $"远程服务接口参数个数和你传入的参数个数不匹配；远程参数{wsInfo.ParameterNames.Count}个，你传入参数{parameters.Length}个";
            //    return result;
            //}

            Dictionary<string, string> dicParams = new Dictionary<string, string>();
            for (int i = 0; i < wsInfo.ParameterNames.Count; i++)
            {
                dicParams.Add(wsInfo.ParameterNames[i], XmlConvertor.SerializeObjectToXml(parameters[i]));
            }
            var response = InvokeService(url, apiName, dicParams,wsInfo.Namespace);

            result.Result = response;
            result.IsSuccess = true;
            result.Message = "success";

            return result;
        }

        /// <summary>
        /// 从XML中提取并转换指定节点的值到泛型指定的类型。
        /// Extracts and converts the value of a specified node in an XML to the type specified by the generic type parameter.
        /// </summary>
        /// <typeparam name="T">期望的返回类型，必须是可以从string转换的基本类型 / The expected return type, must be a primitive type that can be converted from string.</typeparam>
        /// <param name="xml">XML字符串 / XML string.</param>
        /// <param name="nodeName">节点名称，应包括命名空间前缀（如果有） / Node name, should include the namespace prefix if any.</param>
        /// <param name="ns">命名空间 / Namespace.</param>
        /// <returns>转换后的节点值 / Converted node value.</returns>
        public T ExtractBasicValueFromXml<T>(string xml, string nodeName, XNamespace ns)
        {
            XDocument doc = XDocument.Parse(xml);

            // 获取指定节点，并注意使用命名空间 / Get the specified node, being mindful to use the namespace
            var node = doc.Descendants(ns + nodeName).FirstOrDefault();
            if (node == null)
            {
                throw new InvalidOperationException($"Node '{nodeName}' not found in XML.");
            }

            // 尝试将节点值转换为泛型指定的类型 / Attempt to convert the node value to the specified generic type
            try
            {
                return (T)Convert.ChangeType(node.Value, typeof(T));
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to convert node value to type {typeof(T).Name}.", ex);
            }
        }


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
        public T ExtractCustomerValueFromXml<T>(string xml, string rootNode, XNamespace ns) where T : new()
        {
            XDocument doc = XDocument.Parse(xml);

            // 实例化泛型类型 / Instantiate the generic type
            T result = new T();

            // 获取根节点，并注意使用命名空间 / Get the root node, being mindful to use the namespace
            var node = doc.Descendants(ns + rootNode).FirstOrDefault();
            if (node == null)
            {
                throw new InvalidOperationException($"Root node '{rootNode}' not found in XML.");
            }

            // 反射获取所有属性 / Use reflection to get all properties
            var properties = typeof(T).GetProperties();
            foreach (var property in properties)
            {
                var element = node.Element(ns + property.Name);
                if (element != null)
                {
                    if (property.PropertyType.IsGenericType && typeof(IEnumerable).IsAssignableFrom(property.PropertyType))
                    {
                        // 处理集合类型属性 / Handle collection type properties
                        var listItemType = property.PropertyType.GetGenericArguments()[0];
                        var list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(listItemType));

                        foreach (var itemElement in element.Elements())
                        {
                            list.Add(ParseXmlElement(listItemType, itemElement, ns));
                        }

                        property.SetValue(result, list);
                    }
                    else if (!property.PropertyType.IsPrimitive && property.PropertyType != typeof(string))
                    {
                        // 处理嵌套对象 / Handle nested objects
                        var nestedObject = ParseXmlElement(property.PropertyType, element, ns);
                        property.SetValue(result, nestedObject);
                    }
                    else
                    {
                        try
                        {
                            // 尝试将值转换为适当的类型并设置属性 / Attempt to convert the value to the appropriate type and set the property
                            var value = Convert.ChangeType(element.Value, property.PropertyType);
                            property.SetValue(result, value);
                        }
                        catch (Exception ex)
                        {
                            throw new InvalidOperationException($"Error setting property {property.Name} from XML. See inner exception for details.", ex);
                        }
                    }
                }
            }

            return result;
        }

        private object ParseXmlElement(Type type, XElement element, XNamespace ns)
        {
            if (type == typeof(string))  // 直接返回字符串值
            {
                return element.Value;
            }

            var instance = Activator.CreateInstance(type);
            var properties = type.GetProperties();
            foreach (var property in properties)
            {
                var subElement = element.Element(ns + property.Name);
                if (subElement != null)
                {
                    if (property.PropertyType.IsGenericType && typeof(IEnumerable).IsAssignableFrom(property.PropertyType))
                    {
                        var listItemType = property.PropertyType.GetGenericArguments()[0];
                        var list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(listItemType));

                        foreach (var itemElement in subElement.Elements())
                        {
                            list.Add(ParseXmlElement(listItemType, itemElement, ns));
                        }

                        property.SetValue(instance, list);
                    }
                    else if (!property.PropertyType.IsPrimitive && property.PropertyType != typeof(string))
                    {
                        var nestedObject = ParseXmlElement(property.PropertyType, subElement, ns);
                        property.SetValue(instance, nestedObject);
                    }
                    else
                    {
                        // 直接处理基本类型和字符串
                        object value;
                        if (property.PropertyType == typeof(string))
                        {
                            value = subElement.Value;
                        }
                        else
                        {
                            value = Convert.ChangeType(subElement.Value, property.PropertyType);
                        }
                        property.SetValue(instance, value);
                    }
                }
            }
            return instance;
        }


    }
}

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

namespace Wesky.Net.OpenTools.HttpExtensions
{
    public class WebserviceHelper
    {
        public List<string> GetParameterNamesFromWsdl(string wsdlUrl, string operationName)
        {
            var parameterNames = new List<string>();

            // 加载WSDL文档
            XmlDocument wsdlDoc = new XmlDocument();
            wsdlDoc.Load(wsdlUrl);

            // 设置命名空间管理器
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(wsdlDoc.NameTable);
            nsmgr.AddNamespace("wsdl", "http://schemas.xmlsoap.org/wsdl/");
            nsmgr.AddNamespace("s", "http://www.w3.org/2001/XMLSchema");

            // 获取特定操作的输入消息定义
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

        // 动态构建SOAP消息的方法
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

        // 扩展的InvokeService方法，支持多个参数
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

        public OpenToolResult<string> CallWebservice(string url, string apiName, string actionNamespace = "http://tempuri.org/",params object[] parameters)
        {
            OpenToolResult<string> result = new HttpExtensions.OpenToolResult<string>();
            var par = GetParameterNamesFromWsdl(url, apiName);
            if (par == null || par.Count==0)
            {
                if(parameters!=null && parameters.Length > 0)
                {
                    result.IsSuccess = false;
                    result.Message = $"远程服务接口参数个数和你传入的参数个数不匹配,远程参数0个，你传入参数{parameters.Length}个";
                    return result;
                }
            }

            if (parameters == null && par.Count>0)
            {
                result.IsSuccess = false;
                result.Message = $"远程服务接口参数个数和你传入的参数个数不匹配；远程参数{par.Count}个，你传入参数0个";
                return result;
            }

            if (parameters.Length != par.Count)
            {
                result.IsSuccess = false;
                result.Message = $"远程服务接口参数个数和你传入的参数个数不匹配；远程参数{par.Count}个，你传入参数{parameters.Length}个";
                return result;
            }

            Dictionary<string, string> dicParams = new Dictionary<string, string>();
            for (int i = 0; i < par.Count; i++)
            {
                dicParams.Add(par[i], XmlConvertor.SerializeObjectToXml(parameters[i]));
            }
            var response = InvokeService(url, apiName, dicParams);
            string xmlPayload = ExtractXmlFromSoapResponse(response);

            //// 替换根节点名称从ResponseResult到Employee
            //xmlPayload = ReplaceXmlRootNodeName(xmlPayload, typeof(T).Name);

            //var responseData = DeserializeXmlToObject<object>(xmlPayload);
            //  dynamic res = DeserializeXmlToDynamic(xmlPayload);


            result.Result = xmlPayload;//DeserializeXmlToObject<dynamic>(response);
            result.IsSuccess = true;
            result.Message = "success";

            return result;
        }

      

        T DeserializeXmlToObject<T>(string xml)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (StringReader reader = new StringReader(xml))
            {
                try
                {
                    object obj = serializer.Deserialize(reader);
                    return (T)obj;
                }
                catch (Exception ex)
                {
                    // 异常处理逻辑，可以根据需要记录日志或抛出
                    Console.WriteLine("An error occurred: " + ex.Message);
                    throw;
                }
            }
        }

        string ReplaceXmlRootNodeName(string xml, string newNodeName)
        {
            var doc = XDocument.Parse(xml);
            var root = doc.Root;
            if (root != null)
            {
                // 创建新的根节点，使用新名称但继承旧节点的所有内容
                var newRoot = new XElement(newNodeName, root.Elements());
                // 替换文档的根节点
                doc.Root.ReplaceWith(newRoot);
            }
            return doc.ToString();
        }

        string ExtractXmlFromSoapResponse(string soapResponse)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(soapResponse);
            XmlNodeList nodes = doc.GetElementsByTagName("soap:Body");
            if (nodes.Count > 0)
            {
                // 假设有效数据位于Body的第一个子元素内
                return nodes[0].FirstChild.InnerXml;
            }
            return string.Empty;
        }

    }
}

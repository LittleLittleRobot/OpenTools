using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using Wesky.Net.OpenTools.SystemExtensions.XmlExtensions;

namespace Wesky.Net.OpenTools.HttpExtensions
{
    public class WebserviceHelper
    {
        private string BuildSoapEnvelope<T>(string methodName, T parameters, string actionHeader)
        {
            var sb = new StringBuilder();
            sb.Append(@"<?xml version=""1.0"" encoding=""utf-8""?>");
            sb.Append(@"<soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""
                        xmlns:xsd=""http://www.w3.org/2001/XMLSchema""
                        xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">");
            sb.Append(@"<soap:Body>");
            sb.Append($"<{methodName} xmlns=\"{actionHeader}\">");

            sb.Append(SerializeToXml(parameters)); // 确保序列化使用正确的命名空间

            sb.Append($"</{methodName}>");
            sb.Append(@"</soap:Body>");
            sb.Append(@"</soap:Envelope>");

            return sb.ToString();
        }

        public string InvokeService<T>(string url, string methodName, T parameter, string actionHeader = "http://tempuri.org/")
        {
            var soapEnvelope = BuildSoapEnvelope(methodName, parameter, actionHeader);
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
            catch (Exception e)
            {
                return $"Error: {e.Message}";
            }
        }

        private static string SerializeToXml<T>(T obj)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("", "http://tempuri.org/");  // 确保没有多余的命名空间

            using (StringWriter writer = new StringWriter())
            {
                serializer.Serialize(writer, obj, ns);
                string result = writer.ToString();
                // 移除默认XML声明，并确保不会引入utf-16编码
                result = Regex.Replace(result, @"<\?xml.*\?>", "");
                return result;
            }
        }

    }
}

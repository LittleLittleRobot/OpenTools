using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
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

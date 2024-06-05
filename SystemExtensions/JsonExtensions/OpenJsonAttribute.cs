using System;
using System.Collections.Generic;
using System.Text;

namespace Wesky.Net.OpenTools.SystemExtensions.JsonExtensions
{
    /// <summary>
    /// 自定义属性用于指定JSON键名
    /// Custom attribute to specify JSON key names
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class OpenJsonAttribute : Attribute
    {
        public string KeyName { get; private set; }

        public OpenJsonAttribute(string keyName)
        {
            KeyName = keyName;
        }
    }
}

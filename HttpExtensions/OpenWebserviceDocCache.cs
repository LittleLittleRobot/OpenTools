using System;
using System.Collections.Generic;
using System.Text;

namespace Wesky.Net.OpenTools.HttpExtensions
{
    public class OpenWebserviceDocCache
    {
        /// <summary>
        /// Webservice服务地址  以wsdl结尾
        /// </summary>
        public string WebserviceUrl { get; set; }
        /// <summary>
        /// 服务名
        /// </summary>
        public string OperationName { get; set; }
        /// <summary>
        /// 命名空间
        /// </summary>
        public string Namespace { get; set; }
        /// <summary>
        /// 参数名称集合
        /// </summary>
        public List<string> ParameterNames { get; set; }

        /// <summary>
        /// 重设时间
        /// </summary>
        public DateTime ResetTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 过期秒数，用于重新加载ws服务的doc文档
        /// </summary>
        public long ExpireSeconds { get; set; }

    }
    public class OpenWebserviceInfo
    {
       public static List<OpenWebserviceDocCache> OpenWebservice { get; set; } = new List<OpenWebserviceDocCache>();
    }
}

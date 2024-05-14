using System;
using System.Collections.Generic;
using System.Text;

namespace Wesky.Net.OpenTools.SystemExtensions
{
    public interface IRegistryManager
    {
        /// <summary>
        /// 创建注册表键
        /// Create registry key
        /// </summary>
        /// <param name="root"></param>
        /// <param name="subKey"></param>
        /// <exception cref="Exception"></exception>
        void CreateKey(RegistryRoot root, string subKey);
        /// <summary>
        /// 删除注册表键
        /// Delete registry key
        /// </summary>
        /// <param name="root"></param>
        /// <param name="subKey"></param>
        /// <exception cref="Exception"></exception>
        void DeleteKey(RegistryRoot root, string subKey);
        /// <summary>
        /// 设置注册表值
        /// Set registry value
        /// </summary>
        /// <param name="root"></param>
        /// <param name="subKey"></param>
        /// <param name="valueName"></param>
        /// <param name="value"></param>
        /// <exception cref="Exception"></exception>
        void SetValue(RegistryRoot root, string subKey, string valueName, string value);

        /// <summary>
        /// 获取注册表值
        /// Get registry value
        /// </summary>
        /// <param name="root"></param>
        /// <param name="subKey"></param>
        /// <param name="valueName"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        string GetValue(RegistryRoot root, string subKey, string valueName);
        /// <summary>
        /// 删除注册表值
        /// Delete registry value
        /// </summary>
        /// <param name="root"></param>
        /// <param name="subKey"></param>
        /// <param name="valueName"></param>
        /// <exception cref="Exception"></exception>
        void DeleteValue(RegistryRoot root, string subKey, string valueName);
    }
}

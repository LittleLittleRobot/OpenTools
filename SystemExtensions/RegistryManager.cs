using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Wesky.Net.OpenTools.SystemExtensions
{
    /// <summary>
    /// 注册表操作
    /// Registry operation
    /// </summary>
    public class RegistryManager: IRegistryManager
    {
        // P/Invoke 声明
        // P/Invoke declarations
        private const int KEY_QUERY_VALUE = 0x0001;
        private const int KEY_SET_VALUE = 0x0002;
        private const int KEY_CREATE_SUB_KEY = 0x0004;
        private const int KEY_ENUMERATE_SUB_KEYS = 0x0008;
        private const int KEY_NOTIFY = 0x0010;
        private const int KEY_CREATE_LINK = 0x0020;
        private const int KEY_WOW64_32KEY = 0x0200;
        private const int KEY_WOW64_64KEY = 0x0100;
        private const int KEY_READ = 0x20019;
        private const int KEY_WRITE = 0x20006;

        private const int REG_OPTION_NON_VOLATILE = 0x00000000;

        private const int REG_CREATED_NEW_KEY = 0x00000001;
        private const int REG_OPENED_EXISTING_KEY = 0x00000002;

        private const int REG_SZ = 1;
        private const int REG_DWORD = 4;

        private const int ERROR_SUCCESS = 0;

        private const int RRF_RT_REG_SZ = 0x00000002;

        private static readonly IntPtr HKEY_CLASSES_ROOT = new IntPtr(unchecked((int)0x80000000));
        private static readonly IntPtr HKEY_CURRENT_USER = new IntPtr(unchecked((int)0x80000001));
        private static readonly IntPtr HKEY_LOCAL_MACHINE = new IntPtr(unchecked((int)0x80000002));
        private static readonly IntPtr HKEY_USERS = new IntPtr(unchecked((int)0x80000003));
        private static readonly IntPtr HKEY_CURRENT_CONFIG = new IntPtr(unchecked((int)0x80000005));

        [DllImport("advapi32.dll", CharSet = CharSet.Auto)]
        private static extern int RegCreateKeyEx(
            IntPtr hKey,
            string lpSubKey,
            int Reserved,
            string lpClass,
            int dwOptions,
            int samDesired,
            IntPtr lpSecurityAttributes,
            out IntPtr phkResult,
            out int lpdwDisposition);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto)]
        private static extern int RegOpenKeyEx(
            IntPtr hKey,
            string lpSubKey,
            int ulOptions,
            int samDesired,
            out IntPtr phkResult);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto)]
        private static extern int RegCloseKey(IntPtr hKey);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto)]
        private static extern int RegSetValueEx(
            IntPtr hKey,
            string lpValueName,
            int Reserved,
            int dwType,
            byte[] lpData,
            int cbData);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto)]
        private static extern int RegGetValue(
            IntPtr hKey,
            string lpSubKey,
            string lpValue,
            int dwFlags,
            out int pdwType,
            StringBuilder pvData,
            ref int pcbData);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto)]
        private static extern int RegDeleteKey(IntPtr hKey, string lpSubKey);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto)]
        private static extern int RegDeleteValue(IntPtr hKey, string lpValueName);

        /// <summary>
        /// 获取注册表根键
        /// Get registry root key
        /// </summary>
        /// <param name="root"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private IntPtr GetRegistryRootKey(RegistryRoot root)
        {
            switch (root)
            {
                case RegistryRoot.ClassesRoot:
                    return HKEY_CLASSES_ROOT;
                case RegistryRoot.CurrentUser:
                    return HKEY_CURRENT_USER;
                case RegistryRoot.LocalMachine:
                    return HKEY_LOCAL_MACHINE;
                case RegistryRoot.Users:
                    return HKEY_USERS;
                case RegistryRoot.CurrentConfig:
                    return HKEY_CURRENT_CONFIG;
                default:
                    throw new ArgumentOutOfRangeException(nameof(root), root, null);
            }
        }

        /// <summary>
        /// 创建注册表键
        /// Create registry key
        /// </summary>
        /// <param name="root"></param>
        /// <param name="subKey"></param>
        /// <exception cref="Exception"></exception>
        public void CreateKey(RegistryRoot root, string subKey)
        {
            IntPtr hKey = GetRegistryRootKey(root);
            int result = RegCreateKeyEx(hKey, subKey, 0, null, REG_OPTION_NON_VOLATILE, KEY_WRITE, IntPtr.Zero, out IntPtr phkResult, out _);

            if (result != ERROR_SUCCESS)
            {
                throw new Exception("创建注册表key失败。 Failed to create registry key.");
            }

            RegCloseKey(phkResult);
        }

        /// <summary>
        /// 删除注册表键
        /// Delete registry key
        /// </summary>
        /// <param name="root"></param>
        /// <param name="subKey"></param>
        /// <exception cref="Exception"></exception>
        public void DeleteKey(RegistryRoot root, string subKey)
        {
            IntPtr hKey = GetRegistryRootKey(root);
            int result = RegDeleteKey(hKey, subKey);

            if (result != ERROR_SUCCESS)
            {
                throw new Exception("删除注册表key失败。Failed to delete registry key.");
            }
        }

        /// <summary>
        /// 设置注册表值
        /// Set registry value
        /// </summary>
        /// <param name="root"></param>
        /// <param name="subKey"></param>
        /// <param name="valueName"></param>
        /// <param name="value"></param>
        /// <exception cref="Exception"></exception>
        public void SetValue(RegistryRoot root, string subKey, string valueName, string value)
        {
            IntPtr hKey = GetRegistryRootKey(root);

            int result = RegOpenKeyEx(hKey, subKey, 0, KEY_WRITE, out IntPtr phkResult);
            if (result != ERROR_SUCCESS)
            {
                throw new Exception("打开注册表key失败。Failed to open registry key.");
            }

            byte[] data = Encoding.Unicode.GetBytes(value);
            result = RegSetValueEx(phkResult, valueName, 0, REG_SZ, data, data.Length);

            if (result != ERROR_SUCCESS)
            {
                throw new Exception("设置注册表值失败。Failed to set registry value.");
            }

            RegCloseKey(phkResult);
        }

        /// <summary>
        /// 获取注册表值
        /// Get registry value
        /// </summary>
        /// <param name="root"></param>
        /// <param name="subKey"></param>
        /// <param name="valueName"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public string GetValue(RegistryRoot root, string subKey, string valueName)
        {
            IntPtr hKey = GetRegistryRootKey(root);

            int result = RegOpenKeyEx(hKey, subKey, 0, KEY_READ, out IntPtr phkResult);
            if (result != ERROR_SUCCESS)
            {
                throw new Exception("打开注册表key失败。Failed to open registry key.");
            }

            int type = 0;
            int size = 1024;
            StringBuilder data = new StringBuilder(size);

            result = RegGetValue(phkResult, null, valueName, RRF_RT_REG_SZ, out type, data, ref size);

            if (result != ERROR_SUCCESS)
            {
                throw new Exception("获取注册表的值失败。Failed to get registry value.");
            }

            RegCloseKey(phkResult);

            return data.ToString();
        }

        /// <summary>
        /// 删除注册表值
        /// Delete registry value
        /// </summary>
        /// <param name="root"></param>
        /// <param name="subKey"></param>
        /// <param name="valueName"></param>
        /// <exception cref="Exception"></exception>
        public void DeleteValue(RegistryRoot root, string subKey, string valueName)
        {
            IntPtr hKey = GetRegistryRootKey(root);

            int result = RegOpenKeyEx(hKey, subKey, 0, KEY_WRITE, out IntPtr phkResult);
            if (result != ERROR_SUCCESS)
            {
                throw new Exception("打开注册表key失败。Failed to open registry key.");
            }

            result = RegDeleteValue(phkResult, valueName);

            if (result != ERROR_SUCCESS)
            {
                throw new Exception("删除注册表的值失败。Failed to delete registry value.");
            }

            RegCloseKey(phkResult);
        }
    }
}

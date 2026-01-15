namespace _3rdBy.ByFunc.Encryption.Example.Scripts.Helper
{
    /*
     * Windows卸载检测助手
     * API 切换 .Net Framework
     */
    using System;
    using Microsoft.Win32;
    using UnityEngine;

    /// <summary>
    /// 卸载检测助手
    /// </summary>
    public class UninstallDetectionHelper : MonoBehaviour
    {
        public void Test()
        {
            // Win+R 输入regedit打开注册表
            // HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall
            const string uninstallPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\YourSoftware";
            var isUninstalled = IsUninstalled(uninstallPath);
            Debug.Log(isUninstalled ? "软件被卸载了" : "软件未被卸载");
        }

        private static bool IsUninstalled(string uninstallRegistryPath)
        {
            try
            {
                var key = Registry.LocalMachine.OpenSubKey(uninstallRegistryPath); // 从注册表中获取指定路径的软件信息
                if (key == null)
                {
                    Debug.Log("软件可能已被卸载");
                    return true;
                }

                Debug.Log("软件未被卸载");
                return false;
            }
            catch (Exception)
            {
                Debug.LogError("发生异常，可能是权限问题或其他问题");
                return true;
            }
        }
    }
}
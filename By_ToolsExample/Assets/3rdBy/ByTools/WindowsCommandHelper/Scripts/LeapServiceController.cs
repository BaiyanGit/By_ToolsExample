using System;
using System.Diagnostics;
using System.Threading.Tasks;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace _3rdBy.ByTools.WindowsCommandHelper.Scripts
{
    public class LeapServiceController : MonoBehaviour
    {
        [SerializeField, Header("3.2版本服务")]  private string _leapServiceName      = "Leap Service";
        [SerializeField, Header("5.2版本服务")]  private string _ultraleapServiceName = "UltraleapTracking";
        [SerializeField, Header("VR应用进程名称")] private string _vrMonitorName        = "vrmonitor.exe";
        [SerializeField, Header("VR服务进程名称")] private string _vrServiceName        = "vrserver.exe";
        [SerializeField, Header("Steam程序")]  private string _steamExeName         = "steam.exe";

        private static string StartCmd(string    serviceName) => $"net start \"{serviceName}\"";
        private static string StopCmd(string     serviceName) => $"net stop \"{serviceName}\" >nul 2>&1";
        private static string StopProcess(string processName) => $"taskkill /f /im \"{processName}\" >nul 2>&1";
        private static string StartSteam(string  exeName)     => $"start \"\" \"C:\\Program Files (x86)\\Steam\\{exeName}\" -applaunch 250820 >nul 2>&1";

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
                StartLeapService();

            if (Input.GetKeyDown(KeyCode.Alpha2))
                StartUltraleapTrackingService();
        }

        private async void StartLeapService()
        {
            StopVRService();
            await Task.Delay(1000);
            RunCommandWithAdmin(StopCmd(_ultraleapServiceName));
            await Task.Delay(1000);
            RunCommandWithAdmin(StartCmd(_leapServiceName));
            await Task.Delay(1000);
            RunCommandWithAdmin(StartSteam(_steamExeName));
        }

        private async void StartUltraleapTrackingService()
        {
            StopVRService();
            await Task.Delay(1000);
            RunCommandWithAdmin(StopCmd(_leapServiceName));
            await Task.Delay(1000);
            RunCommandWithAdmin(StartCmd(_ultraleapServiceName));
            await Task.Delay(1000);
            RunCommandWithAdmin(StartSteam(_steamExeName));
        }

        private async void StopVRService()
        {
            Debug.Log("正在启动 VR 服务...");
            RunCommandWithAdmin(StopProcess(_vrServiceName));
            RunCommandWithAdmin(StopProcess(_vrMonitorName));

            await Task.Delay(1000);
        }

        private static void RunCommandWithAdmin(string command)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName        = "cmd.exe",
                    Arguments       = "/c " + command,
                    Verb            = "runas",
                    UseShellExecute = true,
                    CreateNoWindow  = true,
                    WindowStyle     = ProcessWindowStyle.Hidden
                };
                Process.Start(psi);
                Debug.Log($"[ServiceAdmin] 已以管理员权限执行命令: {command}");
            }
            catch (Exception ex)
            {
                Debug.LogError("请求管理员权限失败: " + ex.Message);
            }
        }
    }
}
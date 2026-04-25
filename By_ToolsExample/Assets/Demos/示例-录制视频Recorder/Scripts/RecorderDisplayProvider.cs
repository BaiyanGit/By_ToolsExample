//=====================================================
// 文件名称: RecorderDisplayProvider
// 创 建 者: wangbaiyan
// 创建日期: 2026-4-13
// 描    述: 录屏显示器信息提供器，根据不同平台获取当前系统中的显示器列表。
//=====================================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using UnityEngine;
using Debug = UnityEngine.Debug;

public static class RecorderDisplayProvider
{
    /// <summary>
    /// 获取当前平台下的显示器列表。
    /// </summary>
    /// <returns>显示器信息列表。</returns>
    public static List<RecorderDisplayInfo> GetDisplays()
    {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        return GetDisplaysWindows();
#elif UNITY_STANDALONE_LINUX || UNITY_EDITOR_LINUX
        return GetDisplaysLinux();
#else
        Debug.LogError("当前平台不支持录屏！");
        return new List<RecorderDisplayInfo>();
#endif
    }

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN

    #region Windows Native Methods

    /// <summary>
    /// 枚举系统中的显示器。
    /// </summary>
    [DllImport("user32.dll")]
    private static extern bool EnumDisplayMonitors(
        IntPtr hdc,
        IntPtr lprcClip,
        MonitorEnumDelegate lpfnEnum,
        IntPtr dwData);

    /// <summary>
    /// 获取指定显示器的信息。
    /// </summary>
    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern bool GetMonitorInfo(
        IntPtr hMonitor,
        ref Monitorinfo lpmi);

    /// <summary>
    /// 枚举显示设备。
    /// </summary>
    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern bool EnumDisplayDevices(
        string lpDevice,
        uint iDevNum,
        ref DisplayDevice lpDisplayDevice,
        uint dwFlags);

    /// <summary>
    /// 显示器枚举回调委托。
    /// </summary>
    private delegate bool MonitorEnumDelegate(
        IntPtr hMonitor,
        IntPtr hdcMonitor,
        ref Rect lprcMonitor,
        IntPtr dwData);

    /// <summary>
    /// Windows 矩形结构。
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    private struct Rect
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }

    /// <summary>
    /// Windows 显示器信息结构。
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private struct Monitorinfo
    {
        public int cbSize;
        public Rect rcMonitor;
        public Rect rcWork;
        public uint dwFlags;
    }

    /// <summary>
    /// Windows 显示设备结构。
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private struct DisplayDevice
    {
        public int cb;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string DeviceName;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceString;

        public uint StateFlags;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceID;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceKey;
    }

    private const int MONITORINFOF_PRIMARY = 0x00000001;
    private const uint DISPLAY_DEVICE_ATTACHED_TO_DESKTOP = 0x00000001;
    private const uint DISPLAY_DEVICE_MIRRORING_DRIVER = 0x00000008;

    #endregion

    /// <summary>
    /// Windows 监视器中间数据结构。
    /// </summary>
    private class MonitorInfo
    {
        [Header("设备名称")] public string deviceName;

        [Header("显示器宽度")] public int width;

        [Header("显示器高度")] public int height;

        [Header("X偏移")] public int offsetX;

        [Header("Y偏移")] public int offsetY;

        [Header("是否主显示器")] public bool isPrimary;
    }

    /// <summary>
    /// 获取 Windows 平台下的显示器列表。
    /// </summary>
    /// <returns>显示器信息列表。</returns>
    private static List<RecorderDisplayInfo> GetDisplaysWindows()
    {
        var monitors = new List<MonitorInfo>();
        var results  = new List<RecorderDisplayInfo>();

        EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, callback, IntPtr.Zero);

        for (int i = 0; i < monitors.Count; i++)
        {
            var m = monitors[i];
            results.Add(new RecorderDisplayInfo
            {
                index     = i,
                name      = string.IsNullOrEmpty(m.deviceName) ? (i + 1).ToString() : m.deviceName,
                width     = m.width,
                height    = m.height,
                offsetX   = m.offsetX,
                offsetY   = m.offsetY,
                isPrimary = m.isPrimary
            });
        }

        return results;

        bool callback(IntPtr hMonitor, IntPtr hdcMonitor, ref Rect lprcMonitor, IntPtr dwData)
        {
            var mi = new Monitorinfo { cbSize = Marshal.SizeOf(typeof(Monitorinfo)) };

            if (GetMonitorInfo(hMonitor, ref mi))
            {
                int displayOrder = monitors.Count + 1;

                monitors.Add(new MonitorInfo
                {
                    deviceName = GetMonitorDeviceName(displayOrder),
                    offsetX    = mi.rcMonitor.left,
                    offsetY    = mi.rcMonitor.top,
                    width      = mi.rcMonitor.right - mi.rcMonitor.left,
                    height     = mi.rcMonitor.bottom - mi.rcMonitor.top,
                    isPrimary  = (mi.dwFlags & MONITORINFOF_PRIMARY) != 0
                });
            }

            return true;
        }
    }

    /// <summary>
    /// 根据显示顺序获取显示器设备名称。
    /// </summary>
    /// <param name="displayOrder">显示器顺序编号。</param>
    /// <returns>设备名称。</returns>
    private static string GetMonitorDeviceName(int displayOrder)
    {
        var device = new DisplayDevice
        {
            cb = Marshal.SizeOf(typeof(DisplayDevice))
        };

        uint deviceIndex          = 0;
        int  attachedDisplayCount = 0;

        while (EnumDisplayDevices(null, deviceIndex, ref device, 0))
        {
            bool attached  = (device.StateFlags & DISPLAY_DEVICE_ATTACHED_TO_DESKTOP) != 0;
            bool mirroring = (device.StateFlags & DISPLAY_DEVICE_MIRRORING_DRIVER) != 0;

            if (attached && !mirroring)
            {
                attachedDisplayCount++;
                if (attachedDisplayCount == displayOrder)
                {
                    return string.IsNullOrEmpty(device.DeviceName)
                               ? $"\\\\.\\DISPLAY{displayOrder}"
                               : device.DeviceName;
                }
            }

            deviceIndex++;
            device = new DisplayDevice
            {
                cb = Marshal.SizeOf(typeof(DisplayDevice))
            };
        }

        return $@"\\.\DISPLAY{displayOrder}";
    }
#endif

#if UNITY_STANDALONE_LINUX || UNITY_EDITOR_LINUX
    /// <summary>
    /// 获取 Linux 平台下的显示器列表。
    /// </summary>
    /// <returns>显示器信息列表。</returns>
    private static List<RecorderDisplayInfo> GetDisplaysLinux()
    {
        var results = new List<RecorderDisplayInfo>();

        try
        {
            using var process = new Process();
            process.StartInfo.FileName               = "xrandr";
            process.StartInfo.Arguments              = "--current";
            process.StartInfo.UseShellExecute        = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.CreateNoWindow         = true;
            process.Start();

            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            const string pattern = @"^(\S+)\s+connected\s+(primary\s+)?(\d+)x(\d+)([+-]\d+)([+-]\d+)";
            var          regex   = new Regex(pattern, RegexOptions.Multiline);

            int index = 0;
            foreach (Match match in regex.Matches(output))
            {
                results.Add(new RecorderDisplayInfo
                {
                    index     = index,
                    name      = match.Groups[1].Value,
                    isPrimary = match.Groups[2].Value == "primary ",
                    width     = int.Parse(match.Groups[3].Value),
                    height    = int.Parse(match.Groups[4].Value),
                    offsetX   = int.Parse(match.Groups[5].Value),
                    offsetY   = int.Parse(match.Groups[6].Value)
                });

                index++;
            }
        }
        catch (Exception e)
        {
            Debug.LogError("无法执行 xrandr: " + e.Message);
        }

        return results;
    }
#endif
}
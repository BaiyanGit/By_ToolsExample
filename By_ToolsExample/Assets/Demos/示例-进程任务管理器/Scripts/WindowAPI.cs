namespace Demos.示例_进程任务管理器.Scripts
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Text;
    using UnityEngine;
    using UnityEngine.Events;
    using Debug = UnityEngine.Debug;

    public enum WinCmdShow
    {
        [Description("窗口置顶")] HwndTopmost = -1,
        [Description("隐藏窗口并激活其他窗口")] Hide = 0,
        [Description("激活并显示窗口")] ShowNormal = 1, //如果窗口是最小化或最大化的，系统将其恢复到正常大小
        [Description("激活窗口并将其最小化")] ShowMinimized = 2,
        [Description("激活窗口并将其最大化")] ShowMaximized = 3,
        [Description("显示窗口但不激活它")] ShowNoActivate = 4,
        [Description("激活窗口并以当前大小和状态显示它")] Show = 5,
        [Description("最小化窗口，但不激活其他窗口")] Minimize = 6,
        [Description("显示窗口为最小化状态，但不激活它")] ShowMinNoActive = 7,
        [Description("以当前大小和状态显示窗口，但不激活它")] ShowNa = 8,
        [Description("激活并恢复窗口到其原来的大小和位置")] Restore = 9,

        //根据窗口的当前状态，显示窗口的默认状态（这取决于窗口的创建参数）
        [Description("窗口设置为ShowNormal、ShowMinimized、ShowMaximized")]
        ShowDefault = 10,
        [Description("强制窗口最小化，即使它是当前激活的窗口")] ForceMinimize = 11
    }

    public enum WindowState
    {
        [InspectorName("未知状态")] Unknown,
        [InspectorName("恢复大小")] RestoredSize,
        [InspectorName("最小化")] Minimized,
        [InspectorName("最大化")] Maximized,
    }

    public enum WindowActive
    {
        [InspectorName("未知状态")] Unknown,
        [InspectorName("窗口禁用")] Disabled,
        [InspectorName("窗口激活")] Activated,
    }

    public enum WindowBorderMode
    {
        [InspectorName("默认边框")] Normal,
        [InspectorName("无边框")] Borderless,
    }

    public enum WindowHitTestMode
    {
        [InspectorName("正常点击")] Normal,
        [InspectorName("鼠标穿透")] ClickThrough,
    }

    public enum TrayMouseEvent
    {
        [InspectorName("未知事件")] Unknown,
        [InspectorName("左键单击")] LeftClick,
        [InspectorName("左键双击")] LeftDoubleClick,
        [InspectorName("右键单击")] RightClick,
        [InspectorName("中键单击")] MiddleClick,
    }

    public partial class WindowAPI
    {
        #region 窗口控制

        private const int GWL_STYLE = -16;
        private const int GWL_EX_STYLE = -20;
        private const int HWND_TOPMOST = -1;
        private const int HWND_NOT_TOPMOST = -2;
        private const int SWP_NO_SIZE = 0x0001;
        private const int SWP_NO_MOVE = 0x0002;
        private const int SWP_NO_Z_ORDER = 0x0004;
        private const int SWP_NO_ACTIVATE = 0x0010;
        private const int SWP_FRAME_CHANGED = 0x0020;
        private const int SWP_SHOW_WINDOW = 0x0040;
        private const int SWP_HIDE_WINDOW = 0x0080;
        private const int WM_CLOSE = 0x0010;
        private const int WM_NCL_BUTTON_DOWN = 0x00A1;
        private const int WM_L_BUTTON_UP = 0x0202;
        private const int HT_CAPTION = 0x02;
        private const int LWA_ALPHA = 0x00000002;
        private const int MONITOR_DEFAULT_TO_NEAREST = 2;
        private const int DEFAULT_WINDOW_WIDTH = 1280;
        private const int DEFAULT_WINDOW_HEIGHT = 720;
        private const int WM_USER = 0x0400;
        private const int WM_TRAY_ICON = WM_USER + 100;
        private const int WM_L_BUTTON_DOWN = 0x0201;
        private const int WM_L_BUTTON_DBL_CLK = 0x0203;
        private const int WM_R_BUTTON_UP = 0x0205;
        private const int WM_M_BUTTON_UP = 0x0208;
        private const int NIM_ADD = 0x00000000;
        private const int NIM_MODIFY = 0x00000001;
        private const int NIM_DELETE = 0x00000002;
        private const int NIM_SET_VERSION = 0x00000004;
        private const int NIF_MESSAGE = 0x00000001;
        private const int NIF_ICON = 0x00000002;
        private const int NIF_TIP = 0x00000004;
        private const int NIF_INFO = 0x00000010;
        private const int NOTIFY_ICON_VERSION4 = 4;
        private const int IDI_APPLICATION = 32512;
        private const uint NIIF_INFO = 0x00000001;
        private static readonly IntPtr hwndTop = IntPtr.Zero;
        private static readonly IntPtr wsPopup = new(unchecked((int)0x80000000));
        private static readonly IntPtr wsOverlappedWindow = new(0x00CF0000);
        private static readonly IntPtr wsExLayered = new(0x00080000);
        private static readonly IntPtr wsExTransparent = new(0x00000020);
        private static readonly IntPtr wsExToolWindow = new(0x00000080);
        private static readonly IntPtr wsExAppWindow = new(0x00040000);
        private static IntPtr _unityWindowHandle;
        private static IntPtr _originalUnityWindowStyle;
        private static bool _hasOriginalUnityWindowStyle;
        private static bool _isTrayIconCreated;
        private static bool _isWindowHiddenToTray;
        private const uint TRAY_ICON_ID = 1;
        private static string _trayToolTip = Application.productName;
        private static IntPtr _trayIconHandle;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct NotifyIconData
        {
            public int CbSize;
            public IntPtr HWnd;
            public uint UID;
            public uint UFlags;
            public uint UCallbackMessage;
            public IntPtr HIcon;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string SzTip;

            public uint DwState;
            public uint DwStateMask;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string SzInfo;

            public uint UTimeoutOrVersion;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
            public string SzInfoTitle;

            public uint DwInfoFlags;
            public Guid GuidItem;
            public IntPtr HBalloonIcon;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct Rect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MonitorInfo
        {
            public int CbSize;
            public Rect RcMonitor;
            public Rect RcWork;
            public uint DwFlags;
        }

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool ShowWindow(IntPtr handle, int nCmdShow);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool IsWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll", EntryPoint = "SetWindowLong", SetLastError = true)]
        private static extern int SetWindowLong32(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", SetLastError = true)]
        private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", EntryPoint = "GetWindowLong", SetLastError = true)]
        private static extern int GetWindowLong32(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr", SetLastError = true)]
        private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool GetWindowRect(IntPtr hWnd, out Rect lpRect);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool GetClientRect(IntPtr hWnd, out Rect lpRect);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool MoveWindow(IntPtr hWnd, int x, int y, int nWidth, int nHeight, bool bRepaint);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool SetWindowText(IntPtr hWnd, string lpString);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MonitorInfo lpmi);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool ReleaseCapture();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SendMessage(IntPtr handle, int wMsg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetActiveWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr LoadIcon(IntPtr hInstance, IntPtr lpIconName);

        [DllImport("shell32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool Shell_NotifyIcon(uint dwMessage, ref NotifyIconData lpData);
#endif

        #endregion

        #region 窗口状态检测

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hwnd, uint msg, IntPtr wParam,
            IntPtr                                         lParam);
#endif

        #endregion
    }

    public partial class WindowAPI : MonoBehaviour
    {
        #region 程序初始化

#if !UNITY_EDITOR && UNITY_STANDALONE_WIN
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        private static void InitWindow()
        {
            ApplyBorderlessCenteredWindow(DefaultWindowWidth, DefaultWindowHeight);
        }
#endif

        #endregion

        #region 窗口控制

        /// <summary>
        /// 控制窗口的可见性
        /// </summary>
        /// <param name="handle">要控制可见性的窗口句柄</param>
        /// <param name="nCmdShow">控制窗口可见性的命令 </param>
        /// <returns>控制窗口可见性成功返回true，否则返回false</returns>
        public static bool ShowWindow(IntPtr handle, WinCmdShow nCmdShow)
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            if (!IsValidWindow(handle))
            {
                Debug.LogError("未获取到窗口句柄，请检查窗口是否已被关闭!!!");
                return false;
            }

            return ShowWindow(handle, (int)nCmdShow);
#else
            return false;
#endif
        }

        /// <summary>
        /// 显示并激活窗口
        /// </summary>
        /// <param name="handle">要显示并激活的窗口句柄</param>
        /// <returns>显示并激活成功返回true，否则返回false</returns>
        public static bool RestoreAndActivateWindow(IntPtr handle)
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            if (!IsValidWindow(handle))
            {
                Debug.LogError("未获取到窗口句柄，请检查窗口是否已被关闭!!!");
                return false;
            }

            var showResult       = ShowWindow(handle, WinCmdShow.Restore);
            var foregroundResult = SetForegroundWindow(handle);
            return showResult && foregroundResult;
#else
            return false;
#endif
        }

        /// <summary>
        /// 最小化指定窗口
        /// </summary>
        /// <param name="handle">要最小化的窗口句柄</param>
        /// <returns>最小化成功返回true，否则返回false</returns>
        public static bool MinimizeWindow(IntPtr handle)
        {
            return ShowWindow(handle, WinCmdShow.ShowMinimized);
        }

        /// <summary>
        /// 隐藏指定窗口
        /// </summary>
        /// <param name="handle">要隐藏的窗口句柄</param>
        /// <returns>隐藏成功返回true，否则返回false</returns>
        public static bool HideWindow(IntPtr handle)
        {
            return ShowWindow(handle, WinCmdShow.Hide);
        }

        /// <summary>
        /// 最小化Unity窗口
        /// </summary>
        public static void WindowMinimize()
        {
            MinimizeWindow(GetUnityWindowHandle());
        }

        /// <summary>
        /// 最大化Unity窗口
        /// </summary>
        public static void WindowMaximize()
        {
            ShowWindow(GetUnityWindowHandle(), WinCmdShow.ShowMaximized);
        }

        /// <summary>
        /// 恢复并激活Unity窗口
        /// </summary>
        public static void WindowRestore()
        {
            RestoreAndActivateWindow(GetUnityWindowHandle());
        }

        /// <summary>
        /// 关闭Unity程序
        /// </summary>
        public static void WindowClose()
        {
            Application.Quit();
        }

        /// <summary>
        /// 获得前台窗口的句柄
        /// </summary>
        /// <returns>前台窗口的句柄</returns>
        public static IntPtr GetForegroundWindowUnity()
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            return GetForegroundWindow();
#else
            return IntPtr.Zero;
#endif
        }

        /// <summary>
        /// 在窗口结构中为指定的窗口设置信息
        /// </summary>
        /// <param name="handle">要设置信息的窗口句柄</param>
        /// <param name="winStyle">要设置窗口的样式</param>
        /// <param name="dwNewLong">要设置的新信息</param>
        /// <returns>设置信息成功返回非零值，否则返回零</returns>
        public static IntPtr SetWindowPos(IntPtr handle, int winStyle, int dwNewLong)
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            return SetWindowLongPtr(handle, winStyle, new IntPtr(dwNewLong));
#else
            return IntPtr.Zero;
#endif
        }

        /// <summary>
        /// 将窗口置顶
        /// </summary>
        /// <param name="hWnd">要置顶的窗口句柄</param>
        public static void SetWindowTop(IntPtr hWnd)
        {
            SetWindowPosUnity(hWnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NO_MOVE | SWP_NO_SIZE | SWP_SHOW_WINDOW);
        }

        /// <summary>
        /// 取消窗口置顶
        /// </summary>
        /// <param name="hWnd">要取消置顶的窗口句柄</param>
        /// <returns>取消置顶成功返回true，否则返回false</returns>
        public static bool CancelWindowTop(IntPtr hWnd)
        {
            return SetWindowPosUnity(hWnd, HWND_NOT_TOPMOST, 0, 0, 0, 0, SWP_NO_MOVE | SWP_NO_SIZE | SWP_SHOW_WINDOW);
        }

        /// <summary>
        /// 为窗口指定一个新位置和状态
        /// </summary>
        /// <param name="hWnd">窗口句柄</param>
        /// <param name="hWndInsertAfter">窗口层级</param>
        /// <param name="x">窗口左上角的 X 坐标</param>
        /// <param name="y">窗口左上角的 Y 坐标</param>
        /// <param name="cx">窗口的宽度</param>
        /// <param name="cy">窗口的高</param>
        /// <param name="uFlags">控制窗口位置和状态的标志</param>
        /// <returns>设置位置和状态成功返回true，否则返回false</returns>
        public static bool SetWindowPosUnity(IntPtr hWnd, int hWndInsertAfter, int x, int y, int cx, int cy,
            uint                                    uFlags)
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            if (!IsValidWindow(hWnd))
            {
                return false;
            }

            return SetWindowPos(hWnd, hWndInsertAfter, x, y, cx, cy, uFlags);
#else
            return false;
#endif
        }

        /// <summary>
        /// 将Unity窗口设置为无边框并居中显示
        /// </summary>
        /// <param name="width">窗口宽度</param>
        /// <param name="height">窗口高度</param>
        /// <returns>设置成功返回true，否则返回false</returns>
        public static bool ApplyBorderlessCenteredWindow(int width, int height)
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            var hwnd = GetUnityWindowHandle();
            if (!IsValidWindow(hwnd))
            {
                return false;
            }

            if (!_hasOriginalUnityWindowStyle)
            {
                _originalUnityWindowStyle    = GetWindowLongPtr(hwnd, GWL_STYLE);
                _hasOriginalUnityWindowStyle = true;
            }

            var rect = GetCenteredRect(width, height);
            SetWindowLongPtr(hwnd, GWL_STYLE, wsPopup);
            return SetWindowPos(hwnd, hwndTop, rect.x, rect.y, rect.width, rect.height,
                SWP_SHOW_WINDOW | SWP_FRAME_CHANGED);
#else
            return false;
#endif
        }

        /// <summary>
        /// 拖拽Unity窗口
        /// </summary>
        public static void DragWindowsMethod()
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            var hwnd = GetUnityWindowHandle();
            if (!IsValidWindow(hwnd))
            {
                return;
            }

            ReleaseCapture();
            SendMessage(hwnd, WM_NCL_BUTTON_DOWN, new IntPtr(HT_CAPTION), IntPtr.Zero);
            SendMessage(hwnd, WM_L_BUTTON_UP, IntPtr.Zero, IntPtr.Zero);
#endif
        }

        /// <summary>
        /// 为当前的应用程序释放鼠标捕获
        /// </summary>
        /// <returns>释放鼠标捕获成功返回true，否则返回false</returns>
        public static bool ReleaseCaptureUnity()
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            return ReleaseCapture();
#else
            return false;
#endif
        }

        /// <summary>
        /// 调用一个窗口的窗口函数，将一条消息发给那个窗口
        /// </summary>
        /// <param name="handle">要发送消息的窗口句柄</param>
        /// <param name="wMsg">要发送的消息类型</param>
        /// <param name="wParam">消息的第一个参数</param>
        /// <param name="lParam">消息的第二个参数</param>
        /// <returns>发送消息成功返回true，否则返回false</returns>
        public static bool SendMessageUnity(IntPtr handle, int wMsg, int wParam, int lParam)
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            if (!IsValidWindow(handle))
            {
                return false;
            }

            SendMessage(handle, wMsg, new IntPtr(wParam), new IntPtr(lParam));
            return true;
#else
            return false;
#endif
        }

        /// <summary>
        /// 获得活动窗口的句柄
        /// </summary>
        /// <returns>当前活动窗口的句柄</returns>
        public static IntPtr GetActiveWindowUnity()
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            return GetActiveWindow();
#else
            return IntPtr.Zero;
#endif
        }

        /// <summary>
        /// 关闭指定窗口
        /// </summary>
        /// <param name="handle">要关闭的窗口句柄</param>
        /// <returns>发送关闭消息成功返回true，否则返回false</returns>
        public static bool CloseWindow(IntPtr handle)
        {
            return SendMessageUnity(handle, WM_CLOSE, 0, 0);
        }

        /// <summary>
        /// 设置窗口标题
        /// </summary>
        /// <param name="handle">窗口句柄</param>
        /// <param name="title">窗口标题</param>
        /// <returns>设置标题成功返回true，否则返回false</returns>
        public static bool SetWindowTitle(IntPtr handle, string title)
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            return IsValidWindow(handle) && SetWindowText(handle, title);

#else
            return false;
#endif
        }

        /// <summary>
        /// 获取窗口标题
        /// </summary>
        /// <param name="handle">窗口句柄</param>
        /// <returns>窗口标题</returns>
        public static string GetWindowTitle(IntPtr handle)
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            if (!IsValidWindow(handle))
            {
                return string.Empty;
            }

            var stringBuilder = new StringBuilder(512);
            GetWindowText(handle, stringBuilder, stringBuilder.Capacity);
            return stringBuilder.ToString();
#else
            return string.Empty;
#endif
        }

        /// <summary>
        /// 获取窗口矩形
        /// </summary>
        /// <param name="handle">窗口句柄</param>
        /// <param name="rect">窗口矩形</param>
        /// <returns>获取成功返回true，否则返回false</returns>
        public static bool TryGetWindowRect(IntPtr handle, out RectInt rect)
        {
            rect = default;
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            if (!IsValidWindow(handle) || !GetWindowRect(handle, out var winRect))
            {
                return false;
            }

            rect = ConvertRect(winRect);
            return true;
#else
            return false;
#endif
        }

        /// <summary>
        /// 获取窗口客户区矩形
        /// </summary>
        /// <param name="handle">窗口句柄</param>
        /// <param name="rect">客户区矩形</param>
        /// <returns>获取成功返回true，否则返回false</returns>
        public static bool TryGetClientRect(IntPtr handle, out RectInt rect)
        {
            rect = default;
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            if (!IsValidWindow(handle) || !GetClientRect(handle, out var winRect))
            {
                return false;
            }

            rect = ConvertRect(winRect);
            return true;
#else
            return false;
#endif
        }

        /// <summary>
        /// 设置窗口位置和大小
        /// </summary>
        /// <param name="handle">窗口句柄</param>
        /// <param name="x">窗口左上角的 X 坐标</param>
        /// <param name="y">窗口左上角的 Y 坐标</param>
        /// <param name="width">窗口宽度</param>
        /// <param name="height">窗口高度</param>
        /// <returns>设置成功返回true，否则返回false</returns>
        public static bool SetWindowRect(IntPtr handle, int x, int y, int width, int height)
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            if (!IsValidWindow(handle))
            {
                return false;
            }

            width  = Mathf.Max(1, width);
            height = Mathf.Max(1, height);
            return MoveWindow(handle, x, y, width, height, true);
#else
            return false;
#endif
        }

        /// <summary>
        /// 移动窗口位置，不改变窗口大小
        /// </summary>
        /// <param name="handle">窗口句柄</param>
        /// <param name="x">窗口左上角的 X 坐标</param>
        /// <param name="y">窗口左上角的 Y 坐标</param>
        /// <returns>移动成功返回true，否则返回false</returns>
        public static bool MoveWindowPosition(IntPtr handle, int x, int y)
        {
            if (!TryGetWindowRect(handle, out var rect))
            {
                return false;
            }

            return SetWindowRect(handle, x, y, rect.width, rect.height);
        }

        /// <summary>
        /// 设置窗口大小，不改变窗口位置
        /// </summary>
        /// <param name="handle">窗口句柄</param>
        /// <param name="width">窗口宽度</param>
        /// <param name="height">窗口高度</param>
        /// <returns>设置成功返回true，否则返回false</returns>
        public static bool ResizeWindow(IntPtr handle, int width, int height)
        {
            if (!TryGetWindowRect(handle, out var rect))
            {
                return false;
            }

            return SetWindowRect(handle, rect.x, rect.y, width, height);
        }

        /// <summary>
        /// 将指定窗口移动到当前显示器工作区中心
        /// </summary>
        /// <param name="handle">窗口句柄</param>
        /// <param name="width">窗口宽度，传入0则保持当前宽度</param>
        /// <param name="height">窗口高度，传入0则保持当前高度</param>
        /// <returns>居中成功返回true，否则返回false</returns>
        public static bool CenterWindow(IntPtr handle, int width = 0, int height = 0)
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            if (!TryGetWindowRect(handle, out var windowRect))
            {
                return false;
            }

            width  = width > 0 ? width : windowRect.width;
            height = height > 0 ? height : windowRect.height;

            var workArea = GetWindowWorkArea(handle);
            var x        = workArea.x + Mathf.Max(0, (workArea.width - width) / 2);
            var y        = workArea.y + Mathf.Max(0, (workArea.height - height) / 2);
            return SetWindowRect(handle, x, y, width, height);
#else
            return false;
#endif
        }

        /// <summary>
        /// 设置Unity窗口标题
        /// </summary>
        /// <param name="title">窗口标题</param>
        /// <returns>设置标题成功返回true，否则返回false</returns>
        public static bool SetUnityWindowTitle(string title)
        {
            return SetWindowTitle(GetUnityWindowHandle(), title);
        }

        /// <summary>
        /// 设置Unity窗口位置和大小
        /// </summary>
        /// <param name="x">窗口左上角的 X 坐标</param>
        /// <param name="y">窗口左上角的 Y 坐标</param>
        /// <param name="width">窗口宽度</param>
        /// <param name="height">窗口高度</param>
        /// <returns>设置成功返回true，否则返回false</returns>
        public static bool SetUnityWindowRect(int x, int y, int width, int height)
        {
            return SetWindowRect(GetUnityWindowHandle(), x, y, width, height);
        }

        /// <summary>
        /// 将Unity窗口移动到当前显示器工作区中心
        /// </summary>
        /// <param name="width">窗口宽度，传入0则保持当前宽度</param>
        /// <param name="height">窗口高度，传入0则保持当前高度</param>
        /// <returns>居中成功返回true，否则返回false</returns>
        public static bool CenterUnityWindow(int width = 0, int height = 0)
        {
            return CenterWindow(GetUnityWindowHandle(), width, height);
        }

        /// <summary>
        /// 设置窗口透明度
        /// </summary>
        /// <param name="handle">窗口句柄</param>
        /// <param name="alpha">透明度，0为完全透明，255为完全不透明</param>
        /// <returns>设置透明度成功返回true，否则返回false</returns>
        public static bool SetWindowOpacity(IntPtr handle, byte alpha)
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            if (!IsValidWindow(handle))
            {
                return false;
            }

            var exStyle = GetWindowLongPtr(handle, GWL_EX_STYLE);
            SetWindowLongPtr(handle, GWL_EX_STYLE, AddStyle(exStyle, wsExLayered));
            return SetLayeredWindowAttributes(handle, 0, alpha, LWA_ALPHA);
#else
            return false;
#endif
        }

        /// <summary>
        /// 设置窗口透明度
        /// </summary>
        /// <param name="handle">窗口句柄</param>
        /// <param name="alpha">透明度，0为完全透明，1为完全不透明</param>
        /// <returns>设置透明度成功返回true，否则返回false</returns>
        public static bool SetWindowOpacity(IntPtr handle, float alpha)
        {
            alpha = Mathf.Clamp01(alpha);
            return SetWindowOpacity(handle, (byte)Mathf.RoundToInt(alpha * 255));
        }

        /// <summary>
        /// 设置Unity窗口透明度
        /// </summary>
        /// <param name="alpha">透明度，0为完全透明，1为完全不透明</param>
        /// <returns>设置透明度成功返回true，否则返回false</returns>
        public static bool SetUnityWindowOpacity(float alpha)
        {
            return SetWindowOpacity(GetUnityWindowHandle(), alpha);
        }

        /// <summary>
        /// 设置窗口鼠标穿透状态
        /// </summary>
        /// <param name="handle">窗口句柄</param>
        /// <param name="hitTestMode">鼠标命中模式</param>
        /// <returns>设置成功返回true，否则返回false</returns>
        public static bool SetWindowHitTestMode(IntPtr handle, WindowHitTestMode hitTestMode)
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            if (!IsValidWindow(handle))
            {
                return false;
            }

            var exStyle = GetWindowLongPtr(handle, GWL_EX_STYLE);
            exStyle = AddStyle(exStyle, wsExLayered);
            exStyle = hitTestMode == WindowHitTestMode.ClickThrough
                          ? AddStyle(exStyle, wsExTransparent)
                          : RemoveStyle(exStyle, wsExTransparent);

            SetWindowLongPtr(handle, GWL_EX_STYLE, exStyle);
            return SetWindowPos(handle, hwndTop, 0, 0, 0, 0,
                SWP_NO_MOVE | SWP_NO_SIZE | SWP_NO_Z_ORDER | SWP_NO_ACTIVATE | SWP_FRAME_CHANGED);
#else
            return false;
#endif
        }

        /// <summary>
        /// 设置Unity窗口鼠标穿透状态
        /// </summary>
        /// <param name="hitTestMode">鼠标命中模式</param>
        /// <returns>设置成功返回true，否则返回false</returns>
        public static bool SetUnityWindowHitTestMode(WindowHitTestMode hitTestMode)
        {
            return SetWindowHitTestMode(GetUnityWindowHandle(), hitTestMode);
        }

        /// <summary>
        /// 设置窗口边框模式
        /// </summary>
        /// <param name="handle">窗口句柄</param>
        /// <param name="borderMode">窗口边框模式</param>
        /// <returns>设置成功返回true，否则返回false</returns>
        public static bool SetWindowBorderMode(IntPtr handle, WindowBorderMode borderMode)
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            if (!IsValidWindow(handle))
            {
                return false;
            }

            var style = borderMode == WindowBorderMode.Borderless ? wsPopup : wsOverlappedWindow;
            SetWindowLongPtr(handle, GWL_STYLE, style);
            return SetWindowPos(handle, hwndTop, 0, 0, 0, 0,
                SWP_NO_MOVE | SWP_NO_SIZE | SWP_NO_Z_ORDER | SWP_NO_ACTIVATE | SWP_FRAME_CHANGED | SWP_SHOW_WINDOW);
#else
            return false;
#endif
        }

        /// <summary>
        /// 设置Unity窗口边框模式
        /// </summary>
        /// <param name="borderMode">窗口边框模式</param>
        /// <returns>设置成功返回true，否则返回false</returns>
        public static bool SetUnityWindowBorderMode(WindowBorderMode borderMode)
        {
            return SetWindowBorderMode(GetUnityWindowHandle(), borderMode);
        }

        /// <summary>
        /// 恢复Unity窗口原始样式
        /// </summary>
        /// <returns>恢复成功返回true，否则返回false</returns>
        public static bool RestoreUnityWindowStyle()
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            var hwnd = GetUnityWindowHandle();
            if (!IsValidWindow(hwnd) || !_hasOriginalUnityWindowStyle)
            {
                return false;
            }

            SetWindowLongPtr(hwnd, GWL_STYLE, _originalUnityWindowStyle);
            return SetWindowPos(hwnd, hwndTop, 0, 0, 0, 0,
                SWP_NO_MOVE | SWP_NO_SIZE | SWP_NO_Z_ORDER | SWP_NO_ACTIVATE | SWP_FRAME_CHANGED | SWP_SHOW_WINDOW);
#else
            return false;
#endif
        }

        /// <summary>
        /// 设置窗口是否显示在任务栏
        /// </summary>
        /// <param name="handle">窗口句柄</param>
        /// <param name="showInTaskbar">是否显示在任务栏</param>
        /// <returns>设置成功返回true，否则返回false</returns>
        public static bool SetWindowTaskbarVisible(IntPtr handle, bool showInTaskbar)
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            if (!IsValidWindow(handle))
            {
                return false;
            }

            var exStyle = GetWindowLongPtr(handle, GWL_EX_STYLE);
            exStyle = showInTaskbar ? AddStyle(RemoveStyle(exStyle, wsExToolWindow), wsExAppWindow) : AddStyle(RemoveStyle(exStyle, wsExAppWindow), wsExToolWindow);

            SetWindowLongPtr(handle, GWL_EX_STYLE, exStyle);
            SetWindowPos(handle, hwndTop, 0, 0, 0, 0, SWP_NO_MOVE | SWP_NO_SIZE | SWP_NO_Z_ORDER | SWP_NO_ACTIVATE | SWP_HIDE_WINDOW);
            return SetWindowPos(handle, hwndTop, 0, 0, 0, 0, SWP_NO_MOVE | SWP_NO_SIZE | SWP_NO_Z_ORDER | SWP_NO_ACTIVATE | SWP_SHOW_WINDOW | SWP_FRAME_CHANGED);
#else
            return false;
#endif
        }

        /// <summary>
        /// 设置Unity窗口是否显示在任务栏
        /// </summary>
        /// <param name="showInTaskbar">是否显示在任务栏</param>
        /// <returns>设置成功返回true，否则返回false</returns>
        public static bool SetUnityWindowTaskbarVisible(bool showInTaskbar)
        {
            return SetWindowTaskbarVisible(GetUnityWindowHandle(), showInTaskbar);
        }

        /// <summary>
        /// 隐藏Unity窗口任务栏按钮
        /// </summary>
        /// <returns>隐藏成功返回true，否则返回false</returns>
        public static bool HideUnityWindowTaskbar()
        {
            return SetUnityWindowTaskbarVisible(false);
        }

        /// <summary>
        /// 显示Unity窗口任务栏按钮
        /// </summary>
        /// <returns>显示成功返回true，否则返回false</returns>
        public static bool ShowUnityWindowTaskbar()
        {
            return SetUnityWindowTaskbarVisible(true);
        }

        /// <summary>
        /// 隐藏Unity窗口但不退出程序
        /// </summary>
        /// <returns>隐藏成功返回true，否则返回false</returns>
        public static bool HideUnityWindow()
        {
            return HideWindow(GetUnityWindowHandle());
        }

        /// <summary>
        /// 显示Unity窗口并激活
        /// </summary>
        /// <returns>显示成功返回true，否则返回false</returns>
        public static bool ShowUnityWindow()
        {
            return RestoreAndActivateWindow(GetUnityWindowHandle());
        }

        #endregion

        #region 托盘控制

        public static readonly UnityEvent<TrayMouseEvent> OnTrayMouseEvent = new(); //托盘鼠标事件
        public static readonly UnityEvent OnTrayLeftClickEvent = new();             //托盘左键单击事件
        public static readonly UnityEvent OnTrayLeftDoubleClickEvent = new();       //托盘左键双击事件
        public static readonly UnityEvent OnTrayRightClickEvent = new();            //托盘右键单击事件
        public static readonly UnityEvent OnTrayMiddleClickEvent = new();           //托盘中键单击事件

        /// <summary>
        /// 初始化系统托盘图标
        /// </summary>
        /// <param name="toolTip">鼠标悬停提示文本，传空则使用产品名称</param>
        /// <param name="hideTaskbarButton">是否同时隐藏任务栏按钮</param>
        /// <returns>初始化成功返回true，否则返回false</returns>
        public static bool InitTrayIcon(string toolTip = "", bool hideTaskbarButton = false)
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            var hwnd = GetUnityWindowHandle();
            if (!IsValidWindow(hwnd))
            {
                Debug.LogError("未获取到Unity窗口句柄，无法初始化托盘图标!!!");
                return false;
            }

            if (_oldWndProc == IntPtr.Zero)
            {
                InitWindowProcedure();
            }

            _trayToolTip = string.IsNullOrEmpty(toolTip) ? Application.productName : toolTip;
            _trayIconHandle = _trayIconHandle == IntPtr.Zero
                                  ? LoadIcon(IntPtr.Zero, new IntPtr(IDI_APPLICATION))
                                  : _trayIconHandle;

            var data = CreateNotifyIconData(NIF_MESSAGE | NIF_ICON | NIF_TIP);
            var result = _isTrayIconCreated
                             ? Shell_NotifyIcon(NIM_MODIFY, ref data)
                             : Shell_NotifyIcon(NIM_ADD, ref data);

            if (!result)
            {
                Debug.LogError("创建系统托盘图标失败!!!");
                return false;
            }

            data.UTimeoutOrVersion = NOTIFY_ICON_VERSION4;
            Shell_NotifyIcon(NIM_SET_VERSION, ref data);
            _isTrayIconCreated = true;

            if (hideTaskbarButton)
            {
                SetWindowTaskbarVisible(hwnd, false);
            }

            return true;
#else
            return false;
#endif
        }

        /// <summary>
        /// 移除系统托盘图标
        /// </summary>
        /// <returns>移除成功返回true，否则返回false</returns>
        public static bool RemoveTrayIcon()
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            if (!_isTrayIconCreated)
            {
                return true;
            }

            var data   = CreateNotifyIconData(0);
            var result = Shell_NotifyIcon(NIM_DELETE, ref data);
            _isTrayIconCreated = false;
            return result;
#else
            return false;
#endif
        }

        /// <summary>
        /// 设置系统托盘图标提示文本
        /// </summary>
        /// <param name="toolTip">鼠标悬停提示文本</param>
        /// <returns>设置成功返回true，否则返回false</returns>
        public static bool SetTrayToolTip(string toolTip)
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            if (!_isTrayIconCreated)
            {
                return InitTrayIcon(toolTip);
            }

            _trayToolTip = string.IsNullOrEmpty(toolTip) ? Application.productName : toolTip;
            var data = CreateNotifyIconData(NIF_TIP);
            return Shell_NotifyIcon(NIM_MODIFY, ref data);
#else
            return false;
#endif
        }

        /// <summary>
        /// 设置系统托盘图标句柄
        /// </summary>
        /// <param name="iconHandle">图标句柄</param>
        /// <returns>设置成功返回true，否则返回false</returns>
        public static bool SetTrayIcon(IntPtr iconHandle)
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            if (iconHandle == IntPtr.Zero)
            {
                return false;
            }

            _trayIconHandle = iconHandle;
            if (!_isTrayIconCreated)
            {
                return InitTrayIcon(_trayToolTip);
            }

            var data = CreateNotifyIconData(NIF_ICON);
            return Shell_NotifyIcon(NIM_MODIFY, ref data);
#else
            return false;
#endif
        }

        /// <summary>
        /// 显示托盘气泡提示
        /// </summary>
        /// <param name="title">提示标题</param>
        /// <param name="content">提示内容</param>
        /// <returns>显示成功返回true，否则返回false</returns>
        public static bool ShowTrayBalloonTip(string title, string content)
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            if (!_isTrayIconCreated && !InitTrayIcon())
            {
                return false;
            }

            var data = CreateNotifyIconData(NIF_INFO);
            data.SzInfoTitle = LimitString(title, 63);
            data.SzInfo      = LimitString(content, 255);
            data.DwInfoFlags = NIIF_INFO;
            return Shell_NotifyIcon(NIM_MODIFY, ref data);
#else
            return false;
#endif
        }

        /// <summary>
        /// 最小化Unity窗口到系统托盘
        /// </summary>
        /// <param name="toolTip">鼠标悬停提示文本，传空则使用产品名称</param>
        /// <returns>最小化到托盘成功返回true，否则返回false</returns>
        public static bool MinimizeUnityWindowToTray(string toolTip = "")
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            if (!InitTrayIcon(toolTip, true))
            {
                return false;
            }

            _isWindowHiddenToTray = true;
            return HideUnityWindow();
#else
            return false;
#endif
        }

        /// <summary>
        /// 从系统托盘恢复Unity窗口
        /// </summary>
        /// <param name="showTaskbarButton">是否恢复任务栏按钮</param>
        /// <returns>恢复成功返回true，否则返回false</returns>
        public static bool RestoreUnityWindowFromTray(bool showTaskbarButton = true)
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            var hwnd = GetUnityWindowHandle();
            if (!IsValidWindow(hwnd))
            {
                return false;
            }

            if (showTaskbarButton)
            {
                SetWindowTaskbarVisible(hwnd, true);
            }

            _isWindowHiddenToTray = false;
            return RestoreAndActivateWindow(hwnd);
#else
            return false;
#endif
        }

        /// <summary>
        /// 切换Unity窗口托盘隐藏状态
        /// </summary>
        /// <param name="toolTip">鼠标悬停提示文本，传空则使用产品名称</param>
        /// <returns>切换成功返回true，否则返回false</returns>
        public static bool ToggleUnityWindowTrayState(string toolTip = "")
        {
            return _isWindowHiddenToTray ? RestoreUnityWindowFromTray() : MinimizeUnityWindowToTray(toolTip);
        }

        /// <summary>
        /// 获取Unity窗口是否已经隐藏到托盘
        /// </summary>
        /// <returns>已经隐藏到托盘返回true，否则返回false</returns>
        public static bool IsUnityWindowHiddenToTray()
        {
            return _isWindowHiddenToTray;
        }

        /// <summary>
        /// 退出程序前清理托盘图标
        /// </summary>
        public static void DisposeTrayIcon()
        {
            RemoveTrayIcon();
        }

        /// <summary>
        /// 创建托盘图标数据
        /// </summary>
        /// <param name="flags">托盘图标标记</param>
        /// <returns>托盘图标数据</returns>
        private static NotifyIconData CreateNotifyIconData(uint flags)
        {
            return new NotifyIconData
            {
                CbSize           = Marshal.SizeOf<NotifyIconData>(),
                HWnd             = GetUnityWindowHandle(),
                UID              = TRAY_ICON_ID,
                UFlags           = flags,
                UCallbackMessage = WM_TRAY_ICON,
                HIcon            = _trayIconHandle == IntPtr.Zero ? LoadIcon(IntPtr.Zero, new IntPtr(IDI_APPLICATION)) : _trayIconHandle,
                SzTip            = LimitString(_trayToolTip, 127),
                SzInfo           = string.Empty,
                SzInfoTitle      = string.Empty,
                GuidItem         = Guid.Empty,
            };
        }

        /// <summary>
        /// 限制字符串长度
        /// </summary>
        /// <param name="value">原始字符串</param>
        /// <param name="maxLength">最大长度</param>
        /// <returns>限制长度后的字符串</returns>
        private static string LimitString(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            return value.Length <= maxLength ? value : value[..maxLength];
        }

        #endregion

        #region 进程获取窗口句柄

        /// <summary>
        /// 获取Unity窗口句柄
        /// </summary>
        /// <returns>Unity窗口句柄</returns>
        public static IntPtr GetUnityWindowHandle()
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            if (_unityWindowHandle != IntPtr.Zero && IsWindow(_unityWindowHandle))
            {
                return _unityWindowHandle;
            }

            using (var currentProcess = Process.GetCurrentProcess())
            {
                _unityWindowHandle = currentProcess.MainWindowHandle;
            }

            if (_unityWindowHandle == IntPtr.Zero)
            {
                _unityWindowHandle = FindWindow(null, Application.productName);
            }

            return _unityWindowHandle;
#else
            return IntPtr.Zero;
#endif
        }

        /// <summary>
        /// 通过进程名称获取窗口句柄
        /// </summary>
        /// <param name="processName">进程名称</param>
        /// <returns>窗口句柄</returns>
        public static IntPtr GetWindowHandle(string processName)
        {
            var mainWindowHandle = IntPtr.Zero;
            var processArray     = Process.GetProcessesByName(processName);

            foreach (var process in processArray)
            {
                Debug.Log($"获取窗口句柄  进程名: {process.ProcessName} 句柄: {process.MainWindowHandle}");
                if (process.MainWindowHandle == IntPtr.Zero) continue; // 没有窗口句柄则跳过本次循环
                if (process.ProcessName != processName) continue;

                mainWindowHandle = process.MainWindowHandle;
                break;
            }

            return mainWindowHandle;
        }

        /// <summary>
        /// 打印当前活动窗口句柄
        /// </summary>
        public static void PrintHandleLog()
        {
            var processArray = Process.GetProcesses();
            var index        = 0;
            Debug.Log("进程数量：" + processArray.Length);
            foreach (var process in processArray)
            {
                var processId        = process.Id;                // 进程ID
                var mainWindowHandle = process.MainWindowHandle;  // 主窗口句柄
                var processHandle    = process.Handle.ToString(); // 进程句柄
                var processName      = process.ProcessName;       // 进程名称

                if (mainWindowHandle == IntPtr.Zero) continue; // 没有窗口句柄则跳过本次循环

                index++;
                Debug.Log(
                    $"{index}__进程ID: {processId} 主窗口句柄: {mainWindowHandle} 进程句柄: {processHandle} 进程名称: {processName}");
            }
        }

        /// <summary>
        /// 获取当前显示器工作区
        /// </summary>
        /// <param name="handle">窗口句柄</param>
        /// <returns>显示器工作区矩形</returns>
        private static RectInt GetWindowWorkArea(IntPtr handle)
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            var monitor     = MonitorFromWindow(handle, MONITOR_DEFAULT_TO_NEAREST);
            var monitorInfo = new MonitorInfo { CbSize = Marshal.SizeOf<MonitorInfo>() };
            if (monitor != IntPtr.Zero && GetMonitorInfo(monitor, ref monitorInfo))
            {
                return ConvertRect(monitorInfo.RcWork);
            }
#endif
            return GetCenteredRect(Screen.width, Screen.height);
        }

        /// <summary>
        /// 转换Win32矩形为Unity矩形
        /// </summary>
        /// <param name="rect">Win32矩形</param>
        /// <returns>Unity矩形</returns>
        private static RectInt ConvertRect(Rect rect)
        {
            return new RectInt(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);
        }

        /// <summary>
        /// 添加窗口样式
        /// </summary>
        /// <param name="style">原始样式</param>
        /// <param name="addStyle">要添加的样式</param>
        /// <returns>添加后的样式</returns>
        private static IntPtr AddStyle(IntPtr style, IntPtr addStyle)
        {
            return IntPtr.Size == 8
                       ? new IntPtr(style.ToInt64() | addStyle.ToInt64())
                       : new IntPtr(style.ToInt32() | addStyle.ToInt32());
        }

        /// <summary>
        /// 移除窗口样式
        /// </summary>
        /// <param name="style">原始样式</param>
        /// <param name="removeStyle">要移除的样式</param>
        /// <returns>移除后的样式</returns>
        private static IntPtr RemoveStyle(IntPtr style, IntPtr removeStyle)
        {
            return IntPtr.Size == 8
                       ? new IntPtr(style.ToInt64() & ~removeStyle.ToInt64())
                       : new IntPtr(style.ToInt32() & ~removeStyle.ToInt32());
        }

        /// <summary>
        /// 检测窗口句柄是否有效
        /// </summary>
        /// <param name="hwnd">窗口句柄</param>
        /// <returns>窗口句柄有效返回true，否则返回false</returns>
        private static bool IsValidWindow(IntPtr hwnd)
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            return hwnd != IntPtr.Zero && IsWindow(hwnd);
#else
            return false;
#endif
        }

        /// <summary>
        /// 获取居中窗口矩形
        /// </summary>
        /// <param name="width">窗口宽度</param>
        /// <param name="height">窗口高度</param>
        /// <returns>居中窗口矩形</returns>
        private static RectInt GetCenteredRect(int width, int height)
        {
            var screenWidth  = Screen.currentResolution.width > 0 ? Screen.currentResolution.width : Screen.width;
            var screenHeight = Screen.currentResolution.height > 0 ? Screen.currentResolution.height : Screen.height;

            width  = Mathf.Max(1, width);
            height = Mathf.Max(1, height);

            var x = Mathf.Max(0, (screenWidth - width) / 2);
            var y = Mathf.Max(0, (screenHeight - height) / 2);

            return new RectInt(x, y, width, height);
        }

        /// <summary>
        /// 根据程序运行位数获取窗口信息
        /// </summary>
        /// <param name="hWnd">窗口句柄</param>
        /// <param name="nIndex">要获取的窗口信息</param>
        /// <returns>窗口信息</returns>
        private static IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex)
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            if (IntPtr.Size == 8)
            {
                return GetWindowLongPtr64(hWnd, nIndex);
            }

            return new IntPtr(GetWindowLong32(hWnd, nIndex));
#else
            return IntPtr.Zero;
#endif
        }

        /// <summary>
        /// 根据程序运行位数设置窗口信息
        /// </summary>
        /// <param name="hWnd">窗口句柄</param>
        /// <param name="nIndex">要设置的窗口信息</param>
        /// <param name="dwNewLong">要设置的新信息</param>
        /// <returns>设置信息成功返回非零值，否则返回零</returns>
        private static IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            if (IntPtr.Size == 8)
            {
                return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
            }

            return new IntPtr(SetWindowLong32(hWnd, nIndex, dwNewLong.ToInt32()));
#else
            return IntPtr.Zero;
#endif
        }

        #endregion

        #region 窗口状态检测

        private const int WM_SIZE = 0x0005;
        private const int WM_ACTIVATE = 0x0006;
        private const int GWL_WNDPROC = -4;
        private static IntPtr _hwnd;
        private static IntPtr _oldWndProc;
        private static WndProcDelegate _newWndProc;
        private static WindowState _lastWindowState = WindowState.Unknown;
        private static WindowActive _lastWindowMessage = WindowActive.Unknown;

        private delegate IntPtr WndProcDelegate(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam);

        public static readonly UnityEvent<WindowState> OnWindowSizeChangedEvent = new(); //窗口大小变化事件
        public static readonly UnityEvent<WindowActive> OnWindowActivatedEvent = new();  //窗口激活事件

        /// <summary>
        /// 初始化窗口过程
        /// </summary>
        public static void InitWindowProcedure()
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            if (_oldWndProc != IntPtr.Zero)
            {
                return;
            }

            _hwnd = GetUnityWindowHandle(); // 获取 Unity 窗口句柄
            if (!IsValidWindow(_hwnd))
            {
                Debug.LogError($"找不到[ {Application.productName} ]窗口句柄.");
                return;
            }

            _newWndProc = WindowProc;
            _oldWndProc = SetWindowLongPtr(_hwnd, GWL_WNDPROC, Marshal.GetFunctionPointerForDelegate(_newWndProc));
#endif
        }

        /// <summary>
        /// 恢复窗口过程(程序关闭时触发调用)
        /// </summary>
        public static void RestoreWindowProcedure()
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            if (_hwnd != IntPtr.Zero && _oldWndProc != IntPtr.Zero)
            {
                SetWindowLongPtr(_hwnd, GWL_WNDPROC, _oldWndProc);
                _hwnd       = IntPtr.Zero;
                _oldWndProc = IntPtr.Zero;
                _newWndProc = null;
            }
#endif
        }

        /// <summary>
        /// 处理窗口消息的回调函数
        /// </summary>
        /// <param name="hwnd"> 窗口句柄 </param>
        /// <param name="msg"> 消息类型 </param>
        /// <param name="wParam"> 附加参数1 </param>
        /// <param name="lParam"> 附加参数2 </param>
        /// <returns>调用前一个窗口过程后的结果</returns>
        private static IntPtr WindowProc(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            var state   = WindowState.Unknown;
            var message = WindowActive.Unknown;

            if (msg == WM_TRAY_ICON)
            {
                HandleTrayMessage(lParam);
            }

            switch (msg)
            {
                case WM_SIZE:
                    var sizeType = wParam.ToInt32();

                    state = sizeType switch
                    {
                        // 窗口被恢复到普通大小
                        0 => WindowState.RestoredSize,
                        // 窗口被最小化
                        1 => WindowState.Minimized,
                        // 窗口被最大化
                        2 => WindowState.Maximized,
                        _ => state
                    };

                    break;

                case WM_ACTIVATE:
                    var activationState = wParam.ToInt32() & 0xFFFF;

                    message = activationState switch
                    {
                        0 => WindowActive.Disabled,  // 窗口已禁用（最小化到任务栏）
                        1 => WindowActive.Activated, // 窗口已激活（置于最前面）
                        _ => message
                    };

                    break;
            }

            // 窗口状态或消息未发生变化时，不处理
            if (state != WindowState.Unknown && state != _lastWindowState)
            {
                _lastWindowState = state;
                OnWindowSizeChangedEvent?.Invoke(state);
            }

            if (message != WindowActive.Unknown && message != _lastWindowMessage)
            {
                _lastWindowMessage = message;
                OnWindowActivatedEvent?.Invoke(message);
            }

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            return CallWindowProc(_oldWndProc, hwnd, msg, wParam, lParam);
#else
            return IntPtr.Zero;
#endif
        }

        /// <summary>
        /// 处理系统托盘消息
        /// </summary>
        /// <param name="lParam">托盘消息参数</param>
        private static void HandleTrayMessage(IntPtr lParam)
        {
            var trayMouseEvent = lParam.ToInt32() switch
            {
                WM_L_BUTTON_DOWN    => TrayMouseEvent.LeftClick,
                WM_L_BUTTON_DBL_CLK => TrayMouseEvent.LeftDoubleClick,
                WM_R_BUTTON_UP      => TrayMouseEvent.RightClick,
                WM_M_BUTTON_UP      => TrayMouseEvent.MiddleClick,
                _                   => TrayMouseEvent.Unknown
            };

            if (trayMouseEvent == TrayMouseEvent.Unknown)
            {
                return;
            }

            OnTrayMouseEvent?.Invoke(trayMouseEvent);

            switch (trayMouseEvent)
            {
                case TrayMouseEvent.LeftClick:
                    OnTrayLeftClickEvent?.Invoke();
                    break;
                case TrayMouseEvent.LeftDoubleClick:
                    OnTrayLeftDoubleClickEvent?.Invoke();
                    RestoreUnityWindowFromTray();
                    break;
                case TrayMouseEvent.RightClick:
                    OnTrayRightClickEvent?.Invoke();
                    break;
                case TrayMouseEvent.MiddleClick:
                    OnTrayMiddleClickEvent?.Invoke();
                    break;
            }
        }

        #endregion
    }
}
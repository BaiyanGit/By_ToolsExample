namespace KeyPad
{
    using System;
    using System.Runtime.InteropServices;
    using UnityEngine;

    public enum WindowState
    {
        None = 0,
        Reset = 1,
        Minimize = 2,
        Maximize = 3,
    }

    public class WindowAPI : MonoBehaviour
    {
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hwnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowLong(IntPtr hwnd, int _nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int y, int cx, int cy,
            uint uFlags);

        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        private static extern bool SendMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);

        private static IntPtr _handle;
        private const uint WindowShow = 0x0040; // 窗口显示
        private const int WindowStyle = -16; // 窗口样式 -16
        private const int WindowsPopUp = 0x800000; //窗口样式-无边框

        private static Rect _screenPosition;

        private const int ScreenWidth = 1280; // 屏幕分辨率:宽

        private const int ScreenHeight = 720; // 屏幕分辨率:高

#if !UNITY_EDITOR && UNITY_STANDALONE_WIN
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
    public static void InitWindow()
    {
        var r = Screen.resolutions;
        var posX = (r[^1].width - Screen.width) / 2;
        var posY = (r[^1].height - Screen.height) / 2;
        _screenPosition = new Rect(posX, posY, ScreenWidth, ScreenHeight);

        SetWindowLong(GetForegroundWindow(), WindowStyle, WindowsPopUp); //将网上的WS_BORDER替换成WS_POPUP  
        _handle = GetForegroundWindow(); //FindWindow ((string)null, "popu_windows");

        var width = (int)_screenPosition.width;
        var height = (int)_screenPosition.height;
        var winHandle = GetForegroundWindow();
        SetWindowPos(winHandle, 0, posX, posY, width, height, WindowShow);
    }
#endif


        /// <summary>
        /// 最小化窗口
        /// </summary>
        /// <param name="hwnd"></param>
        public static void WindowMiniimize(IntPtr hwnd)
        {
            ShowWindow(hwnd, WindowState.Minimize.GetHashCode());
        }

        /// <summary>
        /// 显示窗口
        /// </summary>
        /// <param name="swnd"></param>
        public static void ShowWindow(IntPtr swnd)
        {
            ShowWindow(swnd, WindowState.Reset.GetHashCode());
        }

        /// <summary>
        /// 当前窗口最小化
        /// </summary>
        public static void WindowMinimize()
        {
            ShowWindow(GetForegroundWindow(), WindowState.Minimize.GetHashCode());
        }

        /// <summary>
        /// 当前窗口最小化大化
        /// </summary>
        public static void WindowMaximize()
        {
            ShowWindow(GetForegroundWindow(), WindowState.Maximize.GetHashCode());
        }

        /// <summary>
        /// 当前窗口还原
        /// </summary>
        public static void WindowRestore()
        {
            ShowWindow(GetForegroundWindow(), WindowState.Reset.GetHashCode());
        }

        /// <summary>
        /// 当前窗口退出
        /// </summary>
        public static void WindowClose()
        {
            Application.Quit();
        }

        /// <summary>
        /// 窗口拖拽区域  此处调用可以在按钮上添加EventTrigger组件 使用Drag方法
        /// </summary>
        public static void DragWindowsMethod()
        {
            ReleaseCapture();
            SendMessage(_handle, 0xA1, 0x02, 0);
            SendMessage(_handle, 0x0202, 0, 0);
        }
    }
}
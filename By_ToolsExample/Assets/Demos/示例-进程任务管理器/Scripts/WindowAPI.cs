namespace Demos.示例_快捷键切换应用.Scripts.KeyPadAPI
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using UnityEngine;

    public enum WindowState
    {
        None = 0,
        Hide = 0,
        Normal = 1,
        Minimize = 2,
        Maximize = 3,
        Restore = 9,
        Reset = 9,
    }

    public static class WindowAPI
    {
        private const int GwlStyle = -16;
        private static readonly IntPtr HwndTop = IntPtr.Zero;

        private const int SwpShowWindow = 0x0040;
        private const int SwpFrameChanged = 0x0020;

        private static readonly IntPtr WsPopup = new(unchecked((int)0x80000000));

        private const int DefaultWindowWidth = 1280;
        private const int DefaultWindowHeight = 720;

        private static IntPtr _unityWindowHandle;

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool ShowWindow(IntPtr hwnd, int nCmdShow);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool IsWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll", EntryPoint = "SetWindowLong", SetLastError = true)]
        private static extern int SetWindowLong32(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", SetLastError = true)]
        private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool ReleaseCapture();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SendMessage(IntPtr hwnd, int wMsg, IntPtr wParam, IntPtr lParam);
#endif

#if !UNITY_EDITOR && UNITY_STANDALONE_WIN
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        public static void InitWindow()
        {
            ApplyBorderlessCenteredWindow(DefaultWindowWidth, DefaultWindowHeight);
        }
#endif

        public static bool WindowMiniimize(IntPtr hwnd)
        {
            return MinimizeWindow(hwnd);
        }

        public static bool ShowWindow(IntPtr hwnd)
        {
            return RestoreAndActivateWindow(hwnd);
        }

        public static void WindowMinimize()
        {
            MinimizeWindow(GetUnityWindowHandle());
        }

        public static void WindowMaximize()
        {
            ExecuteShowWindow(GetUnityWindowHandle(), WindowState.Maximize);
        }

        public static void WindowRestore()
        {
            RestoreAndActivateWindow(GetUnityWindowHandle());
        }

        public static void WindowClose()
        {
            Application.Quit();
        }

        public static bool MinimizeWindow(IntPtr hwnd)
        {
            return ExecuteShowWindow(hwnd, WindowState.Minimize);
        }

        public static bool HideWindow(IntPtr hwnd)
        {
            return ExecuteShowWindow(hwnd, WindowState.Hide);
        }

        public static bool RestoreAndActivateWindow(IntPtr hwnd)
        {
            if (!IsValidWindow(hwnd))
            {
                return false;
            }

            bool showResult = ExecuteShowWindow(hwnd, WindowState.Restore);

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            bool foregroundResult = SetForegroundWindow(hwnd);
            return showResult && foregroundResult;
#else
            return showResult;
#endif
        }

        public static bool ApplyBorderlessCenteredWindow(int width, int height)
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            IntPtr hwnd = GetUnityWindowHandle();
            if (!IsValidWindow(hwnd))
            {
                return false;
            }

            RectInt rect = GetCenteredRect(width, height);
            SetWindowLongPtr(hwnd, GwlStyle, WsPopup);
            return SetWindowPos(
                hwnd,
                HwndTop,
                rect.x,
                rect.y,
                rect.width,
                rect.height,
                SwpShowWindow | SwpFrameChanged);
#else
            return false;
#endif
        }

        public static void DragWindowsMethod()
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            IntPtr hwnd = GetUnityWindowHandle();
            if (!IsValidWindow(hwnd))
            {
                return;
            }

            ReleaseCapture();
            SendMessage(hwnd, 0xA1, new IntPtr(0x02), IntPtr.Zero);
            SendMessage(hwnd, 0x0202, IntPtr.Zero, IntPtr.Zero);
#endif
        }

        private static bool ExecuteShowWindow(IntPtr hwnd, WindowState state)
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            if (!IsValidWindow(hwnd))
            {
                return false;
            }

            return ShowWindow(hwnd, (int)state);
#else
            return false;
#endif
        }

        private static bool IsValidWindow(IntPtr hwnd)
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            return hwnd != IntPtr.Zero && IsWindow(hwnd);
#else
            return false;
#endif
        }

        private static IntPtr GetUnityWindowHandle()
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            if (_unityWindowHandle != IntPtr.Zero && IsWindow(_unityWindowHandle))
            {
                return _unityWindowHandle;
            }

            using (Process currentProcess = Process.GetCurrentProcess())
            {
                _unityWindowHandle = currentProcess.MainWindowHandle;
            }

            return _unityWindowHandle;
#else
            return IntPtr.Zero;
#endif
        }

        private static RectInt GetCenteredRect(int width, int height)
        {
            int screenWidth = Screen.currentResolution.width > 0 ? Screen.currentResolution.width : Screen.width;
            int screenHeight = Screen.currentResolution.height > 0 ? Screen.currentResolution.height : Screen.height;

            width = Mathf.Max(1, width);
            height = Mathf.Max(1, height);

            int x = Mathf.Max(0, (screenWidth - width) / 2);
            int y = Mathf.Max(0, (screenHeight - height) / 2);

            return new RectInt(x, y, width, height);
        }

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        private static IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
        {
            if (IntPtr.Size == 8)
            {
                return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
            }

            return new IntPtr(SetWindowLong32(hWnd, nIndex, dwNewLong.ToInt32()));
        }
#endif
    }
}

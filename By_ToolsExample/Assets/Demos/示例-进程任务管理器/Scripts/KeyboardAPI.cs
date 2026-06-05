namespace Demos.示例_进程任务管理器.Scripts
{
    using System;
    using System.Runtime.InteropServices;

    internal static class KeyboardAPI
    {
        public const int WH_KEYBOARD_LL = 13;

        public delegate IntPtr HookProcessAction(int nCode, IntPtr wParam, IntPtr lParam);

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, HookProcessAction lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hHook, int code, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnhookWindowsHookEx(IntPtr hHook);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
#endif

        public static IntPtr WinSetWindowsHookEx(int idHook, HookProcessAction lpfn, IntPtr hMod, uint dwThreadId)
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            return SetWindowsHookEx(idHook, lpfn, hMod, dwThreadId);
#else
            return IntPtr.Zero;
#endif
        }

        public static IntPtr WinCallNextHookEx(IntPtr hHook, int code, IntPtr wParam, IntPtr lParam)
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            return CallNextHookEx(hHook, code, wParam, lParam);
#else
            return IntPtr.Zero;
#endif
        }

        public static bool WinUnhookWindowsHookEx(IntPtr hHook)
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            return UnhookWindowsHookEx(hHook);
#else
            return true;
#endif
        }

        public static IntPtr WinGetModuleHandle(string moduleName)
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            return GetModuleHandle(moduleName);
#else
            return IntPtr.Zero;
#endif
        }
    }
}

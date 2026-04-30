namespace Demos.示例_快捷键切换应用.Scripts.KeyPadAPI
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using Debug = UnityEngine.Debug;

    internal sealed class GlobalKeyboardHookEventArgs : HandledEventArgs
    {
        public KeyboardState KeyboardState { get; private set; }
        public LowLevelKeyboardInputEvent KeyboardData { get; private set; }

        public GlobalKeyboardHookEventArgs(KeyboardState state, LowLevelKeyboardInputEvent data)
        {
            KeyboardState = state;
            KeyboardData = data;
        }
    }

    internal sealed class KeyboardHook : IDisposable
    {
        public event EventHandler<GlobalKeyboardHookEventArgs> KeyboardPressed;

        private IntPtr _windowsHookHandle;
        private KeyboardAPI.HookProcessAction _hookProcessAction;
        private bool _disposed;

        public KeyboardHook()
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            _hookProcessAction = LowLevelKeyboardProc;
            _windowsHookHandle = KeyboardAPI.WinSetWindowsHookEx(
                KeyboardAPI.WH_KEYBOARD_LL,
                _hookProcessAction,
                GetCurrentModuleHandle(),
                0);

            if (_windowsHookHandle == IntPtr.Zero)
            {
                int errorCode = Marshal.GetLastWin32Error();
                string errorMessage = new Win32Exception(errorCode).Message;
                throw new Win32Exception(errorCode, "安装全局键盘钩子失败：" + errorMessage);
            }
#else
            throw new PlatformNotSupportedException("全局键盘钩子仅支持 Windows 平台。");
#endif
        }

        ~KeyboardHook()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private static IntPtr GetCurrentModuleHandle()
        {
            try
            {
                using (Process process = Process.GetCurrentProcess())
                {
                    string moduleName = process.MainModule != null ? process.MainModule.ModuleName : null;
                    return string.IsNullOrEmpty(moduleName)
                        ? IntPtr.Zero
                        : KeyboardAPI.WinGetModuleHandle(moduleName);
                }
            }
            catch
            {
                return IntPtr.Zero;
            }
        }

        private IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            bool handled = false;

            if (nCode >= 0)
            {
                int message = wParam.ToInt32();
                if (Enum.IsDefined(typeof(KeyboardState), message))
                {
                    var data = (LowLevelKeyboardInputEvent)Marshal.PtrToStructure(lParam, typeof(LowLevelKeyboardInputEvent));
                    var args = new GlobalKeyboardHookEventArgs((KeyboardState)message, data);

                    if (KeyboardPressed != null)
                    {
                        KeyboardPressed(this, args);
                    }

                    handled = args.Handled;
                }
            }

            return handled
                ? (IntPtr)1
                : KeyboardAPI.WinCallNextHookEx(_windowsHookHandle, nCode, wParam, lParam);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            if (_windowsHookHandle != IntPtr.Zero)
            {
                bool success = KeyboardAPI.WinUnhookWindowsHookEx(_windowsHookHandle);
                if (!success && disposing)
                {
                    int errorCode = Marshal.GetLastWin32Error();
                    Debug.LogWarning("[KeyboardHook] 移除键盘钩子失败：" + errorCode + " - " + new Win32Exception(errorCode).Message);
                }

                _windowsHookHandle = IntPtr.Zero;
            }

            if (disposing)
            {
                KeyboardPressed = null;
                _hookProcessAction = null;
            }
        }
    }
}

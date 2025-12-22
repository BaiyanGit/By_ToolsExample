namespace KeyPad
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.InteropServices;

    /// <summary>
    /// 键盘事件
    /// </summary>
    internal class GlobalKeyboardHookEventArgs : HandledEventArgs
    {
        /// <summary>
        /// 来自键盘的按键状态
        /// </summary>
        public KeyboardState KeyboardState { get; private set; }

        /// <summary>
        /// 来自键盘的原始输入数据
        /// </summary>
        public LowLevelKeyboardInputEvent KeyboardData { get; private set; }

        /// <summary>
        /// 初始化状态
        /// </summary>
        /// <param name="state"></param>
        /// <param name="data"></param>
        public GlobalKeyboardHookEventArgs(KeyboardState state, LowLevelKeyboardInputEvent data)
        {
            KeyboardState = state;
            KeyboardData = data;
        }
    }

    internal class KeyboardHook : IDisposable
    {
        public event EventHandler<GlobalKeyboardHookEventArgs> KeyboardPressed;
        private IntPtr _windowsHookHandle;
        private IntPtr _user32LibraryHandle;
        private KeyboardAPI.HookProcessAction _hookProcessAction;

        /// <summary>
        /// 底层键盘钩子，可以捕获全部的系统按键
        /// </summary>
        private const int WH_KEYBOARD_LL = 13;

        /// <summary>
        /// 析构函数
        /// </summary>
        ~KeyboardHook()
        {
            Dispose(false);
        }

        /// <summary>
        /// 释放GC
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public KeyboardHook()
        {
            _windowsHookHandle = IntPtr.Zero;
            _user32LibraryHandle = IntPtr.Zero;

            // 必须保持_hookProcessAction的有效性，因为GC不知道SetWindowsHookEx的行为。
            _hookProcessAction = LowLevelKeyboardProc;

            // 加载User32.dll
            _user32LibraryHandle = KeyboardAPI.WinLoadLibrary("User32");

            if (_user32LibraryHandle == IntPtr.Zero)
            {
                var errorCode = Marshal.GetLastWin32Error();
                var errorStr = $"Failed to load library 'User32.dll'. Error {errorCode}";
                var errorMsg = new Win32Exception(Marshal.GetLastWin32Error()).Message;
                throw new Win32Exception(errorCode, $"{errorStr}: {errorMsg}.");
            }

            _windowsHookHandle =
                KeyboardAPI.WinSetWindowsHookEx(WH_KEYBOARD_LL, _hookProcessAction, _user32LibraryHandle, 0);

            if (_windowsHookHandle == IntPtr.Zero)
            {
                var errorCode = Marshal.GetLastWin32Error();
                var errorStr = $"Failed to adjust keyboard hooks for '{Process.GetCurrentProcess().ProcessName}'.";
                var errorMsg = $" Error {errorCode}: {new Win32Exception(Marshal.GetLastWin32Error()).Message}.";
                throw new Win32Exception(errorCode, $"{errorStr}{errorMsg}");
            }
        }

        public IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            var fEatKeyStroke = false;

            var wParamTyped = wParam.ToInt32();
            if (Enum.IsDefined(typeof(KeyboardState), wParamTyped))
            {
                var o = Marshal.PtrToStructure(lParam, typeof(LowLevelKeyboardInputEvent));
                var p = (LowLevelKeyboardInputEvent)o;

                var eventArguments = new GlobalKeyboardHookEventArgs((KeyboardState)wParamTyped, p);

                KeyboardPressed?.Invoke(this, eventArguments);

                fEatKeyStroke = eventArguments.Handled;
            }

            return fEatKeyStroke ? (IntPtr)1 : KeyboardAPI.WinCallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
        }

        /// <summary>
        /// 释放
        /// </summary>
        /// <param name="disposing"></param>
        /// <exception cref="Win32Exception"></exception>
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                // 因为只能在同一个线程中解除挂钩，而不能在垃圾收集器线程中
                if (_windowsHookHandle != IntPtr.Zero)
                {
                    var isUnhookHook = KeyboardAPI.WinUnhookWindowsHookEx(_windowsHookHandle);
                    if (!isUnhookHook)
                    {
                        var errorCode = Marshal.GetLastWin32Error();
                        var errorStr = $"无法移除的键盘挂钩 '{Process.GetCurrentProcess().ProcessName}'.";
                        var errorMsg = $" 错误 {errorCode}: {new Win32Exception(Marshal.GetLastWin32Error()).Message}.";
                        throw new Win32Exception(errorCode, $"{errorStr}.{errorMsg}");
                    }

                    _windowsHookHandle = IntPtr.Zero;
                    _hookProcessAction -= LowLevelKeyboardProc; // ReSharper 禁用一次 DelegateSubtraction
                }
            }

            if (_user32LibraryHandle != IntPtr.Zero)
            {
                if (!KeyboardAPI.WinFreeLibrary(_user32LibraryHandle)) // 将对库的引用减少1。
                {
                    var errorCode = Marshal.GetLastWin32Error();
                    var errorMsg = $" Error {errorCode}: {new Win32Exception(Marshal.GetLastWin32Error()).Message}.";
                    throw new Win32Exception(errorCode, $"未能卸载“User32.dll”库.{errorMsg}");
                }

                _user32LibraryHandle = IntPtr.Zero;
            }
        }
    }
}
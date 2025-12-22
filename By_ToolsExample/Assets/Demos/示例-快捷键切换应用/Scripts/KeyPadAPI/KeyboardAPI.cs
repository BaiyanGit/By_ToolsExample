using UnityEngine;

namespace KeyPad
{
    using System;
    using System.Runtime.InteropServices;

    /// <summary>
    /// 键盘API
    /// </summary>
    public class KeyboardAPI : MonoBehaviour
    {
        [DllImport("kernel32.dll")]
        private static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern bool FreeLibrary(IntPtr hModule);

        [DllImport("USER32", SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, HookProcessAction lpfn, IntPtr hMod, int dwThreadId);

        [DllImport("USER32", SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hHook, int code, IntPtr wParam, IntPtr lParam);

        [DllImport("USER32", SetLastError = true)]
        private static extern bool UnhookWindowsHookEx(IntPtr hHook);

        /// <summary>
        /// 加载Dll库
        /// </summary>
        /// <param name="lpFileName">Dll库文件名</param>
        /// <returns>返回句柄</returns>
        public static IntPtr WinLoadLibrary(string lpFileName)
        {
            return LoadLibrary(lpFileName);
        }

        /// <summary>
        /// 释放Dll库
        /// </summary>
        /// <param name="hModule"></param>
        /// <returns></returns>
        public static bool WinFreeLibrary(IntPtr hModule)
        {
            return FreeLibrary(hModule);
        }

        public delegate IntPtr HookProcessAction(int nCode, IntPtr wParam, IntPtr lParam);

        /// <summary>
        /// SetWindowsHookEx函数将应用程序定义的钩子过程安装到钩子链中。
        /// 您将安装一个钩子过程来监视系统中某些类型的事件。
        /// 这些事件要么与特定线程相关联，要么与调用线程所在的同一桌面中的所有线程相关联。
        /// </summary>
        /// <param name="idHook">钩型</param>
        /// <param name="lpfn">挂钩程序</param>
        /// <param name="hMod">应用程序实例的句柄</param>
        /// <param name="dwThreadId">线程标识符</param>
        /// <returns>如果函数成功，那么返回值就是钩子过程的句柄。</returns>
        public static IntPtr WinSetWindowsHookEx(int idHook, HookProcessAction lpfn, IntPtr hMod, int dwThreadId)
        {
            return SetWindowsHookEx(idHook, lpfn, hMod, dwThreadId);
        }

        /// <summary>
        /// CallNextHookEx函数将钩子信息传递给当前钩子链中的下一个钩子过程。钩子过程可以在处理钩子信息之前或之后调用此函数。
        /// </summary>
        /// <param name="hHook">当前挂钩的句柄</param>
        /// <param name="code">传递给hook过程的hook代码</param>
        /// <param name="wParam">传递到挂钩过程的值</param>
        /// <param name="lParam">传递到挂钩过程的值</param>
        /// <returns>如果函数成功，则返回值为true。</returns>
        public static IntPtr WinCallNextHookEx(IntPtr hHook, int code, IntPtr wParam, IntPtr lParam)
        {
            return CallNextHookEx(hHook, code, wParam, lParam);
        }

        /// <summary>
        /// UnhookWindowsHookEx函数删除由SetWindowsHookEx函数安装在挂钩链中的挂钩过程。
        /// </summary>
        /// <param name="hHook">手柄到挂钩程序</param>
        /// <returns>如果函数成功，则返回值为true。</returns>
        public static bool WinUnhookWindowsHookEx(IntPtr hHook)
        {
            return UnhookWindowsHookEx(hHook);
        }
    }
}
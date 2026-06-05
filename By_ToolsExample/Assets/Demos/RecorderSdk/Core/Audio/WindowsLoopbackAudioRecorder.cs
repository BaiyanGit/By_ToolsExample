//=====================================================
// 文件名称: WindowsLoopbackAudioRecorder
// 描    述: Unity 侧对原生 WASAPI Loopback 录音插件的封装。
//           将 DLL 放到 Assets/Plugins/x86_64/WASAPILoopbackRecorder.dll
//=====================================================

namespace Demos.RecorderSdk.Core.Audio
{
    using System;
    using System.Runtime.InteropServices;
    using UnityEngine;

    public static class WindowsLoopbackAudioRecorder
    {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        private const string DLL_NAME = "WASAPILoopbackRecorder";

        [DllImport(DLL_NAME, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool StartLoopbackRecord(string wavPath);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool StopLoopbackRecord();

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool IsLoopbackRecording();

        [DllImport(DLL_NAME, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        private static extern void GetLoopbackLastError(System.Text.StringBuilder buffer, int bufferLen);
#endif

        public static bool IsRecording
        {
            get
            {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
                try
                {
                    return IsLoopbackRecording();
                }
                catch (Exception e)
                {
                    Debug.LogError("调用 IsLoopbackRecording 失败: " + e.Message);
                    return false;
                }
#else
            return false;
#endif
            }
        }

        public static bool StartRecording(string wavPath, out string error)
        {
            error = string.Empty;

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            try
            {
                bool ok = StartLoopbackRecord(wavPath);
                if (!ok)
                {
                    error = GetLastErrorText();
                }

                return ok;
            }
            catch (Exception e)
            {
                error = "调用原生 DLL 失败: " + e.Message;
                return false;
            }
#else
        error = "当前平台不是 Windows。";
        return false;
#endif
        }

        public static bool StopRecording(out string error)
        {
            error = string.Empty;

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            try
            {
                bool ok = StopLoopbackRecord();
                if (!ok)
                {
                    error = GetLastErrorText();
                }

                return ok;
            }
            catch (Exception e)
            {
                error = "调用原生 DLL 失败: " + e.Message;
                return false;
            }
#else
        error = "当前平台不是 Windows。";
        return false;
#endif
        }

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        private static string GetLastErrorText()
        {
            try
            {
                var sb = new System.Text.StringBuilder(2048);
                GetLoopbackLastError(sb, sb.Capacity);
                string msg = sb.ToString();
                return string.IsNullOrWhiteSpace(msg) ? "未知错误" : msg;
            }
            catch (Exception e)
            {
                return "读取原生错误信息失败: " + e.Message;
            }
        }
#endif
    }
}
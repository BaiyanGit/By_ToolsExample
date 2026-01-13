#if DEVELOP_LOGFILE
namespace ByTools.DebugHelper
{
    using System;
    using System.IO;
    using UnityEngine;

    public class DebugWriteLocal : MonoBehaviour
    {
        private static string _textPath;
        private static int _logCount;

#if !UNITY_EDITOR
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Init()
    {
        // 创建日志保存路径
        var debugPath = Application.persistentDataPath + "/Debug/";
        if (!Directory.Exists(debugPath)) Directory.CreateDirectory(debugPath);

        // 创建日志文件
        var currentTime = DateTime.Now.ToString("yyy-MM-dd");
        _textPath = $"{debugPath}/{currentTime}.txt";
        if (File.Exists(_textPath)) File.Delete(_textPath);

        // 注册日志回调
        Application.logMessageReceivedThreaded += OnHandleLog;

        Debug.Log("日记保存路径：" + debugPath);
    }

    private void OnDestroy()
    {
        Application.logMessageReceivedThreaded -= OnHandleLog;
    }
#endif
        /// <summary>
        /// 日志回调
        /// </summary>
        /// <param name="message"></param>
        /// <param name="stackTrace"></param>
        /// <param name="type"></param>
        private static void OnHandleLog(string message, string stackTrace, LogType type)
        {
            _logCount++;

            // stackTrace = string.IsNullOrEmpty(stackTrace) ? "" : $"堆栈信息\n{stackTrace}";

            var formattedMessage = $"==[{type}_{_logCount}]==\n[{DateTime.Now:T}] {message}\n{stackTrace}";
            WriteText(formattedMessage);
        }

        /// <summary>
        /// 将日志写入文件
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="message"></param>
        private static void WriteText(string message)
        {
            if (string.IsNullOrEmpty(_textPath) || string.IsNullOrEmpty(message)) return;

            StreamWriter sw = null;
            sw = !File.Exists(_textPath) ? File.CreateText(_textPath) : File.AppendText(_textPath);
            sw.WriteLine(message + '\n');
            sw.Close();
            sw.Dispose();
        }
    }
}

#endif
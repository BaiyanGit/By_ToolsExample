/*
 * 作者: <王柏雁>
 * 日期: 2024/12/18
 * 功能: 日志窗口
 * 使用方法:
 * 1. 脚本挂载到任意 GameObject 上即可。
 * 2. 打开窗口显示 Log 信息. => 按 F2 打开/关闭窗口.
 */

#if DEVELOP_DEBUG
namespace ByTools.DebugHelper
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    public class DebugLogWindow : MonoBehaviour
    {
        public bool isShowWindow;
        private Vector2 _scrollPosition;
        private readonly List<LogEntry> _logs = new();
        private readonly List<LogEntry> _filteredLogs = new();
        private readonly Dictionary<string, (int Count, DateTime LastTime)> _collapsedLogCounts = new();

        private bool _showLog = true;
        private bool _showWarning = true;
        private bool _showError = true;
        private bool _showException = true;
        private bool _collapse;
        private bool _showStackTrace; // 是否显示调用堆栈

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            var go = new GameObject("DebugLogWindow");
            go.AddComponent<DebugLogWindow>();
            Debug.Log("DebugLogWindow is enabled.");
        }

        private void Awake()
        {
            DontDestroyOnLoad(this);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F2))
            {
                isShowWindow = !isShowWindow;
                FilterLogs();
            }
        }

        private void OnEnable()
        {
            Application.logMessageReceived += HandleLog;
        }

        private void OnDisable()
        {
            Application.logMessageReceived -= HandleLog;
        }

        private void HandleLog(string logString, string stackTrace, LogType type)
        {
            _logs.Add(new LogEntry(logString, stackTrace, type, DateTime.Now));
            FilterLogs();
        }

        private void FilterLogs()
        {
            _filteredLogs.Clear();
            _collapsedLogCounts.Clear();

            foreach (var log in _logs)
            {
                if ((_showLog && log.type == LogType.Log) ||
                    (_showWarning && log.type == LogType.Warning) ||
                    (_showError && log.type == LogType.Error) ||
                    (_showException && log.type == LogType.Exception))
                {
                    if (_collapse)
                    {
                        if (_collapsedLogCounts.ContainsKey(log.message))
                        {
                            var current = _collapsedLogCounts[log.message];
                            _collapsedLogCounts[log.message] = (current.Count + 1, log.timestamp);
                            continue;
                        }

                        _collapsedLogCounts[log.message] = (1, log.timestamp);
                    }

                    _filteredLogs.Add(log);
                }
            }
        }

        private void OnGUI()
        {
            if (!isShowWindow) return;

            GUILayout.BeginHorizontal();

            var prevCollapse = _collapse;
            _collapse = GUILayout.Toggle(_collapse, "Collapse", GUILayout.Width(70));

            var prevShowLog = _showLog;
            _showLog = GUILayout.Toggle(_showLog, "Log", GUILayout.Width(50));

            var prevShowWarning = _showWarning;
            _showWarning = GUILayout.Toggle(_showWarning, "Warning", GUILayout.Width(70));

            var prevShowError = _showError;
            _showError = GUILayout.Toggle(_showError, "Error", GUILayout.Width(70));

            var prevShowException = _showException;
            _showException = GUILayout.Toggle(_showException, "Exception", GUILayout.Width(90));

            var prevShowStackTrace = _showStackTrace;
            _showStackTrace = GUILayout.Toggle(_showStackTrace, "Show StackTrace", GUILayout.Width(120));

            if (GUILayout.Button("Clear", GUILayout.Width(70)))
            {
                _logs.Clear();
                FilterLogs();
            }

            GUILayout.EndHorizontal();

            if (prevCollapse != _collapse || prevShowLog != _showLog || prevShowWarning != _showWarning ||
                prevShowError != _showError || prevShowException != _showException ||
                prevShowStackTrace != _showStackTrace)
            {
                FilterLogs();
            }

            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);

            if (_collapse)
            {
                foreach (var entry in _collapsedLogCounts)
                {
                    var msg = $"[{entry.Value.LastTime:HH:mm:ss}] {entry.Key} (Count: {entry.Value.Count})";
                    DrawLogEntryWithBackground(msg, LogType.Log);
                }
            }
            else
            {
                foreach (var log in _filteredLogs)
                {
                    var logContent = $"[{log.timestamp:HH:mm:ss}] {log.message}";
                    if (_showStackTrace)
                    {
                        logContent += $"\n{log.stackTrace}";
                    }

                    DrawLogEntryWithBackground(logContent, log.type);
                }
            }

            GUILayout.EndScrollView();
        }

        private static void DrawLogEntryWithBackground(string content, LogType type)
        {
            var style = GetLogStyle(type);
            var rect = GUILayoutUtility.GetRect(new GUIContent(content), style);
            var originalColor = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 1f); // Gray background
            GUI.Box(rect, GUIContent.none);
            GUI.backgroundColor = originalColor;
            GUI.Label(rect, content, style);
        }

        private static GUIStyle GetLogStyle(LogType type)
        {
            var style = new GUIStyle(GUI.skin.label)
            {
                normal =
                {
                    textColor = type switch
                    {
                        LogType.Warning                    => Color.yellow,
                        LogType.Error or LogType.Exception => Color.red,
                        _                                  => Color.white
                    }
                },
                hover =
                {
                    textColor = Color.gray
                }
            };

            return style;
        }

        private class LogEntry
        {
            public string message { get; }
            public string stackTrace { get; }
            public LogType type { get; }
            public DateTime timestamp { get; }

            public LogEntry(string message, string stackTrace, LogType type, DateTime timestamp)
            {
                this.message    = message;
                this.stackTrace = stackTrace;
                this.type       = type;
                this.timestamp  = timestamp;
            }
        }
    }
}
#endif
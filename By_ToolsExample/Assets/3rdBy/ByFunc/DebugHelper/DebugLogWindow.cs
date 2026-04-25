/*
 * 作者: <王柏雁>
 * 日期: 2024/12/18
 * 最后重构: 2026/04/23
 * 功能: 运行时日志窗口（增强版）
 *
 * 主要特性:
 * 1. F2 打开/关闭日志窗口。
 * 2. 自动收集 Log / Warning / Error / Assert / Exception。
 * 3. 使用 Application.logMessageReceivedThreaded，兼容主线程与非主线程日志。
 * 4. 支持折叠、搜索、显示堆栈、自动滚动、暂停收集。
 * 5. 支持日志数量上限，避免长时间运行导致内存持续增长。
 * 6. 支持将“当前视图”的日志导出为 TXT 文件。
 * 7. 支持单条日志点击复制内容、单条复制堆栈。
 * 8. 支持清空日志二次确认。
 * 9. 支持拖拽窗口与右下角缩放窗口。
 * 10. 支持自动创建，也兼容手动挂载；内部做了重复实例保护。
 *
 * 使用方法:
 * 1. 在 Scripting Define Symbols 中添加 DEVELOP_DEBUG。
 * 2. 默认会在运行时自动创建对象，无需手动挂载。
 * 3. 如果更想手动挂载，也可以直接挂到任意 GameObject 上，脚本会自动避免重复实例。
 * 4. 运行后按 F2 打开/关闭窗口。
 */

#if DEVELOP_DEBUG
namespace _3rdBy.ByFunc.DebugHelper
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    /// <summary>
    /// 运行时日志窗口。
    /// </summary>
    public class DebugLogWindow : MonoBehaviour
    {
        #region Static

        private static DebugLogWindow _instance;

        /// <summary>
        /// 运行时自动初始化。
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            if (_instance != null)
            {
                return;
            }

            var go = new GameObject("DebugLogWindow");
            _instance = go.AddComponent<DebugLogWindow>();
        }

        #endregion

        #region Inspector

        [Header("运行时状态：当前日志窗口是否显示，通常由快捷键控制")] public bool isShowWindow;

        [Header("快捷开关：按下此按键可打开或关闭日志窗口")] [SerializeField]
        private KeyCode _toggleKey = KeyCode.F2;

        [Header("启动行为：是否在游戏启动后默认显示日志窗口")] [SerializeField]
        private bool _startVisible;

        [Header("日志容量：缓存的最大日志数量，超出后会移除最旧日志")] [SerializeField]
        private int _maxLogCount = 2000;

        [Header("窗口标题：显示在日志窗口顶部的标题文字")] [SerializeField]
        private string _windowTitle = "Debug Log Console";

        [Header("默认筛选：普通日志是否默认显示")] [SerializeField]
        private bool _defaultShowLog = true;

        [Header("默认筛选：警告日志是否默认显示")] [SerializeField]
        private bool _defaultShowWarning = true;

        [Header("默认筛选：错误日志是否默认显示")] [SerializeField]
        private bool _defaultShowError = true;

        [Header("默认筛选：断言日志是否默认显示")] [SerializeField]
        private bool _defaultShowAssert = true;

        [Header("默认筛选：异常日志是否默认显示")] [SerializeField]
        private bool _defaultShowException = true;

        [Header("默认显示：启动时是否直接显示堆栈信息")] [SerializeField]
        private bool _defaultShowStackTrace;

        [Header("默认显示：启动时是否启用折叠相同日志")] [SerializeField]
        private bool _defaultCollapse;

        [Header("默认显示：启动时是否自动滚动到最新日志")] [SerializeField]
        private bool _defaultAutoScroll = true;

        [Header("默认排序：是否默认让最新日志显示在最上方")] [SerializeField]
        private bool _defaultNewestFirst = true;

        [Header("窗口尺寸：日志窗口允许的最小尺寸")] [SerializeField]
        private Vector2 _minWindowSize = new(760f, 420f);

        [Header("窗口尺寸：日志窗口首次打开时的默认尺寸")] [SerializeField]
        private Vector2 _defaultWindowSize = new(980f, 620f);

        [Header("界面显示：窗口整体字号缩放，可在运行时继续调节")] [SerializeField, Range(0.85f, 2.00f)]
        private float _uiScale = 1.10f;

        [Header("界面显示：窗口背景透明度，可在运行时继续调节")] [SerializeField, Range(0.65f, 1.00f)]
        private float _windowOpacity = 0.96f;

        [Header("界面显示：顶部标题栏高度")] [SerializeField]
        private float _headerBarHeight = 34f;

        [Header("界面显示：单条日志内容区域的最小高度")] [SerializeField]
        private float _entryMinHeight = 35f;

        #endregion

        #region Runtime Data

        [Header("窗口矩形区域")] private Rect _windowRect;
        [Header("滚动位置")] private Vector2 _scrollPosition;
        [Header("窗口矩形区域是否已初始化")] private bool _windowRectInitialized;
        [Header("是否请求滚动到底部")] private bool _requestScrollToBottom;
        [Header("窗口是否需要刷新")] private bool _isDirty = true;
        [Header("是否显示清除确认提示")] private bool _showClearConfirm;

        [Header("日志条目列表")] private readonly List<LogEntry> _logs = new(256);
        [Header("过滤后的日志条目列表")] private readonly List<LogEntry> _filteredLogs = new(256);
        [Header("折叠的日志条目列表")] private readonly List<CollapsedLogEntry> _collapsedLogs = new(128);

        [Header("等待添加的日志锁定对象")] private readonly object _pendingLogsLock = new();
        [Header("等待添加的日志队列")] private readonly Queue<PendingLog> _pendingLogs = new(128);

        [Header("是否显示日志")] private bool _showLog;
        [Header("是否显示警告")] private bool _showWarning;
        [Header("是否显示错误")] private bool _showError;
        [Header("是否显示断言")] private bool _showAssert;
        [Header("是否显示异常")] private bool _showException;
        [Header("是否显示堆栈跟踪")] private bool _showStackTrace;
        [Header("是否折叠显示")] private bool _collapse;
        [Header("是否自动滚动到底部")] private bool _autoScroll;
        [Header("是否暂停收集日志")] private bool _pauseCollection;
        [Header("是否按最新日志排序")] private bool _sortNewestFirst;

        [Header("展开的堆栈键集合")] private readonly HashSet<string> _expandedStackKeys = new();

        [Header("搜索关键词")] private string _searchKeyword = string.Empty;
        [Header("上次保存路径")] private string _lastSavePath = string.Empty;
        [Header("状态消息")] private string _statusMessage = string.Empty;
        [Header("状态消息显示截止时间")] private float _statusMessageUntil;

        [Header("日志数量")] private int _logCount;
        [Header("警告数量")] private int _warningCount;
        [Header("错误数量")] private int _errorCount;
        [Header("断言数量")] private int _assertCount;
        [Header("异常数量")] private int _exceptionCount;

        [Header("是否正在拖动窗口")] private bool _isDraggingWindow;
        [Header("拖动开始时鼠标在屏幕上的位置")] private Vector2 _dragStartMouseScreenPosition;
        [Header("拖动开始时窗口的位置")] private Vector2 _dragStartWindowPosition;

        [Header("是否正在调整窗口大小")] private bool _isResizing;
        [Header("调整大小开始时鼠标在屏幕上的位置")] private Vector2 _resizeStartMouseScreenPosition;
        [Header("调整大小开始时窗口的大小")] private Vector2 _resizeStartWindowSize;

        [Header("是否正在拖动日志列表")] private bool _isDraggingLogList;
        [Header("拖动日志列表开始时鼠标在屏幕上的位置")] private Vector2 _logListDragStartMouseScreenPosition;
        [Header("拖动日志列表开始时的滚动位置")] private Vector2 _logListDragStartScrollPosition;

        [Header("缓存的UI缩放比例")] private float _cachedUiScale = -1f;

        #endregion

        #region GUI Styles

        [Header("标题样式")] private static GUIStyle _titleStyle;
        [Header("工具栏按钮样式")] private static GUIStyle _toolbarButtonStyle;
        [Header("工具栏危险按钮样式")] private static GUIStyle _toolbarDangerButtonStyle;
        [Header("工具栏切换按钮样式")] private static GUIStyle _toolbarToggleStyle;
        [Header("搜索文本框样式")] private static GUIStyle _searchTextFieldStyle;
        [Header("日志条目框样式")] private static GUIStyle _entryBoxStyle;
        [Header("日志条目头部样式")] private static GUIStyle _entryHeaderStyle;
        [Header("日志条目文本按钮样式")] private static GUIStyle _entryTextButtonStyle;
        [Header("日志条目日志文本样式")] private static GUIStyle _entryLogTextStyle;
        [Header("日志条目警告文本样式")] private static GUIStyle _entryWarningTextStyle;
        [Header("日志条目错误文本样式")] private static GUIStyle _entryErrorTextStyle;
        [Header("日志条目断言文本样式")] private static GUIStyle _entryAssertTextStyle;
        [Header("日志条目异常文本样式")] private static GUIStyle _entryExceptionTextStyle;
        [Header("堆栈跟踪样式")] private static GUIStyle _stackTraceStyle;
        [Header("元数据文本样式")] private static GUIStyle _metaTextStyle;
        [Header("状态文本样式")] private static GUIStyle _statusStyle;
        [Header("调整大小句柄样式")] private static GUIStyle _resizeHandleStyle;
        [Header("日志条目拖动句柄样式")] private static GUIStyle _entryDragHandleStyle;
        [Header("关闭按钮样式")] private static GUIStyle _closeButtonStyle;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            isShowWindow     = _startVisible;
            _showLog         = _defaultShowLog;
            _showWarning     = _defaultShowWarning;
            _showError       = _defaultShowError;
            _showAssert      = _defaultShowAssert;
            _showException   = _defaultShowException;
            _showStackTrace  = _defaultShowStackTrace;
            _collapse        = _defaultCollapse;
            _autoScroll      = _defaultAutoScroll;
            _sortNewestFirst = _defaultNewestFirst;

            _maxLogCount     = Mathf.Max(100, _maxLogCount);
            _minWindowSize.x = Mathf.Max(420f, _minWindowSize.x);
            _minWindowSize.y = Mathf.Max(260f, _minWindowSize.y);
            _uiScale         = Mathf.Clamp(_uiScale, 0.85f, 2.0f);
            _windowOpacity   = Mathf.Clamp(_windowOpacity, 0.65f, 1.0f);
            _headerBarHeight = Mathf.Max(30f, _headerBarHeight);
            _entryMinHeight  = Mathf.Max(36f, _entryMinHeight);
        }

        private void OnEnable()
        {
            Application.logMessageReceivedThreaded += HandleLogThreaded;
        }

        private void OnDisable()
        {
            Application.logMessageReceivedThreaded -= HandleLogThreaded;
        }

        private void Update()
        {
            FlushPendingLogs();
            UpdateGlobalWindowInteraction();

            if (Input.GetKeyDown(_toggleKey))
            {
                isShowWindow = !isShowWindow;
            }

            if (!string.IsNullOrEmpty(_statusMessage) && Time.unscaledTime > _statusMessageUntil)
            {
                _statusMessage = string.Empty;
            }
        }

        private void OnGUI()
        {
            if (!isShowWindow)
            {
                return;
            }

            EnsureWindowRect();
            EnsureStyles();
            EnsureFilteredCache();

            _windowRect = GUILayout.Window(GetInstanceID(), _windowRect, DrawWindow, string.Empty);
            ClampWindowRect();
            DrawWindowOverlayControls();

            if (_requestScrollToBottom)
            {
                _scrollPosition.y      = float.MaxValue;
                _requestScrollToBottom = false;
            }
        }

        #endregion

        #region Log Collect

        /// <summary>
        /// 线程安全地接收 Unity 日志。
        /// </summary>
        private void HandleLogThreaded(string logString, string stackTrace, LogType type)
        {
            if (_pauseCollection)
            {
                return;
            }

            lock (_pendingLogsLock)
            {
                _pendingLogs.Enqueue(new PendingLog(logString, stackTrace, type, DateTime.Now));
            }
        }

        /// <summary>
        /// 将线程队列中的日志合并回主线程缓存。
        /// </summary>
        private void FlushPendingLogs()
        {
            bool addedAny = false;

            lock (_pendingLogsLock)
            {
                while (_pendingLogs.Count > 0)
                {
                    var pending = _pendingLogs.Dequeue();
                    _logs.Add(new LogEntry(pending.message, pending.stackTrace, pending.type, pending.time));
                    addedAny = true;
                }
            }

            if (!addedAny)
            {
                return;
            }

            TrimExcessLogs();
            _isDirty = true;

            if (_autoScroll)
            {
                _requestScrollToBottom = true;
            }
        }

        /// <summary>
        /// 限制日志总数，避免长时间运行导致数据无限膨胀。
        /// </summary>
        private void TrimExcessLogs()
        {
            if (_logs.Count <= _maxLogCount)
            {
                return;
            }

            int removeCount = _logs.Count - _maxLogCount;
            _logs.RemoveRange(0, removeCount);
        }

        #endregion

        #region GUI Draw

        /// <summary>
        /// 绘制窗口主体。
        /// </summary>
        private void DrawWindow(int windowId)
        {
            DrawWindowBackground();
            DrawHeader();
            DrawStatusBar();
            DrawToolbar();
            DrawFilterBar();
            DrawSearchBar();
            DrawDisplayBar();
            DrawLogList();
        }

        /// <summary>
        /// 绘制窗口纯色背景，降低场景内容对日志可读性的影响。
        /// </summary>
        private void DrawWindowBackground()
        {
            var originalColor = GUI.color;
            GUI.color = new Color(0.07f, 0.08f, 0.10f, _windowOpacity);
            GUI.DrawTexture(new Rect(0f, 0f, _windowRect.width, _windowRect.height), Texture2D.whiteTexture, ScaleMode.StretchToFill);
            GUI.color = originalColor;
        }

        /// <summary>
        /// 绘制标题栏，同时支持拖拽窗口与右上角关闭按钮。
        /// </summary>
        private void DrawHeader()
        {
            float headerHeight = Scale(_headerBarHeight);
            var   headerRect   = GUILayoutUtility.GetRect(0f, headerHeight, GUILayout.ExpandWidth(true));

            var originalColor = GUI.color;
            GUI.color = new Color(0.16f, 0.18f, 0.22f, Mathf.Clamp01(_windowOpacity + 0.02f));
            GUI.DrawTexture(headerRect, Texture2D.whiteTexture, ScaleMode.StretchToFill);
            GUI.color = originalColor;

            var titleRect = new Rect(headerRect.x + Scale(12f), headerRect.y + Scale(5f), Mathf.Max(120f, headerRect.width * 0.45f), headerRect.height - Scale(10f));
            var infoRect  = new Rect(headerRect.x + headerRect.width * 0.44f, headerRect.y + Scale(6f), headerRect.width * 0.38f, headerRect.height - Scale(12f));
            var closeRect = new Rect(headerRect.xMax - Scale(34f), headerRect.y + Scale(4f), Scale(28f), headerRect.height - Scale(8f));

            GUI.Label(titleRect, _windowTitle, _titleStyle);
            GUI.Label(infoRect, $"显示: {GetVisibleCount()} / 缓存: {_logs.Count}", _metaTextStyle);

            if (GUI.Button(closeRect, "✕", _closeButtonStyle))
            {
                isShowWindow      = false;
                _showClearConfirm = false;
                StopWindowInteraction();
                ShowStatus("已关闭日志窗口。按快捷键可再次打开。");
            }

            TryBeginWindowDrag(headerRect, closeRect);
            GUILayout.Space(Scale(2f));
        }

        /// <summary>
        /// 绘制功能工具栏。
        /// </summary>
        private void DrawToolbar()
        {
            GUILayout.BeginHorizontal();
            bool newAutoScroll = GUILayout.Toggle(_autoScroll, _autoScroll ? "🟢自动滚动中" : "🔴自动滚动", _toolbarToggleStyle, GUILayout.Height(Scale(30f)));
            if (!newAutoScroll.Equals(_autoScroll))
            {
                _autoScroll = newAutoScroll;
            }

            bool newPauseCollection = GUILayout.Toggle(_pauseCollection, _pauseCollection ? "🟢继续收集" : "🔴暂停收集", _toolbarToggleStyle, GUILayout.Height(Scale(30f)));
            if (newPauseCollection != _pauseCollection)
            {
                _pauseCollection = newPauseCollection;
                ShowStatus(_pauseCollection ? "已暂停日志收集。" : "已继续日志收集。");
            }

            if (GUILayout.Button("📠导出当前日志", _toolbarButtonStyle, GUILayout.Height(Scale(30f))))
            {
                SaveLogsToTxt();
            }

            if (GUILayout.Button("📂打开目录", _toolbarButtonStyle, GUILayout.Height(Scale(30f))))
            {
                OpenSaveDirectory();
            }

            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// 绘制等级过滤栏。
        /// </summary>
        private void DrawFilterBar()
        {
            GUILayout.BeginHorizontal();
            bool newCollapse = GUILayout.Toggle(_collapse, _collapse ? "▶️已折叠" : "🔽折叠", _toolbarToggleStyle, GUILayout.Height(Scale(30f)));
            if (newCollapse != _collapse)
            {
                _collapse = newCollapse;
                _isDirty  = true;
            }

            DrawFilterToggle(ref _showLog, $"{GetTypeIcon(LogType.Log)} Log ({_logCount})");
            DrawFilterToggle(ref _showWarning, $"{GetTypeIcon(LogType.Warning)} Warning ({_warningCount})");
            DrawFilterToggle(ref _showError, $"{GetTypeIcon(LogType.Error)} Error ({_errorCount})");
            DrawFilterToggle(ref _showAssert, $"{GetTypeIcon(LogType.Assert)} Assert ({_assertCount})");
            DrawFilterToggle(ref _showException, $"{GetTypeIcon(LogType.Exception)} Exception ({_exceptionCount})");

            bool newShowStackTrace = GUILayout.Toggle(_showStackTrace, _showStackTrace ? "🖥️堆栈已显示" : "🔴显示堆栈", _toolbarToggleStyle, GUILayout.Height(Scale(30f)));
            if (!newShowStackTrace.Equals(_showStackTrace))
            {
                _showStackTrace = newShowStackTrace;
            }


            bool newSortNewestFirst = GUILayout.Toggle(_sortNewestFirst, _sortNewestFirst ? "🆕最新在上" : "⏰最早在上", _toolbarToggleStyle, GUILayout.Height(Scale(30f)));
            if (newSortNewestFirst != _sortNewestFirst)
            {
                _sortNewestFirst = newSortNewestFirst;
                _isDirty         = true;
            }

            if (_showClearConfirm)
            {
                GUILayout.Label("💡确认清空?", _metaTextStyle, GUILayout.Width(Scale(78f)));

                if (GUILayout.Button("🆗确认", _toolbarDangerButtonStyle, GUILayout.Width(Scale(72f)), GUILayout.Height(Scale(30f))))
                {
                    _showClearConfirm = false;
                    ClearLogs();
                }

                if (GUILayout.Button("❎取消", _toolbarButtonStyle, GUILayout.Width(Scale(72f)), GUILayout.Height(Scale(30f))))
                {
                    _showClearConfirm = false;
                    ShowStatus("已取消清空日志。");
                }
            }
            else
            {
                if (GUILayout.Button("🧹清空日志", _toolbarButtonStyle, GUILayout.Height(Scale(30f))))
                {
                    _showClearConfirm = true;
                    ShowStatus("再次点击确认后将清空全部日志。");
                }
            }


            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// 绘制搜索栏。
        /// </summary>
        private void DrawSearchBar()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("搜索:", _metaTextStyle, GUILayout.Width(Scale(46f)));

            string newSearchKeyword = GUILayout.TextField(_searchKeyword, _searchTextFieldStyle, GUILayout.Height(Scale(28f)));
            if (!string.Equals(newSearchKeyword, _searchKeyword, StringComparison.Ordinal))
            {
                _searchKeyword = newSearchKeyword;
                _isDirty       = true;
            }

            if (GUILayout.Button("清除搜索", _toolbarButtonStyle, GUILayout.Width(Scale(102f)), GUILayout.Height(Scale(28f))))
            {
                if (!string.IsNullOrEmpty(_searchKeyword))
                {
                    _searchKeyword = string.Empty;
                    _isDirty       = true;
                }
            }

            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// 绘制显示参数栏，支持字号与背景透明度运行时调节。
        /// </summary>
        private void DrawDisplayBar()
        {
            GUILayout.BeginHorizontal(GUI.skin.box);
            GUILayout.Label("字号", _metaTextStyle, GUILayout.Width(Scale(34f)));
            float newUiScale = GUILayout.HorizontalSlider(_uiScale, 0.85f, 2.00f, GUILayout.MinWidth(Scale(120f)));
            GUILayout.Label($"{_uiScale:0.00}x", _metaTextStyle, GUILayout.Width(Scale(54f)));

            GUILayout.Space(Scale(10f));
            GUILayout.Label("透明度", _metaTextStyle, GUILayout.Width(Scale(54f)));
            float newOpacity = GUILayout.HorizontalSlider(_windowOpacity, 0.65f, 1.00f, GUILayout.MinWidth(Scale(120f)));
            GUILayout.Label($"{Mathf.RoundToInt(_windowOpacity * 100f)}%", _metaTextStyle, GUILayout.Width(Scale(48f)));
            GUILayout.FlexibleSpace();

            if (!Mathf.Approximately(newUiScale, _uiScale))
            {
                _uiScale = (float)Math.Round(newUiScale, 2);
                MarkStyleDirty();
            }

            if (!Mathf.Approximately(newOpacity, _windowOpacity))
            {
                _windowOpacity = (float)Math.Round(newOpacity, 2);
            }

            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// 绘制状态栏。
        /// </summary>
        private void DrawStatusBar()
        {
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label(string.IsNullOrEmpty(_statusMessage) ? GetDefaultStatusText() : _statusMessage, _statusStyle);
            GUILayout.EndVertical();
        }

        /// <summary>
        /// 绘制日志列表。
        /// </summary>
        private void DrawLogList()
        {
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, GUILayout.ExpandHeight(true));

            if (_collapse)
            {
                for (int i = 0; i < _collapsedLogs.Count; i++)
                {
                    DrawCollapsedLogEntry(_collapsedLogs[i], i);
                }
            }
            else if (_sortNewestFirst)
            {
                for (int i = _filteredLogs.Count - 1; i >= 0; i--)
                {
                    DrawLogEntry(_filteredLogs[i], _filteredLogs.Count - 1 - i);
                }
            }
            else
            {
                for (int i = 0; i < _filteredLogs.Count; i++)
                {
                    DrawLogEntry(_filteredLogs[i], i);
                }
            }

            GUILayout.EndScrollView();
        }

        /// <summary>
        /// 绘制单条普通日志。
        /// 点击日志正文可直接复制内容。
        /// 左侧手柄支持拖拽滚动日志列表。
        /// </summary>
        private void DrawLogEntry(LogEntry entry, int index)
        {
            string entryKey   = GetEntryDisplayKey(entry);
            bool   isExpanded = _expandedStackKeys.Contains(entryKey);

            var backgroundColor = GetEntryBackgroundColor(entry.type, index);
            var originalColor   = GUI.backgroundColor;
            GUI.backgroundColor = backgroundColor;

            GUILayout.BeginVertical(_entryBoxStyle);
            GUI.backgroundColor = originalColor;

            // 绘制日志头部
            GUILayout.BeginHorizontal();
            var dragHandleRect = GUILayoutUtility.GetRect(new GUIContent(""), _entryDragHandleStyle, GUILayout.Width(Scale(30f)), GUILayout.Height(Scale(24f)));
            GUI.Label(dragHandleRect, GetTypeHeadIcon(entry.type), _entryDragHandleStyle);
            TryBeginLogListDrag(dragHandleRect);

            // 绘制日志时间
            GUILayout.Label($"[{entry.time:HH:mm:ss}] {GetTypeIcon(entry.type)} ( {GetTypeLabel(entry.type)} )", _entryHeaderStyle);
            GUILayout.FlexibleSpace();
            GUILayout.Label(isExpanded ? "已展开" : "双击展开", _metaTextStyle, GUILayout.Width(Scale(78f)));

            if (GUILayout.Button("复制内容", _toolbarButtonStyle, GUILayout.Width(Scale(88f)), GUILayout.Height(Scale(24f))))
            {
                CopyToClipboard(entry.message, "已复制日志内容。");
            }

            GUI.enabled = !string.IsNullOrEmpty(entry.stackTrace);
            if (GUILayout.Button("复制堆栈", _toolbarButtonStyle, GUILayout.Width(Scale(88f)), GUILayout.Height(Scale(24f))))
            {
                CopyToClipboard(entry.stackTrace, "已复制日志堆栈。");
            }

            GUI.enabled = true;
            GUILayout.EndHorizontal();

            // 绘制日志内容
            var messageRect = GUILayoutUtility.GetRect(new GUIContent(entry.message), GetEntryMessageButtonStyle(entry.type), GUILayout.ExpandWidth(true), GUILayout.MinHeight(Scale(_entryMinHeight)));
            GUI.Label(messageRect, entry.message, GetEntryMessageButtonStyle(entry.type));
            HandleEntryInteraction(messageRect, entryKey, entry.message, !string.IsNullOrEmpty(entry.stackTrace));

            // 绘制日志堆栈
            if ((_showStackTrace || isExpanded) && !string.IsNullOrEmpty(entry.stackTrace))
            {
                GUILayout.Space(3f);
                GUILayout.Label(entry.stackTrace, _stackTraceStyle);
            }

            GUILayout.EndVertical();
            GUILayout.Space(2f);
        }

        /// <summary>
        /// 绘制折叠日志。
        /// 点击日志正文可直接复制内容。
        /// 左侧手柄支持拖拽滚动日志列表。
        /// </summary>
        private void DrawCollapsedLogEntry(CollapsedLogEntry entry, int index)
        {
            string entryKey   = GetEntryDisplayKey(entry);
            bool   isExpanded = _expandedStackKeys.Contains(entryKey);

            var backgroundColor = GetEntryBackgroundColor(entry.type, index);
            var originalColor   = GUI.backgroundColor;
            GUI.backgroundColor = backgroundColor;

            GUILayout.BeginVertical(_entryBoxStyle);
            GUI.backgroundColor = originalColor;

            // 绘制日志头部
            GUILayout.BeginHorizontal();
            var dragHandleRect = GUILayoutUtility.GetRect(new GUIContent(""), _entryDragHandleStyle, GUILayout.Width(Scale(30f)), GUILayout.Height(Scale(24f)));
            GUI.Label(dragHandleRect, GetTypeHeadIcon(entry.type), _entryDragHandleStyle);
            TryBeginLogListDrag(dragHandleRect);

            // 绘制日志时间
            GUILayout.Label($"[{entry.lastTime:HH:mm:ss}] {GetTypeIcon(entry.type)} ( {GetTypeLabel(entry.type)} *{entry.count} )", _entryHeaderStyle);
            GUILayout.FlexibleSpace();
            GUILayout.Label(isExpanded ? "已展开" : "双击展开", _metaTextStyle, GUILayout.Width(Scale(78f)));

            if (GUILayout.Button("复制内容", _toolbarButtonStyle, GUILayout.Width(Scale(88f)), GUILayout.Height(Scale(24f))))
            {
                CopyToClipboard(entry.message, "已复制日志内容。");
            }

            GUI.enabled = !string.IsNullOrEmpty(entry.stackTrace);
            if (GUILayout.Button("复制堆栈", _toolbarButtonStyle, GUILayout.Width(Scale(88f)), GUILayout.Height(Scale(24f))))
            {
                CopyToClipboard(entry.stackTrace, "已复制日志堆栈。");
            }

            GUI.enabled = true;
            GUILayout.EndHorizontal();

            // 绘制日志内容
            var messageRect = GUILayoutUtility.GetRect(new GUIContent(entry.message), GetEntryMessageButtonStyle(entry.type), GUILayout.ExpandWidth(true), GUILayout.MinHeight(Scale(_entryMinHeight)));
            GUI.Label(messageRect, entry.message, GetEntryMessageButtonStyle(entry.type));
            HandleEntryInteraction(messageRect, entryKey, entry.message, !string.IsNullOrEmpty(entry.stackTrace));

            // 绘制日志堆栈
            if ((_showStackTrace || isExpanded) && !string.IsNullOrEmpty(entry.stackTrace))
            {
                GUILayout.Space(3f);
                GUILayout.Label(entry.stackTrace, _stackTraceStyle);
            }

            GUILayout.EndVertical();
            GUILayout.Space(2f);
        }

        /// <summary>
        /// 在窗口外层用绝对坐标绘制右下角缩放手柄，确保始终贴合实际窗口右下角。
        /// </summary>
        private void DrawWindowOverlayControls()
        {
            DrawResizeHandleOverlay();
        }

        /// <summary>
        /// 绘制右下角缩放手柄，并在顶层 GUI 坐标系下处理按下事件。
        /// </summary>
        private void DrawResizeHandleOverlay()
        {
            float handleSize = Scale(24f);
            var   resizeRect = new Rect(_windowRect.xMax - handleSize - Scale(4f), _windowRect.yMax - handleSize - Scale(4f), handleSize, handleSize);
            GUI.Label(resizeRect, "◢", _resizeHandleStyle);
            TryBeginResize(resizeRect);
        }

        #endregion

        #region Filter / Cache

        /// <summary>
        /// 确保过滤缓存是最新的。
        /// </summary>
        private void EnsureFilteredCache()
        {
            if (!_isDirty)
            {
                return;
            }

            RebuildVisibleCache();
            _isDirty = false;
        }

        /// <summary>
        /// 按当前筛选条件重建可见缓存。
        /// </summary>
        private void RebuildVisibleCache()
        {
            _filteredLogs.Clear();
            _collapsedLogs.Clear();

            _logCount       = 0;
            _warningCount   = 0;
            _errorCount     = 0;
            _assertCount    = 0;
            _exceptionCount = 0;

            var collapsedIndexMap = _collapse ? new Dictionary<string, int>(128) : null;

            for (int i = 0; i < _logs.Count; i++)
            {
                var entry = _logs[i];
                CountType(entry.type);

                if (!PassTypeFilter(entry.type))
                {
                    continue;
                }

                if (!PassSearchFilter(entry))
                {
                    continue;
                }

                if (_collapse)
                {
                    string collapseKey = GetCollapseKey(entry);
                    if (collapsedIndexMap != null && collapsedIndexMap.TryGetValue(collapseKey, out var collapsedIndex))
                    {
                        _collapsedLogs[collapsedIndex].count++;
                        _collapsedLogs[collapsedIndex].lastTime = entry.time;
                    }
                    else
                    {
                        collapsedIndexMap?.Add(collapseKey, _collapsedLogs.Count);
                        _collapsedLogs.Add(new CollapsedLogEntry(entry));
                    }
                }
                else
                {
                    _filteredLogs.Add(entry);
                }
            }

            if (_collapse)
            {
                _collapsedLogs.Sort((left, right) => _sortNewestFirst
                                                         ? right.lastTime.CompareTo(left.lastTime)
                                                         : left.lastTime.CompareTo(right.lastTime));
            }
        }

        /// <summary>
        /// 统计日志类型数量。
        /// </summary>
        private void CountType(LogType type)
        {
            switch (type)
            {
                case LogType.Warning:
                    _warningCount++;
                    break;
                case LogType.Error:
                    _errorCount++;
                    break;
                case LogType.Assert:
                    _assertCount++;
                    break;
                case LogType.Exception:
                    _exceptionCount++;
                    break;
                default:
                    _logCount++;
                    break;
            }
        }

        /// <summary>
        /// 当前类型是否通过筛选。
        /// </summary>
        private bool PassTypeFilter(LogType type)
        {
            return type switch
            {
                LogType.Warning   => _showWarning,
                LogType.Error     => _showError,
                LogType.Assert    => _showAssert,
                LogType.Exception => _showException,
                _                 => _showLog
            };
        }

        /// <summary>
        /// 当前日志是否通过搜索筛选。
        /// </summary>
        private bool PassSearchFilter(LogEntry entry)
        {
            if (string.IsNullOrEmpty(_searchKeyword))
            {
                return true;
            }

            return ContainsIgnoreCase(entry.message, _searchKeyword) || ContainsIgnoreCase(entry.stackTrace, _searchKeyword);
        }

        /// <summary>
        /// 生成折叠键，避免不同类型日志被错误合并。
        /// 如果同一消息但调用堆栈不同，也会分别统计，更准确。
        /// </summary>
        private static string GetCollapseKey(LogEntry entry)
        {
            return (int)entry.type + "|" + entry.message + "|" + entry.stackTrace;
        }

        /// <summary>
        /// 忽略大小写判断字符串包含。
        /// </summary>
        private static bool ContainsIgnoreCase(string source, string target)
        {
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(target))
            {
                return false;
            }

            return source.IndexOf(target, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        #endregion

        #region Entry Interaction

        /// <summary>
        /// 处理单条日志交互：单击复制正文，双击展开或收起堆栈。
        /// </summary>
        private void HandleEntryInteraction(Rect rect, string entryKey, string message, bool hasStackTrace)
        {
            var currentEvent = Event.current;
            if (currentEvent == null)
            {
                return;
            }

            if (currentEvent.type == EventType.MouseDown && currentEvent.button == 0 && rect.Contains(currentEvent.mousePosition))
            {
                if (currentEvent.clickCount >= 2 && hasStackTrace)
                {
                    ToggleEntryExpanded(entryKey);
                }
                else
                {
                    CopyToClipboard(message, "已复制日志内容。");
                }

                currentEvent.Use();
            }
        }

        /// <summary>
        /// 切换单条日志堆栈的展开状态。
        /// </summary>
        private void ToggleEntryExpanded(string entryKey)
        {
            if (!_expandedStackKeys.Add(entryKey))
            {
                _expandedStackKeys.Remove(entryKey);
                ShowStatus("已收起该日志堆栈。");
            }
            else
            {
                ShowStatus("已展开该日志堆栈。");
            }
        }

        /// <summary>
        /// 获取单条日志在界面中的唯一键。
        /// </summary>
        private static string GetEntryDisplayKey(LogEntry entry)
        {
            return $"{(int)entry.type}|{entry.time.Ticks}|{entry.message}|{entry.stackTrace}";
        }

        /// <summary>
        /// 获取折叠日志在界面中的唯一键。
        /// </summary>
        private static string GetEntryDisplayKey(CollapsedLogEntry entry)
        {
            return $"{(int)entry.type}|{entry.firstTime.Ticks}|{entry.message}|{entry.stackTrace}";
        }

        #endregion

        #region Save / Open / Copy

        /// <summary>
        /// 保存当前视图的日志到 TXT。
        /// </summary>
        private void SaveLogsToTxt()
        {
            try
            {
                EnsureFilteredCache();

                string directory = GetSaveDirectory();
                Directory.CreateDirectory(directory);

                string filePath = Path.Combine(directory, $"DebugLog_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
                var    builder  = new StringBuilder(8192);

                builder.AppendLine("================ Debug Log Export ================");
                builder.AppendLine("导出时间: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                builder.AppendLine("当前场景: " + SceneManager.GetActiveScene().name);
                builder.AppendLine("缓存日志数: " + _logs.Count);
                builder.AppendLine("当前可见数: " + GetVisibleCount());
                builder.AppendLine("排序方式: " + (_sortNewestFirst ? "最新在上" : "最早在上"));
                builder.AppendLine("折叠模式: " + (_collapse ? "是" : "否"));
                builder.AppendLine("显示堆栈: " + (_showStackTrace ? "是" : "否"));
                builder.AppendLine("搜索关键字: " + (string.IsNullOrEmpty(_searchKeyword) ? "<空>" : _searchKeyword));
                builder.AppendLine($"类型过滤: Log={_showLog}, Warning={_showWarning}, Error={_showError}, Assert={_showAssert}, Exception={_showException}");
                builder.AppendLine("==================================================");
                builder.AppendLine();

                if (_collapse)
                {
                    for (int i = 0; i < _collapsedLogs.Count; i++)
                    {
                        var entry = _collapsedLogs[i];
                        builder.AppendLine($"[{entry.lastTime:HH:mm:ss}] {GetTypeIcon(entry.type)} [{GetTypeLabel(entry.type)}] x{entry.count}");
                        builder.AppendLine(entry.message);

                        if ((_showStackTrace || _expandedStackKeys.Contains(GetEntryDisplayKey(entry))) && !string.IsNullOrEmpty(entry.stackTrace))
                        {
                            builder.AppendLine(entry.stackTrace);
                        }

                        builder.AppendLine();
                    }
                }
                else if (_sortNewestFirst)
                {
                    for (int i = _filteredLogs.Count - 1; i >= 0; i--)
                    {
                        var entry = _filteredLogs[i];
                        builder.AppendLine($"[{entry.time:HH:mm:ss}] {GetTypeIcon(entry.type)} [{GetTypeLabel(entry.type)}]");
                        builder.AppendLine(entry.message);

                        if ((_showStackTrace || _expandedStackKeys.Contains(GetEntryDisplayKey(entry))) && !string.IsNullOrEmpty(entry.stackTrace))
                        {
                            builder.AppendLine(entry.stackTrace);
                        }

                        builder.AppendLine();
                    }
                }
                else
                {
                    for (int i = 0; i < _filteredLogs.Count; i++)
                    {
                        var entry = _filteredLogs[i];
                        builder.AppendLine($"[{entry.time:HH:mm:ss}] {GetTypeIcon(entry.type)} [{GetTypeLabel(entry.type)}]");
                        builder.AppendLine(entry.message);

                        if ((_showStackTrace || _expandedStackKeys.Contains(GetEntryDisplayKey(entry))) && !string.IsNullOrEmpty(entry.stackTrace))
                        {
                            builder.AppendLine(entry.stackTrace);
                        }

                        builder.AppendLine();
                    }
                }

                File.WriteAllText(filePath, builder.ToString(), Encoding.UTF8);
                _lastSavePath = filePath;
                ShowStatus("当前视图日志已保存: " + filePath);
            }
            catch (Exception ex)
            {
                ShowStatus("保存失败: " + ex.Message);
            }
        }

        /// <summary>
        /// 打开日志保存目录。
        /// </summary>
        private void OpenSaveDirectory()
        {
            try
            {
                string directory = GetSaveDirectory();
                Directory.CreateDirectory(directory);
                Application.OpenURL("file:///" + directory.Replace("\\", "/"));
                ShowStatus("已尝试打开目录: " + directory);
            }
            catch (Exception ex)
            {
                ShowStatus("打开目录失败: " + ex.Message);
            }
        }

        /// <summary>
        /// 将文本复制到剪贴板。
        /// </summary>
        private void CopyToClipboard(string text, string successMessage)
        {
            if (string.IsNullOrEmpty(text))
            {
                ShowStatus("没有可复制的内容。");
                return;
            }

            GUIUtility.systemCopyBuffer = text;
            ShowStatus(successMessage);
        }

        /// <summary>
        /// 获取日志保存目录。
        /// </summary>
        private static string GetSaveDirectory()
        {
            return Path.Combine(Application.persistentDataPath, "DebugLogs");
        }

        #endregion

        #region Helper

        /// <summary>
        /// 清空日志。
        /// </summary>
        private void ClearLogs()
        {
            lock (_pendingLogsLock)
            {
                _pendingLogs.Clear();
            }

            _logs.Clear();
            _filteredLogs.Clear();
            _collapsedLogs.Clear();
            _expandedStackKeys.Clear();
            _scrollPosition = Vector2.zero;
            _isDirty        = true;

            _logCount       = 0;
            _warningCount   = 0;
            _errorCount     = 0;
            _assertCount    = 0;
            _exceptionCount = 0;

            ShowStatus("日志已清空。");
        }

        /// <summary>
        /// 显示状态信息。
        /// </summary>
        private void ShowStatus(string message)
        {
            _statusMessage      = message;
            _statusMessageUntil = Time.unscaledTime + 6f;
        }

        /// <summary>
        /// 获取默认状态文字。
        /// </summary>
        private string GetDefaultStatusText()
        {
            string savePath = string.IsNullOrEmpty(_lastSavePath) ? GetSaveDirectory() : _lastSavePath;
            return "F2 开关窗口 | 单击复制 | 双击展开堆栈 | 顶部可拖拽 | 右下角可缩放 | 日志左侧手柄可拖拽滚动 \n保存路径: " + savePath;
        }

        /// <summary>
        /// 获取当前可见条数。
        /// </summary>
        private int GetVisibleCount()
        {
            return _collapse ? _collapsedLogs.Count : _filteredLogs.Count;
        }

        /// <summary>
        /// 绘制筛选按钮。
        /// </summary>
        private void DrawFilterToggle(ref bool value, string label)
        {
            var  icon     = value ? "🟢" : "⚫";
            bool newValue = GUILayout.Toggle(value, icon + label, _toolbarToggleStyle, GUILayout.Height(Scale(30f)));
            if (newValue != value)
            {
                value    = newValue;
                _isDirty = true;
            }
        }

        /// <summary>
        /// 初始化窗口区域。
        /// </summary>
        private void EnsureWindowRect()
        {
            if (_windowRectInitialized)
            {
                return;
            }

            float width  = Mathf.Clamp(_defaultWindowSize.x, _minWindowSize.x, Screen.width);
            float height = Mathf.Clamp(_defaultWindowSize.y, _minWindowSize.y, Screen.height);
            _windowRect            = new Rect(20f, 20f, width, height);
            _windowRectInitialized = true;
        }

        /// <summary>
        /// 防止窗口跑出屏幕。
        /// </summary>
        private void ClampWindowRect()
        {
            _windowRect.width  = Mathf.Clamp(_windowRect.width, _minWindowSize.x, Screen.width);
            _windowRect.height = Mathf.Clamp(_windowRect.height, _minWindowSize.y, Screen.height);
            _windowRect.x      = Mathf.Clamp(_windowRect.x, 0f, Mathf.Max(0f, Screen.width - _windowRect.width));
            _windowRect.y      = Mathf.Clamp(_windowRect.y, 0f, Mathf.Max(0f, Screen.height - _windowRect.height));
        }

        /// <summary>
        /// 按下标题栏后开始拖拽窗口，使用屏幕坐标持续跟踪，避免鼠标移动过快时脱离。
        /// </summary>
        private void TryBeginWindowDrag(Rect headerRect, Rect closeRect)
        {
            var currentEvent = Event.current;
            if (currentEvent == null)
            {
                return;
            }

            bool canStartDrag = currentEvent.type == EventType.MouseDown && currentEvent.button == 0 && headerRect.Contains(currentEvent.mousePosition) &&
                                !closeRect.Contains(currentEvent.mousePosition);
            if (!canStartDrag)
            {
                return;
            }

            _isDraggingWindow             = true;
            _dragStartMouseScreenPosition = GetMouseScreenPosition();
            _dragStartWindowPosition      = _windowRect.position;
            currentEvent.Use();
        }

        /// <summary>
        /// 开始窗口缩放，使用顶层绝对坐标热区保证位置始终正确。
        /// </summary>
        private void TryBeginResize(Rect resizeRect)
        {
            var currentEvent = Event.current;
            if (currentEvent == null)
            {
                return;
            }

            bool canStartResize = currentEvent.type == EventType.MouseDown && currentEvent.button == 0 && resizeRect.Contains(currentEvent.mousePosition);
            if (!canStartResize)
            {
                return;
            }

            _isResizing                     = true;
            _resizeStartMouseScreenPosition = GetMouseScreenPosition();
            _resizeStartWindowSize          = new Vector2(_windowRect.width, _windowRect.height);
            currentEvent.Use();
        }

        /// <summary>
        /// 通过单条日志左侧拖拽手柄开始滚动日志列表，避免与复制、双击展开产生冲突。
        /// </summary>
        private void TryBeginLogListDrag(Rect dragHandleRect)
        {
            var currentEvent = Event.current;
            if (currentEvent == null)
            {
                return;
            }

            bool canStartDragList = currentEvent.type == EventType.MouseDown && currentEvent.button == 0 && dragHandleRect.Contains(currentEvent.mousePosition);
            if (!canStartDragList)
            {
                return;
            }

            _isDraggingLogList                   = true;
            _logListDragStartMouseScreenPosition = GetMouseScreenPosition();
            _logListDragStartScrollPosition      = _scrollPosition;
            currentEvent.Use();
        }

        /// <summary>
        /// 在 Update 中持续处理拖拽与缩放，避免 IMGUI 鼠标事件离开窗口后丢失。
        /// </summary>
        private void UpdateGlobalWindowInteraction()
        {
            if (!_isDraggingWindow && !_isResizing && !_isDraggingLogList)
            {
                return;
            }

            if (!Input.GetMouseButton(0))
            {
                StopWindowInteraction();
                return;
            }

            var currentMouseScreenPosition = GetMouseScreenPosition();

            if (_isDraggingWindow)
            {
                var delta = currentMouseScreenPosition - _dragStartMouseScreenPosition;
                _windowRect.position = _dragStartWindowPosition + delta;
            }

            if (_isResizing)
            {
                var delta = currentMouseScreenPosition - _resizeStartMouseScreenPosition;
                _windowRect.width  = Mathf.Clamp(_resizeStartWindowSize.x + delta.x, _minWindowSize.x, Screen.width);
                _windowRect.height = Mathf.Clamp(_resizeStartWindowSize.y + delta.y, _minWindowSize.y, Screen.height);
            }

            if (_isDraggingLogList)
            {
                var delta = currentMouseScreenPosition - _logListDragStartMouseScreenPosition;
                _scrollPosition.x = Mathf.Max(0f, _logListDragStartScrollPosition.x - delta.x);
                _scrollPosition.y = Mathf.Max(0f, _logListDragStartScrollPosition.y - delta.y);
            }

            ClampWindowRect();
        }

        /// <summary>
        /// 停止当前窗口拖拽或缩放状态。
        /// </summary>
        private void StopWindowInteraction()
        {
            _isDraggingWindow  = false;
            _isResizing        = false;
            _isDraggingLogList = false;
        }

        /// <summary>
        /// 获取当前鼠标的屏幕坐标，并转换为与 GUI 一致的左上角原点坐标系。
        /// </summary>
        private static Vector2 GetMouseScreenPosition()
        {
            return new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
        }

        /// <summary>
        /// 将基础像素值按当前界面缩放系数转换。
        /// </summary>
        private float Scale(float value)
        {
            return value * _uiScale;
        }

        /// <summary>
        /// 在运行时修改字号后，强制重建 GUI 样式。
        /// </summary>
        private void MarkStyleDirty()
        {
            _cachedUiScale = -1f;
        }

        /// <summary>
        /// 初始化 GUI 样式。
        /// </summary>
        private void EnsureStyles()
        {
            if (_titleStyle != null && Mathf.Approximately(_cachedUiScale, _uiScale))
            {
                return;
            }

            _cachedUiScale = _uiScale;
            int titleFont  = Mathf.RoundToInt(15f * _uiScale);
            int buttonFont = Mathf.RoundToInt(12f * _uiScale);
            int normalFont = Mathf.RoundToInt(13f * _uiScale);
            int smallFont  = Mathf.RoundToInt(12f * _uiScale);
            int resizeFont = Mathf.RoundToInt(18f * _uiScale);

            _titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize  = titleFont,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                richText  = true
            };

            _toolbarButtonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize  = buttonFont,
                alignment = TextAnchor.MiddleCenter,
                padding   = new RectOffset(Mathf.RoundToInt(12f * _uiScale), Mathf.RoundToInt(12f * _uiScale), Mathf.RoundToInt(5f * _uiScale), Mathf.RoundToInt(5f * _uiScale))
            };

            _toolbarDangerButtonStyle = new GUIStyle(_toolbarButtonStyle)
            {
                normal =
                {
                    textColor = new Color(1f, 0.45f, 0.45f, 1f)
                }
            };

            _toolbarToggleStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize  = buttonFont,
                alignment = TextAnchor.MiddleCenter,
                padding   = new RectOffset(Mathf.RoundToInt(12f * _uiScale), Mathf.RoundToInt(12f * _uiScale), Mathf.RoundToInt(5f * _uiScale), Mathf.RoundToInt(5f * _uiScale))
            };

            _searchTextFieldStyle = new GUIStyle(GUI.skin.textField)
            {
                fontSize  = normalFont,
                alignment = TextAnchor.MiddleLeft,
                padding   = new RectOffset(Mathf.RoundToInt(10f * _uiScale), Mathf.RoundToInt(10f * _uiScale), Mathf.RoundToInt(5f * _uiScale), Mathf.RoundToInt(5f * _uiScale))
            };

            _entryBoxStyle = new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.UpperLeft,
                padding   = new RectOffset(Mathf.RoundToInt(14f * _uiScale), Mathf.RoundToInt(14f * _uiScale), Mathf.RoundToInt(10f * _uiScale), Mathf.RoundToInt(10f * _uiScale)),
                margin    = new RectOffset(Mathf.RoundToInt(3f * _uiScale), Mathf.RoundToInt(3f * _uiScale), Mathf.RoundToInt(3f * _uiScale), Mathf.RoundToInt(3f * _uiScale))
            };

            _entryHeaderStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize  = smallFont,
                wordWrap  = false,
                richText  = true,
                alignment = TextAnchor.MiddleLeft,
                normal =
                {
                    textColor = new Color(0.90f, 0.90f, 0.90f, 1f),
                }
            };

            _entryTextButtonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize  = normalFont,
                wordWrap  = true,
                richText  = true,
                alignment = TextAnchor.UpperLeft,
                padding   = new RectOffset(0, 0, Mathf.RoundToInt(2f * _uiScale), Mathf.RoundToInt(2f * _uiScale)),
                margin    = new RectOffset(0, 0, Mathf.RoundToInt(4f * _uiScale), Mathf.RoundToInt(4f * _uiScale)),
                border    = new RectOffset(0, 0, 0, 0),
                normal =
                {
                    background = null
                },
                hover =
                {
                    background = null
                },
                active =
                {
                    background = null
                },
                focused =
                {
                    background = null
                },
                onNormal =
                {
                    background = null
                },
                onHover =
                {
                    background = null
                },
                onActive =
                {
                    background = null
                },
                onFocused =
                {
                    background = null
                }
            };

            _entryLogTextStyle       = CreateEntryTextStyle(Color.white, new Color(0.88f, 0.95f, 1f, 1f));
            _entryWarningTextStyle   = CreateEntryTextStyle(Color.yellow, new Color(0.88f, 0.95f, 1f, 1f));
            _entryErrorTextStyle     = CreateEntryTextStyle(Color.red, new Color(0.88f, 0.95f, 1f, 1f));
            _entryAssertTextStyle    = CreateEntryTextStyle(Color.red, new Color(0.88f, 0.95f, 1f, 1f));
            _entryExceptionTextStyle = CreateEntryTextStyle(Color.red, new Color(0.88f, 0.95f, 1f, 1f));

            _stackTraceStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize  = smallFont,
                wordWrap  = true,
                richText  = true,
                alignment = TextAnchor.UpperLeft,
                padding   = new RectOffset(0, 0, Mathf.RoundToInt(4f * _uiScale), 0),
                normal    = { textColor = new Color(0.75f, 0.85f, 1f, 1f) }
            };

            _metaTextStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize  = smallFont,
                wordWrap  = true,
                richText  = true,
                alignment = TextAnchor.MiddleLeft,
                normal    = { textColor = new Color(0.82f, 0.82f, 0.82f, 1f) }
            };

            _statusStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize  = smallFont,
                wordWrap  = true,
                richText  = true,
                alignment = TextAnchor.MiddleLeft,
                normal    = { textColor = new Color(0.85f, 0.9f, 1f, 1f) }
            };

            _resizeHandleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize  = resizeFont,
                alignment = TextAnchor.MiddleCenter,
                normal    = { textColor = Color.white }
            };

            _entryDragHandleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize   = Mathf.RoundToInt(16f * _uiScale),
                alignment  = TextAnchor.MiddleCenter,
                fixedWidth = Mathf.RoundToInt(30f * _uiScale),
                normal     = { textColor = new Color(0.80f, 0.86f, 0.95f, 0.95f) },
                hover      = { textColor = new Color(0.95f, 0.97f, 1f, 1f) }
            };

            _closeButtonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize  = Mathf.RoundToInt(13f * _uiScale),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                padding   = new RectOffset(0, 0, 0, 0),
                margin    = new RectOffset(0, 0, 0, 0)
            };
        }

        private static GUIStyle CreateEntryTextStyle(Color normalColor, Color hoverColor)
        {
            var style = new GUIStyle(_entryTextButtonStyle)
            {
                normal =
                {
                    textColor = normalColor
                },
                hover =
                {
                    textColor = hoverColor
                },
                active =
                {
                    textColor = hoverColor
                },
                focused =
                {
                    textColor = normalColor
                },
                padding = new RectOffset(Mathf.RoundToInt(10f), Mathf.RoundToInt(10f), 10, 10)
            };
            return style;
        }

        /// <summary>
        /// 获取日志正文按钮样式。
        /// </summary>
        private static GUIStyle GetEntryMessageButtonStyle(LogType type)
        {
            return type switch
            {
                LogType.Warning   => _entryWarningTextStyle,
                LogType.Error     => _entryErrorTextStyle,
                LogType.Assert    => _entryAssertTextStyle,
                LogType.Exception => _entryExceptionTextStyle,
                _                 => _entryLogTextStyle
            };
        }

        /// <summary>
        /// 获取日志背景色。
        /// </summary>
        private Color GetEntryBackgroundColor(LogType type, int index)
        {
            var baseColor = type switch
            {
                LogType.Warning   => new Color(0.38f, 0.30f, 0.08f, Mathf.Clamp01(_windowOpacity)),
                LogType.Error     => new Color(0.38f, 0.12f, 0.12f, Mathf.Clamp01(_windowOpacity)),
                LogType.Assert    => new Color(0.33f, 0.12f, 0.33f, Mathf.Clamp01(_windowOpacity)),
                LogType.Exception => new Color(0.42f, 0.14f, 0.14f, Mathf.Clamp01(_windowOpacity)),
                _                 => new Color(0.16f, 0.16f, 0.16f, Mathf.Clamp01(_windowOpacity))
            };

            if ((index & 1) == 1)
            {
                baseColor.r = Mathf.Clamp01(baseColor.r + 0.03f);
                baseColor.g = Mathf.Clamp01(baseColor.g + 0.03f);
                baseColor.b = Mathf.Clamp01(baseColor.b + 0.03f);
            }

            return baseColor;
        }

        /// <summary>
        /// 获取日志类型图标。
        /// </summary>
        private static string GetTypeIcon(LogType type)
        {
            return type switch
            {
                LogType.Warning   => "", // "⚠️",
                LogType.Error     => "", // "❌",
                LogType.Assert    => "", // "🔘",
                LogType.Exception => "", // "🚫",
                _                 => "", // "🔆"
            };
        }

        /// <summary>
        /// 获取日志类型头图标。
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static string GetTypeHeadIcon(LogType type)
        {
            return type switch
            {
                LogType.Warning   => "🟡",
                LogType.Error     => "🔴",
                LogType.Assert    => "🟠",
                LogType.Exception => "🟣",
                _                 => "⚪"
            };
        }

        /// <summary>
        /// 获取日志类型标签。
        /// </summary>
        private static string GetTypeLabel(LogType type)
        {
            return type switch
            {
                LogType.Warning   => "Warning",
                LogType.Error     => "Error",
                LogType.Assert    => "Assert",
                LogType.Exception => "Exception",
                _                 => "Log"
            };
        }

        #endregion

        #region Inner Types

        /// <summary>
        /// 单条日志记录。
        /// </summary>
        private sealed class LogEntry
        {
            public readonly string message;
            public readonly string stackTrace;
            public readonly LogType type;
            public readonly DateTime time;

            public LogEntry(string message, string stackTrace, LogType type, DateTime time)
            {
                this.message    = message;
                this.stackTrace = stackTrace;
                this.type       = type;
                this.time       = time;
            }
        }

        /// <summary>
        /// 折叠后的日志记录。
        /// </summary>
        private sealed class CollapsedLogEntry
        {
            public readonly string message;
            public readonly string stackTrace;
            public readonly LogType type;
            public int count;
            public readonly DateTime firstTime;
            public DateTime lastTime;

            public CollapsedLogEntry(LogEntry source)
            {
                message    = source.message;
                stackTrace = source.stackTrace;
                type       = source.type;
                count      = 1;
                firstTime  = source.time;
                lastTime   = source.time;
            }
        }

        /// <summary>
        /// 线程队列中的临时日志记录。
        /// </summary>
        private struct PendingLog
        {
            public readonly string message;
            public readonly string stackTrace;
            public readonly LogType type;
            public readonly DateTime time;

            public PendingLog(string message, string stackTrace, LogType type, DateTime time)
            {
                this.message    = message;
                this.stackTrace = stackTrace;
                this.type       = type;
                this.time       = time;
            }
        }

        #endregion
    }
}
#endif
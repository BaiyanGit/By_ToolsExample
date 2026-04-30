namespace Demos.示例_快捷键切换应用.Scripts.KeyPadAPI
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using UnityEngine;
    using UnityEngine.UI;
    using Debug = UnityEngine.Debug;

    /// <summary>
    /// 静态 UGUI 版本进程任务管理器。
    ///
    /// 当前版本：
    /// 1. 不再动态创建主界面。
    /// 2. 只绑定场景中已经搭建好的 UGUI 控件。
    /// 3. 进程列表使用对象池，刷新列表时复用行对象，避免频繁 Instantiate / Destroy 卡顿。
    /// 4. 保留快捷键、刷新、搜索、显示、隐藏/最小化、杀掉、复制名、搜同名等功能。
    /// </summary>
    public class WindowControllerUGUI : MonoBehaviour
    {
        #region Root

        [Header("----------------------------------------------------------------")] [Header("是否启动时按名称自动绑定场景中的 UGUI 控件")] [SerializeField]
        private bool _autoFindReferences = true;

        [Header("目标 Canvas")] [SerializeField] private Canvas _targetCanvas;

        [Header("UGUI 根面板，一般是 SwitchAppPanel")] [SerializeField]
        private RectTransform _rootPanel;

        #endregion

        #region Header

        [Header("----------------------------------------------------------------")] [Header("最小化当前 Unity 窗口按钮")] [SerializeField]
        private Button _btnMinimize;

        [Header("最大化当前 Unity 窗口按钮")] [SerializeField]
        private Button _btnMaximize;

        [Header("还原当前 Unity 窗口按钮")] [SerializeField]
        private Button _btnRestore;

        [Header("隐藏/显示主面板按钮")] [SerializeField]
        private Button _btnTogglePanel;

        [Header("关闭当前应用按钮")] [SerializeField] private Button _btnClose;

        #endregion

        #region ProcessToolbar

        [Header("----------------------------------------------------------------")] [Header("搜索进程输入框")] [SerializeField]
        private InputField _inputSearch;

        [Header("清空搜索内容按钮")] [SerializeField] private Button _btnClearSearch;
        [Header("复制搜索内容按钮")] [SerializeField] private Button _btnCopySearch;
        [Header("刷新进程列表按钮")] [SerializeField] private Button _btnRefresh;

        [Header("是否只显示有主窗口句柄的进程")] [SerializeField]
        private Toggle _toggleOnlyWindow;

        [Header("隐藏按钮是否完全隐藏窗口，否则执行最小化")] [SerializeField]
        private Toggle _toggleUseHide;

        #endregion

        #region ProcessScrollView

        [Header("----------------------------------------------------------------")] [Header("进程列表内容父节点，一般是 ScrollView/Viewport/Content")] [SerializeField]
        private RectTransform _processContent;

        [Header("进程行模板，建议放在 ProcessContent 下，运行时会被隐藏并克隆")] [SerializeField]
        private RectTransform _processRowTemplate;

        [Header("进程列表表头行，刷新进程时不会隐藏或复用")] [SerializeField]
        private RectTransform _processHeaderRow;

        #endregion

        #region Params Settings

        [Header("----------------------------------------------------------------")] [Header("状态文本")] [SerializeField]
        private Text _statusText;

        [Header("启动时是否自动刷新一次进程列表")] [SerializeField]
        private bool _refreshProcessListOnStart = true;

        [Header("快捷键第二个按键索引，0 表示不设置")] [SerializeField]
        private int _hotkeyIndex2 = 17;

        [Header("对象池初始容量")] [SerializeField] private int _initialRowPoolSize = 32;

        [Header("每次扩容时新增的行数量")] [SerializeField]
        private int _rowPoolExpandSize = 16;

        [Header("池中未使用的行是否放到 Content 末尾")] [SerializeField]
        private bool _moveInactiveRowsToBottom = true;

        #endregion


        private readonly HashSet<int> _pressedKeys = new();
        private readonly List<ProcessInfo> _processInfos = new();
        private readonly List<ProcessRowView> _rowPool = new();
        private readonly List<ProcessRowView> _activeRows = new();

        private KeyboardHook _windowsKeyboardHook;
        private bool _shortcutTriggered;
        private bool _panelVisible = true;
        private int _hotkeyTriggerCount;
        private string _statusTextContent = "未启动";
        private bool _eventsBound;
        private bool _isRefreshingView;

        private void Awake()
        {
            Application.runInBackground = true;
            if (_autoFindReferences)
            {
                AutoBindReferences();
            }

            PrepareStaticUi();
            BindEvents();
            SetStatus("UGUI 静态界面已启动。");

            if (_refreshProcessListOnStart)
            {
                RefreshProcessList();
            }
        }

        private void OnDestroy()
        {
            UnbindEvents();
            ClearPool();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F2))
            {
                TogglePanelVisible();
            }
        }

        /// <summary>
        /// 按场景中对象名自动查找 UGUI 控件。
        /// Inspector 手动拖引用也可以；如果引用为空，会尝试自动补齐。
        /// </summary>
        private void AutoBindReferences()
        {
            if (_targetCanvas == null)
            {
                _targetCanvas = FindFirstObjectByType<Canvas>();
            }

            var searchRoot = _targetCanvas != null ? _targetCanvas.transform : null;

            if (_rootPanel == null)
            {
                _rootPanel = FindRect(searchRoot, "SwitchAppPanel");
            }

            var root = _rootPanel != null ? _rootPanel : searchRoot;

            _statusText       = FindComponent(root, "StatusText", _statusText);
            _inputSearch      = FindComponent(root, "InputSearch", _inputSearch);
            _toggleOnlyWindow = FindComponent(root, "ToggleOnlyWindow", _toggleOnlyWindow);
            _toggleUseHide    = FindComponent(root, "ToggleUseHide", _toggleUseHide);
            _btnRefresh       = FindComponent(root, "BtnRefresh", _btnRefresh);
            _btnTogglePanel   = FindComponent(root, "BtnTogglePanel", _btnTogglePanel);
            _btnMinimize      = FindComponent(root, "BtnMinimize", _btnMinimize);
            _btnMaximize      = FindComponent(root, "BtnMaximize", _btnMaximize);
            _btnRestore       = FindComponent(root, "BtnRestore", _btnRestore);
            _btnClose         = FindComponent(root, "BtnClose", _btnClose);
            _btnCopySearch    = FindComponent(root, "BtnCopySearch", _btnCopySearch);
            _btnClearSearch   = FindComponent(root, "BtnClearSearch", _btnClearSearch);

            if (_processContent == null)
            {
                var content = FindRect(root, "Content");
                if (content != null)
                {
                    _processContent = content;
                }
            }

            if (_processHeaderRow == null && _processContent != null)
            {
                _processHeaderRow = FindDirectChildRect(_processContent, "ProcessHeaderRow");
            }

            if (_processRowTemplate == null && _processContent != null)
            {
                _processRowTemplate = FindFirstProcessRowTemplate(_processContent);
            }
        }

        /// <summary>
        /// 准备静态界面和对象池。
        /// </summary>
        private void PrepareStaticUi()
        {
            if (_processRowTemplate != null)
            {
                _processRowTemplate.gameObject.SetActive(false);
            }
            else
            {
                Debug.LogWarning("[进程任务管理器-UGUI] 未绑定 ProcessRowTemplate，进程列表无法生成。");
            }

            if (_toggleOnlyWindow != null)
            {
                _toggleOnlyWindow.isOn = true;
            }

            PrewarmRowPool();
        }

        /// <summary>
        /// 预热对象池，避免第一次刷新时一次性创建太多行导致卡顿。
        /// </summary>
        private void PrewarmRowPool()
        {
            if (_processRowTemplate == null || _processContent == null)
            {
                return;
            }

            _initialRowPoolSize = Mathf.Max(0, _initialRowPoolSize);
            _rowPoolExpandSize  = Mathf.Max(1, _rowPoolExpandSize);

            for (int i = _rowPool.Count; i < _initialRowPoolSize; i++)
            {
                var row = CreateRowInstance();
                ReleaseRow(row);
            }
        }

        private void BindEvents()
        {
            if (_eventsBound)
            {
                return;
            }

            _eventsBound = true;

            if (_btnRefresh) _btnRefresh.onClick.AddListener(RefreshProcessList);
            if (_btnTogglePanel) _btnTogglePanel.onClick.AddListener(TogglePanelVisible);
            if (_btnMinimize) _btnMinimize.onClick.AddListener(WindowAPI.WindowMinimize);
            if (_btnMaximize) _btnMaximize.onClick.AddListener(WindowAPI.WindowMaximize);
            if (_btnRestore) _btnRestore.onClick.AddListener(WindowAPI.WindowRestore);
            if (_btnClose) _btnClose.onClick.AddListener(WindowAPI.WindowClose);
            if (_btnCopySearch != null) _btnCopySearch.onClick.AddListener(CopySearchText);
            if (_btnClearSearch) _btnClearSearch.onClick.AddListener(ClearSearchText);

            if (_inputSearch) _inputSearch.onValueChanged.AddListener(OnSearchChanged);
            if (_toggleOnlyWindow) _toggleOnlyWindow.onValueChanged.AddListener(OnOnlyWindowChanged);
            if (_toggleUseHide) _toggleUseHide.onValueChanged.AddListener(OnHideModeChanged);
        }

        private void UnbindEvents()
        {
            if (!_eventsBound)
            {
                return;
            }

            _eventsBound = false;

            if (_btnRefresh) _btnRefresh.onClick.RemoveListener(RefreshProcessList);
            if (_btnTogglePanel) _btnTogglePanel.onClick.RemoveListener(TogglePanelVisible);
            if (_btnMinimize) _btnMinimize.onClick.RemoveListener(WindowAPI.WindowMinimize);
            if (_btnMaximize) _btnMaximize.onClick.RemoveListener(WindowAPI.WindowMaximize);
            if (_btnRestore) _btnRestore.onClick.RemoveListener(WindowAPI.WindowRestore);
            if (_btnClose) _btnClose.onClick.RemoveListener(WindowAPI.WindowClose);
            if (_btnCopySearch) _btnCopySearch.onClick.RemoveListener(CopySearchText);
            if (_btnClearSearch) _btnClearSearch.onClick.RemoveListener(ClearSearchText);

            if (_inputSearch) _inputSearch.onValueChanged.RemoveListener(OnSearchChanged);
            if (_toggleOnlyWindow) _toggleOnlyWindow.onValueChanged.RemoveListener(OnOnlyWindowChanged);
            if (_toggleUseHide) _toggleUseHide.onValueChanged.RemoveListener(OnHideModeChanged);
        }

        private static void FillKeyDropdown(Dropdown dropdown)
        {
            if (dropdown == null)
            {
                return;
            }

            dropdown.options.Clear();

            for (int i = 0; i < KeyCodeName.KeyNames.Length; i++)
            {
                dropdown.options.Add(new Dropdown.OptionData(KeyCodeName.KeyNames[i]));
            }

            dropdown.RefreshShownValue();
        }

        private void OnSearchChanged(string _)
        {
            RefreshProcessListView();
        }

        private void OnOnlyWindowChanged(bool _)
        {
            RefreshProcessListView();
        }

        private void OnHideModeChanged(bool _)
        {
            RefreshProcessListView();
        }

        private void TogglePanelVisible()
        {
            _panelVisible = !_panelVisible;

            if (_rootPanel != null)
            {
                _rootPanel.gameObject.SetActive(_panelVisible);
            }

            if (_btnTogglePanel != null)
            {
                var buttonText = _btnTogglePanel.GetComponentInChildren<Text>(true);
                if (buttonText != null)
                {
                    buttonText.text = _panelVisible ? "隐藏面板" : "显示面板";
                }
            }
        }

        private void CopySearchText()
        {
            GUIUtility.systemCopyBuffer = _inputSearch != null ? _inputSearch.text : string.Empty;
            SetStatus("已复制搜索内容：" + GUIUtility.systemCopyBuffer);
        }

        private void ClearSearchText()
        {
            if (_inputSearch != null)
            {
                _inputSearch.text = string.Empty;
            }

            RefreshProcessListView();
        }

        /// <summary>
        /// 刷新系统进程数据。
        /// </summary>
        private void RefreshProcessList()
        {
            _processInfos.Clear();

            var processArray = Array.Empty<Process>();

            try
            {
                processArray = Process.GetProcesses();

                for (int i = 0; i < processArray.Length; i++)
                {
                    if (!TryBuildProcessInfo(processArray[i], out var info))
                    {
                        continue;
                    }

                    _processInfos.Add(info);
                }

                _processInfos.Sort((a, b) => string.Compare(a.processName, b.processName, StringComparison.OrdinalIgnoreCase));
                SetStatus("进程列表已刷新：" + _processInfos.Count + " 个。");
            }
            finally
            {
                for (int i = 0; i < processArray.Length; i++)
                {
                    processArray[i].Dispose();
                }
            }

            RefreshProcessListView();
        }

        /// <summary>
        /// 刷新 UGUI 列表。
        /// 这里只复用对象池中的行对象，不销毁旧对象。
        /// </summary>
        private void RefreshProcessListView()
        {
            if (_isRefreshingView)
            {
                return;
            }

            if (_processContent == null)
            {
                SetStatus("未绑定进程列表 Content。");
                return;
            }

            if (_processRowTemplate == null)
            {
                SetStatus("未绑定 ProcessRow 模板。");
                return;
            }

            _isRefreshingView = true;

            try
            {
                ReleaseAllActiveRows();

                int visibleIndex = 0;
                for (int i = 0; i < _processInfos.Count; i++)
                {
                    var info = _processInfos[i];
                    if (!IsProcessVisible(info))
                    {
                        continue;
                    }

                    visibleIndex++;

                    var row = GetRowFromPool();
                    row.rectTransform.SetSiblingIndex(GetRowSiblingIndex(visibleIndex));
                    row.gameObject.SetActive(true);
                    row.Fill(visibleIndex, info, UseHideMode(), ShowProcessWindow, HideOrMinimizeProcessWindow, KillProcess, CopyProcessName, SearchSameProcess);

                    _activeRows.Add(row);
                }

                if (_moveInactiveRowsToBottom)
                {
                    MoveInactiveRowsToBottom();
                }
            }
            finally
            {
                _isRefreshingView = false;
            }
        }

        private int GetRowSiblingIndex(int visibleIndex)
        {
            return _processHeaderRow != null
                       ? Mathf.Min(_processHeaderRow.GetSiblingIndex() + visibleIndex, _processContent.childCount - 1)
                       : Mathf.Min(visibleIndex - 1, _processContent.childCount - 1);
        }

        private bool IsProcessVisible(ProcessInfo info)
        {
            bool onlyWindow = _toggleOnlyWindow != null && _toggleOnlyWindow.isOn;
            if (onlyWindow && info.mainWindowHandle == IntPtr.Zero)
            {
                return false;
            }

            string keyword = _inputSearch != null ? _inputSearch.text : string.Empty;
            if (string.IsNullOrEmpty(keyword))
            {
                return true;
            }

            return info.processName != null &&
                   info.processName.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private ProcessRowView GetRowFromPool()
        {
            for (int i = 0; i < _rowPool.Count; i++)
            {
                var row = _rowPool[i];
                if (!row.isActive)
                {
                    row.isActive = true;
                    return row;
                }
            }

            int            expandCount  = Mathf.Max(1, _rowPoolExpandSize);
            ProcessRowView firstCreated = null;

            for (int i = 0; i < expandCount; i++)
            {
                var row = CreateRowInstance();
                ReleaseRow(row);

                firstCreated ??= row;
            }

            if (firstCreated != null)
                firstCreated.isActive = true;

            return firstCreated;
        }

        private ProcessRowView CreateRowInstance()
        {
            var rowTransform = Instantiate(_processRowTemplate, _processContent);
            rowTransform.name = "PooledProcessRow_" + _rowPool.Count;
            rowTransform.gameObject.SetActive(false);

            var row = new ProcessRowView(rowTransform);
            _rowPool.Add(row);
            return row;
        }

        private void ReleaseAllActiveRows()
        {
            for (int i = 0; i < _activeRows.Count; i++)
            {
                ReleaseRow(_activeRows[i]);
            }

            _activeRows.Clear();
        }

        private static void ReleaseRow(ProcessRowView row)
        {
            if (row == null)
            {
                return;
            }

            row.Clear();
            row.isActive = false;
            row.gameObject.SetActive(false);
        }

        private void MoveInactiveRowsToBottom()
        {
            int bottomIndex = _processContent != null ? _processContent.childCount - 1 : 0;

            for (int i = 0; i < _rowPool.Count; i++)
            {
                var row = _rowPool[i];
                if (row is { isActive: false })
                {
                    row.rectTransform.SetSiblingIndex(bottomIndex);
                }
            }
        }

        private void ClearPool()
        {
            ReleaseAllActiveRows();

            for (int i = 0; i < _rowPool.Count; i++)
            {
                var row = _rowPool[i];
                row?.Clear();
            }

            _rowPool.Clear();
        }

        private void SearchSameProcess(ProcessInfo info)
        {
            if (_inputSearch != null)
            {
                _inputSearch.text = info.processName;
            }
        }

        private void ShowProcessWindow(ProcessInfo info)
        {
            if (info.mainWindowHandle == IntPtr.Zero)
            {
                SetStatus("进程没有可显示的主窗口：" + info.processName);
                return;
            }

            bool success = WindowAPI.RestoreAndActivateWindow(info.mainWindowHandle);
            SetStatus(success ? "已显示并激活：" + info.processName : "显示失败：" + info.processName);
        }

        private void HideOrMinimizeProcessWindow(ProcessInfo info)
        {
            if (info.mainWindowHandle == IntPtr.Zero)
            {
                SetStatus("进程没有可隐藏/最小化的主窗口：" + info.processName);
                return;
            }

            bool success = UseHideMode()
                               ? WindowAPI.HideWindow(info.mainWindowHandle)
                               : WindowAPI.MinimizeWindow(info.mainWindowHandle);

            SetStatus(success
                          ? (UseHideMode() ? "已隐藏：" : "已最小化：") + info.processName
                          : "隐藏/最小化失败：" + info.processName);
        }

        private void KillProcess(ProcessInfo info)
        {
            Process process = null;
            try
            {
                process = Process.GetProcessById(info.processId);
                if (process.HasExited)
                {
                    SetStatus("进程已退出：" + info.processName);
                    return;
                }

                process.Kill();
                SetStatus("已杀掉进程：" + info.processName + " / PID=" + info.processId);
                RefreshProcessList();
            }
            catch (Exception ex)
            {
                SetStatus("杀掉进程失败：" + info.processName + "，原因：" + ex.Message);
                Debug.LogWarning(ex);
            }
            finally
            {
                if (process != null)
                {
                    process.Dispose();
                }
            }
        }

        private void CopyProcessName(ProcessInfo info)
        {
            GUIUtility.systemCopyBuffer = info.processName ?? string.Empty;
            SetStatus("已复制进程名：" + GUIUtility.systemCopyBuffer);
        }

        private static bool TryBuildProcessInfo(Process process, out ProcessInfo processInfo)
        {
            processInfo = default(ProcessInfo);

            try
            {
                if (process == null || process.HasExited)
                {
                    return false;
                }

                process.Refresh();

                string name = process.ProcessName;
                if (string.IsNullOrEmpty(name))
                {
                    return false;
                }

                processInfo = new ProcessInfo
                {
                    processId        = process.Id,
                    processName      = name,
                    mainWindowHandle = process.MainWindowHandle,
                };

                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool UseHideMode()
        {
            return _toggleUseHide != null && _toggleUseHide.isOn;
        }

        private void SetStatus(string message)
        {
            _statusTextContent = message ?? string.Empty;

            if (_statusText != null)
            {
                _statusText.text = "状态：" + _statusTextContent;
            }

            // if (!string.IsNullOrEmpty(message))
            // {
            //     Debug.Log("[进程任务管理器-UGUI] " + message);
            // }
        }

        #region Find Helpers

        private static T FindComponent<T>(Transform root, string objectName, T fallback) where T : Component
        {
            var transform = FindDeep(root, objectName);
            if (transform == null)
            {
                return fallback;
            }

            var component = transform.GetComponent<T>();
            return component != null ? component : fallback;
        }

        private static RectTransform FindRect(Transform root, string objectName)
        {
            var transform = FindDeep(root, objectName);
            return transform as RectTransform;
        }

        private static RectTransform FindDirectChildRect(Transform parent, string childName)
        {
            if (parent == null)
            {
                return null;
            }

            for (int i = 0; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i);
                if (child != null && child.name == childName)
                {
                    return child as RectTransform;
                }
            }

            return null;
        }

        private static RectTransform FindFirstProcessRowTemplate(RectTransform content)
        {
            if (content == null)
            {
                return null;
            }

            for (int i = 0; i < content.childCount; i++)
            {
                var child = content.GetChild(i);
                if (child == null)
                {
                    continue;
                }

                if (child.name.StartsWith("ProcessRow", StringComparison.Ordinal) ||
                    child.name.StartsWith("RuntimeProcessRow", StringComparison.Ordinal) ||
                    child.name.StartsWith("PooledProcessRow", StringComparison.Ordinal))
                {
                    return child as RectTransform;
                }
            }

            return null;
        }

        private static Transform FindDeep(Transform root, string objectName)
        {
            if (root == null || string.IsNullOrEmpty(objectName))
            {
                return null;
            }

            if (root.name == objectName)
            {
                return root;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                var result = FindDeep(root.GetChild(i), objectName);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        #endregion

        private struct ProcessInfo
        {
            public int processId;
            public string processName;
            public IntPtr mainWindowHandle;
        }

        /// <summary>
        /// 进程列表行对象池单元。
        /// 缓存 Text 和 Button，避免每次刷新列表时重复 GetComponentsInChildren。
        /// </summary>
        private sealed class ProcessRowView
        {
            public readonly RectTransform rectTransform;
            public readonly GameObject gameObject;

            private readonly Text[] _cells;
            private readonly Button _btnShow;
            private readonly Button _btnHide;
            private readonly Button _btnKill;
            private readonly Button _btnCopy;
            private readonly Button _btnSearch;

            public bool isActive;

            public ProcessRowView(RectTransform rectTransform)
            {
                this.rectTransform = rectTransform;
                gameObject         = rectTransform.gameObject;

                _cells     = GetProcessRowCells(rectTransform);
                _btnShow   = FindButton(rectTransform, "Show", 0);
                _btnHide   = FindButton(rectTransform, "Hide", 1);
                _btnKill   = FindButton(rectTransform, "Kill", 2);
                _btnCopy   = FindButton(rectTransform, "Copy", 3);
                _btnSearch = FindButton(rectTransform, "Search", 4);
            }

            public void Fill(
                int order,
                ProcessInfo info,
                bool useHideMode,
                Action<ProcessInfo> showAction,
                Action<ProcessInfo> hideAction,
                Action<ProcessInfo> killAction,
                Action<ProcessInfo> copyAction,
                Action<ProcessInfo> searchAction)
            {
                if (_cells.Length >= 4)
                {
                    _cells[0].text = order.ToString();
                    _cells[1].text = info.processId.ToString();
                    _cells[2].text = info.mainWindowHandle.ToString();
                    _cells[3].text = info.processName;
                }

                SetupButton(_btnShow, "显示", () => showAction(info));
                SetupButton(_btnHide, useHideMode ? "隐藏窗口" : "最小化窗口", () => hideAction(info));
                SetupButton(_btnKill, "杀掉", () => killAction(info));
                SetupButton(_btnCopy, "复制名", () => copyAction(info));
                SetupButton(_btnSearch, "搜同名", () => searchAction(info));
            }

            public void Clear()
            {
                ClearButton(_btnShow);
                ClearButton(_btnHide);
                ClearButton(_btnKill);
                ClearButton(_btnCopy);
                ClearButton(_btnSearch);
            }

            private static Text[] GetProcessRowCells(RectTransform row)
            {
                var result = new List<Text>();

                for (int i = 0; i < row.childCount; i++)
                {
                    var child = row.GetChild(i);
                    if (child == null)
                    {
                        continue;
                    }

                    if (child.GetComponent<Button>() != null || child.GetComponentInChildren<Button>(true) != null)
                    {
                        continue;
                    }

                    var text = child.GetComponent<Text>();
                    if (text != null)
                    {
                        result.Add(text);
                    }
                }

                if (result.Count >= 4)
                {
                    return result.ToArray();
                }

                var allTexts = row.GetComponentsInChildren<Text>(true);
                result.Clear();

                for (int i = 0; i < allTexts.Length; i++)
                {
                    var text = allTexts[i];
                    if (text.GetComponentInParent<Button>() != null)
                    {
                        continue;
                    }

                    result.Add(text);
                }

                return result.ToArray();
            }

            private static Button FindButton(RectTransform row, string buttonName, int fallbackIndex)
            {
                var buttons = row.GetComponentsInChildren<Button>(true);

                for (int i = 0; i < buttons.Length; i++)
                {
                    if (buttons[i].name == buttonName)
                    {
                        return buttons[i];
                    }
                }

                return fallbackIndex >= 0 && fallbackIndex < buttons.Length ? buttons[fallbackIndex] : null;
            }

            private static void SetupButton(Button button, string label, UnityEngine.Events.UnityAction action)
            {
                if (button == null)
                {
                    return;
                }

                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(action);

                var text = button.GetComponentInChildren<Text>(true);
                if (text != null)
                {
                    text.text = label;
                }
            }

            private static void ClearButton(Button button)
            {
                if (button != null)
                {
                    button.onClick.RemoveAllListeners();
                }
            }
        }
    }
}
namespace Demos.示例_进程任务管理器.Scripts
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using UnityEngine;
    using Debug = UnityEngine.Debug;

    /// <summary>
    /// OnGUI 版本窗口切换主控制器。
    /// 
    /// 当前版本改动：
    /// 1. UI 支持整体缩放。
    /// 2. 快捷键设置改成 OnGUI 下拉框。
    /// 3. 去掉独立“切换目标”输入区。
    /// 4. 进程列表每一行直接支持：显示、隐藏/最小化、杀掉、复制进程名。
    /// 5. 保留 F2 显示/隐藏 UI、刷新、搜索、窗口最大化/最小化/还原/关闭等功能。
    /// </summary>
    public class WindowControllerOnGUI : MonoBehaviour
    {
        [Header("OnGUI 窗口区域，实际显示会乘以 UI 缩放")] [SerializeField]
        private Rect _windowRect = new(20, 20, 1040, 640);

        [Header("是否显示 OnGUI 主界面")] [SerializeField]
        private bool _showGui = true;

        [Header("UI 整体缩放")] [SerializeField] private float _uiScale = 1.0f;

        [Header("是否只显示有主窗口句柄的进程")] [SerializeField]
        private bool _onlyShowProcessesWithMainWindow = true;

        [Header("隐藏按钮是否完全隐藏窗口，否则执行最小化")] [SerializeField]
        private bool _useHideInsteadOfMinimize;

        [Header("启动时是否自动刷新一次进程列表")] [SerializeField]
        private bool _refreshProcessListOnStart = true;

        [Header("快捷键第一个按键索引")] [SerializeField]
        private int _hotkeyIndex1 = 1;

        [Header("快捷键第二个按键索引，0 表示不设置")] [SerializeField]
        private int _hotkeyIndex2 = 17;

        [Header("进程名称搜索关键字")] [SerializeField] private string _processSearchText = string.Empty;

        private readonly HashSet<int> _pressedKeys = new();
        private readonly List<ProcessInfo> _processInfos = new();

        private KeyboardHook _windowsKeyboardHook;
        private bool _shortcutTriggered;
        private int _hotkeyTriggerCount;
        private string _statusText = "未启动";
        private Vector2 _processScroll;

        private bool _hotkeyDropdown1Open;
        private bool _hotkeyDropdown2Open;
        private Vector2 _dropdownScroll1;
        private Vector2 _dropdownScroll2;

        private GUIStyle _titleStyle;
        private GUIStyle _smallLabelStyle;
        private GUIStyle _headerStyle;
        private GUIStyle _rowStyle;
        private GUIStyle _dropdownButtonStyle;
        private GUIStyle _dropdownBoxStyle;

        private const float MIN_UI_SCALE = 0.75f;
        private const float MAX_UI_SCALE = 1.75f;

        private void Awake()
        {
            Application.runInBackground = true;
            _uiScale                    = Mathf.Clamp(_uiScale, MIN_UI_SCALE, MAX_UI_SCALE);

            if (_refreshProcessListOnStart)
            {
                RefreshProcessList();
            }
        }

        private void OnEnable()
        {
            InstallKeyboardHook();
        }

        private void OnDisable()
        {
            UninstallKeyboardHook();
        }

        private void OnDestroy()
        {
            UninstallKeyboardHook();
        }

        private void OnApplicationQuit()
        {
            UninstallKeyboardHook();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F2))
            {
                _showGui = !_showGui;
            }
        }

        private void OnGUI()
        {
            EnsureStyles();

            Matrix4x4 oldMatrix = GUI.matrix;
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(_uiScale, _uiScale, 1f));

            if (!_showGui)
            {
                DrawCollapsedHint();
                GUI.matrix = oldMatrix;
                return;
            }

            _windowRect = GUI.Window(GetInstanceID(), _windowRect, DrawWindow, "快捷键切换应用 - OnGUI");
            GUI.matrix  = oldMatrix;
        }

        private void DrawCollapsedHint()
        {
            GUILayout.BeginArea(new Rect(12, 12, 380, 30), GUI.skin.box);
            GUILayout.Label("快捷键切换工具已隐藏，按 F2 显示。", _smallLabelStyle);
            GUILayout.EndArea();
        }

        private void DrawWindow(int windowId)
        {
            GUILayout.BeginVertical();

            DrawTopBar();
            GUILayout.Space(6);
            DrawHotkeyAndOptions();
            GUILayout.Space(6);
            DrawProcessToolbar();
            GUILayout.Space(4);
            DrawProcessList();

            GUILayout.EndVertical();

            GUI.DragWindow(new Rect(0, 0, 10000, 22));
        }

        private void DrawTopBar()
        {
            GUILayout.BeginHorizontal();

            GUILayout.Label("快捷键切换应用", _titleStyle, GUILayout.Width(180));
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("最小化本窗口", GUILayout.Width(95)))
            {
                WindowAPI.WindowMinimize();
            }

            if (GUILayout.Button("最大化本窗口", GUILayout.Width(95)))
            {
                WindowAPI.WindowMaximize();
            }

            if (GUILayout.Button("还原本窗口", GUILayout.Width(85)))
            {
                WindowAPI.WindowRestore();
            }

            if (GUILayout.Button("关闭", GUILayout.Width(60)))
            {
                WindowAPI.WindowClose();
            }

            GUILayout.EndHorizontal();

            GUILayout.Label("状态：" + _statusText, _smallLabelStyle);
            GUILayout.Label("提示：按 F2 可显示/隐藏本 OnGUI 面板。快捷键触发时会显示当前鼠标/焦点下的前台窗口操作结果。", _smallLabelStyle);
        }

        private void DrawHotkeyAndOptions()
        {
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label("快捷键与界面设置", _headerStyle);

            GUILayout.BeginHorizontal();

            GUILayout.Label("第一个按键", GUILayout.Width(70));
            _hotkeyIndex1 = DrawKeyDropdown("hotkey1", _hotkeyIndex1, ref _hotkeyDropdown1Open, ref _dropdownScroll1, 170);

            GUILayout.Label("第二个按键", GUILayout.Width(70));
            _hotkeyIndex2 = DrawKeyDropdown("hotkey2", _hotkeyIndex2, ref _hotkeyDropdown2Open, ref _dropdownScroll2, 170);

            GUILayout.Label("当前组合：" + GetHotkeyDisplayText(), GUILayout.Width(230));

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();

            GUILayout.Label("UI缩放", GUILayout.Width(55));
            _uiScale = GUILayout.HorizontalSlider(_uiScale, MIN_UI_SCALE, MAX_UI_SCALE, GUILayout.Width(180));
            _uiScale = Mathf.Clamp(_uiScale, MIN_UI_SCALE, MAX_UI_SCALE);
            GUILayout.Label(_uiScale.ToString("0.00"), GUILayout.Width(45));

            if (GUILayout.Button("重置缩放", GUILayout.Width(80)))
            {
                _uiScale = 1f;
            }

            _useHideInsteadOfMinimize = GUILayout.Toggle(_useHideInsteadOfMinimize, "隐藏按钮使用完全隐藏，否则最小化", GUILayout.Width(230));

            GUILayout.FlexibleSpace();

            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        private int DrawKeyDropdown(string controlId, int selectedIndex, ref bool isOpen, ref Vector2 scroll, float width)
        {
            selectedIndex = Mathf.Clamp(selectedIndex, 0, KeyCodeName.KeyNames.Length - 1);

            GUILayout.BeginVertical(GUILayout.Width(width));

            string buttonText = KeyCodeName.GetKeyNameByIndex(selectedIndex) + " ▼";
            if (GUILayout.Button(buttonText, _dropdownButtonStyle, GUILayout.Width(width), GUILayout.Height(24)))
            {
                isOpen = !isOpen;

                if (controlId == "hotkey1")
                {
                    _hotkeyDropdown2Open = false;
                }
                else
                {
                    _hotkeyDropdown1Open = false;
                }
            }

            if (isOpen)
            {
                GUILayout.BeginVertical(_dropdownBoxStyle, GUILayout.Width(width), GUILayout.Height(180));
                scroll = GUILayout.BeginScrollView(scroll);

                for (int i = 0; i < KeyCodeName.KeyNames.Length; i++)
                {
                    GUIStyle style = i == selectedIndex ? _headerStyle : GUI.skin.button;
                    if (GUILayout.Button(KeyCodeName.KeyNames[i], style, GUILayout.Width(width - 24), GUILayout.Height(22)))
                    {
                        selectedIndex = i;
                        isOpen        = false;
                    }
                }

                GUILayout.EndScrollView();
                GUILayout.EndVertical();
            }

            GUILayout.EndVertical();

            return selectedIndex;
        }

        private void DrawProcessToolbar()
        {
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label("进程列表", _headerStyle);

            GUILayout.BeginHorizontal();

            bool newOnlyShow = GUILayout.Toggle(
                _onlyShowProcessesWithMainWindow,
                "只显示有主窗口句柄的进程",
                GUILayout.Width(180));

            if (newOnlyShow != _onlyShowProcessesWithMainWindow)
            {
                _onlyShowProcessesWithMainWindow = newOnlyShow;
            }

            GUILayout.Label("搜索：", GUILayout.Width(42));
            _processSearchText = GUILayout.TextField(_processSearchText ?? string.Empty, GUILayout.Width(180));

            if (GUILayout.Button("刷新进程列表", GUILayout.Width(100)))
            {
                RefreshProcessList();
            }

            if (GUILayout.Button("复制当前搜索", GUILayout.Width(100)))
            {
                GUIUtility.systemCopyBuffer = _processSearchText ?? string.Empty;
                SetStatus("已复制当前搜索内容：" + GUIUtility.systemCopyBuffer);
            }

            GUILayout.FlexibleSpace();
            GUILayout.Label("数量：" + GetFilteredProcessCount(), GUILayout.Width(70));

            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        private void DrawProcessList()
        {
            GUILayout.BeginVertical(GUI.skin.box);

            GUILayout.BeginHorizontal();
            GUILayout.Label("序号", _headerStyle, GUILayout.Width(45));
            GUILayout.Label("PID", _headerStyle, GUILayout.Width(70));
            GUILayout.Label("主窗口句柄", _headerStyle, GUILayout.Width(120));
            GUILayout.Label("进程名", _headerStyle, GUILayout.Width(250));
            GUILayout.Label("操作", _headerStyle, GUILayout.Width(380));
            GUILayout.EndHorizontal();

            _processScroll = GUILayout.BeginScrollView(_processScroll);

            int visibleIndex = 0;
            for (int i = 0; i < _processInfos.Count; i++)
            {
                ProcessInfo info = _processInfos[i];
                if (!IsProcessVisible(info))
                {
                    continue;
                }

                visibleIndex++;
                DrawProcessRow(visibleIndex, info);
            }

            GUILayout.EndScrollView();

            GUILayout.EndVertical();
        }

        private void DrawProcessRow(int order, ProcessInfo info)
        {
            GUILayout.BeginHorizontal(_rowStyle);

            GUILayout.Label(order.ToString(), GUILayout.Width(45));
            GUILayout.Label(info.processId.ToString(), GUILayout.Width(70));
            GUILayout.Label(info.mainWindowHandle.ToString(), GUILayout.Width(120));
            GUILayout.Label(info.processName, GUILayout.Width(250));

            if (GUILayout.Button("显示", GUILayout.Width(55)))
            {
                ShowProcessWindow(info);
            }

            if (GUILayout.Button(_useHideInsteadOfMinimize ? "隐藏" : "最小化", GUILayout.Width(65)))
            {
                HideOrMinimizeProcessWindow(info);
            }

            if (GUILayout.Button("杀掉", GUILayout.Width(55)))
            {
                KillProcess(info);
            }

            if (GUILayout.Button("复制名", GUILayout.Width(65)))
            {
                CopyProcessName(info);
            }

            if (GUILayout.Button("搜同名", GUILayout.Width(65)))
            {
                _processSearchText = info.processName;
            }

            GUILayout.EndHorizontal();
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

            bool success = _useHideInsteadOfMinimize
                               ? WindowAPI.HideWindow(info.mainWindowHandle)
                               : WindowAPI.MinimizeWindow(info.mainWindowHandle);

            SetStatus(success
                          ? (_useHideInsteadOfMinimize ? "已隐藏：" : "已最小化：") + info.processName
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

        private void InstallKeyboardHook()
        {
            if (_windowsKeyboardHook != null)
            {
                return;
            }

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            try
            {
                _windowsKeyboardHook                 =  new KeyboardHook();
                _windowsKeyboardHook.KeyboardPressed += OnKeyPressed;
                SetStatus("键盘钩子已启动。");
            }
            catch (Exception ex)
            {
                _windowsKeyboardHook = null;
                SetStatus("键盘钩子启动失败：" + ex.Message);
                Debug.LogException(ex);
            }
#else
            SetStatus("当前平台不支持 Windows 全局键盘钩子。");
#endif
        }

        private void UninstallKeyboardHook()
        {
            if (_windowsKeyboardHook == null)
            {
                return;
            }

            _windowsKeyboardHook.KeyboardPressed -= OnKeyPressed;
            _windowsKeyboardHook.Dispose();
            _windowsKeyboardHook = null;
            _pressedKeys.Clear();
            _shortcutTriggered = false;
        }

        private void OnKeyPressed(object sender, GlobalKeyboardHookEventArgs e)
        {
            int code = e.KeyboardData.VirtualCode;

            switch (e.KeyboardState)
            {
                case KeyboardState.KeyDown:
                case KeyboardState.SysKeyDown:
                    _pressedKeys.Add(code);
                    TryTriggerShortcut();
                    break;

                case KeyboardState.KeyUp:
                case KeyboardState.SysKeyUp:
                    _pressedKeys.Remove(code);
                    ResetTriggerIfShortcutReleased();
                    break;
            }
        }

        /// <summary>
        /// 快捷键触发后刷新进程列表。
        /// 因为现在不再维护“显示目标/隐藏目标”，快捷键保留为快速刷新/唤出工具用途。
        /// </summary>
        private void TryTriggerShortcut()
        {
            if (_shortcutTriggered)
            {
                return;
            }

            List<int> requiredKeys = GetRequiredKeys();
            if (requiredKeys.Count == 0)
            {
                return;
            }

            for (int i = 0; i < requiredKeys.Count; i++)
            {
                if (!_pressedKeys.Contains(requiredKeys[i]))
                {
                    return;
                }
            }

            _shortcutTriggered = true;
            _hotkeyTriggerCount++;
            _showGui = true;
            RefreshProcessList();
            SetStatus("快捷键触发：已显示工具并刷新进程列表，次数：" + _hotkeyTriggerCount);
        }

        private void ResetTriggerIfShortcutReleased()
        {
            List<int> requiredKeys = GetRequiredKeys();
            if (requiredKeys.Count == 0)
            {
                _shortcutTriggered = false;
                return;
            }

            for (int i = 0; i < requiredKeys.Count; i++)
            {
                if (!_pressedKeys.Contains(requiredKeys[i]))
                {
                    _shortcutTriggered = false;
                    return;
                }
            }
        }

        private List<int> GetRequiredKeys()
        {
            var result = new List<int>();

            int key1 = KeyCodeName.GetKeyValueByIndex(_hotkeyIndex1);
            int key2 = KeyCodeName.GetKeyValueByIndex(_hotkeyIndex2);

            if (key1 > 0)
            {
                result.Add(key1);
            }

            if (key2 > 0 && key2 != key1)
            {
                result.Add(key2);
            }

            return result;
        }

        private void RefreshProcessList()
        {
            _processInfos.Clear();

            Process[] processArray = new Process[0];

            try
            {
                processArray = Process.GetProcesses();

                for (int i = 0; i < processArray.Length; i++)
                {
                    ProcessInfo info;
                    if (!TryBuildProcessInfo(processArray[i], out info))
                    {
                        continue;
                    }

                    if (_onlyShowProcessesWithMainWindow && info.mainWindowHandle == IntPtr.Zero)
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
        }

        private bool TryBuildProcessInfo(Process process, out ProcessInfo processInfo)
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

        private bool IsProcessVisible(ProcessInfo info)
        {
            if (_onlyShowProcessesWithMainWindow && info.mainWindowHandle == IntPtr.Zero)
            {
                return false;
            }

            if (string.IsNullOrEmpty(_processSearchText))
            {
                return true;
            }

            return info.processName != null &&
                   info.processName.IndexOf(_processSearchText, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private int GetFilteredProcessCount()
        {
            int count = 0;
            for (int i = 0; i < _processInfos.Count; i++)
            {
                if (IsProcessVisible(_processInfos[i]))
                {
                    count++;
                }
            }

            return count;
        }

        private string GetHotkeyDisplayText()
        {
            int key1 = KeyCodeName.GetKeyValueByIndex(_hotkeyIndex1);
            int key2 = KeyCodeName.GetKeyValueByIndex(_hotkeyIndex2);

            string name1 = key1 > 0 ? KeyCodeName.GetKeyNameByIndex(_hotkeyIndex1) : string.Empty;
            string name2 = key2 > 0 && key2 != key1 ? KeyCodeName.GetKeyNameByIndex(_hotkeyIndex2) : string.Empty;

            if (string.IsNullOrEmpty(name1) && string.IsNullOrEmpty(name2))
            {
                return "未设置";
            }

            if (string.IsNullOrEmpty(name2))
            {
                return name1;
            }

            if (string.IsNullOrEmpty(name1))
            {
                return name2;
            }

            return name1 + " + " + name2;
        }

        private void SetStatus(string message)
        {
            _statusText = message ?? string.Empty;

            if (!string.IsNullOrEmpty(message))
            {
                Debug.Log("[快捷键切换应用] " + message);
            }
        }

        private void EnsureStyles()
        {
            if (_titleStyle != null)
            {
                return;
            }

            _titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize  = 16,
                fontStyle = FontStyle.Bold,
                normal    = { textColor = Color.white }
            };

            _smallLabelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                wordWrap = true,
                normal   = { textColor = Color.white }
            };

            _headerStyle = new GUIStyle(GUI.skin.label)
            {
                fontStyle = FontStyle.Bold,
                normal    = { textColor = Color.white }
            };

            _rowStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(2, 2, 2, 2),
            };

            _dropdownButtonStyle = new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleLeft,
                padding   = new RectOffset(8, 8, 2, 2),
            };

            _dropdownBoxStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(4, 4, 4, 4),
            };
        }

        public void OnDragWindow()
        {
            WindowAPI.DragWindowsMethod();
        }

        private struct ProcessInfo
        {
            public int processId;
            public string processName;
            public IntPtr mainWindowHandle;
        }
    }
}
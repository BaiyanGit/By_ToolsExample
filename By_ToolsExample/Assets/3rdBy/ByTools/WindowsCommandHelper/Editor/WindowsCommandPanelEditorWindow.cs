namespace ByTools.WindowsCommandHelper.Editor
{
    using UnityEngine;
    using UnityEditor;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System;
    using Scripts;
    using Debug = UnityEngine.Debug;

    /// <summary>
    /// Windows 命令面板编辑器窗口
    /// </summary>
    public class WindowsCommandPanelEditorWindow : EditorWindow
    {
        private        WindowsCommandPanelCore   _panelCore;
        private static WindowsCommandPanelEditorWindow _window;
        private static float                     _windowHeight;

        private void OnEnable()
        {
            _panelCore = new WindowsCommandPanelCore();
            _panelCore.BuildCategoryMap();
        }

        [MenuItem("ByTools/🧩 Windows 命令面板")]
        public static void ShowWindow()
        {
            _window         = GetWindow<WindowsCommandPanelEditorWindow>("Windows 命令面板");
            _window.minSize = new Vector2(700, 500);
            _windowHeight   = _window.position.height;
        }

        private void OnGUI()
        {
            if (_window)
            {
                _windowHeight = _window.position.height;
            }
            else
            {
                _window = GetWindow<WindowsCommandPanelEditorWindow>("Windows 命令面板");
            }

            EditorGUILayout.HelpBox("全部命令输出可在各命令折叠查看", MessageType.Info);
            GUILayout.Space(5);
            _panelCore.DrawContent();
        }

        private void OpenNotification(string content)
        {
            ShowNotification(new GUIContent(content));
        }

        /// <summary>
        /// 执行命令面板核心逻辑
        /// </summary>
        private class WindowsCommandPanelCore
        {
            private          Vector2                                                  _scrollPos;           // 命令列表滚动位置
            private          Vector2                                                  _outputScroll;        // 输出滚动位置
            private          string                                                   _searchText  = "";    // 搜索文本框内容
            private          List<string>                                             _categories  = new(); // 分类列表
            private readonly Dictionary<string, List<WindowsCommandData.CommandInfo>> _categoryMap = new();
            private          int                                                      _selectedCategoryIndex;   // 当前选择的分类索引
            private          bool                                                     _useAdmin         = true; // 是否使用管理员权限执行命令
            private          bool                                                     _autoScrollOutput = true; // 是否自动滚动输出

            private readonly Dictionary<string, bool> _foldoutMap       = new(); // 命令列表折叠状态
            private readonly Dictionary<string, bool> _selectedMap      = new(); // 命令列表选中状态
            private readonly Dictionary<string, bool> _outputFoldoutMap = new(); // 命令输出折叠状态

            private float _batchProgress;  // 批量执行进度
            private bool  _executingBatch; // 是否正在执行批量命令

            // 输出数据结构
            private class CommandOutput
            {
                public          string           command;
                public readonly List<OutputItem> outputs = new();
            }

            private class OutputItem
            {
                public string text;      // 输出内容
                public bool   isError;   // 是否错误输出
                public string timeStamp; // 输出时间戳
            }

            private readonly List<CommandOutput> _outputList = new(); // 命令输出列表

            public void BuildCategoryMap()
            {
                _categoryMap.Clear();
                _foldoutMap.Clear();
                _selectedMap.Clear();
                _outputFoldoutMap.Clear();

                foreach (var cmd in WindowsCommandData.Commands)
                {
                    var category = "基础系统与通用命令";
                    if (cmd.fullNameChina.Contains("[高级]"))
                        category = "高级系统管理员命令";
                    else if (cmd.fullNameChina.Contains("[工具集]"))
                        category = "工具集（系统管理/网络/磁盘/用户/系统诊断）";
                    else if (cmd.fullNameChina.Contains("[语法]"))
                        category = "基础语法参数";

                    if (!_categoryMap.ContainsKey(category))
                        _categoryMap[category] = new List<WindowsCommandData.CommandInfo>();

                    _categoryMap[category].Add(cmd);

                    _foldoutMap[cmd.command]       = false;
                    _selectedMap[cmd.command]      = false;
                    _outputFoldoutMap[cmd.command] = true;
                }

                _categories = new List<string>(_categoryMap.Keys);
            }

            public void DrawContent()
            {
                GUILayout.BeginVertical();

                // 搜索栏 & 设置
                GUILayout.BeginHorizontal();
                GUILayout.Label("搜索:", GUILayout.Width(40));
                _searchText       = GUILayout.TextField(_searchText);
                _useAdmin         = GUILayout.Toggle(_useAdmin, "管理员权限执行", GUILayout.Width(150));
                _autoScrollOutput = GUILayout.Toggle(_autoScrollOutput, "自动滚动输出", GUILayout.Width(150));
                if (GUILayout.Button("清空输出")) _outputList.Clear();
                GUILayout.EndHorizontal();

                GUILayout.Space(5);

                // 分类 Tab
                _selectedCategoryIndex = GUILayout.Toolbar(_selectedCategoryIndex, _categories.ToArray());

                GUILayout.Space(10);

                #region 命令列表

                // 命令列表
                _scrollPos = GUILayout.BeginScrollView(_scrollPos, GUILayout.Height(_windowHeight - 150));
                if (_categories.Count > 0)
                {
                    var category = _categories[_selectedCategoryIndex];
                    if (_categoryMap.TryGetValue(category, out var value))
                    {
                        foreach (var cmd in value)
                        {
                            if (!string.IsNullOrEmpty(_searchText))
                            {
                                if (!string.IsNullOrEmpty(cmd.command) && !cmd.command.ToLower().Contains(_searchText.ToLower()) &&
                                    !string.IsNullOrEmpty(cmd.fullName) && !cmd.fullName.ToLower().Contains(_searchText.ToLower()) &&
                                    !string.IsNullOrEmpty(cmd.fullNameChina) && !cmd.fullNameChina.ToLower().Contains(_searchText.ToLower()) &&
                                    !string.IsNullOrEmpty(cmd.description) && !cmd.description.ToLower().Contains(_searchText.ToLower())
                                   )
                                    continue;
                            }

                            GUI.backgroundColor = _selectedMap[cmd.command] ? Color.yellow : Color.white;
                            GUI.contentColor    = Color.white;

                            GUILayout.BeginVertical("box");
                            GUILayout.BeginHorizontal();
                            _selectedMap[cmd.command] = GUILayout.Toggle(_selectedMap[cmd.command], "", GUILayout.Width(20));

                            // 输出折叠显示
                            // _foldoutMap[cmd.command] = EditorGUILayout.Foldout(_foldoutMap[cmd.command], $"{cmd.command}", true);
                            _foldoutMap[cmd.command] = EditorGUILayout.Foldout(_foldoutMap[cmd.command], $"{cmd.command}   ({cmd.fullNameChina})", true);

                            var lastRect = GUILayoutUtility.GetLastRect();
                            // 鼠标右键菜单
                            if (Event.current.type == EventType.ContextClick && lastRect.Contains(Event.current.mousePosition))
                            {
                                var menu = new GenericMenu();
                                menu.AddItem(new GUIContent("📋 复制命令"), false, () => CopyCommand(cmd.command));
                                menu.AddItem(new GUIContent("▶️ 打开 CMD 执行"), false, () => ExecuteCommandWithOutput(cmd.command, false));
                                menu.AddItem(new GUIContent("↪️ 打开 PowerShell 执行"), false, () => ExecuteCommandWithOutput("powershell -command " + cmd.command, false));
                                menu.ShowAsContext();
                                Event.current.Use();
                            }

                            GUILayout.EndHorizontal();

                            // 命令详细折叠
                            if (_foldoutMap[cmd.command])
                            {
                                if (!string.IsNullOrEmpty(cmd.fullName)) GUILayout.Label($"全名: {cmd.fullName}");
                                if (!string.IsNullOrEmpty(cmd.description)) GUILayout.Label($"描述: {cmd.description}");
                                if (!string.IsNullOrEmpty(cmd.syntax)) GUILayout.Label($"语法: {cmd.syntax}");
                                if (!string.IsNullOrEmpty(cmd.example)) GUILayout.Label($"例子: \n{cmd.example}");
                                // GUILayout.Label($"运行示例: {cmd.runExample}");
                                GUILayout.Space(5);
                                GUILayout.BeginHorizontal();
                                if (!string.IsNullOrEmpty(cmd.runExample))
                                {
                                    GUILayout.Label("运行示例:", GUILayout.Width(100));
                                    if (string.IsNullOrEmpty(cmd.input))
                                    {
                                        cmd.input = cmd.runExample;
                                    }

                                    cmd.input = GUILayout.TextField(cmd.input);
                                }

                                GUILayout.EndHorizontal();

                                GUILayout.BeginHorizontal();
                                if (GUILayout.Button("📋 复制命令")) CopyCommand(cmd.command);

                                if (!string.IsNullOrEmpty(cmd.input))
                                {
                                    if (GUILayout.Button("▶️ 执行命令")) ExecuteCommandWithOutput(cmd.command, _useAdmin);
                                    if (GUILayout.Button("▶️ 输入命令执行"))
                                    {
                                        if (!string.IsNullOrEmpty(cmd.input))
                                        {
                                            ExecuteCommandWithOutput(cmd.input, _useAdmin);
                                        }
                                        else
                                        {
#if UNITY_EDITOR
                                            GetWindow<WindowsCommandPanelEditorWindow>().OpenNotification("请输入命令....");
#endif
                                        }
                                    }
                                }

                                GUILayout.EndHorizontal();

                                if (!string.IsNullOrEmpty(cmd.runExample))
                                {
                                    // 输出折叠显示
                                    _outputFoldoutMap[cmd.command] = EditorGUILayout.Foldout(_outputFoldoutMap[cmd.command], "命令结果输出", true);
                                    if (_outputFoldoutMap[cmd.command])
                                    {
                                        var co = _outputList.Find(o => o.command == cmd.command);
                                        if (co != null)
                                        {
                                            foreach (var item in co.outputs)
                                            {
                                                var bgColor = item.isError ? new Color(1f, 0.8f, 0.8f) : new Color(0.9f, 0.9f, 0.9f);
                                                var rect    = GUILayoutUtility.GetRect(0, 20, GUILayout.ExpandWidth(true));
                                                EditorGUI.DrawRect(rect, bgColor);
                                                GUI.contentColor = item.isError ? Color.red : Color.black;
                                                // GUI.Label(new Rect(rect.x + 5, rect.y, rect.width - 5, rect.height), $"[{item.timeStamp}] {item.text}");
                                                var textContent = item.text;
                                                if (item.isError)
                                                {
                                                    if (textContent.Equals("The Process object must have the UseShellExecute property set to false in order to redirect IO streams."))
                                                    {
                                                        // 重定向输出 = 程序自己接收命令行输出
                                                        // 不重定向 = 输出直接显示在屏幕上（命令行窗口）
                                                        textContent = "Process对象必须将UseShellExecute属性设置为false，才能重定向IO流。（需：管理员权限执行）";
                                                    }
                                                }

                                                GUI.Label(new Rect(rect.x + 5, rect.y, rect.width - 5, rect.height), $"[{item.timeStamp}] {textContent}");
                                            }
                                        }
                                    }
                                }
                            }

                            GUILayout.EndVertical();
                            GUI.backgroundColor = Color.white;
                            GUILayout.Space(3);
                        }
                    }
                }

                #endregion

                GUILayout.EndScrollView();

                #region 批量执行相关

                GUILayout.Space(10);

                // 批量执行按钮与进度条
                GUILayout.BeginHorizontal();
                if (!_executingBatch && GUILayout.Button("▶️ 执行选中命令", GUILayout.Height(30)))
                {
                    _executingBatch = true;
                    _batchProgress  = 0f;
                    ExecuteSelectedCommandsWithProgress();
                }

                if (_executingBatch)
                {
                    GUILayout.Label("执行中...");
                    var progressRect = GUILayoutUtility.GetRect(200, 30);
                    EditorGUI.ProgressBar(progressRect, _batchProgress, "执行进度");
                }

                GUILayout.EndHorizontal();

                #endregion

                GUILayout.EndVertical();
            }

            private void CopyCommand(string command)
            {
                EditorGUIUtility.systemCopyBuffer = command;
                AppendOutputForCommand("复制命令", false, command);
#if UNITY_EDITOR
                GetWindow<WindowsCommandPanelEditorWindow>().OpenNotification($"已复制 [ {command} ] 到剪贴板");
#endif
            }

            private void ExecuteSelectedCommandsWithProgress()
            {
                var cmdsToExecute = new List<string>();
                foreach (var kv in _selectedMap)
                    if (kv.Value)
                        cmdsToExecute.Add(kv.Key);

                if (cmdsToExecute.Count == 0)
                {
                    _executingBatch = false;
                    return;
                }

                System.Threading.ThreadPool.QueueUserWorkItem(_ =>
                {
                    for (var i = 0; i < cmdsToExecute.Count; i++)
                    {
                        ExecuteCommandWithOutput(cmdsToExecute[i], _useAdmin);
                        _batchProgress = (float)(i + 1) / cmdsToExecute.Count;
                    }

                    _executingBatch = false;
                });
            }

            private void ExecuteCommandWithOutput(string command, bool admin)
            {
                Debug.Log($"执行命令: {command}");
                try
                {
                    var psi = new ProcessStartInfo("cmd.exe", "/c " + command)
                    {
                        RedirectStandardOutput = true,
                        RedirectStandardError  = true,
                        // Tips:
                        // UseShellExecute = true → 程序通过 Shell 打开，会直接显示在命令行窗口，不能被程序捕获。
                        // UseShellExecute = false → 程序自己执行命令，把输出交给Unity的程序，可以用ReadToEnd() 读取。
                        UseShellExecute = !admin,
                        CreateNoWindow  = true
                    };
                    if (admin) psi.Verb = "runas";

                    using var process = Process.Start(psi);
                    if (process != null)
                    {
                        var output = process.StandardOutput.ReadToEnd();
                        var error  = process.StandardError.ReadToEnd();
                        process.WaitForExit();

                        var co = _outputList.Find(o => o.command == command);
                        if (co == null)
                        {
                            co = new CommandOutput { command = command };
                            _outputList.Add(co);
                        }

                        if (!string.IsNullOrEmpty(output))
                            foreach (var line in output.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries))
                                co.outputs.Add(new OutputItem { text = line, isError = false, timeStamp = DateTime.Now.ToString("HH:mm:ss") });

                        if (!string.IsNullOrEmpty(error))
                            foreach (var line in error.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries))
                                co.outputs.Add(new OutputItem { text = line, isError = true, timeStamp = DateTime.Now.ToString("HH:mm:ss") });
                    }
                }
                catch (Exception e)
                {
                    AppendOutputForCommand(command, true, e.Message);
                }
            }

            private void AppendOutputForCommand(string command, bool isError, string text)
            {
                var co = _outputList.Find(o => o.command == command);
                if (co == null)
                {
                    co = new CommandOutput { command = command };
                    _outputList.Add(co);
                }

                co.outputs.Add(new OutputItem { text = text, isError = isError, timeStamp = DateTime.Now.ToString("HH:mm:ss") });
            }
        }
    }
}
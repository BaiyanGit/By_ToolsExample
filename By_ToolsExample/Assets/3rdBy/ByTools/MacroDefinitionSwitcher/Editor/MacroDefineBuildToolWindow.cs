//=====================================================
// 文件名称: MacroDefineBuildToolWindow
// 创 建 者: wangbaiyan
// 创建日期: 2026-4-23
// 描    述: 宏定义切换与打包主窗口。
//=====================================================

namespace MacroDefineBuildToolEditor
{
    using System;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// 宏定义切换与打包主窗口。
    /// </summary>
    public class MacroDefineBuildToolWindow : EditorWindow
    {
        private MacroDefineBuildToolConfig _config;
        private Vector2 _currentDefinesScroll;
        private Vector2 _savedModesScroll;
        private Vector2 _buildSettingsScroll;
        private GUIStyle _defineItemStyle;

        /// <summary>
        /// 打开窗口菜单。
        /// </summary>
        [MenuItem("ByTools/🔖 宏定义切换与打包工具")]
        public static void OpenWindow()
        {
            var window = GetWindow<MacroDefineBuildToolWindow>("宏定义切换与打包工具");
            window.minSize = new Vector2(1050f, 850f);
            window.Show();
        }

        /// <summary>
        /// 重绘所有打开的主窗口。
        /// </summary>
        public static void RepaintAllWindows()
        {
            var windows = Resources.FindObjectsOfTypeAll<MacroDefineBuildToolWindow>();
            foreach (var window in windows)
            {
                window.ReloadConfig();
                window.Repaint();
            }
        }

        private void OnEnable()
        {
            ReloadConfig();
        }

        private void OnFocus()
        {
            ReloadConfig();
        }

        private void ReloadConfig()
        {
            _config = MacroDefineBuildToolStorage.Load();
            _config = MacroDefineBuildToolStorage.FixNullFields(_config);
            MacroDefineBuildToolUtility.SyncCurrentDefineStateFromUnity(_config);
        }

        private void OnGUI()
        {
            if (GUILayout.Button("🔍脚本宏定义扫描", GUILayout.Height(28)))
            {
                MacroDefineScriptScannerWindow.OpenWindow();
            }

            EnsureConfig();
            GUILayout.Space(8);
            _buildSettingsScroll = EditorGUILayout.BeginScrollView(_buildSettingsScroll);
            EditorGUILayout.BeginVertical();
            DrawCurrentDefineListPanel();
            GUILayout.Space(8);
            DrawSavedModesPanel();
            GUILayout.Space(8);
            DrawBuildSettingsPanel();
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// 绘制当前定义列表面板
        /// </summary>
        private void DrawCurrentDefineListPanel()
        {
            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.BeginHorizontal();

            var color = string.IsNullOrEmpty(_config.currentEditingModeId) ? "red" : "green";
            var tip   = string.IsNullOrEmpty(_config.currentEditingModeId) ? "<size=10><color=grey> ( 显示Unity当前生效的宏定义 )</color></size>" : string.Empty;
            bool newFoldout = EditorGUILayout.Foldout(_config.currentDefinesFoldout,
                $"<color={color}>{_config.currentEditingModeName}</color> " + tip + $"<color=cyan>  平台：{_config.currentEditingPlatform}</color>",
                true, EditorStylesEx.FoldoutBold);

            if (newFoldout != _config.currentDefinesFoldout)
            {
                _config.currentDefinesFoldout = newFoldout;
                SaveConfig();
            }

            EditorGUILayout.EndHorizontal();

            if (_config.currentDefinesFoldout)
            {
                _currentDefinesScroll = EditorGUILayout.BeginScrollView(_currentDefinesScroll, GUILayout.Height(150));
                if (_config.currentDefines == null || _config.currentDefines.Count == 0)
                {
                    EditorGUILayout.HelpBox("当前 Unity 没有生效的自定义宏定义。", MessageType.None);
                }
                else
                {
                    for (int i = 0; i < _config.currentDefines.Count; i++)
                    {
                        DrawReadOnlyDefineItem(_config.currentDefines[i], i);
                    }
                }

                EditorGUILayout.EndScrollView();
            }

            GUILayout.Space(4);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("获取系统宏定义", GUILayout.Height(28)))
            {
                ReadCurrentUsedDefines();
            }

            if (GUILayout.Button("创建宏定义模板", GUILayout.Height(28)))
            {
                OpenCreateModeEditorFromCurrentDefines();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// 获取当前可更新的宏定义模板。
        /// </summary>
        /// <param name="item"></param>
        /// <param name="index"></param>
        private static void DrawReadOnlyDefineItem(MacroDefineItem item, int index)
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField($"{index + 1}. {item?.symbol ?? string.Empty}", EditorStylesEx.DefineItemStyle);
            if (!string.IsNullOrWhiteSpace(item?.description))
            {
                EditorGUILayout.LabelField(item.description, EditorStyles.wordWrappedMiniLabel);
            }

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// 打开保存当前宏定义模板弹窗。
        /// </summary>
        private void DrawSavedModesPanel()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("<size=13>已保存宏定义模板</size>", EditorStylesEx.DefineItemStyleBold);

            var isShowList = _config.modes != null && _config.modes.Count != 0;
            var viewHeight = !isShowList ? GUILayout.Height(50) : GUILayout.Height(250);
            _savedModesScroll = EditorGUILayout.BeginScrollView(_savedModesScroll, viewHeight);
            if (!isShowList)
            {
                EditorGUILayout.HelpBox("还没有保存任何宏定义模板。", MessageType.None);
            }
            else
            {
                foreach (var mode in _config.modes.OrderBy(m => m.platform).ThenBy(m => m.modeName))
                {
                    DrawSavedModeItem(mode);
                    GUILayout.Space(4);
                }
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// 绘制已保存宏定义模板项。
        /// </summary>
        /// <param name="mode"></param>
        private void DrawSavedModeItem(MacroDefineMode mode)
        {
            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.BeginHorizontal();
            var foldoutRect = GUILayoutUtility.GetRect(18f, EditorGUIUtility.singleLineHeight, GUILayout.Width(18f));
            mode.isExpanded = EditorGUI.Foldout(foldoutRect, mode.isExpanded, GUIContent.none, false);

            bool   isActive      = MacroDefineBuildToolUtility.IsModeCurrentlyActive(mode);
            string activeMark    = isActive ? "✅ " : string.Empty;
            string color         = isActive ? "green" : "grey";
            string colorPlatform = isActive ? "cyan" : "grey";

            EditorGUILayout.LabelField($"{activeMark} <color={color}>{mode.modeName}</color>", EditorStylesEx.DefineItemStyleBold);
            EditorGUILayout.LabelField($"<color={colorPlatform}>平台：{MacroDefineBuildToolUtility.GetPlatformDisplayName(mode.platform)}</color>", EditorStylesEx.DefineItemStyleBold);

            if (GUILayout.Button("删除", GUILayout.Width(60)))
            {
                DeleteMode(mode);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                return;
            }

            if (GUILayout.Button("应用", GUILayout.Width(60)))
            {
                ApplySavedMode(mode);
            }

            if (GUILayout.Button("查看/编辑", GUILayout.Width(90)))
            {
                MacroDefineModeEditorPopupWindow.Open(mode.id);
            }

            EditorGUILayout.EndHorizontal();

            if (mode.isExpanded)
            {
                GUILayout.Space(2);
                if (mode.defines == null || mode.defines.Count == 0)
                {
                    EditorGUILayout.HelpBox("该模板没有宏定义。", MessageType.None);
                }
                else
                {
                    for (int i = 0; i < mode.defines.Count; i++)
                    {
                        var item = mode.defines[i];
                        EditorGUILayout.BeginVertical("box");
                        EditorGUILayout.LabelField($"{i + 1}. {item.symbol}", EditorStyles.label);
                        if (!string.IsNullOrWhiteSpace(item.description))
                        {
                            EditorGUILayout.LabelField(item.description, EditorStyles.wordWrappedMiniLabel);
                        }

                        EditorGUILayout.EndVertical();
                    }
                }
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawBuildSettingsPanel()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("<size=13>打包设置</size>", EditorStylesEx.DefineItemStyleBold);
            EditorGUILayout.LabelField("当前平台", MacroDefineBuildToolUtility.GetPlatformDisplayName(MacroDefineBuildToolUtility.GetCurrentActivePlatform()));

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("打包路径");
            string newOutputRoot = EditorGUILayout.TextField(_config.buildSettings.outputRoot ?? string.Empty);
            if (newOutputRoot != _config.buildSettings.outputRoot)
            {
                _config.buildSettings.outputRoot = newOutputRoot;
                SaveConfig();
            }

            if (GUILayout.Button("浏览", GUILayout.Width(80)))
            {
                string selected = EditorUtility.OpenFolderPanel("选择打包输出目录", _config.buildSettings.outputRoot, string.Empty);
                if (!string.IsNullOrWhiteSpace(selected))
                {
                    _config.buildSettings.outputRoot = selected;
                    SaveConfig();
                }
            }

            EditorGUILayout.EndHorizontal();

            GUILayout.Space(4);
            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("打包预览");
            EditorGUILayout.HelpBox(MacroDefineBuildToolUtility.ComposeBuildPreviewPath(_config.buildSettings, _config.currentEditingPlatform), MessageType.Info);
            GUILayout.EndHorizontal();

            GUILayout.Space(4);
            EditorGUILayout.LabelField("<size=13>打包选项</size>", EditorStylesEx.DefineItemStyleBold);
            DrawBuildSettingsFields();


            GUILayout.Space(10);
            if (GUILayout.Button("立即打包（使用当前设置）", GUILayout.Height(34)))
            {
                BuildWithCurrentContext();
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawBuildSettingsFields()
        {
            EditorGUI.BeginChangeCheck();
            string buildName = EditorGUILayout.TextField("构建名称", _config.buildSettings.buildName ?? string.Empty);
            GUILayout.BeginHorizontal();
            string buildVersion = EditorGUILayout.TextField("构建版本", _config.buildSettings.buildVersion ?? string.Empty);
            bool   syncVersion  = EditorGUILayout.ToggleLeft("同步到 PlayerSettings.bundleVersion", _config.buildSettings.syncVersionToPlayerSettings);
            GUILayout.EndHorizontal();

            GUILayout.Space(4);
            EditorGUILayout.LabelField("<size=13>高级选项</size>", EditorStylesEx.DefineItemStyleBold);
            GUILayout.BeginHorizontal();

            bool includeTimestamp           = EditorGUILayout.ToggleLeft("包名自动跟随系统时间", _config.buildSettings.includeTimestamp);
            bool includePlatform            = EditorGUILayout.ToggleLeft("平台参与构建名称（平台_项目名）", _config.buildSettings.includePlatformPrefix);
            bool autoRunAfterBuild          = EditorGUILayout.ToggleLeft("构建后自动运行", _config.buildSettings.autoRunAfterBuild);
            bool buildImmediatelyAfterApply = EditorGUILayout.ToggleLeft("切换宏定义后直接打包", _config.buildSettings.buildImmediatelyAfterApply);

            GUILayout.EndHorizontal();
            if (EditorGUI.EndChangeCheck())
            {
                _config.buildSettings.buildName = string.IsNullOrWhiteSpace(buildName)
                                                      ? MacroDefineBuildToolUtility.GetDefaultBuildName()
                                                      : buildName.Trim();
                _config.buildSettings.buildVersion                = buildVersion?.Trim() ?? string.Empty;
                _config.buildSettings.syncVersionToPlayerSettings = syncVersion;
                _config.buildSettings.includeTimestamp            = includeTimestamp;
                _config.buildSettings.includePlatformPrefix       = includePlatform;
                _config.buildSettings.autoRunAfterBuild           = autoRunAfterBuild;
                _config.buildSettings.buildImmediatelyAfterApply  = buildImmediatelyAfterApply;
                SaveConfig();
            }
        }

        private void ReadCurrentUsedDefines()
        {
            MacroDefineBuildToolUtility.SyncCurrentDefineStateFromUnity(_config);
            SaveConfig();
            Repaint();
        }

        private void OpenCreateModeEditorFromCurrentDefines()
        {
            if (!MacroDefineBuildToolUtility.ValidateDefineList(_config.currentDefines, out string error))
            {
                EditorUtility.DisplayDialog("提示", error, "确定");
                return;
            }

            string initialName = string.IsNullOrWhiteSpace(_config.currentEditingModeId)
                                     ? string.Empty
                                     : (_config.currentEditingModeName ?? string.Empty).Trim();

            if (string.Equals(initialName, "未使用任何模板", StringComparison.OrdinalIgnoreCase))
            {
                initialName = string.Empty;
            }

            MacroDefineModeEditorPopupWindow.OpenForCreate(initialName, _config.currentEditingPlatform, MacroDefineBuildToolUtility.CloneDefineList(_config.currentDefines));
        }


        /// <summary>
        /// 应用已保存的宏定义模板。
        /// </summary>
        /// <param name="mode"></param>
        private void ApplySavedMode(MacroDefineMode mode)
        {
            if (mode == null)
            {
                return;
            }

            if (!MacroDefineBuildToolUtility.ValidateDefineList(mode.defines, out string error))
            {
                EditorUtility.DisplayDialog("提示", error, "确定");
                return;
            }

            bool buildAfterApply = _config.buildSettings.buildImmediatelyAfterApply;
            MacroDefineBuildToolUtility.StartApplyPipeline(_config, mode.platform, mode.defines, buildAfterApply, $"保存模板：{mode.modeName}");

            // 刷新
            // EditorUtility.DisplayDialog(
            //     "已开始处理",
            //     buildAfterApply
            //         ? $"已开始应用模板“{mode.modeName}”，并将在编译完成后打包。"
            //         : $"已开始应用模板“{mode.modeName}”。",
            //     "确定");
        }

        private void BuildWithCurrentContext()
        {
            if (!MacroDefineBuildToolUtility.ValidateDefineList(_config.currentDefines, out string error))
            {
                EditorUtility.DisplayDialog("提示", error, "确定");
                return;
            }

            MacroDefineBuildToolUtility.StartApplyPipeline(
                _config,
                _config.currentEditingPlatform,
                _config.currentDefines,
                true,
                "当前设置（立即打包）");

            EditorUtility.DisplayDialog("已开始处理", "已开始执行：切换平台 / 应用宏定义 / 编译完成后打包。", "确定");
        }

        private void DeleteMode(MacroDefineMode mode)
        {
            if (mode == null)
            {
                return;
            }

            bool confirm = EditorUtility.DisplayDialog(
                "删除确认",
                $"确定删除模板：{mode.modeName}？",
                "删除",
                "取消");

            if (!confirm)
            {
                return;
            }

            _config.modes.RemoveAll(m => m.id == mode.id);
            SaveConfig();
            ReloadConfig();
        }

        private void EnsureConfig()
        {
            if (_config == null)
            {
                ReloadConfig();
            }
        }

        private void SaveConfig()
        {
            MacroDefineBuildToolStorage.Save(_config);
        }
    }
}
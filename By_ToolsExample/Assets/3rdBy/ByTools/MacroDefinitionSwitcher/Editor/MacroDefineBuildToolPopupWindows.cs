//=====================================================
// 文件名称: MacroDefineSaveModePopupWindow
// 创 建 者: wangbaiyan
// 创建日期: 2026-4-23
// 描    述: 保存模板命名窗口。
//=====================================================

namespace _3rdBy.ByTools.MacroDefinitionSwitcher.Editor
{
    using System;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// 保存模板命名窗口。
    /// </summary>
    public class MacroDefineSaveModePopupWindow : EditorWindow
    {
        private string _modeName = string.Empty;
        private MacroBuildPlatform _platform = MacroBuildPlatform.Windows;
        private Func<string, MacroBuildPlatform, bool> _confirmHandler;

        public static void Open(string initialModeName, MacroBuildPlatform initialPlatform, Func<string, MacroBuildPlatform, bool> onConfirm)
        {
            var window = CreateInstance<MacroDefineSaveModePopupWindow>();
            window.titleContent    = new GUIContent("命名宏定义列表");
            window.minSize         = new Vector2(420f, 150f);
            window.maxSize         = new Vector2(800f, 220f);
            window._modeName       = initialModeName ?? string.Empty;
            window._platform       = initialPlatform;
            window._confirmHandler = onConfirm;
            window.ShowUtility();
            window.Focus();
        }

        private void OnGUI()
        {
            GUILayout.Space(10);
            EditorGUILayout.LabelField("宏定义列表名称命名", EditorStyles.boldLabel);
            GUILayout.Space(8);

            _modeName = EditorGUILayout.TextField("宏定义模板名称", _modeName ?? string.Empty);
            _platform = (MacroBuildPlatform)EditorGUILayout.EnumPopup("平台", _platform);

            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("确定", GUILayout.Height(28)))
            {
                bool success = _confirmHandler == null || _confirmHandler(_modeName, _platform);
                if (success)
                {
                    Close();
                }
            }

            if (GUILayout.Button("取消", GUILayout.Height(28)))
            {
                Close();
            }

            EditorGUILayout.EndHorizontal();
            GUILayout.Space(8);
        }
    }

    /// <summary>
    /// 单个宏定义新增/修改窗口。
    /// </summary>
    public class MacroDefineEditPopupWindow : EditorWindow
    {
        private string _windowHeader = "新增/修改宏定义";

        // private string _targetLabel = string.Empty;
        private string _symbol = string.Empty;
        private string _description = string.Empty;
        private Func<string, string, bool> _confirmHandler;

        public static void Open(string header, string target, string initialSymbol, string initialDescription, Func<string, string, bool> onConfirm)
        {
            var window = CreateInstance<MacroDefineEditPopupWindow>();
            window.titleContent  = new GUIContent(header ?? "宏定义编辑");
            window.minSize       = new Vector2(460f, 180f);
            window.maxSize       = new Vector2(900f, 260f);
            window._windowHeader = string.IsNullOrWhiteSpace(header) ? "新增/修改宏定义" : header.Trim();
            // window._targetLabel    = target ?? string.Empty;
            window._symbol         = initialSymbol ?? string.Empty;
            window._description    = initialDescription ?? string.Empty;
            window._confirmHandler = onConfirm;
            window.ShowUtility();
            window.Focus();
        }

        private void OnGUI()
        {
            GUILayout.Space(10);
            EditorGUILayout.LabelField(_windowHeader, EditorStyles.boldLabel);
            // if (!string.IsNullOrWhiteSpace(_targetLabel))
            // {
            //     EditorGUILayout.LabelField("当前编辑目标", _targetLabel);
            // }

            GUILayout.Space(8);
            _symbol      = EditorGUILayout.TextField("宏定义", _symbol ?? string.Empty);
            _description = EditorGUILayout.TextField("描述", _description ?? string.Empty);

            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("确定", GUILayout.Height(28)))
            {
                bool success = _confirmHandler == null || _confirmHandler(_symbol, _description);
                if (success)
                {
                    Close();
                }
            }

            if (GUILayout.Button("取消", GUILayout.Height(28)))
            {
                Close();
            }

            EditorGUILayout.EndHorizontal();
            GUILayout.Space(8);
        }
    }

    /// <summary>
    /// 宏定义模板查看/编辑窗口。
    /// </summary>
    public class MacroDefineModeEditorPopupWindow : EditorWindow
    {
        private Vector2 _scroll;
        private MacroDefineMode _draft;

        public static void Open(string modeId)
        {
            var config = MacroDefineBuildToolStorage.Load();
            config = MacroDefineBuildToolStorage.FixNullFields(config);
            var mode = config.modes.FirstOrDefault(m => m.id == modeId);
            if (mode == null)
            {
                EditorUtility.DisplayDialog("提示", "没有找到要编辑的宏定义模板。", "确定");
                return;
            }

            var window = CreateInstance<MacroDefineModeEditorPopupWindow>();
            window.titleContent = new GUIContent("查看/编辑模板");
            window.minSize      = new Vector2(640f, 520f);
            window._draft       = MacroDefineBuildToolUtility.CloneMode(mode);
            window.Show();
            window.Focus();
        }

        /// <summary>
        /// 以指定参数打开一个“新建模板”草稿窗口。
        /// 这里不会直接创建模板，只是把当前宏定义列表带入到查看/编辑模板窗口中。
        /// </summary>
        public static void OpenForCreate(string initialModeName, MacroBuildPlatform initialPlatform, System.Collections.Generic.List<MacroDefineItem> initialDefines)
        {
            var window = CreateInstance<MacroDefineModeEditorPopupWindow>();
            window.titleContent = new GUIContent("查看/编辑模板");
            window.minSize      = new Vector2(640f, 520f);
            window._draft = new MacroDefineMode
            {
                id         = string.Empty,
                modeName   = string.IsNullOrWhiteSpace(initialModeName) ? string.Empty : initialModeName.Trim(),
                platform   = initialPlatform,
                isExpanded = true,
                defines    = MacroDefineBuildToolUtility.CloneDefineList(initialDefines),
            };
            window.Show();
            window.Focus();
        }

        private void OnGUI()
        {
            GUILayout.Space(8);
            string headerText = string.IsNullOrWhiteSpace(_draft?.id) ? "新建 / 编辑模板" : "查看 / 编辑模板";
            EditorGUILayout.LabelField(headerText, EditorStyles.boldLabel);
            GUILayout.Space(6);

            if (_draft == null)
            {
                EditorGUILayout.HelpBox("当前没有可编辑的模板。", MessageType.None);
                if (GUILayout.Button("关闭"))
                {
                    Close();
                }

                return;
            }

            _scroll         = EditorGUILayout.BeginScrollView(_scroll);
            _draft.modeName = EditorGUILayout.TextField("模板名称", _draft.modeName ?? string.Empty);
            _draft.platform = (MacroBuildPlatform)EditorGUILayout.EnumPopup("所属平台", _draft.platform);

            GUILayout.Space(6);
            if (GUILayout.Button("增加宏定义", GUILayout.Height(26)))
            {
                OpenDefineEditor(-1);
            }

            GUILayout.Space(6);
            if (_draft.defines == null || _draft.defines.Count == 0)
            {
                EditorGUILayout.HelpBox("这个模板还没有宏定义。", MessageType.None);
            }
            else
            {
                for (int i = 0; i < _draft.defines.Count; i++)
                {
                    DrawDraftDefineRow(i);
                }
            }

            EditorGUILayout.EndScrollView();

            GUILayout.Space(6);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("保存并应用", GUILayout.Height(28)))
            {
                SaveAndApplyDraft();
            }

            if (GUILayout.Button("保存", GUILayout.Height(28)))
            {
                var success = SaveDraft(true);
                if (success)
                {
                    Close();
                }
            }

            EditorGUILayout.EndHorizontal();
            GUILayout.Space(8);
        }

        private void DrawDraftDefineRow(int index)
        {
            var item = _draft.defines[index];

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"{index + 1}. {item.symbol}", EditorStyles.boldLabel);

            if (GUILayout.Button("删除", GUILayout.Width(60)))
            {
                // if (EditorUtility.DisplayDialog("删除确认", $"确定删除宏定义：{item.symbol}？", "删除", "取消"))
                {
                    _draft.defines.RemoveAt(index);
                    Repaint();
                }

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                return;
            }

            if (GUILayout.Button("修改", GUILayout.Width(60)))
            {
                OpenDefineEditor(index);
            }

            EditorGUILayout.EndHorizontal();

            if (!string.IsNullOrWhiteSpace(item.description))
            {
                EditorGUILayout.LabelField(item.description, EditorStyles.wordWrappedMiniLabel);
            }

            EditorGUILayout.EndVertical();
        }

        private void OpenDefineEditor(int index)
        {
            var    item     = index >= 0 && index < _draft.defines.Count ? _draft.defines[index] : null;
            string winTitle = index >= 0 ? "修改宏定义" : "新增宏定义";

            MacroDefineEditPopupWindow.Open(
                winTitle,
                "查看/编辑窗口中的模板草稿",
                item?.symbol ?? string.Empty,
                item?.description ?? string.Empty,
                (symbol, description) => ConfirmDraftDefineEdit(index, symbol, description));
        }

        private bool ConfirmDraftDefineEdit(int index, string symbol, string description)
        {
            if (!MacroDefineBuildToolUtility.ValidateDefineItem(_draft.defines, symbol, index, out string error))
            {
                EditorUtility.DisplayDialog("提示", error, "确定");
                return false;
            }

            var newItem = new MacroDefineItem
            {
                symbol      = MacroDefineBuildToolUtility.NormalizeSymbol(symbol),
                description = description?.Trim() ?? string.Empty,
            };

            if (index >= 0 && index < _draft.defines.Count)
            {
                _draft.defines[index] = newItem;
            }
            else
            {
                _draft.defines.Add(newItem);
            }

            Repaint();
            return true;
        }

        private bool SaveDraft(bool showDialog)
        {
            if (_draft == null)
            {
                return false;
            }

            string modeName = (_draft.modeName ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(modeName))
            {
                EditorUtility.DisplayDialog("提示", "模板名称不能为空。", "确定");
                return false;
            }

            if (!MacroDefineBuildToolUtility.ValidateDefineList(_draft.defines, out string error))
            {
                EditorUtility.DisplayDialog("提示", error, "确定");
                return false;
            }

            var config = MacroDefineBuildToolStorage.Load();
            config = MacroDefineBuildToolStorage.FixNullFields(config);

            var conflictMode = config.modes.FirstOrDefault(m =>
                m.id != _draft.id &&
                m.platform == _draft.platform &&
                string.Equals(m.modeName, modeName, StringComparison.OrdinalIgnoreCase));

            if (conflictMode != null)
            {
                EditorUtility.DisplayDialog("提示", $"已存在同名同平台模板\n{modeName}\n请修改名称后再保存。", "确定");
                return false;
            }

            if (!MacroDefineBuildToolUtility.ValidateModeConfigurationUnique(config.modes, _draft.id, _draft.platform, _draft.defines, out error))
            {
                EditorUtility.DisplayDialog("提示", error, "确定");
                return false;
            }

            var realMode = config.modes.FirstOrDefault(m => m.id == _draft.id);
            if (realMode == null)
            {
                realMode = MacroDefineBuildToolUtility.CloneMode(_draft);
                if (string.IsNullOrWhiteSpace(realMode.id))
                {
                    realMode.id = Guid.NewGuid().ToString("N");
                }

                realMode.modeName = modeName;
                realMode.platform = _draft.platform;
                realMode.defines  = MacroDefineBuildToolUtility.CloneDefineList(_draft.defines);
                config.modes.Add(realMode);
            }
            else
            {
                realMode.modeName = modeName;
                realMode.platform = _draft.platform;
                realMode.defines  = MacroDefineBuildToolUtility.CloneDefineList(_draft.defines);
            }

            _draft.id = realMode.id;
            MacroDefineBuildToolStorage.Save(config);
            MacroDefineBuildToolWindow.RepaintAllWindows();

            // if (showDialog)
            // {
            //     EditorUtility.DisplayDialog("提示", $"模板【 {modeName} 】已保存。", "确定");
            // }

            return true;
        }

        private void SaveAndApplyDraft()
        {
            if (!SaveDraft(false))
            {
                return;
            }

            var config = MacroDefineBuildToolStorage.Load();
            config = MacroDefineBuildToolStorage.FixNullFields(config);
            var realMode = config.modes.FirstOrDefault(m => m.id == _draft.id);
            if (realMode == null)
            {
                EditorUtility.DisplayDialog("提示", "模板保存成功，但未找到保存后的模板数据。", "确定");
                return;
            }

            bool buildAfterApply = config.buildSettings.buildImmediatelyAfterApply;
            MacroDefineBuildToolUtility.StartApplyPipeline(config, realMode.platform, realMode.defines, buildAfterApply, $"保存并应用模板：{realMode.modeName}");
            MacroDefineBuildToolWindow.RepaintAllWindows();

            EditorUtility.DisplayDialog(
                "已开始处理",
                buildAfterApply
                    ? $"已开始保存并应用模板“{realMode.modeName}”，并将在编译完成后打包。"
                    : $"已开始保存并应用模板“{realMode.modeName}”。",
                "确定");

            Close();
        }
    }
}
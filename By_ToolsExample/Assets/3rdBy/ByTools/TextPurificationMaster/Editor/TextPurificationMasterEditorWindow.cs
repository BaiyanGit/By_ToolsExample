namespace _3rdBy.ByTools.TextPurificationMaster.Editor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using UnityEditor;
    using UnityEngine;
    using Object = UnityEngine.Object;

    /// <summary>
    /// 文本净化大师。
    /// 常用于 TextMeshPro Font Asset 字符集整理：合并多个 txt、去重、过滤无用字符、预览结果、复制结果、覆盖源文件或另存为新文件。
    /// </summary>
    public class TextPurificationMasterEditorWindow : EditorWindow
    {
        #region ===== 枚举 & 数据 =====

        /// <summary>
        /// 输出字符排序模式。
        /// </summary>
        private enum CharacterOrderMode
        {
            [InspectorName("保留首次出现顺序")] KeepFirstAppearOrder,
            [InspectorName("Unicode升序排序")] UnicodeAsc
        }

        /// <summary>
        /// 文件保存编码。
        /// </summary>
        private enum SaveEncodingMode
        {
            [InspectorName("UTF-8 无 BOM")] Utf8NoBom,
            [InspectorName("UTF-8 带 BOM")] Utf8Bom
        }

        /// <summary>
        /// 单次净化统计结果。
        /// </summary>
        [Serializable]
        private class PurificationStats
        {
            [Header("原始字符数")] public int originalCount;
            [Header("正则移除字符数")] public int regexRemovedCount;
            [Header("规则过滤字符数")] public int filteredCount;
            [Header("重复字符数")] public int duplicateCount;
            [Header("最终字符数")] public int resultCount;
            [Header("重复字符预览")] public string duplicatePreview;
            [Header("过滤字符预览")] public string filteredPreview;
        }

        /// <summary>
        /// 编辑器持久化数据。
        /// </summary>
        [Serializable]
        private class PersistentData
        {
            [Header("文件路径列表")] public List<string> filePaths = new List<string>();
            [Header("自定义输入文本")] public string customInputText = string.Empty;
            [Header("是否启用自定义输入")] public bool useCustomInput = true;
            [Header("是否合并文件预览")] public bool mergeFiles = true;
            [Header("是否递归导入文件夹")] public bool importFolderRecursive = true;
            [Header("输出后缀")] public string outputSuffix = "_purified";
            [Header("输出顺序")] public CharacterOrderMode orderMode = CharacterOrderMode.KeepFirstAppearOrder;
            [Header("保存编码")] public SaveEncodingMode saveEncoding = SaveEncodingMode.Utf8NoBom;
            [Header("移除换行")] public bool removeLineBreaks = true;
            [Header("移除空白")] public bool removeWhiteSpaces;
            [Header("移除控制字符")] public bool removeControlCharacters = true;
            [Header("移除 ASCII")] public bool removeAscii;
            [Header("仅保留 CJK")] public bool keepOnlyCjk;
            [Header("大小写合并")] public bool ignoreEnglishCase;
            [Header("启用正则移除")] public bool useRegexRemove;
            [Header("正则移除表达式")] public string regexRemovePattern = string.Empty;
            [Header("排除字符")] public string excludeCharacters = string.Empty;
            [Header("强制包含字符")] public string forceIncludeCharacters = string.Empty;
        }

        #endregion

        #region ===== 常量 =====

        private const float MIN_WIDTH = 920f;
        private const float MIN_HEIGHT = 620f;
        private const float LEFT_PANEL_WIDTH = 360f;
        private const string WINDOW_TITLE = "文本净化大师";
        private const string PERSIST_KEY = "_3rdBy.ByTools.TextPurificationMaster.Editor.TextPurificationMasterEditorWindow.PersistentData.v2";
        private const string TMP_COMMON_PUNCTUATION = "，。！？、；：“”‘’（）《》【】—…·￥,.!?;:'\"()[]<>-_/\\|@#$%^&*+=~` ";

        #endregion

        #region ===== 字段 =====

        [Header("已选择文件路径列表")]
        private readonly List<string> _filePaths = new List<string>();

        [Header("左侧滚动位置")]
        private Vector2 _leftScroll;

        [Header("文件列表滚动位置")]
        private Vector2 _fileListScroll;

        [Header("预览滚动位置")]
        private Vector2 _previewScroll;

        [Header("使用说明滚动位置")]
        private Vector2 _helpScroll;

        [Header("自定义输入文本")]
        private string _customInputText = string.Empty;

        [Header("是否启用自定义输入文本")]
        private bool _useCustomInput = true;

        [Header("是否合并多个文件生成一个结果")]
        private bool _mergeFiles = true;

        [Header("导入文件夹时是否递归扫描子文件夹")]
        private bool _importFolderRecursive = true;

        [Header("输出文件后缀")]
        private string _outputSuffix = "_purified";

        [Header("字符输出顺序")]
        private CharacterOrderMode _orderMode = CharacterOrderMode.KeepFirstAppearOrder;

        [Header("保存编码")]
        private SaveEncodingMode _saveEncoding = SaveEncodingMode.Utf8NoBom;

        [Header("是否移除换行符")]
        private bool _removeLineBreaks = true;

        [Header("是否移除所有空白字符")]
        private bool _removeWhiteSpaces;

        [Header("是否移除控制字符")]
        private bool _removeControlCharacters = true;

        [Header("是否移除 ASCII 字符")]
        private bool _removeAscii;

        [Header("是否仅保留中日韩常用文字")]
        private bool _keepOnlyCjk;

        [Header("英文是否忽略大小写合并")]
        private bool _ignoreEnglishCase;

        [Header("是否启用正则移除")]
        private bool _useRegexRemove;

        [Header("正则移除表达式")]
        private string _regexRemovePattern = string.Empty;

        [Header("排除字符")]
        private string _excludeCharacters = string.Empty;

        [Header("强制包含字符")]
        private string _forceIncludeCharacters = string.Empty;

        [Header("结果预览文本")]
        private string _previewText = string.Empty;

        [Header("状态提示")]
        private string _statusText = "请选择或拖拽 txt 文件，也可以直接在自定义输入中粘贴文字。";

        [Header("状态提示类型")]
        private MessageType _statusType = MessageType.Info;

        [Header("最近输出路径")]
        private string _lastOutputPath = string.Empty;

        [Header("是否已经生成预览")]
        private bool _hasPreview;

        [Header("使用说明折叠")]
        private bool _showHelp = true;

        [Header("高级过滤折叠")]
        private bool _showAdvancedFilter = true;

        [Header("本次净化统计")]
        private PurificationStats _stats = new PurificationStats();

        [Header("标题样式")]
        private GUIStyle _titleStyle;

        [Header("小号富文本样式")]
        private GUIStyle _miniRichStyle;

        [Header("拖拽区域样式")]
        private GUIStyle _dropAreaStyle;

        [Header("路径文本样式")]
        private GUIStyle _pathStyle;

        [Header("预览文本样式")]
        private GUIStyle _previewTextStyle;

        [Header("按钮标题样式")]
        private GUIStyle _sectionTitleStyle;

        #endregion

        #region ===== Window =====

        [MenuItem("ByTools/📄 文本净化大师")]
        private static void ShowEditor()
        {
            var window = GetWindow<TextPurificationMasterEditorWindow>(WINDOW_TITLE);
            window.minSize = new Vector2(MIN_WIDTH, MIN_HEIGHT);
            window.titleContent = new GUIContent(WINDOW_TITLE, "针对 TextMeshPro 字库字符集的文本去重与过滤工具");
            window.Show();
        }

        private void OnEnable()
        {
            InitStyles();
            LoadPersistentData();
        }

        private void OnDisable()
        {
            SavePersistentData();
        }

        private void OnDestroy()
        {
            SavePersistentData();
        }

        private void OnGUI()
        {
            InitStyles();

            using (new EditorGUILayout.VerticalScope())
            {
                DrawHeader();

                using (new EditorGUILayout.HorizontalScope(GUILayout.ExpandHeight(true)))
                {
                    DrawLeftPanel();
                    DrawRightPanel();
                }
            }

            HandleGlobalDragAndDrop();
        }

        #endregion

        #region ===== UI =====

        /// <summary>
        /// 绘制顶部标题与状态统计。
        /// </summary>
        private void DrawHeader()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Label("📄 文本净化大师", _titleStyle);
                    GUILayout.FlexibleSpace();
                    GUILayout.Label($"文件: {_filePaths.Count}  |  原始: {_stats.originalCount}  |  最终: {_stats.resultCount}  |  重复: {_stats.duplicateCount}", EditorStyles.miniBoldLabel);
                }

                EditorGUILayout.HelpBox(_statusText, _statusType);
            }
        }

        /// <summary>
        /// 绘制左侧设置面板。
        /// </summary>
        private void DrawLeftPanel()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox, GUILayout.Width(LEFT_PANEL_WIDTH), GUILayout.ExpandHeight(true)))
            {
                _leftScroll = EditorGUILayout.BeginScrollView(_leftScroll, GUILayout.ExpandHeight(true));

                DrawSourcePanel();
                DrawPurificationSettingsPanel();
                DrawAdvancedFilterPanel();
                DrawHelpPanel();

                EditorGUILayout.EndScrollView();
            }
        }

        /// <summary>
        /// 绘制右侧预览与操作面板。
        /// </summary>
        private void DrawRightPanel()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)))
            {
                DrawActionBar();
                DrawStatsBar();
                DrawPreviewPanel();
            }
        }

        /// <summary>
        /// 绘制文件来源区域。
        /// </summary>
        private void DrawSourcePanel()
        {
            GUILayout.Label("输入来源", _sectionTitleStyle);

            var dropRect = GUILayoutUtility.GetRect(0, 58, GUILayout.ExpandWidth(true));
            GUI.Box(dropRect, "拖拽 .txt / TextAsset 到这里\n支持多个文件，也支持 Project 面板资源", _dropAreaStyle);
            HandleDropArea(dropRect);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("添加Txt", GUILayout.Height(26))) AddTxtFileByPanel();
                if (GUILayout.Button("添加文件夹", GUILayout.Height(26))) AddTxtFolderByPanel();
                if (GUILayout.Button("导入Selection", GUILayout.Height(26))) AddSelectionTextAssets();
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                _importFolderRecursive = EditorGUILayout.ToggleLeft("文件夹递归", _importFolderRecursive, GUILayout.Width(92));
                _mergeFiles = EditorGUILayout.ToggleLeft("多文件合并预览", _mergeFiles);
            }

            DrawFileList();

            _useCustomInput = EditorGUILayout.ToggleLeft("附加自定义输入文本", _useCustomInput);
            using (new EditorGUI.DisabledScope(!_useCustomInput))
            {
                _customInputText = EditorGUILayout.TextArea(_customInputText, GUILayout.MinHeight(64));
            }

            EditorGUILayout.Space(8);
        }

        /// <summary>
        /// 绘制已选择文件列表。
        /// </summary>
        private void DrawFileList()
        {
            using (new EditorGUILayout.VerticalScope("box"))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Label($"文件列表 ({_filePaths.Count})", EditorStyles.boldLabel);
                    GUILayout.FlexibleSpace();

                    using (new EditorGUI.DisabledScope(_filePaths.Count == 0))
                    {
                        if (GUILayout.Button("清空", EditorStyles.miniButton, GUILayout.Width(48)))
                        {
                            _filePaths.Clear();
                            SetStatus("已清空文件列表。", MessageType.Info);
                            SavePersistentData();
                        }
                    }
                }

                _fileListScroll = EditorGUILayout.BeginScrollView(_fileListScroll, GUILayout.Height(96));

                if (_filePaths.Count == 0)
                {
                    GUILayout.Label("暂无文件。", _pathStyle);
                }

                for (var i = 0; i < _filePaths.Count; i++)
                {
                    var path = _filePaths[i];
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        GUILayout.Label((i + 1).ToString("00"), GUILayout.Width(24));
                        if (GUILayout.Button(Path.GetFileName(path), EditorStyles.miniButtonLeft, GUILayout.Width(120)))
                        {
                            RevealFile(path);
                        }

                        EditorGUILayout.SelectableLabel(GetDisplayPath(path), _pathStyle, GUILayout.Height(18));

                        if (GUILayout.Button("×", EditorStyles.miniButtonRight, GUILayout.Width(22)))
                        {
                            _filePaths.RemoveAt(i);
                            SavePersistentData();
                            GUIUtility.ExitGUI();
                        }
                    }
                }

                EditorGUILayout.EndScrollView();
            }
        }

        /// <summary>
        /// 绘制净化设置。
        /// </summary>
        private void DrawPurificationSettingsPanel()
        {
            GUILayout.Label("净化设置", _sectionTitleStyle);

            using (new EditorGUILayout.VerticalScope("box"))
            {
                _orderMode = (CharacterOrderMode)EditorGUILayout.EnumPopup("输出顺序", _orderMode);
                _saveEncoding = (SaveEncodingMode)EditorGUILayout.EnumPopup("保存编码", _saveEncoding);
                _outputSuffix = EditorGUILayout.TextField("另存后缀", _outputSuffix);

                EditorGUILayout.Space(4);
                _removeLineBreaks = EditorGUILayout.ToggleLeft("移除换行符", _removeLineBreaks);
                _removeWhiteSpaces = EditorGUILayout.ToggleLeft("移除所有空白字符", _removeWhiteSpaces);
                _removeControlCharacters = EditorGUILayout.ToggleLeft("移除控制字符", _removeControlCharacters);
                _ignoreEnglishCase = EditorGUILayout.ToggleLeft("英文大小写合并", _ignoreEnglishCase);
                _removeAscii = EditorGUILayout.ToggleLeft("移除 ASCII 字符", _removeAscii);
                _keepOnlyCjk = EditorGUILayout.ToggleLeft("仅保留中日韩常用文字", _keepOnlyCjk);
            }
        }

        /// <summary>
        /// 绘制高级过滤设置。
        /// </summary>
        private void DrawAdvancedFilterPanel()
        {
            _showAdvancedFilter = EditorGUILayout.Foldout(_showAdvancedFilter, "高级过滤", true);
            if (!_showAdvancedFilter) return;

            using (new EditorGUILayout.VerticalScope("box"))
            {
                _excludeCharacters = EditorGUILayout.TextField(new GUIContent("排除字符", "这里填写的字符会强制从结果中移除。"), _excludeCharacters);
                _forceIncludeCharacters = EditorGUILayout.TextField(new GUIContent("强制包含", "这里填写的字符会在最后追加到结果中，适合加入常用标点。"), _forceIncludeCharacters);

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("加入TMP常用标点", EditorStyles.miniButton))
                    {
                        _forceIncludeCharacters = AppendUniqueCharacters(_forceIncludeCharacters, TMP_COMMON_PUNCTUATION);
                        SetStatus("已加入 TMP 常用中英文标点到强制包含列表。", MessageType.Info);
                    }

                    if (GUILayout.Button("清空高级项", EditorStyles.miniButton))
                    {
                        _excludeCharacters = string.Empty;
                        _forceIncludeCharacters = string.Empty;
                        _useRegexRemove = false;
                        _regexRemovePattern = string.Empty;
                    }
                }

                EditorGUILayout.Space(4);
                _useRegexRemove = EditorGUILayout.ToggleLeft("启用正则移除", _useRegexRemove);
                using (new EditorGUI.DisabledScope(!_useRegexRemove))
                {
                    _regexRemovePattern = EditorGUILayout.TextField("正则表达式", _regexRemovePattern);
                    DrawRegexPresetButtons();
                }
            }
        }

        /// <summary>
        /// 绘制常用正则按钮。
        /// </summary>
        private void DrawRegexPresetButtons()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                GUILayout.Label("常用正则预设：", EditorStyles.miniBoldLabel);

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("移除数字", EditorStyles.miniButton)) _regexRemovePattern = "[0-9]";
                    if (GUILayout.Button("移除英文", EditorStyles.miniButton)) _regexRemovePattern = "[A-Za-z]";
                    if (GUILayout.Button("移除空白", EditorStyles.miniButton)) _regexRemovePattern = "\\s+";
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("仅保留中文", EditorStyles.miniButton)) _regexRemovePattern = "[^\\u4e00-\\u9fff]";
                    if (GUILayout.Button("移除中文标点", EditorStyles.miniButton)) _regexRemovePattern = "[，。！？、；：“”‘’（）《》【】]";
                }
            }
        }

        /// <summary>
        /// 绘制使用说明。
        /// </summary>
        private void DrawHelpPanel()
        {
            _showHelp = EditorGUILayout.Foldout(_showHelp, "使用说明", true);
            if (!_showHelp) return;

            _helpScroll = EditorGUILayout.BeginScrollView(_helpScroll, GUILayout.Height(190));
            EditorGUILayout.HelpBox(
                "常用流程：\n" +
                "1. 拖入一个或多个 txt / TextAsset。\n" +
                "2. 点击『生成预览』查看净化后的字符集。\n" +
                "3. 没问题后选择『复制结果』、『覆盖源文件』或『另存结果』。\n\n" +
                "适合 TextMeshPro 字库：\n" +
                "- 多文件合并预览：把多个文本合成一个字库字符集。\n" +
                "- 保留首次出现顺序：适合保持原文本中的字符出现顺序。\n" +
                "- Unicode升序排序：适合生成稳定、方便对比的字符集。\n\n" +
                "正则移除说明：\n" +
                "- [0-9]：移除所有数字。\n" +
                "- [A-Za-z]：移除所有英文。\n" +
                "- \\s+：移除所有空白。\n" +
                "- [^\\u4e00-\\u9fff]：移除非中文字符，也就是只保留中文基础区。\n" +
                "- [，。！？、]：移除指定中文标点。\n\n" +
                "注意：『覆盖源文件』会直接写回原文件；建议先用『另存结果』确认。",
                MessageType.Info);
            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// 绘制右侧操作按钮。
        /// </summary>
        private void DrawActionBar()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.toolbar))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("生成预览", EditorStyles.toolbarButton, GUILayout.Width(82)))
                    {
                        GeneratePreview();
                    }

                    using (new EditorGUI.DisabledScope(!_hasPreview || string.IsNullOrEmpty(_previewText)))
                    {
                        if (GUILayout.Button("复制结果", EditorStyles.toolbarButton, GUILayout.Width(82)))
                        {
                            EditorGUIUtility.systemCopyBuffer = _previewText;
                            SetStatus("已复制净化结果到剪贴板。", MessageType.Info);
                        }

                        if (GUILayout.Button("另存结果", EditorStyles.toolbarButton, GUILayout.Width(82)))
                        {
                            SavePreviewAsFile();
                        }
                    }

                    using (new EditorGUI.DisabledScope(_filePaths.Count == 0))
                    {
                        if (GUILayout.Button("覆盖源文件", EditorStyles.toolbarButton, GUILayout.Width(86)))
                        {
                            ReplaceSourceFiles();
                        }

                        if (GUILayout.Button("按文件另存", EditorStyles.toolbarButton, GUILayout.Width(86)))
                        {
                            SaveEachFileAsCopy();
                        }
                    }

                    GUILayout.FlexibleSpace();

                    using (new EditorGUI.DisabledScope(string.IsNullOrEmpty(_lastOutputPath)))
                    {
                        if (GUILayout.Button("打开输出位置", EditorStyles.toolbarButton, GUILayout.Width(92)))
                        {
                            RevealFile(_lastOutputPath);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 绘制统计条。
        /// </summary>
        private void DrawStatsBar()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    DrawStatLabel("原始", _stats.originalCount);
                    DrawStatLabel("正则移除", _stats.regexRemovedCount);
                    DrawStatLabel("过滤", _stats.filteredCount);
                    DrawStatLabel("重复", _stats.duplicateCount);
                    DrawStatLabel("最终", _stats.resultCount);
                    GUILayout.FlexibleSpace();
                }

                if (!string.IsNullOrEmpty(_stats.duplicatePreview))
                {
                    GUILayout.Label("重复预览：" + _stats.duplicatePreview, _miniRichStyle);
                }

                if (!string.IsNullOrEmpty(_stats.filteredPreview))
                {
                    GUILayout.Label("过滤预览：" + _stats.filteredPreview, _miniRichStyle);
                }
            }
        }

        /// <summary>
        /// 绘制单个统计字段。
        /// </summary>
        private static void DrawStatLabel(string label, int value)
        {
            GUILayout.Label($"{label}: {value}", EditorStyles.miniBoldLabel, GUILayout.Width(86));
        }

        /// <summary>
        /// 绘制预览文本区域。
        /// </summary>
        private void DrawPreviewPanel()
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.ExpandHeight(true)))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Label("结果预览", _sectionTitleStyle);
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(_hasPreview ? $"字符数：{_previewText.Length}" : "未生成预览", EditorStyles.miniLabel);
                }

                _previewScroll = EditorGUILayout.BeginScrollView(_previewScroll, EditorStyles.helpBox, GUILayout.ExpandHeight(true));
                EditorGUILayout.SelectableLabel(string.IsNullOrEmpty(_previewText) ? "点击『生成预览』后，这里会显示净化后的文本。" : _previewText, _previewTextStyle, GUILayout.ExpandHeight(true));
                EditorGUILayout.EndScrollView();
            }
        }

        #endregion

        #region ===== 核心逻辑 =====

        /// <summary>
        /// 生成当前全部输入的预览结果。
        /// </summary>
        private void GeneratePreview()
        {
            try
            {
                var input = BuildMergedInputText();
                _previewText = PurifyText(input, out _stats);
                _hasPreview = true;
                SetStatus("预览已生成。请检查右侧结果，再选择复制、覆盖或另存。", MessageType.Info);
                SavePersistentData();
            }
            catch (Exception ex)
            {
                _previewText = string.Empty;
                _hasPreview = false;
                SetStatus("生成预览失败：" + ex.Message, MessageType.Error);
            }
        }

        /// <summary>
        /// 把已选择文件与自定义输入合并成一个字符串。
        /// </summary>
        private string BuildMergedInputText()
        {
            var builder = new StringBuilder();

            foreach (var path in _filePaths)
            {
                var realPath = GetFileSystemPath(path);
                if (!File.Exists(realPath)) continue;

                builder.Append(File.ReadAllText(realPath));
                builder.Append('\n');
            }

            if (_useCustomInput && !string.IsNullOrEmpty(_customInputText))
            {
                builder.Append(_customInputText);
            }

            return builder.ToString();
        }

        /// <summary>
        /// 按当前设置净化文本。
        /// </summary>
        private string PurifyText(string input, out PurificationStats stats)
        {
            stats = new PurificationStats();
            if (input == null) input = string.Empty;
            stats.originalCount = input.Length;

            var workingText = input;
            if (_useRegexRemove && !string.IsNullOrEmpty(_regexRemovePattern))
            {
                var beforeLength = workingText.Length;
                workingText = Regex.Replace(workingText, _regexRemovePattern, string.Empty);
                stats.regexRemovedCount = Mathf.Max(0, beforeLength - workingText.Length);
            }

            var result = new List<char>();
            var existed = new HashSet<char>();
            var duplicatePreview = new HashSet<char>();
            var filteredPreview = new HashSet<char>();

            foreach (var sourceChar in workingText)
            {
                var currentChar = NormalizeChar(sourceChar);

                if (ShouldFilterChar(currentChar))
                {
                    stats.filteredCount++;
                    if (filteredPreview.Count < 32) filteredPreview.Add(currentChar);
                    continue;
                }

                if (!existed.Add(currentChar))
                {
                    stats.duplicateCount++;
                    if (duplicatePreview.Count < 32) duplicatePreview.Add(currentChar);
                    continue;
                }

                result.Add(currentChar);
            }

            foreach (var forceChar in _forceIncludeCharacters ?? string.Empty)
            {
                var currentChar = NormalizeChar(forceChar);
                if ((_excludeCharacters ?? string.Empty).IndexOf(currentChar) >= 0) continue;
                if (!existed.Add(currentChar)) continue;
                result.Add(currentChar);
            }

            if (_orderMode == CharacterOrderMode.UnicodeAsc)
            {
                result = result.OrderBy(c => c).ToList();
            }

            stats.resultCount = result.Count;
            stats.duplicatePreview = BuildPreviewChars(duplicatePreview);
            stats.filteredPreview = BuildPreviewChars(filteredPreview);
            return new string(result.ToArray());
        }

        /// <summary>
        /// 根据设置归一化单个字符。
        /// </summary>
        private char NormalizeChar(char c)
        {
            if (!_ignoreEnglishCase) return c;
            return char.ToLowerInvariant(c);
        }

        /// <summary>
        /// 判断字符是否需要过滤。
        /// </summary>
        private bool ShouldFilterChar(char c)
        {
            if (!string.IsNullOrEmpty(_excludeCharacters) && _excludeCharacters.IndexOf(c) >= 0) return true;
            if (_removeLineBreaks && (c == '\r' || c == '\n')) return true;
            if (_removeWhiteSpaces && char.IsWhiteSpace(c)) return true;
            if (_removeControlCharacters && char.IsControl(c)) return true;
            if (_removeAscii && c <= 127) return true;
            if (_keepOnlyCjk && !IsCjkChar(c)) return true;
            return false;
        }

        /// <summary>
        /// 覆盖写回源文件。多文件合并模式下会提示，避免误把合并结果写进所有源文件。
        /// </summary>
        private void ReplaceSourceFiles()
        {
            if (_filePaths.Count == 0)
            {
                EditorUtility.DisplayDialog("提示", "请先选择 txt 文件。", "知道了");
                return;
            }

            if (_mergeFiles && _filePaths.Count > 1)
            {
                if (!EditorUtility.DisplayDialog("确认覆盖", "当前开启了多文件合并预览。覆盖源文件会按每个文件单独净化，而不是把合并结果写入所有文件。是否继续？", "继续", "取消"))
                {
                    return;
                }
            }
            else if (!EditorUtility.DisplayDialog("确认覆盖", $"即将覆盖 {_filePaths.Count} 个源文件，此操作不可撤销。是否继续？", "覆盖", "取消"))
            {
                return;
            }

            var successCount = 0;
            foreach (var path in _filePaths)
            {
                var realPath = GetFileSystemPath(path);
                if (!File.Exists(realPath)) continue;

                var input = File.ReadAllText(realPath);
                var output = PurifyText(input, out _);
                File.WriteAllText(realPath, output, GetSaveEncoding());
                _lastOutputPath = realPath;
                successCount++;
            }

            AssetDatabase.Refresh();
            SetStatus($"覆盖完成：成功处理 {successCount} 个源文件。", MessageType.Info);
            GeneratePreview();
        }

        /// <summary>
        /// 每个源文件分别净化并另存为带后缀的新文件。
        /// </summary>
        private void SaveEachFileAsCopy()
        {
            if (_filePaths.Count == 0)
            {
                EditorUtility.DisplayDialog("提示", "请先选择 txt 文件。", "知道了");
                return;
            }

            var successCount = 0;
            foreach (var path in _filePaths)
            {
                var realPath = GetFileSystemPath(path);
                if (!File.Exists(realPath)) continue;

                var input = File.ReadAllText(realPath);
                var output = PurifyText(input, out _);
                var targetPath = BuildCopyFilePath(realPath);
                File.WriteAllText(targetPath, output, GetSaveEncoding());
                _lastOutputPath = targetPath;
                successCount++;
            }

            AssetDatabase.Refresh();
            SetStatus($"另存完成：成功输出 {successCount} 个文件。", MessageType.Info);
        }

        /// <summary>
        /// 将当前预览结果另存为一个指定文件。
        /// </summary>
        private void SavePreviewAsFile()
        {
            if (!_hasPreview) GeneratePreview();
            if (string.IsNullOrEmpty(_previewText)) return;

            var defaultDir = GetDefaultSaveDirectory();
            var defaultName = _filePaths.Count > 0
                                  ? Path.GetFileNameWithoutExtension(GetFileSystemPath(_filePaths[0])) + _outputSuffix + ".txt"
                                  : "PurifiedText.txt";

            var savePath = EditorUtility.SaveFilePanel("另存净化文本", defaultDir, defaultName, "txt");
            if (string.IsNullOrEmpty(savePath)) return;

            File.WriteAllText(savePath, _previewText, GetSaveEncoding());
            _lastOutputPath = savePath;
            AssetDatabase.Refresh();
            SetStatus("已另存结果：" + savePath, MessageType.Info);
        }

        #endregion

        #region ===== 文件导入 =====

        /// <summary>
        /// 打开文件选择面板添加 txt 文件。
        /// </summary>
        private void AddTxtFileByPanel()
        {
            var startDir = _filePaths.Count > 0 ? Path.GetDirectoryName(GetFileSystemPath(_filePaths[0])) : Application.dataPath;
            var path = EditorUtility.OpenFilePanel("选择 txt 文件", startDir, "txt");
            if (string.IsNullOrEmpty(path)) return;
            AddFilePath(path);
        }

        /// <summary>
        /// 打开文件夹选择面板，并导入其中的 txt 文件。
        /// </summary>
        private void AddTxtFolderByPanel()
        {
            var folder = EditorUtility.OpenFolderPanel("选择包含 txt 的文件夹", Application.dataPath, string.Empty);
            if (string.IsNullOrEmpty(folder)) return;

            var option = _importFolderRecursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            var files = Directory.GetFiles(folder, "*.txt", option);
            foreach (var file in files)
            {
                AddFilePath(file);
            }

            SetStatus($"已从文件夹导入 {files.Length} 个 txt 文件。", MessageType.Info);
        }

        /// <summary>
        /// 从 Unity 当前选择导入 TextAsset 或 txt 资源。
        /// </summary>
        private void AddSelectionTextAssets()
        {
            var count = 0;
            foreach (var obj in Selection.objects)
            {
                if (TryAddObjectAsTextFile(obj)) count++;
            }

            SetStatus(count > 0 ? $"已从 Selection 导入 {count} 个文本资源。" : "当前 Selection 中没有可导入的 txt/TextAsset。", count > 0 ? MessageType.Info : MessageType.Warning);
        }

        /// <summary>
        /// 处理全局拖拽，支持拖拽到窗口任意位置。
        /// </summary>
        private void HandleGlobalDragAndDrop()
        {
            var evt = Event.current;
            if (evt == null) return;

            if (evt.type != EventType.DragUpdated && evt.type != EventType.DragPerform) return;

            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            if (evt.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();
                AddDraggedObjects();
            }

            evt.Use();
        }

        /// <summary>
        /// 处理指定拖拽区域。
        /// </summary>
        private void HandleDropArea(Rect rect)
        {
            var evt = Event.current;
            if (evt == null || !rect.Contains(evt.mousePosition)) return;

            if (evt.type == EventType.DragUpdated)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                evt.Use();
            }
            else if (evt.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();
                AddDraggedObjects();
                evt.Use();
            }
        }

        /// <summary>
        /// 添加当前拖拽的对象和路径。
        /// </summary>
        private void AddDraggedObjects()
        {
            var addedCount = 0;

            foreach (var obj in DragAndDrop.objectReferences)
            {
                if (TryAddObjectAsTextFile(obj)) addedCount++;
            }

            foreach (var path in DragAndDrop.paths)
            {
                if (AddFilePath(path)) addedCount++;
            }

            SetStatus(addedCount > 0 ? $"已导入 {addedCount} 个文本文件。" : "没有发现可导入的 txt/TextAsset。", addedCount > 0 ? MessageType.Info : MessageType.Warning);
        }

        /// <summary>
        /// 尝试把 Unity 对象作为文本资源添加。
        /// </summary>
        private bool TryAddObjectAsTextFile(Object obj)
        {
            if (obj == null) return false;

            var assetPath = AssetDatabase.GetAssetPath(obj);
            if (string.IsNullOrEmpty(assetPath)) return false;

            if (obj is TextAsset || string.Equals(Path.GetExtension(assetPath), ".txt", StringComparison.OrdinalIgnoreCase))
            {
                return AddFilePath(assetPath);
            }

            return false;
        }

        /// <summary>
        /// 添加文件路径，自动过滤非 txt、重复、丢失文件。
        /// </summary>
        private bool AddFilePath(string path)
        {
            if (string.IsNullOrEmpty(path)) return false;
            if (Directory.Exists(path)) return false;

            var realPath = GetFileSystemPath(path);
            if (!File.Exists(realPath)) return false;
            if (!string.Equals(Path.GetExtension(realPath), ".txt", StringComparison.OrdinalIgnoreCase)) return false;

            var normalized = NormalizePath(ToUnityRelativePathIfPossible(realPath));
            if (_filePaths.Any(p => string.Equals(NormalizePath(GetFileSystemPath(p)), NormalizePath(realPath), StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }

            _filePaths.Add(normalized);
            SavePersistentData();
            return true;
        }

        #endregion

        #region ===== 持久化 =====

        /// <summary>
        /// 读取上次使用的文件列表和选项。
        /// </summary>
        private void LoadPersistentData()
        {
            var json = EditorPrefs.GetString(PERSIST_KEY, string.Empty);
            if (string.IsNullOrEmpty(json)) return;

            try
            {
                var data = JsonUtility.FromJson<PersistentData>(json);
                if (data == null) return;

                _filePaths.Clear();
                if (data.filePaths != null)
                {
                    foreach (var path in data.filePaths.Where(path => File.Exists(GetFileSystemPath(path))))
                    {
                        _filePaths.Add(path);
                    }
                }

                _customInputText = data.customInputText ?? string.Empty;
                _useCustomInput = data.useCustomInput;
                _mergeFiles = data.mergeFiles;
                _importFolderRecursive = data.importFolderRecursive;
                _outputSuffix = string.IsNullOrEmpty(data.outputSuffix) ? "_purified" : data.outputSuffix;
                _orderMode = data.orderMode;
                _saveEncoding = data.saveEncoding;
                _removeLineBreaks = data.removeLineBreaks;
                _removeWhiteSpaces = data.removeWhiteSpaces;
                _removeControlCharacters = data.removeControlCharacters;
                _removeAscii = data.removeAscii;
                _keepOnlyCjk = data.keepOnlyCjk;
                _ignoreEnglishCase = data.ignoreEnglishCase;
                _useRegexRemove = data.useRegexRemove;
                _regexRemovePattern = data.regexRemovePattern ?? string.Empty;
                _excludeCharacters = data.excludeCharacters ?? string.Empty;
                _forceIncludeCharacters = data.forceIncludeCharacters ?? string.Empty;
            }
            catch (Exception ex)
            {
                SetStatus("读取上次配置失败：" + ex.Message, MessageType.Warning);
            }
        }

        /// <summary>
        /// 保存当前文件列表和选项。
        /// </summary>
        private void SavePersistentData()
        {
            var data = new PersistentData
            {
                filePaths = _filePaths.ToList(),
                customInputText = _customInputText,
                useCustomInput = _useCustomInput,
                mergeFiles = _mergeFiles,
                importFolderRecursive = _importFolderRecursive,
                outputSuffix = _outputSuffix,
                orderMode = _orderMode,
                saveEncoding = _saveEncoding,
                removeLineBreaks = _removeLineBreaks,
                removeWhiteSpaces = _removeWhiteSpaces,
                removeControlCharacters = _removeControlCharacters,
                removeAscii = _removeAscii,
                keepOnlyCjk = _keepOnlyCjk,
                ignoreEnglishCase = _ignoreEnglishCase,
                useRegexRemove = _useRegexRemove,
                regexRemovePattern = _regexRemovePattern,
                excludeCharacters = _excludeCharacters,
                forceIncludeCharacters = _forceIncludeCharacters
            };

            EditorPrefs.SetString(PERSIST_KEY, JsonUtility.ToJson(data));
        }

        #endregion

        #region ===== 工具方法 =====

        /// <summary>
        /// 设置当前状态提示。
        /// </summary>
        private void SetStatus(string text, MessageType type)
        {
            _statusText = text;
            _statusType = type;
            Repaint();
        }

        /// <summary>
        /// 构建另存文件路径。
        /// </summary>
        private string BuildCopyFilePath(string sourcePath)
        {
            var dir = Path.GetDirectoryName(sourcePath) ?? string.Empty;
            var fileName = Path.GetFileNameWithoutExtension(sourcePath);
            var suffix = string.IsNullOrEmpty(_outputSuffix) ? "_purified" : _outputSuffix;
            return Path.Combine(dir, fileName + suffix + ".txt");
        }

        /// <summary>
        /// 获取默认保存目录。
        /// </summary>
        private string GetDefaultSaveDirectory()
        {
            if (_filePaths.Count == 0) return Application.dataPath;

            var realPath = GetFileSystemPath(_filePaths[0]);
            return Path.GetDirectoryName(realPath) ?? Application.dataPath;
        }

        /// <summary>
        /// 获取保存编码。
        /// </summary>
        private Encoding GetSaveEncoding()
        {
            return _saveEncoding == SaveEncodingMode.Utf8Bom ? new UTF8Encoding(true) : new UTF8Encoding(false);
        }

        /// <summary>
        /// 判断字符是否为常用中日韩文字范围。
        /// </summary>
        private static bool IsCjkChar(char c)
        {
            return c >= 0x4E00 && c <= 0x9FFF ||
                   c >= 0x3400 && c <= 0x4DBF ||
                   c >= 0xF900 && c <= 0xFAFF ||
                   c >= 0x3040 && c <= 0x30FF ||
                   c >= 0xAC00 && c <= 0xD7AF;
        }

        /// <summary>
        /// 构建字符预览字符串。
        /// </summary>
        private static string BuildPreviewChars(IEnumerable<char> chars)
        {
            if (chars == null) return string.Empty;
            var text = new string(chars.ToArray());
            return string.IsNullOrEmpty(text) ? string.Empty : text;
        }

        /// <summary>
        /// 向已有字符串中追加不重复字符。
        /// </summary>
        private static string AppendUniqueCharacters(string source, string append)
        {
            if (source == null) source = string.Empty;
            if (append == null) append = string.Empty;

            var set = new HashSet<char>(source);
            var builder = new StringBuilder(source);
            foreach (var c in append)
            {
                if (!set.Add(c)) continue;
                builder.Append(c);
            }

            return builder.ToString();
        }

        /// <summary>
        /// 获取用于 File IO 的真实路径。
        /// </summary>
        private static string GetFileSystemPath(string path)
        {
            if (string.IsNullOrEmpty(path)) return string.Empty;
            path = NormalizePath(path);

            if (path.StartsWith("Assets/", StringComparison.Ordinal))
            {
                var projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? Application.dataPath;
                return NormalizePath(Path.Combine(projectRoot, path));
            }

            return NormalizePath(path);
        }

        /// <summary>
        /// 如果文件位于 Assets 下，则转成 Unity 相对路径。
        /// </summary>
        private static string ToUnityRelativePathIfPossible(string fullPath)
        {
            fullPath = NormalizePath(Path.GetFullPath(fullPath));
            var assetsPath = NormalizePath(Application.dataPath);

            if (fullPath.StartsWith(assetsPath, StringComparison.OrdinalIgnoreCase))
            {
                return "Assets" + fullPath.Substring(assetsPath.Length);
            }

            return fullPath;
        }

        /// <summary>
        /// 统一路径分隔符。
        /// </summary>
        private static string NormalizePath(string path)
        {
            return string.IsNullOrEmpty(path) ? string.Empty : path.Replace("\\", "/");
        }

        /// <summary>
        /// 获取显示路径。
        /// </summary>
        private static string GetDisplayPath(string path)
        {
            return string.IsNullOrEmpty(path) ? string.Empty : NormalizePath(path);
        }

        /// <summary>
        /// 在系统文件管理器中定位文件。
        /// </summary>
        private static void RevealFile(string path)
        {
            var realPath = GetFileSystemPath(path);
            if (string.IsNullOrEmpty(realPath)) return;
            EditorUtility.RevealInFinder(realPath);
        }

        #endregion

        #region ===== 样式 =====

        /// <summary>
        /// 初始化 GUI 样式，避免 OnGUI 中重复创建。
        /// </summary>
        private void InitStyles()
        {
            _titleStyle ??= new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 18,
                alignment = TextAnchor.MiddleLeft
            };

            _miniRichStyle ??= new GUIStyle(EditorStyles.miniLabel)
            {
                richText = true,
                wordWrap = true
            };

            _dropAreaStyle ??= new GUIStyle(EditorStyles.helpBox)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                wordWrap = true
            };

            _pathStyle ??= new GUIStyle(EditorStyles.miniLabel)
            {
                wordWrap = false,
                normal = { textColor = EditorGUIUtility.isProSkin ? new Color(0.72f, 0.72f, 0.72f) : new Color(0.32f, 0.32f, 0.32f) }
            };

            _previewTextStyle ??= new GUIStyle(EditorStyles.textArea)
            {
                wordWrap = true,
                richText = false,
                fontSize = 13,
                padding = new RectOffset(8, 8, 8, 8)
            };

            _sectionTitleStyle ??= new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 13,
                alignment = TextAnchor.MiddleLeft
            };
        }

        #endregion
    }
}

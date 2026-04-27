namespace _3rdBy.ByTools.ObjectsRename.Editor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using Object = UnityEngine.Object;

    /// <summary>
    /// 对象批量重命名工具。
    /// 支持 Scene 对象与 Project 资源混合拖拽、从当前选择导入、预览、校验、撤销 Scene 对象改名。
    /// </summary>
    public class BatchRenameEditorWindow : EditorWindow
    {
        #region ===== 枚举 & 数据 =====

        /// <summary>
        /// 重命名模式。
        /// </summary>
        private enum RenameMode
        {
            [InspectorName("手动改名")] ManualModify,
            [InspectorName("名称替换")] Replace,
            [InspectorName("文本追加/插入")] AppendText,
            [InspectorName("自动序号")] AutoIndex,
            [InspectorName("查找替换")] FindReplace,
            [InspectorName("正则替换")] RegexReplace,
            [InspectorName("清理名称")] CleanupName
        }

        /// <summary>
        /// 文本追加/插入模式下的插入位置。
        /// </summary>
        private enum AppendPosition
        {
            [InspectorName("前缀")] Prefix,
            [InspectorName("中间")] Insert,
            [InspectorName("后缀")] Suffix
        }

        /// <summary>
        /// 自动序号格式。
        /// </summary>
        private enum IndexFormat
        {
            [InspectorName("阿拉伯数字")] Arabic,
            [InspectorName("中文数字")] ChineseLower,
            [InspectorName("中文大写数字")] ChineseUpper,
            [InspectorName("英文字母小写")] EnglishLower,
            [InspectorName("英文字母大写")] EnglishUpper
        }

        /// <summary>
        /// 右侧对象列表排序方式。
        /// </summary>
        private enum SortMode
        {
            [InspectorName("添加顺序")] AddOrder,
            [InspectorName("原名升序")] OriginalNameAsc,
            [InspectorName("原名降序")] OriginalNameDesc,
            [InspectorName("路径升序")] PathAsc,
            [InspectorName("Scene优先")] SceneFirst,
            [InspectorName("Project优先")] ProjectFirst
        }

        /// <summary>
        /// 右侧对象列表来源筛选。
        /// </summary>
        private enum SourceFilter
        {
            [InspectorName("全部")] All,
            [InspectorName("Scene对象")] SceneOnly,
            [InspectorName("Project资源")] ProjectOnly
        }

        /// <summary>
        /// 单行重命名校验状态。
        /// </summary>
        private enum RenameState
        {
            None,
            Changed,
            EmptyName,
            DuplicateName,
            InvalidAssetName,
            MissingObject,
            RegexError
        }

        /// <summary>
        /// 单个待重命名对象的运行时数据。
        /// 说明：这里同时保存原名、新名、来源路径和校验状态，方便 UI 直接显示。
        /// </summary>
        [Serializable]
        private class ObjectInfo
        {
            [Header("对象引用 - Scene 对象或 Project 资源")]
            public Object obj;

            [Header("原始名称 - 当前真实对象名称")] public string originalName;

            [Header("新名称 - 预览生成或手动输入的目标名称")] public string newName;

            [Header("显示路径 - Scene 层级路径或 Project 资源路径")]
            public string path;

            [Header("来源标记 - true 表示 Scene 对象，false 表示 Project 资源")]
            public bool isSceneObject;

            [Header("添加顺序 - 用于排序和显示序号")] public int addOrder;

            [Header("校验状态 - 决定行颜色和是否允许应用")] public RenameState state;

            [Header("状态说明 - 错误或提示文本")] public string stateMessage;

            [Header("层级显示 - 分组ID，含子节点导入时父子对象共享")] public string groupId;

            [Header("层级显示 - 父分组ID，子对象指向父对象分组")] public string parentGroupId;

            [Header("层级显示 - 是否为折叠父项")] public bool isGroupRoot;

            [Header("层级显示 - 是否为子项")] public bool isGroupChild;

            [Header("层级显示 - 相对父项缩进层级")] public int hierarchyDepth;

            [Header("层级显示 - 子项序号，从1开始")] public int childIndex;

            [Header("层级显示 - 父项是否展开")] public bool isExpanded = true;
        }

        /// <summary>
        /// 持久化数据根节点。
        /// 说明：保存到 EditorPrefs 中，用于下次打开窗口时恢复对象列表。
        /// </summary>
        [Serializable]
        private class PersistentData
        {
            [Header("下一个添加顺序值")] public int nextAddOrder;

            [Header("持久化对象列表")] public List<PersistentObjectInfo> objects = new();
        }

        /// <summary>
        /// 单个对象的持久化记录。
        /// 说明：优先使用 GlobalObjectId 恢复对象，失败后再使用资源路径或显示路径兜底。
        /// </summary>
        [Serializable]
        private class PersistentObjectInfo
        {
            [Header("GlobalObjectId - 优先恢复对象使用")] public string globalObjectId;

            [Header("资源路径 - Project 资源兜底恢复使用")] public string assetPath;

            [Header("显示路径 - Scene 对象兜底恢复使用")] public string displayPath;

            [Header("对象名称 - 调试和显示用")] public string objectName;

            [Header("未应用的新名称 - 恢复上次手动输入或预览结果")] public string newName;

            [Header("来源标记 - true 表示 Scene 对象，false 表示 Project 资源")]
            public bool isSceneObject;

            [Header("层级显示 - 分组ID")] public string groupId;

            [Header("层级显示 - 父分组ID")] public string parentGroupId;

            [Header("层级显示 - 是否为折叠父项")] public bool isGroupRoot;

            [Header("层级显示 - 是否为子项")] public bool isGroupChild;

            [Header("层级显示 - 相对父项缩进层级")] public int hierarchyDepth;

            [Header("层级显示 - 子项序号")] public int childIndex;

            [Header("层级显示 - 父项是否展开")] public bool isExpanded;
        }

        #endregion

        #region ===== 字段 =====

        [Header("窗口设置 - 最小宽度")] private const float MIN_WIDTH = 860f;

        [Header("窗口设置 - 最小高度")] private const float MIN_HEIGHT = 620f;

        [Header("窗口设置 - 标题")] private const string WINDOW_TITLE = "对象批量重命名";

        [Header("Undo 设置 - Scene 对象撤销名称")] private const string UNDO_NAME = "批量重命名对象";

        [Header("持久化设置 - EditorPrefs 键")] private const string PERSIST_KEY = "_3rdBy.ByTools.ObjectsRename.Editor.BatchRenameEditorWindow.PersistentObjects.v2";

        [Header("列表布局 - 固定层级序号列宽")] private const float NUMBER_COLUMN_WIDTH = 154f;

        [Header("列表布局 - 移除按钮列宽")] private const float REMOVE_BUTTON_WIDTH = 24f;

        [Header("列表布局 - 层级折叠按钮宽度")] private const float TREE_FOLDOUT_WIDTH = 18f;

        [Header("列表布局 - 层级标签宽度")] private const float TREE_LEVEL_WIDTH = 34f;

        [Header("列表布局 - 同级序号宽度")] private const float TREE_INDEX_WIDTH = 48f;

        [Header("列表布局 - 子节点数量宽度")] private const float TREE_CHILD_COUNT_WIDTH = 46f;

        [Header("使用说明 - 基础流程")] private const string BASIC_HELP_TEXT =
            "常用流程：\n" +
            "1. 单独修改名字：选择『手动改名』→ 直接编辑右侧『新名称』列 → 点击『应用重命名』。\n" +
            "2. 批量规则改名：选择规则模式 → 点击『生成预览』→ 检查右侧红色错误 → 点击『应用重命名』。\n" +
            "3. 『生成预览』只会改右侧新名称，不会真正改对象名；『还原新名称』只会把右侧新名称恢复为原名称。\n" +
            "4. 开启『含子节点』后点击『导入当前选择』，对象会按真实层级折叠显示；每一层都会从 [01] 重新编号。\n" +
            "5. 子节点很多或层级很深时，列表不会继续向右缩进；固定层级列会显示 L0/L1/L2、同级序号和子节点数量，路径行会显示完整层级序号路径。\n\n" +
            "Project 资源重命名会调用 AssetDatabase.RenameAsset；Scene 对象会记录 Undo，可以使用 Ctrl + Z 撤销。";

        [Header("使用说明 - 正则替换详细说明")] private const string REGEX_HELP_TEXT =
            "正则替换使用说明：\n" +
            "• 正则表达式：填写匹配规则，例如 \\d+$ 表示匹配末尾数字。\n" +
            "• 替换为：填写替换后的内容，可以使用 $1、$2、$3 引用括号分组。\n" +
            "• 常用符号：\\d = 数字，\\s = 空白，.+ = 任意内容，^ = 开头，$ = 结尾。\n" +
            "• 如果需要匹配括号本身，请写成 \\( 和 \\)。\n\n" +
            "常用示例：\n" +
            "1. 删除末尾数字：正则表达式 \\d+$，替换为空。\n" +
            "2. Name_001 改成 Name-001：正则表达式 ^(.+?)_(\\d+)$，替换为 $1-$2。\n" +
            "3. 删除 Unity Clone 文本：正则表达式 \\s*\\(Clone\\)$，替换为空。\n" +
            "4. 删除所有空白：正则表达式 \\s+，替换为空。\n" +
            "5. 只保留括号中的内容：正则表达式 ^.*\\((.*?)\\).*$，替换为 $1。";

        [Header("运行时数据 - 待处理对象列表")] private readonly List<ObjectInfo> _items = new();

        [Header("运行时数据 - 下一个添加顺序")] private int _nextAddOrder;

        [Header("滚动位置 - 对象列表")] private Vector2 _listScroll;

        [Header("滚动位置 - 使用说明")] private Vector2 _helpScroll;

        [Header("重命名设置 - 当前模式")] private RenameMode _renameMode = RenameMode.ManualModify;

        [Header("列表筛选 - 来源筛选")] private SourceFilter _sourceFilter = SourceFilter.All;

        [Header("列表排序 - 当前排序模式")] private SortMode _sortMode = SortMode.AddOrder;

        [Header("列表搜索 - 关键字")] private string _searchKeyword = string.Empty;

        [Header("应用选项 - 修改规则时自动预览")] private bool _autoPreview = true;

        [Header("应用选项 - 应用后选中成功对象")] private bool _selectAfterApply = true;

        [Header("应用选项 - 点击对象时定位 Ping")] private bool _pingOnClick = true;

        [Header("界面选项 - 是否展开使用说明")] private bool _showHelp;

        [Header("界面选项 - 是否显示未变化对象")] private bool _showUnchanged = true;

        [Header("导入选项 - 导入当前选择时是否包含子节点")] private bool _includeChildrenWhenImportSelection;

        [Header("持久化状态 - 是否已经读取过持久化数据")] private bool _persistentDataLoaded;

        [Header("预览状态 - 当前是否已经生成规则预览")] private bool _hasPreviewGenerated;

        [Header("预览状态 - 提示文本")] private string _previewStatusText = "手动改名模式下，直接修改右侧『新名称』列后点击应用即可；规则模式下，生成预览只会改右侧新名称，不会真正重命名。";

        [Header("预览状态 - 提示类型")] private MessageType _previewStatusType = MessageType.Info;

        [Header("持久化状态 - 底部提示文本")] private string _persistentStatusText = "对象列表会自动保存；下次打开窗口会恢复上次列表中仍能找到的对象。";

        //============================== 通用序号设置 ==============================

        [Header("通用序号 - 起始序号")] private int _startNumber = 1;

        [Header("通用序号 - 递增步长")] private int _numberStep = 1;

        [Header("通用序号 - 数字补零位数")] private int _padding = 2;

        [Header("通用序号 - 序号分隔符")] private string _indexSeparator = "_";

        //============================== 名称替换模式 ==============================

        [Header("名称替换 - 基础名称")] private string _replaceName = "NewName";

        [Header("名称替换 - 是否追加序号")] private bool _replaceAppendIndex = true;

        [Header("名称替换 - 序号格式")] private IndexFormat _replaceIndexFormat = IndexFormat.Arabic;

        //============================== 追加/插入模式 ==============================

        [Header("追加/插入 - 追加文本")] private string _appendText = "_";

        [Header("追加/插入 - 插入位置")] private AppendPosition _appendPosition = AppendPosition.Suffix;

        [Header("追加/插入 - 中间插入索引")] private int _insertIndex;

        [Header("追加/插入 - 是否追加自动序号")] private bool _appendUseIndex = true;

        [Header("追加/插入 - 自动序号格式")] private IndexFormat _appendIndexFormat = IndexFormat.Arabic;

        //============================== 自动序号模式 ==============================

        [Header("自动序号 - 前缀")] private string _indexPrefix = "Object";

        [Header("自动序号 - 后缀")] private string _indexSuffix = string.Empty;

        [Header("自动序号 - 序号格式")] private IndexFormat _indexOnlyFormat = IndexFormat.Arabic;

        //============================== 查找替换模式 ==============================

        [Header("查找替换 - 查找文本")] private string _findText = string.Empty;

        [Header("查找替换 - 替换文本")] private string _replaceText = string.Empty;

        [Header("查找替换 - 是否忽略大小写")] private bool _ignoreCase;

        //============================== 正则替换模式 ==============================

        [Header("正则替换 - 正则表达式")] private string _regexPattern = string.Empty;

        [Header("正则替换 - 替换为")] private string _regexReplacement = string.Empty;

        [Header("正则替换 - 是否忽略大小写")] private bool _regexIgnoreCase;

        //============================== 清理名称模式 ==============================

        [Header("清理名称 - 去掉首尾空格")] private bool _trimSpaces = true;

        [Header("清理名称 - 移除所有空格")] private bool _removeSpaces;

        [Header("清理名称 - 多个空格压缩成一个")] private bool _collapseSpaces = true;

        [Header("清理名称 - 移除 Unity Clone 文本")] private bool _removeCloneText = true;

        [Header("清理名称 - 移除资源非法文件名字符")] private bool _removeInvalidFileChars = true;

        //============================== GUI 样式缓存 ==============================

        [Header("GUI 样式 - 标题")] private GUIStyle _titleStyle;

        [Header("GUI 样式 - 副标题")] private GUIStyle _subTitleStyle;

        [Header("GUI 样式 - 小号富文本")] private GUIStyle _miniRichStyle;

        [Header("GUI 样式 - 拖拽区域")] private GUIStyle _dropAreaStyle;

        [Header("GUI 样式 - 普通行")] private GUIStyle _rowStyle;

        [Header("GUI 样式 - 变更行")] private GUIStyle _changedRowStyle;

        [Header("GUI 样式 - 错误行")] private GUIStyle _errorRowStyle;

        [Header("GUI 样式 - 父级折叠行")] private GUIStyle _groupRootRowStyle;

        [Header("GUI 样式 - 子对象行")] private GUIStyle _childRowStyle;

        [Header("GUI 样式 - 折叠箭头")] private GUIStyle _foldoutStyle;

        [Header("GUI 样式 - 层级标签")] private GUIStyle _treeLevelStyle;

        [Header("GUI 样式 - 层级序号")] private GUIStyle _treeIndexStyle;

        [Header("GUI 样式 - 子节点数量")] private GUIStyle _treeChildCountStyle;

        [Header("GUI 样式 - 层级路径")] private GUIStyle _treePathStyle;

        [Header("GUI 样式 - 路径文本")] private GUIStyle _pathStyle;

        [Header("GUI 样式 - 工具栏搜索框")] private GUIStyle _toolbarSearchStyle;

        [Header("GUI 样式 - 状态文本")] private GUIStyle _stateTextStyle;

        #endregion

        #region ===== Window =====

        [MenuItem("ByTools/🌀 对象批量重命名")]
        private static void Open()
        {
            var window = GetWindow<BatchRenameEditorWindow>(WINDOW_TITLE);
            window.minSize      = new Vector2(MIN_WIDTH, MIN_HEIGHT);
            window.titleContent = new GUIContent(WINDOW_TITLE, "Scene / Project 对象批量重命名工具");
            window.Show();
        }

        private void OnEnable()
        {
            InitStyles();
            LoadPersistentObjects();
        }

        private void OnDisable()
        {
            SavePersistentObjects();
        }

        private void OnDestroy()
        {
            SavePersistentObjects();
        }

        private void OnGUI()
        {
            InitStyles();

            using (new EditorGUILayout.VerticalScope())
            {
                DrawHeader();
                DrawDropArea();

                using (new EditorGUILayout.HorizontalScope(GUILayout.ExpandHeight(true)))
                {
                    DrawLeftSettingsPanel();
                    DrawRightListPanel();
                }
            }
        }

        #endregion

        #region ===== UI =====

        private void DrawHeader()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Label("🌀 对象批量重命名", _titleStyle);
                    GUILayout.FlexibleSpace();
                    GUILayout.Label($"总数: {_items.Count}  |  变更: {GetChangedCount()}  |  错误: {GetErrorCount()}", EditorStyles.miniBoldLabel);
                }

                GUILayout.Label("支持 Scene 对象和 Project 资源混合处理。建议先预览，确认无红色错误后再应用。", _miniRichStyle);
            }
        }

        private void DrawDropArea()
        {
            var rect = GUILayoutUtility.GetRect(0, 52, GUILayout.ExpandWidth(true));
            GUI.Box(rect, "拖拽 Scene 对象 / Project 资源到这里，或使用右侧对象列表工具栏导入当前选择", _dropAreaStyle);

            var evt = Event.current;
            if (!rect.Contains(evt.mousePosition)) return;

            if (evt.type == EventType.DragUpdated)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                evt.Use();
            }
            else if (evt.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();
                AddObjects(DragAndDrop.objectReferences, false);
                PreviewRename(false);
                evt.Use();
            }
        }

        private void DrawLeftSettingsPanel()
        {
            var leftWidth = Mathf.Clamp(position.width * 0.36f, 300f, 360f);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox, GUILayout.Width(leftWidth), GUILayout.ExpandHeight(true)))
            {
                GUILayout.Label("重命名规则", _subTitleStyle);

                EditorGUI.BeginChangeCheck();
                _renameMode = (RenameMode)EditorGUILayout.EnumPopup("模式", _renameMode);

                EditorGUILayout.Space(4);
                DrawModeSettings();

                if (_renameMode != RenameMode.ManualModify)
                {
                    EditorGUILayout.Space(8);
                    GUILayout.Label("序号设置", EditorStyles.boldLabel);
                    _startNumber    = Mathf.Max(0, EditorGUILayout.IntField("起始序号", _startNumber));
                    _numberStep     = Mathf.Max(1, EditorGUILayout.IntField("递增步长", _numberStep));
                    _padding        = Mathf.Clamp(EditorGUILayout.IntField("数字补零位数", _padding), 0, 12);
                    _indexSeparator = EditorGUILayout.TextField("序号分隔符", _indexSeparator);
                }

                EditorGUILayout.Space(8);
                GUILayout.Label("应用选项", EditorStyles.boldLabel);
                _autoPreview      = EditorGUILayout.ToggleLeft("修改规则时自动预览", _autoPreview);
                _showUnchanged    = EditorGUILayout.ToggleLeft("显示未变化对象", _showUnchanged);
                _selectAfterApply = EditorGUILayout.ToggleLeft("应用后选中成功对象", _selectAfterApply);
                _pingOnClick      = EditorGUILayout.ToggleLeft("点击对象时定位 Ping", _pingOnClick);

                if (EditorGUI.EndChangeCheck())
                {
                    if (_renameMode == RenameMode.ManualModify)
                    {
                        ValidateAllItems();
                        SetPreviewStatus("当前为手动改名模式：请在右侧『新名称』列逐项编辑，点击『应用重命名』后才会真正改名。", MessageType.Info, false);
                    }
                    else if (_autoPreview)
                    {
                        PreviewRename(false);
                    }
                }

                GUILayout.FlexibleSpace();

                using (new EditorGUILayout.HorizontalScope())
                {
                    var previewButtonText = _renameMode == RenameMode.ManualModify ? "校验手动名称" : "生成预览";
                    var previewButtonTip = _renameMode == RenameMode.ManualModify
                                               ? "校验右侧手动填写的新名称，不会真正重命名。"
                                               : "按照左侧规则计算新名称，只修改右侧『新名称』列，不会真正重命名。";
                    if (GUILayout.Button(new GUIContent(previewButtonText, previewButtonTip), GUILayout.Height(30))) PreviewRename(true);
                    if (GUILayout.Button(new GUIContent("还原新名称", "把右侧『新名称』列恢复为原名称，不会影响真实对象。"), GUILayout.Height(30))) ResetPreviewNames();
                }

                DrawPreviewStatusBox();

                _showHelp = EditorGUILayout.Foldout(_showHelp, "使用说明", true);
                if (_showHelp)
                {
                    _helpScroll = EditorGUILayout.BeginScrollView(_helpScroll, GUILayout.Height(220));
                    EditorGUILayout.HelpBox(BASIC_HELP_TEXT, MessageType.Info);
                    EditorGUILayout.HelpBox(REGEX_HELP_TEXT, MessageType.None);
                    EditorGUILayout.EndScrollView();
                }
            }
        }

        private void DrawModeSettings()
        {
            using (new EditorGUILayout.VerticalScope("box"))
            {
                switch (_renameMode)
                {
                    case RenameMode.ManualModify:
                        EditorGUILayout.HelpBox("直接在右侧对象列表的『新名称』列逐个修改名称。这个模式不会根据左侧规则覆盖你手动输入的内容。", MessageType.Info);
                        break;

                    case RenameMode.Replace:
                        _replaceName        = EditorGUILayout.TextField("基础名称", _replaceName);
                        _replaceAppendIndex = EditorGUILayout.Toggle("追加序号", _replaceAppendIndex);
                        if (_replaceAppendIndex)
                        {
                            _replaceIndexFormat = (IndexFormat)EditorGUILayout.EnumPopup("序号格式", _replaceIndexFormat);
                        }

                        break;

                    case RenameMode.AppendText:
                        _appendText     = EditorGUILayout.TextField("追加文本", _appendText);
                        _appendPosition = (AppendPosition)EditorGUILayout.EnumPopup("位置", _appendPosition);
                        if (_appendPosition == AppendPosition.Insert)
                        {
                            _insertIndex = Mathf.Max(0, EditorGUILayout.IntField("插入索引", _insertIndex));
                        }

                        _appendUseIndex = EditorGUILayout.Toggle("追加自动序号", _appendUseIndex);
                        if (_appendUseIndex)
                        {
                            _appendIndexFormat = (IndexFormat)EditorGUILayout.EnumPopup("序号格式", _appendIndexFormat);
                        }

                        break;

                    case RenameMode.AutoIndex:
                        _indexPrefix     = EditorGUILayout.TextField("前缀", _indexPrefix);
                        _indexOnlyFormat = (IndexFormat)EditorGUILayout.EnumPopup("序号格式", _indexOnlyFormat);
                        _indexSuffix     = EditorGUILayout.TextField("后缀", _indexSuffix);
                        break;

                    case RenameMode.FindReplace:
                        _findText    = EditorGUILayout.TextField("查找", _findText);
                        _replaceText = EditorGUILayout.TextField("替换为", _replaceText);
                        _ignoreCase  = EditorGUILayout.Toggle("忽略大小写", _ignoreCase);
                        break;

                    case RenameMode.RegexReplace:
                        _regexPattern     = EditorGUILayout.TextField(new GUIContent("正则表达式", "填写 .NET Regex 匹配规则，例如 \\d+$ 表示匹配末尾数字。"), _regexPattern);
                        _regexReplacement = EditorGUILayout.TextField(new GUIContent("替换为", "支持 $1、$2、$3 引用括号捕获的分组。"), _regexReplacement);
                        _regexIgnoreCase  = EditorGUILayout.Toggle("忽略大小写", _regexIgnoreCase);

                        EditorGUILayout.Space(4);
                        EditorGUILayout.HelpBox("常用：\\d+$ 删除末尾数字；^(.+?)_(\\d+)$ + $1-$2 可把 Name_001 改成 Name-001；\\s*\\(Clone\\)$ 可删除 Clone 后缀。", MessageType.None);
                        DrawRegexPresetButtons();
                        break;

                    case RenameMode.CleanupName:
                        _trimSpaces             = EditorGUILayout.ToggleLeft("去掉首尾空格", _trimSpaces);
                        _collapseSpaces         = EditorGUILayout.ToggleLeft("多个空格压缩成一个", _collapseSpaces);
                        _removeSpaces           = EditorGUILayout.ToggleLeft("移除所有空格", _removeSpaces);
                        _removeCloneText        = EditorGUILayout.ToggleLeft("移除 Unity Clone 文本", _removeCloneText);
                        _removeInvalidFileChars = EditorGUILayout.ToggleLeft("移除资源非法文件名字符", _removeInvalidFileChars);
                        break;
                }
            }
        }

        /// <summary>
        /// 绘制正则替换常用示例按钮。
        /// 说明：只负责填入表达式和替换内容，不会立即真实改名；如果开启自动预览，外层变更检测会自动刷新预览。
        /// </summary>
        private void DrawRegexPresetButtons()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                GUILayout.Label("正则常用示例", EditorStyles.miniBoldLabel);

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button(new GUIContent("删除末尾数字", @"正则：\d+$，替换为空。"), EditorStyles.miniButton))
                    {
                        SetRegexPreset(@"\d+$", string.Empty);
                    }

                    if (GUILayout.Button(new GUIContent("删除(Clone)", @"正则：\s*\(Clone\)$，替换为空。"), EditorStyles.miniButton))
                    {
                        SetRegexPreset(@"\s*\(Clone\)$", string.Empty);
                    }
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button(new GUIContent("Name_001 → Name-001", @"正则：^(.+?)_(\d+)$，替换：$1-$2。"), EditorStyles.miniButton))
                    {
                        SetRegexPreset(@"^(.+?)_(\d+)$", "$1-$2");
                    }

                    if (GUILayout.Button(new GUIContent("删除所有空白", @"正则：\s+，替换为空。"), EditorStyles.miniButton))
                    {
                        SetRegexPreset(@"\s+", string.Empty);
                    }
                }

                if (GUILayout.Button(new GUIContent("只保留括号中的内容", @"正则：^.*\((.*?)\).*$，替换：$1。"), EditorStyles.miniButton))
                {
                    SetRegexPreset(@"^.*\((.*?)\).*$", "$1");
                }
            }
        }

        /// <summary>
        /// 设置正则替换预设。
        /// </summary>
        /// <param name="pattern">正则表达式。</param>
        /// <param name="replacement">替换文本。</param>
        private void SetRegexPreset(string pattern, string replacement)
        {
            _regexPattern     = pattern;
            _regexReplacement = replacement ?? string.Empty;
            GUI.changed       = true;
        }

        /// <summary>
        /// 绘制右侧对象列表。
        /// 说明：列表数据仍然是扁平的，只有显示层根据含子节点导入信息做父子折叠。
        /// </summary>
        private void DrawRightListPanel()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)))
            {
                var visibleItems = GetDisplayItems().ToList();

                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Label("对象列表", _subTitleStyle);
                    GUILayout.FlexibleSpace();
                    GUILayout.Label($"显示: {visibleItems.Count} / {_items.Count}", EditorStyles.miniLabel);
                }

                DrawListActionBar();
                DrawListHeader();

                _listScroll = EditorGUILayout.BeginScrollView(_listScroll, GUILayout.ExpandHeight(true));

                if (visibleItems.Count == 0)
                {
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.HelpBox(_items.Count == 0 ? "暂无对象。请拖拽对象到窗口，或点击『导入当前选择』。" : "当前筛选条件下没有对象。", MessageType.None);
                }

                var displayIndexMap = BuildDisplayIndexMap(visibleItems);
                foreach (var info in visibleItems)
                {
                    // 顶层对象按照当前显示顺序重新编号，避免移除后再次导入出现序号自增。
                    // 层级子对象使用各自父级下的 siblingIndex，每一层都会从 [01] 开始。
                    var rowDisplayIndex = displayIndexMap.TryGetValue(info, out var index) ? index : 1;
                    DrawItemRow(info, rowDisplayIndex, displayIndexMap);
                }

                EditorGUILayout.EndScrollView();
            }
        }

        /// <summary>
        /// 绘制对象列表工具栏。
        /// 说明：原 DrawTopBar 中的导入、筛选、排序、搜索等功能都移动到这里，
        /// 让所有列表相关功能集中在右侧对象列表模块中。
        /// </summary>
        private void DrawListActionBar()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                GUILayout.Label(_persistentStatusText, EditorStyles.miniLabel);

                using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
                {
                    GUILayout.Label("筛选：", GUILayout.Width(40));
                    _sourceFilter = (SourceFilter)EditorGUILayout.EnumPopup(_sourceFilter, EditorStyles.toolbarPopup, GUILayout.Width(92));

                    GUILayout.Label("排序：", GUILayout.Width(40));
                    var newSort = (SortMode)EditorGUILayout.EnumPopup(_sortMode, EditorStyles.toolbarPopup, GUILayout.Width(100));
                    if (newSort != _sortMode)
                    {
                        _sortMode = newSort;
                        SortItems();
                    }

                    GUILayout.Space(8);
                    GUILayout.Label("搜索", GUILayout.Width(30));
                    _searchKeyword = GUILayout.TextField(_searchKeyword, _toolbarSearchStyle, GUILayout.MinWidth(120));
                    if (GUILayout.Button("×", EditorStyles.toolbarButton, GUILayout.Width(22)))
                    {
                        _searchKeyword = string.Empty;
                        GUI.FocusControl(null);
                    }
                }

                GUILayout.Space(3);

                using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
                {
                    // GUILayout.FlexibleSpace();
                    if (GUILayout.Button("导入选中对象", EditorStyles.toolbarButton, GUILayout.Width(96)))
                    {
                        AddObjects(Selection.objects, _includeChildrenWhenImportSelection);
                        PreviewRename(false);
                    }

                    _includeChildrenWhenImportSelection = GUILayout.Toggle(_includeChildrenWhenImportSelection, "含子节点", EditorStyles.toolbarButton, GUILayout.Width(96));

                    if (GUILayout.Button("恢复原文件名", EditorStyles.toolbarButton, GUILayout.Width(96)))
                    {
                        RefreshOriginalNames();
                        PreviewRename(false);
                    }

                    if (GUILayout.Button("移除丢失", EditorStyles.toolbarButton, GUILayout.Width(96)))
                    {
                        _items.RemoveAll(i => i.obj == null);
                        if (_items.Count == 0) _nextAddOrder = 0;
                        PreviewRename(false);
                        SavePersistentObjects();
                    }

                    if (GUILayout.Button("清空列表", EditorStyles.toolbarButton, GUILayout.Width(96)))
                    {
                        _items.Clear();
                        _nextAddOrder = 0;
                        SetPreviewStatus("列表已清空，同时会清除下次打开时恢复的对象列表。", MessageType.Info, false);
                        SavePersistentObjects();
                    }

                    using (new EditorGUI.DisabledScope(_items.Count == 0 || GetErrorCount() > 0 || GetChangedCount() == 0))
                    {
                        var oldColor = GUI.backgroundColor;
                        GUI.backgroundColor = new Color(0.35f, 0.85f, 0.45f);
                        if (GUILayout.Button("应用重命名", GUILayout.Height(20), GUILayout.Width(118))) ApplyRename();
                        GUI.backgroundColor = oldColor;
                    }

                    GUILayout.FlexibleSpace();
                }
            }
        }

        private void DrawListHeader()
        {
            GetListColumnWidths(out var objectWidth, out var originalWidth, out var stateWidth);

            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUILayout.Label("层级 / 序号", GUILayout.Width(NUMBER_COLUMN_WIDTH));
                GUILayout.Label("对象", GUILayout.Width(objectWidth));
                GUILayout.Label("原名称", GUILayout.Width(originalWidth));
                GUILayout.Label("新名称（预览/可手动改）", GUILayout.MinWidth(100));
                GUILayout.Label("状态", GUILayout.Width(stateWidth));
                GUILayout.Space(REMOVE_BUTTON_WIDTH + 4f);
            }
        }

        /// <summary>
        /// 绘制单个对象行。
        /// 说明：含子节点导入时，层级关系显示在固定层级列中，不再无限向右缩进。
        /// </summary>
        private void DrawItemRow(ObjectInfo info, int displayIndex, IReadOnlyDictionary<ObjectInfo, int> displayIndexMap)
        {
            var style = info.state switch
            {
                RenameState.EmptyName or RenameState.DuplicateName or RenameState.InvalidAssetName or RenameState.MissingObject or RenameState.RegexError => _errorRowStyle,
                RenameState.Changed                                                                                                                       => _changedRowStyle,
                _ when IsHierarchyTopLevel(info)                                                                                                          => _groupRootRowStyle,
                _ when IsHierarchyChild(info)                                                                                                             => _childRowStyle,
                _                                                                                                                                         => _rowStyle
            };

            GetListColumnWidths(out var objectWidth, out var originalWidth, out var stateWidth);

            using (new EditorGUILayout.VerticalScope(style))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    DrawIndexOrFoldout(info, displayIndex, displayIndexMap);

                    // 子对象的缩进只放在序号列中，避免对象名称列被挤压。
                    using (new EditorGUILayout.HorizontalScope(GUILayout.Width(objectWidth)))
                    {
                        using (new EditorGUI.DisabledScope(info.obj == null))
                        {
                            var content = GetObjectContent(info);
                            if (GUILayout.Button(content, GUILayout.Width(Mathf.Max(80f, objectWidth)), GUILayout.Height(20)))
                            {
                                SelectAndPing(info.obj);
                            }
                        }
                    }

                    EditorGUILayout.SelectableLabel(info.originalName ?? string.Empty, GUILayout.Width(originalWidth), GUILayout.Height(18));

                    EditorGUI.BeginChangeCheck();
                    info.newName = EditorGUILayout.TextField(info.newName, GUILayout.MinWidth(100));
                    if (EditorGUI.EndChangeCheck())
                    {
                        ValidateAllItems();
                        SavePersistentObjects();
                        SetPreviewStatus("已手动修改右侧『新名称』列。点击『应用重命名』后才会真正改名。", MessageType.Info, _renameMode != RenameMode.ManualModify);
                    }

                    GUILayout.Label(GetStateText(info), _stateTextStyle, GUILayout.Width(stateWidth));

                    if (GUILayout.Button("×", GUILayout.Width(REMOVE_BUTTON_WIDTH), GUILayout.Height(20)))
                    {
                        RemoveItem(info);
                    }
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Space(NUMBER_COLUMN_WIDTH + 4f);
                    GUILayout.Label(GetSourceText(info), _pathStyle, GUILayout.Width(70));
                    if (IsHierarchyItem(info))
                    {
                        GUILayout.Label(GetHierarchyNumberPath(info, displayIndex, displayIndexMap), _treePathStyle, GUILayout.Width(170));
                    }

                    EditorGUILayout.SelectableLabel(GetDisplayPath(info), _pathStyle, GUILayout.Height(17));

                    if (GUILayout.Button("复制路径", EditorStyles.miniButton, GUILayout.Width(64)))
                    {
                        EditorGUIUtility.systemCopyBuffer = GetDisplayPath(info);
                    }
                }

                if (!string.IsNullOrEmpty(info.stateMessage))
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        GUILayout.Space(NUMBER_COLUMN_WIDTH + 4f);
                        GUILayout.Label(info.stateMessage, _pathStyle);
                    }
                }
            }
        }

        /// <summary>
        /// 绘制固定层级序号列。
        /// 说明：不再根据深度无限缩进，避免子对象很多或层级很深时把名称列挤出视野。
        /// 固定列中依次显示：折叠按钮、层级标签 L0/L1、当前同级序号、直接子节点数量。
        /// </summary>
        private void DrawIndexOrFoldout(ObjectInfo info, int displayIndex, IReadOnlyDictionary<ObjectInfo, int> displayIndexMap)
        {
            var cellRect = GUILayoutUtility.GetRect(
                NUMBER_COLUMN_WIDTH,
                EditorGUIUtility.singleLineHeight,
                GUILayout.Width(NUMBER_COLUMN_WIDTH),
                GUILayout.Height(EditorGUIUtility.singleLineHeight));

            var hasChildren = HasHierarchyChildren(info);
            var foldoutRect = new Rect(cellRect.x, cellRect.y, TREE_FOLDOUT_WIDTH, cellRect.height);
            var levelRect   = new Rect(foldoutRect.xMax + 2f, cellRect.y, TREE_LEVEL_WIDTH, cellRect.height);
            var indexRect   = new Rect(levelRect.xMax + 2f, cellRect.y, TREE_INDEX_WIDTH, cellRect.height);
            var countRect   = new Rect(indexRect.xMax + 2f, cellRect.y, TREE_CHILD_COUNT_WIDTH, cellRect.height);

            var tooltip = IsHierarchyItem(info)
                              ? $"层级：L{Mathf.Max(0, info.hierarchyDepth)}\n序号路径：{GetHierarchyNumberPath(info, displayIndex, displayIndexMap)}\n父级：{GetHierarchyParentName(info)}"
                              : "普通对象";

            if (hasChildren)
            {
                EditorGUI.BeginChangeCheck();
                info.isExpanded = EditorGUI.Foldout(
                    foldoutRect,
                    info.isExpanded,
                    GUIContent.none,
                    true,
                    _foldoutStyle);

                if (EditorGUI.EndChangeCheck())
                {
                    SavePersistentObjects();
                }
            }
            else
            {
                GUI.Label(foldoutRect, new GUIContent("•", "叶子节点，没有子对象"), _treeIndexStyle);
            }

            var levelText = IsHierarchyItem(info) ? $"L{Mathf.Max(0, info.hierarchyDepth)}" : "—";
            GUI.Label(levelRect, new GUIContent(levelText, tooltip), _treeLevelStyle);
            GUI.Label(indexRect, new GUIContent(GetDisplayIndexText(info, displayIndex), tooltip), _treeIndexStyle);

            if (hasChildren)
            {
                var childCount = GetDirectHierarchyChildCount(info);
                GUI.Label(countRect, new GUIContent($"子{childCount}", $"直接子对象数量：{childCount}"), _treeChildCountStyle);
            }
            else
            {
                GUI.Label(countRect, GUIContent.none, _treeChildCountStyle);
            }
        }

        /// <summary>
        /// 获取列表序号显示文本。
        /// 普通顶层对象显示 [01]、[02]；每个父级下的子对象也从 [01] 重新开始。
        /// </summary>
        private static string GetDisplayIndexText(ObjectInfo info, int displayIndex)
        {
            if (info == null) return "[-]";
            var index = IsHierarchyChild(info) ? Mathf.Max(1, info.childIndex) : Mathf.Max(1, displayIndex);
            return $"[{index:00}]";
        }

        private void GetListColumnWidths(out float objectWidth, out float originalWidth, out float stateWidth)
        {
            var leftWidth  = Mathf.Clamp(position.width * 0.36f, 300f, 360f);
            var rightWidth = Mathf.Max(460f, position.width - leftWidth - 36f);

            stateWidth = 64f;

            // 固定层级列变宽后，剩余列改为按可用宽度分配，避免小窗口下继续挤压。
            var fixedWidth    = NUMBER_COLUMN_WIDTH + stateWidth + REMOVE_BUTTON_WIDTH + 36f;
            var flexibleWidth = Mathf.Max(260f, rightWidth - fixedWidth);

            objectWidth   = Mathf.Clamp(flexibleWidth * 0.38f, 100f, 220f);
            originalWidth = Mathf.Clamp(flexibleWidth * 0.28f, 80f, 180f);
        }

        private void DrawPreviewStatusBox()
        {
            var changedCount = GetChangedCount();
            var errorCount   = GetErrorCount();
            var summary = _renameMode == RenameMode.ManualModify
                              ? $"手动改名：{changedCount} 个将变更，{errorCount} 个错误。直接编辑右侧『新名称』列，点击『应用重命名』后才会真正改名。"
                              : _hasPreviewGenerated
                                  ? $"当前预览：{changedCount} 个将变更，{errorCount} 个错误。绿色/变更项表示点击『应用重命名』后才会真正改名。"
                                  : "当前未生成有效预览。『生成预览』只计算右侧新名称；『还原新名称』只清空预览，不会改真实对象。";

            if (!string.IsNullOrEmpty(_previewStatusText))
            {
                summary += "\n" + _previewStatusText;
            }

            EditorGUILayout.HelpBox(summary, errorCount > 0 ? MessageType.Warning : _previewStatusType);
        }

        #endregion

        #region ===== 核心逻辑 =====

        /// <summary>
        /// 添加拖拽或当前选择的对象。
        /// 说明：只有点击『导入当前选择』且开启『含子节点』时，Scene 对象才会按父子折叠结构导入。
        /// 拖拽导入仍保持扁平列表，避免误把大量子节点加入列表。
        /// </summary>
        private void AddObjects(IEnumerable<Object> objects, bool includeChildren)
        {
            if (objects == null) return;

            // 如果列表已经被清空，重新导入时从 01 开始显示和排序，避免移除后再次拖入出现 02、03 的体验问题。
            if (_items.Count == 0)
            {
                _nextAddOrder = 0;
            }

            foreach (var obj in objects)
            {
                if (obj == null) continue;

                if (includeChildren && obj is GameObject go && go.scene.IsValid())
                {
                    AddHierarchyObjectGroup(go);
                }
                else
                {
                    AddSingleObject(obj);
                }
            }

            SortItems();
            ValidateAllItems();
            SavePersistentObjects();
            SetPreviewStatus($"已记录 {_items.Count} 个对象；对象列表已自动持久化。", MessageType.Info, _hasPreviewGenerated);
        }

        /// <summary>
        /// 按真实父子树结构导入一个 Scene GameObject 及其所有子节点。
        /// 说明：每个节点都会拥有自己的 nodeId，子节点记录 parentNodeId，显示时即可形成多层折叠树。
        /// </summary>
        /// <param name="root">当前选择的父对象。</param>
        private void AddHierarchyObjectGroup(GameObject root)
        {
            if (root == null) return;
            AddHierarchyObjectNode(root.transform, string.Empty, 0, 0, true);
        }

        /// <summary>
        /// 递归导入层级节点。
        /// </summary>
        /// <param name="node">当前层级节点。</param>
        /// <param name="parentNodeId">父节点ID，顶层为空。</param>
        /// <param name="depth">相对导入根节点的层级深度。</param>
        /// <param name="siblingIndex">当前父级下的同级序号，顶层由显示列表动态编号。</param>
        /// <param name="isRoot">是否是本次导入的顶层对象。</param>
        private void AddHierarchyObjectNode(Transform node, string parentNodeId, int depth, int siblingIndex, bool isRoot)
        {
            if (node == null) return;

            var nodeId = GetGroupId(node.gameObject);
            var info = AddSingleObject(
                node.gameObject,
                nodeId,
                parentNodeId,
                isRoot,
                !string.IsNullOrEmpty(parentNodeId),
                depth,
                siblingIndex,
                true);

            if (info != null)
            {
                info.isExpanded = true;
            }

            var childIndex = 1;
            foreach (Transform child in node)
            {
                AddHierarchyObjectNode(child, nodeId, depth + 1, childIndex, false);
                childIndex++;
            }
        }

        /// <summary>
        /// 添加单个对象，自动过滤重复对象。
        /// </summary>
        /// <param name="obj">待加入的对象。</param>
        /// <param name="groupId">层级节点ID；含子节点导入时每个对象唯一。</param>
        /// <param name="parentGroupId">父层级节点ID；顶层对象为空。</param>
        /// <param name="isGroupRoot">是否是本次导入的顶层对象。</param>
        /// <param name="isGroupChild">是否是层级子项。</param>
        /// <param name="hierarchyDepth">相对导入根节点的层级深度。</param>
        /// <param name="childIndex">当前父级下的同级序号。</param>
        /// <param name="isExpanded">父项默认是否展开。</param>
        private ObjectInfo AddSingleObject(
            Object obj,
            string groupId = "",
            string parentGroupId = "",
            bool isGroupRoot = false,
            bool isGroupChild = false,
            int hierarchyDepth = 0,
            int childIndex = 0,
            bool isExpanded = true)
        {
            if (obj == null) return null;

            var existing = _items.FirstOrDefault(i => i.obj == obj);
            if (existing != null)
            {
                ApplyHierarchyMeta(existing, groupId, parentGroupId, isGroupRoot, isGroupChild, hierarchyDepth, childIndex, isExpanded);
                return existing;
            }

            var isSceneObject = obj is GameObject go && go.scene.IsValid();
            var path          = isSceneObject ? GetSceneObjectPath((GameObject)obj) : AssetDatabase.GetAssetPath(obj);

            var info = new ObjectInfo
            {
                obj           = obj,
                originalName  = obj.name,
                newName       = obj.name,
                path          = path,
                isSceneObject = isSceneObject,
                addOrder      = _nextAddOrder++,
                state         = RenameState.None
            };

            ApplyHierarchyMeta(info, groupId, parentGroupId, isGroupRoot, isGroupChild, hierarchyDepth, childIndex, isExpanded);
            _items.Add(info);
            return info;
        }

        /// <summary>
        /// 写入或刷新对象的父子折叠显示信息。
        /// </summary>
        private static void ApplyHierarchyMeta(
            ObjectInfo info,
            string groupId,
            string parentGroupId,
            bool isGroupRoot,
            bool isGroupChild,
            int hierarchyDepth,
            int childIndex,
            bool isExpanded)
        {
            if (info == null) return;
            if (string.IsNullOrEmpty(groupId) && !isGroupRoot && !isGroupChild) return;

            info.groupId        = groupId;
            info.parentGroupId  = parentGroupId;
            info.isGroupRoot    = isGroupRoot;
            info.isGroupChild   = isGroupChild;
            info.hierarchyDepth = Mathf.Max(0, hierarchyDepth);
            info.childIndex     = Mathf.Max(0, childIndex);
            info.isExpanded     = isExpanded;
        }

        /// <summary>
        /// 根据当前规则生成预览名称。
        /// </summary>
        private void PreviewRename(bool showDialogOnRegexError)
        {
            if (_renameMode == RenameMode.ManualModify)
            {
                ValidateAllItems();
                SetPreviewStatus("手动改名模式不会自动生成新名称；请直接编辑右侧『新名称』列。当前已完成名称校验。", MessageType.Info, false);
                return;
            }

            int    counter    = _startNumber;
            string regexError = null;

            foreach (var item in _items)
            {
                if (item.obj == null)
                {
                    item.state        = RenameState.MissingObject;
                    item.stateMessage = "<color=red>对象已经丢失</color>";
                    continue;
                }

                try
                {
                    item.newName      = BuildNewName(item.originalName, counter);
                    item.stateMessage = string.Empty;
                }
                catch (Exception ex)
                {
                    item.newName      = item.originalName;
                    item.state        = RenameState.RegexError;
                    item.stateMessage = ex.Message;
                    regexError        = ex.Message;
                }

                counter += _numberStep;
            }

            ValidateAllItems();
            SetPreviewStatus(string.IsNullOrEmpty(regexError)
                                 ? "预览已生成。请检查右侧『新名称』列和状态列，确认无错误后再应用。"
                                 : $"预览失败：{regexError}",
                string.IsNullOrEmpty(regexError) ? MessageType.Info : MessageType.Error,
                string.IsNullOrEmpty(regexError));

            if (showDialogOnRegexError && !string.IsNullOrEmpty(regexError))
            {
                EditorUtility.DisplayDialog("正则表达式错误", regexError, "知道了");
            }
        }

        /// <summary>
        /// 根据模式生成新名字。
        /// </summary>
        private string BuildNewName(string originalName, int counter)
        {
            originalName ??= string.Empty;

            switch (_renameMode)
            {
                case RenameMode.ManualModify:
                    return originalName;

                case RenameMode.Replace:
                    return _replaceAppendIndex
                               ? _replaceName + _indexSeparator + IndexFormatter.Format(counter, _replaceIndexFormat, _padding)
                               : _replaceName;

                case RenameMode.AppendText:
                {
                    var indexStr = _appendUseIndex ? IndexFormatter.Format(counter, _appendIndexFormat, _padding) : string.Empty;
                    var append   = _appendText + indexStr;
                    return _appendPosition switch
                    {
                        AppendPosition.Prefix => append + originalName,
                        AppendPosition.Insert => originalName.Insert(Mathf.Clamp(_insertIndex, 0, originalName.Length), append),
                        AppendPosition.Suffix => originalName + append,
                        _                     => originalName
                    };
                }

                case RenameMode.AutoIndex:
                    return _indexPrefix + _indexSeparator + IndexFormatter.Format(counter, _indexOnlyFormat, _padding) + _indexSuffix;

                case RenameMode.FindReplace:
                {
                    if (string.IsNullOrEmpty(_findText)) return originalName;
                    var comparison = _ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
                    return ReplaceString(originalName, _findText, _replaceText, comparison);
                }

                case RenameMode.RegexReplace:
                {
                    if (string.IsNullOrEmpty(_regexPattern)) return originalName;
                    var options = _regexIgnoreCase ? RegexOptions.IgnoreCase : RegexOptions.None;
                    return Regex.Replace(originalName, _regexPattern, _regexReplacement ?? string.Empty, options);
                }

                case RenameMode.CleanupName:
                    return CleanupName(originalName);

                default:
                    return originalName;
            }
        }

        /// <summary>
        /// 应用重命名。
        /// </summary>
        private void ApplyRename()
        {
            ValidateAllItems();
            if (GetErrorCount() > 0)
            {
                EditorUtility.DisplayDialog("无法应用", "列表中存在红色错误项，请先修正后再应用。", "知道了");
                return;
            }

            var changedItems = _items.Where(i => i.obj != null && i.newName != i.originalName).ToList();
            if (changedItems.Count == 0)
            {
                EditorUtility.DisplayDialog("无需应用", "当前没有需要重命名的对象。", "知道了");
                return;
            }

            if (!EditorUtility.DisplayDialog("确认重命名", $"即将重命名 {changedItems.Count} 个对象，是否继续？", "应用", "取消"))
            {
                return;
            }

            var sceneObjects = changedItems.Where(i => i.isSceneObject && i.obj is GameObject).Select(i => i.obj).ToArray();
            if (sceneObjects.Length > 0)
            {
                Undo.RecordObjects(sceneObjects, UNDO_NAME);
            }

            var successObjects = new List<Object>();
            foreach (var item in changedItems)
            {
                if (item.obj == null) continue;

                if (item.isSceneObject && item.obj is GameObject go)
                {
                    go.name           = item.newName;
                    item.originalName = item.newName;
                    item.path         = GetSceneObjectPath(go);
                    successObjects.Add(go);
                }
                else
                {
                    var assetPath = AssetDatabase.GetAssetPath(item.obj);
                    if (string.IsNullOrEmpty(assetPath)) continue;

                    var error = AssetDatabase.RenameAsset(assetPath, item.newName);
                    if (!string.IsNullOrEmpty(error))
                    {
                        item.state        = RenameState.InvalidAssetName;
                        item.stateMessage = error;
                        continue;
                    }

                    item.originalName = item.obj.name;
                    item.newName      = item.obj.name;
                    item.path         = AssetDatabase.GetAssetPath(item.obj);
                    successObjects.Add(item.obj);
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorApplication.RepaintHierarchyWindow();
            EditorApplication.RepaintProjectWindow();

            ValidateAllItems();
            SavePersistentObjects();
            SetPreviewStatus($"已应用重命名：成功处理 {successObjects.Count} 个对象。", MessageType.Info, false);

            if (_selectAfterApply && successObjects.Count > 0)
            {
                Selection.objects = successObjects.ToArray();
            }
        }

        #endregion

        #region ===== 校验 & 筛选 =====

        /// <summary>
        /// 检查空名、非法文件名、同目录 Project 资源重名、同父级 Scene 对象重名。
        /// </summary>
        private void ValidateAllItems()
        {
            foreach (var item in _items)
            {
                item.state        = RenameState.None;
                item.stateMessage = string.Empty;

                if (item.obj == null)
                {
                    item.state        = RenameState.MissingObject;
                    item.stateMessage = "<color=red>对象已经丢失</color>";
                    continue;
                }

                if (string.IsNullOrWhiteSpace(item.newName))
                {
                    item.state        = RenameState.EmptyName;
                    item.stateMessage = "<color=red>名称不能为空</color>";
                    continue;
                }

                if (!item.isSceneObject && HasInvalidFileNameChars(item.newName))
                {
                    item.state        = RenameState.InvalidAssetName;
                    item.stateMessage = "<color=red>Project 资源名称包含非法文件名字符</color>";
                    continue;
                }

                item.state = item.newName == item.originalName ? RenameState.None : RenameState.Changed;
            }

            MarkDuplicateNames();
        }

        private void MarkDuplicateNames()
        {
            var validItems = _items.Where(i => i.obj != null && !string.IsNullOrWhiteSpace(i.newName)).ToList();

            var sceneGroups = validItems
                .Where(i => i.isSceneObject && i.obj is GameObject)
                .GroupBy(i => GetSceneDuplicateKey((GameObject)i.obj, i.newName));

            foreach (var group in sceneGroups.Where(g => g.Count() > 1))
            {
                foreach (var item in group)
                {
                    item.state        = RenameState.DuplicateName;
                    item.stateMessage = "<color=yellow>同父级下存在重复名称</color>";
                }
            }

            var projectGroups = validItems
                .Where(i => !i.isSceneObject && !string.IsNullOrEmpty(AssetDatabase.GetAssetPath(i.obj)))
                .GroupBy(i => GetProjectDuplicateKey(i.obj, i.newName));

            foreach (var group in projectGroups.Where(g => g.Count() > 1))
            {
                foreach (var item in group)
                {
                    item.state        = RenameState.DuplicateName;
                    item.stateMessage = "<color=yellow>同目录下存在重复资源名</color>";
                }
            }
        }

        /// <summary>
        /// 获取满足来源、变化状态、搜索关键字条件的对象。
        /// 注意：此方法不处理折叠隐藏，折叠隐藏只属于 UI 显示层。
        /// </summary>
        private IEnumerable<ObjectInfo> GetFilteredItems()
        {
            IEnumerable<ObjectInfo> query = _items;

            query = _sourceFilter switch
            {
                SourceFilter.SceneOnly   => query.Where(i => i.isSceneObject),
                SourceFilter.ProjectOnly => query.Where(i => !i.isSceneObject),
                _                        => query
            };

            if (!_showUnchanged)
            {
                query = query.Where(i => i.state != RenameState.None);
            }

            if (!string.IsNullOrWhiteSpace(_searchKeyword))
            {
                var keyword = _searchKeyword.Trim();
                query = query.Where(i =>
                    ContainsIgnoreCase(i.originalName, keyword) ||
                    ContainsIgnoreCase(i.newName, keyword) ||
                    ContainsIgnoreCase(GetDisplayPath(i), keyword));
            }

            return query;
        }

        /// <summary>
        /// 构建当前可见列表的显示序号映射。
        /// 说明：顶层对象按可见顺序编号；层级子对象按各自父级下的 childIndex 编号。
        /// </summary>
        private static Dictionary<ObjectInfo, int> BuildDisplayIndexMap(IReadOnlyList<ObjectInfo> visibleItems)
        {
            var map           = new Dictionary<ObjectInfo, int>();
            var topLevelIndex = 1;

            foreach (var item in visibleItems)
            {
                if (item == null) continue;
                map[item] = IsHierarchyChild(item) ? Mathf.Max(1, item.childIndex) : topLevelIndex++;
            }

            return map;
        }

        /// <summary>
        /// 获取固定层级列下方展示的完整序号路径。
        /// 例如：[01] > [02] > [01]，用于替代无限缩进。
        /// </summary>
        private string GetHierarchyNumberPath(ObjectInfo info, int displayIndex, IReadOnlyDictionary<ObjectInfo, int> displayIndexMap)
        {
            if (info == null) return "层级 -";

            var chain   = new List<ObjectInfo>();
            var current = info;
            var safety  = 0;
            while (current != null && safety++ < 128)
            {
                chain.Add(current);
                current = GetHierarchyParent(current);
            }

            chain.Reverse();

            var parts = new List<string>();
            foreach (var node in chain)
            {
                int index;
                if (node == info)
                {
                    index = displayIndex;
                }
                else if (displayIndexMap != null && displayIndexMap.TryGetValue(node, out var mappedIndex))
                {
                    index = mappedIndex;
                }
                else
                {
                    index = IsHierarchyChild(node) ? Mathf.Max(1, node.childIndex) : Mathf.Max(1, node.addOrder + 1);
                }

                parts.Add($"[{Mathf.Max(1, index):00}]");
            }

            return parts.Count == 0 ? "层级 -" : "层级 " + string.Join(" > ", parts);
        }

        /// <summary>
        /// 获取父级对象名称，用于层级列 Tooltip。
        /// </summary>
        private string GetHierarchyParentName(ObjectInfo info)
        {
            var parent = GetHierarchyParent(info);
            return parent?.obj != null ? parent.obj.name : "无";
        }

        /// <summary>
        /// 获取直接子节点数量。
        /// </summary>
        private int GetDirectHierarchyChildCount(ObjectInfo info)
        {
            return info == null || string.IsNullOrEmpty(info.groupId)
                       ? 0
                       : _items.Count(i => i != null && i.parentGroupId == info.groupId);
        }

        /// <summary>
        /// 获取实际显示在右侧列表中的对象。
        /// 说明：没有搜索关键字时，任意祖先节点折叠都会隐藏其子孙节点；搜索时会强制展示匹配结果。
        /// </summary>
        private IEnumerable<ObjectInfo> GetDisplayItems()
        {
            var filteredItems = GetFilteredItems().ToList();
            var keywordMode   = !string.IsNullOrWhiteSpace(_searchKeyword);

            foreach (var item in filteredItems)
            {
                if (IsHierarchyChild(item) && !keywordMode && HasCollapsedAncestor(item))
                {
                    continue;
                }

                yield return item;
            }
        }

        /// <summary>
        /// 排序列表。
        /// 说明：排序只作用于顶层对象；层级子对象始终按父子关系和同级序号跟随父项。
        /// </summary>
        private void SortItems()
        {
            var topLevelItems = _items
                .Where(i => !IsHierarchyChild(i) || GetHierarchyParent(i) == null)
                .ToList();

            IEnumerable<ObjectInfo> sortedTopLevel = _sortMode switch
            {
                SortMode.OriginalNameAsc  => topLevelItems.OrderBy(i => i.originalName).ThenBy(i => i.addOrder),
                SortMode.OriginalNameDesc => topLevelItems.OrderByDescending(i => i.originalName).ThenBy(i => i.addOrder),
                SortMode.PathAsc          => topLevelItems.OrderBy(GetDisplayPath).ThenBy(i => i.addOrder),
                SortMode.SceneFirst       => topLevelItems.OrderByDescending(i => i.isSceneObject).ThenBy(i => i.addOrder),
                SortMode.ProjectFirst     => topLevelItems.OrderBy(i => i.isSceneObject).ThenBy(i => i.addOrder),
                _                         => topLevelItems.OrderBy(i => i.addOrder)
            };

            var result = new List<ObjectInfo>();
            var added  = new HashSet<ObjectInfo>();

            foreach (var item in sortedTopLevel)
            {
                AddItemAndChildrenInTreeOrder(item, result, added);
            }

            foreach (var item in _items.OrderBy(i => i.addOrder))
            {
                AddItemAndChildrenInTreeOrder(item, result, added);
            }

            _items.Clear();
            _items.AddRange(result);
        }

        /// <summary>
        /// 将节点和所有子节点按树顺序加入结果列表。
        /// </summary>
        private void AddItemAndChildrenInTreeOrder(ObjectInfo item, List<ObjectInfo> result, HashSet<ObjectInfo> added)
        {
            if (item == null || !added.Add(item)) return;

            result.Add(item);

            if (string.IsNullOrEmpty(item.groupId)) return;

            var children = _items
                .Where(i => i != item && i.parentGroupId == item.groupId)
                .OrderBy(i => i.childIndex)
                .ThenBy(i => i.addOrder)
                .ToList();

            foreach (var child in children)
            {
                AddItemAndChildrenInTreeOrder(child, result, added);
            }
        }

        #endregion

        #region ===== 工具按钮 =====

        private void SelectVisibleItems()
        {
            Selection.objects = GetDisplayItems().Where(i => i.obj != null).Select(i => i.obj).ToArray();
        }

        private void CopyVisibleNewNames()
        {
            EditorGUIUtility.systemCopyBuffer = string.Join("\n", GetDisplayItems().Select(i => i.newName));
        }

        private void OpenFirstProjectAsset()
        {
            var asset = GetDisplayItems().FirstOrDefault(i => i.obj != null && !i.isSceneObject)?.obj;
            if (asset == null) return;
            AssetDatabase.OpenAsset(asset);
        }

        private void ResetPreviewNames()
        {
            foreach (var item in _items)
            {
                item.newName = item.originalName;
            }

            ValidateAllItems();
            SetPreviewStatus("已还原新名称列：所有预览名恢复为原名称，真实对象没有被修改。", MessageType.Info, false);
        }

        private void RefreshOriginalNames()
        {
            foreach (var item in _items.Where(i => i.obj != null))
            {
                item.originalName = item.obj.name;
                item.newName      = item.obj.name;
                item.path         = item.isSceneObject && item.obj is GameObject go ? GetSceneObjectPath(go) : AssetDatabase.GetAssetPath(item.obj);
            }

            ValidateAllItems();
            SavePersistentObjects();
            SetPreviewStatus("已重新读取对象当前真实名称，并同步更新持久化列表。", MessageType.Info, false);
        }

        /// <summary>
        /// 从列表移除对象。
        /// 说明：如果移除的是层级节点，会同步移除该节点下的所有子孙对象。
        /// </summary>
        private void RemoveItem(ObjectInfo info)
        {
            if (info == null) return;

            if (IsHierarchyItem(info) && !string.IsNullOrEmpty(info.groupId))
            {
                var removeNodeIds = new HashSet<string>();
                CollectHierarchyNodeIds(info.groupId, removeNodeIds);

                var removedCount                     = _items.RemoveAll(i => !string.IsNullOrEmpty(i.groupId) && removeNodeIds.Contains(i.groupId));
                if (_items.Count == 0) _nextAddOrder = 0;
                ValidateAllItems();
                SavePersistentObjects();
                SetPreviewStatus($"已移除层级对象及其子孙对象，共 {removedCount} 项。", MessageType.Info, _hasPreviewGenerated);
                GUIUtility.ExitGUI();
                return;
            }

            _items.Remove(info);
            if (_items.Count == 0) _nextAddOrder = 0;
            ValidateAllItems();
            SavePersistentObjects();
            SetPreviewStatus("已从列表移除对象，并同步更新持久化列表。", MessageType.Info, _hasPreviewGenerated);
            GUIUtility.ExitGUI();
        }

        /// <summary>
        /// 收集指定层级节点及其全部子孙节点ID。
        /// </summary>
        private void CollectHierarchyNodeIds(string nodeId, HashSet<string> result)
        {
            if (string.IsNullOrEmpty(nodeId) || result == null || !result.Add(nodeId)) return;

            foreach (var child in _items.Where(i => i.parentGroupId == nodeId).ToList())
            {
                CollectHierarchyNodeIds(child.groupId, result);
            }
        }

        #endregion

        #region ===== 持久化 =====

        private void LoadPersistentObjects()
        {
            if (_persistentDataLoaded) return;
            _persistentDataLoaded = true;

            var json = EditorPrefs.GetString(PERSIST_KEY, string.Empty);
            if (string.IsNullOrEmpty(json)) return;

            try
            {
                var data = JsonUtility.FromJson<PersistentData>(json);
                if (data?.objects == null || data.objects.Count == 0) return;

                _items.Clear();
                _nextAddOrder = 0;

                var lostCount = 0;
                foreach (var persistentObject in data.objects)
                {
                    var obj = ResolvePersistentObject(persistentObject);
                    if (obj == null)
                    {
                        lostCount++;
                        continue;
                    }

                    var persistentGroupId  = persistentObject.groupId;
                    var persistentParentId = persistentObject.parentGroupId;

                    // 兼容旧版持久化：旧版子对象和父对象共享 groupId；新版要求每个节点都有唯一 nodeId。
                    if (persistentObject.isGroupChild && !string.IsNullOrEmpty(persistentParentId) && persistentGroupId == persistentParentId && obj is GameObject persistentGo)
                    {
                        persistentGroupId = GetGroupId(persistentGo);
                    }

                    var item = AddSingleObject(
                        obj,
                        persistentGroupId,
                        persistentParentId,
                        persistentObject.isGroupRoot,
                        persistentObject.isGroupChild,
                        persistentObject.hierarchyDepth,
                        persistentObject.childIndex,
                        persistentObject.isExpanded);

                    if (item != null && !string.IsNullOrEmpty(persistentObject.newName))
                    {
                        item.newName = persistentObject.newName;
                    }
                }

                _nextAddOrder = Mathf.Max(_nextAddOrder, data.nextAddOrder);
                SortItems();
                ValidateAllItems();

                _persistentStatusText = lostCount > 0
                                            ? $"已恢复 {_items.Count} 个上次对象，{lostCount} 个对象未找到。"
                                            : $"已恢复 {_items.Count} 个上次对象。";
            }
            catch (Exception ex)
            {
                _persistentStatusText = $"恢复上次对象列表失败：{ex.Message}";
            }
        }

        private void SavePersistentObjects()
        {
            if (!_persistentDataLoaded && _items.Count == 0) return;

            var data = new PersistentData
            {
                nextAddOrder = _nextAddOrder,
                objects = _items
                    .Where(i => i.obj != null)
                    .Select(i => new PersistentObjectInfo
                    {
                        globalObjectId = GetPersistentGlobalObjectId(i.obj),
                        assetPath      = i.isSceneObject ? string.Empty : AssetDatabase.GetAssetPath(i.obj),
                        displayPath    = GetDisplayPath(i),
                        objectName     = i.obj.name,
                        newName        = i.newName,
                        isSceneObject  = i.isSceneObject,
                        groupId        = i.groupId,
                        parentGroupId  = i.parentGroupId,
                        isGroupRoot    = i.isGroupRoot,
                        isGroupChild   = i.isGroupChild,
                        hierarchyDepth = i.hierarchyDepth,
                        childIndex     = i.childIndex,
                        isExpanded     = i.isExpanded
                    })
                    .ToList()
            };

            EditorPrefs.SetString(PERSIST_KEY, JsonUtility.ToJson(data));
        }

        private static Object ResolvePersistentObject(PersistentObjectInfo info)
        {
            if (info == null) return null;

#if UNITY_2019_2_OR_NEWER
            if (!string.IsNullOrEmpty(info.globalObjectId) && GlobalObjectId.TryParse(info.globalObjectId, out var globalObjectId))
            {
                var obj = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(globalObjectId);
                if (obj != null) return obj;
            }
#endif

            if (!string.IsNullOrEmpty(info.assetPath))
            {
                var obj = AssetDatabase.LoadAssetAtPath<Object>(info.assetPath);
                if (obj != null) return obj;
            }

            return info.isSceneObject ? FindSceneObjectByDisplayPath(info.displayPath) : null;
        }

        private static string GetPersistentGlobalObjectId(Object obj)
        {
            if (obj == null) return string.Empty;

#if UNITY_2019_2_OR_NEWER
            try
            {
                return GlobalObjectId.GetGlobalObjectIdSlow(obj).ToString();
            }
            catch
            {
                return string.Empty;
            }
#else
            return string.Empty;
#endif
        }

        private static GameObject FindSceneObjectByDisplayPath(string displayPath)
        {
            if (string.IsNullOrEmpty(displayPath)) return null;

            var splitIndex = displayPath.IndexOf('/');
            if (splitIndex < 0) return null;

            var sceneName     = displayPath[..splitIndex];
            var hierarchyPath = displayPath[(splitIndex + 1)..];

            for (var i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (!scene.isLoaded || scene.name != sceneName) continue;

                foreach (var root in scene.GetRootGameObjects())
                {
                    if (GetRelativeHierarchyPath(root.transform) == hierarchyPath) return root;
                    var found = FindChildByRelativePath(root.transform, hierarchyPath);
                    if (found != null) return found.gameObject;
                }
            }

            return null;
        }

        private static Transform FindChildByRelativePath(Transform root, string hierarchyPath)
        {
            foreach (Transform child in root)
            {
                if (GetRelativeHierarchyPath(child) == hierarchyPath) return child;
                var found = FindChildByRelativePath(child, hierarchyPath);
                if (found != null) return found;
            }

            return null;
        }

        private static string GetRelativeHierarchyPath(Transform transform)
        {
            if (transform == null) return string.Empty;

            var path   = transform.name;
            var parent = transform.parent;
            while (parent != null)
            {
                path   = parent.name + "/" + path;
                parent = parent.parent;
            }

            return path;
        }

        private void SetPreviewStatus(string message, MessageType messageType, bool hasPreviewGenerated)
        {
            _previewStatusText   = message;
            _previewStatusType   = messageType;
            _hasPreviewGenerated = hasPreviewGenerated;
            Repaint();
        }

        #endregion

        #region ===== 工具方法 =====

        /// <summary>
        /// 获取含子节点导入使用的稳定节点ID。
        /// </summary>
        private static string GetGroupId(GameObject node)
        {
            if (node == null) return string.Empty;

            var globalId = GetPersistentGlobalObjectId(node);
            return string.IsNullOrEmpty(globalId) ? $"SceneNode_{node.GetInstanceID()}" : globalId;
        }

        /// <summary>
        /// 判断对象是否属于层级树显示。
        /// </summary>
        private static bool IsHierarchyItem(ObjectInfo info)
        {
            return info != null && (!string.IsNullOrEmpty(info.groupId) || info.isGroupRoot || info.isGroupChild);
        }

        /// <summary>
        /// 判断对象是否是层级顶层对象。
        /// </summary>
        private static bool IsHierarchyTopLevel(ObjectInfo info)
        {
            return IsHierarchyItem(info) && string.IsNullOrEmpty(info.parentGroupId);
        }

        /// <summary>
        /// 判断对象是否是层级子对象。
        /// </summary>
        private static bool IsHierarchyChild(ObjectInfo info)
        {
            return info != null && !string.IsNullOrEmpty(info.parentGroupId);
        }

        /// <summary>
        /// 获取层级父节点。
        /// </summary>
        private ObjectInfo GetHierarchyParent(ObjectInfo info)
        {
            if (info == null || string.IsNullOrEmpty(info.parentGroupId)) return null;
            return _items.FirstOrDefault(i => i.groupId == info.parentGroupId);
        }

        /// <summary>
        /// 判断节点是否存在子节点。
        /// </summary>
        private bool HasHierarchyChildren(ObjectInfo info)
        {
            return info != null && !string.IsNullOrEmpty(info.groupId) && _items.Any(i => i.parentGroupId == info.groupId);
        }

        /// <summary>
        /// 判断是否存在已折叠的祖先节点。
        /// </summary>
        private bool HasCollapsedAncestor(ObjectInfo info)
        {
            var parent = GetHierarchyParent(info);
            var safety = 0;
            while (parent != null && safety++ < 128)
            {
                if (!parent.isExpanded) return true;
                parent = GetHierarchyParent(parent);
            }

            return false;
        }

        private static GUIContent GetObjectContent(ObjectInfo info)
        {
            if (info.obj == null) return new GUIContent("Missing", EditorGUIUtility.IconContent("console.erroricon").image);
            var icon = EditorGUIUtility.ObjectContent(info.obj, info.obj.GetType()).image;
            return new GUIContent(info.obj.name, icon, GetDisplayPath(info));
        }

        private void SelectAndPing(Object obj)
        {
            if (obj == null) return;
            Selection.activeObject = obj;
            if (_pingOnClick) EditorGUIUtility.PingObject(obj);
            if (_pingOnClick && obj is GameObject) SceneView.lastActiveSceneView?.FrameSelected();
        }

        private static string GetSceneObjectPath(GameObject go)
        {
            if (go == null) return string.Empty;
            var path   = go.name;
            var parent = go.transform.parent;

            while (parent != null)
            {
                path   = parent.name + "/" + path;
                parent = parent.parent;
            }

            return $"{go.scene.name}/{path}";
        }

        private static string GetDisplayPath(ObjectInfo info)
        {
            if (info == null || info.obj == null) return "Missing";
            if (info.isSceneObject && info.obj is GameObject go) return GetSceneObjectPath(go);
            var path = AssetDatabase.GetAssetPath(info.obj);
            return string.IsNullOrEmpty(path) ? info.path : path;
        }

        private static string GetSourceText(ObjectInfo info)
        {
            return info.isSceneObject ? "[Scene]" : "[Project]";
        }

        private static string GetStateText(ObjectInfo info)
        {
            return info.state switch
            {
                RenameState.Changed          => "<color=green>变更</color>",
                RenameState.EmptyName        => "<color=yellow>空名</color>",
                RenameState.DuplicateName    => "<color=yellow>重名</color>",
                RenameState.InvalidAssetName => "<color=yellow>非法</color>",
                RenameState.MissingObject    => "<color=red>丢失</color>",
                RenameState.RegexError       => "<color=red>正则错</color>",
                _                            => "-"
            };
        }

        private int GetChangedCount()
        {
            return _items.Count(i => i.obj != null && i.state == RenameState.Changed);
        }

        private int GetErrorCount()
        {
            return _items.Count(i => i.state is RenameState.EmptyName or RenameState.DuplicateName or RenameState.InvalidAssetName or RenameState.MissingObject or RenameState.RegexError);
        }

        private static bool ContainsIgnoreCase(string source, string keyword)
        {
            return !string.IsNullOrEmpty(source) && source.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string ReplaceString(string source, string oldValue, string newValue, StringComparison comparison)
        {
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(oldValue)) return source;

            int index = source.IndexOf(oldValue, comparison);
            if (index < 0) return source;

            var result = source;
            while (index >= 0)
            {
                result = result.Remove(index, oldValue.Length).Insert(index, newValue ?? string.Empty);
                index  = result.IndexOf(oldValue, index + (newValue?.Length ?? 0), comparison);
            }

            return result;
        }

        private string CleanupName(string objName)
        {
            if (objName == null) return string.Empty;
            var result = objName;

            if (_removeCloneText)
            {
                result = result.Replace("(Clone)", string.Empty).Replace(" (Clone)", string.Empty);
            }

            if (_trimSpaces) result             = result.Trim();
            if (_collapseSpaces) result         = Regex.Replace(result, "\\s+", " ");
            if (_removeSpaces) result           = Regex.Replace(result, "\\s+", string.Empty);
            if (_removeInvalidFileChars) result = RemoveInvalidFileNameChars(result);

            return result;
        }

        private static bool HasInvalidFileNameChars(string name)
        {
            return name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 || name.Contains("/") || name.Contains("\\");
        }

        private static string RemoveInvalidFileNameChars(string name)
        {
            if (string.IsNullOrEmpty(name)) return string.Empty;
            foreach (var c in Path.GetInvalidFileNameChars())
            {
                name = name.Replace(c.ToString(), string.Empty);
            }

            return name.Replace("/", string.Empty).Replace("\\", string.Empty);
        }

        private static string GetSceneDuplicateKey(GameObject go, string newName)
        {
            var parentId = go.transform.parent == null ? "ROOT" : go.transform.parent.GetInstanceID().ToString();
            return $"{go.scene.handle}|{parentId}|{newName}";
        }

        private static string GetProjectDuplicateKey(Object obj, string newName)
        {
            var path = AssetDatabase.GetAssetPath(obj);
            var dir  = string.IsNullOrEmpty(path) ? string.Empty : Path.GetDirectoryName(path);
            return $"{dir}|{newName}";
        }

        #endregion

        #region ===== 样式 =====

        private void InitStyles()
        {
            _titleStyle ??= new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize  = 18,
                alignment = TextAnchor.MiddleLeft
            };

            _subTitleStyle ??= new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize  = 14,
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
                fontSize  = 13,
                fontStyle = FontStyle.Bold,
                richText  = true
            };

            _rowStyle ??= new GUIStyle(EditorStyles.helpBox)
            {
                padding = new RectOffset(6, 6, 4, 4)
            };

            _changedRowStyle ??= new GUIStyle(EditorStyles.helpBox)
            {
                padding = new RectOffset(6, 6, 4, 4)
            };
            _changedRowStyle.normal.textColor = Color.white;

            _errorRowStyle ??= new GUIStyle(EditorStyles.helpBox)
            {
                padding = new RectOffset(6, 6, 4, 4)
            };
            _errorRowStyle.normal.textColor = Color.red;

            _groupRootRowStyle ??= new GUIStyle(EditorStyles.helpBox)
            {
                padding   = new RectOffset(6, 6, 4, 4),
                fontStyle = FontStyle.Bold
            };

            _childRowStyle ??= new GUIStyle(EditorStyles.helpBox)
            {
                padding = new RectOffset(6, 6, 3, 3)
            };

            _foldoutStyle ??= new GUIStyle(EditorStyles.foldout)
            {
                fontStyle = FontStyle.Bold
            };

            _treeLevelStyle ??= new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                normal    = { textColor = EditorGUIUtility.isProSkin ? new Color(0.58f, 0.78f, 1f) : new Color(0.12f, 0.35f, 0.7f) }
            };

            _treeIndexStyle ??= new GUIStyle(EditorStyles.miniBoldLabel)
            {
                alignment = TextAnchor.MiddleLeft,
                richText  = true
            };

            _treeChildCountStyle ??= new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleLeft,
                normal    = { textColor = EditorGUIUtility.isProSkin ? new Color(0.62f, 0.62f, 0.62f) : new Color(0.42f, 0.42f, 0.42f) }
            };

            _treePathStyle ??= new GUIStyle(EditorStyles.miniLabel)
            {
                wordWrap = false,
                normal   = { textColor = EditorGUIUtility.isProSkin ? new Color(0.55f, 0.75f, 0.95f) : new Color(0.18f, 0.38f, 0.65f) }
            };

            _pathStyle ??= new GUIStyle(EditorStyles.miniLabel)
            {
                wordWrap = false,
                richText = true,
                normal   = { textColor = EditorGUIUtility.isProSkin ? new Color(0.68f, 0.68f, 0.68f) : new Color(0.35f, 0.35f, 0.35f) }
            };

            _toolbarSearchStyle ??= new GUIStyle(EditorStyles.toolbarSearchField);

            _stateTextStyle ??= new GUIStyle(EditorStyles.label)
            {
                richText = true,
            };
        }

        #endregion

        #region ===== 序号格式 =====

        private static class IndexFormatter
        {
            [Header("中文数字 - 小写映射表")] private static readonly string[] cnLower = { "零", "一", "二", "三", "四", "五", "六", "七", "八", "九" };

            [Header("中文数字 - 大写映射表")] private static readonly string[] cnUpper = { "零", "壹", "贰", "叁", "肆", "伍", "陆", "柒", "捌", "玖" };

            public static string Format(int index, IndexFormat format, int padding)
            {
                return format switch
                {
                    IndexFormat.Arabic       => Mathf.Max(0, index).ToString(padding > 0 ? new string('0', padding) : "0"),
                    IndexFormat.EnglishLower => ToLetters(index, false),
                    IndexFormat.EnglishUpper => ToLetters(index, true),
                    IndexFormat.ChineseLower => ToChinese(index, cnLower, false),
                    IndexFormat.ChineseUpper => ToChinese(index, cnUpper, true),
                    _                        => index.ToString()
                };
            }

            private static string ToLetters(int number, bool upper)
            {
                number = Mathf.Max(1, number);
                var result = string.Empty;
                while (number > 0)
                {
                    number--;
                    var c = (char)((upper ? 'A' : 'a') + number % 26);
                    result =  c + result;
                    number /= 26;
                }

                return result;
            }

            private static string ToChinese(int num, string[] map, bool upper)
            {
                if (num < 0) return num.ToString();
                if (num < 10) return map[num];
                if (num < 20) return (upper ? "拾" : "十") + (num % 10 == 0 ? string.Empty : map[num % 10]);
                if (num < 100) return map[num / 10] + (upper ? "拾" : "十") + (num % 10 == 0 ? string.Empty : map[num % 10]);
                return num.ToString();
            }
        }

        #endregion
    }
}
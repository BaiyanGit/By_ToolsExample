namespace _3rdBy.ByTools.FindHierarchyTag.Editor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    /// <summary>
    /// 根据 Tag 查找对象。
    /// 支持查找当前已经打开的 Hierarchy 场景对象，也支持查找 Project 下的 Prefab 资产及其子节点。
    /// </summary>
    public class FindHierarchyTagEditorWindow : EditorWindow
    {
        private const string WindowTitle = "Tag 对象查找";
        private const string UntaggedName = "Untagged";
        private const string MissingTagName = "<Missing Tag>";

        /// <summary>
        /// 当窗口被 Dock 得很窄时，左右结构会自动变成上下结构，避免控件挤压。
        /// </summary>
        private const float CompactLayoutWidth = 680f;

        private const float OuterPadding = 8f;
        private const float SectionGap = 6f;
        private const float HeaderHeight = 58f;
        private const float ToolbarHeight = 78f;
        private const float FooterHeight = 34f;
        private const float CompactTagPanelHeight = 180f;
        private const int ProjectScanProgressThreshold = 200;

        private enum SearchScope
        {
            Hierarchy,
            ProjectPrefabs,
            HierarchyAndProjectPrefabs
        }

        private enum SearchSource
        {
            Scene,
            ProjectPrefab
        }

        private enum SortMode
        {
            TagNameAsc,
            TagNameDesc,
            CountDesc,
            CountAsc
        }

        private sealed class TaggedObjectInfo
        {
            public GameObject GameObject;
            public GameObject PrefabAssetRoot;
            public string Name;
            public string HierarchyPath;
            public string SceneName;
            public string AssetPath;
            public bool ActiveInHierarchy;
            public SearchSource Source;
        }

        private sealed class TagBucket
        {
            public string TagName;
            public readonly List<TaggedObjectInfo> Objects = new List<TaggedObjectInfo>();
        }

        private static readonly string[] SearchScopeLabels =
        {
            "Hierarchy",
            "Project Prefab",
            "Hierarchy + Project"
        };

        private readonly List<TagBucket> _tagBuckets = new List<TagBucket>();

        private SearchScope _searchScope = SearchScope.Hierarchy;
        private string _selectedTagName = string.Empty;
        private string _tagSearchText = string.Empty;
        private string _objectSearchText = string.Empty;

        private bool _includeInactive = true;
        private bool _includeUntagged;
        private bool _autoRefresh = true;
        private bool _frameOnSelect = true;
        private bool _needsRefresh;
        private bool _queuedRefresh;

        private SortMode _sortMode = SortMode.TagNameAsc;
        private Vector2 _tagScrollPosition;
        private Vector2 _objectScrollPosition;

        private int _totalObjectCount;
        private int _sceneObjectCount;
        private int _projectPrefabCount;
        private int _projectObjectCount;
        private string _lastRefreshTime = "--:--:--";

        private GUIStyle _titleStyle;
        private GUIStyle _sectionTitleStyle;
        private GUIStyle _mutedLabelStyle;
        private GUIStyle _pathLabelStyle;
        private GUIStyle _tagButtonStyle;
        private GUIStyle _selectedTagButtonStyle;
        private GUIStyle _objectNameButtonStyle;
        private GUIStyle _badgeStyle;
        private GUIStyle _placeholderStyle;
        private GUIStyle _miniToolbarButtonStyle;

        [MenuItem("ByTools/🏷️ 标签Tag查找对象")]
        public static void ShowWindow()
        {
            var window = GetWindow<FindHierarchyTagEditorWindow>(WindowTitle);
            window.minSize = new Vector2(620, 420);
            window.Show();
        }

        private void OnEnable()
        {
            EditorApplication.hierarchyChanged += OnHierarchyChanged;
            EditorApplication.projectChanged   += OnProjectChanged;
            RefreshData(false);
        }

        private void OnDisable()
        {
            EditorApplication.hierarchyChanged -= OnHierarchyChanged;
            EditorApplication.projectChanged   -= OnProjectChanged;
            EditorApplication.delayCall        -= DelayedRefresh;
            EditorUtility.ClearProgressBar();
        }

        private void OnGUI()
        {
            EnsureStyles();

            // 使用显式 Rect 分区，而不是让左右面板参与整窗高度竞争。
            // 这样左侧 Tag 数量再多，也只能在自己的 Rect 内滚动，不会“抢高度”。
            var root = new Rect(OuterPadding, OuterPadding,
                Mathf.Max(0f, position.width - OuterPadding * 2f),
                Mathf.Max(0f, position.height - OuterPadding * 2f));

            var headerRect  = new Rect(root.x, root.y, root.width, HeaderHeight);
            var toolbarRect = new Rect(root.x, headerRect.yMax + SectionGap, root.width, ToolbarHeight);
            var footerRect  = new Rect(root.x, root.yMax - FooterHeight, root.width, FooterHeight);
            var mainRect = new Rect(root.x, toolbarRect.yMax + SectionGap, root.width,
                Mathf.Max(80f, footerRect.y - toolbarRect.yMax - SectionGap * 2f));

            DrawHeader(headerRect);
            DrawToolbar(toolbarRect);
            DrawMainContent(mainRect);
            DrawFooter(footerRect);
        }

        /// <summary>
        /// 当层级面板对象发生变化时，根据设置自动刷新或标记为待刷新。
        /// </summary>
        private void OnHierarchyChanged()
        {
            if (!ShouldScanHierarchy()) return;

            HandleDataChanged();
        }

        /// <summary>
        /// 当 Project 资源发生变化时，根据当前查找范围决定是否刷新。
        /// </summary>
        private void OnProjectChanged()
        {
            if (!ShouldScanProjectPrefabs()) return;

            HandleDataChanged();
        }

        /// <summary>
        /// 统一处理数据变化。
        /// 自动刷新开启时延迟刷新；关闭时只给出状态提示，避免频繁扫描 Project。
        /// </summary>
        private void HandleDataChanged()
        {
            if (!_autoRefresh)
            {
                _needsRefresh = true;
                Repaint();
                return;
            }

            QueueRefresh();
        }

        /// <summary>
        /// 延迟刷新，避免层级或 Project 变化频繁时一帧内重复扫描。
        /// </summary>
        private void QueueRefresh()
        {
            if (_queuedRefresh) return;

            _queuedRefresh              =  true;
            EditorApplication.delayCall += DelayedRefresh;
        }

        private void DelayedRefresh()
        {
            EditorApplication.delayCall -= DelayedRefresh;
            _queuedRefresh              =  false;

            if (this == null) return;

            RefreshData(false);
            Repaint();
        }

        /// <summary>
        /// 顶部标题和统计信息。
        /// </summary>
        private void DrawHeader(Rect rect)
        {
            GUILayout.BeginArea(rect, GUIContent.none, EditorStyles.helpBox);
            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            EditorGUILayout.LabelField("🏷️ 标签查找器", _titleStyle);
            GUILayout.Space(12f);
            EditorGUILayout.LabelField(
                $"范围：{GetSearchScopeDisplayName()}    Tag：{_tagBuckets.Count}    对象：{_totalObjectCount}    Scene：{_sceneObjectCount}    Prefab：{_projectPrefabCount} / {_projectObjectCount}    最后刷新：{_lastRefreshTime}",
                _mutedLabelStyle);

            EditorGUILayout.EndVertical();
            GUILayout.EndArea();
        }

        /// <summary>
        /// 顶部工具栏。
        /// 固定在 Toolbar Rect 内，不参与主内容区高度计算。
        /// </summary>
        private void DrawToolbar(Rect rect)
        {
            GUILayout.BeginArea(rect, GUIContent.none, EditorStyles.toolbar);
            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));

            EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true), GUILayout.Height(24));

            if (GUILayout.Button("刷新", _miniToolbarButtonStyle, GUILayout.Width(52)))
            {
                RefreshData(true);
            }

            using (new EditorGUI.DisabledScope(!_needsRefresh))
            {
                if (GUILayout.Button(_needsRefresh ? "数据已变化，点击刷新" : "数据无变化", _miniToolbarButtonStyle, GUILayout.Width(132)))
                {
                    RefreshData(true);
                }
            }

            GUILayout.FlexibleSpace();

            EditorGUILayout.LabelField("范围", GUILayout.Width(28));
            EditorGUI.BeginChangeCheck();
            _searchScope = (SearchScope)EditorGUILayout.Popup((int)_searchScope, SearchScopeLabels, EditorStyles.toolbarPopup, GUILayout.Width(138));
            if (EditorGUI.EndChangeCheck())
            {
                RefreshData(false);
            }

            EditorGUILayout.LabelField("排序", GUILayout.Width(28));
            EditorGUI.BeginChangeCheck();
            _sortMode = (SortMode)EditorGUILayout.EnumPopup(_sortMode, EditorStyles.toolbarPopup, GUILayout.Width(116));
            if (EditorGUI.EndChangeCheck())
            {
                SortBuckets();
            }

            if (GUILayout.Button("清空搜索", _miniToolbarButtonStyle, GUILayout.Width(76)))
            {
                _tagSearchText    = string.Empty;
                _objectSearchText = string.Empty;
                GUI.FocusControl(null);
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true), GUILayout.Height(24));

            EditorGUI.BeginChangeCheck();
            _includeInactive = GUILayout.Toggle(_includeInactive, "包含隐藏对象", _miniToolbarButtonStyle, GUILayout.Width(98));
            _includeUntagged = GUILayout.Toggle(_includeUntagged, "包含 Untagged", _miniToolbarButtonStyle, GUILayout.Width(106));
            _autoRefresh     = GUILayout.Toggle(_autoRefresh, "自动刷新", _miniToolbarButtonStyle, GUILayout.Width(78));
            _frameOnSelect   = GUILayout.Toggle(_frameOnSelect, "选中后定位", _miniToolbarButtonStyle, GUILayout.Width(90));
            if (EditorGUI.EndChangeCheck())
            {
                RefreshData(false);
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField("Project 扫描范围：Prefab 资产及其子节点", _mutedLabelStyle, GUILayout.Width(220));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
            GUILayout.EndArea();
        }

        /// <summary>
        /// 主体区域。正常宽度为左右布局；窗口过窄时自动变为上下布局。
        /// 这里使用明确 Rect 切分，彻底避免左侧 ScrollView 抢占窗口高度。
        /// </summary>
        private void DrawMainContent(Rect rect)
        {
            if (rect.width <= 1f || rect.height <= 1f) return;

            if (position.width < CompactLayoutWidth)
            {
                var tagHeight = Mathf.Min(CompactTagPanelHeight, rect.height * 0.42f);
                var tagRect   = new Rect(rect.x, rect.y, rect.width, tagHeight);
                var objectRect = new Rect(rect.x, tagRect.yMax + SectionGap, rect.width,
                    Mathf.Max(80f, rect.height - tagHeight - SectionGap));

                DrawTagPanel(tagRect);
                DrawObjectPanel(objectRect);
                return;
            }

            var leftWidth = GetAdaptiveLeftPanelWidth();
            leftWidth = Mathf.Clamp(leftWidth, 200f, Mathf.Max(200f, rect.width - 360f - SectionGap));

            var tagPanelRect = new Rect(rect.x, rect.y, leftWidth, rect.height);
            var objectPanelRect = new Rect(tagPanelRect.xMax + SectionGap, rect.y,
                Mathf.Max(120f, rect.width - leftWidth - SectionGap), rect.height);

            DrawTagPanel(tagPanelRect);
            DrawObjectPanel(objectPanelRect);
        }

        /// <summary>
        /// 根据窗口宽度计算左侧 Tag 面板宽度，保证右侧对象列表至少有可用空间。
        /// </summary>
        private float GetAdaptiveLeftPanelWidth()
        {
            const float minLeftWidth     = 220f;
            const float maxLeftWidth     = 340f;
            const float minRightWidth    = 380f;
            const float estimatedPadding = 34f;

            var leftWidth  = Mathf.Clamp(position.width * 0.32f, minLeftWidth, maxLeftWidth);
            var rightWidth = position.width - leftWidth - estimatedPadding;

            if (rightWidth < minRightWidth)
            {
                leftWidth = Mathf.Max(200f, position.width - minRightWidth - estimatedPadding);
            }

            return leftWidth;
        }

        /// <summary>
        /// 左侧 Tag 列表。
        /// </summary>
        private void DrawTagPanel(Rect rect)
        {
            GUILayout.BeginArea(rect, GUIContent.none, EditorStyles.helpBox);
            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            EditorGUILayout.LabelField("Tag 列表", _sectionTitleStyle);
            _tagSearchText = DrawSearchField("搜索 Tag", _tagSearchText);

            var visibleBuckets = GetVisibleTagBuckets().ToList();
            EditorGUILayout.LabelField($"显示：{visibleBuckets.Count} / {_tagBuckets.Count}", _mutedLabelStyle);

            _tagScrollPosition = EditorGUILayout.BeginScrollView(_tagScrollPosition, false, true, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            if (visibleBuckets.Count == 0)
            {
                EditorGUILayout.HelpBox("没有找到匹配的 Tag。", MessageType.Info);
            }
            else
            {
                for (var i = 0; i < visibleBuckets.Count; i++)
                {
                    DrawTagRow(visibleBuckets[i]);
                }
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.EndVertical();
            GUILayout.EndArea();
        }

        /// <summary>
        /// 单行 Tag。
        /// </summary>
        private void DrawTagRow(TagBucket bucket)
        {
            var isSelected       = string.Equals(_selectedTagName, bucket.TagName, StringComparison.Ordinal);
            var cachedBackground = GUI.backgroundColor;
            GUI.backgroundColor = isSelected ? new Color(0.42f, 0.62f, 1f) : Color.white;

            var content = new GUIContent($"{bucket.TagName}    ({bucket.Objects.Count})", "点击查看此 Tag 下的所有对象");
            if (GUILayout.Button(content, isSelected ? _selectedTagButtonStyle : _tagButtonStyle, GUILayout.Height(26), GUILayout.ExpandWidth(true)))
            {
                _selectedTagName      = bucket.TagName;
                _objectScrollPosition = Vector2.zero;
                GUI.FocusControl(null);
            }

            GUI.backgroundColor = cachedBackground;
        }

        /// <summary>
        /// 右侧对象列表和操作区。
        /// </summary>
        private void DrawObjectPanel(Rect rect)
        {
            GUILayout.BeginArea(rect, GUIContent.none, EditorStyles.helpBox);
            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            var selectedBucket = GetSelectedBucket();
            if (selectedBucket == null)
            {
                EditorGUILayout.LabelField("对象列表", _sectionTitleStyle);
                EditorGUILayout.HelpBox("请先在左侧选择一个 Tag。", MessageType.Info);
                EditorGUILayout.EndVertical();
                GUILayout.EndArea();
                return;
            }

            var visibleObjects = GetVisibleObjects(selectedBucket).ToList();
            DrawSelectedTagHeader(selectedBucket, visibleObjects, rect.width);

            _objectSearchText = DrawSearchField("搜索对象名 / 路径 / 场景名 / 资源路径", _objectSearchText);

            _objectScrollPosition = EditorGUILayout.BeginScrollView(_objectScrollPosition, false, true, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            if (visibleObjects.Count == 0)
            {
                EditorGUILayout.HelpBox("当前筛选条件下没有对象。", MessageType.Info);
            }
            else
            {
                for (var i = 0; i < visibleObjects.Count; i++)
                {
                    DrawObjectRow(i, visibleObjects[i]);
                }
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.EndVertical();
            GUILayout.EndArea();
        }

        /// <summary>
        /// 已选 Tag 的标题和批量操作按钮。
        /// 宽窗口一行显示，窄窗口分两行显示，避免按钮被压扁。
        /// </summary>
        private void DrawSelectedTagHeader(TagBucket selectedBucket, List<TaggedObjectInfo> visibleObjects, float panelWidth)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Tag：{selectedBucket.TagName}", _sectionTitleStyle);
            GUILayout.Label($"总数 {selectedBucket.Objects.Count} / 显示 {visibleObjects.Count}", _badgeStyle, GUILayout.Width(128));
            EditorGUILayout.EndHorizontal();

            if (panelWidth < 460f)
            {
                DrawObjectActionButtonsCompact(selectedBucket, visibleObjects);
                return;
            }

            EditorGUILayout.BeginHorizontal();
            DrawObjectActionButtons(selectedBucket, visibleObjects);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawObjectActionButtonsCompact(TagBucket selectedBucket, List<TaggedObjectInfo> visibleObjects)
        {
            EditorGUILayout.BeginHorizontal();
            using (new EditorGUI.DisabledScope(visibleObjects.Count == 0))
            {
                if (GUILayout.Button("选择显示对象"))
                {
                    SelectObjects(visibleObjects);
                }

                if (GUILayout.Button("复制显示路径"))
                {
                    CopyObjectPaths(visibleObjects);
                }
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            using (new EditorGUI.DisabledScope(visibleObjects.Count == 0))
            {
                if (GUILayout.Button("Ping 第一个"))
                {
                    PingAndSelect(visibleObjects[0]);
                }

                if (GUILayout.Button("打开 Prefab"))
                {
                    OpenFirstProjectPrefab(visibleObjects);
                }
            }

            if (GUILayout.Button("复制 Tag"))
            {
                GUIUtility.systemCopyBuffer = selectedBucket.TagName;
                ShowNotification(new GUIContent("已复制 Tag"));
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawObjectActionButtons(TagBucket selectedBucket, List<TaggedObjectInfo> visibleObjects)
        {
            using (new EditorGUI.DisabledScope(visibleObjects.Count == 0))
            {
                if (GUILayout.Button("选择显示对象", GUILayout.Width(100)))
                {
                    SelectObjects(visibleObjects);
                }

                if (GUILayout.Button("复制显示路径", GUILayout.Width(100)))
                {
                    CopyObjectPaths(visibleObjects);
                }

                if (GUILayout.Button("Ping 第一个", GUILayout.Width(82)))
                {
                    PingAndSelect(visibleObjects[0]);
                }

                if (GUILayout.Button("打开 Prefab", GUILayout.Width(86)))
                {
                    OpenFirstProjectPrefab(visibleObjects);
                }
            }

            if (GUILayout.Button("复制 Tag", GUILayout.Width(76)))
            {
                GUIUtility.systemCopyBuffer = selectedBucket.TagName;
                ShowNotification(new GUIContent("已复制 Tag"));
            }
        }

        /// <summary>
        /// 单个对象行。
        /// </summary>
        private void DrawObjectRow(int index, TaggedObjectInfo info)
        {
            if (info == null || info.GameObject == null) return;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.ExpandWidth(true));
            EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));

            var activeText = info.ActiveInHierarchy ? "Active" : "Inactive";
            GUILayout.Label((index + 1).ToString(), _mutedLabelStyle, GUILayout.Width(30));
            GUILayout.Label(GetSourceLabel(info), _badgeStyle, GUILayout.Width(62));
            GUILayout.Label(activeText, _badgeStyle, GUILayout.Width(60));

            if (GUILayout.Button(info.Name, _objectNameButtonStyle, GUILayout.MinWidth(80), GUILayout.ExpandWidth(true), GUILayout.Height(22)))
            {
                PingAndSelect(info);
            }

            if (GUILayout.Button("选中", GUILayout.Width(48)))
            {
                PingAndSelect(info);
            }

            using (new EditorGUI.DisabledScope(info.Source != SearchSource.ProjectPrefab))
            {
                if (GUILayout.Button("打开", GUILayout.Width(48)))
                {
                    OpenProjectPrefab(info);
                }
            }

            if (GUILayout.Button("复制路径", GUILayout.Width(72)))
            {
                CopyObjectPath(info);
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.LabelField(GetLocationPath(info), _pathLabelStyle, GUILayout.ExpandWidth(true));
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// 底部提示。固定在 Footer Rect 内，不会被列表挤压。
        /// </summary>
        private void DrawFooter(Rect rect)
        {
            GUILayout.BeginArea(rect, GUIContent.none, EditorStyles.helpBox);
            EditorGUILayout.LabelField("提示：范围选择 Project Prefab 后，会扫描 Project 下 Prefab 资产及其子节点的 Tag；点击“打开”可进入对应 Prefab。", _mutedLabelStyle);
            GUILayout.EndArea();
        }

        /// <summary>
        /// 搜索框。使用显式 Rect 绘制占位文本，避免依赖 LastRect 导致 Dock/缩放时错位。
        /// </summary>
        private string DrawSearchField(string placeholder, string value)
        {
            EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            EditorGUILayout.LabelField("搜索", GUILayout.Width(32));

            var rect = EditorGUILayout.GetControlRect(GUILayout.ExpandWidth(true), GUILayout.Height(EditorGUIUtility.singleLineHeight));
            value = EditorGUI.TextField(rect, value);

            if (string.IsNullOrEmpty(value))
            {
                var placeholderRect = rect;
                placeholderRect.x     += 4f;
                placeholderRect.width -= 8f;
                GUI.Label(placeholderRect, placeholder, _placeholderStyle);
            }

            using (new EditorGUI.DisabledScope(string.IsNullOrEmpty(value)))
            {
                if (GUILayout.Button("×", GUILayout.Width(22), GUILayout.Height(EditorGUIUtility.singleLineHeight)))
                {
                    value = string.Empty;
                    GUI.FocusControl(null);
                }
            }

            EditorGUILayout.EndHorizontal();
            return value;
        }

        /// <summary>
        /// 重新扫描目标范围，并按 Tag 分组。
        /// </summary>
        private void RefreshData(bool showNotification)
        {
            _tagBuckets.Clear();
            _totalObjectCount   = 0;
            _sceneObjectCount   = 0;
            _projectPrefabCount = 0;
            _projectObjectCount = 0;
            _needsRefresh       = false;

            var buckets = new Dictionary<string, TagBucket>(StringComparer.Ordinal);

            if (ShouldScanHierarchy())
            {
                CollectHierarchyObjects(buckets);
            }

            if (ShouldScanProjectPrefabs())
            {
                CollectProjectPrefabObjects(buckets);
            }

            foreach (var bucket in buckets.Values)
            {
                bucket.Objects.Sort(CompareObjectInfo);
                _totalObjectCount += bucket.Objects.Count;
                _tagBuckets.Add(bucket);
            }

            SortBuckets();
            EnsureSelectedTagValid();
            _lastRefreshTime = DateTime.Now.ToString("HH:mm:ss");

            if (showNotification)
            {
                ShowNotification(new GUIContent("Tag 数据已刷新"));
            }
        }

        /// <summary>
        /// 收集当前已经打开且加载完成的所有场景对象。
        /// </summary>
        private void CollectHierarchyObjects(Dictionary<string, TagBucket> buckets)
        {
            for (var i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (!scene.IsValid() || !scene.isLoaded) continue;

                var roots = scene.GetRootGameObjects();
                for (var j = 0; j < roots.Length; j++)
                {
                    CollectSceneGameObjectRecursive(roots[j].transform, buckets);
                }
            }
        }

        /// <summary>
        /// 递归收集场景对象。关闭“包含隐藏对象”时，只显示 Hierarchy 中实际激活的对象。
        /// </summary>
        private void CollectSceneGameObjectRecursive(Transform target, Dictionary<string, TagBucket> buckets)
        {
            if (target == null) return;

            var go = target.gameObject;
            if (_includeInactive || go.activeInHierarchy)
            {
                if (AddObjectToBucket(buckets, new TaggedObjectInfo
                    {
                        GameObject        = go,
                        PrefabAssetRoot   = null,
                        Name              = go.name,
                        HierarchyPath     = GetHierarchyPath(go.transform),
                        SceneName         = go.scene.IsValid() ? go.scene.name : "No Scene",
                        AssetPath         = string.Empty,
                        ActiveInHierarchy = go.activeInHierarchy,
                        Source            = SearchSource.Scene
                    }))
                {
                    _sceneObjectCount++;
                }
            }

            for (var i = 0; i < target.childCount; i++)
            {
                CollectSceneGameObjectRecursive(target.GetChild(i), buckets);
            }
        }

        /// <summary>
        /// 收集 Project 下所有 Prefab 资产及其子节点。
        /// 注意：Project 模式不会打开 .unity 场景文件，只扫描 Prefab，避免触发大量场景加载。
        /// </summary>
        private void CollectProjectPrefabObjects(Dictionary<string, TagBucket> buckets)
        {
            var prefabGuids  = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });
            var showProgress = prefabGuids.Length >= ProjectScanProgressThreshold;

            try
            {
                for (var i = 0; i < prefabGuids.Length; i++)
                {
                    var assetPath = AssetDatabase.GUIDToAssetPath(prefabGuids[i]);
                    if (string.IsNullOrEmpty(assetPath)) continue;

                    if (showProgress)
                    {
                        EditorUtility.DisplayProgressBar("扫描 Project Prefab Tag", assetPath, (float)i / prefabGuids.Length);
                    }

                    var prefabRoot = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                    if (prefabRoot == null) continue;

                    _projectPrefabCount++;
                    CollectProjectPrefabRecursive(prefabRoot.transform, prefabRoot, assetPath, buckets);
                }
            }
            finally
            {
                if (showProgress)
                {
                    EditorUtility.ClearProgressBar();
                }
            }
        }

        /// <summary>
        /// 递归收集 Prefab 内对象。关闭“包含隐藏对象”时，会排除自身或任意父级为 Inactive 的节点。
        /// </summary>
        private void CollectProjectPrefabRecursive(Transform target, GameObject prefabRoot, string assetPath, Dictionary<string, TagBucket> buckets)
        {
            if (target == null) return;

            var go                      = target.gameObject;
            var activeInPrefabHierarchy = IsActiveInPrefabHierarchy(target);
            if (_includeInactive || activeInPrefabHierarchy)
            {
                if (AddObjectToBucket(buckets, new TaggedObjectInfo
                    {
                        GameObject        = go,
                        PrefabAssetRoot   = prefabRoot,
                        Name              = go.name,
                        HierarchyPath     = GetHierarchyPathInPrefab(target, prefabRoot.transform),
                        SceneName         = string.Empty,
                        AssetPath         = assetPath,
                        ActiveInHierarchy = activeInPrefabHierarchy,
                        Source            = SearchSource.ProjectPrefab
                    }))
                {
                    _projectObjectCount++;
                }
            }

            for (var i = 0; i < target.childCount; i++)
            {
                CollectProjectPrefabRecursive(target.GetChild(i), prefabRoot, assetPath, buckets);
            }
        }

        /// <summary>
        /// 将对象加入对应 Tag 分组。
        /// </summary>
        private bool AddObjectToBucket(Dictionary<string, TagBucket> buckets, TaggedObjectInfo info)
        {
            if (info == null || info.GameObject == null) return false;

            var tagName = GetSafeTag(info.GameObject);
            if (!_includeUntagged && string.Equals(tagName, UntaggedName, StringComparison.Ordinal)) return false;

            TagBucket bucket;
            if (!buckets.TryGetValue(tagName, out bucket))
            {
                bucket = new TagBucket { TagName = tagName };
                buckets.Add(tagName, bucket);
            }

            bucket.Objects.Add(info);
            return true;
        }

        private int CompareObjectInfo(TaggedObjectInfo a, TaggedObjectInfo b)
        {
            var sourceCompare = a.Source.CompareTo(b.Source);
            if (sourceCompare != 0) return sourceCompare;

            var locationCompare = string.Compare(GetLocationPath(a), GetLocationPath(b), StringComparison.OrdinalIgnoreCase);
            if (locationCompare != 0) return locationCompare;

            return string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 对 Tag 分组进行排序。
        /// </summary>
        private void SortBuckets()
        {
            switch (_sortMode)
            {
                case SortMode.TagNameAsc:
                    _tagBuckets.Sort((a, b) => string.Compare(a.TagName, b.TagName, StringComparison.OrdinalIgnoreCase));
                    break;
                case SortMode.TagNameDesc:
                    _tagBuckets.Sort((a, b) => string.Compare(b.TagName, a.TagName, StringComparison.OrdinalIgnoreCase));
                    break;
                case SortMode.CountDesc:
                    _tagBuckets.Sort((a, b) => b.Objects.Count.CompareTo(a.Objects.Count));
                    break;
                case SortMode.CountAsc:
                    _tagBuckets.Sort((a, b) => a.Objects.Count.CompareTo(b.Objects.Count));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Repaint();
        }

        /// <summary>
        /// 保证选中的 Tag 在刷新后仍然有效。
        /// </summary>
        private void EnsureSelectedTagValid()
        {
            if (_tagBuckets.Count == 0)
            {
                _selectedTagName = string.Empty;
                return;
            }

            if (_tagBuckets.Any(bucket => string.Equals(bucket.TagName, _selectedTagName, StringComparison.Ordinal))) return;

            _selectedTagName = _tagBuckets[0].TagName;
        }

        private TagBucket GetSelectedBucket()
        {
            if (string.IsNullOrEmpty(_selectedTagName)) return null;
            return _tagBuckets.FirstOrDefault(bucket => string.Equals(bucket.TagName, _selectedTagName, StringComparison.Ordinal));
        }

        private IEnumerable<TagBucket> GetVisibleTagBuckets()
        {
            if (string.IsNullOrWhiteSpace(_tagSearchText)) return _tagBuckets;

            return _tagBuckets.Where(bucket =>
                bucket.TagName.IndexOf(_tagSearchText, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private IEnumerable<TaggedObjectInfo> GetVisibleObjects(TagBucket bucket)
        {
            if (bucket == null) return Enumerable.Empty<TaggedObjectInfo>();
            if (string.IsNullOrWhiteSpace(_objectSearchText)) return bucket.Objects.Where(info => info.GameObject != null);

            return bucket.Objects.Where(info =>
                info.GameObject != null &&
                (ContainsIgnoreCase(info.Name, _objectSearchText) ||
                 ContainsIgnoreCase(info.HierarchyPath, _objectSearchText) ||
                 ContainsIgnoreCase(info.SceneName, _objectSearchText) ||
                 ContainsIgnoreCase(info.AssetPath, _objectSearchText) ||
                 ContainsIgnoreCase(GetSourceLabel(info), _objectSearchText)));
        }

        /// <summary>
        /// 选中并 Ping 指定对象。
        /// </summary>
        private void PingAndSelect(TaggedObjectInfo info)
        {
            if (info == null || info.GameObject == null) return;

            if (info.Source == SearchSource.Scene)
            {
                Selection.activeGameObject = info.GameObject;
                EditorGUIUtility.PingObject(info.GameObject);

                if (_frameOnSelect && SceneView.lastActiveSceneView != null)
                {
                    SceneView.lastActiveSceneView.FrameSelected();
                }

                return;
            }

            Selection.activeObject = info.GameObject;
            EditorGUIUtility.PingObject(info.GameObject);
        }

        /// <summary>
        /// 批量选择对象。可以同时选择 Scene 对象和 Project Prefab 内对象。
        /// </summary>
        private void SelectObjects(List<TaggedObjectInfo> infos)
        {
            if (infos == null || infos.Count == 0) return;

            Selection.objects = infos
                .Where(info => info != null && info.GameObject != null)
                .Select(info => info.GameObject)
                .Cast<UnityEngine.Object>()
                .ToArray();

            if (Selection.objects.Length <= 0) return;

            EditorGUIUtility.PingObject(Selection.objects[0]);

            var firstSceneObject = infos.FirstOrDefault(info => info != null && info.Source == SearchSource.Scene && info.GameObject != null);
            if (firstSceneObject != null && _frameOnSelect && SceneView.lastActiveSceneView != null)
            {
                Selection.activeGameObject = firstSceneObject.GameObject;
                SceneView.lastActiveSceneView.FrameSelected();
            }
        }

        /// <summary>
        /// 打开列表里的第一个 Project Prefab。
        /// </summary>
        private void OpenFirstProjectPrefab(List<TaggedObjectInfo> infos)
        {
            if (infos == null || infos.Count == 0) return;

            var firstPrefabInfo = infos.FirstOrDefault(info => info != null && info.Source == SearchSource.ProjectPrefab);
            if (firstPrefabInfo == null)
            {
                ShowNotification(new GUIContent("当前显示结果里没有 Project Prefab"));
                return;
            }

            OpenProjectPrefab(firstPrefabInfo);
        }

        /// <summary>
        /// 打开 Project Prefab 资产。
        /// </summary>
        private void OpenProjectPrefab(TaggedObjectInfo info)
        {
            if (info == null || info.Source != SearchSource.ProjectPrefab) return;

            var prefabRoot = info.PrefabAssetRoot;
            if (prefabRoot == null && !string.IsNullOrEmpty(info.AssetPath))
            {
                prefabRoot = AssetDatabase.LoadAssetAtPath<GameObject>(info.AssetPath);
            }

            if (prefabRoot == null)
            {
                ShowNotification(new GUIContent("Prefab 资产不存在"));
                return;
            }

            AssetDatabase.OpenAsset(prefabRoot);
            EditorGUIUtility.PingObject(info.GameObject != null ? info.GameObject : prefabRoot);
        }

        /// <summary>
        /// 复制单个对象路径。
        /// </summary>
        private void CopyObjectPath(TaggedObjectInfo info)
        {
            if (info == null) return;

            GUIUtility.systemCopyBuffer = GetLocationPath(info);
            ShowNotification(new GUIContent("已复制对象路径"));
        }

        /// <summary>
        /// 批量复制对象路径。
        /// </summary>
        private void CopyObjectPaths(List<TaggedObjectInfo> infos)
        {
            if (infos == null || infos.Count == 0) return;

            GUIUtility.systemCopyBuffer = string.Join("\n", infos
                .Where(info => info != null && info.GameObject != null)
                .Select(GetLocationPath));

            ShowNotification(new GUIContent("已复制对象路径列表"));
        }

        private bool ShouldScanHierarchy()
        {
            return _searchScope == SearchScope.Hierarchy || _searchScope == SearchScope.HierarchyAndProjectPrefabs;
        }

        private bool ShouldScanProjectPrefabs()
        {
            return _searchScope == SearchScope.ProjectPrefabs || _searchScope == SearchScope.HierarchyAndProjectPrefabs;
        }

        private string GetSearchScopeDisplayName()
        {
            return SearchScopeLabels[(int)_searchScope];
        }

        private static string GetSourceLabel(TaggedObjectInfo info)
        {
            if (info == null) return string.Empty;
            return info.Source == SearchSource.Scene ? "Scene" : "Project";
        }

        private static string GetLocationPath(TaggedObjectInfo info)
        {
            if (info == null) return string.Empty;

            if (info.Source == SearchSource.ProjectPrefab)
            {
                return string.IsNullOrEmpty(info.AssetPath)
                           ? info.HierarchyPath
                           : $"{info.AssetPath}/{info.HierarchyPath}";
            }

            return $"{info.SceneName}/{info.HierarchyPath}";
        }

        private static bool ContainsIgnoreCase(string source, string value)
        {
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(value)) return false;
            return source.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        /// <summary>
        /// 获取 GameObject 当前 Tag，避免异常导致窗口中断。
        /// </summary>
        private static string GetSafeTag(GameObject go)
        {
            if (go == null) return MissingTagName;

            try
            {
                return go.tag;
            }
            catch
            {
                return MissingTagName;
            }
        }

        /// <summary>
        /// 获取 Transform 在 Hierarchy 中的完整路径。
        /// </summary>
        private static string GetHierarchyPath(Transform target)
        {
            if (target == null) return string.Empty;

            var names   = new Stack<string>();
            var current = target;
            while (current != null)
            {
                names.Push(current.name);
                current = current.parent;
            }

            return string.Join("/", names.ToArray());
        }

        /// <summary>
        /// 获取 Prefab 内部相对路径，路径从 Prefab 根节点开始。
        /// </summary>
        private static string GetHierarchyPathInPrefab(Transform target, Transform prefabRoot)
        {
            if (target == null) return string.Empty;
            if (prefabRoot == null) return GetHierarchyPath(target);

            var names   = new Stack<string>();
            var current = target;
            while (current != null)
            {
                names.Push(current.name);
                if (current == prefabRoot) break;
                current = current.parent;
            }

            return string.Join("/", names.ToArray());
        }

        /// <summary>
        /// 判断 Prefab 节点在 Prefab 内部层级是否实际激活。
        /// Project 资产没有真正的 activeInHierarchy，所以这里按 activeSelf 和父级 activeSelf 推导。
        /// </summary>
        private static bool IsActiveInPrefabHierarchy(Transform target)
        {
            var current = target;
            while (current != null)
            {
                if (!current.gameObject.activeSelf) return false;
                current = current.parent;
            }

            return true;
        }

        /// <summary>
        /// 初始化 GUI 样式。样式只创建一次，避免 OnGUI 中频繁 new GUIStyle / Texture。
        /// </summary>
        private void EnsureStyles()
        {
            if (_titleStyle != null) return;

            _titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize    = 18,
                alignment   = TextAnchor.MiddleLeft,
                fixedHeight = 26
            };

            _sectionTitleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize  = 13,
                alignment = TextAnchor.MiddleLeft
            };

            _mutedLabelStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                wordWrap = true,
                normal   = { textColor = EditorGUIUtility.isProSkin ? new Color(0.72f, 0.72f, 0.72f) : new Color(0.35f, 0.35f, 0.35f) }
            };

            _pathLabelStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                wordWrap = true,
                normal   = { textColor = EditorGUIUtility.isProSkin ? new Color(0.58f, 0.72f, 0.95f) : new Color(0.22f, 0.34f, 0.56f) }
            };

            _tagButtonStyle = new GUIStyle(EditorStyles.miniButton)
            {
                alignment = TextAnchor.MiddleLeft,
                fontStyle = FontStyle.Normal
            };

            _selectedTagButtonStyle = new GUIStyle(EditorStyles.miniButton)
            {
                alignment = TextAnchor.MiddleLeft,
                fontStyle = FontStyle.Bold
            };

            _objectNameButtonStyle = new GUIStyle(EditorStyles.miniButton)
            {
                alignment = TextAnchor.MiddleLeft,
                fontStyle = FontStyle.Bold
            };

            _badgeStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                normal    = { textColor = EditorGUIUtility.isProSkin ? new Color(0.9f, 0.9f, 0.9f) : new Color(0.2f, 0.2f, 0.2f) }
            };

            _placeholderStyle = new GUIStyle(EditorStyles.centeredGreyMiniLabel)
            {
                alignment = TextAnchor.MiddleLeft,
                padding   = new RectOffset(3, 3, 0, 0)
            };

            _miniToolbarButtonStyle = new GUIStyle(EditorStyles.toolbarButton)
            {
                alignment   = TextAnchor.MiddleCenter,
                fixedHeight = 0
            };
        }
    }
}
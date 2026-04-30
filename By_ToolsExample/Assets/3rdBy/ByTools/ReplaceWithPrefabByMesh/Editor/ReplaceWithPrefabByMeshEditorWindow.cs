namespace _3rdBy.ByTools.ReplaceWithPrefabByMesh.Editor
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEditorInternal;
    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.SceneManagement;

    /// <summary>
    /// Prefab 场景替换与批量移动工具。
    ///
    /// 设计目标：
    /// 1. 先扫描、再预览、再执行，避免直接批量修改场景。
    /// 2. 执行前会再次校验对象是否仍然匹配，避免扫描后对象发生变化导致误操作。
    /// 3. 替换失败时会回滚新生成的对象，避免场景残留半成品。
    /// 4. 批量移动会校验目标父节点，避免移动到自身子级或移动到 Project 资源下。
    /// 5. 所有执行操作都会进入同一个 Undo 组，方便 Ctrl + Z 撤销。
    ///
    /// 注意：
    /// 该脚本必须放在 Editor 目录下。
    /// </summary>
    public class ReplaceWithPrefabByMeshEditorWindow : EditorWindow
    {
        private enum ScanScope
        {
            ActiveScene,
            LoadedScenes,
            SelectionChildren
        }

        private enum MeshCompareMode
        {
            Reference,
            Name,
            VertexTriangleCount,
            NameVertexTriangleCount,
            Bounds,
            NameVertexTriangleBounds
        }

        private enum StructureCompareMode
        {
            StrictNameAndChildCount,
            AllowExtraSceneChildrenByName,
            IgnoreChildNameAllowExtraSceneChildren
        }

        private enum NameMatchMode
        {
            Exact,
            Contains,
            StartsWith,
            EndsWith,
            Wildcard,
            Regex
        }

        /// <summary>
        /// 场景对象列表项。
        /// Selected 用于让用户手动排除误匹配项。
        /// CachedPath 用于对象被删除后仍然能在日志中看到原路径。
        /// </summary>
        private sealed class SceneObjectEntry
        {
            public GameObject Target;
            public bool Selected;
            public string CachedPath;
            public string Info;
            public string Warning;

            public SceneObjectEntry(GameObject target, string info, string warning)
            {
                Target = target;
                Selected = true;
                CachedPath = target != null ? GetPath(target.transform) : "<Null>";
                Info = info;
                Warning = warning;
            }
        }

        /// <summary>
        /// Mesh 匹配快照。
        /// 不直接在匹配时反复读取 Prefab 组件，避免重复 GC 与逻辑分散。
        /// </summary>
        private sealed class MeshSnapshot
        {
            public Mesh Mesh;
            public string Name;
            public int VertexCount;
            public int TriangleCount;
            public Bounds Bounds;
            public string[] MaterialNames;

            public bool HasMesh => Mesh != null;
        }

        /// <summary>
        /// 执行报告。仅输出到 Console，避免工具静默失败。
        /// </summary>
        private sealed class ExecutionReport
        {
            public int Success;
            public int Failed;
            public int Skipped;
            public readonly List<string> Messages = new List<string>();

            public void AddMessage(string msg)
            {
                if (!string.IsNullOrEmpty(msg))
                    Messages.Add(msg);
            }

            public void Print(string title, bool verbose)
            {
                Debug.Log($"[{title}] 完成。成功：{Success}，失败：{Failed}，跳过：{Skipped}。");

                if (!verbose || Messages.Count == 0)
                    return;

                Debug.Log($"[{title}] 详细报告：\n" + string.Join("\n", Messages));
            }
        }

        [SerializeField, Header("Prefab 替换 - 目标 Prefab")]
        private GameObject _prefab;

        [SerializeField, Header("通用扫描选项")]
        private bool _dryRun = true;

        [SerializeField]
        private bool _includeInactive = true;

        [SerializeField]
        private ScanScope _scanScope = ScanScope.ActiveScene;

        [SerializeField]
        private bool _useLayerFilter;

        [SerializeField]
        private int _scanLayerMask = -1;

        [SerializeField]
        private bool _useTagFilter;

        [SerializeField]
        private string _scanTag = "Untagged";

        [SerializeField, Header("Prefab 替换 - 匹配规则")]
        private bool _matchMeshFilters = true;

        [SerializeField]
        private bool _matchSkinnedMeshes = true;

        [SerializeField]
        private MeshCompareMode _meshCompareMode = MeshCompareMode.Reference;

        [SerializeField]
        private bool _matchMaterials;

        [SerializeField]
        private bool _usePrefabStructure;

        [SerializeField]
        private StructureCompareMode _structureCompareMode = StructureCompareMode.StrictNameAndChildCount;

        [SerializeField]
        private bool _overrideParentPrefabCheck;

        [SerializeField]
        private bool _ignoreNestedReplaceMatches = true;

        [SerializeField, Header("Prefab 替换 - 执行选项")]
        private bool _keepOriginalName = true;

        [SerializeField]
        private bool _parentToOriginalParent = true;

        [SerializeField]
        private bool _copyLayer = true;

        [SerializeField]
        private bool _copyLayerRecursively;

        [SerializeField]
        private bool _copyStaticFlags = true;

        [SerializeField]
        private bool _copyStaticFlagsRecursively;

        [SerializeField]
        private bool _copyRendererSettings;

        [SerializeField]
        private bool _copyLightmapSettings;

        [SerializeField]
        private bool _copyRootColliders;

        [SerializeField]
        private bool _copyRootMonoBehaviours;

        [SerializeField]
        private bool _breakPrefabLink;

        [SerializeField, Header("批量移动 - 目标与名称匹配")]
        private GameObject _moveTargetParent;

        [SerializeField]
        private string _targetName;

        [SerializeField]
        private NameMatchMode _nameMatchMode = NameMatchMode.Exact;

        [SerializeField]
        private bool _ignoreNameCase = true;

        [SerializeField]
        private bool _ignoreNestedMoveMatches = true;

        [SerializeField, Header("批量移动 - 执行选项")]
        private bool _moveKeepWorldTransform = true;

        [SerializeField]
        private bool _deleteEmptyParent;

        [SerializeField]
        private bool _recursiveDeleteEmptyParent = true;

        [SerializeField]
        private bool _protectSceneRootWhenDeleteEmptyParent = true;

        [SerializeField]
        private string _protectedEmptyParentNames = "Root;Environment;Level;Static;Props";

        [SerializeField, Header("窗口显示与日志")]
        private bool _showReplaceSection = true;

        [SerializeField]
        private bool _showMoveSection = true;

        [SerializeField]
        private bool _printVerboseReport = true;

        private Vector2 _mainScroll;
        private Vector2 _replaceScroll;
        private Vector2 _moveScroll;

        [SerializeField, Header("窗口列表过滤")]
        private string _replaceListFilter = string.Empty;

        [SerializeField]
        private bool _replaceListOnlySelected;

        [SerializeField]
        private string _moveListFilter = string.Empty;

        [SerializeField]
        private bool _moveListOnlySelected;

        [SerializeField]
        private int _maxVisibleListItems = 300;

        private bool _pendingReplaceConfirm;
        private bool _pendingMoveConfirm;
        private GUIStyle _tipsStyle;
        private GUIStyle _topTitleStyle;
        private GUIStyle _topSubtitleStyle;
        private GUIStyle _topPillStyle;
        private GUIStyle _cardStyle;
        private GUIStyle _sectionTitleStyle;
        private GUIStyle _subTitleStyle;
        private GUIStyle _toolbarButtonStyle;

        private readonly List<SceneObjectEntry> _replaceEntries = new List<SceneObjectEntry>();
        private readonly List<SceneObjectEntry> _moveEntries = new List<SceneObjectEntry>();
        private readonly List<MeshSnapshot> _prefabMeshSnapshots = new List<MeshSnapshot>();

        // 替换扫描快照：用于防止扫描后修改关键参数仍然直接执行。
        private GameObject _lastReplaceScanPrefab;
        private bool _lastReplaceScanIncludeInactive;
        private ScanScope _lastReplaceScanScope;
        private bool _lastReplaceUseLayerFilter;
        private int _lastReplaceScanLayerMask;
        private bool _lastReplaceUseTagFilter;
        private string _lastReplaceScanTag;
        private bool _lastReplaceMatchMeshFilters;
        private bool _lastReplaceMatchSkinnedMeshes;
        private MeshCompareMode _lastReplaceMeshCompareMode;
        private bool _lastReplaceMatchMaterials;
        private bool _lastReplaceUsePrefabStructure;
        private StructureCompareMode _lastReplaceStructureCompareMode;
        private bool _lastReplaceOverrideParentPrefabCheck;
        private bool _lastReplaceIgnoreNestedMatches;

        // 移动扫描快照：用于防止扫描后修改名称规则或范围仍然直接执行。
        private string _lastMoveScanTargetName;
        private NameMatchMode _lastMoveNameMatchMode;
        private bool _lastMoveIgnoreNameCase;
        private bool _lastMoveScanIncludeInactive;
        private ScanScope _lastMoveScanScope;
        private bool _lastMoveUseLayerFilter;
        private int _lastMoveScanLayerMask;
        private bool _lastMoveUseTagFilter;
        private string _lastMoveScanTag;
        private bool _lastMoveIgnoreNestedMatches;

        [MenuItem("ByTools/🥏 Prefab场景替换与批量移动 &%4", false, 1001)]
        private static void OpenWindow()
        {
            var w = GetWindow<ReplaceWithPrefabByMeshEditorWindow>("Replace By Prefab Mesh");
            w.minSize = new Vector2(720, 760);
        }

        private void OnEnable()
        {
            // 不在 OnEnable 中初始化 GUIStyle。
            // Unity 的 GUI.skin / EditorStyles 只能在 OnGUI 调用链中安全访问，
            // 否则会触发：You can only call GUI functions from inside OnGUI。
        }

        private void InitStyles()
        {
            _tipsStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 10,
                wordWrap = true,
                normal = { textColor = Color.gray },
                richText = true
            };

            _topTitleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize  = 18,
                alignment = TextAnchor.UpperLeft,
                normal    = { textColor = Color.white },
                richText  = true
            };

            _topSubtitleStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 11,
                wordWrap = true,
                normal   = { textColor = new Color(0.86f, 0.9f, 0.94f) },
                richText = true
            };

            _topPillStyle = new GUIStyle(EditorStyles.miniBoldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                normal    = { textColor = Color.white },
                richText  = true
            };

            _cardStyle = new GUIStyle(EditorStyles.helpBox)
            {
                padding  = new RectOffset(10, 10, 8, 10),
                margin   = new RectOffset(4, 4, 6, 6),
                richText = true
            };

            _sectionTitleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize    = 13,
                fixedHeight = 22,
                richText    = true
            };

            _subTitleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 11,
                normal   = { textColor = EditorGUIUtility.isProSkin ? new Color(0.82f, 0.88f, 1f) : new Color(0.12f, 0.22f, 0.38f) },
                richText = true
            };

            _toolbarButtonStyle = new GUIStyle(GUI.skin.button)
            {
                fixedHeight = 26,
                fontStyle   = FontStyle.Bold,
                richText    = true
            };
        }

        private void OnGUI()
        {
            if (_tipsStyle == null)
                InitStyles();

            DrawTopBanner();

            _mainScroll = EditorGUILayout.BeginScrollView(_mainScroll);
            DrawUseTips();
            DrawCommonOptions();
            DrawReplaceSection();
            DrawMoveSection();
            EditorGUILayout.Space(8);
            EditorGUILayout.EndScrollView();

            DrawBottomStatusBar();
        }

        /// <summary>
        /// 顶部横幅：集中展示工具用途、当前执行模式和扫描结果数量，让窗口打开后第一眼就能看懂状态。
        /// </summary>
        private void DrawTopBanner()
        {
            var rect = GUILayoutUtility.GetRect(0, 76, GUILayout.ExpandWidth(true));
            var bg = EditorGUIUtility.isProSkin ? new Color(0.10f, 0.16f, 0.22f) : new Color(0.18f, 0.32f, 0.46f);
            EditorGUI.DrawRect(rect, bg);

            var padding = 14f;
            var titleRect = new Rect(rect.x + padding, rect.y + 10, rect.width - 180, 24);
            var subRect = new Rect(rect.x + padding, rect.y + 38, rect.width - 190, 32);
            GUI.Label(titleRect, "🥏 Prefab 场景替换与批量移动", _topTitleStyle);
            GUI.Label(subRect, "先扫描、再勾选、再执行；执行前二次校验，支持 Undo 与执行报告。", _topSubtitleStyle);

            var pillRect = new Rect(rect.xMax - 146, rect.y + 12, 120, 24);
            EditorGUI.DrawRect(pillRect, _dryRun ? new Color(0.90f, 0.55f, 0.12f) : new Color(0.72f, 0.18f, 0.18f));
            GUI.Label(pillRect, _dryRun ? "DRY RUN" : "LIVE MODE", _topPillStyle);

            var countRect = new Rect(rect.xMax - 172, rect.y + 43, 150, 22);
            GUI.Label(countRect, $"替换 {_replaceEntries.Count}  |  移动 {_moveEntries.Count}", _topSubtitleStyle);
        }

        /// <summary>
        /// 底部状态栏：始终显示当前 Prefab、扫描范围和执行模式，滚动页面后也能看到关键状态。
        /// </summary>
        private void DrawBottomStatusBar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label(_prefab != null ? $"Prefab: {_prefab.name}" : "Prefab: 未选择", EditorStyles.miniLabel, GUILayout.MinWidth(180));
            GUILayout.Label($"扫描范围: {_scanScope}", EditorStyles.miniLabel, GUILayout.MinWidth(130));
            GUILayout.FlexibleSpace();
            GUILayout.Label(_dryRun ? "当前安全模式：干运行" : "当前会修改场景：可 Undo", EditorStyles.miniBoldLabel);
            EditorGUILayout.EndHorizontal();
        }

        private void DrawUseTips()
        {
            BeginCard("使用流程", "建议大批量执行前先保存场景；如果扫描参数发生变化，工具会要求重新扫描。 ");
            EditorGUILayout.LabelField("① 选择 Project 中的 Prefab / Model 资源。", _tipsStyle);
            EditorGUILayout.LabelField("② 点击扫描，检查匹配列表，可通过勾选、筛选、移除控制执行范围。", _tipsStyle);
            EditorGUILayout.LabelField("③ 取消干运行后执行。执行前会二次校验，失败会回滚新对象。", _tipsStyle);
            EditorGUILayout.Space(2);
            EditorGUILayout.HelpBox("Play Mode 下不可执行；替换和移动都会进入同一个 Undo 组，方便 Ctrl + Z 撤销。", MessageType.Info);
            EndCard();
        }

        private void DrawCommonOptions()
        {
            BeginCard("通用扫描与执行模式", "这里的选项会影响替换和移动两个模块。 ");

            EditorGUI.BeginChangeCheck();
            _dryRun = EditorGUILayout.ToggleLeft("干运行（仅扫描和预览，不真正执行替换 / 移动）", _dryRun);
            _includeInactive = EditorGUILayout.ToggleLeft("扫描时包括未激活对象", _includeInactive);
            _scanScope = (ScanScope)EditorGUILayout.EnumPopup("扫描范围", _scanScope);

            EditorGUILayout.Space(3);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Layer 过滤", _subTitleStyle);
            _useLayerFilter = EditorGUILayout.ToggleLeft("启用 Layer 过滤", _useLayerFilter);
            using (new EditorGUI.DisabledScope(!_useLayerFilter))
                _scanLayerMask = DrawLayerMaskField("扫描 Layer", _scanLayerMask);
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Tag 过滤", _subTitleStyle);
            _useTagFilter = EditorGUILayout.ToggleLeft("启用 Tag 过滤", _useTagFilter);
            using (new EditorGUI.DisabledScope(!_useTagFilter))
                _scanTag = EditorGUILayout.TagField("扫描 Tag", _scanTag);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            if (EditorGUI.EndChangeCheck())
            {
                InvalidateReplaceCache();
                InvalidateMoveCache();
            }

            EditorGUILayout.Space(3);
            EditorGUILayout.BeginHorizontal();
            _printVerboseReport = EditorGUILayout.ToggleLeft("执行后输出详细报告到 Console", _printVerboseReport, GUILayout.MinWidth(230));
            GUILayout.Space(8);
            EditorGUILayout.LabelField("列表最大显示数量", GUILayout.Width(100));
            _maxVisibleListItems = Mathf.Clamp(EditorGUILayout.IntField(_maxVisibleListItems, GUILayout.Width(80)), 50, 5000);
            EditorGUILayout.EndHorizontal();

            DrawScanScopeTips();
            EndCard();
        }

        private void DrawScanScopeTips()
        {
            switch (_scanScope)
            {
                case ScanScope.ActiveScene:
                    EditorGUILayout.LabelField("<color=#ff9900>[Tips] 只扫描当前 Active Scene。</color>", _tipsStyle);
                    break;
                case ScanScope.LoadedScenes:
                    EditorGUILayout.LabelField("<color=#ff9900>[Tips] 扫描所有已加载且有效的 Scene。</color>", _tipsStyle);
                    break;
                case ScanScope.SelectionChildren:
                    EditorGUILayout.LabelField("<color=#ff9900>[Tips] 只扫描 Hierarchy 当前选中对象及其子对象。</color>", _tipsStyle);
                    break;
            }
        }

        private void BeginCard(string title, string subtitle = null)
        {
            EditorGUILayout.BeginVertical(_cardStyle);
            EditorGUILayout.LabelField(title, _sectionTitleStyle);
            if (!string.IsNullOrEmpty(subtitle))
                EditorGUILayout.LabelField(subtitle, _tipsStyle);
        }

        private void EndCard()
        {
            EditorGUILayout.EndVertical();
        }

        private void DrawSubCardTitle(string title, string tips = null)
        {
            EditorGUILayout.LabelField(title, _subTitleStyle);
            if (!string.IsNullOrEmpty(tips))
                EditorGUILayout.LabelField(tips, _tipsStyle);
        }

        private void DrawActionButtonRow(string scanLabel, Action scanAction, string revalidateLabel, Action revalidateAction, string executeLabel, Action executeAction, bool disableAll)
        {
            using (new EditorGUI.DisabledScope(disableAll))
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button(scanLabel, _toolbarButtonStyle))
                    scanAction?.Invoke();

                if (GUILayout.Button(revalidateLabel, _toolbarButtonStyle))
                    revalidateAction?.Invoke();

                using (new EditorGUI.DisabledScope(_dryRun))
                {
                    if (GUILayout.Button(executeLabel, _toolbarButtonStyle))
                        executeAction?.Invoke();
                }

                EditorGUILayout.EndHorizontal();
            }

            if (_dryRun)
                EditorGUILayout.LabelField("<color=#ff9900>[Tips] 当前为干运行模式，执行按钮已禁用。取消勾选后才可真正执行。</color>", _tipsStyle);
        }

        #region Prefab 替换场景对象

        private void DrawReplaceSection()
        {
            BeginCard($"Prefab 代替 Mesh    匹配 {_replaceEntries.Count} / 勾选 {GetSelectedEntries(_replaceEntries).Count}", "根据目标 Prefab 的 Mesh 或结构，扫描并替换场景对象。 ");

            _showReplaceSection = EditorGUILayout.Foldout(_showReplaceSection, "展开替换模块", true);
            if (!_showReplaceSection)
            {
                EndCard();
                return;
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            DrawSubCardTitle("目标资源", "只能选择 Project 中的 Prefab / Model 资源，不能选择 Hierarchy 中的场景对象。 ");
            _prefab = (GameObject)EditorGUILayout.ObjectField("目标 Prefab", _prefab, typeof(GameObject), false);
            EditorGUILayout.EndVertical();
            if (EditorGUI.EndChangeCheck())
                InvalidateReplaceCache();

            DrawReplaceScanOptions();
            DrawReplaceExecuteOptions();

            EditorGUILayout.Space(4);
            DrawActionButtonRow(
                "扫描场景",
                FindAndListReplaceMatches,
                "重新校验列表",
                () => RevalidateReplaceEntries(true),
                "执行替换",
                () => _pendingReplaceConfirm = true,
                _prefab == null);

            DrawReplaceMatchList();

            if (_pendingReplaceConfirm)
            {
                _pendingReplaceConfirm = false;
                EditorApplication.delayCall += TryReplaceWithConfirm;
            }

            EndCard();
        }

        private void DrawReplaceScanOptions()
        {
            EditorGUILayout.Space(4);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            DrawSubCardTitle("扫描 / 匹配选项");

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
            _matchMeshFilters = EditorGUILayout.ToggleLeft("匹配 MeshFilter", _matchMeshFilters);
            _matchSkinnedMeshes = EditorGUILayout.ToggleLeft("匹配 SkinnedMeshRenderer", _matchSkinnedMeshes);
            _meshCompareMode = (MeshCompareMode)EditorGUILayout.EnumPopup("Mesh 比较方式", _meshCompareMode);
            _matchMaterials = EditorGUILayout.ToggleLeft("同时比较材质名称", _matchMaterials);
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical();
            _usePrefabStructure = EditorGUILayout.ToggleLeft("按 Prefab 结构匹配", _usePrefabStructure);
            using (new EditorGUI.DisabledScope(!_usePrefabStructure))
                _structureCompareMode = (StructureCompareMode)EditorGUILayout.EnumPopup("结构比较方式", _structureCompareMode);

            _ignoreNestedReplaceMatches = EditorGUILayout.ToggleLeft("父子同时命中时只保留最外层对象", _ignoreNestedReplaceMatches);
            using (new EditorGUI.DisabledScope(_usePrefabStructure))
                _overrideParentPrefabCheck = EditorGUILayout.ToggleLeft("允许替换 Prefab Instance 内部子对象", _overrideParentPrefabCheck);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            if (!_usePrefabStructure)
                EditorGUILayout.LabelField("<color=#ff9900>[Tips] 默认不建议允许替换 Prefab Instance 内部子对象，避免把完整 Prefab 的内部 Mesh 子物体单独替换掉。</color>", _tipsStyle);

            if (EditorGUI.EndChangeCheck())
                InvalidateReplaceCache();

            EditorGUILayout.EndVertical();
        }

        private void DrawReplaceExecuteOptions()
        {
            EditorGUILayout.Space(4);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            DrawSubCardTitle("替换执行选项", "这些选项只影响真正执行替换时的属性继承和 Prefab 处理方式。 ");

            _parentToOriginalParent = EditorGUILayout.ToggleLeft("保持原层级 Parent", _parentToOriginalParent);
            EditorGUILayout.LabelField("<color=#ff9900>[Tips] 勾选后保持 localPosition / localRotation / localScale；不勾选时保持世界坐标并放到场景根节点。</color>", _tipsStyle);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
            _keepOriginalName = EditorGUILayout.ToggleLeft("保持原名称", _keepOriginalName);
            _copyLayer = EditorGUILayout.ToggleLeft("继承原对象 Layer", _copyLayer);
            using (new EditorGUI.DisabledScope(!_copyLayer))
                _copyLayerRecursively = EditorGUILayout.ToggleLeft("递归继承 Layer 到所有子对象", _copyLayerRecursively);
            _copyStaticFlags = EditorGUILayout.ToggleLeft("继承原对象 Static 标记", _copyStaticFlags);
            using (new EditorGUI.DisabledScope(!_copyStaticFlags))
                _copyStaticFlagsRecursively = EditorGUILayout.ToggleLeft("递归继承 Static 标记到所有子对象", _copyStaticFlagsRecursively);
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical();
            _copyRendererSettings = EditorGUILayout.ToggleLeft("继承 Renderer 常用渲染设置", _copyRendererSettings);
            _copyLightmapSettings = EditorGUILayout.ToggleLeft("继承 Lightmap 设置", _copyLightmapSettings);
            _copyRootColliders = EditorGUILayout.ToggleLeft("复制原根节点 Collider 组件", _copyRootColliders);
            _copyRootMonoBehaviours = EditorGUILayout.ToggleLeft("复制原根节点 MonoBehaviour 组件", _copyRootMonoBehaviours);
            _breakPrefabLink = EditorGUILayout.ToggleLeft("替换后解体 Prefab 关联", _breakPrefabLink);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void DrawReplaceMatchList()
        {
            RemoveInvalidEntries(_replaceEntries);
            if (_replaceEntries.Count <= 0)
                return;

            EditorGUILayout.Space(4);
            DrawEntryToolbar(_replaceEntries, "替换匹配对象列表", ref _replaceListFilter, ref _replaceListOnlySelected);

            var shown = 0;
            var visibleIndex = 0;
            _replaceScroll = EditorGUILayout.BeginScrollView(_replaceScroll, GUILayout.Height(260));
            for (var i = 0; i < _replaceEntries.Count; i++)
            {
                var entry = _replaceEntries[i];
                if (!IsEntryVisible(entry, _replaceListFilter, _replaceListOnlySelected))
                    continue;

                visibleIndex++;
                if (shown >= _maxVisibleListItems)
                    continue;

                shown++;
                DrawEntryRow(_replaceEntries, i, visibleIndex);
            }

            EditorGUILayout.EndScrollView();

            if (visibleIndex > shown)
                EditorGUILayout.HelpBox($"当前筛选后共有 {visibleIndex} 条，只显示前 {shown} 条。可以提高“列表最大显示数量”或继续输入筛选关键字。", MessageType.Info);
        }

        private void FindAndListReplaceMatches()
        {
            _replaceEntries.Clear();

            if (!ValidatePrefabForScan())
                return;

            CollectPrefabMeshSnapshots();
            if (_prefabMeshSnapshots.Count == 0)
            {
                EditorUtility.DisplayDialog("提示", "Prefab 中没有可用于匹配的 Mesh，请检查 MeshFilter / SkinnedMeshRenderer 扫描选项。", "OK");
                return;
            }

            foreach (var t in GetCandidateTransforms())
            {
                if (t == null || !PassesCommonScanFilters(t.gameObject))
                    continue;

                var go = t.gameObject;
                var matched = _usePrefabStructure ? IsStructureMatch(go, _prefab) : IsSingleMeshMatch(go);
                if (!matched)
                    continue;

                if (!_usePrefabStructure && !_overrideParentPrefabCheck && IsParentPrefabInstanceWithMesh(go))
                    continue;

                _replaceEntries.Add(new SceneObjectEntry(go, BuildObjectInfo(go), BuildReplaceWarning(go)));
            }

            if (_ignoreNestedReplaceMatches)
                RemoveNestedEntries(_replaceEntries);

            SaveReplaceScanSnapshot();
            Debug.Log($"[ReplaceByPrefabMesh] 找到 {_replaceEntries.Count} 个匹配对象。");
        }

        private bool ValidatePrefabForScan()
        {
            if (_prefab == null)
            {
                EditorUtility.DisplayDialog("提示", "请先选择一个 Prefab。", "OK");
                return false;
            }

            if (!AssetDatabase.Contains(_prefab))
            {
                EditorUtility.DisplayDialog("提示", "请选择 Project 中的 Prefab / Model 资源，不能选择场景对象。", "OK");
                return false;
            }

            if (!_matchMeshFilters && !_matchSkinnedMeshes)
            {
                EditorUtility.DisplayDialog("提示", "请至少勾选一种 Mesh 类型：MeshFilter 或 SkinnedMeshRenderer。", "OK");
                return false;
            }

            if (_scanScope == ScanScope.SelectionChildren && Selection.gameObjects.Length == 0)
            {
                EditorUtility.DisplayDialog("提示", "当前扫描范围为“选中对象及子对象”，请先在 Hierarchy 中选择至少一个对象。", "OK");
                return false;
            }

            return true;
        }

        private void CollectPrefabMeshSnapshots()
        {
            _prefabMeshSnapshots.Clear();
            if (_prefab == null)
                return;

            if (_matchMeshFilters)
            {
                foreach (var mf in _prefab.GetComponentsInChildren<MeshFilter>(true))
                {
                    if (mf == null || mf.sharedMesh == null)
                        continue;

                    var renderer = mf.GetComponent<Renderer>();
                    _prefabMeshSnapshots.Add(CreateMeshSnapshot(mf.sharedMesh, renderer));
                }
            }

            if (_matchSkinnedMeshes)
            {
                foreach (var sk in _prefab.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                {
                    if (sk == null || sk.sharedMesh == null)
                        continue;

                    _prefabMeshSnapshots.Add(CreateMeshSnapshot(sk.sharedMesh, sk));
                }
            }
        }

        private bool IsSingleMeshMatch(GameObject go)
        {
            foreach (var sceneSnapshot in GetNodeMeshSnapshots(go))
            {
                foreach (var prefabSnapshot in _prefabMeshSnapshots)
                {
                    if (AreMeshSnapshotsMatch(sceneSnapshot, prefabSnapshot))
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 判断当前对象是否位于某个 Prefab Instance 内部，且该 Prefab Instance 内含 Mesh。
        /// 默认跳过这类对象，避免把一个完整 Prefab 的内部子 Mesh 单独替换掉。
        /// </summary>
        private bool IsParentPrefabInstanceWithMesh(GameObject go)
        {
            var parent = go.transform.parent;
            while (parent != null)
            {
                if (PrefabUtility.IsPartOfPrefabInstance(parent.gameObject))
                {
                    var prefabRoot = PrefabUtility.GetNearestPrefabInstanceRoot(parent.gameObject);
                    if (prefabRoot != null && prefabRoot != go && HasAnyMesh(prefabRoot))
                        return true;
                }

                parent = parent.parent;
            }

            return false;
        }

        private bool HasAnyMesh(GameObject go)
        {
            if (_matchMeshFilters && go.GetComponentInChildren<MeshFilter>(true) != null)
                return true;

            if (_matchSkinnedMeshes && go.GetComponentInChildren<SkinnedMeshRenderer>(true) != null)
                return true;

            return false;
        }

        /// <summary>
        /// Prefab 结构匹配入口。
        /// 不同 StructureCompareMode 对“子对象数量”和“子对象名称”的要求不同。
        /// 当前节点本身的 Mesh 仍然必须匹配，避免空层级误命中。
        /// </summary>
        private bool IsStructureMatch(GameObject sceneObj, GameObject prefabObj)
        {
            if (sceneObj == null || prefabObj == null)
                return false;

            if (!IsNodeMeshMatch(sceneObj, prefabObj))
                return false;

            var prefabChildCount = prefabObj.transform.childCount;
            var sceneChildCount = sceneObj.transform.childCount;

            if (_structureCompareMode == StructureCompareMode.StrictNameAndChildCount && sceneChildCount != prefabChildCount)
                return false;

            if (_structureCompareMode != StructureCompareMode.StrictNameAndChildCount && sceneChildCount < prefabChildCount)
                return false;

            var usedSceneChildIndex = new HashSet<int>();
            for (var prefabIndex = 0; prefabIndex < prefabChildCount; prefabIndex++)
            {
                var prefabChild = prefabObj.transform.GetChild(prefabIndex).gameObject;
                var matched = false;

                for (var sceneIndex = 0; sceneIndex < sceneChildCount; sceneIndex++)
                {
                    if (usedSceneChildIndex.Contains(sceneIndex))
                        continue;

                    var sceneChild = sceneObj.transform.GetChild(sceneIndex).gameObject;
                    if (_structureCompareMode != StructureCompareMode.IgnoreChildNameAllowExtraSceneChildren && sceneChild.name != prefabChild.name)
                        continue;

                    if (!IsStructureMatch(sceneChild, prefabChild))
                        continue;

                    usedSceneChildIndex.Add(sceneIndex);
                    matched = true;
                    break;
                }

                if (!matched)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 检查当前节点 Mesh 是否匹配。
        /// 如果两边都没有对应类型 Mesh，则视为匹配。
        /// 如果一边有 Mesh 另一边没有 Mesh，则视为不匹配。
        /// </summary>
        private bool IsNodeMeshMatch(GameObject sceneObj, GameObject prefabObj)
        {
            if (_matchMeshFilters)
            {
                var sceneMf = sceneObj.GetComponent<MeshFilter>();
                var prefabMf = prefabObj.GetComponent<MeshFilter>();
                var sceneRenderer = sceneMf != null ? sceneMf.GetComponent<Renderer>() : null;
                var prefabRenderer = prefabMf != null ? prefabMf.GetComponent<Renderer>() : null;

                if (!AreNullableMeshSnapshotsMatch(
                        sceneMf != null ? CreateMeshSnapshot(sceneMf.sharedMesh, sceneRenderer) : null,
                        prefabMf != null ? CreateMeshSnapshot(prefabMf.sharedMesh, prefabRenderer) : null))
                    return false;
            }

            if (_matchSkinnedMeshes)
            {
                var sceneSmr = sceneObj.GetComponent<SkinnedMeshRenderer>();
                var prefabSmr = prefabObj.GetComponent<SkinnedMeshRenderer>();

                if (!AreNullableMeshSnapshotsMatch(
                        sceneSmr != null ? CreateMeshSnapshot(sceneSmr.sharedMesh, sceneSmr) : null,
                        prefabSmr != null ? CreateMeshSnapshot(prefabSmr.sharedMesh, prefabSmr) : null))
                    return false;
            }

            return true;
        }

        private void TryReplaceWithConfirm()
        {
            RemoveInvalidEntries(_replaceEntries);

            if (_replaceEntries.Count == 0)
            {
                EditorUtility.DisplayDialog("提示", "请先扫描场景，未找到匹配对象。", "OK");
                return;
            }

            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("错误", "Play Mode 下不可替换！", "OK");
                return;
            }

            if (_dryRun)
            {
                EditorUtility.DisplayDialog("提示", "当前为干运行模式，请先取消勾选“干运行”。", "OK");
                return;
            }

            if (!IsReplaceScanSnapshotStillValid())
            {
                EditorUtility.DisplayDialog("提示", "Prefab 或扫描参数已变化，请重新扫描后再执行替换。", "OK");
                return;
            }

            CollectPrefabMeshSnapshots();
            var selected = GetSelectedEntries(_replaceEntries);
            if (selected.Count == 0)
            {
                EditorUtility.DisplayDialog("提示", "没有勾选任何替换对象。", "OK");
                return;
            }

            var invalidCount = RevalidateReplaceEntries(false);
            var riskSummary = BuildReplaceRiskSummary(selected, invalidCount);
            var message =
                $"已勾选 {selected.Count} 个对象，失效 / 已不匹配 {invalidCount} 个。\n\n" +
                riskSummary +
                "\n是否继续替换有效对象？";

            if (EditorUtility.DisplayDialog("确认替换", message, "Yes", "Cancel"))
                ReplaceAllMatches();
        }

        private int RevalidateReplaceEntries(bool showDialog)
        {
            CollectPrefabMeshSnapshots();
            var invalidCount = 0;

            foreach (var entry in _replaceEntries)
            {
                if (entry == null || entry.Target == null)
                    continue;

                entry.Warning = BuildReplaceWarning(entry.Target);
                entry.Info = BuildObjectInfo(entry.Target);

                if (!entry.Selected)
                    continue;

                if (!IsStillValidReplaceMatch(entry.Target))
                {
                    invalidCount++;
                    entry.Warning = AppendWarning(entry.Warning, "对象已不再符合当前替换匹配条件，执行时会跳过。请重新扫描或取消勾选。 ");
                }
            }

            if (showDialog)
                EditorUtility.DisplayDialog("校验完成", $"当前列表中失效 / 已不匹配对象：{invalidCount} 个。", "OK");

            Repaint();
            return invalidCount;
        }

        private bool IsStillValidReplaceMatch(GameObject go)
        {
            if (go == null || _prefab == null)
                return false;

            if (!PassesCommonScanFilters(go))
                return false;

            if (_usePrefabStructure)
                return IsStructureMatch(go, _prefab);

            if (!IsSingleMeshMatch(go))
                return false;

            if (!_overrideParentPrefabCheck && IsParentPrefabInstanceWithMesh(go))
                return false;

            return true;
        }

        private void ReplaceAllMatches()
        {
            var selected = GetSelectedEntries(_replaceEntries);
            var report = new ExecutionReport();
            var dirtyScenes = new HashSet<Scene>();

            Undo.IncrementCurrentGroup();
            var undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Replace With Prefab By Mesh");

            try
            {
                for (var i = 0; i < selected.Count; i++)
                {
                    var entry = selected[i];
                    var orig = entry.Target;
                    if (orig == null)
                    {
                        report.Skipped++;
                        report.AddMessage($"跳过：{entry.CachedPath} 已失效。 ");
                        continue;
                    }

                    if (EditorUtility.DisplayCancelableProgressBar("替换中...", $"{i + 1}/{selected.Count} {GetPath(orig.transform)}", (float)(i + 1) / selected.Count))
                    {
                        report.AddMessage("用户取消了替换操作。已完成的替换可以通过 Ctrl+Z 撤销。 ");
                        break;
                    }

                    var origPath = GetPath(orig.transform);
                    var origScene = orig.scene;

                    try
                    {
                        if (!IsStillValidReplaceMatch(orig))
                        {
                            report.Skipped++;
                            report.AddMessage($"跳过：{origPath} 已不再符合当前匹配条件。 ");
                            continue;
                        }

                        var newObj = ReplaceOne(orig);
                        if (newObj != null)
                        {
                            report.Success++;
                            dirtyScenes.Add(origScene);
                            dirtyScenes.Add(newObj.scene);
                            report.AddMessage($"成功：{origPath} -> {GetPath(newObj.transform)}");
                        }
                        else
                        {
                            report.Failed++;
                            report.AddMessage($"失败：{origPath} Prefab 实例化失败。 ");
                        }
                    }
                    catch (Exception e)
                    {
                        report.Failed++;
                        report.AddMessage($"失败：{origPath}\n{e.Message}");
                        Debug.LogError($"[ReplaceByPrefabMesh] 替换失败：{origPath}\n{e}");
                    }
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();

                foreach (var scene in dirtyScenes)
                {
                    if (scene.IsValid())
                        EditorSceneManager.MarkSceneDirty(scene);
                }

                Undo.CollapseUndoOperations(undoGroup);
                RemoveInvalidEntries(_replaceEntries);
                Repaint();
            }

            report.Print("ReplaceByPrefabMesh", _printVerboseReport);
        }

        /// <summary>
        /// 替换单个对象。
        /// 如果中途异常，会销毁新生成对象，避免残留半成品。
        /// </summary>
        private GameObject ReplaceOne(GameObject orig)
        {
            if (orig == null || _prefab == null)
                return null;

            var origParent = orig.transform.parent;
            var origSiblingIndex = orig.transform.GetSiblingIndex();
            var origLocalPos = orig.transform.localPosition;
            var origLocalRot = orig.transform.localRotation;
            var origLocalScale = orig.transform.localScale;
            var origWorldPos = orig.transform.position;
            var origWorldRot = orig.transform.rotation;
            var origWorldScale = orig.transform.lossyScale;
            var origName = orig.name;
            var origTag = orig.tag;
            var origLayer = orig.layer;
            var origActive = orig.activeSelf;
            var origStatic = GameObjectUtility.GetStaticEditorFlags(orig);
            var origScene = orig.scene;

            GameObject newInstance = null;
            try
            {
                newInstance = PrefabUtility.InstantiatePrefab(_prefab, origScene) as GameObject;
                if (newInstance == null)
                    return null;

                Undo.RegisterCreatedObjectUndo(newInstance, "Create Replacement Prefab");

                ApplyTransform(newInstance.transform, origParent, origSiblingIndex, origLocalPos, origLocalRot, origLocalScale, origWorldPos, origWorldRot, origWorldScale, origScene);

                if (_breakPrefabLink && PrefabUtility.IsPartOfPrefabInstance(newInstance))
                    PrefabUtility.UnpackPrefabInstance(newInstance, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);

                if (_keepOriginalName)
                    newInstance.name = origName;

                newInstance.tag = origTag;
                newInstance.SetActive(origActive);

                if (_copyLayer)
                    ApplyLayer(newInstance, origLayer, _copyLayerRecursively);

                if (_copyStaticFlags)
                    ApplyStaticFlags(newInstance, origStatic, _copyStaticFlagsRecursively);

                if (_copyRendererSettings || _copyLightmapSettings)
                    CopyRendererSettings(orig, newInstance, _copyRendererSettings, _copyLightmapSettings);

                if (_copyRootColliders)
                    CopyRootColliders(orig, newInstance);

                if (_copyRootMonoBehaviours)
                    CopyRootMonoBehaviours(orig, newInstance);

                Undo.DestroyObjectImmediate(orig);
                return newInstance;
            }
            catch
            {
                if (newInstance != null)
                    Undo.DestroyObjectImmediate(newInstance);

                throw;
            }
        }

        private void ApplyTransform(
            Transform target,
            Transform origParent,
            int origSiblingIndex,
            Vector3 origLocalPos,
            Quaternion origLocalRot,
            Vector3 origLocalScale,
            Vector3 origWorldPos,
            Quaternion origWorldRot,
            Vector3 origWorldScale,
            Scene origScene)
        {
            if (_parentToOriginalParent)
            {
                target.SetParent(origParent, false);
                target.localPosition = origLocalPos;
                target.localRotation = origLocalRot;
                target.localScale = origLocalScale;
                target.SetSiblingIndex(Mathf.Max(0, origSiblingIndex));
            }
            else
            {
                target.SetParent(null, true);
                target.position = origWorldPos;
                target.rotation = origWorldRot;
                target.localScale = origWorldScale;

                if (origScene.IsValid() && target.gameObject.scene != origScene)
                    SceneManager.MoveGameObjectToScene(target.gameObject, origScene);

                target.SetSiblingIndex(Mathf.Min(Mathf.Max(0, origSiblingIndex), Mathf.Max(0, SceneRootCount(origScene) - 1)));
            }
        }

        private static int SceneRootCount(Scene scene)
        {
            return scene.IsValid() ? scene.rootCount : 0;
        }

        private void SaveReplaceScanSnapshot()
        {
            _lastReplaceScanPrefab = _prefab;
            _lastReplaceScanIncludeInactive = _includeInactive;
            _lastReplaceScanScope = _scanScope;
            _lastReplaceUseLayerFilter = _useLayerFilter;
            _lastReplaceScanLayerMask = _scanLayerMask;
            _lastReplaceUseTagFilter = _useTagFilter;
            _lastReplaceScanTag = _scanTag;
            _lastReplaceMatchMeshFilters = _matchMeshFilters;
            _lastReplaceMatchSkinnedMeshes = _matchSkinnedMeshes;
            _lastReplaceMeshCompareMode = _meshCompareMode;
            _lastReplaceMatchMaterials = _matchMaterials;
            _lastReplaceUsePrefabStructure = _usePrefabStructure;
            _lastReplaceStructureCompareMode = _structureCompareMode;
            _lastReplaceOverrideParentPrefabCheck = _overrideParentPrefabCheck;
            _lastReplaceIgnoreNestedMatches = _ignoreNestedReplaceMatches;
        }

        private bool IsReplaceScanSnapshotStillValid()
        {
            return _lastReplaceScanPrefab == _prefab &&
                   _lastReplaceScanIncludeInactive == _includeInactive &&
                   _lastReplaceScanScope == _scanScope &&
                   _lastReplaceUseLayerFilter == _useLayerFilter &&
                   _lastReplaceScanLayerMask == _scanLayerMask &&
                   _lastReplaceUseTagFilter == _useTagFilter &&
                   _lastReplaceScanTag == _scanTag &&
                   _lastReplaceMatchMeshFilters == _matchMeshFilters &&
                   _lastReplaceMatchSkinnedMeshes == _matchSkinnedMeshes &&
                   _lastReplaceMeshCompareMode == _meshCompareMode &&
                   _lastReplaceMatchMaterials == _matchMaterials &&
                   _lastReplaceUsePrefabStructure == _usePrefabStructure &&
                   _lastReplaceStructureCompareMode == _structureCompareMode &&
                   _lastReplaceOverrideParentPrefabCheck == _overrideParentPrefabCheck &&
                   _lastReplaceIgnoreNestedMatches == _ignoreNestedReplaceMatches;
        }

        private void InvalidateReplaceCache()
        {
            _replaceEntries.Clear();
            _prefabMeshSnapshots.Clear();
            _lastReplaceScanPrefab = null;
        }

        #endregion

        #region 批量移动模块

        private void DrawMoveSection()
        {
            BeginCard($"批量移动模块    匹配 {_moveEntries.Count} / 勾选 {GetSelectedEntries(_moveEntries).Count}", "按名称规则扫描对象，并批量移动到指定父节点或场景根节点。 ");

            _showMoveSection = EditorGUILayout.Foldout(_showMoveSection, "展开移动模块", true);
            if (!_showMoveSection)
            {
                EndCard();
                return;
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            DrawSubCardTitle("目标与名称规则");
            _moveTargetParent = (GameObject)EditorGUILayout.ObjectField("目标父节点", _moveTargetParent, typeof(GameObject), true);
            EditorGUILayout.LabelField("<color=#ff9900>[Tips] 为空时移动到场景根节点；目标父节点必须是 Hierarchy 中的对象。</color>", _tipsStyle);

            EditorGUILayout.BeginHorizontal();
            _targetName = EditorGUILayout.TextField("对象名称 / 规则", _targetName);
            if (GUILayout.Button("从选中对象获取", GUILayout.Width(140)))
            {
                if (Selection.activeGameObject != null)
                    _targetName = Selection.activeGameObject.name;
                else
                    EditorUtility.DisplayDialog("提示", "请先在 Hierarchy 中选中一个对象。", "OK");
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            _nameMatchMode = (NameMatchMode)EditorGUILayout.EnumPopup("名称匹配方式", _nameMatchMode);
            _ignoreNameCase = EditorGUILayout.ToggleLeft("忽略大小写", _ignoreNameCase, GUILayout.Width(100));
            EditorGUILayout.EndHorizontal();
            _ignoreNestedMoveMatches = EditorGUILayout.ToggleLeft("父子同时命中时只保留最外层对象", _ignoreNestedMoveMatches);
            EditorGUILayout.EndVertical();
            if (EditorGUI.EndChangeCheck())
                InvalidateMoveCache();

            EditorGUILayout.Space(4);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            DrawSubCardTitle("移动执行选项");
            _moveKeepWorldTransform = EditorGUILayout.ToggleLeft("移动后保持世界坐标", _moveKeepWorldTransform);
            _deleteEmptyParent = EditorGUILayout.ToggleLeft("移动后删除空父节点", _deleteEmptyParent);
            using (new EditorGUI.DisabledScope(!_deleteEmptyParent))
            {
                _recursiveDeleteEmptyParent = EditorGUILayout.ToggleLeft("递归删除连续空父节点", _recursiveDeleteEmptyParent);
                _protectSceneRootWhenDeleteEmptyParent = EditorGUILayout.ToggleLeft("保护场景根节点不被删除", _protectSceneRootWhenDeleteEmptyParent);
                _protectedEmptyParentNames = EditorGUILayout.TextField("保护节点名称", _protectedEmptyParentNames);
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(4);
            DrawActionButtonRow(
                "扫描场景(按名称)",
                FindAndListMoveMatches,
                "重新校验列表",
                () => RevalidateMoveEntries(true),
                "执行移动",
                () => _pendingMoveConfirm = true,
                false);

            DrawMoveMatchList();

            if (_pendingMoveConfirm)
            {
                _pendingMoveConfirm = false;
                EditorApplication.delayCall += TryMoveWithConfirm;
            }

            EndCard();
        }

        private void DrawMoveMatchList()
        {
            RemoveInvalidEntries(_moveEntries);
            if (_moveEntries.Count <= 0)
                return;

            EditorGUILayout.Space(4);
            DrawEntryToolbar(_moveEntries, "移动匹配对象列表", ref _moveListFilter, ref _moveListOnlySelected);

            var shown = 0;
            var visibleIndex = 0;
            _moveScroll = EditorGUILayout.BeginScrollView(_moveScroll, GUILayout.Height(260));
            for (var i = 0; i < _moveEntries.Count; i++)
            {
                var entry = _moveEntries[i];
                if (!IsEntryVisible(entry, _moveListFilter, _moveListOnlySelected))
                    continue;

                visibleIndex++;
                if (shown >= _maxVisibleListItems)
                    continue;

                shown++;
                DrawEntryRow(_moveEntries, i, visibleIndex);
            }

            EditorGUILayout.EndScrollView();

            if (visibleIndex > shown)
                EditorGUILayout.HelpBox($"当前筛选后共有 {visibleIndex} 条，只显示前 {shown} 条。可以提高“列表最大显示数量”或继续输入筛选关键字。", MessageType.Info);
        }

        private void FindAndListMoveMatches()
        {
            _moveEntries.Clear();

            if (!ValidateMoveScanOptions())
                return;

            foreach (var t in GetCandidateTransforms())
            {
                if (t == null || !PassesCommonScanFilters(t.gameObject))
                    continue;

                if (IsNameMatch(t.name, _targetName))
                    _moveEntries.Add(new SceneObjectEntry(t.gameObject, BuildObjectInfo(t.gameObject), BuildMoveWarning(t.gameObject)));
            }

            if (_ignoreNestedMoveMatches)
                RemoveNestedEntries(_moveEntries);

            SaveMoveScanSnapshot();
            Debug.Log($"[ReplaceByPrefabMesh-Move] 找到 {_moveEntries.Count} 个匹配对象。");
        }

        private bool ValidateMoveScanOptions()
        {
            if (string.IsNullOrWhiteSpace(_targetName))
            {
                EditorUtility.DisplayDialog("提示", "请输入对象名称 / 规则，或从选中对象获取。", "OK");
                return false;
            }

            if (_scanScope == ScanScope.SelectionChildren && Selection.gameObjects.Length == 0)
            {
                EditorUtility.DisplayDialog("提示", "当前扫描范围为“选中对象及子对象”，请先在 Hierarchy 中选择至少一个对象。", "OK");
                return false;
            }

            if (_nameMatchMode == NameMatchMode.Regex)
            {
                try
                {
                    _ = new Regex(_targetName);
                }
                catch (Exception e)
                {
                    EditorUtility.DisplayDialog("正则表达式错误", e.Message, "OK");
                    return false;
                }
            }

            return true;
        }

        private void TryMoveWithConfirm()
        {
            RemoveInvalidEntries(_moveEntries);

            if (_moveEntries.Count == 0)
            {
                EditorUtility.DisplayDialog("提示", "请先扫描场景，未找到匹配对象。", "OK");
                return;
            }

            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("错误", "Play Mode 下不可移动！", "OK");
                return;
            }

            if (_dryRun)
            {
                EditorUtility.DisplayDialog("提示", "当前为干运行模式，请先取消勾选“干运行”。", "OK");
                return;
            }

            if (!IsMoveScanSnapshotStillValid())
            {
                EditorUtility.DisplayDialog("提示", "名称规则或扫描参数已变化，请重新扫描后再执行移动。", "OK");
                return;
            }

            if (!ValidateMoveTarget())
                return;

            var selected = GetSelectedEntries(_moveEntries);
            if (selected.Count == 0)
            {
                EditorUtility.DisplayDialog("提示", "没有勾选任何移动对象。", "OK");
                return;
            }

            var invalidCount = RevalidateMoveEntries(false);
            var message =
                $"已勾选 {selected.Count} 个对象，失效 / 已不匹配 {invalidCount} 个。\n" +
                (_moveTargetParent != null ? $"目标父节点：{GetPath(_moveTargetParent.transform)}" : "目标父节点：场景根节点") +
                "\n\n是否继续移动有效对象？";

            if (EditorUtility.DisplayDialog("确认移动", message, "Yes", "Cancel"))
                MoveAllMatches();
        }

        private int RevalidateMoveEntries(bool showDialog)
        {
            var invalidCount = 0;
            foreach (var entry in _moveEntries)
            {
                if (entry == null || entry.Target == null)
                    continue;

                entry.Warning = BuildMoveWarning(entry.Target);
                entry.Info = BuildObjectInfo(entry.Target);

                if (!entry.Selected)
                    continue;

                if (!IsStillValidMoveMatch(entry.Target))
                {
                    invalidCount++;
                    entry.Warning = AppendWarning(entry.Warning, "对象已不再符合当前移动匹配条件，执行时会跳过。 ");
                }
            }

            if (showDialog)
                EditorUtility.DisplayDialog("校验完成", $"当前列表中失效 / 已不匹配对象：{invalidCount} 个。", "OK");

            Repaint();
            return invalidCount;
        }

        private bool IsStillValidMoveMatch(GameObject go)
        {
            return go != null && PassesCommonScanFilters(go) && IsNameMatch(go.name, _targetName);
        }

        private bool ValidateMoveTarget()
        {
            if (_moveTargetParent != null && AssetDatabase.Contains(_moveTargetParent))
            {
                EditorUtility.DisplayDialog("错误", "目标父节点必须是 Hierarchy 中的场景对象，不能是 Project 中的 Prefab 资源。", "OK");
                return false;
            }

            foreach (var entry in GetSelectedEntries(_moveEntries))
            {
                var go = entry.Target;
                if (go == null || _moveTargetParent == null)
                    continue;

                if (go == _moveTargetParent)
                {
                    EditorUtility.DisplayDialog("错误", $"目标父节点自身也在移动列表中：{GetPath(go.transform)}", "OK");
                    return false;
                }

                if (_moveTargetParent.transform.IsChildOf(go.transform))
                {
                    EditorUtility.DisplayDialog("错误", $"不能将对象移动到自己的子对象下：{GetPath(go.transform)}", "OK");
                    return false;
                }
            }

            return true;
        }

        private void MoveAllMatches()
        {
            var selected = GetSelectedEntries(_moveEntries);
            var report = new ExecutionReport();
            var oldParents = new HashSet<Transform>();
            var movedTransforms = new HashSet<Transform>();
            var dirtyScenes = new HashSet<Scene>();
            var targetParent = _moveTargetParent != null ? _moveTargetParent.transform : null;

            Undo.IncrementCurrentGroup();
            var undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Batch Move Objects By Name");

            try
            {
                for (var i = 0; i < selected.Count; i++)
                {
                    var entry = selected[i];
                    var go = entry.Target;
                    if (go == null)
                    {
                        report.Skipped++;
                        report.AddMessage($"跳过：{entry.CachedPath} 已失效。 ");
                        continue;
                    }

                    if (EditorUtility.DisplayCancelableProgressBar("移动中...", $"{i + 1}/{selected.Count} {GetPath(go.transform)}", (float)(i + 1) / selected.Count))
                    {
                        report.AddMessage("用户取消了移动操作。已完成的移动可以通过 Ctrl+Z 撤销。 ");
                        break;
                    }

                    var path = GetPath(go.transform);
                    try
                    {
                        if (!IsStillValidMoveMatch(go))
                        {
                            report.Skipped++;
                            report.AddMessage($"跳过：{path} 已不再符合当前匹配条件。 ");
                            continue;
                        }

                        if (targetParent != null && (go == targetParent.gameObject || targetParent.IsChildOf(go.transform)))
                        {
                            report.Failed++;
                            report.AddMessage($"失败：{path} 不能移动到自身或自身子节点下。 ");
                            continue;
                        }

                        var oldParent = go.transform.parent;
                        var oldScene = go.scene;
                        var worldPos = go.transform.position;
                        var worldRot = go.transform.rotation;
                        var worldScale = go.transform.lossyScale;

                        if (_deleteEmptyParent && oldParent != null)
                            oldParents.Add(oldParent);

                        Undo.SetTransformParent(go.transform, targetParent, "Move Object");

                        if (_moveKeepWorldTransform)
                            ApplyWorldTransform(go.transform, worldPos, worldRot, worldScale);

                        movedTransforms.Add(go.transform);
                        dirtyScenes.Add(oldScene);
                        dirtyScenes.Add(go.scene);
                        report.Success++;
                        report.AddMessage($"成功：{path} -> {(targetParent != null ? GetPath(targetParent) : "Scene Root")}");
                    }
                    catch (Exception e)
                    {
                        report.Failed++;
                        report.AddMessage($"失败：{path}\n{e.Message}");
                        Debug.LogError($"[ReplaceByPrefabMesh-Move] 移动失败：{path}\n{e}");
                    }
                }

                if (_deleteEmptyParent)
                    DeleteEmptyOldParents(oldParents, movedTransforms, targetParent, dirtyScenes, report);
            }
            finally
            {
                EditorUtility.ClearProgressBar();

                foreach (var scene in dirtyScenes)
                {
                    if (scene.IsValid())
                        EditorSceneManager.MarkSceneDirty(scene);
                }

                Undo.CollapseUndoOperations(undoGroup);
                RemoveInvalidEntries(_moveEntries);
                Repaint();
            }

            report.Print("ReplaceByPrefabMesh-Move", _printVerboseReport);
        }

        private void DeleteEmptyOldParents(HashSet<Transform> oldParents, HashSet<Transform> movedTransforms, Transform targetParent, HashSet<Scene> dirtyScenes, ExecutionReport report)
        {
            var parentsToRemove = new HashSet<Transform>();
            foreach (var oldParent in oldParents)
            {
                var current = oldParent;
                while (current != null)
                {
                    if (!CanDeleteEmptyParent(current, movedTransforms, targetParent))
                        break;

                    if (current.childCount != 0)
                        break;

                    parentsToRemove.Add(current);

                    if (!_recursiveDeleteEmptyParent)
                        break;

                    current = current.parent;
                }
            }

            var sorted = new List<Transform>(parentsToRemove);
            sorted.Sort((a, b) => GetDepth(b).CompareTo(GetDepth(a)));

            foreach (var p in sorted)
            {
                if (p == null || p.childCount != 0)
                    continue;

                dirtyScenes.Add(p.gameObject.scene);
                report.AddMessage($"删除空父节点：{GetPath(p)}");
                Undo.DestroyObjectImmediate(p.gameObject);
            }
        }

        private bool CanDeleteEmptyParent(Transform p, HashSet<Transform> movedTransforms, Transform targetParent)
        {
            if (p == null)
                return false;

            if (targetParent != null && p == targetParent)
                return false;

            if (movedTransforms.Contains(p))
                return false;

            if (_protectSceneRootWhenDeleteEmptyParent && p.parent == null)
                return false;

            return !IsProtectedEmptyParentName(p.name);
        }

        private void SaveMoveScanSnapshot()
        {
            _lastMoveScanTargetName = _targetName;
            _lastMoveNameMatchMode = _nameMatchMode;
            _lastMoveIgnoreNameCase = _ignoreNameCase;
            _lastMoveScanIncludeInactive = _includeInactive;
            _lastMoveScanScope = _scanScope;
            _lastMoveUseLayerFilter = _useLayerFilter;
            _lastMoveScanLayerMask = _scanLayerMask;
            _lastMoveUseTagFilter = _useTagFilter;
            _lastMoveScanTag = _scanTag;
            _lastMoveIgnoreNestedMatches = _ignoreNestedMoveMatches;
        }

        private bool IsMoveScanSnapshotStillValid()
        {
            return _lastMoveScanTargetName == _targetName &&
                   _lastMoveNameMatchMode == _nameMatchMode &&
                   _lastMoveIgnoreNameCase == _ignoreNameCase &&
                   _lastMoveScanIncludeInactive == _includeInactive &&
                   _lastMoveScanScope == _scanScope &&
                   _lastMoveUseLayerFilter == _useLayerFilter &&
                   _lastMoveScanLayerMask == _scanLayerMask &&
                   _lastMoveUseTagFilter == _useTagFilter &&
                   _lastMoveScanTag == _scanTag &&
                   _lastMoveIgnoreNestedMatches == _ignoreNestedMoveMatches;
        }

        private void InvalidateMoveCache()
        {
            _moveEntries.Clear();
            _lastMoveScanTargetName = null;
        }

        #endregion

        #region 组件与渲染设置复制

        private void ApplyLayer(GameObject root, int layer, bool recursively)
        {
            if (root == null)
                return;

            if (!recursively)
            {
                root.layer = layer;
                return;
            }

            foreach (var t in root.GetComponentsInChildren<Transform>(true))
                t.gameObject.layer = layer;
        }

        private void ApplyStaticFlags(GameObject root, StaticEditorFlags flags, bool recursively)
        {
            if (root == null)
                return;

            if (!recursively)
            {
                GameObjectUtility.SetStaticEditorFlags(root, flags);
                return;
            }

            foreach (var t in root.GetComponentsInChildren<Transform>(true))
                GameObjectUtility.SetStaticEditorFlags(t.gameObject, flags);
        }

        /// <summary>
        /// 将原对象第一个 Renderer 的常用渲染设置应用到新对象所有 Renderer。
        /// 多 Renderer 精准映射容易误配，所以这里采用安全、可预期的统一继承方式。
        /// </summary>
        private void CopyRendererSettings(GameObject sourceRoot, GameObject targetRoot, bool copyCommonSettings, bool copyLightmapSettings)
        {
            var sourceRenderer = sourceRoot.GetComponentInChildren<Renderer>(true);
            if (sourceRenderer == null)
                return;

            foreach (var targetRenderer in targetRoot.GetComponentsInChildren<Renderer>(true))
            {
                if (targetRenderer == null)
                    continue;

                Undo.RecordObject(targetRenderer, "Copy Renderer Settings");

                if (copyCommonSettings)
                {
                    targetRenderer.shadowCastingMode = sourceRenderer.shadowCastingMode;
                    targetRenderer.receiveShadows = sourceRenderer.receiveShadows;
                    targetRenderer.lightProbeUsage = sourceRenderer.lightProbeUsage;
                    targetRenderer.reflectionProbeUsage = sourceRenderer.reflectionProbeUsage;
                    targetRenderer.motionVectorGenerationMode = sourceRenderer.motionVectorGenerationMode;
                    targetRenderer.sortingLayerID = sourceRenderer.sortingLayerID;
                    targetRenderer.sortingOrder = sourceRenderer.sortingOrder;
                }

                if (copyLightmapSettings)
                {
                    targetRenderer.lightmapIndex = sourceRenderer.lightmapIndex;
                    targetRenderer.lightmapScaleOffset = sourceRenderer.lightmapScaleOffset;
                    targetRenderer.realtimeLightmapIndex = sourceRenderer.realtimeLightmapIndex;
                    targetRenderer.realtimeLightmapScaleOffset = sourceRenderer.realtimeLightmapScaleOffset;
                }
            }
        }

        private void CopyRootColliders(GameObject sourceRoot, GameObject targetRoot)
        {
            foreach (var collider in sourceRoot.GetComponents<Collider>())
            {
                if (collider == null)
                    continue;

                try
                {
                    ComponentUtility.CopyComponent(collider);
                    ComponentUtility.PasteComponentAsNew(targetRoot);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[ReplaceByPrefabMesh] Collider 复制失败：{collider.GetType().Name}\n{e.Message}");
                }
            }
        }

        /// <summary>
        /// 仅复制根节点 MonoBehaviour，不递归复制，避免把子层级逻辑脚本错误复制到新 Prefab 根节点。
        /// 缺失脚本会返回 null，这里会自动跳过。
        /// </summary>
        private void CopyRootMonoBehaviours(GameObject sourceRoot, GameObject targetRoot)
        {
            foreach (var mono in sourceRoot.GetComponents<MonoBehaviour>())
            {
                if (mono == null)
                    continue;

                try
                {
                    ComponentUtility.CopyComponent(mono);
                    ComponentUtility.PasteComponentAsNew(targetRoot);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[ReplaceByPrefabMesh] MonoBehaviour 复制失败：{mono.GetType().Name}\n{e.Message}");
                }
            }
        }

        #endregion

        #region Mesh 比较工具

        private IEnumerable<MeshSnapshot> GetNodeMeshSnapshots(GameObject go)
        {
            if (go == null)
                yield break;

            if (_matchMeshFilters)
            {
                var mf = go.GetComponent<MeshFilter>();
                if (mf != null && mf.sharedMesh != null)
                    yield return CreateMeshSnapshot(mf.sharedMesh, mf.GetComponent<Renderer>());
            }

            if (_matchSkinnedMeshes)
            {
                var sk = go.GetComponent<SkinnedMeshRenderer>();
                if (sk != null && sk.sharedMesh != null)
                    yield return CreateMeshSnapshot(sk.sharedMesh, sk);
            }
        }

        private static int GetTriangleCount(Mesh mesh)
        {
            if (mesh == null)
                return 0;

            try
            {
                var count = 0;
                for (var i = 0; i < mesh.subMeshCount; i++)
                    count += (int)(mesh.GetIndexCount(i) / 3);

                return count;
            }
            catch
            {
                return 0;
            }
        }

        private static MeshSnapshot CreateMeshSnapshot(Mesh mesh, Renderer renderer)
        {
            if (mesh == null)
                return null;

            var materials = renderer != null ? renderer.sharedMaterials : new Material[0];
            var materialNames = new string[materials.Length];
            for (var i = 0; i < materials.Length; i++)
                materialNames[i] = materials[i] != null ? materials[i].name : "<Null>";

            return new MeshSnapshot
            {
                Mesh = mesh,
                Name = mesh.name,
                VertexCount = mesh.vertexCount,
                TriangleCount = GetTriangleCount(mesh),
                Bounds = mesh.bounds,
                MaterialNames = materialNames
            };
        }

        private bool AreNullableMeshSnapshotsMatch(MeshSnapshot sceneSnapshot, MeshSnapshot prefabSnapshot)
        {
            if (sceneSnapshot == null && prefabSnapshot == null)
                return true;

            if (sceneSnapshot == null || prefabSnapshot == null)
                return false;

            return AreMeshSnapshotsMatch(sceneSnapshot, prefabSnapshot);
        }

        private bool AreMeshSnapshotsMatch(MeshSnapshot sceneSnapshot, MeshSnapshot prefabSnapshot)
        {
            if (sceneSnapshot == null || prefabSnapshot == null || !sceneSnapshot.HasMesh || !prefabSnapshot.HasMesh)
                return false;

            var meshMatched = false;
            switch (_meshCompareMode)
            {
                case MeshCompareMode.Reference:
                    meshMatched = sceneSnapshot.Mesh == prefabSnapshot.Mesh;
                    break;
                case MeshCompareMode.Name:
                    meshMatched = string.Equals(sceneSnapshot.Name, prefabSnapshot.Name, StringComparison.Ordinal);
                    break;
                case MeshCompareMode.VertexTriangleCount:
                    meshMatched = sceneSnapshot.VertexCount == prefabSnapshot.VertexCount &&
                                  sceneSnapshot.TriangleCount == prefabSnapshot.TriangleCount;
                    break;
                case MeshCompareMode.NameVertexTriangleCount:
                    meshMatched = string.Equals(sceneSnapshot.Name, prefabSnapshot.Name, StringComparison.Ordinal) &&
                                  sceneSnapshot.VertexCount == prefabSnapshot.VertexCount &&
                                  sceneSnapshot.TriangleCount == prefabSnapshot.TriangleCount;
                    break;
                case MeshCompareMode.Bounds:
                    meshMatched = IsBoundsApproximately(sceneSnapshot.Bounds, prefabSnapshot.Bounds);
                    break;
                case MeshCompareMode.NameVertexTriangleBounds:
                    meshMatched = string.Equals(sceneSnapshot.Name, prefabSnapshot.Name, StringComparison.Ordinal) &&
                                  sceneSnapshot.VertexCount == prefabSnapshot.VertexCount &&
                                  sceneSnapshot.TriangleCount == prefabSnapshot.TriangleCount &&
                                  IsBoundsApproximately(sceneSnapshot.Bounds, prefabSnapshot.Bounds);
                    break;
            }

            return meshMatched && (!_matchMaterials || AreMaterialNamesEqual(sceneSnapshot.MaterialNames, prefabSnapshot.MaterialNames));
        }

        private static bool AreMaterialNamesEqual(string[] a, string[] b)
        {
            if (a == null && b == null)
                return true;

            if (a == null || b == null || a.Length != b.Length)
                return false;

            for (var i = 0; i < a.Length; i++)
            {
                if (!string.Equals(a[i], b[i], StringComparison.Ordinal))
                    return false;
            }

            return true;
        }

        private static bool IsBoundsApproximately(Bounds a, Bounds b)
        {
            return Approximately(a.center, b.center) && Approximately(a.size, b.size);
        }

        private static bool Approximately(Vector3 a, Vector3 b)
        {
            const float epsilon = 0.0001f;
            return Mathf.Abs(a.x - b.x) < epsilon && Mathf.Abs(a.y - b.y) < epsilon && Mathf.Abs(a.z - b.z) < epsilon;
        }

        #endregion

        #region 扫描、列表、过滤工具

        private List<Transform> GetCandidateTransforms()
        {
            var result = new List<Transform>();
            var visited = new HashSet<Transform>();

            switch (_scanScope)
            {
                case ScanScope.ActiveScene:
                    AddSceneTransforms(SceneManager.GetActiveScene(), result, visited);
                    break;

                case ScanScope.LoadedScenes:
                    for (var i = 0; i < SceneManager.sceneCount; i++)
                        AddSceneTransforms(SceneManager.GetSceneAt(i), result, visited);
                    break;

                case ScanScope.SelectionChildren:
                    foreach (var selected in Selection.gameObjects)
                    {
                        if (selected == null || AssetDatabase.Contains(selected))
                            continue;

                        foreach (var t in selected.GetComponentsInChildren<Transform>(_includeInactive))
                        {
                            if (t != null && visited.Add(t))
                                result.Add(t);
                        }
                    }
                    break;
            }

            return result;
        }

        private void AddSceneTransforms(Scene scene, List<Transform> result, HashSet<Transform> visited)
        {
            if (!scene.IsValid() || !scene.isLoaded)
                return;

            foreach (var root in scene.GetRootGameObjects())
            {
                if (root == null)
                    continue;

                foreach (var t in root.GetComponentsInChildren<Transform>(_includeInactive))
                {
                    if (t != null && visited.Add(t))
                        result.Add(t);
                }
            }
        }

        private bool PassesCommonScanFilters(GameObject go)
        {
            if (go == null)
                return false;

            if (_useLayerFilter && (_scanLayerMask & (1 << go.layer)) == 0)
                return false;

            if (_useTagFilter && !string.Equals(go.tag, _scanTag, StringComparison.Ordinal))
                return false;

            return true;
        }

        private void DrawEntryToolbar(List<SceneObjectEntry> entries, string title, ref string filter, ref bool onlySelected)
        {
            var selectedCount = GetSelectedEntries(entries).Count;
            var visibleCount = CountVisibleEntries(entries, filter, onlySelected);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"{title}：总数 {entries.Count}，勾选 {selectedCount}，当前显示 {visibleCount}", EditorStyles.boldLabel);
            if (GUILayout.Button("清空列表", GUILayout.Width(72)))
                entries.Clear();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("全选"))
                SetAllSelected(entries, true);

            if (GUILayout.Button("全不选"))
                SetAllSelected(entries, false);

            if (GUILayout.Button("反选"))
                InvertSelected(entries);

            if (GUILayout.Button("移除未勾选"))
                entries.RemoveAll(e => e == null || !e.Selected);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("筛选", GUILayout.Width(32));
            filter = EditorGUILayout.TextField(filter);
            onlySelected = EditorGUILayout.ToggleLeft("仅显示勾选", onlySelected, GUILayout.Width(96));
            if (GUILayout.Button("清空筛选", GUILayout.Width(78)))
            {
                filter = string.Empty;
                onlySelected = false;
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// 绘制单条扫描结果。列表中每一行都提供勾选、定位、移除能力，避免用户只能全量执行。
        /// </summary>
        private void DrawEntryRow(List<SceneObjectEntry> entries, int index, int visibleIndex)
        {
            var entry = entries[index];
            if (entry == null || entry.Target == null)
                return;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            entry.Selected = EditorGUILayout.Toggle(entry.Selected, GUILayout.Width(20));
            EditorGUILayout.LabelField($"{visibleIndex}. {entry.CachedPath}", GUILayout.MaxHeight(18));

            if (GUILayout.Button("选中", GUILayout.Width(56)))
            {
                Selection.activeGameObject = entry.Target;
                EditorGUIUtility.PingObject(entry.Target);
            }

            if (GUILayout.Button("移除", GUILayout.Width(56)))
                entries.RemoveAt(index);

            EditorGUILayout.EndHorizontal();

            if (!string.IsNullOrEmpty(entry.Info))
                EditorGUILayout.LabelField(entry.Info, _tipsStyle);

            if (!string.IsNullOrEmpty(entry.Warning))
                EditorGUILayout.HelpBox(entry.Warning, MessageType.Warning);

            EditorGUILayout.EndVertical();
        }

        private bool IsEntryVisible(SceneObjectEntry entry, string filter, bool onlySelected)
        {
            if (entry == null || entry.Target == null)
                return false;

            if (onlySelected && !entry.Selected)
                return false;

            if (string.IsNullOrWhiteSpace(filter))
                return true;

            return ContainsIgnoreCase(entry.CachedPath, filter) || ContainsIgnoreCase(entry.Info, filter) || ContainsIgnoreCase(entry.Warning, filter) || ContainsIgnoreCase(entry.Target.name, filter);
        }

        private int CountVisibleEntries(List<SceneObjectEntry> entries, string filter, bool onlySelected)
        {
            var count = 0;
            foreach (var entry in entries)
            {
                if (IsEntryVisible(entry, filter, onlySelected))
                    count++;
            }

            return count;
        }

        private static bool ContainsIgnoreCase(string value, string filter)
        {
            if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(filter))
                return false;

            return value.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static void RemoveInvalidEntries(List<SceneObjectEntry> entries)
        {
            for (var i = entries.Count - 1; i >= 0; i--)
            {
                if (entries[i] == null || entries[i].Target == null)
                    entries.RemoveAt(i);
            }
        }

        private static List<SceneObjectEntry> GetSelectedEntries(List<SceneObjectEntry> entries)
        {
            var result = new List<SceneObjectEntry>();
            foreach (var entry in entries)
            {
                if (entry != null && entry.Target != null && entry.Selected)
                    result.Add(entry);
            }

            return result;
        }

        private static void SetAllSelected(List<SceneObjectEntry> entries, bool selected)
        {
            foreach (var entry in entries)
            {
                if (entry != null && entry.Target != null)
                    entry.Selected = selected;
            }
        }

        private static void InvertSelected(List<SceneObjectEntry> entries)
        {
            foreach (var entry in entries)
            {
                if (entry != null && entry.Target != null)
                    entry.Selected = !entry.Selected;
            }
        }

        private static void RemoveNestedEntries(List<SceneObjectEntry> entries)
        {
            RemoveInvalidEntries(entries);
            var set = new HashSet<Transform>();
            foreach (var entry in entries)
            {
                if (entry?.Target != null)
                    set.Add(entry.Target.transform);
            }

            for (var i = entries.Count - 1; i >= 0; i--)
            {
                var go = entries[i].Target;
                var parent = go.transform.parent;
                while (parent != null)
                {
                    if (set.Contains(parent))
                    {
                        entries.RemoveAt(i);
                        break;
                    }

                    parent = parent.parent;
                }
            }
        }

        #endregion

        #region 名称匹配与警告信息

        private bool IsNameMatch(string objectName, string rule)
        {
            if (objectName == null || rule == null)
                return false;

            var comparison = _ignoreNameCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            switch (_nameMatchMode)
            {
                case NameMatchMode.Exact:
                    return string.Equals(objectName, rule, comparison);
                case NameMatchMode.Contains:
                    return objectName.IndexOf(rule, comparison) >= 0;
                case NameMatchMode.StartsWith:
                    return objectName.StartsWith(rule, comparison);
                case NameMatchMode.EndsWith:
                    return objectName.EndsWith(rule, comparison);
                case NameMatchMode.Wildcard:
                    return IsWildcardMatch(objectName, rule, _ignoreNameCase);
                case NameMatchMode.Regex:
                    return Regex.IsMatch(objectName, rule, _ignoreNameCase ? RegexOptions.IgnoreCase : RegexOptions.None);
                default:
                    return false;
            }
        }

        private static bool IsWildcardMatch(string value, string pattern, bool ignoreCase)
        {
            var regex = "^" + Regex.Escape(pattern).Replace("\\*", ".*").Replace("\\?", ".") + "$";
            return Regex.IsMatch(value, regex, ignoreCase ? RegexOptions.IgnoreCase : RegexOptions.None);
        }

        private string BuildObjectInfo(GameObject go)
        {
            if (go == null)
                return string.Empty;

            var meshInfo = GetFirstMeshInfo(go);
            return $"Layer: {LayerMask.LayerToName(go.layer)} | Tag: {go.tag}" + (string.IsNullOrEmpty(meshInfo) ? string.Empty : $" | {meshInfo}");
        }

        private string BuildReplaceWarning(GameObject go)
        {
            if (go == null)
                return string.Empty;

            var warning = string.Empty;
            if (PrefabUtility.IsPartOfPrefabInstance(go))
                warning = AppendWarning(warning, "对象属于 Prefab Instance，替换后原 Override 不会自动迁移。 ");

            if (go.GetComponentsInChildren<MonoBehaviour>(true).Length > 0)
                warning = AppendWarning(warning, "对象或子对象包含 MonoBehaviour，替换后默认会丢失这些脚本；如需保留根节点脚本请勾选复制选项。 ");

            if (go.GetComponentsInChildren<Collider>(true).Length > 0 && !_copyRootColliders)
                warning = AppendWarning(warning, "对象包含 Collider，替换后默认不会保留 Collider。 ");

            foreach (var renderer in go.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer != null && renderer.lightmapIndex >= 0 && !_copyLightmapSettings)
                {
                    warning = AppendWarning(warning, "对象包含 Lightmap 信息，当前未勾选继承 Lightmap 设置。 ");
                    break;
                }
            }

            return warning;
        }

        private string BuildMoveWarning(GameObject go)
        {
            if (go == null)
                return string.Empty;

            var warning = string.Empty;
            if (_moveTargetParent != null && _moveTargetParent.transform.IsChildOf(go.transform))
                warning = AppendWarning(warning, "目标父节点是该对象的子节点，执行时会跳过。 ");

            if (_deleteEmptyParent && go.transform.parent != null && IsProtectedEmptyParentName(go.transform.parent.name))
                warning = AppendWarning(warning, "旧父节点名称在保护列表中，不会被删除。 ");

            return warning;
        }

        private string BuildReplaceRiskSummary(List<SceneObjectEntry> selected, int invalidCount)
        {
            var prefabInstanceCount = 0;
            var monoCount = 0;
            var colliderCount = 0;
            var lightmapCount = 0;

            foreach (var entry in selected)
            {
                var go = entry.Target;
                if (go == null)
                    continue;

                if (PrefabUtility.IsPartOfPrefabInstance(go))
                    prefabInstanceCount++;

                if (go.GetComponentsInChildren<MonoBehaviour>(true).Length > 0)
                    monoCount++;

                if (go.GetComponentsInChildren<Collider>(true).Length > 0)
                    colliderCount++;

                foreach (var renderer in go.GetComponentsInChildren<Renderer>(true))
                {
                    if (renderer != null && renderer.lightmapIndex >= 0)
                    {
                        lightmapCount++;
                        break;
                    }
                }
            }

            var msg = string.Empty;
            if (invalidCount > 0)
                msg += $"- {invalidCount} 个对象已失效或不再匹配，将跳过。\n";
            if (prefabInstanceCount > 0)
                msg += $"- {prefabInstanceCount} 个对象属于 Prefab Instance。\n";
            if (monoCount > 0)
                msg += $"- {monoCount} 个对象包含 MonoBehaviour。\n";
            if (colliderCount > 0)
                msg += $"- {colliderCount} 个对象包含 Collider。\n";
            if (lightmapCount > 0)
                msg += $"- {lightmapCount} 个对象包含 Lightmap 信息。\n";
            if (selected.Count >= 100)
                msg += $"- 本次数量较大：{selected.Count} 个，建议先保存场景。\n";

            return string.IsNullOrEmpty(msg) ? "未检测到明显高风险项。\n" : "风险提示：\n" + msg;
        }

        private static string AppendWarning(string origin, string warning)
        {
            if (string.IsNullOrWhiteSpace(warning))
                return origin;

            if (string.IsNullOrWhiteSpace(origin))
                return warning.Trim();

            return origin.TrimEnd() + "\n" + warning.Trim();
        }

        private string GetFirstMeshInfo(GameObject go)
        {
            if (go == null)
                return string.Empty;

            var mf = go.GetComponent<MeshFilter>();
            if (mf != null && mf.sharedMesh != null)
                return $"MeshFilter: {mf.sharedMesh.name}";

            var sk = go.GetComponent<SkinnedMeshRenderer>();
            if (sk != null && sk.sharedMesh != null)
                return $"SkinnedMesh: {sk.sharedMesh.name}";

            return string.Empty;
        }

        private bool IsProtectedEmptyParentName(string objectName)
        {
            if (string.IsNullOrWhiteSpace(objectName) || string.IsNullOrWhiteSpace(_protectedEmptyParentNames))
                return false;

            var names = _protectedEmptyParentNames.Split(new[] { ';', ',', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var name in names)
            {
                if (string.Equals(name.Trim(), objectName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        #endregion

        #region 通用工具方法

        private static void ApplyWorldTransform(Transform t, Vector3 worldPosition, Quaternion worldRotation, Vector3 worldScale)
        {
            t.position = worldPosition;
            t.rotation = worldRotation;
            SetWorldScale(t, worldScale);
        }

        private static void SetWorldScale(Transform t, Vector3 worldScale)
        {
            if (t.parent == null)
            {
                t.localScale = worldScale;
                return;
            }

            var parentScale = t.parent.lossyScale;
            t.localScale = new Vector3(
                Mathf.Abs(parentScale.x) > Mathf.Epsilon ? worldScale.x / parentScale.x : worldScale.x,
                Mathf.Abs(parentScale.y) > Mathf.Epsilon ? worldScale.y / parentScale.y : worldScale.y,
                Mathf.Abs(parentScale.z) > Mathf.Epsilon ? worldScale.z / parentScale.z : worldScale.z);
        }

        private static int DrawLayerMaskField(string label, int realLayerMask)
        {
            var layers = InternalEditorUtility.layers;
            var fieldMask = 0;

            for (var i = 0; i < layers.Length; i++)
            {
                var layer = LayerMask.NameToLayer(layers[i]);
                if (layer >= 0 && (realLayerMask & (1 << layer)) != 0)
                    fieldMask |= 1 << i;
            }

            fieldMask = EditorGUILayout.MaskField(label, fieldMask, layers);

            var newRealMask = 0;
            for (var i = 0; i < layers.Length; i++)
            {
                if ((fieldMask & (1 << i)) == 0)
                    continue;

                var layer = LayerMask.NameToLayer(layers[i]);
                if (layer >= 0)
                    newRealMask |= 1 << layer;
            }

            return newRealMask;
        }

        private static int GetDepth(Transform t)
        {
            var depth = 0;
            while (t != null)
            {
                depth++;
                t = t.parent;
            }

            return depth;
        }

        private static string GetPath(Transform t)
        {
            if (t == null)
                return "<Null>";

            var path = t.name;
            while (t.parent != null)
            {
                t = t.parent;
                path = t.name + "/" + path;
            }

            return path;
        }

        #endregion
    }
}

namespace _3rdBy.ByTools.ReferenceFinder.Editor
{
    using System.Collections.Generic;
    using System.IO;
    using UnityEditor;
    using UnityEditor.IMGUI.Controls;
    using UnityEngine;
    using UnityEngine.Serialization;

    public class ReferenceFinderWindow : EditorWindow
    {
        //依赖模式的key
        private const string IsDependPrefKey = "ReferenceFinderData_IsDepend";

        //是否需要更新信息状态的key
        private const string NeedUpdateStatePrefKey = "ReferenceFinderData_needUpdateState";

        private static readonly ReferenceFinderData data = new ReferenceFinderData();
        private static bool _initializedData = false;

        private bool _isDepend = false;
        private bool _needUpdateState = true;

        private bool _needUpdateAssetTree = false;

        private bool _initializedGUIStyle = false;

        //工具栏按钮样式
        private GUIStyle _toolbarButtonGUIStyle;

        //工具栏样式
        private GUIStyle _toolbarGUIStyle;

        //选中资源列表
        private readonly List<string> _selectedAssetGuid = new List<string>();

        private AssetTreeView _mAssetTreeView;

        [FormerlySerializedAs("mTreeViewState")] [SerializeField]
        private TreeViewState _mTreeViewState;

        //查找资源引用信息
        [MenuItem("Assets/在Project中查找引用 %#&f", false, 25)]
        private static void FindRef()
        {
            InitDataIfNeeded();
            OpenWindow();
            var window = GetWindow<ReferenceFinderWindow>();
            window.UpdateSelectedAssets();
        }

        //打开窗口
        [MenuItem("ByTools/🧩 对象的引用资源查找器", false, 1000)]
        private static void OpenWindow()
        {
            var window = GetWindow<ReferenceFinderWindow>();
            window.wantsMouseMove = false;
            window.titleContent   = new GUIContent("Ref Finder");
            window.Show();
            window.Focus();
        }

        //初始化数据
        private static void InitDataIfNeeded()
        {
            if (!_initializedData)
            {
                //初始化数据
                if (!data.ReadFromCache())
                {
                    data.CollectDependenciesInfo();
                }

                _initializedData = true;
            }
        }

        //初始化GUIStyle
        private void InitGUIStyleIfNeeded()
        {
            if (!_initializedGUIStyle)
            {
                _toolbarButtonGUIStyle = new GUIStyle("ToolbarButton");
                _toolbarGUIStyle       = new GUIStyle("Toolbar");
                _initializedGUIStyle   = true;
            }
        }

        //更新选中资源列表
        private void UpdateSelectedAssets()
        {
            _selectedAssetGuid.Clear();
            foreach (var obj in Selection.objects)
            {
                string path = AssetDatabase.GetAssetPath(obj);
                //如果是文件夹
                if (Directory.Exists(path))
                {
                    string[] folder = new string[] { path };
                    //将文件夹下所有资源作为选择资源
                    string[] guids = AssetDatabase.FindAssets(null, folder);
                    foreach (var guid in guids)
                    {
                        if (!_selectedAssetGuid.Contains(guid) &&
                            !Directory.Exists(AssetDatabase.GUIDToAssetPath(guid)))
                        {
                            _selectedAssetGuid.Add(guid);
                        }
                    }
                }
                //如果是文件资源
                else
                {
                    string guid = AssetDatabase.AssetPathToGUID(path);
                    _selectedAssetGuid.Add(guid);
                }
            }

            _needUpdateAssetTree = true;
        }

        //通过选中资源列表更新TreeView
        private void UpdateAssetTree()
        {
            if (_needUpdateAssetTree && _selectedAssetGuid.Count != 0)
            {
                var root = SelectedAssetGuidToRootItem(_selectedAssetGuid);
                if (_mAssetTreeView == null)
                {
                    //初始化TreeView
                    _mTreeViewState ??= new TreeViewState();
                    var headerState       = AssetTreeView.CreateDefaultMultiColumnHeaderState(position.width);
                    var multiColumnHeader = new MultiColumnHeader(headerState);
                    _mAssetTreeView = new AssetTreeView(_mTreeViewState, multiColumnHeader);
                }

                _mAssetTreeView.assetRoot = root;
                _mAssetTreeView.CollapseAll();
                _mAssetTreeView.Reload();
                _needUpdateAssetTree = false;
            }
        }

        private void OnEnable()
        {
            _isDepend        = PlayerPrefs.GetInt(IsDependPrefKey, 0) == 1;
            _needUpdateState = PlayerPrefs.GetInt(NeedUpdateStatePrefKey, 1) == 1;
        }

        private void OnGUI()
        {
            InitGUIStyleIfNeeded();
            DrawOptionBar();
            UpdateAssetTree();
            //绘制Treeview
            _mAssetTreeView?.OnGUI(new Rect(0, _toolbarGUIStyle.fixedHeight, position.width,
                position.height - _toolbarGUIStyle.fixedHeight));
        }

        //绘制上条
        private void DrawOptionBar()
        {
            EditorGUILayout.BeginHorizontal(_toolbarGUIStyle);
            //刷新数据
            if (GUILayout.Button("刷新数据", _toolbarButtonGUIStyle))
            {
                data.CollectDependenciesInfo();
                _needUpdateAssetTree = true;
                GUIUtility.ExitGUI();
            }

            //修改模式
            var preIsDepend = _isDepend;
            _isDepend = GUILayout.Toggle(_isDepend, _isDepend ? "Model(依赖)" : "Model(引用)",
                _toolbarButtonGUIStyle, GUILayout.Width(100));
            if (preIsDepend != _isDepend)
            {
                OnModelSelect();
            }

            //是否需要更新状态
            bool preNeedUpdateState = _needUpdateState;
            _needUpdateState = GUILayout.Toggle(_needUpdateState, "需要更新状态", _toolbarButtonGUIStyle);
            if (preNeedUpdateState != _needUpdateState)
            {
                PlayerPrefs.SetInt(NeedUpdateStatePrefKey, _needUpdateState ? 1 : 0);
            }

            GUILayout.FlexibleSpace();

            //扩展
            if (GUILayout.Button("展开", _toolbarButtonGUIStyle))
            {
                if (_mAssetTreeView != null) _mAssetTreeView.ExpandAll();
            }

            //折叠
            if (GUILayout.Button("折叠", _toolbarButtonGUIStyle))
            {
                if (_mAssetTreeView != null) _mAssetTreeView.CollapseAll();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void OnModelSelect()
        {
            _needUpdateAssetTree = true;
            PlayerPrefs.SetInt(IsDependPrefKey, _isDepend ? 1 : 0);
        }


        //生成root相关
        private readonly HashSet<string> _updatedAssetSet = new HashSet<string>();

        //通过选择资源列表生成TreeView的根节点
        private AssetViewItem SelectedAssetGuidToRootItem(List<string> selectedAssetGuid)
        {
            _updatedAssetSet.Clear();
            int elementCount = 0;
            var root         = new AssetViewItem { id = elementCount, depth = -1, displayName = "Root", data = null };
            int depth        = 0;
            var stack        = new Stack<string>();
            foreach (var childGuid in selectedAssetGuid)
            {
                var child = CreateTree(childGuid, ref elementCount, depth, stack);
                if (child != null)
                    root.AddChild(child);
            }

            _updatedAssetSet.Clear();
            return root;
        }

        //通过每个节点的数据生成子节点
        private AssetViewItem CreateTree(string guid, ref int elementCount, int depth, Stack<string> stack)
        {
            if (stack.Contains(guid))
                return null;

            stack.Push(guid);
            if (_needUpdateState && !_updatedAssetSet.Contains(guid))
            {
                data.UpdateAssetState(guid);
                _updatedAssetSet.Add(guid);
            }

            ++elementCount;
            var referenceData = data.assetDict[guid];
            var root = new AssetViewItem
                { id = elementCount, displayName = referenceData.name, data = referenceData, depth = depth };
            var childGuids = _isDepend ? referenceData.dependencies : referenceData.references;
            foreach (var childGuid in childGuids)
            {
                var child = CreateTree(childGuid, ref elementCount, depth + 1, stack);
                if (child != null)
                    root.AddChild(child);
            }

            stack.Pop();
            return root;
        }
    }
}
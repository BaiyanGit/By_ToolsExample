namespace _3rdBy.ByTools.ScenePlaySelector.Editor
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    /// <summary>
    /// Toolbar 场景显示配置器窗口。
    /// 用于控制哪些场景显示在 Toolbar 的 Scene 下拉菜单里。
    /// </summary>
    public class SceneSelectorEditorWindow : EditorWindow
    {
        /// <summary>
        /// 配置窗口中显示的一行场景数据。
        /// </summary>
        private class SceneRow
        {
            [Header("场景在当前列表中的索引")]
            public int index;

            [Header("场景名称")]
            public string name;

            [Header("场景资源路径")]
            public string path;

            [Header("是否显示在 Toolbar 场景下拉菜单中")]
            public bool show;

            [Header("场景资源对象")]
            public SceneAsset asset;
        }

        [Header("配置窗口顶部说明文本")]
        private const string HelpBoxMessage =
            "场景显示配置器说明\n" +
            "1. BuildSettings：读取 Build Settings 中的场景，默认只显示 enabled 场景。\n" +
            "2. ProjectAssets：读取项目 Assets/Packages 中所有 Scene 资源，默认全部显示。\n" +
            "3. 保存后会立即刷新 Toolbar。配置保存在 ProjectSettings/ScenePlaySelectorConfig.json。";

        [Header("配置窗口默认尺寸")]
        private static readonly Vector2 WindowSize = new(860, 620);

        [Header("当前正在编辑的场景来源")]
        private ScenePlaySource _source;

        [Header("场景列表滚动位置")]
        private Vector2 _scroll;

        [Header("场景搜索关键字")]
        private string _searchText = string.Empty;

        [Header("当前窗口中的场景行数据")]
        private readonly List<SceneRow> _rows = new();

        /// <summary>
        /// 打开配置窗口。
        /// </summary>
        [MenuItem("ByTools/🖼️ 场景显示配置器")]
        public static void ShowWindow()
        {
            var window = GetWindow<SceneSelectorEditorWindow>("场景显示配置器");
            window.minSize = WindowSize;
            window.Show();
        }

        /// <summary>
        /// 窗口启用时读取当前配置。
        /// </summary>
        private void OnEnable()
        {
            _source = ScenePlaySelectorStorage.GetSelectedSource();
            ReloadRows();
        }

        /// <summary>
        /// 绘制窗口 GUI。
        /// </summary>
        private void OnGUI()
        {
            EditorGUILayout.HelpBox(HelpBoxMessage, MessageType.Info);
            DrawToolbar();
            GUILayout.Space(6);
            DrawSummary();
            GUILayout.Space(4);
            DrawSceneList();
        }

        /// <summary>
        /// 绘制顶部操作栏。
        /// </summary>
        private void DrawToolbar()
        {
            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("场景来源：", GUILayout.Width(72));

            int newSourceIndex = EditorGUILayout.Popup(
                (int)_source,
                ScenePlaySelectorStorage.SourceDisplayNames,
                GUILayout.Width(180));

            if (newSourceIndex != (int)_source)
            {
                _source = ScenePlaySelectorStorage.SourceFromIndex(newSourceIndex);
                ScenePlaySelectorStorage.SaveSelectedSource(_source);
                ReloadRows();
                ScenePlaySelector.RefreshToolbar();
            }

            _searchText = EditorGUILayout.TextField("搜索", _searchText ?? string.Empty);

            if (GUILayout.Button("更新", GUILayout.Width(70)))
            {
                ReloadRows();
            }

            if (GUILayout.Button("保存并刷新", GUILayout.Width(100)))
            {
                SaveRowsAndRefresh();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("全选", GUILayout.Width(70)))
            {
                SetVisibleForFilteredRows(true);
            }

            if (GUILayout.Button("全不选", GUILayout.Width(70)))
            {
                SetVisibleForFilteredRows(false);
            }

            if (GUILayout.Button("反选", GUILayout.Width(70)))
            {
                InvertFilteredRows();
            }

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("重置当前来源配置", GUILayout.Width(130)))
            {
                if (EditorUtility.DisplayDialog("确认重置", $"确定重置 {ScenePlaySelectorStorage.GetSourceDisplayName(_source)} 的显示配置吗？", "重置", "取消"))
                {
                    ScenePlaySelectorStorage.ResetSource(_source);
                    ReloadRows();
                    ScenePlaySelector.RefreshToolbar();
                }
            }

            if (GUILayout.Button("打开配置目录", GUILayout.Width(110)))
            {
                string dir = ScenePlaySelectorStorage.ConfigDirectory;
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                EditorUtility.RevealInFinder(dir);
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// 绘制当前来源、场景数量、配置路径等信息。
        /// </summary>
        private void DrawSummary()
        {
            int total = _rows.Count;
            int visible = _rows.Count(r => r.show);

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField($"当前来源：{ScenePlaySelectorStorage.GetSourceDisplayName(_source)}");
            EditorGUILayout.LabelField($"场景总数：{total}，Toolbar 显示：{visible}，隐藏：{total - visible}");
            EditorGUILayout.LabelField($"配置文件：{ScenePlaySelectorStorage.ConfigPath}", EditorStyles.miniLabel);
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// 绘制场景列表。
        /// </summary>
        private void DrawSceneList()
        {
            var filteredRows = GetFilteredRows();

            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("显示", EditorStyles.boldLabel, GUILayout.Width(42));
            EditorGUILayout.LabelField("序号", EditorStyles.boldLabel, GUILayout.Width(42));
            EditorGUILayout.LabelField("场景", EditorStyles.boldLabel, GUILayout.Width(240));
            EditorGUILayout.LabelField("路径", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("操作", EditorStyles.boldLabel, GUILayout.Width(110));
            EditorGUILayout.EndHorizontal();

            if (filteredRows.Count == 0)
            {
                EditorGUILayout.HelpBox("没有匹配的场景。", MessageType.None);
                EditorGUILayout.EndVertical();
                return;
            }

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            foreach (var row in filteredRows)
            {
                DrawSceneRow(row);
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// 绘制单行场景配置。
        /// </summary>
        private void DrawSceneRow(SceneRow row)
        {
            EditorGUILayout.BeginHorizontal();

            row.show = EditorGUILayout.Toggle(row.show, GUILayout.Width(42));
            EditorGUILayout.LabelField(row.index.ToString(), GUILayout.Width(42));

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.ObjectField(row.asset, typeof(SceneAsset), false, GUILayout.Width(240));
            }

            EditorGUILayout.LabelField(row.path, EditorStyles.miniLabel);

            using (new EditorGUI.DisabledScope(row.asset == null))
            {
                if (GUILayout.Button("打开", GUILayout.Width(50)))
                {
                    OpenScene(row.path);
                }

                if (GUILayout.Button("定位", GUILayout.Width(50)))
                {
                    EditorGUIUtility.PingObject(row.asset);
                    Selection.activeObject = row.asset;
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// 从存储配置和项目真实场景中重新加载窗口列表。
        /// </summary>
        private void ReloadRows()
        {
            _rows.Clear();

            var scenes = ScenePlaySelectorStorage.GetMergedScenes(_source);
            for (int i = 0; i < scenes.Count; i++)
            {
                var scene = scenes[i];
                var asset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scene.path);

                _rows.Add(new SceneRow
                {
                    index = i,
                    name = string.IsNullOrWhiteSpace(scene.name) ? Path.GetFileNameWithoutExtension(scene.path) : scene.name,
                    path = scene.path,
                    show = scene.show,
                    asset = asset,
                });
            }
        }

        /// <summary>
        /// 保存当前窗口中的显示配置，并刷新 Toolbar。
        /// </summary>
        private void SaveRowsAndRefresh()
        {
            var saveList = _rows.Select(row => new ScenePlaySceneItem
            {
                index = row.index,
                name = row.name,
                path = row.path,
                show = row.show,
            }).ToList();

            ScenePlaySelectorStorage.SaveSourceScenes(_source, saveList);
            ScenePlaySelectorStorage.SaveSelectedSource(_source);
            ScenePlaySelector.RefreshToolbar();

            ShowNotification(new GUIContent("已保存并刷新 Toolbar"));
        }

        /// <summary>
        /// 获取搜索过滤后的场景行。
        /// </summary>
        private List<SceneRow> GetFilteredRows()
        {
            if (string.IsNullOrWhiteSpace(_searchText))
            {
                return _rows;
            }

            string keyword = _searchText.Trim();
            return _rows
                .Where(row =>
                    (!string.IsNullOrWhiteSpace(row.name) && row.name.IndexOf(keyword, System.StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (!string.IsNullOrWhiteSpace(row.path) && row.path.IndexOf(keyword, System.StringComparison.OrdinalIgnoreCase) >= 0))
                .ToList();
        }

        /// <summary>
        /// 设置过滤后所有场景是否显示。
        /// </summary>
        private void SetVisibleForFilteredRows(bool visible)
        {
            foreach (var row in GetFilteredRows())
            {
                row.show = visible;
            }
        }

        /// <summary>
        /// 反选过滤后的场景显示状态。
        /// </summary>
        private void InvertFilteredRows()
        {
            foreach (var row in GetFilteredRows())
            {
                row.show = !row.show;
            }
        }

        /// <summary>
        /// 从配置窗口中打开指定场景。
        /// </summary>
        private static void OpenScene(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog("提示", "播放中或即将进入播放模式时不能切换场景。", "确定");
                return;
            }

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) == null)
            {
                EditorUtility.DisplayDialog("提示", $"场景不存在或已被移动：\n{path}", "确定");
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            ScenePlaySelector.RefreshToolbar();
        }
    }
}

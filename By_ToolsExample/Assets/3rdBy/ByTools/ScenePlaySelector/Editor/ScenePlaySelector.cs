namespace _3rdBy.ByTools.ScenePlaySelector.Editor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEngine.UIElements;

    /// <summary>
    /// Editor Toolbar 场景快速切换器。
    /// 
    /// 功能：
    /// 1. 将 Source / Scene 下拉选项放到 Play/Pause/Step 的右侧。
    /// 2. Source 用于切换场景来源：BuildSettings 或 ProjectAssets。
    /// 3. Scene 用于快速切换当前编辑器场景。
    /// 4. 支持刷新和打开配置窗口。
    /// </summary>
    [InitializeOnLoad]
    public static class ScenePlaySelector
    {
        [Header("Toolbar 中当前工具容器的唯一名称")]
        private const string ToolbarElementName = "ScenePlaySelector";

        [Header("没有可用场景时显示的占位文本")]
        private const string EmptySceneLabel = "<无可用场景>";

        [Header("Source 来源下拉菜单控件")]
        private static VisualElementFactory.NativeToolbarDropdown _sourceDropdown;

        [Header("Scene 场景下拉菜单控件")]
        private static VisualElementFactory.NativeToolbarDropdown _sceneDropdown;

        [Header("当前 Source 下可显示的场景路径列表")]
        private static List<string> _currentScenePaths = new();

        [Header("是否正在内部刷新控件，防止刷新时触发回调")]
        private static bool _suppressCallback;

        [Header("下一次允许检测 Toolbar 是否存在的时间")]
        private static double _nextEnsureToolbarTime;

        /// <summary>
        /// 静态构造函数。
        /// Unity 加载编辑器域后自动执行，用于挂载 Toolbar 和注册事件。
        /// </summary>
        static ScenePlaySelector()
        {
            EditorApplication.delayCall += RefreshToolbar;
            EditorApplication.update += EnsureToolbarThrottled;
            EditorSceneManager.activeSceneChangedInEditMode += OnActiveSceneChangedInEditMode;
        }

        /// <summary>
        /// 对外刷新入口。
        /// 配置窗口保存后会调用这里刷新 Toolbar。
        /// </summary>
        public static void RefreshToolbar()
        {
            BuildToolbar();
        }

        /// <summary>
        /// 节流检测 Toolbar 是否存在。
        /// Unity 重新编译、切换布局或 Domain Reload 后，Toolbar 可能会重建，
        /// 所以这里每秒检查一次，缺失时重新挂载。
        /// </summary>
        private static void EnsureToolbarThrottled()
        {
            if (EditorApplication.timeSinceStartup < _nextEnsureToolbarTime)
            {
                return;
            }

            _nextEnsureToolbarTime = EditorApplication.timeSinceStartup + 1.0d;

            if (!EditorToolbarUtil.HasElement(ToolbarElementName))
            {
                BuildToolbar();
            }
        }

        /// <summary>
        /// 构建并挂载 Toolbar 控件。
        /// </summary>
        private static void BuildToolbar()
        {
            var selectedSource = ScenePlaySelectorStorage.GetSelectedSource();

            var container = new VisualElement
            {
                name = ToolbarElementName,
                style =
                {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                    marginLeft = 8,
                    marginRight = 4,
                    height = 22,
                    flexShrink = 0,
                }
            };

            _sourceDropdown = VisualElementFactory.CreateToolbarDropdown(
                "Source",
                ScenePlaySelectorStorage.SourceDisplayNames.ToList(),
                (int)selectedSource,
                150f);

            container.Add(_sourceDropdown);

            var sceneLabels = BuildSceneDropdownData(selectedSource, out int selectedSceneIndex);
            _sceneDropdown = VisualElementFactory.CreateToolbarDropdown(
                "Scene",
                sceneLabels,
                selectedSceneIndex,
                220f);

            container.Add(_sceneDropdown);

            var refreshButton = VisualElementFactory.CreateToolbarButton("↻", RefreshToolbar, "Refresh Scene List");
            container.Add(refreshButton);

            var configButton = VisualElementFactory.CreateToolbarButton("⚙", SceneSelectorEditorWindow.ShowWindow, "Scene Selector Settings");
            container.Add(configButton);

            if (!EditorToolbarUtil.AddOrReplaceToPlayModeRight(ToolbarElementName, container))
            {
                return;
            }

            RegisterCallbacks();
        }

        /// <summary>
        /// 注册 Source / Scene 下拉菜单的回调事件。
        /// </summary>
        private static void RegisterCallbacks()
        {
            if (_sourceDropdown != null)
            {
                _sourceDropdown.IndexChanged += _ =>
                {
                    if (_suppressCallback)
                    {
                        return;
                    }

                    var source = ScenePlaySelectorStorage.SourceFromIndex(_sourceDropdown.Index);
                    ScenePlaySelectorStorage.SaveSelectedSource(source);
                    RefreshSceneDropdown(source, selectActiveSceneWhenPossible: true);
                };
            }

            if (_sceneDropdown != null)
            {
                _sceneDropdown.IndexChanged += _ =>
                {
                    if (_suppressCallback)
                    {
                        return;
                    }

                    TryOpenSelectedScene();
                };
            }
        }

        /// <summary>
        /// 根据指定来源刷新 Scene 下拉菜单。
        /// </summary>
        private static void RefreshSceneDropdown(ScenePlaySource source, bool selectActiveSceneWhenPossible)
        {
            if (_sceneDropdown == null)
            {
                return;
            }

            _suppressCallback = true;
            try
            {
                var labels = BuildSceneDropdownData(source, out int selectedIndex, selectActiveSceneWhenPossible);
                _sceneDropdown.SetChoices(labels, selectedIndex, false);
            }
            finally
            {
                _suppressCallback = false;
            }
        }

        /// <summary>
        /// 构建 Scene 下拉菜单显示数据。
        /// </summary>
        private static List<string> BuildSceneDropdownData(ScenePlaySource source, out int selectedIndex, bool selectActiveSceneWhenPossible = true)
        {
            _currentScenePaths = ScenePlaySelectorStorage.GetVisibleScenePaths(source);

            var labels = BuildSceneLabels(_currentScenePaths);
            if (labels.Count == 0)
            {
                labels.Add(EmptySceneLabel);
                selectedIndex = 0;
                return labels;
            }

            string selectedPath = ScenePlaySelectorStorage.GetSelectedScenePath(source);
            string activePath = NormalizePath(SceneManager.GetActiveScene().path);

            if (selectActiveSceneWhenPossible && !string.IsNullOrWhiteSpace(activePath) && _currentScenePaths.Contains(activePath, StringComparer.OrdinalIgnoreCase))
            {
                selectedPath = activePath;
            }

            selectedIndex = _currentScenePaths.FindIndex(path =>
                string.Equals(path, selectedPath, StringComparison.OrdinalIgnoreCase));

            if (selectedIndex < 0)
            {
                selectedIndex = 0;
            }

            return labels;
        }

        /// <summary>
        /// 根据场景路径生成显示文本。
        /// 如果存在同名场景，会追加目录路径辅助区分。
        /// 当前激活场景前面会显示 ● 标记。
        /// </summary>
        private static List<string> BuildSceneLabels(List<string> scenePaths)
        {
            var result = new List<string>();
            if (scenePaths == null || scenePaths.Count == 0)
            {
                return result;
            }

            string activePath = NormalizePath(SceneManager.GetActiveScene().path);

            var names = scenePaths
                .Select(Path.GetFileNameWithoutExtension)
                .ToList();

            var duplicateNames = names
                .GroupBy(name => name, StringComparer.OrdinalIgnoreCase)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < scenePaths.Count; i++)
            {
                string path = NormalizePath(scenePaths[i]);
                string name = Path.GetFileNameWithoutExtension(path);

                string label = duplicateNames.Contains(name)
                    ? $"{name}  ({Path.GetDirectoryName(path)?.Replace('\\', '/')})"
                    : name;

                if (string.Equals(path, activePath, StringComparison.OrdinalIgnoreCase))
                {
                    label = $"● {label}";
                }

                result.Add(label);
            }

            return result;
        }

        /// <summary>
        /// 打开当前 Scene 下拉菜单选中的场景。
        /// </summary>
        private static void TryOpenSelectedScene()
        {
            if (_sceneDropdown == null || _currentScenePaths == null || _currentScenePaths.Count == 0)
            {
                return;
            }

            int index = _sceneDropdown.Index;
            if (index < 0 || index >= _currentScenePaths.Count)
            {
                return;
            }

            string targetScene = NormalizePath(_currentScenePaths[index]);
            if (string.IsNullOrWhiteSpace(targetScene))
            {
                return;
            }

            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog("提示", "播放中或即将进入播放模式时不能切换场景。", "确定");
                SyncDropdownToActiveScene();
                return;
            }

            var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(targetScene);
            if (sceneAsset == null)
            {
                EditorUtility.DisplayDialog("提示", $"场景不存在或已被移动：\n{targetScene}", "确定");
                RefreshToolbar();
                return;
            }

            string currentScene = NormalizePath(SceneManager.GetActiveScene().path);
            var source = ScenePlaySelectorStorage.SourceFromIndex(_sourceDropdown?.Index ?? 0);
            ScenePlaySelectorStorage.SaveSelectedScenePath(source, targetScene);

            if (string.Equals(currentScene, targetScene, StringComparison.OrdinalIgnoreCase))
            {
                SyncDropdownToActiveScene();
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                SyncDropdownToActiveScene();
                return;
            }

            EditorSceneManager.OpenScene(targetScene, OpenSceneMode.Single);
            SyncDropdownToActiveScene();
        }

        /// <summary>
        /// 当用户通过其它方式切换场景时，同步 Toolbar 上的当前场景显示。
        /// </summary>
        private static void OnActiveSceneChangedInEditMode(Scene oldScene, Scene newScene)
        {
            SyncDropdownToActiveScene();
        }

        /// <summary>
        /// 将 Scene 下拉菜单同步到当前激活场景。
        /// </summary>
        private static void SyncDropdownToActiveScene()
        {
            if (_sceneDropdown == null || _sourceDropdown == null || _currentScenePaths == null || _currentScenePaths.Count == 0)
            {
                return;
            }

            var source = ScenePlaySelectorStorage.SourceFromIndex(_sourceDropdown.Index);
            var labels = BuildSceneDropdownData(source, out int index, selectActiveSceneWhenPossible: true);

            _suppressCallback = true;
            try
            {
                _sceneDropdown.SetChoices(labels, index, false);
            }
            finally
            {
                _suppressCallback = false;
            }
        }

        /// <summary>
        /// 统一路径格式，避免 Windows 反斜杠导致路径比较失败。
        /// </summary>
        private static string NormalizePath(string path)
        {
            return path?.Replace('\\', '/').Trim() ?? string.Empty;
        }
    }
}

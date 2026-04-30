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
    /// 
    /// 当前 Scene 显示规则：
    /// 1. Scene 按钮永远优先显示 Unity 当前实际打开的 Scene。
    /// 2. Source 切换时不自动打开场景。
    /// 3. 当前实际打开的 Scene 如果存在于当前 Source 的可见列表中，则菜单项显示 Checked。
    /// 4. 当前实际打开的 Scene 如果不存在于当前 Source 的可见列表中，则菜单不勾选任何项，但按钮仍显示当前实际打开的 Scene 名称。
    /// 5. 用户手动点击 Scene 菜单项时，才打开该场景并保存为当前 Source 的 selectedScenePath。
    /// </summary>
    [InitializeOnLoad]
    public static class ScenePlaySelector
    {
        [Header("Toolbar 中当前工具容器的唯一名称")]
        private const string ToolbarElementName = "ScenePlaySelector";

        [Header("没有可用场景时显示的占位文本")]
        private const string EmptySceneLabel = "<无可用场景>";

        [Header("无场景名时显示的占位文本")]
        private const string UntitledSceneLabel = "<未保存场景>";

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

            var sceneLabels = BuildSceneDropdownDataByActiveScene(selectedSource, out int selectedSceneIndex);
            _sceneDropdown = VisualElementFactory.CreateToolbarDropdown(
                "Scene",
                sceneLabels,
                selectedSceneIndex,
                220f,
                GetActiveSceneDisplayName());

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

                    // Source 切换时只刷新右侧 Scene 列表，不自动打开场景。
                    // Scene 按钮继续显示 Unity 当前实际打开的 Scene。
                    // 如果当前实际打开的 Scene 存在于新 Source 列表中，则菜单勾选它；
                    // 否则菜单不勾选任何项。
                    RefreshSceneDropdownByActiveScene(source);
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
        /// 
        /// 这里不使用 Source 保存过的 selectedScenePath 决定显示状态，
        /// 因为只要没有主动打开新场景，Unity 当前实际打开的 Scene 才是唯一真实状态。
        /// </summary>
        private static void RefreshSceneDropdownByActiveScene(ScenePlaySource source)
        {
            if (_sceneDropdown == null)
            {
                return;
            }

            _suppressCallback = true;
            try
            {
                _sceneDropdown.SetEmptySelectionText(GetActiveSceneDisplayName());

                var labels = BuildSceneDropdownDataByActiveScene(source, out int selectedIndex);
                _sceneDropdown.SetChoices(labels, selectedIndex, false);
            }
            finally
            {
                _suppressCallback = false;
            }
        }

        /// <summary>
        /// 构建 Scene 下拉菜单显示数据。
        /// 
        /// 规则：
        /// 1. 按当前 Source 获取可见 Scene 列表；
        /// 2. 根据 Unity 当前实际打开的 Scene path 查找是否存在于该列表；
        /// 3. 如果存在，selectedIndex 为对应索引，菜单显示 Checked；
        /// 4. 如果不存在，selectedIndex = -1，按钮仍显示当前实际打开的 Scene 名，菜单不勾选任何项。
        /// </summary>
        private static List<string> BuildSceneDropdownDataByActiveScene(ScenePlaySource source, out int selectedIndex)
        {
            _currentScenePaths = ScenePlaySelectorStorage.GetVisibleScenePaths(source);

            var labels = BuildSceneLabels(_currentScenePaths);
            if (labels.Count == 0)
            {
                selectedIndex = -1;
                return labels;
            }

            string activePath = NormalizePath(SceneManager.GetActiveScene().path);

            selectedIndex = _currentScenePaths.FindIndex(path =>
                string.Equals(path, activePath, StringComparison.OrdinalIgnoreCase));

            if (selectedIndex < 0)
            {
                selectedIndex = -1;
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
        /// 只有用户手动点击 Scene 菜单项时才会进入这里。
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
            if (_sceneDropdown == null || _sourceDropdown == null)
            {
                return;
            }

            var source = ScenePlaySelectorStorage.SourceFromIndex(_sourceDropdown.Index);

            _suppressCallback = true;
            try
            {
                _sceneDropdown.SetEmptySelectionText(GetActiveSceneDisplayName());

                var labels = BuildSceneDropdownDataByActiveScene(source, out int index);
                _sceneDropdown.SetChoices(labels, index, false);
            }
            finally
            {
                _suppressCallback = false;
            }
        }

        /// <summary>
        /// 获取当前 Unity 实际打开的场景名称。
        /// 当当前场景不在当前 Source 列表中时，Scene 按钮仍显示这个名字，但菜单不勾选任何项。
        /// </summary>
        private static string GetActiveSceneDisplayName()
        {
            var activeScene = SceneManager.GetActiveScene();

            if (!string.IsNullOrWhiteSpace(activeScene.name))
            {
                return activeScene.name;
            }

            if (!string.IsNullOrWhiteSpace(activeScene.path))
            {
                return Path.GetFileNameWithoutExtension(activeScene.path);
            }

            return UntitledSceneLabel;
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

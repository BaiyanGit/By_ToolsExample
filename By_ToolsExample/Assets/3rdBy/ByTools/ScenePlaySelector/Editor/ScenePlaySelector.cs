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
    /// 快速切换场景
    /// </summary>
    [InitializeOnLoad]
    public static class ScenePlaySelector
    {
        private const string PrefKeyFoldPath = "ScenePlaySelector_FoldPath";
        private const string PrefKeySelectedIndexSource = "ScenePlaySelector_SelectedIndexSource";
        private const string PrefKeySelectedBuildScene = "ScenePlaySelector_BuildScene";
        private const string PrefKeySelectedProjectScene = "ScenePlaySelector_ProjectScene";
        private static readonly string[] sourceNames = { "BuildSettings", "ProjectAssets" };

        private enum SceneSource
        {
            BuildSettings,
            Project
        }

        static ScenePlaySelector()
        {
            EditorApplication.delayCall += InitToolbar;
        }

        public static void RefreshToolbar()
        {
            InitToolbar();
        }

        private static void InitToolbar()
        {
            var toolbarType = typeof(Editor).Assembly.GetType("UnityEditor.Toolbar");
            var toolbars    = Resources.FindObjectsOfTypeAll(toolbarType);
            if (toolbars.Length == 0)
            {
                Debug.LogError("ScenePlaySelector: 未找到工具栏 (Toolbar)");
                return;
            }

            var toolbar   = (ScriptableObject)toolbars[0];
            var rootField = toolbarType.GetField("m_Root", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (rootField == null)
            {
                Debug.LogError("ScenePlaySelector: 未找到工具栏 (m_Root)");
                return;
            }

            var root = rootField.GetValue(toolbar) as VisualElement;

            var leftZone = root.Q("ToolbarZoneLeftAlign");
            if (leftZone == null)
            {
                Debug.LogError("ScenePlaySelector: 未找到工具栏 (ToolbarZoneLeftAlign)");
                return;
            }

            var existingContainer = leftZone.Q("ScenePlaySelector");
            if (existingContainer != null) leftZone.Remove(existingContainer);

            var container = new VisualElement
            {
                name = "ScenePlaySelector",
                style =
                {
                    flexDirection = FlexDirection.Row,
                    alignItems    = Align.Center,
                    marginLeft    = 6,
                    marginRight   = 6,
                }
            };
            var label = VisualElementFactory.CreateToolbarLabel("Source:", FontStyle.Bold);
            container.Add(label);

            // --- 场景来源选项 ---
            var selectedSource = (SceneSource)EditorPrefs.GetInt(PrefKeySelectedIndexSource, 0);
            var sourcePopup    = VisualElementFactory.CreateToolbarPopup(sourceNames.ToList(), (int)selectedSource);
            container.Add(sourcePopup);

            // --- 场景标签 ---
            var sceneLabel = VisualElementFactory.CreateToolbarLabel("Scene:", FontStyle.Bold);
            container.Add(sceneLabel);

            // --- 选择场景弹出框 ---
            string[] scenePaths = GetScenePaths(selectedSource);
            var      sceneNames = scenePaths.Select(Path.GetFileNameWithoutExtension).ToList();

            int lastSelectedSceneIndex = selectedSource switch
            {
                SceneSource.BuildSettings => EditorPrefs.GetInt(PrefKeySelectedBuildScene, 0),
                SceneSource.Project       => EditorPrefs.GetInt(PrefKeySelectedProjectScene, 0),
                _                         => throw new ArgumentOutOfRangeException()
            };

            int selectedSceneIndex = Mathf.Clamp(value: lastSelectedSceneIndex, 0, sceneNames.Count - 1);
            var scenePopup         = VisualElementFactory.CreateToolbarPopup(sceneNames, selectedSceneIndex);
            container.Add(scenePopup);

            // --- Source 切换回调 ---
            sourcePopup.RegisterValueChangedCallback(evt =>
            {
                selectedSource = (SceneSource)sourcePopup.index;
                EditorPrefs.SetInt(PrefKeySelectedIndexSource, (int)selectedSource);

                // 刷新 Scene 列表
                scenePaths         = GetScenePaths(selectedSource);
                sceneNames         = scenePaths.Select(Path.GetFileNameWithoutExtension).ToList();
                scenePopup.choices = sceneNames;

                int idx = 0;
                if (sceneNames.Count > 0)
                {
                    int lastSelectedIndex = selectedSource switch
                    {
                        SceneSource.BuildSettings => EditorPrefs.GetInt(PrefKeySelectedBuildScene, 0),
                        SceneSource.Project       => EditorPrefs.GetInt(PrefKeySelectedProjectScene, 0),
                        _                         => throw new ArgumentOutOfRangeException()
                    };

                    idx = Mathf.Clamp(lastSelectedIndex, 0, sceneNames.Count - 1);
                }

                scenePopup.index = idx;
            });

            // --- 场景切换回调（立即打开 场景） ---
            scenePopup.RegisterValueChangedCallback(evt =>
            {
                int index = scenePopup.index;
                switch (selectedSource)
                {
                    case SceneSource.BuildSettings:
                        EditorPrefs.SetInt(PrefKeySelectedBuildScene, index);
                        break;
                    case SceneSource.Project:
                        EditorPrefs.SetInt(PrefKeySelectedProjectScene, index);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (index >= 0 && index < scenePaths.Length)
                {
                    string targetScene  = scenePaths[index];
                    string currentScene = SceneManager.GetActiveScene().path;
                    if (currentScene != targetScene)
                    {
                        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                            return;
                        EditorSceneManager.OpenScene(targetScene);
                    }
                }
            });

            var button = VisualElementFactory.CreateToolbarButton("⚙", SceneSelectorEditorWindow.ShowWindow);
            container.Add(button);
            leftZone.Add(container);
        }

        private static readonly List<string> activeScenesPath = new();

        private static string[] GetScenePaths(SceneSource source)
        {
            var foldPath     = EditorPrefs.GetString(PrefKeyFoldPath, string.Empty);
            var fileFullPath = Path.Combine(foldPath, $"{sourceNames[(int)source]}.json");
            var hasFile      = File.Exists(fileFullPath);
            activeScenesPath.Clear();
            // 未设置配置文件，使用默认查找场景
            if (string.IsNullOrEmpty(foldPath) || !hasFile)
            {
                // Debug.Log($"未找到配置文件，使用默认查找场景\n{PrefKeyFoldPath}");
                switch (source)
                {
                    case SceneSource.BuildSettings:
                        return EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
                    case SceneSource.Project:
                        var guids = AssetDatabase.FindAssets("t:Scene");
                        return guids.Select(AssetDatabase.GUIDToAssetPath).ToArray();
                    default:
                        return Array.Empty<string>();
                }
            }

            // Debug.Log($"使用配置文件数据:{fileFullPath}");
            // 使用配置文件数据
            var jsonContent     = File.ReadAllText(fileFullPath);
            var dataListWrapper = JsonUtility.FromJson<SceneDataListWrapper>(jsonContent);

            foreach (var sceneData in dataListWrapper.sceneDataList)
            {
                if (!sceneData.show) continue;
                activeScenesPath.Add(sceneData.path);
            }

            return activeScenesPath.ToArray();
        }
    }
}
namespace _3rdBy.ByTools.ScenePlaySelector.Editor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Toolbar 场景来源类型。
    /// </summary>
    public enum ScenePlaySource
    {
        [InspectorName("BuildSettings 场景")]
        BuildSettings = 0,

        [InspectorName("项目资源中的所有场景")]
        ProjectAssets = 1,
    }

    /// <summary>
    /// 单个场景显示配置。
    /// </summary>
    [Serializable]
    public class ScenePlaySceneItem
    {
        [Header("场景在当前来源列表中的索引")]
        public int index;

        [Header("场景显示名称")]
        public string name;

        [Header("场景资源路径")]
        public string path;

        [Header("是否显示在 Toolbar 场景下拉菜单中")]
        public bool show = true;
    }

    /// <summary>
    /// 某一个场景来源下的配置数据。
    /// </summary>
    [Serializable]
    public class ScenePlaySourceConfig
    {
        [Header("当前场景来源下的场景配置列表")]
        public List<ScenePlaySceneItem> scenes = new();
    }

    /// <summary>
    /// 场景快速切换工具的完整配置数据。
    /// 配置保存到 ProjectSettings/ScenePlaySelectorConfig.json。
    /// </summary>
    [Serializable]
    public class ScenePlaySelectorConfigData
    {
        [Header("当前选择的场景来源索引")]
        public int selectedSource;

        [Header("BuildSettings 来源上一次选择的场景路径")]
        public string selectedBuildScenePath = string.Empty;

        [Header("ProjectAssets 来源上一次选择的场景路径")]
        public string selectedProjectScenePath = string.Empty;

        [Header("BuildSettings 来源的显示配置")]
        public ScenePlaySourceConfig buildSettings = new();

        [Header("ProjectAssets 来源的显示配置")]
        public ScenePlaySourceConfig projectAssets = new();
    }

    /// <summary>
    /// 场景选择工具配置存储类。
    /// 负责读取、保存、修复配置，以及根据来源获取场景列表。
    /// </summary>
    public static class ScenePlaySelectorStorage
    {
        [Header("配置文件名称")]
        private const string ConfigFileName = "ScenePlaySelectorConfig.json";

        [Header("Toolbar 中显示的场景来源名称")]
        public static readonly string[] SourceDisplayNames =
        {
            "BuildSettings",
            "ProjectAssets",
        };

        /// <summary>
        /// 配置文件完整路径。
        /// </summary>
        public static string ConfigPath
        {
            get
            {
                string projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? Application.dataPath;
                return Path.Combine(projectRoot, "ProjectSettings", ConfigFileName);
            }
        }

        /// <summary>
        /// 配置文件所在目录。
        /// </summary>
        public static string ConfigDirectory => Path.GetDirectoryName(ConfigPath);

        /// <summary>
        /// 从磁盘读取配置。
        /// 如果配置文件不存在或读取失败，则返回默认配置。
        /// </summary>
        public static ScenePlaySelectorConfigData Load()
        {
            try
            {
                if (!File.Exists(ConfigPath))
                {
                    return Fix(new ScenePlaySelectorConfigData());
                }

                string json = File.ReadAllText(ConfigPath);
                var config = JsonUtility.FromJson<ScenePlaySelectorConfigData>(json);
                return Fix(config);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ScenePlaySelector] 读取配置失败：{ex.Message}");
                return Fix(new ScenePlaySelectorConfigData());
            }
        }

        /// <summary>
        /// 保存配置到 ProjectSettings。
        /// </summary>
        public static void Save(ScenePlaySelectorConfigData config)
        {
            try
            {
                config = Fix(config);

                if (!string.IsNullOrEmpty(ConfigDirectory) && !Directory.Exists(ConfigDirectory))
                {
                    Directory.CreateDirectory(ConfigDirectory);
                }

                string json = JsonUtility.ToJson(config, true);
                File.WriteAllText(ConfigPath, json);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ScenePlaySelector] 保存配置失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 修复配置中可能存在的空引用、非法索引和非法路径。
        /// </summary>
        public static ScenePlaySelectorConfigData Fix(ScenePlaySelectorConfigData config)
        {
            config ??= new ScenePlaySelectorConfigData();

            if (config.selectedSource < 0 || config.selectedSource >= SourceDisplayNames.Length)
            {
                config.selectedSource = 0;
            }

            config.selectedBuildScenePath ??= string.Empty;
            config.selectedProjectScenePath ??= string.Empty;
            config.buildSettings ??= new ScenePlaySourceConfig();
            config.projectAssets ??= new ScenePlaySourceConfig();
            config.buildSettings.scenes ??= new List<ScenePlaySceneItem>();
            config.projectAssets.scenes ??= new List<ScenePlaySceneItem>();

            FixSceneList(config.buildSettings.scenes);
            FixSceneList(config.projectAssets.scenes);

            return config;
        }

        /// <summary>
        /// 修复场景列表中的空数据和路径格式。
        /// </summary>
        private static void FixSceneList(List<ScenePlaySceneItem> scenes)
        {
            if (scenes == null)
            {
                return;
            }

            for (int i = scenes.Count - 1; i >= 0; i--)
            {
                var scene = scenes[i];
                if (scene == null || string.IsNullOrWhiteSpace(scene.path))
                {
                    scenes.RemoveAt(i);
                    continue;
                }

                scene.path = scene.path.Replace('\\', '/').Trim();
                scene.name = string.IsNullOrWhiteSpace(scene.name)
                    ? Path.GetFileNameWithoutExtension(scene.path)
                    : scene.name.Trim();
            }
        }

        /// <summary>
        /// 获取场景来源的显示名称。
        /// </summary>
        public static string GetSourceDisplayName(ScenePlaySource source)
        {
            int index = Mathf.Clamp((int)source, 0, SourceDisplayNames.Length - 1);
            return SourceDisplayNames[index];
        }

        /// <summary>
        /// 将索引转换为合法的场景来源枚举。
        /// </summary>
        public static ScenePlaySource SourceFromIndex(int index)
        {
            index = Mathf.Clamp(index, 0, SourceDisplayNames.Length - 1);
            return (ScenePlaySource)index;
        }

        /// <summary>
        /// 获取当前选择的场景来源。
        /// </summary>
        public static ScenePlaySource GetSelectedSource()
        {
            return SourceFromIndex(Load().selectedSource);
        }

        /// <summary>
        /// 保存当前选择的场景来源。
        /// </summary>
        public static void SaveSelectedSource(ScenePlaySource source)
        {
            var config = Load();
            config.selectedSource = (int)source;
            Save(config);
        }

        /// <summary>
        /// 获取指定来源上一次选择的场景路径。
        /// </summary>
        public static string GetSelectedScenePath(ScenePlaySource source)
        {
            var config = Load();
            return source switch
            {
                ScenePlaySource.BuildSettings => config.selectedBuildScenePath ?? string.Empty,
                ScenePlaySource.ProjectAssets => config.selectedProjectScenePath ?? string.Empty,
                _ => string.Empty
            };
        }

        /// <summary>
        /// 保存指定来源当前选择的场景路径。
        /// </summary>
        public static void SaveSelectedScenePath(ScenePlaySource source, string scenePath)
        {
            var config = Load();
            scenePath = scenePath?.Replace('\\', '/').Trim() ?? string.Empty;

            switch (source)
            {
                case ScenePlaySource.BuildSettings:
                    config.selectedBuildScenePath = scenePath;
                    break;
                case ScenePlaySource.ProjectAssets:
                    config.selectedProjectScenePath = scenePath;
                    break;
            }

            Save(config);
        }

        /// <summary>
        /// 获取实际场景列表与保存配置合并后的结果。
        /// 新增的场景会自动加入，已删除的场景会被过滤。
        /// </summary>
        public static List<ScenePlaySceneItem> GetMergedScenes(ScenePlaySource source)
        {
            var config = Load();
            var sourceConfig = GetSourceConfig(config, source);
            var savedMap = sourceConfig.scenes
                .Where(s => s != null && !string.IsNullOrWhiteSpace(s.path))
                .GroupBy(s => NormalizePath(s.path), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            var actualPaths = GetActualScenePaths(source);
            var result = new List<ScenePlaySceneItem>();

            for (int i = 0; i < actualPaths.Count; i++)
            {
                string path = NormalizePath(actualPaths[i]);
                savedMap.TryGetValue(path, out var saved);

                result.Add(new ScenePlaySceneItem
                {
                    index = i,
                    path = path,
                    name = Path.GetFileNameWithoutExtension(path),
                    show = saved?.show ?? GetDefaultShow(source, path),
                });
            }

            return result;
        }

        /// <summary>
        /// 获取指定来源中需要显示到 Toolbar 的场景路径列表。
        /// </summary>
        public static List<string> GetVisibleScenePaths(ScenePlaySource source)
        {
            return GetMergedScenes(source)
                .Where(s => s.show)
                .Select(s => s.path)
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .ToList();
        }

        /// <summary>
        /// 保存指定来源的场景显示配置。
        /// </summary>
        public static void SaveSourceScenes(ScenePlaySource source, IEnumerable<ScenePlaySceneItem> scenes)
        {
            var config = Load();
            var sourceConfig = GetSourceConfig(config, source);
            sourceConfig.scenes = scenes?
                .Where(s => s != null && !string.IsNullOrWhiteSpace(s.path))
                .Select((s, i) => new ScenePlaySceneItem
                {
                    index = i,
                    name = string.IsNullOrWhiteSpace(s.name) ? Path.GetFileNameWithoutExtension(s.path) : s.name.Trim(),
                    path = NormalizePath(s.path),
                    show = s.show,
                })
                .ToList() ?? new List<ScenePlaySceneItem>();

            Save(config);
        }

        /// <summary>
        /// 重置指定来源的配置。
        /// </summary>
        public static void ResetSource(ScenePlaySource source)
        {
            var config = Load();
            var sourceConfig = GetSourceConfig(config, source);
            sourceConfig.scenes.Clear();

            switch (source)
            {
                case ScenePlaySource.BuildSettings:
                    config.selectedBuildScenePath = string.Empty;
                    break;
                case ScenePlaySource.ProjectAssets:
                    config.selectedProjectScenePath = string.Empty;
                    break;
            }

            Save(config);
        }

        /// <summary>
        /// 根据场景来源获取项目中真实存在的场景路径。
        /// </summary>
        public static List<string> GetActualScenePaths(ScenePlaySource source)
        {
            switch (source)
            {
                case ScenePlaySource.BuildSettings:
                    return EditorBuildSettings.scenes
                        .Where(s => !string.IsNullOrWhiteSpace(s.path))
                        .Select(s => NormalizePath(s.path))
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToList();

                case ScenePlaySource.ProjectAssets:
                    return AssetDatabase.FindAssets("t:Scene")
                        .Select(AssetDatabase.GUIDToAssetPath)
                        .Where(p => !string.IsNullOrWhiteSpace(p))
                        .Select(NormalizePath)
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .OrderBy(p => p, StringComparer.OrdinalIgnoreCase)
                        .ToList();

                default:
                    return new List<string>();
            }
        }

        /// <summary>
        /// 根据来源获取对应的配置对象。
        /// </summary>
        private static ScenePlaySourceConfig GetSourceConfig(ScenePlaySelectorConfigData config, ScenePlaySource source)
        {
            config = Fix(config);
            return source switch
            {
                ScenePlaySource.BuildSettings => config.buildSettings,
                ScenePlaySource.ProjectAssets => config.projectAssets,
                _ => config.buildSettings
            };
        }

        /// <summary>
        /// 获取新场景默认是否显示。
        /// BuildSettings 来源默认跟随 enabled 状态。
        /// ProjectAssets 来源默认显示。
        /// </summary>
        private static bool GetDefaultShow(ScenePlaySource source, string path)
        {
            if (source != ScenePlaySource.BuildSettings)
            {
                return true;
            }

            var buildScene = EditorBuildSettings.scenes.FirstOrDefault(s =>
                string.Equals(NormalizePath(s.path), NormalizePath(path), StringComparison.OrdinalIgnoreCase));

            return buildScene == null || buildScene.enabled;
        }

        /// <summary>
        /// 统一路径格式，避免 Windows 反斜杠导致匹配失败。
        /// </summary>
        private static string NormalizePath(string path)
        {
            return path?.Replace('\\', '/').Trim() ?? string.Empty;
        }
    }
}

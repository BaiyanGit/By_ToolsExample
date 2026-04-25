//=====================================================
// 文件名称: MacroDefineBuildToolModels
// 创 建 者: wangbaiyan
// 创建日期: 2026-4-23
// 描    述: 宏定义打包工具的模型类。
//=====================================================

namespace MacroDefineBuildToolEditor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using UnityEditor;
    using UnityEditor.Build;
    using UnityEditor.Build.Reporting;
    using UnityEngine;

    /// <summary>
    /// 工具支持的平台枚举。
    /// </summary>
    public enum MacroBuildPlatform
    {
        Windows = 0,
        WebGL = 1,
        Linux = 2,
    }

    /// <summary>
    /// 挂起动作的执行阶段。
    /// 用来处理“切换平台 / 应用宏定义 / 编译完成后继续打包”的跨域流程。
    /// </summary>
    public enum PendingActionStep
    {
        [InspectorName("无操作")] None = 0,
        [InspectorName("切换构建目标")] SwitchBuildTarget = 1,
        [InspectorName("应用宏定义")] ApplyDefineSymbols = 2,
        [InspectorName("构建或完成")] BuildOrFinish = 3,
    }

    /// <summary>
    /// 单个宏定义项。
    /// </summary>
    [Serializable]
    public class MacroDefineItem
    {
        [Header("符号")] public string symbol;
        [Header("描述")] public string description;
    }

    /// <summary>
    /// 一套宏定义模板。
    /// </summary>
    [Serializable]
    public class MacroDefineMode
    {
        [Header("ID")] public string id;
        [Header("模式名称")] public string modeName;
        [Header("平台")] public MacroBuildPlatform platform;
        [Header("是否展开")] public bool isExpanded;
        [Header("定义列表")] public List<MacroDefineItem> defines = new();
    }

    /// <summary>
    /// 打包相关配置。
    /// </summary>
    [Serializable]
    public class MacroBuildSettings
    {
        [Header("输出根目录")] public string outputRoot = string.Empty;
        [Header("构建名称")] public string buildName = string.Empty;
        [Header("构建版本")] public string buildVersion = "1.0.0";
        [Header("同步版本到玩家设置")] public bool syncVersionToPlayerSettings = true;
        [Header("包含时间戳")] public bool includeTimestamp = true;
        [Header("包含平台前缀")] public bool includePlatformPrefix = true;
        [Header("构建后自动运行")] public bool autoRunAfterBuild;
        [Header("应用后立即构建")] public bool buildImmediatelyAfterApply;
    }

    /// <summary>
    /// 挂起动作数据。
    /// </summary>
    [Serializable]
    public class PendingActionData
    {
        [Header("是否有待处理操作")] public bool hasPendingAction;
        [Header("待处理操作步骤")] public PendingActionStep step = PendingActionStep.None;
        [Header("平台")] public MacroBuildPlatform platform;
        [Header("定义符号")] public string defineSymbols;
        [Header("应用后是否构建")] public bool buildAfterApply;
        [Header("源名称")] public string sourceName;
    }

    /// <summary>
    /// 工具总配置。
    /// </summary>
    [Serializable]
    public class MacroDefineBuildToolConfig
    {
        [Header("模板列表")] public List<MacroDefineMode> modes = new();
        [Header("构建设置")] public MacroBuildSettings buildSettings = new();
        [Header("当前编辑模式ID")] public string currentEditingModeId = string.Empty;
        [Header("当前编辑模式名称")] public string currentEditingModeName = "未命名";
        [Header("当前编辑平台")] public MacroBuildPlatform currentEditingPlatform = MacroBuildPlatform.Windows;
        [Header("当前定义折叠状态")] public bool currentDefinesFoldout = true;
        [Header("当前定义列表")] public List<MacroDefineItem> currentDefines = new();
        [Header("待处理操作数据")] public PendingActionData pendingAction = new();
    }

    /// <summary>
    /// 配置存取。
    /// 这里采用 JSON 文件，保存在 ProjectSettings 目录下。
    /// </summary>
    public static class MacroDefineBuildToolStorage
    {
        private const string CONFIG_FILE_NAME = "MacroDefineBuildToolConfig.json";

        /// <summary>
        /// 获取配置文件完整路径。
        /// </summary>
        private static string ConfigPath
        {
            get
            {
                string projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? Application.dataPath;
                return Path.Combine(projectRoot, "ProjectSettings", CONFIG_FILE_NAME);
            }
        }

        /// <summary>
        /// 读取配置。
        /// </summary>
        public static MacroDefineBuildToolConfig Load()
        {
            try
            {
                if (!File.Exists(ConfigPath))
                {
                    return CreateDefaultConfig();
                }

                string json   = File.ReadAllText(ConfigPath);
                var    config = JsonUtility.FromJson<MacroDefineBuildToolConfig>(json);
                return FixNullFields(config);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[宏定义打包工具] 读取配置失败：{ex.Message}");
                return CreateDefaultConfig();
            }
        }

        /// <summary>
        /// 保存配置。
        /// </summary>
        public static void Save(MacroDefineBuildToolConfig config)
        {
            try
            {
                config = FixNullFields(config);
                string json = JsonUtility.ToJson(config, true);
                string dir  = Path.GetDirectoryName(ConfigPath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                File.WriteAllText(ConfigPath, json);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[宏定义打包工具] 保存配置失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 创建默认配置。
        /// </summary>
        private static MacroDefineBuildToolConfig CreateDefaultConfig()
        {
            return FixNullFields(new MacroDefineBuildToolConfig());
        }

        /// <summary>
        /// 修复反序列化后的空字段，避免空引用。
        /// </summary>
        public static MacroDefineBuildToolConfig FixNullFields(MacroDefineBuildToolConfig config)
        {
            config                ??= new MacroDefineBuildToolConfig();
            config.modes          ??= new List<MacroDefineMode>();
            config.buildSettings  ??= new MacroBuildSettings();
            config.currentDefines ??= new List<MacroDefineItem>();
            config.pendingAction  ??= new PendingActionData();
            config.currentEditingModeName = string.IsNullOrWhiteSpace(config.currentEditingModeName)
                                                ? "未命名"
                                                : config.currentEditingModeName.Trim();

            if (string.IsNullOrWhiteSpace(config.buildSettings.buildName))
            {
                config.buildSettings.buildName = MacroDefineBuildToolUtility.GetDefaultBuildName();
            }

            if (string.IsNullOrWhiteSpace(config.buildSettings.buildVersion))
            {
                config.buildSettings.buildVersion = "1.0.0";
            }

            foreach (var mode in config.modes)
            {
                if (mode == null) continue;

                mode.defines  ??= new List<MacroDefineItem>();
                mode.modeName =   string.IsNullOrWhiteSpace(mode.modeName) ? "未命名模板" : mode.modeName.Trim();
                mode.id       =   string.IsNullOrWhiteSpace(mode.id) ? Guid.NewGuid().ToString("N") : mode.id;
            }

            return config;
        }
    }

    /// <summary>
    /// 工具通用逻辑。
    /// </summary>
    public static class MacroDefineBuildToolUtility
    {
        /// <summary>
        /// 获取默认构建名称。
        /// 默认取 PlayerSettings.productName。
        /// </summary>
        public static string GetDefaultBuildName()
        {
            string productName = PlayerSettings.productName;
            if (string.IsNullOrWhiteSpace(productName))
            {
                productName = Application.productName;
            }

            return string.IsNullOrWhiteSpace(productName) ? "NewBuild" : productName.Trim();
        }

        /// <summary>
        /// 让当前宏定义列表始终与 Unity 实际正在使用的宏定义保持一致。
        /// 如果能匹配到已保存模板，则显示对应模板名称；否则显示 Unity 当前宏定义。
        /// </summary>
        public static void SyncCurrentDefineStateFromUnity(MacroDefineBuildToolConfig config)
        {
            config = MacroDefineBuildToolStorage.FixNullFields(config);

            var activePlatform = GetCurrentActivePlatform();
            var activeDefines  = ParseDefineSymbolsString(GetScriptingDefineSymbols(activePlatform));
            var matchedMode    = FindEquivalentMode(config.modes, activePlatform, activeDefines);

            config.currentEditingPlatform = activePlatform;
            config.currentDefines         = CloneDefineList(activeDefines);
            config.currentEditingModeId   = matchedMode != null ? matchedMode.id : string.Empty;
            config.currentEditingModeName = matchedMode != null ? matchedMode.modeName : "未使用任何模板";
        }

        /// <summary>
        /// 复制宏定义列表，避免直接引用原对象。
        /// </summary>
        public static List<MacroDefineItem> CloneDefineList(List<MacroDefineItem> source)
        {
            var result = new List<MacroDefineItem>();
            if (source == null)
            {
                return result;
            }

            foreach (var item in source)
            {
                result.Add(new MacroDefineItem
                {
                    symbol      = item?.symbol ?? string.Empty,
                    description = item?.description ?? string.Empty,
                });
            }

            return result;
        }

        /// <summary>
        /// 复制模板对象。
        /// </summary>
        public static MacroDefineMode CloneMode(MacroDefineMode source)
        {
            if (source == null)
            {
                return null;
            }

            return new MacroDefineMode
            {
                id         = source.id,
                modeName   = source.modeName,
                platform   = source.platform,
                isExpanded = source.isExpanded,
                defines    = CloneDefineList(source.defines),
            };
        }

        /// <summary>
        /// 将宏定义列表转换为 Unity 实际使用的分号字符串。
        /// 会自动去重、去空白。
        /// </summary>
        private static string BuildDefineSymbolsString(List<MacroDefineItem> items)
        {
            if (items == null || items.Count == 0)
            {
                return string.Empty;
            }

            var unique = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var result = new List<string>();

            foreach (var item in items)
            {
                string symbol = NormalizeSymbol(item?.symbol);
                if (string.IsNullOrEmpty(symbol))
                {
                    continue;
                }

                if (unique.Add(symbol))
                {
                    result.Add(symbol);
                }
            }

            return string.Join(";", result);
        }

        /// <summary>
        /// 把 Unity 的分号宏定义字符串转成列表。
        /// </summary>
        public static List<MacroDefineItem> ParseDefineSymbolsString(string defineSymbols)
        {
            var result = new List<MacroDefineItem>();
            if (string.IsNullOrWhiteSpace(defineSymbols))
            {
                return result;
            }

            var      unique = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            string[] split  = defineSymbols.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string raw in split)
            {
                string symbol = NormalizeSymbol(raw);
                if (string.IsNullOrEmpty(symbol) || !unique.Add(symbol))
                {
                    continue;
                }

                result.Add(new MacroDefineItem
                {
                    symbol      = symbol,
                    description = string.Empty,
                });
            }

            return result;
        }

        /// <summary>
        /// 规范化宏定义名。
        /// </summary>
        public static string NormalizeSymbol(string symbol)
        {
            return string.IsNullOrWhiteSpace(symbol) ? string.Empty : symbol.Trim();
        }

        /// <summary>
        /// 校验宏定义项是否可添加 / 可修改。
        /// </summary>
        public static bool ValidateDefineItem(List<MacroDefineItem> targetList, string symbol, int editingIndex, out string error)
        {
            error  = string.Empty;
            symbol = NormalizeSymbol(symbol);

            if (string.IsNullOrEmpty(symbol))
            {
                error = "宏定义不能为空。";
                return false;
            }

            if (!IsValidDefineSymbol(symbol))
            {
                error = "宏定义格式不正确。建议只使用字母、数字和下划线，且不要以数字开头。";
                return false;
            }

            if (targetList != null)
            {
                for (int i = 0; i < targetList.Count; i++)
                {
                    if (i == editingIndex)
                    {
                        continue;
                    }

                    if (string.Equals(NormalizeSymbol(targetList[i]?.symbol), symbol, StringComparison.OrdinalIgnoreCase))
                    {
                        error = $"宏定义“{symbol}”重复，已自动拦截。";
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// 校验整份宏定义列表。
        /// </summary>
        public static bool ValidateDefineList(List<MacroDefineItem> items, out string error)
        {
            error = string.Empty;
            var list   = items ?? new List<MacroDefineItem>();
            var unique = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < list.Count; i++)
            {
                string symbol = NormalizeSymbol(list[i]?.symbol);
                if (string.IsNullOrEmpty(symbol))
                {
                    error = $"第 {i + 1} 项宏定义为空。";
                    return false;
                }

                if (!IsValidDefineSymbol(symbol))
                {
                    error = $"第 {i + 1} 项宏定义格式错误：{symbol}";
                    return false;
                }

                if (!unique.Add(symbol))
                {
                    error = $"存在重复宏定义：{symbol}";
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 校验保存模板时是否与其他模板出现“同平台、同配置”的重复项。
        /// </summary>
        public static bool ValidateModeConfigurationUnique(IEnumerable<MacroDefineMode> modes, string editingModeId, MacroBuildPlatform platform, List<MacroDefineItem> defines, out string error)
        {
            error = string.Empty;
            var duplicateMode = FindEquivalentMode(modes, platform, defines, editingModeId);
            if (duplicateMode != null)
            {
                error = $"已存在相同平台且配置完全一致的宏定义模板：{duplicateMode.modeName}。不能保存重复配置。";
                return false;
            }

            return true;
        }

        /// <summary>
        /// 简单校验宏定义格式。
        /// </summary>
        private static bool IsValidDefineSymbol(string symbol)
        {
            if (string.IsNullOrEmpty(symbol))
            {
                return false;
            }

            if (char.IsDigit(symbol[0]))
            {
                return false;
            }

            for (int i = 0; i < symbol.Length; i++)
            {
                char c = symbol[i];
                if (!(char.IsLetterOrDigit(c) || c == '_'))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 比较两个宏定义列表是否等价（忽略顺序、大小写和空白）。
        /// </summary>
        private static bool AreDefineListsEquivalent(List<MacroDefineItem> a, List<MacroDefineItem> b)
        {
            var setA = new HashSet<string>(ExtractNormalizedSymbols(a), StringComparer.OrdinalIgnoreCase);
            var setB = new HashSet<string>(ExtractNormalizedSymbols(b), StringComparer.OrdinalIgnoreCase);
            return setA.SetEquals(setB);
        }

        /// <summary>
        /// 在模板列表中查找与指定平台和宏定义列表等价的模板。
        /// </summary>
        private static MacroDefineMode FindEquivalentMode(IEnumerable<MacroDefineMode> modes, MacroBuildPlatform platform, List<MacroDefineItem> defines, string excludedModeId = null)
        {
            if (modes == null)
            {
                return null;
            }

            foreach (var mode in modes)
            {
                if (mode == null)
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(excludedModeId) && string.Equals(mode.id, excludedModeId, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (mode.platform != platform)
                {
                    continue;
                }

                if (AreDefineListsEquivalent(mode.defines, defines))
                {
                    return mode;
                }
            }

            return null;
        }

        /// <summary>
        /// 获取列表中的标准化宏定义集合。
        /// </summary>
        public static IEnumerable<string> ExtractNormalizedSymbols(List<MacroDefineItem> items)
        {
            if (items == null)
            {
                yield break;
            }

            var unique = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var item in items)
            {
                string symbol = NormalizeSymbol(item?.symbol);
                if (string.IsNullOrEmpty(symbol) || !unique.Add(symbol))
                {
                    continue;
                }

                yield return symbol;
            }
        }

        /// <summary>
        /// 根据平台获取 BuildTarget。
        /// </summary>
        private static BuildTarget GetBuildTarget(MacroBuildPlatform platform)
        {
            return platform switch
            {
                MacroBuildPlatform.Windows => BuildTarget.StandaloneWindows64,
                MacroBuildPlatform.WebGL   => BuildTarget.WebGL,
                MacroBuildPlatform.Linux   => BuildTarget.StandaloneLinux64,
                _                          => BuildTarget.StandaloneWindows64
            };
        }

        /// <summary>
        /// 根据平台获取 BuildTargetGroup。
        /// 注意：Windows / Linux 在 Unity 底层都属于 Standalone 分组。
        /// </summary>
        private static BuildTargetGroup GetBuildTargetGroup(MacroBuildPlatform platform)
        {
            switch (platform)
            {
                case MacroBuildPlatform.WebGL:
                    return BuildTargetGroup.WebGL;
                case MacroBuildPlatform.Windows:
                case MacroBuildPlatform.Linux:
                default:
                    return BuildTargetGroup.Standalone;
            }
        }

#if UNITY_2021_2_OR_NEWER
        /// <summary>
        /// 根据平台获取 NamedBuildTarget。
        /// 注意：Windows / Linux 共享 Standalone 宏定义组，这是 Unity 的底层规则。
        /// </summary>
        private static NamedBuildTarget GetNamedBuildTarget(MacroBuildPlatform platform)
        {
            switch (platform)
            {
                case MacroBuildPlatform.WebGL:
                    return NamedBuildTarget.WebGL;
                case MacroBuildPlatform.Windows:
                case MacroBuildPlatform.Linux:
                default:
                    return NamedBuildTarget.Standalone;
            }
        }
#endif

        /// <summary>
        /// 获取平台短名，用于输出命名。
        /// </summary>
        private static string GetPlatformShortName(MacroBuildPlatform platform)
        {
            switch (platform)
            {
                case MacroBuildPlatform.Windows:
                    return "Win";
                case MacroBuildPlatform.WebGL:
                    return "WebGL";
                case MacroBuildPlatform.Linux:
                    return "Linux";
                default:
                    return "Unknown";
            }
        }

        /// <summary>
        /// 获取平台显示名。
        /// </summary>
        public static string GetPlatformDisplayName(MacroBuildPlatform platform)
        {
            return platform switch
            {
                MacroBuildPlatform.Windows => "Windows",
                MacroBuildPlatform.WebGL   => "WebGL",
                MacroBuildPlatform.Linux   => "Linux",
                _                          => platform.ToString()
            };
        }

        /// <summary>
        /// 获取当前 Unity 正在使用的平台。
        /// 如果当前不是工具支持的平台，则默认回退到 Windows。
        /// </summary>
        public static MacroBuildPlatform GetCurrentActivePlatform()
        {
            switch (EditorUserBuildSettings.activeBuildTarget)
            {
                case BuildTarget.WebGL:
                    return MacroBuildPlatform.WebGL;
                case BuildTarget.StandaloneLinux64:
                    return MacroBuildPlatform.Linux;
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                default:
                    return MacroBuildPlatform.Windows;
            }
        }

        /// <summary>
        /// 获取指定平台的宏定义字符串。
        /// </summary>
        public static string GetScriptingDefineSymbols(MacroBuildPlatform platform)
        {
#if UNITY_2021_2_OR_NEWER
            return PlayerSettings.GetScriptingDefineSymbols(GetNamedBuildTarget(platform));
#else
            return PlayerSettings.GetScriptingDefineSymbolsForGroup(GetBuildTargetGroup(platform));
#endif
        }

        /// <summary>
        /// 设置指定平台的宏定义字符串。
        /// </summary>
        public static void SetScriptingDefineSymbols(MacroBuildPlatform platform, string defineSymbols)
        {
#if UNITY_2021_2_OR_NEWER
            PlayerSettings.SetScriptingDefineSymbols(GetNamedBuildTarget(platform), defineSymbols ?? string.Empty);
#else
            PlayerSettings.SetScriptingDefineSymbolsForGroup(GetBuildTargetGroup(platform), defineSymbols ?? string.Empty);
#endif
        }

        /// <summary>
        /// 判断某个保存模板是否就是当前项目“正在生效”的模板。
        /// </summary>
        public static bool IsModeCurrentlyActive(MacroDefineMode mode)
        {
            if (mode == null)
            {
                return false;
            }

            if (GetCurrentActivePlatform() != mode.platform)
            {
                return false;
            }

            var current = ParseDefineSymbolsString(GetScriptingDefineSymbols(mode.platform));
            return AreDefineListsEquivalent(mode.defines, current);
        }

        /// <summary>
        /// 尝试切换编辑器当前构建平台。
        /// </summary>
        public static bool SwitchActiveBuildTarget(MacroBuildPlatform platform, out string error)
        {
            error = string.Empty;
            var target = GetBuildTarget(platform);
            var group  = GetBuildTargetGroup(platform);

            if (EditorUserBuildSettings.activeBuildTarget == target)
            {
                return true;
            }

            bool success = EditorUserBuildSettings.SwitchActiveBuildTarget(group, target);
            if (!success)
            {
                error = $"切换平台失败：{GetPlatformDisplayName(platform)}";
            }

            return success;
        }

        /// <summary>
        /// 获取启用状态的构建场景。
        /// </summary>
        private static string[] GetEnabledBuildScenes()
        {
            return EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();
        }

        /// <summary>
        /// 生成最终构建名。
        /// 规则：平台_项目名_v版本_时间（按勾选项拼接）。
        /// </summary>
        private static string ComposeBuildBaseName(MacroBuildSettings settings, MacroBuildPlatform platform)
        {
            settings ??= new MacroBuildSettings();

            string buildName = SanitizeFileName(string.IsNullOrWhiteSpace(settings.buildName)
                                                    ? GetDefaultBuildName()
                                                    : settings.buildName.Trim());
            string result = buildName;

            if (settings.includePlatformPrefix)
            {
                result = $"{GetPlatformShortName(platform)}_{result}";
            }

            if (settings.includeTimestamp)
            {
                result = $"{result}_{DateTime.Now:yyyyMMdd_HHmm}";
            }

            if (!string.IsNullOrWhiteSpace(settings.buildVersion))
            {
                result = $"{result}_v{SanitizeFileName(settings.buildVersion.Trim())}";
            }

            return result;
        }

        /// <summary>
        /// 生成构建输出路径预览。
        /// </summary>
        public static string ComposeBuildPreviewPath(MacroBuildSettings settings, MacroBuildPlatform platform)
        {
            settings ??= new MacroBuildSettings();
            return ComposeBuildLocationPath(settings, platform);
        }

        /// <summary>
        /// 生成真正传给 BuildPipeline 的路径。
        /// </summary>
        private static string ComposeBuildLocationPath(MacroBuildSettings settings, MacroBuildPlatform platform)
        {
            settings ??= new MacroBuildSettings();

            // string root = string.IsNullOrWhiteSpace(settings.outputRoot)
            //                   ? Path.Combine(Directory.GetParent(Application.dataPath)?.FullName ?? Application.dataPath, "Builds")
            //                   : settings.outputRoot.Trim();

            string root = settings.outputRoot = string.IsNullOrWhiteSpace(settings.outputRoot)
                                                    ? Path.Combine(Directory.GetParent(Application.dataPath)?.FullName ?? Application.dataPath, "Builds")
                                                    : settings.outputRoot.Trim();

            string baseName = ComposeBuildBaseName(settings, platform);

            return platform switch
            {
                MacroBuildPlatform.Windows => Path.Combine(root, baseName + ".exe"),
                MacroBuildPlatform.WebGL   => Path.Combine(root, baseName),
                MacroBuildPlatform.Linux   => Path.Combine(root, baseName),
                _                          => Path.Combine(root, baseName)
            };
        }

        /// <summary>
        /// 清理不合法文件名字符。
        /// </summary>
        private static string SanitizeFileName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return "Unnamed";
            }

            foreach (char invalidChar in Path.GetInvalidFileNameChars())
            {
                name = name.Replace(invalidChar, '_');
            }

            return name;
        }

        /// <summary>
        /// 立即执行打包。
        /// </summary>
        public static bool ExecuteBuild(MacroDefineBuildToolConfig config, MacroBuildPlatform platform, out string message)
        {
            message = string.Empty;
            config  = MacroDefineBuildToolStorage.FixNullFields(config);

            string[] scenes = GetEnabledBuildScenes();
            if (scenes == null || scenes.Length == 0)
            {
                message = "当前没有启用的 Build Scenes，请先在 Build Settings 中勾选场景。";
                return false;
            }

            string buildName = string.IsNullOrWhiteSpace(config.buildSettings.buildName)
                                   ? GetDefaultBuildName()
                                   : config.buildSettings.buildName.Trim();
            config.buildSettings.buildName = buildName;

            string locationPath = ComposeBuildLocationPath(config.buildSettings, platform);
            EnsureBuildOutputDirectory(locationPath, platform);

            if (config.buildSettings.syncVersionToPlayerSettings)
            {
                PlayerSettings.bundleVersion = string.IsNullOrWhiteSpace(config.buildSettings.buildVersion)
                                                   ? PlayerSettings.bundleVersion
                                                   : config.buildSettings.buildVersion.Trim();
            }

            var options = BuildOptions.None;
            if (config.buildSettings.autoRunAfterBuild && platform != MacroBuildPlatform.WebGL)
            {
                options |= BuildOptions.AutoRunPlayer;
            }

            var buildPlayerOptions = new BuildPlayerOptions
            {
                scenes           = scenes,
                target           = GetBuildTarget(platform),
                targetGroup      = GetBuildTargetGroup(platform),
                locationPathName = locationPath,
                options          = options,
            };

            var report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            if (report.summary.result == BuildResult.Succeeded)
            {
                message = $"打包成功：{locationPath}\n总大小：{EditorUtility.FormatBytes((long)report.summary.totalSize)}";
                return true;
            }

            message = $"打包失败：{report.summary.result}\n输出路径：{locationPath}";
            return false;
        }

        /// <summary>
        /// 确保构建输出目录存在。
        /// </summary>
        private static void EnsureBuildOutputDirectory(string locationPath, MacroBuildPlatform platform)
        {
            if (string.IsNullOrWhiteSpace(locationPath))
            {
                return;
            }

            string directoryToCreate = platform switch
            {
                MacroBuildPlatform.WebGL => locationPath,
                _                        => Path.GetDirectoryName(locationPath)
            };

            if (!string.IsNullOrWhiteSpace(directoryToCreate) && !Directory.Exists(directoryToCreate))
            {
                Directory.CreateDirectory(directoryToCreate);
            }
        }

        /// <summary>
        /// 启动“应用宏定义 / 切换平台 / 编译完成后继续打包”的流程。
        /// </summary>
        public static void StartApplyPipeline(MacroDefineBuildToolConfig config, MacroBuildPlatform platform, List<MacroDefineItem> defines, bool buildAfterApply, string sourceName)
        {
            config = MacroDefineBuildToolStorage.FixNullFields(config);
            config.pendingAction = new PendingActionData
            {
                hasPendingAction = true,
                step             = PendingActionStep.SwitchBuildTarget,
                platform         = platform,
                defineSymbols    = BuildDefineSymbolsString(defines),
                buildAfterApply  = buildAfterApply,
                sourceName       = sourceName ?? string.Empty,
            };

            MacroDefineBuildToolStorage.Save(config);
            // Debug.Log($"[宏定义打包工具] 已开始流程：{sourceName}，目标平台：{GetPlatformDisplayName(platform)}，打包：{(buildAfterApply ? "是" : "否")}");
        }

        /// <summary>
        /// 清除挂起动作。
        /// </summary>
        public static void ClearPendingAction(MacroDefineBuildToolConfig config)
        {
            config.pendingAction = new PendingActionData();
            MacroDefineBuildToolStorage.Save(config);
        }
    }
}
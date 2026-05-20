namespace Demos.示例_录制视频Recorder.Scripts.UISettings
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using UnityEngine;

    public partial class UIRecorderParamsSettings
    {
        /// <summary>
        /// 创建录制工具需要的配置、FFmpeg 与视频输出目录。
        /// </summary>
        private void EnsureConfigDirectories()
        {
            Directory.CreateDirectory(GetConfigDirectory());
            Directory.CreateDirectory(GetFFmpegAppDirectory());
            Directory.CreateDirectory(GetDefaultVideoSaveDirectory());
        }

        /// <summary>
        /// 创建当前版本内置的六个模板配置文件。
        /// </summary>
        private void EnsureDefaultConfigs()
        {
            var defaults = new List<RecorderParamsConfig>
            {
                CreateDefaultConfig("Win", "Low", 0, true),
                CreateDefaultConfig("Win", "Medium", 1, true),
                CreateDefaultConfig("Win", "High", 2, true),
                CreateDefaultConfig("Linux", "Low", 0, true),
                CreateDefaultConfig("Linux", "Medium", 1, true),
                CreateDefaultConfig("Linux", "High", 2, true)
            };

            foreach (var config in defaults)
            {
                string path = Path.Combine(GetConfigDirectory(), config.fileName);
                if (!File.Exists(path)) File.WriteAllText(path, JsonUtility.ToJson(config, true));
            }
        }

        /// <summary>
        /// 根据平台与质量档位生成模板配置。
        /// </summary>
        private RecorderParamsConfig CreateDefaultConfig(string platform, string presetName, int qualityIndex, bool isDefault)
        {
            int fps = qualityIndex switch
            {
                0 => 20,
                1 => 25,
                _ => 30
            };
            float scale = qualityIndex switch
            {
                0 => 0.5f,
                1 => 0.75f,
                _ => 1f
            };
            int crf = qualityIndex switch
            {
                0 => 26,
                1 => 23,
                _ => 20
            };
            string videoRate = qualityIndex switch
            {
                0 => "2M",
                1 => "3M",
                _ => "5M"
            };
            string audioRate  = qualityIndex == 2 ? "192k" : "128k";
            string videoSpeed = qualityIndex == 2 ? "veryfast" : "ultrafast";
            return new RecorderParamsConfig
            {
                configName                 = presetName,
                platform                   = platform,
                isDefault                  = isDefault,
                fileName                   = BuildTemplateConfigFileName(platform, presetName),
                displayIndex               = 0,
                displayName                = GetSelectedDisplayName(),
                outputFilePrefix           = $"{Application.productName}_",
                useMode                    = 0,
                videoSaveDirectory         = GetDefaultVideoSaveDirectory(),
                outputAsWebm               = true,
                customFFmpegPath           = string.Empty,
                audioMode                  = 1,
                audioCodec                 = "aac",
                webmAudioCodec             = "libvorbis",
                audioBitrate               = audioRate,
                audioSampleRate            = 48000,
                audioChannels              = 2,
                captureFrameRate           = fps,
                outputScale                = scale,
                videoCrf                   = crf,
                pixelFormat                = "yuv420p",
                videoCodec                 = "libx264",
                videoPreset                = videoSpeed,
                webmVideoCodec             = "libvpx",
                webmVideoBitrate           = videoRate,
                webmDeadline               = "realtime",
                webmCpuUsed                = qualityIndex == 2 ? 6 : 8,
                stopVideoTimeoutMs         = 15000,
                waitTempFileReadyTimeoutMs = 8000,
                mergeTimeoutMs             = 0,
                deleteTempFilesAfterMerge  = true
            };
        }

        /// <summary>
        /// 刷新当前平台可用配置，并优先选中正在使用的配置。
        /// </summary>
        private void RefreshConfigDropdown()
        {
            _currentPlatformConfigs.Clear();
            _currentPlatformConfigs.AddRange(LoadConfigsForCurrentPlatform());
            if (drConfig == null || _currentPlatformConfigs.Count == 0) return;

            var reference = LoadCurrentRecordReference();
            _usingConfigFileName = reference?.fileName ?? string.Empty;

            _isRefreshingUI = true;
            drConfig.ClearOptions();
            var options = new List<string>();
            foreach (var config in _currentPlatformConfigs) options.Add(GetConfigDropdownLabel(config));
            drConfig.AddOptions(options);
            drConfig.value = GetConfigIndexByFileName(_usingConfigFileName);
            drConfig.RefreshShownValue();
            _isRefreshingUI = false;

            _currentConfig = _currentPlatformConfigs[drConfig.value].Clone();
            ApplyConfig(_currentConfig);
            _hasInitUsingConfig = true;
        }

        /// <summary>
        /// 获取配置文件下拉框显示名称。
        /// </summary>
        private static string GetConfigDropdownLabel(RecorderParamsConfig config)
        {
            string label = string.IsNullOrWhiteSpace(config.fileName) ? config.configName : Path.GetFileNameWithoutExtension(config.fileName);
            return IsTemplateFileName(config.fileName) ? $"{label}(默认)" : label;
        }

        /// <summary>
        /// 读取当前平台的模板与用户配置。
        /// </summary>
        private List<RecorderParamsConfig> LoadConfigsForCurrentPlatform()
        {
            string platform = GetCurrentPlatformName();
            var    configs  = new List<RecorderParamsConfig>();
            LoadConfigsFromDirectory(GetConfigDirectory(), platform, configs);
            configs.Sort((a, b) =>
            {
                if (a.isDefault != b.isDefault) return a.isDefault ? -1 : 1;
                int order = GetQualityOrder(a.configName).CompareTo(GetQualityOrder(b.configName));
                return order != 0 ? order : string.Compare(a.fileName, b.fileName, StringComparison.OrdinalIgnoreCase);
            });
            return configs;
        }

        /// <summary>
        /// 从指定目录读取符合当前平台命名规则的配置文件。
        /// </summary>
        private void LoadConfigsFromDirectory(string directory, string platform, List<RecorderParamsConfig> configs)
        {
            if (!Directory.Exists(directory)) return;
            foreach (string file in Directory.GetFiles(directory, "*.json", SearchOption.TopDirectoryOnly))
            {
                string fileName = Path.GetFileName(file);
                if (!IsRecordConfigFile(fileName, platform)) continue;

                try
                {
                    var config = JsonUtility.FromJson<RecorderParamsConfig>(File.ReadAllText(file));
                    if (config == null) continue;
                    config.fileName  = fileName;
                    config.platform  = platform;
                    config.isDefault = IsTemplateFileName(fileName);
                    if (string.IsNullOrWhiteSpace(config.configName)) config.configName = GetConfigNameFromFileName(fileName);
                    NormalizeLoadedConfig(config);
                    configs.Add(config);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"读取录制配置失败: {file}\n{e.Message}");
                }
            }
        }

        /// <summary>
        /// 保存配置文件到 Configs 根目录。
        /// </summary>
        private void SaveConfig(RecorderParamsConfig config)
        {
            if (config == null) return;
            config.platform         = GetCurrentPlatformName();
            config.isDefault        = IsTemplateFileName(config.fileName);
            config.customFFmpegPath = GetConfigFFmpegPath(config);
            if (string.IsNullOrWhiteSpace(config.displayName) && drDisplay != null && drDisplay.options != null && drDisplay.options.Count > 0)
            {
                int displayIndex = Mathf.Clamp(config.displayIndex, 0, drDisplay.options.Count - 1);
                config.displayName  = drDisplay.options[displayIndex].text;
                config.displayIndex = displayIndex;
            }

            if (string.IsNullOrWhiteSpace(config.videoSaveDirectory) && config.useMode == 0) config.videoSaveDirectory = GetDefaultVideoSaveDirectory();
            if (string.IsNullOrWhiteSpace(config.fileName)) config.fileName                                            = BuildUniqueUserConfigFileName(config.platform, config.configName);
            config.fileName = Path.GetFileName(config.fileName);
            string path = Path.Combine(GetConfigDirectory(), config.fileName);
            File.WriteAllText(path, JsonUtility.ToJson(config, true));
        }

        /// <summary>
        /// 保存当前平台的使用指针，指向某一个真实配置文件。
        /// </summary>
        private void SaveCurrentRecordConfig(RecorderParamsConfig config)
        {
            if (config == null) return;
            var reference = new RecordConfigReference
            {
                platform   = GetCurrentPlatformName(),
                sourceType = IsTemplateFileName(config.fileName) ? "Template" : "Creat",
                fileName   = Path.GetFileName(config.fileName)
            };
            File.WriteAllText(GetUseRecordConfigPath(), JsonUtility.ToJson(reference, true));
        }

        /// <summary>
        /// 读取当前平台正在使用的配置文件引用，源文件缺失时回退到当前平台 Medium 模板。
        /// </summary>
        private RecordConfigReference LoadCurrentRecordReference()
        {
            string                path      = GetUseRecordConfigPath();
            RecordConfigReference reference = null;
            if (File.Exists(path))
            {
                try
                {
                    reference = JsonUtility.FromJson<RecordConfigReference>(File.ReadAllText(path));
                }
                catch (Exception exception)
                {
                    Debug.LogWarning("读取当前录制配置引用失败: " + exception.Message);
                }
            }

            if (reference == null || !IsCurrentPlatform(reference.platform) || !HasConfigFileName(reference.fileName))
            {
                reference = CreateFallbackRecordReference();
                File.WriteAllText(path, JsonUtility.ToJson(reference, true));
            }

            reference.fileName   = Path.GetFileName(reference.fileName);
            reference.sourceType = IsTemplateFileName(reference.fileName) ? "Template" : "Creat";
            return reference;
        }

        /// <summary>
        /// 读取当前正在使用的真实配置内容。
        /// </summary>
        private RecorderParamsConfig LoadCurrentRecordConfig()
        {
            var    reference = LoadCurrentRecordReference();
            string path      = Path.Combine(GetConfigDirectory(), reference.fileName);
            if (!File.Exists(path)) return null;
            try
            {
                var config = JsonUtility.FromJson<RecorderParamsConfig>(File.ReadAllText(path));
                if (config == null) return null;
                config.fileName  = reference.fileName;
                config.platform  = GetCurrentPlatformName();
                config.isDefault = IsTemplateFileName(config.fileName);
                NormalizeLoadedConfig(config);
                return config;
            }
            catch (Exception exception)
            {
                Debug.LogWarning("读取当前录制配置失败: " + exception.Message);
                return null;
            }
        }

        /// <summary>
        /// 补全旧配置缺失的新目录字段，并兼容 Windows 平台旧命名。
        /// </summary>
        private void NormalizeLoadedConfig(RecorderParamsConfig config)
        {
            if (config == null) return;
            config.platform = NormalizePlatform(config.platform);
            if (string.IsNullOrWhiteSpace(config.videoSaveDirectory) && config.useMode == 0) config.videoSaveDirectory = GetDefaultVideoSaveDirectory();
            if (!string.IsNullOrWhiteSpace(config.customFFmpegPath))
                config.customFFmpegPath = config.customFFmpegPath.Replace("/StreamingAssets/FFmpegApp/", "/StreamingAssets/FFmpegTools/FFmpegApp/");
        }

        /// <summary>
        /// 根据文件名选中配置。
        /// </summary>
        private void SelectConfigByFileName(string fileName)
        {
            if (drConfig == null) return;
            int index = GetConfigIndexByFileName(fileName);
            drConfig.value = index;
            drConfig.RefreshShownValue();
            _currentConfig = _currentPlatformConfigs[index].Clone();
            ApplyConfig(_currentConfig);
        }

        /// <summary>
        /// 获取配置文件在当前平台列表中的索引。
        /// </summary>
        private int GetConfigIndexByFileName(string fileName)
        {
            for (int i = 0; i < _currentPlatformConfigs.Count; i++)
            {
                if (string.Equals(_currentPlatformConfigs[i].fileName, fileName, StringComparison.OrdinalIgnoreCase)) return i;
            }

            return 0;
        }

        /// <summary>
        /// 根据当前配置生成另存为窗口中的默认名称。
        /// </summary>
        private string BuildUserConfigName(RecorderParamsConfig config)
        {
            return config == null || string.IsNullOrWhiteSpace(config.fileName) ? "RecorderConfig" : GetConfigNameFromFileName(config.fileName);
        }

        /// <summary>
        /// 生成唯一的用户配置文件名。
        /// </summary>
        private string BuildUniqueUserConfigFileName(string platform, string configName)
        {
            string safeName = BuildUserConfigFileName(platform, configName);
            string baseName = Path.GetFileNameWithoutExtension(safeName);
            string ext      = Path.GetExtension(safeName);
            string fileName = safeName;
            int    index    = 1;

            while (File.Exists(Path.Combine(GetConfigDirectory(), fileName))) fileName = $"{baseName}_{index++:00}{ext}";
            return fileName;
        }

        /// <summary>
        /// 获取 FFmpegTools 工具根目录。
        /// </summary>
        private string GetToolsDirectory() => Path.Combine(Application.streamingAssetsPath, toolsFolderName);

        /// <summary>
        /// 获取 FFmpeg 可执行文件目录。
        /// </summary>
        private string GetFFmpegAppDirectory() => Path.Combine(GetToolsDirectory(), "FFmpegApp");

        /// <summary>
        /// 获取默认视频保存目录。
        /// </summary>
        private string GetDefaultVideoSaveDirectory() => Path.Combine(GetToolsDirectory(), videosFolderName);

        /// <summary>
        /// 获取配置根目录。
        /// </summary>
        private string GetConfigDirectory() => Path.Combine(GetToolsDirectory(), configFolderName);

        /// <summary>
        /// 获取选项说明 JSON 路径。
        /// </summary>
        private string GetOptionDescriptionPath() => Path.Combine(GetConfigDirectory(), optionDescriptionJsonName);

        /// <summary>
        /// 获取当前平台使用指针文件路径。
        /// </summary>
        private string GetUseRecordConfigPath() => Path.Combine(GetConfigDirectory(), GetUseRecordConfigFileName());

        /// <summary>
        /// 获取当前平台使用指针文件名。
        /// </summary>
        private string GetUseRecordConfigFileName() => GetCurrentPlatformName() == "Linux" ? "UseLinuxRecordConfig.json" : "UseWinRecordConfig.json";

        /// <summary>
        /// 生成当前平台回退到 Medium 模板的引用。
        /// </summary>
        private RecordConfigReference CreateFallbackRecordReference()
        {
            string platform = GetCurrentPlatformName();
            return new RecordConfigReference
            {
                platform   = platform,
                sourceType = "Template",
                fileName   = BuildTemplateConfigFileName(platform, "Medium")
            };
        }

        /// <summary>
        /// 生成模板配置文件名。
        /// </summary>
        private static string BuildTemplateConfigFileName(string platform, string presetName)
        {
            return BuildSafeFileName($"Template_{platform}_{presetName}.json");
        }

        /// <summary>
        /// 生成用户配置文件名。
        /// </summary>
        private static string BuildUserConfigFileName(string platform, string configName)
        {
            string rawName = string.IsNullOrWhiteSpace(configName) ? "RecorderConfig" : configName.Trim();
            rawName = Path.GetFileNameWithoutExtension(rawName);
            if (rawName.StartsWith($"Create_{platform}_", StringComparison.OrdinalIgnoreCase)) return BuildSafeFileName($"{rawName}.json");
            if (rawName.StartsWith($"User_{platform}_", StringComparison.OrdinalIgnoreCase)) rawName = rawName.Substring($"User_{platform}_".Length);
            return BuildSafeFileName($"Create_{platform}_{rawName}.json");
        }

        /// <summary>
        /// 清理非法文件名字符。
        /// </summary>
        private static string BuildSafeFileName(string fileName)
        {
            foreach (char c in Path.GetInvalidFileNameChars()) fileName = fileName.Replace(c, '_');
            return fileName;
        }

        /// <summary>
        /// 获取配置质量排序值。
        /// </summary>
        private static int GetQualityOrder(string configName)
        {
            if (configName.Contains("High") || configName.Contains("高")) return 0;
            if (configName.Contains("Medium") || configName.Contains("中")) return 1;
            if (configName.Contains("Low") || configName.Contains("低")) return 2;
            return 3;
        }

        /// <summary>
        /// 获取当前运行平台名称。
        /// </summary>
        private string GetCurrentPlatformName()
        {
            return Application.platform == RuntimePlatform.LinuxEditor || Application.platform == RuntimePlatform.LinuxPlayer ? "Linux" : "Win";
        }

        /// <summary>
        /// 判断平台字段是否匹配当前平台。
        /// </summary>
        private bool IsCurrentPlatform(string platform)
        {
            return string.IsNullOrWhiteSpace(platform) || string.Equals(NormalizePlatform(platform), GetCurrentPlatformName(), StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 兼容旧配置中的 Windows 平台名。
        /// </summary>
        private static string NormalizePlatform(string platform)
        {
            return string.Equals(platform, "Windows", StringComparison.OrdinalIgnoreCase) ? "Win" : platform;
        }

        /// <summary>
        /// 判断文件是否为当前平台录制配置。
        /// </summary>
        private static bool IsRecordConfigFile(string fileName, string platform)
        {
            return IsTemplateConfigFile(fileName, platform) || IsUserConfigFile(fileName, platform);
        }

        /// <summary>
        /// 判断文件是否为当前平台模板配置。
        /// </summary>
        private static bool IsTemplateConfigFile(string fileName, string platform)
        {
            return fileName.StartsWith($"Template_{platform}_", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 判断文件是否为当前平台用户配置。
        /// </summary>
        private static bool IsUserConfigFile(string fileName, string platform)
        {
            return fileName.StartsWith($"Create_{platform}_", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 判断文件是否为模板配置。
        /// </summary>
        private static bool IsTemplateFileName(string fileName)
        {
            return !string.IsNullOrWhiteSpace(fileName) && fileName.StartsWith("Template_", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 从文件名解析配置名称。
        /// </summary>
        private static string GetConfigNameFromFileName(string fileName)
        {
            string name  = Path.GetFileNameWithoutExtension(fileName);
            int    index = name.LastIndexOf('_');
            return index >= 0 && index < name.Length - 1 ? name.Substring(index + 1) : name;
        }

        /// <summary>
        /// 判断配置文件名是否存在于配置根目录。
        /// </summary>
        private bool HasConfigFileName(string fileName)
        {
            return !string.IsNullOrWhiteSpace(fileName) && File.Exists(Path.Combine(GetConfigDirectory(), Path.GetFileName(fileName)));
        }
    }
}
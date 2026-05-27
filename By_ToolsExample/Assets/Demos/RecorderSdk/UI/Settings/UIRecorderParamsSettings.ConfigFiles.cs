namespace Demos.示例_录制视频Recorder.Scripts.UISettings
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Core;
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
                schemaVersion              = 2,
                configId                   = BuildTemplateConfigId(platform, presetName),
                displayName                = $"{platform}_{presetName}",
                platform                   = platform,
                isDefault                  = isDefault,
                fileName                   = BuildConfigFileName(BuildTemplateConfigId(platform, presetName)),
                displayIndex               = 0,
                captureDisplayName         = GetSelectedDisplayName(),
                outputFilePrefix           = $"{Application.productName}_",
                useMode                    = 0,
                videoSaveDirectory         = string.Empty,
                streamUrl                  = string.Empty,
                streamVideoBitrate         = videoRate,
                streamGop                  = fps * 2,
                streamBufferSize           = qualityIndex == 0 ? "4M" : qualityIndex == 1 ? "6M" : "10M",
                streamLowLatency           = true,
                streamAutoReconnect        = true,
                streamReconnectCount       = 3,
                streamReconnectIntervalMs  = 3000,
                streamIncludeAudio         = false,
                outputAsWebm               = true,
                customFFmpegPath           = string.Empty,
                audioMode                  = 1,
                audioCodec                 = "aac",
                webmAudioCodec             = "libvorbis",
                audioBitrate               = audioRate,
                audioSampleRate            = 48000,
                audioChannels              = 2,
                enableAudioGain            = false,
                audioGainDb                = 0f,
                audioLimiterEnabled        = true,
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
            _usingConfigId = reference?.currentConfigId ?? string.Empty;

            _isRefreshingUI = true;
            drConfig.ClearOptions();
            var options = new List<string>();
            foreach (var config in _currentPlatformConfigs) options.Add(GetConfigDropdownLabel(config));
            drConfig.AddOptions(options);
            drConfig.value = GetConfigIndexByConfigId(_usingConfigId);
            drConfig.RefreshShownValue();
            _isRefreshingUI = false;

            _currentConfig = _currentPlatformConfigs[drConfig.value].Clone();
            ApplyConfig(_currentConfig);
        }

        /// <summary>
        /// 获取配置文件下拉框显示名称。
        /// </summary>
        private static string GetConfigDropdownLabel(RecorderParamsConfig config)
        {
            string label = !string.IsNullOrWhiteSpace(config.displayName) ? config.displayName : string.IsNullOrWhiteSpace(config.configId) ? "未命名配置" : config.configId;
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
                int order = GetQualityOrder(a).CompareTo(GetQualityOrder(b));
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
                if (fileName.StartsWith($"Template_{platform}_", StringComparison.OrdinalIgnoreCase))
                {
                    string newTemplateName = BuildTemplateConfigFileName(platform, GetConfigNameFromFileName(fileName));
                    if (!string.Equals(fileName, newTemplateName, StringComparison.OrdinalIgnoreCase) && File.Exists(Path.Combine(directory, newTemplateName))) continue;
                }

                try
                {
                    string json = File.ReadAllText(file);
                    var config = JsonUtility.FromJson<RecorderParamsConfig>(json);
                    if (config == null) continue;
                    config.fileName  = fileName;
                    config.platform  = platform;
                    config.isDefault = IsTemplateFileName(fileName);
                    RecorderConfigMigrator.MigrateIdentity(config, json, fileName);
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
            config.schemaVersion    = 2;
            config.platform         = GetCurrentPlatformName();
            config.configId         = EnsureConfigId(config, true);
            config.isDefault        = IsTemplateConfigId(config.configId) || IsTemplateFileName(config.fileName);
            config.displayName      = string.IsNullOrWhiteSpace(config.displayName) ? config.configId : config.displayName;
            config.customFFmpegPath = GetConfigFFmpegPath(config);
            if (string.IsNullOrWhiteSpace(config.captureDisplayName) && drDisplay != null && drDisplay.options != null && drDisplay.options.Count > 0)
            {
                int displayIndex = Mathf.Clamp(config.displayIndex, 0, drDisplay.options.Count - 1);
                config.captureDisplayName  = drDisplay.options[displayIndex].text;
                config.displayIndex = displayIndex;
            }

            if (string.IsNullOrWhiteSpace(config.videoSaveDirectory) && config.useMode == 0) config.videoSaveDirectory = GetDefaultVideoSaveDirectory();
            if (config.streamUrl == null) config.streamUrl = string.Empty;
            NormalizeStreamConfig(config);
            config.fileName = BuildConfigFileName(config.configId);
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
                schemaVersion = 2,
                platform   = GetCurrentPlatformName(),
                currentConfigId = config.configId
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
                    string json = File.ReadAllText(path);
                    reference = JsonUtility.FromJson<RecordConfigReference>(json);
                    RecorderConfigMigrator.MigrateReference(reference, json);
                }
                catch (Exception exception)
                {
                    Debug.LogWarning("读取当前录制配置引用失败: " + exception.Message);
                }
            }

            ResolveReferenceConfigId(reference);
            if (reference == null || !IsCurrentPlatform(reference.platform) || !HasConfigId(reference.currentConfigId))
            {
                reference = CreateFallbackRecordReference();
                File.WriteAllText(path, JsonUtility.ToJson(reference, true));
            }

            reference.currentConfigId = RecorderConfigMigrator.NormalizeConfigId(reference.currentConfigId);
            return reference;
        }

        /// <summary>
        /// 补全旧配置缺失的新目录字段，并兼容 Windows 平台旧命名。
        /// </summary>
        private void NormalizeLoadedConfig(RecorderParamsConfig config)
        {
            if (config == null) return;
            if (config.schemaVersion <= 0) config.schemaVersion = 2;
            config.platform = NormalizePlatform(config.platform);
            config.configId = EnsureConfigId(config, true);
            if (string.IsNullOrWhiteSpace(config.displayName)) config.displayName = config.configId;
            if (string.IsNullOrWhiteSpace(config.captureDisplayName)) config.captureDisplayName = GetSelectedDisplayName();
            if (string.IsNullOrWhiteSpace(config.videoSaveDirectory) && config.useMode == 0) config.videoSaveDirectory = GetDefaultVideoSaveDirectory();
            if (config.streamUrl == null) config.streamUrl = string.Empty;
            NormalizeStreamConfig(config);
            if (!string.IsNullOrWhiteSpace(config.customFFmpegPath))
                config.customFFmpegPath = config.customFFmpegPath.Replace("/StreamingAssets/FFmpegApp/", "/StreamingAssets/FFmpegTools/FFmpegApp/");
        }

        /// <summary>
        /// 补全推流配置默认值，兼容旧 JSON。
        /// </summary>
        private static void NormalizeStreamConfig(RecorderParamsConfig config)
        {
            if (config == null) return;
            if (string.IsNullOrWhiteSpace(config.streamVideoBitrate)) config.streamVideoBitrate = string.IsNullOrWhiteSpace(config.webmVideoBitrate) ? "3M" : config.webmVideoBitrate;
            if (config.streamGop <= 0) config.streamGop = Mathf.Clamp(config.captureFrameRate > 0 ? config.captureFrameRate * 2 : 50, 20, 120);
            if (string.IsNullOrWhiteSpace(config.streamBufferSize)) config.streamBufferSize = GetDoubleBitrate(config.streamVideoBitrate);
            if (config.streamReconnectCount <= 0) config.streamReconnectCount = 3;
            if (config.streamReconnectIntervalMs <= 0) config.streamReconnectIntervalMs = 3000;
        }

        /// <summary>
        /// 根据配置 ID 选中配置。
        /// </summary>
        private void SelectConfigByConfigId(string configId)
        {
            if (drConfig == null) return;
            int index = GetConfigIndexByConfigId(configId);
            drConfig.value = index;
            drConfig.RefreshShownValue();
            _currentConfig = _currentPlatformConfigs[index].Clone();
            ApplyConfig(_currentConfig);
        }

        /// <summary>
        /// 获取配置 ID 在当前平台列表中的索引。
        /// </summary>
        private int GetConfigIndexByConfigId(string configId)
        {
            string normalizedId = RecorderConfigMigrator.NormalizeConfigId(configId);
            for (int i = 0; i < _currentPlatformConfigs.Count; i++)
            {
                if (string.Equals(_currentPlatformConfigs[i].configId, normalizedId, StringComparison.OrdinalIgnoreCase)) return i;
            }

            return 0;
        }

        /// <summary>
        /// 根据当前配置生成另存为窗口中的默认名称。
        /// </summary>
        private string BuildUserConfigName(RecorderParamsConfig config)
        {
            if (config == null) return "RecorderConfig";
            if (!string.IsNullOrWhiteSpace(config.displayName)) return config.displayName;
            if (!string.IsNullOrWhiteSpace(config.configId)) return config.configId;
            return "RecorderConfig";
        }

        /// <summary>
        /// 生成唯一的用户配置 ID。
        /// </summary>
        private string BuildUniqueUserConfigId(string platform, string configName)
        {
            string baseId = BuildUserConfigId(platform, configName);
            string configId = baseId;
            int    index    = 1;

            while (HasConfigId(configId)) configId = $"{baseId}_{index++}";
            return configId;
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
                schemaVersion = 2,
                platform   = platform,
                currentConfigId = BuildTemplateConfigId(platform, "Medium")
            };
        }

        /// <summary>
        /// 生成模板配置文件名。
        /// </summary>
        private static string BuildTemplateConfigFileName(string platform, string presetName)
        {
            return BuildConfigFileName(BuildTemplateConfigId(platform, presetName));
        }

        /// <summary>
        /// 生成模板配置 ID。
        /// </summary>
        private static string BuildTemplateConfigId(string platform, string presetName)
        {
            return NormalizeConfigId($"template_{platform}_{presetName}");
        }

        /// <summary>
        /// 根据配置 ID 生成配置文件名。
        /// </summary>
        private static string BuildConfigFileName(string configId)
        {
            return BuildSafeFileName(RecorderConfigMigrator.BuildConfigFileName(configId));
        }

        /// <summary>
        /// 生成用户配置 ID。
        /// </summary>
        private static string BuildUserConfigId(string platform, string configName)
        {
            string rawName = string.IsNullOrWhiteSpace(configName) ? "RecorderConfig" : configName.Trim();
            rawName = Path.GetFileNameWithoutExtension(rawName);
            if (rawName.StartsWith($"Creat_{platform}_", StringComparison.OrdinalIgnoreCase)) rawName = rawName.Substring($"Creat_{platform}_".Length);
            if (rawName.StartsWith($"Create_{platform}_", StringComparison.OrdinalIgnoreCase)) rawName = rawName.Substring($"Create_{platform}_".Length);
            if (rawName.StartsWith($"create_{platform}_", StringComparison.OrdinalIgnoreCase)) rawName = rawName.Substring($"create_{platform}_".Length);
            if (rawName.StartsWith($"User_{platform}_", StringComparison.OrdinalIgnoreCase)) rawName = rawName.Substring($"User_{platform}_".Length);
            return NormalizeConfigId($"create_{platform}_{rawName}");
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
        /// <summary>
        /// 确保配置拥有稳定 ID。
        /// </summary>
        private string EnsureConfigId(RecorderParamsConfig config, bool keepExistingFileName)
        {
            if (config == null) return "recorder_config";
            if (!string.IsNullOrWhiteSpace(config.configId)) return RecorderConfigMigrator.NormalizeConfigId(config.configId);
            if (keepExistingFileName && !string.IsNullOrWhiteSpace(config.fileName)) return GetConfigIdFromFileName(config.fileName);
            string name = string.IsNullOrWhiteSpace(config.displayName) ? "RecorderConfig" : config.displayName;
            return config.isDefault ? BuildTemplateConfigId(config.platform, name) : BuildUniqueUserConfigId(config.platform, name);
        }

        /// <summary>
        /// 从配置文件名推导配置 ID。
        /// </summary>
        private static string GetConfigIdFromFileName(string fileName)
        {
            return RecorderConfigMigrator.GetConfigIdFromFileName(fileName);
        }

        /// <summary>
        /// 将字符串规整成 configId 允许的格式。
        /// </summary>
        private static string NormalizeConfigId(string value)
        {
            return RecorderConfigMigrator.NormalizeConfigId(value);
        }

        private static int GetQualityOrder(RecorderParamsConfig config)
        {
            string name = $"{config?.configId} {config?.displayName}";
            if (name.IndexOf("high", StringComparison.OrdinalIgnoreCase) >= 0 || name.Contains("高")) return 0;
            if (name.IndexOf("medium", StringComparison.OrdinalIgnoreCase) >= 0 || name.Contains("中")) return 1;
            if (name.IndexOf("low", StringComparison.OrdinalIgnoreCase) >= 0 || name.Contains("低")) return 2;
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
            return fileName.StartsWith($"Template_{platform}_", StringComparison.OrdinalIgnoreCase) ||
                   fileName.StartsWith($"template_{platform}_", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 判断文件是否为当前平台用户配置。
        /// </summary>
        private static bool IsUserConfigFile(string fileName, string platform)
        {
            return fileName.StartsWith($"Creat_{platform}_", StringComparison.OrdinalIgnoreCase) ||
                   fileName.StartsWith($"Create_{platform}_", StringComparison.OrdinalIgnoreCase) ||
                   fileName.StartsWith($"create_{platform}_", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 判断文件是否为模板配置。
        /// </summary>
        private static bool IsTemplateFileName(string fileName)
        {
            return !string.IsNullOrWhiteSpace(fileName) && (fileName.StartsWith("Template_", StringComparison.OrdinalIgnoreCase) || fileName.StartsWith("template_", StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// 判断配置 ID 是否为模板。
        /// </summary>
        private static bool IsTemplateConfigId(string configId)
        {
            return !string.IsNullOrWhiteSpace(configId) && configId.StartsWith("template_", StringComparison.OrdinalIgnoreCase);
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
        /// 根据码率计算默认缓冲区大小。
        /// </summary>
        private static string GetDoubleBitrate(string bitrate)
        {
            if (string.IsNullOrWhiteSpace(bitrate)) return "6M";
            string value      = bitrate.Trim();
            char   suffix     = value[value.Length - 1];
            string numberPart = char.IsLetter(suffix) ? value.Substring(0, value.Length - 1) : value;
            if (!float.TryParse(numberPart, out float number)) return "6M";
            return char.IsLetter(suffix) ? $"{number * 2:0.#}{suffix}" : $"{number * 2:0.#}";
        }

        /// <summary>
        /// 判断配置文件名是否存在于配置根目录。
        /// </summary>
        private bool HasConfigFileName(string fileName)
        {
            return !string.IsNullOrWhiteSpace(fileName) && File.Exists(Path.Combine(GetConfigDirectory(), Path.GetFileName(fileName)));
        }

        /// <summary>
        /// 判断配置 ID 对应文件是否存在。
        /// </summary>
        private bool HasConfigId(string configId)
        {
            if (string.IsNullOrWhiteSpace(configId)) return false;
            string normalizedId = RecorderConfigMigrator.NormalizeConfigId(configId);
            if (HasConfigFileName(BuildConfigFileName(normalizedId))) return true;
            return !string.IsNullOrWhiteSpace(FindConfigFileNameByConfigId(normalizedId));
        }

        /// <summary>
        /// 根据 currentConfigId 或旧 fileName 解析当前使用配置 ID。
        /// </summary>
        private void ResolveReferenceConfigId(RecordConfigReference reference)
        {
            if (reference == null) return;
            if (!string.IsNullOrWhiteSpace(reference.currentConfigId))
            {
                reference.currentConfigId = RecorderConfigMigrator.NormalizeConfigId(reference.currentConfigId);
                if (HasConfigId(reference.currentConfigId)) return;
            }

            if (!string.IsNullOrWhiteSpace(reference.fileName)) reference.currentConfigId = GetConfigIdFromFileName(reference.fileName);
        }

        /// <summary>
        /// 在配置目录中按 configId 查找配置文件，兼容旧文件名。
        /// </summary>
        private string FindConfigFileNameByConfigId(string configId)
        {
            if (string.IsNullOrWhiteSpace(configId) || !Directory.Exists(GetConfigDirectory())) return string.Empty;
            foreach (string file in Directory.GetFiles(GetConfigDirectory(), "*.json", SearchOption.TopDirectoryOnly))
            {
                string fileName = Path.GetFileName(file);
                if (!IsRecordConfigFile(fileName, GetCurrentPlatformName())) continue;
                try
                {
                    string json = File.ReadAllText(file);
                    var config = JsonUtility.FromJson<RecorderParamsConfig>(json);
                    if (config == null) continue;
                    RecorderConfigMigrator.MigrateIdentity(config, json, fileName);
                    if (string.Equals(config.configId, RecorderConfigMigrator.NormalizeConfigId(configId), StringComparison.OrdinalIgnoreCase)) return fileName;
                }
                catch
                {
                    // 忽略损坏配置，继续查找其它文件。
                }
            }

            return string.Empty;
        }
    }
}

namespace Demos.RecorderSdk.UI.Settings
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Core.Config;
    using UnityEngine;

    public partial class UIRecorderParamsSettings
    {
        /// <summary>
        /// 鍒涘缓褰曞埗宸ュ叿闇€瑕佺殑閰嶇疆銆丗Fmpeg 涓庤棰戣緭鍑虹洰褰曘€?        /// </summary>
        private void EnsureConfigDirectories()
        {
            Directory.CreateDirectory(GetConfigDirectory());
            Directory.CreateDirectory(GetDefaultTemplateDirectory());
            Directory.CreateDirectory(GetCustomTemplateDirectory());
            Directory.CreateDirectory(GetUseTemplateDirectory());
            Directory.CreateDirectory(GetOptionDescDirectory());
            Directory.CreateDirectory(GetFFmpegDirectory());
            Directory.CreateDirectory(GetDefaultVideoSaveDirectory());
        }

        /// <summary>
        /// 鍒涘缓褰撳墠鐗堟湰鍐呯疆鐨勫叚涓ā鏉块厤缃枃浠躲€?        /// </summary>
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
                string path = Path.Combine(GetDefaultTemplateDirectory(), config.fileName);
                if (!File.Exists(path)) File.WriteAllText(path, JsonUtility.ToJson(config, true));
            }
        }

        /// <summary>
        /// 鏍规嵁骞冲彴涓庤川閲忔。浣嶇敓鎴愭ā鏉块厤缃€?        /// </summary>
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
        /// 鍒锋柊褰撳墠骞冲彴鍙敤閰嶇疆锛屽苟浼樺厛閫変腑姝ｅ湪浣跨敤鐨勯厤缃€?        /// </summary>
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
        /// 鑾峰彇閰嶇疆鏂囦欢涓嬫媺妗嗘樉绀哄悕绉般€?        /// </summary>
        private static string GetConfigDropdownLabel(RecorderParamsConfig config)
        {
            return !string.IsNullOrWhiteSpace(config.displayName) ? config.displayName : GetDisplayNameFromConfigId(config.configId);
        }

        /// <summary>
        /// 璇诲彇褰撳墠骞冲彴鐨勬ā鏉夸笌鐢ㄦ埛閰嶇疆銆?        /// </summary>
        private List<RecorderParamsConfig> LoadConfigsForCurrentPlatform()
        {
            string platform = GetCurrentPlatformName();
            var    configs  = new List<RecorderParamsConfig>();
            LoadConfigsFromDirectory(GetDefaultTemplateDirectory(), platform, configs);
            LoadConfigsFromDirectory(GetCustomTemplateDirectory(), platform, configs);
            configs.Sort((a, b) =>
            {
                if (a.isDefault != b.isDefault) return a.isDefault ? -1 : 1;
                int order = GetQualityOrder(a).CompareTo(GetQualityOrder(b));
                return order != 0 ? order : string.Compare(a.fileName, b.fileName, StringComparison.OrdinalIgnoreCase);
            });
            return configs;
        }

        /// <summary>
        /// 浠庢寚瀹氱洰褰曡鍙栫鍚堝綋鍓嶅钩鍙板懡鍚嶈鍒欑殑閰嶇疆鏂囦欢銆?        /// </summary>
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
                    Debug.LogWarning($"璇诲彇褰曞埗閰嶇疆澶辫触: {file}\n{e.Message}");
                }
            }
        }

        /// <summary>
            /// 淇濆瓨閰嶇疆鏂囦欢鍒板綋鍓嶉厤缃被鍨嬪搴旂洰褰曘€?        /// </summary>
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
            string path = Path.Combine(GetConfigSaveDirectory(config), config.fileName);
            File.WriteAllText(path, JsonUtility.ToJson(config, true));
        }

        /// <summary>
        /// 淇濆瓨褰撳墠骞冲彴鐨勪娇鐢ㄦ寚閽堬紝鎸囧悜鏌愪竴涓湡瀹為厤缃枃浠躲€?        /// </summary>
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
        /// 璇诲彇褰撳墠骞冲彴姝ｅ湪浣跨敤鐨勯厤缃枃浠跺紩鐢紝婧愭枃浠剁己澶辨椂鍥為€€鍒板綋鍓嶅钩鍙?Medium 妯℃澘銆?        /// </summary>
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
                    Debug.LogWarning("璇诲彇褰撳墠褰曞埗閰嶇疆寮曠敤澶辫触: " + exception.Message);
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
        /// 琛ュ叏鏃ч厤缃己澶辩殑鏂扮洰褰曞瓧娈碉紝骞跺吋瀹?Windows 骞冲彴鏃у懡鍚嶃€?        /// </summary>
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
            {
                config.customFFmpegPath = config.customFFmpegPath.Replace("\\", "/");
            }
        }

        /// <summary>
        /// 琛ュ叏鎺ㄦ祦閰嶇疆榛樿鍊硷紝鍏煎鏃?JSON銆?        /// </summary>
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
        /// 鏍规嵁閰嶇疆 ID 閫変腑閰嶇疆銆?        /// </summary>
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
        /// 鑾峰彇閰嶇疆 ID 鍦ㄥ綋鍓嶅钩鍙板垪琛ㄤ腑鐨勭储寮曘€?        /// </summary>
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
        /// 鏍规嵁褰撳墠閰嶇疆鐢熸垚鍙﹀瓨涓虹獥鍙ｄ腑鐨勯粯璁ゅ悕绉般€?        /// </summary>
        private string BuildUserConfigName(RecorderParamsConfig config)
        {
            if (config == null) return "RecorderConfig";
            if (!string.IsNullOrWhiteSpace(config.displayName)) return config.displayName;
            if (!string.IsNullOrWhiteSpace(config.configId)) return config.configId;
            return "RecorderConfig";
        }

        /// <summary>
        /// 鐢熸垚鍞竴鐨勭敤鎴烽厤缃?ID銆?        /// </summary>
        private string BuildUniqueUserConfigId(string platform, string configName)
        {
            string baseId = BuildUserConfigId(platform, configName);
            string configId = baseId;
            int    index    = 1;

            while (HasConfigId(configId)) configId = $"{baseId}_{index++}";
            return configId;
        }

        /// <summary>
        /// 鑾峰彇 RecorderSDK 宸ュ叿鏍圭洰褰曘€?        /// </summary>
        private string GetToolsDirectory() => Path.Combine(Application.streamingAssetsPath, toolsFolderName);

        /// <summary>
        /// 鑾峰彇 FFmpeg 鍙墽琛屾枃浠剁洰褰曘€?        /// </summary>
        private string GetFFmpegDirectory() => Path.Combine(GetToolsDirectory(), "FFmpeg");

        /// <summary>
        /// 鑾峰彇榛樿瑙嗛淇濆瓨鐩綍銆?        /// </summary>
        private string GetDefaultVideoSaveDirectory() => Path.Combine(GetToolsDirectory(), videosFolderName);

        /// <summary>
        /// 鑾峰彇閰嶇疆鏍圭洰褰曘€?        /// </summary>
        private string GetConfigDirectory() => Path.Combine(GetToolsDirectory(), configFolderName);

        private string GetDefaultTemplateDirectory() => Path.Combine(GetConfigDirectory(), "DefaultTemplate");

        private string GetCustomTemplateDirectory() => Path.Combine(GetConfigDirectory(), "CustomTemplate");

        private string GetUseTemplateDirectory() => Path.Combine(GetConfigDirectory(), "UseTemplate");

        private string GetOptionDescDirectory() => Path.Combine(GetToolsDirectory(), "OptionDesc");

        private string GetConfigSaveDirectory(RecorderParamsConfig config) => config != null && (config.isDefault || IsTemplateConfigId(config.configId)) ? GetDefaultTemplateDirectory() : GetCustomTemplateDirectory();

        /// <summary>
        /// 鑾峰彇閫夐」璇存槑 JSON 璺緞銆?        /// </summary>
        private string GetOptionDescriptionPath() => Path.Combine(GetOptionDescDirectory(), optionDescriptionJsonName);

        /// <summary>
        /// 鑾峰彇褰撳墠骞冲彴浣跨敤鎸囬拡鏂囦欢璺緞銆?        /// </summary>
        private string GetUseRecordConfigPath() => Path.Combine(GetUseTemplateDirectory(), GetUseRecordConfigFileName());

        /// <summary>
        /// 鑾峰彇褰撳墠骞冲彴浣跨敤鎸囬拡鏂囦欢鍚嶃€?        /// </summary>
        private string GetUseRecordConfigFileName() => GetCurrentPlatformName() == "Linux" ? "UseLinuxRecordConfig.json" : "UseWinRecordConfig.json";

        /// <summary>
        /// 鐢熸垚褰撳墠骞冲彴鍥為€€鍒?Medium 妯℃澘鐨勫紩鐢ㄣ€?        /// </summary>
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
        /// 鐢熸垚妯℃澘閰嶇疆鏂囦欢鍚嶃€?        /// </summary>
        private static string BuildTemplateConfigFileName(string platform, string presetName)
        {
            return BuildConfigFileName(BuildTemplateConfigId(platform, presetName));
        }

        /// <summary>
        /// 鐢熸垚妯℃澘閰嶇疆 ID銆?        /// </summary>
        private static string BuildTemplateConfigId(string platform, string presetName)
        {
            return NormalizeConfigId($"template_{platform}_{presetName}");
        }

        /// <summary>
        /// 鏍规嵁閰嶇疆 ID 鐢熸垚閰嶇疆鏂囦欢鍚嶃€?        /// </summary>
        private static string BuildConfigFileName(string configId)
        {
            string normalizedId = RecorderConfigMigrator.NormalizeConfigId(configId);
            if (normalizedId.StartsWith("template_", StringComparison.OrdinalIgnoreCase))
            {
                string[] parts = normalizedId.Split('_');
                if (parts.Length >= 3) return BuildSafeFileName($"Template_{ToTitlePart(parts[1])}_{ToTitlePart(parts[2])}.json");
            }

            if (normalizedId.StartsWith("Custom_", StringComparison.OrdinalIgnoreCase))
            {
                string[] parts = normalizedId.Split(new[] { '_' }, 3);
                if (parts.Length >= 3) return BuildSafeFileName($"Custom_{NormalizeCustomPlatform(parts[1])}_{parts[2]}.json");
            }

            if (normalizedId.StartsWith("create_", StringComparison.OrdinalIgnoreCase))
            {
                string[] parts = normalizedId.Split(new[] { '_' }, 3);
                if (parts.Length >= 3) return BuildSafeFileName($"create_{ToTitlePart(parts[1])}_{parts[2]}.json");
            }

            return BuildSafeFileName(RecorderConfigMigrator.BuildConfigFileName(normalizedId));
        }

        private static string ToTitlePart(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : char.ToUpperInvariant(value[0]) + value.Substring(1).ToLowerInvariant();
        }

        /// <summary>
        /// 鐢熸垚鐢ㄦ埛閰嶇疆 ID銆?        /// </summary>
        private static string BuildUserConfigId(string platform, string configName)
        {
            string rawName = string.IsNullOrWhiteSpace(configName) ? "RecorderConfig" : configName.Trim();
            rawName = StripJsonExtension(rawName);
            if (rawName.StartsWith($"Custom_{platform}_", StringComparison.OrdinalIgnoreCase)) rawName = rawName.Substring($"Custom_{platform}_".Length);
            if (rawName.StartsWith($"Creat_{platform}_", StringComparison.OrdinalIgnoreCase)) rawName = rawName.Substring($"Creat_{platform}_".Length);
            if (rawName.StartsWith($"Create_{platform}_", StringComparison.OrdinalIgnoreCase)) rawName = rawName.Substring($"Create_{platform}_".Length);
            if (rawName.StartsWith($"create_{platform}_", StringComparison.OrdinalIgnoreCase)) rawName = rawName.Substring($"create_{platform}_".Length);
            if (rawName.StartsWith($"User_{platform}_", StringComparison.OrdinalIgnoreCase)) rawName = rawName.Substring($"User_{platform}_".Length);
            return NormalizeConfigId($"Custom_{NormalizeCustomPlatform(platform)}_{SanitizeCustomName(rawName)}");
        }

        /// <summary>
        /// 娓呯悊闈炴硶鏂囦欢鍚嶅瓧绗︺€?        /// </summary>
        private static string BuildSafeFileName(string fileName)
        {
            foreach (char c in Path.GetInvalidFileNameChars()) fileName = fileName.Replace(c, '_');
            foreach (char c in new[] { '\\', '/', ':', '*', '?', '"', '<', '>', '|' }) fileName = fileName.Replace(c, '_');
            return fileName;
        }

        private static string SanitizeCustomName(string value)
        {
            string name = string.IsNullOrWhiteSpace(value) ? "RecorderConfig" : value.Trim();
            name = StripJsonExtension(name);
            foreach (char c in Path.GetInvalidFileNameChars()) name = name.Replace(c, '_');
            foreach (char c in new[] { '\\', '/', ':', '*', '?', '"', '<', '>', '|' }) name = name.Replace(c, '_');
            name = name.Trim();
            return string.IsNullOrWhiteSpace(name) ? "RecorderConfig" : name;
        }

        private static string StripJsonExtension(string value)
        {
            return !string.IsNullOrWhiteSpace(value) && value.EndsWith(".json", StringComparison.OrdinalIgnoreCase) ? value.Substring(0, value.Length - ".json".Length) : value;
        }

        private static string NormalizeCustomPlatform(string value)
        {
            return string.Equals(value, "Linux", StringComparison.OrdinalIgnoreCase) ? "Linux" : "Win";
        }

        private static string GetDisplayNameFromConfigId(string configId)
        {
            if (string.IsNullOrWhiteSpace(configId)) return "未命名配置";
            string id = configId.Trim();
            if (id.StartsWith("Custom_", StringComparison.OrdinalIgnoreCase))
            {
                string[] parts = id.Split(new[] { '_' }, 3);
                if (parts.Length >= 3) return parts[2];
            }

            return id;
        }

        /// <summary>
        /// <summary>
        /// 纭繚閰嶇疆鎷ユ湁绋冲畾 ID銆?        /// </summary>
        private string EnsureConfigId(RecorderParamsConfig config, bool keepExistingFileName)
        {
            if (config == null) return "recorder_config";
            if (!string.IsNullOrWhiteSpace(config.configId)) return RecorderConfigMigrator.NormalizeConfigId(config.configId);
            if (keepExistingFileName && !string.IsNullOrWhiteSpace(config.fileName)) return GetConfigIdFromFileName(config.fileName);
            string name = string.IsNullOrWhiteSpace(config.displayName) ? "RecorderConfig" : config.displayName;
            return config.isDefault ? BuildTemplateConfigId(config.platform, name) : BuildUniqueUserConfigId(config.platform, name);
        }

        /// <summary>
        /// 浠庨厤缃枃浠跺悕鎺ㄥ閰嶇疆 ID銆?        /// </summary>
        private static string GetConfigIdFromFileName(string fileName)
        {
            return RecorderConfigMigrator.GetConfigIdFromFileName(fileName);
        }

        /// <summary>
        /// 灏嗗瓧绗︿覆瑙勬暣鎴?configId 鍏佽鐨勬牸寮忋€?        /// </summary>
        private static string NormalizeConfigId(string value)
        {
            return RecorderConfigMigrator.NormalizeConfigId(value);
        }

        private static int GetQualityOrder(RecorderParamsConfig config)
        {
            string name = $"{config?.configId} {config?.displayName}";
            if (name.IndexOf("high", StringComparison.OrdinalIgnoreCase) >= 0) return 0;
            if (name.IndexOf("medium", StringComparison.OrdinalIgnoreCase) >= 0) return 1;
            if (name.IndexOf("low", StringComparison.OrdinalIgnoreCase) >= 0) return 2;
            return 3;
        }

        /// <summary>
        /// 鑾峰彇褰撳墠杩愯骞冲彴鍚嶇О銆?        /// </summary>
        private string GetCurrentPlatformName()
        {
            return Application.platform == RuntimePlatform.LinuxEditor || Application.platform == RuntimePlatform.LinuxPlayer ? "Linux" : "Win";
        }

        /// <summary>
        /// 鍒ゆ柇骞冲彴瀛楁鏄惁鍖归厤褰撳墠骞冲彴銆?        /// </summary>
        private bool IsCurrentPlatform(string platform)
        {
            return string.IsNullOrWhiteSpace(platform) || string.Equals(NormalizePlatform(platform), GetCurrentPlatformName(), StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 鍏煎鏃ч厤缃腑鐨?Windows 骞冲彴鍚嶃€?        /// </summary>
        private static string NormalizePlatform(string platform)
        {
            return string.Equals(platform, "Windows", StringComparison.OrdinalIgnoreCase) ? "Win" : platform;
        }

        /// <summary>
        /// 鍒ゆ柇鏂囦欢鏄惁涓哄綋鍓嶅钩鍙板綍鍒堕厤缃€?        /// </summary>
        private static bool IsRecordConfigFile(string fileName, string platform)
        {
            return IsTemplateConfigFile(fileName, platform) || IsUserConfigFile(fileName, platform);
        }

        /// <summary>
        /// 鍒ゆ柇鏂囦欢鏄惁涓哄綋鍓嶅钩鍙版ā鏉块厤缃€?        /// </summary>
        private static bool IsTemplateConfigFile(string fileName, string platform)
        {
            return fileName.StartsWith($"Template_{platform}_", StringComparison.OrdinalIgnoreCase) ||
                   fileName.StartsWith($"template_{platform}_", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 鍒ゆ柇鏂囦欢鏄惁涓哄綋鍓嶅钩鍙扮敤鎴烽厤缃€?        /// </summary>
        private static bool IsUserConfigFile(string fileName, string platform)
        {
            return fileName.StartsWith($"Creat_{platform}_", StringComparison.OrdinalIgnoreCase) ||
                   fileName.StartsWith($"Create_{platform}_", StringComparison.OrdinalIgnoreCase) ||
                   fileName.StartsWith($"Custom_{platform}_", StringComparison.OrdinalIgnoreCase) ||
                   fileName.StartsWith($"create_{platform}_", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 鍒ゆ柇鏂囦欢鏄惁涓烘ā鏉块厤缃€?        /// </summary>
        private static bool IsTemplateFileName(string fileName)
        {
            return !string.IsNullOrWhiteSpace(fileName) && (fileName.StartsWith("Template_", StringComparison.OrdinalIgnoreCase) || fileName.StartsWith("template_", StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// 鍒ゆ柇閰嶇疆 ID 鏄惁涓烘ā鏉裤€?        /// </summary>
        private static bool IsTemplateConfigId(string configId)
        {
            return !string.IsNullOrWhiteSpace(configId) && configId.StartsWith("template_", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 浠庢枃浠跺悕瑙ｆ瀽閰嶇疆鍚嶇О銆?        /// </summary>
        private static string GetConfigNameFromFileName(string fileName)
        {
            string name  = Path.GetFileNameWithoutExtension(fileName);
            int    index = name.LastIndexOf('_');
            return index >= 0 && index < name.Length - 1 ? name.Substring(index + 1) : name;
        }

        /// <summary>
        /// 鏍规嵁鐮佺巼璁＄畻榛樿缂撳啿鍖哄ぇ灏忋€?        /// </summary>
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
        /// 鍒ゆ柇閰嶇疆鏂囦欢鍚嶆槸鍚﹀瓨鍦ㄤ簬閰嶇疆鏍圭洰褰曘€?        /// </summary>
        private bool HasConfigFileName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName)) return false;
            string safeName = Path.GetFileName(fileName);
            return File.Exists(Path.Combine(GetDefaultTemplateDirectory(), safeName)) ||
                   File.Exists(Path.Combine(GetCustomTemplateDirectory(), safeName)) ||
                   File.Exists(Path.Combine(GetConfigDirectory(), safeName));
        }

        /// <summary>
        /// 鍒ゆ柇閰嶇疆 ID 瀵瑰簲鏂囦欢鏄惁瀛樺湪銆?        /// </summary>
        private bool HasConfigId(string configId)
        {
            if (string.IsNullOrWhiteSpace(configId)) return false;
            string normalizedId = RecorderConfigMigrator.NormalizeConfigId(configId);
            if (HasConfigFileName(BuildConfigFileName(normalizedId))) return true;
            return !string.IsNullOrWhiteSpace(FindConfigFileNameByConfigId(normalizedId));
        }

        /// <summary>
        /// 鏍规嵁 currentConfigId 鎴栨棫 fileName 瑙ｆ瀽褰撳墠浣跨敤閰嶇疆 ID銆?        /// </summary>
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
        /// 鍦ㄩ厤缃洰褰曚腑鎸?configId 鏌ユ壘閰嶇疆鏂囦欢锛屽吋瀹规棫鏂囦欢鍚嶃€?        /// </summary>
        private string FindConfigFileNameByConfigId(string configId)
        {
            if (string.IsNullOrWhiteSpace(configId)) return string.Empty;
            foreach (string directory in new[] { GetDefaultTemplateDirectory(), GetCustomTemplateDirectory(), GetConfigDirectory() })
            {
                if (!Directory.Exists(directory)) continue;
                foreach (string file in Directory.GetFiles(directory, "*.json", SearchOption.TopDirectoryOnly))
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
                        // Ignore broken config files and keep searching.
                    }
                }
            }

            return string.Empty;
        }
    }
}

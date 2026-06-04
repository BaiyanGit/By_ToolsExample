//=====================================================
// 文件名称: RecorderConfigRegistry
// 创 建 者: wangbaiyan
// 创建日期: 2026-05-23
// 描    述: 统一管理录制配置扫描、索引、迁移、自动修复、当前配置指针和用户配置操作。
//=====================================================

using System;
using System.Collections.Generic;
using System.IO;
using Demos.示例_录制视频Recorder.Scripts.UISettings;
using UnityEngine;

namespace Demos.示例_录制视频Recorder.Scripts.Core
{
    /// <summary>
    /// 录制配置注册表，只管理配置文件，不启动录制、不拼接 FFmpeg、不修改录制状态。
    /// </summary>
    public class RecorderConfigRegistry
    {
        private readonly Dictionary<string, RecorderParamsConfig> _configsById = new(StringComparer.OrdinalIgnoreCase);
        private readonly List<RecorderParamsConfig> _configs = new();
        private readonly List<string> _warnings = new();

        public string configDirectory;
        public string platform;

        /// <summary>
        /// 创建配置注册表。
        /// </summary>
        public RecorderConfigRegistry(string configDirectory = "", string platform = "")
        {
            this.configDirectory = string.IsNullOrWhiteSpace(configDirectory) ? GetDefaultConfigDirectory() : configDirectory;
            this.platform = string.IsNullOrWhiteSpace(platform) ? GetCurrentPlatformTag() : NormalizePlatform(platform);
        }

        /// <summary>
        /// 获取已加载配置。
        /// </summary>
        public IReadOnlyList<RecorderParamsConfig> Configs => _configs;

        /// <summary>
        /// 获取本次扫描产生的警告。
        /// </summary>
        public IReadOnlyList<string> Warnings => _warnings;

        /// <summary>
        /// 扫描配置目录并按 configId 建索引。
        /// </summary>
        public RecorderConfigResult Scan()
        {
            _configs.Clear();
            _configsById.Clear();
            _warnings.Clear();

            try
            {
                if (!Directory.Exists(configDirectory))
                {
                    Directory.CreateDirectory(configDirectory);
                    AddWarning("配置目录不存在，已自动创建: " + configDirectory);
                }

                foreach (string directory in GetConfigScanDirectories())
                {
                    if (!Directory.Exists(directory)) continue;
                    foreach (string file in Directory.GetFiles(directory, "*.json", SearchOption.TopDirectoryOnly))
                    {
                        string fileName = Path.GetFileName(file);
                        if (!IsRecordConfigFile(fileName)) continue;
                        LoadConfigFile(file, fileName);
                    }
                }

                if (_configs.Count == 0)
                {
                    var fallback = CreateDefaultConfig(BuildTemplateConfigId(platform, "medium"), $"{platform} 中配置", true);
                    SaveConfig(fallback);
                    AddWarning("未找到任何配置，已创建默认中配置。");
                }

                return CreateResult(true, RecorderErrorCode.None, "配置扫描完成。", null);
            }
            catch (Exception e)
            {
                return RecorderConfigResult.Failed(RecorderErrorCode.ConfigDirectoryMissing, "扫描配置目录失败: " + e.Message);
            }
        }

        /// <summary>
        /// 获取当前平台正在使用的配置。
        /// </summary>
        public RecorderConfigResult GetCurrentConfig()
        {
            var scan = EnsureScanned();
            if (!scan.success) return scan;

            var reference = LoadUseReference();
            string configId = ResolveCurrentConfigId(reference);
            if (!_configsById.TryGetValue(configId, out var config))
            {
                var fallback = FallbackToDefaultTemplate();
                if (!fallback.success) return fallback;
                config = fallback.config;
            }

            SaveUseReference(config.configId);
            return CreateResult(true, RecorderErrorCode.None, "当前配置读取成功。", config);
        }

        /// <summary>
        /// 设置当前正在使用的配置。
        /// </summary>
        public RecorderConfigResult SetCurrentConfig(string configId)
        {
            var scan = EnsureScanned();
            if (!scan.success) return scan;

            string normalizedId = RecorderConfigMigrator.NormalizeConfigId(configId);
            if (!_configsById.TryGetValue(normalizedId, out var config))
                return RecorderConfigResult.Failed(RecorderErrorCode.ConfigNotFound, "配置不存在: " + normalizedId, normalizedId);

            SaveUseReference(config.configId);
            return CreateResult(true, RecorderErrorCode.None, "当前配置已切换。", config);
        }

        /// <summary>
        /// 创建用户配置。
        /// </summary>
        public RecorderConfigResult CreateUserConfig(string displayName)
        {
            var scan = EnsureScanned();
            if (!scan.success) return scan;

            var config = CreateDefaultConfig(string.Empty, displayName, false);
            AutoFixConfig(config, Path.GetFileName(BuildConfigFileName(config.configId)));
            return SaveConfig(config);
        }

        /// <summary>
        /// 克隆配置为用户配置。
        /// </summary>
        public RecorderConfigResult CloneConfig(string sourceConfigId, string displayName)
        {
            var scan = EnsureScanned();
            if (!scan.success) return scan;

            string normalizedId = RecorderConfigMigrator.NormalizeConfigId(sourceConfigId);
            if (!_configsById.TryGetValue(normalizedId, out var source))
                return RecorderConfigResult.Failed(RecorderErrorCode.ConfigNotFound, "克隆源配置不存在: " + normalizedId, normalizedId);

            var clone = source.Clone();
            clone.isDefault = false;
            clone.displayName = string.IsNullOrWhiteSpace(displayName) ? source.displayName + "_Copy" : displayName.Trim();
            clone.configId = GenerateUniqueConfigId(BuildCustomConfigId(platform, clone.displayName));
            clone.fileName = BuildConfigFileName(clone.configId);
            return SaveConfig(clone);
        }

        /// <summary>
        /// 删除用户配置，禁止删除模板。
        /// </summary>
        public RecorderConfigResult DeleteUserConfig(string configId)
        {
            var scan = EnsureScanned();
            if (!scan.success) return scan;

            string normalizedId = RecorderConfigMigrator.NormalizeConfigId(configId);
            if (!_configsById.TryGetValue(normalizedId, out var config))
                return RecorderConfigResult.Failed(RecorderErrorCode.ConfigNotFound, "配置不存在: " + normalizedId, normalizedId);
            if (IsTemplateConfig(config))
                return RecorderConfigResult.Failed(RecorderErrorCode.ConfigDeleteFailed, "模板配置不能删除。", normalizedId);

            try
            {
                string path = GetConfigPath(config);
                if (File.Exists(path)) File.Delete(path);
                string meta = path + ".meta";
                if (File.Exists(meta)) File.Delete(meta);

                Scan();
                var current = GetCurrentConfig();
                if (current.configId == normalizedId) FallbackToDefaultTemplate();
                return RecorderConfigResult.Success(config, "用户配置已删除。");
            }
            catch (Exception e)
            {
                return RecorderConfigResult.Failed(RecorderErrorCode.ConfigDeleteFailed, "删除配置失败: " + e.Message, normalizedId);
            }
        }

        /// <summary>
        /// 删除配置的对外兼容入口，模板配置仍然禁止删除。
        /// </summary>
        public RecorderConfigResult DeleteConfig(string configId)
        {
            return DeleteUserConfig(configId);
        }

        /// <summary>
        /// 重命名配置显示名。
        /// </summary>
        public RecorderConfigResult RenameDisplayName(string configId, string displayName)
        {
            var scan = EnsureScanned();
            if (!scan.success) return scan;

            string normalizedId = RecorderConfigMigrator.NormalizeConfigId(configId);
            if (!_configsById.TryGetValue(normalizedId, out var config))
                return RecorderConfigResult.Failed(RecorderErrorCode.ConfigNotFound, "配置不存在: " + normalizedId, normalizedId);

            config.displayName = string.IsNullOrWhiteSpace(displayName) ? config.configId : displayName.Trim();
            return SaveConfig(config);
        }

        /// <summary>
        /// 保存配置为 schemaVersion=2。
        /// </summary>
        public RecorderConfigResult SaveConfig(RecorderParamsConfig config)
        {
            if (config == null) return RecorderConfigResult.Failed(RecorderErrorCode.ConfigNull, "配置为空。");

            try
            {
                Directory.CreateDirectory(GetWritableConfigDirectory(config));
                var result = RecorderConfigResult.Success(config, "配置保存成功。");
                string originalFileName = config.fileName;
                AutoFixConfig(config, config.fileName, result.warnings);
                string targetFileName = BuildConfigFileName(config.configId);
                if (ConfigFileExists(targetFileName) &&
                    !string.Equals(originalFileName, targetFileName, StringComparison.OrdinalIgnoreCase))
                {
                    string oldId = config.configId;
                    config.configId = GenerateUniqueConfigId(config.configId);
                    config.fileName = BuildConfigFileName(config.configId);
                    result.AddWarning($"configId 重复，已从 {oldId} 修复为 {config.configId}");
                }

                string path = GetConfigPath(config);
                File.WriteAllText(path, JsonUtility.ToJson(config, true));
                Scan();
                result.configId = config.configId;
                result.config = config;
                if (result.warnings.Count > 0) result.errorCode = RecorderErrorCode.ConfigAutoFixed;
                return result;
            }
            catch (Exception e)
            {
                return RecorderConfigResult.Failed(RecorderErrorCode.ConfigSaveFailed, "保存配置失败: " + e.Message, config.configId);
            }
        }

        /// <summary>
        /// 回退到当前平台 Medium 模板，若不存在则逐级回退。
        /// </summary>
        public RecorderConfigResult FallbackToDefaultTemplate()
        {
            var scan = EnsureScanned();
            if (!scan.success) return scan;

            string mediumId = BuildTemplateConfigId(platform, "medium");
            if (_configsById.TryGetValue(mediumId, out var medium))
            {
                SaveUseReference(medium.configId);
                return CreateResult(true, RecorderErrorCode.None, "已回退到当前平台中配置。", medium);
            }

            foreach (var config in _configs)
            {
                if (IsTemplateConfig(config))
                {
                    SaveUseReference(config.configId);
                    return CreateResult(true, RecorderErrorCode.None, "已回退到可用模板配置。", config);
                }
            }

            var fallback = CreateDefaultConfig(mediumId, $"{platform} 中配置", true);
            var save = SaveConfig(fallback);
            if (save.success) SaveUseReference(fallback.configId);
            return save;
        }

        /// <summary>
        /// 加载单个配置文件。
        /// </summary>
        private void LoadConfigFile(string path, string fileName)
        {
            try
            {
                string json = File.ReadAllText(path);
                var config = JsonUtility.FromJson<RecorderParamsConfig>(json);
                if (config == null)
                {
                    AddWarning("配置文件解析为空: " + fileName);
                    return;
                }

                config.fileName = fileName;
                config.isDefault = IsTemplateFileName(fileName);
                RecorderConfigMigrator.MigrateIdentity(config, json, fileName);
                var autoFixWarnings = new List<string>();
                AutoFixConfig(config, fileName, autoFixWarnings);
                foreach (string warning in autoFixWarnings) AddWarning($"{fileName}: {warning}");

                if (_configsById.ContainsKey(config.configId))
                {
                    string oldId = config.configId;
                    config.configId = GenerateUniqueConfigId(config.configId);
                    config.fileName = BuildConfigFileName(config.configId);
                    AddWarning($"发现重复 configId，已从 {oldId} 修复为 {config.configId}");
                }

                _configs.Add(config);
                _configsById[config.configId] = config;
            }
            catch (Exception e)
            {
                AddWarning("配置迁移失败: " + fileName + "，" + e.Message);
            }
        }

        /// <summary>
        /// 自动修复配置并返回警告。
        /// </summary>
        public void AutoFixConfig(RecorderParamsConfig config, string sourceFileName, List<string> warnings = null)
        {
            if (config == null) return;
            warnings ??= _warnings;

            config.schemaVersion = 2;
            config.platform = NormalizePlatform(string.IsNullOrWhiteSpace(config.platform) ? platform : config.platform);
            if (string.IsNullOrWhiteSpace(config.configId))
            {
                config.configId = !string.IsNullOrWhiteSpace(sourceFileName)
                                      ? RecorderConfigMigrator.GetConfigIdFromFileName(sourceFileName)
                                      : GenerateUniqueConfigId(BuildCustomConfigId(config.platform, config.displayName));
                AddFixWarning(warnings, "configId 为空，已自动生成: " + config.configId);
            }

            string normalizedId = RecorderConfigMigrator.NormalizeConfigId(config.configId);
            if (!string.Equals(config.configId, normalizedId, StringComparison.Ordinal))
            {
                AddFixWarning(warnings, $"configId 非法，已修复为: {normalizedId}");
                config.configId = normalizedId;
            }

            if (string.IsNullOrWhiteSpace(config.displayName))
            {
                config.displayName = config.configId;
                AddFixWarning(warnings, "displayName 为空，已使用 configId。");
            }

            if (config.useMode == 0 && string.IsNullOrWhiteSpace(config.videoSaveDirectory))
            {
                AddFixWarning(warnings, "videoSaveDirectory 为空，运行时将回退默认 Videos 目录。");
            }

            if (string.IsNullOrWhiteSpace(config.customFFmpegPath))
            {
                AddFixWarning(warnings, "customFFmpegPath 为空，运行时将回退默认 FFmpeg 路径。");
            }

            if (config.captureFrameRate <= 0)
            {
                config.captureFrameRate = 25;
                AddFixWarning(warnings, "captureFrameRate <= 0，已回退 25。");
            }

            if (config.outputScale <= 0f)
            {
                config.outputScale = 1f;
                AddFixWarning(warnings, "outputScale <= 0，已回退 1.0。");
            }

            if (config.audioGainDb < -20f)
            {
                config.audioGainDb = -20f;
                AddFixWarning(warnings, "audioGainDb < -20，已回退 -20。");
            }

            if (config.audioGainDb > 20f)
            {
                config.audioGainDb = 20f;
                AddFixWarning(warnings, "audioGainDb > 20，已回退 20。");
            }

            if (config.streamReconnectCount < 0)
            {
                config.streamReconnectCount = 0;
                AddFixWarning(warnings, "streamReconnectCount < 0，已回退 0。");
            }

            if (config.streamReconnectIntervalMs <= 0)
            {
                config.streamReconnectIntervalMs = 3000;
                AddFixWarning(warnings, "streamReconnectIntervalMs <= 0，已回退 3000。");
            }

            if (config.streamUrl == null) config.streamUrl = string.Empty;
            if (string.IsNullOrWhiteSpace(config.streamVideoBitrate)) config.streamVideoBitrate = string.IsNullOrWhiteSpace(config.webmVideoBitrate) ? "3M" : config.webmVideoBitrate;
            if (config.streamGop <= 0) config.streamGop = Mathf.Clamp(config.captureFrameRate * 2, 20, 120);
            if (string.IsNullOrWhiteSpace(config.streamBufferSize)) config.streamBufferSize = GetDoubleBitrate(config.streamVideoBitrate);
            if (string.IsNullOrWhiteSpace(config.fileName)) config.fileName = BuildConfigFileName(config.configId);
        }

        /// <summary>
        /// 保存当前配置指针，新版只写 currentConfigId。
        /// </summary>
        private void SaveUseReference(string configId)
        {
            var reference = new RecordConfigReference
            {
                schemaVersion = 2,
                platform = platform,
                currentConfigId = RecorderConfigMigrator.NormalizeConfigId(configId)
            };
            File.WriteAllText(GetUseRecordConfigPath(), JsonUtility.ToJson(reference, true));
        }

        /// <summary>
        /// 读取当前配置指针并兼容旧 fileName。
        /// </summary>
        private RecordConfigReference LoadUseReference()
        {
            string path = GetUseRecordConfigPath();
            if (!File.Exists(path)) return null;
            try
            {
                string json = File.ReadAllText(path);
                var reference = JsonUtility.FromJson<RecordConfigReference>(json);
                RecorderConfigMigrator.MigrateReference(reference, json);
                return reference;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 解析当前配置 ID。
        /// </summary>
        private string ResolveCurrentConfigId(RecordConfigReference reference)
        {
            if (reference != null && !string.IsNullOrWhiteSpace(reference.currentConfigId))
            {
                string id = RecorderConfigMigrator.NormalizeConfigId(reference.currentConfigId);
                if (_configsById.ContainsKey(id)) return id;
            }

            if (reference != null && !string.IsNullOrWhiteSpace(reference.fileName))
            {
                string id = RecorderConfigMigrator.GetConfigIdFromFileName(reference.fileName);
                if (_configsById.ContainsKey(id)) return id;
            }

            string mediumId = BuildTemplateConfigId(platform, "medium");
            if (_configsById.ContainsKey(mediumId)) return mediumId;
            foreach (var config in _configs)
            {
                if (IsTemplateConfig(config)) return config.configId;
            }

            return _configs.Count > 0 ? _configs[0].configId : mediumId;
        }

        /// <summary>
        /// 确保已扫描。
        /// </summary>
        private RecorderConfigResult EnsureScanned()
        {
            return _configs.Count > 0 ? CreateResult(true, RecorderErrorCode.None, "配置已加载。", null) : Scan();
        }

        /// <summary>
        /// 创建默认配置。
        /// </summary>
        private RecorderParamsConfig CreateDefaultConfig(string configId, string displayName, bool isTemplate)
        {
            string safeId = string.IsNullOrWhiteSpace(configId) ? GenerateUniqueConfigId(BuildCustomConfigId(platform, displayName)) : RecorderConfigMigrator.NormalizeConfigId(configId);
            return new RecorderParamsConfig
            {
                schemaVersion = 2,
                configId = safeId,
                displayName = string.IsNullOrWhiteSpace(displayName) ? safeId : displayName,
                description = string.Empty,
                platform = platform,
                useMode = 0,
                videoSaveDirectory = string.Empty,
                outputFilePrefix = "Rec_",
                outputAsWebm = true,
                displayIndex = 0,
                captureDisplayName = string.Empty,
                captureFrameRate = 25,
                outputScale = 1f,
                videoCodec = "libx264",
                videoPreset = "ultrafast",
                videoCrf = 23,
                pixelFormat = "yuv420p",
                webmVideoCodec = "libvpx",
                webmVideoBitrate = "3M",
                webmDeadline = "realtime",
                webmCpuUsed = 8,
                audioMode = 1,
                audioCodec = "aac",
                webmAudioCodec = "libvorbis",
                audioBitrate = "128k",
                audioSampleRate = 48000,
                audioChannels = 2,
                enableAudioGain = false,
                audioGainDb = 0f,
                audioLimiterEnabled = true,
                streamUrl = string.Empty,
                streamVideoBitrate = "3M",
                streamGop = 50,
                streamBufferSize = "6M",
                streamLowLatency = true,
                streamAutoReconnect = true,
                streamReconnectCount = 3,
                streamReconnectIntervalMs = 3000,
                streamIncludeAudio = false,
                customFFmpegPath = string.Empty,
                stopVideoTimeoutMs = 15000,
                waitTempFileReadyTimeoutMs = 8000,
                mergeTimeoutMs = 0,
                deleteTempFilesAfterMerge = true,
                isDefault = isTemplate,
                fileName = BuildConfigFileName(safeId)
            };
        }

        /// <summary>
        /// 生成唯一配置 ID。
        /// </summary>
        private string GenerateUniqueConfigId(string rawId)
        {
            string baseId = RecorderConfigMigrator.NormalizeConfigId(rawId);
            string id = baseId;
            int index = 1;
            while (_configsById.ContainsKey(id) || ConfigFileExists(BuildConfigFileName(id)))
            {
                id = $"{baseId}_{index++}";
            }

            return id;
        }

        /// <summary>
        /// 创建操作结果并附带扫描警告。
        /// </summary>
        private RecorderConfigResult CreateResult(bool success, RecorderErrorCode errorCode, string message, RecorderParamsConfig config)
        {
            var result = success ? RecorderConfigResult.Success(config, message) : RecorderConfigResult.Failed(errorCode, message, config?.configId ?? string.Empty);
            foreach (string warning in _warnings) result.AddWarning(warning);
            return result;
        }

        /// <summary>
        /// 添加修复警告。
        /// </summary>
        private static void AddFixWarning(List<string> warnings, string message)
        {
            if (!string.IsNullOrWhiteSpace(message)) warnings.Add(message);
        }

        /// <summary>
        /// 添加扫描警告。
        /// </summary>
        private void AddWarning(string message)
        {
            if (!string.IsNullOrWhiteSpace(message)) _warnings.Add(message);
        }

        /// <summary>
        /// 获取配置文件路径。
        /// </summary>
        private string GetConfigPath(RecorderParamsConfig config)
        {
            string fileName = BuildConfigFileName(config.configId);
            config.fileName = fileName;
            return Path.Combine(GetWritableConfigDirectory(config), fileName);
        }

        /// <summary>
        /// 判断文件是否为配置文件。
        /// </summary>
        private static bool IsRecordConfigFile(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName)) return false;
            if (fileName.StartsWith("Use", StringComparison.OrdinalIgnoreCase)) return false;
            if (fileName.IndexOf("Description", StringComparison.OrdinalIgnoreCase) >= 0) return false;
            return fileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 判断配置是否为模板。
        /// </summary>
        private static bool IsTemplateConfig(RecorderParamsConfig config)
        {
            return config != null && (config.isDefault || IsTemplateFileName(config.fileName) || (!string.IsNullOrWhiteSpace(config.configId) && config.configId.StartsWith("template_", StringComparison.OrdinalIgnoreCase)));
        }

        /// <summary>
        /// 判断文件名是否为模板。
        /// </summary>
        private static bool IsTemplateFileName(string fileName)
        {
            return !string.IsNullOrWhiteSpace(fileName) && fileName.StartsWith("template_", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 根据 configId 生成文件名。
        /// </summary>
        private static string BuildConfigFileName(string configId)
        {
            string normalizedId = RecorderConfigMigrator.NormalizeConfigId(configId);
            if (normalizedId.StartsWith("template_", StringComparison.OrdinalIgnoreCase))
            {
                string[] parts = normalizedId.Split('_');
                if (parts.Length >= 3)
                {
                    string platformPart = ToTitlePart(parts[1]);
                    string presetPart = ToTitlePart(parts[2]);
                    return $"Template_{platformPart}_{presetPart}.json";
                }
            }

            if (normalizedId.StartsWith("Custom_", StringComparison.OrdinalIgnoreCase))
            {
                string[] parts = normalizedId.Split(new[] { '_' }, 3);
                if (parts.Length >= 3)
                {
                    return BuildSafeFileName($"Custom_{NormalizeCustomPlatform(parts[1])}_{parts[2]}.json");
                }
            }

            if (normalizedId.StartsWith("create_", StringComparison.OrdinalIgnoreCase))
            {
                string[] parts = normalizedId.Split(new[] { '_' }, 3);
                if (parts.Length >= 3)
                {
                    return $"create_{ToTitlePart(parts[1])}_{parts[2]}.json";
                }
            }

            return RecorderConfigMigrator.BuildConfigFileName(normalizedId);
        }

        private static string ToTitlePart(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : char.ToUpperInvariant(value[0]) + value.Substring(1).ToLowerInvariant();
        }

        private static string BuildCustomConfigId(string platform, string displayName)
        {
            return $"Custom_{NormalizeCustomPlatform(platform)}_{SanitizeCustomName(displayName)}";
        }

        private static string NormalizeCustomPlatform(string value)
        {
            return string.Equals(value, "Linux", StringComparison.OrdinalIgnoreCase) ? "Linux" : "Win";
        }

        private static string SanitizeCustomName(string value)
        {
            string name = string.IsNullOrWhiteSpace(value) ? "RecorderConfig" : value.Trim();
            if (name.EndsWith(".json", StringComparison.OrdinalIgnoreCase)) name = name.Substring(0, name.Length - ".json".Length);
            foreach (char c in Path.GetInvalidFileNameChars()) name = name.Replace(c, '_');
            foreach (char c in new[] { '\\', '/', ':', '*', '?', '"', '<', '>', '|' }) name = name.Replace(c, '_');
            name = name.Trim();
            return string.IsNullOrWhiteSpace(name) ? "RecorderConfig" : name;
        }

        private static string BuildSafeFileName(string fileName)
        {
            foreach (char c in Path.GetInvalidFileNameChars()) fileName = fileName.Replace(c, '_');
            foreach (char c in new[] { '\\', '/', ':', '*', '?', '"', '<', '>', '|' }) fileName = fileName.Replace(c, '_');
            return fileName;
        }

        /// <summary>
        /// 生成模板 ID。
        /// </summary>
        private static string BuildTemplateConfigId(string platform, string presetName)
        {
            return RecorderConfigMigrator.NormalizeConfigId($"template_{platform}_{presetName}");
        }

        /// <summary>
        /// 获取当前平台 Use 指针文件路径。
        /// </summary>
        private string GetUseRecordConfigPath()
        {
            string fileName = platform == "Linux" ? "UseLinuxRecordConfig.json" : "UseWinRecordConfig.json";
            return Path.Combine(GetUseTemplateDirectory(), fileName);
        }

        /// <summary>
        /// 获取默认配置目录。
        /// </summary>
        private static string GetDefaultConfigDirectory()
        {
            return Path.Combine(Application.streamingAssetsPath, "RecorderSDK", "Configs");
        }

        private IEnumerable<string> GetConfigScanDirectories()
        {
            yield return GetDefaultTemplateDirectory();
            yield return GetCustomTemplateDirectory();
            yield return configDirectory;
        }

        private string GetWritableConfigDirectory(RecorderParamsConfig config)
        {
            return IsTemplateConfig(config) ? GetDefaultTemplateDirectory() : GetCustomTemplateDirectory();
        }

        private string GetDefaultTemplateDirectory()
        {
            return Path.Combine(configDirectory, "DefaultTemplate");
        }

        private string GetCustomTemplateDirectory()
        {
            return Path.Combine(configDirectory, "CustomTemplate");
        }

        private string GetUseTemplateDirectory()
        {
            string directory = Path.Combine(configDirectory, "UseTemplate");
            Directory.CreateDirectory(directory);
            return directory;
        }

        private bool ConfigFileExists(string fileName)
        {
            foreach (string directory in GetConfigScanDirectories())
            {
                if (File.Exists(Path.Combine(directory, fileName))) return true;
            }

            return false;
        }

        /// <summary>
        /// 获取当前平台标签。
        /// </summary>
        public static string GetCurrentPlatformTag()
        {
            return Application.platform == RuntimePlatform.LinuxEditor || Application.platform == RuntimePlatform.LinuxPlayer ? "Linux" : "Win";
        }

        /// <summary>
        /// 兼容旧平台名。
        /// </summary>
        private static string NormalizePlatform(string value)
        {
            return string.Equals(value, "Windows", StringComparison.OrdinalIgnoreCase) ? "Win" : string.IsNullOrWhiteSpace(value) ? GetCurrentPlatformTag() : value;
        }

        /// <summary>
        /// 根据码率计算默认缓冲区大小。
        /// </summary>
        private static string GetDoubleBitrate(string bitrate)
        {
            if (string.IsNullOrWhiteSpace(bitrate)) return "6M";
            string value = bitrate.Trim();
            char suffix = value[value.Length - 1];
            string numberPart = char.IsLetter(suffix) ? value.Substring(0, value.Length - 1) : value;
            if (!float.TryParse(numberPart, out float number)) return "6M";
            return char.IsLetter(suffix) ? $"{number * 2:0.#}{suffix}" : $"{number * 2:0.#}";
        }
    }
}

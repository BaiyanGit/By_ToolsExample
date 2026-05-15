using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public partial class UIRecorderParamsSettings
{
    /// <summary>
    /// 功能：执行 EnsureConfigDirectories 相关逻辑。
    /// </summary>
    private void EnsureConfigDirectories()
    {
        Directory.CreateDirectory(GetDefaultConfigDirectory());
        Directory.CreateDirectory(GetUserConfigDirectory());
    }

    /// <summary>
    /// 功能：执行 EnsureDefaultConfigs 相关逻辑。
    /// </summary>
    private void EnsureDefaultConfigs()
    {
        var defaults = new List<RecorderParamsConfig>
        {
            CreateDefaultConfig("Windows", "低", 0, true), CreateDefaultConfig("Windows", "中", 1, true), CreateDefaultConfig("Windows", "高", 2, true),
            CreateDefaultConfig("Linux", "低", 0, true), CreateDefaultConfig("Linux", "中", 1, true), CreateDefaultConfig("Linux", "高", 2, true)
        };
        foreach (var config in defaults)
        {
            string path = Path.Combine(GetDefaultConfigDirectory(), config.fileName);
            if (!File.Exists(path)) File.WriteAllText(path, JsonUtility.ToJson(config, true));
        }
    }

    /// <summary>
    /// 功能：执行 CreateDefaultConfig 相关逻辑。
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
            configName         = presetName, platform              = platform, isDefault                         = isDefault, fileName = BuildSafeFileName($"{platform}_{presetName}.json"),
            displayIndex       = 0, outputFilePrefix               = $"{Application.productName}_", outputAsWebm = true, ffmpegExecutablePath = string.Empty,
            audioMode          = 1, audioCodec                     = "aac", webmAudioCodec                       = "libvorbis", audioBitrate = audioRate, audioSampleRate = 48000, audioChannels = 2,
            captureFrameRate   = fps, outputScale                  = scale, videoCrf                             = crf, pixelFormat = "yuv420p", videoCodec = "libx264", videoPreset = videoSpeed,
            webmVideoCodec     = "libvpx", webmVideoBitrate        = videoRate, webmDeadline                     = "realtime", webmCpuUsed = qualityIndex == 2 ? 6 : 8,
            stopVideoTimeoutMs = 15000, waitTempFileReadyTimeoutMs = 8000, mergeTimeoutMs                        = 0, deleteTempFilesAfterMerge = true
        };
    }

    /// <summary>
    /// 功能：执行 RefreshConfigDropdown 相关逻辑。
    /// </summary>
    private void RefreshConfigDropdown()
    {
        _currentPlatformConfigs.Clear();
        _currentPlatformConfigs.AddRange(LoadConfigsForCurrentPlatform());
        if (drConfig == null || _currentPlatformConfigs.Count == 0) return;
        _isRefreshingUI = true;
        drConfig.ClearOptions();
        var options = new List<string>();
        foreach (var config in _currentPlatformConfigs) options.Add(config.isDefault ? $"{config.configName}（默认）" : config.configName);
        drConfig.AddOptions(options);
        drConfig.value = Mathf.Clamp(drConfig.value, 0, _currentPlatformConfigs.Count - 1);
        drConfig.RefreshShownValue();
        _isRefreshingUI = false;
        _currentConfig  = _currentPlatformConfigs[drConfig.value].Clone();
        ApplyConfig(_currentConfig);
        if (!_hasInitUsingConfig)
        {
            UseCurrentConfig();
            _hasInitUsingConfig = true;
        }
    }

    /// <summary>
    /// 功能：执行 LoadConfigsForCurrentPlatform 相关逻辑。
    /// </summary>
    private List<RecorderParamsConfig> LoadConfigsForCurrentPlatform()
    {
        string platform = GetCurrentPlatformName();
        var    configs  = new List<RecorderParamsConfig>();
        LoadConfigsFromDirectory(GetDefaultConfigDirectory(), platform, configs);
        LoadConfigsFromDirectory(GetUserConfigDirectory(), platform, configs);
        configs.Sort((a, b) =>
        {
            if (a.isDefault != b.isDefault) return a.isDefault ? -1 : 1;
            int order = GetQualityOrder(a.configName).CompareTo(GetQualityOrder(b.configName));
            return order != 0 ? order : string.Compare(a.configName, b.configName, StringComparison.OrdinalIgnoreCase);
        });
        return configs;
    }

    /// <summary>
    /// 功能：执行 LoadConfigsFromDirectory 相关逻辑。
    /// </summary>
    private void LoadConfigsFromDirectory(string directory, string platform, List<RecorderParamsConfig> configs)
    {
        if (!Directory.Exists(directory)) return;
        foreach (string file in Directory.GetFiles(directory, "*.json"))
        {
            try
            {
                var config = JsonUtility.FromJson<RecorderParamsConfig>(File.ReadAllText(file));
                if (config != null && string.Equals(config.platform, platform, StringComparison.OrdinalIgnoreCase))
                {
                    config.fileName = Path.GetFileName(file);
                    configs.Add(config);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"读取录制配置失败: {file}\n{e.Message}");
            }
        }
    }

    /// <summary>
    /// 功能：执行 SaveConfig 相关逻辑。
    /// </summary>
    private void SaveConfig(RecorderParamsConfig config)
    {
        config.platform = GetCurrentPlatformName();
        if (string.IsNullOrWhiteSpace(config.fileName)) config.fileName = BuildUniqueUserConfigFileName(config.platform, config.configName);
        string path                                                     = Path.Combine(GetUserConfigDirectory(), config.fileName);
        config.fileName = Path.GetFileName(path);
        File.WriteAllText(path, JsonUtility.ToJson(config, true));
    }

    /// <summary>
    /// 功能：执行 SelectConfigByFileName 相关逻辑。
    /// </summary>
    private void SelectConfigByFileName(string fileName)
    {
        if (drConfig == null) return;
        for (int i = 0; i < _currentPlatformConfigs.Count; i++)
        {
            if (!string.Equals(_currentPlatformConfigs[i].fileName, fileName, StringComparison.OrdinalIgnoreCase)) continue;
            drConfig.value = i;
            drConfig.RefreshShownValue();
            _currentConfig = _currentPlatformConfigs[i].Clone();
            ApplyConfig(_currentConfig);
            return;
        }
    }

    /// <summary>
    /// 功能：执行 BuildUserConfigName 相关逻辑。
    /// </summary>
    private string BuildUserConfigName(RecorderParamsConfig config)
    {
        string prefix = config == null || string.IsNullOrWhiteSpace(config.outputFilePrefix) ? "自定义配置" : config.outputFilePrefix.Trim();
        return $"{prefix}_{DateTime.Now:yyyyMMdd_HHmmss}";
    }

    /// <summary>
    /// 功能：执行 BuildUniqueUserConfigFileName 相关逻辑。
    /// </summary>
    private string BuildUniqueUserConfigFileName(string platform, string configName)
    {
        string safeName = BuildSafeFileName($"{platform}_{configName}.json");
        string baseName = Path.GetFileNameWithoutExtension(safeName);
        string ext      = Path.GetExtension(safeName);
        string fileName = safeName;
        int    index    = 1;

        while (File.Exists(Path.Combine(GetUserConfigDirectory(), fileName))) fileName = $"{baseName}_{index++:00}{ext}";
        return fileName;
    }

    /// <summary>
    /// 功能：执行 getDefaultConfigDirectory 相关逻辑。
    /// </summary>
    private string GetDefaultConfigDirectory() => Path.Combine(Application.streamingAssetsPath, configFolderName, defaultConfigFolderName);

    /// <summary>
    /// 功能：执行 getUserConfigDirectory 相关逻辑。
    /// </summary>
    private string GetUserConfigDirectory() => Path.Combine(Application.streamingAssetsPath, configFolderName, userConfigFolderName);

    /// <summary>
    /// 功能：执行 getOptionDescriptionPath 相关逻辑。
    /// </summary>
    private string GetOptionDescriptionPath() => Path.Combine(Application.streamingAssetsPath, configFolderName, optionDescriptionJsonName);

    /// <summary>
    /// 功能：执行 BuildSafeFileName 相关逻辑。
    /// </summary>
    private static string BuildSafeFileName(string fileName)
    {
        foreach (char c in Path.GetInvalidFileNameChars()) fileName = fileName.Replace(c, '_');
        return fileName;
    }

    /// <summary>
    /// 功能：执行 getQualityOrder 相关逻辑。
    /// </summary>
    private static int GetQualityOrder(string configName)
    {
        if (configName.Contains("高")) return 0;
        if (configName.Contains("中")) return 1;
        if (configName.Contains("低")) return 2;
        return 3;
    }

    /// <summary>
    /// 功能：执行 getCurrentPlatformName 相关逻辑。
    /// </summary>
    private string GetCurrentPlatformName()
    {
        return Application.platform == RuntimePlatform.LinuxEditor || Application.platform == RuntimePlatform.LinuxPlayer ? "Linux" : "Windows";
    }
}
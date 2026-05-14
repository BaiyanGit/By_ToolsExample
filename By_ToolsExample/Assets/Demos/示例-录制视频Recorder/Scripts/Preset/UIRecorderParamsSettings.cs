using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

//=====================================================
// 文件名称: RecorderParamsSettings
// 创 建 者: wangbaiyan
// 创建日期: 2026-5-7
// 描    述: 录像参数设置脚本
//=====================================================

public class UIRecorderParamsSettings : MonoBehaviour
{
    [Header("录屏核心控制器")] public CrossPlatformScreenRecorder recorder;
    [Header("配置文件")] public Dropdown drConfig;
    [Header("显示器")] public Dropdown drDisplay;
    [Header("运行平台")] public Dropdown drPlatform;
    [Header("视频格式")] public Dropdown drVideoFormat;
    [Header("视频文件前缀")] public InputField ifVideoPrefix;
    [Header("音频采集模式")] public Dropdown drAudioMode;
    [Header("音频编码器")] public Dropdown drAudioCoder;
    [Header("音频码率")] public Dropdown drAudioBitrate;
    [Header("音频采样率")] public Dropdown drAudioSampleRate;
    [Header("音频声道数")] public Dropdown drAudioChannel;
    [Header("视频录制帧率")] public Dropdown videoCaptureFrameRate;
    [Header("视频输出缩放比例")] public Dropdown videoOutputScale;
    [Header("视频画质档位")] public Dropdown videoCrf;
    [Header("视频像素格式")] public Dropdown videoPixelFormat;
    [Header("视频编码器")] public Dropdown videoCodec;
    [Header("视频编码预设")] public Dropdown videoPreset;
    [Header("视频码率")] public Dropdown webmVideoBitrate;
    [Header("视频实时编码模式")] public Dropdown webmVideoDeadlineMode;
    [Header("视频CPU使用等级")] public Dropdown webmVideoCpuUsed;
    [Header("视频停止录制等待ffmpeg退出超时")] public Dropdown stopVideoTimeoutMs;
    [Header("视频等待临时文件释放超时")] public Dropdown waitTempFileReadyTimeoutMs;
    [Header("视频后台合并音视频等待超时")] public Dropdown mergeTimeoutMs;
    [Header("视频是否在后台合并完成后删除临时文件")] public Dropdown deleteTempFilesAfterMerge;
    [Header("FFmpeg可执行文件路径")] public InputField ifFFmpegPath;
    [Header("选择FFmpeg文件")] public Button btnSelectFFmpeg;
    [Header("重置FFmpeg路径")] public Button btnResetFFmpeg;
    [Header("FFmpeg路径提示")] public Text txtFFmpegPathTips;
    [Header("另存为")] public Button btnSaveAs;
    [Header("保存")] public Button btnSave;
    [Header("使用")] public Button btnUse;
    [Header("另存为窗口")] public GameObject saveAsWindowRoot;
    [Header("另存为名称输入")] public InputField ifSaveAsName;
    [Header("确定另存为")] public Button btnSaveAsConfirm;
    [Header("取消另存为")] public Button btnSaveAsCancel;
    [Header("选项说明JSON")] public string optionDescriptionJsonName = "RecorderOptionDescriptions.json";
    [Header("选项说明窗口")] public GameObject optionDescWindowRoot;
    [Header("选项说明标题")] public Text txtOptionDescTitle;
    [Header("选项说明内容")] public Text txtOptionDescContent;
    [Header("配置目录")] public string configFolderName = "RecorderConfigs";
    [Header("默认配置目录")] public string defaultConfigFolderName = "Default";
    [Header("用户配置目录")] public string userConfigFolderName = "User";

    private readonly List<RecorderParamsConfig> _currentPlatformConfigs = new List<RecorderParamsConfig>();
    private readonly List<Toggle> _descToggles = new List<Toggle>();
    private RecorderParamsConfig _currentConfig;
    private RecorderOptionDescriptionTable _optionDescriptionTable;
    private bool _isRefreshingUI;

    private void Start()
    {
        InitUI();
        BindEvents();
    }

    private void InitUI()
    {
        if (recorder == null) recorder = FindFirstObjectByType<CrossPlatformScreenRecorder>();
        EnsureConfigDirectories();
        EnsureDefaultConfigs();
        InitDisplayDropdown();
        InitPlatformDropdown();
        InitParameterOptions();
        RefreshConfigDropdown();
        RefreshFFmpegPathTips();
        LoadOptionDescriptions();
        EnsureUseButton();
        EnsureSaveAsWindow();
        EnsureOptionDescWindow();
        BindDescToggles();
    }

    private void BindEvents()
    {
        BindDropdown(drConfig, OnConfigChanged);
        BindDropdown(drVideoFormat, _ => OnAnyParamsChanged());
        BindDropdown(drAudioMode, _ => OnAnyParamsChanged());
        BindDropdown(drAudioCoder, _ => OnAnyParamsChanged());
        BindDropdown(drAudioBitrate, _ => OnAnyParamsChanged());
        BindDropdown(drAudioSampleRate, _ => OnAnyParamsChanged());
        BindDropdown(drAudioChannel, _ => OnAnyParamsChanged());
        BindDropdown(videoCaptureFrameRate, _ => OnAnyParamsChanged());
        BindDropdown(videoOutputScale, _ => OnAnyParamsChanged());
        BindDropdown(videoCrf, _ => OnAnyParamsChanged());
        BindDropdown(videoPixelFormat, _ => OnAnyParamsChanged());
        BindDropdown(videoCodec, _ => OnAnyParamsChanged());
        BindDropdown(videoPreset, _ => OnAnyParamsChanged());
        BindDropdown(webmVideoBitrate, _ => OnAnyParamsChanged());
        BindDropdown(webmVideoDeadlineMode, _ => OnAnyParamsChanged());
        BindDropdown(webmVideoCpuUsed, _ => OnAnyParamsChanged());
        BindDropdown(stopVideoTimeoutMs, _ => OnAnyParamsChanged());
        BindDropdown(waitTempFileReadyTimeoutMs, _ => OnAnyParamsChanged());
        BindDropdown(mergeTimeoutMs, _ => OnAnyParamsChanged());
        BindDropdown(deleteTempFilesAfterMerge, _ => OnAnyParamsChanged());
        if (ifVideoPrefix != null) { ifVideoPrefix.onValueChanged.RemoveListener(SetDefaultVideoPrefix); ifVideoPrefix.onValueChanged.AddListener(SetDefaultVideoPrefix); }
        if (ifFFmpegPath != null) { ifFFmpegPath.onValueChanged.RemoveListener(SetFFmpegPath); ifFFmpegPath.onValueChanged.AddListener(SetFFmpegPath); }
        if (btnSave != null) { btnSave.onClick.RemoveListener(SaveCurrentConfig); btnSave.onClick.AddListener(SaveCurrentConfig); }
        if (btnSaveAs != null) { btnSaveAs.onClick.RemoveListener(SaveAsConfig); btnSaveAs.onClick.AddListener(SaveAsConfig); }
        if (btnUse != null) { btnUse.onClick.RemoveListener(UseCurrentConfig); btnUse.onClick.AddListener(UseCurrentConfig); }
        if (btnSaveAsConfirm != null) { btnSaveAsConfirm.onClick.RemoveListener(ConfirmSaveAsConfig); btnSaveAsConfirm.onClick.AddListener(ConfirmSaveAsConfig); }
        if (btnSaveAsCancel != null) { btnSaveAsCancel.onClick.RemoveListener(HideSaveAsWindow); btnSaveAsCancel.onClick.AddListener(HideSaveAsWindow); }
        if (btnSelectFFmpeg != null) { btnSelectFFmpeg.onClick.RemoveListener(SelectFFmpegPath); btnSelectFFmpeg.onClick.AddListener(SelectFFmpegPath); }
        if (btnResetFFmpeg != null) { btnResetFFmpeg.onClick.RemoveListener(ResetFFmpegPath); btnResetFFmpeg.onClick.AddListener(ResetFFmpegPath); }
    }

    private void InitDisplayDropdown()
    {
        if (drDisplay == null) return;
        var options = new List<string>();
        int count = Mathf.Max(1, Display.displays.Length);
        for (int i = 0; i < count; i++) options.Add($"显示器 {i + 1}");
        SetDropdownOptions(drDisplay, options);
    }

    private void InitPlatformDropdown()
    {
        if (drPlatform == null) return;
        SetDropdownOptions(drPlatform, new List<string> { GetCurrentPlatformName() });
        drPlatform.interactable = false;
    }

    private void InitParameterOptions()
    {
        SetDropdownOptions(drVideoFormat, new List<string> { "webm", "mp4" });
        SetDropdownOptions(drAudioMode, new List<string> { "静音录制", "系统音频" });
        SetDropdownOptions(drAudioCoder, new List<string> { "aac", "libvorbis", "libopus" });
        SetDropdownOptions(drAudioBitrate, new List<string> { "96k", "128k", "192k", "256k" });
        SetDropdownOptions(drAudioSampleRate, new List<string> { "44100", "48000" });
        SetDropdownOptions(drAudioChannel, new List<string> { "1", "2" });
        SetDropdownOptions(videoCaptureFrameRate, new List<string> { "20", "25", "30", "45", "60" });
        SetDropdownOptions(videoOutputScale, new List<string> { "0.5", "0.75", "1" });
        SetDropdownOptions(videoCrf, new List<string> { "20", "23", "26", "30" });
        SetDropdownOptions(videoPixelFormat, new List<string> { "yuv420p", "yuv444p" });
        SetDropdownOptions(videoCodec, new List<string> { "libx264", "libx265", "libvpx", "libvpx-vp9" });
        SetDropdownOptions(videoPreset, new List<string> { "ultrafast", "veryfast", "faster", "fast", "medium" });
        SetDropdownOptions(webmVideoBitrate, new List<string> { "2M", "3M", "5M", "8M" });
        SetDropdownOptions(webmVideoDeadlineMode, new List<string> { "realtime", "good", "best" });
        SetDropdownOptions(webmVideoCpuUsed, new List<string> { "4", "6", "8" });
        SetDropdownOptions(stopVideoTimeoutMs, new List<string> { "5000", "8000", "15000", "30000" });
        SetDropdownOptions(waitTempFileReadyTimeoutMs, new List<string> { "5000", "8000", "15000", "30000" });
        SetDropdownOptions(mergeTimeoutMs, new List<string> { "0", "30000", "60000", "120000" });
        SetDropdownOptions(deleteTempFilesAfterMerge, new List<string> { "是", "否" });
    }
    private void OnConfigChanged(int index)
    {
        if (_isRefreshingUI || index < 0 || index >= _currentPlatformConfigs.Count) return;
        _currentConfig = _currentPlatformConfigs[index].Clone();
        ApplyConfig(_currentConfig);
    }

    private void OnAnyParamsChanged()
    {
        if (_isRefreshingUI || _currentConfig == null) return;
        _currentConfig = BuildConfigFromUI(_currentConfig);
        ApplyConfigToRecorder(_currentConfig);
    }

    private void ApplyConfig(RecorderParamsConfig config)
    {
        if (config == null) return;
        ApplyConfigToUI(config);
        ApplyConfigToRecorder(config);
        RefreshSaveButtonState();
    }

    private void ApplyConfigToUI(RecorderParamsConfig config)
    {
        _isRefreshingUI = true;
        SetDropdownValue(drVideoFormat, config.OutputAsWebM ? "webm" : "mp4");
        SetDropdownValue(drAudioMode, config.AudioMode == 1 ? "系统音频" : "静音录制");
        SetDropdownValue(drAudioCoder, config.OutputAsWebM ? config.WebmAudioCodec : config.AudioCodec);
        SetDropdownValue(drAudioBitrate, config.AudioBitrate);
        SetDropdownValue(drAudioSampleRate, config.AudioSampleRate.ToString());
        SetDropdownValue(drAudioChannel, config.AudioChannels.ToString());
        SetDropdownValue(videoCaptureFrameRate, config.CaptureFrameRate.ToString());
        SetDropdownValue(videoOutputScale, config.OutputScale.ToString("0.##"));
        SetDropdownValue(videoCrf, config.VideoCrf.ToString());
        SetDropdownValue(videoPixelFormat, config.PixelFormat);
        SetDropdownValue(videoCodec, config.OutputAsWebM ? config.WebmVideoCodec : config.VideoCodec);
        SetDropdownValue(videoPreset, config.VideoPreset);
        SetDropdownValue(webmVideoBitrate, config.WebmVideoBitrate);
        SetDropdownValue(webmVideoDeadlineMode, config.WebmDeadline);
        SetDropdownValue(webmVideoCpuUsed, config.WebmCpuUsed.ToString());
        SetDropdownValue(stopVideoTimeoutMs, config.StopVideoTimeoutMs.ToString());
        SetDropdownValue(waitTempFileReadyTimeoutMs, config.WaitTempFileReadyTimeoutMs.ToString());
        SetDropdownValue(mergeTimeoutMs, config.MergeTimeoutMs.ToString());
        SetDropdownValue(deleteTempFilesAfterMerge, config.DeleteTempFilesAfterMerge ? "是" : "否");
        if (drDisplay != null) { drDisplay.value = Mathf.Clamp(config.DisplayIndex, 0, Mathf.Max(0, drDisplay.options.Count - 1)); drDisplay.RefreshShownValue(); }
        if (ifVideoPrefix != null) ifVideoPrefix.text = config.OutputFilePrefix;
        if (ifFFmpegPath != null) ifFFmpegPath.text = config.FFmpegExecutablePath;
        _isRefreshingUI = false;
    }

    private void ApplyConfigToRecorder(RecorderParamsConfig config)
    {
        if (recorder == null || config == null) return;
        recorder.outputFilePrefix = config.OutputFilePrefix;
        recorder.customFFmpegPath = config.FFmpegExecutablePath;
        recorder.outputAsWebM = config.OutputAsWebM;
        recorder.audioMode = config.AudioMode == 1 ? RecorderAudioMode.SystemAudio : RecorderAudioMode.None;
        recorder.audioCodec = config.AudioCodec;
        recorder.webmAudioCodec = config.WebmAudioCodec;
        recorder.audioBitrate = config.AudioBitrate;
        recorder.audioSampleRate = config.AudioSampleRate;
        recorder.audioChannels = config.AudioChannels;
        recorder.captureFrameRate = config.CaptureFrameRate;
        recorder.outputScale = config.OutputScale;
        recorder.videoCrf = config.VideoCrf;
        recorder.pixelFormat = config.PixelFormat;
        recorder.videoCodec = config.VideoCodec;
        recorder.videoPreset = config.VideoPreset;
        recorder.webmVideoCodec = config.WebmVideoCodec;
        recorder.webmVideoBitrate = config.WebmVideoBitrate;
        recorder.webmDeadline = config.WebmDeadline;
        recorder.webmCpuUsed = config.WebmCpuUsed;
        recorder.stopVideoTimeoutMs = config.StopVideoTimeoutMs;
        recorder.waitTempFileReadyTimeoutMs = config.WaitTempFileReadyTimeoutMs;
        recorder.mergeTimeoutMs = config.MergeTimeoutMs;
        recorder.deleteTempFilesAfterMerge = config.DeleteTempFilesAfterMerge;
    }

    private RecorderParamsConfig BuildConfigFromUI(RecorderParamsConfig baseConfig)
    {
        var config = baseConfig != null ? baseConfig.Clone() : CreateDefaultConfig(GetCurrentPlatformName(), "自定义", 1, false);
        config.Platform = GetCurrentPlatformName();
        config.DisplayIndex = drDisplay != null ? drDisplay.value : 0;
        config.OutputFilePrefix = ifVideoPrefix != null ? ifVideoPrefix.text.Trim() : config.OutputFilePrefix;
        config.FFmpegExecutablePath = ifFFmpegPath != null ? ifFFmpegPath.text.Trim() : config.FFmpegExecutablePath;
        config.OutputAsWebM = string.Equals(GetDropdownText(drVideoFormat), "webm", StringComparison.OrdinalIgnoreCase);
        config.AudioMode = GetDropdownText(drAudioMode) == "系统音频" ? 1 : 0;
        config.AudioBitrate = GetDropdownTextOrDefault(drAudioBitrate, config.AudioBitrate);
        config.AudioSampleRate = ParseInt(GetDropdownText(drAudioSampleRate), config.AudioSampleRate);
        config.AudioChannels = ParseInt(GetDropdownText(drAudioChannel), config.AudioChannels);
        config.CaptureFrameRate = ParseInt(GetDropdownText(videoCaptureFrameRate), config.CaptureFrameRate);
        config.OutputScale = ParseFloat(GetDropdownText(videoOutputScale), config.OutputScale);
        config.VideoCrf = ParseInt(GetDropdownText(videoCrf), config.VideoCrf);
        config.PixelFormat = GetDropdownTextOrDefault(videoPixelFormat, config.PixelFormat);
        config.VideoPreset = GetDropdownTextOrDefault(videoPreset, config.VideoPreset);
        config.WebmVideoBitrate = GetDropdownTextOrDefault(webmVideoBitrate, config.WebmVideoBitrate);
        config.WebmDeadline = GetDropdownTextOrDefault(webmVideoDeadlineMode, config.WebmDeadline);
        config.WebmCpuUsed = ParseInt(GetDropdownText(webmVideoCpuUsed), config.WebmCpuUsed);
        config.StopVideoTimeoutMs = ParseInt(GetDropdownText(stopVideoTimeoutMs), config.StopVideoTimeoutMs);
        config.WaitTempFileReadyTimeoutMs = ParseInt(GetDropdownText(waitTempFileReadyTimeoutMs), config.WaitTempFileReadyTimeoutMs);
        config.MergeTimeoutMs = ParseInt(GetDropdownText(mergeTimeoutMs), config.MergeTimeoutMs);
        config.DeleteTempFilesAfterMerge = GetDropdownText(deleteTempFilesAfterMerge) != "否";
        string audioCodec = GetDropdownText(drAudioCoder);
        if (!string.IsNullOrWhiteSpace(audioCodec)) { if (config.OutputAsWebM) config.WebmAudioCodec = audioCodec; else config.AudioCodec = audioCodec; }
        string vCodec = GetDropdownText(videoCodec);
        if (!string.IsNullOrWhiteSpace(vCodec)) { if (config.OutputAsWebM) config.WebmVideoCodec = vCodec; else config.VideoCodec = vCodec; }
        return config;
    }

    private void SaveCurrentConfig()
    {
        if (_currentConfig == null) return;
        if (_currentConfig.IsDefault) { Debug.LogWarning("默认配置不允许修改，请使用另存为生成新的用户配置。"); return; }
        _currentConfig = BuildConfigFromUI(_currentConfig);
        SaveConfig(_currentConfig);
        RefreshConfigDropdown();
        SelectConfigByFileName(_currentConfig.FileName);
        Debug.Log("已保存录制配置: " + _currentConfig.ConfigName);
    }

    private void SaveAsConfig()
    {
        if (_currentConfig == null) return;
        EnsureSaveAsWindow();
        if (ifSaveAsName != null) { ifSaveAsName.text = BuildUserConfigName(BuildConfigFromUI(_currentConfig)); ifSaveAsName.ActivateInputField(); }
        if (saveAsWindowRoot != null) saveAsWindowRoot.SetActive(true);
    }

    private void UseCurrentConfig()
    {
        if (_currentConfig == null) return;
        _currentConfig = BuildConfigFromUI(_currentConfig);
        ApplyConfigToRecorder(_currentConfig);
        Debug.Log("已使用录制配置: " + _currentConfig.ConfigName);
    }

    private void ConfirmSaveAsConfig()
    {
        string configName = ifSaveAsName != null ? ifSaveAsName.text.Trim() : string.Empty;
        if (string.IsNullOrWhiteSpace(configName)) configName = BuildUserConfigName(BuildConfigFromUI(_currentConfig));
        var config = BuildConfigFromUI(_currentConfig);
        config.IsDefault = false;
        config.ConfigName = configName;
        config.FileName = BuildUniqueUserConfigFileName(config.Platform, config.ConfigName);
        SaveConfig(config);
        HideSaveAsWindow();
        RefreshConfigDropdown();
        SelectConfigByFileName(config.FileName);
        Debug.Log("已另存为录制配置: " + config.ConfigName);
    }

    private void HideSaveAsWindow()
    {
        if (saveAsWindowRoot != null) saveAsWindowRoot.SetActive(false);
    }
    private void SelectFFmpegPath()
    {
#if UNITY_EDITOR
        string extension = Application.platform == RuntimePlatform.WindowsEditor ? "exe" : string.Empty;
        string path = EditorUtility.OpenFilePanel("选择 FFmpeg 可执行文件", string.Empty, extension);
        if (!string.IsNullOrWhiteSpace(path)) { SetFFmpegPath(path); if (ifFFmpegPath != null) ifFFmpegPath.text = path; }
#else
        Debug.LogWarning("运行时请直接在 FFmpeg 路径输入框中填写可执行文件路径。");
#endif
    }

    private void SetFFmpegPath(string value)
    {
        if (_isRefreshingUI) return;
        if (recorder != null) recorder.customFFmpegPath = value.Trim();
        OnAnyParamsChanged();
        RefreshFFmpegPathTips();
    }

    private void ResetFFmpegPath()
    {
        if (ifFFmpegPath != null) ifFFmpegPath.text = string.Empty;
        if (recorder != null) recorder.customFFmpegPath = string.Empty;
        OnAnyParamsChanged();
        RefreshFFmpegPathTips();
    }

    private void SetDefaultVideoPrefix(string value)
    {
        OnAnyParamsChanged();
    }

    private void RefreshSaveButtonState()
    {
        if (btnSave != null) btnSave.interactable = _currentConfig != null && !_currentConfig.IsDefault;
        if (btnSaveAs != null) btnSaveAs.interactable = _currentConfig != null;
        if (btnUse != null) btnUse.interactable = _currentConfig != null;
    }

    private void RefreshFFmpegPathTips()
    {
        if (txtFFmpegPathTips == null || recorder == null) return;
        string currentPath = ifFFmpegPath != null ? ifFFmpegPath.text.Trim() : recorder.customFFmpegPath;
        string defaultPath = recorder.GetPlatformDefaultFFmpegPath();
        txtFFmpegPathTips.text = string.IsNullOrWhiteSpace(currentPath) ? $"当前使用平台默认 FFmpeg: {defaultPath}" : $"当前使用自定义 FFmpeg: {currentPath}";
    }

    private void EnsureConfigDirectories()
    {
        Directory.CreateDirectory(GetDefaultConfigDirectory());
        Directory.CreateDirectory(GetUserConfigDirectory());
    }

    private void EnsureDefaultConfigs()
    {
        var defaults = new List<RecorderParamsConfig>
        {
            CreateDefaultConfig("Windows", "低", 0, true), CreateDefaultConfig("Windows", "中", 1, true), CreateDefaultConfig("Windows", "高", 2, true),
            CreateDefaultConfig("Linux", "低", 0, true), CreateDefaultConfig("Linux", "中", 1, true), CreateDefaultConfig("Linux", "高", 2, true)
        };
        foreach (var config in defaults)
        {
            string path = Path.Combine(GetDefaultConfigDirectory(), config.FileName);
            if (!File.Exists(path)) File.WriteAllText(path, JsonUtility.ToJson(config, true));
        }
    }

    private RecorderParamsConfig CreateDefaultConfig(string platform, string presetName, int qualityIndex, bool isDefault)
    {
        int fps = qualityIndex == 0 ? 20 : qualityIndex == 1 ? 25 : 30;
        float scale = qualityIndex == 0 ? 0.5f : qualityIndex == 1 ? 0.75f : 1f;
        int crf = qualityIndex == 0 ? 26 : qualityIndex == 1 ? 23 : 20;
        string videoRate = qualityIndex == 0 ? "2M" : qualityIndex == 1 ? "3M" : "5M";
        string audioRate = qualityIndex == 2 ? "192k" : "128k";
        string videoSpeed = qualityIndex == 2 ? "veryfast" : "ultrafast";
        return new RecorderParamsConfig
        {
            ConfigName = presetName, Platform = platform, IsDefault = isDefault, FileName = BuildSafeFileName($"{platform}_{presetName}.json"),
            DisplayIndex = 0, OutputFilePrefix = $"{Application.productName}_", OutputAsWebM = true, FFmpegExecutablePath = string.Empty,
            AudioMode = 1, AudioCodec = "aac", WebmAudioCodec = "libvorbis", AudioBitrate = audioRate, AudioSampleRate = 48000, AudioChannels = 2,
            CaptureFrameRate = fps, OutputScale = scale, VideoCrf = crf, PixelFormat = "yuv420p", VideoCodec = "libx264", VideoPreset = videoSpeed,
            WebmVideoCodec = "libvpx", WebmVideoBitrate = videoRate, WebmDeadline = "realtime", WebmCpuUsed = qualityIndex == 2 ? 6 : 8,
            StopVideoTimeoutMs = 15000, WaitTempFileReadyTimeoutMs = 8000, MergeTimeoutMs = 0, DeleteTempFilesAfterMerge = true
        };
    }

    private void RefreshConfigDropdown()
    {
        _currentPlatformConfigs.Clear();
        _currentPlatformConfigs.AddRange(LoadConfigsForCurrentPlatform());
        if (drConfig == null || _currentPlatformConfigs.Count == 0) return;
        _isRefreshingUI = true;
        drConfig.ClearOptions();
        var options = new List<string>();
        foreach (var config in _currentPlatformConfigs) options.Add(config.IsDefault ? $"{config.ConfigName}（默认）" : config.ConfigName);
        drConfig.AddOptions(options);
        drConfig.value = Mathf.Clamp(drConfig.value, 0, _currentPlatformConfigs.Count - 1);
        drConfig.RefreshShownValue();
        _isRefreshingUI = false;
        _currentConfig = _currentPlatformConfigs[drConfig.value].Clone();
        ApplyConfig(_currentConfig);
    }

    private List<RecorderParamsConfig> LoadConfigsForCurrentPlatform()
    {
        string platform = GetCurrentPlatformName();
        var configs = new List<RecorderParamsConfig>();
        LoadConfigsFromDirectory(GetDefaultConfigDirectory(), platform, configs);
        LoadConfigsFromDirectory(GetUserConfigDirectory(), platform, configs);
        configs.Sort((a, b) =>
        {
            if (a.IsDefault != b.IsDefault) return a.IsDefault ? -1 : 1;
            int order = GetQualityOrder(a.ConfigName).CompareTo(GetQualityOrder(b.ConfigName));
            return order != 0 ? order : string.Compare(a.ConfigName, b.ConfigName, StringComparison.OrdinalIgnoreCase);
        });
        return configs;
    }

    private void LoadConfigsFromDirectory(string directory, string platform, List<RecorderParamsConfig> configs)
    {
        if (!Directory.Exists(directory)) return;
        foreach (string file in Directory.GetFiles(directory, "*.json"))
        {
            try
            {
                var config = JsonUtility.FromJson<RecorderParamsConfig>(File.ReadAllText(file));
                if (config != null && string.Equals(config.Platform, platform, StringComparison.OrdinalIgnoreCase)) { config.FileName = Path.GetFileName(file); configs.Add(config); }
            }
            catch (Exception e) { Debug.LogWarning($"读取录制配置失败: {file}\n{e.Message}"); }
        }
    }

    private void SaveConfig(RecorderParamsConfig config)
    {
        config.Platform = GetCurrentPlatformName();
        if (string.IsNullOrWhiteSpace(config.FileName)) config.FileName = BuildUniqueUserConfigFileName(config.Platform, config.ConfigName);
        string path = Path.Combine(GetUserConfigDirectory(), config.FileName);
        config.FileName = Path.GetFileName(path);
        File.WriteAllText(path, JsonUtility.ToJson(config, true));
    }

    private void SelectConfigByFileName(string fileName)
    {
        if (drConfig == null) return;
        for (int i = 0; i < _currentPlatformConfigs.Count; i++)
        {
            if (!string.Equals(_currentPlatformConfigs[i].FileName, fileName, StringComparison.OrdinalIgnoreCase)) continue;
            drConfig.value = i; drConfig.RefreshShownValue(); _currentConfig = _currentPlatformConfigs[i].Clone(); ApplyConfig(_currentConfig); return;
        }
    }

    private string BuildUserConfigName(RecorderParamsConfig config)
    {
        string prefix = config == null || string.IsNullOrWhiteSpace(config.OutputFilePrefix) ? "自定义配置" : config.OutputFilePrefix.Trim();
        return $"{prefix}_{DateTime.Now:yyyyMMdd_HHmmss}";
    }

    private string BuildUniqueUserConfigFileName(string platform, string configName)
    {
        string safeName = BuildSafeFileName($"{platform}_{configName}.json");
        string baseName = Path.GetFileNameWithoutExtension(safeName);
        string ext = Path.GetExtension(safeName);
        string fileName = safeName;
        int index = 1;
        while (File.Exists(Path.Combine(GetUserConfigDirectory(), fileName))) fileName = $"{baseName}_{index++:00}{ext}";
        return fileName;
    }

    private string GetDefaultConfigDirectory() => Path.Combine(Application.streamingAssetsPath, configFolderName, defaultConfigFolderName);
    private string GetUserConfigDirectory() => Path.Combine(Application.streamingAssetsPath, configFolderName, userConfigFolderName);
    private string GetOptionDescriptionPath() => Path.Combine(Application.streamingAssetsPath, configFolderName, optionDescriptionJsonName);
    private static string BuildSafeFileName(string fileName)
    {
        foreach (char c in Path.GetInvalidFileNameChars()) fileName = fileName.Replace(c, '_');
        return fileName;
    }

    private static int GetQualityOrder(string configName)
    {
        if (configName.Contains("高")) return 0;
        if (configName.Contains("中")) return 1;
        if (configName.Contains("低")) return 2;
        return 3;
    }

    private string GetCurrentPlatformName()
    {
        return Application.platform == RuntimePlatform.LinuxEditor || Application.platform == RuntimePlatform.LinuxPlayer ? "Linux" : "Windows";
    }

    private void BindDropdown(Dropdown dropdown, UnityEngine.Events.UnityAction<int> action)
    {
        if (dropdown == null) return;
        dropdown.onValueChanged.RemoveListener(action);
        dropdown.onValueChanged.AddListener(action);
    }

    private static void SetDropdownOptions(Dropdown dropdown, List<string> options)
    {
        if (dropdown == null) return;
        dropdown.ClearOptions();
        dropdown.AddOptions(options);
        dropdown.value = 0;
        dropdown.RefreshShownValue();
    }

    private static void SetDropdownValue(Dropdown dropdown, string value)
    {
        if (dropdown == null || string.IsNullOrWhiteSpace(value)) return;
        for (int i = 0; i < dropdown.options.Count; i++)
        {
            if (!string.Equals(dropdown.options[i].text, value, StringComparison.OrdinalIgnoreCase)) continue;
            dropdown.value = i; dropdown.RefreshShownValue(); return;
        }
    }

    private static string GetDropdownText(Dropdown dropdown)
    {
        if (dropdown == null || dropdown.options.Count == 0 || dropdown.value < 0 || dropdown.value >= dropdown.options.Count) return string.Empty;
        return dropdown.options[dropdown.value].text;
    }

    private static string GetDropdownTextOrDefault(Dropdown dropdown, string defaultValue)
    {
        string value = GetDropdownText(dropdown);
        return string.IsNullOrWhiteSpace(value) ? defaultValue : value;
    }

    private static int ParseInt(string value, int defaultValue) => int.TryParse(value, out int result) ? result : defaultValue;
    private static float ParseFloat(string value, float defaultValue) => float.TryParse(value, out float result) ? result : defaultValue;

    private void LoadOptionDescriptions()
    {
        string path = GetOptionDescriptionPath();
        if (!File.Exists(path)) { Directory.CreateDirectory(Path.GetDirectoryName(path)); File.WriteAllText(path, JsonUtility.ToJson(CreateDefaultOptionDescriptionTable(), true)); }
        try { _optionDescriptionTable = JsonUtility.FromJson<RecorderOptionDescriptionTable>(File.ReadAllText(path)); }
        catch (Exception e) { Debug.LogWarning("读取选项说明配置失败，将使用内置说明: " + e.Message); _optionDescriptionTable = CreateDefaultOptionDescriptionTable(); }
        if (_optionDescriptionTable == null || _optionDescriptionTable.Groups == null) _optionDescriptionTable = CreateDefaultOptionDescriptionTable();
    }

    private void BindDescToggles()
    {
        _descToggles.Clear();
        foreach (var toggle in GetComponentsInChildren<Toggle>(true))
        {
            if (!string.Equals(toggle.name, "TogDesc", StringComparison.OrdinalIgnoreCase)) continue;
            string settingName = toggle.transform.parent != null ? toggle.transform.parent.name : toggle.name;
            _descToggles.Add(toggle);
            toggle.onValueChanged.AddListener(isOn => OnDescToggleChanged(toggle, settingName, isOn));
        }
    }

    private void OnDescToggleChanged(Toggle toggle, string settingName, bool isOn)
    {
        if (!isOn) { if (optionDescWindowRoot != null) optionDescWindowRoot.SetActive(false); return; }
        foreach (var item in _descToggles) if (item != null && item != toggle) item.SetIsOnWithoutNotify(false);
        ShowOptionDescWindow(settingName, toggle.GetComponent<RectTransform>());
    }

    private void ShowOptionDescWindow(string settingName, RectTransform source)
    {
        EnsureOptionDescWindow();
        if (optionDescWindowRoot == null) return;
        if (txtOptionDescTitle != null) txtOptionDescTitle.text = settingName + " 选项说明";
        if (txtOptionDescContent != null) txtOptionDescContent.text = BuildOptionDescriptionText(settingName);
        optionDescWindowRoot.SetActive(true);
        PlaceDescWindowBelowSource(source);
    }

    private string BuildOptionDescriptionText(string settingName)
    {
        Dropdown dropdown = GetDropdownBySettingName(settingName);
        var group = FindOptionDescriptionGroup(GetOptionDescriptionKey(settingName), settingName);
        var lines = new List<string>();
        if (dropdown == drConfig)
        {
            foreach (var config in _currentPlatformConfigs) lines.Add($"{config.ConfigName}: {config.Platform} 平台，{(config.IsDefault ? "默认配置，不允许覆盖" : "用户配置，可保存覆盖")}");
        }
        else if (dropdown != null)
        {
            foreach (var option in dropdown.options) lines.Add($"{option.text}: {GetConfiguredOptionDescription(group, option.text)}");
        }
        else lines.Add("当前项没有下拉选项，可在说明 JSON 中补充描述。");
        return string.Join("\n", lines);
    }

    private RecorderOptionDescriptionGroup FindOptionDescriptionGroup(string key, string settingName)
    {
        if (_optionDescriptionTable == null || _optionDescriptionTable.Groups == null) return null;
        foreach (var group in _optionDescriptionTable.Groups)
        {
            if (group == null) continue;
            if (string.Equals(group.Key, key, StringComparison.OrdinalIgnoreCase) || string.Equals(group.SettingName, settingName, StringComparison.OrdinalIgnoreCase)) return group;
        }
        return null;
    }

    private static string GetConfiguredOptionDescription(RecorderOptionDescriptionGroup group, string option)
    {
        if (group != null && group.Options != null)
        {
            foreach (var item in group.Options) if (item != null && string.Equals(item.Option, option, StringComparison.OrdinalIgnoreCase)) return item.Description;
        }
        return "可在 RecorderOptionDescriptions.json 中配置该选项说明。";
    }

    private Dropdown GetDropdownBySettingName(string n)
    {
        if (n.Contains("配置文件")) return drConfig; if (n.Contains("显示器")) return drDisplay; if (n.Contains("运行平台")) return drPlatform; if (n.Contains("视频格式")) return drVideoFormat;
        if (n.Contains("音频采集模式")) return drAudioMode; if (n.Contains("音频编码器")) return drAudioCoder; if (n.Contains("音频码率")) return drAudioBitrate; if (n.Contains("音频采样率")) return drAudioSampleRate; if (n.Contains("音频声道")) return drAudioChannel;
        if (n.Contains("视频录制帧率")) return videoCaptureFrameRate; if (n.Contains("视频输出缩放")) return videoOutputScale; if (n.Contains("视频画质")) return videoCrf; if (n.Contains("视频像素格式")) return videoPixelFormat; if (n.Contains("视频编码器")) return videoCodec; if (n.Contains("视频编码预设")) return videoPreset; if (n.Contains("视频码率")) return webmVideoBitrate; if (n.Contains("实时编码")) return webmVideoDeadlineMode; if (n.Contains("CPU")) return webmVideoCpuUsed; if (n.Contains("ffmpeg退出超时")) return stopVideoTimeoutMs; if (n.Contains("临时文件释放超时")) return waitTempFileReadyTimeoutMs; if (n.Contains("后台合并")) return mergeTimeoutMs; if (n.Contains("删除临时文件")) return deleteTempFilesAfterMerge;
        return null;
    }

    private string GetOptionDescriptionKey(string n)
    {
        if (n.Contains("配置文件")) return "Config"; if (n.Contains("显示器")) return "Display"; if (n.Contains("运行平台")) return "Platform"; if (n.Contains("视频格式")) return "VideoFormat";
        if (n.Contains("音频采集模式")) return "AudioMode"; if (n.Contains("音频编码器")) return "AudioCodec"; if (n.Contains("音频码率")) return "AudioBitrate"; if (n.Contains("音频采样率")) return "AudioSampleRate"; if (n.Contains("音频声道")) return "AudioChannel";
        if (n.Contains("视频录制帧率")) return "FrameRate"; if (n.Contains("视频输出缩放")) return "OutputScale"; if (n.Contains("视频画质")) return "VideoCrf"; if (n.Contains("视频像素格式")) return "PixelFormat"; if (n.Contains("视频编码器")) return "VideoCodec"; if (n.Contains("视频编码预设")) return "VideoPreset"; if (n.Contains("视频码率")) return "VideoBitrate"; if (n.Contains("实时编码")) return "Deadline"; if (n.Contains("CPU")) return "CpuUsed"; if (n.Contains("ffmpeg退出超时")) return "StopTimeout"; if (n.Contains("临时文件释放超时")) return "TempFileTimeout"; if (n.Contains("后台合并")) return "MergeTimeout"; if (n.Contains("删除临时文件")) return "DeleteTempFiles";
        return n;
    }
    private void EnsureUseButton()
    {
        if (btnUse != null || btnSave == null) return;
        var go = Instantiate(btnSave.gameObject, btnSave.transform.parent);
        go.name = "BtnUse";
        btnUse = go.GetComponent<Button>();
        if (btnUse != null) btnUse.onClick.RemoveAllListeners();
        var rect = go.GetComponent<RectTransform>();
        var saveRect = btnSave.GetComponent<RectTransform>();
        if (rect != null && saveRect != null) rect.anchoredPosition = saveRect.anchoredPosition + new Vector2(saveRect.rect.width + 12f, 0f);
        var text = go.GetComponentInChildren<Text>(true);
        if (text != null) text.text = "使用";
    }

    private void EnsureSaveAsWindow()
    {
        if (saveAsWindowRoot != null) return;
        Transform canvas = GetCanvasTransform();
        if (canvas == null) return;
        saveAsWindowRoot = CreatePanel("SaveAsConfigWindow", canvas, new Vector2(460f, 190f), Vector2.zero);
        CreateText("TxtTitle", saveAsWindowRoot.transform, "另存为配置", new Vector2(420f, 32f), new Vector2(0f, 62f), 20, TextAnchor.MiddleCenter);
        ifSaveAsName = CreateInputField("IfSaveAsName", saveAsWindowRoot.transform, new Vector2(390f, 36f), new Vector2(0f, 18f), "配置名称");
        btnSaveAsConfirm = CreateButton("BtnConfirmSaveAs", saveAsWindowRoot.transform, "确定", new Vector2(120f, 36f), new Vector2(-72f, -54f));
        btnSaveAsCancel = CreateButton("BtnCancelSaveAs", saveAsWindowRoot.transform, "取消", new Vector2(120f, 36f), new Vector2(72f, -54f));
        saveAsWindowRoot.SetActive(false);
    }

    private void EnsureOptionDescWindow()
    {
        if (optionDescWindowRoot != null) return;
        Transform canvas = GetCanvasTransform();
        if (canvas == null) return;
        optionDescWindowRoot = CreatePanel("RecorderOptionDescWindow", canvas, new Vector2(520f, 220f), Vector2.zero);
        txtOptionDescTitle = CreateText("TxtDescTitle", optionDescWindowRoot.transform, "选项说明", new Vector2(480f, 28f), new Vector2(0f, 84f), 18, TextAnchor.MiddleLeft);
        txtOptionDescContent = CreateText("TxtDescContent", optionDescWindowRoot.transform, string.Empty, new Vector2(480f, 150f), new Vector2(0f, -8f), 15, TextAnchor.UpperLeft);
        optionDescWindowRoot.SetActive(false);
    }

    private Transform GetCanvasTransform()
    {
        var canvas = GetComponentInParent<Canvas>();
        if (canvas == null) canvas = FindFirstObjectByType<Canvas>();
        return canvas != null ? canvas.transform : null;
    }

    private GameObject CreatePanel(string name, Transform parent, Vector2 size, Vector2 anchoredPosition)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.sizeDelta = size;
        rect.anchoredPosition = anchoredPosition;
        go.GetComponent<Image>().color = new Color(0.12f, 0.14f, 0.16f, 0.96f);
        return go;
    }

    private Text CreateText(string name, Transform parent, string content, Vector2 size, Vector2 anchoredPosition, int fontSize, TextAnchor alignment)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.sizeDelta = size;
        rect.anchoredPosition = anchoredPosition;
        var text = go.GetComponent<Text>();
        text.font = GetBuiltinFont();
        text.text = content;
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = Color.white;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        return text;
    }

    private InputField CreateInputField(string name, Transform parent, Vector2 size, Vector2 anchoredPosition, string placeholder)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(InputField));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.sizeDelta = size;
        rect.anchoredPosition = anchoredPosition;
        go.GetComponent<Image>().color = Color.white;
        var text = CreateText("Text", go.transform, string.Empty, size - new Vector2(20f, 8f), Vector2.zero, 16, TextAnchor.MiddleLeft);
        text.color = Color.black;
        var hint = CreateText("Placeholder", go.transform, placeholder, size - new Vector2(20f, 8f), Vector2.zero, 16, TextAnchor.MiddleLeft);
        hint.color = new Color(0.45f, 0.45f, 0.45f, 1f);
        var input = go.GetComponent<InputField>();
        input.textComponent = text;
        input.placeholder = hint;
        return input;
    }

    private Button CreateButton(string name, Transform parent, string label, Vector2 size, Vector2 anchoredPosition)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.sizeDelta = size;
        rect.anchoredPosition = anchoredPosition;
        go.GetComponent<Image>().color = new Color(0.22f, 0.45f, 0.78f, 1f);
        CreateText("Text", go.transform, label, size, Vector2.zero, 16, TextAnchor.MiddleCenter);
        return go.GetComponent<Button>();
    }

    private static Font GetBuiltinFont()
    {
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        return font != null ? font : Resources.GetBuiltinResource<Font>("Arial.ttf");
    }

    private void PlaceDescWindowBelowSource(RectTransform source)
    {
        if (source == null || optionDescWindowRoot == null) return;
        var canvas = optionDescWindowRoot.GetComponentInParent<Canvas>();
        var canvasRect = canvas != null ? canvas.GetComponent<RectTransform>() : null;
        var windowRect = optionDescWindowRoot.GetComponent<RectTransform>();
        if (canvasRect == null || windowRect == null) return;
        var corners = new Vector3[4];
        source.GetWorldCorners(corners);
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, corners[0]);
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, canvas.worldCamera, out Vector2 localPoint)) windowRect.anchoredPosition = localPoint + new Vector2(250f, -118f);
    }

    private RecorderOptionDescriptionTable CreateDefaultOptionDescriptionTable()
    {
        return new RecorderOptionDescriptionTable { Groups = new List<RecorderOptionDescriptionGroup>
        {
            CreateDescGroup("VideoFormat", "视频格式", "webm", "适合 Unity 内播放和跨平台分发。", "mp4", "兼容性更广。"),
            CreateDescGroup("AudioMode", "音频采集模式", "静音录制", "只录制画面。", "系统音频", "录制系统输出声音。"),
            CreateDescGroup("AudioCodec", "音频编码器", "aac", "MP4 常用编码器。", "libvorbis", "WebM 默认推荐。", "libopus", "质量更好。"),
            CreateDescGroup("FrameRate", "视频录制帧率", "20", "低负载。", "25", "均衡。", "30", "默认推荐。", "45", "更流畅。", "60", "性能消耗更高。"),
            CreateDescGroup("OutputScale", "视频输出缩放比例", "0.5", "半分辨率。", "0.75", "均衡。", "1", "原始分辨率。"),
            CreateDescGroup("VideoCrf", "视频画质档位", "20", "更清晰。", "23", "均衡推荐。", "26", "文件更小。", "30", "压缩更强。"),
            CreateDescGroup("VideoCodec", "视频编码器", "libx264", "MP4 推荐。", "libx265", "压缩率更高。", "libvpx", "WebM VP8。", "libvpx-vp9", "WebM VP9。"),
            CreateDescGroup("DeleteTempFiles", "删除临时文件", "是", "合并成功后删除。", "否", "保留方便排查。")
        }};
    }

    private RecorderOptionDescriptionGroup CreateDescGroup(string key, string settingName, params string[] options)
    {
        var group = new RecorderOptionDescriptionGroup { Key = key, SettingName = settingName, Options = new List<RecorderOptionDescriptionItem>() };
        for (int i = 0; i + 1 < options.Length; i += 2)
        {
            group.Options.Add(new RecorderOptionDescriptionItem { Option = options[i], Description = options[i + 1] });
        }
        return group;
    }
}

[Serializable]
public class RecorderParamsConfig
{
    [Header("配置名称")] public string ConfigName;
    [Header("运行平台")] public string Platform;
    [Header("是否默认配置")] public bool IsDefault;
    [Header("文件名")] public string FileName;
    [Header("显示器")] public int DisplayIndex;
    [Header("输出文件前缀")] public string OutputFilePrefix;
    [Header("输出WebM")] public bool OutputAsWebM;
    [Header("FFmpeg可执行文件路径")] public string FFmpegExecutablePath;
    [Header("音频采集模式")] public int AudioMode;
    [Header("音频编码器")] public string AudioCodec;
    [Header("WebM音频编码器")] public string WebmAudioCodec;
    [Header("音频码率")] public string AudioBitrate;
    [Header("音频采样率")] public int AudioSampleRate;
    [Header("音频声道数")] public int AudioChannels;
    [Header("视频录制帧率")] public int CaptureFrameRate;
    [Header("视频输出缩放比例")] public float OutputScale;
    [Header("视频画质档位")] public int VideoCrf;
    [Header("视频像素格式")] public string PixelFormat;
    [Header("视频编码器")] public string VideoCodec;
    [Header("视频编码预设")] public string VideoPreset;
    [Header("WebM视频编码器")] public string WebmVideoCodec;
    [Header("视频码率")] public string WebmVideoBitrate;
    [Header("视频实时编码模式")] public string WebmDeadline;
    [Header("视频CPU使用等级")] public int WebmCpuUsed;
    [Header("停止录制等待ffmpeg退出超时")] public int StopVideoTimeoutMs;
    [Header("等待临时文件释放超时")] public int WaitTempFileReadyTimeoutMs;
    [Header("后台合并音视频等待超时")] public int MergeTimeoutMs;
    [Header("后台合并完成后删除临时文件")] public bool DeleteTempFilesAfterMerge;

    public RecorderParamsConfig Clone()
    {
        return JsonUtility.FromJson<RecorderParamsConfig>(JsonUtility.ToJson(this));
    }
}

[Serializable]
public class RecorderOptionDescriptionTable
{
    [Header("选项说明组")] public List<RecorderOptionDescriptionGroup> Groups = new List<RecorderOptionDescriptionGroup>();
}

[Serializable]
public class RecorderOptionDescriptionGroup
{
    [Header("配置键")] public string Key;
    [Header("参数名称")] public string SettingName;
    [Header("选项说明")] public List<RecorderOptionDescriptionItem> Options = new List<RecorderOptionDescriptionItem>();
}

[Serializable]
public class RecorderOptionDescriptionItem
{
    [Header("选项")] public string Option;
    [Header("说明")] public string Description;
}

using System;
using System.Collections;
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

public partial class UIRecorderParamsSettings : MonoBehaviour
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
    [Header("FFmpeg可执行文件路径")] public InputField ifFfmpegPath;
    [Header("选择FFmpeg文件")] public Button btnSelectFfmpeg;
    [Header("重置FFmpeg路径")] public Button btnResetFfmpeg;
    [Header("FFmpeg路径提示")] public Text txtFfmpegPathTips;
    [Header("另存为")] public Button btnSaveAs;
    [Header("保存")] public Button btnSave;
    [Header("使用")] public Button btnUse;
    [Header("另存为窗口")] public GameObject saveAsWindowRoot;
    [Header("另存为名称输入")] public InputField ifSaveAsName;
    [Header("确定另存为")] public Button btnSaveAsConfirm;
    [Header("取消另存为")] public Button btnSaveAsCancel;
    [Header("另存为提示文字")] public Text txtSaveAsTips;
    [Header("修改标记颜色")] public Color changedColor = new(1f, 0.72f, 0.18f, 1f);
    [Header("说明框最小尺寸")] public Vector2 descWindowMinSize = new(420f, 150f);
    [Header("说明框最大尺寸")] public Vector2 descWindowMaxSize = new(640f, 360f);
    [Header("选项说明JSON")] public string optionDescriptionJsonName = "RecorderOptionDescriptions.json";
    [Header("选项说明窗口")] public GameObject optionDescWindowRoot;
    [Header("选项说明标题")] public Text txtOptionDescTitle;
    [Header("选项说明内容")] public Text txtOptionDescContent;
    [Header("配置目录")] public string configFolderName = "RecorderConfigs";
    [Header("默认配置目录")] public string defaultConfigFolderName = "Default";
    [Header("用户配置目录")] public string userConfigFolderName = "User";

    private readonly List<RecorderParamsConfig> _currentPlatformConfigs = new();
    private readonly List<Button> _descButtons = new();
    private readonly Dictionary<Graphic, Color> _originGraphicColors = new();
    private readonly Dictionary<Selectable, bool> _originSelectableStates = new();
    private RecorderParamsConfig _currentConfig;
    private RecorderOptionDescriptionTable _optionDescriptionTable;
    private string _loadedConfigJson;
    private string _usingConfigFileName;
    private string _usingConfigJson;
    private Coroutine _saveAsTipsCoroutine;
    private bool _hasInitUsingConfig;
    private bool _isRefreshingUI;

    /// <summary>
    /// 功能：执行 Start 相关逻辑。
    /// </summary>
    private void Start()
    {
        InitUI();
        BindEvents();
    }

    /// <summary>
    /// 功能：执行 InitUI 相关逻辑。
    /// </summary>
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
        BindDescButtons();
    }

    /// <summary>
    /// 功能：执行 BindEvents 相关逻辑。
    /// </summary>
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
        if (ifVideoPrefix != null)
        {
            ifVideoPrefix.onValueChanged.RemoveListener(SetDefaultVideoPrefix);
            ifVideoPrefix.onValueChanged.AddListener(SetDefaultVideoPrefix);
        }

        if (ifFfmpegPath != null)
        {
            ifFfmpegPath.onValueChanged.RemoveListener(SetFFmpegPath);
            ifFfmpegPath.onValueChanged.AddListener(SetFFmpegPath);
        }

        if (btnSave != null)
        {
            btnSave.onClick.RemoveListener(SaveCurrentConfig);
            btnSave.onClick.AddListener(SaveCurrentConfig);
        }

        if (btnSaveAs != null)
        {
            btnSaveAs.onClick.RemoveListener(SaveAsConfig);
            btnSaveAs.onClick.AddListener(SaveAsConfig);
        }

        if (btnUse != null)
        {
            btnUse.onClick.RemoveListener(UseCurrentConfig);
            btnUse.onClick.AddListener(UseCurrentConfig);
        }

        if (btnSaveAsConfirm != null)
        {
            btnSaveAsConfirm.onClick.RemoveListener(ConfirmSaveAsConfig);
            btnSaveAsConfirm.onClick.AddListener(ConfirmSaveAsConfig);
        }

        if (btnSaveAsCancel != null)
        {
            btnSaveAsCancel.onClick.RemoveListener(HideSaveAsWindow);
            btnSaveAsCancel.onClick.AddListener(HideSaveAsWindow);
        }

        if (btnSelectFfmpeg != null)
        {
            btnSelectFfmpeg.onClick.RemoveListener(SelectFFmpegPath);
            btnSelectFfmpeg.onClick.AddListener(SelectFFmpegPath);
        }

        if (btnResetFfmpeg != null)
        {
            btnResetFfmpeg.onClick.RemoveListener(ResetFFmpegPath);
            btnResetFfmpeg.onClick.AddListener(ResetFFmpegPath);
        }
    }

    /// <summary>
    /// 功能：执行 InitDisplayDropdown 相关逻辑。
    /// </summary>
    private void InitDisplayDropdown()
    {
        if (drDisplay == null) return;
        var options = new List<string>();
        int count   = Mathf.Max(1, Display.displays.Length);
        for (int i = 0; i < count; i++) options.Add($"显示器 {i + 1}");
        SetDropdownOptions(drDisplay, options);
    }

    /// <summary>
    /// 功能：执行 InitPlatformDropdown 相关逻辑。
    /// </summary>
    private void InitPlatformDropdown()
    {
        if (drPlatform == null) return;
        SetDropdownOptions(drPlatform, new List<string> { GetCurrentPlatformName() });
        drPlatform.interactable = false;
    }

    /// <summary>
    /// 功能：执行 InitParameterOptions 相关逻辑。
    /// </summary>
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

    /// <summary>
    /// 功能：执行 OnConfigChanged 相关逻辑。
    /// </summary>
    private void OnConfigChanged(int index)
    {
        if (_isRefreshingUI || index < 0 || index >= _currentPlatformConfigs.Count) return;
        _currentConfig = _currentPlatformConfigs[index].Clone();
        ApplyConfig(_currentConfig);
    }

    /// <summary>
    /// 功能：执行 OnAnyParamsChanged 相关逻辑。
    /// </summary>
    private void OnAnyParamsChanged()
    {
        if (_isRefreshingUI || _currentConfig == null) return;
        _currentConfig = BuildConfigFromUI(_currentConfig);
        RefreshEditState();
    }

    /// <summary>
    /// 功能：执行 ApplyConfig 相关逻辑。
    /// </summary>
    private void ApplyConfig(RecorderParamsConfig config)
    {
        if (config == null) return;
        ApplyConfigToUI(config);
        _loadedConfigJson = JsonUtility.ToJson(BuildConfigFromUI(config));
        RefreshEditState();
        RefreshParameterInteractable();
    }

    /// <summary>
    /// 功能：执行 ApplyConfigToUI 相关逻辑。
    /// </summary>
    private void ApplyConfigToUI(RecorderParamsConfig config)
    {
        _isRefreshingUI = true;
        SetDropdownValue(drVideoFormat, config.outputAsWebm ? "webm" : "mp4");
        SetDropdownValue(drAudioMode, config.audioMode == 1 ? "系统音频" : "静音录制");
        SetDropdownValue(drAudioCoder, config.outputAsWebm ? config.webmAudioCodec : config.audioCodec);
        SetDropdownValue(drAudioBitrate, config.audioBitrate);
        SetDropdownValue(drAudioSampleRate, config.audioSampleRate.ToString());
        SetDropdownValue(drAudioChannel, config.audioChannels.ToString());
        SetDropdownValue(videoCaptureFrameRate, config.captureFrameRate.ToString());
        SetDropdownValue(videoOutputScale, config.outputScale.ToString("0.##"));
        SetDropdownValue(videoCrf, config.videoCrf.ToString());
        SetDropdownValue(videoPixelFormat, config.pixelFormat);
        SetDropdownValue(videoCodec, config.outputAsWebm ? config.webmVideoCodec : config.videoCodec);
        SetDropdownValue(videoPreset, config.videoPreset);
        SetDropdownValue(webmVideoBitrate, config.webmVideoBitrate);
        SetDropdownValue(webmVideoDeadlineMode, config.webmDeadline);
        SetDropdownValue(webmVideoCpuUsed, config.webmCpuUsed.ToString());
        SetDropdownValue(stopVideoTimeoutMs, config.stopVideoTimeoutMs.ToString());
        SetDropdownValue(waitTempFileReadyTimeoutMs, config.waitTempFileReadyTimeoutMs.ToString());
        SetDropdownValue(mergeTimeoutMs, config.mergeTimeoutMs.ToString());
        SetDropdownValue(deleteTempFilesAfterMerge, config.deleteTempFilesAfterMerge ? "是" : "否");
        if (drDisplay != null)
        {
            drDisplay.value = Mathf.Clamp(config.displayIndex, 0, Mathf.Max(0, drDisplay.options.Count - 1));
            drDisplay.RefreshShownValue();
        }

        if (ifVideoPrefix != null) ifVideoPrefix.text = config.outputFilePrefix;
        if (ifFfmpegPath != null) ifFfmpegPath.text   = config.ffmpegExecutablePath;
        _isRefreshingUI = false;
    }

    /// <summary>
    /// 功能：执行 ApplyConfigToRecorder 相关逻辑。
    /// </summary>
    private void ApplyConfigToRecorder(RecorderParamsConfig config)
    {
        if (recorder == null || config == null) return;
        recorder.outputFilePrefix           = config.outputFilePrefix;
        recorder.customFFmpegPath           = config.ffmpegExecutablePath;
        recorder.outputAsWebM               = config.outputAsWebm;
        recorder.audioMode                  = config.audioMode == 1 ? RecorderAudioMode.SystemAudio : RecorderAudioMode.None;
        recorder.audioCodec                 = config.audioCodec;
        recorder.webmAudioCodec             = config.webmAudioCodec;
        recorder.audioBitrate               = config.audioBitrate;
        recorder.audioSampleRate            = config.audioSampleRate;
        recorder.audioChannels              = config.audioChannels;
        recorder.captureFrameRate           = config.captureFrameRate;
        recorder.outputScale                = config.outputScale;
        recorder.videoCrf                   = config.videoCrf;
        recorder.pixelFormat                = config.pixelFormat;
        recorder.videoCodec                 = config.videoCodec;
        recorder.videoPreset                = config.videoPreset;
        recorder.webmVideoCodec             = config.webmVideoCodec;
        recorder.webmVideoBitrate           = config.webmVideoBitrate;
        recorder.webmDeadline               = config.webmDeadline;
        recorder.webmCpuUsed                = config.webmCpuUsed;
        recorder.stopVideoTimeoutMs         = config.stopVideoTimeoutMs;
        recorder.waitTempFileReadyTimeoutMs = config.waitTempFileReadyTimeoutMs;
        recorder.mergeTimeoutMs             = config.mergeTimeoutMs;
        recorder.deleteTempFilesAfterMerge  = config.deleteTempFilesAfterMerge;
    }

    /// <summary>
    /// 功能：执行 BuildConfigFromUI 相关逻辑。
    /// </summary>
    private RecorderParamsConfig BuildConfigFromUI(RecorderParamsConfig baseConfig)
    {
        var config = baseConfig != null ? baseConfig.Clone() : CreateDefaultConfig(GetCurrentPlatformName(), "自定义", 1, false);
        config.platform                   = GetCurrentPlatformName();
        config.displayIndex               = drDisplay != null ? drDisplay.value : 0;
        config.outputFilePrefix           = ifVideoPrefix != null ? ifVideoPrefix.text.Trim() : config.outputFilePrefix;
        config.ffmpegExecutablePath       = ifFfmpegPath != null ? ifFfmpegPath.text.Trim() : config.ffmpegExecutablePath;
        config.outputAsWebm               = string.Equals(getDropdownText(drVideoFormat), "webm", StringComparison.OrdinalIgnoreCase);
        config.audioMode                  = getDropdownText(drAudioMode) == "系统音频" ? 1 : 0;
        config.audioBitrate               = getDropdownTextOrDefault(drAudioBitrate, config.audioBitrate);
        config.audioSampleRate            = ParseInt(getDropdownText(drAudioSampleRate), config.audioSampleRate);
        config.audioChannels              = ParseInt(getDropdownText(drAudioChannel), config.audioChannels);
        config.captureFrameRate           = ParseInt(getDropdownText(videoCaptureFrameRate), config.captureFrameRate);
        config.outputScale                = ParseFloat(getDropdownText(videoOutputScale), config.outputScale);
        config.videoCrf                   = ParseInt(getDropdownText(videoCrf), config.videoCrf);
        config.pixelFormat                = getDropdownTextOrDefault(videoPixelFormat, config.pixelFormat);
        config.videoPreset                = getDropdownTextOrDefault(videoPreset, config.videoPreset);
        config.webmVideoBitrate           = getDropdownTextOrDefault(webmVideoBitrate, config.webmVideoBitrate);
        config.webmDeadline               = getDropdownTextOrDefault(webmVideoDeadlineMode, config.webmDeadline);
        config.webmCpuUsed                = ParseInt(getDropdownText(webmVideoCpuUsed), config.webmCpuUsed);
        config.stopVideoTimeoutMs         = ParseInt(getDropdownText(stopVideoTimeoutMs), config.stopVideoTimeoutMs);
        config.waitTempFileReadyTimeoutMs = ParseInt(getDropdownText(waitTempFileReadyTimeoutMs), config.waitTempFileReadyTimeoutMs);
        config.mergeTimeoutMs             = ParseInt(getDropdownText(mergeTimeoutMs), config.mergeTimeoutMs);
        config.deleteTempFilesAfterMerge  = getDropdownText(deleteTempFilesAfterMerge) != "否";
        string audioCodec = getDropdownText(drAudioCoder);
        if (!string.IsNullOrWhiteSpace(audioCodec))
        {
            if (config.outputAsWebm) config.webmAudioCodec = audioCodec;
            else config.audioCodec                         = audioCodec;
        }

        string vCodec = getDropdownText(videoCodec);
        if (!string.IsNullOrWhiteSpace(vCodec))
        {
            if (config.outputAsWebm) config.webmVideoCodec = vCodec;
            else config.videoCodec                         = vCodec;
        }

        return config;
    }

    /// <summary>
    /// 功能：执行 SaveCurrentConfig 相关逻辑。
    /// </summary>
    private void SaveCurrentConfig()
    {
        if (_currentConfig == null) return;
        if (_currentConfig.isDefault)
        {
            Debug.LogWarning("默认配置不允许修改，请使用另存为生成新的用户配置。");
            return;
        }

        _currentConfig = BuildConfigFromUI(_currentConfig);
        SaveConfig(_currentConfig);
        string savedFileName = _currentConfig.fileName;
        if (string.Equals(savedFileName, _usingConfigFileName, StringComparison.OrdinalIgnoreCase))
        {
            ApplyConfigToRecorder(_currentConfig);
            _usingConfigJson = JsonUtility.ToJson(_currentConfig);
        }

        _loadedConfigJson = JsonUtility.ToJson(_currentConfig);
        RefreshConfigDropdown();
        SelectConfigByFileName(savedFileName);
        RefreshEditState();
        Debug.Log("已保存录制配置: " + _currentConfig.configName);
    }

    /// <summary>
    /// 功能：执行 SaveAsConfig 相关逻辑。
    /// </summary>
    private void SaveAsConfig()
    {
        if (_currentConfig == null) return;
        EnsureSaveAsWindow();
        if (ifSaveAsName != null)
        {
            ifSaveAsName.text = Path.GetFileNameWithoutExtension(_currentConfig.fileName);
            ifSaveAsName.ActivateInputField();
        }

        HideSaveAsTips();
        if (saveAsWindowRoot != null) saveAsWindowRoot.SetActive(true);
    }

    /// <summary>
    /// 功能：执行 UseCurrentConfig 相关逻辑。
    /// </summary>
    private void UseCurrentConfig()
    {
        if (_currentConfig == null) return;
        _currentConfig = BuildConfigFromUI(_currentConfig);
        ApplyConfigToRecorder(_currentConfig);
        _usingConfigFileName = _currentConfig.fileName;
        _usingConfigJson     = JsonUtility.ToJson(_currentConfig);
        RefreshEditState();
        Debug.Log("已使用录制配置: " + _currentConfig.configName);
    }

    /// <summary>
    /// 功能：执行 ConfirmSaveAsConfig 相关逻辑。
    /// </summary>
    private void ConfirmSaveAsConfig()
    {
        string configName                                     = ifSaveAsName != null ? ifSaveAsName.text.Trim() : string.Empty;
        if (string.IsNullOrWhiteSpace(configName)) configName = _currentConfig != null ? _currentConfig.fileName : "RecorderConfig.json";
        string fileName                                       = BuildSafeFileName(configName.EndsWith(".json", StringComparison.OrdinalIgnoreCase) ? configName : configName + ".json");
        if (HasConfigFileName(fileName))
        {
            ShowSaveAsTips("本地已存在同名配置文件，请修改名称后再保存。");
            return;
        }

        var config = BuildConfigFromUI(_currentConfig);
        config.isDefault  = false;
        config.fileName   = fileName;
        config.configName = Path.GetFileNameWithoutExtension(fileName);
        SaveConfig(config);
        ApplyConfigToRecorder(config);
        _usingConfigFileName = config.fileName;
        _usingConfigJson     = JsonUtility.ToJson(config);
        _loadedConfigJson    = JsonUtility.ToJson(config);
        HideSaveAsWindow();
        RefreshConfigDropdown();
        SelectConfigByFileName(config.fileName);
        RefreshEditState();
        Debug.Log("已另存为录制配置: " + config.configName);
    }

    /// <summary>
    /// 功能：执行 HideSaveAsWindow 相关逻辑。
    /// </summary>
    private void HideSaveAsWindow()
    {
        if (saveAsWindowRoot != null) saveAsWindowRoot.SetActive(false);
    }

    /// <summary>
    /// 功能：执行 HasConfigFileName 相关逻辑。
    /// </summary>
    private bool HasConfigFileName(string fileName)
    {
        return HasConfigFileNameInDirectory(GetDefaultConfigDirectory(), fileName) || HasConfigFileNameInDirectory(GetUserConfigDirectory(), fileName);
    }

    /// <summary>
    /// 功能：执行 HasConfigFileNameInDirectory 相关逻辑。
    /// </summary>
    private bool HasConfigFileNameInDirectory(string directory, string fileName)
    {
        if (!Directory.Exists(directory)) return false;
        foreach (string file in Directory.GetFiles(directory, "*.json"))
        {
            if (string.Equals(Path.GetFileName(file), fileName, StringComparison.OrdinalIgnoreCase)) return true;
        }

        return false;
    }

    /// <summary>
    /// 功能：执行 ShowSaveAsTips 相关逻辑。
    /// </summary>
    private void ShowSaveAsTips(string tips)
    {
        EnsureSaveAsWindow();
        if (txtSaveAsTips == null) return;
        txtSaveAsTips.text = tips;
        txtSaveAsTips.gameObject.SetActive(true);
        if (_saveAsTipsCoroutine != null) StopCoroutine(_saveAsTipsCoroutine);
        _saveAsTipsCoroutine = StartCoroutine(HideSaveAsTipsDelay());
    }

    /// <summary>
    /// 功能：执行 HideSaveAsTipsDelay 相关逻辑。
    /// </summary>
    private IEnumerator HideSaveAsTipsDelay()
    {
        yield return new WaitForSeconds(5f);
        if (txtSaveAsTips != null)
        {
            txtSaveAsTips.text = string.Empty;
            txtSaveAsTips.gameObject.SetActive(false);
        }

        _saveAsTipsCoroutine = null;
    }

    /// <summary>
    /// 功能：执行 HideSaveAsTips 相关逻辑。
    /// </summary>
    private void HideSaveAsTips()
    {
        if (_saveAsTipsCoroutine != null)
        {
            StopCoroutine(_saveAsTipsCoroutine);
            _saveAsTipsCoroutine = null;
        }

        if (txtSaveAsTips != null)
        {
            txtSaveAsTips.text = string.Empty;
            txtSaveAsTips.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 功能：执行 SelectFFmpegPath 相关逻辑。
    /// </summary>
    private void SelectFFmpegPath()
    {
#if UNITY_EDITOR
        string extension = Application.platform == RuntimePlatform.WindowsEditor ? "exe" : string.Empty;
        string path      = EditorUtility.OpenFilePanel("选择 FFmpeg 可执行文件", string.Empty, extension);
        if (!string.IsNullOrWhiteSpace(path))
        {
            SetFFmpegPath(path);
            if (ifFfmpegPath != null) ifFfmpegPath.text = path;
        }
#else
        Debug.LogWarning("运行时请直接在 FFmpeg 路径输入框中填写可执行文件路径。");
#endif
    }

    /// <summary>
    /// 功能：执行 SetFFmpegPath 相关逻辑。
    /// </summary>
    private void SetFFmpegPath(string value)
    {
        if (_isRefreshingUI) return;
        if (recorder != null) recorder.customFFmpegPath = value.Trim();
        OnAnyParamsChanged();
        RefreshFFmpegPathTips();
    }

    /// <summary>
    /// 功能：执行 ResetFFmpegPath 相关逻辑。
    /// </summary>
    private void ResetFFmpegPath()
    {
        if (ifFfmpegPath != null) ifFfmpegPath.text     = string.Empty;
        if (recorder != null) recorder.customFFmpegPath = string.Empty;
        OnAnyParamsChanged();
        RefreshFFmpegPathTips();
    }

    /// <summary>
    /// 功能：执行 SetDefaultVideoPrefix 相关逻辑。
    /// </summary>
    private void SetDefaultVideoPrefix(string value)
    {
        OnAnyParamsChanged();
    }

    /// <summary>
    /// 功能：执行 RefreshSaveButtonState 相关逻辑。
    /// </summary>
    private void RefreshSaveButtonState()
    {
        bool hasConfig      = _currentConfig != null;
        bool isDirty        = IsCurrentConfigDirty();
        bool isDefaultDirty = hasConfig && _currentConfig.isDefault && isDirty;
        if (btnSave != null)
        {
            bool showSave = hasConfig && !_currentConfig.isDefault && isDirty;
            btnSave.gameObject.SetActive(showSave);
            btnSave.interactable = showSave;
            var text                    = btnSave.GetComponentInChildren<Text>(true);
            if (text != null) text.text = "保存";
        }

        if (btnSaveAs != null)
        {
            btnSaveAs.gameObject.SetActive(hasConfig);
            btnSaveAs.interactable = hasConfig;
        }

        RefreshUseButtonState(isDefaultDirty);
    }

    /// <summary>
    /// 功能：执行 RefreshEditState 相关逻辑。
    /// </summary>
    private void RefreshEditState()
    {
        RefreshSaveButtonState();
        RefreshModifiedControlColors();
    }

    /// <summary>
    /// 功能：执行 IsCurrentConfigDirty 相关逻辑。
    /// </summary>
    private bool IsCurrentConfigDirty()
    {
        if (_currentConfig == null || string.IsNullOrEmpty(_loadedConfigJson)) return false;
        return JsonUtility.ToJson(BuildConfigFromUI(_currentConfig)) != _loadedConfigJson;
    }

    /// <summary>
    /// 功能：执行 RefreshUseButtonState 相关逻辑。
    /// </summary>
    private void RefreshUseButtonState(bool forceHide)
    {
        if (btnUse == null) return;
        bool isUsing = _currentConfig != null
                       && string.Equals(_currentConfig.fileName, _usingConfigFileName, StringComparison.OrdinalIgnoreCase)
                       && JsonUtility.ToJson(BuildConfigFromUI(_currentConfig)) == _usingConfigJson;
        bool showUse = _currentConfig != null && !forceHide;
        btnUse.gameObject.SetActive(showUse);
        btnUse.interactable = showUse && !isUsing;

        var text                    = btnUse.GetComponentInChildren<Text>(true);
        if (text != null) text.text = isUsing ? "正在使用" : "使用";
    }

    /// <summary>
    /// 功能：执行 RefreshModifiedControlColors 相关逻辑。
    /// </summary>
    private void RefreshModifiedControlColors()
    {
        if (_currentConfig == null || string.IsNullOrEmpty(_loadedConfigJson))
        {
            ClearModifiedControlColors();
            return;
        }

        var baseConfig = JsonUtility.FromJson<RecorderParamsConfig>(_loadedConfigJson);
        var current    = BuildConfigFromUI(_currentConfig);
        if (baseConfig == null || current == null) return;

        SetControlModified(drDisplay, current.displayIndex != baseConfig.displayIndex);
        SetControlModified(drVideoFormat, current.outputAsWebm != baseConfig.outputAsWebm);
        SetControlModified(ifVideoPrefix, current.outputFilePrefix != baseConfig.outputFilePrefix);
        SetControlModified(ifFfmpegPath, current.ffmpegExecutablePath != baseConfig.ffmpegExecutablePath);
        SetControlModified(drAudioMode, current.audioMode != baseConfig.audioMode);
        SetControlModified(drAudioCoder, current.audioCodec != baseConfig.audioCodec || current.webmAudioCodec != baseConfig.webmAudioCodec);
        SetControlModified(drAudioBitrate, current.audioBitrate != baseConfig.audioBitrate);
        SetControlModified(drAudioSampleRate, current.audioSampleRate != baseConfig.audioSampleRate);
        SetControlModified(drAudioChannel, current.audioChannels != baseConfig.audioChannels);
        SetControlModified(videoCaptureFrameRate, current.captureFrameRate != baseConfig.captureFrameRate);
        SetControlModified(videoOutputScale, !Mathf.Approximately(current.outputScale, baseConfig.outputScale));
        SetControlModified(videoCrf, current.videoCrf != baseConfig.videoCrf);
        SetControlModified(videoPixelFormat, current.pixelFormat != baseConfig.pixelFormat);
        SetControlModified(videoCodec, current.videoCodec != baseConfig.videoCodec || current.webmVideoCodec != baseConfig.webmVideoCodec);
        SetControlModified(videoPreset, current.videoPreset != baseConfig.videoPreset);
        SetControlModified(webmVideoBitrate, current.webmVideoBitrate != baseConfig.webmVideoBitrate);
        SetControlModified(webmVideoDeadlineMode, current.webmDeadline != baseConfig.webmDeadline);
        SetControlModified(webmVideoCpuUsed, current.webmCpuUsed != baseConfig.webmCpuUsed);
        SetControlModified(stopVideoTimeoutMs, current.stopVideoTimeoutMs != baseConfig.stopVideoTimeoutMs);
        SetControlModified(waitTempFileReadyTimeoutMs, current.waitTempFileReadyTimeoutMs != baseConfig.waitTempFileReadyTimeoutMs);
        SetControlModified(mergeTimeoutMs, current.mergeTimeoutMs != baseConfig.mergeTimeoutMs);
        SetControlModified(deleteTempFilesAfterMerge, current.deleteTempFilesAfterMerge != baseConfig.deleteTempFilesAfterMerge);
    }

    /// <summary>
    /// 功能：执行 ClearModifiedControlColors 相关逻辑。
    /// </summary>
    private void ClearModifiedControlColors()
    {
        foreach (var item in _originGraphicColors)
        {
            if (item.Key != null) item.Key.color = item.Value;
        }
    }

    /// <summary>
    /// 功能：执行 SetControlModified 相关逻辑。
    /// </summary>
    private void SetControlModified(Selectable selectable, bool isModified)
    {
        if (selectable == null || selectable.targetGraphic == null) return;
        var graphic = selectable.targetGraphic;
        if (!_originGraphicColors.ContainsKey(graphic)) _originGraphicColors.Add(graphic, graphic.color);
        graphic.color = isModified ? changedColor : _originGraphicColors[graphic];
    }

    /// <summary>
    /// 功能：执行 RefreshParameterInteractable 相关逻辑。
    /// </summary>
    private void RefreshParameterInteractable()
    {
        bool canEdit = _currentConfig != null;
        SetParameterInteractable(drDisplay, canEdit);
        SetParameterInteractable(drVideoFormat, canEdit);
        SetParameterInteractable(ifVideoPrefix, canEdit);
        SetParameterInteractable(ifFfmpegPath, canEdit);
        SetParameterInteractable(drAudioMode, canEdit);
        SetParameterInteractable(drAudioCoder, canEdit);
        SetParameterInteractable(drAudioBitrate, canEdit);
        SetParameterInteractable(drAudioSampleRate, canEdit);
        SetParameterInteractable(drAudioChannel, canEdit);
        SetParameterInteractable(videoCaptureFrameRate, canEdit);
        SetParameterInteractable(videoOutputScale, canEdit);
        SetParameterInteractable(videoCrf, canEdit);
        SetParameterInteractable(videoPixelFormat, canEdit);
        SetParameterInteractable(videoCodec, canEdit);
        SetParameterInteractable(videoPreset, canEdit);
        SetParameterInteractable(webmVideoBitrate, canEdit);
        SetParameterInteractable(webmVideoDeadlineMode, canEdit);
        SetParameterInteractable(webmVideoCpuUsed, canEdit);
        SetParameterInteractable(stopVideoTimeoutMs, canEdit);
        SetParameterInteractable(waitTempFileReadyTimeoutMs, canEdit);
        SetParameterInteractable(mergeTimeoutMs, canEdit);
        SetParameterInteractable(deleteTempFilesAfterMerge, canEdit);
        SetParameterInteractable(btnSelectFfmpeg, canEdit);
        SetParameterInteractable(btnResetFfmpeg, canEdit);
    }

    /// <summary>
    /// 功能：执行 SetParameterInteractable 相关逻辑。
    /// </summary>
    private void SetParameterInteractable(Selectable selectable, bool canEdit)
    {
        if (selectable == null) return;
        if (!_originSelectableStates.ContainsKey(selectable)) _originSelectableStates.Add(selectable, selectable.interactable);
        selectable.interactable = canEdit && _originSelectableStates[selectable];
    }

    /// <summary>
    /// 功能：执行 RefreshFFmpegPathTips 相关逻辑。
    /// </summary>
    private void RefreshFFmpegPathTips()
    {
        if (txtFfmpegPathTips == null || recorder == null) return;
        string currentPath = ifFfmpegPath != null ? ifFfmpegPath.text.Trim() : recorder.customFFmpegPath;
        string defaultPath = recorder.GetPlatformDefaultFFmpegPath();
        txtFfmpegPathTips.text = string.IsNullOrWhiteSpace(currentPath) ? $"当前使用平台默认 FFmpeg: {defaultPath}" : $"当前使用自定义 FFmpeg: {currentPath}";
    }


    /// <summary>
    /// 功能：执行 BindDropdown 相关逻辑。
    /// </summary>
    private void BindDropdown(Dropdown dropdown, UnityEngine.Events.UnityAction<int> action)
    {
        if (dropdown == null) return;
        dropdown.onValueChanged.RemoveListener(action);
        dropdown.onValueChanged.AddListener(action);
    }

    /// <summary>
    /// 功能：执行 SetDropdownOptions 相关逻辑。
    /// </summary>
    private static void SetDropdownOptions(Dropdown dropdown, List<string> options)
    {
        if (dropdown == null) return;
        dropdown.ClearOptions();
        dropdown.AddOptions(options);
        dropdown.value = 0;
        dropdown.RefreshShownValue();
    }

    /// <summary>
    /// 功能：执行 SetDropdownValue 相关逻辑。
    /// </summary>
    private static void SetDropdownValue(Dropdown dropdown, string value)
    {
        if (dropdown == null || string.IsNullOrWhiteSpace(value)) return;
        for (int i = 0; i < dropdown.options.Count; i++)
        {
            if (!string.Equals(dropdown.options[i].text, value, StringComparison.OrdinalIgnoreCase)) continue;
            dropdown.value = i;
            dropdown.RefreshShownValue();
            return;
        }
    }

    /// <summary>
    /// 功能：执行 getDropdownText 相关逻辑。
    /// </summary>
    private static string getDropdownText(Dropdown dropdown)
    {
        if (dropdown == null || dropdown.options.Count == 0 || dropdown.value < 0 || dropdown.value >= dropdown.options.Count) return string.Empty;
        return dropdown.options[dropdown.value].text;
    }

    /// <summary>
    /// 功能：执行 getDropdownTextOrDefault 相关逻辑。
    /// </summary>
    private static string getDropdownTextOrDefault(Dropdown dropdown, string defaultValue)
    {
        string value = getDropdownText(dropdown);
        return string.IsNullOrWhiteSpace(value) ? defaultValue : value;
    }

    /// <summary>
    /// 功能：执行 ParseInt 相关逻辑。
    /// </summary>
    private static int ParseInt(string value, int defaultValue) => int.TryParse(value, out int result) ? result : defaultValue;

    /// <summary>
    /// 功能：执行 ParseFloat 相关逻辑。
    /// </summary>
    private static float ParseFloat(string value, float defaultValue) => float.TryParse(value, out float result) ? result : defaultValue;
}
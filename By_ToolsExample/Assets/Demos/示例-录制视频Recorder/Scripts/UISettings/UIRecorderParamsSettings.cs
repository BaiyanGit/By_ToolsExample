using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif
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
    [Header("核心组件")] [Tooltip("录屏核心控制器")] public CrossPlatformScreenRecorder recorder;

    [Header("配置与基础参数")] [Tooltip("配置文件")] public Dropdown drConfig;

    #region 参数设置

    [Tooltip("显示器")] public Dropdown drDisplay;
    [Tooltip("运行平台")] public Dropdown drPlatform;
    [Tooltip("视频格式")] public Dropdown drVideoFormat;
    [Tooltip("视频文件前缀输入框")] public InputField ifVideoPrefix;
    [Header("音频参数")] [Tooltip("音频采集模式")] public Dropdown drAudioMode;
    [Tooltip("音频编码器")] public Dropdown drAudioCoder;
    [Tooltip("音频码率")] public Dropdown drAudioBitrate;
    [Tooltip("音频采样率")] public Dropdown drAudioSampleRate;
    [Tooltip("音频声道数")] public Dropdown drAudioChannel;
    [Header("视频参数")] [Tooltip("视频录制帧率。")] public Dropdown videoCaptureFrameRate;
    [Tooltip("视频像素格式")] public Dropdown videoPixelFormat;
    [Tooltip("视频码率")] public Dropdown webmVideoBitrate;
    [Tooltip("视频画质档位")] public Dropdown videoCrf;
    [Tooltip("视频编码预设")] public Dropdown videoPreset;
    [Tooltip("视频编码器")] public Dropdown videoCodec;
    [Tooltip("视频输出缩放比例")] public Dropdown videoOutputScale;
    [Tooltip("视频实时编码模式")] public Dropdown webmVideoDeadlineMode;
    [Tooltip("后台合并音视频等待超时")] public Dropdown mergeTimeoutMs;
    [Tooltip("后台合并完成后是否删除临时文件")] public Dropdown deleteTempFilesAfterMerge;
    [Tooltip("停止录制等待 ffmpeg 退出超时")] public Dropdown stopVideoTimeoutMs;
    [Tooltip("视频CPU使用等级")] public Dropdown webmVideoCpuUsed;
    [Tooltip("等待临时文件释放超时")] public Dropdown waitTempFileReadyTimeoutMs;

    #endregion

    #region FFmpeg相关

    [Header("FFmpeg路径")] [Tooltip("FFmpeg 可执行文件路径输入框。")]
    public InputField ifFfmpegPath;

    [Tooltip("选择 FFmpeg 文件按钮。")] public Button btnSelectFfmpeg;
    [Tooltip("重置 FFmpeg 路径按钮。")] public Button btnResetFfmpeg;

    #endregion

    #region 按钮功能相关

    [Header("配置操作按钮")] [Tooltip("另存为按钮")] public Button btnSaveAs;
    [Tooltip("保存按钮")] public Button btnSave;
    [Tooltip("使用按钮")] public Button btnUse;
    [Tooltip("删除用户配置按钮")] public Button btnDeleteConfig;

    [Header("另存为窗口")] [Tooltip("另存为窗口根节点")]
    public GameObject saveAsWindowRoot;

    [Tooltip("另存为名称输入框")] public InputField ifSaveAsName;
    [Tooltip("确定另存为按钮")] public Button btnSaveAsConfirm;
    [Tooltip("取消另存为按钮")] public Button btnSaveAsCancel;
    [Tooltip("另存为提示文字")] public Text txtSaveAsTips;

    #endregion

    #region 选项说明相关

    [Header("选项说明窗口")] [Tooltip("修改参数时用于标记控件的颜色。")]
    public Color changedColor = new(1f, 0.72f, 0.18f, 1f);

    [Tooltip("说明框最小尺寸")] public Vector2 descWindowMinSize = new(420f, 150f);
    [Tooltip("说明框最大尺寸")] public Vector2 descWindowMaxSize = new(640f, 360f);
    [Tooltip("选项说明 JSON 文件名")] public string optionDescriptionJsonName = "RecorderOptionDescriptions.json";
    [Tooltip("选项说明窗口根节点")] public GameObject optionDescWindowRoot;
    [Tooltip("选项说明标题文本")] public Text txtOptionDescTitle;
    [Tooltip("选项说明内容文本")] public Text txtOptionDescContent;

    #endregion

    #region 配置保存与使用

    [Header("配置文件目录")] [Tooltip("配置根目录名称")]
    public string configFolderName = "RecorderConfigs";

    [Tooltip("默认配置目录名称")] public string defaultConfigFolderName = "Default";
    [Tooltip("用户配置目录名称")] public string userConfigFolderName = "User";

    #endregion


    private readonly List<RecorderParamsConfig> _currentPlatformConfigs = new();

    private readonly Dictionary<Graphic, Color> _originGraphicColors = new();
    private readonly Dictionary<Selectable, bool> _originSelectableStates = new();
    private RecorderParamsConfig _currentConfig;
    private RecorderOptionDescriptionTable _optionDescriptionTable;
    private string _loadedConfigJson;
    private string _usingConfigFileName;
    private string _usingConfigJson;
    private Coroutine _saveAsTipsCoroutine;
    private GameObject _messageWindowRoot;
    private Text _txtMessageContent;
    private Button _btnMessageConfirm;
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
        LoadOptionDescriptions();
        EnsureUseButton();
        EnsureDeleteButton();
        EnsureSaveAsWindow();
        EnsureMessageWindow();
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

        if (btnDeleteConfig != null)
        {
            btnDeleteConfig.onClick.RemoveListener(DeleteCurrentConfig);
            btnDeleteConfig.onClick.AddListener(DeleteCurrentConfig);
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
        string ffmpegPath = GetConfigFFmpegPath(config);
        if (ifFfmpegPath != null) ifFfmpegPath.text = File.Exists(ffmpegPath) ? ffmpegPath : string.Empty;
        _isRefreshingUI = false;
    }

    /// <summary>
    /// 功能：执行 ApplyConfigToRecorder 相关逻辑。
    /// </summary>
    private void ApplyConfigToRecorder(RecorderParamsConfig config)
    {
        if (recorder == null || config == null) return;
        string ffmpegPath                  = GetConfigFFmpegPath(config);
        recorder.outputFilePrefix           = config.outputFilePrefix;
        recorder.customFFmpegPath           = ffmpegPath;
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
        config.customFFmpegPath           = ifFfmpegPath != null ? ifFfmpegPath.text.Trim() : GetConfigFFmpegPath(config);
        config.ffmpegExecutablePath       = config.customFFmpegPath;
        config.outputAsWebm               = string.Equals(GetDropdownText(drVideoFormat), "webm", StringComparison.OrdinalIgnoreCase);
        config.audioMode                  = GetDropdownText(drAudioMode) == "系统音频" ? 1 : 0;
        config.audioBitrate               = GetDropdownTextOrDefault(drAudioBitrate, config.audioBitrate);
        config.audioSampleRate            = ParseInt(GetDropdownText(drAudioSampleRate), config.audioSampleRate);
        config.audioChannels              = ParseInt(GetDropdownText(drAudioChannel), config.audioChannels);
        config.captureFrameRate           = ParseInt(GetDropdownText(videoCaptureFrameRate), config.captureFrameRate);
        config.outputScale                = ParseFloat(GetDropdownText(videoOutputScale), config.outputScale);
        config.videoCrf                   = ParseInt(GetDropdownText(videoCrf), config.videoCrf);
        config.pixelFormat                = GetDropdownTextOrDefault(videoPixelFormat, config.pixelFormat);
        config.videoPreset                = GetDropdownTextOrDefault(videoPreset, config.videoPreset);
        config.webmVideoBitrate           = GetDropdownTextOrDefault(webmVideoBitrate, config.webmVideoBitrate);
        config.webmDeadline               = GetDropdownTextOrDefault(webmVideoDeadlineMode, config.webmDeadline);
        config.webmCpuUsed                = ParseInt(GetDropdownText(webmVideoCpuUsed), config.webmCpuUsed);
        config.stopVideoTimeoutMs         = ParseInt(GetDropdownText(stopVideoTimeoutMs), config.stopVideoTimeoutMs);
        config.waitTempFileReadyTimeoutMs = ParseInt(GetDropdownText(waitTempFileReadyTimeoutMs), config.waitTempFileReadyTimeoutMs);
        config.mergeTimeoutMs             = ParseInt(GetDropdownText(mergeTimeoutMs), config.mergeTimeoutMs);
        config.deleteTempFilesAfterMerge  = GetDropdownText(deleteTempFilesAfterMerge) != "否";
        string audioCodec = GetDropdownText(drAudioCoder);
        if (!string.IsNullOrWhiteSpace(audioCodec))
        {
            if (config.outputAsWebm) config.webmAudioCodec = audioCodec;
            else config.audioCodec                         = audioCodec;
        }

        string vCodec = GetDropdownText(videoCodec);
        if (!string.IsNullOrWhiteSpace(vCodec))
        {
            if (config.outputAsWebm) config.webmVideoCodec = vCodec;
            else config.videoCodec                         = vCodec;
        }

        if (baseConfig != null && baseConfig.isDefault) KeepDefaultLockedParams(config, baseConfig);
        return config;
    }

    /// <summary>
    /// 功能：默认配置只允许基础参数和 FFmpeg 路径写回，音频、视频参数保持模板原值。
    /// </summary>
    private static void KeepDefaultLockedParams(RecorderParamsConfig config, RecorderParamsConfig baseConfig)
    {
        config.audioMode                 = baseConfig.audioMode;
        config.audioCodec                = baseConfig.audioCodec;
        config.webmAudioCodec            = baseConfig.webmAudioCodec;
        config.audioBitrate              = baseConfig.audioBitrate;
        config.audioSampleRate           = baseConfig.audioSampleRate;
        config.audioChannels             = baseConfig.audioChannels;
        config.captureFrameRate          = baseConfig.captureFrameRate;
        config.outputScale               = baseConfig.outputScale;
        config.videoCrf                  = baseConfig.videoCrf;
        config.pixelFormat               = baseConfig.pixelFormat;
        config.videoCodec                = baseConfig.videoCodec;
        config.videoPreset               = baseConfig.videoPreset;
        config.webmVideoCodec            = baseConfig.webmVideoCodec;
        config.webmVideoBitrate          = baseConfig.webmVideoBitrate;
        config.webmDeadline              = baseConfig.webmDeadline;
        config.webmCpuUsed               = baseConfig.webmCpuUsed;
        config.stopVideoTimeoutMs        = baseConfig.stopVideoTimeoutMs;
        config.waitTempFileReadyTimeoutMs = baseConfig.waitTempFileReadyTimeoutMs;
        config.mergeTimeoutMs            = baseConfig.mergeTimeoutMs;
        config.deleteTempFilesAfterMerge = baseConfig.deleteTempFilesAfterMerge;
    }

    /// <summary>
    /// 功能：执行 SaveCurrentConfig 相关逻辑。
    /// </summary>
    private void SaveCurrentConfig()
    {
        if (_currentConfig == null) return;
        if (!CheckFFmpegPathBeforeOperate()) return;

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
        if (!CheckFFmpegPathBeforeOperate()) return;
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
        if (!CheckFFmpegPathBeforeOperate()) return;
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
        if (!CheckFFmpegPathBeforeOperate(true)) return;
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
    /// 功能：删除当前选中的用户配置文件。
    /// </summary>
    private void DeleteCurrentConfig()
    {
        if (_currentConfig == null) return;
        if (_currentConfig.isDefault)
        {
            ShowMessageTips("默认配置模板不能删除。");
            return;
        }

        string fileName = _currentConfig.fileName;
        string path     = Path.Combine(GetUserConfigDirectory(), fileName);
        if (!File.Exists(path))
        {
            ShowMessageTips("未找到要删除的用户配置文件。");
            RefreshConfigDropdown();
            return;
        }

        File.Delete(path);
        string metaPath = path + ".meta";
        if (File.Exists(metaPath)) File.Delete(metaPath);
        if (string.Equals(fileName, _usingConfigFileName, StringComparison.OrdinalIgnoreCase))
        {
            _usingConfigFileName = string.Empty;
            _usingConfigJson     = string.Empty;
        }

        RefreshConfigDropdown();
        RefreshEditState();
        ShowMessageTips("已删除当前用户配置。");
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
    /// 功能：显示通用操作提示弹窗。
    /// </summary>
    private void ShowMessageTips(string tips)
    {
        EnsureMessageWindow();
        if (_messageWindowRoot == null) return;
        if (_txtMessageContent != null) _txtMessageContent.text = tips;
        _messageWindowRoot.SetActive(true);
    }

    /// <summary>
    /// 功能：隐藏通用操作提示弹窗。
    /// </summary>
    private void HideMessageTips()
    {
        if (_messageWindowRoot != null) _messageWindowRoot.SetActive(false);
    }

    /// <summary>
    /// 功能：执行 SelectFFmpegPath 相关逻辑。
    /// </summary>
    private void SelectFFmpegPath()
    {
        string path = OpenFFmpegExecutableDialog();
        if (string.IsNullOrWhiteSpace(path)) return;
        if (ifFfmpegPath != null) ifFfmpegPath.text = path;
        SetFFmpegPath(path);
    }

    /// <summary>
    /// 功能：打开当前平台支持的 FFmpeg 可执行文件选择对话框。
    /// </summary>
    private string OpenFFmpegExecutableDialog()
    {
#if UNITY_EDITOR
        string extension = Application.platform == RuntimePlatform.WindowsEditor ? "exe" : string.Empty;
        return EditorUtility.OpenFilePanel("选择 FFmpeg 可执行文件", GetFFmpegDialogStartDirectory(), extension);
#elif UNITY_STANDALONE_WIN
        return OpenWindowsFFmpegExecutableDialog();
#elif UNITY_STANDALONE_LINUX
        return OpenLinuxFFmpegExecutableDialog();
#else
        Debug.LogWarning("当前平台暂不支持 FFmpeg 文件选择对话框，请直接填写 FFmpeg 路径。");
        return string.Empty;
#endif
    }

    /// <summary>
    /// 功能：获取 FFmpeg 文件选择对话框的初始目录。
    /// </summary>
    private string GetFFmpegDialogStartDirectory()
    {
        string currentPath = ifFfmpegPath != null ? ifFfmpegPath.text.Trim() : string.Empty;
        if (!string.IsNullOrWhiteSpace(currentPath))
        {
            string directory = Path.GetDirectoryName(currentPath);
            if (!string.IsNullOrWhiteSpace(directory) && Directory.Exists(directory)) return directory;
        }

        return Application.streamingAssetsPath;
    }

#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
    /// <summary>
    /// 功能：在 Windows Player 中打开系统文件选择对话框并返回 FFmpeg 路径。
    /// </summary>
    private string OpenWindowsFFmpegExecutableDialog()
    {
        var openFileName = new OpenFileName();
        openFileName.structSize   = Marshal.SizeOf(openFileName);
        openFileName.filter       = "FFmpeg (ffmpeg.exe)\0ffmpeg.exe\0Executable Files (*.exe)\0*.exe\0All Files (*.*)\0*.*\0";
        openFileName.file         = new string(new char[4096]);
        openFileName.maxFile      = openFileName.file.Length;
        openFileName.fileTitle    = new string(new char[256]);
        openFileName.maxFileTitle = openFileName.fileTitle.Length;
        openFileName.initialDir   = GetFFmpegDialogStartDirectory().Replace('/', '\\');
        openFileName.title        = "选择 FFmpeg 可执行文件";
        openFileName.defExt       = "exe";
        openFileName.flags        = OpenFileNameFlags.OFN_EXPLORER | OpenFileNameFlags.OFN_FILEMUSTEXIST | OpenFileNameFlags.OFN_PATHMUSTEXIST | OpenFileNameFlags.OFN_NOCHANGEDIR;
        return GetOpenFileName(openFileName) ? openFileName.file.TrimEnd('\0') : string.Empty;
    }

    [DllImport("Comdlg32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
    private static extern bool GetOpenFileName([In, Out] OpenFileName openFileName);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private class OpenFileName
    {
        public int structSize;
        public IntPtr dlgOwner = IntPtr.Zero;
        public IntPtr instance = IntPtr.Zero;
        public string filter;
        public string customFilter;
        public int maxCustFilter;
        public int filterIndex;
        public string file;
        public int maxFile;
        public string fileTitle;
        public int maxFileTitle;
        public string initialDir;
        public string title;
        public int flags;
        public short fileOffset;
        public short fileExtension;
        public string defExt;
        public IntPtr custData = IntPtr.Zero;
        public IntPtr hook = IntPtr.Zero;
        public string templateName;
        public IntPtr reservedPtr = IntPtr.Zero;
        public int reservedInt;
        public int flagsEx;
    }

    private static class OpenFileNameFlags
    {
        public const int OFN_NOCHANGEDIR   = 0x00000008;
        public const int OFN_PATHMUSTEXIST = 0x00000800;
        public const int OFN_FILEMUSTEXIST = 0x00001000;
        public const int OFN_EXPLORER      = 0x00080000;
    }
#endif

#if UNITY_STANDALONE_LINUX && !UNITY_EDITOR
    /// <summary>
    /// 功能：在 Linux Player 中调用常见桌面文件选择器并返回 FFmpeg 路径。
    /// </summary>
    private string OpenLinuxFFmpegExecutableDialog()
    {
        string startDirectory = GetFFmpegDialogStartDirectory();
        if (TryOpenLinuxFileDialog("zenity", $"--file-selection --title=\"选择 FFmpeg 可执行文件\" --filename=\"{startDirectory.TrimEnd('/')}/\"", out string path)) return path;
        if (TryOpenLinuxFileDialog("kdialog", $"--title \"选择 FFmpeg 可执行文件\" --getopenfilename \"{startDirectory}\"", out path)) return path;
        Debug.LogWarning("未找到可用的 Linux 文件选择器，请安装 zenity 或 kdialog，或直接填写 FFmpeg 路径。");
        return string.Empty;
    }

    /// <summary>
    /// 功能：启动指定 Linux 文件选择器并读取用户选择的文件路径。
    /// </summary>
    private bool TryOpenLinuxFileDialog(string executable, string arguments, out string path)
    {
        path = string.Empty;
        try
        {
            var process = new System.Diagnostics.Process();
            process.StartInfo.FileName               = executable;
            process.StartInfo.Arguments              = arguments;
            process.StartInfo.UseShellExecute        = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.CreateNoWindow         = true;
            process.Start();
            path = process.StandardOutput.ReadLine();
            process.WaitForExit();
            return process.ExitCode == 0 && !string.IsNullOrWhiteSpace(path);
        }
        catch (Exception)
        {
            return false;
        }
    }
#endif

    /// <summary>
    /// 功能：执行 SetFFmpegPath 相关逻辑。
    /// </summary>
    private void SetFFmpegPath(string value)
    {
        if (_isRefreshingUI) return;
        if (recorder != null) recorder.customFFmpegPath = value.Trim();
        OnAnyParamsChanged();
    }

    /// <summary>
    /// 功能：执行 ResetFFmpegPath 相关逻辑。
    /// </summary>
    private void ResetFFmpegPath()
    {
        if (ifFfmpegPath != null) ifFfmpegPath.text     = string.Empty;
        if (recorder != null) recorder.customFFmpegPath = string.Empty;
        OnAnyParamsChanged();
    }

    /// <summary>
    /// 功能：执行 SetDefaultVideoPrefix 相关逻辑。
    /// </summary>
    private void SetDefaultVideoPrefix(string value)
    {
        OnAnyParamsChanged();
    }

    /// <summary>
    /// 功能：检查当前输入框中的 FFmpeg 路径是否存在。
    /// </summary>
    private bool HasValidFFmpegPath()
    {
        string path = ifFfmpegPath != null ? ifFfmpegPath.text.Trim() : string.Empty;
        return !string.IsNullOrWhiteSpace(path) && File.Exists(path);
    }

    /// <summary>
    /// 功能：获取配置中的 FFmpeg 路径，兼容新旧配置字段。
    /// </summary>
    private static string GetConfigFFmpegPath(RecorderParamsConfig config)
    {
        if (config == null) return string.Empty;
        return !string.IsNullOrWhiteSpace(config.customFFmpegPath) ? config.customFFmpegPath : config.ffmpegExecutablePath;
    }

    /// <summary>
    /// 功能：在保存、另存为、使用前校验 FFmpeg 路径，失败时提示用户。
    /// </summary>
    private bool CheckFFmpegPathBeforeOperate(bool showInSaveAsWindow = false)
    {
        if (HasValidFFmpegPath()) return true;
        string path = ifFfmpegPath != null ? ifFfmpegPath.text.Trim() : string.Empty;
        string tips = string.IsNullOrWhiteSpace(path) ? "请先配置 FFmpeg 可执行文件路径。" : "FFmpeg 可执行文件不存在，请重新选择。";
        if (ifFfmpegPath != null) ifFfmpegPath.ActivateInputField();
        if (showInSaveAsWindow) ShowSaveAsTips(tips);
        ShowMessageTips(tips);
        Debug.LogWarning(tips);
        return false;
    }

    /// <summary>
    /// 功能：执行 RefreshSaveButtonState 相关逻辑。
    /// </summary>
    private void RefreshSaveButtonState()
    {
        bool hasConfig      = _currentConfig != null;
        bool isDirty        = IsCurrentConfigDirty();
        if (btnSave != null)
        {
            bool showSave = hasConfig && isDirty;
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

        RefreshUseButtonState(false);
        RefreshDeleteButtonState();
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
    /// 功能：刷新删除按钮显示状态。
    /// </summary>
    private void RefreshDeleteButtonState()
    {
        if (btnDeleteConfig == null) return;
        bool showDelete = _currentConfig != null && !_currentConfig.isDefault;
        btnDeleteConfig.gameObject.SetActive(showDelete);
        btnDeleteConfig.interactable = showDelete;
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
        SetControlModified(ifFfmpegPath, GetConfigFFmpegPath(current) != GetConfigFFmpegPath(baseConfig));
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
        bool canEditFullParams = canEdit && !_currentConfig.isDefault;
        SetParameterInteractable(drDisplay, canEdit);
        SetParameterInteractable(drVideoFormat, canEdit);
        SetParameterInteractable(ifVideoPrefix, canEdit);
        SetParameterInteractable(ifFfmpegPath, canEdit);
        SetParameterInteractable(drAudioMode, canEditFullParams);
        SetParameterInteractable(drAudioCoder, canEditFullParams);
        SetParameterInteractable(drAudioBitrate, canEditFullParams);
        SetParameterInteractable(drAudioSampleRate, canEditFullParams);
        SetParameterInteractable(drAudioChannel, canEditFullParams);
        SetParameterInteractable(videoCaptureFrameRate, canEditFullParams);
        SetParameterInteractable(videoOutputScale, canEditFullParams);
        SetParameterInteractable(videoCrf, canEditFullParams);
        SetParameterInteractable(videoPixelFormat, canEditFullParams);
        SetParameterInteractable(videoCodec, canEditFullParams);
        SetParameterInteractable(videoPreset, canEditFullParams);
        SetParameterInteractable(webmVideoBitrate, canEditFullParams);
        SetParameterInteractable(webmVideoDeadlineMode, canEditFullParams);
        SetParameterInteractable(webmVideoCpuUsed, canEditFullParams);
        SetParameterInteractable(stopVideoTimeoutMs, canEditFullParams);
        SetParameterInteractable(waitTempFileReadyTimeoutMs, canEditFullParams);
        SetParameterInteractable(mergeTimeoutMs, canEditFullParams);
        SetParameterInteractable(deleteTempFilesAfterMerge, canEditFullParams);
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
    /// 功能：执行 BindDropdown 相关逻辑。
    /// </summary>
    private static void BindDropdown(Dropdown dropdown, UnityEngine.Events.UnityAction<int> action)
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
    private static string GetDropdownText(Dropdown dropdown)
    {
        if (dropdown == null || dropdown.options.Count == 0 || dropdown.value < 0 || dropdown.value >= dropdown.options.Count) return string.Empty;
        return dropdown.options[dropdown.value].text;
    }

    /// <summary>
    /// 功能：执行 getDropdownTextOrDefault 相关逻辑。
    /// </summary>
    private static string GetDropdownTextOrDefault(Dropdown dropdown, string defaultValue)
    {
        string value = GetDropdownText(dropdown);
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

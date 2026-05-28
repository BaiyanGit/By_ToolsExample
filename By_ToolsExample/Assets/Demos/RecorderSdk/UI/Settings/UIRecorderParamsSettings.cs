namespace Demos.示例_录制视频Recorder.Scripts.UISettings
{
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
    using System.Runtime.InteropServices;
#endif
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using Core;
#if UNITY_EDITOR
    using UnityEditor;
#endif
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEngine.UI;

    //=====================================================
    // 文件名称: RecorderParamsSettings
    // 创 建 者: wangbaiyan
    // 创建日期: 2026-5-7
    // 描    述: 录像参数设置脚本
    //=====================================================
    public partial class UIRecorderParamsSettings : MonoBehaviour
    {
        [Tooltip("关闭按钮")] public Button btnClose;

        #region 状态列表

        [Header("状态列表")] [Tooltip("当前状态文本。")]
        public Text txtStatusUseState;

        [Tooltip("模板名称文本。")] public Text txtStatusTemplateName;
        [Tooltip("输出格式文本。")] public Text txtStatusOutputFormat;
        [Tooltip("分辨率比例文本。")] public Text txtStatusOutputScale;
        [Tooltip("帧率文本。")] public Text txtStatusFrameRate;
        [Tooltip("视频编码器文本。")] public Text txtStatusVideoCodec;
        [Tooltip("音频编码器文本。")] public Text txtStatusAudioCodec;
        [Tooltip("当前配置最后更新时间文本。")] public Text txtStatusLastUpdateTime;

        [Tooltip("当前使用状态图标；未绑定时会从第一条状态文本同级 Icon 中自动查找。")]
        public Graphic imgUsingStatusIcon;

        [Tooltip("配置正在使用时的状态颜色。")] public Color usingStatusColor = new(0.12f, 0.78f, 0.32f, 1f);
        [Tooltip("配置未使用时的状态颜色。")] public Color unusedStatusColor = new(0.46f, 0.46f, 0.46f, 1f);

        #endregion

        #region 参数设置

        [Header("配置参数")] [Tooltip("配置文件")] public Dropdown drConfig;
        [Tooltip("FFmpeg 可执行文件路径输入框。")] public InputField ifFfmpegPath;
        [Tooltip("选择 FFmpeg 文件按钮。")] public Button btnSelectFfmpeg;
        [Tooltip("重置 FFmpeg 路径按钮。")] public Button btnResetFfmpeg;
        [Tooltip("视频文件保存路径对象")] public GameObject goVideoSavePathRoot;

        [Tooltip("视频文件保存路径输入框")] public InputField ifVideoSavePath;
        [Tooltip("选择视频文件保存路径按钮")] public Button btnSelectVideoSavePath;
        [Tooltip("视频推流地址对象")] public GameObject goStreamUrlRoot;

        [Tooltip("视频推流地址输入框")] public InputField ifStreamUrl;

        // 基础参数
        [Header("基础参数")] [Tooltip("运行平台")] public Dropdown drPlatform;
        [Tooltip("文件前缀对象")] public GameObject goVideoPrefixRoot;
        [Tooltip("文件前缀")] public InputField ifVideoPrefix;
        [Tooltip("使用模式")] public Dropdown drUseMode;
        [Tooltip("显示器")] public Dropdown drDisplay;
        [Tooltip("视频格式")] public Dropdown drVideoFormat;

        // 推流参数
        [Header("推流参数")] [Tooltip("推流参数对象")] public GameObject goStreamSettingsRoot;
        [Tooltip("推流视频码率")] public Dropdown drStreamVideoBitrate;
        [Tooltip("推流GOP帧间隔")] public Dropdown drStreamGop;
        [Tooltip("推流缓冲区大小")] public Dropdown drStreamBufferSize;

        [Tooltip("推流重连次数")] public Dropdown drStreamReconnectCount;
        [Tooltip("推流重连间隔")] public Dropdown drStreamReconnectInterval;
        [Tooltip("推流低延迟模式")] public Toggle togStreamLowLatency;
        [Tooltip("推流自动重连")] public Toggle togStreamAutoReconnect;
        [Tooltip("推流包含系统音频")] public Toggle togStreamIncludeAudio;


        // 音频参数
        [Header("音频参数")] [Tooltip("音频采集模式")] public Dropdown drAudioMode;
        [Tooltip("音频编码器")] public Dropdown drAudioCoder;
        [Tooltip("音频码率")] public Dropdown drAudioBitrate;
        [Tooltip("音频采样率")] public Dropdown drAudioSampleRate;

        [Tooltip("音频声道数")] public Dropdown drAudioChannel;
        [Tooltip("录制音量增强开关")] public Dropdown drEnableAudioGain;
        [Tooltip("录制音量增益预设，单位 dB")] public Dropdown drAudioGainDb;
        [Tooltip("音频限幅器开关")] public Dropdown drAudioLimiterEnabled;

        // 视频参数
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

        // [Header("FFmpeg路径")] [Tooltip("FFmpeg 可执行文件路径输入框。")]
        // public InputField ifFfmpegPath;
        //
        // [Tooltip("选择 FFmpeg 文件按钮。")] public Button btnSelectFfmpeg;
        // [Tooltip("重置 FFmpeg 路径按钮。")] public Button btnResetFfmpeg;

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
        [Tooltip("选项说明 JSON 文件名")] public string optionDescriptionJsonName = "OptionDescriptions.json";
        [Tooltip("选项说明窗口根节点")] public GameObject optionDescWindowRoot;
        [Tooltip("选项说明标题文本")] public Text txtOptionDescTitle;
        [Tooltip("选项说明内容文本")] public Text txtOptionDescContent;

        [Header("Legacy运行时UI")]
        [SerializeField, Tooltip("Legacy 兼容开关。默认关闭；开启后才允许运行时补建缺失的弹窗或按钮。")]
        private bool createLegacyRuntimeUI = false;

        #endregion

        #region 配置保存与使用

        [Header("配置文件目录")] [Tooltip("工具根目录名称")]
        public string toolsFolderName = "FFmpegTools";

        [Tooltip("配置根目录名称")] public string configFolderName = "Configs";
        [Tooltip("视频输出目录名称")] public string videosFolderName = "Videos";

        #endregion


        private CrossPlatformScreenRecorder _recorder; // 录屏核心控制器核心组件
        private readonly List<RecorderParamsConfig> _currentPlatformConfigs = new();
        private readonly Dictionary<Graphic, Color> _originGraphicColors = new();
        private readonly Dictionary<Selectable, bool> _originSelectableStates = new();
        private RecorderParamsConfig _currentConfig;
        private RecorderOptionDescriptionTable _optionDescriptionTable;
        private string _loadedConfigJson;
        private string _usingConfigId;
        private Coroutine _saveAsTipsCoroutine;
        private GameObject _messageWindowRoot;
        private Text _txtMessageContent;
        private Button _btnMessageConfirm;
        private Color? _btnUseOriginColor;
        private GameObject _lastLocalModePanelRoot;
        private Transform _modePanelParentCache;
        private readonly List<ModePanelBinding> _modePanelBindings = new();
        private bool _isRefreshingUI;
        private bool _displayConfigAutoCorrected;
        private const int USE_MODE_LOCAL = 0;
        private const int USE_MODE_STREAM = 1;
        private const string USE_MODE_LOCAL_NAME = "存储本地";
        private const string USE_MODE_STREAM_NAME = "视频推流";
        private const string AUDIO_OPTION_USE = "使用";
        private const string AUDIO_OPTION_NOT_USE = "不使用";
        private static readonly float[] AudioGainPresetValues = { -20f, -12f, -9f, -6f, -3f, 0f, 3f, 6f, 9f, 12f, 20f };
        private static readonly List<string> AudioGainPresetLabels = new() { "-20 dB", "-12 dB", "-9 dB", "-6 dB", "-3 dB", "0 dB", "3 dB", "6 dB", "9 dB", "12 dB", "20 dB" };

        private sealed class ModePanelBinding
        {
            public Toggle menuToggle;
            public GameObject menuRoot;
            public GameObject panelRoot;
        }

        /// <summary>
        /// 执行 Start 相关逻辑。
        /// </summary>
        private void Start()
        {
            InitUI();
            BindEvents();
            RefreshDynamicUIState();
        }

        /// <summary>
        /// 每帧兜底校验右侧参数页面互斥状态，防止场景 Toggle 旧事件重新打开隐藏页面。
        /// </summary>
        private void LateUpdate()
        {
            EnforceModePanelExclusivity();
        }

        /// <summary>
        /// 执行 InitUI 相关逻辑。
        /// </summary>
        private void InitUI()
        {
            if (_recorder == null) _recorder = CrossPlatformScreenRecorder.ins;
            NormalizeFolderSettings();
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
            RefreshDynamicUIState();
        }

        /// <summary>
        /// 执行 BindEvents 相关逻辑。
        /// </summary>
        private void BindEvents()
        {
            BindDropdown(drConfig, OnConfigChanged);
            BindDropdown(drDisplay, _ => OnAnyParamsChanged());
            BindDropdown(drUseMode, _ => OnUseModeChanged());
            BindDropdown(drVideoFormat, _ => OnVideoFormatChanged());
            BindDropdown(drAudioMode, _ => OnAnyParamsChanged());
            BindDropdown(drAudioCoder, _ => OnAnyParamsChanged());
            BindDropdown(drAudioBitrate, _ => OnAnyParamsChanged());
            BindDropdown(drAudioSampleRate, _ => OnAnyParamsChanged());
            BindDropdown(drAudioChannel, _ => OnAnyParamsChanged());
            BindDropdown(drEnableAudioGain, _ => OnAudioGainDropdownChanged());
            BindDropdown(drAudioGainDb, _ => OnAnyParamsChanged());
            BindDropdown(drAudioLimiterEnabled, _ => OnAnyParamsChanged());

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
            BindDropdown(drStreamVideoBitrate, _ => OnAnyParamsChanged());
            BindDropdown(drStreamGop, _ => OnAnyParamsChanged());
            BindDropdown(drStreamBufferSize, _ => OnAnyParamsChanged());
            BindDropdown(drStreamReconnectCount, _ => OnAnyParamsChanged());
            BindDropdown(drStreamReconnectInterval, _ => OnAnyParamsChanged());

            if (togStreamLowLatency != null)
            {
                togStreamLowLatency.onValueChanged.RemoveListener(SetStreamLowLatency);
                togStreamLowLatency.onValueChanged.AddListener(SetStreamLowLatency);
            }

            if (togStreamAutoReconnect != null)
            {
                togStreamAutoReconnect.onValueChanged.RemoveListener(SetStreamAutoReconnect);
                togStreamAutoReconnect.onValueChanged.AddListener(SetStreamAutoReconnect);
            }

            if (togStreamIncludeAudio != null)
            {
                togStreamIncludeAudio.onValueChanged.RemoveListener(SetStreamIncludeAudio);
                togStreamIncludeAudio.onValueChanged.AddListener(SetStreamIncludeAudio);
            }

            if (btnClose != null)
            {
                btnClose.onClick.RemoveListener(CloseUIRecorderParamsSettings);
                btnClose.onClick.AddListener(CloseUIRecorderParamsSettings);
            }

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

            if (ifVideoSavePath != null)
            {
                ifVideoSavePath.onValueChanged.RemoveListener(SetVideoSavePath);
                ifVideoSavePath.onValueChanged.AddListener(SetVideoSavePath);
            }

            if (ifStreamUrl != null)
            {
                ifStreamUrl.onValueChanged.RemoveListener(SetStreamUrl);
                ifStreamUrl.onValueChanged.AddListener(SetStreamUrl);
            }

            if (btnSelectVideoSavePath != null)
            {
                btnSelectVideoSavePath.onClick.RemoveListener(SelectVideoSaveDirectory);
                btnSelectVideoSavePath.onClick.AddListener(SelectVideoSaveDirectory);
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

        private void CloseUIRecorderParamsSettings()
        {
            // 检查场景中是否存在多个场景，如果存在，则卸载当前场景，若不存在直接隐藏此UI
            int loadedSceneCount = SceneManager.sceneCount;
            if (loadedSceneCount > 1)
            {
                SceneManager.UnloadSceneAsync("录制器_设置中心");
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 执行 InitDisplayDropdown 相关逻辑。
        /// </summary>
        private void InitDisplayDropdown()
        {
            var displays = RecorderDisplayProvider.GetDisplays();
            if (displays.Count > 0)
            {
                var options = new List<string>();
                foreach (var display in displays)
                {
                    options.Add(display.name);
                }

                SetDropdownOptions(drDisplay, options);
            }
        }

        /// <summary>
        /// 执行 InitPlatformDropdown 相关逻辑。
        /// </summary>
        private void InitPlatformDropdown()
        {
            if (drPlatform == null) return;
            SetDropdownOptions(drPlatform, new List<string> { GetCurrentPlatformName() });
            drPlatform.interactable = false;
        }

        /// <summary>
        /// 执行 InitParameterOptions 相关逻辑。
        /// </summary>
        private void InitParameterOptions()
        {
            SetDropdownOptions(drUseMode, new List<string> { USE_MODE_LOCAL_NAME, USE_MODE_STREAM_NAME });
            SetDropdownOptions(drVideoFormat, new List<string> { "webm", "mp4" });
            SetDropdownOptions(drAudioMode, new List<string> { "静音录制", "系统音频" });
            SetDropdownOptions(drAudioCoder, new List<string> { "aac", "libvorbis", "libopus" });
            SetDropdownOptions(drAudioBitrate, new List<string> { "96k", "128k", "192k", "256k" });
            SetDropdownOptions(drAudioSampleRate, new List<string> { "44100", "48000" });
            SetDropdownOptions(drAudioChannel, new List<string> { "1", "2" });
            SetDropdownOptions(drEnableAudioGain, new List<string> { AUDIO_OPTION_USE, AUDIO_OPTION_NOT_USE });
            SetDropdownOptions(drAudioGainDb, AudioGainPresetLabels);
            SetDropdownOptions(drAudioLimiterEnabled, new List<string> { AUDIO_OPTION_USE, AUDIO_OPTION_NOT_USE });
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
            SetDropdownOptions(drStreamVideoBitrate, new List<string> { "1M", "2M", "3M", "5M", "8M" });
            SetDropdownOptions(drStreamGop, new List<string> { "20", "30", "50", "60", "120" });
            SetDropdownOptions(drStreamBufferSize, new List<string> { "2M", "4M", "6M", "10M", "16M" });
            SetDropdownOptions(drStreamReconnectCount, new List<string> { "0", "3", "5", "10" });
            SetDropdownOptions(drStreamReconnectInterval, new List<string> { "1000", "3000", "5000", "10000" });
            RefreshFormatOptionVisibility();
        }

        /// <summary>
        /// 执行 OnConfigChanged 相关逻辑。
        /// </summary>
        private void OnConfigChanged(int index)
        {
            if (_isRefreshingUI || index < 0 || index >= _currentPlatformConfigs.Count) return;
            _currentConfig = _currentPlatformConfigs[index].Clone();
            ApplyConfig(_currentConfig);
        }

        /// <summary>
        /// 执行 OnAnyParamsChanged 相关逻辑。
        /// </summary>
        private void OnAnyParamsChanged()
        {
            if (_isRefreshingUI || _currentConfig == null) return;
            _currentConfig = BuildConfigFromUI(_currentConfig);
            RefreshDynamicUIState();
        }

        /// <summary>
        /// 执行 ApplyConfig 相关逻辑。
        /// </summary>
        private void ApplyConfig(RecorderParamsConfig config)
        {
            if (config == null) return;
            _displayConfigAutoCorrected = CorrectConfigDisplayIfNeeded(config);
            ApplyConfigToUI(config);
            _loadedConfigJson = JsonUtility.ToJson(BuildConfigFromUI(config));
            RefreshDynamicUIState();
        }

        /// <summary>
        /// 执行 ApplyConfigToUI 相关逻辑。
        /// </summary>
        private void ApplyConfigToUI(RecorderParamsConfig config)
        {
            _isRefreshingUI = true;
            SetDropdownValue(drVideoFormat, config.outputAsWebm ? "webm" : "mp4");
            SetDropdownValue(drUseMode, GetUseModeName(config.useMode));
            RefreshFormatOptionVisibility();
            SetDropdownValue(drAudioMode, config.audioMode == 1 ? "系统音频" : "静音录制");
            SetDropdownValue(drAudioCoder, config.outputAsWebm ? config.webmAudioCodec : config.audioCodec);
            SetDropdownValue(drAudioBitrate, config.audioBitrate);
            SetDropdownValue(drAudioSampleRate, config.audioSampleRate.ToString());
            SetDropdownValue(drAudioChannel, config.audioChannels.ToString());
            SetDropdownValue(drEnableAudioGain, config.enableAudioGain ? AUDIO_OPTION_USE : AUDIO_OPTION_NOT_USE);
            SetDropdownValue(drAudioGainDb, GetNearestAudioGainLabel(config.audioGainDb));
            SetDropdownValue(drAudioLimiterEnabled, config.audioLimiterEnabled ? AUDIO_OPTION_USE : AUDIO_OPTION_NOT_USE);
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
            SetDropdownValue(drStreamVideoBitrate, config.streamVideoBitrate);
            SetDropdownValue(drStreamGop, config.streamGop.ToString());
            SetDropdownValue(drStreamBufferSize, config.streamBufferSize);
            SetDropdownValue(drStreamReconnectCount, config.streamReconnectCount.ToString());
            SetDropdownValue(drStreamReconnectInterval, config.streamReconnectIntervalMs.ToString());
            if (togStreamLowLatency != null) togStreamLowLatency.isOn       = config.streamLowLatency;
            if (togStreamAutoReconnect != null) togStreamAutoReconnect.isOn = config.streamAutoReconnect;
            if (togStreamIncludeAudio != null) togStreamIncludeAudio.isOn   = config.streamIncludeAudio;
            if (drDisplay != null)
            {
                drDisplay.value = Mathf.Clamp(config.displayIndex, 0, Mathf.Max(0, drDisplay.options.Count - 1));
                drDisplay.RefreshShownValue();
            }

            if (ifVideoPrefix != null) ifVideoPrefix.text     = config.outputFilePrefix;
            string saveDirectory                              = GetConfigVideoSaveDirectory(config);
            if (ifVideoSavePath != null) ifVideoSavePath.text = saveDirectory;
            if (ifStreamUrl != null) ifStreamUrl.text         = config.streamUrl ?? string.Empty;
            string ffmpegPath                                 = GetConfigFFmpegPath(config);
            if (ifFfmpegPath != null) ifFfmpegPath.text       = File.Exists(ffmpegPath) ? ffmpegPath : string.Empty;
            RefreshUseModeUI();
            _isRefreshingUI = false;
        }

        /// <summary>
        /// 检查配置中的显示屏是否仍匹配当前电脑，失效时回落到第一个显示屏。
        /// </summary>
        private bool CorrectConfigDisplayIfNeeded(RecorderParamsConfig config)
        {
            if (config == null || drDisplay == null || drDisplay.options == null || drDisplay.options.Count == 0) return false;
            bool   invalidIndex       = config.displayIndex < 0 || config.displayIndex >= drDisplay.options.Count;
            string currentDisplayName = invalidIndex ? string.Empty : drDisplay.options[config.displayIndex].text;
            bool   invalidName        = string.IsNullOrWhiteSpace(config.captureDisplayName) || !string.Equals(config.captureDisplayName, currentDisplayName, StringComparison.OrdinalIgnoreCase);
            if (!invalidIndex && !invalidName) return false;

            config.displayIndex = 0;
            config.captureDisplayName  = drDisplay.options[0].text;
            Debug.LogWarning($"录制配置显示屏与当前电脑不一致，已回落到第一个显示屏: {config.captureDisplayName}");
            return true;
        }

        /// <summary>
        /// 获取当前选择的显示屏名称。
        /// </summary>
        private string GetSelectedDisplayName()
        {
            if (drDisplay == null || drDisplay.options == null || drDisplay.options.Count == 0) return string.Empty;
            int index = Mathf.Clamp(drDisplay.value, 0, drDisplay.options.Count - 1);
            return drDisplay.options[index].text;
        }

        /// <summary>
        /// 兼容场景中旧的序列化目录名，统一到 FFmpegTools 新目录结构。
        /// </summary>
        private void NormalizeFolderSettings()
        {
            if (string.IsNullOrWhiteSpace(toolsFolderName)) toolsFolderName                                                                                = "FFmpegTools";
            if (string.IsNullOrWhiteSpace(configFolderName) || configFolderName == "RecorderConfigs") configFolderName                                     = "Configs";
            if (string.IsNullOrWhiteSpace(videosFolderName) || videosFolderName == "ReocderVideo" || videosFolderName == "RecorderVideo") videosFolderName = "Videos";
        }

        /// <summary>
        /// 执行 ApplyConfigToRecorder 相关逻辑。
        /// </summary>
        private void ApplyConfigToRecorder(RecorderParamsConfig config)
        {
            if (_recorder == null || config == null) return;
            _recorder.ReloadConfig();
        }

        /// <summary>
        /// 执行 BuildConfigFromUI 相关逻辑。
        /// </summary>
        private RecorderParamsConfig BuildConfigFromUI(RecorderParamsConfig baseConfig)
        {
            var config = baseConfig != null ? baseConfig.Clone() : CreateDefaultConfig(GetCurrentPlatformName(), "自定义", 1, false);
            config.platform                   = GetCurrentPlatformName();
            config.displayIndex               = drDisplay != null ? drDisplay.value : 0;
            config.captureDisplayName         = GetSelectedDisplayName();
            config.outputFilePrefix           = ifVideoPrefix != null ? ifVideoPrefix.text.Trim() : config.outputFilePrefix;
            config.useMode                    = GetSelectedUseMode();
            config.videoSaveDirectory         = ifVideoSavePath != null ? ifVideoSavePath.text.Trim() : GetConfigVideoSaveDirectory(config);
            config.streamUrl                  = ifStreamUrl != null ? ifStreamUrl.text.Trim() : config.streamUrl;
            config.streamVideoBitrate         = GetDropdownTextOrDefault(drStreamVideoBitrate, config.streamVideoBitrate);
            config.streamGop                  = ParseInt(GetDropdownText(drStreamGop), config.streamGop);
            config.streamBufferSize           = GetDropdownTextOrDefault(drStreamBufferSize, config.streamBufferSize);
            config.streamLowLatency           = togStreamLowLatency == null ? config.streamLowLatency : togStreamLowLatency.isOn;
            config.streamAutoReconnect        = togStreamAutoReconnect == null ? config.streamAutoReconnect : togStreamAutoReconnect.isOn;
            config.streamReconnectCount       = ParseInt(GetDropdownText(drStreamReconnectCount), config.streamReconnectCount);
            config.streamReconnectIntervalMs  = ParseInt(GetDropdownText(drStreamReconnectInterval), config.streamReconnectIntervalMs);
            config.streamIncludeAudio         = togStreamIncludeAudio == null ? config.streamIncludeAudio : togStreamIncludeAudio.isOn;
            config.customFFmpegPath           = ifFfmpegPath != null ? ifFfmpegPath.text.Trim() : GetConfigFFmpegPath(config);
            config.outputAsWebm               = string.Equals(GetDropdownText(drVideoFormat), "webm", StringComparison.OrdinalIgnoreCase);
            config.audioMode                  = GetDropdownText(drAudioMode) == "系统音频" ? 1 : 0;
            config.audioBitrate               = GetDropdownTextOrDefault(drAudioBitrate, config.audioBitrate);
            config.audioSampleRate            = ParseInt(GetDropdownText(drAudioSampleRate), config.audioSampleRate);
            config.audioChannels              = ParseInt(GetDropdownText(drAudioChannel), config.audioChannels);
            config.enableAudioGain            = drEnableAudioGain == null ? config.enableAudioGain : IsUseOptionSelected(drEnableAudioGain);
            config.audioGainDb                = drAudioGainDb == null ? config.audioGainDb : GetSelectedAudioGainDb();
            config.audioLimiterEnabled        = drAudioLimiterEnabled == null ? config.audioLimiterEnabled : IsUseOptionSelected(drAudioLimiterEnabled);
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
        /// 默认配置只允许基础参数和 FFmpeg 路径写回，音频、视频参数保持模板原值。
        /// </summary>
        private static void KeepDefaultLockedParams(RecorderParamsConfig config, RecorderParamsConfig baseConfig)
        {
            config.audioMode                  = baseConfig.audioMode;
            config.audioCodec                 = baseConfig.audioCodec;
            config.webmAudioCodec             = baseConfig.webmAudioCodec;
            config.audioBitrate               = baseConfig.audioBitrate;
            config.audioSampleRate            = baseConfig.audioSampleRate;
            config.audioChannels              = baseConfig.audioChannels;
            config.enableAudioGain            = baseConfig.enableAudioGain;
            config.audioGainDb                = baseConfig.audioGainDb;
            config.audioLimiterEnabled        = baseConfig.audioLimiterEnabled;
            config.captureFrameRate           = baseConfig.captureFrameRate;
            config.outputScale                = baseConfig.outputScale;
            config.videoCrf                   = baseConfig.videoCrf;
            config.pixelFormat                = baseConfig.pixelFormat;
            config.videoCodec                 = baseConfig.videoCodec;
            config.videoPreset                = baseConfig.videoPreset;
            config.webmVideoCodec             = baseConfig.webmVideoCodec;
            config.webmVideoBitrate           = baseConfig.webmVideoBitrate;
            config.webmDeadline               = baseConfig.webmDeadline;
            config.webmCpuUsed                = baseConfig.webmCpuUsed;
            config.stopVideoTimeoutMs         = baseConfig.stopVideoTimeoutMs;
            config.waitTempFileReadyTimeoutMs = baseConfig.waitTempFileReadyTimeoutMs;
            config.mergeTimeoutMs             = baseConfig.mergeTimeoutMs;
            config.deleteTempFilesAfterMerge  = baseConfig.deleteTempFilesAfterMerge;
        }

        /// <summary>
        /// 执行 SaveCurrentConfig 相关逻辑。
        /// </summary>
        private void SaveCurrentConfig()
        {
            if (_currentConfig == null) return;
            if (!CheckFFmpegPathBeforeOperate()) return;
            if (!CheckVideoSaveDirectoryBeforeOperate()) return;
            if (!CheckStreamUrlBeforeOperate()) return;

            try
            {
                bool wasUsingSelectedConfig = IsSelectedConfigUsing();
                _currentConfig = BuildConfigFromUI(_currentConfig);
                SaveConfig(_currentConfig);
                string savedConfigId = _currentConfig.configId;
                if (wasUsingSelectedConfig)
                {
                    SaveCurrentRecordConfig(_currentConfig);
                    ApplyConfigToRecorder(_currentConfig);
                    _usingConfigId = savedConfigId;
                }

                _loadedConfigJson = JsonUtility.ToJson(_currentConfig);
                RefreshConfigDropdown();
                SelectConfigByConfigId(savedConfigId);
                RefreshEditState();
                ShowMessageTips("保存成功");
                Debug.Log("已保存录制配置: " + _currentConfig.displayName);
            }
            catch (Exception exception)
            {
                Debug.LogError("保存录制配置失败: " + exception);
                ShowMessageTips("保存失败，请重试。");
            }
        }

        /// <summary>
        /// 执行 SaveAsConfig 相关逻辑。
        /// </summary>
        private void SaveAsConfig()
        {
            if (_currentConfig == null) return;
            if (!CheckFFmpegPathBeforeOperate()) return;
            if (!CheckVideoSaveDirectoryBeforeOperate()) return;
            if (!CheckStreamUrlBeforeOperate()) return;
            EnsureSaveAsWindow();
            if (ifSaveAsName != null)
            {
                ifSaveAsName.text = BuildUserConfigName(_currentConfig);
                ifSaveAsName.ActivateInputField();
            }

            HideSaveAsTips();
            if (saveAsWindowRoot != null) saveAsWindowRoot.SetActive(true);
        }

        /// <summary>
        /// 执行 UseCurrentConfig 相关逻辑。
        /// </summary>
        private void UseCurrentConfig()
        {
            if (_currentConfig == null) return;
            if (!CheckFFmpegPathBeforeOperate()) return;
            if (!CheckVideoSaveDirectoryBeforeOperate()) return;
            if (!CheckStreamUrlBeforeOperate()) return;
            _currentConfig = BuildConfigFromUI(_currentConfig);
            SaveConfig(_currentConfig);
            SaveCurrentRecordConfig(_currentConfig);
            ApplyConfigToRecorder(_currentConfig);
            _usingConfigId       = _currentConfig.configId;
            _loadedConfigJson    = JsonUtility.ToJson(_currentConfig);
            RefreshEditState();
            Debug.Log("已使用录制配置: " + _currentConfig.displayName);
        }

        /// <summary>
        /// 执行 ConfirmSaveAsConfig 相关逻辑。
        /// </summary>
        private void ConfirmSaveAsConfig()
        {
            if (!CheckFFmpegPathBeforeOperate(true)) return;
            if (!CheckVideoSaveDirectoryBeforeOperate(true)) return;
            if (!CheckStreamUrlBeforeOperate(true)) return;
            string configName                                     = ifSaveAsName != null ? ifSaveAsName.text.Trim() : string.Empty;
            if (string.IsNullOrWhiteSpace(configName)) configName = _currentConfig != null ? BuildUserConfigName(_currentConfig) : "RecorderConfig";
            string configId                                       = BuildUserConfigId(GetCurrentPlatformName(), configName);
            if (HasConfigId(configId))
            {
                ShowSaveAsTips("本地已存在同名配置文件，请修改名称后再保存。");
                return;
            }

            var config = BuildConfigFromUI(_currentConfig);
            config.isDefault  = false;
            config.configId   = configId;
            config.fileName   = BuildConfigFileName(configId);
            config.displayName = configName;
            SaveConfig(config);
            SaveCurrentRecordConfig(config);
            ApplyConfigToRecorder(config);
            _usingConfigId       = config.configId;
            _loadedConfigJson    = JsonUtility.ToJson(BuildConfigFromUI(config));
            HideSaveAsWindow();
            RefreshConfigDropdown();
            SelectConfigByConfigId(config.configId);
            RefreshEditState();
            Debug.Log("已另存为录制配置: " + config.displayName);
        }

        /// <summary>
        /// 删除当前选中的用户配置文件。
        /// </summary>
        private void DeleteCurrentConfig()
        {
            if (_currentConfig == null) return;
            if (_currentConfig.isDefault)
            {
                ShowMessageTips("默认配置模板不能删除。");
                return;
            }

            string configId = _currentConfig.configId;
            string path     = Path.Combine(GetConfigDirectory(), BuildConfigFileName(configId));
            if (!File.Exists(path))
            {
                ShowMessageTips("未找到要删除的用户配置文件。");
                RefreshConfigDropdown();
                return;
            }

            File.Delete(path);
            string metaPath = path + ".meta";
            if (File.Exists(metaPath)) File.Delete(metaPath);
            if (string.Equals(configId, _usingConfigId, StringComparison.OrdinalIgnoreCase))
            {
                var fallback = CreateFallbackRecordReference();
                File.WriteAllText(GetUseRecordConfigPath(), JsonUtility.ToJson(fallback, true));
                _usingConfigId = fallback.currentConfigId;
                ApplyConfigToRecorder(_currentConfig);
            }

            RefreshConfigDropdown();
            RefreshEditState();
            ShowMessageTips("已删除当前用户配置。");
        }

        /// <summary>
        /// 执行 HideSaveAsWindow 相关逻辑。
        /// </summary>
        private void HideSaveAsWindow()
        {
            if (saveAsWindowRoot != null) saveAsWindowRoot.SetActive(false);
        }

        /// <summary>
        /// 执行 ShowSaveAsTips 相关逻辑。
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
        /// 执行 HideSaveAsTipsDelay 相关逻辑。
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
        /// 执行 HideSaveAsTips 相关逻辑。
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
        /// 显示通用操作提示弹窗。
        /// </summary>
        private void ShowMessageTips(string tips)
        {
            EnsureMessageWindow();
            if (_messageWindowRoot == null) return;
            if (_txtMessageContent != null) _txtMessageContent.text = tips;
            _messageWindowRoot.SetActive(true);
        }

        /// <summary>
        /// 隐藏通用操作提示弹窗。
        /// </summary>
        private void HideMessageTips()
        {
            if (_messageWindowRoot != null) _messageWindowRoot.SetActive(false);
        }

        /// <summary>
        /// 执行 SelectFFmpegPath 相关逻辑。
        /// </summary>
        private void SelectFFmpegPath()
        {
            string path = OpenFFmpegExecutableDialog();
            if (string.IsNullOrWhiteSpace(path)) return;
            if (ifFfmpegPath != null) ifFfmpegPath.text = path;
            SetFFmpegPath(path);
        }

        /// <summary>
        /// 打开当前平台支持的 FFmpeg 可执行文件选择对话框。
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
        /// 获取 FFmpeg 文件选择对话框的初始目录。
        /// </summary>
        private string GetFFmpegDialogStartDirectory()
        {
            string currentPath = ifFfmpegPath != null ? ifFfmpegPath.text.Trim() : string.Empty;
            if (!string.IsNullOrWhiteSpace(currentPath))
            {
                string directory = Path.GetDirectoryName(currentPath);
                if (!string.IsNullOrWhiteSpace(directory) && Directory.Exists(directory)) return directory;
            }

            return GetFFmpegAppDirectory();
        }

#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
    /// <summary>
    /// 在 Windows Player 中打开系统文件选择对话框并返回 FFmpeg 路径。
    /// </summary>
    private string OpenWindowsFFmpegExecutableDialog()
    {
        var openFileName = new OpenFileName();
        openFileName.structSize = Marshal.SizeOf(openFileName);
        openFileName.filter = "FFmpeg (ffmpeg.exe)\0ffmpeg.exe\0Executable Files (*.exe)\0*.exe\0All Files (*.*)\0*.*\0";
        openFileName.file = new string(new char[4096]);
        openFileName.maxFile = openFileName.file.Length;
        openFileName.fileTitle = new string(new char[256]);
        openFileName.maxFileTitle = openFileName.fileTitle.Length;
        openFileName.initialDir = GetFFmpegDialogStartDirectory().Replace('/', '\\');
        openFileName.title = "选择 FFmpeg 可执行文件";
        openFileName.defExt = "exe";
        openFileName.flags = OpenFileNameFlags.OFN_EXPLORER | OpenFileNameFlags.OFN_FILEMUSTEXIST | OpenFileNameFlags.OFN_PATHMUSTEXIST | OpenFileNameFlags.OFN_NOCHANGEDIR;
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
        public const int OFN_NOCHANGEDIR = 0x00000008;
        public const int OFN_PATHMUSTEXIST = 0x00000800;
        public const int OFN_FILEMUSTEXIST = 0x00001000;
        public const int OFN_EXPLORER = 0x00080000;
    }

    /// <summary>
    /// 在 Windows Player 中打开系统文件夹选择对话框。
    /// </summary>
    private string OpenWindowsFolderDialog(string title, string startDirectory)
    {
        var browseInfo = new BrowseInfo
        {
            title = title,
            flags = BrowseInfoFlags.BIF_RETURNONLYFSDIRS | BrowseInfoFlags.BIF_NEWDIALOGSTYLE,
            root = IntPtr.Zero,
            owner = IntPtr.Zero,
            displayName = new string(new char[260])
        };

        IntPtr itemIdList = SHBrowseForFolder(browseInfo);
        if (itemIdList == IntPtr.Zero) return string.Empty;
        var path = new System.Text.StringBuilder(260);
        bool result = SHGetPathFromIDList(itemIdList, path);
        Marshal.FreeCoTaskMem(itemIdList);
        return result ? path.ToString() : string.Empty;
    }

    [DllImport("shell32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr SHBrowseForFolder([In, Out] BrowseInfo browseInfo);

    [DllImport("shell32.dll", CharSet = CharSet.Auto)]
    private static extern bool SHGetPathFromIDList(IntPtr pidl, System.Text.StringBuilder path);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private class BrowseInfo
    {
        public IntPtr owner;
        public IntPtr root;
        public string displayName;
        public string title;
        public uint flags;
        public IntPtr callback;
        public IntPtr param;
        public int image;
    }

    private static class BrowseInfoFlags
    {
        public const uint BIF_RETURNONLYFSDIRS = 0x00000001;
        public const uint BIF_NEWDIALOGSTYLE = 0x00000040;
    }
#endif

#if UNITY_STANDALONE_LINUX && !UNITY_EDITOR
    /// <summary>
    /// 在 Linux Player 中调用常见桌面文件选择器并返回 FFmpeg 路径。
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
    /// 启动指定 Linux 文件选择器并读取用户选择的文件路径。
    /// </summary>
    private bool TryOpenLinuxFileDialog(string executable, string arguments, out string path)
    {
        path = string.Empty;
        try
        {
            var process = new System.Diagnostics.Process();
            process.StartInfo.FileName = executable;
            process.StartInfo.Arguments = arguments;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.CreateNoWindow = true;
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
        /// 执行 SetFFmpegPath 相关逻辑。
        /// </summary>
        private void SetFFmpegPath(string value)
        {
            if (_isRefreshingUI) return;
            OnAnyParamsChanged();
        }

        /// <summary>
        /// 执行 ResetFFmpegPath 相关逻辑。
        /// </summary>
        private void ResetFFmpegPath()
        {
            if (ifFfmpegPath != null) ifFfmpegPath.text = string.Empty;
            OnAnyParamsChanged();
        }

        /// <summary>
        /// 选择视频文件保存目录。
        /// </summary>
        private void SelectVideoSaveDirectory()
        {
            string path = OpenVideoSaveDirectoryDialog();
            if (string.IsNullOrWhiteSpace(path)) return;
            if (ifVideoSavePath != null) ifVideoSavePath.text = path;
            SetVideoSavePath(path);
        }

        /// <summary>
        /// 设置视频文件保存路径并刷新编辑状态。
        /// </summary>
        private void SetVideoSavePath(string value)
        {
            if (_isRefreshingUI) return;
            OnAnyParamsChanged();
        }

        /// <summary>
        /// 推流地址发生变化时刷新配置修改状态。
        /// </summary>
        private void SetStreamUrl(string value)
        {
            if (_isRefreshingUI) return;
            OnAnyParamsChanged();
        }

        /// <summary>
        /// 推流低延迟开关变化时刷新配置修改状态。
        /// </summary>
        private void SetStreamLowLatency(bool value)
        {
            if (_isRefreshingUI) return;
            OnAnyParamsChanged();
        }

        /// <summary>
        /// 推流自动重连开关变化时刷新配置修改状态。
        /// </summary>
        private void SetStreamAutoReconnect(bool value)
        {
            if (_isRefreshingUI) return;
            OnAnyParamsChanged();
        }

        /// <summary>
        /// 推流系统音频开关变化时刷新配置修改状态。
        /// </summary>
        private void SetStreamIncludeAudio(bool value)
        {
            if (_isRefreshingUI) return;
            OnAnyParamsChanged();
        }

        /// <summary>
        /// 录制音量增强下拉框变化时刷新相关控件状态。
        /// </summary>
        private void OnAudioGainDropdownChanged()
        {
            if (_isRefreshingUI) return;
            RefreshAudioGainUI();
            OnAnyParamsChanged();
        }

        /// <summary>
        /// 视频格式变化时刷新格式相关选项和编辑状态。
        /// </summary>
        private void OnVideoFormatChanged()
        {
            RefreshFormatOptionVisibility();
            OnAnyParamsChanged();
        }

        /// <summary>
        /// 使用方式发生变化时刷新本地保存路径区域。
        /// </summary>
        private void OnUseModeChanged()
        {
            RefreshUseModeUI();
            OnAnyParamsChanged();
        }

        /// <summary>
        /// 根据使用方式显示或隐藏本地保存路径对象。
        /// </summary>
        private void RefreshUseModeUI()
        {
            bool showLocalPath = IsLocalSaveMode();
            bool showStreamUrl = IsStreamMode();
            RefreshVisibleMenuItems();
            SelectFirstVisibleMenuIfCurrentHidden();
            RefreshVisiblePanels();
            SetControlRootActive(goVideoPrefixRoot, ifVideoPrefix, showLocalPath);
            if (goVideoSavePathRoot != null) goVideoSavePathRoot.SetActive(showLocalPath);
            else if (ifVideoSavePath != null) ifVideoSavePath.gameObject.SetActive(showLocalPath);
            if (btnSelectVideoSavePath != null) btnSelectVideoSavePath.gameObject.SetActive(showLocalPath);
            if (goStreamUrlRoot != null) goStreamUrlRoot.SetActive(showStreamUrl);
            else if (ifStreamUrl != null) ifStreamUrl.gameObject.SetActive(showStreamUrl);
            SetControlActive(drVideoFormat, showLocalPath);
            SetControlActive(videoCrf, showLocalPath);
            SetControlActive(mergeTimeoutMs, showLocalPath);
            SetControlActive(deleteTempFilesAfterMerge, showLocalPath);
            SetControlActive(stopVideoTimeoutMs, showLocalPath);
            SetControlActive(waitTempFileReadyTimeoutMs, showLocalPath);
            RefreshAudioGainUI();
            RefreshFormatOptionVisibility();
            RefreshStatusDetails();
        }

        /// <summary>
        /// 根据使用方式刷新左侧分类菜单可见性。
        /// </summary>
        private void RefreshVisibleMenuItems()
        {
            EnsureModePanelBindings();
            bool streamMode = IsStreamMode();
            foreach (var binding in _modePanelBindings)
            {
                if (binding?.menuRoot == null || binding.panelRoot == null) continue;
                bool visible = streamMode || !IsStreamPanel(binding.panelRoot);
                binding.menuRoot.SetActive(visible);
                if (!visible && binding.menuToggle != null) binding.menuToggle.SetIsOnWithoutNotify(false);
            }
        }

        /// <summary>
        /// 当前菜单被隐藏时自动切换到第一个可见菜单。
        /// </summary>
        private void SelectFirstVisibleMenuIfCurrentHidden()
        {
            EnsureModePanelBindings();
            var current = GetSelectedVisibleMenuBinding();
            if (current != null) return;

            var firstVisible = GetFirstVisibleMenuBinding();
            foreach (var binding in _modePanelBindings)
            {
                if (binding?.menuToggle == null) continue;
                binding.menuToggle.SetIsOnWithoutNotify(binding == firstVisible);
            }
        }

        /// <summary>
        /// 根据当前可见菜单刷新右侧页面，保证页面与菜单严格一一对应。
        /// </summary>
        private void RefreshVisiblePanels()
        {
            EnsureModePanelBindings();
            HideAllModePanels();
            var selected = GetSelectedVisibleMenuBinding() ?? GetFirstVisibleMenuBinding();
            if (selected?.panelRoot == null) return;
            selected.panelRoot.SetActive(true);
            if (!IsStreamPanel(selected.panelRoot)) _lastLocalModePanelRoot = selected.panelRoot;
        }

        /// <summary>
        /// 隐藏所有右侧参数页面，避免不同列表叠加显示。
        /// </summary>
        private void HideAllModePanels()
        {
            var pageParent = GetModePanelParent();
            if (pageParent == null) return;
            for (int i = 0; i < pageParent.childCount; i++)
            {
                pageParent.GetChild(i).gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 兼容旧调用：根据当前菜单状态显示唯一页面。
        /// </summary>
        private void ShowPanelByUseMode(int useMode, GameObject localPanelToShow)
        {
            RefreshVisiblePanels();
        }

        /// <summary>
        /// 获取当前激活的本地录制参数页面。
        /// </summary>
        private GameObject GetActiveLocalModePanelRoot()
        {
            var pageParent = GetModePanelParent();
            if (pageParent == null) return null;
            GameObject activePanel = null;
            for (int i = 0; i < pageParent.childCount; i++)
            {
                var child = pageParent.GetChild(i).gameObject;
                if (child == goStreamSettingsRoot || !child.activeSelf) continue;
                activePanel = child;
            }

            if (activePanel != null) _lastLocalModePanelRoot = activePanel;
            return activePanel;
        }

        /// <summary>
        /// 获取第一个本地录制参数页面。
        /// </summary>
        private GameObject GetFirstLocalModePanelRoot()
        {
            var pageParent = GetModePanelParent();
            if (pageParent == null) return null;
            for (int i = 0; i < pageParent.childCount; i++)
            {
                var child = pageParent.GetChild(i).gameObject;
                if (child == goStreamSettingsRoot) continue;
                return child;
            }

            return null;
        }

        /// <summary>
        /// 获取右侧参数页面列表父节点。
        /// </summary>
        private Transform GetModePanelParent()
        {
            return goStreamSettingsRoot != null ? goStreamSettingsRoot.transform.parent : null;
        }

        /// <summary>
        /// 强制右侧参数页面保持互斥显示。
        /// </summary>
        private void EnforceModePanelExclusivity()
        {
            if (_isRefreshingUI) return;
            EnsureModePanelBindings();
            if (_modePanelBindings.Count == 0) return;

            int activeCount = 0;
            bool hasHiddenMenuActive = false;
            foreach (var binding in _modePanelBindings)
            {
                if (binding?.panelRoot == null || binding.menuRoot == null) continue;
                if (binding.panelRoot.activeSelf) activeCount++;
                if (!binding.menuRoot.activeSelf && binding.panelRoot.activeSelf) hasHiddenMenuActive = true;
            }

            var selected = GetSelectedVisibleMenuBinding();
            bool selectedPanelVisible = selected != null && selected.panelRoot != null && selected.panelRoot.activeSelf;
            bool needsRefresh = activeCount != 1 || hasHiddenMenuActive || !selectedPanelVisible;
            if (!needsRefresh) return;

            RefreshVisibleMenuItems();
            SelectFirstVisibleMenuIfCurrentHidden();
            RefreshVisiblePanels();
        }

        /// <summary>
        /// 构建左侧菜单和右侧页面的绑定关系。
        /// </summary>
        private void EnsureModePanelBindings()
        {
            var pageParent = GetModePanelParent();
            if (pageParent == null)
            {
                _modePanelBindings.Clear();
                _modePanelParentCache = null;
                return;
            }

            if (_modePanelParentCache == pageParent && _modePanelBindings.Count > 0) return;

            _modePanelParentCache = pageParent;
            _modePanelBindings.Clear();
            var pageRoots = new HashSet<GameObject>();
            for (int i = 0; i < pageParent.childCount; i++)
            {
                pageRoots.Add(pageParent.GetChild(i).gameObject);
            }

            var toggles = FindObjectsOfType<Toggle>(true);
            foreach (var toggle in toggles)
            {
                if (toggle == null || toggle.gameObject.scene != gameObject.scene) continue;
                GameObject targetPage = null;
                int eventCount = toggle.onValueChanged.GetPersistentEventCount();
                for (int i = 0; i < eventCount; i++)
                {
                    if (toggle.onValueChanged.GetPersistentMethodName(i) != "SetActive") continue;
                    var target = toggle.onValueChanged.GetPersistentTarget(i) as GameObject;
                    if (target == null || !pageRoots.Contains(target)) continue;
                    targetPage = target;
                    break;
                }

                if (targetPage == null || HasModePanelBinding(targetPage)) continue;
                _modePanelBindings.Add(new ModePanelBinding
                {
                    menuToggle = toggle,
                    menuRoot = toggle.gameObject,
                    panelRoot = targetPage
                });
            }

            _modePanelBindings.Sort((a, b) => GetSiblingPath(a.menuRoot.transform).CompareTo(GetSiblingPath(b.menuRoot.transform)));
        }

        /// <summary>
        /// 判断指定页面是否已经建立绑定。
        /// </summary>
        private bool HasModePanelBinding(GameObject panelRoot)
        {
            foreach (var binding in _modePanelBindings)
            {
                if (binding.panelRoot == panelRoot) return true;
            }

            return false;
        }

        /// <summary>
        /// 获取当前选中的可见菜单。
        /// </summary>
        private ModePanelBinding GetSelectedVisibleMenuBinding()
        {
            foreach (var binding in _modePanelBindings)
            {
                if (binding?.menuToggle == null || binding.menuRoot == null) continue;
                if (!binding.menuRoot.activeSelf || !binding.menuToggle.isOn) continue;
                return binding;
            }

            return null;
        }

        /// <summary>
        /// 获取第一个可见菜单。
        /// </summary>
        private ModePanelBinding GetFirstVisibleMenuBinding()
        {
            foreach (var binding in _modePanelBindings)
            {
                if (binding?.menuRoot == null || !binding.menuRoot.activeSelf) continue;
                return binding;
            }

            return null;
        }

        /// <summary>
        /// 判断页面是否为推流设置页面。
        /// </summary>
        private bool IsStreamPanel(GameObject panelRoot)
        {
            return goStreamSettingsRoot != null && panelRoot == goStreamSettingsRoot;
        }

        /// <summary>
        /// 获取 Transform 的层级序号路径，用于保持菜单顺序。
        /// </summary>
        private static string GetSiblingPath(Transform transform)
        {
            if (transform == null) return string.Empty;
            var parts = new Stack<string>();
            while (transform != null)
            {
                parts.Push(transform.GetSiblingIndex().ToString("D4"));
                transform = transform.parent;
            }

            return string.Join("/", parts);
        }

        /// <summary>
        /// 根据音量增强开关控制增益和限幅器控件状态。
        /// </summary>
        private void RefreshAudioGainUI()
        {
            bool enableGain = drEnableAudioGain == null || IsUseOptionSelected(drEnableAudioGain);
            SetControlActive(drAudioGainDb, enableGain);
            SetControlActive(drAudioLimiterEnabled, enableGain);
        }

        /// <summary>
        /// 根据当前格式和使用方式刷新编码器选项与无关参数显示。
        /// </summary>
        private void RefreshFormatOptionVisibility()
        {
            bool isStream = IsStreamMode();
            bool isWebm   = string.Equals(GetDropdownText(drVideoFormat), "webm", StringComparison.OrdinalIgnoreCase);
            if (isStream)
            {
                SetDropdownOptionsKeepValue(videoCodec, new List<string> { "libx264" }, "libx264");
                SetDropdownOptionsKeepValue(drAudioCoder, new List<string> { "aac" }, "aac");
            }
            else if (isWebm)
            {
                SetDropdownOptionsKeepValue(videoCodec, new List<string> { "libvpx", "libvpx-vp9" }, "libvpx");
                SetDropdownOptionsKeepValue(drAudioCoder, new List<string> { "libvorbis", "libopus" }, "libvorbis");
            }
            else
            {
                SetDropdownOptionsKeepValue(videoCodec, new List<string> { "libx264", "libx265" }, "libx264");
                SetDropdownOptionsKeepValue(drAudioCoder, new List<string> { "aac" }, "aac");
            }

            SetControlActive(webmVideoBitrate, !isStream && isWebm);
            SetControlActive(webmVideoDeadlineMode, !isStream && isWebm);
            SetControlActive(webmVideoCpuUsed, !isStream && isWebm);
            SetControlActive(videoCrf, !isStream && !isWebm);
            SetControlActive(videoPixelFormat, !isStream);
        }

        /// <summary>
        /// 打开当前平台支持的视频保存目录选择对话框。
        /// </summary>
        private string OpenVideoSaveDirectoryDialog()
        {
#if UNITY_EDITOR
            return EditorUtility.OpenFolderPanel("选择视频文件保存路径", GetVideoSaveDialogStartDirectory(), string.Empty);
#elif UNITY_STANDALONE_WIN
        return OpenWindowsFolderDialog("选择视频文件保存路径", GetVideoSaveDialogStartDirectory());
#elif UNITY_STANDALONE_LINUX
        string startDirectory = GetVideoSaveDialogStartDirectory();
        if (TryOpenLinuxFileDialog("zenity", $"--file-selection --directory --title=\"选择视频文件保存路径\" --filename=\"{startDirectory.TrimEnd('/')}/\"", out string path)) return path;
        if (TryOpenLinuxFileDialog("kdialog", $"--title \"选择视频文件保存路径\" --getexistingdirectory \"{startDirectory}\"", out path)) return path;
        Debug.LogWarning("未找到可用的 Linux 文件夹选择器，请安装 zenity 或 kdialog，或直接填写保存路径。");
        return string.Empty;
#else
        Debug.LogWarning("当前平台暂不支持文件夹选择对话框，请直接填写保存路径。");
        return string.Empty;
#endif
        }

        /// <summary>
        /// 获取视频保存目录选择对话框的初始目录。
        /// </summary>
        private string GetVideoSaveDialogStartDirectory()
        {
            string currentPath = ifVideoSavePath != null ? ifVideoSavePath.text.Trim() : string.Empty;
            if (!string.IsNullOrWhiteSpace(currentPath) && Directory.Exists(currentPath)) return currentPath;
            return GetDefaultVideoSaveDirectory();
        }

        /// <summary>
        /// 执行 SetDefaultVideoPrefix 相关逻辑。
        /// </summary>
        private void SetDefaultVideoPrefix(string value)
        {
            OnAnyParamsChanged();
        }

        /// <summary>
        /// 检查当前输入框中的 FFmpeg 路径是否存在。
        /// </summary>
        private bool HasValidFFmpegPath()
        {
            string path = ifFfmpegPath != null ? ifFfmpegPath.text.Trim() : string.Empty;
            return !string.IsNullOrWhiteSpace(path) && File.Exists(path);
        }

        /// <summary>
        /// 获取配置中的 FFmpeg 路径，兼容新旧配置字段。
        /// </summary>
        private static string GetConfigFFmpegPath(RecorderParamsConfig config)
        {
            if (config == null) return string.Empty;
            return config.customFFmpegPath;
        }

        /// <summary>
        /// 获取配置中的视频保存目录，未配置时使用 FFmpegTools/Videos。
        /// </summary>
        private string GetConfigVideoSaveDirectory(RecorderParamsConfig config)
        {
            if (config != null && !string.IsNullOrWhiteSpace(config.videoSaveDirectory)) return config.videoSaveDirectory;
            return GetDefaultVideoSaveDirectory();
        }

        /// <summary>
        /// 在保存、另存为、使用前校验 FFmpeg 路径，失败时提示用户。
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
        /// 当前是否为本地存储模式。
        /// </summary>
        private bool IsLocalSaveMode()
        {
            return IsLocalSaveMode(GetSelectedUseMode());
        }

        /// <summary>
        /// 当前是否为视频推流模式。
        /// </summary>
        private bool IsStreamMode()
        {
            return GetSelectedUseMode() == USE_MODE_STREAM;
        }

        /// <summary>
        /// 判断指定使用方式是否为视频推流模式。
        /// </summary>
        private static bool IsStreamMode(int useMode)
        {
            return useMode == USE_MODE_STREAM;
        }

        /// <summary>
        /// 判断指定使用方式是否需要本地保存路径。
        /// </summary>
        private static bool IsLocalSaveMode(int useMode)
        {
            return useMode == USE_MODE_LOCAL;
        }

        /// <summary>
        /// 获取当前使用方式下拉框对应的配置值。
        /// </summary>
        private int GetSelectedUseMode()
        {
            return GetDropdownText(drUseMode) == USE_MODE_STREAM_NAME ? USE_MODE_STREAM : USE_MODE_LOCAL;
        }

        /// <summary>
        /// 获取使用方式配置值对应的显示名称。
        /// </summary>
        private static string GetUseModeName(int useMode)
        {
            return useMode == USE_MODE_STREAM ? USE_MODE_STREAM_NAME : USE_MODE_LOCAL_NAME;
        }

        /// <summary>
        /// 检查本地存储模式下的视频文件保存路径是否有效。
        /// </summary>
        private bool HasValidVideoSaveDirectory()
        {
            if (!IsLocalSaveMode()) return true;
            string path = ifVideoSavePath != null ? ifVideoSavePath.text.Trim() : string.Empty;
            return !string.IsNullOrWhiteSpace(path) && Directory.Exists(path);
        }

        /// <summary>
        /// 在保存、另存为、使用前校验视频文件保存路径。
        /// </summary>
        private bool CheckVideoSaveDirectoryBeforeOperate(bool showInSaveAsWindow = false)
        {
            if (HasValidVideoSaveDirectory()) return true;
            string path = ifVideoSavePath != null ? ifVideoSavePath.text.Trim() : string.Empty;
            string tips = string.IsNullOrWhiteSpace(path) ? "请选择视频文件保存路径。" : "视频文件保存路径不存在，请重新选择。";
            if (ifVideoSavePath != null) ifVideoSavePath.ActivateInputField();
            if (showInSaveAsWindow) ShowSaveAsTips(tips);
            ShowMessageTips(tips);
            Debug.LogWarning(tips);
            return false;
        }

        /// <summary>
        /// 检查推流模式下的视频推流地址是否有效。
        /// </summary>
        private bool HasValidStreamUrl()
        {
            if (!IsStreamMode()) return true;
            string url = ifStreamUrl != null ? ifStreamUrl.text.Trim() : string.Empty;
            return !string.IsNullOrWhiteSpace(url) &&
                   (url.StartsWith("rtmp://", StringComparison.OrdinalIgnoreCase) ||
                    url.StartsWith("rtmps://", StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// 在保存、另存为、使用前校验视频推流地址。
        /// </summary>
        private bool CheckStreamUrlBeforeOperate(bool showInSaveAsWindow = false)
        {
            if (HasValidStreamUrl()) return true;
            string url  = ifStreamUrl != null ? ifStreamUrl.text.Trim() : string.Empty;
            string tips = string.IsNullOrWhiteSpace(url) ? "请输入视频推流地址。" : "视频推流地址格式不正确，请使用 rtmp:// 或 rtmps:// 地址。";
            if (ifStreamUrl != null) ifStreamUrl.ActivateInputField();
            if (showInSaveAsWindow) ShowSaveAsTips(tips);
            ShowMessageTips(tips);
            Debug.LogWarning(tips);
            return false;
        }

        /// <summary>
        /// 执行 RefreshSaveButtonState 相关逻辑。
        /// </summary>
        private void RefreshSaveButtonState()
        {
            bool hasConfig = _currentConfig != null;
            bool isDirty   = IsCurrentConfigDirty() || _displayConfigAutoCorrected;
            if (btnSave != null)
            {
                bool showSave = hasConfig && isDirty;
                btnSave.gameObject.SetActive(showSave);
                btnSave.interactable = showSave;
                var text                    = btnSave.GetComponentInChildren<Text>(true);
                if (text != null) text.text = hasConfig && _currentConfig.isDefault ? "更新" : "保存";
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
        /// 执行 RefreshEditState 相关逻辑。
        /// </summary>
        private void RefreshEditState()
        {
            RefreshSaveButtonState();
            RefreshModifiedControlColors();
            RefreshStatusDetails();
        }

        /// <summary>
        /// 统一刷新受使用方式、格式、修改状态影响的动态 UI。
        /// </summary>
        private void RefreshDynamicUIState()
        {
            bool lastRefreshing = _isRefreshingUI;
            try
            {
                _isRefreshingUI = true;
                RefreshUseModeUI();
                RefreshParameterInteractable();
            }
            finally
            {
                _isRefreshingUI = lastRefreshing;
            }

            RefreshEditState();
        }

        /// <summary>
        /// 刷新左上角状态列表详细信息。
        /// </summary>
        private void RefreshStatusDetails()
        {
            var current = _currentConfig == null ? null : BuildConfigFromUI(_currentConfig);
            if (current == null)
            {
                SetStatusText(txtStatusUseState, "未使用");
                SetStatusText(txtStatusTemplateName, "未选择");
                SetStatusText(txtStatusOutputFormat, "未配置");
                SetStatusText(txtStatusOutputScale, "未配置");
                SetStatusText(txtStatusFrameRate, "未配置");
                SetStatusText(txtStatusVideoCodec, "未配置");
                SetStatusText(txtStatusAudioCodec, "未配置");
                SetStatusText(txtStatusLastUpdateTime, "未保存");
            }
            else
            {
                bool isUsing = IsSelectedConfigUsing();
                SetStatusText(txtStatusUseState, isUsing ? "正在使用" : "未使用");
                SetStatusText(txtStatusTemplateName, GetStatusConfigName(current));
                SetStatusText(txtStatusOutputFormat, GetStatusOutputFormat(current));
                SetStatusText(txtStatusOutputScale, current.outputScale.ToString("0.##"));
                SetStatusText(txtStatusFrameRate, current.captureFrameRate + " fps");
                SetStatusText(txtStatusVideoCodec, GetStatusVideoCodec(current));
                SetStatusText(txtStatusAudioCodec, GetStatusAudioCodec(current));
                SetStatusText(txtStatusLastUpdateTime, GetStatusLastWriteTime(current));
            }

            RefreshStatusIconColor(current != null && IsSelectedConfigUsing());
        }

        /// <summary>
        /// 设置指定状态文本内容。
        /// </summary>
        private static void SetStatusText(Text text, string content)
        {
            if (text == null) return;
            text.text = content;
        }

        /// <summary>
        /// 获取状态列表中的配置名称。
        /// </summary>
        private static string GetStatusConfigName(RecorderParamsConfig config)
        {
            if (config == null) return "未选择";
            if (!string.IsNullOrWhiteSpace(config.displayName)) return config.displayName;
            if (!string.IsNullOrWhiteSpace(config.configId)) return config.configId;
            return "未命名配置";
        }

        /// <summary>
        /// 获取状态列表中的输出格式。
        /// </summary>
        private string GetStatusOutputFormat(RecorderParamsConfig config)
        {
            if (config == null) return "未配置";
            if (IsStreamMode(config.useMode)) return "推流";
            string videoFormat = GetDropdownText(drVideoFormat);
            return string.IsNullOrWhiteSpace(videoFormat) ? (config.outputAsWebm ? "webm" : "mp4") : videoFormat;
        }

        /// <summary>
        /// 获取状态列表中的视频编码器。
        /// </summary>
        private static string GetStatusVideoCodec(RecorderParamsConfig config)
        {
            if (config == null) return "未配置";
            return config.outputAsWebm ? config.webmVideoCodec : config.videoCodec;
        }

        /// <summary>
        /// 获取状态列表中的音频编码器。
        /// </summary>
        private static string GetStatusAudioCodec(RecorderParamsConfig config)
        {
            if (config == null) return "未配置";
            return config.outputAsWebm ? config.webmAudioCodec : config.audioCodec;
        }

        /// <summary>
        /// 获取当前配置文件最后保存日期。
        /// </summary>
        private string GetStatusLastWriteTime(RecorderParamsConfig config)
        {
            if (config == null || string.IsNullOrWhiteSpace(config.fileName)) return "未保存";
            string path = Path.Combine(GetConfigDirectory(), Path.GetFileName(config.fileName));
            if (!File.Exists(path)) return "未保存";
            return File.GetLastWriteTime(path).ToString("yyyy-MM-dd HH:mm");
        }

        /// <summary>
        /// 刷新当前使用状态图标颜色。
        /// </summary>
        private void RefreshStatusIconColor(bool isUsing)
        {
            if (imgUsingStatusIcon == null) return;
            imgUsingStatusIcon.color = isUsing ? usingStatusColor : unusedStatusColor;
        }

        /// <summary>
        /// 执行 IsCurrentConfigDirty 相关逻辑。
        /// </summary>
        private bool IsCurrentConfigDirty()
        {
            if (_currentConfig == null || string.IsNullOrEmpty(_loadedConfigJson)) return false;
            return JsonUtility.ToJson(BuildConfigFromUI(_currentConfig)) != _loadedConfigJson;
        }

        /// <summary>
        /// 执行 RefreshUseButtonState 相关逻辑。
        /// </summary>
        private void RefreshUseButtonState(bool forceHide)
        {
            if (btnUse == null) return;
            bool isUsing = IsSelectedConfigUsing();
            bool showUse = _currentConfig != null && !forceHide;
            btnUse.gameObject.SetActive(showUse);
            btnUse.interactable = showUse && !isUsing;

            var text                    = btnUse.GetComponentInChildren<Text>(true);
            if (text != null) text.text = isUsing ? "正在使用" : "使用";
            RefreshUseButtonColor(isUsing);
        }

        /// <summary>
        /// 判断当前选中的配置文件是否就是当前平台正在使用的源配置。
        /// </summary>
        private bool IsSelectedConfigUsing()
        {
            return _currentConfig != null && string.Equals(_currentConfig.configId, _usingConfigId, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 刷新使用按钮颜色，正在使用时显示为绿色状态。
        /// </summary>
        private void RefreshUseButtonColor(bool isUsing)
        {
            if (btnUse == null || btnUse.targetGraphic == null) return;
            if (!_btnUseOriginColor.HasValue) _btnUseOriginColor = btnUse.targetGraphic.color;
            btnUse.targetGraphic.color = isUsing ? Color.green : _btnUseOriginColor.Value;
        }

        /// <summary>
        /// 刷新删除按钮显示状态。
        /// </summary>
        private void RefreshDeleteButtonState()
        {
            if (btnDeleteConfig == null) return;
            bool showDelete = _currentConfig != null && !_currentConfig.isDefault;
            btnDeleteConfig.gameObject.SetActive(showDelete);
            btnDeleteConfig.interactable = showDelete;
        }

        /// <summary>
        /// 执行 RefreshModifiedControlColors 相关逻辑。
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

            SetControlModified(drDisplay, _displayConfigAutoCorrected || current.displayIndex != baseConfig.displayIndex || current.captureDisplayName != baseConfig.captureDisplayName);
            SetControlModified(drUseMode, current.useMode != baseConfig.useMode);
            SetControlModified(ifVideoSavePath, current.videoSaveDirectory != baseConfig.videoSaveDirectory);
            SetControlModified(ifStreamUrl, current.streamUrl != baseConfig.streamUrl);
            SetControlModified(drStreamVideoBitrate, current.streamVideoBitrate != baseConfig.streamVideoBitrate);
            SetControlModified(drStreamGop, current.streamGop != baseConfig.streamGop);
            SetControlModified(drStreamBufferSize, current.streamBufferSize != baseConfig.streamBufferSize);
            SetControlModified(togStreamLowLatency, current.streamLowLatency != baseConfig.streamLowLatency);
            SetControlModified(togStreamAutoReconnect, current.streamAutoReconnect != baseConfig.streamAutoReconnect);
            SetControlModified(drStreamReconnectCount, current.streamReconnectCount != baseConfig.streamReconnectCount);
            SetControlModified(drStreamReconnectInterval, current.streamReconnectIntervalMs != baseConfig.streamReconnectIntervalMs);
            SetControlModified(togStreamIncludeAudio, current.streamIncludeAudio != baseConfig.streamIncludeAudio);
            SetControlModified(drVideoFormat, current.outputAsWebm != baseConfig.outputAsWebm);
            SetControlModified(ifVideoPrefix, current.outputFilePrefix != baseConfig.outputFilePrefix);
            SetControlModified(ifFfmpegPath, GetConfigFFmpegPath(current) != GetConfigFFmpegPath(baseConfig));
            SetControlModified(drAudioMode, current.audioMode != baseConfig.audioMode);
            SetControlModified(drAudioCoder, current.audioCodec != baseConfig.audioCodec || current.webmAudioCodec != baseConfig.webmAudioCodec);
            SetControlModified(drAudioBitrate, current.audioBitrate != baseConfig.audioBitrate);
            SetControlModified(drAudioSampleRate, current.audioSampleRate != baseConfig.audioSampleRate);
            SetControlModified(drAudioChannel, current.audioChannels != baseConfig.audioChannels);
            SetControlModified(drEnableAudioGain, current.enableAudioGain != baseConfig.enableAudioGain);
            SetControlModified(drAudioGainDb, !Mathf.Approximately(current.audioGainDb, baseConfig.audioGainDb));
            SetControlModified(drAudioLimiterEnabled, current.audioLimiterEnabled != baseConfig.audioLimiterEnabled);
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
        /// 执行 ClearModifiedControlColors 相关逻辑。
        /// </summary>
        private void ClearModifiedControlColors()
        {
            foreach (var item in _originGraphicColors)
            {
                if (item.Key != null) item.Key.color = item.Value;
            }
        }

        /// <summary>
        /// 执行 SetControlModified 相关逻辑。
        /// </summary>
        private void SetControlModified(Selectable selectable, bool isModified)
        {
            if (selectable == null || selectable.targetGraphic == null) return;
            var graphic = selectable.targetGraphic;
            if (!_originGraphicColors.ContainsKey(graphic)) _originGraphicColors.Add(graphic, graphic.color);
            graphic.color = isModified ? changedColor : _originGraphicColors[graphic];
        }

        /// <summary>
        /// 执行 RefreshParameterInteractable 相关逻辑。
        /// </summary>
        private void RefreshParameterInteractable()
        {
            bool canEdit           = _currentConfig != null;
            bool canEditFullParams = canEdit && !_currentConfig.isDefault;
            SetParameterInteractable(drDisplay, canEdit);
            SetParameterInteractable(drUseMode, canEdit);
            SetParameterInteractable(ifVideoSavePath, canEdit);
            SetParameterInteractable(btnSelectVideoSavePath, canEdit);
            SetParameterInteractable(ifStreamUrl, canEdit);
            SetParameterInteractable(drStreamVideoBitrate, canEdit);
            SetParameterInteractable(drStreamGop, canEdit);
            SetParameterInteractable(drStreamBufferSize, canEdit);
            SetParameterInteractable(togStreamLowLatency, canEdit);
            SetParameterInteractable(togStreamAutoReconnect, canEdit);
            SetParameterInteractable(drStreamReconnectCount, canEdit);
            SetParameterInteractable(drStreamReconnectInterval, canEdit);
            SetParameterInteractable(togStreamIncludeAudio, canEdit);
            SetParameterInteractable(drVideoFormat, canEdit);
            SetParameterInteractable(ifVideoPrefix, canEdit);
            SetParameterInteractable(ifFfmpegPath, canEdit);
            SetParameterInteractable(drAudioMode, canEditFullParams);
            SetParameterInteractable(drAudioCoder, canEditFullParams);
            SetParameterInteractable(drAudioBitrate, canEditFullParams);
            SetParameterInteractable(drAudioSampleRate, canEditFullParams);
            SetParameterInteractable(drAudioChannel, canEditFullParams);
            SetParameterInteractable(drEnableAudioGain, canEditFullParams);
            SetParameterInteractable(drAudioGainDb, canEditFullParams);
            SetParameterInteractable(drAudioLimiterEnabled, canEditFullParams);
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
        /// 执行 SetParameterInteractable 相关逻辑。
        /// </summary>
        private void SetParameterInteractable(Selectable selectable, bool canEdit)
        {
            if (selectable == null) return;
            if (!_originSelectableStates.ContainsKey(selectable)) _originSelectableStates.Add(selectable, selectable.interactable);
            selectable.interactable = canEdit && _originSelectableStates[selectable];
        }

        /// <summary>
        /// 执行 BindDropdown 相关逻辑。
        /// </summary>
        private static void BindDropdown(Dropdown dropdown, UnityEngine.Events.UnityAction<int> action)
        {
            if (dropdown == null) return;
            dropdown.onValueChanged.RemoveListener(action);
            dropdown.onValueChanged.AddListener(action);
        }

        /// <summary>
        /// 执行 SetDropdownOptions 相关逻辑。
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
        /// 设置下拉选项并尽量保留当前值，当前值不合法时使用默认值。
        /// </summary>
        private static void SetDropdownOptionsKeepValue(Dropdown dropdown, List<string> options, string defaultValue)
        {
            if (dropdown == null) return;
            string oldValue = GetDropdownText(dropdown);
            dropdown.ClearOptions();
            dropdown.AddOptions(options);
            SetDropdownValue(dropdown, options.Contains(oldValue) ? oldValue : defaultValue);
        }

        /// <summary>
        /// 设置控件根节点显示状态。
        /// </summary>
        private static void SetControlActive(Selectable selectable, bool isActive)
        {
            if (selectable == null) return;
            var parent = selectable.transform.parent;
            if (parent != null) parent.gameObject.SetActive(isActive);
            else selectable.gameObject.SetActive(isActive);
        }

        /// <summary>
        /// 设置控件所在行或控件自身的显示状态。
        /// </summary>
        private static void SetControlRootActive(GameObject root, Selectable selectable, bool isActive)
        {
            if (root != null) root.SetActive(isActive);
            else SetControlActive(selectable, isActive);
        }

        /// <summary>
        /// 执行 SetDropdownValue 相关逻辑。
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
        /// 执行 getDropdownText 相关逻辑。
        /// </summary>
        private static string GetDropdownText(Dropdown dropdown)
        {
            if (dropdown == null || dropdown.options.Count == 0 || dropdown.value < 0 || dropdown.value >= dropdown.options.Count) return string.Empty;
            return dropdown.options[dropdown.value].text;
        }

        /// <summary>
        /// 执行 getDropdownTextOrDefault 相关逻辑。
        /// </summary>
        private static string GetDropdownTextOrDefault(Dropdown dropdown, string defaultValue)
        {
            string value = GetDropdownText(dropdown);
            return string.IsNullOrWhiteSpace(value) ? defaultValue : value;
        }

        /// <summary>
        /// 判断“使用 / 不使用”下拉框是否选择使用。
        /// </summary>
        private static bool IsUseOptionSelected(Dropdown dropdown)
        {
            return string.Equals(GetDropdownText(dropdown), AUDIO_OPTION_USE, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 获取当前选择的录制音量增益预设。
        /// </summary>
        private float GetSelectedAudioGainDb()
        {
            int index = drAudioGainDb == null ? Array.IndexOf(AudioGainPresetValues, 0f) : Mathf.Clamp(drAudioGainDb.value, 0, AudioGainPresetValues.Length - 1);
            return AudioGainPresetValues[Mathf.Max(0, index)];
        }

        /// <summary>
        /// 将配置中的增益值映射到最近的下拉框预设。
        /// </summary>
        private static string GetNearestAudioGainLabel(float value)
        {
            float clamped = Mathf.Clamp(value, -20f, 20f);
            int nearestIndex = Array.IndexOf(AudioGainPresetValues, 0f);
            float nearestDistance = float.MaxValue;
            for (int i = 0; i < AudioGainPresetValues.Length; i++)
            {
                float distance = Mathf.Abs(AudioGainPresetValues[i] - clamped);
                if (distance >= nearestDistance) continue;
                nearestDistance = distance;
                nearestIndex = i;
            }

            return AudioGainPresetLabels[Mathf.Clamp(nearestIndex, 0, AudioGainPresetLabels.Count - 1)];
        }

        /// <summary>
        /// 执行 ParseInt 相关逻辑。
        /// </summary>
        private static int ParseInt(string value, int defaultValue) => int.TryParse(value, out int result) ? result : defaultValue;

        /// <summary>
        /// 执行 ParseFloat 相关逻辑。
        /// </summary>
        private static float ParseFloat(string value, float defaultValue) => float.TryParse(value, out float result) ? result : defaultValue;
    }
}

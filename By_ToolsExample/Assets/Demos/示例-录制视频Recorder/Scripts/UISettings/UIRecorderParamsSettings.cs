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
    using UnityEditor;
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
        private string _usingConfigFileName;
        private Coroutine _saveAsTipsCoroutine;
        private GameObject _messageWindowRoot;
        private Text _txtMessageContent;
        private Button _btnMessageConfirm;
        private Color? _btnUseOriginColor;
        private bool _isRefreshingUI;
        private bool _displayConfigAutoCorrected;
        private const int USE_MODE_LOCAL = 0;
        private const int USE_MODE_STREAM = 1;
        private const string USE_MODE_LOCAL_NAME = "存储本地";
        private const string USE_MODE_STREAM_NAME = "视频推流";

        /// <summary>
        /// 执行 Start 相关逻辑。
        /// </summary>
        private void Start()
        {
            InitUI();
            BindEvents();
        }

        /// <summary>
        /// 执行 InitUI 相关逻辑。
        /// </summary>
        private void InitUI()
        {
            if (_recorder == null) _recorder = CrossPlatformScreenRecorder.ins;
            NormalizeFolderSettings();
            ResolveOptionalUIReferences();
            EnsureStreamUrlInput();
            EnsureStreamSettingsControls();
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
                SceneManager.UnloadSceneAsync("录制视频Recorder");
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
            RefreshEditState();
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
            RefreshEditState();
            RefreshParameterInteractable();
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
            bool   invalidName        = string.IsNullOrWhiteSpace(config.displayName) || !string.Equals(config.displayName, currentDisplayName, StringComparison.OrdinalIgnoreCase);
            if (!invalidIndex && !invalidName) return false;

            config.displayIndex = 0;
            config.displayName  = drDisplay.options[0].text;
            Debug.LogWarning($"录制配置显示屏与当前电脑不一致，已回落到第一个显示屏: {config.displayName}");
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
        /// 按常见节点名自动查找新加入的可选 UI 控件。
        /// </summary>
        private void ResolveOptionalUIReferences()
        {
            foreach (var item in GetComponentsInChildren<Transform>(true))
            {
                string n                                                                                                              = item.name;
                if (drUseMode == null && (n == "DrUseMode" || n.Contains("使用方式"))) drUseMode                                          = item.GetComponent<Dropdown>();
                if (goVideoSavePathRoot == null && n == "VideoSavePathRoot") goVideoSavePathRoot                                      = item.gameObject;
                if (ifVideoSavePath == null && (n == "IfVideoSavePath" || n == "InputVideoSavePath")) ifVideoSavePath                 = item.GetComponent<InputField>();
                if (btnSelectVideoSavePath == null && (n == "BtnSelectVideoSavePath" || n.Contains("选择保存路径"))) btnSelectVideoSavePath = item.GetComponent<Button>();
                if (goStreamUrlRoot == null && (n == "StreamUrlRoot" || n.Contains("推流地址"))) goStreamUrlRoot                          = item.gameObject;
                if (ifStreamUrl == null && (n == "IfStreamUrl" || n == "InputStreamUrl" || n.Contains("推流地址"))) ifStreamUrl           = item.GetComponent<InputField>();
            }

            if (goVideoSavePathRoot == null && ifVideoSavePath != null && ifVideoSavePath.transform.parent != null) goVideoSavePathRoot = ifVideoSavePath.transform.parent.gameObject;
            if (goStreamUrlRoot == null && ifStreamUrl != null && ifStreamUrl.transform.parent != null) goStreamUrlRoot                 = ifStreamUrl.transform.parent.gameObject;
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
            config.displayName                = GetSelectedDisplayName();
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
                string savedFileName = _currentConfig.fileName;
                if (wasUsingSelectedConfig)
                {
                    ApplyConfigToRecorder(_currentConfig);
                    _usingConfigFileName = savedFileName;
                }

                _loadedConfigJson = JsonUtility.ToJson(_currentConfig);
                RefreshConfigDropdown();
                SelectConfigByFileName(savedFileName);
                RefreshEditState();
                ShowMessageTips("保存成功");
                Debug.Log("已保存录制配置: " + _currentConfig.configName);
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
                ifSaveAsName.text = Path.GetFileNameWithoutExtension(_currentConfig.fileName);
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
            _usingConfigFileName = _currentConfig.fileName;
            _loadedConfigJson    = JsonUtility.ToJson(_currentConfig);
            RefreshEditState();
            Debug.Log("已使用录制配置: " + _currentConfig.configName);
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
            string fileName                                       = BuildUserConfigFileName(GetCurrentPlatformName(), configName);
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
            SaveCurrentRecordConfig(config);
            ApplyConfigToRecorder(config);
            _usingConfigFileName = config.fileName;
            _loadedConfigJson    = JsonUtility.ToJson(BuildConfigFromUI(config));
            HideSaveAsWindow();
            RefreshConfigDropdown();
            SelectConfigByFileName(config.fileName);
            RefreshEditState();
            Debug.Log("已另存为录制配置: " + config.configName);
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

            string fileName = _currentConfig.fileName;
            string path     = Path.Combine(GetConfigDirectory(), fileName);
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
                var fallback = CreateFallbackRecordReference();
                File.WriteAllText(GetUseRecordConfigPath(), JsonUtility.ToJson(fallback, true));
                _usingConfigFileName = fallback.fileName;
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
            if (goVideoSavePathRoot != null) goVideoSavePathRoot.SetActive(showLocalPath);
            else if (ifVideoSavePath != null) ifVideoSavePath.gameObject.SetActive(showLocalPath);
            if (btnSelectVideoSavePath != null) btnSelectVideoSavePath.gameObject.SetActive(showLocalPath);
            if (goStreamUrlRoot != null) goStreamUrlRoot.SetActive(showStreamUrl);
            else if (ifStreamUrl != null) ifStreamUrl.gameObject.SetActive(showStreamUrl);
            if (goStreamSettingsRoot != null) goStreamSettingsRoot.SetActive(showStreamUrl);
            SetControlActive(drVideoFormat, showLocalPath);
            SetControlActive(videoCrf, showLocalPath);
            SetControlActive(mergeTimeoutMs, showLocalPath);
            SetControlActive(deleteTempFilesAfterMerge, showLocalPath);
            SetControlActive(stopVideoTimeoutMs, showLocalPath);
            SetControlActive(waitTempFileReadyTimeoutMs, showLocalPath);
            RefreshFormatOptionVisibility();
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
            return _currentConfig != null && string.Equals(_currentConfig.fileName, _usingConfigFileName, StringComparison.OrdinalIgnoreCase);
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

            SetControlModified(drDisplay, _displayConfigAutoCorrected || current.displayIndex != baseConfig.displayIndex || current.displayName != baseConfig.displayName);
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
            if (selectable != null) selectable.gameObject.transform.parent.gameObject.SetActive(isActive);
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
        /// 执行 ParseInt 相关逻辑。
        /// </summary>
        private static int ParseInt(string value, int defaultValue) => int.TryParse(value, out int result) ? result : defaultValue;

        /// <summary>
        /// 执行 ParseFloat 相关逻辑。
        /// </summary>
        private static float ParseFloat(string value, float defaultValue) => float.TryParse(value, out float result) ? result : defaultValue;
    }
}
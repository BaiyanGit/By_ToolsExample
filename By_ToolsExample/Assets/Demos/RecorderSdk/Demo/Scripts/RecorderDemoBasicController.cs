//=====================================================
// 文件名称: RecorderDemoBasicController
// 创 建 者: wangbaiyan
// 创建日期: 2026-05-25
// 描    述: Recorder SDK 基础 Demo 控制器，只负责绑定场景 UI、调用 SDK 和刷新状态。
//=====================================================

namespace Demos.示例_录制视频Recorder.Scripts.Core
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using Demos.示例_录制视频Recorder.Scripts.UISettings;
    using UnityEngine;
    using UnityEngine.UI;
    using Debug = UnityEngine.Debug;

    /// <summary>
    /// Recorder SDK 基础使用示例控制器。
    /// </summary>
    public class RecorderDemoBasicController : MonoBehaviour
    {
        [Header("Recorder")]
        [Tooltip("录制器实例。")]
        public CrossPlatformScreenRecorder recorder;

        [Header("兼容设置")]
        [SerializeField, Tooltip("Legacy 兼容开关。默认关闭，Demo UI 应提前存在于场景中。")]
        private bool createUiAtRuntime = false;

        [Header("基础按钮")]
        [Tooltip("开始录制按钮。")]
        public Button btnStartRecording;
        [Tooltip("停止录制按钮。")]
        public Button btnStopRecording;
        [Tooltip("打开输出目录按钮。")]
        public Button btnOpenOutputFolder;
        [Tooltip("清空会话历史按钮。")]
        public Button btnClearSessionHistory;

        [Header("状态文本")]
        [Tooltip("当前状态文本。")]
        public Text txtCurrentState;
        [Tooltip("最近结果文本。")]
        public Text txtLastResult;
        [Tooltip("当前配置 ID 文本。")]
        public Text txtCurrentConfigId;
        [Tooltip("输出路径文本。")]
        public Text txtOutputPath;
        [Tooltip("最近错误码文本。")]
        public Text txtLastErrorCode;
        [Tooltip("会话数量文本。")]
        public Text txtSessionCount;

        [Header("简单配置入口")]
        [Tooltip("配置 ID 下拉框。")]
        public Dropdown drConfigId;
        [Tooltip("是否录制音频。")]
        public Toggle togEnableAudio;
        [Tooltip("是否启用录制音量增强。")]
        public Toggle togEnableAudioGain;
        [Tooltip("录制音量增益滑动条。")]
        public Slider slAudioGainDb;
        [Tooltip("录制音量增益文本。")]
        public Text txtAudioGainDb;

        private readonly List<RecorderParamsConfig> _dropdownConfigs = new List<RecorderParamsConfig>();
        private RecorderConfigRegistry _registry;
        private RecorderParamsConfig _currentConfig;
        private RecorderResult _lastResult;
        private RecorderErrorCode _lastErrorCode = RecorderErrorCode.None;
        private bool _isRefreshingUI;

        /// <summary>
        /// 初始化 Demo 绑定、配置和 Recorder 事件。
        /// </summary>
        private void Awake()
        {
            recorder = recorder != null ? recorder : GetComponent<CrossPlatformScreenRecorder>();
            if (recorder == null) recorder = CrossPlatformScreenRecorder.ins;
            if (recorder == null) recorder = gameObject.AddComponent<CrossPlatformScreenRecorder>();

            if (!ValidateUIBindings())
            {
                Debug.LogWarning("RecorderDemoBasicController: Demo UI 未完整绑定。请在场景中提前放置 UI，或执行菜单 ByTools/🔴Recorder SDK/UI创建/基础演示 后保存场景。");
                if (createUiAtRuntime)
                {
                    Debug.LogWarning("RecorderDemoBasicController: createUiAtRuntime 已保留为 Legacy 兼容开关，但当前版本不再在运行时动态创建 Demo UI。");
                }
            }

            BindUIEvents();
            LoadConfigs();
            SubscribeRecorderEvents();
            RefreshStatus("Ready");
        }

        /// <summary>
        /// 释放事件订阅。
        /// </summary>
        private void OnDestroy()
        {
            UnsubscribeRecorderEvents();
        }

        /// <summary>
        /// 检查基础 Demo UI 是否已经在场景中绑定。
        /// </summary>
        private bool ValidateUIBindings()
        {
            bool hasButtons = btnStartRecording != null && btnStopRecording != null && btnOpenOutputFolder != null && btnClearSessionHistory != null;
            bool hasTexts = txtCurrentState != null && txtLastResult != null && txtCurrentConfigId != null && txtOutputPath != null && txtLastErrorCode != null && txtSessionCount != null;
            bool hasSimpleConfig = drConfigId != null && togEnableAudio != null && togEnableAudioGain != null && slAudioGainDb != null && txtAudioGainDb != null;
            return hasButtons && hasTexts && hasSimpleConfig;
        }

        /// <summary>
        /// 绑定 Demo UI 事件。
        /// </summary>
        private void BindUIEvents()
        {
            if (btnStartRecording != null)
            {
                btnStartRecording.onClick.RemoveListener(StartRecording);
                btnStartRecording.onClick.AddListener(StartRecording);
            }

            if (btnStopRecording != null)
            {
                btnStopRecording.onClick.RemoveListener(StopRecording);
                btnStopRecording.onClick.AddListener(StopRecording);
            }

            if (btnOpenOutputFolder != null)
            {
                btnOpenOutputFolder.onClick.RemoveListener(OpenOutputFolder);
                btnOpenOutputFolder.onClick.AddListener(OpenOutputFolder);
            }

            if (btnClearSessionHistory != null)
            {
                btnClearSessionHistory.onClick.RemoveListener(ClearSessionHistory);
                btnClearSessionHistory.onClick.AddListener(ClearSessionHistory);
            }

            if (drConfigId != null)
            {
                drConfigId.onValueChanged.RemoveListener(ChangeConfigId);
                drConfigId.onValueChanged.AddListener(ChangeConfigId);
            }

            if (togEnableAudio != null)
            {
                togEnableAudio.onValueChanged.RemoveListener(SetEnableAudio);
                togEnableAudio.onValueChanged.AddListener(SetEnableAudio);
            }

            if (togEnableAudioGain != null)
            {
                togEnableAudioGain.onValueChanged.RemoveListener(SetEnableAudioGain);
                togEnableAudioGain.onValueChanged.AddListener(SetEnableAudioGain);
            }

            if (slAudioGainDb != null)
            {
                slAudioGainDb.minValue = -20f;
                slAudioGainDb.maxValue = 20f;
                slAudioGainDb.onValueChanged.RemoveListener(SetAudioGainDb);
                slAudioGainDb.onValueChanged.AddListener(SetAudioGainDb);
            }
        }

        /// <summary>
        /// 扫描配置并刷新下拉框。
        /// </summary>
        private void LoadConfigs()
        {
            _registry = new RecorderConfigRegistry();
            _registry.Scan();
            var current = _registry.GetCurrentConfig();
            _currentConfig = current.config;
            _dropdownConfigs.Clear();

            if (drConfigId != null)
            {
                drConfigId.ClearOptions();
                var options = new List<string>();
                int selectedIndex = 0;
                for (int i = 0; i < _registry.Configs.Count; i++)
                {
                    var config = _registry.Configs[i];
                    _dropdownConfigs.Add(config);
                    options.Add(string.IsNullOrWhiteSpace(config.displayName) ? config.configId : $"{config.displayName} ({config.configId})");
                    if (_currentConfig != null && config.configId == _currentConfig.configId) selectedIndex = i;
                }

                drConfigId.AddOptions(options);
                drConfigId.SetValueWithoutNotify(selectedIndex);
            }

            ApplyConfigToSimpleUI();
        }

        /// <summary>
        /// 开始录制。
        /// </summary>
        private async void StartRecording()
        {
            ApplySimpleConfigChanges();
            _lastResult = await recorder.StartRecordingAsync();
            _lastErrorCode = _lastResult.errorCode;
            RefreshStatus(_lastResult.message);
        }

        /// <summary>
        /// 停止录制。
        /// </summary>
        private async void StopRecording()
        {
            _lastResult = await recorder.StopRecordingAsync();
            _lastErrorCode = _lastResult.errorCode;
            RefreshStatus(_lastResult.message);
        }

        /// <summary>
        /// 打开输出目录。
        /// </summary>
        private void OpenOutputFolder()
        {
            string outputPath = recorder.CurrentOutputFilePath;
            if (string.IsNullOrWhiteSpace(outputPath)) outputPath = _lastResult?.outputPath ?? string.Empty;
            string directory = !string.IsNullOrWhiteSpace(outputPath) ? Path.GetDirectoryName(outputPath) : RecorderPathService.GetDefaultVideoDirectory();
            if (string.IsNullOrWhiteSpace(directory) || !Directory.Exists(directory))
            {
                RefreshStatus("输出目录不存在。");
                return;
            }

            try
            {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
                Process.Start("explorer.exe", directory.Replace("/", "\\"));
#elif UNITY_STANDALONE_LINUX || UNITY_EDITOR_LINUX
                Process.Start("xdg-open", directory);
#endif
            }
            catch (System.Exception e)
            {
                RefreshStatus("打开输出目录失败: " + e.Message);
            }
        }

        /// <summary>
        /// 清空 SessionHistory。
        /// </summary>
        private void ClearSessionHistory()
        {
            recorder.ClearSessionHistory();
            RefreshStatus("SessionHistory 已清空。");
        }

        /// <summary>
        /// 切换当前配置 ID。
        /// </summary>
        private void ChangeConfigId(int index)
        {
            if (_isRefreshingUI || _registry == null || index < 0 || index >= _dropdownConfigs.Count) return;
            var config = _dropdownConfigs[index];
            var result = _registry.SetCurrentConfig(config.configId);
            _currentConfig = result.config;
            recorder.ReloadConfig();
            ApplyConfigToSimpleUI();
            RefreshStatus(result.message);
        }

        /// <summary>
        /// 切换是否录制系统音频。
        /// </summary>
        private void SetEnableAudio(bool value)
        {
            if (_isRefreshingUI || _currentConfig == null) return;
            _currentConfig.audioMode = value ? 1 : 0;
            SaveCurrentConfig();
        }

        /// <summary>
        /// 切换是否启用录制音量增强。
        /// </summary>
        private void SetEnableAudioGain(bool value)
        {
            if (_isRefreshingUI || _currentConfig == null) return;
            _currentConfig.enableAudioGain = value;
            if (value && Mathf.Approximately(_currentConfig.audioGainDb, 0f)) _currentConfig.audioGainDb = 6f;
            SaveCurrentConfig();
            ApplyConfigToSimpleUI();
        }

        /// <summary>
        /// 设置录制音量增益。
        /// </summary>
        private void SetAudioGainDb(float value)
        {
            if (_isRefreshingUI || _currentConfig == null) return;
            _currentConfig.audioGainDb = Mathf.Clamp(value, -20f, 20f);
            SaveCurrentConfig();
            RefreshGainText();
        }

        /// <summary>
        /// 保存当前简单配置。
        /// </summary>
        private void SaveCurrentConfig()
        {
            if (_registry == null || _currentConfig == null) return;
            _registry.SaveConfig(_currentConfig);
            _registry.SetCurrentConfig(_currentConfig.configId);
            recorder.ReloadConfig();
        }

        /// <summary>
        /// 启动前同步简单配置改动。
        /// </summary>
        private void ApplySimpleConfigChanges()
        {
            if (_currentConfig == null) return;
            SaveCurrentConfig();
        }

        /// <summary>
        /// 将当前配置同步到简单 UI。
        /// </summary>
        private void ApplyConfigToSimpleUI()
        {
            _isRefreshingUI = true;
            if (_currentConfig != null)
            {
                if (togEnableAudio != null) togEnableAudio.SetIsOnWithoutNotify(_currentConfig.audioMode == 1);
                if (togEnableAudioGain != null) togEnableAudioGain.SetIsOnWithoutNotify(_currentConfig.enableAudioGain);
                if (slAudioGainDb != null) slAudioGainDb.SetValueWithoutNotify(Mathf.Clamp(_currentConfig.audioGainDb, -20f, 20f));
            }

            RefreshGainText();
            _isRefreshingUI = false;
        }

        /// <summary>
        /// 订阅 Recorder 事件。
        /// </summary>
        private void SubscribeRecorderEvents()
        {
            if (recorder == null) return;
            recorder.OnRecorderStateChanged += OnRecorderEvent;
            recorder.OnRecorderWarning += OnRecorderEvent;
            recorder.OnRecorderError += OnRecorderEvent;
            recorder.OnRecorderProgress += OnRecorderEvent;
            recorder.OnConfigChanged += OnRecorderEvent;
        }

        /// <summary>
        /// 取消订阅 Recorder 事件。
        /// </summary>
        private void UnsubscribeRecorderEvents()
        {
            if (recorder == null) return;
            recorder.OnRecorderStateChanged -= OnRecorderEvent;
            recorder.OnRecorderWarning -= OnRecorderEvent;
            recorder.OnRecorderError -= OnRecorderEvent;
            recorder.OnRecorderProgress -= OnRecorderEvent;
            recorder.OnConfigChanged -= OnRecorderEvent;
        }

        /// <summary>
        /// 处理 Recorder 事件。
        /// </summary>
        private void OnRecorderEvent(RecorderEventArgs args)
        {
            if (args != null && args.errorCode != RecorderErrorCode.None) _lastErrorCode = args.errorCode;
            RefreshStatus(args?.message ?? string.Empty);
        }

        /// <summary>
        /// 刷新状态文本。
        /// </summary>
        private void RefreshStatus(string message)
        {
            if (txtCurrentState != null) txtCurrentState.text = "当前状态：" + recorder.State;
            if (txtLastResult != null) txtLastResult.text = "最近结果：" + (string.IsNullOrWhiteSpace(message) ? "-" : message);
            if (txtCurrentConfigId != null) txtCurrentConfigId.text = "当前配置ID：" + (_currentConfig?.configId ?? "-");
            string outputPath = !string.IsNullOrWhiteSpace(recorder.CurrentOutputFilePath) ? recorder.CurrentOutputFilePath : (_lastResult?.outputPath ?? string.Empty);
            if (txtOutputPath != null) txtOutputPath.text = "输出路径：" + (string.IsNullOrWhiteSpace(outputPath) ? "-" : outputPath);
            if (txtLastErrorCode != null) txtLastErrorCode.text = "最近错误码：" + _lastErrorCode;
            if (txtSessionCount != null) txtSessionCount.text = "会话数量：" + recorder.SessionHistory.Count;
        }

        /// <summary>
        /// 刷新增益数值文本。
        /// </summary>
        private void RefreshGainText()
        {
            float gain = _currentConfig == null ? 0f : Mathf.Clamp(_currentConfig.audioGainDb, -20f, 20f);
            if (txtAudioGainDb != null) txtAudioGainDb.text = $"{gain:0.###} dB";
        }
    }
}

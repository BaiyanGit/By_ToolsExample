//=====================================================
// 文件名称: RecorderDemoBasicController
// 创 建 者: wangbaiyan
// 创建日期: 2026-05-25
// 描    述: Recorder SDK 屏幕录制 Demo 控制器，只负责绑定场景 UI、调用 SDK 和刷新状态。
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

        [Header("已停用的旧音频控件")]
        [SerializeField, HideInInspector, Tooltip("旧版屏幕录制音频开关。当前音频参数统一由配置文件控制。")]
        public Toggle togEnableAudio;
        [SerializeField, HideInInspector, Tooltip("旧版屏幕录制音量增强开关。当前音频参数统一由配置文件控制。")]
        public Toggle togEnableAudioGain;
        [SerializeField, HideInInspector, Tooltip("旧版屏幕录制音量增益滑动条。当前音频参数统一由配置文件控制。")]
        public Slider slAudioGainDb;
        [SerializeField, HideInInspector, Tooltip("旧版屏幕录制音量增益文本。当前音频参数统一由配置文件控制。")]
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
                Debug.LogWarning("RecorderDemoBasicController: Demo UI 未完整绑定。请在场景中提前放置 UI，或执行菜单 ByTools/🔴Recorder SDK/UI创建/屏幕录制 后保存场景。");
                if (createUiAtRuntime)
                {
                    Debug.LogWarning("RecorderDemoBasicController: createUiAtRuntime 已保留为 Legacy 兼容开关，但当前版本不再在运行时动态创建 Demo UI。");
                }
            }

            HideObsoleteAudioControls();
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
        /// 检查屏幕录制 Demo UI 是否已经在场景中绑定。
        /// </summary>
        private bool ValidateUIBindings()
        {
            bool hasButtons = btnStartRecording != null && btnStopRecording != null && btnOpenOutputFolder != null && btnClearSessionHistory != null;
            bool hasTexts = txtCurrentState != null && txtLastResult != null && txtCurrentConfigId != null && txtOutputPath != null && txtLastErrorCode != null && txtSessionCount != null;
            bool hasSimpleConfig = drConfigId != null;
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

            UnbindObsoleteAudioEvents();
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
                    options.Add(GetConfigDropdownLabel(config));
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
            _lastResult = await recorder.StartRecordingAsync();
            _lastErrorCode = _lastResult.errorCode;
            RefreshStatus(_lastResult.message);
        }

        private static string GetConfigDropdownLabel(RecorderParamsConfig config)
        {
            if (config == null) return "Unnamed Config";
            if (!string.IsNullOrWhiteSpace(config.displayName)) return config.displayName;
            return string.IsNullOrWhiteSpace(config.configId) ? "Unnamed Config" : config.configId;
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
        /// 将当前配置同步到简单 UI。
        /// </summary>
        private void ApplyConfigToSimpleUI()
        {
            _isRefreshingUI = true;
            HideObsoleteAudioControls();
            _isRefreshingUI = false;
        }

        /// <summary>
        /// 隐藏旧版屏幕录制里的音频快捷控件，音频参数统一由配置文件控制。
        /// </summary>
        private void HideObsoleteAudioControls()
        {
            SetObsoleteControlActive(togEnableAudio, false);
            SetObsoleteControlActive(togEnableAudioGain, false);
            SetObsoleteControlActive(slAudioGainDb, false);
            SetObsoleteControlActive(txtAudioGainDb, false);
            HideObsoleteObjectByName("启用音频开关");
            HideObsoleteObjectByName("启用音量增强开关");
            HideObsoleteObjectByName("音量增益滑动条");
            HideObsoleteObjectByName("音量增益文本");
            UnbindObsoleteAudioEvents();
        }

        /// <summary>
        /// 解除旧版音频控件事件，避免屏幕录制覆盖配置文件。
        /// </summary>
        private void UnbindObsoleteAudioEvents()
        {
            if (togEnableAudio != null) togEnableAudio.onValueChanged.RemoveAllListeners();
            if (togEnableAudioGain != null) togEnableAudioGain.onValueChanged.RemoveAllListeners();
            if (slAudioGainDb != null) slAudioGainDb.onValueChanged.RemoveAllListeners();
        }

        private static void SetObsoleteControlActive(Selectable selectable, bool active)
        {
            if (selectable == null) return;
            selectable.gameObject.SetActive(active);
        }

        private static void SetObsoleteControlActive(Graphic graphic, bool active)
        {
            if (graphic == null) return;
            graphic.gameObject.SetActive(active);
        }

        private static void HideObsoleteObjectByName(string objectName)
        {
            var target = GameObject.Find(objectName);
            if (target != null) target.SetActive(false);
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

    }
}

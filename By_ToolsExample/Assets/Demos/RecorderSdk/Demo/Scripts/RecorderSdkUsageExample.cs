//=====================================================
// 文件名称: RecorderSdkUsageExample
// 创 建 者: wangbaiyan
// 创建日期: 2026-05-23
// 描    述: 演示 Recorder SDK 的最小调用方式，包括事件订阅、配置切换、开始停止录制和读取历史。
//=====================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Demos.示例_录制视频Recorder.Scripts.Core
{
    /// <summary>
    /// Recorder SDK 最小使用示例。
    /// </summary>
    public class RecorderSdkUsageExample : MonoBehaviour
    {
        [Header("录制器")] public CrossPlatformScreenRecorder recorder;
        [Header("配置ID")] public string configId;

        private RecorderConfigRegistry _configRegistry;

        /// <summary>
        /// 初始化示例并订阅事件。
        /// </summary>
        private void Awake()
        {
            if (recorder == null) recorder = CrossPlatformScreenRecorder.ins;
            _configRegistry = new RecorderConfigRegistry();
            SubscribeRecorderEvents();
        }

        /// <summary>
        /// 退出时取消事件订阅。
        /// </summary>
        private void OnDestroy()
        {
            UnsubscribeRecorderEvents();
        }

        /// <summary>
        /// 示例：切换配置并开始录制。
        /// </summary>
        public async void StartExample()
        {
            RecorderResult result = await StartExampleAsync();
            Debug.Log($"[RecorderSdkUsageExample] Start success={result.success}, error={result.errorCodeText}, session={result.sessionId}, message={result.message}");
        }

        /// <summary>
        /// 示例：停止录制并读取 SessionHistory。
        /// </summary>
        public async void StopExample()
        {
            RecorderResult result = await StopExampleAsync();
            Debug.Log($"[RecorderSdkUsageExample] Stop success={result.success}, error={result.errorCodeText}, output={result.outputPath}, message={result.message}");

            IReadOnlyList<RecorderSessionInfo> history = recorder != null ? recorder.GetSessionHistory() : null;
            if (history == null) return;

            foreach (var item in history)
            {
                Debug.Log($"[RecorderSdkUsageExample] History {item.eventType}, state={item.state}, error={item.errorCodeText}, session={item.sessionId}, output={item.outputPath}");
            }
        }

        /// <summary>
        /// 可 await 的开始录制示例。
        /// </summary>
        public async Task<RecorderResult> StartExampleAsync()
        {
            if (recorder == null) return RecorderResult.Failed(RecorderErrorCode.InitializeFailed, "Recorder 未绑定。");

            string targetConfigId = string.IsNullOrWhiteSpace(configId) ? GetPlatformMediumConfigId() : configId;
            RecorderConfigResult configResult = _configRegistry.SetCurrentConfig(targetConfigId);
            if (!configResult.success)
            {
                return RecorderResult.Failed(configResult.errorCode, configResult.message);
            }

            return await recorder.StartRecordingAsync();
        }

        /// <summary>
        /// 可 await 的停止录制示例。
        /// </summary>
        public async Task<RecorderResult> StopExampleAsync()
        {
            if (recorder == null) return RecorderResult.Failed(RecorderErrorCode.InitializeFailed, "Recorder 未绑定。");
            return await recorder.StopRecordingAsync();
        }

        /// <summary>
        /// 清空录制历史。
        /// </summary>
        public void ClearHistory()
        {
            recorder?.ClearSessionHistory();
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
            recorder.OnMergeStarted += OnRecorderEvent;
            recorder.OnMergeCompleted += OnRecorderEvent;
            recorder.OnRecordStarted += OnRecordStarted;
            recorder.OnRecordStopped += OnRecordStopped;
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
            recorder.OnMergeStarted -= OnRecorderEvent;
            recorder.OnMergeCompleted -= OnRecorderEvent;
            recorder.OnRecordStarted -= OnRecordStarted;
            recorder.OnRecordStopped -= OnRecordStopped;
        }

        /// <summary>
        /// 接收统一 Recorder 事件。
        /// </summary>
        private void OnRecorderEvent(RecorderEventArgs args)
        {
            Debug.Log($"[RecorderSdkUsageExample] Event={args.eventType}, state={args.state}, error={args.errorCodeText}, session={args.sessionId}, message={args.message}");
        }

        /// <summary>
        /// 接收旧兼容开始事件。
        /// </summary>
        private void OnRecordStarted(string outputPath)
        {
            Debug.Log("[RecorderSdkUsageExample] OnRecordStarted: " + outputPath);
        }

        /// <summary>
        /// 接收旧兼容停止事件。
        /// </summary>
        private void OnRecordStopped(string outputPath)
        {
            Debug.Log("[RecorderSdkUsageExample] OnRecordStopped: " + outputPath);
        }

        /// <summary>
        /// 获取当前平台默认中配置 ID。
        /// </summary>
        private static string GetPlatformMediumConfigId()
        {
            bool isLinux = Application.platform == RuntimePlatform.LinuxEditor || Application.platform == RuntimePlatform.LinuxPlayer;
            return isLinux ? "template_linux_medium" : "template_win_medium";
        }
    }
}

//=====================================================
// 文件名称: RecorderSession
// 创 建 者: wangbaiyan
// 创建日期: 2026-05-23
// 描    述: 保存一次录制或推流会话的路径、音频状态和配置摘要。
//=====================================================

using System;

namespace Demos.示例_录制视频Recorder.Scripts.Core
{
    /// <summary>
    /// 一次录制会话的运行时数据。
    /// </summary>
    [Serializable]
    public class RecorderSession
    {
        public string sessionId;
        public DateTime startTime;
        public string outputPath;
        public string videoTempPath;
        public string audioTempPath;
        public bool containsSystemAudio;
        public bool isStreaming;
        public string resolvedLinuxAudioSource;
        public string configId;
        public string configName;
        public string displayName;
        public bool audioGainEnabled;
        public float audioGainDb;

        /// <summary>
        /// 转换成外部只读会话信息。
        /// </summary>
        public RecorderSessionInfo ToInfo(
            RecorderState state,
            RecorderSessionEventType eventType = RecorderSessionEventType.None,
            RecorderErrorCode errorCode = RecorderErrorCode.None,
            string message = "",
            DateTime? endedAt = null,
            bool isMerged = false,
            string mergeOutputPath = "")
        {
            DateTime safeEndedAt = endedAt ?? default;
            return new RecorderSessionInfo
            {
                sessionId = sessionId,
                startTime = startTime,
                startedAt = startTime,
                endedAt = safeEndedAt,
                durationMs = safeEndedAt == default ? 0 : (long)Math.Max(0, (safeEndedAt - startTime).TotalMilliseconds),
                outputPath = outputPath,
                videoTempPath = videoTempPath,
                audioTempPath = audioTempPath,
                isStreaming = isStreaming,
                state = state,
                eventType = eventType,
                errorCode = errorCode,
                errorCodeText = errorCode.ToString(),
                message = message,
                configId = configId,
                configName = configName,
                displayName = displayName,
                audioGainEnabled = audioGainEnabled,
                audioGainDb = audioGainDb,
                isMerged = isMerged,
                mergeOutputPath = mergeOutputPath
            };
        }
    }
}

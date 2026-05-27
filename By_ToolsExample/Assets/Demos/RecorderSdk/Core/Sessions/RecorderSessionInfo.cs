//=====================================================
// 文件名称: RecorderSessionInfo
// 创 建 者: wangbaiyan
// 创建日期: 2026-05-23
// 描    述: 对外公开当前录制会话的只读摘要，便于 UI、日志、上传和任务追踪。
//=====================================================

using System;

namespace Demos.示例_录制视频Recorder.Scripts.Core
{
    /// <summary>
    /// 录制会话只读摘要。
    /// </summary>
    [Serializable]
    public class RecorderSessionInfo
    {
        public string sessionId;
        public DateTime startTime;
        public DateTime startedAt;
        public DateTime endedAt;
        public long durationMs;
        public string outputPath;
        public string videoTempPath;
        public string audioTempPath;
        public bool isStreaming;
        public bool isMerged;
        public string mergeOutputPath;
        public RecorderState state;
        public RecorderSessionEventType eventType;
        public RecorderErrorCode errorCode;
        public string errorCodeText;
        public string message;
        public string configId;
        public string configName;
        public string displayName;
        public bool audioGainEnabled;
        public float audioGainDb;
    }
}

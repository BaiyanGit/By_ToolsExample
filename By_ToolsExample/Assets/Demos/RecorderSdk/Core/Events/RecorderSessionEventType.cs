//=====================================================
// 文件名称: RecorderSessionEventType
// 创 建 者: wangbaiyan
// 创建日期: 2026-05-23
// 描    述: 定义录制会话历史中的事件类型，便于 SDK 调用方追踪完整生命周期。
//=====================================================

namespace Demos.示例_录制视频Recorder.Scripts.Core
{
    /// <summary>
    /// 录制会话历史事件类型。
    /// </summary>
    public enum RecorderSessionEventType
    {
        None,
        StartSucceeded,
        StartFailed,
        StopRequested,
        StopSucceeded,
        StopFailed,
        MergeStarted,
        MergeSucceeded,
        MergeFailed,
        StreamEnded,
        ErrorOccurred,
        WarningOccurred
    }
}

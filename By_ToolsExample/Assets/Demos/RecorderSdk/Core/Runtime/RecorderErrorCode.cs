//=====================================================
// 文件名称: RecorderErrorCode
// 创 建 者: wangbaiyan
// 创建日期: 2026-05-23
// 描    述: 定义 Recorder SDK 对外稳定错误码，方便调用方判断失败原因。
//=====================================================

namespace Demos.示例_录制视频Recorder.Scripts.Core
{
    /// <summary>
    /// 录制器错误码。
    /// </summary>
    public enum RecorderErrorCode
    {
        None,
        ConfigNull,
        ConfigInvalid,
        ConfigNotFound,
        ConfigDuplicateId,
        ConfigMigrationFailed,
        ConfigSaveFailed,
        ConfigDeleteFailed,
        ConfigInvalidId,
        ConfigAutoFixed,
        ConfigDirectoryMissing,
        InitializeFailed,
        AlreadyRunning,
        NotRecording,
        StateBusy,
        StateTransitionInvalid,
        FFmpegPathMissing,
        FFmpegStartFailed,
        AudioFilterInvalid,
        AudioStartFailed,
        AudioStopFailed,
        DisplayNotFound,
        StreamUrlEmpty,
        StreamUrlInvalid,
        StopFailed,
        MergeFailed,
        TempFileNotReady,
        Unknown
    }
}

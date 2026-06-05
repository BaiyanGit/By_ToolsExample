//=====================================================
// 文件名称: RecorderState
// 创 建 者: wangbaiyan
// 创建日期: 2026-05-23
// 描    述: 定义录制器主流程状态，用于统一约束开始、停止、合并和异常流转。
//=====================================================

namespace Demos.RecorderSdk.Core.Runtime
{
    /// <summary>
    /// 录制器主流程状态。
    /// </summary>
    public enum RecorderState
    {
        Uninitialized,
        Ready,
        Starting,
        Recording,
        Stopping,
        Merging,
        Completed,
        Error
    }
}

//=====================================================
// 文件名称: RecorderStateTransition
// 创 建 者: wangbaiyan
// 创建日期: 2026-05-23
// 描    述: 定义 RecorderState 合法流转表，集中约束录制生命周期。
//=====================================================

namespace Demos.RecorderSdk.Core.Runtime
{
    /// <summary>
    /// 录制状态流转表。
    /// </summary>
    public static class RecorderStateTransition
    {
        /// <summary>
        /// 判断状态流转是否合法。
        /// </summary>
        public static bool CanTransit(RecorderState from, RecorderState to)
        {
            if (from == to) return true;
            if (to == RecorderState.Error) return true;
            return from switch
            {
                RecorderState.Uninitialized => to == RecorderState.Ready,
                RecorderState.Ready => to == RecorderState.Starting,
                RecorderState.Starting => to == RecorderState.Recording || to == RecorderState.Ready,
                RecorderState.Recording => to == RecorderState.Stopping,
                RecorderState.Stopping => to == RecorderState.Merging || to == RecorderState.Completed || to == RecorderState.Ready,
                RecorderState.Merging => to == RecorderState.Completed || to == RecorderState.Ready || to == RecorderState.Starting,
                RecorderState.Completed => to == RecorderState.Ready || to == RecorderState.Starting,
                RecorderState.Error => to == RecorderState.Ready || to == RecorderState.Starting,
                _ => false
            };
        }

        /// <summary>
        /// 获取状态流转说明。
        /// </summary>
        public static string GetTransitionText()
        {
            return "Uninitialized->Ready, Ready->Starting, Starting->Recording, Recording->Stopping, Stopping->Merging/Completed, Merging->Completed/Starting, Completed/Error->Ready/Starting, any->Error";
        }
    }
}

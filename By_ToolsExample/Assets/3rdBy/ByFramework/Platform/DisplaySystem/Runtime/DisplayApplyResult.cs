//=====================================================
// 文件名称: DisplayApplyResult.cs
// 创建者: Codex
// 创建日期: 2026-06-09
// 描    述: 表达显示配置应用结果。
//=====================================================

namespace _3rdBy.ByFramework.Platform.DisplaySystem
{
    public sealed class DisplayApplyResult
    {
        public DisplayApplyResult(
            bool success,
            DisplayApplyStatus status,
            DisplayProfileId requestedProfileId,
            DisplayProfileId previousProfileId,
            DisplayProfileId currentProfileId)
        {
            Success = success;
            Status = status;
            RequestedProfileId = requestedProfileId;
            PreviousProfileId = previousProfileId;
            CurrentProfileId = currentProfileId;
        }

        public bool Success { get; }

        public DisplayApplyStatus Status { get; }

        public DisplayProfileId RequestedProfileId { get; }

        public DisplayProfileId PreviousProfileId { get; }

        public DisplayProfileId CurrentProfileId { get; }
    }
}

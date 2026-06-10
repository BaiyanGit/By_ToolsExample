//=====================================================
// 文件名称: DisplayContextChangedEvent.cs
// 创建者: Codex
// 创建日期: 2026-06-09
// 描    述: 表达一次已提交的显示上下文切换事实。
//=====================================================

namespace _3rdBy.ByFramework.Platform.DisplaySystem
{
    public readonly struct DisplayContextChangedEvent
    {
        public DisplayContextChangedEvent(
            DisplayProfileId previousProfileId,
            DisplayProfileId currentProfileId,
            DisplayMode previousMode,
            DisplayMode currentMode)
        {
            PreviousProfileId = previousProfileId;
            CurrentProfileId = currentProfileId;
            PreviousMode = previousMode;
            CurrentMode = currentMode;
        }

        public DisplayProfileId PreviousProfileId { get; }

        public DisplayProfileId CurrentProfileId { get; }

        public DisplayMode PreviousMode { get; }

        public DisplayMode CurrentMode { get; }
    }
}

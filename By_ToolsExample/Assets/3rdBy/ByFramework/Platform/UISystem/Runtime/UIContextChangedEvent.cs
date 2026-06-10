//=====================================================
// 文件名称: UIContextChangedEvent.cs
// 创建者: Codex
// 创建日期: 2026-06-09
// 描    述: 表达一次已提交的 UIContext 变化。
//=====================================================

namespace _3rdBy.ByFramework.Platform.UISystem
{
    public readonly struct UIContextChangedEvent
    {
        public UIContextChangedEvent(int previousVersion, int currentVersion)
        {
            PreviousVersion = previousVersion;
            CurrentVersion = currentVersion;
        }

        public int PreviousVersion { get; }

        public int CurrentVersion { get; }
    }
}

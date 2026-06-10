//=====================================================
// 文件名称: LanguageChangedEvent.cs
// 创 建 者: Codex
// 创建日期: 2026-06-09
// 描    述: 定义 LocalizationSystem 语言切换成功后的事件载荷。
//=====================================================

namespace _3rdBy.ByFramework.Platform.LocalizationSystem
{
    public readonly struct LanguageChangedEvent
    {
        public LanguageChangedEvent(LanguageCode previousLanguage, LanguageCode currentLanguage)
        {
            PreviousLanguage = previousLanguage;
            CurrentLanguage = currentLanguage;
        }

        public LanguageCode PreviousLanguage { get; }

        public LanguageCode CurrentLanguage { get; }
    }
}

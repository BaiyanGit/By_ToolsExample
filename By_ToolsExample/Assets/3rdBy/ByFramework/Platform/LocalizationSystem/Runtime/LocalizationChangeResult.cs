//=====================================================
// 文件名称: LocalizationChangeResult.cs
// 创 建 者: Codex
// 创建日期: 2026-06-09
// 描    述: 定义 LocalizationSystem 语言切换结果模型。
//=====================================================

namespace _3rdBy.ByFramework.Platform.LocalizationSystem
{
    public readonly struct LocalizationChangeResult
    {
        public LocalizationChangeResult(
            bool success,
            LocalizationChangeStatus status,
            LanguageCode requestedLanguage,
            LanguageCode previousLanguage,
            LanguageCode effectiveLanguage)
        {
            Success = success;
            Status = status;
            RequestedLanguage = requestedLanguage;
            PreviousLanguage = previousLanguage;
            EffectiveLanguage = effectiveLanguage;
        }

        public bool Success { get; }

        public LocalizationChangeStatus Status { get; }

        public LanguageCode RequestedLanguage { get; }

        public LanguageCode PreviousLanguage { get; }

        public LanguageCode EffectiveLanguage { get; }
    }
}

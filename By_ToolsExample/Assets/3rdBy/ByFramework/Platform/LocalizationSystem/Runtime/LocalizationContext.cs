//=====================================================
// 文件名称: LocalizationContext.cs
// 创 建 者: Codex
// 创建日期: 2026-06-09
// 描    述: 定义 LocalizationSystem 当前运行时上下文的不可变快照。
//=====================================================

namespace _3rdBy.ByFramework.Platform.LocalizationSystem
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    public sealed class LocalizationContext
    {
        public LocalizationContext(
            LanguageCode currentLanguage,
            LanguageCode defaultLanguage,
            LanguageCode? fallbackLanguage,
            IReadOnlyList<LanguageCode> fallbackChain,
            IReadOnlyList<LanguageCode> availableLanguages,
            IReadOnlyList<LocalizationProviderDescriptor> providers,
            bool runtimeSwitchEnabled)
        {
            CurrentLanguage = currentLanguage;
            DefaultLanguage = defaultLanguage;
            FallbackLanguage = fallbackLanguage;
            FallbackChain = fallbackChain == null
                ? Array.Empty<LanguageCode>()
                : new ReadOnlyCollection<LanguageCode>(new List<LanguageCode>(fallbackChain));
            AvailableLanguages = availableLanguages == null
                ? Array.Empty<LanguageCode>()
                : new ReadOnlyCollection<LanguageCode>(new List<LanguageCode>(availableLanguages));
            Providers = providers == null
                ? Array.Empty<LocalizationProviderDescriptor>()
                : new ReadOnlyCollection<LocalizationProviderDescriptor>(new List<LocalizationProviderDescriptor>(providers));
            RuntimeSwitchEnabled = runtimeSwitchEnabled;
        }

        public LanguageCode CurrentLanguage { get; }

        public LanguageCode DefaultLanguage { get; }

        public LanguageCode? FallbackLanguage { get; }

        public IReadOnlyList<LanguageCode> FallbackChain { get; }

        public IReadOnlyList<LanguageCode> AvailableLanguages { get; }

        public IReadOnlyList<LocalizationProviderDescriptor> Providers { get; }

        public bool RuntimeSwitchEnabled { get; }
    }
}

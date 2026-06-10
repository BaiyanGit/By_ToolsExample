//=====================================================
// 文件名称: ILocalizationService.cs
// 创 建 者: Codex
// 创建日期: 2026-06-09
// 描    述: 定义 LocalizationSystem Runtime 服务接口。
//=====================================================

namespace _3rdBy.ByFramework.Platform.LocalizationSystem
{
    using System;
    using System.Collections.Generic;

    public interface ILocalizationService : _3rdBy.ByFramework.Platform.PlatformServiceRegistry.IPlatformService
    {
        event Action<LanguageChangedEvent> LanguageChanged;

        string GetText(LocalizationKey key);

        bool TryGetText(LocalizationKey key, out string text, out LocalizationQueryStatus status);

        string GetFormattedText(LocalizationKey key, LocalizationArguments arguments);

        LocalizationChangeResult SetLanguage(LanguageCode language, bool persistPreference = true);

        LanguageCode GetCurrentLanguage();

        IReadOnlyList<LanguageCode> GetAvailableLanguages();

        LocalizationContext GetCurrentContext();
    }
}

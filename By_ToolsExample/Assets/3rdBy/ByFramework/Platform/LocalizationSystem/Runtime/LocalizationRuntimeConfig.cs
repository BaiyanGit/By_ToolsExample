//=====================================================
// 文件名称: LocalizationRuntimeConfig.cs
// 创 建 者: Codex
// 创建日期: 2026-06-09
// 描    述: 定义 LocalizationSystem Runtime 内部使用的模块配置模型。
//=====================================================

namespace _3rdBy.ByFramework.Platform.LocalizationSystem
{
    using System;

    [Serializable]
    internal sealed class LocalizationRuntimeConfig
    {
        public string defaultLanguage = "zh_cn";
        public string fallbackLanguage = "en_us";
        public string languagePackPath = "ByFramework/Localization";
        public string externalLanguagePackPath = "ByFramework/Localization";
        public bool enableRuntimeSwitch = true;
        public bool enableMissingKeyLog = true;
        public bool showMissingKeyPlaceholder = true;
        public string preferredProviderProfileId = string.Empty;

        public string DefaultLanguage => defaultLanguage ?? string.Empty;
        public string FallbackLanguage => fallbackLanguage ?? string.Empty;
        public string LanguagePackPath => languagePackPath ?? string.Empty;
        public string ExternalLanguagePackPath => externalLanguagePackPath ?? string.Empty;
        public bool EnableRuntimeSwitch => enableRuntimeSwitch;
        public bool EnableMissingKeyLog => enableMissingKeyLog;
        public bool ShowMissingKeyPlaceholder => showMissingKeyPlaceholder;
        public string PreferredProviderProfileId => preferredProviderProfileId ?? string.Empty;

        public static LocalizationRuntimeConfig CreateDefault()
        {
            return new LocalizationRuntimeConfig();
        }

        public LocalizationRuntimeConfig Normalize()
        {
            return new LocalizationRuntimeConfig
            {
                defaultLanguage = string.IsNullOrWhiteSpace(defaultLanguage) ? "zh_cn" : defaultLanguage,
                fallbackLanguage = fallbackLanguage ?? string.Empty,
                languagePackPath = string.IsNullOrWhiteSpace(languagePackPath) ? "ByFramework/Localization" : languagePackPath,
                externalLanguagePackPath = string.IsNullOrWhiteSpace(externalLanguagePackPath)
                    ? "ByFramework/Localization"
                    : externalLanguagePackPath,
                enableRuntimeSwitch = enableRuntimeSwitch,
                enableMissingKeyLog = enableMissingKeyLog,
                showMissingKeyPlaceholder = showMissingKeyPlaceholder,
                preferredProviderProfileId = preferredProviderProfileId ?? string.Empty,
            };
        }
    }
}

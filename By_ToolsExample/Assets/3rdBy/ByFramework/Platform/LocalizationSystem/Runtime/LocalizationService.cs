//=====================================================
// 文件名称: LocalizationService.cs
// 创 建 者: Codex
// 创建日期: 2026-06-09
// 描    述: 实现 LocalizationSystem Runtime 的统一语言管理与文本查询服务。
//=====================================================

namespace _3rdBy.ByFramework.Platform.LocalizationSystem
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Text.RegularExpressions;
    using _3rdBy.ByFramework.Platform.FrameworkConfig;
    using _3rdBy.ByFramework.Platform.SaveSystem;
    using UnityEngine;

    public sealed class LocalizationService : ILocalizationService
    {
        private const string UserLanguageSaveKey = "platform.localization.current_language";
        private static readonly Regex NamedArgumentPattern = new(@"\{([A-Za-z0-9_]+)\}", RegexOptions.Compiled);
        private readonly object _syncRoot = new();
        private readonly List<JsonLanguagePackProvider> _providers;
        private readonly LocalizationRuntimeConfig _config;
        private readonly ISaveService _saveService;
        private LocalizationContext _currentContext;

        public LocalizationService(
            IReadOnlyList<JsonLanguagePackProvider> providers = null,
            IFrameworkConfigService frameworkConfigService = null,
            ISaveService saveService = null)
        {
            _saveService = saveService;
            _config = LoadRuntimeConfig(frameworkConfigService);
            _providers = providers == null
                ? CreateDefaultProviders(_config)
                : CreateProviderList(providers, _config.PreferredProviderProfileId);
            _currentContext = BuildInitialContext();
        }

        public event Action<LanguageChangedEvent> LanguageChanged;

        public string GetText(LocalizationKey key)
        {
            if (key.IsEmpty)
            {
                throw new ArgumentException("[LocalizationSystem] Localization key cannot be empty.", nameof(key));
            }

            if (TryGetText(key, out string text, out _))
            {
                return text;
            }

            return BuildMissingText(key);
        }

        public bool TryGetText(LocalizationKey key, out string text, out LocalizationQueryStatus status)
        {
            if (key.IsEmpty)
            {
                throw new ArgumentException("[LocalizationSystem] Localization key cannot be empty.", nameof(key));
            }

            lock (_syncRoot)
            {
                return TryGetTextCore(_currentContext, key, out text, out status);
            }
        }

        public string GetFormattedText(LocalizationKey key, LocalizationArguments arguments)
        {
            if (key.IsEmpty)
            {
                throw new ArgumentException("[LocalizationSystem] Localization key cannot be empty.", nameof(key));
            }

            lock (_syncRoot)
            {
                if (!TryGetTextCore(_currentContext, key, out string template, out _))
                {
                    return BuildMissingText(key);
                }

                return TryFormatTemplate(template, arguments, out string formattedText)
                    ? formattedText
                    : template;
            }
        }

        public LocalizationChangeResult SetLanguage(LanguageCode language, bool persistPreference = true)
        {
            if (language.IsEmpty)
            {
                throw new ArgumentException("[LocalizationSystem] Language code cannot be empty.", nameof(language));
            }

            LocalizationChangeResult result;
            LanguageChangedEvent changedEvent = default;
            bool shouldPublish = false;

            lock (_syncRoot)
            {
                LocalizationContext previousContext = _currentContext;
                LanguageCode previousLanguage = previousContext.CurrentLanguage;
                if (!_currentContext.RuntimeSwitchEnabled)
                {
                    return new LocalizationChangeResult(false, LocalizationChangeStatus.RuntimeSwitchDisabled, language, previousLanguage, previousLanguage);
                }

                ReloadProviders();
                if (!HasHealthyProvider())
                {
                    return new LocalizationChangeResult(false, LocalizationChangeStatus.ProviderFailure, language, previousLanguage, previousLanguage);
                }

                if (!IsLanguageAvailable(language))
                {
                    return new LocalizationChangeResult(false, LocalizationChangeStatus.LanguageNotAvailable, language, previousLanguage, previousLanguage);
                }

                if (persistPreference && !TryPersistUserLanguage(language))
                {
                    return new LocalizationChangeResult(false, LocalizationChangeStatus.PersistenceFailure, language, previousLanguage, previousLanguage);
                }

                _currentContext = BuildContext(language);
                result = new LocalizationChangeResult(
                    true,
                    LocalizationChangeStatus.Success,
                    language,
                    previousLanguage,
                    _currentContext.CurrentLanguage);

                if (_currentContext.CurrentLanguage != previousLanguage)
                {
                    changedEvent = new LanguageChangedEvent(previousLanguage, _currentContext.CurrentLanguage);
                    shouldPublish = true;
                }
            }

            if (shouldPublish)
            {
                LanguageChanged?.Invoke(changedEvent);
            }

            return result;
        }

        public LanguageCode GetCurrentLanguage()
        {
            lock (_syncRoot)
            {
                return _currentContext.CurrentLanguage;
            }
        }

        public IReadOnlyList<LanguageCode> GetAvailableLanguages()
        {
            lock (_syncRoot)
            {
                return _currentContext.AvailableLanguages;
            }
        }

        public LocalizationContext GetCurrentContext()
        {
            lock (_syncRoot)
            {
                return _currentContext;
            }
        }

        private LocalizationContext BuildInitialContext()
        {
            ReloadProviders();
            LanguageCode defaultLanguage = ResolveConfiguredLanguageOrFallback(_config.DefaultLanguage, "zh_cn");
            if (TryRestoreUserLanguage(out LanguageCode persistedLanguage) && IsLanguageAvailable(persistedLanguage))
            {
                return BuildContext(persistedLanguage);
            }

            return BuildContext(defaultLanguage);
        }

        private LocalizationContext BuildContext(LanguageCode currentLanguage)
        {
            LanguageCode defaultLanguage = ResolveConfiguredLanguageOrFallback(_config.DefaultLanguage, "zh_cn");
            LanguageCode? fallbackLanguage = ResolveOptionalLanguage(_config.FallbackLanguage);
            List<LanguageCode> availableLanguages = BuildSortedAvailableLanguages(currentLanguage, fallbackLanguage, defaultLanguage);
            List<LanguageCode> fallbackChain = BuildFallbackChain(currentLanguage, fallbackLanguage, defaultLanguage);
            List<LocalizationProviderDescriptor> descriptors = new();
            for (int index = 0; index < _providers.Count; index++)
            {
                JsonLanguagePackProvider provider = _providers[index];
                descriptors.Add(new LocalizationProviderDescriptor(provider.ProviderId, provider.ProviderType, index, provider.IsWritable));
            }

            return new LocalizationContext(
                currentLanguage,
                defaultLanguage,
                fallbackLanguage,
                fallbackChain,
                availableLanguages,
                descriptors,
                _config.EnableRuntimeSwitch);
        }

        private bool TryGetTextCore(LocalizationContext context, LocalizationKey key, out string text, out LocalizationQueryStatus status)
        {
            bool anyLanguageAvailable = false;
            for (int index = 0; index < context.FallbackChain.Count; index++)
            {
                LanguageCode language = context.FallbackChain[index];
                if (!ContainsLanguage(context.AvailableLanguages, language))
                {
                    continue;
                }

                anyLanguageAvailable = true;
                for (int providerIndex = 0; providerIndex < _providers.Count; providerIndex++)
                {
                    JsonLanguagePackProvider provider = _providers[providerIndex];
                    if (provider.TryGetText(language, key, out text, out bool languageExists))
                    {
                        status = LocalizationQueryStatus.Success;
                        return true;
                    }

                    if (languageExists)
                    {
                        continue;
                    }
                }
            }

            text = string.Empty;
            if (!HasHealthyProvider())
            {
                status = LocalizationQueryStatus.ProviderFailure;
                LogQueryFailure(status, key);
                return false;
            }

            status = anyLanguageAvailable ? LocalizationQueryStatus.MissingKey : LocalizationQueryStatus.MissingLanguage;
            LogQueryFailure(status, key);
            return false;
        }

        private void ReloadProviders()
        {
            for (int index = 0; index < _providers.Count; index++)
            {
                _providers[index].Reload(out _, out _);
            }
        }

        private bool HasHealthyProvider()
        {
            for (int index = 0; index < _providers.Count; index++)
            {
                if (_providers[index].IsHealthy)
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsLanguageAvailable(LanguageCode language)
        {
            for (int index = 0; index < _providers.Count; index++)
            {
                if (_providers[index].HasLanguage(language))
                {
                    return true;
                }
            }

            return false;
        }

        private bool TryRestoreUserLanguage(out LanguageCode language)
        {
            language = default;
            if (_saveService == null)
            {
                return false;
            }

            SaveRequest request = new(UserLanguageSaveKey, SaveScope.User);
            if (!_saveService.TryLoad(request, out LocalizationPreferenceData data, out LoadResult<LocalizationPreferenceData> result)
                || !result.Success
                || data == null)
            {
                return false;
            }

            return LanguageCode.TryParse(data.currentLanguage, out language);
        }

        private bool TryPersistUserLanguage(LanguageCode language)
        {
            if (_saveService == null)
            {
                return true;
            }

            SaveRequest<LocalizationPreferenceData> request = new(
                UserLanguageSaveKey,
                SaveScope.User,
                new LocalizationPreferenceData
                {
                    currentLanguage = language.Value,
                });

            return _saveService.TrySave(request, out SaveResult result) && result.Success;
        }

        private string BuildMissingText(LocalizationKey key)
        {
            return _config.ShowMissingKeyPlaceholder ? $"[Missing: {key.Value}]" : key.Value;
        }

        private static bool TryFormatTemplate(string template, LocalizationArguments arguments, out string formattedText)
        {
            try
            {
                string namedFormatted = ApplyNamedArguments(template, arguments.NamedArguments);
                if (arguments.PositionalArguments.Count == 0)
                {
                    formattedText = namedFormatted;
                    return true;
                }

                object[] positional = new object[arguments.PositionalArguments.Count];
                for (int index = 0; index < positional.Length; index++)
                {
                    positional[index] = arguments.PositionalArguments[index];
                }

                formattedText = string.Format(CultureInfo.InvariantCulture, namedFormatted, positional);
                return true;
            }
            catch
            {
                formattedText = template;
                return false;
            }
        }

        private static string ApplyNamedArguments(string template, System.Collections.Generic.IReadOnlyDictionary<string, object> namedArguments)
        {
            if (namedArguments == null || namedArguments.Count == 0)
            {
                return template;
            }

            return NamedArgumentPattern.Replace(
                template,
                match =>
                {
                    string argumentName = match.Groups[1].Value;
                    return namedArguments.TryGetValue(argumentName, out object value)
                        ? Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty
                        : match.Value;
                });
        }

        private void LogQueryFailure(LocalizationQueryStatus status, LocalizationKey key)
        {
            if (!_config.EnableMissingKeyLog)
            {
                return;
            }

            switch (status)
            {
                case LocalizationQueryStatus.MissingKey:
                    Debug.LogWarning($"[LocalizationSystem] 缺失文本 Key：{key.Value}");
                    break;
                case LocalizationQueryStatus.MissingLanguage:
                    Debug.LogWarning($"[LocalizationSystem] 语言不可用，无法解析 Key：{key.Value}");
                    break;
                case LocalizationQueryStatus.ProviderFailure:
                    Debug.LogError($"[LocalizationSystem] Provider 失败，无法解析 Key：{key.Value}");
                    break;
            }
        }

        private static List<JsonLanguagePackProvider> CreateDefaultProviders(LocalizationRuntimeConfig config)
        {
            List<JsonLanguagePackProvider> providers = new();
            string streamingRoot = ResolveProviderRoot(Application.streamingAssetsPath, config.LanguagePackPath);
            providers.Add(new JsonLanguagePackProvider("StreamingAssets", "StreamingAssets", streamingRoot));

            string persistentRoot = ResolveProviderRoot(Application.persistentDataPath, config.ExternalLanguagePackPath);
            providers.Add(new JsonLanguagePackProvider("Persistent", "Persistent", persistentRoot, true));
            return CreateProviderList(providers, config.PreferredProviderProfileId);
        }

        private static List<JsonLanguagePackProvider> CreateProviderList(
            IReadOnlyList<JsonLanguagePackProvider> providers,
            string preferredProviderProfileId)
        {
            List<JsonLanguagePackProvider> providerList = new();
            if (providers == null)
            {
                return providerList;
            }

            for (int index = 0; index < providers.Count; index++)
            {
                if (providers[index] != null)
                {
                    providerList.Add(providers[index]);
                }
            }

            if (!string.IsNullOrWhiteSpace(preferredProviderProfileId))
            {
                providerList.Sort((left, right) =>
                {
                    bool leftPreferred = string.Equals(left.ProviderId, preferredProviderProfileId, StringComparison.Ordinal);
                    bool rightPreferred = string.Equals(right.ProviderId, preferredProviderProfileId, StringComparison.Ordinal);
                    if (leftPreferred == rightPreferred)
                    {
                        return 0;
                    }

                    return leftPreferred ? -1 : 1;
                });
            }

            return providerList;
        }

        private static string ResolveProviderRoot(string basePath, string configuredPath)
        {
            if (string.IsNullOrWhiteSpace(configuredPath))
            {
                return basePath ?? string.Empty;
            }

            if (Path.IsPathRooted(configuredPath))
            {
                return configuredPath;
            }

            string normalizedPath = configuredPath
                .Replace('/', Path.DirectorySeparatorChar)
                .Replace('\\', Path.DirectorySeparatorChar);
            return Path.Combine(basePath ?? string.Empty, normalizedPath);
        }

        private static LocalizationRuntimeConfig LoadRuntimeConfig(IFrameworkConfigService frameworkConfigService)
        {
            if (frameworkConfigService != null
                && frameworkConfigService.TryGetModuleConfig(out LocalizationRuntimeConfig config)
                && config != null)
            {
                return config.Normalize();
            }

            return LocalizationRuntimeConfig.CreateDefault();
        }

        private static LanguageCode ResolveConfiguredLanguageOrFallback(string configuredLanguage, string fallbackValue)
        {
            if (LanguageCode.TryParse(configuredLanguage, out LanguageCode configured))
            {
                return configured;
            }

            return new LanguageCode(fallbackValue);
        }

        private static LanguageCode? ResolveOptionalLanguage(string configuredLanguage)
        {
            return LanguageCode.TryParse(configuredLanguage, out LanguageCode fallback)
                ? fallback
                : null;
        }

        private static bool ContainsLanguage(IReadOnlyList<LanguageCode> languages, LanguageCode language)
        {
            for (int index = 0; index < languages.Count; index++)
            {
                if (languages[index] == language)
                {
                    return true;
                }
            }

            return false;
        }

        private List<LanguageCode> BuildSortedAvailableLanguages(
            LanguageCode currentLanguage,
            LanguageCode? fallbackLanguage,
            LanguageCode defaultLanguage)
        {
            Dictionary<string, LanguageCode> uniqueLanguages = new(StringComparer.Ordinal);
            for (int index = 0; index < _providers.Count; index++)
            {
                IReadOnlyList<LanguageCode> languages = _providers[index].GetAvailableLanguages();
                for (int languageIndex = 0; languageIndex < languages.Count; languageIndex++)
                {
                    uniqueLanguages[languages[languageIndex].Value] = languages[languageIndex];
                }
            }

            List<LanguageCode> availableLanguages = new(uniqueLanguages.Values);
            availableLanguages.Sort((left, right) =>
            {
                int leftRank = GetLanguageSortRank(left, currentLanguage, fallbackLanguage, defaultLanguage);
                int rightRank = GetLanguageSortRank(right, currentLanguage, fallbackLanguage, defaultLanguage);
                int compareRank = leftRank.CompareTo(rightRank);
                if (compareRank != 0)
                {
                    return compareRank;
                }

                return string.CompareOrdinal(left.Value, right.Value);
            });
            return availableLanguages;
        }

        private static int GetLanguageSortRank(
            LanguageCode language,
            LanguageCode currentLanguage,
            LanguageCode? fallbackLanguage,
            LanguageCode defaultLanguage)
        {
            if (language == currentLanguage)
            {
                return 0;
            }

            if (fallbackLanguage.HasValue && language == fallbackLanguage.Value)
            {
                return 1;
            }

            if (language == defaultLanguage)
            {
                return 2;
            }

            return 3;
        }

        private static List<LanguageCode> BuildFallbackChain(
            LanguageCode currentLanguage,
            LanguageCode? fallbackLanguage,
            LanguageCode defaultLanguage)
        {
            List<LanguageCode> fallbackChain = new();
            AddDistinctLanguage(fallbackChain, currentLanguage);
            if (fallbackLanguage.HasValue)
            {
                AddDistinctLanguage(fallbackChain, fallbackLanguage.Value);
            }

            AddDistinctLanguage(fallbackChain, defaultLanguage);
            return fallbackChain;
        }

        private static void AddDistinctLanguage(List<LanguageCode> languages, LanguageCode language)
        {
            for (int index = 0; index < languages.Count; index++)
            {
                if (languages[index] == language)
                {
                    return;
                }
            }

            languages.Add(language);
        }

        [Serializable]
        private sealed class LocalizationPreferenceData
        {
            public string currentLanguage = string.Empty;
        }
    }
}

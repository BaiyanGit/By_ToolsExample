//=====================================================
// 文件名称: JsonLanguagePackProvider.cs
// 创 建 者: Codex
// 创建日期: 2026-06-09
// 描    述: 提供基于 JSON 文件系统的基础语言包 Provider。
//=====================================================

namespace _3rdBy.ByFramework.Platform.LocalizationSystem
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using _3rdBy.Plugins.LitJson;

    public sealed class JsonLanguagePackProvider
    {
        private readonly object _syncRoot = new();
        private readonly string _rootDirectory;
        private Dictionary<LanguageCode, IReadOnlyDictionary<string, string>> _tables = new();
        private IReadOnlyList<LanguageCode> _availableLanguages = Array.Empty<LanguageCode>();

        public JsonLanguagePackProvider(string providerId, string providerType, string rootDirectory, bool isWritable = false)
        {
            ProviderId = string.IsNullOrWhiteSpace(providerId)
                ? throw new ArgumentException("[LocalizationSystem] ProviderId cannot be empty.", nameof(providerId))
                : providerId;
            ProviderType = string.IsNullOrWhiteSpace(providerType)
                ? throw new ArgumentException("[LocalizationSystem] ProviderType cannot be empty.", nameof(providerType))
                : providerType;
            _rootDirectory = rootDirectory?.Trim() ?? string.Empty;
            IsWritable = isWritable;
            LastErrorCode = string.Empty;
            LastErrorMessage = string.Empty;
        }

        public string ProviderId { get; }

        public string ProviderType { get; }

        public bool IsWritable { get; }

        public string LastErrorCode { get; private set; }

        public string LastErrorMessage { get; private set; }

        public bool IsHealthy => string.IsNullOrEmpty(LastErrorCode);

        public bool Reload(out string errorCode, out string errorMessage)
        {
            lock (_syncRoot)
            {
                try
                {
                    Dictionary<LanguageCode, IReadOnlyDictionary<string, string>> tables = new();
                    List<LanguageCode> availableLanguages = new();

                    if (!string.IsNullOrWhiteSpace(_rootDirectory) && Directory.Exists(_rootDirectory))
                    {
                        string[] files = Directory.GetFiles(_rootDirectory, "*.json", SearchOption.TopDirectoryOnly);
                        Array.Sort(files, StringComparer.OrdinalIgnoreCase);
                        for (int index = 0; index < files.Length; index++)
                        {
                            string path = files[index];
                            LanguageCode language;
                            IReadOnlyDictionary<string, string> table = LoadLanguageTable(path, out language);
                            tables[language] = table;
                            availableLanguages.Add(language);
                        }
                    }

                    _tables = tables;
                    _availableLanguages = new ReadOnlyCollection<LanguageCode>(availableLanguages);
                    LastErrorCode = string.Empty;
                    LastErrorMessage = string.Empty;
                    errorCode = string.Empty;
                    errorMessage = string.Empty;
                    return true;
                }
                catch (Exception exception)
                {
                    _tables = new Dictionary<LanguageCode, IReadOnlyDictionary<string, string>>();
                    _availableLanguages = Array.Empty<LanguageCode>();
                    LastErrorCode = "ProviderReloadFailed";
                    LastErrorMessage = exception.Message;
                    errorCode = LastErrorCode;
                    errorMessage = LastErrorMessage;
                    return false;
                }
            }
        }

        public IReadOnlyList<LanguageCode> GetAvailableLanguages()
        {
            lock (_syncRoot)
            {
                return _availableLanguages;
            }
        }

        public bool HasLanguage(LanguageCode language)
        {
            lock (_syncRoot)
            {
                return _tables.ContainsKey(language);
            }
        }

        public bool TryGetText(LanguageCode language, LocalizationKey key, out string text, out bool languageExists)
        {
            if (language.IsEmpty)
            {
                throw new ArgumentException("[LocalizationSystem] Language code cannot be empty.", nameof(language));
            }

            if (key.IsEmpty)
            {
                throw new ArgumentException("[LocalizationSystem] Localization key cannot be empty.", nameof(key));
            }

            lock (_syncRoot)
            {
                if (!_tables.TryGetValue(language, out IReadOnlyDictionary<string, string> table))
                {
                    languageExists = false;
                    text = string.Empty;
                    return false;
                }

                languageExists = true;
                return table.TryGetValue(key.Value, out text);
            }
        }

        private static IReadOnlyDictionary<string, string> LoadLanguageTable(string path, out LanguageCode language)
        {
            string content = File.ReadAllText(path);
            JsonData root = JsonMapper.ToObject(content);
            if (root == null || !root.IsObject)
            {
                throw new InvalidOperationException($"[LocalizationSystem] Language pack root must be a JSON object: {path}");
            }

            string fileLanguage = Path.GetFileNameWithoutExtension(path);
            string declaredLanguage = TryReadLanguageCode(root);
            if (!LanguageCode.TryParse(declaredLanguage, out language)
                && !LanguageCode.TryParse(fileLanguage, out language))
            {
                throw new InvalidOperationException($"[LocalizationSystem] Invalid language code in language pack: {path}");
            }

            JsonData entriesNode = ExtractEntriesNode(root, out bool usesExplicitEntriesNode);
            Dictionary<string, string> entries = new(StringComparer.Ordinal);
            foreach (string entryKey in entriesNode.Keys)
            {
                if (!usesExplicitEntriesNode && IsReservedMetadataKey(entryKey))
                {
                    continue;
                }

                JsonData entryValue = entriesNode[entryKey];
                if (entryValue == null)
                {
                    continue;
                }

                if (entryValue.IsString)
                {
                    entries[entryKey] = (string)entryValue;
                    continue;
                }

                if (entryValue.IsBoolean || entryValue.IsDouble || entryValue.IsInt || entryValue.IsLong)
                {
                    entries[entryKey] = entryValue.ToString();
                }
            }

            return new ReadOnlyDictionary<string, string>(entries);
        }

        private static string TryReadLanguageCode(JsonData root)
        {
            if (root.ContainsKey("language") && root["language"] != null && root["language"].IsString)
            {
                return (string)root["language"];
            }

            if (root.ContainsKey("languageCode") && root["languageCode"] != null && root["languageCode"].IsString)
            {
                return (string)root["languageCode"];
            }

            return string.Empty;
        }

        private static JsonData ExtractEntriesNode(JsonData root, out bool usesExplicitEntriesNode)
        {
            if (root.ContainsKey("entries"))
            {
                usesExplicitEntriesNode = true;
                JsonData entriesNode = root["entries"];
                if (entriesNode == null || !entriesNode.IsObject)
                {
                    throw new InvalidOperationException("[LocalizationSystem] The entries node must be a JSON object.");
                }

                return entriesNode;
            }

            usesExplicitEntriesNode = false;
            return root;
        }

        private static bool IsReservedMetadataKey(string key)
        {
            return string.Equals(key, "language", StringComparison.Ordinal)
                || string.Equals(key, "languageCode", StringComparison.Ordinal)
                || string.Equals(key, "displayName", StringComparison.Ordinal)
                || string.Equals(key, "nativeName", StringComparison.Ordinal)
                || string.Equals(key, "direction", StringComparison.Ordinal);
        }
    }
}

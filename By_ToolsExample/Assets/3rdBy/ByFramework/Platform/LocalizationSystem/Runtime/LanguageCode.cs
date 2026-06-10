//=====================================================
// 文件名称: LanguageCode.cs
// 创 建 者: Codex
// 创建日期: 2026-06-09
// 描    述: 定义 LocalizationSystem 使用的稳定语言标识模型。
//=====================================================

namespace _3rdBy.ByFramework.Platform.LocalizationSystem
{
    using System;

    /// <summary>
    /// 稳定语言标识。
    /// </summary>
    public readonly struct LanguageCode : IEquatable<LanguageCode>
    {
        private readonly string _value;

        public LanguageCode(string value)
        {
            string normalized = Normalize(value);
            if (string.IsNullOrEmpty(normalized))
            {
                throw new ArgumentException("[LocalizationSystem] Language code cannot be null or empty.", nameof(value));
            }

            _value = normalized;
        }

        private LanguageCode(string value, bool _)
        {
            _value = value ?? string.Empty;
        }

        public string Value => _value ?? string.Empty;

        public bool IsEmpty => string.IsNullOrEmpty(Value);

        public static bool TryParse(string value, out LanguageCode languageCode)
        {
            string normalized = Normalize(value);
            if (string.IsNullOrEmpty(normalized))
            {
                languageCode = default;
                return false;
            }

            languageCode = new LanguageCode(normalized, true);
            return true;
        }

        public bool Equals(LanguageCode other)
        {
            return string.Equals(Value, other.Value, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return obj is LanguageCode other && Equals(other);
        }

        public override int GetHashCode()
        {
            return StringComparer.Ordinal.GetHashCode(Value);
        }

        public override string ToString()
        {
            return Value;
        }

        public static bool operator ==(LanguageCode left, LanguageCode right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(LanguageCode left, LanguageCode right)
        {
            return !left.Equals(right);
        }

        private static string Normalize(string value)
        {
            string trimmed = value?.Trim();
            if (string.IsNullOrEmpty(trimmed))
            {
                return string.Empty;
            }

            string normalized = trimmed.Replace('-', '_').ToLowerInvariant();
            string[] segments = normalized.Split('_');
            for (int index = 0; index < segments.Length; index++)
            {
                string segment = segments[index];
                if (string.IsNullOrEmpty(segment))
                {
                    return string.Empty;
                }

                for (int charIndex = 0; charIndex < segment.Length; charIndex++)
                {
                    char current = segment[charIndex];
                    if (!(current is >= 'a' and <= 'z') && !(current is >= '0' and <= '9'))
                    {
                        return string.Empty;
                    }
                }
            }

            return normalized;
        }
    }
}

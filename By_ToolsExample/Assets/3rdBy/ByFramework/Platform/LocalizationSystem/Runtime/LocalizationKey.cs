//=====================================================
// 文件名称: LocalizationKey.cs
// 创 建 者: Codex
// 创建日期: 2026-06-09
// 描    述: 定义 LocalizationSystem 使用的稳定文本键模型。
//=====================================================

namespace _3rdBy.ByFramework.Platform.LocalizationSystem
{
    using System;

    /// <summary>
    /// 稳定文本键。
    /// </summary>
    public readonly struct LocalizationKey : IEquatable<LocalizationKey>
    {
        private readonly string _value;

        public LocalizationKey(string value)
        {
            string normalized = Normalize(value);
            if (string.IsNullOrEmpty(normalized))
            {
                throw new ArgumentException("[LocalizationSystem] Localization key cannot be null or empty.", nameof(value));
            }

            _value = normalized;
        }

        private LocalizationKey(string value, bool _)
        {
            _value = value ?? string.Empty;
        }

        public string Value => _value ?? string.Empty;

        public bool IsEmpty => string.IsNullOrEmpty(Value);

        public static bool TryParse(string value, out LocalizationKey key)
        {
            string normalized = Normalize(value);
            if (string.IsNullOrEmpty(normalized))
            {
                key = default;
                return false;
            }

            key = new LocalizationKey(normalized, true);
            return true;
        }

        public bool Equals(LocalizationKey other)
        {
            return string.Equals(Value, other.Value, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return obj is LocalizationKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            return StringComparer.Ordinal.GetHashCode(Value);
        }

        public override string ToString()
        {
            return Value;
        }

        public static bool operator ==(LocalizationKey left, LocalizationKey right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(LocalizationKey left, LocalizationKey right)
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

            string[] segments = trimmed.Split('.');
            for (int index = 0; index < segments.Length; index++)
            {
                if (string.IsNullOrWhiteSpace(segments[index]))
                {
                    return string.Empty;
                }
            }

            return trimmed;
        }
    }
}

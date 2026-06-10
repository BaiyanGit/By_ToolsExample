//=====================================================
// 文件名称: DisplayProfileId.cs
// 创建者: Codex
// 创建日期: 2026-06-09
// 描    述: 表达稳定显示配置标识。
//=====================================================

namespace _3rdBy.ByFramework.Platform.DisplaySystem
{
    using System;

    public readonly struct DisplayProfileId : IEquatable<DisplayProfileId>
    {
        public DisplayProfileId(string value)
        {
            Value = Normalize(value);
        }

        public string Value { get; }

        public bool IsEmpty => string.IsNullOrWhiteSpace(Value);

        public bool Equals(DisplayProfileId other)
        {
            return string.Equals(Value, other.Value, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return obj is DisplayProfileId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value == null ? 0 : StringComparer.Ordinal.GetHashCode(Value);
        }

        public override string ToString()
        {
            return Value ?? string.Empty;
        }

        public static bool TryParse(string value, out DisplayProfileId profileId)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                profileId = default;
                return false;
            }

            profileId = new DisplayProfileId(value);
            return true;
        }

        public static bool operator ==(DisplayProfileId left, DisplayProfileId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(DisplayProfileId left, DisplayProfileId right)
        {
            return !left.Equals(right);
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }
}

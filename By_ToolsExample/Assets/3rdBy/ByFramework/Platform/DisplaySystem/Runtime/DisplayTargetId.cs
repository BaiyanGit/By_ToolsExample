//=====================================================
// 文件名称: DisplayTargetId.cs
// 创建者: Codex
// 创建日期: 2026-06-09
// 描    述: 表达稳定显示目标标识。
//=====================================================

namespace _3rdBy.ByFramework.Platform.DisplaySystem
{
    using System;

    public readonly struct DisplayTargetId : IEquatable<DisplayTargetId>
    {
        public DisplayTargetId(string value)
        {
            Value = Normalize(value);
        }

        public string Value { get; }

        public bool IsEmpty => string.IsNullOrWhiteSpace(Value);

        public bool Equals(DisplayTargetId other)
        {
            return string.Equals(Value, other.Value, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return obj is DisplayTargetId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value == null ? 0 : StringComparer.Ordinal.GetHashCode(Value);
        }

        public override string ToString()
        {
            return Value ?? string.Empty;
        }

        public static bool TryParse(string value, out DisplayTargetId targetId)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                targetId = default;
                return false;
            }

            targetId = new DisplayTargetId(value);
            return true;
        }

        public static bool operator ==(DisplayTargetId left, DisplayTargetId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(DisplayTargetId left, DisplayTargetId right)
        {
            return !left.Equals(right);
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }
}

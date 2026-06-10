//=====================================================
// 文件名称: UIKey.cs
// 创建者: Codex
// 创建日期: 2026-06-09
// 描    述: 表达稳定 UI 标识。
//=====================================================

namespace _3rdBy.ByFramework.Platform.UISystem
{
    using System;

    public readonly struct UIKey : IEquatable<UIKey>
    {
        public UIKey(string value)
        {
            Value = Normalize(value);
        }

        public string Value { get; }

        public bool IsEmpty => string.IsNullOrWhiteSpace(Value);

        public bool Equals(UIKey other)
        {
            return string.Equals(Value, other.Value, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return obj is UIKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value == null ? 0 : StringComparer.Ordinal.GetHashCode(Value);
        }

        public override string ToString()
        {
            return Value ?? string.Empty;
        }

        public static bool TryParse(string value, out UIKey uiKey)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                uiKey = default;
                return false;
            }

            uiKey = new UIKey(value);
            return true;
        }

        public static bool operator ==(UIKey left, UIKey right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(UIKey left, UIKey right)
        {
            return !left.Equals(right);
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }
}

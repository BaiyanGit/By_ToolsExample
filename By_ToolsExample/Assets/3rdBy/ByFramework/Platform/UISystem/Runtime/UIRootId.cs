//=====================================================
// 文件名称: UIRootId.cs
// 创建者: Codex
// 创建日期: 2026-06-09
// 描    述: 表达稳定 UI Root 标识。
//=====================================================

namespace _3rdBy.ByFramework.Platform.UISystem
{
    using System;

    public readonly struct UIRootId : IEquatable<UIRootId>
    {
        public UIRootId(string value)
        {
            Value = Normalize(value);
        }

        public string Value { get; }

        public bool IsEmpty => string.IsNullOrWhiteSpace(Value);

        public bool Equals(UIRootId other)
        {
            return string.Equals(Value, other.Value, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return obj is UIRootId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value == null ? 0 : StringComparer.Ordinal.GetHashCode(Value);
        }

        public override string ToString()
        {
            return Value ?? string.Empty;
        }

        public static bool TryParse(string value, out UIRootId rootId)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                rootId = default;
                return false;
            }

            rootId = new UIRootId(value);
            return true;
        }

        public static bool operator ==(UIRootId left, UIRootId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(UIRootId left, UIRootId right)
        {
            return !left.Equals(right);
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }
}

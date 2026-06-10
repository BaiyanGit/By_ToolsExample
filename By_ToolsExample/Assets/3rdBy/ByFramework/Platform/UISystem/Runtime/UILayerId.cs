//=====================================================
// 文件名称: UILayerId.cs
// 创建者: Codex
// 创建日期: 2026-06-09
// 描    述: 表达稳定 UI Layer 标识。
//=====================================================

namespace _3rdBy.ByFramework.Platform.UISystem
{
    using System;

    public readonly struct UILayerId : IEquatable<UILayerId>
    {
        public UILayerId(string value)
        {
            Value = Normalize(value);
        }

        public string Value { get; }

        public bool IsEmpty => string.IsNullOrWhiteSpace(Value);

        public bool Equals(UILayerId other)
        {
            return string.Equals(Value, other.Value, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return obj is UILayerId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value == null ? 0 : StringComparer.Ordinal.GetHashCode(Value);
        }

        public override string ToString()
        {
            return Value ?? string.Empty;
        }

        public static bool TryParse(string value, out UILayerId layerId)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                layerId = default;
                return false;
            }

            layerId = new UILayerId(value);
            return true;
        }

        public static bool operator ==(UILayerId left, UILayerId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(UILayerId left, UILayerId right)
        {
            return !left.Equals(right);
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }
}

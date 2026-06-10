//=====================================================
// 文件名称: InputContextHandle.cs
// 创 建 者: Codex
// 创建日期: 2026-06-09
// 描    述: 表达输入上下文实例标识。
//=====================================================

namespace _3rdBy.ByFramework.Platform.InputSystem
{
    using System;

    public readonly struct InputContextHandle : IEquatable<InputContextHandle>
    {
        public static readonly InputContextHandle Empty = default;

        public InputContextHandle(string value)
        {
            Value = Normalize(value);
        }

        public string Value { get; }

        public bool IsEmpty => string.IsNullOrWhiteSpace(Value);

        public bool Equals(InputContextHandle other)
        {
            return string.Equals(Value, other.Value, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return obj is InputContextHandle other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value == null ? 0 : StringComparer.Ordinal.GetHashCode(Value);
        }

        public override string ToString()
        {
            return Value ?? string.Empty;
        }

        public static bool operator ==(InputContextHandle left, InputContextHandle right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(InputContextHandle left, InputContextHandle right)
        {
            return !left.Equals(right);
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }
}

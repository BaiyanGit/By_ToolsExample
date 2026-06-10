//=====================================================
// 文件名称: InputActionId.cs
// 创 建 者: Codex
// 创建日期: 2026-06-09
// 描    述: 表达稳定输入动作标识。
//=====================================================

namespace _3rdBy.ByFramework.Platform.InputSystem
{
    using System;

    public readonly struct InputActionId : IEquatable<InputActionId>
    {
        public InputActionId(string value)
        {
            Value = Normalize(value);
        }

        public string Value { get; }

        public bool IsEmpty => string.IsNullOrWhiteSpace(Value);

        public bool Equals(InputActionId other)
        {
            return string.Equals(Value, other.Value, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return obj is InputActionId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value == null ? 0 : StringComparer.Ordinal.GetHashCode(Value);
        }

        public override string ToString()
        {
            return Value ?? string.Empty;
        }

        public static bool operator ==(InputActionId left, InputActionId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(InputActionId left, InputActionId right)
        {
            return !left.Equals(right);
        }

        public static bool TryParse(string value, out InputActionId actionId)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                actionId = default;
                return false;
            }

            actionId = new InputActionId(value);
            return true;
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }
}

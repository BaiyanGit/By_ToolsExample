//=====================================================
// 文件名称: InputActionDescriptor.cs
// 创 建 者: Codex
// 创建日期: 2026-06-09
// 描    述: 描述输入动作元数据。
//=====================================================

namespace _3rdBy.ByFramework.Platform.InputSystem
{
    using System;

    public sealed class InputActionDescriptor
    {
        public InputActionDescriptor(
            InputActionId actionId,
            string scope,
            InputValueKind valueKind,
            bool isPlatformOwned,
            bool isUserRebindAllowed)
        {
            if (actionId.IsEmpty)
            {
                throw new ArgumentException("[InputSystem] ActionId cannot be empty.", nameof(actionId));
            }

            ActionId = actionId;
            Scope = string.IsNullOrWhiteSpace(scope) ? string.Empty : scope.Trim();
            ValueKind = valueKind;
            IsPlatformOwned = isPlatformOwned;
            IsUserRebindAllowed = isUserRebindAllowed;
        }

        public InputActionId ActionId { get; }

        public string Scope { get; }

        public InputValueKind ValueKind { get; }

        public bool IsPlatformOwned { get; }

        public bool IsUserRebindAllowed { get; }
    }
}

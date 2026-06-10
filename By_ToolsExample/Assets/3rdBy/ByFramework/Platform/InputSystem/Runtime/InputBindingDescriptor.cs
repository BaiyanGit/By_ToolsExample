//=====================================================
// 文件名称: InputBindingDescriptor.cs
// 创 建 者: Codex
// 创建日期: 2026-06-09
// 描    述: 描述输入绑定关系。
//=====================================================

namespace _3rdBy.ByFramework.Platform.InputSystem
{
    using System;

    public sealed class InputBindingDescriptor
    {
        public InputBindingDescriptor(
            InputActionId actionId,
            InputDeviceKind deviceKind,
            string bindingPath,
            bool isComposite)
        {
            if (actionId.IsEmpty)
            {
                throw new ArgumentException("[InputSystem] ActionId cannot be empty.", nameof(actionId));
            }

            ActionId = actionId;
            DeviceKind = deviceKind;
            BindingPath = string.IsNullOrWhiteSpace(bindingPath) ? string.Empty : bindingPath.Trim();
            IsComposite = isComposite;
        }

        public InputActionId ActionId { get; }

        public InputDeviceKind DeviceKind { get; }

        public string BindingPath { get; }

        public bool IsComposite { get; }
    }
}

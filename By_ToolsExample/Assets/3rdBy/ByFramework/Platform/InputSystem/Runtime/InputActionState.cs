//=====================================================
// 文件名称: InputActionState.cs
// 创 建 者: Codex
// 创建日期: 2026-06-09
// 描    述: 表达输入动作最近一次已知状态。
//=====================================================

namespace _3rdBy.ByFramework.Platform.InputSystem
{
    using System;

    public sealed class InputActionState
    {
        public InputActionState(
            InputActionId actionId,
            InputStage stage,
            InputValueKind valueKind,
            bool isAvailable)
        {
            if (actionId.IsEmpty)
            {
                throw new ArgumentException("[InputSystem] ActionId cannot be empty.", nameof(actionId));
            }

            ActionId = actionId;
            Stage = stage;
            ValueKind = valueKind;
            IsAvailable = isAvailable;
        }

        public InputActionId ActionId { get; }

        public InputStage Stage { get; }

        public InputValueKind ValueKind { get; }

        public bool IsAvailable { get; }
    }
}

//=====================================================
// 文件名称: InputActionEvent.cs
// 创 建 者: Codex
// 创建日期: 2026-06-09
// 描    述: 表达一次已解析输入动作事实。
//=====================================================

namespace _3rdBy.ByFramework.Platform.InputSystem
{
    public readonly struct InputActionEvent
    {
        public InputActionEvent(
            InputActionId actionId,
            InputStage stage,
            InputValueKind valueKind,
            InputContextHandle contextHandle,
            InputActionResolveStatus status)
        {
            ActionId = actionId;
            Stage = stage;
            ValueKind = valueKind;
            ContextHandle = contextHandle;
            Status = status;
        }

        public InputActionId ActionId { get; }

        public InputStage Stage { get; }

        public InputValueKind ValueKind { get; }

        public InputContextHandle ContextHandle { get; }

        public InputActionResolveStatus Status { get; }
    }
}

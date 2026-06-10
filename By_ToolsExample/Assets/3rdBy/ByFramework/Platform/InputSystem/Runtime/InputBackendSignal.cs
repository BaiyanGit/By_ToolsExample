//=====================================================
// 文件名称: InputBackendSignal.cs
// 创 建 者: Codex
// 创建日期: 2026-06-09
// 描    述: 表达后端上报的一次原始动作信号。
//=====================================================

namespace _3rdBy.ByFramework.Platform.InputSystem
{
    internal readonly struct InputBackendSignal
    {
        public InputBackendSignal(InputActionId actionId, InputStage stage, InputValueKind valueKind)
        {
            ActionId = actionId;
            Stage = stage;
            ValueKind = valueKind;
        }

        public InputActionId ActionId { get; }

        public InputStage Stage { get; }

        public InputValueKind ValueKind { get; }
    }
}

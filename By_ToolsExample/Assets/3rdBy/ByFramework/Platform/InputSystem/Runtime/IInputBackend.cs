//=====================================================
// 文件名称: IInputBackend.cs
// 创 建 者: Codex
// 创建日期: 2026-06-09
// 描    述: 定义 InputSystem Runtime 内部后端契约。
//=====================================================

namespace _3rdBy.ByFramework.Platform.InputSystem
{
    using System;
    using System.Collections.Generic;

    internal interface IInputBackend
    {
        event Action<InputBackendSignal> SignalPublished;

        bool IsAvailable { get; }

        string ProviderType { get; }

        IReadOnlyList<InputActionDescriptor> GetAvailableActions();

        bool TryGetActionDescriptor(InputActionId actionId, out InputActionDescriptor descriptor);

        bool TryGetActionState(InputActionId actionId, out InputActionState state);

        void SetResolvedState(InputActionId actionId, InputStage stage, InputValueKind valueKind, bool isAvailable);

        IReadOnlyList<InputDeviceDescriptor> GetDevices();

        InputProfileSnapshot GetProfileSnapshot();

        bool HasConnectedDevice();
    }
}

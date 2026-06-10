//=====================================================
// 文件名称: IInputService.cs
// 创 建 者: Codex
// 创建日期: 2026-06-09
// 描    述: 定义 InputSystem Runtime 服务接口。
//=====================================================

namespace _3rdBy.ByFramework.Platform.InputSystem
{
    using System;
    using System.Collections.Generic;

    public interface IInputService : _3rdBy.ByFramework.Platform.PlatformServiceRegistry.IPlatformService
    {
        event Action<InputActionEvent> ActionTriggered;

        InputContextSnapshot GetCurrentContextSnapshot();

        IReadOnlyList<InputContextHandle> GetActiveContexts();

        IReadOnlyList<InputActionDescriptor> GetAvailableActions();

        bool TryGetActionDescriptor(InputActionId actionId, out InputActionDescriptor descriptor);

        bool IsActionAvailable(InputActionId actionId);

        InputContextHandle ActivateContext(InputContextRegistration registration);

        bool TryReleaseContext(InputContextHandle handle);

        bool TryResolveActionState(InputActionId actionId, out InputActionState state);
    }
}

//=====================================================
// 文件名称: IUIService.cs
// 创建者: Codex
// 创建日期: 2026-06-09
// 描    述: 定义 UISystem Runtime 服务接口。
//=====================================================

namespace _3rdBy.ByFramework.Platform.UISystem
{
    using System;
    using System.Collections.Generic;

    public interface IUIService : _3rdBy.ByFramework.Platform.PlatformServiceRegistry.IPlatformService
    {
        UIContext GetCurrentContext();

        IReadOnlyList<UIRootId> GetRegisteredRoots();

        IReadOnlyList<UILayerId> GetAvailableLayers();

        IReadOnlyList<UIViewDescriptor> GetOpenViews();

        bool TryGetView(
            UIKey uiKey,
            out UIViewDescriptor view,
            out UIQueryStatus status);

        UIOperationResult Open(
            UIKey uiKey,
            UIOpenOptions options = null);

        UIOperationResult Close(
            UIKey uiKey,
            UICloseReason reason = UICloseReason.Programmatic);

        event Action<UIContextChangedEvent> ContextChanged;
    }
}

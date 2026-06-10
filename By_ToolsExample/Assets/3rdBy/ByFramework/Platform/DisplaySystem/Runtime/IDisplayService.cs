//=====================================================
// 文件名称: IDisplayService.cs
// 创建者: Codex
// 创建日期: 2026-06-09
// 描    述: 定义 DisplaySystem Runtime 服务接口。
//=====================================================

namespace _3rdBy.ByFramework.Platform.DisplaySystem
{
    using System;
    using System.Collections.Generic;

    public interface IDisplayService : _3rdBy.ByFramework.Platform.PlatformServiceRegistry.IPlatformService
    {
        DisplayContext GetCurrentDisplayContext();

        IReadOnlyList<DisplayProfileId> GetAvailableProfiles();

        bool TryGetProfile(
            DisplayProfileId profileId,
            out DisplayProfileSnapshot profile,
            out DisplayQueryStatus status);

        IReadOnlyList<DisplayTargetDescriptor> GetAvailableTargets();

        bool TryGetTarget(
            DisplayTargetId targetId,
            out DisplayTargetDescriptor target,
            out DisplayQueryStatus status);

        DisplayMode GetCurrentMode();

        DisplayApplyResult ApplyProfile(
            DisplayProfileId profileId,
            bool persistPreference = true);

        event Action<DisplayContextChangedEvent> DisplayContextChanged;
    }
}

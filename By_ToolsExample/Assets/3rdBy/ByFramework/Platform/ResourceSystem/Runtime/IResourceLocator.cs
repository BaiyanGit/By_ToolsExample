//=====================================================
// 文件名称: IResourceLocator.cs
// 创建作者: Codex
// 创建日期: 2026-06-08
// 描    述: 定义 ResourceSystem 资源定位接口。
//=====================================================

namespace _3rdBy.ByFramework.Platform.ResourceSystem
{
    using System.Collections.Generic;

    /// <summary>
    /// 资源定位接口。
    /// </summary>
    public interface IResourceLocator
    {
        /// <summary>
        /// 获取当前资源清单快照。
        /// </summary>
        ResourceManifestSnapshot ManifestSnapshot { get; }

        /// <summary>
        /// 强制获取资源位置。
        /// </summary>
        ResourceLocation GetLocation(string resourceKey);

        /// <summary>
        /// 安全获取资源位置。
        /// </summary>
        bool TryGetLocation(string resourceKey, out ResourceLocation location);

        /// <summary>
        /// 获取指定分组的资源位置集合。
        /// </summary>
        IReadOnlyList<ResourceLocation> GetLocationsByGroup(string group);

        /// <summary>
        /// 获取全部可用分组。
        /// </summary>
        IReadOnlyList<string> GetAvailableGroups();
    }
}

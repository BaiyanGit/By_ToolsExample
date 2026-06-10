//=====================================================
// 文件名称: IResourceService.cs
// 创建作者: Codex
// 创建日期: 2026-06-08
// 描    述: 定义 ResourceSystem Runtime 服务接口。
//=====================================================

namespace _3rdBy.ByFramework.Platform.ResourceSystem
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using UnityObject = UnityEngine.Object;

    /// <summary>
    /// ResourceSystem Runtime 服务接口。
    /// </summary>
    public interface IResourceService : _3rdBy.ByFramework.Platform.PlatformServiceRegistry.IPlatformService
    {
        /// <summary>
        /// 获取当前资源定位器。
        /// </summary>
        IResourceLocator Locator { get; }

        /// <summary>
        /// 获取当前资源清单快照。
        /// </summary>
        ResourceManifestSnapshot ManifestSnapshot { get; }

        /// <summary>
        /// 强制加载指定资源。
        /// </summary>
        ResourceResult<TAsset> Load<TAsset>(ResourceRequest request) where TAsset : UnityObject;

        /// <summary>
        /// 安全加载指定资源。
        /// </summary>
        bool TryLoad<TAsset>(ResourceRequest request, out ResourceResult<TAsset> result) where TAsset : UnityObject;

        /// <summary>
        /// 异步加载指定资源。
        /// </summary>
        Task<ResourceResult<TAsset>> LoadAsync<TAsset>(ResourceRequest request, CancellationToken cancellationToken = default)
            where TAsset : UnityObject;

        /// <summary>
        /// 释放资源句柄。
        /// </summary>
        void Release(ResourceHandle handle);

        /// <summary>
        /// 判断指定资源是否存在。
        /// </summary>
        bool Exists(string resourceKey);

        /// <summary>
        /// 判断指定请求对应资源是否存在。
        /// </summary>
        bool Exists(ResourceRequest request);

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

        /// <summary>
        /// 获取当前运行时缓存快照。
        /// </summary>
        IReadOnlyList<ResourceCacheEntry> GetRuntimeCacheEntries();

        /// <summary>
        /// 清空运行时缓存。
        /// </summary>
        void ClearRuntimeCache();
    }
}

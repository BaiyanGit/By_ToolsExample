//=====================================================
// 文件名称: ResourcesProvider.cs
// 创建作者: Codex
// 创建日期: 2026-06-08
// 描    述: 实现基于 Unity Resources 的资源 Provider。
//=====================================================

namespace _3rdBy.ByFramework.Platform.ResourceSystem
{
    using System.Threading;
    using System.Threading.Tasks;
    using UnityEngine;
    using UnityObject = UnityEngine.Object;

    /// <summary>
    /// 基于 Unity Resources 的资源 Provider。
    /// </summary>
    public sealed class ResourcesProvider : IResourceProvider
    {
        /// <summary>
        /// 获取 Provider 类型标识。
        /// </summary>
        public string ProviderType => nameof(ResourcesProvider);

        /// <summary>
        /// 判断当前 Provider 是否可处理指定位置。
        /// </summary>
        public bool CanHandle(ResourceLocation location)
        {
            return location != null
                   && string.Equals(location.ProviderType, ProviderType, System.StringComparison.Ordinal);
        }

        /// <summary>
        /// 同步加载资源。
        /// </summary>
        public ResourceResult<TAsset> Load<TAsset>(ResourceRequest request, ResourceLocation location) where TAsset : UnityObject
        {
            if (request == null)
            {
                throw new System.ArgumentNullException(nameof(request));
            }

            if (location == null)
            {
                throw new System.ArgumentNullException(nameof(location));
            }

            string resourcePath = NormalizeResourcePath(location.Address);
            TAsset asset = Resources.Load<TAsset>(resourcePath);
            if (asset == null)
            {
                return new ResourceResult<TAsset>(
                    false,
                    null,
                    request.ResourceKey,
                    ProviderType,
                    resourcePath,
                    location,
                    null,
                    "ResourceNotFound",
                    $"[ResourceSystem] ResourcesProvider 未找到资源：{resourcePath}");
            }

            return new ResourceResult<TAsset>(true, asset, request.ResourceKey, ProviderType, resourcePath, location);
        }

        /// <summary>
        /// 异步加载资源。
        /// </summary>
        public async Task<ResourceResult<TAsset>> LoadAsync<TAsset>(
            ResourceRequest request,
            ResourceLocation location,
            CancellationToken cancellationToken = default) where TAsset : UnityObject
        {
            if (request == null)
            {
                throw new System.ArgumentNullException(nameof(request));
            }

            if (location == null)
            {
                throw new System.ArgumentNullException(nameof(location));
            }

            string resourcePath = NormalizeResourcePath(location.Address);
            global::UnityEngine.ResourceRequest loadRequest = Resources.LoadAsync<TAsset>(resourcePath);
            while (!loadRequest.isDone)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await Task.Yield();
            }

            TAsset asset = loadRequest.asset as TAsset;
            if (asset == null)
            {
                return new ResourceResult<TAsset>(
                    false,
                    null,
                    request.ResourceKey,
                    ProviderType,
                    resourcePath,
                    location,
                    null,
                    "ResourceNotFound",
                    $"[ResourceSystem] ResourcesProvider 未找到资源：{resourcePath}");
            }

            return new ResourceResult<TAsset>(true, asset, request.ResourceKey, ProviderType, resourcePath, location);
        }

        /// <summary>
        /// 判断资源是否存在。
        /// </summary>
        public bool Exists(ResourceLocation location)
        {
            if (location == null)
            {
                throw new System.ArgumentNullException(nameof(location));
            }

            string resourcePath = NormalizeResourcePath(location.Address);
            UnityObject asset = Resources.Load(resourcePath);
            return asset != null;
        }

        /// <summary>
        /// 释放资源。
        /// </summary>
        public void Release(ResourceHandle handle)
        {
            if (handle == null)
            {
                throw new System.ArgumentNullException(nameof(handle));
            }

            // ResourcesProvider 不主动卸载资源实例，避免误卸载共享 prefab 或场景资源。
        }

        private static string NormalizeResourcePath(string address)
        {
            string path = (address ?? string.Empty).Replace("\\", "/");
            while (path.StartsWith("/", System.StringComparison.Ordinal))
            {
                path = path.Substring(1);
            }

            if (path.EndsWith(".prefab", System.StringComparison.OrdinalIgnoreCase)
                || path.EndsWith(".asset", System.StringComparison.OrdinalIgnoreCase)
                || path.EndsWith(".png", System.StringComparison.OrdinalIgnoreCase)
                || path.EndsWith(".jpg", System.StringComparison.OrdinalIgnoreCase)
                || path.EndsWith(".txt", System.StringComparison.OrdinalIgnoreCase)
                || path.EndsWith(".json", System.StringComparison.OrdinalIgnoreCase))
            {
                int separatorIndex = path.LastIndexOf('.');
                if (separatorIndex > 0)
                {
                    path = path.Substring(0, separatorIndex);
                }
            }

            return path;
        }
    }
}

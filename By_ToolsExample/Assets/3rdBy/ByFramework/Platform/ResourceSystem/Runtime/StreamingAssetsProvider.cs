//=====================================================
// 文件名称: StreamingAssetsProvider.cs
// 创建作者: Codex
// 创建日期: 2026-06-08
// 描    述: 实现基于 StreamingAssets 的资源 Provider。
//=====================================================

namespace _3rdBy.ByFramework.Platform.ResourceSystem
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using UnityObject = UnityEngine.Object;

    /// <summary>
    /// 基于 StreamingAssets 的资源 Provider。
    /// </summary>
    public sealed class StreamingAssetsProvider : IResourceProvider
    {
        /// <summary>
        /// 获取 Provider 类型标识。
        /// </summary>
        public string ProviderType => nameof(StreamingAssetsProvider);

        /// <summary>
        /// 判断当前 Provider 是否可处理指定位置。
        /// </summary>
        public bool CanHandle(ResourceLocation location)
        {
            return location != null
                   && string.Equals(location.ProviderType, ProviderType, StringComparison.Ordinal);
        }

        /// <summary>
        /// 同步加载资源。
        /// </summary>
        public ResourceResult<TAsset> Load<TAsset>(ResourceRequest request, ResourceLocation location) where TAsset : UnityObject
        {
            return LoadCore<TAsset>(request, location);
        }

        /// <summary>
        /// 异步加载资源。
        /// </summary>
        public Task<ResourceResult<TAsset>> LoadAsync<TAsset>(
            ResourceRequest request,
            ResourceLocation location,
            CancellationToken cancellationToken = default) where TAsset : UnityObject
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(LoadCore<TAsset>(request, location));
        }

        /// <summary>
        /// 判断资源是否存在。
        /// </summary>
        public bool Exists(ResourceLocation location)
        {
            if (location == null)
            {
                throw new ArgumentNullException(nameof(location));
            }

            return File.Exists(ResolvePath(location.Address));
        }

        /// <summary>
        /// 释放资源。
        /// </summary>
        public void Release(ResourceHandle handle)
        {
            if (handle == null)
            {
                throw new ArgumentNullException(nameof(handle));
            }

            LocalFileProvider.ReleaseDynamicAsset(handle.Asset);
        }

        private ResourceResult<TAsset> LoadCore<TAsset>(ResourceRequest request, ResourceLocation location) where TAsset : UnityObject
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (location == null)
            {
                throw new ArgumentNullException(nameof(location));
            }

            string resolvedPath = ResolvePath(location.Address);
            ResourceLocation resolvedLocation = new(
                location.ResourceKey,
                ProviderType,
                resolvedPath,
                location.Group,
                location.AssetTypeName,
                location.IsOptional,
                location.Tags);

            LocalFileProvider localFileProvider = new();
            ResourceResult<TAsset> localResult = localFileProvider.Load<TAsset>(request, resolvedLocation);
            if (!localResult.Success)
            {
                return new ResourceResult<TAsset>(
                    false,
                    null,
                    request.ResourceKey,
                    ProviderType,
                    resolvedPath,
                    resolvedLocation,
                    null,
                    localResult.ErrorCode,
                    localResult.ErrorMessage);
            }

            return new ResourceResult<TAsset>(
                true,
                localResult.Asset,
                request.ResourceKey,
                ProviderType,
                resolvedPath,
                resolvedLocation);
        }

        private static string ResolvePath(string address)
        {
            if (Path.IsPathRooted(address))
            {
                return address;
            }

            string relativePath = (address ?? string.Empty).Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
            return Path.Combine(UnityEngine.Application.streamingAssetsPath, relativePath);
        }
    }
}

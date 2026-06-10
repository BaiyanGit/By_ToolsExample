//=====================================================
// 文件名称: LocalFileProvider.cs
// 创建作者: Codex
// 创建日期: 2026-06-08
// 描    述: 实现基于本地文件系统的资源 Provider。
//=====================================================

namespace _3rdBy.ByFramework.Platform.ResourceSystem
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using UnityEngine;
    using UnityObject = UnityEngine.Object;

    /// <summary>
    /// 基于本地文件系统的资源 Provider。
    /// </summary>
    public sealed class LocalFileProvider : IResourceProvider
    {
        /// <summary>
        /// 获取 Provider 类型标识。
        /// </summary>
        public string ProviderType => nameof(LocalFileProvider);

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
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (location == null)
            {
                throw new ArgumentNullException(nameof(location));
            }

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

            return File.Exists(location.Address);
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

            ReleaseDynamicAsset(handle.Asset);
        }

        private ResourceResult<TAsset> LoadCore<TAsset>(ResourceRequest request, ResourceLocation location) where TAsset : UnityObject
        {
            string fullPath = location.Address;
            if (!File.Exists(fullPath))
            {
                return new ResourceResult<TAsset>(
                    false,
                    null,
                    request.ResourceKey,
                    ProviderType,
                    fullPath,
                    location,
                    null,
                    "FileNotFound",
                    $"[ResourceSystem] LocalFileProvider 未找到文件：{fullPath}");
            }

            if (typeof(TAsset) == typeof(TextAsset))
            {
                string text = File.ReadAllText(fullPath);
                TextAsset textAsset = new(text);
                return new ResourceResult<TAsset>(true, textAsset as TAsset, request.ResourceKey, ProviderType, fullPath, location);
            }

            if (typeof(TAsset) == typeof(Texture2D))
            {
                byte[] bytes = File.ReadAllBytes(fullPath);
                Texture2D texture = new(2, 2, TextureFormat.RGBA32, false);
                if (!texture.LoadImage(bytes))
                {
                    ReleaseDynamicAsset(texture);
                    return new ResourceResult<TAsset>(
                        false,
                        null,
                        request.ResourceKey,
                        ProviderType,
                        fullPath,
                        location,
                        null,
                        "LoadImageFailed",
                        $"[ResourceSystem] LocalFileProvider 无法解析图片：{fullPath}");
                }

                return new ResourceResult<TAsset>(true, texture as TAsset, request.ResourceKey, ProviderType, fullPath, location);
            }

            return new ResourceResult<TAsset>(
                false,
                null,
                request.ResourceKey,
                ProviderType,
                fullPath,
                location,
                null,
                "UnsupportedAssetType",
                $"[ResourceSystem] LocalFileProvider 暂不支持资源类型：{typeof(TAsset).FullName}");
        }

        internal static void ReleaseDynamicAsset(UnityObject asset)
        {
            if (asset == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                UnityObject.Destroy(asset);
                return;
            }

            UnityObject.DestroyImmediate(asset);
        }
    }
}

//=====================================================
// 文件名称: AssetBundleProviderAdapter.cs
// 创建作者: Codex
// 创建日期: 2026-06-09
// 描    述: 实现 AssetBundle 到 ResourceSystem 的 Provider 适配层。
//=====================================================

namespace _3rdBy.ByFramework.Platform.ResourceSystem.AssetBundleSystem
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using _3rdBy.ByFramework.Platform.ResourceSystem;
    using UnityObject = UnityEngine.Object;

    /// <summary>
    /// AssetBundle 到 ResourceSystem 的 Provider 适配层。
    /// </summary>
    public sealed class AssetBundleProviderAdapter : IAssetBundleProviderAdapter
    {
        private const string AssetBundleProviderType = "AssetBundleProvider";

        /// <summary>
        /// 初始化 Provider 适配层。
        /// </summary>
        public AssetBundleProviderAdapter(IAssetBundleService assetBundleService)
        {
            AssetBundleService = assetBundleService ?? throw new ArgumentNullException(nameof(assetBundleService));
        }

        /// <summary>
        /// 获取 Provider 类型。
        /// </summary>
        public string ProviderType => AssetBundleProviderType;

        /// <summary>
        /// 获取 AssetBundle 服务。
        /// </summary>
        public IAssetBundleService AssetBundleService { get; }

        /// <summary>
        /// 判断是否可处理指定位置。
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

            if (!AssetBundleService.Manifest.TryGetAssetEntry(location.ResourceKey, out AssetBundleAssetEntry assetEntry))
            {
                return new ResourceResult<TAsset>(
                    false,
                    null,
                    request.ResourceKey,
                    ProviderType,
                    string.Empty,
                    location,
                    null,
                    "AssetEntryNotFound",
                    $"[AssetBundle] 未找到资源条目：{location.ResourceKey}");
            }

            if (!AssetBundleService.TryLoadAsset(assetEntry.BundleName, assetEntry.AssetName, out TAsset asset))
            {
                return new ResourceResult<TAsset>(
                    false,
                    null,
                    request.ResourceKey,
                    ProviderType,
                    BuildResolvedAddress(assetEntry),
                    location,
                    null,
                    "LoadAssetFailed",
                    $"[AssetBundle] 资源加载失败：{assetEntry.BundleName}/{assetEntry.AssetName}");
            }

            return new ResourceResult<TAsset>(
                true,
                asset,
                request.ResourceKey,
                ProviderType,
                BuildResolvedAddress(assetEntry),
                location);
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
            return Task.FromResult(Load<TAsset>(request, location));
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

            if (!AssetBundleService.Manifest.TryGetAssetEntry(location.ResourceKey, out AssetBundleAssetEntry assetEntry))
            {
                return false;
            }

            return AssetBundleService.Locator.TryGetBundlePath(assetEntry.BundleName, out string bundlePath)
                   && System.IO.File.Exists(bundlePath);
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

            if (!TryParseResolvedAddress(handle.ResolvedAddress, out string bundleName, out string assetName))
            {
                return;
            }

            AssetBundleService.ReleaseAssetReference(bundleName, assetName);
        }

        private static string BuildResolvedAddress(AssetBundleAssetEntry assetEntry)
        {
            return $"{assetEntry.BundleName}::{assetEntry.AssetName}";
        }

        private static bool TryParseResolvedAddress(string resolvedAddress, out string bundleName, out string assetName)
        {
            bundleName = string.Empty;
            assetName = string.Empty;
            if (string.IsNullOrWhiteSpace(resolvedAddress))
            {
                return false;
            }

            string[] parts = resolvedAddress.Split(new[] { "::" }, StringSplitOptions.None);
            if (parts.Length != 2)
            {
                return false;
            }

            bundleName = parts[0];
            assetName = parts[1];
            return !string.IsNullOrWhiteSpace(bundleName) && !string.IsNullOrWhiteSpace(assetName);
        }
    }
}

//=====================================================
// 文件名称: ResourceService.cs
// 创建作者: Codex
// 创建日期: 2026-06-08
// 描    述: 实现 ResourceSystem Runtime 的统一资源访问服务。
//=====================================================

namespace _3rdBy.ByFramework.Platform.ResourceSystem
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Threading;
    using System.Threading.Tasks;
    using UnityObject = UnityEngine.Object;

    /// <summary>
    /// ResourceSystem Runtime 的统一资源访问服务。
    /// </summary>
    public sealed class ResourceService : IResourceService
    {
        private readonly object _syncRoot = new();
        private readonly Dictionary<string, IResourceProvider> _providers;
        private readonly Dictionary<string, ResourceCacheEntry> _cacheEntries;

        /// <summary>
        /// 初始化 ResourceSystem Runtime 服务。
        /// </summary>
        public ResourceService(
            IResourceLocator locator = null,
            IReadOnlyList<IResourceProvider> providers = null,
            ResourceManifestSnapshot manifestSnapshot = null)
        {
            _providers = BuildProviderMap(providers ?? CreateDefaultProviders());
            _cacheEntries = new Dictionary<string, ResourceCacheEntry>(StringComparer.Ordinal);
            Locator = locator ?? new ManifestResourceLocator(manifestSnapshot ?? ResourceManifestSnapshot.Empty);
        }

        /// <summary>
        /// 获取当前资源定位器。
        /// </summary>
        public IResourceLocator Locator { get; }

        /// <summary>
        /// 获取当前资源清单快照。
        /// </summary>
        public ResourceManifestSnapshot ManifestSnapshot => Locator.ManifestSnapshot;

        /// <summary>
        /// 强制加载指定资源。
        /// </summary>
        public ResourceResult<TAsset> Load<TAsset>(ResourceRequest request) where TAsset : UnityObject
        {
            if (TryLoad(request, out ResourceResult<TAsset> result))
            {
                return result;
            }

            throw new InvalidOperationException($"[ResourceSystem] 资源加载失败：{result.ErrorMessage}");
        }

        /// <summary>
        /// 安全加载指定资源。
        /// </summary>
        public bool TryLoad<TAsset>(ResourceRequest request, out ResourceResult<TAsset> result) where TAsset : UnityObject
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            try
            {
                lock (_syncRoot)
                {
                    return TryLoadCore(request, out result);
                }
            }
            catch (Exception exception)
            {
                result = new ResourceResult<TAsset>(
                    false,
                    null,
                    request.ResourceKey,
                    request.ProviderHint,
                    string.Empty,
                    null,
                    null,
                    "TryLoadFailed",
                    $"[ResourceSystem] 资源加载失败：{exception.Message}");
                return false;
            }
        }

        /// <summary>
        /// 异步加载指定资源。
        /// </summary>
        public async Task<ResourceResult<TAsset>> LoadAsync<TAsset>(
            ResourceRequest request,
            CancellationToken cancellationToken = default) where TAsset : UnityObject
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            ResourceLocation location;
            IResourceProvider provider;
            ResourceCacheEntry cachedEntry;
            string cacheKey;

            lock (_syncRoot)
            {
                location = NormalizeLocationForRequest(request);
                provider = ResolveProvider(location);
                cacheKey = BuildCacheKey(location, typeof(TAsset));
                cachedEntry = TryGetUsableCacheEntry(cacheKey, typeof(TAsset));
                if (cachedEntry != null)
                {
                    ResourceHandle cachedHandle = CreateCachedHandle(cachedEntry);
                    return BuildSuccessResult(location, cachedEntry.ProviderType, cachedEntry.ResolvedAddress, cachedEntry.Asset as TAsset, cachedHandle);
                }
            }

            ResourceResult<TAsset> providerResult = await provider.LoadAsync<TAsset>(request, location, cancellationToken);
            if (!providerResult.Success || providerResult.Asset == null)
            {
                return providerResult;
            }

            lock (_syncRoot)
            {
                if (!request.CachePolicy.EnableRuntimeCache)
                {
                    ResourceHandle nonCachedHandle = CreateNonCachedHandle(
                        location,
                        providerResult.ProviderType,
                        providerResult.ResolvedAddress,
                        request.CachePolicy,
                        providerResult.Asset);
                    return BuildSuccessResult(location, providerResult.ProviderType, providerResult.ResolvedAddress, providerResult.Asset, nonCachedHandle);
                }

                ResourceCacheEntry entry = new(
                    cacheKey,
                    providerResult.ProviderType,
                    providerResult.ResolvedAddress,
                    location,
                    request.CachePolicy,
                    providerResult.Asset,
                    typeof(TAsset));
                _cacheEntries[cacheKey] = entry;
                ResourceHandle handle = CreateCachedHandle(entry);
                return BuildSuccessResult(location, providerResult.ProviderType, providerResult.ResolvedAddress, providerResult.Asset, handle);
            }
        }

        /// <summary>
        /// 释放资源句柄。
        /// </summary>
        public void Release(ResourceHandle handle)
        {
            if (handle == null)
            {
                throw new ArgumentNullException(nameof(handle));
            }

            handle.Release();
        }

        /// <summary>
        /// 判断指定资源是否存在。
        /// </summary>
        public bool Exists(string resourceKey)
        {
            if (string.IsNullOrWhiteSpace(resourceKey))
            {
                throw new ArgumentException("[ResourceSystem] 资源 Key 不能为空。", nameof(resourceKey));
            }

            return Exists(new ResourceRequest(resourceKey));
        }

        /// <summary>
        /// 判断指定请求对应资源是否存在。
        /// </summary>
        public bool Exists(ResourceRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            lock (_syncRoot)
            {
                ResourceLocation location = NormalizeLocationForRequest(request);
                IResourceProvider provider = ResolveProvider(location);
                return provider.Exists(location);
            }
        }

        /// <summary>
        /// 强制获取资源位置。
        /// </summary>
        public ResourceLocation GetLocation(string resourceKey)
        {
            return Locator.GetLocation(resourceKey);
        }

        /// <summary>
        /// 安全获取资源位置。
        /// </summary>
        public bool TryGetLocation(string resourceKey, out ResourceLocation location)
        {
            return Locator.TryGetLocation(resourceKey, out location);
        }

        /// <summary>
        /// 获取指定分组的资源位置集合。
        /// </summary>
        public IReadOnlyList<ResourceLocation> GetLocationsByGroup(string group)
        {
            return Locator.GetLocationsByGroup(group);
        }

        /// <summary>
        /// 获取全部可用分组。
        /// </summary>
        public IReadOnlyList<string> GetAvailableGroups()
        {
            return Locator.GetAvailableGroups();
        }

        /// <summary>
        /// 获取当前运行时缓存快照。
        /// </summary>
        public IReadOnlyList<ResourceCacheEntry> GetRuntimeCacheEntries()
        {
            lock (_syncRoot)
            {
                return new ReadOnlyCollection<ResourceCacheEntry>(new List<ResourceCacheEntry>(_cacheEntries.Values));
            }
        }

        /// <summary>
        /// 清空运行时缓存。
        /// </summary>
        public void ClearRuntimeCache()
        {
            lock (_syncRoot)
            {
                foreach (KeyValuePair<string, ResourceCacheEntry> pair in _cacheEntries)
                {
                    ReleaseEntryAsset(pair.Value);
                }

                _cacheEntries.Clear();
            }
        }

        private static IReadOnlyList<IResourceProvider> CreateDefaultProviders()
        {
            return new IResourceProvider[]
            {
                new ResourcesProvider(),
                new LocalFileProvider(),
                new StreamingAssetsProvider(),
            };
        }

        private static Dictionary<string, IResourceProvider> BuildProviderMap(IReadOnlyList<IResourceProvider> providers)
        {
            Dictionary<string, IResourceProvider> map = new(StringComparer.Ordinal);
            for (int index = 0; index < providers.Count; index++)
            {
                IResourceProvider provider = providers[index];
                if (provider == null)
                {
                    continue;
                }

                map[provider.ProviderType] = provider;
            }

            return map;
        }

        private bool TryLoadCore<TAsset>(ResourceRequest request, out ResourceResult<TAsset> result) where TAsset : UnityObject
        {
            ResourceLocation location = NormalizeLocationForRequest(request);
            IResourceProvider provider = ResolveProvider(location);
            string cacheKey = BuildCacheKey(location, typeof(TAsset));
            ResourceCacheEntry cachedEntry = TryGetUsableCacheEntry(cacheKey, typeof(TAsset));
            if (cachedEntry != null)
            {
                ResourceHandle cachedHandle = CreateCachedHandle(cachedEntry);
                result = BuildSuccessResult(
                    location,
                    cachedEntry.ProviderType,
                    cachedEntry.ResolvedAddress,
                    cachedEntry.Asset as TAsset,
                    cachedHandle);
                return true;
            }

            ResourceResult<TAsset> providerResult = provider.Load<TAsset>(request, location);
            if (!providerResult.Success || providerResult.Asset == null)
            {
                result = providerResult;
                return false;
            }

            if (!request.CachePolicy.EnableRuntimeCache)
            {
                ResourceHandle nonCachedHandle = CreateNonCachedHandle(
                    location,
                    providerResult.ProviderType,
                    providerResult.ResolvedAddress,
                    request.CachePolicy,
                    providerResult.Asset);
                result = BuildSuccessResult(location, providerResult.ProviderType, providerResult.ResolvedAddress, providerResult.Asset, nonCachedHandle);
                return true;
            }

            ResourceCacheEntry entry = new(
                cacheKey,
                providerResult.ProviderType,
                providerResult.ResolvedAddress,
                location,
                request.CachePolicy,
                providerResult.Asset,
                typeof(TAsset));
            _cacheEntries[cacheKey] = entry;
            ResourceHandle handle = CreateCachedHandle(entry);
            result = BuildSuccessResult(location, providerResult.ProviderType, providerResult.ResolvedAddress, providerResult.Asset, handle);
            return true;
        }

        private ResourceLocation ResolveLocation(ResourceRequest request)
        {
            if (Locator.TryGetLocation(request.ResourceKey, out ResourceLocation location))
            {
                return location;
            }

            if (!string.IsNullOrWhiteSpace(request.ProviderHint))
            {
                return new ResourceLocation(request.ResourceKey, request.ProviderHint, request.ResourceKey, request.Group);
            }

            throw new InvalidOperationException($"[ResourceSystem] 未找到资源定位信息：{request.ResourceKey}");
        }

        private ResourceLocation NormalizeLocationForRequest(ResourceRequest request)
        {
            ResourceLocation location = ResolveLocation(request);
            if (string.IsNullOrWhiteSpace(request.ProviderHint)
                || string.Equals(location.ProviderType, request.ProviderHint, StringComparison.Ordinal))
            {
                return location;
            }

            return new ResourceLocation(
                location.ResourceKey,
                request.ProviderHint,
                location.Address,
                string.IsNullOrWhiteSpace(request.Group) ? location.Group : request.Group,
                location.AssetTypeName,
                location.IsOptional,
                location.Tags);
        }

        private IResourceProvider ResolveProvider(ResourceLocation location)
        {
            if (!_providers.TryGetValue(location.ProviderType, out IResourceProvider provider))
            {
                throw new InvalidOperationException($"[ResourceSystem] 未找到资源 Provider：{location.ProviderType}");
            }

            if (!provider.CanHandle(location))
            {
                throw new InvalidOperationException(
                    $"[ResourceSystem] Provider 无法处理资源位置：{location.ProviderType} -> {location.ResourceKey}");
            }

            return provider;
        }

        private ResourceCacheEntry TryGetUsableCacheEntry(string cacheKey, Type assetType)
        {
            if (!_cacheEntries.TryGetValue(cacheKey, out ResourceCacheEntry entry))
            {
                return null;
            }

            if (entry.Asset == null || entry.AssetType != assetType)
            {
                _cacheEntries.Remove(cacheKey);
                return null;
            }

            entry.Touch();
            return entry;
        }

        private ResourceHandle CreateCachedHandle(ResourceCacheEntry entry)
        {
            entry.RefCounter.Increment();
            entry.Touch();
            return new ResourceHandle(
                entry.CacheKey,
                entry.Location.ResourceKey,
                entry.ProviderType,
                entry.ResolvedAddress,
                entry.Location,
                entry.CachePolicy,
                entry.Asset,
                OnHandleReleased);
        }

        private ResourceHandle CreateNonCachedHandle(
            ResourceLocation location,
            string providerType,
            string resolvedAddress,
            ResourceCachePolicy cachePolicy,
            UnityObject asset)
        {
            return new ResourceHandle(
                string.Empty,
                location.ResourceKey,
                providerType,
                resolvedAddress,
                location,
                cachePolicy,
                asset,
                OnHandleReleased);
        }

        private void OnHandleReleased(ResourceHandle handle)
        {
            lock (_syncRoot)
            {
                if (!string.IsNullOrWhiteSpace(handle.CacheKey)
                    && _cacheEntries.TryGetValue(handle.CacheKey, out ResourceCacheEntry entry))
                {
                    int count = entry.RefCounter.Decrement();
                    if (count <= 0 && !entry.CachePolicy.RetainLoadedAsset)
                    {
                        _cacheEntries.Remove(handle.CacheKey);
                        ReleaseEntryAsset(entry);
                    }

                    return;
                }

                if (_providers.TryGetValue(handle.ProviderType, out IResourceProvider provider))
                {
                    provider.Release(handle);
                }
            }
        }

        private void ReleaseEntryAsset(ResourceCacheEntry entry)
        {
            if (entry == null)
            {
                return;
            }

            if (_providers.TryGetValue(entry.ProviderType, out IResourceProvider provider))
            {
                provider.Release(new ResourceHandle(
                    entry.CacheKey,
                    entry.Location.ResourceKey,
                    entry.ProviderType,
                    entry.ResolvedAddress,
                    entry.Location,
                    entry.CachePolicy,
                    entry.Asset,
                    null));
            }
        }

        private static string BuildCacheKey(ResourceLocation location, Type assetType)
        {
            return $"{location.ProviderType}::{location.ResourceKey}::{assetType.FullName}";
        }

        private static ResourceResult<TAsset> BuildSuccessResult<TAsset>(
            ResourceLocation location,
            string providerType,
            string resolvedAddress,
            TAsset asset,
            ResourceHandle handle) where TAsset : UnityObject
        {
            return new ResourceResult<TAsset>(true, asset, location.ResourceKey, providerType, resolvedAddress, location, handle);
        }

        private sealed class ManifestResourceLocator : IResourceLocator
        {
            private readonly IReadOnlyDictionary<string, ResourceLocation> _locationMap;
            private readonly IReadOnlyDictionary<string, IReadOnlyList<ResourceLocation>> _groupMap;

            public ManifestResourceLocator(ResourceManifestSnapshot manifestSnapshot)
            {
                ManifestSnapshot = manifestSnapshot ?? ResourceManifestSnapshot.Empty;

                Dictionary<string, ResourceLocation> locationMap = new(StringComparer.Ordinal);
                Dictionary<string, List<ResourceLocation>> groupMap = new(StringComparer.Ordinal);
                for (int index = 0; index < ManifestSnapshot.Entries.Count; index++)
                {
                    ResourceManifestEntry entry = ManifestSnapshot.Entries[index];
                    if (entry == null)
                    {
                        continue;
                    }

                    locationMap[entry.ResourceKey] = entry.Location;
                    string group = entry.Location.Group ?? string.Empty;
                    if (!groupMap.TryGetValue(group, out List<ResourceLocation> locations))
                    {
                        locations = new List<ResourceLocation>();
                        groupMap[group] = locations;
                    }

                    locations.Add(entry.Location);
                }

                _locationMap = new ReadOnlyDictionary<string, ResourceLocation>(locationMap);

                Dictionary<string, IReadOnlyList<ResourceLocation>> readonlyGroupMap = new(StringComparer.Ordinal);
                foreach (KeyValuePair<string, List<ResourceLocation>> pair in groupMap)
                {
                    readonlyGroupMap[pair.Key] = new ReadOnlyCollection<ResourceLocation>(pair.Value);
                }

                _groupMap = new ReadOnlyDictionary<string, IReadOnlyList<ResourceLocation>>(readonlyGroupMap);
            }

            public ResourceManifestSnapshot ManifestSnapshot { get; }

            public ResourceLocation GetLocation(string resourceKey)
            {
                if (TryGetLocation(resourceKey, out ResourceLocation location))
                {
                    return location;
                }

                throw new InvalidOperationException($"[ResourceSystem] 未找到资源定位信息：{resourceKey}");
            }

            public bool TryGetLocation(string resourceKey, out ResourceLocation location)
            {
                if (string.IsNullOrWhiteSpace(resourceKey))
                {
                    location = null;
                    return false;
                }

                return _locationMap.TryGetValue(resourceKey, out location);
            }

            public IReadOnlyList<ResourceLocation> GetLocationsByGroup(string group)
            {
                string normalizedGroup = group ?? string.Empty;
                if (_groupMap.TryGetValue(normalizedGroup, out IReadOnlyList<ResourceLocation> locations))
                {
                    return locations;
                }

                return Array.Empty<ResourceLocation>();
            }

            public IReadOnlyList<string> GetAvailableGroups()
            {
                return new ReadOnlyCollection<string>(new List<string>(_groupMap.Keys));
            }
        }
    }
}

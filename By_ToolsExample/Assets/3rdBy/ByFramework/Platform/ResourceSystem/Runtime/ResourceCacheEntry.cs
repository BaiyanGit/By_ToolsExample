//=====================================================
// 文件名称: ResourceCacheEntry.cs
// 创建作者: Codex
// 创建日期: 2026-06-08
// 描    述: 定义 ResourceSystem Runtime 缓存条目。
//=====================================================

namespace _3rdBy.ByFramework.Platform.ResourceSystem
{
    using System;
    using UnityObject = UnityEngine.Object;

    /// <summary>
    /// 运行时缓存条目。
    /// </summary>
    public sealed class ResourceCacheEntry
    {
        /// <summary>
        /// 初始化缓存条目。
        /// </summary>
        public ResourceCacheEntry(
            string cacheKey,
            string providerType,
            string resolvedAddress,
            ResourceLocation location,
            ResourceCachePolicy cachePolicy,
            UnityObject asset,
            Type assetType)
        {
            CacheKey = cacheKey ?? string.Empty;
            ProviderType = providerType ?? string.Empty;
            ResolvedAddress = resolvedAddress ?? string.Empty;
            Location = location;
            CachePolicy = cachePolicy ?? ResourceCachePolicy.NoCache;
            Asset = asset;
            AssetType = assetType;
            RefCounter = new ResourceRefCounter();
            CachedTimeUtc = DateTime.UtcNow;
            LastAccessTimeUtc = CachedTimeUtc;
        }

        /// <summary>
        /// 获取缓存 Key。
        /// </summary>
        public string CacheKey { get; }

        /// <summary>
        /// 获取 Provider 类型。
        /// </summary>
        public string ProviderType { get; }

        /// <summary>
        /// 获取解析后的地址。
        /// </summary>
        public string ResolvedAddress { get; }

        /// <summary>
        /// 获取资源位置。
        /// </summary>
        public ResourceLocation Location { get; }

        /// <summary>
        /// 获取缓存策略。
        /// </summary>
        public ResourceCachePolicy CachePolicy { get; }

        /// <summary>
        /// 获取缓存资源。
        /// </summary>
        public UnityObject Asset { get; }

        /// <summary>
        /// 获取缓存资源类型。
        /// </summary>
        public Type AssetType { get; }

        /// <summary>
        /// 获取引用计数器。
        /// </summary>
        public ResourceRefCounter RefCounter { get; }

        /// <summary>
        /// 获取缓存时间。
        /// </summary>
        public DateTime CachedTimeUtc { get; }

        /// <summary>
        /// 获取最近访问时间。
        /// </summary>
        public DateTime LastAccessTimeUtc { get; private set; }

        /// <summary>
        /// 更新最近访问时间。
        /// </summary>
        public void Touch()
        {
            LastAccessTimeUtc = DateTime.UtcNow;
        }
    }
}

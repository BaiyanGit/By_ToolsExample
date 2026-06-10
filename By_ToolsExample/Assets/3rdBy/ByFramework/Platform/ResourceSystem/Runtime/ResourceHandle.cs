//=====================================================
// 文件名称: ResourceHandle.cs
// 创建作者: Codex
// 创建日期: 2026-06-08
// 描    述: 定义 ResourceSystem 资源句柄。
//=====================================================

namespace _3rdBy.ByFramework.Platform.ResourceSystem
{
    using System;
    using System.Threading;
    using UnityObject = UnityEngine.Object;

    /// <summary>
    /// 资源句柄。
    /// </summary>
    public sealed class ResourceHandle
    {
        private readonly Action<ResourceHandle> _releaseAction;
        private int _isReleased;

        internal ResourceHandle(
            string cacheKey,
            string resourceKey,
            string providerType,
            string resolvedAddress,
            ResourceLocation location,
            ResourceCachePolicy cachePolicy,
            UnityObject asset,
            Action<ResourceHandle> releaseAction)
        {
            CacheKey = cacheKey ?? string.Empty;
            ResourceKey = resourceKey ?? string.Empty;
            ProviderType = providerType ?? string.Empty;
            ResolvedAddress = resolvedAddress ?? string.Empty;
            Location = location;
            CachePolicy = cachePolicy ?? ResourceCachePolicy.NoCache;
            Asset = asset;
            _releaseAction = releaseAction;
        }

        /// <summary>
        /// 获取缓存 Key。
        /// </summary>
        public string CacheKey { get; }

        /// <summary>
        /// 获取资源 Key。
        /// </summary>
        public string ResourceKey { get; }

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
        /// 获取资源对象。
        /// </summary>
        public UnityObject Asset { get; }

        /// <summary>
        /// 获取句柄是否有效。
        /// </summary>
        public bool IsValid => !IsReleased && Asset != null;

        /// <summary>
        /// 获取句柄是否已释放。
        /// </summary>
        public bool IsReleased => Volatile.Read(ref _isReleased) != 0;

        /// <summary>
        /// 释放当前句柄。
        /// </summary>
        public void Release()
        {
            if (Interlocked.Exchange(ref _isReleased, 1) != 0)
            {
                return;
            }

            _releaseAction?.Invoke(this);
        }
    }
}

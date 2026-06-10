//=====================================================
// 文件名称: ResourceRequest.cs
// 创建作者: Codex
// 创建日期: 2026-06-08
// 描    述: 定义 ResourceSystem 资源加载请求。
//=====================================================

namespace _3rdBy.ByFramework.Platform.ResourceSystem
{
    using System;

    /// <summary>
    /// 资源加载请求。
    /// </summary>
    public sealed class ResourceRequest
    {
        /// <summary>
        /// 初始化资源请求。
        /// </summary>
        public ResourceRequest(
            string resourceKey,
            string providerHint = "",
            string group = "",
            bool isRequired = true,
            ResourceCachePolicy cachePolicy = null)
        {
            if (string.IsNullOrWhiteSpace(resourceKey))
            {
                throw new ArgumentException("[ResourceSystem] 资源 Key 不能为空。", nameof(resourceKey));
            }

            ResourceKey = resourceKey;
            ProviderHint = providerHint ?? string.Empty;
            Group = group ?? string.Empty;
            IsRequired = isRequired;
            CachePolicy = cachePolicy ?? ResourceCachePolicy.Default;
        }

        /// <summary>
        /// 获取资源 Key。
        /// </summary>
        public string ResourceKey { get; }

        /// <summary>
        /// 获取 Provider 提示。
        /// </summary>
        public string ProviderHint { get; }

        /// <summary>
        /// 获取资源分组提示。
        /// </summary>
        public string Group { get; }

        /// <summary>
        /// 获取资源是否为必需资源。
        /// </summary>
        public bool IsRequired { get; }

        /// <summary>
        /// 获取缓存策略。
        /// </summary>
        public ResourceCachePolicy CachePolicy { get; }
    }
}

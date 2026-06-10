//=====================================================
// 文件名称: ResourceLocation.cs
// 创建作者: Codex
// 创建日期: 2026-06-08
// 描    述: 定义 ResourceSystem 资源位置模型。
//=====================================================

namespace _3rdBy.ByFramework.Platform.ResourceSystem
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    /// <summary>
    /// 资源位置模型。
    /// </summary>
    public sealed class ResourceLocation
    {
        /// <summary>
        /// 初始化资源位置。
        /// </summary>
        public ResourceLocation(
            string resourceKey,
            string providerType,
            string address,
            string group = "",
            string assetTypeName = "",
            bool isOptional = false,
            IReadOnlyList<string> tags = null)
        {
            if (string.IsNullOrWhiteSpace(resourceKey))
            {
                throw new ArgumentException("[ResourceSystem] 资源 Key 不能为空。", nameof(resourceKey));
            }

            if (string.IsNullOrWhiteSpace(providerType))
            {
                throw new ArgumentException("[ResourceSystem] ProviderType 不能为空。", nameof(providerType));
            }

            ResourceKey = resourceKey;
            ProviderType = providerType;
            Address = string.IsNullOrWhiteSpace(address) ? resourceKey : address;
            Group = group ?? string.Empty;
            AssetTypeName = assetTypeName ?? string.Empty;
            IsOptional = isOptional;
            Tags = tags == null
                ? Array.Empty<string>()
                : new ReadOnlyCollection<string>(new List<string>(tags));
        }

        /// <summary>
        /// 获取资源 Key。
        /// </summary>
        public string ResourceKey { get; }

        /// <summary>
        /// 获取 Provider 类型。
        /// </summary>
        public string ProviderType { get; }

        /// <summary>
        /// 获取资源地址。
        /// </summary>
        public string Address { get; }

        /// <summary>
        /// 获取资源分组。
        /// </summary>
        public string Group { get; }

        /// <summary>
        /// 获取资源类型名称。
        /// </summary>
        public string AssetTypeName { get; }

        /// <summary>
        /// 获取是否为可选资源。
        /// </summary>
        public bool IsOptional { get; }

        /// <summary>
        /// 获取资源标签。
        /// </summary>
        public IReadOnlyList<string> Tags { get; }
    }
}

//=====================================================
// 文件名称: AssetBundleAssetEntry.cs
// 创建作者: Codex
// 创建日期: 2026-06-09
// 描    述: 定义 AssetBundle 资源条目模型。
//=====================================================

namespace _3rdBy.ByFramework.Platform.ResourceSystem.AssetBundleSystem
{
    using System;

    /// <summary>
    /// AssetBundle 资源条目模型。
    /// </summary>
    public sealed class AssetBundleAssetEntry
    {
        /// <summary>
        /// 初始化资源条目。
        /// </summary>
        public AssetBundleAssetEntry(
            string bundleName,
            string assetName,
            string assetPath = "",
            string resourceKey = "",
            string assetTypeName = "",
            string providerType = "",
            string group = "",
            bool isOptional = false)
        {
            if (string.IsNullOrWhiteSpace(bundleName))
            {
                throw new ArgumentException("[AssetBundle] BundleName 不能为空。", nameof(bundleName));
            }

            if (string.IsNullOrWhiteSpace(assetName))
            {
                throw new ArgumentException("[AssetBundle] AssetName 不能为空。", nameof(assetName));
            }

            BundleName = bundleName;
            AssetName = assetName;
            AssetPath = assetPath ?? string.Empty;
            ResourceKey = string.IsNullOrWhiteSpace(resourceKey) ? assetName : resourceKey;
            AssetTypeName = assetTypeName ?? string.Empty;
            ProviderType = providerType ?? string.Empty;
            Group = group ?? string.Empty;
            IsOptional = isOptional;
        }

        /// <summary>
        /// 获取 Bundle 名称。
        /// </summary>
        public string BundleName { get; }

        /// <summary>
        /// 获取资源名称。
        /// </summary>
        public string AssetName { get; }

        /// <summary>
        /// 获取资源路径。
        /// </summary>
        public string AssetPath { get; }

        /// <summary>
        /// 获取资源 Key。
        /// </summary>
        public string ResourceKey { get; }

        /// <summary>
        /// 获取资源类型名称。
        /// </summary>
        public string AssetTypeName { get; }

        /// <summary>
        /// 获取 Provider 类型。
        /// </summary>
        public string ProviderType { get; }

        /// <summary>
        /// 获取资源分组。
        /// </summary>
        public string Group { get; }

        /// <summary>
        /// 获取是否为可选资源。
        /// </summary>
        public bool IsOptional { get; }
    }
}

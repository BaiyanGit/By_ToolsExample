//=====================================================
// 文件名称: AssetBundleManifestEntry.cs
// 创建作者: Codex
// 创建日期: 2026-06-09
// 描    述: 定义 AssetBundle Manifest 条目模型。
//=====================================================

namespace _3rdBy.ByFramework.Platform.ResourceSystem.AssetBundleSystem
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    /// <summary>
    /// AssetBundle Manifest 条目模型。
    /// </summary>
    public sealed class AssetBundleManifestEntry
    {
        /// <summary>
        /// 初始化 Manifest 条目。
        /// </summary>
        public AssetBundleManifestEntry(
            string bundleName,
            string bundleFileName,
            string bundleRelativePath,
            string hash = "",
            string version = "",
            long size = 0,
            IReadOnlyList<string> dependencyBundleNames = null,
            IReadOnlyList<AssetBundleAssetEntry> assetEntries = null)
        {
            if (string.IsNullOrWhiteSpace(bundleName))
            {
                throw new ArgumentException("[AssetBundle] BundleName 不能为空。", nameof(bundleName));
            }

            BundleName = bundleName;
            BundleFileName = string.IsNullOrWhiteSpace(bundleFileName) ? bundleName : bundleFileName;
            BundleRelativePath = bundleRelativePath ?? string.Empty;
            Hash = hash ?? string.Empty;
            Version = version ?? string.Empty;
            Size = size;
            DependencyBundleNames = dependencyBundleNames == null
                ? Array.Empty<string>()
                : new ReadOnlyCollection<string>(new List<string>(dependencyBundleNames));
            AssetEntries = assetEntries == null
                ? Array.Empty<AssetBundleAssetEntry>()
                : new ReadOnlyCollection<AssetBundleAssetEntry>(new List<AssetBundleAssetEntry>(assetEntries));
        }

        /// <summary>
        /// 获取 Bundle 名称。
        /// </summary>
        public string BundleName { get; }

        /// <summary>
        /// 获取 Bundle 文件名。
        /// </summary>
        public string BundleFileName { get; }

        /// <summary>
        /// 获取 Bundle 相对路径。
        /// </summary>
        public string BundleRelativePath { get; }

        /// <summary>
        /// 获取 Hash。
        /// </summary>
        public string Hash { get; }

        /// <summary>
        /// 获取版本号。
        /// </summary>
        public string Version { get; }

        /// <summary>
        /// 获取大小。
        /// </summary>
        public long Size { get; }

        /// <summary>
        /// 获取依赖 Bundle 名称集合。
        /// </summary>
        public IReadOnlyList<string> DependencyBundleNames { get; }

        /// <summary>
        /// 获取资产条目集合。
        /// </summary>
        public IReadOnlyList<AssetBundleAssetEntry> AssetEntries { get; }
    }
}

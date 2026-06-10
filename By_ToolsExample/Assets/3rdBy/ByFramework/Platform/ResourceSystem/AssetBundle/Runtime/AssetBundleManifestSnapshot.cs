//=====================================================
// 文件名称: AssetBundleManifestSnapshot.cs
// 创建作者: Codex
// 创建日期: 2026-06-09
// 描    述: 定义 AssetBundle Manifest 只读快照。
//=====================================================

namespace _3rdBy.ByFramework.Platform.ResourceSystem.AssetBundleSystem
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    /// <summary>
    /// AssetBundle Manifest 只读快照。
    /// </summary>
    public sealed class AssetBundleManifestSnapshot
    {
        private readonly IReadOnlyDictionary<string, AssetBundleManifestEntry> _bundleMap;
        private readonly IReadOnlyDictionary<string, AssetBundleAssetEntry> _assetMap;

        /// <summary>
        /// 初始化 Manifest 快照。
        /// </summary>
        public AssetBundleManifestSnapshot(
            string manifestVersion,
            string resourceVersion,
            IReadOnlyList<AssetBundleManifestEntry> bundleEntries,
            DateTime generatedTimeUtc)
        {
            ManifestVersion = string.IsNullOrWhiteSpace(manifestVersion) ? "1.0.0" : manifestVersion;
            ResourceVersion = string.IsNullOrWhiteSpace(resourceVersion) ? "1.0.0" : resourceVersion;
            GeneratedTimeUtc = generatedTimeUtc;

            List<AssetBundleManifestEntry> entryList = bundleEntries == null
                ? new List<AssetBundleManifestEntry>()
                : new List<AssetBundleManifestEntry>(bundleEntries);
            BundleEntries = new ReadOnlyCollection<AssetBundleManifestEntry>(entryList);

            Dictionary<string, AssetBundleManifestEntry> bundleMap = new(StringComparer.Ordinal);
            Dictionary<string, AssetBundleAssetEntry> assetMap = new(StringComparer.Ordinal);
            for (int bundleIndex = 0; bundleIndex < entryList.Count; bundleIndex++)
            {
                AssetBundleManifestEntry entry = entryList[bundleIndex];
                if (entry == null)
                {
                    continue;
                }

                bundleMap[entry.BundleName] = entry;
                for (int assetIndex = 0; assetIndex < entry.AssetEntries.Count; assetIndex++)
                {
                    AssetBundleAssetEntry assetEntry = entry.AssetEntries[assetIndex];
                    if (assetEntry == null)
                    {
                        continue;
                    }

                    assetMap[assetEntry.ResourceKey] = assetEntry;
                }
            }

            _bundleMap = new ReadOnlyDictionary<string, AssetBundleManifestEntry>(bundleMap);
            _assetMap = new ReadOnlyDictionary<string, AssetBundleAssetEntry>(assetMap);
        }

        /// <summary>
        /// 获取空快照。
        /// </summary>
        public static AssetBundleManifestSnapshot Empty { get; } =
            new("1.0.0", "1.0.0", Array.Empty<AssetBundleManifestEntry>(), DateTime.UtcNow);

        /// <summary>
        /// 获取 Manifest 版本号。
        /// </summary>
        public string ManifestVersion { get; }

        /// <summary>
        /// 获取资源版本号。
        /// </summary>
        public string ResourceVersion { get; }

        /// <summary>
        /// 获取生成时间。
        /// </summary>
        public DateTime GeneratedTimeUtc { get; }

        /// <summary>
        /// 获取 Bundle 条目集合。
        /// </summary>
        public IReadOnlyList<AssetBundleManifestEntry> BundleEntries { get; }

        /// <summary>
        /// 安全获取 Bundle 条目。
        /// </summary>
        public bool TryGetBundleEntry(string bundleName, out AssetBundleManifestEntry entry)
        {
            if (string.IsNullOrWhiteSpace(bundleName))
            {
                entry = null;
                return false;
            }

            return _bundleMap.TryGetValue(bundleName, out entry);
        }

        /// <summary>
        /// 安全获取资源条目。
        /// </summary>
        public bool TryGetAssetEntry(string resourceKey, out AssetBundleAssetEntry entry)
        {
            if (string.IsNullOrWhiteSpace(resourceKey))
            {
                entry = null;
                return false;
            }

            return _assetMap.TryGetValue(resourceKey, out entry);
        }
    }
}

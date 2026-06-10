//=====================================================
// 文件名称: AssetBundleManifest.cs
// 创建作者: Codex
// 创建日期: 2026-06-09
// 描    述: 实现 AssetBundle Manifest 访问对象。
//=====================================================

namespace _3rdBy.ByFramework.Platform.ResourceSystem.AssetBundleSystem
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// AssetBundle Manifest 访问对象。
    /// </summary>
    public sealed class AssetBundleManifest : IAssetBundleManifest
    {
        /// <summary>
        /// 初始化 Manifest 访问对象。
        /// </summary>
        public AssetBundleManifest(AssetBundleManifestSnapshot snapshot)
        {
            Snapshot = snapshot ?? AssetBundleManifestSnapshot.Empty;
        }

        /// <summary>
        /// 获取当前 Manifest 快照。
        /// </summary>
        public AssetBundleManifestSnapshot Snapshot { get; }

        /// <summary>
        /// 强制获取 Bundle 条目。
        /// </summary>
        public AssetBundleManifestEntry GetBundleEntry(string bundleName)
        {
            if (TryGetBundleEntry(bundleName, out AssetBundleManifestEntry entry))
            {
                return entry;
            }

            throw new InvalidOperationException($"[AssetBundle] 未找到 Bundle 条目：{bundleName}");
        }

        /// <summary>
        /// 安全获取 Bundle 条目。
        /// </summary>
        public bool TryGetBundleEntry(string bundleName, out AssetBundleManifestEntry entry)
        {
            return Snapshot.TryGetBundleEntry(bundleName, out entry);
        }

        /// <summary>
        /// 获取全部 Bundle 条目。
        /// </summary>
        public IReadOnlyList<AssetBundleManifestEntry> GetAllBundleEntries()
        {
            return Snapshot.BundleEntries;
        }

        /// <summary>
        /// 强制获取资源条目。
        /// </summary>
        public AssetBundleAssetEntry GetAssetEntry(string resourceKey)
        {
            if (TryGetAssetEntry(resourceKey, out AssetBundleAssetEntry entry))
            {
                return entry;
            }

            throw new InvalidOperationException($"[AssetBundle] 未找到资源条目：{resourceKey}");
        }

        /// <summary>
        /// 安全获取资源条目。
        /// </summary>
        public bool TryGetAssetEntry(string resourceKey, out AssetBundleAssetEntry entry)
        {
            return Snapshot.TryGetAssetEntry(resourceKey, out entry);
        }

        /// <summary>
        /// 获取 Bundle 版本信息。
        /// </summary>
        public AssetBundleVersionInfo GetVersionInfo(string bundleName)
        {
            AssetBundleManifestEntry entry = GetBundleEntry(bundleName);
            return new AssetBundleVersionInfo(
                entry.BundleName,
                entry.Version,
                entry.Hash,
                entry.Size,
                Snapshot.ManifestVersion,
                Snapshot.GeneratedTimeUtc);
        }
    }
}

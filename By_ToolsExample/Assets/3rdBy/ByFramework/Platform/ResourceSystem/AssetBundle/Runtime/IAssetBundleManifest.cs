//=====================================================
// 文件名称: IAssetBundleManifest.cs
// 创建作者: Codex
// 创建日期: 2026-06-09
// 描    述: 定义 AssetBundle Manifest 访问接口。
//=====================================================

namespace _3rdBy.ByFramework.Platform.ResourceSystem.AssetBundleSystem
{
    using System.Collections.Generic;

    /// <summary>
    /// AssetBundle Manifest 访问接口。
    /// </summary>
    public interface IAssetBundleManifest
    {
        /// <summary>
        /// 获取当前 Manifest 快照。
        /// </summary>
        AssetBundleManifestSnapshot Snapshot { get; }

        /// <summary>
        /// 强制获取 Bundle 条目。
        /// </summary>
        AssetBundleManifestEntry GetBundleEntry(string bundleName);

        /// <summary>
        /// 安全获取 Bundle 条目。
        /// </summary>
        bool TryGetBundleEntry(string bundleName, out AssetBundleManifestEntry entry);

        /// <summary>
        /// 获取全部 Bundle 条目。
        /// </summary>
        IReadOnlyList<AssetBundleManifestEntry> GetAllBundleEntries();

        /// <summary>
        /// 强制获取资源条目。
        /// </summary>
        AssetBundleAssetEntry GetAssetEntry(string resourceKey);

        /// <summary>
        /// 安全获取资源条目。
        /// </summary>
        bool TryGetAssetEntry(string resourceKey, out AssetBundleAssetEntry entry);

        /// <summary>
        /// 获取 Bundle 版本信息。
        /// </summary>
        AssetBundleVersionInfo GetVersionInfo(string bundleName);
    }
}

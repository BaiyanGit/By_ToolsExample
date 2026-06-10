//=====================================================
// 文件名称: IAssetBundleLocator.cs
// 创建作者: Codex
// 创建日期: 2026-06-09
// 描    述: 定义 AssetBundle 资源定位接口。
//=====================================================

namespace _3rdBy.ByFramework.Platform.ResourceSystem.AssetBundleSystem
{
    /// <summary>
    /// AssetBundle 资源定位接口。
    /// </summary>
    public interface IAssetBundleLocator
    {
        /// <summary>
        /// 获取当前 Manifest。
        /// </summary>
        IAssetBundleManifest Manifest { get; }

        /// <summary>
        /// 强制获取 Bundle 文件路径。
        /// </summary>
        string GetBundlePath(string bundleName);

        /// <summary>
        /// 安全获取 Bundle 文件路径。
        /// </summary>
        bool TryGetBundlePath(string bundleName, out string bundlePath);

        /// <summary>
        /// 强制获取 Bundle 条目。
        /// </summary>
        AssetBundleManifestEntry GetBundleEntry(string bundleName);

        /// <summary>
        /// 安全获取 Bundle 条目。
        /// </summary>
        bool TryGetBundleEntry(string bundleName, out AssetBundleManifestEntry entry);

        /// <summary>
        /// 强制获取资源条目。
        /// </summary>
        AssetBundleAssetEntry GetAssetEntry(string resourceKey);

        /// <summary>
        /// 安全获取资源条目。
        /// </summary>
        bool TryGetAssetEntry(string resourceKey, out AssetBundleAssetEntry entry);
    }
}

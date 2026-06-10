//=====================================================
// 文件名称: IAssetBundleService.cs
// 创建作者: Codex
// 创建日期: 2026-06-09
// 描    述: 定义 AssetBundle Runtime 服务接口。
//=====================================================

namespace _3rdBy.ByFramework.Platform.ResourceSystem.AssetBundleSystem
{
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// AssetBundle Runtime 服务接口。
    /// </summary>
    public interface IAssetBundleService : _3rdBy.ByFramework.Platform.PlatformServiceRegistry.IPlatformService
    {
        /// <summary>
        /// 获取 Manifest 访问对象。
        /// </summary>
        IAssetBundleManifest Manifest { get; }

        /// <summary>
        /// 获取 Bundle 定位器。
        /// </summary>
        IAssetBundleLocator Locator { get; }

        /// <summary>
        /// 获取依赖图。
        /// </summary>
        AssetBundleDependencyGraph DependencyGraph { get; }

        /// <summary>
        /// 强制加载 Bundle。
        /// </summary>
        AssetBundle LoadBundle(string bundleName);

        /// <summary>
        /// 安全加载 Bundle。
        /// </summary>
        bool TryLoadBundle(string bundleName, out AssetBundle bundle);

        /// <summary>
        /// 卸载 Bundle。
        /// </summary>
        void UnloadBundle(string bundleName, bool unloadAllLoadedObjects = false);

        /// <summary>
        /// 判断 Bundle 是否已加载。
        /// </summary>
        bool IsBundleLoaded(string bundleName);

        /// <summary>
        /// 判断 Manifest 中是否包含指定 Bundle。
        /// </summary>
        bool ContainsBundle(string bundleName);

        /// <summary>
        /// 强制加载 Bundle 内资源。
        /// </summary>
        TAsset LoadAsset<TAsset>(string bundleName, string assetName) where TAsset : Object;

        /// <summary>
        /// 安全加载 Bundle 内资源。
        /// </summary>
        bool TryLoadAsset<TAsset>(string bundleName, string assetName, out TAsset asset) where TAsset : Object;

        /// <summary>
        /// 获取 Bundle 版本信息。
        /// </summary>
        AssetBundleVersionInfo GetBundleInfo(string bundleName);

        /// <summary>
        /// 获取全部 Bundle 版本信息。
        /// </summary>
        IReadOnlyList<AssetBundleVersionInfo> GetAllBundleInfos();

        /// <summary>
        /// 获取依赖集合。
        /// </summary>
        IReadOnlyList<string> GetDependencies(string bundleName, bool includeTransitive = true);

        /// <summary>
        /// 获取 Manifest 快照。
        /// </summary>
        AssetBundleManifestSnapshot GetManifestSnapshot();

        /// <summary>
        /// 释放资源对应的 Bundle 引用。
        /// </summary>
        void ReleaseAssetReference(string bundleName, string assetName, bool unloadAllLoadedObjects = false);
    }
}

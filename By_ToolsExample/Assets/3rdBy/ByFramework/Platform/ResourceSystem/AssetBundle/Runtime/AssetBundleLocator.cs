//=====================================================
// 文件名称: AssetBundleLocator.cs
// 创建作者: Codex
// 创建日期: 2026-06-09
// 描    述: 实现 AssetBundle 资源定位器。
//=====================================================

namespace _3rdBy.ByFramework.Platform.ResourceSystem.AssetBundleSystem
{
    using System;
    using System.IO;
    using UnityEngine;

    /// <summary>
    /// AssetBundle 资源定位器。
    /// </summary>
    public sealed class AssetBundleLocator : IAssetBundleLocator
    {
        private readonly string _rootPath;

        /// <summary>
        /// 初始化定位器。
        /// </summary>
        public AssetBundleLocator(IAssetBundleManifest manifest, string rootPath = "")
        {
            Manifest = manifest ?? throw new ArgumentNullException(nameof(manifest));
            _rootPath = string.IsNullOrWhiteSpace(rootPath)
                ? Path.Combine(Application.streamingAssetsPath, "AB")
                : rootPath;
        }

        /// <summary>
        /// 获取 Manifest 访问对象。
        /// </summary>
        public IAssetBundleManifest Manifest { get; }

        /// <summary>
        /// 强制获取 Bundle 文件路径。
        /// </summary>
        public string GetBundlePath(string bundleName)
        {
            if (TryGetBundlePath(bundleName, out string bundlePath))
            {
                return bundlePath;
            }

            throw new InvalidOperationException($"[AssetBundle] 未找到 Bundle 路径：{bundleName}");
        }

        /// <summary>
        /// 安全获取 Bundle 文件路径。
        /// </summary>
        public bool TryGetBundlePath(string bundleName, out string bundlePath)
        {
            if (!TryGetBundleEntry(bundleName, out AssetBundleManifestEntry entry))
            {
                bundlePath = string.Empty;
                return false;
            }

            bundlePath = ResolveBundlePath(entry);
            return true;
        }

        /// <summary>
        /// 强制获取 Bundle 条目。
        /// </summary>
        public AssetBundleManifestEntry GetBundleEntry(string bundleName)
        {
            return Manifest.GetBundleEntry(bundleName);
        }

        /// <summary>
        /// 安全获取 Bundle 条目。
        /// </summary>
        public bool TryGetBundleEntry(string bundleName, out AssetBundleManifestEntry entry)
        {
            return Manifest.TryGetBundleEntry(bundleName, out entry);
        }

        /// <summary>
        /// 强制获取资源条目。
        /// </summary>
        public AssetBundleAssetEntry GetAssetEntry(string resourceKey)
        {
            return Manifest.GetAssetEntry(resourceKey);
        }

        /// <summary>
        /// 安全获取资源条目。
        /// </summary>
        public bool TryGetAssetEntry(string resourceKey, out AssetBundleAssetEntry entry)
        {
            return Manifest.TryGetAssetEntry(resourceKey, out entry);
        }

        private string ResolveBundlePath(AssetBundleManifestEntry entry)
        {
            string relativePath = entry.BundleRelativePath;
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                relativePath = entry.BundleFileName;
            }

            if (Path.IsPathRooted(relativePath))
            {
                return relativePath;
            }

            if (string.IsNullOrWhiteSpace(_rootPath))
            {
                return relativePath.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
            }

            string normalizedRelativePath = relativePath.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
            return Path.Combine(_rootPath, normalizedRelativePath);
        }
    }
}

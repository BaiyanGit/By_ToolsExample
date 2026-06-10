//=====================================================
// 文件名称: AssetBundleVersionInfo.cs
// 创建作者: Codex
// 创建日期: 2026-06-09
// 描    述: 定义 AssetBundle 版本与 Hash 元数据模型。
//=====================================================

namespace _3rdBy.ByFramework.Platform.ResourceSystem.AssetBundleSystem
{
    using System;

    /// <summary>
    /// AssetBundle 版本与 Hash 元数据模型。
    /// </summary>
    public sealed class AssetBundleVersionInfo
    {
        /// <summary>
        /// 初始化版本信息。
        /// </summary>
        public AssetBundleVersionInfo(
            string bundleName,
            string version,
            string hash,
            long fileSize,
            string manifestVersion,
            DateTime generatedTimeUtc)
        {
            BundleName = bundleName ?? string.Empty;
            Version = version ?? string.Empty;
            Hash = hash ?? string.Empty;
            FileSize = fileSize;
            ManifestVersion = manifestVersion ?? string.Empty;
            GeneratedTimeUtc = generatedTimeUtc;
        }

        /// <summary>
        /// 获取 Bundle 名称。
        /// </summary>
        public string BundleName { get; }

        /// <summary>
        /// 获取版本号。
        /// </summary>
        public string Version { get; }

        /// <summary>
        /// 获取 Hash。
        /// </summary>
        public string Hash { get; }

        /// <summary>
        /// 获取文件大小。
        /// </summary>
        public long FileSize { get; }

        /// <summary>
        /// 获取 Manifest 版本号。
        /// </summary>
        public string ManifestVersion { get; }

        /// <summary>
        /// 获取生成时间。
        /// </summary>
        public DateTime GeneratedTimeUtc { get; }
    }
}

//=====================================================
// 文件名称: ResourceManifestEntry.cs
// 创建作者: Codex
// 创建日期: 2026-06-08
// 描    述: 定义 ResourceSystem 资源清单条目。
//=====================================================

namespace _3rdBy.ByFramework.Platform.ResourceSystem
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    /// <summary>
    /// 资源清单条目。
    /// </summary>
    public sealed class ResourceManifestEntry
    {
        /// <summary>
        /// 初始化资源清单条目。
        /// </summary>
        public ResourceManifestEntry(
            ResourceLocation location,
            string version = "",
            string hash = "",
            long size = 0,
            IReadOnlyList<string> dependencies = null)
        {
            Location = location ?? throw new ArgumentNullException(nameof(location));
            Version = version ?? string.Empty;
            Hash = hash ?? string.Empty;
            Size = size;
            Dependencies = dependencies == null
                ? Array.Empty<string>()
                : new ReadOnlyCollection<string>(new List<string>(dependencies));
        }

        /// <summary>
        /// 获取资源位置。
        /// </summary>
        public ResourceLocation Location { get; }

        /// <summary>
        /// 获取资源 Key。
        /// </summary>
        public string ResourceKey => Location.ResourceKey;

        /// <summary>
        /// 获取 Provider 类型。
        /// </summary>
        public string ProviderType => Location.ProviderType;

        /// <summary>
        /// 获取资源版本。
        /// </summary>
        public string Version { get; }

        /// <summary>
        /// 获取资源哈希。
        /// </summary>
        public string Hash { get; }

        /// <summary>
        /// 获取资源大小。
        /// </summary>
        public long Size { get; }

        /// <summary>
        /// 获取资源依赖列表。
        /// </summary>
        public IReadOnlyList<string> Dependencies { get; }
    }
}

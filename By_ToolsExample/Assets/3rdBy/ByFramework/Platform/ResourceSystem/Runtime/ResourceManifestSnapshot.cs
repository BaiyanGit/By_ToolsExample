//=====================================================
// 文件名称: ResourceManifestSnapshot.cs
// 创建作者: Codex
// 创建日期: 2026-06-08
// 描    述: 定义 ResourceSystem 资源清单只读快照。
//=====================================================

namespace _3rdBy.ByFramework.Platform.ResourceSystem
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    /// <summary>
    /// 资源清单只读快照。
    /// </summary>
    public sealed class ResourceManifestSnapshot
    {
        private readonly IReadOnlyDictionary<string, ResourceManifestEntry> _entryMap;

        /// <summary>
        /// 初始化资源清单快照。
        /// </summary>
        public ResourceManifestSnapshot(
            string manifestVersion,
            IReadOnlyList<ResourceManifestEntry> entries,
            DateTime createdTimeUtc)
        {
            ManifestVersion = string.IsNullOrWhiteSpace(manifestVersion) ? "1.0.0" : manifestVersion;
            CreatedTimeUtc = createdTimeUtc;

            List<ResourceManifestEntry> entryList = entries == null
                ? new List<ResourceManifestEntry>()
                : new List<ResourceManifestEntry>(entries);
            Entries = new ReadOnlyCollection<ResourceManifestEntry>(entryList);

            Dictionary<string, ResourceManifestEntry> entryMap = new(StringComparer.Ordinal);
            for (int index = 0; index < entryList.Count; index++)
            {
                ResourceManifestEntry entry = entryList[index];
                if (entry == null)
                {
                    continue;
                }

                entryMap[entry.ResourceKey] = entry;
            }

            _entryMap = new ReadOnlyDictionary<string, ResourceManifestEntry>(entryMap);
        }

        /// <summary>
        /// 获取空快照。
        /// </summary>
        public static ResourceManifestSnapshot Empty { get; } =
            new("1.0.0", Array.Empty<ResourceManifestEntry>(), DateTime.UtcNow);

        /// <summary>
        /// 获取清单版本。
        /// </summary>
        public string ManifestVersion { get; }

        /// <summary>
        /// 获取快照创建时间。
        /// </summary>
        public DateTime CreatedTimeUtc { get; }

        /// <summary>
        /// 获取清单条目集合。
        /// </summary>
        public IReadOnlyList<ResourceManifestEntry> Entries { get; }

        /// <summary>
        /// 安全获取指定条目。
        /// </summary>
        public bool TryGetEntry(string resourceKey, out ResourceManifestEntry entry)
        {
            if (string.IsNullOrWhiteSpace(resourceKey))
            {
                entry = null;
                return false;
            }

            return _entryMap.TryGetValue(resourceKey, out entry);
        }
    }
}

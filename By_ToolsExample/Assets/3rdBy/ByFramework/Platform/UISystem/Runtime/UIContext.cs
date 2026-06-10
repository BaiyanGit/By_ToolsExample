//=====================================================
// 文件名称: UIContext.cs
// 创建者: Codex
// 创建日期: 2026-06-09
// 描    述: 表达当前 UI 运行时不可变快照。
//=====================================================

namespace _3rdBy.ByFramework.Platform.UISystem
{
    using System;
    using System.Collections.Generic;

    public sealed class UIContext
    {
        public UIContext(
            IReadOnlyList<UIRootId> registeredRoots,
            IReadOnlyList<UILayerId> availableLayers,
            IReadOnlyList<UIViewDescriptor> openViews,
            int version)
        {
            if (version < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(version), "[UISystem] Version cannot be negative.");
            }

            RegisteredRoots = CopyRoots(registeredRoots);
            AvailableLayers = CopyLayers(availableLayers);
            OpenViews = CopyViews(openViews);
            Version = version;
        }

        public IReadOnlyList<UIRootId> RegisteredRoots { get; }

        public IReadOnlyList<UILayerId> AvailableLayers { get; }

        public IReadOnlyList<UIViewDescriptor> OpenViews { get; }

        public int Version { get; }

        private static IReadOnlyList<UIRootId> CopyRoots(IReadOnlyList<UIRootId> source)
        {
            if (source == null || source.Count == 0)
            {
                return Array.Empty<UIRootId>();
            }

            List<UIRootId> copied = new(source.Count);
            for (int index = 0; index < source.Count; index++)
            {
                copied.Add(source[index]);
            }

            return copied.AsReadOnly();
        }

        private static IReadOnlyList<UILayerId> CopyLayers(IReadOnlyList<UILayerId> source)
        {
            if (source == null || source.Count == 0)
            {
                return Array.Empty<UILayerId>();
            }

            List<UILayerId> copied = new(source.Count);
            for (int index = 0; index < source.Count; index++)
            {
                copied.Add(source[index]);
            }

            return copied.AsReadOnly();
        }

        private static IReadOnlyList<UIViewDescriptor> CopyViews(IReadOnlyList<UIViewDescriptor> source)
        {
            if (source == null || source.Count == 0)
            {
                return Array.Empty<UIViewDescriptor>();
            }

            List<UIViewDescriptor> copied = new(source.Count);
            for (int index = 0; index < source.Count; index++)
            {
                copied.Add(source[index]);
            }

            return copied.AsReadOnly();
        }
    }
}

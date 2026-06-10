//=====================================================
// 文件名称: DisplayContext.cs
// 创建者: Codex
// 创建日期: 2026-06-09
// 描    述: 表达当前运行时显示上下文不可变快照。
//=====================================================

namespace _3rdBy.ByFramework.Platform.DisplaySystem
{
    using System;
    using System.Collections.Generic;

    public sealed class DisplayContext
    {
        public DisplayContext(
            DisplayProfileId currentProfileId,
            DisplayMode currentMode,
            IReadOnlyList<DisplayTargetDescriptor> activeTargets,
            int version)
        {
            if (currentProfileId.IsEmpty)
            {
                throw new ArgumentException("[DisplaySystem] CurrentProfileId cannot be empty.", nameof(currentProfileId));
            }

            if (version < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(version), "[DisplaySystem] Version cannot be negative.");
            }

            CurrentProfileId = currentProfileId;
            CurrentMode = currentMode;
            ActiveTargets = CopyTargets(activeTargets);
            Version = version;
        }

        public DisplayProfileId CurrentProfileId { get; }

        public DisplayMode CurrentMode { get; }

        public IReadOnlyList<DisplayTargetDescriptor> ActiveTargets { get; }

        public int Version { get; }

        private static IReadOnlyList<DisplayTargetDescriptor> CopyTargets(IReadOnlyList<DisplayTargetDescriptor> source)
        {
            if (source == null || source.Count == 0)
            {
                return Array.Empty<DisplayTargetDescriptor>();
            }

            List<DisplayTargetDescriptor> copied = new(source.Count);
            for (int index = 0; index < source.Count; index++)
            {
                copied.Add(source[index]);
            }

            return copied.AsReadOnly();
        }
    }
}

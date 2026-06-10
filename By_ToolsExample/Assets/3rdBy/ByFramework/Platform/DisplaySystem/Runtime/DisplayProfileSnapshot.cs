//=====================================================
// 文件名称: DisplayProfileSnapshot.cs
// 创建者: Codex
// 创建日期: 2026-06-09
// 描    述: 表达只读显示配置快照。
//=====================================================

namespace _3rdBy.ByFramework.Platform.DisplaySystem
{
    using System;
    using System.Collections.Generic;

    public sealed class DisplayProfileSnapshot
    {
        public DisplayProfileSnapshot(
            DisplayProfileId profileId,
            DisplayMode mode,
            IReadOnlyList<DisplayTargetId> targetIds)
        {
            if (profileId.IsEmpty)
            {
                throw new ArgumentException("[DisplaySystem] ProfileId cannot be empty.", nameof(profileId));
            }

            ProfileId = profileId;
            Mode = mode;
            TargetIds = CopyTargetIds(targetIds);
        }

        public DisplayProfileId ProfileId { get; }

        public DisplayMode Mode { get; }

        public IReadOnlyList<DisplayTargetId> TargetIds { get; }

        private static IReadOnlyList<DisplayTargetId> CopyTargetIds(IReadOnlyList<DisplayTargetId> source)
        {
            if (source == null || source.Count == 0)
            {
                return Array.Empty<DisplayTargetId>();
            }

            List<DisplayTargetId> copied = new(source.Count);
            for (int index = 0; index < source.Count; index++)
            {
                copied.Add(source[index]);
            }

            return copied.AsReadOnly();
        }
    }
}

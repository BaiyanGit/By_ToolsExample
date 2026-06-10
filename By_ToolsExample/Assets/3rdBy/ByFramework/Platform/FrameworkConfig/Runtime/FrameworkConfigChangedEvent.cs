//=====================================================
// 文件名称: FrameworkConfigChangedEvent.cs
// 创 建 者: Codex
// 创建日期: 2026-06-08
// 描    述: 表示 FrameworkConfig 快照更新时对外发布的事件数据。
//=====================================================

namespace _3rdBy.ByFramework.Platform.FrameworkConfig
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    /// <summary>
    /// FrameworkConfig 变化事件。
    /// </summary>
    public sealed class FrameworkConfigChangedEvent
    {
        /// <summary>
        /// 初始化变化事件。
        /// </summary>
        /// <param name="previousSnapshot">旧快照。</param>
        /// <param name="currentSnapshot">新快照。</param>
        /// <param name="changedSources">发生变化的配置来源集合。</param>
        /// <param name="changeSummary">变化摘要。</param>
        /// <param name="changeTimeUtc">变化时间（UTC）。</param>
        public FrameworkConfigChangedEvent(
            FrameworkConfigSnapshot previousSnapshot,
            FrameworkConfigSnapshot currentSnapshot,
            IReadOnlyList<ConfigSource> changedSources,
            string changeSummary,
            DateTime changeTimeUtc)
        {
            CurrentSnapshot = currentSnapshot ?? throw new ArgumentNullException(nameof(currentSnapshot));
            PreviousSnapshot = previousSnapshot;
            ChangedSources = changedSources == null
                ? Array.Empty<ConfigSource>()
                : new ReadOnlyCollection<ConfigSource>(new List<ConfigSource>(changedSources));
            ChangeSummary = changeSummary ?? string.Empty;
            ChangeTimeUtc = changeTimeUtc;
        }

        /// <summary>
        /// 获取旧快照。
        /// </summary>
        public FrameworkConfigSnapshot PreviousSnapshot { get; }

        /// <summary>
        /// 获取新快照。
        /// </summary>
        public FrameworkConfigSnapshot CurrentSnapshot { get; }

        /// <summary>
        /// 获取发生变化的配置来源集合。
        /// </summary>
        public IReadOnlyList<ConfigSource> ChangedSources { get; }

        /// <summary>
        /// 获取变化摘要。
        /// </summary>
        public string ChangeSummary { get; }

        /// <summary>
        /// 获取变化时间（UTC）。
        /// </summary>
        public DateTime ChangeTimeUtc { get; }
    }
}

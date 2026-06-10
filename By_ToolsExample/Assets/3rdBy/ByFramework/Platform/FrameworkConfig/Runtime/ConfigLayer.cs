//=====================================================
// 文件名称: ConfigLayer.cs
// 创 建 者: Codex
// 创建日期: 2026-06-08
// 描    述: 表示单个配置来源加载后的只读层数据。
//=====================================================

namespace _3rdBy.ByFramework.Platform.FrameworkConfig
{
    using System;

    /// <summary>
    /// 配置层描述。
    /// </summary>
    public sealed class ConfigLayer
    {
        /// <summary>
        /// 初始化配置层。
        /// </summary>
        /// <param name="source">配置来源。</param>
        /// <param name="priority">配置优先级。</param>
        /// <param name="identifier">来源标识。</param>
        /// <param name="rawJson">原始 JSON 文本。</param>
        /// <param name="loadedTimeUtc">加载时间（UTC）。</param>
        public ConfigLayer(
            ConfigSource source,
            ConfigPriority priority,
            string identifier,
            string rawJson,
            DateTime loadedTimeUtc)
        {
            if (string.IsNullOrWhiteSpace(identifier))
            {
                throw new ArgumentException("Config layer identifier cannot be null or empty.", nameof(identifier));
            }

            Source = source;
            Priority = priority;
            Identifier = identifier;
            RawJson = string.IsNullOrWhiteSpace(rawJson) ? "{}" : rawJson;
            LoadedTimeUtc = loadedTimeUtc;
        }

        /// <summary>
        /// 获取配置来源。
        /// </summary>
        public ConfigSource Source { get; }

        /// <summary>
        /// 获取配置优先级。
        /// </summary>
        public ConfigPriority Priority { get; }

        /// <summary>
        /// 获取来源标识。
        /// </summary>
        public string Identifier { get; }

        /// <summary>
        /// 获取原始 JSON 文本。
        /// </summary>
        public string RawJson { get; }

        /// <summary>
        /// 获取 UTC 加载时间。
        /// </summary>
        public DateTime LoadedTimeUtc { get; }
    }
}

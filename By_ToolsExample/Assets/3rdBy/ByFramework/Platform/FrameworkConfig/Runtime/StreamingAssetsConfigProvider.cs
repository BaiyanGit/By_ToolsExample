//=====================================================
// 文件名称: StreamingAssetsConfigProvider.cs
// 创 建 者: Codex
// 创建日期: 2026-06-08
// 描    述: 提供 StreamingAssets 部署配置层读取能力。
//=====================================================

namespace _3rdBy.ByFramework.Platform.FrameworkConfig
{
    using System.IO;
    using UnityEngine;

    /// <summary>
    /// StreamingAssets 配置提供器。
    /// </summary>
    public sealed class StreamingAssetsConfigProvider : JsonFileConfigProvider
    {
        /// <summary>
        /// 获取提供器对应的配置来源。
        /// </summary>
        public override ConfigSource Source => ConfigSource.StreamingAssets;

        /// <summary>
        /// 获取提供器优先级。
        /// </summary>
        public override ConfigPriority Priority => ConfigPriority.StreamingAssets;

        /// <summary>
        /// 获取提供器描述。
        /// </summary>
        public override string Description => "StreamingAssetsConfigProvider";

        protected override string ResolveFilePath()
        {
            return Path.Combine(
                Application.streamingAssetsPath,
                "ByFramework",
                "Config",
                "framework_config.json");
        }
    }
}

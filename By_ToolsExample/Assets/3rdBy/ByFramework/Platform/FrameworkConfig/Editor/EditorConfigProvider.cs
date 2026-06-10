//=====================================================
// 文件名称: EditorConfigProvider.cs
// 创 建 者: Codex
// 创建日期: 2026-06-08
// 描    述: 提供 Unity Editor 环境下的 Editor 生成配置层读取能力。
//=====================================================

namespace _3rdBy.ByFramework.Platform.FrameworkConfig
{
    using System.IO;
    using UnityEngine;

    /// <summary>
    /// Editor 生成配置提供器。
    /// </summary>
    public sealed class EditorConfigProvider : JsonFileConfigProvider
    {
        /// <summary>
        /// 获取提供器对应的配置来源。
        /// </summary>
        public override ConfigSource Source => ConfigSource.Editor;

        /// <summary>
        /// 获取提供器优先级。
        /// </summary>
        public override ConfigPriority Priority => ConfigPriority.Editor;

        /// <summary>
        /// 获取提供器描述。
        /// </summary>
        public override string Description => "EditorConfigProvider";

        protected override string ResolveFilePath()
        {
            if (!Application.isEditor)
            {
                return string.Empty;
            }

            return Path.Combine(
                Application.dataPath,
                "StreamingAssets",
                "ByFramework",
                "Config",
                "framework_config.editor.json");
        }
    }
}

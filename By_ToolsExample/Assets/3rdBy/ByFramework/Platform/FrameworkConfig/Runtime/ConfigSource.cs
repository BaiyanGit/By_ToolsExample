//=====================================================
// 文件名称: ConfigSource.cs
// 创 建 者: Codex
// 创建日期: 2026-06-08
// 描    述: 定义 FrameworkConfig 的配置来源类型。
//=====================================================

namespace _3rdBy.ByFramework.Platform.FrameworkConfig
{
    /// <summary>
    /// 配置来源枚举。
    /// </summary>
    public enum ConfigSource
    {
        /// <summary>
        /// 内置默认配置。
        /// </summary>
        Default = 0,

        /// <summary>
        /// Editor 生成配置。
        /// </summary>
        Editor = 1,

        /// <summary>
        /// StreamingAssets 部署配置。
        /// </summary>
        StreamingAssets = 2,

        /// <summary>
        /// PersistentDataPath 现场配置。
        /// </summary>
        Persistent = 3,

        /// <summary>
        /// 命令行覆盖配置。
        /// </summary>
        CommandLine = 4,
    }
}

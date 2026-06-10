//=====================================================
// 文件名称: ConfigPriority.cs
// 创 建 者: Codex
// 创建日期: 2026-06-08
// 描    述: 定义 FrameworkConfig 各配置来源的优先级。
//=====================================================

namespace _3rdBy.ByFramework.Platform.FrameworkConfig
{
    /// <summary>
    /// 配置优先级枚举。
    /// </summary>
    public enum ConfigPriority
    {
        /// <summary>
        /// 默认内置配置。
        /// </summary>
        Default = 0,

        /// <summary>
        /// Editor 生成配置。
        /// </summary>
        Editor = 100,

        /// <summary>
        /// StreamingAssets 部署配置。
        /// </summary>
        StreamingAssets = 200,

        /// <summary>
        /// PersistentDataPath 现场配置。
        /// </summary>
        Persistent = 300,

        /// <summary>
        /// 命令行覆盖配置。
        /// </summary>
        CommandLine = 400,
    }
}

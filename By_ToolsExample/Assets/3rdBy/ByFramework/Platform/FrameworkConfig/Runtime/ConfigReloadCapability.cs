//=====================================================
// 文件名称: ConfigReloadCapability.cs
// 创 建 者: Codex
// 创建日期: 2026-06-08
// 描    述: 定义 FrameworkConfig 对外暴露的配置重载能力状态。
//=====================================================

namespace _3rdBy.ByFramework.Platform.FrameworkConfig
{
    /// <summary>
    /// 配置重载能力状态。
    /// </summary>
    public enum ConfigReloadCapability
    {
        /// <summary>
        /// 不支持重载。
        /// </summary>
        NotSupported = 0,

        /// <summary>
        /// 支持重载。
        /// </summary>
        Reloadable = 1,

        /// <summary>
        /// 允许刷新配置，但业务生效需要模块自行决定是否重启。
        /// </summary>
        RestartRequired = 2,
    }
}

//=====================================================
// 文件名称: ServiceState.cs
// 创 建 者: Codex
// 创建日期: 2026-06-08
// 描    述: 定义 Platform 服务注册表记录的服务状态。
//=====================================================

namespace _3rdBy.ByFramework.Platform.PlatformServiceRegistry
{
    /// <summary>
    /// Platform 服务状态。
    /// </summary>
    public enum ServiceState
    {
        /// <summary>
        /// 未定义状态。
        /// </summary>
        None,

        /// <summary>
        /// 已注册。
        /// </summary>
        Registered,

        /// <summary>
        /// 初始化中。
        /// </summary>
        Initializing,

        /// <summary>
        /// 已初始化。
        /// </summary>
        Initialized,

        /// <summary>
        /// 释放中。
        /// </summary>
        Disposing,

        /// <summary>
        /// 已释放。
        /// </summary>
        Disposed,

        /// <summary>
        /// 失败。
        /// </summary>
        Failed
    }
}

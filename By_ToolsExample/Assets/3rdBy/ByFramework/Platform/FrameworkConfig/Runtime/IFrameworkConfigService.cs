//=====================================================
// 文件名称: IFrameworkConfigService.cs
// 创 建 者: Codex
// 创建日期: 2026-06-08
// 描    述: 定义 FrameworkConfig Runtime 服务的只读访问、重载与变更通知能力。
//=====================================================

namespace _3rdBy.ByFramework.Platform.FrameworkConfig
{
    using System;

    /// <summary>
    /// FrameworkConfig Runtime 服务接口。
    /// </summary>
    public interface IFrameworkConfigService : _3rdBy.ByFramework.Platform.PlatformServiceRegistry.IPlatformService
    {
        /// <summary>
        /// 获取当前配置快照。
        /// </summary>
        FrameworkConfigSnapshot CurrentSnapshot { get; }

        /// <summary>
        /// 当配置快照发生变化时触发。
        /// </summary>
        event Action<FrameworkConfigChangedEvent> ConfigChanged;

        /// <summary>
        /// 强制重新加载全部配置源，并返回最新快照。
        /// </summary>
        /// <returns>最新的只读配置快照。</returns>
        FrameworkConfigSnapshot Reload();

        /// <summary>
        /// 强制获取指定模块配置。
        /// </summary>
        /// <typeparam name="TConfig">模块配置类型。</typeparam>
        /// <returns>已解析的模块配置对象。</returns>
        /// <exception cref="System.InvalidOperationException">当模块配置不存在或无法解析时抛出。</exception>
        TConfig GetModuleConfig<TConfig>() where TConfig : class;

        /// <summary>
        /// 安全获取指定模块配置。
        /// </summary>
        /// <typeparam name="TConfig">模块配置类型。</typeparam>
        /// <param name="config">输出的模块配置对象。</param>
        /// <returns>获取成功返回 true，否则返回 false。</returns>
        bool TryGetModuleConfig<TConfig>(out TConfig config) where TConfig : class;
    }
}

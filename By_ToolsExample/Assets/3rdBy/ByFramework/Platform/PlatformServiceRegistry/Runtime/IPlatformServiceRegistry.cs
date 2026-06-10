//=====================================================
// 文件名称: IPlatformServiceRegistry.cs
// 创 建 者: Codex
// 创建日期: 2026-06-08
// 描    述: 定义 PlatformServiceRegistry 运行时接口能力。
//=====================================================

namespace _3rdBy.ByFramework.Platform.PlatformServiceRegistry
{
    using System.Collections.Generic;

    /// <summary>
    /// Platform 服务注册表接口。
    /// </summary>
    public interface IPlatformServiceRegistry
    {
        /// <summary>
        /// 注册指定类型的服务实例。
        /// </summary>
        /// <typeparam name="TService">服务接口类型。</typeparam>
        /// <param name="service">待注册的服务实例。</param>
        /// <exception cref="System.ArgumentNullException">当 <paramref name="service"/> 为空时抛出。</exception>
        /// <exception cref="System.InvalidOperationException">当服务已注册时抛出。</exception>
        void Register<TService>(TService service);

        /// <summary>
        /// 尝试注册指定类型的服务实例。
        /// </summary>
        /// <typeparam name="TService">服务接口类型。</typeparam>
        /// <param name="service">待注册的服务实例。</param>
        /// <returns>注册成功返回 true；已存在返回 false。</returns>
        /// <exception cref="System.ArgumentNullException">当 <paramref name="service"/> 为空时抛出。</exception>
        bool TryRegister<TService>(TService service);

        /// <summary>
        /// 显式替换指定类型的服务实例。
        /// </summary>
        /// <typeparam name="TService">服务接口类型。</typeparam>
        /// <param name="service">新的服务实例。</param>
        /// <exception cref="System.ArgumentNullException">当 <paramref name="service"/> 为空时抛出。</exception>
        void Replace<TService>(TService service);

        /// <summary>
        /// 注销指定类型的服务实例。
        /// </summary>
        /// <typeparam name="TService">服务接口类型。</typeparam>
        /// <returns>存在并移除时返回 true；不存在返回 false。</returns>
        bool Unregister<TService>();

        /// <summary>
        /// 判断指定类型的服务是否已注册。
        /// </summary>
        /// <typeparam name="TService">服务接口类型。</typeparam>
        /// <returns>已注册返回 true；否则返回 false。</returns>
        bool Contains<TService>();

        /// <summary>
        /// 获取指定类型的服务实例。
        /// </summary>
        /// <typeparam name="TService">服务接口类型。</typeparam>
        /// <returns>已注册的服务实例。</returns>
        /// <exception cref="System.InvalidOperationException">当服务未注册时抛出。</exception>
        TService Get<TService>();

        /// <summary>
        /// 尝试获取指定类型的服务实例。
        /// </summary>
        /// <typeparam name="TService">服务接口类型。</typeparam>
        /// <param name="service">输出的服务实例。</param>
        /// <returns>获取成功返回 true；否则返回 false。</returns>
        bool TryGet<TService>(out TService service);

        /// <summary>
        /// 将指定服务标记为 Initializing 状态。
        /// </summary>
        /// <typeparam name="TService">服务接口类型。</typeparam>
        /// <returns>服务存在并更新成功时返回 true；不存在返回 false。</returns>
        bool MarkInitializing<TService>();

        /// <summary>
        /// 将指定服务标记为 Initialized 状态。
        /// </summary>
        /// <typeparam name="TService">服务接口类型。</typeparam>
        /// <returns>服务存在并更新成功时返回 true；不存在返回 false。</returns>
        bool MarkInitialized<TService>();

        /// <summary>
        /// 将指定服务标记为 Disposing 状态。
        /// </summary>
        /// <typeparam name="TService">服务接口类型。</typeparam>
        /// <returns>服务存在并更新成功时返回 true；不存在返回 false。</returns>
        bool MarkDisposing<TService>();

        /// <summary>
        /// 将指定服务标记为 Disposed 状态。
        /// </summary>
        /// <typeparam name="TService">服务接口类型。</typeparam>
        /// <returns>服务存在并更新成功时返回 true；不存在返回 false。</returns>
        bool MarkDisposed<TService>();

        /// <summary>
        /// 将指定服务标记为 Failed 状态。
        /// </summary>
        /// <typeparam name="TService">服务接口类型。</typeparam>
        /// <returns>服务存在并更新成功时返回 true；不存在返回 false。</returns>
        bool MarkFailed<TService>();

        /// <summary>
        /// 获取全部服务描述信息的只读副本。
        /// </summary>
        /// <returns>当前服务表的只读副本。</returns>
        IReadOnlyList<ServiceDescriptor> GetAllDescriptors();

        /// <summary>
        /// 清空当前服务表。
        /// </summary>
        void Clear();
    }
}

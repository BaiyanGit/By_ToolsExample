//=====================================================
// 文件名称: PlatformServiceRegistry.cs
// 创 建 者: Codex
// 创建日期: 2026-06-08
// 描    述: 对外暴露 Platform 服务注册表静态入口。
//=====================================================

namespace _3rdBy.ByFramework.Platform.PlatformServiceRegistry
{
    using System.Collections.Generic;

    /// <summary>
    /// Platform 服务注册表静态入口。
    /// </summary>
    public static class PlatformServiceRegistry
    {
        // 内部默认实例保持可替换，便于后续验证工具与测试隔离静态状态。
        private static IPlatformServiceRegistry _instance = new PlatformServiceRegistryInstance();

        /// <summary>
        /// 获取当前内部注册表实例。
        /// </summary>
        public static IPlatformServiceRegistry Instance => _instance;

        /// <summary>
        /// 注册指定类型的服务实例。
        /// </summary>
        /// <typeparam name="TService">服务接口类型。</typeparam>
        /// <param name="service">待注册的服务实例。</param>
        public static void Register<TService>(TService service)
        {
            _instance.Register(service);
        }

        /// <summary>
        /// 尝试注册指定类型的服务实例。
        /// </summary>
        /// <typeparam name="TService">服务接口类型。</typeparam>
        /// <param name="service">待注册的服务实例。</param>
        /// <returns>注册成功返回 true；已存在返回 false。</returns>
        public static bool TryRegister<TService>(TService service)
        {
            return _instance.TryRegister(service);
        }

        /// <summary>
        /// 显式替换指定类型的服务实例。
        /// </summary>
        /// <typeparam name="TService">服务接口类型。</typeparam>
        /// <param name="service">新的服务实例。</param>
        public static void Replace<TService>(TService service)
        {
            _instance.Replace(service);
        }

        /// <summary>
        /// 注销指定类型的服务实例。
        /// </summary>
        /// <typeparam name="TService">服务接口类型。</typeparam>
        /// <returns>存在并移除时返回 true；不存在返回 false。</returns>
        public static bool Unregister<TService>()
        {
            return _instance.Unregister<TService>();
        }

        /// <summary>
        /// 判断指定类型的服务是否已注册。
        /// </summary>
        /// <typeparam name="TService">服务接口类型。</typeparam>
        /// <returns>已注册返回 true；否则返回 false。</returns>
        public static bool Contains<TService>()
        {
            return _instance.Contains<TService>();
        }

        /// <summary>
        /// 获取指定类型的服务实例。
        /// </summary>
        /// <typeparam name="TService">服务接口类型。</typeparam>
        /// <returns>已注册的服务实例。</returns>
        public static TService Get<TService>()
        {
            return _instance.Get<TService>();
        }

        /// <summary>
        /// 尝试获取指定类型的服务实例。
        /// </summary>
        /// <typeparam name="TService">服务接口类型。</typeparam>
        /// <param name="service">输出的服务实例。</param>
        /// <returns>获取成功返回 true；否则返回 false。</returns>
        public static bool TryGet<TService>(out TService service)
        {
            return _instance.TryGet(out service);
        }

        /// <summary>
        /// 将指定服务标记为 Initializing 状态。
        /// </summary>
        /// <typeparam name="TService">服务接口类型。</typeparam>
        /// <returns>服务存在并更新成功时返回 true；不存在返回 false。</returns>
        public static bool MarkInitializing<TService>()
        {
            return _instance.MarkInitializing<TService>();
        }

        /// <summary>
        /// 将指定服务标记为 Initialized 状态。
        /// </summary>
        /// <typeparam name="TService">服务接口类型。</typeparam>
        /// <returns>服务存在并更新成功时返回 true；不存在返回 false。</returns>
        public static bool MarkInitialized<TService>()
        {
            return _instance.MarkInitialized<TService>();
        }

        /// <summary>
        /// 将指定服务标记为 Disposing 状态。
        /// </summary>
        /// <typeparam name="TService">服务接口类型。</typeparam>
        /// <returns>服务存在并更新成功时返回 true；不存在返回 false。</returns>
        public static bool MarkDisposing<TService>()
        {
            return _instance.MarkDisposing<TService>();
        }

        /// <summary>
        /// 将指定服务标记为 Disposed 状态。
        /// </summary>
        /// <typeparam name="TService">服务接口类型。</typeparam>
        /// <returns>服务存在并更新成功时返回 true；不存在返回 false。</returns>
        public static bool MarkDisposed<TService>()
        {
            return _instance.MarkDisposed<TService>();
        }

        /// <summary>
        /// 将指定服务标记为 Failed 状态。
        /// </summary>
        /// <typeparam name="TService">服务接口类型。</typeparam>
        /// <returns>服务存在并更新成功时返回 true；不存在返回 false。</returns>
        public static bool MarkFailed<TService>()
        {
            return _instance.MarkFailed<TService>();
        }

        /// <summary>
        /// 获取全部服务描述信息的只读副本。
        /// </summary>
        /// <returns>当前服务表的只读副本。</returns>
        public static IReadOnlyList<ServiceDescriptor> GetAllDescriptors()
        {
            return _instance.GetAllDescriptors();
        }

        /// <summary>
        /// 清空当前服务表。
        /// </summary>
        public static void Clear()
        {
            _instance.Clear();
        }

        internal static void SetInstanceForTesting(IPlatformServiceRegistry registry)
        {
            _instance = registry ?? new PlatformServiceRegistryInstance();
        }
    }
}

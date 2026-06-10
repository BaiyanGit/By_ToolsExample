//=====================================================
// 文件名称: ServiceDescriptor.cs
// 创 建 者: Codex
// 创建日期: 2026-06-08
// 描    述: 保存 Platform 服务注册表对外暴露的只读服务描述信息。
//=====================================================

namespace _3rdBy.ByFramework.Platform.PlatformServiceRegistry
{
    using System;

    /// <summary>
    /// Platform 服务描述信息。
    /// </summary>
    public sealed class ServiceDescriptor
    {
        /// <summary>
        /// 初始化服务描述信息。
        /// </summary>
        /// <param name="serviceType">服务接口类型。</param>
        /// <param name="implementationType">服务实现类型。</param>
        /// <param name="instance">服务实例。</param>
        /// <param name="state">服务状态。</param>
        /// <param name="registerTime">注册时间。</param>
        /// <param name="source">来源描述。</param>
        /// <param name="isPlatformService">是否实现 IPlatformService。</param>
        /// <exception cref="ArgumentNullException">当关键参数为空时抛出。</exception>
        public ServiceDescriptor(
            Type serviceType,
            Type implementationType,
            object instance,
            ServiceState state,
            DateTime registerTime,
            string source,
            bool isPlatformService)
        {
            ServiceType = serviceType ?? throw new ArgumentNullException(nameof(serviceType));
            ImplementationType = implementationType ?? throw new ArgumentNullException(nameof(implementationType));
            Instance = instance ?? throw new ArgumentNullException(nameof(instance));
            Source = source ?? throw new ArgumentNullException(nameof(source));
            State = state;
            RegisterTime = registerTime;
            IsPlatformService = isPlatformService;
        }

        /// <summary>
        /// 获取服务接口类型。
        /// </summary>
        public Type ServiceType { get; }

        /// <summary>
        /// 获取服务实现类型。
        /// </summary>
        public Type ImplementationType { get; }

        /// <summary>
        /// 获取服务实例。
        /// </summary>
        public object Instance { get; }

        /// <summary>
        /// 获取服务状态。
        /// </summary>
        public ServiceState State { get; }

        /// <summary>
        /// 获取服务注册时间。
        /// </summary>
        public DateTime RegisterTime { get; }

        /// <summary>
        /// 获取服务来源描述。
        /// </summary>
        public string Source { get; }

        /// <summary>
        /// 获取服务是否实现 IPlatformService 标记接口。
        /// </summary>
        public bool IsPlatformService { get; }

        internal ServiceDescriptor WithState(ServiceState state)
        {
            return new ServiceDescriptor(
                ServiceType,
                ImplementationType,
                Instance,
                state,
                RegisterTime,
                Source,
                IsPlatformService);
        }
    }
}

//=====================================================
// 文件名称: PlatformServiceRegistryInstance.cs
// 创 建 者: Codex
// 创建日期: 2026-06-08
// 描    述: 承载 PlatformServiceRegistry 真实运行逻辑与线程安全控制。
//=====================================================

namespace _3rdBy.ByFramework.Platform.PlatformServiceRegistry
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Platform 服务注册表内部实例。
    /// </summary>
    public sealed class PlatformServiceRegistryInstance : IPlatformServiceRegistry
    {
        private const string LogPrefix = "[PlatformServiceRegistry]";

        // 服务字典只允许按 Type 注册，保持 R1.2 阶段的唯一 Service Key 规则。
        private readonly Dictionary<Type, ServiceDescriptor> _descriptors = new Dictionary<Type, ServiceDescriptor>();

        // 统一锁保护注册、替换、状态更新和枚举快照，避免并发读写破坏服务表一致性。
        private readonly object _syncRoot = new object();

        /// <summary>
        /// 注册指定类型的服务实例。
        /// </summary>
        /// <typeparam name="TService">服务接口类型。</typeparam>
        /// <param name="service">待注册的服务实例。</param>
        /// <exception cref="ArgumentNullException">当 <paramref name="service"/> 为空时抛出。</exception>
        /// <exception cref="InvalidOperationException">当服务已注册时抛出。</exception>
        public void Register<TService>(TService service)
        {
            Type serviceType = GetServiceType<TService>();
            ValidateServiceNotNull(service, serviceType);

            lock (_syncRoot)
            {
                if (_descriptors.ContainsKey(serviceType))
                {
                    throw new InvalidOperationException($"{LogPrefix} 服务已注册：{serviceType.FullName}");
                }

                _descriptors.Add(serviceType, CreateDescriptor(serviceType, service));
            }
        }

        /// <summary>
        /// 尝试注册指定类型的服务实例。
        /// </summary>
        /// <typeparam name="TService">服务接口类型。</typeparam>
        /// <param name="service">待注册的服务实例。</param>
        /// <returns>注册成功返回 true；已存在返回 false。</returns>
        /// <exception cref="ArgumentNullException">当 <paramref name="service"/> 为空时抛出。</exception>
        public bool TryRegister<TService>(TService service)
        {
            Type serviceType = GetServiceType<TService>();
            ValidateServiceNotNull(service, serviceType);

            lock (_syncRoot)
            {
                if (_descriptors.ContainsKey(serviceType))
                {
                    return false;
                }

                _descriptors.Add(serviceType, CreateDescriptor(serviceType, service));
                return true;
            }
        }

        /// <summary>
        /// 显式替换指定类型的服务实例。
        /// </summary>
        /// <typeparam name="TService">服务接口类型。</typeparam>
        /// <param name="service">新的服务实例。</param>
        /// <exception cref="ArgumentNullException">当 <paramref name="service"/> 为空时抛出。</exception>
        public void Replace<TService>(TService service)
        {
            Type serviceType = GetServiceType<TService>();
            ValidateServiceNotNull(service, serviceType);

            lock (_syncRoot)
            {
                _descriptors[serviceType] = CreateDescriptor(serviceType, service);
            }
        }

        /// <summary>
        /// 注销指定类型的服务实例。
        /// </summary>
        /// <typeparam name="TService">服务接口类型。</typeparam>
        /// <returns>存在并移除时返回 true；不存在返回 false。</returns>
        public bool Unregister<TService>()
        {
            Type serviceType = GetServiceType<TService>();

            lock (_syncRoot)
            {
                return _descriptors.Remove(serviceType);
            }
        }

        /// <summary>
        /// 判断指定类型的服务是否已注册。
        /// </summary>
        /// <typeparam name="TService">服务接口类型。</typeparam>
        /// <returns>已注册返回 true；否则返回 false。</returns>
        public bool Contains<TService>()
        {
            Type serviceType = GetServiceType<TService>();

            lock (_syncRoot)
            {
                return _descriptors.ContainsKey(serviceType);
            }
        }

        /// <summary>
        /// 获取指定类型的服务实例。
        /// </summary>
        /// <typeparam name="TService">服务接口类型。</typeparam>
        /// <returns>已注册的服务实例。</returns>
        /// <exception cref="InvalidOperationException">当服务未注册时抛出。</exception>
        public TService Get<TService>()
        {
            if (TryGet<TService>(out TService service))
            {
                return service;
            }

            Type serviceType = GetServiceType<TService>();
            throw new InvalidOperationException($"{LogPrefix} 未注册服务：{serviceType.FullName}");
        }

        /// <summary>
        /// 尝试获取指定类型的服务实例。
        /// </summary>
        /// <typeparam name="TService">服务接口类型。</typeparam>
        /// <param name="service">输出的服务实例。</param>
        /// <returns>获取成功返回 true；否则返回 false。</returns>
        public bool TryGet<TService>(out TService service)
        {
            Type serviceType = GetServiceType<TService>();

            lock (_syncRoot)
            {
                if (_descriptors.TryGetValue(serviceType, out ServiceDescriptor descriptor))
                {
                    service = (TService)descriptor.Instance;
                    return true;
                }
            }

            service = default;
            return false;
        }

        /// <summary>
        /// 将指定服务标记为 Initializing 状态。
        /// </summary>
        /// <typeparam name="TService">服务接口类型。</typeparam>
        /// <returns>服务存在并更新成功时返回 true；不存在返回 false。</returns>
        public bool MarkInitializing<TService>()
        {
            return MarkState<TService>(ServiceState.Initializing);
        }

        /// <summary>
        /// 将指定服务标记为 Initialized 状态。
        /// </summary>
        /// <typeparam name="TService">服务接口类型。</typeparam>
        /// <returns>服务存在并更新成功时返回 true；不存在返回 false。</returns>
        public bool MarkInitialized<TService>()
        {
            return MarkState<TService>(ServiceState.Initialized);
        }

        /// <summary>
        /// 将指定服务标记为 Disposing 状态。
        /// </summary>
        /// <typeparam name="TService">服务接口类型。</typeparam>
        /// <returns>服务存在并更新成功时返回 true；不存在返回 false。</returns>
        public bool MarkDisposing<TService>()
        {
            return MarkState<TService>(ServiceState.Disposing);
        }

        /// <summary>
        /// 将指定服务标记为 Disposed 状态。
        /// </summary>
        /// <typeparam name="TService">服务接口类型。</typeparam>
        /// <returns>服务存在并更新成功时返回 true；不存在返回 false。</returns>
        public bool MarkDisposed<TService>()
        {
            return MarkState<TService>(ServiceState.Disposed);
        }

        /// <summary>
        /// 将指定服务标记为 Failed 状态。
        /// </summary>
        /// <typeparam name="TService">服务接口类型。</typeparam>
        /// <returns>服务存在并更新成功时返回 true；不存在返回 false。</returns>
        public bool MarkFailed<TService>()
        {
            return MarkState<TService>(ServiceState.Failed);
        }

        /// <summary>
        /// 获取全部服务描述信息的只读副本。
        /// </summary>
        /// <returns>当前服务表的只读副本。</returns>
        public IReadOnlyList<ServiceDescriptor> GetAllDescriptors()
        {
            lock (_syncRoot)
            {
                // 这里返回快照副本，避免 Editor 工具或调用方持有内部集合后跨线程读写。
                return new List<ServiceDescriptor>(_descriptors.Values).AsReadOnly();
            }
        }

        /// <summary>
        /// 清空当前服务表。
        /// </summary>
        public void Clear()
        {
            lock (_syncRoot)
            {
                _descriptors.Clear();
            }
        }

        private static Type GetServiceType<TService>()
        {
            return typeof(TService);
        }

        private static void ValidateServiceNotNull<TService>(TService service, Type serviceType)
        {
            if (ReferenceEquals(service, null))
            {
                throw new ArgumentNullException(nameof(service), $"{LogPrefix} 注册服务不能为空：{serviceType.FullName}");
            }
        }

        private static ServiceDescriptor CreateDescriptor<TService>(Type serviceType, TService service)
        {
            Type implementationType = service.GetType();
            string source = implementationType.FullName ?? implementationType.Name;

            return new ServiceDescriptor(
                serviceType,
                implementationType,
                service,
                ServiceState.Registered,
                DateTime.UtcNow,
                source,
                service is IPlatformService);
        }

        private bool MarkState<TService>(ServiceState state)
        {
            Type serviceType = GetServiceType<TService>();

            lock (_syncRoot)
            {
                if (_descriptors.TryGetValue(serviceType, out ServiceDescriptor descriptor) == false)
                {
                    return false;
                }

                _descriptors[serviceType] = descriptor.WithState(state);
                return true;
            }
        }
    }
}

# P3.5B Platform Service Registration Runtime API Freeze

## 1. 目标

P3.5B 的目标是冻结 Platform Service Registration 的最终 Runtime API。

本阶段只冻结：

* Core 可见服务契约
* Runtime 注册贡献入口
* 生命周期方法签名
* 错误、取消、依赖与顺序规则
* Root Registry 与 Scoped Registry 边界

本阶段不实现：

* Registry 运行时代码
* FrameworkEntry 接入
* InputSystem 实现
* 任何 Platform Service 具体实现

## 2. 最终结论

### 2.1 架构归属

* `IService`
* `IServiceRegistry`
* `IServiceRegistrationProvider`
* `ServiceDescriptor`
* `ServiceLifetime`
* `ServiceDependency`
* `ServiceContext`
* `ServiceStage`
* `ServiceResult`
* `ServiceBatchResult`

以上类型归属 Core 中立契约层。

### 2.2 生命周期

Registry 固定编排：

```text
Register -> Initialize -> Start -> Stop -> Shutdown
```

说明：

* Register 通过 `Register(ServiceDescriptor)` 完成。
* Initialize / Start 正序执行。
* Stop 严格逆实际 Start 顺序。
* Shutdown 严格逆实际 Initialize 顺序。

### 2.3 生命周期权威

* Registry 是已注册 Service 的唯一生命周期权威。
* Singleton 只能作为迁移期访问方式。
* FrameworkEntry 只驱动 Root Registry，不持有 Platform 实现构造细节。

## 3. 冻结的 Runtime API

### 3.1 IService

```csharp
public interface IService
{
    ValueTask InitializeAsync(ServiceContext context, CancellationToken cancellationToken);
    ValueTask StartAsync(ServiceContext context, CancellationToken cancellationToken);
    ValueTask StopAsync(ServiceContext context, CancellationToken cancellationToken);
    ValueTask ShutdownAsync(ServiceContext context, CancellationToken cancellationToken);
}
```

冻结说明：

* Register 不属于 `IService` 方法，由 Descriptor 提交完成。
* 所有生命周期方法统一使用 `ValueTask`。
* 不引入 Unity 专用异步类型，保持 Core 中立。
* 无同步重载；同步实现直接返回已完成 `ValueTask`。

### 3.2 ServiceLifetime

```csharp
public enum ServiceLifetime
{
    Singleton = 0,
    Scoped = 1
}
```

冻结说明：

* Root Registry 管理 Singleton。
* Child / Scoped Registry 管理 Scoped。
* 当前不冻结 Transient，避免退化为 Service Locator。

### 3.3 ServiceStage

```csharp
public enum ServiceStage
{
    Register = 0,
    Initialize = 1,
    Start = 2,
    Stop = 3,
    Shutdown = 4
}
```

### 3.4 ServiceDependency

```csharp
public readonly record struct ServiceDependency(
    Type ServiceType,
    bool Optional = false);
```

冻结说明：

* 依赖声明使用 Service 契约类型，不使用实现类型。
* Optional 依赖不进入拓扑硬阻塞，但必须在 `ServiceContext` 中可判空消费。

### 3.5 ServiceDescriptor

```csharp
public sealed class ServiceDescriptor
{
    public string ServiceId { get; }
    public Type ServiceType { get; }
    public Type ImplementationType { get; }
    public ServiceLifetime Lifetime { get; }
    public IReadOnlyList<ServiceDependency> Dependencies { get; }
    public int StartupOrder { get; }
    public Func<ServiceContext, IService> Factory { get; }
}
```

冻结说明：

* `ServiceId` 是稳定标识，用于日志、诊断和错误聚合。
* `ServiceType` 是公开契约类型，例如 `typeof(IResourceService)`。
* `ImplementationType` 只用于诊断，不作为依赖匹配键。
* `StartupOrder` 只在同层无依赖服务之间生效。
* Factory 必须显式提供，不允许默认反射构造。

### 3.6 ServiceContext

```csharp
public sealed class ServiceContext
{
    public IServiceRegistry Registry { get; }
    public ServiceDescriptor Descriptor { get; }
    public string ScopeName { get; }
    public IReadOnlyDictionary<Type, IService> ResolvedDependencies { get; }

    public TService GetRequired<TService>() where TService : class, IService;
    public bool TryGet<TService>(out TService service) where TService : class, IService;
}
```

冻结说明：

* Context 是当前 Service 的只读运行时上下文。
* Context 不携带 FrameworkConfig 强类型 Platform 配置对象。
* Context 不作为事件总线、配置中心或业务状态容器。

### 3.7 IServiceRegistry

```csharp
public interface IServiceRegistry
{
    string ScopeName { get; }
    IServiceRegistry? Parent { get; }

    void Register(ServiceDescriptor descriptor);
    bool IsRegistered(Type serviceType);
    IReadOnlyList<ServiceDescriptor> GetDescriptors();

    bool TryGet<TService>(out TService service) where TService : class, IService;
    TService GetRequired<TService>() where TService : class, IService;

    ValueTask<ServiceBatchResult> InitializeAsync(CancellationToken cancellationToken = default);
    ValueTask<ServiceBatchResult> StartAsync(CancellationToken cancellationToken = default);
    ValueTask<ServiceBatchResult> StopAsync(CancellationToken cancellationToken = default);
    ValueTask<ServiceBatchResult> ShutdownAsync(CancellationToken cancellationToken = default);

    IServiceRegistry CreateScope(string scopeName);
}
```

冻结说明：

* Root Registry 由应用组合层创建。
* `CreateScope(string scopeName)` 创建 Child Registry，用于 Scoped Service。
* `TryGet` / `GetRequired` 只返回已注册且已创建实例。
* Registry 不提供“按字符串或任意类型自动构造”能力。

### 3.8 IServiceRegistrationProvider

```csharp
public interface IServiceRegistrationProvider
{
    void RegisterServices(IServiceRegistry registry);
}
```

冻结说明：

* 这是 Runtime 注册贡献入口。
* Platform、FeatureModule 或应用组合层通过 Provider 向 Registry 提交 Descriptor。
* FrameworkEntry 不扫描 Provider；Provider 列表由组合层显式提供。

### 3.9 ServiceResult

```csharp
public enum ServiceResultCode
{
    Success = 0,
    Cancelled = 1,
    DuplicateRegistration = 2,
    InvalidDescriptor = 3,
    DependencyMissing = 4,
    DependencyCycle = 5,
    FactoryFailed = 6,
    InitializeFailed = 7,
    StartFailed = 8,
    StopFailed = 9,
    ShutdownFailed = 10
}

public sealed class ServiceResult
{
    public ServiceStage Stage { get; }
    public string ServiceId { get; }
    public Type ServiceType { get; }
    public ServiceResultCode Code { get; }
    public string Message { get; }
    public Exception? Exception { get; }
    public bool IsSuccess { get; }
}
```

### 3.10 ServiceBatchResult

```csharp
public sealed class ServiceBatchResult
{
    public ServiceStage Stage { get; }
    public IReadOnlyList<ServiceResult> Results { get; }
    public bool IsSuccess { get; }
}
```

冻结说明：

* Stop 与 Shutdown 必须返回聚合结果。
* 单个 Service 失败不得阻断同阶段其它 Service 的清理。

## 4. 取消与错误规则

### 4.1 Initialize / Start

* 支持 `CancellationToken`。
* 取消后不得继续启动未执行服务。
* 已完成 Initialize 或 Start 的服务必须记录为实际结果，不得伪装为未执行。

### 4.2 Stop / Shutdown

* 支持 `CancellationToken` 作为有界等待信号。
* Registry 即使收到取消，也必须继续完成已进入清理队列的服务收口。
* Shutdown 返回后不得残留“后台继续释放”的悬空任务。

### 4.3 错误聚合

* Factory、Initialize、Start、Stop、Shutdown 错误统一映射到 `ServiceResultCode`。
* Registry 必须返回完整 `ServiceBatchResult`。
* 不允许第一个异常直接中断整个批次并丢失后续结果。

## 5. 顺序与依赖图规则

* Required Dependency 完全决定 Initialize / Start 拓扑顺序。
* `StartupOrder` 只用于无依赖同层稳定排序。
* Optional Dependency 不改变拓扑阻塞。
* 依赖循环直接返回 `DependencyCycle`，禁止运行时隐式打破。

## 6. Root Registry 与 Scoped Registry

### 6.1 Root Registry

Root Registry 负责：

* Framework 级 Singleton Service
* FrameworkEntry 启动编排入口
* Platform 默认服务组合

### 6.2 Scoped Registry

Scoped Registry 负责：

* FeatureModule 独立服务
* 临时运行域服务
* 用户会话或模块会话服务

冻结说明：

* Scoped Service 允许依赖 Root Singleton。
* Root Singleton 不允许依赖 Scoped Service。
* FrameworkEntry 只直接编排 Root Registry。

## 7. FrameworkConfig 边界

FrameworkConfig 只允许保存：

* 稳定默认 Provider / Profile / Policy 标识
* 外部配置入口引用
* 启动必需的中立配置标识

FrameworkConfig 禁止保存：

* ServiceDescriptor
* Factory
* Registry 结构
* Platform 强类型配置对象
* 运行时已解析服务实例

补充说明：

* 当前源码目录中 `Core/Config` 仍承载 FrameworkConfig 入口。
* 架构归属上，FrameworkConfig 按 Platform 配置能力看待；Core 只消费其中立入口。

## 8. Platform 范围映射

P3.5B 只冻结注册 API，不冻结以下系统实现，但确认它们未来通过 Registry 接入：

* InputSystem
* UISystem
* UIThemeSystem
* DisplaySystem
* LocalizationSystem
* ResourceSystem
* SaveSystem
* BuildProfileSystem
* LicenseSystem
* NetworkSystem

补充约束：

* AssetBundle 的 Editor 打包工具、Runtime 加载、Manifest、Hash 与 Dependency 归属 ResourceSystem。
* Client、Server、Http 与 LargeFileDownloader 归属 NetworkSystem。
* NetworkSystem 内部必须拆分 NetworkCore 与 NetworkSystem Adapter。
* NetworkCore 必须保持纯 C#、低依赖、可独立抽离，不允许依赖 FrameworkEntry、PlatformServiceRegistry、FrameworkConfig、ThreadDispatcher、EventManager、FSMManager 或 UnityEngine。
* NetworkSystem Adapter 负责把 NetworkCore 接入 PlatformServiceRegistry、FrameworkConfig、ThreadDispatcher 与 EventManager。
* Downloader 的多线程下载、断点续传、Hash 校验和下载缓存属于 NetworkSystem 内部能力，不单独变成 Root Service。
* RuntimeConfigUI 属于 FrameworkConfig 消费边界，不进入 Core 生命周期链。
* DOF 暂不进入主架构。

## 9. 与当前阶段关系

P3.5A 已完成本地 Unity 验证，Core Early 生命周期链状态为：

```text
ThreadDispatcher -> EventManager -> FSMManager
Shutdown:
FSMManager -> EventManager -> ThreadDispatcher
```

P3.5B 在此基础上继续推进：

* 不回退到 P3.5A
* 不重新审查 FrameworkEntry
* 不进入 InputSystem 实现

## 10. 下一阶段

P3.5B 冻结完成后，下一阶段进入：

```text
P3.6 InputSystem Foundation
```

不是 `UISystem Foundation`，也不是 `InputSystem Implementation`。

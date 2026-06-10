# P3.1 Platform Service Registration Design

## 1. 目标

Platform Service Registration 用于解决以下问题：

* 避免 `FrameworkConfig -> Platform` 强类型引用将 `Core -> Platform` 反向依赖固化。
* 避免 FrameworkEntry 直接引用、扫描或构造 Platform Service 具体实现。
* 为 ResourceSystem、SaveSystem、InputSystem、LocalizationSystem、DisplaySystem、LicenseSystem、NetworkSystem 等 Platform 能力提供统一注册入口。
* 为后续 FeatureModule 扩展提供可组合、可替换、可验证的服务接入边界。

## 2. P3.1 已冻结结论

P3.1 只冻结方向与边界，不冻结最终 C# 方法签名。

已确认：

* `IService`、`IServiceRegistry`、`ServiceDescriptor`、`ServiceLifetime` 与 `ServiceContext` 属于 Core 可见中立契约。
* Platform、FeatureModule 与应用组合层显式提交 Descriptor、Factory 和依赖声明。
* FrameworkEntry 只编排 Core 可见 Registry，不扫描程序集、不引用 Platform 类型、不直接构造 Platform 实现。
* 生命周期固定为 `Register -> Initialize -> Start -> Stop -> Shutdown`。
* Registry 是已注册 Service 的唯一生命周期权威。
* Singleton 仅允许作为迁移期兼容访问方式。

## 3. 核心边界

### 3.1 Core

Core 只负责：

* 服务契约定义。
* 生命周期编排接口。
* 依赖图与确定性顺序规则。
* 错误聚合边界。

Core 不负责：

* Platform 具体实现。
* 自动反射扫描。
* 业务配置解析。
* FeatureModule 具体实例创建。

### 3.2 Platform

Platform 负责：

* 提供具体 Service 契约与实现。
* 提供 Descriptor、Factory、依赖和生命周期行为。
* 通过组合层显式注册服务。

Platform 不负责：

* 让 Core 静态引用 Platform 类型。
* 让 FrameworkEntry 直接 `new` 平台对象。

### 3.3 FeatureModule

FeatureModule 可以：

* 消费已注册 Platform Service。
* 为自身能力提交额外 ServiceDescriptor。
* 基于 Scoped Registry 承载独立模块服务。

FeatureModule 不得：

* 反向污染 Core 或 Platform 契约。
* 绕过 Registry 建立新的生命周期权威。

## 4. 发现与注册原则

默认原则：

* 不使用程序集反射扫描。
* 不使用未注册具体类型自动创建。
* 不把 Descriptor、Factory 或注册列表写入 FrameworkConfig。

默认流程：

1. 应用组合层创建 Registry。
2. 组合层调用一个或多个注册提供者提交 ServiceDescriptor。
3. FrameworkEntry 只驱动 Registry 生命周期。
4. 消费方只依赖公开服务契约。

## 5. 生命周期原则

* Register 只发生在 Descriptor 提交阶段。
* Initialize 与 Start 顺序由显式依赖图决定。
* `StartupOrder` 只用于无依赖同层稳定排序。
* Stop 严格逆实际 Start 顺序。
* Shutdown 严格逆实际 Initialize / 创建顺序。
* Stop 与 Shutdown 必须隔离异常并聚合结果。
* Shutdown 完成后禁止后台释放继续漂移。

## 6. FrameworkConfig 关系

FrameworkConfig 不保存：

* ServiceDescriptor
* Factory
* 服务注册列表
* Platform 强类型 Provider / Policy / Context

FrameworkConfig 只允许保存：

* 默认 Profile 标识
* 默认 Provider 标识
* 必要启动引用
* 稳定中立配置入口

## 7. 与 FrameworkEntry Phase2A 的关系

Phase2A 的 ThreadDispatcher、EventManager、FSMManager 接管验证了：

* 生命周期五阶段可以落地。
* Registry 才能成为下一步正式权威。
* FrameworkEntry 适合作为编排入口，但不适合作为具体对象构造者。

P3.1 的设计结论为 P3.5B 的最终 Runtime API 冻结提供方向基线。

## 8. 后续阶段

P3.1 之后需要完成：

* P3.5A Core Early Lifecycle Unity Verification
* P3.5B Platform Service Registration Runtime API Freeze
* Runtime 注册贡献入口与应用组合层冻结
* 后续最小实现与架构守卫

最终 Runtime C# API 见 `PlatformServiceRegistrationRuntimeAPIFreeze.md`。

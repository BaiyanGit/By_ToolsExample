# ByFramework 当前项目上下文

> 本文是当前 ByFramework 的权威项目快照。
>
> 快照日期：2026-06-08
>
> 后续 Codex 会话必须优先阅读本文，不依赖旧会话。

# 项目定位

ByFramework 是面向 Unity 的跨平台、模块化、可配置、可扩展通用应用框架。

长期目标：

* 保持 Core、Platform、FeatureModule 的单向依赖结构
* 为工业仿真、驾驶模拟、数字孪生、培训系统、管理系统和在线系统提供可复用基础设施
* 通过稳定契约、生命周期编排、资源、存储、网络与授权能力支撑不同项目组合

# 当前架构基线

```text
Core
  - FrameworkEntry
  - EventManager
  - FSM
  - ThreadDispatcher
  - Singleton
  - Common

Platform
  - PlatformServiceRegistry
  - FrameworkConfig
  - InputSystem
  - UISystem
  - UIThemeSystem
  - DisplaySystem
  - LocalizationSystem
  - ResourceSystem
  - SaveSystem
  - BuildProfileSystem
  - LicenseSystem
  - NetworkSystem

FeatureModule
  - SimulationSync
  - SimulationServer
  - VehicleSimulation
  - Other Modules
```

依赖规则：

* Core 不依赖 Platform
* Platform 不依赖 FeatureModule
* FeatureModule 可以依赖 Platform + Core

# 当前已完成阶段

## P0 已完成

* MetaFramework -> ByFramework
* 框架路径硬编码整改
* EventManager 合并旧事件中心能力
* FrameworkEntry 第一阶段基础入口
* FrameworkConfig 第一阶段基础设施

## P1 已完成

* SingletonAudit
* SingletonSafetyAudit
* Architecture Alignment

## P2 已完成

* FrameworkEntry Phase2 Design
* P2.1 ~ P2.12 全部设计与集成审查
* Platform Architecture、InputSystem、UISystem、DisplaySystem、LocalizationSystem、SaveSystem、ResourceSystem、BuildProfileSystem、LicenseSystem、NetworkSystem、FrameworkConfig 设计冻结

## P3 已完成到当前节点

* P3.1 Platform Service Registration Design
* P3.2 ResourceSystem Contract Design
* P3.3 SaveSystem Contract Design
* P3.4 FrameworkEntry Phase2A Design Review
* P3.4A ThreadDispatcher Narrow Implementation
* P3.4B EventManager Integration Review
* P3.4C EventManager Narrow Implementation
* P3.4D FSMManager Integration Review
* P3.4E FSMManager Narrow Implementation
* P3.4F FrameworkEntry Phase2A Closure Review
* P3.5A Core Early Lifecycle Unity Verification
* P3.5B Platform Service Registration Runtime API Freeze
* P3.6 InputSystem Foundation

# 当前冻结结论

## FrameworkEntry 生命周期链

当前 Core Early 生命周期链已冻结为：

```text
ThreadDispatcher -> EventManager -> FSMManager
```

Shutdown 顺序已冻结为：

```text
FSMManager -> EventManager -> ThreadDispatcher
```

生命周期阶段已冻结为：

```text
Register -> Initialize -> Start -> Stop -> Shutdown
```

## 生命周期权威

* `FrameworkEntry` 是当前 Core Early 生命周期唯一编排权威
* `Singleton / Instance` 仅作为兼容访问方式
* 不允许旧 `AfterSceneLoad Init()` 与 FrameworkEntry 双权威并存

## P3.5A Unity 验证结论

项目维护者已完成本地 Unity 实机验证：

```text
FrameworkEntry = 1
ThreadDispatcher = 1
EventManager = 1
FSMManager = 1
```

并确认：

* 静态访问正常
* 重复实例检查正常
* Core 生命周期链验证通过

## Platform Service Registration 当前状态

`P3.1` 已完成概念设计，`P3.5B` 已完成 Runtime API 冻结。

当前冻结方向：

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

关键原则：

* Core 只拥有中立契约，不直接引用 Platform 强类型
* Platform Service 必须显式注册，不做反射扫描
* FrameworkEntry 只编排 Core 可见 Registry，不直接构造 Platform 业务对象
* Registry 是唯一生命周期权威

## InputSystem 当前状态

`P2.2 InputSystem Design` 已完成总体架构设计，`P3.6 InputSystem Foundation` 已完成 Foundation 冻结。

当前已冻结：
* `InputAction` 身份格式、别名迁移与稳定标识规则
* `InputValueKind` 与 `InputStage` 基础运行时分类
* `InputContext` 生命周期、所有权、优先级、独占、透传与消费规则
* `InputProfile` 的 Framework Default、Project、FeatureModule、User Override 分层与合并规则
* `InputDevice / InputAdapter` Foundation 边界
* `IndustrialControlPanel` 作为复合设备建模，而不是退化为简单按钮集合

当前仍未开始：
* InputSystem Runtime Implementation
* 具体平台 InputAdapter
* 最终 C# Runtime API 落地

# Platform 关键边界

## FrameworkConfig

FrameworkConfig 支持：

* EditorWindow 配置
* StreamingAssets 外部配置
* PersistentDataPath 用户配置
* RuntimeConfigUI

但 FrameworkConfig 不得重新回到“万能配置中心”定位，也不得保存 ServiceDescriptor、Factory 或 Platform 强类型配置对象。

当前仓库中的 `Core/Config/FrameworkConfig.cs`、`FrameworkConfigProvider.cs` 与 `Resources/FrameworkConfig.asset` 属于早期兼容配置基础设施，不代表最终架构归属。后续不得继续把 UI、Network、Download 等 Platform 强类型配置扩展进 Core。新增配置能力应以 `Platform/FrameworkConfig` 为目标设计，并通过迁移 / Adapter 保持旧项目兼容。

## ResourceSystem

AssetBundle 归属 ResourceSystem，包含：

* Editor 打包工具
* Runtime 加载系统
* Manifest
* Hash
* Dependency

当前仍以 `Resources` 为默认实现方向，不引入 Addressables。

## NetworkSystem

NetworkSystem 包含：

* NetworkCore
* NetworkSystem Adapter
* Client
* Server
* Http
* LargeFileDownloader

Downloader 能力包括：

* 多线程下载
* 断点续传
* Hash 校验
* 下载缓存


## NetworkCore 独立性

NetworkCore 必须保持纯 C#、低依赖、可独立抽离。

NetworkCore 不得依赖：

* FrameworkEntry
* PlatformServiceRegistry
* FrameworkConfig
* ThreadDispatcher
* EventManager
* FSMManager
* UnityEngine
* MonoBehaviour
* ScriptableObject

NetworkSystem Adapter 负责把 NetworkCore 接入 ByFramework，包括读取 FrameworkConfig、注册 PlatformServiceRegistry、接入 ThreadDispatcher 与 EventManager。

当前旧 `Socket` 与 `Http/DownLoad` 代码只能作为迁移参考，不得直接作为最终 NetworkCore 实现。

## DOF

DOF 暂时不进入主架构。

# 当前主要风险

* Platform Service Registration 已完成 API 冻结，但尚未进入 Runtime 实现
* ResourceSystem 与 SaveSystem 已冻结运行时契约，但尚未完成最终 C# API 与实现设计
* InputSystem 已完成 Foundation 冻结，但尚未进入实现
* 目录结构与架构分层仍有历史遗留，后续迁移必须保持 API 稳定

# 下一阶段

当前应直接进入：

```text
P3.7 UISystem Foundation
```

不是 `InputSystem Implementation`，也不是 `NetworkSystem Implementation`。

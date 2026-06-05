# ByFramework 当前项目上下文

> 本文是 P2 设计阶段完成后、进入 P3 前的统一项目上下文快照。
>
> 快照日期：2026-06-05
>
> 后续新 Codex 窗口必须优先阅读本文，不依赖历史会话。

# 项目定位

ByFramework 是基于 Unity 的跨平台、模块化、可配置、可扩展通用应用框架。它提供行业无关的 Core 基础设施与 Platform 能力，并允许具体项目通过 FeatureModule 独立组合和扩展。

长期目标：

* 建立稳定、低耦合、可替换、可裁剪的框架能力。
* 保持 Core 与 Platform 不绑定具体行业、设备、车辆或业务流程。
* 支持不同项目按需组合 FeatureModule。
* 建立清晰的服务注册、生命周期、配置、资源、存储和协议扩展规范。
* 逐步形成可验证的模块生态、示例工程和 API 文档。

当前平台支持目标：

* Windows
* Linux

长期支持项目类型：

* 工业仿真
* 驾驶模拟
* 数字孪生
* 培训系统
* 管理系统
* 在线系统
* 其它 Unity 项目

---

# 当前架构

```text
Core
├─ FrameworkEntry
├─ FrameworkConfig
├─ EventManager
├─ FSM
├─ ThreadDispatcher
├─ Singleton
└─ Common

Platform
├─ InputSystem
├─ UISystem
├─ DisplaySystem
├─ LocalizationSystem
├─ SaveSystem
├─ ResourceSystem
├─ BuildProfileSystem
├─ LicenseSystem
└─ NetworkSystem

FeatureModule
├─ SimulationSync
├─ SimulationServer
├─ VehicleSimulation
└─ Other Modules
```

分层规则：

* Core 是最小基础层，不依赖 Platform 或 FeatureModule。
* Platform 提供行业无关的可复用平台能力，可以依赖 Core，不依赖 FeatureModule。
* FeatureModule 承载业务能力，可以依赖 Core 与 Platform。
* Platform 子系统之间只允许经过确认的单向依赖。

---

# Core

## FrameworkEntry

统一框架启动与生命周期编排入口。当前第一阶段只创建持久化 `[ByFramework]` 根节点并预留初始化阶段，尚未接管 Platform System。

## FrameworkConfig

框架启动配置与默认配置入口。只提供轻量、稳定、业务无关的默认标识、配置引用和必要启动开关。

## EventManager

通用跨模块事件通知能力。不得作为业务状态容器、同步服务接口或无类型服务定位器。

## FSM

通用状态机基础能力。不得内置 Platform 状态模型或具体业务流程。

## ThreadDispatcher

通用跨线程回主线程调度能力，可被未来高性能网络等系统消费，但不负责业务任务生命周期。

## Singleton

现有实例基础设施与兼容基线。仍存在 Domain Reload、静态状态和场景实例生命周期风险，后续必须渐进整改。

---

# Platform

## InputSystem

将键盘、鼠标、手柄、VR 控制器、硬件按钮、工业控制面板和自定义设备输入转换为设备无关的 InputAction；负责 Binding、Profile、Device 与 Context，不负责 UI 导航或业务行为。

## UISystem

负责 Theme、Focus、Navigation、Window 与 Layer；单向消费 Input、Display、Localization 与 Resource 能力，不负责业务流程。

## DisplaySystem

负责显示能力、模式、区域、布局、Adapter 与只读 DisplayContext；不负责 UI、业务 Camera 或业务内容。

## LocalizationSystem

负责 Language、LanguagePack、LocalizationKey、查询、回退与运行时语言切换；不直接控制 UI，不使用翻译文本作为业务标识。

## SaveSystem

负责 Scope、Key、Profile、Provider、版本、迁移、原子写入、备份与恢复；不理解消费系统的业务数据语义，不保存资源内容。

## ResourceSystem

负责 ResourceKey、Location、Provider、Catalog、Context 与 Handle；当前继续使用 Resources 与 AssetDatabase，不引入 Addressables，不开始大规模重构。

## BuildProfileSystem

Editor 优先的构建编排系统，负责模块、Feature、环境、变体与 BuildManifest；Runtime 只消费只读构建元数据。

## LicenseSystem

负责激活、校验、Policy、Feature 与 BuildVariant 授权；支持完全离线模式，可选消费 NetworkSystem，不负责账号、支付或加密算法实现。

## NetworkSystem

负责通用 Transport、Connection、Session、协议封装、序列化、Connectivity、Heartbeat、Reconnect 与 Diagnostics；不理解业务协议语义或仿真同步。

---

# FeatureModule

## SimulationSync

负责车辆位置、姿态、关节、物理、碰撞、Command、State、Snapshot、Delta、Event Sync、Interpolation、Prediction 与 Rollback 等同步语义。

## SimulationServer

负责 Unity Headless、Dedicated Server、Linux Server 与 Windows Server 的业务同步和 Server 权威逻辑。NetworkSystem 只提供通信管道。

## VehicleSimulation

负责车辆相关输入动作、业务状态、业务 UI、协议、资源和同步模型，不进入 Core 或 Platform。

## Other Modules

其它培训、管理、数字孪生、工具或项目业务模块遵守相同边界：可以消费 Core 与 Platform，不得反向污染框架层。

---

# 已完成阶段

## P0 已完成

* [x] MetaFramework -> ByFramework
* [x] 修复旧命名空间引用
* [x] P0-0.1 移除硬编码框架路径
* [x] EventManager 合并旧事件中心能力
* [x] FrameworkEntry 第一阶段基础入口
* [x] FrameworkConfig 第一阶段配置基础设施

## P1 已完成

* [x] SingletonAudit
* [x] P1.1 SingletonSafetyAudit，包括代码复审与最小生命周期安全修复
* [x] P1.2 Architecture Alignment

尚未完成的 P1 实现项包括 Singleton 重构、日志系统和统一生命周期管理。

## P2 已完成设计

* [x] FrameworkEntry Phase2 Design
* [x] P2.1 Platform Architecture Design
* [x] P2.2 InputSystem Design
* [x] P2.3 UISystem Design
* [x] P2.4 DisplaySystem Design
* [x] P2.5 LocalizationSystem Design
* [x] P2.6 SaveSystem Design
* [x] P2.7 ResourceSystem Design
* [x] P2.8 BuildProfileSystem Design
* [x] P2.9 LicenseSystem Design
* [x] P2.10 NetworkSystem Design
* [x] P2.11 FrameworkConfig Phase2 Design
* [x] P2.12 Platform Integration Review
* [x] P2 阶段统一项目上下文快照

说明：P2 完成表示总体架构和边界设计已形成，不表示 Platform System 已实现、迁移或接入 FrameworkEntry。

---

# 已确认架构原则

## 低耦合原则

* 每个系统必须明确职责、不负责范围、依赖、禁止依赖和扩展点。
* 消费者依赖公开契约，不依赖具体 Provider 或业务实现。
* 不建立集中式全能管理器。

## 单向依赖原则

* Core 不依赖 Platform。
* Core 不依赖 FeatureModule。
* Platform 可以依赖 Core。
* Platform 不依赖 FeatureModule。
* FeatureModule 可以依赖 Core 与 Platform。
* Platform 子系统禁止形成循环依赖。

## 行业无关原则

* Core 与 Platform 禁止硬编码具体车辆、关节、碰撞、物理状态、项目协议或行业流程。
* 仿真同步、业务协议和 Server 权威逻辑归属 FeatureModule。

## FrameworkConfig 精简原则

* 标识优先于内容。
* 默认值优先于运行时状态。
* 用户和机器数据进入 SaveSystem 或专用存储。
* 构建组合进入 BuildProfileSystem。
* 资源图谱和内容进入 ResourceSystem。
* 敏感凭据进入 LicenseSystem 或平台安全存储。

---

# FrameworkConfig 最终定位

FrameworkConfig 是：

> 框架启动配置与默认配置入口。

FrameworkConfig 不是：

> 万能配置中心。

允许保存：

* 轻量框架启动开关。
* 默认 Profile、Provider、Policy 标识。
* 启动阶段必要的独立配置引用。

禁止保存：

* 用户数据、机器校准和运行时状态。
* 完整 BuildProfile、ResourceCatalog、Manifest 和资源清单。
* LicenseKey、激活数据、授权缓存和敏感凭据。
* NetworkSession、窗口状态、焦点状态和当前 Context。
* 具体业务数据或模块通信状态。

当前高风险：FrameworkConfig 位于 Core，若未来直接引用 Platform 强类型配置，会形成 `Core → Platform` 反向依赖。

---

# NetworkSystem 关键结论

* NetworkSystem 与 SimulationSync 分离。NetworkSystem 不负责车辆、关节、物理、碰撞或业务状态同步。
* NetworkSystem 与 SimulationServer 分离。NetworkSystem 只提供通信管道、Session、Codec、序列化和诊断。
* Google.Protobuf 运行库位于 `Assets/3rdBy/ByFramework/Socket/Plugins/GoogleProto/`，禁止重复引入。
* pb 转 C# 工具位于 `Assets/3rdBy/ByTools/CompileProto/Editor/CompileProtoEditorWindow.cs`，禁止重复实现或破坏。
* 当前仓库未发现 `.proto` 源文件；协议源文件归属、目录、版本和可重复生成能力待审计。
* 现有生成代码位于 `Socket/ProtoFile/Proto/` 与 `ProtoCSharp/`。
* 高性能通信长期规划包括多线程收发、主线程解耦、ThreadDispatcher、批处理、压缩、带宽控制、频率控制、延迟统计、丢包统计、诊断和非阻塞通信。
* UDP、KCP / Reliable UDP 与 LocalLoopbackProvider 当前仅预留，不实现。

---

# 当前风险与待办

## 高风险

* FrameworkConfig 强类型引用 Platform 可能形成 `Core → Platform` 反向依赖。
* FrameworkEntry 直接构造 Platform 具体实现会扩大 Core 依赖。
* BuildProfileSystem Editor 逻辑进入 Runtime，或 Runtime ResourceSystem 依赖 BuildProfile Editor。
* NetworkSystem 被业务协议、SimulationSync 或 SimulationServer 权威逻辑污染。

## P3 前置待办

* Platform Service Registration 尚未设计。
* Core、Platform、FeatureModule 依赖方向架构守卫尚未设计。
* SaveSystem Scope、Key、Schema、Provider、迁移和错误契约待细化与实现。
* ResourceSystem Key、Catalog、Handle、Provider 与兼容迁移契约待细化与实现。
* FrameworkConfig 当前 Guide、UI 路径、Socket 地址端口和下载目录字段待迁移审计。
* Platform System 仍处于设计态，禁止一次性批量实现或重构。

---

# P3 推荐实施顺序

## P3.1 Platform Service Registration Design

设计服务注册、接口发现、生命周期、错误模型、Provider 替换和依赖方向架构守卫。

## P3.2 ResourceSystem Contract Design

冻结 ResourceKey、ResourceLocation、ResourceCatalog、ResourceProvider、ResourceContext、ResourceHandle 与现有 Resources / AssetDatabase 兼容契约。

## P3.3 SaveSystem Contract Design

冻结 SaveScope、SaveKey、SaveEntry、SaveProfile、SaveProvider、SaveContext、迁移和可靠写入契约。

## P3.4 FrameworkEntry Phase2A

按既有设计渐进接管 ThreadDispatcher，并验证初始化权威、退出、静态重置和 Domain Reload 行为。

## P3.5 InputSystem Implementation

在 Action、Binding、Profile、Context、Adapter 与 SaveSystem 契约冻结后进行最小实现和迁移审计。

## P3.6 UISystem Foundation

在 Input、Display、Localization 与 Resource 契约稳定后，建立 Theme、Focus、Navigation、Window 与 Layer 基础能力。

每一阶段必须先确认范围、契约、允许修改文件和验证方式，不得一次性实现全部 Platform System。

---

# 新窗口启动说明

未来新 Codex 窗口开始工作时：

1. 请先阅读 `Documentation/ByFramework_Current_Context.md`。
2. 再阅读当前任务对应的独立设计文档。
3. 所有整改、设计与实现均以本文档和当前仓库状态为准。
4. 不要依赖历史会话。
5. 开始任务前确认本次属于设计、审计还是实现，并确认允许修改范围。
6. 未经明确批准，不修改 Runtime、Editor、API，不开始大规模迁移或重构。

P2 设计阶段已完成。下一推荐任务为：

> P3.1 Platform Service Registration Design

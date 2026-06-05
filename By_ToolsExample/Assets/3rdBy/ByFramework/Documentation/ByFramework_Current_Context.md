# ByFramework 当前项目上下文

> 本文是后续新窗口继续开发时的统一上下文快照。
>
> 快照日期：2026-06-05

# 项目定位

ByFramework 是一个基于 Unity 的跨平台、模块化、可配置、可扩展的通用应用框架。它提供业务无关的基础能力与平台能力，并允许具体项目通过 FeatureModule 按需组合和扩展。

长期目标：

* 建立稳定、低耦合、可替换、可裁剪的 Core 与 Platform 能力。
* 保持框架与具体行业、设备、车辆和业务流程解耦。
* 支持不同项目独立组合 FeatureModule，并逐步形成清晰的模块生态、示例工程和 API 文档。

当前目标平台为 Windows 与 Linux，长期可支持：

* 工业仿真
* 驾驶模拟
* 数字孪生
* 培训系统
* 机器人项目
* 无人机项目
* 其它 Unity 应用

---

# 当前架构

```text
Core
├── FrameworkEntry
├── FrameworkConfig
├── EventManager
├── FSM
├── ThreadDispatcher
├── Singleton
└── Common

Platform
├── DisplaySystem
├── InputSystem
├── UISystem
├── LocalizationSystem
├── ResourceSystem
├── LicenseSystem
├── BuildProfileSystem
├── NetworkSystem
└── SaveSystem

FeatureModule
├── VehicleSimulation
├── DrivingSimulation
├── DigitalTwin
├── TrainingSystem
├── RobotSystem
└── OtherProjectModules
```

## Core

Core 是框架的最小基础层，负责统一入口、轻量启动配置、事件、状态机、线程调度、单例基础设施和通用能力。Core 必须保持业务无关，不依赖 Platform 或 FeatureModule。

## Platform

Platform 提供可复用、业务无关的平台能力，可以依赖 Core，但不得依赖 FeatureModule。Platform 子系统必须保持职责独立和单向依赖，禁止形成循环依赖。

Platform 当前按职责分组：

* Foundation：ResourceSystem、SaveSystem
* Environment：DisplaySystem、InputSystem
* Experience：LocalizationSystem、UISystem
* Operations：NetworkSystem、LicenseSystem、BuildProfileSystem

## FeatureModule

FeatureModule 是业务扩展层，可以依赖 Core 与 Platform。具体车辆、设备、训练流程、项目输入配置、业务协议和业务资源均归属 FeatureModule，不得反向污染 Core 或 Platform。

---

# 已完成整改

## P0 全部完成项

* [x] MetaFramework -> ByFramework
* [x] 修复旧命名空间引用
* [x] P0-0.1 移除硬编码框架路径
* [x] EventManager 合并旧事件中心能力
* [x] FrameworkEntry 第一阶段基础入口
* [x] FrameworkConfig 第一阶段配置基础设施

## P1 全部完成项

* [x] SingletonAudit
* [x] P1.1 SingletonSafetyAudit，包括代码复审与最小生命周期安全修复
* [x] P1.2 Architecture Alignment

说明：Singleton 重构、日志系统和统一生命周期管理尚未开始，不属于已完成项。

## P2 已完成设计项

* [x] FrameworkEntry Phase2 Design
* [x] P2.1 Platform Architecture Design
* [x] P2.2 InputSystem Design 总体设计

说明：以上条目仅表示设计基线已经形成，不表示对应系统已经实现、迁移或由 FrameworkEntry 接管。

---

# 已确认架构原则

## 低耦合原则

* 所有系统必须模块化、低耦合并允许替换实现。
* 每个系统必须明确职责、不负责范围、依赖边界、扩展点及配置关系。
* 不建立集中式全能管理器。
* 不因单个项目需求污染通用框架设计。

## 依赖规则

* Core 不依赖 Platform，也不依赖 FeatureModule。
* Platform 可以依赖 Core，但不得依赖 FeatureModule。
* FeatureModule 可以依赖 Core 与 Platform。
* Platform 子系统之间只允许经过设计确认的单向依赖，禁止随意双向依赖或循环依赖。
* 模块间优先通过接口、EventManager、配置或服务注册通信。
* 避免直接构造其它模块的具体实现或硬编码具体类型。
* InputSystem、DisplaySystem、LocalizationSystem 不依赖 UISystem；UISystem 单向消费这些系统的公开契约。

## FrameworkConfig 约束

FrameworkConfig 只保存轻量、稳定、业务无关的启动配置，例如模块启用开关、默认 Profile 标识、默认 Provider 标识和默认运行策略。

FrameworkConfig 不承载：

* 业务数据或运行时状态
* 用户配置或用户存档
* 完整语言包或完整资源清单
* 授权状态、授权码、令牌或密钥
* 机器特定校准数据
* 模块通信总线职责

## FeatureModule 约束

* 业务动作、业务输入配置、业务协议和业务资源归属 FeatureModule。
* `VehicleSimulationInputProfile` 等具体业务配置不得进入 Core 或 Platform。
* Core 与 Platform 不得引用具体 FeatureModule 类型。
* FeatureModule 应通过通用接口、事件、Profile 和 Provider 扩展框架能力。

---

# Platform 当前规划

## DisplaySystem

统一描述显示设备、逻辑视口和输出布局，规划支持单屏、分屏、多联屏、融屏、特殊比例、多分辨率、VR 与 Non-VR；不负责 UI 主题、焦点或具体业务摄像机逻辑。

## InputSystem

将 Keyboard、Mouse、Gamepad、VRController、HardwareButton 和 CustomDevice 等输入映射为设备无关的 InputAction。核心模型包括 InputAction、InputBinding、InputProfile、InputDevice 和 InputContext。

## UISystem

规划负责 UI Theme、UIFocusSystem、UIInputNavigationSystem 和显示适配。UISystem 单向消费 DisplaySystem、InputSystem、LocalizationSystem 与 ResourceSystem。

## LocalizationSystem

规划支持默认语言、运行时语言切换、文本查询、语言包扩展和 FeatureModule 独立语言包。系统负责本地化机制，各模块维护自己的具体翻译内容。

## ResourceSystem

规划负责统一资源定位、加载、释放、模块资源扩展与隔离，并保留 AssetBundle 扩展方向。当前继续使用 Resources，不开始重构，也不引入 Addressables。

## LicenseSystem

规划支持软件激活、授权验证、机器码、离线授权与可选在线验证。密钥、令牌和授权状态不得写入 FrameworkConfig 或源码。

## BuildProfileSystem

规划负责构建配置、功能裁剪、模块裁剪和资源裁剪。它属于构建期编排能力，Runtime 只消费构建生成的只读 Profile 元数据。

## NetworkSystem

规划统一 HTTP、Socket、连接、断线、重连、超时与退出生命周期边界；不定义具体业务协议，也不自动建立项目专属连接。

## SaveSystem

规划负责跨平台持久化位置、序列化、版本、迁移、备份和错误恢复。用户 Binding、设备偏好、机器校准、授权状态和 FeatureModule 存档应通过 SaveSystem 或专用安全存储管理。

---

# 当前禁止事项

暂不实现：

* Addressables
* ResourceSystem 重构
* FrameworkEntry Phase2A
* UI 重构
* InputSystem 实现

同时禁止在未独立设计和确认范围前批量实现 Platform 系统，或将业务数据与运行时状态写入 FrameworkConfig。

---

# 下一阶段任务

## P2.2 InputSystem Design

P2.2 已完成总体设计基线。下一阶段继续进行 InputSystem 契约细化，不进入代码实现。

设计目标：

* 建立稳定、设备无关的 InputAction 语义及标识策略。
* 定义 InputContext 的优先级、独占、透传、消费和异常恢复规则。
* 定义 InputProfile 的分层合并、冲突诊断、覆盖和版本迁移策略。
* 定义可替换的 InputDevice Adapter 接口，隔离不同设备和外部硬件协议。
* 支持开发期键鼠模拟与运行期外部硬件共享同一 Action 消费路径。
* 明确 FrameworkConfig、SaveSystem、LocalizationSystem、UISystem 和 FeatureModule 的输入边界。
* 保持 InputSystem 对 UISystem、DisplaySystem、LocalizationSystem 和具体 FeatureModule 实现零依赖。
* 在任何实现工作开始前，完成现有直接输入调用的迁移审计与风险清单。

建议细化顺序：

1. InputAction 标识与值类型设计
2. InputContext 优先级与消费规则设计
3. InputProfile 合并、冲突与版本策略设计
4. InputDevice Adapter 接口设计
5. SaveSystem 用户 Binding 持久化契约设计
6. Localization Binding Display Token 契约设计
7. 现有直接输入调用迁移审计

---

# 后续新窗口启动说明

新窗口开始工作时，按以下顺序读取：

1. `AGENTS.md`
2. `Documentation/ByFramework_Current_Context.md`
3. `Documentation/Architecture.md`
4. `Documentation/Roadmap.md`
5. 当前任务对应的独立设计文档

继续 P2.2 InputSystem Design 时，还需读取：

* `Documentation/PlatformArchitectureDesign.md`
* `Documentation/InputSystemDesign.md`

新窗口启动任务前必须先确认：

* 当前任务属于设计、审计还是实现。
* 本次允许修改的文件范围。
* 是否禁止修改 Runtime、Editor 或 API。
* 是否会改变已验证行为。
* 是否需要同步更新 Changelog、Roadmap 或 Todo。

除非用户明确批准，不得开始 FrameworkEntry Phase2A、ResourceSystem 重构、Addressables、UI 重构或 InputSystem 实现。

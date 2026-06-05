# ByFramework Architecture

## 框架定位

ByFramework 是基于 Unity 的跨平台、模块化、可配置、可扩展的通用应用框架。

框架可用于工业仿真、驾驶模拟、数字孪生、培训系统、机器人、无人机及其它类型 Unity 项目，但 ByFramework 本身不绑定具体行业、设备、车型或业务流程。

业务能力应作为 FeatureModule 扩展，不应反向污染 Core 与 Platform。

当前目标平台：

* Windows
* Linux

后续可扩展其它平台。

## 架构原则

* Core 提供业务无关的基础能力和统一生命周期入口。
* Platform 提供可复用的平台级系统，并通过配置或接口支持不同项目需求。
* FeatureModule 承载具体业务功能，可按项目独立组合与裁剪。
* Core 与 Platform 不得写死具体车辆、设备或业务类型。
* FeatureModule 可以依赖 Core 与 Platform。
* Platform 可以依赖 Core，但不应依赖具体 FeatureModule。
* Core 不应依赖 Platform 或 FeatureModule。
* 当前目录结构与该长期分层不完全一致，后续应逐步迁移，禁止一次性大范围搬迁。

## 模块化与低耦合约束

ByFramework 所有现有系统与未来系统必须遵守模块化、低耦合和可替换原则：

* Core 不依赖 Platform，也不依赖 FeatureModule。
* Platform 可以依赖 Core，但 Platform 子系统之间禁止随意双向依赖或形成循环依赖。
* FeatureModule 可以依赖 Core 与 Platform，但 Core 和 Platform 禁止反向依赖 FeatureModule。
* 模块之间优先通过接口、事件、配置和服务注册访问，避免直接 `new` 其它模块实现或硬编码具体类型。
* 跨模块通知优先使用 EventManager；需要同步调用或能力替换时优先使用接口抽象。
* 业务模块不得污染框架核心，不得因单个项目需求将具体业务类型写入 Core 或 Platform。
* FrameworkConfig 只保存轻量启动配置，不承载业务数据、运行时状态、用户配置或资源清单。
* InputSystem、UISystem、DisplaySystem 与 LocalizationSystem 必须保持明确的单向依赖关系。
* 每个系统必须具备清晰职责、依赖边界和可替换扩展点，不应成为集中式全能管理器。

后续每个系统设计文档必须包含：

* 系统职责。
* 不负责什么。
* 可依赖模块。
* 禁止依赖模块。
* 可扩展点。
* 与 FrameworkConfig 的关系。

## 架构分层

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
├─ DisplaySystem
├─ UISystem
├─ InputSystem
├─ LocalizationSystem
├─ ResourceSystem
├─ LicenseSystem
├─ BuildProfileSystem
├─ NetworkSystem
└─ SaveSystem

FeatureModule
├─ VehicleSimulation
├─ DrivingSimulation
├─ DigitalTwin
├─ TrainingSystem
├─ RobotSystem
└─ OtherProjectModules
```

### Core

Core 是 ByFramework 的最小基础层，负责框架启动、配置、事件、状态机、线程调度、单例基础设施与通用能力。

Core 必须保持业务无关，不包含行业规则、具体设备逻辑或项目专属流程。

### Platform

Platform 是可复用的平台能力层，为不同类型 Unity 应用提供显示、资源、本地化、授权、构建、网络与存档能力。

Platform 系统应支持按项目配置和按需启用，不应假设某一种业务场景。

Platform 内部按职责分为：

* Foundation：ResourceSystem、SaveSystem。
* Environment：DisplaySystem、InputSystem。
* Experience：LocalizationSystem、UISystem。
* Operations：NetworkSystem、LicenseSystem、BuildProfileSystem。

Platform 系统可以依赖 Core，但不得依赖 FeatureModule。Platform 内部依赖必须保持单向，避免系统之间形成循环依赖。

FrameworkConfig 只保存 Platform 启动所需的轻量默认值、开关和 Profile 标识，不保存语言包内容、资源清单、用户数据、授权状态或构建过程数据。

Platform 整体职责、依赖与配置边界见 `PlatformArchitectureDesign.md`。

### FeatureModule

FeatureModule 是业务模块扩展层，不绑定具体行业。它可用于仿真、培训、数字孪生、机器人项目及其它 Unity 应用。

FeatureModule 不属于 ByFramework Core。不同项目可以独立实现、组合、替换或移除 FeatureModule。

## 当前 Core 决策

### FrameworkEntry

* FrameworkEntry 作为统一启动入口。
* 第一阶段只负责创建持久化框架根节点并预留初始化阶段。
* 当前不接管现有模块，后续模块必须逐个迁移并独立验证。
* 第二阶段采用渐进式接管策略，FrameworkEntry 作为生命周期编排入口，不强制统一所有模块的单例形式。
* 第一优先级候选为 ThreadDispatcher、EventManager、FSMManager，必须逐个迁移、验证和保留回滚能力。
* Guide Dispatcher 与 SoundManager 为第二优先级候选，接入前必须先明确各自生命周期与资源边界。
* UIRoot、UIManager、ClientManager、DownloadManager 与 GuideManager 当前保持场景或现有调用方管理，不由 FrameworkEntry 接管。
* FrameworkEntry 不应直接依赖场景数据、业务模块或尚未完成生命周期设计的 Platform 模块。
* 详细设计与迁移门槛见 `FrameworkEntryPhase2Design.md`。

### FrameworkConfig

* FrameworkConfig 作为 Runtime 统一配置中心。
* FrameworkConfigProvider 负责加载配置并提供全局访问入口。
* FrameworkEditorConfig 与 Runtime 配置分离，仅用于 Editor 工具。
* 第一阶段只建立配置基础设施，现有模块后续逐步迁移读取逻辑。

### EventManager

* EventManager 是唯一事件入口，旧事件中心兼容层已移除。
* 支持 Struct、Class、Async、Type 与 Enum 事件。
* Runtime Listener 对外 API 使用 `AddListener`、`Broadcast`、`RemoveListener`。
* Type 事件使用 Type 本身作为 Key。
* Enum 事件使用枚举实例本身作为 Key，不使用 `eventId.GetType()`。
* 保留 `Publish`、`PublishAsync`、`PublishClass` API。

### Singleton

* 当前存在多种单例实现，详见 `SingletonAudit.md` 与 `SingletonSafetyAudit.md`。
* Singleton 重构必须保持现有 API，并根据模块生命周期逐步实施。
* 不应为了统一形式，将依赖场景引用的组件改为隐式创建。

## Platform 长期规划

以下内容仅记录架构方向，当前尚未开始实现。

### DisplaySystem

DisplaySystem 负责不同显示环境与输出形态的统一适配，长期支持：

* 单屏
* 分屏
* 多联屏
* 融屏
* 特殊比例屏幕
* 多分辨率
* VR
* Non-VR

目标显示比例与形态包括但不限于：

* 16:9
* 21:9
* 32:9
* 3:4
* 竖屏
* 自定义显示墙

UI 系统未来应具备分辨率与显示形态适配能力。当前阶段不修改 UIRoot、CanvasScaler 或 UIManager。

### UISystem

UISystem 是 Platform 层的通用 UI 能力，长期负责主题、输入导航、焦点规则和显示适配，不包含具体业务界面。

#### UI Theme System

UI Theme System 长期支持统一切换：

* 颜色
* 字体
* 按钮样式
* 图标
* 背景

主题能力属于框架级能力，具体业务内容仍由 FeatureModule 提供。

#### UIInputNavigationSystem

UIInputNavigationSystem 长期支持：

* 纯键盘操作
* 键盘与鼠标
* 手柄
* 外部硬件按钮
* 控制面板按钮

开发阶段可以使用键盘和鼠标模拟最终硬件输入。

#### UIFocusSystem

UI 必须支持由框架控制的焦点导航规则，包括：

* 上下移动
* 左右切换
* 标签切换
* 确认
* 取消

不应将 UGUI Navigation 作为未来核心方案。复杂界面、多屏、特殊布局和硬件按钮输入场景需要更明确、可控的焦点规则。

### InputSystem

InputSystem 是 Platform 层的通用输入抽象。UI 与业务模块不直接依赖具体输入设备，而是消费统一输入指令，例如：

* Up
* Down
* Left
* Right
* Confirm
* Cancel
* SwitchTabLeft
* SwitchTabRight

输入来源可以是键盘、鼠标、手柄、VR 控制器、单片机按钮、控制面板按钮或其它外部设备，所有来源统一映射为框架输入指令。

InputSystem 的核心模型包括：

* InputAction：稳定的设备无关动作语义。
* InputBinding：Action 与具体设备输入之间的映射。
* InputProfile：默认、项目、业务模块与用户覆盖配置的组合。
* InputDevice：设备发现、能力描述与底层适配边界。
* InputContext：UI、Gameplay、Debug、Tool 等上下文的优先级与消费规则。

InputSystem 负责产生语义动作，不负责执行 UI 焦点规则或业务行为。UISystem 和 FeatureModule 消费 InputSystem，InputSystem 不依赖它们。

#### Input Profile

ByFramework 长期提供业务无关的通用输入配置能力：

* InputAction
* InputBinding
* InputProfile
* InputDevice

框架不定义具体业务键位。不同车辆、设备或项目的键值配置必须由 FeatureModule 提供，例如 `VehicleSimulationInputProfile`，并在业务模块内维护设备或业务动作映射。

FrameworkConfig 后续只保存默认 Input Profile 标识、基础 Context 和设备选择策略。用户 Binding、设备校准和机器特定配置由 SaveSystem 持久化，不写入 FrameworkConfig。

LocalizationSystem 负责 Binding 显示名称与输入提示模板的本地化；UISystem 只消费 UI 语义动作，不直接读取具体设备。

详细设计见 `InputSystemDesign.md`。当前阶段不实现 UISystem、InputSystem 或 Input Profile，也不修改现有输入与 UI 代码。

### BuildProfileSystem

BuildProfileSystem 负责业务无关的构建配置与项目能力组合，长期支持：

* 构建配置
* 模块裁剪
* 功能开关
* 资源裁剪

BuildProfile 使用类似 `Profile_A`、`Profile_B`、`Profile_C` 的业务无关命名，不使用具体行业、设备或车型命名。

### LicenseSystem

LicenseSystem 负责通用软件授权能力，长期支持：

* 软件激活
* 授权验证
* 机器码
* 离线授权

### ResourceSystem

ResourceSystem 负责统一资源加载与模块资源边界，长期支持：

* AssetBundle 打包
* AssetBundle 加载
* 模块资源扩展
* 资源隔离

当前 UI 仍使用 Resources。现阶段不开始 ResourceSystem 重构，也不引入 Addressables。

### LocalizationSystem

LocalizationSystem 是 Platform 层的通用多语言能力，不绑定具体业务模块。

长期支持语言包括但不限于：

* 中文（简体）
* 中文（繁体）
* 英文
* 日文
* 韩文
* 俄文
* 阿拉伯文
* 其它扩展语言

长期支持本地化内容：

* UI 文本
* 提示信息
* 日志文本
* 配置文本
* 帮助文档
* 动态加载文本

LocalizationSystem 应支持运行时切换语言、配置默认语言、扩展语言资源，以及由业务模块增加独立语言包。

推荐语言包边界：

```text
Localization
├─ CoreLanguagePack
├─ VehicleSimulationLanguagePack
├─ TrainingLanguagePack
└─ OtherModuleLanguagePack
```

框架只负责：

* 语言管理
* 语言切换
* 文本查询
* 语言资源加载

具体翻译内容由 Core、Platform 或 FeatureModule 对应模块自行维护。框架不得集中硬编码具体业务翻译。

当前阶段仅记录 LocalizationSystem 架构方向，不实现多语言系统。

### 其它 Platform 系统

* NetworkSystem：统一 HTTP、Socket 等网络能力边界。
* SaveSystem：提供跨平台、可扩展的数据持久化能力。

以上系统均处于长期规划阶段，尚未形成最终 API。

Platform 系统必须独立设计与实施。当前不实现任何 Platform 系统，不开始 ResourceSystem 重构，不引入 Addressables，也不由 FrameworkEntry 接管尚未完成生命周期设计的 Platform 模块。

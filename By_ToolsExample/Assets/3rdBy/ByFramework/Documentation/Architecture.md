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
├─ SimulationSync
├─ SimulationServer
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

P2.12 Platform Integration Review 已确认 Platform 总体设计依赖方向成立，未发现必须存在的 Runtime 循环依赖，并识别 FrameworkConfig 强类型引用 Platform、Platform 服务注册、BuildProfile Editor / Runtime 隔离，以及 SaveSystem 与 ResourceSystem 详细契约等 P3 风险。完整审查见 `PlatformIntegrationReview.md`。

P3.1 Platform Service Registration Design 已冻结服务组合方向：`IService`、`IServiceRegistry`、`ServiceDescriptor`、`ServiceLifetime` 与 `ServiceContext` 的中立契约归属 Core；Platform、FeatureModule 与应用组合层显式提交 Descriptor、Factory 和实现。FrameworkEntry 只编排 Core 可见 Registry，不扫描、不引用或直接构造 Platform 类型。详细设计见 `PlatformServiceRegistrationDesign.md`。

P3.2 ResourceSystem Runtime Contract Design 已冻结 `IResourceService`、`IResourceProvider`、`IResourceCatalog`、`IResourceHandle`、`ResourceRequest` 与 `ResourceResult` 的 Runtime 边界。ResourceKey 与路径、文件名和 Provider 解耦；Catalog 使用不可变版本化 Snapshot；每个成功消费者获得独立 Handle，底层资源可以共享；Runtime 禁止依赖 AssetDatabase 或 Editor。详细设计见 `ResourceSystemContractDesign.md`。

P3.3 SaveSystem Runtime Contract Design 已冻结 `ISaveService`、`ISaveProvider`、`SaveScope`、`SaveProfile`、`SaveEntry`、`SaveTransaction` 与 `SaveMigration` 的 Runtime 边界。Scope 使用 Owner 与 Subject 组合隔离；Profile 是不可变策略 Snapshot；事务原子性只在单一 Provider 明确能力边界内保证；迁移失败不得覆盖最后一个可恢复版本。详细设计见 `SaveSystemContractDesign.md`。

P3.4 FrameworkEntry Phase2A Design Review 已冻结 FrameworkEntry 的最终编排职责与 Platform Service 五阶段生命周期。Phase2A 可以进入仅 ThreadDispatcher 的窄范围实施准备；完整 Registry 编排和所有 Platform Service 当前仍不得直接进入 Runtime 实现。详细审查见 `FrameworkEntryPhase2AReview.md`。

P3.4A 已完成 ThreadDispatcher 窄范围代码接管：FrameworkEntry 成为唯一生命周期编排入口，ThreadDispatcher 保留 `Current` 与现有公开 API，并保持实际 `AfterSceneLoad` 初始化时点以复用首场景预放置实例。完整 Registry、其它 Core 模块与 Platform Service 均未接入。实施记录见 `FrameworkEntryPhase2AThreadDispatcherImplementation.md`。

P3.4B EventManager Integration Review 已确认 EventManager 适合进入 FrameworkEntry 窄范围生命周期编排，但必须处理旧 `AfterSceneLoad` 自动入口、`EnsureInstance()` 兼容访问、运行时监听表、声明式事件表和 EventTypePool 的关闭清理边界。详细审查见 `EventManagerIntegrationReview.md`。

P3.4C 已完成 EventManager 窄范围代码接管：FrameworkEntry 在 ThreadDispatcher 之后编排 EventManager，并在 Shutdown 时先关闭 EventManager 再关闭 ThreadDispatcher。`EventManager.Instance` 与 `EnsureInstance()` 保持兼容，Shutdown 覆盖运行时监听表、声明式事件表和 EventTypePool。实施记录见 `FrameworkEntryPhase2AEventManagerImplementation.md`。

P3.4D FSMManager Integration Review 已确认 FSMManager 适合进入 FrameworkEntry 窄范围生命周期编排，但必须处理旧 `AfterSceneLoad` 自动入口、`_machines` 状态机列表、StateMachine 条件委托、当前状态退出和 Shutdown 清理边界。详细审查见 `FSMManagerIntegrationReview.md`。

### FeatureModule

FeatureModule 是业务模块扩展层，不绑定具体行业。它可用于仿真、培训、数字孪生、机器人项目及其它 Unity 应用。

FeatureModule 不属于 ByFramework Core。不同项目可以独立实现、组合、替换或移除 FeatureModule。

SimulationSync、SimulationServer、VehicleSimulation 与其它业务模块必须保持在 FeatureModule。车辆、关节、物理、碰撞、仿真协议与 Server 权威逻辑禁止进入 Platform。

## 当前 Core 决策

### FrameworkEntry

* FrameworkEntry 作为统一启动入口。
* 第一阶段负责创建持久化框架根节点并预留初始化阶段。
* P3.4A 已窄范围接管 ThreadDispatcher，P3.4C 已窄范围接管 EventManager；其它模块仍必须逐个迁移并独立验证。
* 第二阶段采用渐进式接管策略，FrameworkEntry 作为生命周期编排入口，不强制统一所有模块的单例形式。
* 第一优先级候选为 ThreadDispatcher、EventManager、FSMManager，必须逐个迁移、验证和保留回滚能力。
* Guide Dispatcher 与 SoundManager 为第二优先级候选，接入前必须先明确各自生命周期与资源边界。
* UIRoot、UIManager、ClientManager、DownloadManager 与 GuideManager 当前保持场景或现有调用方管理，不由 FrameworkEntry 接管。
* FrameworkEntry 不应直接依赖场景数据、业务模块或尚未完成生命周期设计的 Platform 模块。
* FrameworkEntry 未来只通过 Core 可见 `IServiceRegistry` 编排已注册 Platform Service，不扫描程序集、不引用 Platform 类型，也不直接构造 Platform 实现。
* FrameworkEntry 只驱动 Register、Initialize、Start、Stop 与 Shutdown 阶段；具体 Service 顺序、实例和状态由 Registry 管理。
* Phase2A 已渐进接管 ThreadDispatcher 与 EventManager，未实现 Registry、未启动 Platform Service，也未顺带接管 FSMManager。
* P3.4D 已完成 FSMManager 接入评审；P3.4E 仅允许窄范围接管 FSMManager，不接管其它模块、不修改业务状态机逻辑、不引入 Registry。
* 详细设计与迁移门槛见 `FrameworkEntryPhase2Design.md`。

### FrameworkConfig

* FrameworkConfig 固定为“框架启动配置与默认配置入口”，不是 Runtime 万能配置中心。
* FrameworkConfigProvider 负责加载配置并提供只读默认配置访问入口。
* FrameworkEditorConfig 与 Runtime 配置分离，仅用于 Editor 工具。
* FrameworkConfig 只保存轻量、稳定、业务无关的默认 Profile、Provider、Policy 标识、配置引用与必要启动开关。
* FrameworkConfig 源码禁止声明 Platform 强类型 Profile、Provider、Policy、Context 或配置字段，也不保存 ServiceDescriptor、Factory 或服务注册列表。
* Platform 强类型配置由 Platform 或应用组合层拥有和解析；FrameworkConfig 如确需提供 Platform 启动默认，只能使用经过准入审查的稳定标识或 Core 中立引用。
* 用户和机器数据归属 SaveSystem，构建组合归属 BuildProfileSystem，资源内容与清单归属 ResourceSystem，授权凭据归属 LicenseSystem 或安全存储，运行时状态归属对应 Platform System。
* FrameworkEntry 可以在未来读取 FrameworkConfig 并编排已完成生命周期设计的系统，但不得持有完整系统配置或运行时状态。
* 第一阶段现有字段不自动代表 Phase2 最终字段；具体 Socket 地址端口、UI Resources 路径、下载目录和 Guide 开关需要后续独立迁移审计。
* 详细设计见 `FrameworkConfigPhase2Design.md`。当前阶段不修改配置代码、资产或字段。

### Platform Service Registration

* Core 只定义业务无关的服务注册与生命周期契约，不静态引用 Platform 或 FeatureModule 类型。
* Platform、FeatureModule 和应用组合层负责显式注册具体服务、Factory、强类型配置和依赖声明。
* FrameworkEntry 只发现组合层提前提交的 ServiceDescriptor，不采用程序集反射扫描作为默认发现机制。
* Framework 级服务按显式依赖图进行确定性拓扑初始化，并按实际成功初始化顺序严格逆序销毁。
* Registry 不是 Service Locator、配置中心、事件中心或业务状态容器；禁止隐式创建未注册具体类型。
* Registry 与现有 Singleton 必须保持单一生命周期权威，并按模块渐进迁移。
* 对已注册 Service，Registry 是唯一生命周期权威；Singleton 只能作为迁移期兼容访问方式，不得创建、启动或销毁 Service。
* Service 生命周期为 Register、Initialize、Start、Stop、Shutdown；依赖图决定 Initialize 与 Start 顺序，手工优先级只用于无依赖同层稳定排序。
* Stop 严格逆实际 Start 顺序，Shutdown 严格逆实际创建或 Initialize 顺序；异常隔离并聚合，允许有界异步排空，禁止 Shutdown 完成后的后台释放。
* 详细设计见 `PlatformServiceRegistrationDesign.md`。当前阶段不实现 Registry 或接入任何 Platform Service。

### EventManager

* EventManager 是唯一事件入口，旧事件中心兼容层已移除。
* 支持 Struct、Class、Async、Type 与 Enum 事件。
* Runtime Listener 对外 API 使用 `AddListener`、`Broadcast`、`RemoveListener`。
* Type 事件使用 Type 本身作为 Key。
* Enum 事件使用枚举实例本身作为 Key，不使用 `eventId.GetType()`。
* 保留 `Publish`、`PublishAsync`、`PublishClass` API。
* P3.4C 已移除 EventManager 独立 `AfterSceneLoad` 自动初始化入口，`EnsureInstance()` 保留为兼容访问并进入同一套内部生命周期。
* EventManager Shutdown 边界覆盖运行时监听表、声明式事件表和 EventTypePool，避免 Domain Reload 关闭时状态残留。

### FSM

* FSM 是 Core 层通用状态机基础能力，不内置 Platform 状态模型或具体业务流程。
* FSMManager 当前通过 `Instance` 提供状态机创建、销毁和查询能力。
* P3.4D 已确认 FSMManager 适合进入 FrameworkEntry 编排，建议顺序为 ThreadDispatcher、EventManager、FSMManager。
* FSMManager 接管前必须明确 Stop 停止 Update 驱动，Shutdown 逆序销毁所有 StateMachine 并清理状态、条件和当前状态。
* FSMManager 不应反向依赖 ThreadDispatcher 或 EventManager；如业务状态需要事件通知，应由业务状态显式调用公开事件 API。

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
* 多显示器
* 自定义显示布局

目标显示比例与形态包括但不限于：

* 16:9
* 21:9
* 32:9
* 3:4
* 竖屏
* 自定义显示墙

DisplaySystem 的核心模型包括：

* DisplayProfile：默认、项目、业务模块、用户与机器校准覆盖的组合。
* DisplayMode：可组合的输出拓扑与 VR / NonVR 呈现形态。
* DisplayRegion：稳定的逻辑显示区域。
* DisplayLayout：区域、逻辑画布与输出目标的组合。
* DisplayAdapter：普通显示器、多显示器、融合、VR 与工业显示环境适配边界。
* DisplayContext：对外发布的当前只读显示状态。

DisplaySystem 描述显示环境，不管理 UI 主题、窗口、焦点、业务 Camera 或业务内容。UISystem 单向消费 DisplayContext。详细设计见 `DisplaySystemDesign.md`。

UI 系统未来应具备分辨率与显示形态适配能力。当前阶段不修改 UIRoot、CanvasScaler、UIManager 或 Camera 逻辑。

### UISystem

UISystem 是 Platform 层的通用 UI 能力，长期负责主题、焦点、导航、窗口、层级和显示适配，不包含具体业务界面。

#### UI Theme System

UI Theme System 长期支持统一切换：

* 颜色
* 字体
* 按钮样式
* 图标
* 背景

主题能力属于框架级能力，具体业务内容仍由 FeatureModule 提供。

#### UINavigationSystem

UINavigationSystem 长期支持：

* 纯键盘操作
* 键盘与鼠标
* 手柄
* 外部硬件按钮
* 控制面板按钮

开发阶段可以使用键盘和鼠标模拟最终硬件输入。

`UINavigationSystem` 是后续统一规划名称，替代早期文档中的 `UIInputNavigationSystem` 术语。它消费 InputSystem 提供的设备无关 UI InputAction，不直接读取具体设备。

#### UIFocusSystem

UI 必须支持由框架控制的焦点导航规则，包括：

* 上下移动
* 左右切换
* 标签切换
* 确认
* 取消

不应将 UGUI Navigation 作为未来核心方案。复杂界面、多屏、特殊布局和硬件按钮输入场景需要更明确、可控的焦点规则。

#### UIWindowSystem 与 UILayerSystem

UIWindowSystem 长期负责窗口打开、关闭、栈管理、返回逻辑与通用生命周期，不决定具体业务流程。

UILayerSystem 长期提供 Background、Normal、Popup、Overlay 与 Debug 层级，并分别描述展示顺序、输入阻断和窗口职责。

UISystem 单向消费 InputSystem、DisplaySystem、LocalizationSystem 与 ResourceSystem 的公开契约。这些系统不得反向依赖 UISystem。详细设计见 `UISystemDesign.md`。

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

输入来源可以是键盘、鼠标、手柄、VR 控制器、硬件按钮、工业控制面板或其它自定义设备，所有来源统一映射为框架输入指令。

InputSystem 的核心模型包括：

* InputAction：稳定的设备无关动作语义。
* InputBinding：Action 与具体设备输入之间的映射。
* InputProfile：默认、项目、业务模块与用户覆盖配置的组合。
* InputDevice：设备发现、能力描述与底层适配边界。
* InputContext：UI、Gameplay、Debug、Tool 等上下文的优先级与消费规则。

InputSystem 负责产生语义动作，不负责执行 UI 焦点规则或业务行为。UISystem 和 FeatureModule 消费 InputSystem，InputSystem 不依赖它们。

InputAction 使用稳定作用域标识，显示名称、翻译文本和当前 Binding 不参与动作身份。InputContext 必须具有明确所有者与生命周期，避免界面关闭、场景切换或模块停用后残留输入拦截。

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

BuildProfileSystem 是 Editor 优先的业务无关构建配置系统，负责在构建期组合、校验并解析不同平台、环境、项目、客户、模块与功能配置，长期支持：

* `BuildProfile`：当前构建定义与组合入口
* `BuildFeature`：稳定、业务无关的能力标识
* `BuildModule`：可扩展模块描述与依赖声明
* `BuildEnvironment`：Development、Test、Production 等非敏感环境标识
* `BuildVariant`：Standard、Professional、Enterprise 等通用构建变体
* `BuildManifest`：构建后生成的不可变、可追溯结果元数据

BuildProfileSystem 可以为 ResourceSystem 提供资源选择与裁剪输入，但不负责资源加载。Runtime 仅消费只读 BuildManifest 与最小构建元数据，不编辑或重新解析完整 BuildProfile。

BuildProfile 使用稳定、业务无关的标识；框架核心不硬编码具体客户、项目、行业、设备或车型。BuildVariant 决定构建中包含什么，不替代 LicenseSystem 的运行时授权。完整构建配置不进入 FrameworkConfig，FrameworkConfig 最多保存当前构建产品的只读 Profile 标识或 Manifest 引用。

### LicenseSystem

LicenseSystem 是 Platform Operations 层的 Runtime 授权与激活能力，长期支持：

* `LicenseIdentity`：设备、授权主体与项目的稳定身份描述
* `LicenseKey`：授权凭据与激活凭据，不等同于安全存储
* `LicenseFeature`：稳定的 Runtime 授权能力标识
* `LicensePolicy`：到期、离线、Feature 与 BuildVariant 授权规则
* `LicenseProvider`：Offline、Online 与 Hybrid 授权来源
* `LicenseContext`：当前授权来源、校验结果与 Feature 授权快照
* `LicenseState`：Unlicensed、Trial、Activated、Expired 与 Invalid 总体状态

BuildProfileSystem 决定构建包含什么，LicenseSystem 决定 Runtime 允许使用什么。OnlineProvider 可以可选消费 NetworkSystem，但完全离线模式不得依赖 NetworkSystem。

SaveSystem 只能按 LicenseSystem 规则缓存允许保存的状态与验证元数据，缓存不等于授权真相。FrameworkConfig 不保存 LicenseKey、机器码结果、LicenseState 或其它授权数据，最多保存是否启用检查、默认 Provider 与 Policy 的轻量标识。

### ResourceSystem

ResourceSystem 是 Platform Foundation 层的统一资源访问能力，长期支持：

* Resources
* AssetBundle 扩展点
* 自定义 Provider
* 模块资源扩展
* 资源隔离
* 多语言资源
* Theme 资源
* UI 资源
* 配置资源

ResourceSystem 的核心模型包括：

* ResourceKey：稳定、与路径和文件名解耦、支持模块作用域的资源身份。
* ResourceLocation：Provider 路由后的内部定位描述。
* ResourceProvider：Resources、AssetBundle、AssetDatabase Editor 与 Custom 后端适配边界。
* ResourceCatalog：Key 到 Location 映射、Provider 路由和模块资源注册索引。
* ResourceContext：当前 Provider、Catalog 和资源环境的只读状态。
* ResourceHandle：加载结果、拥有关系、状态和释放请求边界。

ResourceCatalog 不依赖具体 FeatureModule；模块只注册独立资源描述或 Catalog Fragment。FrameworkConfig 不保存完整资源清单、Catalog 或 Manifest。

当前 Runtime 继续使用 Resources，Editor 工具继续使用 AssetDatabase。AssetDatabase 仅允许存在于 Editor 边界。现阶段不开始 ResourceSystem 重构，不引入 Addressables，也不实现 AssetBundle。

P3.2 Runtime 契约确认：

* `IResourceService` 是 Load、Release、Query 与 Exists 的统一访问入口。
* Query 与 Exists 只表示当前 Catalog 可解析，不保证实际 Load 成功。
* Async 是通用默认能力；Sync 只允许 Provider 明确支持，禁止阻塞等待异步操作伪造同步。
* Catalog Fragment 变更完整验证后原子发布新 Snapshot，移除 Fragment 不使既有 Handle 立即失效。
* 每次成功 Load 返回独立消费者 Handle；Handle Release 不保证 Unity 底层资源立即卸载。
* ResourcesProvider 是当前默认实现方向，AssetBundleProvider 与 CustomProvider 保持扩展边界，AssetDatabaseProvider 禁止进入 Runtime。
* `IResourceService` 通过 Platform Service Registration 显式注册；Provider 与 Catalog 默认是 Resource Service 内部组合扩展点。

总体设计见 `ResourceSystemDesign.md`，冻结的 Runtime 契约见 `ResourceSystemContractDesign.md`。

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

LocalizationSystem 的核心模型包括：

* Language：稳定 LanguageCode、显示名称、母语名称和文本方向等语言元数据。
* LanguagePack：Core、Platform 与 FeatureModule 独立维护的语言包。
* LocalizationKey：稳定、与显示文本分离、支持模块作用域的查询标识。
* LocalizationEntry：特定语言下的文本模板与格式信息。
* LocalizationProvider：查询、切换、语言包加载与缺失文本诊断边界。
* LocalizationContext：当前语言、默认语言、回退链与运行时切换状态的只读快照。

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

FrameworkConfig 只允许保存默认语言标识，不保存翻译文本或语言包。LicenseSystem 与 BuildProfileSystem 只能在展示或工具边界消费本地化文本，核心逻辑继续使用稳定状态码、错误码和 LocalizationKey。

详细设计见 `LocalizationSystemDesign.md`。当前阶段不实现多语言系统，也不修改现有文本、UI、License 或 BuildProfile 代码。

### 其它 Platform 系统

* NetworkSystem：提供行业无关的 HTTP、TCP Socket、WebSocket 传输、通用协议封装、序列化与连接生命周期能力，通过 NetworkProfile、NetworkEndpoint、NetworkProvider、NetworkContext、NetworkSession、NetworkState 与 NetworkPolicy 统一环境、端点、会话和策略边界，并为 UDP、KCP、本地环回和高性能通信预留扩展。

### SaveSystem

SaveSystem 是 Platform Foundation 层的统一持久化能力，长期支持用户配置、用户偏好、输入配置、显示配置、本地缓存、授权状态边界、机器校准与 FeatureModule 独立存储。

SaveSystem 的核心模型包括：

* SaveScope：OwnerScope 与 SubjectScope 组合的数据隔离边界。
* SaveKey：稳定、可版本化、支持作用域的逻辑数据标识。
* SaveEntry：载荷、Schema 版本、完整性与迁移元数据封装。
* SaveProfile：Default、Project 与 User 存储策略组合。
* SaveProvider：Json、Binary、Encrypted 等可替换存储边界。
* SaveContext：当前用户、Profile、路径、Provider 与存储状态的只读快照。

FrameworkConfig 只保存默认 SaveProfile、Provider 与轻量启动策略，不保存用户数据或运行时状态。SaveSystem 不保存资源内容，不负责网络同步，也不理解消费系统的业务数据语义。

NetworkSystem 不理解业务协议或业务消息。LicenseSystem 可以可选消费 NetworkSystem，FeatureModule 可以在其上定义自己的协议层；完全离线 Runtime 不得强制依赖 NetworkSystem。FrameworkConfig 仅保存默认 NetworkProfile、Provider 与 Policy 的轻量标识，不保存完整端点清单、凭据、会话或连接状态。

NetworkSystem 可以提供 MessageId、MessageEnvelope、MessageCodec、IMessageSerializer 与 SerializerProvider 的通用机制，但具体 proto、业务消息和协议语义归属 FeatureModule。车辆、关节、碰撞、物理状态同步以及 Command、State、Snapshot、Delta、Event Sync 等能力属于 FeatureModule/SimulationSync；FeatureModule/SimulationServer 负责 Headless 或 Dedicated Server 的业务同步，NetworkSystem 只提供通信管道。

FeatureModule 可以拥有独立 SaveScope。EncryptedProvider 不自动等同于安全密钥存储；授权密钥与高敏感令牌需要 LicenseSystem 和平台安全存储专项边界。

P3.3 Runtime 契约确认：

* `ISaveService` 是 Load、Save、Delete、Exists 与 Enumerate 的统一存储入口。
* Core、Platform、Module 属于 OwnerScope，User 属于 SubjectScope；Project、Machine、Session 保留为必要主体隔离维度。
* SaveKey 保持稳定，SchemaVersion 位于 SaveEntry 元数据；数据所有者负责业务迁移语义。
* SaveProfile 是不可变策略 Snapshot，进行中 Transaction 固定使用开始时的 Profile 与 Provider 路由。
* SaveTransaction 负责可靠写入、Commit、Rollback 与 Recovery；跨 Provider 操作不得宣称原子。
* SaveMigration 写回必须通过可靠事务，失败不得覆盖最后一个可恢复版本。
* JsonProvider、BinaryProvider 与 EncryptedProvider 遵守同一可靠写入与恢复边界；EncryptedProvider 不等同于密钥管理系统。
* `ISaveService` 通过 Platform Service Registration 显式注册；Provider、Transaction 和 Migration Registry 默认是 Save Service 内部组合扩展点。

总体设计见 `SaveSystemDesign.md`，冻结的 Runtime 契约见 `SaveSystemContractDesign.md`。当前阶段不实现 SaveSystem、Provider 或数据迁移，也不修改现有存储代码。

以上系统均处于长期规划阶段，尚未形成最终 API。

Platform 系统必须独立设计与实施。当前不实现任何 Platform 系统，不开始 ResourceSystem 重构，不引入 Addressables，也不由 FrameworkEntry 接管尚未完成生命周期设计的 Platform 模块。

# ByFramework Todo

## 当前阶段

* P2 设计阶段已完成，`Documentation/ByFramework_Current_Context.md` 已更新为进入 P3 前的统一权威快照
* 后续新窗口必须先阅读当前上下文快照，不依赖历史会话
* P3.1 Platform Service Registration Design 已完成，仅完成设计，未修改 Runtime、Editor 或 API
* P3.2 ResourceSystem Contract Design 已完成，仅冻结 Runtime 契约，未修改 Runtime、Editor 或 API
* P3.3 SaveSystem Contract Design 已完成，仅冻结 Runtime 契约，未修改 Runtime、Editor 或 API
* P3.4 FrameworkEntry Phase2A Design Review 已完成，仅完成最终设计冻结，未修改 Runtime、Editor 或 API
* P3.4A FrameworkEntry Phase2A：ThreadDispatcher 窄范围实现已完成代码接管，待 Unity 专项验证
* P3.4B EventManager Integration Review 已完成，仅完成文档评审，未修改 Runtime、Editor 或 API
* P3.4C EventManager Narrow Implementation 已完成代码接管，待 Unity 专项验证
* P3.4D FSMManager Integration Review 已完成，仅完成文档评审，未修改 Runtime、Editor 或 API
* P3.4E FSMManager Narrow Implementation 已完成代码接管，待 Unity 专项验证
* P3.4F FrameworkEntry Phase2A Closure Review 已完成，仅更新文档；Phase2A 允许正式关闭
* P3.5A Core Early Lifecycle Unity Verification 已由项目维护者完成本地 Unity 实机验证，结果通过
* P3.5B Platform Service Registration Runtime API Freeze 已完成，当前已冻结 Runtime API 与注册贡献入口设计
* P3.6 InputSystem Foundation 已完成 Foundation 冻结，当前未进入 Runtime Implementation
* 下一推荐任务为 P3.7 UISystem Foundation
* P3 推荐顺序：Platform Service Registration、ResourceSystem Contract、SaveSystem Contract、FrameworkEntry Phase2A、Platform Service Registration Runtime API Freeze、InputSystem Foundation、UISystem Foundation

## 当前已知技术债

* EventManager 模块公开命名空间仍保留 _3rdBy.MetaFramework 兼容命名，最终收敛到 ByFramework 命名空间需要独立迁移方案
* 命名空间仍保留 _3rdBy.MetaFramework 兼容命名，后续需要单独做命名空间迁移
* CodingStandard 需要细化纯 C# 逻辑类字段注释规则，避免仅为 HeaderAttribute 引入 UnityEngine 依赖
* UIAutoCreate 的业务输出目录已在 FrameworkEditorConfig 中建立配置项，但现有工具尚未迁移读取逻辑
* ByFramework 当前没有顶层 Runtime 与 Samples 目录，模块级 Samples 路径需要由对应模块自行管理
* UIManager 依赖 Resources
* MonoSingletonTemplate 的重复实例会由基类销毁；未显式检查 `Instance != this` 的派生 Awake 仍可能在销毁帧继续执行后续逻辑，需要逐个模块验证
* BaseNetModel 的派生 Awake 会在检测到重复实例时抛出异常；该策略受 MonoSingletonTemplate 重复实例保护影响，需要在 Socket 模块专项复审
* MonoObjSingletonTemplate 会优先复用场景中的同类型组件，但无法主动销毁场景中预放置的多个实例；无场景实例时仍会隐式创建 GameObject，也没有统一父节点规范
* SingletonTemplate 缺少统一销毁、重置和生命周期管理
* SingletonTemplate<T> 与 EventTypePool.Instance 等纯 C# 静态实例在关闭 Domain Reload 时可能跨 Play Session 保留状态
* 部分单例依赖场景引用，不能直接挂到 `[ByFramework]`，详见 SingletonAudit.md
* 泛型 MonoSingletonTemplate 与 MonoObjSingletonTemplate 在关闭 Domain Reload 时的静态状态重置需要 Unity 专项验证
* Guide Dispatcher 缺少显式重建、静态清理和退出生命周期策略
* GuideManager 当前销毁后出现的重复场景实例；如果未来需要按场景替换 GuideManager，需单独设计交接策略
* 缺少统一日志系统
* FrameworkEntry Phase2 已完成设计并完成 ThreadDispatcher 窄范围代码接管；后续模块仍必须逐个独立设计、实施与验证
* P3.4 审查确认 Phase2A 只允许接管 ThreadDispatcher，不实现 Registry、不接入 Platform Service，也不顺带接管 EventManager 或 FSMManager
* Phase2A 实施必须在同一批改动中消除 ThreadDispatcher 原有自动初始化与 FrameworkEntry 接管并存路径
* P3.4A 代码审查确认 ThreadDispatcher 改动前实际使用 AfterSceneLoad，与早期文档中的 BeforeSceneLoad 描述不一致；本次保持实际时点以保护首场景预放置实例复用
* P3.4A 已移除 ThreadDispatcher 原有 AfterSceneLoad 自动初始化入口，由 FrameworkEntry 作为唯一生命周期编排权威
* P3.4A 保留 DispatcherThread.Current、QueueOnMainThread 与 RunAsync 公开 API；FrameworkEntry 不直接创建 DispatcherThread 实例
* P3.4A Unity 待验证项包括 Domain Reload 开关、重复进入退出、场景切换、场景预放置实例复用、队列执行和 Shutdown
* P3.4B 已确认 EventManager 当前存在 AfterSceneLoad 自动初始化入口和 EnsureInstance 兼容创建入口；P3.4C 必须消除双重生命周期权威
* P3.4C 已移除 EventManager 原有 AfterSceneLoad 独立初始化入口，由 FrameworkEntry 在 ThreadDispatcher 之后编排 EventManager
* P3.4C 已保留 EventManager.Instance、EnsureInstance、Publish、PublishAsync、PublishClass、AddListener、Broadcast、RemoveListener 公开 API
* P3.4C 已将 EnsureInstance 调整为兼容路径，进入同一套 Register、Initialize、Start 内部生命周期，不再维护独立创建逻辑
* P3.4C 已在 Shutdown 中清理 `_runtimeEventTable`、`_allEvents`、`_allEventTypes` 与 `EventTypePool.Instance`
* P3.4C 待验证项包括 Domain Reload 开关、多次进入退出 Play Mode、场景切换、预放置 EventManager、运行时访问 EventManager.Instance 和 Samples 行为
* P3.4D 已确认 FSMManager 当前存在 AfterSceneLoad 自动初始化入口；P3.4E 必须消除双重生命周期权威
* P3.4D 已确认 FSMManager Shutdown 边界必须覆盖 `_machines`、StateMachine states、conditions、CurrentState 和状态委托引用
* P3.4D 已确认 FSMManager 适合在 ThreadDispatcher 与 EventManager 之后由 FrameworkEntry 编排
* P3.4D 已确认 FSMManager Stop 阶段必须停止 Update 驱动，避免 Shutdown 期间状态继续 OnStay 或条件切换
* P3.4E 已移除 FSMManager 旧 AfterSceneLoad 自动初始化入口，改由 FrameworkEntry 统一编排 Register、Initialize、Start、Stop、Shutdown
* P3.4E 已保留 FSMManager.Instance 与状态机创建、销毁、查询公开 API，不新增 EnsureInstance 或 Service Registry
* P3.4E 已补齐 FSMManager Shutdown 对 `_machines` 的逆序销毁，以及 StateMachine 对 CurrentState、conditions、状态委托和反向引用的清理
* P3.4E 待 Unity 验证项包括 Domain Reload 开关、多次进入退出 Play Mode、场景切换、预放置 FSMManager 与运行时访问 FSMManager.Instance
* P3.4F 已确认 Phase2A 的 Core Early 生命周期链冻结为 ThreadDispatcher -> EventManager -> FSMManager
* P3.4F 已确认 Shutdown 严格逆序冻结为 FSMManager -> EventManager -> ThreadDispatcher
* P3.4F 已确认 Singleton / Instance 仅作为兼容访问方式，生命周期权威收敛到 FrameworkEntry
* P3.4F 已确认双权威风险在三个试点模块的代码路径上已消除，但 EventManager.EnsureInstance 兼容路径仍需审计
* P3.4F 已确认 Phase2A 关闭不等于 Unity 实机验收完成，也不等于 Platform Service Registration 已实现
* P3.5A 静态检查确认 ThreadDispatcher、EventManager 与 FSMManager 不再保留独立 AfterSceneLoad Init 自动入口
* P3.5A 静态检查确认 FrameworkEntry 生命周期顺序仍为 ThreadDispatcher -> EventManager -> FSMManager，Shutdown 仍为 FSMManager -> EventManager -> ThreadDispatcher
* P3.5A 静态扫描未在 ByFramework 自身场景、Prefab 或 Asset 中发现预放置 Core Early 实例
* P3.5A 早期批处理验证曾受 Unity Editor License 影响，但该阻塞已由项目维护者本地实机验证关闭
* P3.5A 已确认 FrameworkEntry、ThreadDispatcher、EventManager、FSMManager 单实例状态分别为 1
* P3.5A 已确认静态访问正常、重复实例检查正常、Core 生命周期链验证通过
* P3.5A 新增的 Editor-only 验证窗口 `ByFramework/验证/Core Early 生命周期验证` 继续保留，用于后续回归验证
* ByFramework 后续所有 Editor 工具默认中文优先，MenuItem、EditorWindow 标题、Button、Label、HelpBox、Validation Report 与 Console Log 均应面向国内开发、实施和测试人员
* ByFramework 后续所有新增日志默认中文优先，Runtime 日志采用“中文说明 + 英文对象名”，技术对象名如 EventManager、FSMManager、ThreadDispatcher、ResourceKey、Protobuf、TCP、UDP 保留英文
* ByFramework 中文化规范整改已完成第一轮安全扫描与文案整改，覆盖 FrameworkEntry、ThreadDispatcher、EventManager、FSMManager、Core Early 验证窗口、UI 自动生成器、网络/下载日志、Socket/Guide/RedPoint/UILineRenderer 明显英文输出
* 中文化暂缓范围包括 Protobuf 生成代码、协议字段、资源路径、配置 Key、EventKey、ResourceKey、API 名称、类名、方法名、字段名、命名空间、文件名和仅用于调试的原始数值/路径直出
* P3.5A 当前状态已关闭，P3.5B 已完成，后续不再保留 Manual Verification Required / Not allowed yet 状态
* FrameworkEntry 接管单个模块时，必须同步消除该模块原有自动初始化与入口初始化并存造成的双重初始化风险
* ThreadDispatcher、EventManager 与 FSMManager 当前初始化时机不同，迁移时需要分别验证 BeforeSceneLoad 与 AfterSceneLoad 行为兼容性
* Guide Dispatcher 在接入 FrameworkEntry 前缺少显式初始化、宿主重建、静态清理和退出生命周期接口
* SoundManager 在接入 FrameworkEntry 前需要明确场景实例优先级、音频子节点、Resources 与退出清理边界
* UIRoot、UIManager、ClientManager、DownloadManager 与 GuideManager 当前存在场景、资源、连接或任务生命周期依赖，暂不接入 FrameworkEntry
* FrameworkConfig 第一阶段尚未接入 Guide、UI、Socket 与下载模块，现有模块仍使用原配置来源
* 当前目录结构尚未完全按 Core、Platform、FeatureModule 分层，后续迁移必须分模块实施并保持 API 稳定
* DisplaySystem Design 已完成总体架构，但 DisplayMode、DisplayProfile、DisplayRegion、DisplayLayout、DisplayAdapter 与 DisplayContext 的最终契约和 API 尚未详细设计或实现
* DisplayMode 必须使用可组合模型表达输出拓扑与 VR / NonVR 呈现形态，不能退化为互斥单枚举
* DisplayProfile 需要继续定义 Framework Default、Project、FeatureModule、User Override 与 Machine Calibration Override 的合并、冲突和迁移规则
* DisplayRegion 与 DisplayLayout 需要继续定义稳定标识、空间模型、缺失区域回退、多窗口和多显示目标规则
* DisplayAdapter 需要隔离普通显示器、多显示器、融屏、VR 与自定义工业显示设备差异
* DisplayContext 需要继续定义期望 Profile 与实际生效状态的区分、只读快照和变化通知契约
* 当前 UIRoot、CanvasScaler、UIManager、Camera 与显示逻辑属于未来独立迁移审计范围，禁止一次性显示或 UI 重构
* UISystem Design 已完成总体架构，但 UIThemeSystem、UIFocusSystem、UINavigationSystem、UIWindowSystem 与 UILayerSystem 的最终契约和 API 尚未详细设计或实现
* UINavigationSystem 已统一替代早期规划术语 UIInputNavigationSystem；当前不应将 UGUI Navigation 固化为未来核心方案
* UIFocusSystem 需要继续定义焦点稳定标识、焦点恢复、焦点锁所有权与失效对象安全回退规则
* UINavigationSystem 需要继续定义导航图、确定性目标选择、列表与 Tab 导航及跨焦点组规则
* UIWindowSystem 需要继续定义窗口生命周期、返回请求、模态策略、栈行为与资源释放边界
* UILayerSystem 需要继续定义 Background、Normal、Popup、Overlay、Debug 的展示顺序、输入阻断和多显示目标规则
* UIThemeSystem 需要继续定义 Theme Token、资源覆盖、语言字体回退与运行时刷新契约
* 当前 UIRoot、UIManager、UIAutoCreate、UGUI Navigation、Resources 加载和业务窗口属于未来独立迁移审计范围，禁止一次性 UI 重构
* P3.6 已冻结 InputAction 标识格式、别名迁移、InputValueKind、InputStage 与后续 Runtime API 设计边界
* P3.6 已冻结 InputContext 的所有权、生命周期、优先级、独占、透传、消费、冲突确定性与恢复规则
* P3.6 已冻结 InputProfile 的 Framework Default、Project、FeatureModule、User Override 分层与合并策略
* P3.6 已冻结 InputDevice / InputAdapter Foundation 边界，覆盖键盘、鼠标、手柄、VR 控制器、硬件按钮、工业控制面板与自定义外部设备
* IndustrialControlPanel 需要以复合设备能力建模，不能仅退化为无语义 HardwareButton 通道集合
* 框架不得定义具体业务键位；VehicleSimulationInputProfile 等业务输入配置必须归属对应 FeatureModule
* 当前 Guide、Extension、UI 与 Samples 中仍存在 `Input.GetAxis`、`Input.GetMouseButton`、`Input.GetKeyDown`、`KeyCode` 和 `StandaloneInputModule` 等直接输入依赖，后续需要独立迁移审计
* 用户 InputBinding、设备偏好和校准数据需要通过 SaveSystem 差异化持久化，不应写入 FrameworkConfig
* InputSystem、LocalizationSystem 与 UISystem 已明确 Binding Display Token 组合边界，但 Token 命名、查询与动态输入提示刷新契约仍需详细设计
* UISystem 与 DisplaySystem 已明确通过只读 DisplayContext 单向连接，但逻辑视口、安全区域、多屏目标和显示适配细节仍需独立契约设计
* UISystem 与 ResourceSystem 之间尚未建立窗口、Theme、字体和图标资源的加载、缓存、失败回退与释放契约
* LocalizationSystem Design 已完成总体架构，但 Language、LanguagePack、LocalizationKey、LocalizationEntry、LocalizationProvider 与 LocalizationContext 的最终契约和 API 尚未详细设计或实现
* LanguageCode 必须使用稳定标准化标识，不能使用显示名称、列表索引或枚举序号作为持久化身份
* LocalizationKey 必须与显示文本分离并具备模块作用域；Key 别名、迁移、重复检测和命名验证规则仍需详细设计
* LanguagePack 已明确按 Core、Platform 与 FeatureModule 所有权独立维护，但 Schema、依赖、覆盖、版本和卸载规则仍需详细设计
* LocalizationEntry 的参数、复数、语境变体、RTL 与格式化规则仍需详细设计
* LocalizationProvider 与 LocalizationContext 需要继续定义缓存、缺失诊断、回退链、原子切换和失败恢复规则
* UISystem 需要继续定义语言变化后的动态文本刷新、RTL、字体回退和布局适配契约
* LicenseSystem 与 BuildProfileSystem 核心逻辑必须使用稳定状态码、错误码或 LocalizationKey，只能在展示或工具边界解析翻译文本
* 当前 UI、日志、配置、帮助文本与业务代码中的硬编码显示文本属于未来独立迁移审计范围
* DisplaySystem、UISystem、InputSystem、BuildProfileSystem、LicenseSystem、ResourceSystem、LocalizationSystem、NetworkSystem 与 SaveSystem 尚处于架构规划阶段
* 当前业务扩展模块与通用框架能力之间缺少明确的 FeatureModule 边界规范
* P3.2 已冻结 ResourceSystem Runtime Contract；最终 C# 数据结构、方法签名与实现仍未设计或实现
* P3.2 已确定 IResourceService 是 Load、Release、Query 与 Exists 的统一入口；Query 与 Exists 不保证实际 Load 成功
* P3.2 已确定 ResourceKey 必须稳定、与路径、文件名、Bundle 名、Provider 和 Unity GUID 分离，并支持模块作用域、类型验证、别名和迁移
* P3.2 已确定 ResourceCatalog 使用不可变版本化 Snapshot；Fragment 变更完整验证后原子发布，冲突禁止静默覆盖
* P3.2 已确定模块 Fragment 移除只影响新请求，既有 Handle 继续有效直到消费者释放
* ResourcesProvider 只用于长期兼容当前 Resources；当前不重构现有 Resources 调用
* AssetDatabaseProvider 只能存在于 Editor 工具和 Catalog 构建边界，禁止进入 Runtime 契约或构建产物
* AssetBundleProvider 与 CustomProvider 仅作为长期扩展点，当前不实现 AssetBundle，也不引入 Addressables
* P3.2 已确定 Async 是通用默认能力；Sync 只允许 Provider 明确支持，禁止阻塞等待异步操作伪造同步
* P3.2 已确定每个成功消费者获得独立 ResourceHandle，底层资源与加载操作可以共享
* P3.2 已确定 Handle Release 表示消费者放弃租约，不保证 Unity 底层资源立即卸载
* ResourceContext 需要继续定义当前 Provider、Catalog、资源环境、降级状态和只读快照规则
* 继续设计 ResourceSystem 最终 C# 数据结构、错误码、方法签名、取消、进度、线程和主线程调度 API
* 继续设计 ResourcesProvider 的真实加载、共享、释放与 Unity 对象限制
* 继续设计 Catalog Fragment Schema、Snapshot 构建、原子发布、Handle 泄漏诊断和 Service 停止策略
* 继续设计 Localization LanguagePack、UISystem Theme 与 Window 的具体 Handle 交接规则
* IResourceService 通过 P3.1 Service Registration 显式注册；Provider 与 Catalog 默认保持 Resource Service 内部组合边界
* 当前 FrameworkConfigProvider、UIManager、ResourceManager、SoundManager 的 Resources 调用，以及 UIAutoCreate、ByFrameworkPathUtility 的 AssetDatabase 调用属于未来独立迁移审计范围
* Platform 长期规划尚未进入实现阶段，后续启动任何系统前需要单独设计、确认范围并更新 Roadmap
* Platform Architecture Design 已完成，但九个 Platform 系统均未开始实现；后续必须按独立设计任务逐项确认职责与 API
* Platform 内部需要建立单向依赖契约，避免 UISystem、ResourceSystem、LocalizationSystem 与 InputSystem 形成循环依赖
* FrameworkConfig 后续只能增加轻量启动配置、开关与 Profile 标识，不应承载语言包、资源清单、用户绑定、授权状态或构建过程数据
* FrameworkConfig 对 LocalizationSystem 仅允许保存默认语言标识及可选轻量 Provider 标识，不得保存翻译文本、语言包、当前用户语言或运行时 LocalizationContext
* P2.11 已完成 FrameworkConfig Phase2 最终职责边界设计；FrameworkConfig 固定为“框架启动配置与默认配置入口”，禁止作为万能配置中心
* FrameworkConfig 字段必须通过启动必需、业务无关、稳定默认、非用户机器会话数据、非运行时状态、非敏感内容和明确所有者准入测试
* FrameworkConfig 只保存默认 Profile、Provider、Policy 标识、独立配置引用与必要启动开关，不保存完整配置内容
* FrameworkConfig 与 SaveSystem 必须分离：前者提供构建产物只读默认值，后者保存用户、机器、模块与运行实例数据
* FrameworkConfig 与 BuildProfileSystem 必须分离：前者提供 Runtime 默认入口，后者负责构建组合、裁剪和预设选择
* FrameworkConfig 与 ResourceSystem 必须分离：前者选择默认 Provider 或环境，后者管理 Catalog、Manifest、资源清单和资源内容
* FrameworkConfig 与 LicenseSystem 必须分离：前者选择默认 Provider 或 Policy，后者管理凭据、激活、状态、缓存和安全边界
* 审计当前 `enableGuide`、UI Resources 路径、Socket 地址端口与下载目录字段的最终所有者和迁移方向
* 设计 FrameworkConfig Schema 版本、字段校验、缺失、兼容与迁移策略
* 设计 BuildProfileSystem 选择 FrameworkConfig 预设，以及 FrameworkEntry 消费默认入口的生命周期契约
* P2.12 Platform Integration Review 已完成；总体依赖方向通过，未发现必须存在的 Runtime 循环依赖
* P3.1 已确定 FrameworkConfig 源码禁止直接引用 Platform 强类型 Profile、Provider、Policy、Context 或配置字段；Platform 强类型配置由 Platform 或应用组合层拥有和解析
* P3.1 已确定 FrameworkConfig 不保存 ServiceDescriptor、Factory 或服务注册列表，只能保留经过准入审查的稳定标识或 Core 中立引用
* P3.1 已确定 FrameworkEntry 只编排 Core 可见 IServiceRegistry，不扫描、不引用或直接构造 Platform 类型
* 高风险：BuildProfileSystem Editor 逻辑禁止进入 Runtime，Runtime ResourceSystem 禁止依赖 BuildProfile Editor
* 高风险：NetworkSystem 禁止吸收 SimulationSync、业务协议处理或 SimulationServer 权威逻辑
* P3.1 已完成 Platform 服务注册、接口发现、生命周期、错误模型与 Provider 替换设计
* P3.1 已确定 IService、IServiceRegistry、ServiceDescriptor、ServiceLifetime 与 ServiceContext 的中立契约归属 Core
* Platform、FeatureModule 与应用组合层必须显式提交 ServiceDescriptor、Factory 和依赖声明；默认禁止程序集反射扫描和未注册具体类型自动创建
* Framework 级服务必须按显式依赖图确定性初始化，并按实际成功初始化顺序严格逆序销毁
* P3.4 已冻结 Platform Service 生命周期为 Register、Initialize、Start、Stop、Shutdown
* P3.4 已确定 Required Dependency 图完全决定 Initialize 与 Start 顺序；StartupOrder 只用于无依赖同层稳定排序
* P3.4 已确定 Stop 严格逆实际 Start 顺序，Shutdown 严格逆实际创建或 Initialize 顺序
* P3.4 已确定允许有界异步排空与释放，禁止 Shutdown 完成后的后台工作；Stop 与 Shutdown 异常必须隔离并聚合
* P3.4 已确定 Registry 是已注册 Service 的唯一生命周期权威；Singleton 只能作为迁移期兼容访问方式
* Service Registry 不是 Service Locator、FrameworkConfig、EventManager 或业务状态容器
* Service Registry 与现有 Singleton 必须保持单一生命周期权威；现有自动初始化模块接入前必须消除双重初始化路径
* 已完成 Platform Service Registration 最终 API、异步、取消、结果类型、Runtime 注册贡献入口、应用组合层和 Scoped Registry 冻结设计
* 实现 Core、Platform、FeatureModule 依赖方向架构守卫和跨系统契约测试模板
* 实现前审计 FrameworkConfig 当前字段与 Platform 强类型引用风险
* ResourceSystem Runtime 契约已在 P3.2 冻结，SaveSystem Runtime 契约已在 P3.3 冻结
* UISystem 应在 Input、Display、Localization 与 Resource 契约稳定后实施
* SimulationSync、SimulationServer、VehicleSimulation 必须保持 FeatureModule 归属
* ResourceSystem 已完成总体设计与 P3.2 Runtime Contract Design；当前继续保留 Resources 与 AssetDatabase，不开始 Runtime 重构、AssetBundle 实现或 Addressables 接入
* P3.3 已冻结 SaveSystem Runtime Contract；最终 C# 数据结构、方法签名与实现仍未设计或实现
* P3.3 已确定 Core、Platform、Module 属于 OwnerScope，User 属于 SubjectScope，并保留 Project、Machine、Session 主体隔离维度
* P3.3 已确定 SaveKey 必须稳定、与文件路径、文件名、类名和 Provider 分离；SchemaVersion 位于 SaveEntry 元数据
* P3.3 已确定 SaveEntry 是不可变、可校验、可版本化载荷封装；SaveProfile 是不可变策略 Snapshot
* P3.3 已确定 JsonProvider、BinaryProvider 与 EncryptedProvider 遵守同一可靠写入、版本、恢复和错误边界
* EncryptedProvider 不自动等同于安全密钥存储；LicenseSystem 敏感数据需要平台安全存储、密钥轮换与完整性验证专项设计
* P3.3 已确定 SaveTransaction 负责可靠写入、Commit、Rollback 与 Recovery；原子性只在单一 Provider 明确能力边界内保证
* P3.3 已确定跨 Provider 操作不得宣称原子，未来如需要必须采用独立补偿事务设计
* P3.3 已确定 SaveMigration 由数据所有者定义转换语义，SaveSystem 管理迁移链、事务、备份和恢复
* P3.3 已确定迁移失败不得覆盖最后一个可恢复版本，SaveSystem 必须支持备份、恢复和损坏隔离边界
* P3.3 已确定 Exists 不保证 Entry 完整、兼容或业务有效；Enumerate 默认只返回授权 Scope 内元数据
* SaveContext 仍需继续定义用户切换、Profile 切换、Provider 降级、只读状态和原子替换最终 API
* SaveSystem 不保存资源内容，不负责网络或云同步，不理解 Input、Display、Localization、License 和 FeatureModule 数据语义
* ResourceSystem 与 SaveSystem 默认互不依赖；SaveSystem 不保存 ResourceCatalog、资源文件或 ResourceHandle，未来下载缓存需要独立契约
* FeatureModule 拥有独立 Module SaveScope；跨模块访问默认禁止，模块负责自己的 SaveKey、Schema 与迁移语义
* ISaveService 通过 P3.1 Service Registration 显式注册；Provider、Transaction 和 Migration Registry 默认保持 Save Service 内部组合边界
* 继续设计 SaveSystem 最终 C# 数据结构、错误码、方法签名、同步异步、取消、线程和并发 API
* 继续设计 SaveProfile Schema、Provider 路由、路径、配额、原子切换和 SaveContext 最终 API
* 继续设计 Transaction 日志、崩溃恢复、并发冲突、Migration Registry、迁移链验证和幂等实现
* 继续设计 JsonProvider 单 Entry 可靠事务、备份、恢复和损坏隔离最小实现
* 当前 JSaver、PlayerPrefs、文件读写、配置资源与模块专属持久化代码属于未来独立迁移审计范围
* P3.4 实施准备度审查确认：ResourceSystem 与 SaveSystem 只可进入最终 API / 实现设计，尚不可直接进入 Runtime 实现
* P3.4 实施准备度审查确认：InputSystem、LocalizationSystem、DisplaySystem、NetworkSystem 与 LicenseSystem 尚未准备进入 Runtime 实现
* P3.6 InputSystem Foundation 已完成：前置条件已满足且 Foundation 契约已冻结；后续应进入 P3.7 UISystem Foundation，而不是直接进入 InputSystem Implementation
* 多联屏现场校准、用户 InputBinding、授权状态等机器或用户特定数据需要独立持久化方案，不应写入 FrameworkConfig
* 多联屏、融屏、投影和工业显示现场校准必须作为机器覆盖持久化，不能写入 FrameworkConfig 或通用 Project DisplayProfile
* SaveSystem 必须为用户 InputBinding、用户语言偏好、显示偏好与机器校准提供不同 SubjectScope，禁止互相覆盖
* BuildProfileSystem 必须保持构建期编排与 Runtime 配置分离，Runtime 只消费只读 Profile 元数据
* BuildProfileSystem Editor 可以消费 Resource Catalog 描述并生成裁剪结果，但 Runtime ResourceSystem 禁止依赖 BuildProfileSystem Editor 实现
* P2.8 已完成 BuildProfileSystem 总体架构设计；构建工具、模块裁剪、资源裁剪与 BuildManifest 生成仍未实现
* BuildProfile 必须使用稳定、业务无关的标识；框架核心禁止硬编码具体客户、项目、车辆类型或行业
* BuildFeature 是稳定能力标识，不直接等同于脚本宏、业务开关或 Runtime 授权
* FeatureModule 可以声明 BuildModule，但 BuildProfileSystem 禁止依赖具体业务模块类型
* BuildEnvironment 只保存非敏感环境标识与配置引用，禁止保存密钥、令牌与证书私钥
* BuildVariant 只描述构建组合，不替代 LicenseSystem 的 Runtime 授权
* BuildManifest 必须是构建生成的不可变、可追溯结果元数据，禁止保存用户数据与运行时状态
* BuildProfile 解析需要补充确定性合并、依赖、冲突、平台兼容性与错误诊断详细设计
* 现有构建流程需要在实现前完成迁移审计，确认 Editor 配置与 Runtime 只读元数据边界
* P2.9 已完成 LicenseSystem 总体架构设计；激活、校验、Provider、Policy、安全存储与网络授权仍未实现
* LicenseSystem 只负责 Runtime 授权，不负责 Feature 实现、构建裁剪、账号、支付或加密算法实现
* LicenseFeature 与 BuildFeature、BuildVariant 必须显式映射；构建包含能力不代表能力已授权
* OfflineProvider 必须支持完全离线运行，LicenseSystem 禁止强制依赖 NetworkSystem
* LicenseState 只表达总体状态，Feature 级授权必须返回独立结果和稳定原因码
* LicenseKey 不等同于安全存储，凭据、机器标识和 Provider 响应禁止进入普通日志与 FrameworkConfig
* SaveSystem 授权缓存不能成为永久授权真相，必须补充完整性、到期、复验、迁移和损坏恢复规则
* LicensePolicy 需要补充试用、到期、离线窗口、时钟异常、在线失败与默认拒绝详细规则
* LicenseContext 必须对消费者只读，并补充原子更新与状态变化通知详细设计
* 现有授权、机器码与激活流程需要在实现前完成迁移审计
* P2.10 已完成 NetworkSystem 总体架构设计；Provider、Session、Policy、状态监控与重连仍未实现
* NetworkSystem 只负责通用传输和连接生命周期，禁止理解业务协议、业务消息和业务状态
* NetworkProfile 的 Runtime 服务环境与 BuildEnvironment 的构建环境必须显式映射，Production 禁止静默回退至 Test 或 Development
* NetworkEndpoint 必须通过稳定 ServiceName 与 EndpointId 解耦消费者和环境地址，禁止业务逻辑散落硬编码地址
* NetworkState.Connected 只表示对应传输连接建立，不代表业务认证成功、服务健康或互联网整体可用
* NetworkSession 必须具有明确所有者、取消、关闭和释放规则，禁止遗留无所有者连接
* NetworkPolicy 必须补充幂等、最大尝试次数、退避、抖动、取消和失败终止规则，禁止无限隐式重试
* LicenseSystem 仅可选消费 NetworkSystem，完全离线 Runtime 禁止强制依赖 NetworkSystem
* SaveSystem 不保存活动 Session、Socket、请求对象或连接状态；网络同步与消息持久化需要独立设计
* ResourceSystem 当前不依赖 NetworkSystem，远程资源 Provider、完整性、续传与缓存需要独立设计
* 现有 ClientManager、DownloadManager、Socket 与 HTTP 调用需要在实现前完成迁移审计
* 已确认 Google.Protobuf 运行库位于 `Assets/3rdBy/ByFramework/Socket/Plugins/GoogleProto/`，禁止重复引入
* 已确认 pb 转 C# 工具位于 `Assets/3rdBy/ByTools/CompileProto/Editor/CompileProtoEditorWindow.cs`，禁止重复实现或破坏现有工具链
* 当前仓库未发现 `.proto` 源文件；需要审计协议源文件归属、版本控制状态与生成可重复性
* 当前生成代码位于 `Socket/ProtoFile/Proto/` 与 `ProtoCSharp/`，工具默认输出 `Assets/Scripts/ProtoCSharp/`；需要整理统一生成目录与命名空间规范
* 审计 Protobuf 包位置、版本、依赖与平台兼容性
* 审计 pb 转 C# 工具位置、输入输出、protoc 版本与可重复生成能力
* 整理 proto 目录规范
* 整理生成代码目录规范
* 设计 MessageId、MessageEnvelope、MessageCodec、IMessageSerializer 与 SerializerProvider 抽象
* 设计协议版本、MessageId 分配、字段保留与兼容策略
* NetworkSystem 可以提供通用协议封装和序列化机制，但禁止硬编码业务消息或业务协议语义
* FeatureModule 可以维护自己的 proto、生成代码和协议版本；生成代码禁止反向依赖业务逻辑
* 设计 SimulationSync 与 NetworkSystem 边界，禁止将车辆、关节、碰撞、物理状态或同步算法写入 Platform/NetworkSystem
* 设计 SimulationServer 独立模块边界，NetworkSystem 只提供通信管道
* 设计多线程收发、主线程解耦、ThreadDispatcher、批处理、压缩、带宽、频率、延迟、丢包与非阻塞通信边界
* UDP、KCP / Reliable UDP 与 LocalLoopbackProvider 仅预留，不得在专项设计前实现
* 后续每个系统设计文档必须补齐职责、不负责范围、可依赖模块、禁止依赖模块、可扩展点与 FrameworkConfig 关系六项契约
* 当前模块尚未实现统一服务注册机制；后续实现必须遵守 P3.1 已确定的可替换接口注册与生命周期边界
* Platform 子系统后续实现时需要持续审计循环依赖、直接构造其它模块实现和 FeatureModule 反向污染风险
* InputSystem 后续实现必须保持对 UISystem、DisplaySystem、LocalizationSystem 与 FeatureModule 具体实现零依赖

* FrameworkConfig 当前仍存在早期 `Core/Config/FrameworkConfig.cs`、`FrameworkConfigProvider.cs` 与 `Resources/FrameworkConfig.asset` 兼容代码；后续不得继续扩展为万能配置中心，需要在 Platform/FrameworkConfig 阶段迁移为 EditorConfig、RuntimeConfig、ExternalConfig 与 RuntimeConfigUI 体系。

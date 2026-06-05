# ByFramework Todo

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
* FrameworkEntry Phase2 已完成设计，但尚未接管任何模块；后续必须按 ThreadDispatcher、EventManager、FSMManager 顺序逐个实施与验证
* FrameworkEntry 接管单个模块时，必须同步消除该模块原有自动初始化与入口初始化并存造成的双重初始化风险
* ThreadDispatcher、EventManager 与 FSMManager 当前初始化时机不同，迁移时需要分别验证 BeforeSceneLoad 与 AfterSceneLoad 行为兼容性
* Guide Dispatcher 在接入 FrameworkEntry 前缺少显式初始化、宿主重建、静态清理和退出生命周期接口
* SoundManager 在接入 FrameworkEntry 前需要明确场景实例优先级、音频子节点、Resources 与退出清理边界
* UIRoot、UIManager、ClientManager、DownloadManager 与 GuideManager 当前存在场景、资源、连接或任务生命周期依赖，暂不接入 FrameworkEntry
* FrameworkConfig 第一阶段尚未接入 Guide、UI、Socket 与下载模块，现有模块仍使用原配置来源
* 当前目录结构尚未完全按 Core、Platform、FeatureModule 分层，后续迁移必须分模块实施并保持 API 稳定
* UI 当前缺少面向单屏、分屏、多联屏、融屏、特殊比例、多分辨率、VR 与 Non-VR 的统一适配方案
* UISystem 尚未建立统一主题、UIInputNavigationSystem 与 UIFocusSystem，当前不应将 UGUI Navigation 固化为未来核心方案
* InputSystem Design 已完成，但 InputAction 标识、值类型、事件阶段和最终 API 尚未设计或实现
* InputContext 的优先级、独占、透传、消费与异常恢复规则需要独立设计
* InputProfile 的默认、项目、FeatureModule 与用户覆盖合并规则，以及冲突诊断和版本迁移策略尚未设计
* InputDevice Adapter 需要隔离键盘、鼠标、手柄、VR 控制器、工业按钮和自定义外部设备协议差异
* 框架不得定义具体业务键位；VehicleSimulationInputProfile 等业务输入配置必须归属对应 FeatureModule
* 当前 Guide、Extension、UI 与 Samples 中仍存在 `Input.GetAxis`、`Input.GetMouseButton`、`Input.GetKeyDown`、`KeyCode` 和 `StandaloneInputModule` 等直接输入依赖，后续需要独立迁移审计
* 用户 InputBinding、设备偏好和校准数据需要通过 SaveSystem 差异化持久化，不应写入 FrameworkConfig
* InputSystem、LocalizationSystem 与 UISystem 之间尚未建立 Binding Display Token 和动态输入提示刷新契约
* LocalizationSystem 尚未建立语言管理、运行时切换、默认语言和模块语言包扩展机制
* 本地化翻译内容尚未按 Core、Platform 与 FeatureModule 建立独立维护边界
* DisplaySystem、UISystem、InputSystem、BuildProfileSystem、LicenseSystem、ResourceSystem、LocalizationSystem、NetworkSystem 与 SaveSystem 尚处于架构规划阶段
* 当前业务扩展模块与通用框架能力之间缺少明确的 FeatureModule 边界规范
* ResourceSystem 尚未建立；当前阶段继续保留 Resources，不开始资源系统重构或 Addressables 接入
* Platform 长期规划尚未进入实现阶段，后续启动任何系统前需要单独设计、确认范围并更新 Roadmap
* Platform Architecture Design 已完成，但九个 Platform 系统均未开始实现；后续必须按独立设计任务逐项确认职责与 API
* Platform 内部需要建立单向依赖契约，避免 UISystem、ResourceSystem、LocalizationSystem 与 InputSystem 形成循环依赖
* FrameworkConfig 后续只能增加轻量启动配置、开关与 Profile 标识，不应承载语言包、资源清单、用户绑定、授权状态或构建过程数据
* ResourceSystem 需要先完成 Contract Design；当前继续保留 Resources，不开始 AssetBundle 实现或 Addressables 接入
* SaveSystem 需要定义用户配置、授权状态和 FeatureModule 存档的隔离、版本与迁移边界
* 多联屏现场校准、用户 InputBinding、授权状态等机器或用户特定数据需要独立持久化方案，不应写入 FrameworkConfig
* BuildProfileSystem 必须保持构建期编排与 Runtime 配置分离，Runtime 只消费只读 Profile 元数据
* 后续每个系统设计文档必须补齐职责、不负责范围、可依赖模块、禁止依赖模块、可扩展点与 FrameworkConfig 关系六项契约
* 当前模块尚未建立统一服务注册机制；跨模块同步能力调用需要先设计可替换的接口注册边界
* Platform 子系统后续实现时需要持续审计循环依赖、直接构造其它模块实现和 FeatureModule 反向污染风险
* InputSystem 后续实现必须保持对 UISystem、DisplaySystem、LocalizationSystem 与 FeatureModule 具体实现零依赖

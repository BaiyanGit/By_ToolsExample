
## 2026-06-08 架构文档补充

* 补充 NetworkCore 独立性原则，明确 NetworkCore 必须保持纯 C#、低依赖、可独立抽离。
* 明确 NetworkSystem Adapter 才负责接入 PlatformServiceRegistry、FrameworkConfig、ThreadDispatcher 与 EventManager。
* 明确当前旧 Socket 与 Http/DownLoad 代码仅作为迁移参考，不得直接视为最终 NetworkCore 实现。
* 补充 FrameworkConfig 外部配置、StreamingAssets、PersistentDataPath 与 RuntimeConfigUI 边界。
* 补充 AssetBundle 与 ResourceSystem、Downloader、BuildProfileSystem 的归属关系。

# ByFramework Changelog

## 2026-06

### 文档

* 建立 ByFramework 框架文档体系
* 建立 ByFramework 编码规范文档，并更新 README 与 AGENTS 工作规范
* 更新 README 中对框架安装目录的说明，避免要求固定目录
* 完成 P1.2 Architecture Alignment，明确 ByFramework 为业务无关的 Unity 通用应用框架
* 建立 Core、Platform、FeatureModule 三层长期架构与依赖边界
* 补充 DisplaySystem、BuildProfileSystem、LicenseSystem 与 ResourceSystem 长期规划
* Roadmap 调整为平台化、FeatureModule 与生态演进方向，并移除 Addressables 既定接入项
* 明确 FeatureModule 不绑定具体行业，并统一 BuildProfileSystem 与 ResourceSystem 长期规划术语
* 明确 Platform Roadmap 条目仅为长期规划，当前未开始系统实现
* 明确 Windows 与 Linux 为当前目标平台，并保留其它平台扩展方向
* 将 UISystem 纳入 Platform 长期架构，补充 UI Theme、输入导航、焦点控制与输入抽象规划
* 补充 InputAction、InputBinding、InputProfile 与 InputDevice 的业务无关边界
* 补充 DisplaySystem 显示比例、BuildProfile 业务无关命名与 ResourceSystem 资源隔离规划
* 补充 LocalizationSystem 长期规划，明确多语言范围、运行时切换与模块语言包扩展方向
* 明确框架负责语言管理机制，各模块独立维护具体翻译内容
* 将 InputSystem 纳入 Platform 分层，并统一 UIFocusSystem 与 UIInputNavigationSystem 长期规划命名
* 明确 Core 与 Platform 不得写死具体车辆或设备类型，业务键值配置由 FeatureModule 的 InputProfile 维护
* 完成 P2.1 Platform Architecture Design，明确 Foundation、Environment、Experience 与 Operations 职责分组
* 明确 DisplaySystem、UISystem、InputSystem、LocalizationSystem、ResourceSystem、LicenseSystem、BuildProfileSystem、NetworkSystem 与 SaveSystem 的职责边界
* 建立 Platform 单向依赖原则，并明确 UISystem 对 Display、Input、Localization 与 Resource 能力的消费边界
* 明确 FrameworkConfig 只承载轻量启动配置、开关与 Profile 标识，不承载语言包、资源清单、用户状态或授权数据
* 明确 BuildProfileSystem 构建期职责与 FrameworkConfig Runtime 配置职责的边界
* 新增 Documentation/PlatformArchitectureDesign.md，记录 Platform 依赖关系、配置边界、风险与实现优先级
* 完成 P2.2 InputSystem Design，明确 InputAction、InputBinding、InputProfile、InputDevice 与 InputContext 的职责边界
* 补齐 IndustrialControlPanel 复合设备边界，区分单一 HardwareButton 与包含按钮、旋钮、轴和厂商协议的工业控制面板
* 明确 InputAction 使用稳定作用域标识，显示名称、翻译文本和当前 Binding 不参与动作身份
* 明确 InputContext 必须具备所有者与生命周期，并要求输入冲突处理保持确定性和可诊断
* 明确开发期键鼠模拟与运行期外部硬件输入共享同一设备无关 Action 消费路径
* 建立 Framework Default、Project、FeatureModule 与 User Override Profile 配置层级
* 明确 FrameworkConfig 只保存默认 Profile 标识与设备选择策略，用户 Binding 和校准数据由 SaveSystem 持久化
* 明确 LocalizationSystem 负责 Binding 显示名称和输入提示模板本地化，UISystem 只消费 UI 语义动作
* 记录现有直接使用 Unity 输入 API 的调用为未来独立迁移审计范围
* 新增 Documentation/InputSystemDesign.md，记录输入上下文、依赖关系、配置边界与后续设计任务
* 补充 ByFramework 全局低耦合、模块化与可替换架构约束
* 明确 Core、Platform 与 FeatureModule 的单向依赖规则，以及跨模块优先使用接口、EventManager、配置和服务注册
* 建立后续系统设计文档的六项必需契约：职责、不负责范围、可依赖、禁止依赖、可扩展点与 FrameworkConfig 关系
* 明确 InputSystem 不反向依赖 UISystem、DisplaySystem、LocalizationSystem 或 FeatureModule 具体实现
* 明确 FrameworkConfig 不作为模块通信总线或运行时状态容器
* 新增 Documentation/ByFramework_Current_Context.md，汇总当前项目定位、架构、完成状态、约束与新窗口启动说明
* 完成 P2.3 UISystem Design，明确 UIThemeSystem、UIFocusSystem、UINavigationSystem、UIWindowSystem 与 UILayerSystem 的职责边界
* 统一使用 UINavigationSystem 作为框架级导航规划名称，替代早期 UIInputNavigationSystem 术语
* 明确 UISystem 单向消费 InputSystem、DisplaySystem、LocalizationSystem 与 ResourceSystem 的公开契约
* 明确 Background、Normal、Popup、Overlay 与 Debug 层级职责，以及视觉层级与输入阻断策略必须分离
* 明确窗口栈、返回逻辑、焦点恢复、焦点锁所有权与运行时 Theme 切换边界
* 新增 Documentation/UISystemDesign.md，记录 UISystem 核心模型、配置边界、业务模块边界、风险与后续详细设计任务
* 完成 P2.4 DisplaySystem Design，明确 DisplayProfile、DisplayMode、DisplayRegion、DisplayLayout、DisplayAdapter 与 DisplayContext 的职责边界
* 明确 DisplayMode 使用可组合的输出拓扑与 VR / NonVR 呈现形态，避免互斥单枚举无法表达真实显示环境
* 建立 Framework Default、Project、FeatureModule、User Override 与 Machine Calibration Override 显示配置层级
* 明确 DisplayProfile 描述期望配置，DisplayContext 描述当前实际生效的只读显示状态
* 明确 UISystem 单向消费 DisplayContext，DisplaySystem 不管理 UIRoot、CanvasScaler、UIManager、业务 Camera 或业务内容
* 明确多联屏、融屏、投影与工业显示现场校准属于机器特定覆盖，不进入 FrameworkConfig
* 新增 Documentation/DisplaySystemDesign.md，记录显示模式、区域、布局、适配器、配置边界、风险与后续详细设计任务
* 完成 P2.5 LocalizationSystem Design，明确 Language、LanguagePack、LocalizationKey、LocalizationEntry、LocalizationProvider 与 LocalizationContext 的职责边界
* 明确 LanguageCode 使用稳定标准化语言标签，LocalizationKey 与显示文本分离并支持模块作用域
* 建立 Core、Platform 与 FeatureModule 独立语言包所有权边界，允许业务模块注册自己的 ModuleLanguagePack
* 明确 FrameworkConfig 只保存默认语言标识，不保存翻译文本、语言包、当前用户语言或运行时 LocalizationContext
* 明确 UISystem 负责动态文本刷新、RTL、字体回退与布局适配，LocalizationSystem 不直接控制 UI
* 明确 LicenseSystem 与 BuildProfileSystem 核心逻辑使用稳定状态码、错误码或 LocalizationKey，只在展示或工具边界消费翻译文本
* 明确 ResourceSystem 提供可选语言包加载能力，SaveSystem 只保存用户 LanguageCode 偏好
* 新增 Documentation/LocalizationSystemDesign.md，记录语言、语言包、查询、回退、运行时切换、配置边界、风险与后续详细设计任务
* 完成 P2.6 SaveSystem Design，明确 SaveScope、SaveKey、SaveEntry、SaveProfile、SaveProvider 与 SaveContext 的职责边界
* 明确 SaveScope 分离 OwnerScope 与 SubjectScope，以表达模块用户数据、平台机器校准和项目数据等组合所有权
* 明确 SaveKey 稳定、可版本化、支持作用域并与文件路径、类名和显示文本分离
* 建立 DefaultProfile、ProjectProfile 与 UserProfile 存储策略边界，并明确 Machine 数据通过独立 SubjectScope 隔离
* 规划 JsonProvider、BinaryProvider 与 EncryptedProvider，并明确 EncryptedProvider 不自动等同于安全密钥存储
* 明确 SaveSystem 负责版本、迁移、校验、原子写入、备份与恢复，不负责网络同步、资源内容或业务数据语义
* 明确 Input、Display、Localization、License 与 FeatureModule 的持久化消费边界，以及 FrameworkConfig 不保存用户数据和运行时状态
* 新增 Documentation/SaveSystemDesign.md，记录存储作用域、Key、Provider、安全边界、数据分类、风险与后续详细设计任务
* 完成 P2.7 ResourceSystem Design，明确 ResourceKey、ResourceLocation、ResourceProvider、ResourceCatalog、ResourceContext 与 ResourceHandle 的职责边界
* 明确 ResourceKey 与路径、文件名、Bundle 名和 Provider 分离，并支持模块作用域
* 明确 ResourceCatalog 由模块资源描述或 Catalog Fragment 组合，不反向依赖具体 FeatureModule 类型
* 规划 ResourcesProvider、AssetBundleProvider、CustomProvider 与 AssetDatabaseProvider Editor 边界
* 明确当前 Runtime 继续使用 Resources、Editor 工具继续使用 AssetDatabase，不开始 ResourceSystem 重构、不引入 Addressables、不实现 AssetBundle
* 明确 ResourceHandle 表达加载状态、拥有关系和释放请求，不保证所有 Unity 对象均可立即卸载
* 明确 Localization LanguagePack、UI、Theme 与配置资源通过 ResourceSystem 公开契约获取
* 明确 BuildProfileSystem Editor 可以消费资源描述生成裁剪 Catalog，但 Runtime ResourceSystem 不依赖 BuildProfileSystem Editor 实现
* 明确 FrameworkConfig 不保存 ResourceCatalog、Manifest、完整资源清单或运行时 ResourceHandle
* 新增 Documentation/ResourceSystemDesign.md，记录资源身份、定位、Provider、Catalog、Handle、模块隔离、风险与后续详细设计任务
* 完成 P2.8 BuildProfileSystem Design，明确 BuildProfile、BuildFeature、BuildModule、BuildEnvironment、BuildVariant 与 BuildManifest 的职责边界
* 明确 BuildProfileSystem 为 Editor 优先系统，Runtime 仅消费只读 BuildManifest 与最小构建元数据
* 明确构建组合与 LicenseSystem Runtime 授权分离，BuildVariant 不替代授权规则
* 明确 BuildProfileSystem 可向 ResourceSystem 提供资源选择与裁剪输入，但不负责资源加载
* 明确 FrameworkConfig 不保存完整构建配置，框架核心不绑定具体客户、项目或车辆类型
* 新增 Documentation/BuildProfileSystemDesign.md，记录构建组合、依赖校验、Editor 与 Runtime 边界、风险与后续详细设计任务
* 完成 P2.9 LicenseSystem Design，明确 LicenseIdentity、LicenseKey、LicenseFeature、LicensePolicy、LicenseProvider、LicenseContext 与 LicenseState 的职责边界
* 明确 BuildProfileSystem 决定构建包含能力，LicenseSystem 决定 Runtime 能力授权，两者通过稳定标识显式映射
* 明确 OfflineProvider 必须支持完全离线运行，OnlineProvider 与 HybridProvider 仅可选依赖 NetworkSystem
* 明确 SaveSystem 授权缓存不等于授权真相，LicenseKey 与 EncryptedProvider 不自动等同于安全存储
* 明确 FrameworkConfig 不保存授权数据，LicenseSystem 不负责加密算法、账号、支付或业务模块实现
* 新增 Documentation/LicenseSystemDesign.md，记录授权状态、Feature 授权、Provider、Policy、安全边界与后续详细设计任务
* 完成 P2.10 NetworkSystem Design，明确 NetworkProfile、NetworkEndpoint、NetworkProvider、NetworkContext、NetworkSession、NetworkState 与 NetworkPolicy 的职责边界
* 明确 NetworkSystem 只负责通用传输与连接生命周期，不理解业务协议、业务消息或业务状态
* 明确 LicenseSystem 仅可选消费 NetworkSystem，完全离线 Runtime 不得强制依赖网络能力
* 明确 NetworkProfile Runtime 环境与 BuildEnvironment 构建环境分离，Production 禁止静默回退至非生产环境
* 明确 NetworkState.Connected 不代表业务认证、服务健康或互联网整体可用
* 明确 SaveSystem 不保存活动网络会话，ResourceSystem 当前不依赖 NetworkSystem
* 新增 Documentation/NetworkSystemDesign.md，记录端点、Provider、Session、策略、离线边界与后续详细设计任务
* 扩充 P2.10 NetworkSystem Design，明确 NetworkSystem 是行业无关通信基础设施，不等于仿真同步系统
* 增加 MessageId、MessageEnvelope、MessageCodec、IMessageSerializer 与 SerializerProvider 通用协议和序列化规划
* 完成现有 Protobuf 工具链文档审计：确认 Google.Protobuf 运行库、CompileProto Editor 工具与现有生成代码位置，并确认仓库内未发现 `.proto` 源文件
* 明确不重复引入 Protobuf、不重复实现 pb 转 C# 工具、不修改已有生成代码且不破坏现有工具链
* 明确车辆、关节、碰撞、物理与同步模型属于 FeatureModule/SimulationSync，SimulationServer 负责 Headless / Dedicated Server 业务同步
* 增加 UDP、KCP、本地环回、多线程收发、ThreadDispatcher、批处理、压缩、带宽、频率和网络诊断长期规划
* 完成 P2.11 FrameworkConfig Phase2 Design，将 FrameworkConfig 最终定位固定为“框架启动配置与默认配置入口”
* 移除“Runtime 万能配置中心”架构定位，明确 FrameworkConfig 不作为用户存储、运行时状态容器、资源清单、构建配置或模块通信总线
* 汇总 Input、UI、Display、Localization、Save、Resource、BuildProfile、License 与 NetworkSystem 的 FrameworkConfig 允许项和禁止项
* 明确 FrameworkConfig 与 SaveSystem、BuildProfileSystem、ResourceSystem、LicenseSystem、FrameworkEntry 的最终职责边界
* 建立 FrameworkConfig 字段准入测试、精简原则与最终逻辑字段建议
* 记录当前 Guide 开关、UI Resources 路径、Socket 地址端口和下载目录字段为后续独立迁移审计范围
* 新增 Documentation/FrameworkConfigPhase2Design.md，记录当前实现、最终边界、字段建议和后续迁移任务
* 完成 P2.12 Platform Integration Review，验证 Core、Platform 与 FeatureModule 的整体依赖方向和职责边界
* 确认 UI→Input、UI→Display、Localization→Resource、License→Network、License→Save 等依赖保持单向
* 确认 BuildProfileSystem Editor→Resource descriptors 仅为构建期依赖，Runtime 禁止反向依赖
* 识别 FrameworkConfig 强类型引用 Platform 可能形成 `Core → Platform` 反向依赖的高风险
* 确认车辆、关节、物理、碰撞、仿真协议和 Server 权威逻辑归属 FeatureModule
* 输出 FrameworkConfig 风险清单、Platform 职责检查结果、扩展性验证和 P3 推荐实施路线
* 新增 Documentation/PlatformIntegrationReview.md，记录依赖关系图、风险、审查结论与实施准备建议
* 完成 P2 设计阶段统一项目上下文快照，更新 Documentation/ByFramework_Current_Context.md
* 汇总当前项目定位、Core / Platform / FeatureModule 架构、P0 / P1 / P2 完成状态与已确认架构原则
* 汇总 FrameworkConfig 最终定位、NetworkSystem 关键结论、Protobuf 工具链现状与当前风险
* 明确新窗口启动说明，并将下一推荐任务设为 P3.1 Platform Service Registration Design
* 建立 P3.1 至 P3.6 推荐实施顺序
* 完成 P3.1 Platform Service Registration Design，冻结 Core 可见服务注册、生命周期、依赖顺序与错误边界
* 明确 IService、IServiceRegistry、ServiceDescriptor、ServiceLifetime 与 ServiceContext 的中立契约归属 Core
* 明确 FrameworkEntry 只编排组合层提前提交的 ServiceDescriptor，不扫描、不引用或直接构造 Platform 类型
* 明确 Platform、FeatureModule 与应用组合层显式注册具体服务、Factory、强类型配置和依赖声明
* 明确 FrameworkConfig 禁止直接引用 Platform 强类型配置，也不保存 Descriptor、Factory 或服务注册列表
* 明确 Framework 级服务按显式依赖图确定性初始化，并按实际成功初始化顺序严格逆序销毁
* 明确 Service Registry 不替代 Singleton、EventManager、FrameworkConfig 或业务状态容器，并要求单一生命周期权威
* 新增 Documentation/PlatformServiceRegistrationDesign.md，记录核心模型、发现、注册、生命周期、架构守卫与后续任务
* 完成 P3.2 ResourceSystem Runtime Contract Design，冻结 IResourceService、IResourceProvider、IResourceCatalog、IResourceHandle、ResourceRequest 与 ResourceResult 边界
* 明确 IResourceService 是 Load、Release、Query 与 Exists 的统一 Runtime 入口，Query 与 Exists 不保证实际加载成功
* 明确 ResourceKey 与路径、文件名、Bundle 名、Provider 和 Unity GUID 解耦，并冻结其注册、弃用、迁移与移除生命周期
* 明确 Catalog 使用不可变版本化 Snapshot，Fragment 变更完整验证后原子发布，移除 Fragment 不使既有 Handle 立即失效
* 明确 Async 是通用默认加载能力，Sync 仅允许 Provider 明确支持，禁止阻塞等待异步操作伪造同步
* 明确每个成功消费者获得独立 ResourceHandle，底层资源与加载操作可以共享，Handle Release 不保证资源立即卸载
* 明确 ResourcesProvider 是当前默认实现方向，AssetBundleProvider 与 CustomProvider 保持扩展边界，AssetDatabaseProvider 禁止进入 Runtime
* 明确 LocalizationSystem、UISystem、BuildProfileSystem、FeatureModule 与 Service Registration 的 ResourceSystem 集成边界
* 新增 Documentation/ResourceSystemContractDesign.md，记录 Runtime 契约、生命周期、Provider、Catalog、Handle 与后续实现建议
* 完成 P3.3 SaveSystem Runtime Contract Design，冻结 ISaveService、ISaveProvider、SaveScope、SaveProfile、SaveEntry、SaveTransaction 与 SaveMigration 边界
* 明确 Core、Platform、Module 为 OwnerScope，User 为 SubjectScope，并保留 Project、Machine 与 Session 主体隔离维度
* 明确 SaveKey 与路径、文件名、类名和 Provider 解耦，SchemaVersion 存储于 SaveEntry 元数据
* 明确 SaveEntry 是不可变、可校验、可版本化载荷封装，SaveProfile 是不可变策略 Snapshot
* 明确 SaveTransaction 负责可靠写入、Commit、Rollback 与 Recovery，原子性只在单一 Provider 明确能力边界内保证
* 明确跨 Provider 操作不得宣称原子，迁移失败不得覆盖最后一个可恢复版本
* 明确 JsonProvider、BinaryProvider 与 EncryptedProvider 遵守同一可靠写入、版本、备份、恢复和错误边界
* 明确 EncryptedProvider 不等同于密钥管理系统，SaveSystem 不保存资源内容、不负责网络同步、不理解业务语义
* 明确 FrameworkConfig、ResourceSystem、LocalizationSystem、LicenseSystem、FeatureModule 与 Service Registration 的 SaveSystem 集成边界
* 新增 Documentation/SaveSystemContractDesign.md，记录 Runtime 契约、生命周期、Provider、Transaction、Migration 与后续实现建议
* 完成 P3.4 FrameworkEntry Phase2A Design Review，冻结 FrameworkEntry 最终职责、Platform Service 生命周期和实施准备度结论
* 明确 FrameworkEntry 只负责启动、Service 生命周期与 Shutdown 编排，不实现 Service、不管理具体创建细节、不承载业务逻辑
* 冻结 Platform Service 生命周期为 Register、Initialize、Start、Stop 与 Shutdown
* 明确 Required Dependency 图完全决定 Initialize 与 Start 顺序，手工优先级只用于无依赖同层稳定排序
* 明确 Stop 严格逆实际 Start 顺序，Shutdown 严格逆实际创建或 Initialize 顺序，并要求异常隔离与聚合
* 明确允许生命周期阶段内有界异步排空与释放，禁止 Shutdown 完成后的后台工作
* 明确 Registry 是已注册 Service 的唯一生命周期权威，Singleton 只能作为迁移期兼容访问方式
* 确认 FrameworkConfig 保持启动默认配置定位，设计层强类型依赖风险已消除，代码层仍待架构守卫与字段迁移审计
* 确认 Phase2A 可进入仅 ThreadDispatcher 的窄范围实施准备，完整 Registry 编排和所有 Platform Service 尚不得直接进入 Runtime 实现
* 输出 ResourceSystem、SaveSystem、InputSystem、LocalizationSystem、DisplaySystem、NetworkSystem 与 LicenseSystem 实施准备度评估
* 新增 Documentation/FrameworkEntryPhase2AReview.md，记录生命周期图、顺序、Singleton 关系、风险和 P3.5 前置条件
* 完成 P3.4A FrameworkEntry Phase2A ThreadDispatcher 窄范围代码接管，未引入 Registry、Service 容器、自动发现或 Platform Service
* 移除 ThreadDispatcher 原有 AfterSceneLoad 自动初始化入口，由 FrameworkEntry 编排 Register、Initialize、Start、Stop 与 Shutdown
* 保持 ThreadDispatcher 实际 AfterSceneLoad 初始化时点，由 FrameworkEntry 回调触发，以继续支持首场景预放置实例复用
* 保持 DispatcherThread.Current、QueueOnMainThread 与 RunAsync 公开 API，并由 DispatcherThread 内部继续负责实例复用与创建细节
* 新增 Documentation/FrameworkEntryPhase2AThreadDispatcherImplementation.md，记录生命周期映射、风险、回滚与 Unity 验证步骤
* 完成 P3.4B EventManager Integration Review，确认 EventManager 可进入 FrameworkEntry 窄范围生命周期接管
* 完成 P3.4C EventManager Narrow Implementation，FrameworkEntry 开始在 ThreadDispatcher 之后窄范围编排 EventManager 生命周期
* 移除 EventManager 原有 AfterSceneLoad 独立自动初始化入口，避免与 FrameworkEntry 形成双重生命周期权威
* 保留 EventManager.Instance 与 EnsureInstance 兼容 API，EnsureInstance 改为进入同一套内部生命周期入口
* EventManager Shutdown 新增运行时监听表、声明式事件表、EventTypePool 与 Instance 清理
* 新增 Documentation/EventManagerIntegrationReview.md，记录当前状态、生命周期映射、依赖关系、风险和 P3.4C 实施边界
* 新增 Documentation/FrameworkEntryPhase2AEventManagerImplementation.md，记录生命周期映射、双权威处理、Shutdown 清理、风险、回滚与 Unity 验证步骤
* 完成 P3.4D FSMManager Integration Review，确认 FSMManager 可进入 FrameworkEntry 窄范围生命周期接管评审
* 明确 FSMManager 当前存在 AfterSceneLoad 自动初始化入口，P3.4E 必须消除双重生命周期权威
* 明确 FSMManager 接管顺序建议为 ThreadDispatcher、EventManager、FSMManager，销毁顺序严格逆序
* 明确 FSMManager Shutdown 必须清理状态机列表、StateMachine 状态、条件委托和当前状态，避免 Domain Reload 关闭时状态残留
* 新增 Documentation/FSMManagerIntegrationReview.md，记录当前状态、生命周期映射、依赖关系、风险和 P3.4E 实施边界
* 完成 P3.4E FSMManager Narrow Implementation，FrameworkEntry 开始在 EventManager 之后窄范围编排 FSMManager 生命周期
* 移除 FSMManager 旧 AfterSceneLoad 自动初始化入口，避免与 FrameworkEntry 形成双重生命周期权威
* FSMManager 新增 Register、Initialize、Start、Stop、Shutdown 内部生命周期入口，保留 Instance 与状态机公开 API
* FSMManager Stop 阶段停止 Update 驱动，Shutdown 阶段逆序销毁 `_machines` 并清理静态引用
* StateMachine Shutdown 清理补齐 CurrentState 退出、conditions 清空、状态委托缓存释放与 State 反向引用清理
* 新增 Documentation/FrameworkEntryPhase2AFSMManagerImplementation.md，记录生命周期映射、双权威处理、风险、回滚方案与 Unity 验证步骤
* 完成 P3.4F FrameworkEntry Phase2A Closure Review，确认 Phase2A 可以正式关闭
* 冻结 Core Early 生命周期链为 ThreadDispatcher、EventManager、FSMManager，Shutdown 严格逆序
* 确认 Singleton / Instance 在 Phase2A 中仅作为兼容访问方式，FrameworkEntry 为生命周期编排权威
* 记录 Phase2A 关闭后的遗留风险：Unity PlayMode、Domain Reload、EventManager.EnsureInstance 兼容路径、重复预放置实例与缺少自动化验证
* 新增 Documentation/FrameworkEntryPhase2AClosureReview.md，记录完成度评估、风险清单、遗留问题、关闭结论与 P3.5 推荐路线
* 开始 P3.5A Core Early Lifecycle Unity Verification，完成当前环境可执行的静态入口、生命周期顺序和预放置实例扫描
* 确认 FrameworkEntry 仍为唯一 Core Early AfterSceneLoad 编排入口，ThreadDispatcher、EventManager 与 FSMManager 未发现独立 AfterSceneLoad Init 残留
* 尝试使用 Unity 6000.0.48f1 batchmode 加载项目，但受本机 Unity Editor License 阻塞，未能进入有效项目加载、编译或 PlayMode 验证阶段
* 明确 P3.5A 当前未通过完整验收，不建议进入 P3.5B
* 继续 P3.5A：因当前环境 Unity Editor License 不可用，不再尝试运行 Unity，由本地手动执行实机验证
* 新增 Core Early 生命周期 Editor 验证窗口，菜单为 `ByFramework/验证/Core Early 生命周期验证`
* 补充 P3.5A 手动验证步骤、验证结果记录模板，并将状态保持为 Manual Verification Required
* 新增 Editor 工具中文优先规则，后续 MenuItem、EditorWindow、Button、Label、HelpBox、Validation Report、Console Log、Build Tool、Verification Tool 与 Config Tool 默认使用中文 UI
* 新增日志中文优先规则，后续 Debug.Log、Debug.LogWarning、Debug.LogError、Debug.Assert、Console 输出和验证报告默认使用中文说明，技术对象名保留英文
* 完成 ByFramework 中文化规范第一轮安全整改，中文化 FrameworkEntry、ThreadDispatcher、EventManager、FSMManager、Core Early 验证窗口、UI 自动生成器、网络/下载、Socket、Guide、RedPoint 与 UILineRenderer 的明显英文提示
* 补充 Editor UI Language Convention 与 Debug Log Language Convention，明确不得为了中文化修改 API、路径、Key、Protobuf 生成代码或协议字段
* 新增 Documentation/CoreEarlyLifecycleUnityVerification.md，记录验证场景、已执行结果、阻塞问题、修复建议和 P3.5B 前置条件
* 项目维护者已完成 P3.5A 本地 Unity 实机验证，确认 FrameworkEntry、ThreadDispatcher、EventManager、FSMManager 单实例状态均为 1，静态访问正常，重复实例检查正常，Core 生命周期链验证通过
* 关闭 P3.5A `Manual Verification Required` 与 `P3.5B Not allowed yet` 过期状态，允许直接进入并完成 P3.5B Platform Service Registration Runtime API Freeze
* 新增 Documentation/PlatformServiceRegistrationRuntimeAPIFreeze.md，冻结 Platform Service Registration 的 Runtime API、注册贡献入口、Scoped Registry 边界、异步/取消与错误聚合规则
* 更新 Documentation/ByFramework_Current_Context.md、Roadmap.md、Todo.md、Architecture.md、FrameworkEntryPhase2AClosureReview.md 与 Core/README.md，同步当前真实阶段状态
* 明确 P3.5B 完成后下一阶段为 P3.6 InputSystem Foundation，而不是 InputSystem Implementation 或 UISystem Foundation
* 完成 P3.6 InputSystem Foundation，冻结 InputAction 标识格式、Alias / Migration、InputValueKind、InputStage、InputContext 规则、InputProfile 分层合并策略与 InputDevice / InputAdapter Foundation 边界
* 新增 Documentation/InputSystemFoundation.md，记录 P3.6 Foundation 冻结范围、跨系统边界、非目标与后续实现前置条件
* 更新 Documentation/ByFramework_Current_Context.md、Roadmap.md、Todo.md 与 Architecture.md，同步 P3.6 已完成、下一阶段为 P3.7 UISystem Foundation，且当前仍未进入 InputSystem Implementation

### 事件系统

* 合并 EventManager 与旧事件中心的运行时监听能力
* EventManager 新增 Type/Enum 事件监听、移除与广播 API
* 移除旧事件中心兼容门面，运行时监听能力统一由 EventManager 提供
* 明确 Runtime Listener 主入口仅使用 AddListener、Broadcast、RemoveListener
* 明确 Enum 事件使用枚举实例本身作为 Key，避免同一枚举类型下不同枚举值共用事件
* 新增 P0-1 EventManager Runtime Listener 最小验证示例
* 新增 EventManager 模块 README
* 完成 EventManager 目录整理后的规范化检查
* 完成 IEvent 拆分后的引用检查
* 确认 TypePool 已同步为 EventTypePool 引用
* 补齐 EventManager 模块代码规范注释
* 恢复 EventManager 模块命名空间兼容性，修正目录整理后的 using 错误
* 新增 EventCallback.cs，将 CallBack 委托迁移到 EventManager 模块
* 删除旧事件中心兼容层代码，解除 EventManager 对旧事件中心命名空间的依赖
* 修正 EventManager 模块中的 Core、Handler 与 Utility 相对 using，统一使用完整命名空间

### 路径系统

* 新增 ByFrameworkPathUtility，通过 AGENTS.md、README.md 与 Documentation/Architecture.md 自动定位框架根目录
* UIAutoCreatePathSetting 改为通过 ByFrameworkPathUtility 获取框架内部路径
* UI 自动生成工具移除对固定 ByFramework 安装目录的依赖
* 全项目确认无 Assets/3rdBy/ByFramework、Assets/3rdBy/MetaFramework、3rdBy/ByFramework、3rdBy/MetaFramework 固定路径残留
* P0-0.1A：为 UIAutoCreate 配置与代码模板加载补充空引用保护和清晰错误信息
* P0-0.1B：GetRuntimePath 与 GetSamplesPath 在目标目录不存在时改为明确抛出异常
* P0-0.1C：ByFrameworkPathUtility 缓存根目录失效时自动重新扫描
* 关闭 P0-0.1 移除框架硬编码路径整改

### 框架入口

* 新增 FrameworkEntry，创建并持久化 `[ByFramework]` 框架根节点
* FrameworkEntry 增加重复创建保护、初始化日志与分阶段初始化入口
* 第一阶段保持现有模块初始化方式不变，暂不接管 EventManager、FSMManager 等模块
* 新增 Core/README.md，记录 FrameworkEntry 当前职责与验证方式
* 完成 FrameworkEntry Phase2 设计，明确渐进式模块接管原则、初始化顺序与迁移门槛
* 将 ThreadDispatcher、EventManager 与 FSMManager 定为第一优先级接管候选
* 将 Guide Dispatcher 与 SoundManager 定为需要先补生命周期设计的第二优先级候选
* 明确 UIRoot、UIManager、ClientManager、DownloadManager 与 GuideManager 当前暂不接入 FrameworkEntry
* 新增 Documentation/FrameworkEntryPhase2Design.md，记录生命周期依赖、风险与后续拆分任务

### 配置系统

* 新增 FrameworkConfig Runtime 配置结构与默认 ScriptableObject 配置资源
* 新增 FrameworkConfigProvider，提供 Resources 配置加载与全局访问入口
* 新增独立的 FrameworkEditorConfig 与默认 Editor 配置资源
* 第一阶段仅建立配置基础设施，暂不改变现有模块的配置读取行为
* 新增 Core/Config/README.md，记录配置职责与后续接入规划

### 稳定性

* 完成 P1 SingletonAudit，记录单例模板、全局实例、GameObject 创建与 DontDestroyOnLoad 使用情况
* 明确未来可逐步挂到 `[ByFramework]` 的模块，以及暂时不应由 FrameworkEntry 接管的场景依赖模块
* 新增 Documentation/SingletonAudit.md，作为后续 Singleton 重构基线
* 完成 P1.1 SingletonSafetyAudit，补充单例重复实例、静态清理与关闭 Domain Reload 风险审计
* 为 FrameworkEntry、EventManager、FSMManager、ThreadDispatcher、DownloadManager 与 GuideManager 增加安全的静态引用重置和条件清理
* 为 FSMManager 与 ThreadDispatcher 增加自动初始化重复创建保护
* 修复 MonoSingletonTemplate 与 MonoObjSingletonTemplate 销毁非当前实例时误清理静态引用的问题
* 新增 Documentation/SingletonSafetyAudit.md，记录保留风险与 Unity 验证重点
* 在 Architecture Alignment 后复审 SingletonSafetyAudit，并进入实际代码复审与最小生命周期安全修复
* 补充纯 C# 静态单例在关闭 Domain Reload 时可能跨 Play Session 保留状态的风险
* 完成 P1.1 实际代码复审，为 MonoSingletonTemplate 与 GuideManager 增加重复实例保护
* MonoObjSingletonTemplate 首次访问时优先复用场景实例，避免隐式创建重复节点
* UIRoot 与 ClientManager 在重复实例销毁后停止执行派生 Awake 初始化逻辑
* FSMManager 与 ThreadDispatcher 自动初始化时优先复用场景已有实例
* 更新 SingletonSafetyAudit、Roadmap 与 Todo，记录已修复项和保留的 Domain Reload 风险

### MetaFramework -> ByFramework

### 修复

* UIAutoCreatePathSetting
* JSaver
* VoiceData
* TestVoiceMsg
* TestGetAudioClip

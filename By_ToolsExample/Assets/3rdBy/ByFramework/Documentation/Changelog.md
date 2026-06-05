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

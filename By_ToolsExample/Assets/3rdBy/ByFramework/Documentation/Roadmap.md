# ByFramework Roadmap

## P0 核心整改

* [x] MetaFramework -> ByFramework
* [x] 修复旧命名空间引用
* [x] P0-0.1 移除硬编码框架路径
* [x] EventManager 合并旧事件中心能力
* [x] FrameworkEntry（第一阶段：基础入口）
* [x] FrameworkConfig（第一阶段：配置基础设施）

## P1 稳定性

* [x] SingletonAudit
* [x] P1.1 SingletonSafetyAudit（代码复审与最小生命周期安全修复完成）
* [x] P1.2 Architecture Alignment
* [ ] Singleton重构
* [ ] 日志系统
* [ ] 生命周期管理

## P2 平台化与扩展性

以下系统均为长期规划，不代表已经开始实现：

* [x] FrameworkEntry Phase2 Design
* [x] P2.1 Platform Architecture Design
* [x] P2.2 InputSystem Design
* [x] P2.2 InputSystem 设备分类、作用域标识与 Context 生命周期边界
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
* [x] FrameworkEntry Phase2A：ThreadDispatcher 渐进接管实现（代码完成，待 Unity 专项验证）
* [ ] FrameworkEntry Phase2B：EventManager 渐进接管
* [ ] FrameworkEntry Phase2C：FSMManager 渐进接管
* [ ] FrameworkConfig Phase2 实现
* [ ] FrameworkConfig 当前字段归属与迁移审计
* [ ] FrameworkConfig Schema、校验、缺失与兼容策略详细设计
* [ ] BuildProfileSystem 选择 FrameworkConfig 预设契约详细设计
* [ ] FrameworkEntry 消费 FrameworkConfig 默认入口生命周期详细设计
* [x] Platform 服务注册、接口发现、生命周期与错误模型设计
* [x] Core、Platform、FeatureModule 依赖方向架构守卫设计
* [x] FrameworkConfig Core / Platform 强类型依赖风险设计处理
* [ ] Platform Service Registration 最终 API、异步、取消与结果类型设计
* [ ] Platform Service Registration Runtime 注册贡献入口与应用组合层设计
* [ ] Platform Service Registration 实现与 FrameworkEntry 编排接入
* [x] FrameworkEntry Phase2A Design Review
* [x] Platform Service Register、Initialize、Start、Stop、Shutdown 阶段设计冻结
* [x] Registry 与 Singleton 唯一生命周期权威关系审查
* [x] Platform Service 实施准备度审查
* [ ] Core、Platform、FeatureModule 依赖方向架构守卫实现
* [ ] FrameworkConfig Core / Platform 强类型依赖风险实现整改
* [ ] BuildProfileSystem Editor / Runtime 隔离验证
* [ ] Guide Dispatcher 生命周期设计
* [ ] SoundManager 生命周期设计
* [ ] IUILoader
* [ ] DisplaySystem 实现
* [ ] UISystem 实现
* [ ] UIThemeSystem 详细设计与实现
* [ ] UIFocusSystem 详细设计与实现
* [ ] UINavigationSystem 详细设计与实现
* [ ] UIWindowSystem 详细设计与实现
* [ ] UILayerSystem 详细设计与实现
* [ ] InputSystem
* [ ] Input Profile
* [ ] ResourceSystem 实现
* [ ] BuildProfileSystem 实现
* [ ] LicenseSystem 实现
* [ ] LocalizationSystem 实现
* [ ] Localization Language Pack 详细设计与实现
* [ ] Localization Runtime Language Switching 详细设计与实现
* [ ] NetworkSystem 实现
* [ ] SaveSystem 实现
* [x] ResourceSystem Runtime Contract Design
* [x] ResourceKey Runtime 身份与生命周期契约设计
* [x] ResourceCatalog Runtime Snapshot、查询与模块隔离契约设计
* [x] ResourceProvider Runtime 加载、能力与释放边界设计
* [x] ResourceHandle Runtime 生命周期与引用策略设计
* [x] ResourceRequest 与 ResourceResult Runtime 契约设计
* [ ] ResourceSystem Runtime 最终 C# 数据结构、错误码与 API 设计
* [ ] ResourceSystem 同步、异步、取消、进度与线程最终 API 设计
* [ ] ResourceCatalog Fragment Schema、Snapshot 构建与原子发布实现设计
* [ ] ResourceHandle 内部加载记录、缓存、泄漏诊断与停止策略实现设计
* [ ] ResourcesProvider 兼容与迁移策略设计
* [ ] AssetDatabaseProvider Editor 隔离与 Catalog 构建设计
* [ ] BuildProfileSystem 资源裁剪与 Catalog 生成契约设计
* [ ] 现有资源系统迁移审计
* [x] SaveSystem Runtime Contract Design
* [x] SaveScope 与 SaveKey Runtime 契约设计
* [x] SaveEntry 与 SaveProfile Runtime 契约设计
* [x] SaveProvider Runtime 能力与可靠写入边界设计
* [x] SaveTransaction 原子性、提交、回滚与恢复契约设计
* [x] SaveMigration 版本升级与兼容契约设计
* [ ] SaveSystem Runtime 最终 C# 数据结构、错误码与 API 设计
* [ ] SaveSystem 同步、异步、取消、线程与并发最终 API 设计
* [ ] SaveProfile Schema、Provider 路由、路径、配额与原子切换实现设计
* [ ] SaveTransaction 日志、崩溃恢复与并发冲突实现设计
* [ ] SaveMigration Registry、迁移链验证与幂等实现设计
* [ ] 平台安全存储与 LicenseSystem 敏感数据边界设计
* [ ] 现有存储系统迁移审计
* [ ] InputAction 标识、别名迁移与值类型详细设计
* [ ] InputContext 优先级、消费、所有权与恢复规则详细设计
* [ ] InputProfile 合并与版本策略设计
* [ ] InputDevice Adapter 接口设计
* [ ] 现有直接输入调用迁移审计
* [ ] DisplayMode 可组合模型详细设计
* [ ] DisplayProfile 合并与机器校准覆盖详细设计
* [ ] DisplayRegion 与 DisplayLayout 详细设计
* [ ] DisplayAdapter 与 DisplayContext 详细设计
* [ ] 现有显示系统迁移审计
* [ ] Language 与 LanguageCode 元数据详细设计
* [ ] LocalizationKey 命名、别名与迁移详细设计
* [ ] LocalizationEntry 格式化与变体详细设计
* [ ] LocalizationProvider 与 LocalizationContext 详细设计
* [ ] 现有硬编码显示文本迁移审计
* [ ] 现有 UI 系统迁移审计
* [ ] NetworkProfile 环境选择、覆盖与 BuildEnvironment 映射详细设计
* [ ] NetworkEndpoint 稳定标识、ServiceName 注册与敏感参数边界详细设计
* [ ] NetworkProvider 统一结果、错误码、取消与生命周期契约详细设计
* [ ] HttpProvider 请求、响应、幂等与重试契约详细设计
* [ ] SocketProvider 与 WebSocketProvider Session、消息边界和关闭契约详细设计
* [ ] NetworkPolicy 超时、退避、抖动、重连和心跳详细设计
* [ ] NetworkContext 状态监控、诊断与离线模式详细设计
* [ ] 审计 Protobuf 包位置、版本、依赖与平台兼容性
* [ ] 审计 pb 转 C# 工具位置、输入输出与可重复生成能力
* [ ] 整理 proto 源文件目录规范
* [ ] 整理生成代码目录与命名空间规范
* [ ] 设计 MessageId、MessageEnvelope、MessageCodec、IMessageSerializer 与 SerializerProvider 抽象
* [ ] 设计协议版本、MessageId 分配与兼容策略
* [ ] 设计 SimulationSync 与 NetworkSystem 边界
* [ ] 设计 SimulationServer 独立模块边界
* [ ] 设计多线程收发、ThreadDispatcher、批处理、压缩、带宽、频率和网络诊断边界
* [ ] 远程资源 Provider、云同步、UDP 与本地环回通信专项设计
* [ ] 现有 ClientManager、DownloadManager、Socket 与 HTTP 调用迁移审计
* [ ] LicenseIdentity 设备标识、隐私与平台适配详细设计
* [ ] LicenseKey Schema、凭据生命周期、脱敏与安全存储边界详细设计
* [ ] LicenseFeature 与 BuildFeature、BuildVariant 映射规则详细设计
* [ ] LicensePolicy 到期、试用、离线窗口、复验与时钟异常规则详细设计
* [ ] OfflineProvider、OnlineProvider 与 HybridProvider 契约详细设计
* [ ] LicenseContext 原子更新、状态变化通知与 Feature 查询详细设计
* [ ] SaveSystem 授权缓存、完整性、迁移与失效契约详细设计
* [ ] NetworkSystem 可选在线验证契约详细设计
* [ ] 现有授权、机器码与激活流程迁移审计
* [ ] BuildProfile 继承、覆盖、合并顺序与确定性解析详细设计
* [ ] BuildFeature 与 BuildModule 依赖、冲突和平台兼容性详细设计
* [ ] BuildEnvironment 与 BuildVariant 配置模型详细设计
* [ ] BuildManifest Schema、版本升级与可追溯性详细设计
* [ ] Editor 构建编排、CI 集成与 Runtime 元数据消费边界详细设计
* [ ] 现有项目构建流程迁移审计
* [ ] 模块热插拔

## P3 FeatureModule 与生态

* [x] P3.1 Platform Service Registration Design
* [x] P3.2 ResourceSystem Contract Design
* [x] P3.3 SaveSystem Contract Design
* [x] P3.4 FrameworkEntry Phase2A Design Review
* [x] P3.4A FrameworkEntry Phase2A：ThreadDispatcher 窄范围实现（代码完成，待 Unity 专项验证）
* [ ] P3.5 InputSystem Implementation
* [ ] P3.6 UISystem Foundation
* [ ] FeatureModule 边界规范
* [ ] FeatureModule 注册、配置、资源、存储、协议与卸载规范
* [ ] SimulationSync 独立模块设计
* [ ] SimulationServer 独立模块设计
* [ ] 示例工程
* [ ] API文档
* [ ] Wiki

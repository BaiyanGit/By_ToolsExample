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
* [ ] FrameworkEntry Phase2A：ThreadDispatcher 渐进接管
* [ ] FrameworkEntry Phase2B：EventManager 渐进接管
* [ ] FrameworkEntry Phase2C：FSMManager 渐进接管
* [ ] Guide Dispatcher 生命周期设计
* [ ] SoundManager 生命周期设计
* [ ] IUILoader
* [ ] DisplaySystem
* [ ] UISystem
* [ ] UI Theme System
* [ ] UIFocusSystem
* [ ] UIInputNavigationSystem
* [ ] InputSystem
* [ ] Input Profile
* [ ] ResourceSystem
* [ ] BuildProfileSystem
* [ ] LicenseSystem
* [ ] LocalizationSystem
* [ ] Localization Language Pack 边界
* [ ] Localization Runtime Language Switching
* [ ] NetworkSystem
* [ ] SaveSystem
* [ ] ResourceSystem Contract Design
* [ ] SaveSystem Contract Design
* [ ] InputAction 标识与值类型设计
* [ ] InputContext 优先级与消费规则设计
* [ ] InputProfile 合并与版本策略设计
* [ ] InputDevice Adapter 接口设计
* [ ] 现有直接输入调用迁移审计
* [ ] DisplaySystem Design
* [ ] LocalizationSystem Design
* [ ] UISystem Design
* [ ] NetworkSystem Design
* [ ] LicenseSystem Design
* [ ] BuildProfileSystem Design
* [ ] 模块热插拔

## P3 FeatureModule 与生态

* [ ] FeatureModule 边界规范
* [ ] 示例工程
* [ ] API文档
* [ ] Wiki

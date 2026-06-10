# P1.1 Singleton Safety Audit

## 范围与原则

本轮检查单例重复实例、静态引用清理、DontDestroyOnLoad、关闭 Domain Reload 后的静态状态，以及隐式创建节点规范。

仅修复局部且明确的安全问题。不接管模块、不改变公开 API、不批量替换 Instance，也不改变 FrameworkEntry 初始化职责。

## 复审结论

P1.2 Architecture Alignment 完成后已重新执行生命周期安全审计，并对明确的重复实例与静态引用覆盖风险实施最小修复。

本轮不改变公开 API、模块结构与 FrameworkEntry 职责。剩余问题涉及泛型静态状态、纯 C# 静态实例生命周期、协程服务重建策略与持久节点归属，需要在后续 Singleton 重构中逐项处理。

## 审计结果

| 实现 | 重复实例 | 静态引用清理 | Domain Reload 关闭 | GameObject / DontDestroyOnLoad | 本轮处理 |
| --- | --- | --- | --- | --- | --- |
| `MonoSingletonTemplate` | 销毁后出现的重复实例，保留原 Instance | 仅当前实例销毁时清空 Instance | 泛型模板静态状态仍需专项验证 | 不创建节点；持久化当前实例 GameObject | 增加重复实例保护，并保持条件清理 |
| `MonoObjSingletonTemplate` | 首次访问优先复用场景实例，找不到时才创建 | 仅当前实例销毁时清空 `_instance` | 泛型模板静态状态仍需专项验证 | 隐式创建 `[Single_Type]`，无统一父节点 | 避免场景已有实例时重复创建节点 |
| `FrameworkEntry` | 已销毁重复实例 | 原先缺少 OnDestroy 清理 | 原先可能残留 Instance | 创建并持久化 `[ByFramework]` | 增加条件清理与 SubsystemRegistration 重置 |
| `EventManager` | `EnsureInstance` 已有实例保护 | 原先缺少 OnDestroy 清理 | 原先可能残留 Instance | 创建并持久化 `[Event]` | 增加条件清理与 SubsystemRegistration 重置 |
| `FSMManager` | 自动初始化优先复用场景实例 | 条件清理 Instance | 通过 SubsystemRegistration 重置 | 找不到场景实例时创建并持久化 `[FSM]` | 增加场景实例复用保护 |
| `ThreadDispatcher` | 自动初始化优先复用场景实例 | 条件清理 Current | 重置 Current 与线程计数 | 找不到场景实例时创建并持久化 `ThreadDispatcher` | 增加场景实例复用保护 |
| `Guide Dispatcher` | 静态构造器仅检查内部引用 | 没有主动清理 | 关闭 Domain Reload 时恢复策略不明确 | 创建并持久化 `Dispatcher` | 保留，需单独设计协程服务生命周期 |
| `SoundManager` | 首次访问优先复用场景实例 | 由模板负责清理 | 受泛型模板风险影响 | 找不到场景实例时创建 `[Single_SoundManager]` 及子节点 | 继承模板复用场景实例能力 |
| `UIRoot` | 重复实例由模板销毁 | 由模板负责清理 | 受泛型模板风险影响 | 场景节点持久化 | 派生 Awake 在重复对象上提前退出 |
| `ClientManager` | 重复实例由模板销毁 | 由模板负责清理 | 受泛型模板风险影响 | 场景节点持久化 | 派生 Awake 在重复对象上提前退出 |
| `DownloadManager` | 原先重复实例不会被销毁 | 原先缺少 OnDestroy 清理 | 原先可能残留 Instance | 不持久化，依赖场景放置 | 销毁重复实例，增加条件清理与静态重置 |
| `GuideManager` | 销毁后出现的重复场景实例 | 仅当前实例销毁时清空 Instance | 通过 SubsystemRegistration 重置 | 不持久化，依赖场景数据 | 增加重复实例保护 |

## DontDestroyOnLoad 结论

* FrameworkEntry、EventManager、FSMManager 与 ThreadDispatcher 均对各自创建的根节点调用一次 DontDestroyOnLoad。
* MonoSingletonTemplate 与 MonoObjSingletonTemplate 仍由模板统一调用 DontDestroyOnLoad。
* FrameworkEntry 在找到已有入口时可能再次调用 DontDestroyOnLoad，该调用是幂等的，本轮不改变启动流程。
* 当前没有将任何模块重新挂到 `[ByFramework]`。

## 隐式创建节点结论

`MonoObjSingletonTemplate` 会创建命名为 `[Single_Type]` 的 GameObject，已有统一命名，但没有统一父节点。

本轮不增加父节点规范，原因是这会改变 SoundManager 与 Socket 示例 Heartbeat 的层级和生命周期。后续应在模块接管方案中逐个处理。

## 保留风险

* 泛型单例模板在关闭 Domain Reload 时的静态状态重置仍需 Unity 专项验证。
* `SingletonTemplate<T>` 与 `EventTypePool.Instance` 等纯 C# 静态实例在关闭 Domain Reload 时可能保留上一次 Play Session 状态。
* `Guide Dispatcher` 的静态构造器与内部协程节点缺少显式重建和清理策略。
* `MonoObjSingletonTemplate` 无场景实例时仍会隐式创建持久节点，且没有统一父节点。
* `MonoObjSingletonTemplate` 没有统一 Awake，无法主动销毁场景中预放置的多个同类型实例。
* 其他继承 `MonoSingletonTemplate` 的派生 Awake 如果未检查 `Instance != this`，重复对象仍可能在销毁帧继续执行后续逻辑。
* `BaseNetModel` 在重复实例路径会抛出异常，需在 Socket 模块专项复审其预期行为。
* `GuideManager` 当前采用保留首个实例的策略，未来若需要按场景替换实例需单独设计。

## Unity 验证重点

* 开启和关闭 Enter Play Mode Options 的 Domain Reload，分别重复进入 Play Mode。
* 确认 `[ByFramework]`、`[Event]`、`[FSM]` 与 `ThreadDispatcher` 不重复创建。
* 分别在场景预放置 FSMManager 与 ThreadDispatcher，确认自动初始化复用场景实例。
* 销毁当前实例后，确认对应静态 Instance 或 Current 被清空。
* 创建重复 DownloadManager，确认重复对象被销毁且原 Instance 保持不变。
* 创建重复 UIRoot、ClientManager 与 GuideManager，确认旧 Instance 不被覆盖，重复对象不继续初始化。
* 在场景预放置 SoundManager 后首次访问 Instance，确认不会额外创建 `[Single_SoundManager]`。
* 验证 SoundManager、UIRoot、ClientManager 与 GuideManager 的正常单实例流程保持现有行为。

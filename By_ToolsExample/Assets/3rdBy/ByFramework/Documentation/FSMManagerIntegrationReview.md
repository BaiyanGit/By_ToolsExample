# P3.4D FSMManager Integration Review

## 1. 审查目标与范围

P3.4D 在 ThreadDispatcher 与 EventManager 已完成 FrameworkEntry 窄范围接管后，评审 FSMManager 是否具备进入 FrameworkEntry 生命周期编排条件。

本阶段只更新文档：

* 不修改 Runtime、Editor 或 API。
* 不开始 FSMManager 接管。
* 不引入 Registry、Service 容器、自动发现或反射扫描。
* 不修改 Samples 行为。

审查范围：

* FrameworkEntry
* FSMManager
* StateMachine
* State
* StateBuilder
* StateSwitchCondition
* FSM Samples（当前未发现独立 FSM Samples 目录）

## 2. 当前状态分析

### 2.1 FSMManager 当前生命周期

FSMManager 当前是 `MonoBehaviour` 单例，运行时存在一个 `FSMManager.Instance`。

当前生命周期事实：

* `AfterSceneLoad` 自动调用 `Init()`。
* `Init()` 优先复用场景中已有 FSMManager。
* 无场景实例时创建 `[FSM]` GameObject，并挂载 FSMManager。
* 若存在 `[ByFramework]`，新建 `[FSM]` 会挂到该节点下。
* `DontDestroyOnLoad(container)` 保持节点跨场景存活。
* `SubsystemRegistration` 只重置 `Instance = null`。
* `OnDestroy()` 只在当前实例销毁时清理 `Instance`。
* `Update()` 每帧遍历 `_machines` 并调用 `StateMachine.OnUpdate()`。

当前没有显式 Stop 或 Shutdown 阶段，也没有在 Manager 销毁时统一销毁所有状态机。

### 2.2 当前初始化入口

FSMManager 当前只有一个自动初始化入口：

```text
RuntimeInitializeOnLoadMethod(AfterSceneLoad)
  -> Init()
```

当前没有公开 `EnsureInstance()` 兼容入口。调用方依赖 `FSMManager.Instance` 访问已创建实例。

### 2.3 当前销毁逻辑

当前 Manager 销毁逻辑只有：

```text
OnDestroy
  -> if Instance == this
     -> Instance = null
```

未清理内容：

* `_machines`
* 每个 `StateMachine` 的状态列表
* 每个 `StateMachine` 的状态切换条件列表
* 当前状态 `CurrentState`
* 状态委托引用，如 `onEnter`、`onStay`、`onExit` 等

### 2.4 StateMachine 当前行为

StateMachine 持有：

* `states`
* `conditions`
* `Name`
* `CurrentState`

当前更新行为：

* `FSMManager.Update()` 调用每个 StateMachine 的 `OnUpdate()`。
* `OnUpdate()` 先调用 `CurrentState?.OnStay()`。
* 然后遍历 `conditions`，满足条件时切换状态。

当前销毁行为：

* `StateMachine.OnDestroy()` 逆序调用每个 State 的 `OnTermination()` 并移除状态。
* 清空 `states`。
* 未清空 `conditions`。
* 未显式调用当前状态 `OnExit()`。
* 未显式将 `CurrentState = null`。

这些点是 P3.4E 实现前必须确认的清理边界。

### 2.5 State / StateBuilder / StateSwitchCondition

State 是纯 C# 状态对象，持有生命周期委托：

* `onInitialization`
* `onEnter`
* `onStay`
* `onExit`
* `onTermination`

StateBuilder 会设置这些委托，并在 `Complete()` 时调用 `state.OnInitialization()`。

StateSwitchCondition 持有 `Func<bool> predicate`，该委托可能闭包引用场景对象、业务对象或 UI 对象。若 FSMManager Shutdown 不清理 StateMachine 条件，关闭 Domain Reload 或场景切换时可能产生残留引用风险。

## 3. 生命周期映射建议

建议 FSMManager 接入 FrameworkEntry 时映射为：

| 阶段 | 建议职责 | 禁止 |
| --- | --- | --- |
| Register | FrameworkEntry 固定注册 FSMManager 试点对象 | 创建 `[FSM]`、创建业务状态机 |
| Initialize | FSMManager 自行复用或创建实例，准备 `_machines` 容器 | 创建默认业务状态机、访问业务对象 |
| Start | 启用每帧 Update 状态机调度 | 修改现有状态机 API 行为 |
| Stop | 停止每帧驱动状态机，避免 Shutdown 期间继续 OnStay / 条件切换 | 销毁状态机、清空列表 |
| Shutdown | 逆序销毁所有状态机，清理状态、条件、当前状态和静态 Instance | 保留状态机或条件委托到下一次 Play Session |

P3.4E 仍应采用窄范围内部生命周期钩子，不实现通用 Registry。

## 4. 初始化顺序建议

建议顺序：

```text
FrameworkEntry BeforeSceneLoad
  -> Register ThreadDispatcher
  -> Register EventManager
  -> Register FSMManager

FrameworkEntry AfterSceneLoad
  -> Initialize ThreadDispatcher
  -> Start ThreadDispatcher
  -> Initialize EventManager
  -> Start EventManager
  -> Initialize FSMManager
  -> Start FSMManager
```

理由：

* FSMManager 当前实际自动入口是 `AfterSceneLoad`，保持该时点可降低行为变化风险。
* ThreadDispatcher 属于 Core Early，应最先可用。
* EventManager 已完成接管，应先于 FSMManager 可用，便于未来状态机使用事件通知，但本次不引入直接依赖。
* FSMManager 消费者通常在场景对象 Start / 后续流程创建状态机，因此放在 EventManager 之后更安全。

## 5. 销毁顺序建议

建议销毁顺序严格逆初始化顺序：

```text
FrameworkEntry Shutdown
  -> Stop FSMManager
  -> Shutdown FSMManager
  -> Stop EventManager
  -> Shutdown EventManager
  -> Stop ThreadDispatcher
  -> Shutdown ThreadDispatcher
```

理由：

* FSMManager 持有业务状态和业务委托引用，应先停止和清理。
* 状态退出、终止或清理流程未来可能广播事件，因此 EventManager 应在 FSMManager Shutdown 期间仍可用。
* EventManager 关闭后，不应再允许状态机触发事件。
* ThreadDispatcher 是更底层能力，应最后关闭。

## 6. 与 ThreadDispatcher 的关系

当前 FSMManager 代码没有直接调用 DispatcherThread。

接入建议：

* ThreadDispatcher 不依赖 FSMManager。
* FSMManager 生命周期顺序位于 ThreadDispatcher 之后。
* P3.4E 不应引入 FSMManager 对 ThreadDispatcher 的强代码依赖。
* 如果未来状态机支持异步状态或线程回主线程派发，应先独立设计 FSM 异步边界。

## 7. 与 EventManager 的关系

当前 FSMManager 代码没有直接调用 EventManager。

接入建议：

* EventManager 不依赖 FSMManager。
* FSMManager 可在顺序上依赖 EventManager 已启动，但 P3.4E 不建立强代码依赖。
* 如果状态进入、退出或切换需要事件通知，应由业务状态或后续 FSM 扩展显式调用 EventManager，不应在本次接管中新增广播逻辑。
* FSMManager Shutdown 应先于 EventManager Shutdown，为未来状态清理事件保留空间。

## 8. Singleton / Instance 关系

`FSMManager.Instance` 当前是主要公开访问方式，必须保留。

P3.4E 建议：

* FrameworkEntry 是唯一生命周期编排权威。
* `Instance` 只作为兼容访问入口。
* 原有 `AfterSceneLoad Init()` 自动初始化入口必须在同一批实现中移除或改由 FrameworkEntry 触发。
* 不新增公开 `EnsureInstance()`，除非实施任务明确要求兼容路径。
* 不改变 `Create`、`Destroy`、`GetMachine` 公开 API。

## 9. 接入风险清单

### 高风险

1. 旧 `AfterSceneLoad Init()` 与 FrameworkEntry 同时初始化，形成双重生命周期权威。
2. FSMManager Shutdown 不销毁 `_machines`，导致状态机和状态委托跨 Play Session 残留。
3. StateMachine `OnDestroy()` 不清理 `conditions`，闭包委托可能残留场景对象引用。
4. StateMachine `OnDestroy()` 不显式 `OnExit()` 当前状态，业务退出逻辑可能丢失。
5. 关闭 Domain Reload 时，`FSMManager.Instance` 已重置但旧状态机对象或委托引用可能仍被其它静态对象持有。

### 中风险

1. 场景中预放置多个 FSMManager 时，当前逻辑只复用第一个，不主动清理重复实例。
2. `Update()` 遍历 `_machines` 时，如果状态机在更新期间被 Destroy，可能出现列表修改时序风险。
3. `StateBuilder.Complete()` 会调用 `OnInitialization()`，而 `StateMachine.Add()` 也会调用 `OnInitialization()`，不同创建路径的初始化语义需要验证。
4. 条件切换时 `CurrentState.name` 未空检查；若存在带 sourceStateName 的条件且当前状态为空，可能抛出空引用。
5. FSMManager 没有 Stop 状态保护，Shutdown 期间仍可能继续 Update。

### 保留风险

* FSMManager 与 FSM 核心类当前代码规范注释不完整，属于后续代码规范整改，不应混入 P3.4E 窄范围接管。
* 当前未发现独立 FSM Samples 目录；若项目场景中有使用样例，需要在 Unity 验证时补充场景级验证。

## 10. 是否建议进入 P3.4E

建议进入：

> P3.4E FSMManager Narrow Implementation

但必须限定为窄范围实现，且只在以下前置条件满足时开始：

1. 明确允许修改的文件仅限 FSMManager、StateMachine、FrameworkEntry 最小必要改动和直接相关文档。
2. 在同一批实现中消除 FSMManager 原有自动初始化与 FrameworkEntry 接管并存路径。
3. 保留 `FSMManager.Instance`、`Create`、`Destroy`、`GetMachine` 公开 API。
4. 不修改 State、StateBuilder、StateSwitchCondition 公开契约。
5. 不修改 Samples 或业务行为。
6. 不接管 Guide、SoundManager、UI、Network 或 Platform Service。
7. 不引入 Registry、Service 容器、反射扫描或自动发现。
8. 明确 Stop 时如何停止 Update 驱动。
9. 明确 Shutdown 时如何销毁所有状态机、清理条件和当前状态。
10. 完成 Unity Play Mode、重复进入退出、Domain Reload 开关、场景切换、预放置 FSMManager 和状态机创建/销毁验证。

## 11. 推荐实施边界

P3.4E 推荐只做：

* FSMManager 内部增加 Register、Initialize、Start、Stop、Shutdown 对应的非公开生命周期入口。
* FrameworkEntry 固定编排 FSMManager，顺序位于 ThreadDispatcher 与 EventManager 之后。
* 移除或失效 FSMManager 原有独立 `AfterSceneLoad Init()` 自动创建路径。
* 保留 `FSMManager.Instance` 作为兼容访问。
* Stop 阶段停止 Update 驱动。
* Shutdown 阶段逆序销毁所有 StateMachine。
* StateMachine 销毁时补齐条件清理与当前状态清理边界。
* 补充 Unity 验证文档。

P3.4E 禁止：

* 修改 FSM 公开 API。
* 修改业务状态机逻辑。
* 接管其它模块。
* 引入 IServiceRegistry 或 Service 容器。
* 将 FSMManager 变成业务状态容器、事件中心或 Service Locator。

## 12. 审查结论

FSMManager 属于 Core Service，适合由 FrameworkEntry 编排；但它持有运行时状态机、状态对象和业务委托引用，比 ThreadDispatcher 和 EventManager 更需要关注 Shutdown 清理。

P3.4E 可以开始窄范围实现，但实现重点必须是“单一生命周期权威 + 停止 Update + 状态机清理边界”，不是重构 FSM 模型。

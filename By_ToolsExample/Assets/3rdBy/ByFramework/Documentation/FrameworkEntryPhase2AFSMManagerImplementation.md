# P3.4E FrameworkEntry Phase2A FSMManager Implementation

## 1. 实施目标

P3.4E 在 P3.4D FSMManager Integration Review 完成后，进行 FSMManager 的窄范围生命周期接管验证。

本次目标只验证：

* FSMManager 可以由 FrameworkEntry 编排。
* FSMManager 生命周期可以映射到 Register、Initialize、Start、Stop、Shutdown。
* FrameworkEntry 成为 FSMManager 生命周期唯一编排权威。
* 保留 `FSMManager.Instance`、`Create`、`Destroy`、`GetMachine` 等现有公开 API。
* 不引入 Service Registry、Service 容器、自动发现或反射扫描。
* 不接管 EventManager、ThreadDispatcher 或其它 Platform / FeatureModule。

## 2. 修改文件列表

代码修改：

* `Core/FrameworkEntry.cs`
* `FSM/FSMManager.cs`
* `FSM/StateMachine.cs`

文档修改：

* `Documentation/FrameworkEntryPhase2AFSMManagerImplementation.md`
* `Documentation/Architecture.md`
* `Documentation/Roadmap.md`
* `Documentation/Todo.md`
* `Documentation/Changelog.md`

## 3. 生命周期映射方案

### Register

FrameworkEntry 在 Core 初始化阶段固定调用：

```text
FSMManager.RegisterForFrameworkEntry()
```

该阶段只标记 FSMManager 已纳入 FrameworkEntry 编排，不创建状态机、不创建业务状态、不访问业务对象。

### Initialize

FrameworkEntry 在 Core Start 阶段、EventManager 启动后调用：

```text
FSMManager.InitializeForFrameworkEntry(frameworkRoot)
```

Initialize 负责：

* 校验 Register 已完成。
* 优先复用场景中已有 FSMManager。
* 无实例时创建 `[FSM]` 节点。
* 有 FrameworkEntry 根节点时挂载到 `[ByFramework]` 下。
* 保持 `FSMManager.Instance` 指向当前实例。

### Start

FrameworkEntry 调用：

```text
FSMManager.StartForFrameworkEntry()
```

Start 负责启用 FSMManager 的每帧状态机驱动。`Update()` 在未 Start 时直接返回，避免未启动或 Stop 后继续执行状态切换。

### Stop

FrameworkEntry Shutdown 时先调用：

```text
FSMManager.StopForFrameworkEntry()
```

Stop 负责停止 `Update()` 驱动，不销毁状态机，不清理列表。

### Shutdown

FrameworkEntry 在 EventManager 和 ThreadDispatcher 之前调用：

```text
FSMManager.ShutdownForFrameworkEntry()
```

Shutdown 负责：

* 停止 FSMManager。
* 逆序销毁 `_machines` 中所有 StateMachine。
* 清空 `_machines`。
* 清理 `FSMManager.Instance`。
* 重置 FSMManager 内部生命周期标记。

## 4. 初始化顺序

P3.4E 后 Core 编排顺序为：

```text
FrameworkEntry
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

该顺序保持 P3.4D 冻结结论：

```text
ThreadDispatcher -> EventManager -> FSMManager
```

FSMManager 当前不直接依赖 ThreadDispatcher 或 EventManager；顺序只表达 Core 生命周期可用性，不引入代码依赖。

## 5. 销毁顺序

P3.4E 后 Shutdown 顺序为：

```text
FrameworkEntry Shutdown
  -> Stop FSMManager
  -> Shutdown FSMManager
  -> Stop EventManager
  -> Shutdown EventManager
  -> Stop ThreadDispatcher
  -> Shutdown ThreadDispatcher
```

该顺序保持 P3.4D 冻结结论：

```text
FSMManager -> EventManager -> ThreadDispatcher
```

FSMManager 先关闭，可以优先释放业务状态委托与状态机引用；EventManager 保留到 FSMManager 之后，为后续状态退出事件通知保留扩展空间。

## 6. 双权威风险处理

P3.4E 已移除 FSMManager 原有 `AfterSceneLoad Init()` 自动初始化入口。

新的生命周期权威为：

```text
FrameworkEntry -> FSMManager.*ForFrameworkEntry()
```

保留的兼容访问方式为：

```text
FSMManager.Instance
```

`Instance` 只作为访问入口，不再负责触发生命周期创建或启动。

本阶段没有新增公开 `EnsureInstance()`，避免引入新的隐式创建入口。

## 7. Shutdown 清理说明

FSMManager Shutdown 覆盖：

* `_machines`
* 每个 StateMachine 的 `CurrentState`
* 每个 StateMachine 的状态列表
* 每个 StateMachine 的 `conditions`
* State 上的生命周期委托缓存
* State 到 StateMachine 的反向引用
* `FSMManager.Instance`

StateMachine `OnDestroy()` 的清理顺序为：

```text
CurrentState.OnExit()
CurrentState = null
for each state reverse:
  state.OnTermination()
  state.machine = null
  clear state delegates
  remove state
states.Clear()
conditions.Clear()
```

该清理只发生在状态机销毁边界，不改变正常状态切换、创建和查询 API。

## 8. 兼容 API 保留说明

本次未修改公开 API：

* `FSMManager.Instance`
* `FSMManager.Create<T>(string stateMachineName)`
* `FSMManager.Destroy(string stateMachineName)`
* `FSMManager.Destroy<T>(T stateMachine)`
* `FSMManager.GetMachine<T>(string stateMachineName)`
* `StateMachine.Add`
* `StateMachine.Remove`
* `StateMachine.Switch`
* `StateMachine.Switch2Next`
* `StateMachine.Switch2Last`
* `StateMachine.Switch2Null`
* `StateMachine.Build`
* `StateMachine.SwitchWhen`

业务代码仍按原方式通过 `FSMManager.Instance` 获取管理器并创建、销毁或查询状态机。

## 9. 风险评估

已降低风险：

* 旧 `AfterSceneLoad Init()` 与 FrameworkEntry 的双生命周期权威风险。
* Stop 后继续 `Update()` 状态机的风险。
* Shutdown 后 `_machines` 残留风险。
* `conditions` 委托闭包跨 PlayMode 残留风险。
* `CurrentState` 未退出和未置空风险。
* State 生命周期委托缓存残留风险。

保留风险：

* Unity Domain Reload 关闭、多次进入退出 Play Mode 仍需要 Unity 实机验证。
* 场景中预放置多个 FSMManager 时，当前仍只复用 Unity 查找到的第一个实例，本阶段不新增重复实例清理策略。
* `Update()` 遍历期间业务代码修改 `_machines` 的时序风险仍存在，本阶段不重构更新队列。
* `StateBuilder.Complete()` 与 `StateMachine.Add()` 的初始化语义差异仍保留，本阶段不改变构建模型。
* `StateMachine.OnUpdate()` 中带 `sourceStateName` 的条件在 `CurrentState == null` 时仍存在空引用风险，本阶段未修改正常运行逻辑。

## 10. 回滚方案

如需回滚 P3.4E：

1. 从 FrameworkEntry 移除 FSMManager 的 Register、Initialize、Start、Stop、Shutdown 编排调用。
2. 在 FSMManager 恢复旧 `AfterSceneLoad Init()` 自动初始化入口。
3. 移除 FSMManager 内部 `_isRegistered`、`_isInitialized`、`_isStarted` 标记及 `*ForFrameworkEntry()` 入口。
4. 移除 `Update()` 对 `_isStarted` 的保护。
5. 如需完全回到旧状态，可还原 StateMachine `OnDestroy()` 中新增的 `CurrentState`、`conditions` 和 State 委托清理逻辑。

回滚后会重新引入 P3.4D 已识别的双权威与 Shutdown 残留风险。

## 11. Unity 验证步骤

建议在 Unity 中逐项验证：

1. Domain Reload 开启时进入 Play Mode，确认 `[ByFramework]` 下出现 `[FSM]`，且无重复 FSMManager。
2. Domain Reload 关闭时多次进入和退出 Play Mode，确认 `FSMManager.Instance` 不残留旧状态机。
3. 场景切换后确认 FSMManager 仍由 FrameworkEntry 管理，状态机可正常创建和销毁。
4. 在首场景预放置 FSMManager，确认 FrameworkEntry 复用该实例。
5. 运行时通过 `FSMManager.Instance.Create<T>()` 创建状态机，确认状态 `OnStay` 在 Start 后正常执行。
6. 退出 Play Mode 或销毁 FrameworkEntry，确认 StateMachine 执行 `OnExit`、`OnTermination` 并清理条件委托。
7. 反复创建和销毁同名状态机，确认公开 API 行为保持不变。
8. 与 EventManager 联合验证，确认 FSMManager 关闭早于 EventManager。

## 12. 实施结论

P3.4E 已完成 FSMManager 窄范围生命周期接管试点。

FrameworkEntry 现在可以按以下顺序编排 Core Early 模块：

```text
ThreadDispatcher -> EventManager -> FSMManager
```

Shutdown 按严格逆序执行：

```text
FSMManager -> EventManager -> ThreadDispatcher
```

本次未引入 Registry、Service 容器、自动发现或反射扫描，未修改 FSM 公开 API，未接管其它模块。

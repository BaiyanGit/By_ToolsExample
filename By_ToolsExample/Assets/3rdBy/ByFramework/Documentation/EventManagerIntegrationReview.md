# P3.4B EventManager Integration Review

## 1. 审查目标与范围

P3.4B 在 P3.4A ThreadDispatcher 试点成功后，评审 EventManager 是否具备进入 FrameworkEntry 生命周期编排条件。

本阶段只更新文档：

* 不修改 Runtime、Editor 或 API。
* 不开始 EventManager 接管。
* 不引入 Registry、Service 容器、自动发现或反射扫描。
* 不修改 Samples 行为。

审查范围：

* FrameworkEntry
* EventManager
* AEvent
* AEventAsync
* AEventClass
* EventTypePool
* EventManager Samples

## 2. 当前状态分析

### 2.1 EventManager 当前生命周期

EventManager 当前是 `MonoBehaviour` 单例，运行时存在一个 `EventManager.Instance`。

当前生命周期事实：

* `AfterSceneLoad` 自动调用 `Init()`。
* `Init()` 调用 `EnsureInstance()`。
* `EnsureInstance()` 在没有实例时创建 `[Event]` GameObject，并挂载 EventManager。
* 若存在 `[ByFramework]`，新建 `[Event]` 会挂到该节点下。
* `EnsureInstance()` 会调用 `LoadAll()` 扫描声明式事件处理器。
* `DontDestroyOnLoad(container)` 保持节点跨场景存活。
* `SubsystemRegistration` 只重置 `Instance = null`。
* `OnDestroy()` 只在当前实例销毁时清理 `Instance`。

当前没有显式 Stop 或 Shutdown 阶段，也没有统一清理声明式事件表、运行时监听表和 EventTypePool 的生命周期入口。

### 2.2 当前初始化入口

当前存在两个初始化入口：

* 自动入口：`RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)`。
* 兼容入口：`EventManager.EnsureInstance()`。

Samples 中 `EventManagerRuntimeListenerExample.RunValidation()` 会主动调用 `EventManager.EnsureInstance()`，因此后续接管不能删除该公开方法或改变 Samples 行为。

### 2.3 当前销毁逻辑

当前销毁逻辑只有：

```text
OnDestroy
  -> if Instance == this
     -> Instance = null
```

未清理内容：

* `_allEventTypes`
* `_allEvents`
* `_runtimeEventTable`
* `EventTypePool.Instance`
* 已注册但未移除的 Runtime Listener 委托引用

### 2.4 AEvent / AEventAsync / AEventClass

三类声明式事件处理器均通过 `[Event]` 标记，并在 `LoadAll()` 中由反射扫描实例化。

当前行为：

* `AEvent<T>` 处理 Struct 同步事件。
* `AEventAsync<T>` 处理 Struct 异步事件。
* `AEventClass<T>` 处理 Class 事件。
* 处理器实例由 EventManager 在 Initialize 语义中创建并保存在 `_allEvents`。

接入 FrameworkEntry 时，声明式处理器扫描和实例化应继续归属 EventManager 内部实现，不应上移到 FrameworkEntry。

### 2.5 EventTypePool

EventTypePool 是纯 C# 静态对象池：

```text
public static EventTypePool Instance = new EventTypePool();
```

风险：

* `SubsystemRegistration` 当前不会重建或清空 `EventTypePool.Instance`。
* 关闭 Domain Reload 时，池中对象可能跨 Play Session 残留。
* `Dispose()` 只清空池内容，不重置静态 Instance。
* 当前没有 EventManager Shutdown 调用 EventTypePool 清理。

## 3. 生命周期映射建议

建议 EventManager 接入 FrameworkEntry 时映射为：

| 阶段 | 建议职责 | 禁止 |
| --- | --- | --- |
| Register | FrameworkEntry 固定注册 EventManager 试点对象 | 创建 `[Event]`、扫描处理器、访问业务事件 |
| Initialize | EventManager 自行复用或创建实例，加载声明式事件处理器，初始化事件表 | 开始接受业务广播、修改 Samples 行为 |
| Start | 标记事件系统可接受 Publish、Broadcast、AddListener、RemoveListener | 重新扫描处理器、重复创建实例 |
| Stop | 停止接受新的业务事件请求，并隔离关闭期调用 | 销毁实例、清理静态状态 |
| Shutdown | 清理 `_runtimeEventTable`、`_allEvents`、`_allEventTypes`、`EventTypePool`，释放 `Instance` | 保留监听委托或对象池状态到下一次 Play Session |

P3.4C 仍应采用窄范围内部生命周期钩子，不实现通用 Registry。

## 4. 初始化顺序建议

建议顺序：

```text
FrameworkEntry BeforeSceneLoad
  -> Register ThreadDispatcher
  -> Register EventManager

FrameworkEntry AfterSceneLoad
  -> Initialize ThreadDispatcher
  -> Start ThreadDispatcher
  -> Initialize EventManager
  -> Start EventManager
```

理由：

* EventManager 当前实际自动入口是 `AfterSceneLoad`，保持该时点可以降低行为变化风险。
* ThreadDispatcher 已由 P3.4A 接管，应先于 EventManager 完成 Start。
* EventManager 当前不依赖 ThreadDispatcher，但后续异步事件、网络、任务调度或主线程派发场景可能需要 ThreadDispatcher 已可用。
* EventManager 应先于 FSMManager 接管，因为 FSMManager 未来可能消费事件能力。

## 5. 销毁顺序建议

建议销毁顺序严格逆初始化顺序：

```text
FrameworkEntry Shutdown
  -> Stop EventManager
  -> Shutdown EventManager
  -> Stop ThreadDispatcher
  -> Shutdown ThreadDispatcher
```

理由：

* EventManager 是上层 Core Service，ThreadDispatcher 是更底层的 Core Early 能力。
* 如果 EventManager Stop 或 Shutdown 内部未来需要主线程派发或异步排空，应在 ThreadDispatcher 仍可用时完成。
* EventManager Shutdown 后不应再允许业务广播或新增监听。

## 6. 与 ThreadDispatcher 的关系

当前 EventManager 代码没有直接调用 DispatcherThread。

接入建议：

* ThreadDispatcher 不依赖 EventManager。
* EventManager 可以在生命周期顺序上位于 ThreadDispatcher 之后。
* EventManager 不应在 P3.4C 中引入对 ThreadDispatcher 的强代码依赖，除非实际实现需要主线程调度。
* 如未来异步事件需要主线程回调，应通过明确契约处理，而不是在 EventManager 内部隐式调用 DispatcherThread。

## 7. 与 FSMManager 的关系

当前审查未发现 EventManager 直接依赖 FSMManager。

建议关系：

* EventManager 先于 FSMManager Initialize 和 Start。
* FSMManager 未来可以消费 EventManager，但 EventManager 不应反向依赖 FSMManager。
* P3.4C 不接管 FSMManager，不修改 FSMManager。
* EventManager 接管验证通过后，才能进入 FSMManager Integration Review。

## 8. Singleton / Instance 关系

`EventManager.Instance` 当前是主要公开访问方式，必须保留。

P3.4C 建议：

* FrameworkEntry 是唯一生命周期编排权威。
* `Instance` 只作为兼容访问入口。
* `EnsureInstance()` 保留公开 API，但不得继续成为独立生命周期权威。
* 原有 `AfterSceneLoad` 自动初始化入口必须在同一批实现中移除或改由 FrameworkEntry 触发。
* `EnsureInstance()` 在未初始化阶段的行为需要谨慎设计：可以兼容创建，但必须不绕开 Register / Initialize / Start 状态，避免双权威。

## 9. 接入风险清单

### 高风险

1. 旧 `AfterSceneLoad Init()` 与 FrameworkEntry 同时初始化，形成双重生命周期权威。
2. `EnsureInstance()` 保留隐式创建能力，绕开 FrameworkEntry 生命周期状态。
3. `_runtimeEventTable` 未在 Shutdown 清理，导致监听委托引用跨场景或跨 Play Session 残留。
4. `EventTypePool.Instance` 在关闭 Domain Reload 时残留对象池状态。
5. `LoadAll()` 重复执行导致 `_allEvents` 中声明式处理器重复注册。

### 中风险

1. 声明式事件处理器扫描使用程序集反射，但这是 EventManager 既有内部行为；P3.4C 不应扩大为 FrameworkEntry 或 Registry 的自动发现机制。
2. `PublishClass<T>` 会在发布后 Dispose 事件对象，Shutdown 期间若仍允许业务发布，可能产生难以追踪的释放时序。
3. `RemoveListenerInternal()` 对不存在事件抛异常，Shutdown 清理若与业务移除监听交错，可能出现异常。
4. 场景中预放置 EventManager 时，当前 `EnsureInstance()` 不复用场景实例，后续需要决定是否保持旧行为或补复用策略。
5. `DontDestroyOnLoad` 与 `SetParent([ByFramework])` 同时使用的层级和持久化行为需要 Unity 验证。

### 保留风险

* EventManager README 仍包含部分 `_3rdBy.MetaFramework` 示例 using，属于命名空间兼容和文档迁移问题，不属于 P3.4C 接管必改项。
* 当前 P3.4、P3.4A 若干独立文档在工作区缺失，但 Architecture、Roadmap、Todo 与 Changelog 已记录阶段结论；该文档漂移需要后续专项整理。

## 10. 是否建议进入 P3.4C

建议进入：

> P3.4C EventManager Narrow Implementation

但必须限定为窄范围实现，且只在以下前置条件满足时开始：

1. 明确允许修改的文件仅限 EventManager、FrameworkEntry 最小必要改动和直接相关文档。
2. 在同一批实现中消除 EventManager 原有自动初始化与 FrameworkEntry 接管并存路径。
3. 保留 `EventManager.Instance`、`EnsureInstance()`、`Publish`、`PublishAsync`、`PublishClass`、`AddListener`、`Broadcast`、`RemoveListener` 公开 API。
4. 不修改 AEvent、AEventAsync、AEventClass 的公开契约。
5. 不修改 Samples 行为。
6. 不接管 FSMManager。
7. 不引入 Registry、Service 容器、反射扫描扩展或 Platform Service。
8. 明确 EventTypePool 的 Reset / Shutdown 策略。
9. 明确 `_runtimeEventTable` 与 `_allEvents` 的 Shutdown 清理策略。
10. 完成 Unity Play Mode、重复进入退出、Domain Reload 开关、Samples 验证和场景切换验证。

## 11. 推荐实施边界

P3.4C 推荐只做：

* EventManager 内部增加 Register、Initialize、Start、Stop、Shutdown 对应的非公开生命周期入口。
* FrameworkEntry 固定编排 EventManager，顺序位于 ThreadDispatcher 之后、FSMManager 之前。
* 移除或失效 EventManager 原有独立 `AfterSceneLoad Init()` 自动创建路径。
* 保留 `EnsureInstance()` 作为兼容访问，不改变调用方公开 API。
* 在 Shutdown 中清理运行时监听、声明式事件表和 EventTypePool。
* 补充 Unity 验证文档。

P3.4C 禁止：

* 修改 EventManager Samples 行为。
* 修改 AEvent / AEventAsync / AEventClass 公开契约。
* 接管 FSMManager 或其它模块。
* 引入 IServiceRegistry 或 Service 容器。
* 将 EventManager 变成业务状态容器或 Service Locator。

## 12. 审查结论

EventManager 属于 Core Service，适合由 FrameworkEntry 编排；但它比 ThreadDispatcher 更复杂，因为它持有声明式处理器表、运行时监听表和 EventTypePool 静态对象池。

P3.4C 可以开始窄范围实现，但实现重点必须是“单一生命周期权威 + 清理边界”，不是重构事件模型。

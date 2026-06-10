# P3.4C FrameworkEntry Phase2A EventManager Narrow Implementation

## 1. 实施范围

P3.4C 只验证 `FrameworkEntry + EventManager` 生命周期编排是否可以安全落地。

本次未引入 Registry、Service 容器、自动发现或新的反射扫描，也未接管 FSMManager 或任何 Platform Service。

允许修改范围：

* FrameworkEntry 最小必要编排逻辑。
* EventManager 内部生命周期入口。
* EventTypePool 静态清理能力。
* 直接相关文档。

## 2. 修改文件列表

代码：

* `Core/FrameworkEntry.cs`
* `EventManager/Core/EventManager.cs`
* `EventManager/Core/EventManager.RuntimeListener.cs`
* `EventManager/Core/EventTypePool.cs`

文档：

* `Documentation/FrameworkEntryPhase2AEventManagerImplementation.md`
* `Documentation/Architecture.md`
* `Documentation/Roadmap.md`
* `Documentation/Todo.md`
* `Documentation/Changelog.md`

## 3. 生命周期映射方案

| 阶段 | FrameworkEntry 编排行为 | EventManager 行为 |
| --- | --- | --- |
| Register | Core 初始化阶段固定注册 EventManager | 记录已注册状态，不创建实例 |
| Initialize | `AfterSceneLoad` 中在 ThreadDispatcher 后调用 | 优先复用场景已有 EventManager；无实例时创建 `[Event]`；加载声明式事件处理器 |
| Start | Initialize 后调用 | 标记 EventManager 进入可用状态，不改变公开事件 API |
| Stop | FrameworkEntry Shutdown 时先于 ThreadDispatcher Stop | 标记 EventManager 停止生命周期运行状态 |
| Shutdown | Stop 后调用 | 清理事件缓存、运行时监听表、EventTypePool，并释放 `Instance` |

实际顺序：

```text
Register:
  ThreadDispatcher
  EventManager

Start:
  ThreadDispatcher.Initialize
  ThreadDispatcher.Start
  EventManager.Initialize
  EventManager.Start

Shutdown:
  EventManager.Stop
  EventManager.Shutdown
  ThreadDispatcher.Stop
  ThreadDispatcher.Shutdown
```

## 4. 修改内容说明

### FrameworkEntry

* 在 Core 初始化阶段固定注册 EventManager。
* 在 `AfterSceneLoad` 启动 ThreadDispatcher 后，继续 Initialize / Start EventManager。
* 在 Shutdown 中按逆序先关闭 EventManager，再关闭 ThreadDispatcher。
* 未接管 FSMManager。
* 未引入 Registry 或 Service 容器。

### EventManager

* 移除原有独立 `AfterSceneLoad Init()` 自动初始化入口。
* 新增内部 Register、Initialize、Start、Stop、Shutdown 生命周期入口。
* Initialize 阶段优先复用场景已有 EventManager。
* 无实例时仍按兼容逻辑创建 `[Event]`。
* `LoadAll()` 在加载声明式事件处理器前清理旧表，避免重复注册。
* `OnDestroy()` 复用 Shutdown 清理路径。

### EventManager.RuntimeListener

* 保留 `EnsureInstance()` 公开 API。
* `EnsureInstance()` 不再直接手写创建逻辑，而是进入同一套 Register / Initialize / Start 内部生命周期。
* 新增运行时监听表清理入口。

### EventTypePool

* 新增内部静态重置能力。
* EventManager `SubsystemRegistration` 和 Shutdown 均会清理并重建 `EventTypePool.Instance`。

## 5. 双权威风险处理说明

已处理：

* EventManager 原有 `AfterSceneLoad Init()` 自动入口已移除。
* FrameworkEntry 成为 EventManager 正常启动路径的唯一生命周期编排入口。
* `EnsureInstance()` 保留，但不再维护独立创建逻辑，改为调用同一套内部生命周期入口。

兼容路径：

* 如果外部代码在 FrameworkEntry 编排前调用 `EnsureInstance()`，它会执行兼容 Register / Initialize / Start。
* 该路径用于保留现有 API 和 Samples 行为，不作为推荐生命周期入口。
* 后续验证应确认业务代码不会长期依赖该兼容路径绕过 FrameworkEntry。

## 6. Shutdown 清理说明

Shutdown 覆盖：

* `_runtimeEventTable`
* `_allEvents`
* `_allEventTypes`
* `EventTypePool.Instance`
* `EventManager.Instance`

清理目标：

* 避免 Runtime Listener 委托引用残留。
* 避免声明式事件处理器重复注册。
* 避免 EventTypePool 在关闭 Domain Reload 时跨 Play Session 保留对象。
* 避免 EventManager 静态 Instance 指向已销毁对象。

## 7. 兼容 API 保留说明

公开 API 保持不变：

* `EventManager.Instance`
* `EventManager.EnsureInstance()`
* `Publish`
* `PublishAsync`
* `PublishClass`
* `AddListener`
* `Broadcast`
* `RemoveListener`
* `EventTypePool.Instance`
* AEvent / AEventAsync / AEventClass 公开契约

Samples 行为不修改。

## 8. 风险评估

### 高风险

1. 外部代码过早调用 `EnsureInstance()` 仍可能走兼容启动路径，需要 Unity 验证和后续调用审计。
2. 场景预放置多个 EventManager 时，本次只复用第一个，不做重复实例清理重构。
3. Shutdown 后如果业务继续调用 `EventManager.Instance`，行为仍取决于调用方是否检查空引用。

### 中风险

1. `LoadAll()` 仍使用 EventManager 既有程序集扫描方式；本次未新增 FrameworkEntry 或 Registry 级自动发现。
2. `DontDestroyOnLoad` 与父节点挂载在不同 Unity 版本下需要验证层级和跨场景行为。
3. `EnsureInstance()` 兼容路径为了保留 API，会在没有 FrameworkEntry 父节点时创建独立持久对象。
4. 运行时监听表 Shutdown 清理会丢弃未移除监听，符合关闭语义，但需要验证退出期间没有业务继续广播。

## 9. 回滚方案

1. 从 FrameworkEntry 移除 EventManager Register、Initialize、Start、Stop、Shutdown 调用。
2. 从 EventManager 移除内部生命周期状态和入口。
3. 恢复 EventManager 原有 `RuntimeInitializeOnLoadMethod(AfterSceneLoad)` `Init()` 自动初始化入口。
4. 恢复 `EnsureInstance()` 原直接创建 `[Event]`、挂载 `[ByFramework]`、`LoadAll()` 和 `DontDestroyOnLoad` 的逻辑。
5. 移除 EventTypePool 静态重置入口。

回滚时必须保证 EventManager 旧自动初始化与 FrameworkEntry 编排不会同时存在。

## 10. Unity 验证步骤

1. 打开 Unity，确认 Console 无编译错误。
2. 开启 Domain Reload，进入 Play Mode，确认 Hierarchy 中只有一个 `[ByFramework]` 和一个 EventManager 实例。
3. 无预放置实例时，确认自动创建节点名为 `[Event]`。
4. 首场景预放置一个 EventManager，确认复用场景实例且不创建第二个 `[Event]`。
5. 运行 `EventManagerRuntimeListenerExample`，确认 Type / Enum 事件和 RemoveListener 行为保持通过。
6. 运行 `EventCallExample`，确认声明式 Struct 事件仍可发布和处理。
7. 调用 `EventManager.EnsureInstance()`，确认不创建重复实例。
8. 切换场景，确认 EventManager 不重复创建，事件 API 可继续使用。
9. 退出 Play Mode，确认 Shutdown 无空引用和重复清理错误。
10. 连续进入和退出 Play Mode 两次，确认事件表和 EventTypePool 不残留。
11. 关闭 Domain Reload，重复步骤 2 至 10。
12. 场景中预放置多个 EventManager，记录重复实例行为；本次不扩展为重复实例清理重构。

## 11. 实施结论

代码层已完成 EventManager 的窄范围生命周期接管。

已使用 Unity 6000.0.48f1 当前引用集完成窄范围编译，覆盖 FrameworkEntry、ThreadDispatcher、EventManager、EventTypePool 及 EventManager 直接依赖文件，结果为 0 个错误。

当前完整 `Assembly-CSharp` 编译受工作区既有缺失源码与缺失插件引用影响，无法作为本次改动的有效验证信号；这些缺失项不属于 P3.4C 修改范围。

进入 FSMManager 接管前，必须先完成上述 Unity 验证并记录结果；未验证通过前不得进入下一模块接管。

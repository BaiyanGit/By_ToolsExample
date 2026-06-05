# ByFramework Architecture

## 事件系统

* 最终只保留 EventManager
* 旧事件中心兼容层已移除

EventManager 支持：

* Struct事件
* Class事件
* Async事件
* Type事件
* Enum事件

Runtime Listener 对外 API 保持统一命名：

* 添加监听：`AddListener`
* 广播事件：`Broadcast`
* 移除监听：`RemoveListener`

Type 事件使用 `Type` 本身作为 Key：

```csharp
EventManager.Instance.AddListener(typeof(PlayerDeadEvent), OnPlayerDead);
EventManager.Instance.Broadcast(typeof(PlayerDeadEvent));
EventManager.Instance.RemoveListener(typeof(PlayerDeadEvent), OnPlayerDead);
```

Enum 事件使用枚举实例本身作为 Key，不使用 `eventId.GetType()`：

```csharp
EventManager.Instance.AddListener(EventNotice.OpenBag, OnOpenBag);
EventManager.Instance.Broadcast(EventNotice.OpenBag);
EventManager.Instance.RemoveListener(EventNotice.OpenBag, OnOpenBag);
```

带参数事件同样使用同名重载：

```csharp
EventManager.Instance.AddListener<int>(EventNotice.HpChange, OnHpChange);
EventManager.Instance.Broadcast(EventNotice.HpChange, 100);
EventManager.Instance.RemoveListener<int>(EventNotice.HpChange, OnHpChange);
```

原有框架事件接口保持不改名：

* `Publish`
* `PublishAsync`
* `PublishClass`

## UI系统

* 当前使用 Resources
* 后续增加 IUILoader

## 框架入口

* 后续增加 FrameworkEntry

## 配置系统

* 后续增加 FrameworkConfig

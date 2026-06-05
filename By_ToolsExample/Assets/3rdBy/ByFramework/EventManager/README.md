# EventManager

EventManager 是 ByFramework 的统一事件入口，负责两类事件：

* 框架声明式事件：`Publish`、`PublishAsync`、`PublishClass`
* 运行时监听事件：`AddListener`、`Broadcast`、`RemoveListener`

旧事件中心兼容层已移除，新代码和旧代码迁移后都应统一使用 `EventManager.Instance`。

## Struct 事件

Struct 事件适合传递轻量值类型数据。处理器继承 `AEvent<T>`，发布方调用 `Publish`。

```csharp
using _3rdBy.MetaFramework.EventNotice;
using UnityEngine;

public struct PlayerDeadEvent
{
    public int playerId;
}

public class PlayerDeadEventHandler : AEvent<PlayerDeadEvent>
{
    protected override void Run(PlayerDeadEvent a)
    {
        Debug.Log(a.playerId);
    }
}

EventManager.Instance.Publish(new PlayerDeadEvent
{
    playerId = 1001,
});
```

## Async 事件

Async 事件适合需要等待异步流程完成的 Struct 事件。处理器继承 `AEventAsync<T>`，发布方调用 `PublishAsync`。

```csharp
using _3rdBy.MetaFramework.EventNotice;
using Cysharp.Threading.Tasks;
using UnityEngine;

public struct LoadFinishedEvent
{
    public string sceneName;
}

public class LoadFinishedEventHandler : AEventAsync<LoadFinishedEvent>
{
    protected override async UniTask Run(LoadFinishedEvent a)
    {
        await UniTask.Yield();
        Debug.Log(a.sceneName);
    }
}

await EventManager.Instance.PublishAsync(new LoadFinishedEvent
{
    sceneName = "Main",
});
```

## Class 事件

Class 事件适合需要引用类型数据的场景。当前 `PublishClass<T>` 要求事件数据实现 `IDisposable`，发布后会调用 `Dispose`。

```csharp
using System;
using _3rdBy.MetaFramework.EventNotice;
using UnityEngine;

public sealed class OpenWindowEvent : IDisposable
{
    public string windowName;

    public void Dispose()
    {
    }
}

public class OpenWindowEventHandler : AEventClass<OpenWindowEvent>
{
    protected override void Run(object a)
    {
        var eventData = (OpenWindowEvent)a;
        Debug.Log(eventData.windowName);
    }
}

EventManager.Instance.PublishClass(new OpenWindowEvent
{
    windowName = "Bag",
});
```

## Type 事件

Type 事件使用 `Type` 本身作为 Key。用法与 `EventManagerRuntimeListenerExample.cs` 中的 `TypeNoArgEvent`、`TypeIntEvent` 保持一致。

```csharp
private sealed class TypeNoArgEvent
{
}

private void OnTypeNoArgEvent()
{
}

EventManager.Instance.AddListener(typeof(TypeNoArgEvent), OnTypeNoArgEvent);
EventManager.Instance.Broadcast(typeof(TypeNoArgEvent));
EventManager.Instance.RemoveListener(typeof(TypeNoArgEvent), OnTypeNoArgEvent);
```

## Enum 事件

Enum 事件使用枚举实例本身作为 Key，不能使用 `eventId.GetType()`。这样可以确保同一个枚举类型下的不同枚举值不会互相触发。

```csharp
private enum EventNotice
{
    A,
    B,
    HpChange,
}

private void OnEnumAEvent()
{
}

EventManager.Instance.AddListener(EventNotice.A, OnEnumAEvent);
EventManager.Instance.Broadcast(EventNotice.A);
EventManager.Instance.RemoveListener(EventNotice.A, OnEnumAEvent);
```

`EventNotice.A` 与 `EventNotice.B` 是两个不同事件：

```csharp
EventManager.Instance.AddListener(EventNotice.A, OnEnumAEvent);
EventManager.Instance.AddListener(EventNotice.B, OnEnumBEvent);

EventManager.Instance.Broadcast(EventNotice.A);
```

上例只会触发 `OnEnumAEvent`，不会触发 `OnEnumBEvent`。

## 参数事件

Runtime Listener 支持 0 到 5 个参数，Type 事件和 Enum 事件都使用同名重载。

### 0 参数

```csharp
EventManager.Instance.AddListener(EventNotice.A, OnEnumAEvent);
EventManager.Instance.Broadcast(EventNotice.A);
EventManager.Instance.RemoveListener(EventNotice.A, OnEnumAEvent);
```

### 1 参数

```csharp
private void OnHpChange(int value)
{
}

EventManager.Instance.AddListener<int>(EventNotice.HpChange, OnHpChange);
EventManager.Instance.Broadcast(EventNotice.HpChange, 100);
EventManager.Instance.RemoveListener<int>(EventNotice.HpChange, OnHpChange);
```

### 2 参数

```csharp
private void OnHpChange(int value, string reason)
{
}

EventManager.Instance.AddListener<int, string>(EventNotice.HpChange, OnHpChange);
EventManager.Instance.Broadcast(EventNotice.HpChange, 100, "damage");
EventManager.Instance.RemoveListener<int, string>(EventNotice.HpChange, OnHpChange);
```

### 3 参数

```csharp
private void OnHpChange(int value, string reason, bool isCritical)
{
}

EventManager.Instance.AddListener<int, string, bool>(EventNotice.HpChange, OnHpChange);
EventManager.Instance.Broadcast(EventNotice.HpChange, 100, "damage", true);
EventManager.Instance.RemoveListener<int, string, bool>(EventNotice.HpChange, OnHpChange);
```

### 4 参数

```csharp
private void OnHpChange(int value, string reason, bool isCritical, float delay)
{
}

EventManager.Instance.AddListener<int, string, bool, float>(EventNotice.HpChange, OnHpChange);
EventManager.Instance.Broadcast(EventNotice.HpChange, 100, "damage", true, 1.5f);
EventManager.Instance.RemoveListener<int, string, bool, float>(EventNotice.HpChange, OnHpChange);
```

### 5 参数

```csharp
private void OnHpChange(int value, string reason, bool isCritical, float delay, long sourceId)
{
}

EventManager.Instance.AddListener<int, string, bool, float, long>(EventNotice.HpChange, OnHpChange);
EventManager.Instance.Broadcast(EventNotice.HpChange, 100, "damage", true, 1.5f, 999L);
EventManager.Instance.RemoveListener<int, string, bool, float, long>(EventNotice.HpChange, OnHpChange);
```

## 推荐使用规范

* 新代码统一使用 `EventManager.Instance`。
* 添加监听使用 `AddListener`。
* 广播事件使用 `Broadcast`。
* 移除监听使用 `RemoveListener`。
* Type 事件使用 `typeof(SomeEvent)` 作为 Key。
* Enum 事件使用具体枚举值作为 Key，例如 `EventNotice.OpenBag`。
* 不要使用 `eventId.GetType()` 作为 Enum 事件 Key。
* 对生命周期明确的监听，必须在合适时机调用 `RemoveListener`。
* 声明式 Struct/Class/Async 事件继续使用 `Publish`、`PublishAsync`、`PublishClass`。
* Runtime Listener 的最小验证示例见 `EventManager/Samples/EventManagerRuntimeListenerExample.cs`。

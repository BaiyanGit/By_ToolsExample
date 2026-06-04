# EventCenter使用说明（README）

## 简介

EventCenter 是一个同时支持 **Type事件** 和 **Enum事件** 的统一事件中心。

### 推荐

- 框架层、业务层：使用 Type 事件
- UI层、界面通知：使用 Enum 事件

---

## Type事件

### 定义事件

```csharp
public class PlayerDeadEvent
{
}
```

### 注册

```csharp
EventCenter.AddTypeListener(typeof(PlayerDeadEvent),OnPlayerDead);
```

### 广播

```csharp
EventCenter.BroadcastType(typeof(PlayerDeadEvent));
```

### 注销

```csharp
EventCenter.RemoveTypeListener(typeof(PlayerDeadEvent),OnPlayerDead);
```

---

## 泛型Type事件（推荐）

```csharp
public class PlayerDeadEvent
{
}
```

### 注册

```csharp
EventCenter.AddTypeListener<PlayerDeadEvent>(OnPlayerDead);
```

### 广播

```csharp
EventCenter.BroadcastType<PlayerDeadEvent>();
```

### 注销

```csharp
EventCenter.RemoveTypeListener<PlayerDeadEvent>(OnPlayerDead);
```

---

## Enum事件

### 定义

```csharp
public enum EventNotice
{
    GameStart,
    GamePause,
    GameOver
}
```

### 注册

```csharp
EventCenter.AddEnumListener(EventNotice.GameStart,OnGameStart);
```

### 广播

```csharp
EventCenter.BroadcastEnum(EventNotice.GameStart);
```

### 注销

```csharp
EventCenter.RemoveEnumListener(EventNotice.GameStart,OnGameStart);
```

---

## 带参数事件

### 一个参数

```csharp
EventCenter.AddEnumListener<int>(EventNotice.PlayerHpChange,OnHpChange);

EventCenter.BroadcastEnum(EventNotice.PlayerHpChange,100);
```

### 两个参数

```csharp
EventCenter.AddEnumListener<int,int>(EventNotice.PlayerMove,OnMove);

EventCenter.BroadcastEnum(EventNotice.PlayerMove,10,20);
```

### 三~五个参数

```csharp
AddEnumListener<T1,T2,T3>()
AddEnumListener<T1,T2,T3,T4>()
AddEnumListener<T1,T2,T3,T4,T5>()

BroadcastEnum<T1,T2,T3>()
BroadcastEnum<T1,T2,T3,T4>()
BroadcastEnum<T1,T2,T3,T4,T5>()
```

---

## 兼容旧代码

以下写法仍然可用：

```csharp
EventCenter.AddListener(...);
EventCenter.RemoveListener(...);
EventCenter.Broadcast(...);
```

---

## 清空所有事件

```csharp
EventCenter.Clear();
```

适用于：

- 场景切换
- 热更新
- Enter PlayMode

---

## 获取事件数量

```csharp
Debug.Log(EventCenter.EventCount);
```

---

## 推荐目录结构

```text
Scripts
│
├─ EventCenter
│   └─ EventCenter_Mixed.cs
│
├─ Events
│   ├─ PlayerDeadEvent.cs
│   ├─ SceneLoadedEvent.cs
│   └─ NetworkConnectedEvent.cs
│
├─ Enums
│   └─ EventNotice.cs
│
└─ UI
└─ UIPanel.cs
```

---

## 最佳实践

```text
框架模块   -> Type事件
业务系统   -> Type事件
UI通知 -> Enum事件
临时功能   -> Enum事件
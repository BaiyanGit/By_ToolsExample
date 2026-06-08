# FrameworkEntry

`FrameworkEntry` 是 ByFramework 的统一启动入口。当前已完成 `ThreadDispatcher`、`EventManager`、`FSMManager` 三个 Core Early 模块的窄范围生命周期接管。

## 当前职责

* 在场景加载前创建 `[ByFramework]` 根节点
* 使用 `DontDestroyOnLoad` 保持根节点跨场景存在
* 防止重复创建 `FrameworkEntry`
* 在 Core Early 阶段编排 `ThreadDispatcher`、`EventManager`、`FSMManager`
* 为后续 Platform Service Registration 保留统一生命周期入口

## 当前冻结生命周期链

初始化顺序：

```text
ThreadDispatcher -> EventManager -> FSMManager
```

Shutdown 顺序：

```text
FSMManager -> EventManager -> ThreadDispatcher
```

生命周期阶段：

```text
Register -> Initialize -> Start -> Stop -> Shutdown
```

## 兼容性原则

* `FrameworkEntry` 是当前 Core Early 生命周期的唯一编排权威
* `DispatcherThread.Current`
* `EventManager.Instance`
* `EventManager.EnsureInstance()`
* `FSMManager.Instance`

以上访问方式仅保留为兼容入口，不再独立拥有生命周期权威。

## 当前未接管范围

以下模块仍不在本阶段接管范围内：

* Guide Dispatcher
* SoundManager
* UIRoot / UIManager
* ResourceSystem
* SaveSystem
* InputSystem
* LocalizationSystem
* DisplaySystem
* LicenseSystem
* NetworkSystem

## Unity 验证结论

项目维护者已在本地 Unity 完成 P3.5A Core Early Lifecycle Unity Verification，结论如下：

```text
FrameworkEntry = 1
ThreadDispatcher = 1
EventManager = 1
FSMManager = 1
```

并已确认：

* 静态访问正常
* 重复实例检查正常
* Core 生命周期链验证通过

详细记录见 `Documentation/18_CoreEarlyLifecycleUnityVerification.md`。

# ByFramework Singleton Audit

## 审计范围

本次审计仅记录当前单例与全局实例实现，不修改业务逻辑、不接管模块，也不改变 FrameworkEntry 行为。

扫描范围：

* `MonoSingletonTemplate`
* `MonoObjSingletonTemplate`
* `SingletonTemplate`
* `static Instance` / `Current`
* `Manager.Instance` 调用
* `DontDestroyOnLoad`

## 单例模板

| 实现 | 类型 | 创建 GameObject | DontDestroyOnLoad | 当前风险 |
| --- | --- | --- | --- | --- |
| `Singleton/MonoSingletonTemplate.cs` | MonoBehaviour 模板 | 否，依赖场景或外部创建 | 是 | `Awake` 直接覆盖 Instance，重复实例不会被销毁 |
| `Singleton/MonoObjSingletonTemplate.cs` | MonoBehaviour 模板 | 是，首次访问 Instance 时创建 | 是 | 隐式创建节点，无法由场景预先配置序列化引用 |
| `Singleton/SingletonTemplate.cs` | 纯 C# 模板 | 否 | 否 | 没有统一销毁、重置和生命周期管理 |

## 当前全局实例

| 实现位置 | 形式 | 创建 GameObject | DontDestroyOnLoad | 后续建议 |
| --- | --- | --- | --- | --- |
| `Core/FrameworkEntry.cs` | 手写 MonoBehaviour Instance | 创建 `[ByFramework]` | 是 | 保持为框架根节点 |
| `EventManager/Core/EventManager.cs` | 手写 MonoBehaviour Instance | 创建 `[Event]` | 是 | 适合后续挂到 `[ByFramework]`，迁移前保持已验证行为 |
| `FSM/FSMManager.cs` | 手写 MonoBehaviour Instance | 创建 `[FSM]` | 是 | 适合后续挂到 `[ByFramework]`，迁移前需补重复保护 |
| `Extension/ThreadDispatcher.cs` | 手写 MonoBehaviour Current | 创建 `Loom` | 是 | 适合后续挂到 `[ByFramework]`，必须保持 BeforeSceneLoad 时机 |
| `Guide/Dispatcher.cs` | 静态类持有内部 MonoBehaviour | 创建 `Dispatcher` | 是 | 可后续统一为框架协程服务，当前先保持独立 |
| `SoundManager/SoundManager.cs` | `MonoObjSingletonTemplate` | 创建 `[Single_SoundManager]` 及音频子节点 | 是 | 可后续接入，但需先确定音频配置与节点生命周期 |
| `UI/UIRoot.cs` | `MonoSingletonTemplate` | 模板不创建；文件内未使用方法可创建 UIRoot | 是 | 暂不接管，依赖场景层级、Canvas、Camera 与 UI 层节点 |
| `Socket/Scripts/ClientManager.cs` | `MonoSingletonTemplate` | 模板不创建 | 是 | 暂不接管，需先确定网络连接生命周期与配置 |
| `Socket/Scripts/BaseNetModel.cs` | `MonoSingletonTemplate` 派生基类 | 模板不创建 | 是 | 暂不接管，属于具体网络业务模型 |
| `Socket/Example/Heartbeat.cs` | `MonoObjSingletonTemplate` | 创建 `[Single_Heartbeat]` | 是 | 示例代码，不应由框架入口接管 |
| `UI/UIManager.cs` | `SingletonTemplate` | 自身不创建；构造时访问 UIRoot | 否 | 纯 C# 服务，不能直接挂节点；需与 UIRoot 一并设计生命周期 |
| `Socket/Scripts/NetEventHandler.cs` | `SingletonTemplate` | 否 | 否 | 纯 C# 网络服务，后续随 Socket 生命周期处理 |
| `EventManager/Core/EventTypePool.cs` | 手写纯 C# Instance | 否 | 否 | EventManager 内部对象池，不应单独挂到框架根节点 |
| `Guide/GuideManager.cs` | 场景 MonoBehaviour static Instance | 否 | 否 | 暂不接管，依赖场景相机、Guide 数据和 UI 引用 |
| `Http/DownLoad/DownloadSystem/Core/DownloadManager.cs` | 场景 MonoBehaviour static Instance | 否 | 否 | 暂不接管，当前依赖场景放置且没有持久化策略 |
| `Extension/ReferenceCollector.cs` | FindObjectOfType 延迟查找 | 否 | 否 | 暂不接管，语义是查找当前场景中的引用收集器 |

## DontDestroyOnLoad 使用位置

当前会主动持久化的实现：

* FrameworkEntry
* EventManager
* FSMManager
* ThreadDispatcher
* Guide Dispatcher
* MonoSingletonTemplate 的所有派生实例
* MonoObjSingletonTemplate 的所有派生实例

`UIRoot.CreateUIRoot()` 内也调用了 `DontDestroyOnLoad`，但该创建方法当前未被实际调用。

## 后续可逐步挂到 [ByFramework]

建议优先级：

1. ThreadDispatcher
2. EventManager
3. FSMManager
4. Guide Dispatcher
5. SoundManager

迁移前必须为目标模块建立幂等初始化、明确销毁行为，并验证原有初始化时机。

## 暂时不应接管

* `GuideManager`：依赖场景相机、Guide 数据和 UI 引用。
* `UIRoot`：依赖预制体或场景中的 Canvas、Camera 与层级节点。
* `ClientManager`、`BaseNetModel<T>`、`NetEventHandler`：需要先定义网络连接与断线生命周期。
* `DownloadManager`：当前为场景组件，尚未定义跨场景持久化策略。
* `ReferenceCollector`：设计目标是查找当前场景引用。
* `Heartbeat`：属于 Socket 示例。
* `EventTypePool`、`UIManager`：纯 C# 实例，不能作为组件直接挂到 `[ByFramework]`。

## 后续重构约束

* Singleton 重构前不得批量替换现有 Instance API。
* 不应仅为统一形式，把场景依赖组件改成自动创建组件。
* 挂到 `[ByFramework]` 前必须先解决重复实例、初始化顺序、退出清理和关闭 Domain Reload 时的静态状态问题。
* FrameworkEntry 接管模块时应逐个迁移并分别完成 Unity 验证。

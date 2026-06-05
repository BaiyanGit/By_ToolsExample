# FrameworkEntry Phase2 Design

## 目标与边界

FrameworkEntry Phase2 的目标是设计统一的框架模块初始化顺序与生命周期边界。

本阶段只确定接管策略，不修改 Runtime、Editor 或 API，不真正接管模块，也不创建新的初始化流程。

FrameworkEntry 后续应作为模块生命周期的编排入口，而不是把所有模块强制改造成同一种单例，也不应接管依赖场景数据的业务组件。

## 设计原则

* 每次只迁移一个模块，完成 Unity 验证后再迁移下一个模块。
* 接管过程中必须保持模块现有公开 API。
* 同一模块在任意迁移阶段只能存在一个初始化权威，避免 FrameworkEntry 与 `RuntimeInitializeOnLoadMethod` 同时创建实例。
* 模块接入前必须具备幂等初始化、重复实例保护、静态引用清理和明确的退出策略。
* `[ByFramework]` 只承载长期存活且业务无关的框架服务。
* 场景引用、连接会话、业务数据和界面层级不应被入口隐式创建。
* Phase2 实施时仍按模块保留独立回滚能力，不进行批量接管。

## 当前模块分类

| 模块 | 当前初始化方式 | 接入结论 | 主要原因 |
| --- | --- | --- | --- |
| `FrameworkEntry` | `BeforeSceneLoad` 自动创建 `[ByFramework]` | 保持入口职责 | 负责提供持久根节点与初始化阶段 |
| `ThreadDispatcher` | `BeforeSceneLoad` 自动创建或复用场景实例 | 第一优先级 | 基础线程调度服务，无场景数据依赖，且其它模块可能较早使用 |
| `EventManager` | `AfterSceneLoad` 自动创建，加载事件处理器 | 第一优先级 | Core 通用服务，适合作为后续模块通信基础 |
| `FSMManager` | `AfterSceneLoad` 自动创建或复用场景实例 | 第一优先级 | Core 通用服务，无场景配置依赖 |
| `Guide Dispatcher` | 静态构造器隐式创建协程节点 | 第二优先级 | 可归入通用协程服务，但缺少显式初始化、重建和退出接口 |
| `SoundManager` | 首次访问 `Instance` 时复用或隐式创建节点 | 第二优先级 | 可长期存活，但当前初始化音频子节点并依赖 Resources |
| `UIRoot` | 场景组件，依赖 Canvas、Camera 与 UI 层级 | 暂不接入 | 必须保留场景或预制体引用语义 |
| `UIManager` | 纯 C# 单例，构造时立即访问 `UIRoot.Instance` | 暂不接入 | 生命周期与 UIRoot、Resources 加载强耦合 |
| `ClientManager` | 场景组件，Awake 注册 Socket 会话 | 暂不接入 | 连接、断线、消息队列和 Heartbeat 生命周期尚未定义 |
| `DownloadManager` | 场景组件，维护下载任务队列 | 暂不接入 | 跨场景持久化、任务恢复和退出取消策略尚未定义 |
| `GuideManager` | 场景组件，依赖相机、Guide 数据和 UI 引用 | 暂不接入 | 属于场景级功能控制器，不是全局基础服务 |

## 推荐接入顺序

### 第一批：Core 基础服务

建议按以下顺序逐个实施：

1. `ThreadDispatcher`
2. `EventManager`
3. `FSMManager`

`ThreadDispatcher` 应最早可用，以满足早期主线程调度需求。

`EventManager` 应在需要事件通信的服务之前可用。迁移时必须保持事件处理器扫描、Runtime Listener 和现有公开 API 行为不变。

`FSMManager` 放在 EventManager 之后，便于未来状态机生命周期使用统一事件能力，但当前不要求建立新的依赖。

### 第二批：需先补生命周期边界的服务

1. `Guide Dispatcher`
2. `SoundManager`

Guide Dispatcher 在接管前需要先明确：

* 显式初始化入口
* 协程宿主丢失后的重建策略
* Play Mode 退出与 Domain Reload 关闭时的静态清理
* 是否应继续归属 Guide 模块，或演进为业务无关的 Coroutine Service

SoundManager 在接管前需要先明确：

* 音频子节点是否允许由框架入口创建
* 场景预配置实例与自动创建实例的优先级
* AudioClip、Resources 与未来 ResourceSystem 的边界
* 停止播放、协程回收与退出清理策略

## 建议初始化阶段

Phase2 实施时建议保留 FrameworkEntry 当前阶段划分，并逐步明确其职责：

```text
SubsystemRegistration
└─ 各模块静态状态重置

FrameworkEntry Bootstrap / BeforeSceneLoad
├─ 创建或复用 [ByFramework]
├─ Core Early
│  └─ ThreadDispatcher
├─ Core Services
│  ├─ EventManager
│  └─ FSMManager
└─ Optional Services
   ├─ Guide Dispatcher
   └─ SoundManager

Scene Ready
└─ UIRoot / UIManager / ClientManager / DownloadManager / GuideManager
   继续由场景或现有调用方管理
```

本设计不要求本阶段修改现有 `RuntimeInitializeOnLoadMethod`。实际迁移某个模块时，必须在同一批改动中消除该模块的双重初始化路径。

## 生命周期依赖关系

```text
FrameworkEntry
├─ ThreadDispatcher
├─ EventManager
├─ FSMManager
├─ Guide Dispatcher（候选，需先补生命周期接口）
└─ SoundManager（候选，需先确定音频与资源边界）

UIManager
└─ UIRoot

ClientManager
├─ ClientSession
├─ NetEventHandler
└─ Heartbeat

GuideManager
├─ Scene Cameras
├─ Guide Data
└─ Guide UI

DownloadManager
└─ Scene / Download Task Lifecycle
```

FrameworkEntry 不应直接依赖 `UIRoot`、`UIManager`、`ClientManager`、`DownloadManager` 或 `GuideManager`。

## 暂不接入模块说明

### UIRoot 与 UIManager

UIRoot 依赖场景中的 Canvas、Camera 和固定 UI 层级。UIManager 是纯 C# 单例，并在构造时立即访问 UIRoot，同时依赖 Resources 加载 UI。

在 UISystem、IUILoader 与 UIRoot 创建策略确定前，FrameworkEntry 不应接管它们。

### ClientManager

ClientManager 初始化时注册 Socket 会话，并关联消息队列、NetEventHandler 与 Heartbeat。连接建立、断线释放、场景切换和应用退出边界尚未统一。

在 NetworkSystem 生命周期设计完成前，不应由 FrameworkEntry 自动创建或连接。

### DownloadManager

DownloadManager 当前是场景组件。接管前必须先决定下载任务是否跨场景存活、退出时如何取消、失败任务是否恢复，以及队列状态归属。

### GuideManager

GuideManager 依赖场景相机、Guide 数据与 UI 引用，属于场景级控制器。FrameworkEntry 可以在未来提供 Guide 平台服务，但不应直接拥有具体场景的 GuideManager。

## 迁移实施门槛

每个候选模块真正接入前必须满足：

* 明确初始化时机与依赖模块。
* 初始化可重复调用且不会重复创建节点。
* 明确场景已有实例与自动创建实例的优先级。
* 明确 `OnDestroy`、应用退出和 Domain Reload 关闭时的状态清理。
* 保持现有公开 API 与功能行为。
* 完成独立 Unity Console 编译和运行验证。
* 完成单模块迁移记录与回滚说明。

## 风险分析

### 双重初始化

FrameworkEntry 接管模块时，如果模块原有 `RuntimeInitializeOnLoadMethod` 或静态构造器仍负责创建实例，可能出现重复节点、重复事件扫描或重复注册。

### 初始化时机变化

ThreadDispatcher 当前在 `BeforeSceneLoad` 初始化；EventManager 与 FSMManager 当前在 `AfterSceneLoad` 初始化。提前或延后初始化都可能影响现有调用方。

### 节点层级与 DontDestroyOnLoad

将模块节点挂到 `[ByFramework]` 会改变层级、销毁顺序和调试视图。迁移时必须验证父子节点持久化行为，避免对子节点重复调用持久化产生歧义。

### Domain Reload 关闭

FrameworkEntry 与模块静态引用可能跨 Play Session 残留。即使由入口统一编排，各模块仍必须保留可靠的静态重置策略。

### 隐式依赖扩大

如果 FrameworkEntry 直接访问 Platform 或场景模块，Core 会逐渐依赖具体实现。入口只应编排已确认属于 Core 或业务无关长期服务的模块。

### 一次性迁移风险

批量移除多个模块原有初始化入口会让问题难以定位。Phase2 必须按模块独立实施、验证和回滚。

## Phase2 实施建议

后续真正开始接管时，建议拆分为独立任务：

1. `FrameworkEntry Phase2A - ThreadDispatcher`
2. `FrameworkEntry Phase2B - EventManager`
3. `FrameworkEntry Phase2C - FSMManager`
4. `Guide Dispatcher Lifecycle Design`
5. `SoundManager Lifecycle Design`

本设计完成不代表任何模块已经由 FrameworkEntry 接管。

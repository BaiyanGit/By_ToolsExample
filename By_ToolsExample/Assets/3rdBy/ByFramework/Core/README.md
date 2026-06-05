# FrameworkEntry

`FrameworkEntry` 是 ByFramework 的统一启动入口。P3.4A 已完成 ThreadDispatcher 窄范围生命周期接管，其它模块仍保持原有初始化逻辑。

## 当前职责

* 在场景加载前创建 `[ByFramework]` 根节点
* 使用 `DontDestroyOnLoad` 保持根节点跨场景存在
* 防止重复创建 `FrameworkEntry`
* 在 Core Early 阶段注册 ThreadDispatcher，并在 `AfterSceneLoad` 保持原有时点完成 Initialize 与 Start
* 预留其它核心模块、服务模块与场景模块初始化阶段
* 在入口销毁时编排 ThreadDispatcher Stop 与 Shutdown
* 向 Unity Console 输出初始化日志

## 初始化阶段

FrameworkEntry 当前按以下顺序调用初始化阶段：

1. `InitializeCoreModules`
2. `InitializeServiceModules`
3. `InitializeSceneModules`

`InitializeCoreModules` 当前只编排 ThreadDispatcher 的 Register；FrameworkEntry 的 `AfterSceneLoad` 回调继续编排其 Initialize 与 Start。其它阶段仍仅作为后续模块逐步接入的位置。

## 当前未接管模块

EventManager、FSMManager、UIRoot、SoundManager、Guide、Network 与 Socket 仍保持原有初始化方式和行为。

## Unity 验证

进入 Play Mode 后，在 Hierarchy 中应只存在一个 `[ByFramework]` 根节点和一个 ThreadDispatcher 实例，并且切换场景后不会重复创建。无预放置实例时，自动创建节点名为 `[DspThread]`。

Unity Console 应按顺序输出 FrameworkEntry 初始化开始、三个初始化阶段就绪和初始化完成日志。

完整 P3.4A 生命周期映射、风险、回滚与验证步骤见 `Documentation/FrameworkEntryPhase2AThreadDispatcherImplementation.md`。

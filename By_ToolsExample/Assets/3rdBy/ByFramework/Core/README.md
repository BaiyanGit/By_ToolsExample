# FrameworkEntry

`FrameworkEntry` 是 ByFramework 的统一启动入口。第一阶段只负责创建并维护框架根节点，不接管现有模块的初始化逻辑。

## 当前职责

* 在场景加载前创建 `[ByFramework]` 根节点
* 使用 `DontDestroyOnLoad` 保持根节点跨场景存在
* 防止重复创建 `FrameworkEntry`
* 预留核心模块、服务模块与场景模块初始化阶段
* 向 Unity Console 输出初始化日志

## 初始化阶段

FrameworkEntry 当前按以下顺序调用初始化阶段：

1. `InitializeCoreModules`
2. `InitializeServiceModules`
3. `InitializeSceneModules`

这些阶段第一阶段不初始化任何现有模块，仅作为后续模块逐步接入的位置。

## 当前未接管模块

EventManager、FSMManager、ThreadDispatcher、UIRoot、SoundManager、Guide、Network 与 Socket 仍保持原有初始化方式和行为。

## Unity 验证

进入 Play Mode 后，在 Hierarchy 中应只存在一个 `[ByFramework]` 根节点，并且切换场景后该节点不会被销毁。

Unity Console 应按顺序输出 FrameworkEntry 初始化开始、三个初始化阶段就绪和初始化完成日志。

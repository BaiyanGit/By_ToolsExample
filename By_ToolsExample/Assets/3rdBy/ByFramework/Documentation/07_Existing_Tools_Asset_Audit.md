# Existing Tools Asset Audit

> 本文用于记录当前仓库中已有工具和历史代码资产的归属建议。  
> 这些代码可以作为后续实现参考，但不得直接覆盖已冻结架构边界。

---

# 一、Http/DownLoad/DownloadSystem

位置：

```text
Http/DownLoad/DownloadSystem
```

现状：

```text
已有 DownloadManager、DownloadTask、UnityHttpDownloader、IDownloadListener 等代码。
支持 UnityWebRequest 下载、临时文件、断点 Range、进度、速度、完成回调。
```

价值：

```text
可作为 LargeFileDownloader 的历史参考。
```

限制：

```text
依赖 MonoBehaviour
依赖 UnityWebRequest
依赖 UnityEngine
依赖 UniTask
默认路径写死到 Application.persistentDataPath/Downloads
当前不是纯 C# NetworkCore
当前不是多线程分片下载器完整实现
```

建议：

```text
未来 NetworkSystem 实现时，只参考其下载流程和回调设计。
最终 LargeFileDownloader 应进入 NetworkCore/Http/Downloader，并尽量纯 C# 化。
UnityWebRequest 版本可作为 Unity Adapter 或兼容 Provider。
```

---

# 二、Socket

位置：

```text
Socket
```

现状：

```text
已有 ClientManager、ClientSession、ServerManager、ByteBuffer、ProtoDictionary、Heartbeat、Protobuf 示例。
包含 Client / Server / Protobuf / 心跳方向的历史代码。
```

价值：

```text
可作为 NetworkCore Socket、Protocol、Serialization、Server 的迁移参考。
```

限制：

```text
依赖 UnityEngine
依赖 MonoSingletonTemplate
依赖 Update 派发消息
ServerManager 使用 Thread.Abort
协议结构仍偏示例化
ClientManager 混合了 Protobuf、事件派发、Unity 日志和业务示例
```

建议：

```text
未来 NetworkCore 实现时，不直接搬运。
应抽象出纯 C# TCP / UDP / WebSocket Transport、Session、Protocol、Serializer。
Protobuf 工具链可复用，但 NetworkCore 不依赖 Unity 或 ByFramework。
```

---

# 三、UI/Editor/UIAutoCreate

位置：

```text
UI/Editor/UIAutoCreate
```

现状：

```text
已有 UI 自动生成器 EditorWindow，可生成 UI 预制体、View、Model 等代码。
```

价值：

```text
可作为 UISystem Editor 工具资产。
```

限制：

```text
当前服务于旧 UI 架构。
不代表最终 UISystem Runtime。
```

建议：

```text
后续 UISystem Editor 实现时，可参考其中文化 UI、路径配置、模板生成能力。
需要按 UIKey、ResourceKey、UITheme、UILayer 新架构重构。
```

---

# 四、Editor/ByFrameworkPathUtility 与 Editor/Config

位置：

```text
Editor/ByFrameworkPathUtility.cs
Editor/Config/FrameworkEditorConfig.cs
```

现状：

```text
已有框架路径工具和 Editor 配置基础。
```

价值：

```text
可作为 FrameworkConfig Editor 工具和路径配置的参考。
```

限制：

```text
当前不等于最终 Platform/FrameworkConfig。
```

建议：

```text
后续 FrameworkConfig 实现时，可迁移其路径工具思想，但配置中心应归入 Platform/FrameworkConfig。
```

---

# 五、Core/Config

位置：

```text
Core/Config
```

现状：

```text
FrameworkConfig.cs、FrameworkConfigProvider.cs 和 FrameworkConfig.asset 是早期兼容配置基础设施。
```

价值：

```text
可作为旧项目兼容入口。
```

限制：

```text
不代表最终 FrameworkConfig 架构归属。
不得继续扩展 UI、Network、Download、Display、Resource、License 等 Platform 强类型配置。
```

建议：

```text
暂时保留兼容。
后续迁移到 Platform/FrameworkConfig。
```

---

# 六、Extension/ResourceManager

位置：

```text
Extension/ResourceManager.cs
```

现状：

```text
历史资源辅助工具。
```

价值：

```text
可作为现有 Resources 使用点审计参考。
```

限制：

```text
不代表最终 ResourceSystem。
```

建议：

```text
后续 ResourceSystem 实现时逐步迁移，不要直接把旧 ResourceManager 升级为最终资源系统。
```

---

# 七、UI/UIManager 与 UIRoot

位置：

```text
UI/UIManager.cs
UI/UIRoot.cs
```

现状：

```text
旧 UI 管理系统。
```

价值：

```text
可作为 UISystem 迁移审计对象。
```

限制：

```text
不代表最终 UISystem。
```

建议：

```text
后续 UISystem Runtime 实现时，以 21_UISystemFoundation.md 为准，逐步迁移旧 UI 能力。
```

---

# 八、Guide 与 SoundManager

位置：

```text
Guide
SoundManager
```

现状：

```text
已有功能模块。
```

价值：

```text
可作为后续 FeatureModule 或 Platform 边界审计样本。
```

建议：

```text
Guide 更像 FeatureModule / Sample。
SoundManager 可能未来成为 Platform 音频服务或 FeatureModule，需单独设计。
```

---

# 九、结论

当前仓库已有工具有复用价值，但不能直接决定最终架构。

后续原则：

```text
先按 Foundation 文档冻结 API
再评估旧代码是否迁移
能参考则参考
不符合边界则重构
不允许旧代码反向改变新架构
```

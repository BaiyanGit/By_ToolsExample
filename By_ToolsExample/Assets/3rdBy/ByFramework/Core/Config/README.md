# FrameworkConfig

FrameworkConfig 是 ByFramework 的 Runtime 统一配置中心。第一阶段仅建立配置结构、默认配置资源与全局访问入口，不改变现有模块行为。

## Runtime 配置

默认资源位于 `Core/Config/Resources/FrameworkConfig.asset`。

通过以下入口读取：

```csharp
FrameworkConfig config = FrameworkConfigProvider.Config;
```

当前包含：

* `ModuleSettings`：Guide 模块启用开关
* `UISettings`：UI 预制体 Resources 路径
* `NetworkSettings`：Socket 地址与端口
* `DownloadSettings`：下载目录名称

配置资源加载失败时，FrameworkConfigProvider 会创建仅供本次运行使用的默认配置并输出警告。

## Editor 配置

FrameworkEditorConfig 是独立的 Editor 配置资源，位于 `Editor/Config/FrameworkEditorConfig.asset`，不会进入 Runtime 配置加载流程。

当前包含：

* UI 预制体输出路径
* UI 脚本输出路径
* UIViewAutoCreateConfig 引用

## 后续接入规划

后续按模块逐步将现有硬编码默认值迁移到配置中心。模块接入前应保留现有 API 与默认行为，并分别完成 Unity 验证。

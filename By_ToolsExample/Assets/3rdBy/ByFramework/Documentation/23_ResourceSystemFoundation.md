# P3.9 ResourceSystem Foundation 设计冻结

> 阶段性质：设计冻结阶段  
> 不是 Runtime 实现阶段  
> 不创建 ResourceSystem Runtime 脚本  
> 不实现 AssetBundle 加载、Manifest 解析、缓存、引用计数或远程更新代码  

---

# 一、阶段目标

P3.9 的目标是冻结 ResourceSystem Foundation 的基础设计，为后续 ResourceSystem Runtime Implementation、AssetBundle 子系统、远程资源更新和资源 Editor 工具提供明确边界。

本阶段只允许完成：

```text
ResourceSystem 职责冻结
ResourceSystem 边界冻结
多 Provider 资源加载模型冻结
AssetBundle 归属与独立性方向冻结
ResourceKey 模型冻结
ResourceManifest 模型冻结
资源分组规则冻结
缓存与引用计数方向冻结
远程资源更新预留规则冻结
现场资源路径配置规则冻结
ResourceSystem 与 FrameworkConfig / BuildProfileSystem / NetworkSystem / LargeFileDownloader 的关系冻结
```

本阶段不进入具体运行时代码实现。

---

# 二、架构位置

ResourceSystem 属于：

```text
Platform/ResourceSystem
```

ResourceSystem 不属于 Core。

ResourceSystem 不属于 FeatureModule。

ResourceSystem 是 Platform 层通用资源能力模块。

AssetBundle 属于 ResourceSystem 的重要子系统。

---

# 三、ResourceSystem 定位

ResourceSystem 是 ByFramework 的统一资源访问系统。

它负责统一：

```text
资源定位
资源加载
资源卸载
资源缓存
资源引用计数
资源分组
资源清单
资源版本
资源依赖
资源来源适配
```

ResourceSystem 的核心目标是：

```text
让业务模块通过统一 ResourceKey 访问资源，而不关心资源来自 Resources、AssetBundle、Addressables、本地文件还是远程缓存。
```

---

# 四、资源加载路线冻结

ResourceSystem 必须采用多 Provider 共存模式：

```text
Resources
AssetBundle
Addressables
LocalFile
RemoteResource
StreamingAssets
PersistentDataPath
```

对应选择为：

```text
D：多 Provider 共存
```

其中 AssetBundle 是重点支持方案，但不强制所有项目都必须使用 AssetBundle。

---

# 五、AssetBundle 定位

AssetBundle 是 ResourceSystem 的重要资源来源。

AssetBundle 可以作为：

```text
主资源方案
可选资源方案
跨项目可复用资源模块
```

最终策略：

```text
AssetBundle 可以选择作为主方案
也可以选择不作为主方案
```

也就是：

```text
C：前期可选，后期可能成为主方案
```

---

## 5.1 AssetBundle 独立性原则

AssetBundle 子系统应尽量保持可独立复用。

目标：

```text
AssetBundle 打包、Manifest、Hash、依赖分析、加载逻辑可以被其它 Unity 项目复用
```

AssetBundle 子系统不应强依赖具体业务模块。

AssetBundle Runtime 不应理解：

```text
车辆资源
训练资源
设备资源
客户资源
```

它只理解：

```text
Bundle
Asset
Manifest
Dependency
Hash
Version
Load
Unload
```

---

## 5.2 AssetBundle 与 ResourceSystem 的关系

正确关系：

```text
ResourceSystem
↓
AssetBundleProvider
↓
AssetBundle Runtime
```

ResourceSystem 提供统一访问入口。

AssetBundleProvider 是 ResourceProvider 的一种实现方向。

AssetBundle Runtime 负责具体 AB 加载、卸载、依赖和 Manifest 查询。

---

## 5.3 AssetBundle 默认现场路径

考虑现场部署便利性，AssetBundle 应支持一个默认部署路径：

```text
StreamingAssets/AB/
```

现场人员可以把 AB 包直接放入打包后的：

```text
StreamingAssets/AB/
```

ResourceSystem 应支持从该目录读取本地 AB 包。

---

## 5.4 AssetBundle 可配置路径

不能强制现场人员一定按默认路径操作。

因此必须支持配置文件指定 AB 包路径。

配置方向：

```text
abRootPath
abManifestPath
abCachePath
abRemoteBaseUrl
```

示例路径：

```text
StreamingAssets/AB/
D:/ProjectAssets/AB/
E:/CustomerA/ResourceBundles/
./AB/
../AB/
```

如果现场人员把 AB 放到了其它文件夹，应能通过配置文件修改路径，而不是重新打包程序。

---

## 5.5 AssetBundle 路径优先级

AB 路径建议遵守 FrameworkConfig 总优先级：

```text
命令行参数
>
PersistentDataPath 现场配置
>
StreamingAssets 部署配置
>
Editor 生成配置
>
默认内置配置
```

默认路径为：

```text
StreamingAssets/AB/
```

现场修改配置后可覆盖默认路径。

---

# 六、远程资源更新

ResourceSystem 需要支持远程资源更新能力。

当前阶段没有强制远程更新需求，但后期可能需要。

因此 P3.9 冻结为：

```text
远程资源更新需要支持
但当前阶段只做架构预留
```

---

## 6.1 远程资源更新服务对象

远程更新未来可服务：

```text
AssetBundle 远程包
补丁包
地图包
训练素材包
客户资源包
配置包
离线数据包
```

---

## 6.2 与 NetworkSystem / Downloader 的关系

远程下载不属于 ResourceSystem 直接职责。

正确关系：

```text
ResourceSystem
↓
RemoteResourceProvider / AssetBundleProvider
↓
NetworkSystem.Http.LargeFileDownloader
↓
下载到本地缓存
↓
ResourceSystem 从本地缓存加载
```

Downloader 只负责下载文件。

ResourceSystem 负责识别资源、校验资源、加载资源。

AssetBundleProvider 负责加载 AB。

---

## 6.3 当前阶段策略

P3.9 不实现：

```text
远程下载
补丁更新
版本对比
资源热更新
```

只冻结接口方向和模块关系。

---

# 七、ResourceManifest

ResourceManifest 是资源清单。

选择：

```text
C：记录完整资源目录、平台、版本、依赖、Hash、远程地址、分组
```

---

## 7.1 Manifest 职责

ResourceManifest 负责描述：

```text
资源 Key
资源类型
资源来源
资源分组
资源平台
资源版本
资源 Hash
资源大小
资源依赖
Bundle 名称
Asset 名称
本地路径
远程地址
缓存路径
是否必需
是否可选
```

---

## 7.2 Manifest 字段方向

建议包含：

```text
manifestVersion
buildTarget
buildProfile
resourceVersion
generatedTime
groups
bundles
assets
dependencies
hash
size
remoteBaseUrl
localBasePath
```

Asset 记录方向：

```text
resourceKey
resourceType
providerType
group
bundleName
assetName
assetPath
hash
size
version
dependencies
tags
platform
isOptional
```

---

## 7.3 Manifest 不负责

ResourceManifest 不负责：

```text
实际加载资源
实际下载资源
业务解释
资源使用逻辑
```

它只描述资源信息。

---

# 八、ResourceKey

ResourceSystem 资源访问应支持：

```text
字符串 Key
ResourceKey 对象
```

即：

```text
C + D
```

---

## 8.1 字符串 Key

字符串 Key 示例：

```text
ui.main_menu
ui.settings
ui.runtime_config
vehicle.car_a
vehicle.truck_b
scene.training_yard
audio.warning
config.default_display
```

字符串 Key 适合：

```text
配置文件
外部引用
Editor 可视化配置
人工维护
跨项目迁移
```

---

## 8.2 ResourceKey 对象

ResourceKey 对象适合 Runtime 类型安全和扩展。

ResourceKey 可包含：

```text
Key
Group
Type
ProviderHint
Version
Tags
```

本阶段不冻结具体 C# 结构，只冻结方向。

---

## 8.3 禁止直接路径依赖

业务模块不应直接依赖资源物理路径。

禁止业务层长期使用：

```text
Assets/xxx/xxx.prefab
StreamingAssets/xxx
D:/xxx
```

业务层应优先使用：

```text
ResourceKey
```

路径由 ResourceSystem / Manifest / Provider 解析。

---

# 九、资源分组

ResourceSystem 必须支持资源分组。

选择：

```text
A：需要
```

---

## 9.1 建议分组

建议基础资源分组：

```text
UI
Theme
Scene
Vehicle
Audio
Config
Training
Common
Customer
Device
Localization
```

项目可扩展：

```text
CustomerA
CustomerB
ProjectA
ProjectB
Demo
Official
VR
MultiDisplay
```

---

## 9.2 分组用途

资源分组可用于：

```text
批量加载
批量卸载
AssetBundle 打包分组
远程更新分组
客户资源裁剪
BuildProfile 资源裁剪
License 功能资源控制
资源报告
```

---

# 十、缓存与引用计数

ResourceSystem 必须支持：

```text
缓存
引用计数
```

选择：

```text
A：需要
```

---

## 10.1 缓存职责

缓存用于：

```text
避免重复加载
提升运行时性能
管理常用资源
支持资源复用
支持预加载
```

---

## 10.2 引用计数职责

引用计数用于：

```text
判断资源是否仍被使用
控制资源卸载时机
避免误卸载
避免内存泄漏
```

---

## 10.3 缓存策略方向

后续 Runtime 可支持：

```text
常驻资源
临时资源
按组缓存
按场景缓存
按 UI 缓存
手动释放
自动释放
```

本阶段只冻结策略方向，不实现代码。

---

# 十一、ResourceHandle

ResourceHandle 是加载结果句柄。

后续 Runtime 可通过 ResourceHandle 管理：

```text
资源实例
加载状态
引用关系
释放行为
错误信息
```

ResourceHandle 方向：

```text
ResourceHandle<T>
```

应支持：

```text
IsValid
IsDone
Asset
Error
Release
```

本阶段不实现代码。

---

# 十二、ResourceRequest / ResourceResult

ResourceRequest 表示资源加载请求。

ResourceResult 表示加载结果。

ResourceRequest 可包含：

```text
ResourceKey
LoadMode
ProviderHint
Async
Priority
Group
Version
```

ResourceResult 可包含：

```text
Success
Asset
Handle
ErrorCode
ErrorMessage
ProviderType
```

本阶段只冻结概念。

---

# 十三、Provider 模型

ResourceSystem 采用 Provider 模型。

基础 Provider：

```text
ResourcesProvider
AssetBundleProvider
AddressablesProvider
LocalFileProvider
RemoteResourceProvider
StreamingAssetsProvider
PersistentDataPathProvider
```

---

## 13.1 Provider 职责

Provider 负责处理具体资源来源。

例如：

```text
ResourcesProvider       从 Resources 加载
AssetBundleProvider     从 AssetBundle 加载
AddressablesProvider    从 Addressables 加载
LocalFileProvider       从本地文件加载
RemoteResourceProvider  从远程资源缓存加载
```

Provider 不负责业务含义。

---

## 13.2 Provider 选择规则

ResourceSystem 可根据以下信息选择 Provider：

```text
ResourceManifest
ResourceKey
ProviderHint
ResourceConfig
BuildProfile
运行时配置
```

---

# 十四、AssetBundle Editor 工具

ResourceSystem 必须提供 Editor 资源管理窗口。

选择：

```text
A：需要
```

---

## 14.1 Editor 工具职责

AssetBundle Editor 工具应支持：

```text
AssetBundle 标记工具
AssetBundle 打包工具
AssetBundle 构建配置
AssetBundle Manifest 生成
AssetBundle Hash 生成
AssetBundle 依赖分析
AssetBundle 清理工具
AssetBundle 构建报告
资源 Key 映射
资源分组管理
资源路径检查
资源重复检查
```

---

## 14.2 EditorWindow 方向

建议工具：

```text
ResourceSystemConfigWindow
ResourceKeyMappingWindow
AssetBundleBuilderWindow
AssetBundleProfileWindow
AssetBundleManifestWindow
AssetBundleDependencyAnalyzerWindow
AssetBundleBuildReportWindow
```

所有 Editor UI 遵守中文化规范。

示例：

```text
菜单：ByFramework/平台/ResourceSystem 配置
按钮：生成资源清单
按钮：构建 AssetBundle
提示：请选择资源分组
日志：[ResourceSystem] 资源清单生成完成
```

---

# 十五、现场资源路径配置

现场人员必须能配置资源路径。

选择：

```text
A：需要
```

---

## 15.1 必须支持的路径配置

至少应支持：

```text
AssetBundle 本地根路径
AssetBundle Manifest 路径
AssetBundle 缓存路径
远程资源服务器地址
补丁包路径
下载缓存目录
资源日志目录
```

---

## 15.2 配置文件路径

部署初始配置：

```text
StreamingAssets/ByFramework/Config/resource_config.json
```

现场修改后配置：

```text
PersistentDataPath/ByFramework/Config/resource_config.json
```

AssetBundle 默认目录：

```text
StreamingAssets/AB/
```

---

## 15.3 防止现场误放路径

考虑现场人员可能没有按默认规则放置 AB 包。

因此 ResourceSystem 必须允许通过配置修正 AB 路径。

例如：

```json
{
  "assetBundleRootPath": "D:/ProjectAssets/AB/",
  "assetBundleManifestPath": "D:/ProjectAssets/AB/manifest.json",
  "assetBundleCachePath": "D:/ProjectCache/AB/"
}
```

如果配置路径无效，应提供明确错误提示。

例如：

```text
[ResourceSystem] AssetBundle 路径不存在，请检查 resource_config.json 中的 assetBundleRootPath
```

---

# 十六、ResourceSystem 与 FrameworkConfig 的关系

FrameworkConfig 负责加载和合并资源配置。

ResourceSystem 只消费最终配置。

正确关系：

```text
FrameworkConfig
↓
ResourceConfig
↓
ResourceSystem
```

ResourceSystem 不负责配置优先级合并。

配置优先级由 FrameworkConfig 统一管理。

---

# 十七、ResourceSystem 与 BuildProfileSystem 的关系

BuildProfileSystem 可影响资源构建和资源裁剪。

例如：

```text
Windows 资源
Linux 资源
VR 资源
多屏资源
客户 A 资源
客户 B 资源
演示版资源
正式版资源
```

BuildProfileSystem 可决定：

```text
哪些资源进入包体
哪些资源进入 AssetBundle
哪些资源分组启用
哪些资源生成 Manifest
哪些资源需要远程更新
```

---

# 十八、ResourceSystem 与 LicenseSystem 的关系

LicenseSystem 可控制资源组是否允许使用。

例如：

```text
高级车辆资源
VR 资源包
客户定制资源包
SimulationServer 资源
```

ResourceSystem 不负责授权规则。

ResourceSystem 只在需要时询问 LicenseSystem 某资源组是否允许加载。

---

# 十九、ResourceSystem 与 UISystem 的关系

UISystem 不直接加载 UI 资源。

正确关系：

```text
UISystem
↓
ResourceSystem
↓
Resources / AssetBundle / Addressables
```

UIKey 应映射到 ResourceKey。

ResourceSystem 负责实际资源加载。

---

# 二十、ResourceSystem 与 LocalizationSystem 的关系

LocalizationSystem 可通过 ResourceSystem 加载语言包。

例如：

```text
LanguagePack
LocalizationTable
字体资源
```

ResourceSystem 不理解语言业务。

它只负责加载资源。

---

# 二十一、ResourceSystem 与 NetworkSystem 的关系

ResourceSystem 不直接实现网络通信。

远程资源下载应通过：

```text
NetworkSystem.Http.LargeFileDownloader
```

同时需要遵守 NetworkCore 独立性原则：

```text
Downloader 可保持在 NetworkCore 中
ResourceSystem 通过 NetworkSystem Adapter 使用下载能力
```

ResourceSystem 不应直接把网络下载逻辑写死在资源模块中。

---

# 二十二、错误与降级策略

ResourceSystem 必须支持明确错误和降级策略。

常见错误：

```text
ResourceKey 不存在
Manifest 不存在
AssetBundle 路径不存在
AssetBundle Hash 不匹配
依赖包缺失
资源版本不匹配
Provider 不存在
资源加载失败
远程资源不可达
缓存文件损坏
```

降级方向：

```text
AB 路径无效 → 尝试默认 StreamingAssets/AB/
远程不可用 → 尝试本地缓存
Manifest 加载失败 → 使用内置默认 Manifest
Provider 不可用 → 返回明确错误
可选资源失败 → 记录警告并继续
必需资源失败 → 返回严重错误
```

---

# 二十三、Required / Optional

资源应支持：

```text
Required
Optional
```

Required 资源失败时：

```text
返回严重错误
阻止依赖功能继续
提示现场人员检查资源
```

Optional 资源失败时：

```text
记录警告
继续运行
```

---

# 二十四、建议核心对象

后续 Runtime 可围绕以下对象设计。

本阶段只冻结概念，不实现代码。

```text
ResourceKey
ResourceHandle
ResourceRequest
ResourceResult
ResourceProvider
ResourceManifest
ResourceCatalog
ResourceGroup
ResourceLocation
ResourceDependency
ResourceCache
ResourceRefCounter
ResourceConfig
AssetBundleManifestInfo
AssetBundleBuildProfile
```

---

# 二十五、建议目录结构

本阶段仅冻结目录方向，不要求创建代码文件。

```text
Assets/ByFramework/Platform/ResourceSystem
├─ Runtime
│  ├─ Core
│  │  ├─ ResourceKey
│  │  ├─ ResourceHandle
│  │  ├─ ResourceRequest
│  │  ├─ ResourceResult
│  │  └─ ResourceConfig
│  │
│  ├─ Provider
│  │  ├─ ResourcesProvider
│  │  ├─ AssetBundleProvider
│  │  ├─ AddressablesProvider
│  │  ├─ LocalFileProvider
│  │  ├─ StreamingAssetsProvider
│  │  ├─ PersistentDataPathProvider
│  │  └─ RemoteResourceProvider
│  │
│  ├─ Manifest
│  │  ├─ ResourceManifest
│  │  ├─ ResourceCatalog
│  │  ├─ ResourceGroup
│  │  └─ ResourceDependency
│  │
│  ├─ Cache
│  │  ├─ ResourceCache
│  │  └─ ResourceRefCounter
│  │
│  ├─ AssetBundle
│  │  ├─ AssetBundleProvider
│  │  ├─ AssetBundleManifestInfo
│  │  ├─ AssetBundleDependency
│  │  └─ AssetBundleLocation
│  │
│  └─ Service
│     └─ IResourceService
│
└─ Editor
   ├─ ResourceSystemConfigWindow
   ├─ ResourceKeyMappingWindow
   ├─ AssetBundleBuilderWindow
   ├─ AssetBundleProfileWindow
   ├─ AssetBundleManifestWindow
   ├─ AssetBundleDependencyAnalyzerWindow
   └─ AssetBundleBuildReportWindow
```

---

# 二十六、配置文件方向

建议配置文件：

```text
StreamingAssets/ByFramework/Config/resource_config.json
```

现场修改后：

```text
PersistentDataPath/ByFramework/Config/resource_config.json
```

AssetBundle 默认目录：

```text
StreamingAssets/AB/
```

示例字段方向：

```text
defaultProvider
assetBundleRootPath
assetBundleManifestPath
assetBundleCachePath
remoteBaseUrl
downloadCachePath
enableRemoteUpdate
enableHashCheck
enableVersionCheck
enableRefCount
enableCache
groups
```

本阶段不冻结具体 JSON Schema，只冻结配置方向。

---

# 二十七、PlatformServiceRegistry 接入方向

ResourceSystem 后续可注册为 Platform 服务。

接口方向：

```text
IResourceService
```

注册方向：

```text
PlatformServiceRegistry.Register<IResourceService>(resourceService)
```

获取方向：

```text
PlatformServiceRegistry.Get<IResourceService>()
PlatformServiceRegistry.TryGet<IResourceService>(out resourceService)
```

本阶段不实现注册代码，只冻结接入方向。

---

# 二十八、禁止事项

P3.9 阶段禁止：

```text
实现 ResourceSystem Runtime 管理器
实现 AssetBundle 加载代码
实现 AssetBundle 打包代码
实现 Manifest 解析代码
实现缓存系统
实现引用计数
实现远程下载
实现资源热更新
实现 ResourceProvider 代码
实现 EditorWindow 工具代码
修改 FrameworkEntry 生命周期
扩展 Core/Config/FrameworkConfig.cs
进入 UISystem Runtime Implementation
进入 DisplaySystem Runtime Implementation
进入 NetworkSystem Implementation
```

---

# 二十九、P3.9 输出物

P3.9 应输出：

```text
Documentation/23_ResourceSystemFoundation.md
Documentation/00_ByFramework_Current_Context.md 更新
02_Documentation/02_Roadmap.md 更新
03_Documentation/03_Todo.md 更新
Documentation/04_Changelog.md 更新
```

不要求输出 Runtime 代码。

---

# 三十、阶段关闭条件

P3.9 关闭条件：

```text
ResourceSystem 职责明确
ResourceSystem 边界明确
多 Provider 共存模型明确
AssetBundle 可选主方案与独立性原则明确
StreamingAssets/AB 默认路径明确
AB 路径可配置规则明确
远程资源更新预留明确
ResourceManifest 完整字段方向明确
ResourceKey 字符串 + 对象方向明确
资源分组规则明确
缓存与引用计数方向明确
Editor 资源管理窗口方向明确
现场资源路径配置规则明确
ResourceSystem 与 FrameworkConfig / BuildProfileSystem / LicenseSystem / UISystem / LocalizationSystem / NetworkSystem 的关系明确
禁止事项写入文档
Current_Context 更新完成
Roadmap / Todo / Changelog 同步完成
```

满足以上条件后，P3.9 可关闭。

下一阶段建议进入：

```text
P3.10 AssetBundle Foundation
```

或如果项目维护者认为 AssetBundle 已包含在 P3.9 中，也可以进入：

```text
P3.10 SaveSystem Foundation
```

---

# 三十一、最终结论

P3.9 ResourceSystem Foundation 是设计冻结阶段。

它只确定：

```text
ResourceSystem 是什么
ResourceSystem 管什么
ResourceSystem 不管什么
资源从哪里来
AssetBundle 如何归属
资源 Key 如何定位
Manifest 如何描述资源
现场如何配置资源路径
远程资源更新如何预留
后续 Runtime 实现应遵守什么边界
```

不得在本阶段进入具体 Runtime 实现。

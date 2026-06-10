# P3.10 AssetBundle Foundation 设计冻结

> 阶段性质：设计冻结阶段  
> 不是 Runtime 实现阶段  
> 不创建 AssetBundle Runtime 脚本  
> 不实现 AssetBundle 打包、加载、Manifest 解析、Hash 校验、加密或 EditorWindow 代码  

---

# 一、阶段目标

P3.10 的目标是冻结 AssetBundle Foundation 的基础设计，为后续 AssetBundle Editor 工具、AssetBundle Runtime 加载、Manifest 生成、客户差异打包和远程资源更新提供明确边界。

本阶段只允许完成：

```text
AssetBundle 子系统定位冻结
AssetBundle 独立性原则冻结
打包策略冻结
客户 / 版本差异打包方向冻结
输出目录规则冻结
双 Manifest 规则冻结
Hash 校验方向冻结
版本号模型冻结
加密预留方向冻结
Editor 工具复杂度冻结
AssetBundle 与 ResourceSystem / BuildProfileSystem / FrameworkConfig / NetworkSystem 的关系冻结
```

本阶段不进入具体运行时代码实现。

---

# 二、架构位置

AssetBundle 属于：

```text
Platform/ResourceSystem/AssetBundle
```

同时，AssetBundle 子系统应尽量保持独立复用能力。

也就是：

```text
AssetBundle 是 ResourceSystem 的子系统
但 AssetBundle 内部能力应尽量可跨项目复用
```

AssetBundle 不属于 Core。

AssetBundle 不属于 FeatureModule。

---

# 三、AssetBundle 定位

AssetBundle 是 ByFramework 的资源打包、资源发布、资源加载和资源更新基础能力之一。

它可以作为：

```text
项目主资源方案
项目可选资源方案
跨项目可复用资源打包与加载模块
```

当前设计选择：

```text
B：尽量独立，可跨项目复用
```

---

# 四、AssetBundle 独立性原则

AssetBundle 子系统应保持低耦合。

目标：

```text
AssetBundle 打包工具可以被其它 Unity 项目复用
AssetBundle Manifest 规则可以跨项目复用
AssetBundle Runtime 加载逻辑可以跨项目复用
AssetBundle Hash / Version / Dependency 规则可以跨项目复用
```

---

## 4.1 AssetBundle 允许依赖

AssetBundle 子系统允许依赖：

```text
UnityEditor
UnityEngine
System
System.IO
System.Collections.Generic
基础 JSON 序列化
ResourceSystem 抽象接口
```

Editor 部分允许依赖 UnityEditor。

Runtime 部分不允许依赖 UnityEditor。

---

## 4.2 AssetBundle 禁止依赖

AssetBundle 核心逻辑不应直接依赖：

```text
FeatureModule
VehicleSimulation
TrainingSystem
DeviceIntegration
SimulationSync
具体客户项目代码
具体业务枚举
```

AssetBundle Runtime 不应理解：

```text
车辆资源是什么
训练素材是什么
设备模型是什么
客户业务是什么
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

# 五、AssetBundle 与 ResourceSystem 的关系

正确关系：

```text
ResourceSystem
↓
AssetBundleProvider
↓
AssetBundle Runtime
```

ResourceSystem 负责统一资源访问入口。

AssetBundleProvider 是 ResourceProvider 的一种。

AssetBundle Runtime 负责：

```text
Bundle 加载
Bundle 卸载
Bundle 内资源加载
依赖查询
Manifest 查询
Hash / Version 校验
```

---

# 六、打包策略

AssetBundle 必须支持多打包策略。

选择：

```text
D：支持多策略
```

支持：

```text
按目录打包
按资源分组打包
按单资源打包
自定义规则打包
```

---

## 6.1 按目录打包

适用于：

```text
UI 资源目录
车辆资源目录
场景资源目录
音频资源目录
客户资源目录
```

特点：

```text
简单
直观
适合项目初期
适合现场资源目录清晰的项目
```

---

## 6.2 按资源分组打包

适用于：

```text
UI
Theme
Vehicle
Scene
Audio
Training
CustomerA
CustomerB
VR
MultiDisplay
```

特点：

```text
适合 BuildProfile
适合客户差异
适合远程更新
适合资源裁剪
```

---

## 6.3 按单资源打包

适用于：

```text
独立更新资源
体积较大资源
高频替换资源
客户单独资源
```

特点：

```text
粒度细
更新灵活
包数量可能较多
```

---

## 6.4 自定义规则打包

适用于：

```text
项目特殊目录规则
客户特殊资源规则
按标签打包
按平台打包
按资源类型打包
按依赖关系优化打包
```

自定义规则应由配置或 Editor 工具提供，不应写死到业务代码中。

---

# 七、客户 / 版本差异打包

AssetBundle 必须支持客户版本差异打包。

选择：

```text
A：需要
```

---

## 7.1 支持差异类型

应支持：

```text
客户 A 资源
客户 B 资源
演示版资源
正式版资源
VR 资源
非 VR 资源
多屏资源
单屏资源
Windows 资源
Linux 资源
Debug 资源
Release 资源
```

---

## 7.2 与 BuildProfileSystem 的关系

BuildProfileSystem 决定当前构建使用哪套资源规则。

关系：

```text
BuildProfileSystem
↓
AssetBundleBuildProfile
↓
AssetBundleBuilder
↓
输出对应平台 / 客户 / 版本的 AB 包
```

AssetBundle 不直接决定业务版本。

它根据 BuildProfile 提供的配置执行构建。

---

# 八、AB 输出目录

AssetBundle 输出目录必须同时支持：

```text
StreamingAssets/AB/
项目外部目录
```

选择：

```text
C：两者都支持
```

---

## 8.1 StreamingAssets 输出

默认输出路径：

```text
StreamingAssets/AB/
```

适用于：

```text
随包发布
现场直接替换 AB
无远程更新项目
局域网现场部署
```

---

## 8.2 外部目录输出

外部输出路径示例：

```text
BuildOutput/AB/
D:/ByFrameworkBuild/AB/
E:/CustomerA/AB/
```

适用于：

```text
构建服务器输出
客户版本资源输出
远程资源服务器部署
人工拷贝到现场
```

---

## 8.3 输出目录配置

输出目录应由配置决定，不应写死。

配置来源：

```text
Editor 配置
BuildProfile
命令行构建参数
```

---

# 九、AB 部署目录与现场配置

AssetBundle Runtime 默认支持从以下路径读取：

```text
StreamingAssets/AB/
```

同时必须支持现场配置覆盖路径。

配置路径：

```text
StreamingAssets/ByFramework/Config/resource_config.json
PersistentDataPath/ByFramework/Config/resource_config.json
```

可配置字段方向：

```text
assetBundleRootPath
assetBundleManifestPath
assetBundleCachePath
assetBundleRemoteBaseUrl
```

用于解决现场人员把 AB 放到其它文件夹的问题。

---

# 十、构建报告

AssetBundle 必须生成构建报告。

选择：

```text
A：需要
```

---

## 10.1 构建报告内容

构建报告应包含：

```text
构建时间
构建平台
BuildProfile
输出目录
Bundle 数量
Asset 数量
总大小
每个 Bundle 大小
每个 Bundle Hash
每个 Bundle 版本
依赖关系
重复资源
缺失资源
未分组资源
构建警告
构建错误
```

---

## 10.2 构建报告用途

用于：

```text
资源审查
客户交付记录
远程更新比对
包体大小分析
依赖问题排查
重复资源优化
构建问题追踪
```

---

# 十一、Manifest 格式

AssetBundle 必须同时支持：

```text
Unity 原生 AssetBundleManifest
自定义 JSON Manifest
```

选择：

```text
C：两者都要
```

---

## 11.1 Unity AssetBundleManifest

Unity 原生 AssetBundleManifest 负责：

```text
Unity 依赖关系
Bundle 依赖查询
Unity 内部 Hash
Unity 加载辅助
```

---

## 11.2 自定义 JSON Manifest

自定义 JSON Manifest 负责框架资源信息。

应包含：

```text
Manifest 版本
资源版本
BuildProfile
平台
客户
Bundle 列表
Asset 列表
ResourceKey
资源分组
Bundle Hash
Bundle 大小
Asset 类型
依赖关系
本地路径
远程地址
是否必需
是否可选
标签
```

---

## 11.3 双 Manifest 关系

正确关系：

```text
Unity AssetBundleManifest
负责 Unity 依赖

Custom ResourceManifest
负责 ByFramework 资源索引、版本、分组、远程更新、路径配置
```

两者都应保留。

不能只依赖 Unity 原生 Manifest。

---

# 十二、Hash 校验

AssetBundle 需要支持 Hash 校验，但不强制所有项目启用。

选择：

```text
B：可选支持
```

---

## 12.1 Hash 校验用途

用于：

```text
资源完整性检查
远程更新校验
缓存校验
现场资源误替换检测
构建版本比对
```

---

## 12.2 Hash 开关

Hash 校验应可配置：

```text
enableHashCheck = true / false
```

Hash 不匹配时应返回明确错误。

例如：

```text
[AssetBundle] Hash 校验失败：vehicle_car_a.ab
```

---

# 十三、版本号模型

AssetBundle 必须支持版本号。

选择：

```text
A：需要
```

---

## 13.1 版本号类型

建议支持：

```text
ManifestVersion
ResourceVersion
BundleVersion
GroupVersion
AssetVersion
```

---

## 13.2 版本用途

用于：

```text
远程资源更新
客户版本管理
构建记录
缓存判断
资源回滚
资源差异比对
```

---

## 13.3 版本来源

版本号可来自：

```text
BuildProfile
Editor 配置
手动输入
自动生成
命令行构建参数
```

---

# 十四、加密预留

AssetBundle 加载需要预留加密能力。

选择：

```text
A：需要预留
```

---

## 14.1 当前阶段策略

当前阶段只预留接口方向。

不实现具体加密算法。

---

## 14.2 加密预留方向

未来可支持：

```text
Bundle 文件加密
Manifest 加密
Hash + 加密组合校验
客户资源加密
License 控制解密
```

---

## 14.3 边界

AssetBundle 子系统不应写死某一种加密算法。

应通过抽象接口接入。

例如：

```text
IAssetBundleDecryptor
```

本阶段不创建接口代码，只冻结方向。

---

# 十五、Editor 工具复杂度

Editor 工具复杂度选择：

```text
B：中等工具
```

即支持：

```text
配置
分组
构建
报告
依赖分析
```

后期可升级为完整资源管理器。

---

## 15.1 EditorWindow 方向

建议包含：

```text
AssetBundleBuilderWindow
AssetBundleProfileWindow
AssetBundleGroupWindow
AssetBundleDependencyAnalyzerWindow
AssetBundleBuildReportWindow
AssetBundlePathCheckWindow
```

---

## 15.2 Editor 工具功能

中等复杂度工具应支持：

```text
选择 BuildProfile
配置输出路径
配置打包策略
配置资源分组
配置客户版本
执行构建
清理旧 Bundle
生成 Unity Manifest
生成自定义 JSON Manifest
生成 Hash
生成版本号
依赖分析
重复资源检查
输出构建报告
打开输出目录
```

---

## 15.3 Editor 中文化规范

所有 Editor UI 必须中文化。

示例：

```text
菜单：ByFramework/平台/AssetBundle 构建工具
按钮：构建 AssetBundle
按钮：生成资源清单
按钮：依赖分析
按钮：打开输出目录
提示：请选择 BuildProfile
日志：[AssetBundle] 构建完成
警告：[AssetBundle] 发现重复资源
错误：[AssetBundle] 构建失败
```

技术对象名保留英文。

---

# 十六、AssetBundle BuildProfile

AssetBundleBuildProfile 是 AB 构建配置。

它可以由 BuildProfileSystem 管理。

字段方向：

```text
profileName
buildTarget
customer
version
outputPath
buildStrategy
resourceGroups
includeGroups
excludeGroups
enableHash
enableVersion
enableEncryption
generateUnityManifest
generateCustomManifest
```

本阶段不冻结具体数据结构，只冻结字段方向。

---

# 十七、AssetBundle 分组规则

AssetBundle 构建必须支持资源分组。

建议基础分组：

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
VR
MultiDisplay
```

项目可扩展：

```text
CustomerA
CustomerB
Demo
Official
Debug
Release
```

分组可影响：

```text
打包策略
远程更新
客户裁剪
License 控制
BuildProfile 构建
```

---

# 十八、依赖分析

AssetBundle Editor 工具必须支持依赖分析方向。

依赖分析用于：

```text
发现重复依赖
发现循环依赖
发现遗漏依赖
发现包体过大
优化资源分组
生成构建报告
```

依赖分析结果应写入构建报告。

---

# 十九、路径检查

AssetBundle Editor 工具应支持路径检查。

检查内容：

```text
输出目录是否存在
StreamingAssets/AB 是否可写
外部输出目录是否可写
资源路径是否有效
资源 Key 是否重复
资源分组是否为空
Manifest 输出路径是否有效
```

现场 Runtime 也应提供路径错误提示方向。

---

# 二十、AssetBundle Runtime 方向

后续 Runtime 可支持：

```text
加载 Manifest
加载依赖包
加载 Bundle
加载 Bundle 内资源
卸载 Bundle
引用计数
缓存
Hash 校验
版本校验
路径回退
错误提示
```

本阶段不实现代码，只冻结方向。

---

# 二十一、AssetBundle 与 NetworkSystem / Downloader 的关系

AssetBundle Runtime 不直接实现下载器。

远程 AB 下载应通过：

```text
NetworkSystem.Http.LargeFileDownloader
```

关系：

```text
AssetBundleProvider
↓
需要远程 AB
↓
NetworkSystem.Http.LargeFileDownloader
↓
下载到本地缓存
↓
AssetBundleProvider 加载本地缓存
```

Downloader 不理解 AB 业务。

AssetBundleProvider 不实现底层下载。

---

# 二十二、AssetBundle 与 FrameworkConfig 的关系

FrameworkConfig 负责加载和合并 AB 相关配置。

AssetBundle 子系统只消费最终配置。

配置来源：

```text
Editor 配置
StreamingAssets 外部配置
PersistentDataPath 现场配置
命令行参数
```

配置优先级：

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

---

# 二十三、AssetBundle 与 LicenseSystem 的关系

LicenseSystem 可控制某些资源组是否允许使用。

例如：

```text
VR 资源组
高级车辆资源组
客户定制资源组
SimulationServer 资源组
```

AssetBundle 不负责授权规则。

AssetBundle 只在加载前接收 ResourceSystem / LicenseSystem 的允许结果。

---

# 二十四、AssetBundle 与 FeatureModule 的关系

FeatureModule 不应直接依赖 AssetBundle 路径。

正确关系：

```text
FeatureModule
↓
ResourceSystem
↓
AssetBundleProvider
↓
AssetBundle Runtime
```

FeatureModule 使用：

```text
ResourceKey
```

而不是：

```text
具体 AB 文件路径
```

---

# 二十五、建议核心对象

后续实现可围绕以下对象设计。

本阶段只冻结概念，不实现代码。

```text
AssetBundleBuildProfile
AssetBundleBuildRule
AssetBundleBuildStrategy
AssetBundleGroup
AssetBundleBuildReport
AssetBundleManifestInfo
AssetBundleAssetInfo
AssetBundleDependencyInfo
AssetBundleHashInfo
AssetBundleVersionInfo
AssetBundleLocation
AssetBundleLoadRequest
AssetBundleLoadResult
AssetBundleDecryptor
```

---

# 二十六、建议目录结构

本阶段仅冻结目录方向，不要求创建代码文件。

```text
Assets/ByFramework/Platform/ResourceSystem/AssetBundle
├─ Runtime
│  ├─ Core
│  │  ├─ AssetBundleManifestInfo
│  │  ├─ AssetBundleAssetInfo
│  │  ├─ AssetBundleDependencyInfo
│  │  ├─ AssetBundleLocation
│  │  └─ AssetBundleVersionInfo
│  │
│  ├─ Loader
│  │  ├─ AssetBundleLoader
│  │  ├─ AssetBundleLoadRequest
│  │  └─ AssetBundleLoadResult
│  │
│  ├─ Cache
│  │  ├─ AssetBundleCache
│  │  └─ AssetBundleRefCounter
│  │
│  ├─ Security
│  │  └─ AssetBundleDecryptor
│  │
│  └─ Provider
│     └─ AssetBundleProvider
│
└─ Editor
   ├─ Builder
   │  ├─ AssetBundleBuilder
   │  ├─ AssetBundleBuildProfile
   │  ├─ AssetBundleBuildRule
   │  └─ AssetBundleBuildStrategy
   │
   ├─ Window
   │  ├─ AssetBundleBuilderWindow
   │  ├─ AssetBundleProfileWindow
   │  ├─ AssetBundleGroupWindow
   │  ├─ AssetBundleDependencyAnalyzerWindow
   │  └─ AssetBundleBuildReportWindow
   │
   ├─ Manifest
   │  ├─ UnityManifestExporter
   │  └─ CustomManifestGenerator
   │
   └─ Report
      └─ AssetBundleBuildReport
```

---

# 二十七、配置文件方向

AssetBundle 配置可属于：

```text
resource_config.json
```

也可拆分为：

```text
assetbundle_config.json
```

建议路径：

```text
StreamingAssets/ByFramework/Config/assetbundle_config.json
PersistentDataPath/ByFramework/Config/assetbundle_config.json
```

字段方向：

```text
assetBundleRootPath
assetBundleManifestPath
assetBundleCachePath
assetBundleRemoteBaseUrl
defaultBuildProfile
buildStrategy
enableHashCheck
enableVersionCheck
enableEncryption
generateUnityManifest
generateCustomManifest
groups
```

本阶段不冻结具体 JSON Schema。

---

# 二十八、禁止事项

P3.10 阶段禁止：

```text
实现 AssetBundle 打包代码
实现 AssetBundle 加载代码
实现 Manifest 解析代码
实现 Hash 校验代码
实现版本比对代码
实现加密算法
实现 EditorWindow 工具
实现依赖分析算法
实现构建报告生成
实现远程 AB 下载
修改 FrameworkEntry 生命周期
扩展 Core/Config/FrameworkConfig.cs
进入 ResourceSystem Runtime Implementation
进入 NetworkSystem Implementation
进入 BuildProfileSystem Implementation
```

---

# 二十九、P3.10 输出物

P3.10 应输出：

```text
Documentation/24_AssetBundleFoundation.md
Documentation/00_ByFramework_Current_Context.md 更新
02_Documentation/02_Roadmap.md 更新
03_Documentation/03_Todo.md 更新
Documentation/04_Changelog.md 更新
```

不要求输出 Runtime 代码。

---

# 三十、阶段关闭条件

P3.10 关闭条件：

```text
AssetBundle 子系统定位明确
AssetBundle 独立性原则明确
多打包策略明确
客户 / 版本差异打包明确
输出目录规则明确
构建报告规则明确
Unity Manifest + 自定义 JSON Manifest 关系明确
Hash 可选支持规则明确
版本号模型明确
加密预留方向明确
Editor 工具复杂度明确
AssetBundle 与 ResourceSystem / BuildProfileSystem / FrameworkConfig / NetworkSystem / LicenseSystem 关系明确
禁止事项写入文档
Current_Context 更新完成
Roadmap / Todo / Changelog 同步完成
```

满足以上条件后，P3.10 可关闭。

下一阶段建议进入：

```text
P3.11 SaveSystem Foundation
```

---

# 三十一、最终结论

P3.10 AssetBundle Foundation 是设计冻结阶段。

它只确定：

```text
AssetBundle 是什么
AssetBundle 管什么
AssetBundle 不管什么
如何打包
如何输出
如何生成 Manifest
如何做版本与 Hash
如何支持客户差异
如何预留加密
如何与 ResourceSystem 协作
后续 Runtime / Editor 实现应遵守什么边界
```

不得在本阶段进入具体 Runtime 或 Editor 实现。

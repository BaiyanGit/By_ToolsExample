# P3.15 BuildProfileSystem Foundation 设计冻结

> 阶段性质：设计冻结阶段  
> 不是 Runtime 实现阶段  
> 不创建 BuildProfileSystem Runtime 或 Editor 脚本  
> 不实现构建流程、AssetBundle 构建、功能裁剪、资源裁剪、命令行构建或构建报告代码  

---

# 一、阶段目标

P3.15 的目标是冻结 BuildProfileSystem Foundation 的基础设计，为后续多平台构建、客户版本构建、功能裁剪、资源裁剪、AssetBundle 构建、命令行构建、构建报告和默认配置生成提供明确边界。

本阶段只允许完成：

```text
BuildProfileSystem 职责冻结
BuildProfileSystem 边界冻结
BuildProfile 覆盖范围冻结
AssetBundle 构建参与规则冻结
功能裁剪规则冻结
资源裁剪规则冻结
配置形式冻结
构建报告方向冻结
命令行构建方向冻结
BuildProfile 与 LicenseSystem 关系冻结
BuildProfile 与 FrameworkConfig 关系冻结
现场覆盖规则冻结
BuildProfile 与 ResourceSystem / AssetBundle / FeatureModule 的关系冻结
```

本阶段不进入具体运行时或 Editor 构建代码实现。

---

# 二、架构位置

BuildProfileSystem 属于：

```text
Platform/BuildProfileSystem
```

BuildProfileSystem 不属于 Core。

BuildProfileSystem 不属于 FeatureModule。

BuildProfileSystem 是 Platform 层的构建配置与构建裁剪系统。

---

# 三、BuildProfileSystem 定位

BuildProfileSystem 是 ByFramework 的统一构建配置系统。

它负责统一管理：

```text
平台差异
客户差异
功能差异
资源差异
构建参数
构建裁剪
AssetBundle 构建配置
默认配置生成
License 默认功能关系
构建报告
命令行构建入口
```

BuildProfileSystem 的核心目标是：

```text
让不同客户、不同平台、不同版本、不同功能组合可以通过统一 BuildProfile 构建，而不需要手动反复修改项目代码和配置。
```

---

# 四、BuildProfile 覆盖范围

BuildProfile 主要解决：

```text
平台差异
客户差异
功能差异
资源差异
```

选择：

```text
D：全部支持
```

---

## 4.1 平台差异

支持：

```text
Windows
Linux
```

未来可扩展：

```text
国产系统环境
专用运行环境
```

平台差异可能影响：

```text
构建目标
输出目录
资源格式
AssetBundle 目标平台
默认配置
路径格式
VR 支持状态
```

---

## 4.2 客户差异

支持：

```text
客户 A
客户 B
客户 C
通用版本
定制版本
```

客户差异可能影响：

```text
资源包
主题
语言
License 默认功能
默认网络配置
默认显示布局
默认 UI 配置
FeatureModule 启用状态
```

---

## 4.3 功能差异

支持：

```text
VR
多屏
SimulationServer
Network Server
License
AssetBundle
RuntimeConfigUI
DeviceIntegration
FeatureModule
```

功能差异可能影响：

```text
宏定义
模块启用
默认配置
资源裁剪
License 默认功能
构建报告
```

---

## 4.4 资源差异

支持：

```text
客户资源
VR 资源
多屏资源
演示版资源
正式版资源
训练素材
设备模型
场景资源
```

资源差异通过 ResourceSystem / AssetBundle 配合完成。

---

# 五、AssetBundle 构建参与

BuildProfile 必须参与 AssetBundle 构建。

选择：

```text
A：参与
```

---

## 5.1 正确关系

```text
BuildProfileSystem
↓
AssetBundleBuildProfile
↓
AssetBundleBuilder
↓
输出对应平台 / 客户 / 功能 / 版本的 AB 包
```

---

## 5.2 BuildProfile 影响 AB 的内容

BuildProfile 可影响：

```text
BuildTarget
输出目录
资源分组
资源包含规则
资源排除规则
客户资源
版本号
Hash 开关
Manifest 生成
加密开关预留
远程更新配置
```

---

# 六、功能裁剪

BuildProfile 必须控制功能裁剪。

选择：

```text
A：需要
```

---

## 6.1 可裁剪功能

可裁剪功能包括：

```text
VR
多屏
SimulationServer
Network Server
RuntimeConfigUI
AssetBundle
License
DebugTools
DeviceIntegration
FeatureModule
```

---

## 6.2 裁剪方式方向

后续可支持：

```text
Scripting Define Symbols
配置文件裁剪
资源裁剪
模块启用状态
构建时目录过滤
场景列表裁剪
```

本阶段不实现具体裁剪代码，只冻结方向。

---

## 6.3 边界

BuildProfile 决定构建时是否包含某功能。

运行时是否允许使用某功能还可能由：

```text
LicenseSystem
FrameworkConfig
现场配置
```

共同决定。

---

# 七、资源裁剪

BuildProfile 必须控制资源裁剪。

选择：

```text
A：需要
```

---

## 7.1 资源裁剪对象

资源裁剪包括：

```text
客户资源
VR 资源
多屏资源
场景资源
训练素材
设备模型
语言包
主题资源
AssetBundle 资源组
```

---

## 7.2 与 ResourceSystem 的关系

```text
BuildProfileSystem
↓
决定资源组启用 / 禁用
↓
ResourceSystem / AssetBundle
↓
生成对应 ResourceManifest / AB 包
```

---

## 7.3 示例

客户 A 版本：

```text
包含 CustomerA 资源
排除 CustomerB 资源
包含正式 License 默认配置
包含客户 A 主题
```

演示版：

```text
包含 Demo 资源
排除高级资源
限制功能模块
生成演示版配置
```

---

# 八、配置形式

BuildProfile 配置形式选择：

```text
D：ScriptableObject + EditorWindow + 可导出 JSON
```

---

## 8.1 ScriptableObject

ScriptableObject 用于 Unity Editor 内维护 BuildProfile。

适合：

```text
可视化编辑
版本管理
资源引用
构建配置资产化
```

---

## 8.2 EditorWindow

EditorWindow 用于：

```text
创建 BuildProfile
编辑 BuildProfile
选择当前 BuildProfile
执行构建
生成配置
生成 AB
查看构建报告
```

---

## 8.3 JSON 导出

JSON 用于：

```text
命令行构建
构建服务器
跨项目复用
版本记录
构建报告归档
```

---

# 九、构建报告

BuildProfileSystem 必须生成构建报告。

选择：

```text
A：需要
```

---

## 9.1 构建报告内容

构建报告应包含：

```text
构建时间
构建平台
BuildProfile 名称
客户名称
版本号
输出目录
Unity 版本
构建目标
启用功能
禁用功能
宏定义
场景列表
资源组
AssetBundle 输出路径
AssetBundle 数量
资源总大小
License 模式
FrameworkConfig 输出路径
构建警告
构建错误
构建耗时
```

---

## 9.2 构建报告用途

用于：

```text
客户交付记录
版本追踪
问题排查
自动化构建归档
资源体积分析
功能开关确认
```

---

# 十、命令行构建

BuildProfileSystem 必须支持命令行构建。

选择：

```text
A：需要
```

---

## 10.1 用途

命令行构建用于：

```text
自动化构建
批量客户版本构建
构建服务器
夜间构建
持续集成
```

---

## 10.2 命令行参数方向

命令行构建可支持：

```text
buildProfile
outputPath
buildTarget
version
customer
buildAssetBundle
generateConfig
generateReport
```

本阶段不实现命令行代码，只冻结方向。

---

# 十一、BuildProfile 与 LicenseSystem

BuildProfile 必须和 LicenseSystem 关联。

选择：

```text
A：需要
```

---

## 11.1 关联方式

BuildProfile 可决定：

```text
默认 License 模式
默认启用功能
默认禁用功能
授权功能清单
客户授权类型
试用版 / 正式版
```

---

## 11.2 边界

BuildProfile 只决定构建期默认授权配置。

LicenseSystem 负责运行时授权判断。

正确关系：

```text
BuildProfileSystem
↓
生成默认 License 配置
↓
LicenseSystem
↓
运行时校验授权
```

---

# 十二、BuildProfile 与 FrameworkConfig

BuildProfile 必须和 FrameworkConfig 关联。

选择：

```text
A：需要
```

---

## 12.1 关联方式

BuildProfile 可生成默认配置文件：

```text
framework_config.json
display_config.json
resource_config.json
assetbundle_config.json
network_config.json
ui_config.json
theme_config.json
localization_config.json
save_config.json
license_config.json
```

---

## 12.2 输出位置

默认输出到：

```text
StreamingAssets/ByFramework/Config/
```

也可输出到：

```text
BuildOutput/Config/
```

---

## 12.3 边界

BuildProfile 负责构建期生成默认配置。

FrameworkConfig 负责运行时加载、合并和校验配置。

---

# 十三、现场修改规则

BuildProfile 是否允许现场修改选择：

```text
B：允许部分运行时配置覆盖，但不修改 BuildProfile 本身
```

---

## 13.1 不允许现场修改 BuildProfile

BuildProfile 是构建期概念。

现场人员不应直接修改 BuildProfile。

打包后也不应依赖 BuildProfile 资产进行运行时逻辑判断。

---

## 13.2 允许现场覆盖配置

现场人员可以通过：

```text
RuntimeConfigUI
外部配置文件
PersistentDataPath 配置
```

覆盖部分运行时配置。

例如：

```text
服务器 IP
显示布局
是否启用 VR
AB 路径
资源服务器地址
语言
主题
日志等级
```

---

## 13.3 正确关系

```text
BuildProfile
↓
生成默认配置
↓
FrameworkConfig
↓
现场配置覆盖
```

---

# 十四、BuildProfile 与 FeatureModule

BuildProfile 可以影响 FeatureModule 的构建期启用状态。

例如：

```text
VehicleSimulation
TrainingSystem
DeviceIntegration
SimulationSync
SimulationServerFeature
```

但 BuildProfile 不写业务逻辑。

它只决定：

```text
是否包含
是否启用默认配置
是否生成对应资源
```

---

# 十五、BuildProfile 与 DisplaySystem

BuildProfile 可影响：

```text
默认 DisplayProfile
是否包含多屏配置
是否包含 VR 配置
是否包含驾驶舱布局
是否启用 RuntimeConfigUI 显示配置
```

---

# 十六、BuildProfile 与 ResourceSystem

BuildProfile 可影响：

```text
默认资源 Provider
资源组裁剪
资源路径配置
Manifest 生成
远程更新配置
AssetBundle 构建配置
```

---

# 十七、BuildProfile 与 NetworkSystem

BuildProfile 可影响：

```text
是否启用 Client
是否启用 Server
默认服务器地址
默认端口
是否启用 Downloader
是否启用 SimulationServer 基础网络配置
```

但 NetworkSystem 运行时连接逻辑不属于 BuildProfile。

---

# 十八、BuildProfile 与 UISystem / UIThemeSystem

BuildProfile 可影响：

```text
默认主题
客户主题
UI 资源组
RuntimeConfigUI 是否启用
大屏主题
驾驶舱主题
```

---

# 十九、BuildProfile 与 LocalizationSystem

BuildProfile 可影响：

```text
默认语言
包含哪些语言包
是否包含阿拉伯语语言包
语言包输出路径
```

---

# 二十、BuildProfile 与 SaveSystem

BuildProfile 可影响：

```text
默认保存目录
是否启用备份
是否启用迁移
是否启用配置导入导出
```

但 SaveSystem 运行时保存逻辑不属于 BuildProfile。

---

# 二十一、建议核心对象

后续实现可围绕以下对象设计。

本阶段只冻结概念，不实现代码。

```text
BuildProfile
BuildProfileSystem
BuildProfileConfig
BuildTargetProfile
CustomerProfile
FeatureProfile
ResourceProfile
AssetBundleBuildProfile
LicenseBuildProfile
BuildProfileExporter
BuildProfileValidator
BuildReport
BuildCommandLineArgs
BuildPipelineContext
```

---

# 二十二、建议目录结构

本阶段仅冻结目录方向，不要求创建代码文件。

```text
Assets/ByFramework/Platform/BuildProfileSystem
├─ Runtime
│  ├─ Core
│  │  ├─ BuildProfile
│  │  ├─ BuildProfileConfig
│  │  ├─ BuildTargetProfile
│  │  ├─ CustomerProfile
│  │  ├─ FeatureProfile
│  │  └─ ResourceProfile
│  │
│  └─ Service
│     └─ IBuildProfileService
│
└─ Editor
   ├─ BuildProfileWindow
   ├─ BuildProfileAsset
   ├─ BuildProfileValidator
   ├─ BuildProfileExporter
   ├─ BuildCommandLineBuilder
   ├─ BuildReportWindow
   └─ BuildPipelineContext
```

说明：

```text
BuildProfile 主要是构建期概念
Runtime 部分只保留必要的只读信息或服务接口方向
不应把构建流程写入 Runtime
```

---

# 二十三、配置文件方向

BuildProfile 可导出 JSON。

建议路径：

```text
Assets/ByFramework/Platform/BuildProfileSystem/Profiles/
BuildOutput/BuildProfiles/
StreamingAssets/ByFramework/Config/
```

配置方向：

```text
buildProfileName
customer
version
buildTarget
outputPath
enabledFeatures
disabledFeatures
resourceGroups
assetBundleProfile
licenseProfile
frameworkConfigOutput
generateAssetBundle
generateConfig
generateReport
```

本阶段不冻结具体 JSON Schema。

---

# 二十四、Editor 工具方向

BuildProfileSystem 后续应提供 EditorWindow。

工具方向：

```text
BuildProfileWindow
BuildProfileValidatorWindow
BuildReportWindow
BuildCommandLineWindow
```

功能：

```text
创建 BuildProfile
编辑 BuildProfile
选择当前 BuildProfile
校验 BuildProfile
导出 JSON
生成默认配置
执行构建
构建 AssetBundle
生成构建报告
打开输出目录
```

所有 Editor UI 遵守中文化规范。

示例：

```text
菜单：ByFramework/平台/BuildProfile 构建配置
按钮：创建 BuildProfile
按钮：执行构建
按钮：生成默认配置
按钮：构建 AssetBundle
按钮：导出 JSON
提示：请选择构建目标
日志：[BuildProfile] 构建配置校验通过
```

---

# 二十五、构建校验

BuildProfileSystem 必须支持构建前校验。

校验内容：

```text
BuildTarget 是否有效
输出路径是否有效
客户信息是否完整
版本号是否填写
功能裁剪是否冲突
资源组是否存在
AssetBundle 配置是否存在
License 配置是否存在
FrameworkConfig 输出路径是否有效
场景列表是否有效
```

---

# 二十六、错误与降级策略

BuildProfileSystem 应提供明确错误。

常见错误：

```text
BuildProfile 不存在
BuildTarget 不支持
输出目录不可写
资源组不存在
AssetBundleProfile 不存在
LicenseProfile 缺失
FrameworkConfig 生成失败
命令行参数无效
构建报告生成失败
```

构建期错误一般不应自动降级。

应明确提示并阻止构建，避免生成错误交付包。

---

# 二十七、PlatformServiceRegistry 接入方向

BuildProfileSystem 后续可注册为 Platform 服务。

接口方向：

```text
IBuildProfileService
```

注册方向：

```text
PlatformServiceRegistry.Register<IBuildProfileService>(buildProfileService)
```

获取方向：

```text
PlatformServiceRegistry.Get<IBuildProfileService>()
PlatformServiceRegistry.TryGet<IBuildProfileService>(out buildProfileService)
```

注意：

```text
BuildProfile 主要服务 Editor / 构建期
Runtime 仅保留必要只读访问
```

本阶段不实现注册代码，只冻结方向。

---

# 二十八、禁止事项

P3.15 阶段禁止：

```text
实现 BuildProfile EditorWindow
实现 BuildPipeline 构建代码
实现 AssetBundle 构建调用
实现功能裁剪代码
实现资源裁剪代码
实现命令行构建代码
实现构建报告生成代码
实现 License 配置生成代码
实现 FrameworkConfig 文件生成代码
修改 FrameworkEntry 生命周期
扩展 Core/Config/FrameworkConfig.cs
进入 BuildProfileSystem Implementation
进入 AssetBundle Implementation
进入 ResourceSystem Runtime Implementation
进入 LicenseSystem Implementation
```

---

# 二十九、P3.15 输出物

P3.15 应输出：

```text
Documentation/29_BuildProfileSystemFoundation.md
Documentation/00_ByFramework_Current_Context.md 更新
02_Documentation/02_Roadmap.md 更新
03_Documentation/03_Todo.md 更新
Documentation/04_Changelog.md 更新
```

不要求输出 Runtime 或 Editor 代码。

---

# 三十、阶段关闭条件

P3.15 关闭条件：

```text
BuildProfileSystem 职责明确
BuildProfileSystem 边界明确
平台 / 客户 / 功能 / 资源差异支持明确
参与 AssetBundle 构建规则明确
功能裁剪规则明确
资源裁剪规则明确
ScriptableObject + EditorWindow + JSON 导出配置形式明确
构建报告规则明确
命令行构建方向明确
BuildProfile 与 LicenseSystem 关系明确
BuildProfile 与 FrameworkConfig 关系明确
现场只允许运行时配置覆盖、不修改 BuildProfile 本身的规则明确
BuildProfile 与 ResourceSystem / AssetBundle / FeatureModule / DisplaySystem / NetworkSystem 等关系明确
禁止事项写入文档
Current_Context 更新完成
Roadmap / Todo / Changelog 同步完成
```

满足以上条件后，P3.15 可关闭。

下一阶段建议进入：

```text
P3.16 LicenseSystem Foundation
```

---

# 三十一、最终结论

P3.15 BuildProfileSystem Foundation 是设计冻结阶段。

它只确定：

```text
BuildProfileSystem 是什么
BuildProfileSystem 管什么
BuildProfileSystem 不管什么
如何区分平台
如何区分客户
如何控制功能裁剪
如何控制资源裁剪
如何参与 AssetBundle 构建
如何生成默认配置
如何关联 License
如何支持命令行构建
现场配置如何覆盖构建默认值
后续 Editor / 构建实现应遵守什么边界
```

不得在本阶段进入具体 Runtime、Editor 或 BuildPipeline 实现。

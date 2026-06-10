# P3.12 FrameworkConfig Foundation 设计冻结

> 阶段性质：设计冻结阶段  
> 不是 Runtime 实现阶段  
> 不创建 FrameworkConfig Runtime 脚本  
> 不实现配置加载、合并、校验、热重载、RuntimeConfigUI、导入导出或备份恢复代码  

---

# 一、阶段目标

P3.12 的目标是冻结 FrameworkConfig Foundation 的基础设计，为后续框架配置系统、现场配置、Editor 配置、RuntimeConfigUI、模块配置合并、配置校验和配置热重载提供明确边界。

本阶段只允许完成：

```text
FrameworkConfig 职责冻结
FrameworkConfig 边界冻结
配置来源规则冻结
配置格式规则冻结
配置优先级冻结
分模块配置规则冻结
RuntimeConfigUI 关系冻结
配置校验规则冻结
配置导入导出规则冻结
备份恢复归属冻结
热重载规则冻结
旧 Core/Config/FrameworkConfig.cs 兼容策略冻结
FrameworkConfig 与 SaveSystem / UISystem / DisplaySystem / ResourceSystem / NetworkSystem 等模块关系冻结
```

本阶段不进入具体运行时代码实现。

---

# 二、架构位置

FrameworkConfig 属于：

```text
Platform/FrameworkConfig
```

FrameworkConfig 不属于 Core。

FrameworkConfig 不属于 FeatureModule。

FrameworkConfig 是 Platform 层的统一配置基础模块。

---

# 三、FrameworkConfig 定位

FrameworkConfig 是 ByFramework 的统一配置入口。

它负责统一管理：

```text
默认内置配置
Editor 生成配置
StreamingAssets 部署配置
PersistentDataPath 现场配置
命令行覆盖配置
远程配置预留
配置加载
配置合并
配置校验
配置导入
配置导出
配置热重载通知
```

FrameworkConfig 的核心目标是：

```text
让框架和 Platform 模块通过统一配置模型读取最终配置，而不需要各模块自行处理配置来源、优先级和合并规则。
```

---

# 四、配置来源

FrameworkConfig 必须支持：

```text
Editor 配置
StreamingAssets 配置
PersistentDataPath 配置
命令行参数
```

同时预留：

```text
远程配置
```

选择：

```text
C + 预留 D
```

---

## 4.1 默认内置配置

默认内置配置用于：

```text
框架最小可运行默认值
模块默认参数
缺失配置时的兜底
```

默认内置配置不应承载客户现场配置。

---

## 4.2 Editor 配置

Editor 配置用于 Unity Editor 内的可视化配置。

形式可包括：

```text
ScriptableObject
EditorWindow
Inspector
JSON 生成工具
```

用途：

```text
生成默认配置
配置模块启用状态
配置默认路径
配置默认主题
配置默认语言
配置默认显示方案
配置默认资源策略
配置默认网络参数
```

---

## 4.3 StreamingAssets 部署配置

StreamingAssets 配置用于打包后随程序一起发布的部署配置。

建议路径：

```text
StreamingAssets/ByFramework/Config/
```

用途：

```text
现场初始配置
客户部署配置
默认服务器地址
默认显示布局
默认资源路径
默认 AB 路径
默认语言主题
```

StreamingAssets 配置适合作为现场可见、可复制、可修改的初始配置。

---

## 4.4 PersistentDataPath 现场配置

PersistentDataPath 配置用于程序运行后的现场修改配置。

建议路径：

```text
PersistentDataPath/ByFramework/Config/
```

用途：

```text
RuntimeConfigUI 修改后的配置
现场人员调整后的配置
用户配置
客户现场保存配置
运行后覆盖配置
```

PersistentDataPath 配置优先级高于 StreamingAssets 配置。

---

## 4.5 命令行参数

命令行参数用于临时覆盖配置。

适合：

```text
临时指定服务器 IP
临时指定端口
临时指定显示模式
临时指定配置文件路径
临时指定资源路径
调试启动参数
```

命令行参数优先级最高。

---

## 4.6 远程配置预留

远程配置当前阶段不实现。

只预留方向。

未来可用于：

```text
局域网配置服务器
客户配置中心
集中部署配置
批量设备配置
```

远程配置必须是可选能力，不得影响离线现场运行。

---

# 五、配置文件格式

FrameworkConfig 必须支持多格式扩展，默认使用 JSON。

选择：

```text
D：多格式支持，默认 JSON
```

---

## 5.1 JSON

JSON 是默认格式。

适合：

```text
现场人员阅读
现场人员修改
Editor 生成
RuntimeConfigUI 保存
版本迁移
导入导出
```

---

## 5.2 INI

INI 可作为后期扩展。

适合：

```text
简单现场配置
服务器 IP / 端口
路径配置
开关配置
```

---

## 5.3 XML

XML 可作为后期扩展。

适合：

```text
已有客户系统兼容
第三方工具兼容
```

---

## 5.4 自定义格式

自定义格式用于：

```text
客户特定配置格式
加密配置
二进制配置
第三方配置源
```

---

# 六、配置优先级

FrameworkConfig 必须采用以下优先级：

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

选择：

```text
A：是
```

---

## 6.1 合并原则

低优先级提供默认值。

高优先级覆盖低优先级。

缺失字段保留低优先级值。

错误字段应产生校验警告或错误。

---

## 6.2 示例

例如服务器 IP：

```text
默认内置配置：127.0.0.1
Editor 生成配置：192.168.1.10
StreamingAssets 配置：192.168.1.20
PersistentDataPath 配置：192.168.1.30
命令行参数：192.168.1.40
```

最终结果：

```text
192.168.1.40
```

---

# 七、分模块配置

FrameworkConfig 必须支持：

```text
总配置 + 分模块配置覆盖
```

选择：

```text
C：两者都支持
```

---

## 7.1 总配置

总配置可用于：

```text
framework_config.json
```

包含：

```text
框架启动配置
模块启用状态
配置目录
日志等级
默认 Profile
```

---

## 7.2 分模块配置

建议分模块配置：

```text
network_config.json
display_config.json
resource_config.json
assetbundle_config.json
save_config.json
ui_config.json
theme_config.json
input_config.json
localization_config.json
buildprofile_config.json
license_config.json
```

---

## 7.3 配置关系

总配置负责：

```text
框架级配置
模块启用状态
配置索引
默认路径
```

分模块配置负责：

```text
具体模块参数
```

---

# 八、RuntimeConfigUI

FrameworkConfig 必须支持 RuntimeConfigUI。

选择：

```text
A：需要
```

---

## 8.1 RuntimeConfigUI 定位

RuntimeConfigUI 是打包后给现场人员使用的可视化配置界面。

它属于 FrameworkConfig 的运行时配置能力。

但显示界面依赖：

```text
UISystem
```

关系：

```text
FrameworkConfig
↓
RuntimeConfigUI 数据逻辑
↓
UISystem 展示界面
```

---

## 8.2 RuntimeConfigUI 可配置内容

RuntimeConfigUI 可配置：

```text
服务器 IP
端口
显示模式
DisplayProfile
是否启用 VR
AssetBundle 路径
资源服务器地址
下载缓存路径
默认语言
默认主题
输入设备配置
串口号
日志等级
```

---

## 8.3 RuntimeConfigUI 不负责

RuntimeConfigUI 不负责：

```text
业务规则
训练流程
车辆控制
网络连接实现
资源加载实现
显示系统实现
```

它只负责展示和修改配置。

---

# 九、配置校验

FrameworkConfig 必须支持配置校验。

选择：

```text
A：需要
```

---

## 9.1 校验内容

校验内容包括：

```text
IP 是否合法
端口是否有效
路径是否存在
路径是否可读
路径是否可写
显示器编号是否合法
DisplayProfile 是否存在
AB 路径是否存在
资源 Manifest 是否存在
语言 Key 是否存在
主题是否存在
布尔开关是否合法
枚举值是否合法
必填字段是否缺失
```

---

## 9.2 校验级别

建议支持：

```text
Info
Warning
Error
Fatal
```

说明：

```text
Info      普通提示
Warning   配置可用但存在风险
Error     配置项错误，相关功能不可用
Fatal     关键配置错误，可能阻止启动
```

---

## 9.3 校验责任

FrameworkConfig 负责统一调度校验。

各模块可提供自己的配置校验器。

例如：

```text
DisplaySystem 提供 DisplayConfigValidator
ResourceSystem 提供 ResourceConfigValidator
NetworkSystem 提供 NetworkConfigValidator
SaveSystem 提供 SaveConfigValidator
```

FrameworkConfig 汇总校验结果。

---

# 十、导入 / 导出

FrameworkConfig 必须支持配置导入和导出。

选择：

```text
A：需要
```

---

## 10.1 导出用途

用于：

```text
现场配置备份
客户迁移
多台机器复制配置
问题排查
技术支持
版本升级
```

---

## 10.2 导入用途

用于：

```text
恢复客户配置
批量部署配置
导入技术支持提供的配置
导入旧版本配置
```

---

## 10.3 导入校验

导入配置后必须执行校验。

若存在错误，应提示：

```text
配置导入成功但存在警告
配置导入失败
配置字段不兼容
配置版本需要迁移
```

---

# 十一、备份 / 恢复

配置备份与恢复由 SaveSystem 负责，FrameworkConfig 调用 SaveSystem。

选择：

```text
C：由 SaveSystem 负责，FrameworkConfig 只调用
```

---

## 11.1 正确关系

```text
FrameworkConfig
↓
请求备份 / 恢复配置
↓
SaveSystem
↓
执行备份 / 恢复
```

FrameworkConfig 不重复实现备份恢复逻辑。

---

## 11.2 备份时机

建议方向：

```text
修改配置前备份
导入配置前备份
版本迁移前备份
恢复默认配置前备份
```

---

# 十二、热重载

FrameworkConfig 支持部分热重载。

选择：

```text
B：部分支持
```

---

## 12.1 可热重载配置

建议支持热重载：

```text
主题
语言
日志等级
部分 UI 配置
部分显示配置
资源路径配置
网络地址配置
```

---

## 12.2 不建议热重载配置

不建议热重载：

```text
Core 生命周期配置
FrameworkEntry 配置
关键 Platform 服务启用状态
部分资源 Provider 结构
BuildProfile
License 基础授权配置
```

这些配置修改后应提示：

```text
需要重启后生效
```

---

## 12.3 热重载通知

FrameworkConfig 可发布配置变更通知。

例如：

```text
ConfigChanged
ConfigReloaded
ConfigValidationFailed
```

但 FrameworkConfig 不直接执行业务逻辑。

各模块决定如何响应配置变化。

---

# 十三、配置版本与迁移

FrameworkConfig 配置应支持版本字段。

字段方向：

```text
configVersion
schemaVersion
frameworkVersion
createdTime
modifiedTime
```

配置迁移由 SaveSystem 的迁移能力或 FrameworkConfig 的配置迁移器协同完成。

本阶段只冻结方向。

---

# 十四、配置 Provider 模型

FrameworkConfig 可采用 Provider 模型。

建议 Provider：

```text
DefaultConfigProvider
EditorConfigProvider
StreamingAssetsConfigProvider
PersistentConfigProvider
CommandLineConfigProvider
RemoteConfigProvider
```

---

## 14.1 Provider 职责

Provider 负责从不同来源读取配置。

FrameworkConfig 负责合并。

各模块消费最终配置。

---

# 十五、旧 Core/Config/FrameworkConfig.cs 兼容策略

当前项目中存在：

```text
Core/Config/FrameworkConfig.cs
```

它属于早期兼容代码。

处理方式选择：

```text
D：暂时保留兼容，文档明确不再作为最终配置中心
```

---

## 15.1 保留原则

暂时保留该文件以避免破坏现有代码。

但禁止继续扩展为万能配置中心。

---

## 15.2 禁止新增内容

禁止继续向 Core/Config/FrameworkConfig.cs 添加：

```text
UI 配置
Network 配置
Download 配置
Display 配置
Resource 配置
Save 配置
License 配置
BuildProfile 配置
FeatureModule 配置
```

---

## 15.3 最终迁移方向

最终配置中心应迁移到：

```text
Platform/FrameworkConfig
```

Core 不应承担 Platform 配置职责。

未来 Runtime 实现阶段可逐步：

```text
保留旧字段兼容
标记 Obsolete
引导迁移到 Platform/FrameworkConfig
最终移除或仅保留最小 Core 启动配置
```

---

# 十六、FrameworkConfig 与 PlatformServiceRegistry 的关系

FrameworkConfig 后续可作为 Platform 服务注册。

接口方向：

```text
IFrameworkConfigService
```

注册方向：

```text
PlatformServiceRegistry.Register<IFrameworkConfigService>(configService)
```

获取方向：

```text
PlatformServiceRegistry.Get<IFrameworkConfigService>()
PlatformServiceRegistry.TryGet<IFrameworkConfigService>(out configService)
```

本阶段不实现注册代码，只冻结接入方向。

---

# 十七、FrameworkConfig 与 UISystem 的关系

RuntimeConfigUI 需要 UISystem 展示。

关系：

```text
FrameworkConfig
↓
RuntimeConfigUI
↓
UISystem
```

FrameworkConfig 不直接创建 UI GameObject。

UISystem 负责界面展示。

---

# 十八、FrameworkConfig 与 DisplaySystem 的关系

FrameworkConfig 提供 DisplayConfig。

DisplaySystem 消费 DisplayConfig。

FrameworkConfig 不负责实际显示切换。

DisplaySystem 负责应用显示配置。

---

# 十九、FrameworkConfig 与 ResourceSystem 的关系

FrameworkConfig 提供 ResourceConfig 和 AssetBundleConfig。

ResourceSystem 消费配置。

FrameworkConfig 不负责加载资源。

---

# 二十、FrameworkConfig 与 SaveSystem 的关系

SaveSystem 负责保存现场配置、备份配置、恢复配置。

FrameworkConfig 负责读取最终合并配置。

二者关系：

```text
FrameworkConfig
↓
读取 / 合并配置

SaveSystem
↓
保存 / 备份 / 恢复配置
```

---

# 二十一、FrameworkConfig 与 NetworkSystem 的关系

FrameworkConfig 提供 NetworkConfig。

NetworkSystem 消费 NetworkConfig。

FrameworkConfig 不负责连接服务器。

---

# 二十二、FrameworkConfig 与 InputSystem 的关系

FrameworkConfig 提供 InputConfig。

InputSystem 消费 InputConfig。

FrameworkConfig 不负责采集输入。

---

# 二十三、FrameworkConfig 与 LocalizationSystem 的关系

FrameworkConfig 提供默认语言和语言配置路径。

LocalizationSystem 消费配置。

FrameworkConfig 不负责语言翻译。

---

# 二十四、FrameworkConfig 与 LicenseSystem 的关系

FrameworkConfig 可提供 License 配置路径、授权模式等基础配置。

LicenseSystem 负责授权判断。

FrameworkConfig 不负责授权校验。

---

# 二十五、FrameworkConfig 与 BuildProfileSystem 的关系

BuildProfileSystem 可生成或影响 Editor 配置。

FrameworkConfig 可读取 BuildProfile 生成的默认配置。

关系：

```text
BuildProfileSystem
↓
生成默认配置
↓
FrameworkConfig
↓
运行时加载配置
```

---

# 二十六、建议核心对象

后续 Runtime 可围绕以下对象设计。

本阶段只冻结概念，不实现代码。

```text
FrameworkConfigService
FrameworkConfig
ConfigSource
ConfigProvider
ConfigMerger
ConfigValidator
ConfigValidationResult
ConfigImportExport
ConfigReloadWatcher
ConfigChangeEvent
ConfigProfile
ModuleConfig
RuntimeConfigUIModel
```

---

# 二十七、建议目录结构

本阶段仅冻结目录方向，不要求创建代码文件。

```text
Assets/ByFramework/Platform/FrameworkConfig
├─ Runtime
│  ├─ Core
│  │  ├─ FrameworkConfig
│  │  ├─ ModuleConfig
│  │  ├─ ConfigSource
│  │  └─ ConfigProfile
│  │
│  ├─ Provider
│  │  ├─ DefaultConfigProvider
│  │  ├─ StreamingAssetsConfigProvider
│  │  ├─ PersistentConfigProvider
│  │  ├─ CommandLineConfigProvider
│  │  └─ RemoteConfigProvider
│  │
│  ├─ Merge
│  │  └─ ConfigMerger
│  │
│  ├─ Validation
│  │  ├─ ConfigValidator
│  │  └─ ConfigValidationResult
│  │
│  ├─ ImportExport
│  │  └─ ConfigImportExport
│  │
│  ├─ Reload
│  │  ├─ ConfigReloadWatcher
│  │  └─ ConfigChangeEvent
│  │
│  ├─ RuntimeUI
│  │  ├─ RuntimeConfigUIModel
│  │  └─ RuntimeConfigUIViewModel
│  │
│  └─ Service
│     └─ IFrameworkConfigService
│
└─ Editor
   ├─ FrameworkConfigWindow
   ├─ ModuleConfigWindow
   ├─ ConfigValidationWindow
   ├─ ConfigImportExportWindow
   └─ RuntimeConfigPreviewWindow
```

---

# 二十八、配置文件方向

建议配置目录：

```text
StreamingAssets/ByFramework/Config/
PersistentDataPath/ByFramework/Config/
```

建议配置文件：

```text
framework_config.json
network_config.json
display_config.json
resource_config.json
assetbundle_config.json
save_config.json
ui_config.json
theme_config.json
input_config.json
localization_config.json
buildprofile_config.json
license_config.json
```

本阶段不冻结具体 JSON Schema。

---

# 二十九、Editor 工具方向

FrameworkConfig 后续应提供 EditorWindow。

工具方向：

```text
FrameworkConfigWindow
ModuleConfigWindow
ConfigValidationWindow
ConfigImportExportWindow
RuntimeConfigPreviewWindow
```

功能：

```text
编辑默认配置
编辑模块配置
生成 StreamingAssets 配置
校验配置
导入配置
导出配置
预览 RuntimeConfigUI 配置项
```

所有 Editor UI 遵守中文化规范。

示例：

```text
菜单：ByFramework/平台/FrameworkConfig 配置
按钮：保存配置
按钮：导出配置
按钮：校验配置
提示：配置校验通过
日志：[FrameworkConfig] 配置导出完成
```

---

# 三十、禁止事项

P3.12 阶段禁止：

```text
实现 FrameworkConfig Runtime 服务
实现配置读取代码
实现配置合并代码
实现配置校验代码
实现命令行解析代码
实现 RuntimeConfigUI
实现配置导入导出
实现热重载监听
实现远程配置
实现 EditorWindow 工具
扩展 Core/Config/FrameworkConfig.cs
修改 FrameworkEntry 生命周期
进入 SaveSystem Runtime Implementation
进入 ResourceSystem Runtime Implementation
进入 NetworkSystem Implementation
```

---

# 三十一、P3.12 输出物

P3.12 应输出：

```text
Documentation/FrameworkConfigFoundation.md
Documentation/ByFramework_Current_Context.md 更新
Roadmap.md 更新
Todo.md 更新
Documentation/Changelog.md 更新
```

不要求输出 Runtime 代码。

---

# 三十二、阶段关闭条件

P3.12 关闭条件：

```text
FrameworkConfig 职责明确
FrameworkConfig 边界明确
配置来源明确
默认 JSON / 多格式扩展明确
配置优先级明确
总配置 + 分模块配置规则明确
RuntimeConfigUI 关系明确
配置校验规则明确
导入导出规则明确
备份恢复归属明确
部分热重载规则明确
远程配置预留明确
旧 Core/Config/FrameworkConfig.cs 兼容策略明确
FrameworkConfig 与各 Platform 模块关系明确
禁止事项写入文档
Current_Context 更新完成
Roadmap / Todo / Changelog 同步完成
```

满足以上条件后，P3.12 可关闭。

下一阶段建议进入：

```text
P3.13 LocalizationSystem Foundation
```

---

# 三十三、最终结论

P3.12 FrameworkConfig Foundation 是设计冻结阶段。

它只确定：

```text
FrameworkConfig 是什么
FrameworkConfig 管什么
FrameworkConfig 不管什么
配置从哪里来
配置如何合并
配置如何校验
配置如何导入导出
配置如何现场修改
哪些配置可以热重载
旧 Core 配置如何兼容
后续 Runtime / Editor 实现应遵守什么边界
```

不得在本阶段进入具体 Runtime 或 Editor 实现。

# P3.11 SaveSystem Foundation 设计冻结

> 阶段性质：设计冻结阶段  
> 不是 Runtime 实现阶段  
> 不创建 SaveSystem Runtime 脚本  
> 不实现 JSON / Binary / SQLite Provider、异步保存、加密、版本迁移、备份恢复或可视化工具代码  

---

# 一、阶段目标

P3.11 的目标是冻结 SaveSystem Foundation 的基础设计，为后续 SaveSystem Runtime Implementation、配置保存、用户数据保存、训练记录保存、项目数据保存、缓存数据保存和可视化维护工具提供明确边界。

本阶段只允许完成：

```text
SaveSystem 职责冻结
SaveSystem 边界冻结
保存数据类型冻结
多格式 Provider 模型冻结
SaveScope 模型冻结
保存位置规则冻结
现场可修改规则冻结
加密预留规则冻结
版本迁移规则冻结
备份与恢复规则冻结
异步保存方向冻结
Editor / Runtime 可视化工具方向冻结
SaveSystem 与 FrameworkConfig / RuntimeConfigUI / FeatureModule 的关系冻结
```

本阶段不进入具体运行时代码实现。

---

# 二、架构位置

SaveSystem 属于：

```text
Platform/SaveSystem
```

SaveSystem 不属于 Core。

SaveSystem 不属于 FeatureModule。

SaveSystem 是 Platform 层的通用数据保存能力模块。

---

# 三、SaveSystem 定位

SaveSystem 是 ByFramework 的统一保存与读取系统。

它负责统一：

```text
保存
读取
删除
判断存在
备份
恢复
版本迁移
加密预留
异步保存
多格式存储
多位置存储
保存范围管理
```

SaveSystem 的核心目标是：

```text
让业务模块通过统一 SaveKey / SaveScope 保存和读取数据，而不关心数据最终存储在 JSON、Binary、SQLite 还是自定义 Provider 中。
```

---

# 四、保存数据类型

SaveSystem 必须支持多类型数据。

选择：

```text
D：全部支持
```

包括：

```text
用户设置
训练记录
项目数据
配置数据
缓存数据
日志索引
运行记录
设备参数
显示布局
输入映射
语言选择
主题选择
资源路径配置
网络配置
```

---

## 4.1 用户设置

例如：

```text
语言
主题
输入偏好
显示偏好
音量
上次打开项目
```

---

## 4.2 训练记录

例如：

```text
训练开始时间
训练结束时间
训练成绩
错误记录
操作记录
评分结果
回放索引
```

训练记录属于业务数据。

SaveSystem 只负责保存，不理解训练含义。

---

## 4.3 项目数据

例如：

```text
项目状态
场景配置
客户配置
运行参数
业务记录
```

项目数据由 FeatureModule 定义结构。

SaveSystem 不理解业务含义。

---

## 4.4 配置数据

例如：

```text
network_config
display_config
resource_config
ui_config
theme_config
input_config
```

配置数据可以允许现场人员直接修改。

---

## 4.5 缓存数据

例如：

```text
下载缓存索引
资源缓存索引
临时状态
上次运行记录
```

缓存数据可以清理。

---

# 五、保存格式

SaveSystem 必须支持多格式。

选择：

```text
D：JSON / Binary / SQLite / 自定义 Provider
```

默认格式建议：

```text
JSON
```

---

## 5.1 JSON

JSON 适合：

```text
配置数据
用户设置
现场可修改数据
调试可读数据
```

优点：

```text
可读
可编辑
便于现场维护
便于版本迁移
```

---

## 5.2 Binary

Binary 适合：

```text
体积较大数据
不希望人工修改的数据
性能敏感数据
```

---

## 5.3 SQLite

SQLite 适合：

```text
训练记录
运行记录
大量结构化记录
查询型数据
日志索引
```

---

## 5.4 自定义 Provider

自定义 Provider 用于：

```text
客户自定义格式
第三方存储
加密存储
网络存储
特殊行业存储
```

---

# 六、Provider 模型

SaveSystem 采用 Provider 模型。

建议 Provider：

```text
JsonSaveProvider
BinarySaveProvider
SQLiteSaveProvider
CustomSaveProvider
```

Provider 负责具体存储格式。

SaveSystem 负责统一调度。

---

## 6.1 Provider 不负责

Provider 不负责：

```text
业务解释
训练评分
车辆逻辑
设备逻辑
显示逻辑
网络逻辑
```

Provider 只负责数据读写。

---

# 七、SaveKey

SaveKey 是保存数据的统一标识。

示例：

```text
config.network
config.display
config.resource
config.ui
user.preference
training.record
project.state
device.parameter
cache.download
```

SaveKey 不应直接等于物理路径。

SaveSystem 根据 SaveKey、SaveScope 和 SaveProfile 解析最终存储位置。

---

# 八、SaveScope

SaveSystem 必须支持 SaveScope。

选择：

```text
A：需要
```

建议 SaveScope：

```text
Global
User
Project
Runtime
Cache
Training
Device
Config
```

---

## 8.1 Global

全局数据。

例如：

```text
框架全局设置
默认语言
默认主题
```

---

## 8.2 User

用户数据。

例如：

```text
用户偏好
用户输入配置
用户显示配置
```

---

## 8.3 Project

项目数据。

例如：

```text
项目运行状态
项目业务配置
项目场景选择
```

---

## 8.4 Runtime

运行时数据。

例如：

```text
上次运行状态
临时状态
运行会话数据
```

---

## 8.5 Cache

缓存数据。

例如：

```text
资源缓存索引
下载缓存索引
临时缓存
```

---

## 8.6 Training

训练数据。

例如：

```text
训练记录
训练成绩
训练回放索引
```

---

## 8.7 Device

设备数据。

例如：

```text
串口配置
设备参数
设备校准数据
```

---

## 8.8 Config

配置数据。

例如：

```text
网络配置
显示配置
资源配置
UI 配置
主题配置
```

---

# 九、保存位置

SaveSystem 必须支持多保存位置。

选择：

```text
C：PersistentDataPath / StreamingAssets 初始配置 / 外部自定义路径
```

---

## 9.1 PersistentDataPath

用于运行时写入。

适合：

```text
用户设置
现场修改配置
训练记录
项目数据
缓存数据
```

建议路径：

```text
PersistentDataPath/ByFramework/Save/
PersistentDataPath/ByFramework/Config/
PersistentDataPath/ByFramework/Cache/
PersistentDataPath/ByFramework/Training/
```

---

## 9.2 StreamingAssets

用于部署初始配置。

适合：

```text
初始 network_config
初始 display_config
初始 resource_config
初始 ui_config
初始 theme_config
```

建议路径：

```text
StreamingAssets/ByFramework/Config/
```

注意：

```text
StreamingAssets 适合读取部署配置
不作为主要运行时写入目录
```

---

## 9.3 外部自定义路径

用于现场自定义保存位置。

例如：

```text
D:/ByFrameworkData/
E:/TrainingRecords/
../Config/
./Save/
```

外部路径由 FrameworkConfig 提供。

---

# 十、现场人员直接修改规则

现场人员是否允许直接改保存文件：

```text
C：配置类允许，业务记录类不建议直接改
```

---

## 10.1 允许直接修改

允许直接修改的数据：

```text
网络配置
显示配置
资源配置
UI 配置
主题配置
语言配置
输入配置
AssetBundle 路径配置
```

这些应优先使用 JSON，并提供字段说明。

---

## 10.2 不建议直接修改

不建议直接修改的数据：

```text
训练记录
项目运行记录
设备校准记录
业务状态数据
缓存索引
```

这些数据可通过 Editor 工具或 RuntimeConfigUI 查看、导入、导出、备份、恢复。

---

# 十一、加密策略

SaveSystem 需要预留加密接口。

选择：

```text
C：预留接口，部分数据可加密
```

---

## 11.1 加密适用数据

未来可对以下数据启用加密：

```text
License 相关缓存
客户敏感配置
设备参数
训练成绩
业务记录
```

---

## 11.2 不建议默认加密的数据

不建议默认加密：

```text
现场配置文件
网络配置
显示配置
资源路径配置
UI 配置
主题配置
```

原因：

```text
现场人员需要可读、可修改、可维护
```

---

## 11.3 加密边界

SaveSystem 不应写死某一种加密算法。

应预留抽象方向：

```text
ISaveEncryptor
ISaveDecryptor
```

本阶段不创建接口代码，只冻结方向。

---

# 十二、版本迁移

SaveSystem 必须支持版本迁移。

选择：

```text
A：需要
```

---

## 12.1 版本迁移用途

用于：

```text
旧版本配置升级
旧版本用户设置升级
旧版本训练数据兼容
旧版本项目数据兼容
字段重命名
字段默认值补齐
数据结构变化
```

---

## 12.2 版本字段

保存数据应支持版本信息。

字段方向：

```text
schemaVersion
dataVersion
appVersion
frameworkVersion
createdTime
modifiedTime
```

---

## 12.3 迁移器

后续 Runtime 可设计：

```text
SaveMigration
SaveMigrationStep
SaveMigrationRegistry
```

迁移流程方向：

```text
读取旧数据
识别版本
执行迁移步骤
生成新数据
备份旧数据
保存新数据
```

---

# 十三、备份与恢复

SaveSystem 必须支持备份与恢复。

选择：

```text
A：需要
```

---

## 13.1 备份用途

用于：

```text
配置修改前备份
版本迁移前备份
训练记录备份
客户现场维护
数据恢复
异常回滚
```

---

## 13.2 备份策略

建议支持：

```text
手动备份
自动备份
迁移前备份
覆盖前备份
定期备份预留
```

---

## 13.3 恢复策略

建议支持：

```text
恢复指定备份
恢复最近备份
恢复默认配置
导入外部备份
```

本阶段只冻结方向，不实现代码。

---

# 十四、异步保存

SaveSystem 必须支持异步保存。

选择：

```text
A：需要
```

---

## 14.1 异步保存适用场景

适用于：

```text
训练记录写入
大量项目数据保存
SQLite 写入
Binary 大文件保存
缓存索引保存
```

---

## 14.2 同步保存适用场景

适用于：

```text
小型配置文件
用户设置
启动前关键配置
退出前强制保存
```

---

## 14.3 异步边界

异步保存不应破坏数据一致性。

后续 Runtime 应考虑：

```text
保存队列
取消保存
保存完成回调
保存失败回调
退出时 Flush
```

本阶段不实现代码。

---

# 十五、Editor / Runtime 可视化工具

SaveSystem 需要 Editor 工具和 RuntimeConfigUI 支持。

选择：

```text
C：两者都需要
```

---

## 15.1 Editor 工具

Editor 工具方向：

```text
SaveSystemConfigWindow
SaveDataViewerWindow
SaveBackupWindow
SaveMigrationWindow
SaveProviderWindow
```

用途：

```text
查看保存目录
查看保存文件
清理缓存
导入配置
导出配置
执行备份
执行恢复
测试版本迁移
检查 SaveKey
```

---

## 15.2 RuntimeConfigUI

RuntimeConfigUI 方向：

```text
查看现场配置
修改配置类数据
导入配置
导出配置
恢复默认配置
备份配置
恢复备份
清理缓存
```

RuntimeConfigUI 不建议直接编辑：

```text
训练记录
业务记录
设备校准数据
```

但可以提供查看和导出。

---

# 十六、SaveProfile

SaveProfile 描述保存策略。

字段方向：

```text
profileName
scope
providerType
storagePath
fileName
format
enableEncryption
enableBackup
enableMigration
enableAsync
allowManualEdit
```

SaveProfile 可由 FrameworkConfig 加载。

---

# 十七、SaveResult / SaveError

SaveSystem 应提供明确结果和错误。

SaveResult 方向：

```text
Success
SaveKey
Scope
Path
ErrorCode
ErrorMessage
ProviderType
```

常见错误：

```text
SaveKey 不存在
Provider 不存在
路径不存在
路径不可写
JSON 格式错误
版本迁移失败
备份失败
恢复失败
加密失败
解密失败
SQLite 打开失败
异步保存失败
```

---

# 十八、SaveSystem 与 FrameworkConfig 的关系

FrameworkConfig 负责加载和合并保存配置。

SaveSystem 只消费最终 SaveConfig。

关系：

```text
FrameworkConfig
↓
SaveConfig
↓
SaveSystem
```

SaveSystem 不负责配置优先级合并。

配置优先级由 FrameworkConfig 统一管理：

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

# 十九、SaveSystem 与 RuntimeConfigUI 的关系

RuntimeConfigUI 通过 SaveSystem 保存现场修改后的配置。

关系：

```text
RuntimeConfigUI
↓
FrameworkConfig / SaveSystem
↓
PersistentDataPath 现场配置
```

RuntimeConfigUI 可用于：

```text
保存显示配置
保存网络配置
保存资源路径配置
保存 UI 主题配置
保存输入配置
```

---

# 二十、SaveSystem 与 FeatureModule 的关系

FeatureModule 可以使用 SaveSystem 保存业务数据。

例如：

```text
TrainingSystem 保存训练记录
VehicleSimulation 保存车辆配置
DeviceIntegration 保存设备参数
ScenarioSystem 保存场景状态
```

但 SaveSystem 不理解业务含义。

正确关系：

```text
FeatureModule
↓
ISaveService
↓
SaveSystem
```

---

# 二十一、SaveSystem 与 ResourceSystem 的关系

ResourceSystem 可使用 SaveSystem 保存缓存索引。

例如：

```text
下载缓存索引
AB 缓存记录
资源版本缓存
```

但资源加载逻辑仍属于 ResourceSystem。

SaveSystem 只负责保存索引数据。

---

# 二十二、SaveSystem 与 LicenseSystem 的关系

LicenseSystem 可使用 SaveSystem 保存非敏感授权缓存或状态。

敏感授权数据如果需要保存，应启用加密预留。

SaveSystem 不负责授权判断。

---

# 二十三、SaveSystem 与 NetworkSystem 的关系

NetworkSystem 可使用 SaveSystem 保存：

```text
服务器地址历史
网络配置
下载任务缓存
断点续传记录
```

但网络通信逻辑不属于 SaveSystem。

---

# 二十四、建议核心对象

后续 Runtime 可围绕以下对象设计。

本阶段只冻结概念，不实现代码。

```text
SaveKey
SaveScope
SaveProfile
SaveConfig
SaveData
SaveRequest
SaveResult
SaveProvider
JsonSaveProvider
BinarySaveProvider
SQLiteSaveProvider
CustomSaveProvider
SaveMigration
SaveMigrationStep
SaveBackup
SaveBackupInfo
SaveEncryptor
SaveDecryptor
SaveQueue
```

---

# 二十五、建议目录结构

本阶段仅冻结目录方向，不要求创建代码文件。

```text
Assets/ByFramework/Platform/SaveSystem
├─ Runtime
│  ├─ Core
│  │  ├─ SaveKey
│  │  ├─ SaveScope
│  │  ├─ SaveProfile
│  │  ├─ SaveConfig
│  │  ├─ SaveRequest
│  │  └─ SaveResult
│  │
│  ├─ Provider
│  │  ├─ JsonSaveProvider
│  │  ├─ BinarySaveProvider
│  │  ├─ SQLiteSaveProvider
│  │  └─ CustomSaveProvider
│  │
│  ├─ Migration
│  │  ├─ SaveMigration
│  │  ├─ SaveMigrationStep
│  │  └─ SaveMigrationRegistry
│  │
│  ├─ Backup
│  │  ├─ SaveBackup
│  │  └─ SaveBackupInfo
│  │
│  ├─ Security
│  │  ├─ SaveEncryptor
│  │  └─ SaveDecryptor
│  │
│  ├─ Async
│  │  └─ SaveQueue
│  │
│  └─ Service
│     └─ ISaveService
│
└─ Editor
   ├─ SaveSystemConfigWindow
   ├─ SaveDataViewerWindow
   ├─ SaveBackupWindow
   ├─ SaveMigrationWindow
   └─ SaveProviderWindow
```

---

# 二十六、配置文件方向

SaveSystem 配置建议：

```text
StreamingAssets/ByFramework/Config/save_config.json
PersistentDataPath/ByFramework/Config/save_config.json
```

字段方向：

```text
defaultProvider
defaultSaveRootPath
externalSaveRootPath
enableBackup
enableMigration
enableAsync
enableEncryption
profiles
scopes
providers
```

本阶段不冻结具体 JSON Schema。

---

# 二十七、PlatformServiceRegistry 接入方向

SaveSystem 后续可注册为 Platform 服务。

接口方向：

```text
ISaveService
```

注册方向：

```text
PlatformServiceRegistry.Register<ISaveService>(saveService)
```

获取方向：

```text
PlatformServiceRegistry.Get<ISaveService>()
PlatformServiceRegistry.TryGet<ISaveService>(out saveService)
```

本阶段不实现注册代码，只冻结接入方向。

---

# 二十八、禁止事项

P3.11 阶段禁止：

```text
实现 SaveSystem Runtime 管理器
实现 JSON Provider
实现 Binary Provider
实现 SQLite Provider
实现异步保存队列
实现加密算法
实现版本迁移代码
实现备份恢复代码
实现 EditorWindow 工具
实现 RuntimeConfigUI 保存界面
修改 FrameworkEntry 生命周期
扩展 Core/Config/FrameworkConfig.cs
进入 FrameworkConfig Runtime Implementation
进入 ResourceSystem Runtime Implementation
进入 NetworkSystem Implementation
```

---

# 二十九、P3.11 输出物

P3.11 应输出：

```text
Documentation/SaveSystemFoundation.md
Documentation/ByFramework_Current_Context.md 更新
Roadmap.md 更新
Todo.md 更新
Documentation/Changelog.md 更新
```

不要求输出 Runtime 代码。

---

# 三十、阶段关闭条件

P3.11 关闭条件：

```text
SaveSystem 职责明确
SaveSystem 边界明确
保存数据类型明确
多格式 Provider 模型明确
默认 JSON 方向明确
SaveScope 明确
保存位置规则明确
现场可修改规则明确
加密预留明确
版本迁移明确
备份与恢复明确
异步保存明确
Editor / RuntimeConfigUI 可视化工具方向明确
SaveSystem 与 FrameworkConfig / RuntimeConfigUI / FeatureModule / ResourceSystem / NetworkSystem 的关系明确
禁止事项写入文档
Current_Context 更新完成
Roadmap / Todo / Changelog 同步完成
```

满足以上条件后，P3.11 可关闭。

下一阶段建议进入：

```text
P3.12 FrameworkConfig Foundation
```

---

# 三十一、最终结论

P3.11 SaveSystem Foundation 是设计冻结阶段。

它只确定：

```text
SaveSystem 是什么
SaveSystem 管什么
SaveSystem 不管什么
保存什么数据
保存到哪里
用什么格式保存
哪些数据允许现场修改
如何迁移版本
如何备份恢复
如何支持异步保存
后续 Runtime / Editor 实现应遵守什么边界
```

不得在本阶段进入具体 Runtime 或 Editor 实现。

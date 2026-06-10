# P3.13 LocalizationSystem Foundation 设计冻结

> 阶段性质：设计冻结阶段  
> 不是 Runtime 实现阶段  
> 不创建 LocalizationSystem Runtime 脚本  
> 不实现语言包加载、Key 查询、参数格式化、缺失检查、语言切换或 UI 刷新代码  

---

# 一、阶段目标

P3.13 的目标是冻结 LocalizationSystem Foundation 的基础设计，为后续多语言运行时、语言包管理、外部语言包覆盖、参数格式化、缺失 Key 检查、语言回退和运行时语言切换提供明确边界。

本阶段只允许完成：

```text
LocalizationSystem 职责冻结
LocalizationSystem 边界冻结
多语言范围冻结
语言包格式冻结
外部语言包规则冻结
参数格式化规则冻结
缺失 Key 检查规则冻结
语言回退规则冻结
运行时语言切换规则冻结
UI 文本刷新关系冻结
阿拉伯语插件适配方向冻结
字体归属规则冻结
LocalizationSystem 与 UISystem / UIThemeSystem / FrameworkConfig / ResourceSystem 的关系冻结
```

本阶段不进入具体运行时代码实现。

---

# 二、架构位置

LocalizationSystem 属于：

```text
Platform/LocalizationSystem
```

LocalizationSystem 不属于 Core。

LocalizationSystem 不属于 FeatureModule。

LocalizationSystem 是 Platform 层的通用多语言能力模块。

---

# 三、LocalizationSystem 定位

LocalizationSystem 是 ByFramework 的统一多语言系统。

它负责统一：

```text
语言管理
语言包加载
语言 Key 查询
参数格式化
缺失 Key 检查
语言回退
运行时语言切换
语言变更通知
外部语言包覆盖
```

LocalizationSystem 的核心目标是：

```text
让 UI、日志、配置提示、运行时提示通过统一 LocalizationKey 获取文本，而不直接硬编码多语言内容。
```

---

# 四、多语言范围

多语言范围选择：

```text
B：中文 / 英文为主，预留其它语言
```

同时明确预留：

```text
阿拉伯语
其它扩展语言
```

---

## 4.1 默认重点语言

默认重点支持：

```text
中文
英文
```

中文是 ByFramework 默认团队语言。

Editor UI、MenuItem、EditorWindow、Button、HelpBox、验证报告、Debug.Log、Debug.LogWarning、Debug.LogError 默认使用中文。

技术对象名保留英文。

---

## 4.2 其它语言预留

LocalizationSystem 应允许扩展：

```text
日文
韩文
俄文
法文
德文
阿拉伯文
其它语言
```

但当前阶段不要求完整实现所有语言。

---

## 4.3 阿拉伯语支持

阿拉伯语方向：

```text
可通过插件 arabic-support-unity 支持
```

LocalizationSystem 不应内置复杂阿拉伯文字形处理算法。

正确关系：

```text
LocalizationSystem
↓
提供阿拉伯语文本
↓
ArabicSupportAdapter
↓
arabic-support-unity 插件
↓
UISystem 显示
```

---

# 五、语言包格式

语言包格式选择：

```text
D：多格式支持，Runtime 默认 JSON
```

支持方向：

```text
JSON
CSV
Excel
```

Runtime 默认加载：

```text
JSON
```

---

## 5.1 JSON

JSON 是 Runtime 默认语言包格式。

适合：

```text
运行时读取
现场外部覆盖
StreamingAssets 部署
PersistentDataPath 现场修改
版本管理
```

---

## 5.2 Excel

Excel 适合编辑期维护。

项目已有 Excel 转 JSON 工具，可以继续使用。

因此 LocalizationSystem 不重新设计 Excel 转 JSON 工具链。

---

## 5.3 CSV

CSV 作为可选扩展。

适合：

```text
简单语言表
第三方翻译工具导出
```

---

# 六、Editor 语言包工具

Editor 是否需要语言包工具选择：

```text
B：不需要
```

原因：

```text
项目已有 Excel 转 JSON 工具
```

---

## 6.1 工具链原则

LocalizationSystem 不重新实现 Excel 转 JSON 工具。

后续只需要能读取该工具生成的 JSON。

如未来需要增强，可再增加：

```text
缺失 Key 检查窗口
语言包校验窗口
```

但 P3.13 不设计独立完整语言包 Editor 工具。

---

# 七、Runtime 外部语言包

Runtime 是否允许现场人员改语言包选择：

```text
C：只允许修改外部语言包，不允许修改内置语言包
```

---

## 7.1 内置语言包

内置语言包来源：

```text
项目内置资源
ResourceSystem
AssetBundle
Editor 生成默认语言包
```

内置语言包不建议现场直接修改。

---

## 7.2 外部语言包

允许现场人员修改外部语言包。

建议路径：

```text
StreamingAssets/ByFramework/Localization/
PersistentDataPath/ByFramework/Localization/
```

示例：

```text
StreamingAssets/ByFramework/Localization/zh_cn.json
StreamingAssets/ByFramework/Localization/en_us.json
StreamingAssets/ByFramework/Localization/ar.json
```

现场修改后可保存到：

```text
PersistentDataPath/ByFramework/Localization/
```

---

## 7.3 外部语言包优先级

语言包优先级建议：

```text
PersistentDataPath 外部语言包
>
StreamingAssets 外部语言包
>
AssetBundle 语言包
>
内置默认语言包
```

---

# 八、参数格式化

LocalizationSystem 必须支持参数格式化。

选择：

```text
A：需要
```

---

## 8.1 参数格式化示例

位置参数：

```text
欢迎 {0}
当前速度：{0} km/h
```

命名参数：

```text
当前速度：{speed} km/h
用户：{userName}
```

---

## 8.2 参数格式化方向

建议支持：

```text
位置参数
命名参数
数字格式化预留
日期格式化预留
单位格式化预留
```

本阶段不实现代码，只冻结方向。

---

# 九、缺失 Key 检查

LocalizationSystem 必须支持缺失 Key 检查。

选择：

```text
A：需要
```

---

## 9.1 缺失 Key 类型

需要检查：

```text
当前语言缺失 Key
默认语言缺失 Key
多语言表 Key 不一致
空文本
重复 Key
非法 Key
```

---

## 9.2 缺失 Key 处理

运行时缺失 Key 时，建议显示：

```text
[Missing: ui.xxx]
```

同时输出警告：

```text
[LocalizationSystem] 缺失语言 Key：ui.xxx
```

---

## 9.3 缺失 Key 报告

未来可生成：

```text
LocalizationMissingKeyReport
```

用于检查语言包完整性。

P3.13 不实现报告生成，只冻结方向。

---

# 十、语言回退

LocalizationSystem 必须支持语言回退。

选择：

```text
A：需要
```

---

## 10.1 回退链

建议支持：

```text
当前语言
↓
指定回退语言
↓
默认语言
↓
Key 自身
↓
Missing 标记
```

示例：

```text
ar
↓
en_us
↓
zh_cn
↓
[Missing: key]
```

具体默认回退语言可由 FrameworkConfig 配置。

---

## 10.2 回退策略

回退策略可配置：

```text
fallbackLanguage
defaultLanguage
showMissingKey
logMissingKey
```

---

# 十一、运行时语言切换

LocalizationSystem 必须支持运行时语言切换。

选择：

```text
A：需要
```

---

## 11.1 切换流程方向

运行时切换语言方向：

```text
设置目标语言
加载语言包
校验语言包
更新当前语言
发布语言变更通知
相关模块自行响应
```

---

## 11.2 语言保存

当前语言选择应通过 SaveSystem / FrameworkConfig 保存。

关系：

```text
LocalizationSystem
↓
FrameworkConfig / SaveSystem
↓
保存 currentLanguage
```

---

# 十二、UI 文本刷新

UI 文本是否自动刷新选择：

```text
C：只通知，由 UI 自己刷新
```

---

## 12.1 正确关系

LocalizationSystem 不直接刷新 UI。

正确关系：

```text
LocalizationSystem
↓
发布 LanguageChanged 通知
↓
UISystem / UIView / UIWidget
↓
自行刷新文本
```

---

## 12.2 为什么不直接刷新

原因：

```text
LocalizationSystem 不应依赖 UISystem 具体 UI 对象
不应持有 Text / TMP_Text 引用
不应理解 UI 生命周期
不应污染模块边界
```

---

# 十三、字体归属

字体是否归 LocalizationSystem 管选择：

```text
C：LocalizationSystem 只提供语言信息，字体由 UIThemeSystem 管
```

---

## 13.1 正确关系

```text
LocalizationSystem
负责语言、文本、Key、回退、参数

UIThemeSystem
负责字体、颜色、样式、图标

UISystem
负责 UI 对象和显示
```

---

## 13.2 字体切换方向

当语言切换时：

```text
LocalizationSystem 发布 LanguageChanged
↓
UIThemeSystem 可根据语言选择字体
↓
UISystem 刷新 UI
```

LocalizationSystem 不直接设置字体。

---

# 十四、LanguageCode

LanguageCode 是语言标识。

建议格式：

```text
zh_cn
en_us
ar
ja_jp
ko_kr
```

语言标识应统一小写，避免平台差异。

---

# 十五、LocalizationKey

LocalizationKey 是多语言文本标识。

示例：

```text
ui.main_menu.start
ui.main_menu.exit
ui.settings.language
ui.settings.theme
error.resource.not_found
error.network.timeout
log.event.initialized
```

---

## 15.1 Key 命名规则

建议：

```text
模块.页面.含义
模块.类型.含义
```

示例：

```text
ui.runtime_config.server_ip
display.profile.name
resource.error.ab_path_missing
network.error.connect_failed
```

---

## 15.2 禁止事项

禁止在业务代码长期硬编码：

```text
中文文本
英文文本
```

应优先使用：

```text
LocalizationKey
```

---

# 十六、LanguagePack

LanguagePack 是语言包。

字段方向：

```text
languageCode
languageName
version
fallbackLanguage
entries
```

entries 可为：

```text
key -> text
```

本阶段不冻结具体 JSON Schema。

---

# 十七、LocalizationTable

LocalizationTable 是语言表。

可表示：

```text
某个模块的语言表
某个语言的完整表
某个资源包中的语言表
```

建议支持按模块拆分：

```text
ui.json
error.json
network.json
resource.json
training.json
```

也支持合并总表。

---

# 十八、LocalizationSystem 与 ResourceSystem 的关系

LocalizationSystem 不直接关心语言包来自哪里。

正确关系：

```text
LocalizationSystem
↓
ResourceSystem
↓
Resources / AssetBundle / LocalFile / StreamingAssets
```

语言包可以来自：

```text
内置资源
StreamingAssets
PersistentDataPath
AssetBundle
```

---

# 十九、LocalizationSystem 与 FrameworkConfig 的关系

FrameworkConfig 提供 Localization 配置。

包括：

```text
defaultLanguage
currentLanguage
fallbackLanguage
languagePackPath
externalLanguagePackPath
enableMissingKeyLog
enableRuntimeSwitch
```

LocalizationSystem 消费最终配置。

FrameworkConfig 不负责翻译文本。

---

# 二十、LocalizationSystem 与 SaveSystem 的关系

SaveSystem 可保存：

```text
当前语言
语言偏好
外部语言包修改记录
```

LocalizationSystem 不直接处理保存细节。

---

# 二十一、LocalizationSystem 与 UISystem 的关系

UISystem 使用 LocalizationSystem 获取文本。

关系：

```text
UISystem
↓
LocalizationSystem.GetText(LocalizationKey)
```

语言切换时：

```text
LocalizationSystem
↓
LanguageChanged
↓
UISystem 自行刷新
```

---

# 二十二、LocalizationSystem 与 UIThemeSystem 的关系

UIThemeSystem 根据语言选择字体和样式。

例如：

```text
中文字体
英文字体
阿拉伯语字体
```

LocalizationSystem 只提供当前语言信息。

UIThemeSystem 决定字体。

---

# 二十三、LocalizationSystem 与 FeatureModule 的关系

FeatureModule 可以使用 LocalizationSystem 获取业务提示文本。

例如：

```text
TrainingSystem 获取训练提示
DeviceIntegration 获取设备错误提示
VehicleSimulation 获取车辆状态文本
```

但 LocalizationSystem 不理解业务含义。

---

# 二十四、阿拉伯语适配边界

阿拉伯语适配可通过：

```text
arabic-support-unity
```

LocalizationSystem 不直接实现：

```text
阿拉伯文字形重排
RTL 排版
复杂文字 shaping
```

建议未来通过 Adapter 接入：

```text
ArabicTextAdapter
```

本阶段不创建代码，只冻结方向。

---

# 二十五、建议核心对象

后续 Runtime 可围绕以下对象设计。

本阶段只冻结概念，不实现代码。

```text
LocalizationKey
LanguageCode
LanguagePack
LocalizationTable
LocalizedText
LocalizationConfig
LocalizationProvider
JsonLanguagePackProvider
CsvLanguagePackProvider
ExcelConvertedLanguagePackProvider
LocalizationFormatter
LocalizationFallbackPolicy
LocalizationMissingKeyReport
LanguageChangedEvent
ArabicTextAdapter
```

---

# 二十六、建议目录结构

本阶段仅冻结目录方向，不要求创建代码文件。

```text
Assets/ByFramework/Platform/LocalizationSystem
├─ Runtime
│  ├─ Core
│  │  ├─ LocalizationKey
│  │  ├─ LanguageCode
│  │  ├─ LanguagePack
│  │  ├─ LocalizationTable
│  │  └─ LocalizationConfig
│  │
│  ├─ Provider
│  │  ├─ JsonLanguagePackProvider
│  │  ├─ CsvLanguagePackProvider
│  │  └─ ExcelConvertedLanguagePackProvider
│  │
│  ├─ Format
│  │  └─ LocalizationFormatter
│  │
│  ├─ Fallback
│  │  └─ LocalizationFallbackPolicy
│  │
│  ├─ Report
│  │  └─ LocalizationMissingKeyReport
│  │
│  ├─ Adapter
│  │  └─ ArabicTextAdapter
│  │
│  └─ Service
│     └─ ILocalizationService
│
└─ Editor
   ├─ LocalizationCheckWindow
   └─ LocalizationMissingKeyReportWindow
```

说明：

```text
Editor 工具不是 P3.13 必须实现项
仅预留缺失检查和报告窗口方向
Excel 转 JSON 工具继续使用现有工具链
```

---

# 二十七、配置文件方向

建议配置文件：

```text
StreamingAssets/ByFramework/Config/localization_config.json
PersistentDataPath/ByFramework/Config/localization_config.json
```

建议语言包路径：

```text
StreamingAssets/ByFramework/Localization/
PersistentDataPath/ByFramework/Localization/
```

字段方向：

```text
defaultLanguage
currentLanguage
fallbackLanguage
languagePackPath
externalLanguagePackPath
enableRuntimeSwitch
enableMissingKeyLog
enableFallback
enableArabicSupport
```

本阶段不冻结具体 JSON Schema。

---

# 二十八、PlatformServiceRegistry 接入方向

LocalizationSystem 后续可注册为 Platform 服务。

接口方向：

```text
ILocalizationService
```

注册方向：

```text
PlatformServiceRegistry.Register<ILocalizationService>(localizationService)
```

获取方向：

```text
PlatformServiceRegistry.Get<ILocalizationService>()
PlatformServiceRegistry.TryGet<ILocalizationService>(out localizationService)
```

本阶段不实现注册代码，只冻结接入方向。

---

# 二十九、禁止事项

P3.13 阶段禁止：

```text
实现 LocalizationSystem Runtime 服务
实现语言包加载代码
实现 JSON 解析代码
实现参数格式化代码
实现语言切换代码
实现 UI 文本刷新代码
实现阿拉伯语插件接入代码
实现字体切换代码
实现 EditorWindow 工具
重新设计 Excel 转 JSON 工具
修改 FrameworkEntry 生命周期
扩展 Core/Config/FrameworkConfig.cs
进入 UISystem Runtime Implementation
进入 ResourceSystem Runtime Implementation
进入 FrameworkConfig Runtime Implementation
```

---

# 三十、P3.13 输出物

P3.13 应输出：

```text
Documentation/27_LocalizationSystemFoundation.md
Documentation/00_ByFramework_Current_Context.md 更新
02_Documentation/02_Roadmap.md 更新
03_Documentation/03_Todo.md 更新
Documentation/04_Changelog.md 更新
```

不要求输出 Runtime 代码。

---

# 三十一、阶段关闭条件

P3.13 关闭条件：

```text
LocalizationSystem 职责明确
LocalizationSystem 边界明确
中文 / 英文为主、其它语言预留明确
阿拉伯语通过 arabic-support-unity 插件适配方向明确
Runtime 默认 JSON、多格式支持方向明确
现有 Excel 转 JSON 工具继续复用原则明确
外部语言包可现场修改规则明确
参数格式化明确
缺失 Key 检查明确
语言回退明确
运行时语言切换明确
UI 只接收通知自行刷新规则明确
字体归属 UIThemeSystem 明确
LocalizationSystem 与 UISystem / UIThemeSystem / FrameworkConfig / ResourceSystem / SaveSystem 关系明确
禁止事项写入文档
Current_Context 更新完成
Roadmap / Todo / Changelog 同步完成
```

满足以上条件后，P3.13 可关闭。

下一阶段建议进入：

```text
P3.14 NetworkSystem Foundation
```

---

# 三十二、最终结论

P3.13 LocalizationSystem Foundation 是设计冻结阶段。

它只确定：

```text
LocalizationSystem 是什么
LocalizationSystem 管什么
LocalizationSystem 不管什么
语言包从哪里来
语言 Key 如何组织
如何处理参数
如何处理缺失 Key
如何处理语言回退
如何处理运行时切换
如何适配阿拉伯语
如何与 UIThemeSystem 分工
后续 Runtime 实现应遵守什么边界
```

不得在本阶段进入具体 Runtime 或 Editor 实现。

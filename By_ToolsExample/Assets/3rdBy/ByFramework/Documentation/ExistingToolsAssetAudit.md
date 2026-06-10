# P3.22 ExistingToolsAssetAudit 设计冻结

> 阶段性质：历史工具资产审计设计冻结  
> 不是 Runtime 实现阶段  
> 不改造现有工具代码  
> 不迁移历史工具代码  
> 不把历史工具直接纳入 Runtime  
> 本阶段只冻结现有工具资产的分类、归属、复用方式、版本记录和审计规则  

---

# 一、阶段目标

P3.22 的目标是冻结 ByFramework 现有工具资产的审计规则，避免 Codex 后续重复开发、错误重写、错误迁移或破坏已有可复用工具。

本阶段只允许完成：

```text
历史工具资产处理原则冻结
Excel 转 Json 工具复用规则冻结
FFMPEG Recorder 归属规则冻结
推流工具归属规则冻结
Downloader 复用规则冻结
串口工具复用规则冻结
Json 工具独立规则冻结
Editor 工具分类规则冻结
工具版本记录规则冻结
ToolAssetRegistry 规则冻结
历史工具进入 Runtime 的审查规则冻结
```

本阶段不进入任何工具代码实现或迁移。

---

# 二、工具资产最终目标

工具资产最终目标选择：

```text
C：分类处理
```

---

## 2.1 含义

历史工具不一刀切。

不同工具按价值和边界分类：

```text
纳入 ByFramework
保持 ByTools 独立
ByFramework 调用
作为重构参考
仅保留历史
建议废弃
```

---

## 2.2 原则

ByFramework 不是从零开始。

已有工具必须先审计，再决定：

```text
直接复用
重构复用
接口接入
保持独立
废弃
```

禁止 Codex 未审查就重写已有能力。

---

# 三、Excel 转 Json 工具

Excel 转 Json 工具选择：

```text
C：ByTools 为主，ByFramework 调用
```

---

## 3.1 含义

Excel 转 Json 工具本身具有独立价值。

它不应被强行绑死到 ByFramework。

---

## 3.2 规则

Excel 转 Json 工具应保持：

```text
可独立导出
可独立使用
可跨项目复用
可由 ByFramework 调用
```

---

## 3.3 与 LocalizationSystem 的关系

LocalizationSystem 可以使用 Excel 转 Json 工具生成的 JSON 语言包。

但 LocalizationSystem 不重新设计 Excel 工具链。

正确关系：

```text
ByTools/ExcelToJson
↓
生成 JSON
↓
LocalizationSystem Runtime 加载 JSON
```

---

# 四、FFMPEG Recorder

FFMPEG Recorder 选择：

```text
C：独立 + Platform 预留接口
```

---

## 4.1 含义

FFMPEG Recorder 是重要媒体工具资产。

但不直接属于 DeviceIntegration 主逻辑。

---

## 4.2 推荐归属

推荐保持独立：

```text
ByTools/FFMPEGRecorder
```

未来可预留接入：

```text
Platform/MediaSystem
Platform/RecorderSystem
```

---

## 4.3 与 DeviceIntegration 的关系

DeviceIntegration 负责设备数据录制。

FFMPEG Recorder 负责视频录制 / 屏幕录制 / 媒体录制。

二者可以通过时间戳对齐，但模块边界必须分开。

---

# 五、推流工具

推流工具选择：

```text
C：独立 + Platform 预留接口
```

---

## 5.1 含义

推流工具属于媒体 / 视频 / 运维能力。

不应塞进 NetworkSystem 主逻辑，也不应塞进 DeviceIntegration 主逻辑。

---

## 5.2 推荐归属

推荐保持独立：

```text
ByTools/StreamingTool
```

未来可预留接入：

```text
Platform/StreamingSystem
Platform/MediaSystem
```

---

## 5.3 与 NetworkSystem 的关系

NetworkSystem 提供基础网络能力。

推流工具可以使用网络能力，但推流协议和媒体管线不属于 NetworkSystem 主职责。

---

# 六、Downloader 下载器

Downloader 选择：

```text
C：NetworkSystem 核心能力 + 独立工具
```

---

## 6.1 含义

大文件下载器既是通用工具，也是 NetworkSystem 的重要基础能力。

---

## 6.2 规则

现有 Downloader 可作为后续 NetworkCore Downloader 的实现参考或重构来源。

但必须经过审计和抽象。

禁止直接复制旧代码进入 Runtime。

---

## 6.3 最终关系

```text
NetworkCore/Http/Downloader
↓
提供框架级下载能力

ByTools/DownloaderTool
↓
保持独立工具能力
```

---

## 6.4 目标能力

Downloader 未来应支持：

```text
多线程下载
断点续传
暂停
恢复
取消
Hash 校验
下载进度
速度统计
下载缓存
```

---

# 七、串口工具

串口工具选择：

```text
C：DeviceIntegration 基础实现来源
```

---

## 7.1 含义

历史串口工具是 DeviceIntegration 的重要资产。

但不能直接无审查进入 Platform。

---

## 7.2 规则

现有串口工具应作为：

```text
SerialPort 基础实现参考
SerialPort 适配来源
DeviceIntegration 重构输入
```

必须经过：

```text
API 审查
线程安全审查
异常处理审查
平台兼容审查
配置接入审查
RuntimeConfigUI 接入审查
```

---

# 八、Json 工具

Json 工具选择：

```text
B：保持独立
```

---

## 8.1 含义

Json 工具本身属于基础工具资产，但不应被强行塞进 Core 或 Platform。

---

## 8.2 规则

Json 工具保持独立：

```text
ByTools/JsonTools
```

ByFramework 可通过接口或工具引用使用。

但禁止 Core 直接膨胀成 Json 工具集合。

---

# 九、Editor 工具

Editor 工具选择：

```text
C：分类处理
```

---

## 9.1 分类规则

历史 Editor 工具按用途分类：

```text
ByFramework 内部工具
ByTools 独立工具
模块专属工具
临时工具
废弃工具
```

---

## 9.2 可纳入 ByFramework 的 Editor 工具

例如：

```text
FrameworkConfig 配置工具
AssetBundle 构建工具
BuildProfile 工具
License 工具
PlatformServiceRegistry 监视器
验证报告工具
```

---

## 9.3 应保持独立的 Editor 工具

例如：

```text
Excel 转 Json
通用 Json 工具
通用文件处理工具
通用媒体工具
可导出到其它项目的小工具
```

---

# 十、工具资产版本记录

是否记录版本选择：

```text
A：需要
```

---

## 10.1 记录内容

每个工具资产应记录：

```text
工具名称
工具类型
当前版本
作者
来源
用途
归属
是否可独立导出
是否已纳入 ByFramework
是否需要重构
最后审计时间
审计结论
```

---

## 10.2 价值

用于：

```text
避免重复开发
避免误删工具
避免误迁移工具
追踪工具来源
判断工具是否可复用
记录工具演进
```

---

# 十一、ToolAssetRegistry

是否建立 ToolAssetRegistry 选择：

```text
A：建立
```

---

## 11.1 含义

ToolAssetRegistry 是工具资产注册表。

它记录当前项目已有工具的状态。

---

## 11.2 建议格式

可使用：

```text
Markdown
Json
ScriptableObject
```

初期建议：

```text
Documentation/ToolAssetRegistry.md
```

后期可扩展为：

```text
ToolAssetRegistry.json
ToolAssetRegistry.asset
```

---

## 11.3 工具状态分类

建议状态：

```text
Active
Reusable
NeedRefactor
ReferenceOnly
Deprecated
Unknown
```

---

## 11.4 工具归属分类

建议归属：

```text
ByFramework
ByTools
ExternalPlugin
Legacy
FeatureModule
PlatformCandidate
```

---

# 十二、历史工具进入 Runtime 的规则

历史工具代码是否允许直接进入 Runtime 选择：

```text
C：必须审查后再决定
```

---

## 12.1 含义

Codex 不允许看到旧工具后直接复制进入 Runtime。

必须先审查：

```text
架构边界
依赖关系
线程安全
异常处理
配置方式
日志方式
平台兼容
可测试性
可维护性
是否依赖 UnityEditor
是否依赖业务代码
```

---

## 12.2 进入 Runtime 前必须确认

必须确认：

```text
是否符合当前模块 Foundation
是否符合 API Freeze
是否不会污染 Core
是否不会让 Platform 依赖 FeatureModule
是否不会让 Runtime 依赖 UnityEditor
是否支持配置化
是否支持中文日志规范
是否支持现场维护需求
```

---

# 十三、工具资产审计分类模板

每个工具建议按以下模板审计：

```text
工具名称：
工具路径：
工具类型：
当前用途：
是否可独立导出：
是否依赖 ByFramework：
是否依赖 UnityEditor：
是否依赖业务模块：
是否可复用：
建议归属：
审计结论：
处理建议：
```

---

# 十四、初始工具资产分类方向

根据目前讨论，初始分类方向如下。

---

## 14.1 Excel 转 Json

```text
建议归属：ByTools
ByFramework 关系：可调用
处理建议：保持独立，不重写
```

---

## 14.2 FFMPEG Recorder

```text
建议归属：ByTools / MediaTools
ByFramework 关系：Platform MediaSystem 预留接口
处理建议：保持独立，后期按需接入
```

---

## 14.3 推流工具

```text
建议归属：ByTools / StreamingTool
ByFramework 关系：Platform StreamingSystem 预留接口
处理建议：保持独立，后期按需接入
```

---

## 14.4 Downloader

```text
建议归属：NetworkCore + ByTools 双形态
ByFramework 关系：作为 NetworkSystem Downloader 重构来源
处理建议：审计后重构，不直接复制
```

---

## 14.5 串口工具

```text
建议归属：DeviceIntegration 基础实现来源
ByFramework 关系：作为 Platform SerialPort 能力候选
处理建议：审计后重构
```

---

## 14.6 Json 工具

```text
建议归属：ByTools
ByFramework 关系：按需调用
处理建议：保持独立
```

---

## 14.7 Editor 工具

```text
建议归属：分类处理
ByFramework 关系：部分纳入 EditorTools，部分保持 ByTools
处理建议：逐个审计
```

---

# 十五、与 EditorTools 的关系

EditorTools 提供工具资产审计入口。

但 EditorTools 不拥有所有工具。

正确关系：

```text
EditorTools
↓
提供统一入口 / 审计面板 / 菜单

ByTools
↓
保持独立工具资产
```

---

# 十六、与 BuildProfileSystem 的关系

BuildProfileSystem 可控制：

```text
是否包含某些工具
是否包含 Samples
是否包含 Debug Tools
是否包含 EditorTools
```

但 BuildProfile 不改变工具归属。

---

# 十七、与 RuntimeConfigUI 的关系

RuntimeConfigUI 可接入部分运行时工具状态。

例如：

```text
Downloader 状态
设备状态
Recorder 状态
Streaming 状态
```

但 RuntimeConfigUI 不直接拥有这些工具。

---

# 十八、禁止事项

P3.22 阶段禁止：

```text
迁移历史工具代码
重构历史工具代码
把历史工具直接复制进 Runtime
把 Excel 工具绑死到 ByFramework
把 FFMPEG Recorder 塞进 DeviceIntegration
把推流工具塞进 NetworkSystem
把 Json 工具塞进 Core
未审查就让 Codex 重写 Downloader
未审查就让 Codex 重写串口工具
破坏现有工具独立导出能力
```

---

# 十九、P3.22 输出物

P3.22 应输出：

```text
Documentation/ExistingToolsAssetAudit.md
Documentation/ToolAssetRegistry.md
Documentation/ByFramework_Current_Context.md 更新
Roadmap.md 更新
Todo.md 更新
Documentation/Changelog.md 更新
```

---

# 二十、阶段关闭条件

P3.22 关闭条件：

```text
历史工具分类处理原则明确
Excel 转 Json 工具保持 ByTools 为主、ByFramework 可调用明确
FFMPEG Recorder 独立 + Platform 预留接口明确
推流工具独立 + Platform 预留接口明确
Downloader 作为 NetworkSystem 核心能力 + 独立工具明确
串口工具作为 DeviceIntegration 基础实现来源明确
Json 工具保持独立明确
Editor 工具分类处理明确
工具版本记录规则明确
ToolAssetRegistry 规则明确
历史工具进入 Runtime 必须审查明确
禁止事项写入文档
Current_Context 更新完成
Roadmap / Todo / Changelog 同步完成
```

满足以上条件后：

```text
P3 Foundation 可视为 100% 完成
```

---

# 二十一、最终结论

P3.22 ExistingToolsAssetAudit 是 ByFramework P3 Foundation 的最后收口阶段。

它只确定：

```text
历史工具怎么处理
哪些工具可复用
哪些工具保持独立
哪些工具可作为实现来源
哪些工具需要重构
哪些工具不允许直接进入 Runtime
如何避免 Codex 重复开发
如何保护已有工具资产
```

完成后，ByFramework 可正式以：

```text
Foundation 设计完成
Runtime API Freeze + Runtime Implementation
```

的模式继续推进。

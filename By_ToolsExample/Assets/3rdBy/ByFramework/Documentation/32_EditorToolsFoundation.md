# P3.18 EditorTools Foundation 设计冻结

> 阶段性质：设计冻结阶段  
> 不是 Runtime 实现阶段  
> 不创建 EditorTools 代码  
> 不实现 EditorWindow、工具基类、验证报告、资产审计入口或导出工具代码  

---

# 一、阶段目标

P3.18 的目标是冻结 ByFramework EditorTools 的统一设计，为后续各模块 EditorWindow、验证工具、构建工具、配置工具、资源工具、授权工具和工具资产审计入口提供统一规范。

本阶段只允许完成：

```text
EditorTools 定位冻结
菜单根路径冻结
菜单分类规则冻结
中文化规则冻结
统一工具基类方向冻结
统一验证报告方向冻结
危险操作确认规则冻结
Editor 调用 Runtime 逻辑边界冻结
工具资产审计入口规则冻结
工具独立导出规则冻结
EditorTools 模块归属冻结
EditorTools 与各 Platform 模块关系冻结
```

本阶段不进入具体 Editor 工具实现。

---

# 二、架构位置

EditorTools 属于 Platform 的编辑器工具公共基础能力。

最终定位为：

```text
Platform/EditorTools 提供公共能力
各模块保留自己的 EditorWindow
```

选择：

```text
C：Platform/EditorTools 提供公共基础，各模块保留自己的 EditorWindow
```

---

# 三、EditorTools 定位

EditorTools 定位选择：

```text
B：建立统一 EditorTools 体系，各模块工具挂到统一菜单下
```

EditorTools 不是零散工具集合。

它负责统一：

```text
菜单入口
工具分类
中文化规范
工具基类
验证报告格式
危险操作确认
工具资产审计入口
工具导出规范
```

EditorTools 的核心目标是：

```text
让 ByFramework 的所有 Editor 工具看起来像一套完整系统，而不是散落在 Unity 菜单里的零散脚本。
```

---

# 四、菜单根路径

菜单根路径选择：

```text
A：ByFramework
```

所有框架相关 Editor 工具必须统一挂在：

```text
ByFramework
```

下方。

---

## 4.1 示例菜单

```text
ByFramework/平台/FrameworkConfig 配置
ByFramework/平台/PlatformServiceRegistry 监视器
ByFramework/资源/AssetBundle 构建工具
ByFramework/构建/BuildProfile 配置
ByFramework/授权/License 管理工具
ByFramework/验证/Core 生命周期验证
ByFramework/工具资产/工具资产审计
```

---

# 五、工具分类方式

工具分类方式选择：

```text
C：架构分类 + 功能分类结合
```

推荐菜单结构：

```text
ByFramework
├─ Core
├─ 平台
├─ 配置
├─ 资源
├─ 构建
├─ 授权
├─ 验证
├─ 调试
└─ 工具资产
```

---

# 六、Editor 工具中文化规则

Editor 工具中文化选择：

```text
B：中文为主，技术名英文保留
```

必须中文化：

```text
菜单
窗口标题
按钮
提示
HelpBox
验证报告
日志说明
错误说明
警告说明
```

技术对象名保留英文：

```text
FrameworkEntry
PlatformServiceRegistry
ResourceSystem
AssetBundle
BuildProfile
License
NetworkCore
RuntimeConfigUI
```

---

# 七、统一工具基类

是否需要统一工具基类选择：

```text
A：需要
```

未来可设计：

```text
ByFrameworkEditorWindow
ByFrameworkToolWindow
ByFrameworkValidationWindow
```

公共能力方向：

```text
统一标题
统一工具栏
统一日志区域
统一状态提示
统一确认弹窗
统一验证报告展示
统一刷新按钮
统一保存按钮
```

本阶段不实现代码，只冻结方向。

---

# 八、统一验证报告

是否需要统一验证报告选择：

```text
A：需要
```

统一验证报告可包含：

```text
ReportName
ModuleName
RunTime
Result
InfoCount
WarningCount
ErrorCount
FatalCount
Entries
```

单条记录可包含：

```text
Level
Code
Message
Suggestion
RelatedPath
```

验证级别：

```text
Info
Warning
Error
Fatal
```

---

# 九、危险操作确认

是否需要危险操作确认选择：

```text
A：需要
```

以下操作必须二次确认：

```text
清空服务注册表
清理 AssetBundle 输出目录
删除缓存
重置配置
恢复默认配置
删除保存数据
删除构建报告
覆盖授权文件
覆盖现场配置
```

---

# 十、EditorTools 调用 Runtime 逻辑边界

EditorTools 是否允许调用 Runtime 逻辑选择：

```text
C：只允许调用纯数据 / 纯工具逻辑
```

允许 Editor 工具调用：

```text
纯数据模型
纯工具类
纯校验逻辑
纯序列化逻辑
纯路径工具
纯 Manifest 生成逻辑
```

禁止让 Runtime 依赖：

```text
UnityEditor
EditorWindow
MenuItem
EditorGUILayout
AssetDatabase
```

---

# 十一、工具资产审计入口

是否需要工具资产审计入口选择：

```text
A：需要
```

同时增加规则：

```text
工具必须保持独立导出能力
```

工具资产审计入口不是把所有工具绑死到 ByFramework。

它只是提供一个统一查看和跳转入口。

可显示：

```text
Excel 转 JSON 工具
Json 工具
下载器工具
串口工具
配置工具
自定义工具
```

---

## 11.1 工具独立导出能力

现有工具支持导出到别的项目中使用。

因此必须保持：

```text
工具本体独立
入口层可选
ByFramework 不强绑定工具
工具可单独导出
工具可单独升级
工具可单独复用
```

正确关系：

```text
ByFramework/Platform/EditorTools
↓
提供统一入口和审计面板

ByTools / ExcelTools / DownloaderTools / SerialTools
↓
保持独立工具资产
```

不允许为了统一入口，把独立工具强行改成：

```text
必须依赖 ByFramework
必须依赖 PlatformServiceRegistry
必须依赖 FrameworkConfig
不可单独导出
```

---

# 十二、EditorTools 模块归属

EditorTools 是否作为独立 Platform 模块选择：

```text
C：Platform/EditorTools 提供公共基础，各模块保留自己的 EditorWindow
```

公共能力放在：

```text
Platform/EditorTools
```

模块自己的工具仍然放在各自模块下：

```text
Platform/ResourceSystem/Editor
Platform/BuildProfileSystem/Editor
Platform/LicenseSystem/Editor
Platform/FrameworkConfig/Editor
```

---

# 十三、EditorTools 与 RuntimeConfigUI 的区别

EditorTools 是 Unity Editor 内工具。

RuntimeConfigUI 是打包后现场人员使用的运行时配置界面。

```text
EditorTools
    Unity Editor 内使用
    面向开发者 / 构建人员

RuntimeConfigUI
    打包后运行时使用
    面向现场人员 / 运维人员
```

禁止混淆。

---

# 十四、现有工具资产规则

项目中已有工具应进行资产审计。

审计结果建议分为：

```text
可直接复用
需要重构后复用
仅作为参考
建议废弃
独立导出工具
ByFramework 内部工具
```

可独立导出的工具，例如：

```text
Excel 转 JSON 工具
Json 工具
下载器工具
串口工具
自定义小工具
```

如原本支持导出到其它项目，应继续保留该能力。

---

# 十五、建议核心对象

后续实现可围绕以下对象设计。

本阶段只冻结概念，不实现代码。

```text
ByFrameworkEditorWindow
ByFrameworkToolWindow
ByFrameworkValidationWindow
ByFrameworkValidationReport
ByFrameworkValidationEntry
ByFrameworkToolAssetInfo
ByFrameworkToolAssetAudit
ByFrameworkEditorConfirmUtility
ByFrameworkEditorLogUtility
ByFrameworkEditorMenuPaths
```

---

# 十六、建议目录结构

本阶段仅冻结目录方向，不要求创建代码文件。

```text
Assets/ByFramework/Platform/EditorTools
├─ Editor
│  ├─ Core
│  ├─ Report
│  ├─ Utility
│  └─ ToolAsset
```

各模块仍保留自己的 Editor 目录：

```text
Platform/ResourceSystem/Editor
Platform/BuildProfileSystem/Editor
Platform/LicenseSystem/Editor
Platform/FrameworkConfig/Editor
```

---

# 十七、菜单命名规范

建议统一常量管理菜单路径。

方向：

```text
ByFramework/Core
ByFramework/平台
ByFramework/配置
ByFramework/资源
ByFramework/构建
ByFramework/授权
ByFramework/验证
ByFramework/调试
ByFramework/工具资产
```

---

# 十八、禁止事项

P3.18 阶段禁止：

```text
实现 EditorTools 代码
实现 EditorWindow
实现工具基类
实现验证报告系统
实现工具资产审计窗口
强制改造现有工具
破坏现有工具独立导出能力
让 Runtime 依赖 UnityEditor
让 Editor 工具污染 Runtime 主逻辑
进入 RuntimeConfigUI 实现
进入 ResourceSystem / AssetBundle / BuildProfile 工具实现
```

---

# 十九、P3.18 输出物

P3.18 应输出：

```text
Documentation/EditorToolsFoundation.md
Documentation/00_ByFramework_Current_Context.md 更新
Documentation/02_Roadmap.md 更新
Documentation/03_Todo.md 更新
Documentation/04_Changelog.md 更新
```

不要求输出 Runtime 或 Editor 代码。

---

# 二十、阶段关闭条件

P3.18 关闭条件：

```text
EditorTools 定位明确
菜单根路径明确
菜单分类规则明确
中文化规则明确
统一工具基类方向明确
统一验证报告方向明确
危险操作确认规则明确
Editor 调用 Runtime 逻辑边界明确
工具资产审计入口明确
工具独立导出能力明确
EditorTools 作为公共基础、各模块保留自己的 EditorWindow 规则明确
禁止事项写入文档
Current_Context 更新完成
Roadmap / Todo / Changelog 同步完成
```

满足以上条件后，可进入：

```text
P3.19 RuntimeConfigUI Foundation
```

---

# 二十一、最终结论

P3.18 EditorTools Foundation 是设计冻结阶段。

它只确定：

```text
EditorTools 是什么
EditorTools 管什么
EditorTools 不管什么
菜单如何统一
工具如何分类
工具如何中文化
验证报告如何统一
危险操作如何确认
现有工具如何审计
工具如何保持独立导出能力
后续 Editor 实现应遵守什么边界
```

不得在本阶段进入具体 Editor 工具实现。

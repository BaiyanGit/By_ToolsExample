# P3 Foundation Closure And Runtime Entry 设计冻结

> 阶段性质：P3 Foundation 总结与 Runtime Implementation 入口设计  
> 不是 Runtime 实现阶段  
> 不创建 Runtime 脚本  
> 不实现任何模块代码  
> 本文档用于约束 Codex 后续进入 Runtime 实现阶段的顺序、规则、关闭条件和禁止事项  

---

# 一、阶段目标

本阶段目标是对 P3 Foundation 阶段进行收口，并定义后续 Runtime Implementation 的入口规则。

本阶段只允许完成：

```text
P3 Foundation 完成条件冻结
Runtime 实现顺序冻结
Runtime API Freeze 规则冻结
Runtime Implementation 规则冻结
Codex 执行边界冻结
验证工具规则冻结
文档同步规则冻结
Foundation 文档修改规则冻结
接口变更处理规则冻结
Runtime 阶段关闭条件冻结
```

本阶段不进入任何具体 Runtime 实现。

---

# 二、P3 Foundation 已覆盖范围

P3 Foundation 已覆盖以下模块设计：

```text
PlatformServiceRegistry
FrameworkConfig
InputSystem
UISystem
DisplaySystem
ResourceSystem
AssetBundle
SaveSystem
LocalizationSystem
NetworkSystem
BuildProfileSystem
LicenseSystem
FeatureModule Boundary
EditorTools
RuntimeConfigUI
DeviceIntegration
Samples
ExistingToolsAssetAudit
ToolAssetRegistry
```

这些设计文件作为后续 Runtime 实现的架构依据。

---

# 三、P3 Foundation 设计文档清单

后续 Codex 必须优先读取以下文档：

```text
Documentation/00_ByFramework_Current_Context.md
Documentation/01_Architecture.md
Documentation/PlatformServiceRegistrationRuntimeAPIFreeze.md
Documentation/21_UISystemFoundation.md
Documentation/22_DisplaySystemFoundation.md
Documentation/23_ResourceSystemFoundation.md
Documentation/24_AssetBundleFoundation.md
Documentation/25_SaveSystemFoundation.md
Documentation/26_FrameworkConfigFoundation.md
Documentation/27_LocalizationSystemFoundation.md
Documentation/28_NetworkSystemFoundation.md
Documentation/29_BuildProfileSystemFoundation.md
Documentation/30_LicenseSystemFoundation.md
Documentation/31_FeatureModuleBoundaryFoundation.md
Documentation/40_P3_Foundation_Closure_And_Runtime_Entry.md
```

如存在文档冲突，应停止实现并报告冲突，不允许 Codex 自行选择方向。

---

# 四、Runtime 实现总原则

Runtime 实现必须遵守：

```text
先底层
后上层

先服务注册
后模块实现

先配置系统
后配置驱动模块

先 Runtime API Freeze
后 Runtime Implementation

一次只做一个模块

不跨阶段实现

不提前实现未进入阶段的模块
```

---

# 五、Runtime 实现顺序

Runtime 实现顺序选择：

```text
B：先实现最底层基础，再实现上层模块
```

建议顺序：

```text
R1.1 PlatformServiceRegistry Runtime API Freeze
R1.2 PlatformServiceRegistry Runtime Implementation

R2.1 FrameworkConfig Runtime API Freeze
R2.2 FrameworkConfig Runtime Implementation

R3.1 SaveSystem Runtime API Freeze
R3.2 SaveSystem Runtime Implementation

R4.1 ResourceSystem Runtime API Freeze
R4.2 ResourceSystem Runtime Implementation

R5.1 AssetBundle Runtime / Editor API Freeze
R5.2 AssetBundle Runtime / Editor Implementation

R6.1 LocalizationSystem Runtime API Freeze
R6.2 LocalizationSystem Runtime Implementation

R7.1 InputSystem Runtime API Freeze
R7.2 InputSystem Runtime Implementation

R8.1 DisplaySystem Runtime API Freeze
R8.2 DisplaySystem Runtime Implementation

R9.1 UISystem Runtime API Freeze
R9.2 UISystem Runtime Implementation

R10.1 NetworkSystem Runtime API Freeze
R10.2 NetworkCore / NetworkSystem Adapter Implementation

R11.1 BuildProfileSystem Editor API Freeze
R11.2 BuildProfileSystem Editor Implementation

R12.1 LicenseSystem Runtime / Editor API Freeze
R12.2 LicenseSystem Runtime / Editor Implementation

R13.1 FeatureModule Example Boundary Freeze
R13.2 FeatureModule Samples Implementation
```

---

# 六、为什么先做 PlatformServiceRegistry

PlatformServiceRegistry 必须第一个进入 Runtime。

原因：

```text
所有 Platform 模块后续都需要统一注册
所有 Platform 服务都需要统一访问
避免每个模块各自做 Singleton
避免 Platform 模块接入方式混乱
```

因此选择：

```text
先做 PlatformServiceRegistry Runtime
```

---

# 七、为什么 FrameworkConfig 要尽早实现

FrameworkConfig 应尽早实现。

原因：

```text
ResourceSystem 需要资源路径配置
DisplaySystem 需要显示配置
UISystem 需要 UI / Theme 配置
NetworkSystem 需要网络配置
SaveSystem 需要保存路径配置
LicenseSystem 需要授权配置
BuildProfileSystem 需要生成默认配置
RuntimeConfigUI 需要统一配置来源
```

因此选择：

```text
FrameworkConfig 尽早实现
```

但注意：

```text
PlatformServiceRegistry 必须先于 FrameworkConfig Runtime
```

---

# 八、一次只实现一个模块

Codex 是否允许一次实现多个模块：

```text
A：不允许
```

规则：

```text
一次只允许推进一个 Runtime 阶段
一次只允许实现一个模块
一次只允许修改与当前阶段直接相关的文件
```

禁止：

```text
实现 PlatformServiceRegistry 时顺手实现 FrameworkConfig
实现 FrameworkConfig 时顺手实现 SaveSystem
实现 ResourceSystem 时顺手实现 AssetBundle 完整工具链
实现 NetworkSystem 时顺手实现 SimulationSync
```

---

# 九、Runtime API Freeze 规则

是否先做 Runtime API Freeze：

```text
A：需要
```

每个模块 Runtime 实现前，必须先进行：

```text
Runtime API Freeze
```

然后才能进入：

```text
Runtime Implementation
```

---

## 9.1 Runtime API Freeze 内容

每个 Runtime API Freeze 必须明确：

```text
接口名称
接口职责
接口方法
核心数据结构
错误结果
生命周期
注册方式
配置依赖
禁止事项
验证方式
```

---

## 9.2 Runtime API Freeze 输出物

每个模块 Runtime API Freeze 应输出：

```text
Documentation/{ModuleName}RuntimeAPIFreeze.md
```

例如：

```text
Documentation/50_PlatformServiceRegistryRuntimeAPIFreeze.md
Documentation/FrameworkConfigRuntimeAPIFreeze.md
Documentation/SaveSystemRuntimeAPIFreeze.md
```

---

# 十、Runtime Implementation 规则

Runtime Implementation 必须严格依据：

```text
Foundation 文档
Runtime API Freeze 文档
Current_Context
AGENTS.md
```

实现时不得自行重设计架构。

如果发现 Foundation 与实际代码存在冲突，Codex 必须：

```text
停止实现
报告冲突
提出建议
等待确认
```

不得自行改方向。

---

# 十一、验证工具规则

每个 Runtime 阶段是否必须有验证工具：

```text
C：关键模块必须有
```

---

## 11.1 必须有验证工具的模块

以下模块必须有验证工具或验证场景：

```text
PlatformServiceRegistry
FrameworkConfig
SaveSystem
ResourceSystem
AssetBundle
DisplaySystem
NetworkSystem
LicenseSystem
```

---

## 11.2 可选验证工具的模块

以下模块可根据实际复杂度决定：

```text
LocalizationSystem
InputSystem
UISystem
BuildProfileSystem
FeatureModule Samples
```

---

## 11.3 验证工具要求

验证工具必须：

```text
中文输出
中文菜单
中文按钮
中文报告
明确成功 / 失败
明确错误原因
不依赖业务项目
不污染 Runtime 主逻辑
```

---

# 十二、文档同步规则

每个 Runtime 阶段是否必须更新文档：

```text
A：必须更新
```

每个阶段完成后必须同步：

```text
Documentation/00_ByFramework_Current_Context.md
02_Roadmap.md
03_Todo.md
Documentation/04_Changelog.md
```

必要时还需要同步：

```text
Documentation/01_Architecture.md
AGENTS.md
```

---

# 十三、Foundation 文档修改规则

Codex 是否允许修改已冻结 Foundation 文档：

```text
B：可以小修
```

---

## 13.1 允许的小修

允许：

```text
修正错别字
修正文档路径
补充实现状态
补充已确认的关闭状态
补充不改变架构方向的说明
```

---

## 13.2 禁止的修改

禁止未经确认修改：

```text
模块职责
模块边界
依赖关系
实现顺序
禁止事项
架构红线
阶段关闭条件
```

如果确实需要修改，必须停止并报告。

---

# 十四、Runtime 接口调整规则

Runtime 实现是否允许调整接口：

```text
B：可以提出变更，但必须停止并说明原因
```

---

## 14.1 允许提出变更的情况

允许提出变更：

```text
Foundation 文档遗漏必要接口
接口无法实现
接口与 Unity 限制冲突
接口与现有代码冲突
接口会导致循环依赖
接口会破坏架构边界
```

---

## 14.2 处理流程

流程：

```text
发现问题
↓
停止实现
↓
说明冲突
↓
提出建议
↓
等待确认
↓
确认后再修改文档或实现
```

---

# 十五、Codex 执行边界

Codex 后续只负责：

```text
读取文档
按当前阶段实现
补充必要代码
编写验证工具
同步文档状态
记录 Changelog
```

Codex 不负责：

```text
重新设计架构
跳过阶段
一次做多个模块
修改已冻结边界
把业务塞进 Platform
把 Platform 依赖 FeatureModule
把 NetworkCore 依赖 ByFramework
扩展 Core/Config/FrameworkConfig.cs 为万能配置中心
```

---

# 十六、旧 Core/Config/FrameworkConfig.cs 规则

旧文件：

```text
Core/Config/FrameworkConfig.cs
```

属于早期兼容代码。

后续 Runtime 阶段规则：

```text
暂时保留兼容
禁止继续扩展
不得作为最终配置中心
不得新增 Platform 配置字段
最终迁移到 Platform/FrameworkConfig
```

---

# 十七、NetworkCore 红线

NetworkCore 必须保持：

```text
纯 C#
低依赖
可独立抽离
可独立测试
可跨项目复用
```

禁止 NetworkCore 依赖：

```text
FrameworkEntry
PlatformServiceRegistry
FrameworkConfig
EventManager
ThreadDispatcher
UnityEngine
MonoBehaviour
ScriptableObject
FeatureModule
```

NetworkSystem Adapter 才允许接入 ByFramework。

---

# 十八、SimulationSync 红线

SimulationSync 属于：

```text
FeatureModule/SimulationSync
```

不属于：

```text
NetworkSystem
```

NetworkSystem 只负责：

```text
Transport
Session
Protocol
Serialization
Connectivity
Http
Download
Upload
Client
Server
```

---

# 十九、Runtime 阶段关闭条件

每个 Runtime 阶段关闭必须满足：

```text
当前阶段目标完成
未实现非当前阶段内容
未破坏架构边界
无 Core 生命周期回退
无 Platform → FeatureModule 依赖
无 NetworkCore → ByFramework 依赖
代码可编译
关键模块验证通过
文档已同步
Changelog 已记录
Todo / Roadmap 已更新
```

---

# 二十、Runtime 阶段提交内容

每个 Runtime 阶段应输出：

```text
代码实现
必要验证工具
Runtime API Freeze 文档
实现说明
验证说明
Changelog
Current_Context 更新
Todo / Roadmap 更新
```

如果阶段只是 API Freeze，不要求代码实现。

---

# 二十一、P3 Foundation 总关闭条件

P3 Foundation 总关闭条件（已满足）：

```text
所有 Foundation 设计文件已放入 Documentation
01_Architecture.md 已同步最终架构
00_ByFramework_Current_Context.md 已同步当前阶段状态
AGENTS.md 已写明 Codex 执行规则
02_Roadmap.md 已同步 Runtime 阶段顺序
03_Todo.md 已同步下一阶段任务
04_Changelog.md 已记录 P3 Foundation Closure
```

满足后允许进入：

```text
R1.1 PlatformServiceRegistry Runtime API Freeze
```

---

# 二十二、推荐 Codex 入口指令

当所有 Foundation 文档导入项目后，可以给 Codex 以下指令：

```text
请先阅读 AGENTS.md、Documentation/00_ByFramework_Current_Context.md、Documentation/01_Architecture.md、Documentation/40_P3_Foundation_Closure_And_Runtime_Entry.md。

当前 P3 Foundation 已关闭。

进入 R1.1 PlatformServiceRegistry Runtime API Freeze。

本阶段只允许冻结 PlatformServiceRegistry Runtime API。

不要实现 Runtime 代码。
不要进入 FrameworkConfig。
不要修改 FrameworkEntry 生命周期。
不要扩展 Core/Config/FrameworkConfig.cs。
不要修改已冻结 Foundation 文档的架构边界。

完成后同步：
Documentation/50_PlatformServiceRegistryRuntimeAPIFreeze.md
Documentation/00_ByFramework_Current_Context.md
02_Roadmap.md
03_Todo.md
Documentation/04_Changelog.md
```

---

# 二十三、下一阶段

P3 Foundation Closure 完成后，下一阶段是：

```text
R1.1 PlatformServiceRegistry Runtime API Freeze
```

不是：

```text
Runtime Implementation
```

不是：

```text
FrameworkConfig Runtime
```

不是：

```text
InputSystem Runtime
```

---

# 二十四、最终结论

本文档用于结束 P3 Foundation 设计阶段，并打开 Runtime Implementation 的入口。

后续全部实现必须遵守：

```text
先 API Freeze
后 Implementation

一次一个模块

先底层
后上层

Codex 不重新设计架构

如遇冲突先停止并报告
```

当前允许进入的唯一下一阶段：

```text
R1.1 PlatformServiceRegistry Runtime API Freeze
```


---

# 二十五、P3 Foundation 最终状态

```text
P3 Foundation 已 100% 完成。
```

后续不再继续新增 Foundation 设计，除非用户明确提出新的框架级模块。

当前唯一允许继续的阶段：

```text
R1.2 PlatformServiceRegistry Runtime Implementation
```

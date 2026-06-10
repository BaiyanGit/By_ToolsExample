# P4.1 Runtime Development Rules 设计冻结

> 阶段性质：Runtime 开发规范冻结  
> 不是 Runtime Implementation  
> 不实现任何代码  
> 本文档用于约束 Codex 后续所有 Runtime / Editor / Tool 实现阶段的代码风格、边界、日志、异常、验证和文档同步规则  

---

# 一、阶段目标

P4.1 的目标是冻结 ByFramework 后续代码实现阶段的统一开发规则。

从 R1.2 开始，Codex 所有代码实现必须遵守本文档。

本文档约束范围：

```text
Runtime 代码
Editor 代码
Tool 代码
Samples 代码
验证工具
模块实现文档
Changelog
Todo
Roadmap
```

---

# 二、总原则

ByFramework 后续代码实现必须遵守：

```text
先读文档
再写代码

先 API Freeze
再 Implementation

一次只做一个阶段
一次只做一个模块

不得跨阶段实现
不得自行重设计架构
不得修改已冻结边界

发现冲突必须停止并报告
```

---

# 三、命名空间规则

建议命名空间按层级组织：

```text
ByFramework.Core
ByFramework.Platform
ByFramework.Platform.PlatformServiceRegistry
ByFramework.Platform.FrameworkConfig
ByFramework.Platform.SaveSystem
ByFramework.Platform.ResourceSystem
ByFramework.Platform.NetworkSystem
ByFramework.FeatureModule
```

NetworkCore 特殊规则：

```text
ByFramework.NetworkCore
```

或：

```text
ByFramework.Platform.NetworkSystem.NetworkCore
```

但 NetworkCore 内部禁止依赖 ByFramework Platform。

---

# 四、目录规则

代码目录必须符合对应 Foundation / API Freeze 文档。

禁止：

```text
为了方便随便新建 Scripts
把 Platform 代码放进 Core
把 FeatureModule 代码放进 Platform
把 Editor 代码放进 Runtime
把 Runtime 代码依赖 UnityEditor
```

---

# 五、Runtime / Editor 边界

Runtime 代码禁止引用：

```text
UnityEditor
EditorWindow
MenuItem
AssetDatabase
EditorGUILayout
EditorUtility
```

Editor 代码可以引用 Runtime 的纯数据模型和纯工具逻辑。

正确关系：

```text
Runtime
↑
Editor 可调用

Editor
↓
Runtime 不可依赖
```

---

# 六、日志规范

所有日志必须有模块前缀。

格式：

```text
[ModuleName] 日志内容
```

示例：

```text
[PlatformServiceRegistry] 服务注册成功：IInputService
[FrameworkConfig] 配置加载完成
[ResourceSystem] 资源清单生成完成
[NetworkSystem] 网络连接失败：127.0.0.1:9000
```

---

## 6.1 中文化规则

Editor 日志、Editor UI、验证报告：

```text
中文为主
技术名保留英文
```

Runtime 日志：

```text
中文为主
技术名保留英文
```

禁止输出含糊日志：

```text
Error
Failed
Something wrong
```

必须说明：

```text
哪个模块
哪个操作
失败原因
建议处理方式
```

---

# 七、异常规则

异常使用原则：

```text
必须存在但不存在 → 抛异常
可选存在 → TryXXX 返回 false
参数非法 → ArgumentException / ArgumentNullException
状态非法 → InvalidOperationException
```

示例：

```text
Get<T>() 找不到服务 → 抛 InvalidOperationException
TryGet<T>() 找不到服务 → 返回 false
Register<T>(null) → 抛 ArgumentNullException
```

---

## 7.1 禁止吞异常

禁止：

```csharp
try
{
}
catch
{
}
```

必须至少记录清楚：

```text
模块
操作
异常信息
```

---

# 八、TryXXX 规则

只要操作允许失败但不属于严重错误，应提供 TryXXX。

例如：

```text
TryGet
TryRegister
TryLoad
TrySave
TryParse
TryGetConfig
```

TryXXX 不应抛出常规失败异常。

但参数错误仍可抛异常。

---

# 九、单例规则

禁止随意创建：

```text
MonoSingleton
DontDestroyOnLoad 单例
全局万能 Manager
```

允许：

```text
PlatformServiceRegistry 静态入口 + 内部实例
纯工具类 static
不可变常量类
```

如确实需要单例，必须：

```text
有文档依据
有生命周期边界
不污染 Core
不跨模块强耦合
```

---

# 十、生命周期规则

禁止未经确认修改：

```text
FrameworkEntry
Core 初始化顺序
EventManager 初始化
FSMManager 初始化
ThreadDispatcher 初始化
```

Platform 模块不得自行改 Core 生命周期链。

如果必须接入生命周期，必须先进入对应 Runtime API Freeze。

---

# 十一、线程规则

涉及线程的模块必须明确：

```text
是否主线程
是否后台线程
是否线程安全
是否需要 ThreadDispatcher
是否可取消
是否可超时
```

禁止后台线程直接操作 Unity 对象。

后台线程需要回到主线程时，必须通过：

```text
ThreadDispatcher
```

或对应主线程派发机制。

---

# 十二、配置规则

不得继续扩展：

```text
Core/Config/FrameworkConfig.cs
```

旧文件仅保留兼容。

新的配置必须走：

```text
Platform/FrameworkConfig
```

配置来源遵守：

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

# 十三、PlatformServiceRegistry 使用规则

PlatformServiceRegistry 只注册 Platform 服务。

禁止注册：

```text
VehicleSimulationService
TrainingService
CustomerAService
SimulationSyncService
```

未来业务服务另走：

```text
FeatureServiceRegistry
```

或其它独立机制。

---

# 十四、NetworkCore 规则

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
UnityEngine
UnityEditor
FrameworkEntry
PlatformServiceRegistry
FrameworkConfig
EventManager
FeatureModule
```

NetworkSystem Adapter 才允许接入 ByFramework。

---

# 十五、FeatureModule 边界

禁止：

```text
Platform 依赖 FeatureModule
Core 依赖 FeatureModule
NetworkSystem 理解 SimulationSync
ResourceSystem 理解车辆资源
SaveSystem 理解训练记录
UISystem 理解训练流程
```

FeatureModule 可以使用 Platform。

Platform 不可以反向引用 FeatureModule。

---

# 十六、Editor 工具规则

Editor 工具必须：

```text
挂在 ByFramework 菜单下
中文菜单
中文按钮
中文提示
危险操作二次确认
生成清晰验证报告
```

危险操作包括：

```text
删除
清空
覆盖
重置
恢复默认
清理缓存
清理输出目录
```

---

# 十七、验证规则

关键模块实现完成后必须提供验证方式。

如果无法自动化测试，必须提供人工验证步骤。

每个验证说明必须包含：

```text
验证目标
前置条件
操作步骤
预期结果
失败排查
```

示例：

```text
验证目标：验证 PlatformServiceRegistry 基础注册流程

操作步骤：
1. 点击 ByFramework/验证/PlatformServiceRegistry 基础验证
2. 查看 Console 输出
3. 打开 PlatformServiceRegistry 监视器

预期结果：
1. Console 输出验证通过
2. 服务注册、获取、替换、注销均成功
3. 无异常日志
```

---

# 十八、Samples 规则

Samples 必须：

```text
可删除
不影响主框架
不被 Runtime 依赖
不污染 Platform
默认优先 Mock / Simulated / Playback
真实设备可选
必须附带 README 或验证步骤
```

复杂 Sample 应包含：

```text
Sample Scene
Sample Config
验证步骤
预期结果
```

---

# 十九、历史工具资产规则

历史工具不得直接进入 Runtime。

必须先审计：

```text
依赖关系
线程安全
异常处理
配置方式
Runtime / Editor 边界
是否可独立导出
是否污染 ByFramework
```

Downloader、串口工具、FFMPEG、推流工具、Excel 转 Json、Json 工具均必须遵守 ToolAssetRegistry 决策。

---

# 二十、文档同步规则

每个阶段完成后必须同步：

```text
Documentation/00_ByFramework_Current_Context.md
Roadmap.md
Todo.md
Documentation/04_Changelog.md
```

必要时同步：

```text
Documentation/01_Architecture.md
Documentation/06_Documentation_Reading_Order.md
AGENTS.md
```

---

# 二十一、Codex 执行规则

Codex 后续实现时必须：

```text
先读 AGENTS.md
先读 Current_Context
先读 Architecture
先读当前阶段 API Freeze / Implementation 文档

只做当前阶段
不跨模块
不跨阶段
不自行扩展架构
不重写历史工具
不修改冻结边界
```

如发现问题：

```text
停止实现
说明冲突
提出建议
等待确认
```

---

# 二十二、禁止事项总表

禁止：

```text
未经确认修改 FrameworkEntry 生命周期
扩展 Core/Config/FrameworkConfig.cs
让 Platform 依赖 FeatureModule
让 NetworkCore 依赖 ByFramework
让 Runtime 依赖 UnityEditor
把 Samples 写进 Runtime 主逻辑
把历史工具直接复制进 Runtime
一次实现多个模块
跳过 API Freeze
跳过验证说明
跳过文档同步
```

---

# 二十三、阶段输出物

P4.1 应输出：

```text
Documentation/RuntimeDevelopmentRules.md
Documentation/00_ByFramework_Current_Context.md 更新
Roadmap.md 更新
Todo.md 更新
Documentation/04_Changelog.md 更新
```

---

# 二十四、最终结论

P4.1 Runtime Development Rules 是 Codex 后续写代码的总开发规范。

它用于保证：

```text
代码不跑偏
模块不串层
日志可读
异常清晰
Editor / Runtime 不混淆
历史工具不被乱重写
每个阶段可验证
每个阶段可审查
```

从 R1.2 开始，所有实现阶段必须遵守本文档。

# P3.21 Samples Foundation 设计冻结

> 阶段性质：设计冻结阶段  
> 不是 Runtime 实现阶段  
> 不创建 Samples 代码或场景  
> 不实现示例业务逻辑  
> 本阶段只冻结 Samples 的定位、目录、依赖边界、可删除规则、场景/配置/文档要求和人工验证规则  

---

# 一、阶段目标

P3.21 的目标是冻结 ByFramework Samples 的设计规则，避免示例代码污染 Runtime、Platform 或 FeatureModule 主逻辑。

本阶段只允许完成：

```text
Samples 定位冻结
Samples 目录规则冻结
Platform Samples 与 Feature Samples 边界冻结
Samples 可删除规则冻结
Sample Scene 规则冻结
Sample Config 规则冻结
Sample 文档规则冻结
业务参考示例规则冻结
联调示例规则冻结
真实设备 / 模拟设备规则冻结
人工验证优先规则冻结
```

本阶段不进入具体 Sample 实现。

---

# 二、Samples 定位

Samples 的定位选择：

```text
C：教学 + 可运行参考示例
```

Samples 是官方参考实现。

它负责说明：

```text
模块怎么用
推荐怎么接入
最佳实践是什么
如何验证基础能力
```

Samples 不是业务代码仓库。

---

# 三、Samples 放置位置

Samples 放置位置选择：

```text
C：集中 + 模块内
```

集中示例目录：

```text
Assets/ByFramework/Samples
```

用于：

```text
GettingStarted
跨模块示例
完整流程示例
新手示例
```

模块内示例：

```text
Platform/NetworkSystem/Samples
Platform/ResourceSystem/Samples
FeatureModule/DeviceIntegration/Samples
```

用于模块专属示例。

---

# 四、Platform Samples 与 Feature Samples 边界

Samples 是否允许依赖 FeatureModule 选择：

```text
C：分层
```

规则：

```text
Platform Samples 不允许依赖 FeatureModule
Feature Samples 允许依赖 FeatureModule
```

示例：

```text
Platform/NetworkSystem/Samples
不得依赖 VehicleSimulation / TrainingSystem

FeatureModule/SimulationSync/Samples
可以依赖 SimulationSync 业务模型
```

---

# 五、Samples 可删除规则

Samples 是否允许被删除选择：

```text
A + C：必须可删除，并可由 BuildProfile 控制是否包含
```

规则：

```text
Samples 不属于框架运行必要内容
Samples 删除后不影响 Runtime
Samples 可被 BuildProfile 裁剪
Samples 不应被 Core / Platform 主逻辑依赖
```

BuildProfile 可提供：

```text
IncludeSamples
```

选项。

---

# 六、Sample Scene

是否需要 Sample Scene 选择：

```text
C：只对复杂模块需要
```

建议需要场景的模块：

```text
NetworkSystem
DeviceIntegration
DisplaySystem
UISystem
RuntimeConfigUI
```

简单模块可只提供代码示例：

```text
FrameworkConfig
SaveSystem
LocalizationSystem
PlatformServiceRegistry
```

---

# 七、Sample 配置文件

是否需要 Sample 配置文件选择：

```text
C：复杂示例需要
```

复杂示例应提供：

```text
network_sample.json
device_sample.json
display_sample.json
resource_sample.json
```

Sample 配置不得覆盖项目正式配置。

应放在 Samples 自己目录下。

---

# 八、Samples 文档

是否需要 Samples 文档选择：

```text
A：需要
```

每个 Sample 必须提供：

```text
README.md
```

至少包含：

```text
用途
依赖
运行步骤
预期结果
验证清单
扩展方式
注意事项
```

---

# 九、业务参考示例

是否需要业务参考示例选择：

```text
A：需要
```

允许提供：

```text
VehicleSimulation Sample
TrainingSystem Sample
ScenarioSystem Sample
SimulationSync Sample
DeviceIntegration Sample
CustomerModule Sample
```

但必须放在 FeatureModule Samples 中。

不得进入 Platform 主代码。

---

# 十、联调示例

是否需要联调示例选择：

```text
A：需要
```

建议提供：

```text
Client ↔ Server
TCP ↔ TCP
WebSocket ↔ WebSocket
Downloader ↔ HttpServer
PLC Simulator ↔ PLC Client
Device Simulator ↔ Device Driver
AssetBundle Build ↔ Load
```

联调示例应默认可在无真实设备情况下运行。

---

# 十一、真实设备与模拟设备规则

Samples 是否允许使用真实设备选择：

```text
C：两套都提供，模拟优先，真实设备可选
```

规则：

```text
Sample 默认必须可运行
没有真实设备也能运行
真实设备作为增强验证
```

默认使用：

```text
MockDevice
SimulatedDevice
PlaybackDevice
```

真实设备示例必须注明：

```text
需要真实设备
需要端口 / IP
需要驱动
需要现场配置
```

---

# 十二、自动化测试与人工验证

Sample 是否允许成为自动化测试基础的最终规则：

```text
D：人工验证优先
```

原因：

```text
Unity License / Unity Editor / 设备环境 / VR 环境 / PLC 环境不一定能在自动化环境中运行。
```

Codex 不强制提供 CI 或 Unity Test Runner。

但每个 Sample 完成后必须提供：

```text
验证步骤
预期结果
验证清单
```

由开发人员在本地 Unity 环境中完成最终验证。

---

# 十三、验证步骤模板

Sample README 必须包含：

```text
验证步骤：
1. 打开 SampleScene
2. 执行指定操作
3. 观察窗口 / Console / 状态面板

预期结果：
1. 指定状态变为 Online / Success
2. Console 无异常
3. 输出符合预期

验证清单：
[ ] 场景可打开
[ ] 操作可完成
[ ] 无异常日志
[ ] 结果符合预期
```

---

# 十四、建议目录结构

```text
Assets/ByFramework/Samples
├─ GettingStarted
├─ PlatformServiceRegistry
├─ FrameworkConfig
├─ SaveSystem
├─ ResourceSystem
├─ NetworkSystem
└─ RuntimeConfigUI
```

模块内：

```text
Assets/ByFramework/Platform/NetworkSystem/Samples
Assets/ByFramework/Platform/DeviceIntegration/Samples
Assets/ByFramework/FeatureModule/SimulationSync/Samples
Assets/ByFramework/FeatureModule/VehicleSimulation/Samples
```

---

# 十五、禁止事项

P3.21 阶段禁止：

```text
实现 Samples 代码
创建 Sample Scene
把 Sample 代码写入 Runtime 主逻辑
让 Platform Sample 依赖 FeatureModule
让 Core / Platform 依赖 Samples
让 Sample 成为框架必要运行条件
默认依赖真实设备才能运行
缺少 README 的 Sample
缺少验证步骤的 Sample
```

---

# 十六、P3.21 输出物

P3.21 应输出：

```text
Documentation/35_SamplesFoundation.md
Documentation/00_ByFramework_Current_Context.md 更新
Documentation/02_Roadmap.md 更新
Documentation/03_Todo.md 更新
Documentation/04_Changelog.md 更新
```

不要求输出 Runtime、Scene 或 Sample 代码。

---

# 十七、阶段关闭条件

P3.21 关闭条件：

```text
Samples 定位明确
Samples 目录规则明确
Platform Samples 与 Feature Samples 边界明确
Samples 必须可删除并可由 BuildProfile 裁剪明确
复杂模块提供 Sample Scene 明确
复杂示例提供 Sample Config 明确
每个 Sample 必须提供 README 明确
业务参考示例放 FeatureModule 明确
联调示例需要明确
模拟优先、真实设备可选明确
人工验证优先、必须提供验证步骤和预期结果明确
禁止事项写入文档
Current_Context 更新完成
Roadmap / Todo / Changelog 同步完成
```

满足以上条件后，可进入：

```text
P3.22 ExistingToolsAssetAudit
```

---

# 十八、最终结论

Samples 是教学和可运行参考示例，不是 Runtime 主逻辑。

它帮助使用者理解 ByFramework，但必须可删除、可裁剪、可验证，并且不能污染框架架构。

# R8.1 DisplaySystem Runtime API Freeze

> 阶段性质：Runtime API Freeze
> 不是 Runtime Implementation
> 本阶段只冻结 DisplaySystem 的运行时 API、核心模型、上下文边界、结果模型、配置归属和依赖方向
> 不实现具体显示切换代码

---

# 一、阶段目标

R8.1 的目标是冻结 DisplaySystem Runtime API，为后续 R8.2 DisplaySystem Runtime Implementation 提供明确实现依据。

本阶段只允许完成：

```text
DisplaySystem 服务边界冻结
IDisplayService 方法面冻结
Foundation 必需模型冻结
未来扩展模型边界声明
DisplayContext Immutable Snapshot 规则冻结
FrameworkConfig / SaveSystem / Runtime State 边界冻结
结果模型与事件行为冻结
与 R1 ~ R7 已关闭模块的依赖关系冻结
验证方式冻结
```

本阶段不进入具体运行时代码实现。

---

# 二、架构位置

DisplaySystem 属于：

```text
Platform/DisplaySystem
```

它是 Platform 层的统一显示环境抽象服务。

它不属于：

```text
Core
FeatureModule
UISystem
```

---

# 三、核心定位

DisplaySystem 的核心定位选择：

```text
A：只负责显示环境抽象、显示配置选择、显示目标组织与当前显示状态发布
```

---

## 3.1 它负责

```text
维护当前生效 DisplayProfile
维护当前生效 DisplayMode
维护当前可用 DisplayTarget 集合快照
发布当前只读 DisplayContext
执行显示配置应用请求与结果表达
聚合 FrameworkConfig 默认配置与 SaveSystem 用户偏好
向 UISystem 和其他消费者暴露设备无关的显示状态
```

---

## 3.2 它不负责

```text
直接管理 UI 焦点
直接管理 UI 窗口栈
直接管理 UI Theme
持有业务 Camera 对象
持有 FeatureModule 状态
执行具体业务场景布置
实现具体 Unity Multi Display / 独立窗口 / VR 驱动代码
直接控制 RenderTexture 渲染业务
```

---

# 四、为什么需要进入 Freeze

DisplaySystem 必须进入 R8.1 Freeze，原因如下：

```text
Foundation 只冻结了设计方向，没有冻结最终 Runtime API 面
UISystem 后续需要单向消费 DisplayContext，如果没有 Freeze，后续很容易反向渗透显示细节
FrameworkConfig / SaveSystem / Runtime State 的显示归属如果不先冻结，后续实现会混淆启动配置、用户偏好和当前生效状态
DisplaySystem 涉及多屏、窗口、VR 等长期扩展点，如果第一版不控制 Freeze 规模，Runtime 很容易过度设计
```

如果不进入 Freeze，会直接影响 Foundation 落地：

```text
会影响 DisplayContext 的唯一职责定义
会影响 UISystem 对 DisplaySystem 的消费边界
会影响 FrameworkConfig 与 SaveSystem 的显示归属边界
会影响 R8.2 实现时是否出现 API 漂移
```

---

# 五、Freeze 规模控制

R8.1 第一版 Freeze 只冻结 Foundation 落地所必需的最小 API 面。

本轮优先冻结：

```text
服务边界
当前显示状态快照
最小配置应用能力
可用 Profile / Target 查询能力
结果模型
事件行为
```

本轮不提前冻结：

```text
具体窗口坐标与尺寸编辑 API
具体多屏拓扑编辑 API
具体 VR 设备控制 API
具体 Camera 绑定 API
具体 RenderTexture 绑定 API
机器校准工作流 API
```

---

# 六、Foundation 必需模型 与 未来扩展模型

## 6.1 Foundation 必需模型

以下模型必须进入 R8.1 Freeze：

```text
DisplayProfileId
DisplayProfileSnapshot
DisplayMode
DisplayTargetId
DisplayTargetDescriptor
DisplayContext
DisplayApplyStatus
DisplayApplyResult
DisplayQueryStatus
DisplayContextChangedEvent
```

原因：

```text
这些模型直接支撑 Foundation 已冻结的“显示配置、显示目标、当前显示状态、结果表达”主路径
没有这些模型，DisplaySystem 无法形成最小可消费 Runtime API
这些模型进入 Freeze 后，R8.2 才能在不漂移 API 的前提下实现
```

如果不进入 Freeze，对 Foundation 落地的影响：

```text
DisplaySystem 无法形成统一服务边界
DisplayContext 无法成为权威运行时状态
FrameworkConfig / SaveSystem / Runtime State 边界无法落地
UISystem 后续无法稳定消费显示上下文
```

---

## 6.2 未来扩展模型

以下模型保留为未来扩展模型，本轮不进入 R8.1 Freeze：

```text
DisplayRegionId
DisplayRegionDescriptor
DisplayLayoutSnapshot
DisplayWindowDescriptor
DisplayCalibrationSnapshot
DisplayAdapterDescriptor
VRDisplayDescriptor
```

原因：

```text
这些模型属于显示环境细化、机器校准、窗口控制、VR 细节和适配器分层
Foundation 已经给出方向，但当前 R8.1 不需要它们才能形成最小 Runtime API
如果现在冻结，容易把第一版 API 面做得过宽
```

如果不进入 Freeze，对 Foundation 落地的影响：

```text
不会阻塞第一版 DisplaySystem Runtime API 成立
不会阻塞后续 UISystem 只读消费当前显示状态
会把更细的显示布局问题延后到后续阶段单独收口
```

---

# 七、服务边界冻结

DisplaySystem 只作为 Platform 统一显示服务暴露：

```text
IDisplayService
```

它是显示抽象能力边界，不是：

```text
UI 窗口系统
UI 焦点系统
业务 Camera 编排系统
VR 驱动系统
窗口管理工具总线
```

---

# 八、IDisplayService 方法面冻结

建议冻结以下 API：

```csharp
public interface IDisplayService
{
    DisplayContext GetCurrentDisplayContext();

    IReadOnlyList<DisplayProfileId> GetAvailableProfiles();

    bool TryGetProfile(
        DisplayProfileId profileId,
        out DisplayProfileSnapshot profile,
        out DisplayQueryStatus status);

    IReadOnlyList<DisplayTargetDescriptor> GetAvailableTargets();

    bool TryGetTarget(
        DisplayTargetId targetId,
        out DisplayTargetDescriptor target,
        out DisplayQueryStatus status);

    DisplayMode GetCurrentMode();

    DisplayApplyResult ApplyProfile(
        DisplayProfileId profileId,
        bool persistPreference = true);

    event Action<DisplayContextChangedEvent> DisplayContextChanged;
}
```

---

## 8.1 GetCurrentDisplayContext

行为：

```text
返回当前只读显示上下文快照
快照只表达当前生效 Profile、Mode、Target 集合和状态版本
不暴露内部可变集合
```

为什么需要进入 Freeze：

```text
DisplayContext 是 DisplaySystem 的唯一运行时权威状态出口
```

如果不进入 Freeze：

```text
Foundation 中“UISystem 单向消费 DisplayContext”的关系无法落地
```

---

## 8.2 GetAvailableProfiles

行为：

```text
返回当前可用 Profile 标识集合
顺序稳定
不等同于当前已生效 Profile
```

为什么需要进入 Freeze：

```text
Foundation 已明确 DisplayProfile 是显示配置的核心模型
```

如果不进入 Freeze：

```text
R8.2 将无法形成最小配置查询路径
```

---

## 8.3 TryGetProfile

行为：

```text
根据 DisplayProfileId 查询 Profile 快照
成功返回 true
失败返回 false，并输出明确状态
不抛出预期运行时异常
```

为什么需要进入 Freeze：

```text
需要明确 Profile 查询是结果模型，不是异常控制流
```

如果不进入 Freeze：

```text
Profile 查询行为会在实现阶段漂移
```

---

## 8.4 GetAvailableTargets

行为：

```text
返回当前可用显示目标集合
目标只描述稳定身份、能力类别和连接可用性
不绑定 UI 或业务对象
```

为什么需要进入 Freeze：

```text
Foundation 已冻结显示目标作为长期核心概念
```

如果不进入 Freeze：

```text
DisplayMode 与 DisplayContext 无法形成最小闭环
```

---

## 8.5 TryGetTarget

行为：

```text
根据 DisplayTargetId 查询目标描述
成功返回 true
失败返回 false，并输出明确状态
```

为什么需要进入 Freeze：

```text
需要把显示目标查询面与未来窗口/布局细节分开
```

如果不进入 Freeze：

```text
后续很容易把目标查询和具体窗口实现耦合
```

---

## 8.6 GetCurrentMode

行为：

```text
返回当前生效 DisplayMode
DisplayMode 只表达模式身份，不表达实现细节
```

为什么需要进入 Freeze：

```text
Foundation 已明确单屏、多屏、独立窗口、混合模式是长期能力边界
```

如果不进入 Freeze：

```text
DisplayContext 将缺少最小模式状态
```

---

## 8.7 ApplyProfile

行为：

```text
尝试应用指定 Profile
可选持久化用户偏好
返回结果模型，不使用预期运行时异常表达切换失败
成功提交后才允许发布 DisplayContextChanged
失败时保持旧显示上下文
```

为什么需要进入 Freeze：

```text
需要先冻结显示切换结果路径和失败恢复规则
```

如果不进入 Freeze：

```text
FrameworkConfig / SaveSystem / Runtime State 的显示归属会在实现阶段混淆
```

---

## 8.8 DisplayContextChanged

行为：

```text
只在新上下文成功提交后触发
事件只发布 DisplayContextChangedEvent
不直接驱动 UI 或业务流程
```

为什么需要进入 Freeze：

```text
需要先冻结状态变更事件的发布时机
```

如果不进入 Freeze：

```text
后续实现容易出现“切换失败也发布事件”或“事件持有可变上下文”的问题
```

---

# 九、核心模型冻结

## 9.1 DisplayProfileId

```csharp
public readonly struct DisplayProfileId
{
    public string Value { get; }
}
```

规则：

```text
使用稳定配置身份
与显示名称、本地化文本、窗口实例、设备实例解耦
```

---

## 9.2 DisplayProfileSnapshot

```csharp
public sealed class DisplayProfileSnapshot
{
    public DisplayProfileId ProfileId { get; }
    public DisplayMode Mode { get; }
    public IReadOnlyList<DisplayTargetId> TargetIds { get; }
}
```

规则：

```text
ProfileSnapshot 是只读配置快照
第一版只冻结最小字段，不提前冻结布局细节
```

---

## 9.3 DisplayMode

```csharp
public enum DisplayMode
{
    SingleScreen = 0,
    UnityMultiDisplay = 1,
    SeparateWindows = 2,
    Hybrid = 3,
    VREnabled = 4
}
```

规则：

```text
第一版只冻结模式身份
不冻结具体 Unity 实现路径
```

---

## 9.4 DisplayTargetId

```csharp
public readonly struct DisplayTargetId
{
    public string Value { get; }
}
```

规则：

```text
TargetId 只表达稳定显示目标身份
不是窗口句柄
不是设备对象引用
不是 UI 对象引用
```

---

## 9.5 DisplayTargetDescriptor

```csharp
public sealed class DisplayTargetDescriptor
{
    public DisplayTargetId TargetId { get; }
    public string TargetType { get; }
    public bool IsConnected { get; }
}
```

说明：

```text
TargetType 用于表达目标类别，例如 MainView / InstrumentView / DebugView / VR
第一版不冻结更细窗口和拓扑字段
```

---


---

## 9.5.1 DisplayTargetDescriptor.TargetType 稳定值规则

```text
TargetType 是稳定目标类别标识，不是显示名称，不是本地化文本，不是任意自由文本。
```

R8.2 实现必须至少内置并统一维护以下稳定类别值方向：

```text
MainView
LeftView
RightView
InstrumentView
CenterConsoleView
RearMirrorView
DebugView
ControlView
VR
```

规则：

```text
DisplayTargetDescriptor.TargetType 允许第一版保持 string，但实现不得在各处散落魔法字符串。
应通过常量、静态类或等价集中定义方式维护稳定类别。
显示名称、本地化文本、UI Label 不得作为 TargetType。
```


## 9.6 DisplayContext

```csharp
public sealed class DisplayContext
{
    public DisplayProfileId CurrentProfileId { get; }
    public DisplayMode CurrentMode { get; }
    public IReadOnlyList<DisplayTargetDescriptor> ActiveTargets { get; }
    public int Version { get; }
}
```

含义：

```text
DisplayContext 是当前运行时唯一权威显示状态快照
只读
不可变
不作为可变容器
```

为什么必须进入 Freeze：

```text
它承接 Foundation 到 Runtime 的核心状态出口
```

---

# 十、Immutable Snapshot 规则冻结

DisplayContext 必须遵守以下 Immutable Snapshot 规则：

```text
DisplayContext 创建后不可修改
内部集合必须以只读快照形式暴露
事件不得长期持有可变内部对象引用
消费者不得通过返回对象反向修改服务内部状态
每次成功切换都生成新版本快照
旧快照在新快照生成后仍保持语义稳定
```

同样规则适用于：

```text
DisplayProfileSnapshot
未来进入 Runtime 的 DisplayLayoutSnapshot
未来进入 Runtime 的 DisplayCalibrationSnapshot
```

---

# 十一、FrameworkConfig / SaveSystem / Runtime State 边界冻结

## 11.1 FrameworkConfig 归属

FrameworkConfig 只持有：

```text
defaultDisplayProfileId
defaultDisplayMode
enableVR
displayProfileSelector
```

含义：

```text
FrameworkConfig 只负责启动默认配置
不负责用户偏好
不负责当前运行时真实显示状态
```

---

## 11.2 SaveSystem 归属

SaveSystem 可持有：

```text
userDisplayProfilePreference
userDisplayModePreference
machineDisplayPreferenceOverride
```

含义：

```text
这些属于用户偏好或机器偏好持久化
不属于 FrameworkConfig
```

---

## 11.3 Runtime State 归属

Runtime State 归属：

```text
DisplayContext
```

含义：

```text
当前真正生效的显示 Profile、Mode、Target 集合只存在于 DisplayContext
```

---

## 11.4 禁止事项

禁止：

```text
把 current display state 写回 FrameworkConfig
把 DisplayContext 直接持久化到 SaveSystem 作为整体对象
让 SaveSystem 持有 FrameworkConfig 启动默认配置的主权
把窗口实例、Camera 实例、UI 实例写入 FrameworkConfig
```

---

# 十二、结果模型冻结

## 12.1 DisplayQueryStatus

```csharp
public enum DisplayQueryStatus
{
    Success = 0,
    ProfileNotFound = 1,
    TargetNotFound = 2,
    ProviderFailure = 3
}
```

---

## 12.2 DisplayApplyStatus

```csharp
public enum DisplayApplyStatus
{
    Success = 0,
    ProfileNotFound = 1,
    ModeUnsupported = 2,
    TargetUnavailable = 3,
    PersistenceFailure = 4,
    ProviderFailure = 5
}
```

---

## 12.3 DisplayApplyResult

```csharp
public sealed class DisplayApplyResult
{
    public bool Success { get; }
    public DisplayApplyStatus Status { get; }
    public DisplayProfileId RequestedProfileId { get; }
    public DisplayProfileId PreviousProfileId { get; }
    public DisplayProfileId CurrentProfileId { get; }
}
```

规则：

```text
预期运行时冲突与不可用状态使用结果模型表达
调用约束错误仍可抛出参数异常
```

---

# 十三、事件模型冻结

## 13.1 DisplayContextChangedEvent

```csharp
public readonly struct DisplayContextChangedEvent
{
    public DisplayProfileId PreviousProfileId { get; }
    public DisplayProfileId CurrentProfileId { get; }
    public DisplayMode PreviousMode { get; }
    public DisplayMode CurrentMode { get; }
}
```

规则：

```text
事件只表达一次已提交的显示上下文切换事实
不持有 DisplayContext 快照
调用方如需当前上下文，应再次调用 GetCurrentDisplayContext()
```

---

# 十四、与已关闭模块的依赖关系冻结

## 14.1 与 R1 PlatformServiceRegistry 的关系

接入方向：

```text
IDisplayService
```

注册方向：

```csharp
PlatformServiceRegistry.Register<IDisplayService>(displayService);
```

---

## 14.2 与 R2 FrameworkConfig 的关系

关系：

```text
DisplaySystem 读取启动默认显示配置
FrameworkConfig 不持有当前运行时显示状态
```

---

## 14.3 与 R3 SaveSystem 的关系

关系：

```text
DisplaySystem 可选持久化用户显示偏好
SaveSystem 不拥有 Runtime State 主权
```

---

## 14.4 与 R4 ResourceSystem 的关系

关系：

```text
可作为未来 DisplayProfile 资源来源
R8.1 只冻结依赖方向，不冻结加载实现
```

---

## 14.5 与 R5 AssetBundle 的关系

关系：

```text
仅作为未来资源分发候选
R8.1 不直接依赖 AssetBundle Runtime API
```

---

## 14.6 与 R6 LocalizationSystem 的关系

关系：

```text
DisplaySystem 不直接依赖 LocalizationSystem
显示名称、本地化文本属于消费层问题
```

---

## 14.7 与 R7 InputSystem 的关系

关系：

```text
DisplaySystem 不直接依赖 InputSystem
UISystem 后续可同时消费 InputSystem 与 DisplaySystem
```

---

# 十五、验证方式冻结

R8.1 需要完成以下验证说明：

```text
检查 DisplaySystem 仍保持显示抽象能力定位
检查 IDisplayService 方法面只覆盖最小配置查询、目标查询、状态快照和配置应用
检查第一版 Freeze 已明确区分 Foundation 必需模型与未来扩展模型
检查 DisplayContext Immutable Snapshot 规则明确
检查 FrameworkConfig / SaveSystem / Runtime State 边界明确
检查与 R1 ~ R7 的依赖关系保持单向且最小
检查文档未进入 R8.2 Runtime Implementation
```

---

# 十六、禁止事项

R8.1 阶段禁止：

```text
进入 R8.2 Runtime Implementation
创建 DisplaySystem Runtime 代码
讨论具体 Unity Multi Display 实现
讨论具体独立窗口实现
讨论具体 VR 驱动实现
让 DisplaySystem 吞并 UISystem
让 DisplaySystem 持有 FeatureModule 状态
```

---

# 十七、阶段输出物

R8.1 应输出：

```text
Documentation/DisplaySystemRuntimeAPIFreeze.md
```

---

# 十八、关闭条件

R8.1 关闭条件：

```text
候选模块定位明确
服务边界明确
方法面明确
Foundation 必需模型明确
未来扩展模型边界明确
Immutable Snapshot 规则明确
FrameworkConfig / SaveSystem / Runtime State 边界明确
结果模型与事件行为明确
与 R1 ~ R7 的依赖关系明确
Freeze 规模受控，未过度设计
```

满足以上条件后，R8.1 可关闭。

下一阶段仍为：

```text
R8.2 DisplaySystem Runtime Implementation
```

但本次不进入。

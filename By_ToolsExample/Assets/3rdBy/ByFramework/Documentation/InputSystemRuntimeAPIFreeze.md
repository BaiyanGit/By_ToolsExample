# R7.1 InputSystem Runtime API Freeze

> 阶段性质：Runtime API Freeze  
> 不是 Runtime Implementation  
> 本阶段只冻结 InputSystem 的运行时 API、核心模型、上下文边界、设备抽象、映射关系和集成方向  
> 不实现代码  

---

# 一、阶段目标

R7.1 的目标是冻结 InputSystem Runtime API，为 R7.2 InputSystem Runtime Implementation 提供明确实现依据。

本阶段只允许完成：

```text
InputSystem 服务边界冻结
IInputService 方法面冻结
InputAction 模型冻结
InputContext 模型冻结
输入设备抽象冻结
输入映射关系冻结
与 UISystem 的关系冻结
与 FeatureModule 的关系冻结
与 Unity Input System 的关系冻结
错误结果与验证方式冻结
```

本阶段不进入具体代码实现。

---

# 二、架构位置

InputSystem 属于：

```text
Platform/InputSystem
```

它是 Platform 层的统一输入抽象服务。

它不属于 Core。

它不属于 FeatureModule。

---

# 三、核心定位

InputSystem 的核心定位选择：

```text
A：只负责设备无关输入抽象、动作映射、上下文管理与输入分发
```

---

## 3.1 它负责

```text
维护稳定 InputAction 身份
维护 InputContext 激活状态与优先级
管理输入动作与设备输入源的映射
向 UISystem、FeatureModule 与工具模块暴露设备无关输入动作
暴露只读输入上下文快照
标准化输入值和输入阶段
聚合输入设备描述与能力快照
```

---

## 3.2 它不负责

```text
实现具体业务语义
持有 FeatureModule 业务状态
吞并 UISystem 焦点、窗口、导航逻辑
直接实现 UI 输入流程
直接接入 Unity Input System Package
实现手柄驱动、热插拔、硬件协议细节
```

---

# 四、服务边界冻结

InputSystem 只作为 Platform 统一输入服务暴露：

```text
IInputService
```

它是输入抽象能力边界，不是：

```text
业务状态机
UI 导航系统
具体设备驱动框架
FeatureModule 命令总线
```

---

# 五、IInputService 方法面冻结

建议冻结以下 API：

```csharp
public interface IInputService
{
    InputContextSnapshot GetCurrentContextSnapshot();

    IReadOnlyList<InputContextHandle> GetActiveContexts();

    IReadOnlyList<InputActionDescriptor> GetAvailableActions();

    bool TryGetActionDescriptor(InputActionId actionId, out InputActionDescriptor descriptor);

    bool IsActionAvailable(InputActionId actionId);

    InputContextHandle ActivateContext(InputContextRegistration registration);

    bool TryReleaseContext(InputContextHandle handle);

    bool TryResolveActionState(InputActionId actionId, out InputActionState state);

    event Action<InputActionEvent> ActionTriggered;
}
```

---

## 5.1 GetCurrentContextSnapshot

行为：

```text
返回当前只读输入上下文快照
快照只表达当前激活上下文、优先级、独占与透传关系
不暴露可变内部集合
```

---

## 5.2 GetActiveContexts

行为：

```text
返回当前激活的 Context Handle 集合
顺序稳定
不保证调用方可通过 Handle 直接修改内部上下文
```

---

## 5.3 GetAvailableActions

行为：

```text
返回当前已注册动作描述集合
动作身份稳定，与当前绑定、显示名称和设备实例解耦
```

---

## 5.4 TryGetActionDescriptor

行为：

```text
根据 InputActionId 查询动作描述
成功返回 true
动作不存在返回 false
不抛出预期运行时异常
```

---

## 5.5 IsActionAvailable

行为：

```text
判断指定动作在当前上下文下是否可被消费
只回答动作可见性与上下文可达性
不等同于设备当前是否有输入
```

---

## 5.6 ActivateContext

行为：

```text
显式激活一个 InputContext
返回稳定 Context Handle
同一 Owner 可激活多个 Context，但必须明确优先级与消费规则
```

失败策略：

```text
无效注册参数属于调用约束错误，可抛出参数异常
上下文冲突、所有者失效或规则不满足属于结果模型范围
```

---

## 5.7 TryReleaseContext

行为：

```text
显式释放指定 Context Handle
成功返回 true
Handle 不存在、已失效或不归当前运行时所有时返回 false
```

---

## 5.8 TryResolveActionState

行为：

```text
查询指定动作的当前解析状态
只读返回最近一次已知状态快照
不强制依赖具体输入后端
```

---

## 5.9 ActionTriggered

行为：

```text
当输入动作被当前上下文成功解析后触发
事件只发布 InputActionEvent
不直接驱动 UI 或业务流程
```

---

# 六、InputAction 模型冻结

## 6.1 InputActionId

```csharp
public readonly struct InputActionId
{
    public string Value { get; }
}
```

规则：

```text
使用稳定语义标识
与设备、显示名称、本地化文本、当前绑定和 Unity ActionAsset 解耦
```

格式方向：

```text
<Scope>.<ActionName>
```

示例：

```text
UI.Confirm
UI.Cancel
UI.Up
UI.Down
Tool.ToggleDebugPanel
Debug.ToggleStats
VehicleSimulation.Throttle
Training.NextStep
```

---

## 6.2 InputActionDescriptor

```csharp
public sealed class InputActionDescriptor
{
    public InputActionId ActionId { get; }
    public string Scope { get; }
    public InputValueKind ValueKind { get; }
    public bool IsPlatformOwned { get; }
    public bool IsUserRebindAllowed { get; }
}
```

规则：

```text
Platform 可声明 UI / Tool / Debug 等通用动作
业务动作必须归属 FeatureModule
Framework 不定义具体业务键位
```

---

## 6.3 Alias / Migration

冻结规则：

```text
Action 标识变更必须通过显式 Alias 或 Migration 处理
Alias 只参与身份迁移
Alias 不参与运行时显示
用户绑定迁移由 SaveSystem 消费迁移规则完成
```

---

# 七、InputValue 模型冻结

```csharp
public enum InputValueKind
{
    Button = 0,
    Axis1D = 1,
    Axis2D = 2,
    Pointer = 3,
    Text = 4,
    DeviceState = 5
}
```

说明：

```text
Foundation 已冻结值类型集合
R7.1 继续冻结 Runtime API 对这些值类型的消费边界
本阶段不冻结具体后端事件载荷实现
```

---

## 7.1 InputActionState

```csharp
public sealed class InputActionState
{
    public InputActionId ActionId { get; }
    public InputStage Stage { get; }
    public InputValueKind ValueKind { get; }
    public bool IsAvailable { get; }
}
```

含义：

```text
表达动作最近一次已知解析状态
不承载底层设备实现细节
不承载 UI 焦点信息
```

---

## 7.2 InputActionEvent

```csharp
public readonly struct InputActionEvent
{
    public InputActionId ActionId { get; }
    public InputStage Stage { get; }
    public InputValueKind ValueKind { get; }
    public InputContextHandle ContextHandle { get; }
    public InputActionResolveStatus Status { get; }
}
```

冻结规则：

```text
InputActionEvent 只表达一次已解析输入动作事实
它不持有 UI 对象
它不持有 FeatureModule 对象
它不持有具体设备对象
它不持有 Unity Input System 原生对象
ActionTriggered 只发布 InputActionEvent，不直接驱动 UI 或业务流程
```

---

## 7.3 InputStage

```csharp
public enum InputStage
{
    Started = 0,
    Performed = 1,
    Canceled = 2,
    ValueChanged = 3
}
```

---

# 八、InputContext 模型冻结

## 8.1 InputContextHandle

```csharp
public readonly struct InputContextHandle
{
    public string Value { get; }
}
```

规则：

```text
Handle 只表示上下文实例身份
Handle 不是业务对象引用
Handle 不暴露内部可变状态
```

---

## 8.2 InputContextRegistration

```csharp
public sealed class InputContextRegistration
{
    public string ContextId { get; }
    public string OwnerId { get; }
    public int Priority { get; }
    public bool Exclusive { get; }
    public bool PassThrough { get; }
    public IReadOnlyList<InputActionId> AllowedActions { get; }
}
```

规则：

```text
Context 必须有 Owner
Context 必须显式 Activate
Context 必须显式 Release
Owner 失效后上下文必须可回收
高优先级 Context 优先处理同一 Action
独占 Context 可阻止低优先级 Context 收到动作
透传 Context 可只拦截部分动作
冲突处理必须确定且可诊断
```

---

## 8.3 InputContextSnapshot

```csharp
public sealed class InputContextSnapshot
{
    public IReadOnlyList<InputContextHandle> ActiveContexts { get; }
    public IReadOnlyList<InputActionId> ResolvableActions { get; }
}
```

含义：

```text
只读快照
表达当前激活上下文和当前可解析动作集合
不作为可变上下文容器
```

---

# 九、输入设备抽象冻结

## 9.1 InputDeviceKind

```csharp
public enum InputDeviceKind
{
    Keyboard = 0,
    Mouse = 1,
    Gamepad = 2,
    VRController = 3,
    HardwareButton = 4,
    IndustrialControlPanel = 5,
    CustomDevice = 6
}
```

说明：

```text
R7.1 只冻结设备类别与抽象接口方向
不实现手柄驱动
不实现热插拔
不实现硬件协议
```

---

## 9.2 InputDeviceDescriptor

```csharp
public sealed class InputDeviceDescriptor
{
    public string DeviceId { get; }
    public InputDeviceKind DeviceKind { get; }
    public string ProviderType { get; }
    public bool IsConnected { get; }
}
```

规则：

```text
描述设备身份与能力来源
不暴露具体驱动对象
不承载 FeatureModule 状态
```

---

## 9.3 InputAdapter 边界

InputAdapter 负责：

```text
发现设备
描述设备能力
读取原始输入
标准化输入值
上报连接、断开、异常状态
```

InputAdapter 不负责：

```text
业务语义解释
UISystem 逻辑
FeatureModule 状态管理
具体业务命令执行
```

---

# 十、输入映射关系冻结

## 10.1 映射方向

InputSystem Runtime 只冻结以下映射方向：

```text
InputDevice / RawInput
-> InputBinding
-> InputActionId
-> InputContext
-> InputActionEvent
```

---

## 10.2 InputBindingDescriptor

```csharp
public sealed class InputBindingDescriptor
{
    public InputActionId ActionId { get; }
    public InputDeviceKind DeviceKind { get; }
    public string BindingPath { get; }
    public bool IsComposite { get; }
}
```

规则：

```text
Binding 只描述映射关系
Binding 不等于 Action 身份
Binding 可被 Project / FeatureModule / User Override 覆盖
```

---

## 10.3 InputProfileSnapshot

```csharp
public sealed class InputProfileSnapshot
{
    public string ProfileId { get; }
    public int Version { get; }
    public IReadOnlyList<InputBindingDescriptor> Bindings { get; }
}
```

规则：

```text
Profile 分层顺序保持：
Framework Default -> Project -> FeatureModule -> User Override
User Override 只保存用户修改项
FeatureModule 只能覆盖自己声明的业务动作
```

---

# 十一、与 UISystem 的关系冻结

正确关系：

```text
UISystem
-> 消费 InputSystem 暴露的设备无关 UI InputAction
```

规则：

```text
UISystem 不直接读取具体输入设备
InputSystem 不直接实现 UI 导航
UI 焦点、窗口、层级和导航逻辑归属 UISystem
InputSystem 只提供统一动作和上下文能力
```

禁止：

```text
让 InputSystem 持有 UIWidget / UIWindow 引用
让 InputSystem 决定焦点跳转
让 UISystem 反向控制 InputAction 身份模型
```

---

# 十二、与 FeatureModule 的关系冻结

正确关系：

```text
FeatureModule
-> 声明自己的业务 InputAction / Context / Profile
-> 消费 InputSystem 公开契约
```

规则：

```text
InputSystem 不直接依赖具体业务
InputSystem 不持有 FeatureModule 状态
FeatureModule 业务动作必须归属对应模块
Platform 不集中维护业务键位
```

禁止：

```text
把车辆、训练、客户专属输入流程写进 Platform InputSystem
让 InputSystem 直接驱动业务状态机
```

---

# 十三、与 Unity Input System 的关系冻结

定位：

```text
Unity Input System 是候选底层后端之一
不是 InputSystem Runtime API 本身
```

冻结规则：

```text
R7.1 只冻结抽象边界，不接入 Unity Input System Package
InputActionId 不等于 Unity InputAction 名称
InputContext 不等于 Unity Action Map
Unity Input System 未来只能作为 InputAdapter / Backend 候选实现
InputSystem Runtime API 不得绑定到某个具体 Unity Package 类型
```

---

# 十四、错误模型冻结

## 14.1 InputActionResolveStatus

```csharp
public enum InputActionResolveStatus
{
    Success = 0,
    ActionNotFound = 1,
    ContextBlocked = 2,
    ContextNotFound = 3,
    DeviceUnavailable = 4,
    ProviderFailure = 5
}
```

---

## 14.2 InputContextChangeStatus

```csharp
public enum InputContextChangeStatus
{
    Success = 0,
    OwnerInvalid = 1,
    ConflictRejected = 2,
    HandleNotFound = 3,
    ProviderFailure = 4
}
```

说明：

```text
预期运行时冲突与不可用状态使用结果模型表达
调用约束错误仍可抛出参数异常
```

---

# 十五、PlatformServiceRegistry 接入

接口方向：

```text
IInputService
```

注册方向：

```csharp
PlatformServiceRegistry.Register<IInputService>(inputService);
```

获取方向：

```csharp
PlatformServiceRegistry.Get<IInputService>();
PlatformServiceRegistry.TryGet<IInputService>(out var inputService);
```

---

# 十六、验证方式

R7.1 需要完成以下验证说明：

```text
检查 InputSystem 仍保持输入抽象能力定位
检查 IInputService 方法面覆盖动作、上下文、映射与事件
检查 InputAction 模型与具体设备、显示名称和业务文本解耦
检查 InputContext 模型具备 Owner、优先级、独占与透传边界
检查输入设备抽象未下沉到具体驱动实现
检查与 UISystem 保持单向消费关系
检查与 FeatureModule 保持“平台提供抽象、业务模块声明业务动作”的分工
检查与 Unity Input System 的关系仍是后端候选，不是 Runtime API 绑定
检查文档未进入 R7.2 实现
```

---

# 十七、禁止事项

R7.1 阶段禁止：

```text
进入 R7.2 Runtime Implementation
创建 InputSystem Runtime 代码
接入 Unity Input System Package
实现 UI 输入逻辑
实现手柄驱动
实现设备热插拔
实现 FeatureModule 业务输入
让 InputSystem 吞并 UISystem
让 InputSystem 持有 FeatureModule 状态
```

---

# 十八、阶段输出物

R7.1 应输出：

```text
Documentation/InputSystemRuntimeAPIFreeze.md
Documentation/00_ByFramework_Current_Context.md 更新
Documentation/02_Roadmap.md 更新
Documentation/03_Todo.md 更新
Documentation/06_Documentation_Reading_Order.md 更新
Documentation/Changelog.md 更新
```

---

# 十九、关闭条件

R7.1 关闭条件：

```text
服务边界明确
IInputService 方法面明确
InputAction 模型明确
InputContext 模型明确
输入设备抽象明确
输入映射关系明确
与 UISystem 的关系明确
与 FeatureModule 的关系明确
与 Unity Input System 的关系明确
错误模型与验证方式明确
Current Context / Roadmap / Todo / Reading Order / Changelog 已同步
```

满足以上条件后，R7.1 可关闭。

下一阶段仍为：

```text
R7.2 InputSystem Runtime Implementation
```

但本次不进入。

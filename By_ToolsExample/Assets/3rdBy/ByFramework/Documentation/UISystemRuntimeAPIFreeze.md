# R9.1 UISystem Runtime API Freeze

> 阶段性质：Runtime API Freeze
> 不是 Runtime Implementation
> 本阶段只冻结 UISystem 的运行时服务边界、最小 Runtime API 面、当前上下文模型、结果模型、配置归属与依赖方向
> 不实现具体 Unity UI、主题系统、焦点系统、导航系统或业务窗口逻辑

---

# 一、阶段目标

R9.1 的目标是冻结 UISystem Runtime API，为后续 R9.2 UISystem Runtime Implementation 提供明确实现依据。

本阶段只允许完成：

```text
UISystem 服务边界冻结
IUIService 方法面冻结
Foundation 必需 Runtime 模型冻结
未来扩展模型边界声明
UIContext Immutable Snapshot 规则冻结
FrameworkConfig / SaveSystem / Runtime State 边界冻结
与 R1 ~ R8 已关闭模块的依赖关系冻结
验证方式冻结
```

本阶段不进入具体 Runtime 实现。

---

# 二、架构位置

UISystem 属于：

```text
Platform/UISystem
```

它是 Platform 层的统一 UI 抽象服务。

它不属于：

```text
Core
FeatureModule
```

---

# 三、核心定位

UISystem 的第一版 Runtime 定位选择为：

```text
A：只负责 UI 根节点、层级、视图实例与当前 UI 运行时状态的统一抽象与管理
```

它是以下系统的消费方：

```text
DisplaySystem 消费方
InputSystem 消费方
LocalizationSystem 消费方
ResourceSystem 消费方
```

不允许反向依赖：

```text
DisplaySystem -> UISystem
InputSystem -> UISystem
LocalizationSystem -> UISystem
ResourceSystem -> UISystem
```

---

## 3.1 它负责

```text
维护当前已注册 UI Root 集合
维护当前已打开视图集合
维护当前 Layer 状态快照
发布当前只读 UIContext
执行视图打开请求与关闭请求
向消费方暴露最小查询能力
聚合 FrameworkConfig 默认配置与 SaveSystem 用户偏好
消费 DisplayContext、InputAction、LocalizationKey 与 ResourceKey
```

---

## 3.2 它不负责

```text
持有 FeatureModule 状态
执行业务流程
执行业务校验
直接读取具体输入设备
直接检测具体显示设备
管理语言包内容
管理底层资源加载实现
实现 Theme/Focus/Navigation 子系统细节
实现业务窗口、业务页面、业务控件基类
吞并 RuntimeConfigUI
```

---

# 四、为什么需要进入 Freeze

UISystem 必须进入 R9.1 Freeze，原因如下：

```text
Foundation 只冻结了 UISystem 设计方向，没有冻结最小 Runtime API 面
UISystem 位于 Display / Input / Localization / Resource 四个已关闭模块之上，最容易在实现阶段发生 API 漂移
如果不先 Freeze，后续很容易把 UIThemeSystem、UIFocusSystem、UINavigationSystem、UIWindowSystem 一次性塞进首版 Runtime
FrameworkConfig / SaveSystem / Runtime State 的 UI 归属如果不先冻结，后续实现会混淆启动默认值、用户偏好和当前运行态
```

如果不进入 Freeze，会直接影响 Foundation 落地：

```text
会影响 IUIService 的最小服务边界
会影响 UISystem 作为 Display/Input/Localization/Resource 消费方的单向依赖边界
会影响 UIContext 是否能成为权威 Runtime 状态出口
会影响 R9.2 是否在实现阶段临时扩 API
```

---

# 五、Freeze 规模控制

R9.1 第一版 Freeze 只冻结 Foundation 落地所必需的最小 Runtime API 面。

本轮优先冻结：

```text
服务边界
当前 UI 状态快照
最小 Root / Layer / View 查询能力
最小打开 / 关闭能力
结果模型
事件行为
```

本轮不提前冻结：

```text
UIThemeSystem Runtime API
UIFocusSystem Runtime API
UINavigationSystem Runtime API
UIWindowSystem Runtime API
复杂窗口栈策略
动画系统
多显示目标高级适配
Safe Area / 逻辑视口 / RTL 细节
业务 UI 基类
RuntimeConfigUI 运行时细节
```

---

# 六、Foundation 必需模型 与 未来扩展模型

## 6.1 Foundation 必需模型

以下模型必须进入 R9.1 Freeze：

```text
UIRootId
UILayerId
UIKey
UIViewDescriptor
UIOpenOptions
UICloseReason
UIQueryStatus
UIOpenStatus
UICloseStatus
UIOperationResult
UIContext
UIContextChangedEvent
```

原因：

```text
这些模型直接支撑 Foundation 已冻结的 UI Root、UI Layer、UIView、UI 当前上下文、打开关闭请求与结果表达主路径
没有这些模型，UISystem 无法形成最小可消费 Runtime API
这些模型进入 Freeze 后，R9.2 才能在不漂移 API 的前提下实现
```

如果不进入 Freeze，对 Foundation 落地的影响：

```text
UISystem 无法形成统一服务边界
UIContext 无法成为权威运行时状态
Display/Input/Localization/Resource 的消费边界无法稳定落地
```

---

## 6.2 未来扩展模型

以下模型保留为未来扩展模型，本轮不进入 R9.1 Freeze：

```text
UIThemeId
UIThemeSnapshot
UIFocusHandle
UIFocusSnapshot
UINavigationGraph
UIWindowId
UIWindowStackSnapshot
UISafeAreaSnapshot
UIDisplayBindingSnapshot
UIAnimationDescriptor
```

原因：

```text
这些模型属于主题、焦点、导航、窗口栈、多显示适配和表现层细化问题
Foundation 已给出方向，但当前 R9.1 不需要它们才能形成最小 Runtime API
现在冻结会让第一版 API 面过大
```

如果不进入 Freeze，对 Foundation 落地的影响：

```text
不会阻塞第一版 UISystem Runtime API 成立
不会阻塞 UISystem 作为 Display/Input/Localization/Resource 消费方
会把更细的 UI 体验问题延后到后续阶段独立收口
```

---

# 七、服务边界冻结

UISystem 第一版只作为 Platform 统一 UI 服务暴露：

```text
IUIService
```

它是 UI 抽象能力边界，不是：

```text
Theme 服务总线
焦点服务总线
导航服务总线
业务页面框架
RuntimeConfigUI 本体
```

---

# 八、IUIService 方法面冻结

建议冻结以下 API：

```csharp
public interface IUIService
{
    UIContext GetCurrentContext();

    IReadOnlyList<UIRootId> GetRegisteredRoots();

    IReadOnlyList<UILayerId> GetAvailableLayers();

    IReadOnlyList<UIViewDescriptor> GetOpenViews();

    bool TryGetView(
        UIKey uiKey,
        out UIViewDescriptor view,
        out UIQueryStatus status);

    UIOperationResult Open(
        UIKey uiKey,
        UIOpenOptions options = null);

    UIOperationResult Close(
        UIKey uiKey,
        UICloseReason reason = UICloseReason.Programmatic);

    event Action<UIContextChangedEvent> ContextChanged;
}
```

---

## 8.1 GetCurrentContext

行为：

```text
返回当前只读 UI 运行时上下文快照
快照只表达当前已注册 Root、可用 Layer、已打开 View 与版本信息
不暴露内部可变集合
```

为什么需要进入 Freeze：

```text
UIContext 是 UISystem 的唯一 Runtime 权威状态出口
```

如果不进入 Freeze：

```text
Foundation 中“UISystem 对外暴露当前 UI 运行态”的关系无法落地
```

---

## 8.2 GetRegisteredRoots

行为：

```text
返回当前已注册 Root 标识集合
顺序稳定
不等同于显示目标集合
```

为什么需要进入 Freeze：

```text
Foundation 已明确 UIRoot 是核心对象
```

如果不进入 Freeze：

```text
R9.2 将无法形成最小 Root 查询路径
```

---

## 8.3 GetAvailableLayers

行为：

```text
返回当前可用 Layer 标识集合
只表达稳定层级身份
不提前冻结排序策略实现细节
```

为什么需要进入 Freeze：

```text
Foundation 已明确 UILayer 是核心对象
```

如果不进入 Freeze：

```text
UIContext 将缺失最小层级状态表达
```

---

## 8.4 GetOpenViews

行为：

```text
返回当前已打开视图快照集合
视图只表达 UIKey、Root、Layer 与可见状态
不持有具体 MonoBehaviour 或业务对象
```

为什么需要进入 Freeze：

```text
Foundation 已明确 UIView 是核心对象
```

如果不进入 Freeze：

```text
R9.2 会在实现期临时决定视图查询边界
```

---

## 8.5 TryGetView

行为：

```text
根据 UIKey 查询当前视图快照
成功返回 true
失败返回 false，并输出明确状态
不抛出预期运行时异常
```

为什么需要进入 Freeze：

```text
需要把视图查询路径先冻结为结果模型，而不是异常控制流
```

如果不进入 Freeze：

```text
视图查询行为会在实现阶段漂移
```

---

## 8.6 Open

行为：

```text
尝试打开指定 UIKey 对应视图
通过 UIOpenOptions 指定目标 Root、Layer、上下文与缓存偏好
返回统一结果模型，不使用预期运行时异常表达打开失败
成功提交后才允许发布 ContextChanged
失败时保持旧 UIContext
```

为什么需要进入 Freeze：

```text
需要先冻结最小打开路径、失败恢复规则和状态提交时机
```

如果不进入 Freeze：

```text
R9.2 很容易临时引入业务参数、主题参数或导航参数
```

---

## 8.7 Close

行为：

```text
尝试关闭指定 UIKey 对应视图
关闭原因只表达关闭事实，不表达业务流程
返回统一结果模型
成功提交后才允许发布 ContextChanged
失败时保持旧 UIContext
```

为什么需要进入 Freeze：

```text
需要先冻结最小关闭路径与结果表达
```

如果不进入 Freeze：

```text
视图关闭行为会在实现期与窗口栈、导航、业务结果混在一起
```

---

## 8.8 ContextChanged

行为：

```text
只在新上下文成功提交后触发
事件只发布 UIContextChangedEvent
不直接驱动业务流程
```

为什么需要进入 Freeze：

```text
需要先冻结 UI 运行态变更事件的发布时机
```

如果不进入 Freeze：

```text
后续实现容易出现“打开失败也发事件”或“事件持有可变上下文”的问题
```

---

# 九、核心模型冻结

## 9.1 UIRootId

```csharp
public readonly struct UIRootId
{
    public string Value { get; }
}
```

规则：

```text
使用稳定 Root 身份
不与具体 GameObject、Canvas、DisplayTarget 或场景路径绑定
```

---

## 9.2 UILayerId

```csharp
public readonly struct UILayerId
{
    public string Value { get; }
}
```

规则：

```text
只表达稳定层级身份
不表达具体排序实现、CanvasOrder 或导航规则
```

---

## 9.3 UIKey

```csharp
public readonly struct UIKey
{
    public string Value { get; }
}
```

规则：

```text
UIKey 是稳定 UI 标识
不绑定资源路径
不绑定 Prefab 名称
不绑定业务类型
```

---

## 9.4 UIViewDescriptor

```csharp
public sealed class UIViewDescriptor
{
    public UIKey UIKey { get; }
    public UIRootId RootId { get; }
    public UILayerId LayerId { get; }
    public bool IsVisible { get; }
}
```

规则：

```text
第一版只冻结最小只读视图快照
不持有 UI 对象引用
不持有 FeatureModule 对象
不持有 Unity 原生对象
```

---

## 9.5 UIOpenOptions

```csharp
public sealed class UIOpenOptions
{
    public UIRootId RootId { get; }
    public UILayerId LayerId { get; }
    public bool AddToHistory { get; }
    public bool CacheOnClose { get; }
}
```

说明：

```text
UIOpenOptions 只表达最小打开策略
本轮不提前冻结导航、主题、动画、业务载荷字段
```

---

## 9.6 UIContext

```csharp
public sealed class UIContext
{
    public IReadOnlyList<UIRootId> RegisteredRoots { get; }
    public IReadOnlyList<UILayerId> AvailableLayers { get; }
    public IReadOnlyList<UIViewDescriptor> OpenViews { get; }
    public int Version { get; }
}
```

含义：

```text
UIContext 是当前运行时唯一权威 UI 状态快照
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

UIContext 必须遵守以下 Immutable Snapshot 规则：

```text
UIContext 创建后不可修改
内部集合必须以只读快照形式暴露
事件不得长期持有可变内部对象引用
消费者不得通过返回对象反向修改服务内部状态
每次成功打开或关闭都生成新版本快照
旧快照在新快照生成后仍保持语义稳定
```

同样规则适用于：

```text
UIViewDescriptor 集合
未来进入 Runtime 的 UIThemeSnapshot
未来进入 Runtime 的 UIFocusSnapshot
未来进入 Runtime 的 UINavigationSnapshot
```

---

# 十一、FrameworkConfig / SaveSystem / Runtime State 边界冻结

## 11.1 FrameworkConfig 归属

FrameworkConfig 只持有：

```text
defaultUIRootId
defaultUILayerId
defaultThemeId
enableRuntimeConfigUI
uiResourcePolicy
uiCachePolicy
```

含义：

```text
FrameworkConfig 只负责启动默认 UI 配置
不负责用户偏好
不负责当前运行时真实 UI 状态
```

---

## 11.2 SaveSystem 归属

SaveSystem 可持有：

```text
userThemePreference
userLastOpenedView
userUIRootPreference
```

含义：

```text
这些属于用户偏好或用户持久化 UI 选择
不属于 FrameworkConfig
```

说明：

```text
R9.1 只冻结归属边界
不要求首版 IUIService 必须暴露这些持久化操作
```

---

## 11.3 Runtime State 归属

Runtime State 归属：

```text
UIContext
```

含义：

```text
当前真实生效的 Root、Layer、OpenViews 只存在于 UIContext
```

---

## 11.4 禁止事项

禁止：

```text
把 current UI state 写回 FrameworkConfig
把 UIContext 整体直接持久化到 SaveSystem 作为整体对象
让 SaveSystem 持有 FrameworkConfig 启动默认配置主权
把具体 GameObject、Canvas、EventSystem、FeatureModule 对象写入 FrameworkConfig
```

---

# 十二、结果模型冻结

## 12.1 UIQueryStatus

```csharp
public enum UIQueryStatus
{
    Success = 0,
    ViewNotFound = 1,
    RootNotFound = 2,
    LayerNotFound = 3,
    ProviderFailure = 4
}
```

---

## 12.2 UIOpenStatus

```csharp
public enum UIOpenStatus
{
    Success = 0,
    ViewAlreadyOpen = 1,
    ResourceUnavailable = 2,
    RootNotFound = 3,
    LayerNotFound = 4,
    ProviderFailure = 5
}
```

---

## 12.3 UICloseStatus

```csharp
public enum UICloseStatus
{
    Success = 0,
    ViewNotFound = 1,
    ViewAlreadyClosed = 2,
    ProviderFailure = 3
}
```

---

## 12.4 UICloseReason

```csharp
public enum UICloseReason
{
    Programmatic = 0,
    UserDismissed = 1,
    ContextReplaced = 2
}
```

规则：

```text
关闭原因只表达关闭事实
不表达业务流程状态
```

---

## 12.5 UIOperationResult

```csharp
public sealed class UIOperationResult
{
    public bool Success { get; }
    public UIKey RequestedUIKey { get; }
    public string OperationType { get; }
    public int PreviousVersion { get; }
    public int CurrentVersion { get; }
    public int StatusCode { get; }
}
```

规则：

```text
第一版使用统一结果模型承载 Open / Close 结果
StatusCode 由调用方法语义解释：
Open 时对应 UIOpenStatus
Close 时对应 UICloseStatus
调用约束错误仍可抛出参数异常
```

---

# 十三、事件模型冻结

## 13.1 UIContextChangedEvent

```csharp
public readonly struct UIContextChangedEvent
{
    public int PreviousVersion { get; }
    public int CurrentVersion { get; }
}
```

规则：

```text
事件只表达一次已提交的 UIContext 变化事实
不持有 UIContext 快照
调用方如需当前上下文，应再次调用 GetCurrentContext()
```

---

# 十四、与已关闭模块的依赖关系冻结

## 14.1 与 R1 PlatformServiceRegistry 的关系

接入方向：

```text
IUIService
```

注册方向：

```csharp
PlatformServiceRegistry.Register<IUIService>(uiService);
```

---

## 14.2 与 R2 FrameworkConfig 的关系

关系：

```text
UISystem 读取启动默认 UI 配置
FrameworkConfig 不持有当前运行时 UI 状态
```

---

## 14.3 与 R3 SaveSystem 的关系

关系：

```text
UISystem 可选持久化用户 UI 偏好
SaveSystem 不拥有 Runtime State 主权
```

---

## 14.4 与 R4 ResourceSystem 的关系

关系：

```text
UISystem 只通过 UIKey / ResourceKey 请求 UI 资源
R9.1 只冻结依赖方向，不冻结具体加载实现
```

---

## 14.5 与 R5 AssetBundle 的关系

关系：

```text
仅作为未来 UI 资源分发来源候选
R9.1 不直接依赖 AssetBundle Runtime API
```

---

## 14.6 与 R6 LocalizationSystem 的关系

关系：

```text
UISystem 消费 LocalizationSystem 获取文本
UISystem 不管理语言包
LocalizationSystem 不反向依赖 UISystem
```

---

## 14.7 与 R7 InputSystem 的关系

关系：

```text
UISystem 消费统一 InputAction
UISystem 不直接读取具体设备
InputSystem 不反向依赖 UISystem
```

---

## 14.8 与 R8 DisplaySystem 的关系

关系：

```text
UISystem 消费只读 DisplayContext
UISystem 按 DisplayContext 绑定或组织 UI Root
DisplaySystem 不反向依赖 UISystem
```

---

# 十五、验证方式冻结

R9.1 需要完成以下验证说明：

```text
检查 UISystem 仍保持 UI 抽象服务定位
检查 IUIService 方法面只覆盖最小 Root / Layer / View 查询与 Open / Close 能力
检查第一版 Freeze 明确区分 Foundation 必需模型与未来扩展模型
检查 UIContext Immutable Snapshot 规则明确
检查 FrameworkConfig / SaveSystem / Runtime State 边界明确
检查与 R1 ~ R8 的依赖关系保持单向且最小
检查文档未进入 R9.2 Runtime Implementation
```

---

# 十六、禁止事项

R9.1 阶段禁止：

```text
进入 R9.2 Runtime Implementation
创建 UISystem Runtime 代码
冻结 UIThemeSystem Runtime API
冻结 UIFocusSystem Runtime API
冻结 UINavigationSystem Runtime API
冻结 UIWindowSystem Runtime API
讨论具体 Unity Canvas / EventSystem 实现细节
让 UISystem 吞并 RuntimeConfigUI
让 UISystem 持有 FeatureModule 状态
```

---

# 十七、阶段输出物

R9.1 应输出：

```text
Documentation/UISystemRuntimeAPIFreeze.md
```

---

# 十八、关闭条件

R9.1 关闭条件：

```text
候选模块定位明确
服务边界明确
方法面明确
Foundation 必需模型明确
未来扩展模型边界明确
Immutable Snapshot 规则明确
FrameworkConfig / SaveSystem / Runtime State 边界明确
与 R1 ~ R8 的依赖关系明确
Freeze 规模受控，未过度设计
```

满足以上条件后，R9.1 可关闭。

下一阶段仍为：

```text
R9.2 UISystem Runtime Implementation
```

但本次不进入。

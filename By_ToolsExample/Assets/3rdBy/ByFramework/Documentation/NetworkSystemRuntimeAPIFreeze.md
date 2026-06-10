# R10.1 NetworkSystem Runtime API Freeze

> 阶段性质：Runtime API Freeze  
> 不是 Runtime Implementation  
> 本阶段只冻结 NetworkSystem 的运行时服务边界、最小 Runtime API 面、NetworkCore / Adapter 边界、连接状态模型、请求/下载结果模型、配置归属与依赖方向  
> 不实现具体 HTTP、Socket、WebSocket、RPC、SimulationSync、数据库或业务协议逻辑

---

# 一、阶段目标

R10.1 的目标是冻结 NetworkSystem Runtime API，为后续 R10.2 NetworkSystem Runtime Implementation 提供明确实现依据。

本阶段只允许完成：

```text
NetworkSystem 服务边界冻结
INetworkService 方法面冻结
NetworkCore / ByFramework Adapter 边界冻结
Foundation 必需 Runtime 模型冻结
未来扩展模型边界声明
NetworkContext Immutable Snapshot 规则冻结
FrameworkConfig / SaveSystem / Runtime State 边界冻结
与 R1 ~ R9 已关闭模块的依赖关系冻结
验证方式冻结
```

本阶段不进入具体 Runtime 实现。

---

# 二、架构位置

NetworkSystem 属于：

```text
Platform/NetworkSystem
```

它是 Platform 层的统一网络抽象服务。

它不属于：

```text
Core
FeatureModule
UISystem
SimulationSync
LicenseSystem
DatabaseSystem
```

---

# 三、核心定位

NetworkSystem 的第一版 Runtime 定位选择为：

```text
A：只负责网络能力抽象、网络请求入口、下载任务入口、连接状态发布与 NetworkCore 适配
```

NetworkSystem 不是业务协议系统。

NetworkSystem 不是仿真同步系统。

NetworkSystem 不是数据库访问层。

NetworkSystem 不是 License 规则系统。

---

## 3.1 它负责

```text
维护当前网络运行状态快照
发布当前只读 NetworkContext
提供最小 HTTP 请求入口
提供最小下载任务入口
提供网络可用性查询能力
表达请求、下载、连接状态结果
消费 FrameworkConfig 中的默认网络配置
可选消费 SaveSystem 中的用户网络偏好
适配纯 C# NetworkCore
```

---

## 3.2 它不负责

```text
业务协议语义
业务数据解释
业务重试策略
账号登录流程
License 授权规则
数据库读写逻辑
仿真状态同步
多人联机规则
RPC 框架
完整 TCP Server
完整 UDP 协议栈
完整 WebSocket 协议栈
Unity Transport / Mirror / Netcode 绑定
UI 提示与错误弹窗
FeatureModule 状态持有
```

---

# 四、NetworkCore / Adapter 边界冻结

R10 必须明确区分：

```text
NetworkCore
ByFramework NetworkSystem Adapter
```

## 4.1 NetworkCore

NetworkCore 是纯 C# 网络核心库方向。

NetworkCore 可以负责：

```text
HTTP 客户端抽象
下载器基础能力
连接状态探测
请求取消
超时控制
基础结果模型
```

NetworkCore 禁止依赖：

```text
UnityEngine
ByFramework
PlatformServiceRegistry
FrameworkConfig
SaveSystem
ResourceSystem
UISystem
FeatureModule
```

## 4.2 NetworkSystem Adapter

ByFramework NetworkSystem Adapter 负责：

```text
把 NetworkCore 能力适配为 INetworkService
读取 FrameworkConfig 默认配置
读取 SaveSystem 用户偏好
生成 NetworkContext
发布 NetworkContextChanged
注册到 PlatformServiceRegistry
```

Adapter 可以依赖 ByFramework 已关闭模块，但不得反向污染 NetworkCore。

---

# 五、为什么需要进入 Freeze

NetworkSystem 必须进入 R10.1 Freeze，原因如下：

```text
Foundation 只冻结了网络方向，没有冻结最终 Runtime API 面
NetworkSystem 容易膨胀为 HTTP / Socket / RPC / Sync / Database / License 混合系统
如果不先 Freeze，R10.2 实现会很容易把业务协议、仿真同步和 UI 错误提示塞进网络层
NetworkCore / Adapter 边界如果不先冻结，后续会把 Unity 或 ByFramework 依赖污染到纯 C# 核心库
```

如果不进入 Freeze，会直接影响 Foundation 落地：

```text
会影响 INetworkService 的最小服务边界
会影响 NetworkCore 是否保持纯 C#
会影响 FrameworkConfig / SaveSystem / Runtime State 的网络归属
会影响后续 LicenseSystem、RuntimeConfigUI、FeatureModule 对网络能力的消费边界
```

---

# 六、Freeze 规模控制

R10.1 第一版 Freeze 只冻结 Foundation 落地所必需的最小 Runtime API 面。

本轮优先冻结：

```text
服务边界
当前网络状态快照
最小 HTTP 请求能力
最小下载任务能力
请求取消能力
结果模型
事件行为
NetworkCore / Adapter 边界
```

本轮不提前冻结：

```text
完整 TCP Server API
完整 UDP API
完整 WebSocket API
完整 RPC API
完整 SimulationSync API
完整数据库访问 API
完整认证 / 登录 API
完整 License 联动 API
完整多人房间 / 匹配 API
完整协议序列化框架
```

---

# 七、Foundation 必需模型 与 未来扩展模型

## 7.1 Foundation 必需模型

以下模型必须进入 R10.1 Freeze：

```text
NetworkEndpoint
NetworkRequestId
NetworkDownloadId
NetworkContext
NetworkConnectionStatus
NetworkRequestOptions
NetworkResponseSnapshot
NetworkDownloadOptions
NetworkDownloadSnapshot
NetworkOperationStatus
NetworkOperationResult
NetworkContextChangedEvent
```

原因：

```text
这些模型直接支撑网络状态、请求、下载、结果表达和上下文发布主路径
没有这些模型，NetworkSystem 无法形成最小可消费 Runtime API
这些模型进入 Freeze 后，R10.2 才能在不漂移 API 的前提下实现
```

如果不进入 Freeze，对 Foundation 落地的影响：

```text
NetworkSystem 无法形成统一服务边界
NetworkContext 无法成为权威 Runtime 状态出口
NetworkCore / Adapter 边界无法稳定落地
```

---

## 7.2 未来扩展模型

以下模型保留为未来扩展模型，本轮不进入 R10.1 Freeze：

```text
NetworkSocketId
NetworkChannelId
WebSocketConnectionSnapshot
RpcRequestDescriptor
RpcResponseDescriptor
ProtocolDescriptor
NetworkAuthContext
NetworkRoomSnapshot
SimulationSyncSnapshot
DatabaseConnectionDescriptor
```

原因：

```text
这些模型属于 Socket、WebSocket、RPC、协议、认证、房间、仿真同步和数据库访问细化问题
Foundation 已给出方向，但当前 R10.1 不需要它们才能形成最小 Runtime API
现在冻结会让第一版 API 面过大
```

如果不进入 Freeze，对 Foundation 落地的影响：

```text
不会阻塞第一版 NetworkSystem Runtime API 成立
不会阻塞后续 FeatureModule 使用基础请求与下载能力
会把更复杂的网络协议问题延后到后续阶段独立收口
```

---

# 八、服务边界冻结

NetworkSystem 第一版只作为 Platform 统一网络服务暴露：

```text
INetworkService
```

它是网络抽象能力边界，不是：

```text
业务协议总线
RPC 总线
数据库访问服务
仿真同步服务
License 校验服务
UI 错误提示服务
```

---

# 九、INetworkService 方法面冻结

建议冻结以下 API：

```csharp
public interface INetworkService
{
    NetworkContext GetCurrentContext();

    bool IsNetworkAvailable();

    NetworkOperationResult SendRequest(
        NetworkEndpoint endpoint,
        NetworkRequestOptions options,
        out NetworkResponseSnapshot response);

    NetworkOperationResult StartDownload(
        NetworkEndpoint endpoint,
        NetworkDownloadOptions options,
        out NetworkDownloadId downloadId);

    bool TryGetDownload(
        NetworkDownloadId downloadId,
        out NetworkDownloadSnapshot download,
        out NetworkOperationStatus status);

    NetworkOperationResult CancelDownload(
        NetworkDownloadId downloadId);

    event Action<NetworkContextChangedEvent> ContextChanged;
}
```

---

## 9.1 GetCurrentContext

行为：

```text
返回当前只读网络运行时上下文快照
快照只表达当前连接状态、可用性、活动下载数、版本信息
不暴露内部可变集合
```

为什么需要进入 Freeze：

```text
NetworkContext 是 NetworkSystem 的唯一 Runtime 权威状态出口
```

如果不进入 Freeze：

```text
后续实现会把网络状态分散在请求器、下载器和业务模块中
```

---

## 9.2 IsNetworkAvailable

行为：

```text
返回当前网络是否可用的抽象判断
不保证目标业务服务器一定可达
不触发 UI 提示
```

为什么需要进入 Freeze：

```text
需要提供最小网络可用性查询能力
```

如果不进入 Freeze：

```text
FeatureModule 会直接绕过 NetworkSystem 自行检测网络
```

---

## 9.3 SendRequest

行为：

```text
发送一次最小网络请求
通过 NetworkEndpoint 描述目标
通过 NetworkRequestOptions 描述方法、Header、Body、Timeout
返回 NetworkOperationResult
响应以 NetworkResponseSnapshot 输出
预期运行时失败不通过异常表达
```

为什么需要进入 Freeze：

```text
HTTP 请求是 NetworkSystem 第一版最小闭环能力
```

如果不进入 Freeze：

```text
后续会出现多个业务模块各自实现 HTTP 请求入口
```

---

## 9.4 StartDownload

行为：

```text
启动下载任务
成功时返回 NetworkDownloadId
不直接绑定 ResourceSystem 缓存策略
不直接绑定 AssetBundle 分发策略
```

为什么需要进入 Freeze：

```text
下载能力是 ResourceSystem / AssetBundle 未来消费 NetworkSystem 的关键边界
```

如果不进入 Freeze：

```text
资源下载会和 ResourceSystem 或 AssetBundle 实现直接耦合
```

---

## 9.5 TryGetDownload

行为：

```text
查询下载任务快照
成功返回 true
失败返回 false，并输出明确状态
不暴露内部下载任务对象
```

为什么需要进入 Freeze：

```text
需要冻结下载状态查询为快照模型，而不是任务对象泄漏
```

如果不进入 Freeze：

```text
后续实现容易把下载器内部对象暴露给业务层
```

---

## 9.6 CancelDownload

行为：

```text
取消指定下载任务
返回统一结果模型
取消失败不改变其他任务状态
```

为什么需要进入 Freeze：

```text
下载任务需要最小取消能力
```

如果不进入 Freeze：

```text
业务层会绕过服务直接操作底层下载器
```

---

## 9.7 ContextChanged

行为：

```text
只在 NetworkContext 成功提交为新状态后触发
事件只发布 NetworkContextChangedEvent
不直接驱动 UI 或业务流程
```

为什么需要进入 Freeze：

```text
需要冻结网络状态变化事件的发布时机
```

如果不进入 Freeze：

```text
后续实现容易出现网络探测失败也乱发状态事件的问题
```

---

# 十、核心模型冻结

## 10.1 NetworkEndpoint

```csharp
public readonly struct NetworkEndpoint
{
    public string Url { get; }
}
```

规则：

```text
Endpoint 表达稳定网络目标
第一版使用 URL 表达
不绑定业务协议语义
不绑定服务器角色语义
```

---

## 10.2 NetworkRequestId

```csharp
public readonly struct NetworkRequestId
{
    public string Value { get; }
}
```

规则：

```text
RequestId 表达一次请求身份
不等同于业务订单号、账号ID或协议序列号
```

---

## 10.3 NetworkDownloadId

```csharp
public readonly struct NetworkDownloadId
{
    public string Value { get; }
}
```

规则：

```text
DownloadId 表达一次下载任务身份
不等同于资源ID、AssetBundle名或文件路径
```

---

## 10.4 NetworkConnectionStatus

```csharp
public enum NetworkConnectionStatus
{
    Unknown = 0,
    Offline = 1,
    Online = 2,
    Degraded = 3
}
```

规则：

```text
只表达抽象连接状态
不表达业务服务器健康状态
不表达登录状态
```

---

## 10.5 NetworkContext

```csharp
public sealed class NetworkContext
{
    public NetworkConnectionStatus ConnectionStatus { get; }
    public bool IsAvailable { get; }
    public int ActiveDownloadCount { get; }
    public int Version { get; }
}
```

含义：

```text
NetworkContext 是当前运行时唯一权威网络状态快照
只读
不可变
不作为可变容器
```

---

## 10.6 NetworkRequestOptions

```csharp
public sealed class NetworkRequestOptions
{
    public string Method { get; }
    public IReadOnlyDictionary<string, string> Headers { get; }
    public byte[] Body { get; }
    public int TimeoutMilliseconds { get; }
}
```

规则：

```text
第一版只冻结最小请求参数
不冻结业务协议字段
Headers 和 Body 必须按快照处理
```

---

## 10.7 NetworkResponseSnapshot

```csharp
public sealed class NetworkResponseSnapshot
{
    public NetworkRequestId RequestId { get; }
    public int StatusCode { get; }
    public IReadOnlyDictionary<string, string> Headers { get; }
    public byte[] Body { get; }
}
```

规则：

```text
响应是只读快照
不暴露底层 HttpResponseMessage、UnityWebRequest 或 Stream
Body 必须按快照处理
```

---

## 10.8 NetworkDownloadOptions

```csharp
public sealed class NetworkDownloadOptions
{
    public string TargetPath { get; }
    public bool Overwrite { get; }
    public int TimeoutMilliseconds { get; }
}
```

规则：

```text
TargetPath 是下载保存目标
不等同于 ResourceSystem 资源ID
不等同于 AssetBundle 名称
```

---

## 10.9 NetworkDownloadSnapshot

```csharp
public sealed class NetworkDownloadSnapshot
{
    public NetworkDownloadId DownloadId { get; }
    public NetworkEndpoint Endpoint { get; }
    public string TargetPath { get; }
    public long DownloadedBytes { get; }
    public long TotalBytes { get; }
    public NetworkOperationStatus Status { get; }
}
```

规则：

```text
下载状态是只读快照
不暴露内部下载任务对象
```

---

# 十一、Immutable Snapshot 规则冻结

NetworkContext 必须遵守以下 Immutable Snapshot 规则：

```text
NetworkContext 创建后不可修改
事件不得长期持有可变内部对象引用
消费者不得通过返回对象反向修改服务内部状态
每次成功状态提交都生成新版本快照
旧快照在新快照生成后仍保持语义稳定
```

同样规则适用于：

```text
NetworkResponseSnapshot
NetworkDownloadSnapshot
未来进入 Runtime 的 WebSocketConnectionSnapshot
未来进入 Runtime 的 SimulationSyncSnapshot
```

---

# 十二、FrameworkConfig / SaveSystem / Runtime State 边界冻结

## 12.1 FrameworkConfig 归属

FrameworkConfig 只持有：

```text
defaultBaseUrl
defaultTimeoutMilliseconds
enableNetwork
downloadRootPath
networkProfileSelector
```

含义：

```text
FrameworkConfig 只负责启动默认网络配置
不负责用户偏好
不负责当前运行时真实网络状态
```

---

## 12.2 SaveSystem 归属

SaveSystem 可持有：

```text
userNetworkProfilePreference
userDownloadPathPreference
userProxyPreference
```

含义：

```text
这些属于用户偏好或机器偏好持久化
不属于 FrameworkConfig
```

说明：

```text
R10.1 只冻结归属边界
不要求首版 INetworkService 必须暴露这些持久化操作
```

---

## 12.3 Runtime State 归属

Runtime State 归属：

```text
NetworkContext
```

含义：

```text
当前真实网络状态只存在于 NetworkContext
```

---

## 12.4 禁止事项

禁止：

```text
把 current network state 写回 FrameworkConfig
把 NetworkContext 整体直接持久化到 SaveSystem 作为整体对象
让 SaveSystem 持有 FrameworkConfig 启动默认配置主权
把 HttpClient、UnityWebRequest、Socket、Stream 写入 FrameworkConfig
把业务服务器语义写入 NetworkContext
```

---

# 十三、结果模型冻结

## 13.1 NetworkOperationStatus

```csharp
public enum NetworkOperationStatus
{
    Success = 0,
    NetworkUnavailable = 1,
    InvalidEndpoint = 2,
    Timeout = 3,
    Cancelled = 4,
    NotFound = 5,
    AccessDenied = 6,
    ProviderFailure = 7
}
```

---

## 13.2 NetworkOperationResult

```csharp
public sealed class NetworkOperationResult
{
    public bool Success { get; }
    public NetworkOperationStatus Status { get; }
    public string Message { get; }
}
```

规则：

```text
预期运行时失败使用结果模型表达
调用约束错误仍可抛出参数异常
Message 仅用于诊断，不作为业务逻辑判断依据
```

---

# 十四、事件模型冻结

## 14.1 NetworkContextChangedEvent

```csharp
public readonly struct NetworkContextChangedEvent
{
    public NetworkConnectionStatus PreviousStatus { get; }
    public NetworkConnectionStatus CurrentStatus { get; }
    public int PreviousVersion { get; }
    public int CurrentVersion { get; }
}
```

规则：

```text
事件只表达一次已提交的 NetworkContext 变化事实
不持有 NetworkContext 快照
调用方如需当前上下文，应再次调用 GetCurrentContext()
```

---

# 十五、与已关闭模块的依赖关系冻结

## 15.1 与 R1 PlatformServiceRegistry 的关系

接入方向：

```text
INetworkService
```

注册方向：

```csharp
PlatformServiceRegistry.Register<INetworkService>(networkService);
```

---

## 15.2 与 R2 FrameworkConfig 的关系

关系：

```text
NetworkSystem 读取启动默认网络配置
FrameworkConfig 不持有当前运行时网络状态
```

---

## 15.3 与 R3 SaveSystem 的关系

关系：

```text
NetworkSystem 可选持久化用户网络偏好
SaveSystem 不拥有 Runtime State 主权
```

---

## 15.4 与 R4 ResourceSystem 的关系

关系：

```text
ResourceSystem 可在未来消费 NetworkSystem 下载能力
R10.1 不反向依赖 ResourceSystem
```

---

## 15.5 与 R5 AssetBundle 的关系

关系：

```text
AssetBundle 可在未来经 ResourceSystem 间接消费 NetworkSystem
R10.1 不直接依赖 AssetBundle Runtime API
```

---

## 15.6 与 R6 LocalizationSystem 的关系

关系：

```text
NetworkSystem 不直接依赖 LocalizationSystem
诊断消息本地化属于 UI 或工具消费层
```

---

## 15.7 与 R7 InputSystem 的关系

关系：

```text
NetworkSystem 不直接依赖 InputSystem
```

---

## 15.8 与 R8 DisplaySystem 的关系

关系：

```text
NetworkSystem 不直接依赖 DisplaySystem
```

---

## 15.9 与 R9 UISystem 的关系

关系：

```text
NetworkSystem 不直接依赖 UISystem
UISystem 可消费 NetworkContext 展示网络状态
```

---

# 十六、验证方式冻结

R10.1 需要完成以下验证说明：

```text
检查 NetworkSystem 仍保持网络抽象服务定位
检查 INetworkService 方法面只覆盖最小状态查询、请求、下载、取消能力
检查第一版 Freeze 明确区分 Foundation 必需模型与未来扩展模型
检查 NetworkContext Immutable Snapshot 规则明确
检查 FrameworkConfig / SaveSystem / Runtime State 边界明确
检查 NetworkCore 不依赖 UnityEngine / ByFramework
检查 ByFramework Adapter 不把 NetworkCore 细节泄漏到 Runtime API
检查与 R1 ~ R9 的依赖关系保持单向且最小
检查文档未进入 R10.2 Runtime Implementation
```

---

# 十七、禁止事项

R10.1 阶段禁止：

```text
进入 R10.2 Runtime Implementation
创建 NetworkSystem Runtime 代码
冻结完整 TCP Server API
冻结完整 UDP API
冻结完整 WebSocket API
冻结 RPC API
冻结 SimulationSync API
冻结 Database API
冻结 License 联动 API
讨论具体 UnityWebRequest / HttpClient 实现细节
让 NetworkSystem 吞并 FeatureModule 业务协议
让 NetworkSystem 持有 UISystem 状态
让 NetworkCore 依赖 UnityEngine 或 ByFramework
```

---

# 十八、阶段输出物

R10.1 应输出：

```text
Documentation/NetworkSystemRuntimeAPIFreeze.md
```

---

# 十九、关闭条件

R10.1 关闭条件：

```text
候选模块定位明确
服务边界明确
方法面明确
NetworkCore / Adapter 边界明确
Foundation 必需模型明确
未来扩展模型边界明确
Immutable Snapshot 规则明确
FrameworkConfig / SaveSystem / Runtime State 边界明确
与 R1 ~ R9 的依赖关系明确
Freeze 规模受控，未过度设计
```

满足以上条件后，R10.1 可关闭。

下一阶段仍为：

```text
R10.2 NetworkSystem Runtime Implementation
```

但本次不进入。

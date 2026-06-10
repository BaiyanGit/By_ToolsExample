# P3.14 NetworkSystem Foundation 设计冻结

> 阶段性质：设计冻结阶段  
> 不是 Runtime 实现阶段  
> 不创建 NetworkSystem Runtime 脚本  
> 不实现 TCP / UDP / WebSocket / HTTP / Server / Downloader / Protocol / Protobuf 代码  

---

# 一、阶段目标

P3.14 的目标是冻结 NetworkSystem Foundation 的基础设计，为后续 NetworkCore、Socket 通信、HTTP、LargeFileDownloader、Client / Server、Protocol、Serialization、Connectivity 和 ByFramework Adapter 提供明确边界。

本阶段只允许完成：

```text
NetworkCore 独立性冻结
NetworkSystem Adapter 边界冻结
Socket 支持范围冻结
Client / Server 方向冻结
HTTP 能力范围冻结
LargeFileDownloader 能力冻结
Serialization 方向冻结
Protobuf 工具链复用原则冻结
统一消息头方向冻结
心跳 / 重连 / 超时方向冻结
网络状态检测方向冻结
NetworkSystem 不做仿真同步的边界冻结
Server 用途冻结
NetworkSystem 与 FrameworkConfig / SaveSystem / ResourceSystem / FeatureModule 的关系冻结
```

本阶段不进入具体运行时代码实现。

---

# 二、架构位置

NetworkSystem 属于：

```text
Platform/NetworkSystem
```

但 NetworkSystem 内部必须拆分为：

```text
NetworkCore
NetworkSystem Adapter
```

其中：

```text
NetworkCore 是纯 C# 网络核心库
NetworkSystem Adapter 是 ByFramework 适配层
```

---

# 三、NetworkCore 独立性原则

选择：

```text
A：必须坚持
```

NetworkCore 必须保持：

```text
纯 C#
低依赖
可独立抽离
可独立测试
可跨项目复用
```

NetworkCore 应能被以下项目使用：

```text
其它 Unity 项目
WPF 项目
WinForm 项目
Console 工具
ASP.NET 工具
独立服务器程序
设备调试工具
局域网服务程序
```

---

## 3.1 NetworkCore 允许依赖

NetworkCore 允许依赖：

```text
System
System.IO
System.Net
System.Net.Sockets
System.Threading
System.Threading.Tasks
System.Collections.Generic
基础序列化接口
基础日志接口
```

基础日志接口必须是抽象接口，不能直接依赖 ByFramework 的 Log 或 Unity 的 Debug.Log。

---

## 3.2 NetworkCore 禁止依赖

NetworkCore 禁止依赖：

```text
FrameworkEntry
PlatformServiceRegistry
FrameworkConfig
ThreadDispatcher
EventManager
FSMManager
ResourceSystem
SaveSystem
UISystem
UnityEngine
MonoBehaviour
ScriptableObject
GameObject
Scene
```

禁止在 NetworkCore 中出现：

```text
PlatformServiceRegistry.Get
EventManager.Publish
Debug.Log
MonoBehaviour.Start
MonoBehaviour.Update
ScriptableObject 配置
Unity 协程
```

---

## 3.3 正确依赖关系

正确关系：

```text
NetworkCore
        ↑
NetworkSystem Adapter
        ↑
ByFramework Platform
        ↑
FeatureModule
```

也就是：

```text
ByFramework 依赖 NetworkCore

NetworkCore 不依赖 ByFramework
```

禁止关系：

```text
NetworkCore
        ↓
ByFramework Core / Platform
```

---

# 四、NetworkSystem Adapter

NetworkSystem Adapter 位于：

```text
Platform/NetworkSystem
```

职责：

```text
读取 FrameworkConfig 中的网络配置
注册 INetworkService 到 PlatformServiceRegistry
把 NetworkCore 的回调派发到 ThreadDispatcher
把连接事件转换为 EventManager 事件
统一参与 Platform 生命周期
向 FeatureModule 暴露框架侧网络服务接口
```

NetworkSystem Adapter 可以依赖 ByFramework。

NetworkCore 不可以依赖 ByFramework。

---

# 五、Socket 支持范围

Socket 支持范围选择：

```text
D：TCP / UDP / WebSocket 都支持
```

---

## 5.1 TCP

TCP 适用于：

```text
可靠消息通信
客户端 / 服务端连接
控制指令
业务请求响应
设备网关
局域网管理系统
```

---

## 5.2 UDP

UDP 适用于：

```text
低延迟状态广播
局域网发现
实时状态采样
非关键数据传输
```

UDP 不保证可靠性。

可靠性如有需要，应由上层协议自行设计。

---

## 5.3 WebSocket

WebSocket 适用于：

```text
Web 管理端
浏览器调试工具
局域网 Web 控制台
跨平台通信
```

---

# 六、Client / Server

Client / Server 选择：

```text
B：Client + Server 都支持
```

NetworkSystem 不应只考虑客户端。

必须同时考虑：

```text
NetworkClient
NetworkServer
ClientSession
ServerSession
ConnectionManager
ClientConnection
```

---

## 6.1 Client 用途

Client 用于：

```text
连接服务器
连接 SimulationServer
连接设备网关
连接局域网控制端
连接 HTTP 服务
```

---

## 6.2 Server 用途

Server 用途选择：

```text
D：本地联调 / SimulationServer 基础网络层 / 设备网关基础都支持，但保持行业无关
```

Server 可用于：

```text
本地联调
功能测试
局域网测试
SimulationServer 基础网络层
设备网关基础
多人协同基础
数据库服务桥接
```

注意：

```text
Server 可以提供基础网络服务
但不承载具体仿真同步业务
```

---

## 6.3 数据库连接说明

如果项目没有专业服务支持，NetworkSystem Server 可以作为局域网服务基础。

未来可由上层业务或独立服务模块连接数据库。

但 NetworkCore / NetworkSystem 不直接负责数据库业务。

数据库访问应属于：

```text
FeatureModule
独立 Server 应用层
业务服务层
```

---

# 七、HTTP 支持

HTTP 支持范围选择：

```text
B：GET / POST / PUT / DELETE / Upload / Download
```

---

## 7.1 HTTP 能力

HTTP 子系统应支持方向：

```text
GET
POST
PUT
DELETE
Upload
Download
Header
Cookie
Token
Timeout
Retry
Progress
```

---

## 7.2 HTTP 归属

HTTP 应优先位于：

```text
NetworkCore/Http
```

ByFramework 侧通过 NetworkSystem Adapter 使用。

---

## 7.3 HTTP 不负责

HTTP 不理解：

```text
业务接口含义
登录业务
训练业务
资源业务
数据库业务
```

HTTP 只负责请求与响应。

---

# 八、LargeFileDownloader

LargeFileDownloader 选择：

```text
A：必须支持，多线程 / 断点续传 / Hash / 暂停恢复
```

项目中已有下载器功能，可作为后续实现参考。

但需要在后续实现阶段进行：

```text
整理
抽象
优化
低耦合改造
NetworkCore 化
```

---

## 8.1 Downloader 能力

LargeFileDownloader 应支持方向：

```text
大文件下载
多线程分片下载
断点续传
暂停
恢复
取消
失败重试
速度统计
进度回调
Hash 校验
临时文件管理
下载完成合并
下载缓存
```

---

## 8.2 Downloader 服务对象

Downloader 服务对象：

```text
AssetBundle 远程包
补丁包
资源包
地图包
训练素材包
配置包
离线数据包
```

---

## 8.3 Downloader 边界

Downloader 只负责下载文件。

不理解：

```text
AssetBundle 业务
资源版本业务
补丁业务
训练素材业务
```

正确关系：

```text
ResourceSystem / AssetBundleProvider
↓
请求下载文件
↓
NetworkSystem.Http.LargeFileDownloader
↓
下载到本地缓存
↓
ResourceSystem 加载
```

---

# 九、Serialization

序列化选择：

```text
D：多格式支持
```

支持：

```text
Protobuf
JSON
Binary
```

---

## 9.1 Protobuf

Protobuf 适用于：

```text
高性能消息
跨语言协议
Client / Server 通信
SimulationServer 基础消息
设备网关消息
```

---

## 9.2 JSON

JSON 适用于：

```text
调试消息
管理接口
HTTP 请求
简单配置传输
WebSocket 调试
```

---

## 9.3 Binary

Binary 适用于：

```text
自定义高性能协议
内部消息
特殊设备协议
```

---

# 十、Protobuf 工具链

Protobuf 工具链选择：

```text
A：直接复用，不重新设计
```

项目已存在：

```text
Protobuf 包
pb 转 C# 工具
```

NetworkSystem 不重新设计工具链。

后续只定义：

```text
如何接入已有 Protobuf
如何注册 Protobuf Serializer
如何在 Protocol 中使用 Protobuf
```

---

# 十一、统一消息头

协议层需要统一消息头。

选择：

```text
A：需要
```

---

## 11.1 消息头字段方向

建议消息头包含：

```text
Magic
Version
HeaderLength
BodyLength
MessageId
Opcode
RequestId
SessionId
Timestamp
ErrorCode
CompressType
SerializeType
Checksum
```

本阶段只冻结方向，不冻结具体二进制结构。

---

## 11.2 消息头用途

用于：

```text
识别协议
区分消息类型
请求响应匹配
版本兼容
错误处理
序列化选择
压缩选择
完整性校验
```

---

# 十二、Protocol

Protocol 负责消息协议。

包括：

```text
编码
解码
粘包拆包
消息头解析
消息体解析
请求响应
错误码
协议版本
```

Protocol 不理解业务含义。

业务含义由 FeatureModule 或上层应用解释。

---

# 十三、心跳 / 重连 / 超时

心跳 / 重连 / 超时选择：

```text
A：需要
```

---

## 13.1 心跳

心跳用于：

```text
维持连接
检测断线
统计延迟
判断连接质量
```

---

## 13.2 重连

重连用于：

```text
客户端断线恢复
局域网不稳定恢复
服务器重启后恢复
```

重连策略应可配置。

例如：

```text
重连次数
重连间隔
最大重连时间
是否自动重连
```

---

## 13.3 超时

超时用于：

```text
连接超时
请求超时
心跳超时
下载超时
上传超时
```

---

# 十四、网络状态检测

网络状态检测选择：

```text
A：需要
```

---

## 14.1 检测内容

应支持方向：

```text
Ping
延迟
断线
丢包
连接状态
重连状态
下载速度
上传速度
```

---

## 14.2 Connectivity

Connectivity 负责网络状态。

可输出：

```text
Connected
Disconnected
Connecting
Reconnecting
Timeout
Failed
```

本阶段不实现代码。

---

# 十五、NetworkSystem 与仿真同步边界

NetworkSystem 是否允许做仿真同步选择：

```text
A：不允许
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

NetworkSystem 不负责：

```text
车辆同步
机械臂同步
关节同步
碰撞同步
物理同步
场景同步
训练同步
业务状态同步
```

这些属于：

```text
FeatureModule/SimulationSync
```

---

## 15.1 示例允许原则

虽然 NetworkSystem 不做仿真同步，但可以提供示例。

允许提供：

```text
Socket Client 示例
Socket Server 示例
Protobuf 消息示例
请求响应示例
简单广播示例
SimulationSync 参考示例
```

示例必须放在：

```text
Samples
Examples
Documentation
```

不得把示例业务逻辑写入 NetworkCore 或 NetworkSystem 主代码。

---

# 十六、NetworkSystem 与 SimulationSync 的关系

正确关系：

```text
SimulationSync
↓
生成业务同步消息
↓
NetworkSystem
↓
NetworkCore
↓
发送消息
```

NetworkSystem 不理解：

```text
这是车辆
这是机械臂
这是场景对象
这是碰撞
```

NetworkSystem 只负责传输。

---

# 十七、NetworkSystem 与 FrameworkConfig 的关系

FrameworkConfig 提供 NetworkConfig。

NetworkSystem Adapter 消费 NetworkConfig。

配置内容方向：

```text
服务器 IP
端口
协议类型
是否启用 Server
是否启用 Client
心跳间隔
重连次数
超时时间
HTTP 基础地址
下载缓存路径
是否启用 Downloader
```

NetworkCore 不直接读取 FrameworkConfig。

---

# 十八、NetworkSystem 与 SaveSystem 的关系

SaveSystem 可保存：

```text
服务器地址历史
网络配置
下载任务缓存
断点续传记录
```

NetworkSystem 不直接负责保存细节。

关系：

```text
NetworkSystem Adapter
↓
SaveSystem
```

NetworkCore 不直接依赖 SaveSystem。

---

# 十九、NetworkSystem 与 ResourceSystem 的关系

ResourceSystem 可使用 NetworkSystem 的下载能力。

例如：

```text
下载 AssetBundle
下载补丁包
下载配置包
```

正确关系：

```text
ResourceSystem
↓
NetworkSystem Adapter
↓
NetworkCore Downloader
```

NetworkCore 不直接依赖 ResourceSystem。

---

# 二十、NetworkSystem 与 LicenseSystem 的关系

LicenseSystem 可控制：

```text
联网功能
Server 功能
远程更新功能
SimulationServer 功能
```

NetworkSystem 不负责授权规则。

NetworkSystem 只接收是否允许启用某项网络能力的结果。

---

# 二十一、NetworkSystem 与 FeatureModule 的关系

FeatureModule 可使用 NetworkSystem。

例如：

```text
SimulationSync 使用 Socket 通信
DeviceIntegration 使用 TCP / UDP / WebSocket
TrainingSystem 上传训练记录
ScenarioSystem 请求场景数据
```

但 FeatureModule 不允许把业务逻辑塞入 NetworkSystem。

---

# 二十二、Server 与业务服务边界

NetworkSystem Server 是基础通信 Server。

它可以支持：

```text
监听连接
管理客户端
收发消息
请求响应
心跳
断线检测
协议解析
```

它不直接负责：

```text
用户系统
数据库业务
训练业务
车辆业务
设备业务
权限业务
```

如果需要连接数据库，应由：

```text
独立业务服务层
FeatureModule Server 侧模块
SimulationServerFeature
```

来实现。

---

# 二十三、建议核心对象

后续实现可围绕以下对象设计。

本阶段只冻结概念，不实现代码。

```text
NetworkCore
NetworkClient
NetworkServer
NetworkSession
ClientSession
ServerSession
ConnectionManager
NetworkMessage
NetworkHeader
NetworkPacket
NetworkProtocol
NetworkSerializer
ProtobufSerializer
JsonSerializer
BinarySerializer
NetworkTransport
TcpTransport
UdpTransport
WebSocketTransport
HttpClient
HttpRequest
HttpResponse
LargeFileDownloader
DownloadTask
ConnectivityMonitor
HeartbeatPolicy
ReconnectPolicy
TimeoutPolicy
NetworkConfig
NetworkResult
NetworkError
```

---

# 二十四、建议目录结构

本阶段仅冻结目录方向，不要求创建代码文件。

```text
Assets/ByFramework/Platform/NetworkSystem
├─ NetworkCore
│  ├─ Client
│  │  └─ NetworkClient
│  │
│  ├─ Server
│  │  ├─ NetworkServer
│  │  ├─ ConnectionManager
│  │  └─ ClientConnection
│  │
│  ├─ Session
│  │  ├─ NetworkSession
│  │  ├─ ClientSession
│  │  └─ ServerSession
│  │
│  ├─ Transport
│  │  ├─ TcpTransport
│  │  ├─ UdpTransport
│  │  └─ WebSocketTransport
│  │
│  ├─ Protocol
│  │  ├─ NetworkHeader
│  │  ├─ NetworkPacket
│  │  └─ NetworkProtocol
│  │
│  ├─ Serialization
│  │  ├─ NetworkSerializer
│  │  ├─ ProtobufSerializer
│  │  ├─ JsonSerializer
│  │  └─ BinarySerializer
│  │
│  ├─ Http
│  │  ├─ HttpClient
│  │  ├─ HttpRequest
│  │  ├─ HttpResponse
│  │  └─ Downloader
│  │     ├─ LargeFileDownloader
│  │     └─ DownloadTask
│  │
│  └─ Connectivity
│     ├─ ConnectivityMonitor
│     ├─ HeartbeatPolicy
│     ├─ ReconnectPolicy
│     └─ TimeoutPolicy
│
├─ Adapter
│  ├─ NetworkService
│  ├─ NetworkConfigAdapter
│  ├─ NetworkEventBridge
│  └─ NetworkThreadDispatcher
│
├─ Runtime
│  └─ Service
│     └─ INetworkService
│
└─ Samples
   ├─ TcpClientServerSample
   ├─ WebSocketSample
   ├─ HttpDownloadSample
   └─ SimulationSyncReferenceSample
```

---

# 二十五、配置文件方向

建议配置文件：

```text
StreamingAssets/ByFramework/Config/network_config.json
PersistentDataPath/ByFramework/Config/network_config.json
```

字段方向：

```text
enableClient
enableServer
defaultProtocol
serverIp
serverPort
tcpPort
udpPort
webSocketUrl
httpBaseUrl
heartbeatInterval
reconnectCount
reconnectInterval
connectTimeout
requestTimeout
enableDownloader
downloadThreadCount
downloadCachePath
enableHashCheck
enableNetworkStateMonitor
```

本阶段不冻结具体 JSON Schema。

---

# 二十六、PlatformServiceRegistry 接入方向

NetworkSystem Adapter 后续可注册为 Platform 服务。

接口方向：

```text
INetworkService
```

注册方向：

```text
PlatformServiceRegistry.Register<INetworkService>(networkService)
```

获取方向：

```text
PlatformServiceRegistry.Get<INetworkService>()
PlatformServiceRegistry.TryGet<INetworkService>(out networkService)
```

注意：

```text
NetworkCore 不注册 PlatformServiceRegistry
NetworkSystem Adapter 才注册 PlatformServiceRegistry
```

---

# 二十七、Samples / Examples 规则

NetworkSystem 可以提供示例。

示例用于：

```text
帮助项目接入
提供联调参考
提供 SimulationSync 写法参考
```

示例不得污染主框架。

示例位置：

```text
Samples
Examples
Documentation
```

示例可以包含：

```text
简单 TCP Echo Server
简单 TCP Client
UDP 广播示例
WebSocket 示例
HTTP 下载示例
Protobuf 消息示例
SimulationSync 参考示例
```

SimulationSync 示例只能作为参考，不得成为 NetworkSystem 正式职责。

---

# 二十八、禁止事项

P3.14 阶段禁止：

```text
实现 TCP / UDP / WebSocket 代码
实现 Client / Server 代码
实现 HTTP 请求代码
实现 LargeFileDownloader 代码
实现 Protobuf 序列化代码
实现协议头编码解码
实现心跳 / 重连 / 超时
实现网络状态检测
实现数据库连接代码
实现 SimulationSync 业务逻辑
让 NetworkCore 依赖 ByFramework
让 NetworkCore 依赖 UnityEngine
修改 FrameworkEntry 生命周期
扩展 Core/Config/FrameworkConfig.cs
进入 NetworkSystem Runtime Implementation
进入 ResourceSystem Runtime Implementation
进入 FeatureModule Implementation
```

---

# 二十九、P3.14 输出物

P3.14 应输出：

```text
Documentation/NetworkSystemFoundation.md
Documentation/ByFramework_Current_Context.md 更新
Roadmap.md 更新
Todo.md 更新
Documentation/Changelog.md 更新
```

不要求输出 Runtime 代码。

---

# 三十、阶段关闭条件

P3.14 关闭条件：

```text
NetworkCore 独立性明确
NetworkSystem Adapter 边界明确
TCP / UDP / WebSocket 支持范围明确
Client / Server 都支持的方向明确
HTTP 能力范围明确
LargeFileDownloader 能力明确
现有下载器后续可参考但需抽象优化的原则明确
多序列化支持明确
Protobuf 工具链直接复用原则明确
统一消息头方向明确
心跳 / 重连 / 超时方向明确
网络状态检测方向明确
NetworkSystem 不做仿真同步边界明确
Server 用途明确且保持行业无关
示例允许但不得污染主框架规则明确
NetworkSystem 与 FrameworkConfig / SaveSystem / ResourceSystem / LicenseSystem / FeatureModule 的关系明确
禁止事项写入文档
Current_Context 更新完成
Roadmap / Todo / Changelog 同步完成
```

满足以上条件后，P3.14 可关闭。

下一阶段建议进入：

```text
P3.15 BuildProfileSystem Foundation
```

---

# 三十一、最终结论

P3.14 NetworkSystem Foundation 是设计冻结阶段。

它只确定：

```text
NetworkSystem 是什么
NetworkCore 是什么
NetworkSystem Adapter 是什么
Socket 支持什么
Client / Server 如何定位
HTTP 与 Downloader 如何归属
Protocol 如何定位
Serialization 如何扩展
Server 能做什么
Server 不能做什么
为什么不能做仿真同步
后续 Runtime 实现应遵守什么边界
```

不得在本阶段进入具体 Runtime、Server、Downloader 或 SimulationSync 实现。

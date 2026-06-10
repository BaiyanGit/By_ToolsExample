# P3.20 DeviceIntegration Foundation 设计冻结

> 阶段性质：设计冻结阶段  
> 不是 Runtime 实现阶段  
> 不创建 DeviceIntegration Runtime 脚本  
> 不实现串口、PLC、CAN、Modbus、DOF、模拟设备、设备录制回放、状态监控或 RuntimeConfigUI 设备配置代码  

---

# 一、阶段目标

P3.20 的目标是冻结 DeviceIntegration Foundation 的基础设计，为后续设备接入、工业协议、驾驶模拟硬件、运动平台、设备模拟器、设备数据监控、设备录制回放和现场设备配置提供明确边界。

本阶段只允许完成：

```text
DeviceIntegration 架构归属冻结
Platform / FeatureModule 分层冻结
SerialPort 归属冻结
PLC 支持方式冻结
CAN 支持规则冻结
Modbus RTU / TCP 支持规则冻结
DOF 运动平台归属冻结
真实设备 / 模拟设备规则冻结
设备录制与回放规则冻结
设备状态监控范围冻结
视频监控 / 推流工具边界冻结
RuntimeConfigUI 设备配置接入规则冻结
DeviceIntegration 与 NetworkSystem / InputSystem / SaveSystem / FrameworkConfig / RuntimeConfigUI 的关系冻结
```

本阶段不进入具体设备接入实现。

---

# 二、架构总定位

DeviceIntegration 不是单一模块，而是一套分层规则。

最终选择：

```text
C：Platform + FeatureModule 分层
```

即：

```text
Platform 提供通用设备基础能力

FeatureModule 实现具体设备业务
```

---

# 三、为什么必须分层

设备接入中存在两类能力：

```text
通用通信能力
具体业务设备能力
```

通用通信能力例如：

```text
SerialPort
CAN
Modbus
PLC 协议基础
TCP 设备通信基础
设备状态基础模型
设备配置模型
```

具体业务设备能力例如：

```text
某型号方向盘
某型号踏板
某客户 PLC 点位逻辑
某运动平台控制算法
某驾驶舱硬件逻辑
某训练设备协议解释
```

这两类不能混在一起。

---

# 四、Platform 层职责

Platform 层可以提供：

```text
通用设备通信基础
通用设备协议基础
通用设备状态模型
通用设备配置模型
通用设备模拟器基础
通用设备录制回放基础
通用设备数据监控基础
RuntimeConfigUI 设备配置入口
```

Platform 不理解具体设备业务。

---

# 五、FeatureModule 层职责

FeatureModule 负责具体设备业务。

例如：

```text
方向盘设备
踏板设备
档位设备
仪表设备
驾驶舱设备
PLC 项目逻辑
MCU 接入逻辑
运动平台业务逻辑
DOF 洗出算法
客户定制设备
```

这些属于：

```text
FeatureModule/DeviceIntegration
```

---

# 六、SerialPort 串口归属

串口归属选择：

```text
A：Platform
```

---

## 6.1 含义

SerialPort 是基础通信能力，类似：

```text
Network
File
Save
Config
```

不应该放到具体业务模块中。

---

## 6.2 Platform 可提供

Platform 可提供：

```text
SerialPort 打开
SerialPort 关闭
SerialPort 读写
SerialPort 配置
SerialPort 状态
SerialPort 错误
SerialPort 数据事件
```

---

## 6.3 FeatureModule 负责

FeatureModule 负责解释串口数据含义。

例如：

```text
这是方向盘角度
这是踏板值
这是 MCU 状态
这是设备报警码
```

Platform 不理解这些业务含义。

---

# 七、PLC 支持方式

PLC 支持方式选择：

```text
C：协议层 Platform，设备层 FeatureModule
```

---

## 7.1 Platform 可提供

Platform 可提供 PLC 基础协议能力：

```text
Modbus TCP
Modbus RTU
S7 协议预留
MC 协议预留
PLC 连接配置
PLC 读写基础接口
PLC 状态模型
```

---

## 7.2 FeatureModule 负责

FeatureModule 负责具体 PLC 点位和业务逻辑：

```text
某客户设备点位表
某工厂 PLC 地址映射
某产线动作逻辑
某训练设备控制逻辑
```

---

# 八、CAN 总线支持

CAN 总线选择：

```text
A：支持
```

---

## 8.1 含义

CAN 对驾驶模拟、车辆仿真、设备联动有较高价值。

Platform 应预留或提供：

```text
CAN 设备配置
CAN 通道配置
CAN 帧模型
CAN 数据收发
CAN 状态监控
```

---

## 8.2 边界

Platform 不解释具体 CAN 帧业务。

例如：

```text
车速
转速
档位
仪表灯
报警
```

这些属于 FeatureModule。

---

# 九、Modbus 支持

Modbus 支持选择：

```text
C：RTU + TCP
```

---

## 9.1 Modbus RTU

Modbus RTU 依赖串口。

关系：

```text
Modbus RTU
↓
SerialPort
```

---

## 9.2 Modbus TCP

Modbus TCP 依赖网络通信。

关系：

```text
Modbus TCP
↓
NetworkSystem / NetworkCore
```

---

## 9.3 边界

Platform 提供 Modbus 基础读写能力。

FeatureModule 负责：

```text
点位含义
设备动作
业务流程
报警解释
```

---

# 十、DOF 运动平台归属

DOF 归属选择：

```text
C：平台接口 + Feature 实现
```

---

## 10.1 Platform 可提供

Platform 可提供：

```text
IDofDevice
IDofController
DOF 连接状态
DOF 输入输出数据结构
DOF 配置模型
DOF 安全边界预留
```

---

## 10.2 FeatureModule 负责

FeatureModule 负责：

```text
3DOF / 6DOF 具体设备接入
运动平台协议
运动算法
洗出算法
驾驶模拟运动反馈
客户设备适配
```

---

## 10.3 当前规则

DOF 当前不进入 Platform 主实现。

但可以在 Platform 中预留接口方向。

具体实现放在：

```text
FeatureModule/DeviceIntegration/DOF
```

---

# 十一、真实设备与模拟设备

真实设备与模拟设备选择：

```text
A：支持
```

---

## 11.1 含义

当现场没有真实设备时，可以用模拟设备代替。

例如：

```text
模拟方向盘
模拟踏板
模拟 PLC
模拟 CAN 数据
模拟运动平台
模拟串口设备
```

---

## 11.2 价值

支持模拟设备可以提升：

```text
开发效率
联调效率
自动化测试能力
客户演示能力
故障复现能力
```

---

## 11.3 设计方向

建议支持：

```text
RealDevice
MockDevice
SimulatedDevice
PlaybackDevice
```

并通过配置切换：

```text
真实模式
模拟模式
回放模式
```

---

# 十二、设备录制与回放

设备录制与回放选择：

```text
A：支持
```

---

## 12.1 含义

记录设备输入、输出、状态变化，然后回放。

适用于：

```text
培训
测试
故障复现
设备联调
自动化验证
演示
```

---

## 12.2 录制内容

设备录制应主要面向数据。

可录制：

```text
设备输入
设备输出
设备状态
时间戳
错误码
连接状态
协议帧
解析后的数据值
```

---

## 12.3 回放模式

回放模式可用于：

```text
无设备开发
故障复现
训练回放
设备模拟
自动化测试
```

---

# 十三、FFMPEG Recorder / 推流工具边界

用户已有：

```text
FFMPEG Recorder 工具
推流工具
```

这些属于重要工具资产。

但它们不应直接塞入 DeviceIntegration 主逻辑。

---

## 13.1 推荐归属

推荐归属为：

```text
MediaSystem
RecorderSystem
StreamingSystem
ByTools/MediaTools
```

或者作为独立工具资产：

```text
ByTools/FFMPEGRecorder
ByTools/StreamingTool
```

---

## 13.2 与 DeviceIntegration 的关系

DeviceIntegration 可关联这些工具，但不直接拥有它们。

例如：

```text
DeviceIntegration
负责设备数据录制

FFMPEG Recorder
负责视频录制 / 推流
```

未来可以通过统一时间戳对齐：

```text
设备数据记录
+
视频记录
+
训练记录
```

但模块边界必须分开。

---

# 十四、设备状态监控

设备状态监控选择：

```text
A：支持
```

此处明确：

```text
DeviceIntegration 的状态监控主要指数据状态监控
```

不是视频监控。

---

## 14.1 数据状态监控

必须支持：

```text
在线
离线
连接中
重连中
超时
错误
数据频率
延迟
最后接收时间
最后发送时间
错误码
连接端口
设备名称
设备类型
```

---

## 14.2 视频监控边界

视频监控、视频推流、画面录制属于：

```text
Media / Recorder / Streaming
```

不属于 DeviceIntegration 主职责。

用户已有推流工具，可作为独立工具或未来 MediaSystem 资产。

---

# 十五、RuntimeConfigUI 接入设备

RuntimeConfigUI 是否接入设备选择：

```text
A：接入
```

---

## 15.1 可配置内容

现场人员可以配置：

```text
串口号
波特率
数据位
停止位
校验位
PLC IP
PLC 端口
Modbus 站号
CAN 通道
CAN 波特率
设备启用状态
设备模式
真实 / 模拟 / 回放模式
设备配置文件路径
```

---

## 15.2 可查看内容

RuntimeConfigUI 应能查看：

```text
设备在线状态
设备错误状态
数据频率
最后通信时间
重连次数
设备日志
当前模式
```

---

## 15.3 危险操作

以下操作应要求权限和确认：

```text
切换真实 / 模拟模式
重连设备
重置设备配置
清空设备录制数据
导入设备配置
覆盖设备配置
```

---

# 十六、DeviceIntegration 与 InputSystem 的关系

InputSystem 负责输入抽象。

DeviceIntegration 负责设备接入。

关系：

```text
DeviceIntegration
↓
采集设备数据
↓
InputSystem
↓
转换为输入信号
↓
FeatureModule
```

示例：

```text
方向盘设备
↓
DeviceIntegration
↓
InputSystem
↓
VehicleSimulation
```

---

# 十七、DeviceIntegration 与 NetworkSystem 的关系

NetworkSystem 提供 TCP / UDP / WebSocket / HTTP 基础通信。

DeviceIntegration 可以使用 NetworkSystem 接入网络设备。

例如：

```text
TCP 设备
UDP 广播设备
WebSocket 设备
PLC TCP
Modbus TCP
```

但 NetworkSystem 不理解设备业务。

---

# 十八、DeviceIntegration 与 SaveSystem 的关系

SaveSystem 可保存：

```text
设备配置
设备校准数据
设备录制索引
设备状态快照
```

SaveSystem 不理解设备业务。

---

# 十九、DeviceIntegration 与 FrameworkConfig 的关系

FrameworkConfig 提供设备配置来源。

例如：

```text
device_config.json
serial_config.json
plc_config.json
can_config.json
modbus_config.json
dof_config.json
```

FrameworkConfig 负责加载和合并配置。

DeviceIntegration 消费最终配置。

---

# 二十、DeviceIntegration 与 BuildProfileSystem 的关系

BuildProfileSystem 可决定：

```text
是否包含设备模块
是否包含 CAN 支持
是否包含 PLC 支持
是否包含 DOF 支持
是否包含模拟设备
是否包含设备录制回放
```

---

# 二十一、DeviceIntegration 与 LicenseSystem 的关系

LicenseSystem 可控制：

```text
PLC 功能是否授权
CAN 功能是否授权
DOF 功能是否授权
设备录制回放是否授权
高级设备模块是否授权
```

DeviceIntegration 不负责授权规则。

---

# 二十二、DeviceIntegration 与 RuntimeConfigUI 的关系

RuntimeConfigUI 提供现场设备配置和状态查看入口。

关系：

```text
RuntimeConfigUI
↓
显示 / 修改设备配置
↓
FrameworkConfig / SaveSystem
↓
DeviceIntegration 应用配置
```

---

# 二十三、建议核心对象

后续实现可围绕以下对象设计。

本阶段只冻结概念，不实现代码。

```text
IDevice
IDeviceConnection
IDeviceProtocol
IDeviceDriver
IDeviceSimulator
IDeviceRecorder
IDevicePlayback
DeviceConfig
DeviceProfile
DeviceStatus
DeviceState
DeviceError
DeviceDataFrame
DeviceMonitor
DeviceRuntimeMode

SerialPortDevice
CanDevice
ModbusRtuDevice
ModbusTcpDevice
PlcDevice
DofDevice
MockDevice
PlaybackDevice
```

---

# 二十四、建议目录结构

本阶段仅冻结目录方向，不要求创建代码文件。

```text
Assets/ByFramework/Platform/DeviceIntegration
├─ Runtime
│  ├─ Core
│  │  ├─ IDevice
│  │  ├─ DeviceConfig
│  │  ├─ DeviceStatus
│  │  ├─ DeviceState
│  │  └─ DeviceDataFrame
│  │
│  ├─ SerialPort
│  │  └─ SerialPortDevice
│  │
│  ├─ CAN
│  │  └─ CanDevice
│  │
│  ├─ Modbus
│  │  ├─ ModbusRtuDevice
│  │  └─ ModbusTcpDevice
│  │
│  ├─ PLC
│  │  └─ PlcProtocolBase
│  │
│  ├─ Simulation
│  │  ├─ MockDevice
│  │  └─ PlaybackDevice
│  │
│  ├─ Recorder
│  │  ├─ DeviceRecorder
│  │  └─ DevicePlayback
│  │
│  ├─ Monitor
│  │  └─ DeviceMonitor
│  │
│  └─ Service
│     └─ IDeviceIntegrationService
```

FeatureModule 具体设备建议：

```text
Assets/ByFramework/FeatureModule/DeviceIntegration
├─ SteeringWheel
├─ Pedal
├─ Gear
├─ Instrument
├─ MotionPlatform
├─ DOF
├─ CustomerDevices
└─ Samples
```

FFMPEG / 推流工具建议保留独立：

```text
Assets/ByTools/FFMPEGRecorder
Assets/ByTools/StreamingTool
```

或未来：

```text
Assets/ByFramework/Platform/MediaSystem
```

---

# 二十五、配置文件方向

建议配置文件：

```text
StreamingAssets/ByFramework/Config/device_config.json
PersistentDataPath/ByFramework/Config/device_config.json
```

可拆分：

```text
serial_config.json
plc_config.json
can_config.json
modbus_config.json
dof_config.json
```

字段方向：

```text
deviceId
deviceName
deviceType
enabled
runtimeMode
connectionType
serialPort
baudRate
ip
port
timeout
retryCount
protocol
profile
recordEnabled
playbackFile
```

本阶段不冻结具体 JSON Schema。

---

# 二十六、状态监控字段方向

DeviceStatus 可包含：

```text
DeviceId
DeviceName
DeviceType
State
IsOnline
LastReceiveTime
LastSendTime
DataRate
LatencyMs
ErrorCode
ErrorMessage
ReconnectCount
RuntimeMode
```

状态枚举方向：

```text
None
Disabled
Connecting
Online
Offline
Timeout
Error
Reconnecting
Playback
Simulated
```

---

# 二十七、平台服务注册方向

DeviceIntegration 后续如进入 Platform 实现，可注册：

```text
IDeviceIntegrationService
```

注册方向：

```text
PlatformServiceRegistry.Register<IDeviceIntegrationService>(deviceService)
```

但当前阶段仅冻结方向，不实现代码。

---

# 二十八、禁止事项

P3.20 阶段禁止：

```text
实现串口代码
实现 PLC 代码
实现 CAN 代码
实现 Modbus 代码
实现 DOF 代码
实现设备模拟器
实现设备录制回放
实现状态监控 UI
实现 RuntimeConfigUI 设备页面
实现 FFMPEG Recorder
实现推流工具
把视频监控塞进 DeviceIntegration 主逻辑
把具体业务设备写进 Platform 主逻辑
让 Platform 理解具体客户设备业务
进入 Runtime Implementation
```

---

# 二十九、P3.20 输出物

P3.20 应输出：

```text
Documentation/DeviceIntegrationFoundation.md
Documentation/00_ByFramework_Current_Context.md 更新
Documentation/02_Roadmap.md 更新
Documentation/03_Todo.md 更新
Documentation/04_Changelog.md 更新
```

不要求输出 Runtime 或 Editor 代码。

---

# 三十、阶段关闭条件

P3.20 关闭条件：

```text
DeviceIntegration Platform / FeatureModule 分层明确
SerialPort 属于 Platform 明确
PLC 协议层 Platform、设备层 FeatureModule 明确
CAN 支持明确
Modbus RTU + TCP 支持明确
DOF 平台接口 + Feature 实现明确
真实设备与模拟设备支持明确
设备录制回放支持明确
FFMPEG Recorder / 推流工具边界明确
设备状态监控定义为数据状态监控明确
RuntimeConfigUI 接入设备配置和状态明确
DeviceIntegration 与 InputSystem / NetworkSystem / SaveSystem / FrameworkConfig / BuildProfileSystem / LicenseSystem 的关系明确
禁止事项写入文档
Current_Context 更新完成
Roadmap / Todo / Changelog 同步完成
```

满足以上条件后，可进入：

```text
P3.21 Samples Foundation
```

---

# 三十一、最终结论

P3.20 DeviceIntegration Foundation 是设计冻结阶段。

它只确定：

```text
DeviceIntegration 是什么
DeviceIntegration 管什么
DeviceIntegration 不管什么
哪些设备能力属于 Platform
哪些具体设备属于 FeatureModule
真实设备和模拟设备如何统一
设备数据如何录制回放
设备状态如何监控
视频录制和推流工具如何保持独立
现场人员如何配置设备
后续实现应遵守什么边界
```

不得在本阶段进入具体设备接入实现。

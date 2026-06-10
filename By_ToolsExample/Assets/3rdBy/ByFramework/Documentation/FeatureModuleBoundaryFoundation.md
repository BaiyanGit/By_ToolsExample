# P3.17 FeatureModule Boundary Foundation 设计冻结

> 阶段性质：设计冻结阶段  
> 不是 Runtime 实现阶段  
> 不创建 FeatureModule Runtime 脚本  
> 不实现 VehicleSimulation、TrainingSystem、ScenarioSystem、SimulationSync、DeviceIntegration 或 CustomerModules 代码  

---

# 一、阶段目标

P3.17 的目标是冻结 FeatureModule Boundary Foundation 的基础设计，为后续业务模块、行业模块、客户定制模块和示例模块提供明确边界。

本阶段只允许完成：

```text
FeatureModule 定位冻结
FeatureModule 与 Core / Platform 依赖关系冻结
Platform 不依赖 FeatureModule 的红线冻结
DeviceIntegration 边界冻结
VehicleSimulation 归属冻结
TrainingSystem 归属冻结
ScenarioSystem 归属冻结
SimulationSync 归属冻结
FeatureModule 访问 Unity 的规则冻结
客户定制模块组织方式冻结
FeatureModule 示例规则冻结
FeatureModule 禁止污染 Platform 的规则冻结
```

本阶段不进入具体业务模块实现。

---

# 二、架构位置

FeatureModule 属于 ByFramework 的业务扩展层。

整体架构仍然是：

```text
Core
↓
Platform
↓
FeatureModule
```

依赖规则：

```text
Core 不依赖 Platform
Platform 不依赖 FeatureModule
FeatureModule 可以依赖 Platform + Core
```

FeatureModule 是：

```text
业务层
行业解决方案层
客户定制层
```

选择：

```text
D：以上全部
```

---

# 三、FeatureModule 定位

FeatureModule 是 ByFramework 中承载具体业务、行业能力、项目能力和客户定制能力的层。

它负责：

```text
车辆仿真
训练系统
场景系统
仿真同步
设备接入业务
数字孪生业务
客户定制模块
项目业务流程
行业解决方案
```

FeatureModule 的核心目标是：

```text
把业务能力留在业务层，不污染 Core 和 Platform。
```

---

# 四、FeatureModule 可以依赖 Platform

FeatureModule 是否允许依赖 Platform：

```text
A：允许
```

FeatureModule 可以使用：

```text
InputSystem
UISystem
DisplaySystem
LocalizationSystem
ResourceSystem
SaveSystem
FrameworkConfig
NetworkSystem
BuildProfileSystem
LicenseSystem
```

示例：

```text
TrainingSystem 使用 UISystem 显示训练界面
TrainingSystem 使用 SaveSystem 保存训练记录
VehicleSimulation 使用 InputSystem 获取驾驶输入
VehicleSimulation 使用 ResourceSystem 加载车辆资源
SimulationSync 使用 NetworkSystem 发送同步消息
DeviceIntegration 使用 NetworkSystem / InputSystem 接入设备数据
```

---

# 五、Platform 绝对禁止依赖 FeatureModule

Platform 是否允许依赖 FeatureModule：

```text
A：绝对禁止
```

这是 ByFramework 的架构红线。

禁止：

```text
Platform → FeatureModule
```

禁止在 Platform 中引用：

```text
VehicleSimulation
TrainingSystem
ScenarioSystem
SimulationSync
DeviceIntegration 具体设备业务
CustomerModules
```

---

## 5.1 禁止示例

禁止在 NetworkSystem 中出现：

```text
VehicleState
TrainingScore
RobotJointState
SimulationSyncMessage
```

禁止在 ResourceSystem 中出现：

```text
VehicleResource
TrainingResource
CustomerAResource
```

禁止在 SaveSystem 中出现：

```text
TrainingRecord 业务结构
VehicleConfig 业务结构
DeviceCalibration 业务结构
```

禁止在 UISystem 中出现：

```text
LoginBusiness
TrainingFlow
ExamScore
VehicleControl
```

---

# 六、FeatureModule 与 Core 的关系

FeatureModule 可以依赖 Core。

例如：

```text
EventManager
FSMManager
ThreadDispatcher
Core Utilities
```

但 FeatureModule 不允许修改 Core 生命周期设计。

禁止：

```text
FeatureModule 修改 FrameworkEntry 生命周期链
FeatureModule 修改 Core 初始化顺序
FeatureModule 强制 Core 依赖业务模块
```

---

# 七、DeviceIntegration 边界

DeviceIntegration 处理方式选择：

```text
C：Platform 提供基础能力，FeatureModule 实现具体设备
```

---

## 7.1 Platform 可提供的基础能力

Platform 可以提供通用能力：

```text
InputSystem
NetworkSystem
FrameworkConfig
SaveSystem
ResourceSystem
```

如果未来需要，也可以设计通用设备基础模块，例如：

```text
DeviceSystem
SerialPort 基础通信
Modbus 基础协议
CANBus 基础协议
```

但当前主架构中暂不强制加入 DeviceSystem。

---

## 7.2 FeatureModule 负责具体设备

具体设备业务属于 FeatureModule。

例如：

```text
PLC 接入
MCU 接入
机械臂接入
运动平台接入
方向盘踏板接入
编码器接入
客户设备接入
```

这些属于：

```text
FeatureModule/DeviceIntegration
```

---

## 7.3 DOF 处理规则

DOF 当前暂不进入 ByFramework 主干 Platform 模块。

DOF 主要面向硬件对接和设备业务。

因此暂定归入：

```text
FeatureModule/DeviceIntegration/DOF
```

或未来如确有通用价值，再抽象为：

```text
Platform/DeviceSystem
```

但当前阶段不加入 Platform 主架构。

---

# 八、VehicleSimulation 归属

VehicleSimulation 归属选择：

```text
B：FeatureModule
```

VehicleSimulation 包括：

```text
车辆动力学
车辆控制
车辆状态
车辆配置
车辆资源业务
车辆仪表业务
车辆训练逻辑
车辆同步业务
```

这些不属于 Platform。

---

## 8.1 与 InputSystem 的关系

正确关系：

```text
InputSystem
↓
输出输入信号
↓
VehicleSimulation
↓
解释为油门 / 刹车 / 方向盘 / 档位业务
```

InputSystem 不控制车辆。

---

## 8.2 与 ResourceSystem 的关系

正确关系：

```text
VehicleSimulation
↓
ResourceSystem
↓
加载车辆资源
```

ResourceSystem 不理解车辆业务。

---

# 九、TrainingSystem 归属

TrainingSystem 归属选择：

```text
B：FeatureModule
```

TrainingSystem 包括：

```text
训练流程
考试流程
评分
学员记录
训练结果
训练回放
训练报告
训练任务
```

这些属于业务模块。

---

## 9.1 与 SaveSystem 的关系

正确关系：

```text
TrainingSystem
↓
SaveSystem
↓
保存训练记录
```

SaveSystem 不理解训练成绩含义。

---

## 9.2 与 UISystem 的关系

正确关系：

```text
TrainingSystem
↓
UISystem
↓
显示训练界面
```

UISystem 不写训练流程。

---

# 十、ScenarioSystem 归属

ScenarioSystem 归属选择：

```text
B：FeatureModule
```

ScenarioSystem 包括：

```text
训练场景
任务系统
关卡
天气
事件
触发器
场景状态
场景流程
```

这些属于业务和项目逻辑。

---

## 10.1 与 ResourceSystem 的关系

ScenarioSystem 可以通过 ResourceSystem 加载场景资源。

但 ResourceSystem 不理解场景业务。

---

# 十一、SimulationSync 归属

SimulationSync 归属选择：

```text
B：FeatureModule
```

SimulationSync 绝不属于 NetworkSystem。

---

## 11.1 SimulationSync 职责

SimulationSync 负责：

```text
车辆状态同步
物体状态同步
机械臂状态同步
关节状态同步
场景状态同步
碰撞事件同步
仿真时间同步
多人协同状态同步
```

---

## 11.2 与 NetworkSystem 的关系

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

NetworkSystem 不理解消息代表：

```text
车辆
机械臂
场景对象
碰撞
物理状态
```

它只负责通信。

---

## 11.3 示例规则

NetworkSystem 可以提供 SimulationSync 参考示例。

但示例必须放在：

```text
Samples
Examples
Documentation
```

不得进入 NetworkSystem 主代码。

---

# 十二、FeatureModule 是否允许直接访问 Unity

FeatureModule 访问 Unity 选择：

```text
C：混合
```

原因：

```text
业务 MonoBehaviour 很多场景下不可避免
```

例如：

```text
VehicleController
ScenarioTrigger
TrainingUI
DeviceStatusView
```

可以直接使用 Unity。

---

## 12.1 允许直接访问 Unity 的情况

允许：

```text
业务 MonoBehaviour
场景触发器
车辆控制脚本
训练 UI 绑定脚本
设备状态显示脚本
场景对象脚本
```

---

## 12.2 建议通过 Framework 的情况

建议通过 Platform 的情况：

```text
资源加载
数据保存
网络通信
UI 打开关闭
多语言文本
显示布局
授权判断
配置读取
```

即业务可以使用 Unity，但不要绕过 Platform 自己重复造通用系统。

---

# 十三、客户定制模块

客户定制模块处理方式选择：

```text
B + 预留 C
```

即：

```text
单独 CustomerModules
预留 Plugin 化
```

---

## 13.1 CustomerModules

客户定制模块建议放在：

```text
FeatureModule/CustomerModules
```

示例：

```text
CustomerModules/CustomerA
CustomerModules/CustomerB
CustomerModules/CustomerC
```

---

## 13.2 预留 Plugin 化

未来可扩展为：

```text
客户模块插件化
独立程序集
独立包
按 License 加载
按 BuildProfile 裁剪
```

当前阶段只预留方向，不实现插件系统。

---

## 13.3 客户模块禁止事项

客户模块不允许反向污染 Platform。

禁止：

```text
为 CustomerA 修改 ResourceSystem 主逻辑
为 CustomerB 修改 NetworkSystem 主逻辑
为 CustomerC 修改 UISystem 主逻辑
```

应通过：

```text
配置
扩展点
Provider
Adapter
FeatureModule
```

完成定制。

---

# 十四、FeatureModule 示例模块

建议 FeatureModule 方向：

```text
SimulationSync
VehicleSimulation
TrainingSystem
ScenarioSystem
DigitalTwinSystem
DeviceIntegration
IndustrialProcess
DrivingEvaluation
MultiMachineCoordination
SimulationServerFeature
CustomerModules
```

---

## 14.1 SimulationServerFeature

SimulationServerFeature 属于 FeatureModule。

它可以使用 NetworkSystem Server 基础能力。

但 Server 的业务逻辑、数据库连接、用户系统、仿真服务逻辑不属于 NetworkSystem。

---

## 14.2 DigitalTwinSystem

DigitalTwinSystem 属于 FeatureModule。

它可以使用：

```text
ResourceSystem
NetworkSystem
SaveSystem
UISystem
DisplaySystem
```

但 Platform 不理解数字孪生业务。

---

## 14.3 IndustrialProcess

IndustrialProcess 属于 FeatureModule。

用于工业流程、设备状态、生产线逻辑等业务。

---

## 14.4 DrivingEvaluation

DrivingEvaluation 属于 FeatureModule。

用于驾驶评分、驾驶行为分析、考试结果等业务。

---

# 十五、FeatureModule 与 LicenseSystem

FeatureModule 可以询问 LicenseSystem。

例如：

```text
某客户模块是否授权
某高级训练功能是否授权
某设备接入模块是否授权
SimulationServerFeature 是否授权
```

LicenseSystem 不理解业务流程。

FeatureModule 根据授权结果决定是否启用业务功能。

---

# 十六、FeatureModule 与 BuildProfileSystem

BuildProfileSystem 可决定某些 FeatureModule 是否包含在构建中。

例如：

```text
客户 A 构建包含 CustomerA 模块
演示版排除高级训练模块
非 Server 版本排除 SimulationServerFeature
非设备版排除 DeviceIntegration
```

BuildProfileSystem 只做构建期裁剪。

FeatureModule 自身负责业务逻辑。

---

# 十七、FeatureModule 与 FrameworkConfig

FrameworkConfig 可提供 FeatureModule 配置。

例如：

```text
training_config.json
vehicle_config.json
scenario_config.json
device_config.json
customer_config.json
```

FrameworkConfig 只负责配置加载和合并。

FeatureModule 负责解释业务配置。

---

# 十八、FeatureModule 与 SaveSystem

FeatureModule 使用 SaveSystem 保存业务数据。

例如：

```text
训练记录
车辆配置
设备参数
场景状态
客户业务数据
```

SaveSystem 不理解业务含义。

---

# 十九、FeatureModule 与 ResourceSystem

FeatureModule 使用 ResourceSystem 加载业务资源。

例如：

```text
车辆模型
训练场景
设备模型
客户资源
任务资源
音频素材
```

ResourceSystem 不理解业务含义。

---

# 二十、FeatureModule 与 UISystem

FeatureModule 使用 UISystem 展示业务界面。

例如：

```text
训练界面
车辆状态界面
设备状态界面
客户定制界面
评分界面
```

UISystem 不写业务流程。

---

# 二十一、FeatureModule 与 NetworkSystem

FeatureModule 使用 NetworkSystem 通信。

例如：

```text
SimulationSync 通信
设备网关通信
训练记录上传
多人协同通信
SimulationServerFeature 通信
```

NetworkSystem 不写业务协议解释。

---

# 二十二、FeatureModule 与 LocalizationSystem

FeatureModule 使用 LocalizationSystem 获取业务文本。

例如：

```text
训练提示
设备错误
车辆状态
客户 UI 文本
```

LocalizationSystem 不理解业务含义。

---

# 二十三、FeatureModule 与 DisplaySystem

FeatureModule 使用 DisplaySystem 获取显示目标或绑定业务显示内容。

例如：

```text
车辆主视角
仪表屏
中控屏
训练控制屏
大屏展示
```

DisplaySystem 不写业务显示逻辑。

---

# 二十四、FeatureModule 与 InputSystem

FeatureModule 使用 InputSystem 获取输入。

例如：

```text
驾驶输入
训练操作输入
设备输入
调试输入
```

InputSystem 不解释业务。

---

# 二十五、业务示例与框架代码边界

ByFramework 可以提供业务示例。

例如：

```text
Samples/VehicleSimulationSample
Samples/TrainingSystemSample
Samples/SimulationSyncSample
Samples/DeviceIntegrationSample
```

示例必须遵守：

```text
可删除
不被 Core / Platform 依赖
不影响框架主干
不写入 Platform 主逻辑
```

---

# 二十六、建议目录结构

本阶段仅冻结目录方向，不要求创建代码文件。

```text
Assets/ByFramework/FeatureModule
├─ SimulationSync
│  ├─ Documentation
│  └─ Samples
│
├─ VehicleSimulation
│  ├─ Runtime
│  ├─ Documentation
│  └─ Samples
│
├─ TrainingSystem
│  ├─ Runtime
│  ├─ Documentation
│  └─ Samples
│
├─ ScenarioSystem
│  ├─ Runtime
│  ├─ Documentation
│  └─ Samples
│
├─ DigitalTwinSystem
│  ├─ Runtime
│  ├─ Documentation
│  └─ Samples
│
├─ DeviceIntegration
│  ├─ PLC
│  ├─ MCU
│  ├─ SerialPort
│  ├─ CANBus
│  ├─ Modbus
│  ├─ DOF
│  ├─ Documentation
│  └─ Samples
│
├─ SimulationServerFeature
│  ├─ Runtime
│  ├─ Documentation
│  └─ Samples
│
└─ CustomerModules
   ├─ CustomerA
   ├─ CustomerB
   └─ CustomerC
```

---

# 二十七、FeatureModule 禁止事项

P3.17 阶段以及后续实现中禁止：

```text
让 Platform 依赖 FeatureModule
把 VehicleSimulation 写入 Platform
把 TrainingSystem 写入 Platform
把 ScenarioSystem 写入 Platform
把 SimulationSync 写入 NetworkSystem
把 DeviceIntegration 具体设备业务写入 InputSystem
把客户定制逻辑写入 Platform 主代码
为了某个项目修改 Core 生命周期
为了某个客户污染通用模块
让 NetworkSystem 理解车辆 / 场景 / 机械臂业务
让 SaveSystem 理解训练记录业务含义
让 ResourceSystem 理解车辆资源业务含义
让 UISystem 承载训练流程
```

---

# 二十八、FeatureModule 设计原则

FeatureModule 应遵守：

```text
业务模块可以依赖 Platform
业务模块可以依赖 Core
业务模块之间尽量低耦合
业务模块通过接口协作
业务模块不反向污染 Platform
客户模块独立组织
行业模块可选启用
示例模块可删除
```

---

# 二十九、P3.17 输出物

P3.17 应输出：

```text
Documentation/FeatureModuleBoundaryFoundation.md
Documentation/ByFramework_Current_Context.md 更新
Roadmap.md 更新
Todo.md 更新
Documentation/Changelog.md 更新
```

不要求输出 Runtime 代码。

---

# 三十、阶段关闭条件

P3.17 关闭条件：

```text
FeatureModule 定位明确
FeatureModule 与 Core / Platform 依赖关系明确
Platform 绝对禁止依赖 FeatureModule 明确
DeviceIntegration 归属策略明确
VehicleSimulation 归属明确
TrainingSystem 归属明确
ScenarioSystem 归属明确
SimulationSync 归属明确
FeatureModule 直接访问 Unity 的规则明确
客户定制模块组织方式明确
Samples / Examples 规则明确
FeatureModule 禁止污染 Platform 的规则明确
禁止事项写入文档
Current_Context 更新完成
Roadmap / Todo / Changelog 同步完成
```

满足以上条件后，P3.17 可关闭。

下一阶段建议进入：

```text
P3 Foundation 总结与 Runtime Implementation 入口设计
```

---

# 三十一、最终结论

P3.17 FeatureModule Boundary Foundation 是设计冻结阶段。

它只确定：

```text
FeatureModule 是什么
FeatureModule 管什么
FeatureModule 不管什么
哪些模块必须放 FeatureModule
哪些能力必须留在 Platform
业务如何使用 Platform
Platform 为什么绝对不能依赖业务
客户定制如何组织
后续业务模块实现应遵守什么边界
```

不得在本阶段进入具体业务实现。

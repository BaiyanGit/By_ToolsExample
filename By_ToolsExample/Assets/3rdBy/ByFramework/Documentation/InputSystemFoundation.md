# P3.6 InputSystem Foundation

## 1. 目标

P3.6 的目标是完成 InputSystem 的 Foundation 冻结。

本阶段只冻结：

* InputSystem Foundation 运行时契约边界
* InputAction 身份模型
* InputValue 类型模型
* InputStage 事件阶段模型
* InputContext 生命周期与消费规则
* InputProfile 分层、合并与迁移边界
* InputDevice / InputAdapter 基础契约
* 与 FrameworkConfig、SaveSystem、UISystem、LocalizationSystem、DisplaySystem、PlatformServiceRegistry 的关系

本阶段不进入：

* InputSystem Runtime 实现
* Unity Input System 或旧 Input API 适配实现
* UI 导航实现
* 硬件设备接入实现
* NetworkSystem 实现
* FeatureModule 业务输入实现

## 2. Foundation 定位

InputSystem 是 Platform 层的通用输入基础设施。

它负责：

* 统一输入语义
* 标准化输入值
* 管理输入上下文
* 管理 Profile 分层与覆盖
* 向 UISystem、FeatureModule、工具模块提供设备无关输入动作

它不负责：

* 业务语义解释
* UI 焦点导航
* 训练流程或仿真控制
* 设备驱动实现细节
* 网络输入同步

## 3. 冻结结论

### 3.1 InputAction 身份模型

InputAction 冻结为稳定语义标识，而不是具体按键定义。

冻结规则：

* Action 身份必须稳定
* Action 身份必须与设备解耦
* Action 身份必须与显示名称解耦
* Action 身份必须与本地化文本解耦
* Action 身份必须与当前 Binding 解耦

建议标识格式冻结为：

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
VehicleSimulation.Throttle
VehicleSimulation.Brake
Training.NextStep
```

冻结结论：

* `UI.*`、`Tool.*`、`Debug.*` 可由 Platform 提供通用语义
* 业务动作必须归属对应 FeatureModule
* Framework 不定义具体业务键位

### 3.2 InputAction Alias / Migration

Action 重命名不允许直接破坏用户绑定。

冻结规则：

* Action 标识变更必须通过显式 Alias 或 Migration 处理
* Alias 只解决身份迁移，不参与运行时显示
* 用户覆盖绑定迁移由 SaveSystem 消费迁移规则完成

## 4. InputValue Foundation

InputSystem Foundation 冻结以下值类型：

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

冻结说明：

* `Button` 表示离散按下类输入
* `Axis1D` 表示单轴连续值
* `Axis2D` 表示二维连续值
* `Pointer` 表示指针位置或位移
* `Text` 表示文本录入
* `DeviceState` 表示设备连接、故障、超时等状态

Foundation 阶段不冻结具体 C# 载荷结构实现，但冻结值类型集合。

## 5. InputStage Foundation

InputSystem 事件阶段冻结为：

```csharp
public enum InputStage
{
    Started = 0,
    Performed = 1,
    Canceled = 2,
    ValueChanged = 3
}
```

冻结说明：

* `Started` 表示输入开始
* `Performed` 表示满足触发条件
* `Canceled` 表示输入结束或被取消
* `ValueChanged` 表示连续值变化

Foundation 阶段只冻结阶段语义，不冻结具体事件总线实现。

## 6. InputContext Foundation

### 6.1 Context 语义

InputContext 表示当前允许响应的动作范围。

框架级基础 Context 冻结为：

* `UI`
* `Gameplay`
* `Tool`
* `Debug`

FeatureModule 可声明自己的业务 Context，例如：

* `VehicleSimulation.Driving`
* `Training.StepControl`

### 6.2 Context 生命周期

InputContext 必须具备明确所有者。

冻结规则：

* Context 必须有 Owner
* Context 必须显式 Activate
* Context 必须显式 Release
* Owner 失效后，Context 必须可被系统回收
* 场景切换、窗口关闭、模块停用后不得残留高优先级 Context

### 6.3 Context 消费规则

Foundation 冻结以下消费能力：

* 优先级
* 独占
* 透传
* 消费
* 失效恢复

冻结结论：

* 高优先级 Context 优先处理同一 Action
* 独占 Context 可阻止低优先级 Context 收到动作
* 透传 Context 可以只拦截部分动作
* 冲突处理必须确定且可诊断
* 不允许依赖注册顺序得到随机结果

## 7. InputProfile Foundation

InputProfile 分层冻结为：

1. Framework Default Profile
2. Project Profile
3. FeatureModule Profile
4. User Override Profile

合并顺序冻结为：

```text
Framework Default
-> Project
-> FeatureModule
-> User Override
```

冻结规则：

* 越后层优先级越高
* 冲突必须可诊断
* 不允许静默覆盖不兼容配置
* FeatureModule 只能覆盖自己声明的业务动作
* User Override 只保存用户修改项

### 7.1 Profile 归属

* Framework Default 归属 Platform
* Project Profile 归属项目组合层
* FeatureModule Profile 归属对应 FeatureModule
* User Override Profile 归属 SaveSystem 持久化

### 7.2 Profile 迁移

冻结规则：

* Profile 必须具备版本概念
* 旧版本用户绑定必须支持迁移
* 删除 Action 时必须给出迁移或失效诊断

## 8. InputDevice / InputAdapter Foundation

### 8.1 设备分类

Foundation 冻结以下设备类别：

* Keyboard
* Mouse
* Gamepad
* VRController
* HardwareButton
* IndustrialControlPanel
* CustomDevice

### 8.2 Adapter 边界

InputAdapter 负责：

* 发现设备
* 描述设备能力
* 读取原始输入
* 标准化输入值
* 上报连接、断开、异常状态

InputAdapter 不负责：

* 业务动作解释
* UI 导航
* Save 持久化
* Localization 文本
* FeatureModule 逻辑

### 8.3 IndustrialControlPanel 结论

`IndustrialControlPanel` 冻结为复合设备模型，不退化为简单 `HardwareButton[]` 集合。

原因：

* 工业面板通常包含按钮、旋钮、轴、灯态、协议状态
* 它需要表达复合设备能力，而不是仅表达离散通道

## 9. Foundation 契约边界

### 9.1 与 FrameworkConfig 的关系

FrameworkConfig 只允许保存轻量启动配置：

* 是否启用 InputSystem
* 默认 Profile 标识
* 默认基础 Context
* 默认设备选择策略
* 输入配置资源引用

FrameworkConfig 禁止保存：

* 完整 Binding 列表
* 用户覆盖绑定
* 设备序列号
* 设备校准结果
* 当前运行时输入状态

### 9.2 与 SaveSystem 的关系

SaveSystem 负责持久化：

* User Override Profile
* 用户绑定修改
* 设备偏好
* 机器特定校准数据

SaveSystem 不负责理解：

* 业务动作语义
* UI 导航语义
* InputAdapter 内部状态

### 9.3 与 UISystem 的关系

UISystem 只消费设备无关的 UI InputAction。

冻结规则：

* UISystem 不直接读取具体输入设备
* InputSystem 不直接实现 UI 导航
* UI 焦点、窗口、层级逻辑归属 UISystem Foundation

### 9.4 与 LocalizationSystem 的关系

LocalizationSystem 负责：

* Binding Display Token 的本地化显示
* 输入提示文本本地化

InputSystem 不负责：

* 翻译文本
* UI 展示字符串

### 9.5 与 DisplaySystem 的关系

DisplaySystem 不参与输入动作语义。

它仅可能影响：

* Pointer 坐标解释
* 多屏目标区域映射

但这些属于后续实现细节，不在本阶段冻结。

### 9.6 与 PlatformServiceRegistry 的关系

InputSystem 后续通过 PlatformServiceRegistry / Service Registration 接入。

Foundation 冻结结论：

* InputSystem 是 Platform Service
* FrameworkEntry 不直接构造 InputSystem
* InputSystem 通过注册贡献入口显式注册
* InputSystem 生命周期受 Registry 编排

## 10. 非目标

P3.6 不做以下事情：

* 不实现 Keyboard / Mouse / Gamepad / VR 适配器
* 不实现旧 `Input.GetKey` 迁移
* 不实现 UGUI / UI Toolkit 导航
* 不实现硬件串口、MCU、PLC 接入
* 不实现 Network 输入分发
* 不实现业务 FeatureModule 动作

## 11. 实施前置条件

InputSystem 进入 Runtime Implementation 前，至少还需要完成：

1. InputSystem 最终 Runtime C# API
2. InputEvent / InputSnapshot 数据结构设计
3. InputContext 冲突诊断模型
4. InputProfile Schema 与迁移规则
5. InputAdapter 接口与设备能力模型
6. 现有直接输入调用迁移审计

## 12. 当前结论

P3.6 完成后，InputSystem 进入“Foundation 已冻结、Implementation 尚未开始”状态。

下一阶段应进入：

```text
P3.7 UISystem Foundation
```

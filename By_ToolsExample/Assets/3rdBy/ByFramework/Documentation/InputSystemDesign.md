# P2.2 InputSystem Design

## 目标与边界

InputSystem 是 Platform 层的统一输入抽象，负责将键盘、鼠标、手柄、VR 控制器、单片机按钮、工业控制面板和自定义外部设备输入转换为稳定的语义动作。

UI 与业务逻辑只消费 `InputAction`，不直接依赖具体按键、轴、设备协议或硬件型号。

本设计只确定输入模型、职责边界、配置层级与依赖关系，不修改 Runtime、Editor 或 API，也不实现 InputSystem。

## 设计原则

* 动作语义与设备输入分离。
* 开发期键盘与鼠标模拟、运行期外部硬件输入使用同一 Action 消费路径。
* 框架只定义业务无关的基础动作与扩展机制，不定义具体业务键位。
* 输入设备适配器只负责采集和标准化输入，不执行 UI 或业务逻辑。
* InputContext 控制当前允许响应的动作集合，不由具体设备决定。
* 用户配置与运行时状态不写入 FrameworkConfig。
* InputSystem 可以依赖 Core，并可选使用 SaveSystem；UISystem 和 FeatureModule 消费 InputSystem。
* InputSystem 不依赖 UISystem、DisplaySystem、LocalizationSystem 或任何 FeatureModule 具体实现。
* 模块间通知优先通过 EventManager，能力访问优先通过接口、配置或服务注册。

## 系统契约摘要

### 系统职责

InputSystem 负责设备发现与适配、输入标准化、Binding 映射、Profile 合并、Context 管理，以及向消费者提供设备无关的 InputAction。

### 不负责什么

InputSystem 不执行 UI 焦点导航，不实现业务行为，不维护翻译文本，不保存用户配置，也不直接控制显示输出。

### 可依赖模块

* Core 通用能力。
* EventManager，用于设备变化、Profile 变化和 Action 等跨模块通知。
* SaveSystem 的持久化接口，用于可选加载与保存用户覆盖。

InputSystem 对 SaveSystem 的依赖必须通过接口或服务注册建立，不能硬编码具体实现。

### 禁止依赖模块

* UISystem、DisplaySystem 与 LocalizationSystem 的具体实现。
* 任何 FeatureModule 具体类型。
* 具体项目、车辆、设备或业务流程代码。
* SaveSystem 的具体 Provider 实现。

### 可扩展点

* InputDevice Adapter。
* InputAction 注册源。
* InputBinding Provider。
* InputProfile Provider 与合并策略。
* InputContext 策略。
* 用户 Binding 持久化接口。
* Binding Display Token Provider。

### 与 FrameworkConfig 的关系

InputSystem 只从 FrameworkConfig 读取轻量启动配置，例如启用开关、默认 Profile 标识、基础 Context、设备选择策略和配置资源标识。

完整 Binding、用户覆盖、设备校准、运行时状态和 FeatureModule 业务动作禁止写入 FrameworkConfig。

## 总体结构

```text
Physical Input
├─ Keyboard
├─ Mouse
├─ Gamepad
├─ VRController
├─ HardwareButton
└─ CustomDevice
        │
        ▼
InputDevice Adapter
        │ 标准化设备输入
        ▼
InputBinding
        │ 映射
        ▼
InputAction
        │ 受 InputContext 与 Profile 控制
        ▼
Consumers
├─ UISystem
├─ FeatureModule
├─ Debug Tools
└─ Runtime Tools
```

## 核心概念

### InputAction

InputAction 表示稳定的输入语义，不表示具体设备按键。

框架可以提供业务无关的基础 UI 动作：

* `Up`
* `Down`
* `Left`
* `Right`
* `Confirm`
* `Cancel`
* `SwitchTabLeft`
* `SwitchTabRight`

未来动作应支持不同值类型：

* Button：按下、保持、释放。
* Axis1D：单轴连续值。
* Axis2D：二维方向值。
* Pointer：指针位置或增量。

动作标识必须稳定、可扩展并避免名称冲突。FeatureModule 应使用自己的模块作用域定义业务动作，例如：

```text
VehicleSimulation.Throttle
VehicleSimulation.Brake
Training.NextStep
DigitalTwin.SelectDevice
```

这些业务动作属于对应 FeatureModule，不属于框架核心。

本文中的 `InputAction` 是 ByFramework 的概念模型，不预先绑定 Unity Input System 包中的同名类型。后续实现阶段需单独决定底层输入后端。

### InputBinding

InputBinding 描述一个 InputAction 与具体设备输入之间的映射关系。

Binding 至少需要表达：

* 目标 Action。
* 设备类型或设备能力。
* 具体按键、轴、按钮编号或硬件通道。
* 触发方式，例如按下、释放、保持或连续值。
* 可选的死区、灵敏度、反转和组合键规则。
* 生效的 InputContext。
* Binding 来源与优先级。

示例关系：

```text
UI.Confirm
├─ Keyboard.Enter
├─ Gamepad.SouthButton
└─ HardwareButton.Channel04
```

InputBinding 只描述映射，不执行业务行为，也不保存本地化显示文本。

### InputProfile

InputProfile 是一组 InputBinding、设备偏好和上下文配置的集合。

建议支持以下层级：

1. Framework Default Profile
2. Project Profile
3. FeatureModule Profile
4. User Override Profile

合并原则：

* Framework Default 提供通用 UI 与工具动作的默认映射。
* Project Profile 选择项目默认设备与通用覆盖。
* FeatureModule Profile 注册自己的业务动作与默认绑定。
* User Override 只保存用户修改项，并拥有最高优先级。
* Profile 合并发生冲突时必须可诊断，不能静默覆盖不兼容动作。

例如 `VehicleSimulationInputProfile` 属于 VehicleSimulation FeatureModule。它可以定义车辆仿真动作和默认绑定，但不能进入 Core 或通用 Platform 配置。

### InputDevice

InputDevice 表示输入来源及其能力。建议规划：

* Keyboard
* Mouse
* Gamepad
* VRController
* HardwareButton
* CustomDevice

InputDevice Adapter 负责：

* 发现与识别设备。
* 读取设备原始输入。
* 标准化按钮、轴、指针和状态。
* 上报连接、断开和能力变化。
* 屏蔽串口、网络、SDK 或平台 API 等底层差异。

InputDevice Adapter 不负责：

* 决定输入对应哪个业务动作。
* 切换 UI 焦点。
* 保存用户绑定。
* 直接调用 FeatureModule。

设备应优先使用稳定能力或逻辑标识进行绑定。机器特定端口、设备序列号和硬件校准数据不应写入 FrameworkConfig。

### InputContext

InputContext 描述当前允许响应的动作范围和优先级。

框架级基础 Context：

* UI
* Gameplay
* Debug
* Tool

FeatureModule 可以注册自己的 Context，但不得修改框架 Context 的通用语义。

建议采用上下文栈或优先级模型：

```text
Top Priority
├─ Modal UI
├─ UI
├─ Tool
├─ Gameplay
└─ Debug / Global
Bottom Priority
```

Context 应支持：

* 激活与停用。
* 临时压入与弹出。
* 独占或透传输入。
* 按动作控制是否消费事件。
* 场景切换和界面关闭后的安全恢复。

例如打开模态确认框时，`Confirm` 与 `Cancel` 应由 Modal UI 消费，不应继续触发 Gameplay 动作。

## 输入处理流程

推荐流程：

```text
Device Raw Input
→ Device Adapter 标准化
→ Active Profile 查找 Binding
→ Active Context 过滤与排序
→ 生成 InputAction 状态或事件
→ UISystem / FeatureModule 消费
```

同一个物理输入可以在不同 Context 中映射到不同 Action，但冲突规则必须明确且可诊断。

输入事件至少应区分：

* Started：输入开始。
* Performed：输入满足触发条件。
* Canceled：输入结束或被取消。
* Value Changed：连续值变化。

具体事件模型与 API 留待实现设计确认。

## 框架核心与业务模块边界

### 属于 Platform InputSystem

* InputAction 的通用模型与注册机制。
* InputBinding 的通用映射模型。
* InputProfile 的加载、合并与覆盖规则。
* InputDevice 能力与适配器边界。
* InputContext 的激活、优先级与消费规则。
* 通用 UI、Debug 和 Tool 动作语义。
* 设备连接、断开和当前活动设备状态。
* 向消费者提供设备无关的动作事件或状态。

### 属于 FeatureModule

* 具体业务动作定义。
* 业务模块默认 InputProfile。
* 业务动作与模块逻辑的绑定。
* 具体车辆、设备、训练流程或项目专属键位。
* 业务 Context 和业务输入冲突规则。

FeatureModule 可以注册动作、Binding 和 Profile，但 InputSystem 不应引用具体 FeatureModule 类型。

## FrameworkConfig 边界

### 建议进入 FrameworkConfig

FrameworkConfig 后续可以保存轻量启动配置：

* InputSystem 是否启用。
* 默认 Input Profile 标识。
* 默认启用的基础 InputContext。
* 默认设备选择策略，例如自动、键鼠优先或手柄优先。
* 是否允许加载用户 Binding 覆盖。
* 输入配置资源的引用或标识。

### 不应进入 FrameworkConfig

* 完整 InputBinding 列表。
* 用户修改后的 Binding。
* 具体业务模块动作与键位。
* 设备序列号、串口号和机器特定硬件通道。
* 设备校准结果。
* 当前按键状态、活动 Context 栈和运行时输入状态。

FrameworkConfig 负责选择默认配置，不负责保存输入配置内容和用户状态。

## SaveSystem 与 InputProfile

SaveSystem 负责持久化用户 InputProfile 覆盖与机器特定输入设置。

建议保存：

* 用户修改后的 Binding 差异。
* 用户选择的活动 Profile。
* 用户设备偏好。
* 可选的设备校准数据。

不建议复制保存完整默认 Profile。保存差异可以让框架、项目和 FeatureModule 默认配置升级后继续生效。

SaveSystem 还需要为输入配置提供：

* Schema 版本。
* 配置迁移。
* 无效 Binding 检测。
* 缺失设备回退。
* 恢复默认配置。

InputSystem 消费 SaveSystem 提供的持久化能力，但 SaveSystem 不应理解 InputAction 的具体业务语义。

## Localization 与输入提示

输入提示不应直接显示硬编码按键文字。

推荐流程：

```text
InputAction
→ 查询当前生效 InputBinding
→ 获取设备无关的 Binding Display Token
→ LocalizationSystem 本地化设备名、按键名与提示模板
→ UISystem 显示最终提示
```

例如同一动作可以显示为：

```text
按 Enter 确认
Press A to Confirm
按控制面板确认键继续
```

边界要求：

* InputSystem 提供当前 Binding 的稳定显示标识，不维护完整翻译文本。
* LocalizationSystem 负责按键名称、设备名称和提示模板本地化。
* UISystem 负责组合并显示提示。
* FeatureModule 维护业务动作名称与业务提示翻译。
* 切换设备、Binding 或语言后，提示应能够刷新。

InputSystem 不直接依赖 LocalizationSystem。它只提供稳定的 Binding Display Token；LocalizationSystem 或上层组合服务负责将 Token 转换为本地化文本。

## UISystem 消费方式

UISystem 只消费 InputSystem 提供的 UI 语义动作：

* `Up`
* `Down`
* `Left`
* `Right`
* `Confirm`
* `Cancel`
* `SwitchTabLeft`
* `SwitchTabRight`

UISystem 不应：

* 直接调用 `Input.GetKey`、`Input.GetAxis` 或读取硬件 SDK。
* 根据具体键盘、手柄或硬件按钮实现焦点规则。
* 把 UGUI Navigation 作为未来核心导航方案。

依赖方向必须保持为 `UISystem -> InputSystem`。InputSystem 不得反向访问 UISystem、UIFocusSystem 或 UIInputNavigationSystem。

建议职责分工：

```text
InputSystem
└─ 产生 UI InputAction

UIInputNavigationSystem
└─ 将 UI InputAction 转换为导航意图

UIFocusSystem
└─ 根据焦点规则移动、确认或取消
```

鼠标与指针输入可以作为 UISystem 的指针通道，但仍应通过 InputSystem 的设备与上下文边界统一管理。

## 开发期模拟与运行期硬件

开发期可以使用 Keyboard 与 Mouse Binding 模拟最终硬件动作。

例如：

```text
HardwareButton.Channel04 ──> UI.Confirm
Keyboard.Enter ────────────> UI.Confirm
```

两种 Binding 产生同一个 InputAction，UISystem 与业务模块不需要区分输入来源。

运行阶段切换为外部硬件时，只替换或启用对应 InputDevice Adapter 与 InputProfile，不修改消费逻辑。

## 当前代码迁移边界

当前项目仍存在 `Input.GetAxis`、`Input.GetMouseButton`、`Input.GetKeyDown`、`KeyCode` 与 `StandaloneInputModule` 等直接输入依赖。

这些调用属于未来迁移审计范围。本阶段不修改现有 Guide、Extension、UI、Samples 或其它模块代码，也不要求现有调用立即迁移。

## 风险分析

### InputAction 命名冲突

不同模块如果使用无作用域字符串注册动作，可能发生冲突。业务动作必须具备模块作用域或等价的稳定标识策略。

### Context 泄漏

界面关闭、场景切换或异常退出后，如果 Context 未正确弹出，可能导致输入被错误拦截。未来实现必须支持诊断与安全恢复。

### Binding 冲突

同一设备输入可能绑定多个 Action。系统必须定义 Context 优先级、消费和透传规则，并提供冲突诊断。

### 外部设备不稳定

外部硬件可能断连、延迟、重复上报或产生异常值。Device Adapter 必须隔离这些问题，不让消费者直接处理底层协议。

### 用户配置失效

模块升级或 Action 重命名可能导致用户 Binding 无效。SaveSystem 与 InputProfile 必须支持版本和迁移。

### 输入提示与实际 Binding 不一致

提示若使用硬编码文本，会在设备、配置或语言切换后失真。提示必须从当前生效 Binding 动态生成。

### Platform 与 FeatureModule 边界污染

将具体车辆、设备或项目动作放入框架默认 Profile，会让 InputSystem 失去业务无关性。

## 后续设计任务建议

InputSystem 真正实现前建议继续完成：

1. InputAction 标识与值类型设计。
2. InputContext 优先级、消费与透传规则设计。
3. InputProfile 合并、冲突与版本策略设计。
4. InputDevice Adapter 接口设计。
5. SaveSystem 用户 Binding 持久化契约设计。
6. Localization Binding Display Token 契约设计。
7. 现有直接输入调用迁移审计。

本设计完成不代表 InputSystem 已经开始实现。

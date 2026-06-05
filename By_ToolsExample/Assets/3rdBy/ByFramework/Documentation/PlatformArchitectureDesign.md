# P2.1 Platform Architecture Design

## 目标与边界

Platform 层为不同 Unity 项目提供可复用、业务无关的平台能力。

本设计只确定系统职责、依赖方向、配置边界和实现优先级，不修改 Runtime、Editor 或 API，不实现任何系统，也不开始 FrameworkEntry Phase2A 接管。

## Platform 分层设计

Platform 内部按职责分为四组：

```text
Platform
├─ Foundation
│  ├─ ResourceSystem
│  └─ SaveSystem
├─ Environment
│  ├─ DisplaySystem
│  └─ InputSystem
├─ Experience
│  ├─ LocalizationSystem
│  └─ UISystem
└─ Operations
   ├─ NetworkSystem
   ├─ LicenseSystem
   └─ BuildProfileSystem
```

该分组用于表达职责与依赖方向，不要求当前立即调整目录结构。

## 通用系统设计契约

所有 Platform 子系统必须保持低耦合、模块化和可替换：

* Platform 子系统可以依赖 Core，不得依赖 FeatureModule。
* Platform 子系统之间只能建立经过设计确认的单向依赖，禁止随意双向引用或形成循环依赖。
* 模块访问优先使用接口、事件、配置或服务注册，不直接构造其它模块的具体实现。
* 跨模块通知优先使用 EventManager；同步能力调用优先依赖接口。
* 具体项目、行业、车辆、设备和业务流程不得进入 Platform 通用类型。
* 每个系统的默认实现必须允许后续替换，消费者不应依赖具体 Provider。

后续每份 Platform 系统设计文档必须明确：

| 必需章节 | 需要回答的问题 |
| --- | --- |
| 系统职责 | 系统提供哪些业务无关能力 |
| 不负责什么 | 哪些能力明确留给其它系统或 FeatureModule |
| 可依赖模块 | 允许消费哪些 Core 或 Platform 契约 |
| 禁止依赖模块 | 哪些反向依赖、业务类型或具体实现被禁止 |
| 可扩展点 | 哪些 Provider、Adapter、Profile 或接口允许替换 |
| FrameworkConfig 关系 | 只读取哪些轻量启动配置，哪些数据禁止进入配置中心 |

### Foundation

Foundation 提供其它 Platform 系统可复用的底层能力。

* ResourceSystem：统一资源定位、加载、释放和模块资源边界。
* SaveSystem：统一持久化位置、序列化边界、版本与迁移策略。

### Environment

Environment 抽象应用运行时所处的显示与输入环境。

* DisplaySystem：描述显示设备、视口、输出布局和显示模式。
* InputSystem：把不同输入设备映射为业务无关的统一输入动作。

### Experience

Experience 提供面向用户体验的通用系统。

* LocalizationSystem：管理语言选择、文本查询和模块语言包。
* UISystem：消费显示、输入、本地化与资源能力，提供主题、焦点和导航。

### Operations

Operations 提供部署、连接和授权相关能力。

* NetworkSystem：统一 HTTP、Socket 和连接生命周期边界。
* LicenseSystem：负责软件激活、授权验证、机器码与离线授权。
* BuildProfileSystem：在构建阶段组合模块、功能、配置与资源。

## 系统职责边界

### DisplaySystem

负责：

* 单屏、分屏、多联屏、融屏和特殊比例显示布局。
* 多分辨率、横屏、竖屏与自定义显示墙描述。
* VR 与 Non-VR 显示模式选择。
* 显示设备、逻辑视口和输出目标的映射。
* 向 UISystem 和 FeatureModule 提供只读显示上下文。

不负责：

* UI 主题、焦点与导航。
* 具体业务摄像机逻辑。
* 具体车型、设备或项目专属显示规则。

### UISystem

负责：

* Theme：颜色、字体、按钮样式、图标和背景主题。
* Focus：焦点状态、焦点组和焦点切换规则。
* Navigation：统一处理上下、左右、标签切换、确认和取消。
* 消费 DisplaySystem 提供的显示上下文。
* 消费 InputSystem 提供的统一 UI 输入动作。
* 消费 LocalizationSystem 和 ResourceSystem 提供的文本与资源。

不负责：

* 定义具体业务界面流程。
* 直接读取键盘、手柄或硬件按钮。
* 把 UGUI Navigation 作为未来核心导航方案。
* 直接管理 AssetBundle。

### InputSystem

负责：

* Keyboard、Mouse、Gamepad、HardwareButton 等输入设备适配。
* 将设备输入映射为统一 `InputAction`。
* 管理 `InputBinding`、`InputProfile` 与设备切换。
* 支持 FeatureModule 提供自己的业务输入配置。
* 向 UISystem 和 FeatureModule 发布统一输入动作。

不负责：

* 在框架核心中定义具体车辆、设备或业务键位。
* 执行 UI 焦点规则。
* 保存具体项目的用户绑定数据；持久化由 SaveSystem 负责。

### LocalizationSystem

负责：

* 默认语言与当前语言管理。
* 运行时语言切换。
* 文本 Key 查询与缺失文本回退。
* Core、Platform 与 FeatureModule 语言包注册。
* 通过 ResourceSystem 加载语言资源。

不负责：

* 集中维护所有模块的具体翻译内容。
* 在 FrameworkConfig 中保存完整语言文本。
* 直接控制 UI 布局。

### ResourceSystem

负责：

* 统一资源定位、加载、释放与错误处理边界。
* AssetBundle 打包与加载的长期扩展点。
* 模块资源注册、隔离与卸载。
* 为 UISystem、LocalizationSystem 和 FeatureModule 提供资源访问能力。
* 保留未来替换资源后端的能力。

不负责：

* 当前阶段引入 Addressables。
* 管理业务对象生命周期。
* 在 FrameworkConfig 中保存完整资源清单或 AssetBundle Manifest。

### LicenseSystem

负责：

* 软件激活与授权验证。
* 机器码生成与验证边界。
* 离线授权。
* 可选的在线授权验证。
* 授权结果与模块可用性状态。

不负责：

* 在 FrameworkConfig 或源码中保存密钥、授权码和用户授权状态。
* 直接决定 BuildProfile 的资源裁剪。
* 依赖具体业务模块。

### BuildProfileSystem

负责：

* 构建配置选择。
* 功能开关、模块裁剪和资源裁剪。
* 组合构建目标、配置预设与模块清单。
* 生成业务无关的构建产物描述。

不负责：

* 在运行时动态卸载任意已编译模块。
* 使用具体行业、车型或设备名称定义 Profile。
* 把用户设置或运行时状态写回 FrameworkConfig。

BuildProfileSystem 主要属于构建期能力。运行时仅可消费构建生成的只读 Profile 元数据，不应依赖 Editor 实现。

### NetworkSystem

负责：

* HTTP、Socket 等传输能力的统一边界。
* 连接、断线、重连、超时和退出生命周期。
* 请求与响应的通用错误模型。
* 为 LicenseSystem 或 FeatureModule 提供可选网络能力。

不负责：

* 定义具体业务协议。
* 自动建立项目专属服务器连接。
* 将业务消息类型写入 Core 或通用 Platform 层。

### SaveSystem

负责：

* 跨平台持久化位置与文件组织。
* 序列化、版本、迁移、备份和错误恢复边界。
* 为 InputProfile 用户覆盖、License 状态和 FeatureModule 存档提供存储能力。

不负责：

* 决定具体业务数据结构。
* 在 FrameworkConfig 中保存运行时用户数据。
* 直接管理云同步或网络协议。

## 模块依赖关系

依赖原则：

* 所有 Platform 系统可以依赖 Core。
* Platform 系统不得依赖 FeatureModule。
* FeatureModule 可以依赖 Platform。
* Platform 内部依赖必须保持单向，避免循环依赖。
* BuildProfileSystem 的 Editor 实现只在构建期消费模块与资源描述，不成为 Runtime 系统的直接依赖。
* InputSystem、DisplaySystem 与 LocalizationSystem 不依赖 UISystem；UISystem 作为消费者单向依赖其公开契约。
* 跨模块通信优先使用接口、EventManager、配置或服务注册，避免硬编码具体实现引用。

推荐依赖图：

```text
ResourceSystem ────────> Core
SaveSystem ────────────> Core
DisplaySystem ─────────> Core
InputSystem ───────────> Core
NetworkSystem ─────────> Core

LocalizationSystem ────> ResourceSystem

UISystem ───────────────> DisplaySystem
UISystem ───────────────> InputSystem
UISystem ───────────────> LocalizationSystem
UISystem ───────────────> ResourceSystem

InputSystem ────────────> SaveSystem（用户绑定持久化，可选）
LicenseSystem ──────────> SaveSystem
LicenseSystem ──────────> NetworkSystem（在线验证，可选）

FeatureModule ──────────> Platform

BuildProfileSystem Editor ──> 模块描述 / 配置预设 / ResourceSystem 资源清单
```

图中的箭头表示左侧系统依赖或消费右侧系统。系统之间应通过明确接口或数据契约连接。LicenseSystem 的网络验证必须是可选能力，离线授权不能强制依赖 NetworkSystem。

## FrameworkConfig 边界

FrameworkConfig 应保存启动所需的轻量、稳定、业务无关默认配置，以及各 Platform 配置资源的引用或标识。

它不是所有平台数据的集中存储，也不应承担用户状态、资源内容或构建过程数据。

FrameworkConfig 不能被用作模块通信总线或运行时状态容器。系统运行状态必须由对应系统管理，用户与机器特定数据应交由 SaveSystem 或专用安全存储。

### 应进入 FrameworkConfig

建议后续按独立设计逐步纳入：

* Platform 模块默认启用开关。
* 默认 Display Profile 标识。
* 默认显示模式，例如 VR 或 Non-VR。
* 默认语言与缺失文本回退语言。
* 默认 UI Theme 标识。
* 默认 Input Profile 标识。
* ResourceSystem 默认 Provider 类型或资源根标识。
* NetworkSystem 通用默认超时、重试策略与环境端点标识。
* SaveSystem 默认目录名、存储 Provider 与版本策略标识。
* LicenseSystem Provider 类型和是否启用授权检查。
* 当前构建产物使用的只读 Build Profile 标识。

这些配置项必须保持业务无关。具体服务器地址、显示布局或输入绑定可以由引用的独立配置资源提供，而不是无限扩张 FrameworkConfig 本体。

### 不应进入 FrameworkConfig

* 完整语言文本和模块语言包内容。
* UI 图标、字体、Prefab、完整 Theme 资源内容。
* 每个用户修改后的 InputBinding。
* 具体车辆、设备或项目业务键位。
* AssetBundle 文件、Manifest 和完整资源清单。
* 用户存档、下载任务、运行时缓存和历史记录。
* 激活码、私钥、授权令牌、机器码结果和授权状态。
* Socket 会话状态、连接对象和业务协议。
* 多联屏现场校准结果等机器特定运行数据。
* Editor 构建流程状态和构建产物临时数据。

以上内容应分别归属独立配置资源、SaveSystem、安全存储、ResourceSystem、BuildProfileSystem 或 FeatureModule。

## BuildProfile 与 FrameworkConfig 的关系

BuildProfileSystem 负责在构建期选择：

* 启用哪些模块。
* 使用哪个 FrameworkConfig 预设。
* 包含哪些资源。
* 生成哪些只读运行时 Profile 元数据。

FrameworkConfig 负责运行时默认配置，不负责执行模块裁剪或资源裁剪。

二者必须保持边界清晰，避免运行时配置资源承担构建编排职责。

## 实现优先级建议

### P2.1：架构与契约设计

当前阶段只完成 Platform 整体架构设计。后续每个系统启动前仍需独立设计，不能根据本文件直接批量实现。

### 第一优先级：基础边界

1. ResourceSystem Contract Design
2. SaveSystem Contract Design
3. InputSystem Design
4. DisplaySystem Design

ResourceSystem 与 SaveSystem 先定义契约，不代表立即替换 Resources 或实现 AssetBundle。

InputSystem 与 DisplaySystem 应先于 UISystem，以免 UI 再次直接绑定具体设备和显示形态。

### 第二优先级：用户体验系统

1. LocalizationSystem Design
2. UISystem Design

LocalizationSystem 依赖资源加载边界。UISystem 应在 Display、Input、Localization 与 Resource 契约明确后设计。

### 第三优先级：部署与运营系统

1. NetworkSystem Design
2. LicenseSystem Design
3. BuildProfileSystem Design

LicenseSystem 需要先明确 SaveSystem 与可选 NetworkSystem 边界。

BuildProfileSystem 应在模块边界与 ResourceSystem 资源描述形成后设计，避免过早固化裁剪规则。

## 风险分析

### Platform 变成集中式大模块

九个系统必须保持独立职责和可选启用能力，不应合并成一个全能管理器。

### FrameworkConfig 无限膨胀

如果把语言内容、资源清单、用户绑定和授权状态全部放入 FrameworkConfig，会造成配置耦合、加载膨胀和安全风险。

### UISystem 形成依赖环

UISystem 可以消费 Display、Input、Localization 与 Resource，但这些系统不得反向依赖具体 UISystem 实现。

### BuildProfile 污染 Runtime

构建期裁剪逻辑不得进入 Runtime 主流程。运行时只消费构建生成的只读结果。

### ResourceSystem 过早重构

当前仍使用 Resources。本阶段只定义边界，不引入 Addressables，也不立即开始 AssetBundle 实现。

### LicenseSystem 安全边界

密钥、授权令牌和机器码结果不能进入普通配置资源或源码。后续设计必须包含安全存储与验证边界。

### 业务类型渗入 Platform

Platform 配置、接口和 Profile 不得出现具体车辆、设备或项目专属类型。业务扩展必须留在 FeatureModule。

## 后续设计任务建议

Platform Architecture Design 完成后，建议拆分为独立任务：

1. ResourceSystem Contract Design
2. SaveSystem Contract Design
3. InputSystem Design
4. DisplaySystem Design
5. LocalizationSystem Design
6. UISystem Design
7. NetworkSystem Design
8. LicenseSystem Design
9. BuildProfileSystem Design

本设计完成不代表任何 Platform 系统已经开始实现。

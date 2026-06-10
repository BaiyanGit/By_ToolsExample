# P3.8 DisplaySystem Foundation 设计冻结

> 阶段性质：设计冻结阶段  
> 不是 Runtime 实现阶段  
> 不创建 DisplaySystem Runtime 脚本  
> 不实现多屏窗口、VR、Camera 绑定或运行时显示切换代码  

---

# 一、阶段目标

P3.8 的目标是冻结 DisplaySystem Foundation 的基础设计，为后续 DisplaySystem Runtime Implementation 提供明确边界。

本阶段只允许完成：

```text
DisplaySystem 职责冻结
DisplaySystem 边界冻结
多屏模式冻结
独立窗口模式冻结
驾驶舱显示目标冻结
VR 显示目标冻结
DisplayProfile 模型冻结
DisplayTarget 模型冻结
现场配置关系冻结
DisplaySystem 与 UISystem / RuntimeConfigUI / FrameworkConfig / BuildProfile / LicenseSystem 的关系冻结
```

本阶段不进入具体运行时代码实现。

---

# 二、架构位置

DisplaySystem 属于：

```text
Platform/DisplaySystem
```

DisplaySystem 不属于 Core。

DisplaySystem 不属于 FeatureModule。

DisplaySystem 是 Platform 层的通用显示能力模块。

---

# 三、DisplaySystem 定位

DisplaySystem 是 ByFramework 的统一显示环境管理系统。

它负责统一管理：

```text
显示设备
显示目标
显示布局
多屏模式
窗口模式
全屏模式
VR 显示目标
驾驶舱显示布局
UI 显示绑定
Camera 显示绑定
RenderTexture 显示绑定
现场显示配置
```

DisplaySystem 的核心目标是：

```text
让不同项目在不同现场硬件环境下，可以通过统一配置管理显示方案。
```

---

# 四、适用场景

DisplaySystem 面向：

```text
单屏项目
双屏项目
三联屏驾驶模拟
五联屏驾驶模拟
仪表屏
中控屏
后视镜屏
控制台屏
大屏展示
投影显示
VR 项目
非 VR 项目
局域网现场部署项目
国产机器 / 非主流显卡环境
```

---

# 五、显示模式冻结

DisplaySystem 必须同时支持两类多屏方式：

```text
Unity Multi Display
独立窗口模式
```

即多屏模式选择为：

```text
C：两者都支持
```

---

## 5.1 Unity Multi Display 模式

Unity Multi Display 模式适用于：

```text
固定多屏环境
驾驶舱多屏
三联屏
五联屏
仪表屏
中控屏
副屏展示
```

特点：

```text
依赖 Unity Display
适合全屏多显示器部署
适合现场固定显示器编号
适合驾驶模拟类项目
```

---

## 5.2 独立窗口模式

独立窗口模式适用于：

```text
主窗口
仪表窗口
后视镜窗口
控制台窗口
调试窗口
配置窗口
```

特点：

```text
每个显示目标可映射为独立窗口
支持窗口位置配置
支持窗口大小配置
支持窗口显示器映射
适合现场调试
适合非标准显示环境
```

---

## 5.3 两种模式的关系

DisplaySystem 不应把两种模式写死为互斥架构。

应允许 DisplayProfile 决定当前项目使用：

```text
单屏模式
Unity Multi Display 模式
独立窗口模式
混合模式
```

混合模式示例：

```text
主视角使用 Unity Multi Display
配置界面使用独立窗口
调试界面使用独立窗口
```

---

# 六、驾驶模拟显示目标

驾驶模拟是 DisplaySystem 的重要目标场景。

DisplaySystem 必须支持驾驶舱显示布局。

建议内置显示目标概念：

```text
MainView            主视角
LeftView            左视角
RightView           右视角
InstrumentView      仪表屏
CenterConsoleView   中控屏
RearMirrorView      后视镜屏
DebugView           调试屏
ControlView         控制台屏
```

---

## 6.1 三联屏

三联屏典型布局：

```text
LeftView
MainView
RightView
```

适用于：

```text
驾驶模拟
车辆仿真
训练系统
```

---

## 6.2 五联屏

五联屏典型布局：

```text
FarLeftView
LeftView
MainView
RightView
FarRightView
```

适用于：

```text
高沉浸驾驶舱
环绕视野仿真
大型驾驶训练系统
```

---

## 6.3 仪表屏

InstrumentView 用于：

```text
速度表
转速表
车辆状态
告警状态
训练评分
```

InstrumentView 是显示目标，不负责仪表业务逻辑。

具体仪表内容属于 UISystem 或 FeatureModule。

---

## 6.4 中控屏

CenterConsoleView 用于：

```text
中控 UI
导航 UI
系统设置
训练控制
设备状态
```

中控屏内容由 UISystem / FeatureModule 提供。

DisplaySystem 只负责显示目标和显示绑定。

---

## 6.5 后视镜屏

RearMirrorView 用于：

```text
左后视镜
右后视镜
中央后视镜
```

DisplaySystem 负责显示目标和 RenderTexture / Camera 映射方向。

后视镜渲染逻辑不属于 DisplaySystem 的业务层。

---

# 七、VR 显示目标

VR 是 DisplaySystem 的可选显示目标。

不是所有项目都启用 VR。

VR 支持必须满足：

```text
可启用
可关闭
可现场配置
可通过 BuildProfile 控制
可通过 License 控制
可通过 FrameworkConfig 配置
```

---

## 7.1 VR 启用原则

VR 不应作为 DisplaySystem 的强制能力。

DisplaySystem 应支持：

```text
VR Enabled
VR Disabled
VR Unsupported
VR NotConfigured
```

其中：

```text
VR Enabled        项目启用 VR 显示目标
VR Disabled       项目或现场配置关闭 VR
VR Unsupported    当前硬件环境不支持 VR
VR NotConfigured  未配置 VR 显示目标
```

---

## 7.2 国产机器 / 非主流显卡环境

部分现场可能存在：

```text
国产机器
非 NVIDIA / AMD 主流显卡
驱动限制
VR Runtime 不可用
VR 设备未安装
```

DisplaySystem 不应强制判定现场硬件是否可用。

硬件是否支持 VR 由现场人员或部署人员判别。

DisplaySystem 只提供：

```text
VR 开关
VR 显示目标配置
VR 配置保存
VR 配置读取
VR 状态提示
```

不得因 VR 不可用影响非 VR 项目运行。

---

## 7.3 VRSystem 关系

DisplaySystem 只管理 VR 显示目标。

未来如有 VRSystem，则分工为：

```text
DisplaySystem
负责 VR 显示目标、显示配置、显示开关

VRSystem
负责 VR 设备、控制器、头显 SDK、交互逻辑
```

DisplaySystem 不直接管理 VR 输入设备。

VR 控制器输入属于 InputSystem 或未来 VRSystem。

---

# 八、DisplayProfile

DisplayProfile 是显示布局配置。

DisplaySystem 必须支持 DisplayProfile 保存与切换。

DisplayProfile 用于描述：

```text
当前显示模式
显示目标列表
显示器映射
窗口位置
窗口大小
分辨率
全屏模式
VR 开关
Canvas 绑定
Camera 绑定
RenderTexture 绑定
```

---

## 8.1 DisplayProfile 场景

DisplayProfile 可用于：

```text
默认单屏
客户 A 多屏布局
客户 B 多屏布局
驾驶舱三联屏
驾驶舱五联屏
大屏展示
VR 模式
非 VR 模式
现场调试模式
```

---

## 8.2 DisplayProfile 配置来源

DisplayProfile 应支持：

```text
Editor 配置
StreamingAssets 外部配置
PersistentDataPath 现场配置
RuntimeConfigUI 修改
命令行参数覆盖
```

配置优先级遵守 FrameworkConfig 总规则：

```text
命令行参数
>
PersistentDataPath 现场配置
>
StreamingAssets 部署配置
>
Editor 生成配置
>
默认内置配置
```

---

# 九、DisplayTarget

DisplayTarget 是具体显示目标。

DisplayTarget 不是业务对象。

它描述：

```text
这个内容显示到哪里
这个内容使用哪个显示器或窗口
这个内容绑定哪个 Camera / Canvas / RenderTexture
```

---

## 9.1 DisplayTarget 类型

建议支持：

```text
ScreenTarget
WindowTarget
RenderTextureTarget
VRTTarget
CanvasTarget
```

说明：

```text
ScreenTarget         显示器目标
WindowTarget         独立窗口目标
RenderTextureTarget  渲染纹理目标
VRTTarget            VR 显示目标
CanvasTarget         UI Canvas 目标
```

---

## 9.2 DisplayTarget 字段方向

DisplayTarget 可包含：

```text
TargetId
TargetName
TargetType
DisplayIndex
WindowPosition
WindowSize
Resolution
FullscreenMode
CameraBinding
CanvasBinding
RenderTextureBinding
IsEnabled
Priority
```

本阶段只冻结字段方向，不实现代码。

---

# 十、DisplayLayout

DisplayLayout 描述多个 DisplayTarget 的组合关系。

示例：

```text
SingleScreenLayout
TripleScreenDrivingLayout
FiveScreenDrivingLayout
DashboardLayout
VRLayout
DebugLayout
```

DisplayLayout 不负责业务内容。

它只描述显示结构。

---

# 十一、现场配置能力

DisplaySystem 必须允许现场修改显示配置。

现场人员可以通过两种方式修改：

```text
RuntimeConfigUI 可视化配置
直接修改外部配置文件
```

---

## 11.1 RuntimeConfigUI 配置项

RuntimeConfigUI 中应允许配置：

```text
当前 DisplayProfile
屏幕数量
显示目标启用状态
显示器映射
窗口位置
窗口大小
分辨率
全屏模式
VR 是否启用
默认主显示目标
仪表屏显示目标
中控屏显示目标
后视镜屏显示目标
```

---

## 11.2 外部配置文件

建议外部配置路径：

```text
StreamingAssets/ByFramework/Config/display_config.json
```

现场修改后保存路径：

```text
PersistentDataPath/ByFramework/Config/display_config.json
```

StreamingAssets 用于部署初始配置。

PersistentDataPath 用于现场运行后修改配置。

---

## 11.3 直接修改文件

DisplaySystem 的外部配置文件应尽量保持：

```text
结构清晰
字段可读
字段可注释说明
不依赖二进制格式
```

优先使用：

```text
JSON
```

如果未来需要更适合人工修改，也可以扩展：

```text
INI
YAML
```

---

# 十二、DisplaySystem 与 UISystem 的关系

DisplaySystem 管显示目标。

UISystem 管 UI 内容。

正确关系：

```text
DisplaySystem
↓
DisplayTarget / DisplayContext
↓
UISystem
↓
UIRoot / UILayer / UIView
```

UISystem 不直接检测显示器。

UISystem 通过 DisplaySystem 提供的 DisplayContext 绑定 UI 根节点。

---

# 十三、DisplaySystem 与 RuntimeConfigUI 的关系

RuntimeConfigUI 是 FrameworkConfig 的运行时配置界面。

RuntimeConfigUI 使用 UISystem 展示。

RuntimeConfigUI 可以修改 DisplaySystem 配置。

关系：

```text
FrameworkConfig
↓
RuntimeConfigUI
↓
UISystem
↓
DisplaySystem 配置项
```

DisplaySystem 负责应用显示配置。

RuntimeConfigUI 负责展示和编辑配置。

---

# 十四、DisplaySystem 与 FrameworkConfig 的关系

DisplaySystem 配置由 FrameworkConfig 统一加载和合并。

FrameworkConfig 提供：

```text
Editor 配置
StreamingAssets 配置
PersistentDataPath 配置
命令行覆盖
```

DisplaySystem 只消费最终合并后的 DisplayConfig。

DisplaySystem 不负责配置文件合并策略。

---

# 十五、DisplaySystem 与 BuildProfileSystem 的关系

BuildProfileSystem 可决定构建版本支持哪些显示能力。

例如：

```text
单屏版
多屏版
VR版
非VR版
客户A版
客户B版
演示版
正式版
```

BuildProfileSystem 可影响：

```text
默认 DisplayProfile
是否包含 VR 配置
是否包含多屏配置
是否包含驾驶舱布局
是否包含调试显示目标
```

---

# 十六、DisplaySystem 与 LicenseSystem 的关系

LicenseSystem 可控制显示相关功能授权。

例如：

```text
多屏功能是否授权
VR 功能是否授权
高级驾驶舱布局是否授权
大屏展示功能是否授权
```

DisplaySystem 不直接决定授权规则。

DisplaySystem 只在需要时询问 LicenseSystem：

```text
某显示能力是否允许启用
```

---

# 十七、DisplaySystem 与 ResourceSystem 的关系

DisplaySystem 不直接加载业务资源。

但它可能需要 DisplayProfile 或显示配置资源。

资源加载仍通过：

```text
ResourceSystem
```

例如：

```text
显示配置资源
默认布局资源
显示图标资源
```

---

# 十八、DisplaySystem 与 FeatureModule 的关系

FeatureModule 可以使用 DisplaySystem 获取显示目标。

例如：

```text
VehicleSimulation 绑定驾驶主视角 Camera
TrainingSystem 打开训练控制屏
DeviceIntegration 打开设备状态屏
SimulationSync 同步多机显示状态
```

但 FeatureModule 不允许修改 DisplaySystem 的架构边界。

正确关系：

```text
FeatureModule
↓
IDisplayService
↓
DisplaySystem
```

---

# 十九、DisplaySystem 与 PlatformServiceRegistry 的关系

DisplaySystem 后续可注册为 Platform 服务。

接口方向：

```text
IDisplayService
```

注册方向：

```text
PlatformServiceRegistry.Register<IDisplayService>(displayService)
```

获取方向：

```text
PlatformServiceRegistry.Get<IDisplayService>()
PlatformServiceRegistry.TryGet<IDisplayService>(out displayService)
```

本阶段不实现注册代码，只冻结接入方向。

---

# 二十、建议核心对象

后续 Runtime 可围绕以下对象设计。

本阶段只冻结概念，不实现代码。

```text
DisplayProfile
DisplayConfig
DisplayTarget
DisplayLayout
DisplayContext
DisplayBinding
DisplayMode
DisplayCapability
DisplayResult
DisplayError
```

---

# 二十一、DisplayMode 枚举方向

DisplayMode 可包含：

```text
SingleScreen
UnityMultiDisplay
IndependentWindow
Mixed
VR
Disabled
```

说明：

```text
SingleScreen          单屏模式
UnityMultiDisplay     Unity 多显示器模式
IndependentWindow     独立窗口模式
Mixed                 混合模式
VR                    VR 显示模式
Disabled              禁用显示目标
```

---

# 二十二、DisplayCapability

DisplayCapability 用于描述当前构建或当前配置允许的显示能力。

可包含：

```text
SupportMultiDisplay
SupportIndependentWindow
SupportVR
SupportDrivingCockpit
SupportDebugWindow
SupportRuntimeConfig
```

注意：

```text
Capability 表示能力允许或配置支持
不等同于当前硬件一定可用
```

硬件是否可用可由现场人员配置或后续 Runtime 检测辅助提示。

---

# 二十三、错误与降级策略

DisplaySystem 必须支持降级。

例如：

```text
VR 不可用 → 降级为非 VR 显示
多屏不可用 → 降级为单屏
指定显示器不存在 → 使用主显示器
独立窗口创建失败 → 使用主窗口
DisplayProfile 加载失败 → 使用默认配置
```

DisplaySystem 不应因为某个显示目标失败导致整个程序无法启动，除非该目标被明确标记为 Required。

---

## 23.1 Required / Optional

DisplayTarget 应支持：

```text
Required
Optional
```

Required 目标失败时：

```text
返回严重错误
提示现场人员检查配置
```

Optional 目标失败时：

```text
记录警告
继续运行
```

---

# 二十四、Editor 工具方向

DisplaySystem 后续可以提供 EditorWindow 配置工具。

工具方向：

```text
DisplaySystemConfigWindow
DisplayProfileEditorWindow
DisplayTargetEditorWindow
DrivingCockpitLayoutWindow
VRDisplayConfigWindow
```

用于配置：

```text
默认 DisplayProfile
显示目标列表
显示器映射
窗口位置
窗口大小
分辨率
全屏模式
驾驶舱布局
VR 默认开关
UIRoot 绑定目标
Camera 绑定目标
```

所有 Editor UI 遵守中文化规范。

示例：

```text
菜单：ByFramework/平台/DisplaySystem 配置
按钮：保存显示配置
提示：请选择默认 DisplayProfile
日志：[DisplaySystem] 显示配置保存完成
```

---

# 二十五、建议目录结构

本阶段仅冻结目录方向，不要求创建代码文件。

```text
Assets/ByFramework/Platform/DisplaySystem
├─ Runtime
│  ├─ Core
│  │  ├─ DisplayProfile
│  │  ├─ DisplayConfig
│  │  ├─ DisplayTarget
│  │  ├─ DisplayLayout
│  │  └─ DisplayContext
│  │
│  ├─ Mode
│  │  ├─ SingleScreenMode
│  │  ├─ UnityMultiDisplayMode
│  │  ├─ IndependentWindowMode
│  │  ├─ MixedDisplayMode
│  │  └─ VRDisplayMode
│  │
│  ├─ Binding
│  │  ├─ CameraDisplayBinding
│  │  ├─ CanvasDisplayBinding
│  │  └─ RenderTextureDisplayBinding
│  │
│  ├─ Service
│  │  └─ IDisplayService
│  │
│  └─ Config
│     ├─ DisplayConfig
│     └─ DisplayProfile
│
└─ Editor
   ├─ DisplaySystemConfigWindow
   ├─ DisplayProfileEditorWindow
   ├─ DisplayTargetEditorWindow
   ├─ DrivingCockpitLayoutWindow
   └─ VRDisplayConfigWindow
```

---

# 二十六、配置文件方向

建议配置文件：

```text
StreamingAssets/ByFramework/Config/display_config.json
```

现场修改后：

```text
PersistentDataPath/ByFramework/Config/display_config.json
```

示例字段方向：

```text
defaultProfile
profiles
targets
mode
resolution
windowPosition
windowSize
displayIndex
fullscreen
vrEnabled
required
```

本阶段不冻结具体 JSON Schema，只冻结配置方向。

---

# 二十七、禁止事项

P3.8 阶段禁止：

```text
实现 DisplaySystem Runtime 管理器
实现 Unity Multi Display 激活代码
实现独立窗口创建代码
实现 VR SDK 接入
实现 Camera 绑定逻辑
实现 Canvas 绑定逻辑
实现 RenderTexture 绑定逻辑
实现 RuntimeConfigUI 显示配置界面
修改 FrameworkEntry 生命周期
扩展 Core/Config/FrameworkConfig.cs
进入 UISystem Runtime Implementation
进入 InputSystem Runtime Implementation
进入 NetworkSystem Implementation
```

---

# 二十八、P3.8 输出物

P3.8 应输出：

```text
Documentation/22_DisplaySystemFoundation.md
Documentation/00_ByFramework_Current_Context.md 更新
02_Documentation/02_Roadmap.md 更新
03_Documentation/03_Todo.md 更新
Documentation/04_Changelog.md 更新
```

不要求输出 Runtime 代码。

---

# 二十九、阶段关闭条件

P3.8 关闭条件：

```text
DisplaySystem 职责明确
DisplaySystem 边界明确
Unity Multi Display 与独立窗口模式关系明确
驾驶舱多屏目标明确
VR 可选启用 / 可关闭 / 可现场配置的规则明确
DisplayProfile 模型明确
DisplayTarget 模型明确
现场配置方式明确
DisplaySystem 与 UISystem / RuntimeConfigUI / FrameworkConfig / BuildProfileSystem / LicenseSystem 关系明确
禁止事项写入文档
Current_Context 更新完成
Roadmap / Todo / Changelog 同步完成
```

满足以上条件后，P3.8 可关闭。

下一阶段建议进入：

```text
P3.9 ResourceSystem Foundation
```

---

# 三十、最终结论

P3.8 DisplaySystem Foundation 是设计冻结阶段。

它只确定：

```text
DisplaySystem 是什么
DisplaySystem 管什么
DisplaySystem 不管什么
多屏如何定位
独立窗口如何定位
驾驶舱显示如何定位
VR 显示如何定位
现场配置如何定位
后续 Runtime 实现应遵守什么边界
```

不得在本阶段进入具体 Runtime 实现。

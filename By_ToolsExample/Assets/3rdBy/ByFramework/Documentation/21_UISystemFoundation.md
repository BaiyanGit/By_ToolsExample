# P3.7 UISystem Foundation 设计冻结

> 阶段性质：设计冻结阶段  
> 不是 Runtime 实现阶段  
> 不创建 UISystem Runtime 脚本  
> 不实现 UIWindow / UIPanel / UILayer / UIThemeManager 运行时代码  

---

# 一、阶段目标

P3.7 的目标是冻结 UISystem Foundation 的基础设计，为后续 UISystem Runtime Implementation 提供明确边界。

本阶段只允许完成：

```text
UISystem 职责冻结
UISystem 边界冻结
UIThemeSystem 关系冻结
UI 基础对象模型冻结
UI 层级模型冻结
UI 生命周期方向冻结
UI 资源加载关系冻结
UI 配置关系冻结
UI 与 PlatformServiceRegistry 接入方向冻结
```

本阶段不进入具体运行时代码实现。

---

# 二、架构位置

UISystem 属于：

```text
Platform/UISystem
```

UIThemeSystem 属于 UISystem 的主题子系统。

可在目录上独立，但架构归属仍然是：

```text
Platform/UISystem/UIThemeSystem
```

UISystem 不属于 Core。

UISystem 不属于 FeatureModule。

---

# 三、UISystem 定位

UISystem 是 ByFramework 的统一 UI 管理平台。

它负责为项目提供统一的 UI 打开、关闭、层级、生命周期、焦点、导航、主题和资源接入能力。

UISystem 面向：

```text
菜单界面
设置界面
弹窗界面
HUD
Loading
运行时配置界面
培训系统界面
管理系统界面
大屏界面
驾驶舱界面
```

---

# 四、UISystem 职责

UISystem 负责：

```text
UI 窗口管理
UI 面板管理
UI 弹窗管理
UI 页面管理
UI 层级管理
UI 生命周期管理
UI 焦点管理
UI 导航管理
UI 打开关闭流程
UI 资源加载接入
UI 主题接入
UI 配置接入
UI 事件桥接
```

---

# 五、UISystem 不负责

UISystem 不负责：

```text
登录校验
训练流程
车辆控制
网络业务
数据库业务
设备业务
仿真同步
业务规则
业务状态机
具体项目业务流程
```

UI 可以触发业务事件，但不能承载业务规则。

例如：

```text
LoginPanel 可以点击“登录”按钮

但账号校验逻辑不属于 UISystem
```

---

# 六、核心对象模型

UISystem 后续 Runtime 可围绕以下对象设计。

本阶段只冻结概念，不实现代码。

```text
UIRoot
UILayer
UIView
UIWindow
UIPanel
UIDialog
UIPage
UIWidget
UIContext
UIOpenRequest
UICloseRequest
UIResult
```

---

## 6.1 UIRoot

UIRoot 是 UI 系统根节点。

职责方向：

```text
承载全部 UI 层级
绑定 Canvas
绑定 EventSystem
管理基础 UI 层
作为 UI 生命周期根节点
```

---

## 6.2 UILayer

UILayer 是 UI 层级。

建议基础层级：

```text
BackgroundLayer
MainLayer
PanelLayer
PopupLayer
ToastLayer
LoadingLayer
SystemLayer
DebugLayer
```

层级职责：

```text
BackgroundLayer    背景类 UI
MainLayer          主界面
PanelLayer         普通面板
PopupLayer         弹窗
ToastLayer         提示信息
LoadingLayer       加载遮罩
SystemLayer        系统级界面
DebugLayer         调试界面
```

---

## 6.3 UIView

UIView 是所有 UI 视图的基础概念。

职责方向：

```text
管理 UI 生命周期
接收 UIContext
响应打开关闭
响应主题变更
响应语言变更
```

---

## 6.4 UIWindow

UIWindow 表示窗口型 UI。

适用于：

```text
设置窗口
配置窗口
管理窗口
工具窗口
```

---

## 6.5 UIPanel

UIPanel 表示面板型 UI。

适用于：

```text
主菜单面板
功能面板
信息面板
状态面板
```

---

## 6.6 UIDialog

UIDialog 表示弹窗型 UI。

适用于：

```text
确认弹窗
提示弹窗
错误弹窗
输入弹窗
```

---

## 6.7 UIPage

UIPage 表示页面型 UI。

适用于：

```text
登录页
主页面
设置页
训练页
管理页
```

---

## 6.8 UIWidget

UIWidget 表示局部组件。

适用于：

```text
按钮组
状态条
进度条
列表项
设备状态小组件
```

---

# 七、UI 生命周期方向

UISystem 后续 Runtime 可采用统一生命周期。

建议生命周期方向：

```text
OnCreate
OnOpen
OnShow
OnRefresh
OnHide
OnClose
OnDestroy
```

说明：

```text
OnCreate      实例创建
OnOpen        打开时调用
OnShow        显示时调用
OnRefresh     数据刷新
OnHide        隐藏时调用
OnClose       关闭时调用
OnDestroy     销毁时调用
```

本阶段只冻结生命周期方向，不实现接口。

---

# 八、UI 打开关闭模型

后续 Runtime 可采用请求模型：

```text
UIOpenRequest
UICloseRequest
```

UIOpenRequest 可包含：

```text
UIKey
UILayer
UIContext
是否独占
是否入栈
是否缓存
是否异步加载
```

UICloseRequest 可包含：

```text
UIKey
是否销毁
是否返回结果
关闭原因
```

---

# 九、UIKey

UIKey 是 UI 资源和 UI 类型的统一标识。

UIKey 不应绑定具体路径。

它应由 ResourceSystem 解析到具体资源。

示例：

```text
ui.main_menu
ui.settings
ui.runtime_config
ui.loading
ui.error_dialog
```

---

# 十、UISystem 与 ResourceSystem 的关系

UISystem 不直接关心 UI 资源来自哪里。

正确关系：

```text
UISystem
↓
ResourceSystem
↓
Resources / Addressables / AssetBundle / LocalFile
```

UISystem 只通过 ResourceKey / UIKey 请求资源。

ResourceSystem 负责加载。

AssetBundleProvider 负责加载 AB 包。

Downloader 负责下载远程资源。

---

# 十一、UISystem 与 LocalizationSystem 的关系

UISystem 不直接管理语言包。

正确关系：

```text
UISystem
↓
LocalizationSystem
↓
LanguagePack
```

UISystem 内的文本应通过 LocalizationKey 获取。

UI 需要支持语言切换刷新。

---

# 十二、UISystem 与 UIThemeSystem 的关系

UIThemeSystem 是 UISystem 的主题子系统。

它负责：

```text
主题配置
颜色配置
字体配置
图标配置
样式配置
运行时主题切换
主题资源加载
主题应用
```

UISystem 负责：

```text
管理 UI 对象
通知 UI 响应主题变化
```

UIThemeSystem 不负责业务逻辑。

---

# 十三、UIThemeSystem 基础模型

UIThemeSystem 后续可包含：

```text
ThemeProfile
ColorPalette
FontProfile
IconSet
StyleProfile
SkinProfile
ThemeBinding
```

支持主题：

```text
默认主题
浅色主题
深色主题
客户主题
大屏主题
驾驶舱主题
培训系统主题
管理系统主题
```

---

# 十四、运行时主题切换

后续 Runtime 应支持：

```text
加载主题
切换主题
保存当前主题
通知已打开 UI 刷新
缺失主题回退
```

配置来源：

```text
FrameworkConfig
StreamingAssets 外部配置
PersistentDataPath 现场配置
RuntimeConfigUI
```

---

# 十五、UISystem 与 DisplaySystem 的关系

DisplaySystem 负责显示目标。

UISystem 负责 UI 内容。

正确关系：

```text
DisplaySystem
↓
DisplayTarget / Canvas / Camera
↓
UISystem
↓
UIRoot / UILayer / UIView
```

多屏或 VR 场景下，UISystem 不直接检测显示设备。

DisplaySystem 提供显示上下文。

UISystem 按 DisplayContext 创建或绑定 UI 根节点。

---

# 十六、UISystem 与 FrameworkConfig 的关系

UISystem 可以读取 FrameworkConfig 中的 UI 配置。

配置方向包括：

```text
默认主题
默认语言
默认 UI 根节点配置
默认层级配置
是否启用 RuntimeConfigUI
UI 资源加载策略
UI 缓存策略
UI 分辨率适配策略
```

但 UISystem 不应直接修改 FrameworkConfig 的底层存储逻辑。

---

# 十七、RuntimeConfigUI

RuntimeConfigUI 属于 FrameworkConfig 的可视化运行时配置界面。

但它需要使用 UISystem 展示。

关系：

```text
FrameworkConfig
↓
RuntimeConfigUI 数据与配置逻辑
↓
UISystem 展示界面
```

RuntimeConfigUI 可以作为 UISystem 的一个使用者，而不是 UISystem 的核心职责。

现场人员可通过 RuntimeConfigUI 配置：

```text
服务器 IP
端口
语言
主题
显示模式
资源路径
AssetBundle 本地路径
日志等级
串口号
```

---

# 十八、UISystem 与 PlatformServiceRegistry 的关系

UISystem 后续可注册为 Platform 服务。

接口方向：

```text
IUIService
IThemeService
```

注册方向：

```text
PlatformServiceRegistry.Register<IUIService>(uiService)
PlatformServiceRegistry.Register<IThemeService>(themeService)
```

获取方向：

```text
PlatformServiceRegistry.Get<IUIService>()
PlatformServiceRegistry.TryGet<IThemeService>(out themeService)
```

本阶段不实现注册代码，只冻结接入方向。

---

# 十九、UISystem 与 EventManager 的关系

UISystem 可以通过 EventManager 发布 UI 生命周期事件。

例如：

```text
UIOpened
UIClosed
UIFocusChanged
ThemeChanged
LanguageChanged
```

但 EventManager 只是事件桥接，不承载 UI 管理逻辑。

---

# 二十、UISystem 与 FeatureModule 的关系

FeatureModule 可以使用 UISystem。

例如：

```text
TrainingSystem 打开训练面板
VehicleSimulation 打开车辆状态面板
DeviceIntegration 打开设备状态窗口
ScenarioSystem 打开场景选择界面
```

但 FeatureModule 不允许把业务规则塞入 UISystem。

正确关系：

```text
FeatureModule
↓
IUIService
↓
UISystem
```

---

# 二十一、建议目录结构

本阶段仅冻结目录方向，不要求创建代码文件。

```text
Assets/ByFramework/Platform/UISystem
├─ Runtime
│  ├─ Core
│  │  ├─ UIRoot
│  │  ├─ UILayer
│  │  ├─ UIView
│  │  └─ UIContext
│  │
│  ├─ Window
│  │  ├─ UIWindow
│  │  └─ UIWindowRequest
│  │
│  ├─ Panel
│  │  ├─ UIPanel
│  │  └─ UIPanelRequest
│  │
│  ├─ Dialog
│  │  ├─ UIDialog
│  │  └─ UIDialogRequest
│  │
│  ├─ Theme
│  │  ├─ ThemeProfile
│  │  ├─ ColorPalette
│  │  ├─ FontProfile
│  │  ├─ IconSet
│  │  └─ StyleProfile
│  │
│  └─ Service
│     ├─ IUIService
│     └─ IThemeService
│
└─ Editor
   ├─ UISystemConfigWindow
   ├─ UIThemeConfigWindow
   └─ UIResourceBindingWindow
```

---

# 二十二、Editor 工具方向

UISystem 后续可以提供 EditorWindow 配置工具。

工具方向：

```text
UISystemConfigWindow
UIThemeConfigWindow
UIResourceBindingWindow
UILayerConfigWindow
```

用于配置：

```text
默认 UI Root
默认层级
默认主题
UIKey 与 ResourceKey 映射
主题资源
字体资源
图标资源
UI 缓存策略
```

所有 Editor UI 遵守中文化规范。

例如：

```text
菜单：ByFramework/平台/UISystem 配置
按钮：保存配置
提示：请选择默认主题
日志：[UISystem] 配置保存完成
```

---

# 二十三、配置文件方向

UISystem 配置应支持：

```text
Editor 配置
StreamingAssets 外部配置
PersistentDataPath 现场配置
```

示例配置：

```text
StreamingAssets/ByFramework/Config/ui_config.json
StreamingAssets/ByFramework/Config/theme_config.json
```

现场修改后的配置：

```text
PersistentDataPath/ByFramework/Config/ui_config.json
PersistentDataPath/ByFramework/Config/theme_config.json
```

优先级遵守 FrameworkConfig 总规则：

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

# 二十四、禁止事项

P3.7 阶段禁止：

```text
实现 UISystem Runtime 管理器
实现 UIWindow 打开关闭逻辑
实现 UIPanel 打开关闭逻辑
实现 UI 栈
实现 UI 缓存池
实现 UIThemeManager
实现 ResourceSystem 加载调用
实现实际 UI MonoBehaviour
修改 FrameworkEntry 生命周期
扩展 Core/Config/FrameworkConfig.cs
进入 InputSystem Runtime Implementation
进入 NetworkSystem Implementation
```

---

# 二十五、P3.7 输出物

P3.7 应输出：

```text
Documentation/21_UISystemFoundation.md
Documentation/00_ByFramework_Current_Context.md 更新
02_Documentation/02_Roadmap.md 更新
03_Documentation/03_Todo.md 更新
Documentation/04_Changelog.md 更新
```

不要求输出 Runtime 代码。

---

# 二十六、阶段关闭条件

P3.7 关闭条件：

```text
UISystem 职责明确
UISystem 边界明确
UIThemeSystem 关系明确
UI 基础对象模型明确
UI 生命周期方向明确
UI 与 ResourceSystem / LocalizationSystem / DisplaySystem / FrameworkConfig 关系明确
UI 与 PlatformServiceRegistry 接入方向明确
禁止事项写入文档
Current_Context 更新完成
Roadmap / Todo / Changelog 同步完成
```

满足以上条件后，P3.7 可关闭。

下一阶段由项目维护者决定是否进入：

```text
P3.8 DisplaySystem Foundation
```

或：

```text
P3.x ResourceSystem / SaveSystem 后续冻结阶段
```

---

# 二十七、最终结论

P3.7 UISystem Foundation 是设计冻结阶段。

它只确定：

```text
UISystem 是什么
UISystem 管什么
UISystem 不管什么
UISystem 如何接入其它 Platform 模块
UIThemeSystem 如何归属
后续 Runtime 实现应遵守什么边界
```

不得在本阶段进入具体 Runtime 实现。

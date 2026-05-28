# Recorder Settings 修改记录

## v0.3.0-beta - Stability Baseline

本版本完成 Recorder SDK 稳定性基线：

- 状态机稳定
- 错误码稳定
- 事件流稳定
- SessionHistory
- configId 配置体系
- RecorderConfigRegistry
- SDK 使用示例
- 总体验收文档

### 控制台布局重设计：2026-05-27

- 为什么这么做：原控制台首页和工具页偏表单化，按钮横向铺满后层级不清晰；封版前需要统一成更适合 Editor 工具的状态总览、卡片分区和检查表格。
- 做了什么：`RecorderSdkHubWindow` 改为纯文字入口和窗口标题；首页重构为 SDK 状态、运行环境、路径信息、快速开始、配置管理、最近使用和常用目录卡片；UI 工具页改为 UI 创建和布局管理两大区；环境检查和打包发布页改为表格检查；打包页仅保留检查和复制建议导出清单，不再提供自动导出入口；SDK API 说明改为分类加卡片显示。
- 验证结果：只修改 EditorWindow 显示逻辑和文档记录；未修改 Runtime 录制流程、RecorderState、ConfigRegistry、FFmpegCommandBuilder、SessionHistory 或音量增强逻辑。

### 首页信息架构收口：2026-05-27

- 为什么这么做：首页不应该是平铺 Dashboard，而应该像 SDK 工作台一样按“环境是否正常、下一步做什么、辅助工具、最近状态、最近更新”的顺序阅读。
- 做了什么：将 SDK 状态、运行环境和路径信息合并为唯一主区域“当前环境状态”；快速开始改为主操作区；常用目录改为轻量工具按钮；最近状态和最近更新改为纵向信息段；左侧菜单降低按钮高度和视觉权重，改为细条选中提示。
- 验证结果：只修改 `RecorderSdkHubWindow` 首页和左侧导航显示逻辑；未修改 Runtime 录制逻辑、SDK API 分类、文档系统、ScrollView 逻辑或打包逻辑。

### SDK API 文档显示与项目结构编码修复：2026-05-27

- 为什么这么做：SDK API 页面右侧内容被过度拆成卡片后出现标题重复、阅读割裂；部分左侧分类仍直接使用英文类型名；`ProjectStructure.md` 文件内容存在编码异常，导致文档中心显示乱码。
- 做了什么：SDK API 左侧分类统一中文化，将 `RecorderResult`、`RecorderErrorCode`、`SessionHistory` 改为“返回结果”“错误码”“会话历史”；右侧内容改为单页开发者文档滚动显示，并自动移除正文开头与页面标题重复的行；重写 `ProjectStructure.md` 为标准 UTF-8 中文内容。
- 验证结果：只修改 Editor 控制台显示逻辑和文档内容；未修改 Runtime 录制逻辑、配置 JSON 或录制核心。

### API 参数页 IMGUI Assertion 修复：2026-05-27

- 为什么这么做：`FFmpegParameterGuideContent.DrawPagination()` 在控制台内嵌场景中触发 Unity IMGUI Assertion，旧分页按钮和缓存 GUI 对象生命周期不适合复用到 `RecorderSdkHubWindow`。
- 做了什么：移除 FFmpeg 参数和视频编码参数内容中的分页状态、分页按钮和分页渲染；右侧内容改为分类列表驱动的完整 ScrollView；移除自建 `Texture2D` 背景缓存和 `HideAndDontSave` 纹理，按钮继续使用纯字符串绘制。
- 验证结果：静态检查已确认 Editor 目录不再包含 `DrawPagination`、`PAGE_SIZE`、`_pageIndex`、`MakeTexture`、`Texture2D` 或 `HideAndDontSave`；未修改 Runtime 录制逻辑。

### SDK API 右侧滚动修复：2026-05-27

- 为什么这么做：SDK API 页面左侧分类使用 Toggle 时，已选中项每帧都会返回 true，导致右侧滚动位置被持续重置；同时页面外层 ScrollView 与右侧内容 ScrollView 嵌套，容易造成滚动响应不稳定。
- 做了什么：SDK API 页面从控制台右侧全局 ScrollView 中独立出来；右侧内容只使用 `_sdkApiContentScroll` 一个主 ScrollView；左侧分类只有在真正切换到新分类时才重置右侧滚动位置。
- 验证结果：只修改 `RecorderSdkHubWindow` 的 EditorWindow 显示逻辑；未修改 Runtime 录制逻辑。

### 控制台内容简化与 SDK API 文档：2026-05-26

- 为什么这么做：控制台已经成为主入口，旧阶段文档、完整日志和完整验证记录内容过长，默认展示会增加阅读和加载负担；同时旧快捷菜单继续暴露会让入口分散。
- 做了什么：新增 `RecorderSdkAcceptanceSummary.md`、`RecorderSdkChangeLogSummary.md`、`RecorderSdkValidationSummary.md`；旧 Phase1/2/3/Stability 验收文档复制归档到 `Documentation/Archive/` 并在控制台归入 `历史文档`；控制台默认显示摘要版更新日志和验证摘要；新增 `SDK API` 页，集中说明 `CrossPlatformScreenRecorder`、常用函数、属性、事件、`RecorderResult`、错误码、SessionHistory 和配置系统；打包发布页支持从 `RecorderSdkHubWindow.cs` 自动反推 SDK 根目录，也支持重新扫描和手动选择并保存到 EditorPrefs；隐藏旧 UI 创建、布局导出/应用和 API 参数快捷菜单，功能保留在控制台中。
- 验证结果：只修改 Editor 控制台、文档和打包工具显示/路径逻辑；未修改 Runtime 录制流程、RecorderState、FFmpegCommandBuilder、ConfigRegistry、SessionHistory 或音量增强逻辑。

### SDK 目录移动收口：2026-05-26

- 为什么这么做：RecorderSdk 已移动到 `Assets/Demos/RecorderSdk`，旧路径会影响控制台 fallback、布局模板路径和文档说明。
- 做了什么：将控制台 fallback 根目录、Layout Profile 默认目录、README、ProjectStructure、UnityPackageValidation、RuntimeValidation 和封版验证报告中的正式 SDK 路径同步为 `Assets/Demos/RecorderSdk`。
- 验证结果：只修改 Editor 路径常量和文档文本；未修改 Runtime 录制逻辑。

### 控制台封版收口修正：2026-05-26

- 为什么这么做：验证记录和更新日志长文本需要稳定滚到底部，SDK API 页需要满足开发者接入文档要求，打包工具根目录识别需要优先脚本路径而不是固定目录。
- 做了什么：`RecorderSdkHubWindow` 的文档显示统一改为显式 `CalcHeight` 的稳定滚动 Viewer，文档中心、更新日志、验证记录和 SDK API 共用该显示方式；SDK API 页扩展为快速接入流程、核心组件、生命周期 API、状态机、事件系统、RecorderResult、RecorderErrorCode、SessionHistory、配置系统、音量增强、示例代码等分类；首页调整为状态总览和快捷入口，完整 UI 操作集中到 UI 工具页；文档中心默认保留摘要文档和稳定性总体验收，Phase1/2/3 只保留在 Archive；打包工具根目录识别顺序改为脚本路径、EditorPrefs、手动选择、最后 fallback。
- 验证结果：只修改 Editor 控制台、文档和打包工具显示/路径逻辑；未修改 Runtime 录制核心、RecorderState、ConfigRegistry、FFmpegCommandBuilder、SessionHistory 或音量增强逻辑。

### SDK 控制台：2026-05-26

- 为什么这么做：Recorder SDK 的 Editor 工具逐渐增多，继续增加菜单会让入口分散；统一控制台能把 UI 创建、布局工具、API 参数、环境检查、打包发布和文档查看集中起来。
- 做了什么：新增 `RecorderSdkHubWindow`，菜单入口为 `ByTools/🔴Recorder SDK控制台`；首页显示版本、SDK 路径、StreamingAssets 路径、FFmpeg 状态、WASAPI DLL 状态和平台；UI 工具页复用现有创建/导出/应用逻辑；API 页复用 FFmpeg 和视频编码参数窗口；环境检查页检测 FFmpeg、wasapi、alimiter、WASAPI DLL、Configs、currentConfigId 和输出目录；打包发布页提供必要文件检查和 unitypackage 导出；文档中心、更新日志和验证记录页读取 Documentation 下的 Markdown。
- 验证结果：新增窗口仅位于 Editor 目录；保留原有快捷菜单；未修改 Runtime 录制逻辑、ConfigRegistry、FFmpegCommandBuilder 或 SessionHistory。

### API 参数内置控制台：2026-05-26

- 为什么这么做：FFmpeg 参数和视频编码参数原本分别在独立窗口中查看，入口仍然分散；控制台需要能直接浏览这两类参数，同时避免维护两份说明内容。
- 做了什么：把 FFmpeg 参数窗口内容抽成 `FFmpegParameterGuideContent`，把视频编码参数窗口内容抽成 `VideoEncodingGuideContent`；独立窗口继续保留，只负责承载共享 Content；`RecorderSdkHubWindow` 的 `API 参数` 页增加 `FFmpeg 参数` 和 `视频编码参数` 两个切换页签，并直接渲染同一套 Content。
- 验证结果：现有 `ByTools/🔴Recorder SDK/API/🎬 FFmpeg 参数` 和 `ByTools/🔴Recorder SDK/API/🎬 视频编码 参数` 快捷菜单保留；Runtime 录制逻辑、ConfigRegistry、FFmpegCommandBuilder、SessionHistory 和配置 JSON 未修改。

### 控制台文档与 API 缓存优化：2026-05-26

- 为什么这么做：文档中心和 API 参数页内容较多，打开控制台时一次性读取文档或初始化全部参数会造成卡顿；左侧分类和按钮也需要更符合中文使用习惯。
- 做了什么：文档中心分类改为 `使用说明`、`项目结构`、`验收文档`、`验证记录`、`发布说明`、`更新日志`；文档按钮改为中文显示名并在右侧显示原文件名；文档内容改为点击后读取并按修改时间缓存，增加 `刷新文档缓存`；API 参数页改为首次进入对应页签时才初始化，FFmpeg 参数和视频编码参数独立缓存，增加 `刷新API缓存`；参数筛选结果按搜索和筛选条件缓存，普通重绘不再重复 LINQ 排序整套数据。
- 验证结果：`RecorderSdkHubWindow.OnEnable()` 不再读取所有文档或初始化全部 API 内容；`OnGUI()` 不直接执行 `File.ReadAllText`；Runtime 录制逻辑未修改。

### 控制台更新日志卡死修复：2026-05-26

- 问题：点击 `Recorder SDK控制台 -> 更新日志` 时可能卡死 Unity，风险来自更新日志内容较大并在绘制阶段反复读取或刷新。
- 做了什么：更新日志页改为独立懒加载缓存，切换到更新日志页时读取一次，之后 `OnGUI` 只显示缓存内容；增加 `刷新更新日志`、`用默认编辑器打开`、`打开所在目录`；文档读取最多加载 50000 字符用于显示，超过后自动截断并提示打开文件查看完整内容；验证记录页改为按路径缓存，避免多个文档在同一页交替触发重复读取。
- 验证结果：更新日志不会在每帧重新读取文件；文件不存在或读取失败时显示提示文本，不抛异常；Runtime 录制逻辑未修改。

### 打包发布与 API 分页收口：2026-05-26

- 问题：打包发布页直接导出整个 `Assets/StreamingAssets/FFmpegTools` 时会把 `Videos` 目录中的实际录制文件一起打进 unitypackage；API 参数页在参数较多时仍可能一次性绘制过多卡片。
- 做了什么：unitypackage 导出路径改为按项收集，只导出 Recorder SDK 必要目录、`Configs`、可选 `FFmpegApp` 和可选 `Videos/README.md`，不再导出 `Videos` 实际录制文件；打包发布页增加 `包含 FFmpegApp`、`包含 WASAPI 原生 DLL`、`包含 Documentation`、`包含 Demo 场景`、`包含 Videos 空目录说明` Toggle；正式包检查始终校验 `WASAPILoopbackRecorder.dll`；补齐实际 SDK 文档目录中的 `ProjectStructure.md`；FFmpeg 参数页和视频编码参数页增加分页，每页最多绘制 20 项。
- 验证结果：打包路径不再包含 `Assets/StreamingAssets/FFmpegTools/Videos` 整个目录；Runtime 录制逻辑未修改。

### 控制台文档滚动与验证页标题修复：2026-05-26

- 问题：更新日志内容很长时正文区域无法正常下拉；验证记录页同时显示两个文档标题块，造成标题重复和页面撑高。
- 做了什么：新增统一 `DrawMarkdownViewer` 文档查看器，标题和按钮固定，正文使用独立 ScrollView；更新日志页改为使用独立滚动位置；验证记录页改为 `运行时验证记录 / UnityPackage 验证记录` 两个 Tab，只显示当前选中文档；验证记录正文也使用独立 ScrollView。
- 验证结果：更新日志和验证记录正文不会撑破窗口；文档中心和 API 参数页逻辑未修改；Runtime 录制逻辑未修改。

### 收口修正：2026-05-26

- 为什么这么做：RecorderSDK(4) 交付包需要统一正式路径、Editor 菜单命名和项目结构说明，避免导入后用户看到旧路径或旧菜单分组。
- 做了什么：修正 UI Layout Profile 路径为当前 SDK 路径下的 `UI/Settings/LayoutProfiles`；统一 Editor 菜单根路径为 `ByTools/🔴Recorder SDK/` 并按功能分组；删除旧版屏幕录制兼容菜单入口；新增 `ProjectStructure.md`；将 README、Validation、Changelog 中的旧版路径文本统一到当时的正式路径。
- 验证结果：`rg` 未再检出旧版 Demos 路径、带图标的 Recorder SDK 菜单根路径或旧版屏幕录制菜单名；确认当前工程内存在 `WASAPILoopbackRecorder.dll`，正式导出 unitypackage 时需要包含。

## v0.3.0-beta Runtime Validation

### 任务计划：2026-05-25 16:20

- 问题：Recorder SDK 后续 UI 创建方式需要从运行时动态创建切换为 Editor 工具一次性生成到场景中；屏幕录制 UI 和设置中心 UI 职责不同，需要两套独立 Builder，并支持手动调整后导出/应用布局模板。
- 计划：保留并正规化 `ByTools/🔴Recorder SDK/UI创建/屏幕录制`；新增 `ByTools/🔴Recorder SDK/UI创建/设置中心`；新增屏幕录制/设置中心布局导出和应用菜单；新增通用布局 Profile 工具导出 RectTransform、字体、颜色、间距、Panel/按钮/ScrollView 尺寸等信息；将 `UIRecorderParamsSettings` 的运行时 UI 创建改为 Legacy 默认关闭，只做已有对象绑定和 Warning；README 同步新的菜单与工作流。
- 风险点：设置中心 Builder 先生成可绑定、可手动调整的基础设置界面骨架，真实项目美术布局仍建议在 Unity Editor 中微调后导出布局模板；新增脚本未生成 `.meta`，Unity 打开后会自动生成。

### 修改记录：2026-05-25 16:50

- 为什么这么做：UI 必须真实存在于场景 Hierarchy 中，才能被手动调整、保存和复用；运行时动态创建整套 UI 会让布局不可控，也让业务脚本职责过重。
- 做了什么：`UIRecorderParamsSettings.RuntimeUI` 的按钮/弹窗/说明窗口动态补建改为 `createLegacyRuntimeUI=false` 默认关闭；保留 Legacy 代码但默认只查找已有对象并给 Warning；`RecorderDemoUIBuilderEditor` 菜单正规化为 `创建屏幕录制UI` 并保留旧菜单兼容；新增 `RecorderSettingsUIBuilderEditor` 负责创建设置中心 Canvas、顶部配置区、左侧菜单、右侧参数页、底部说明、弹窗、RecorderManager、CrossPlatformScreenRecorder 和 UIRecorderParamsSettings 字段绑定；新增 `RecorderUILayoutProfileUtilityEditor` 和 `RecorderUILayoutMenuEditor`，支持导出/应用 `DemoUILayoutProfile.json` 与 `SettingsUILayoutProfile.json`；README 补充 UI Builder 和布局模板工作流。
- 验证结果：CreateXXXUI、导出/应用布局菜单均位于 Editor 目录；Runtime 脚本默认不再 Instantiate 或 new GameObject 补建设置 UI；RecorderState、ConfigRegistry、SessionHistory、FFmpegCommandBuilder、音量增强和配置 JSON 系统未改动。

### 任务计划：2026-05-25 15:10

- 问题：旧场景已移动到 `Demo/LegacyScenes/`，后续只维护 `录制器_屏幕录制` 和 `录制器_设置中心`；当前 `录制器_设置中心` 在切换使用方式为 `视频推流` 后，右侧参数页面没有互斥刷新，音频/视频旧页面可能与推流页面叠加。
- 计划：只修改 `录制器_设置中心` 使用的 UI 脚本，不改 LegacyScenes；在 `UIRecorderParamsSettings` 中增加 `HideAllModePanels()` 和 `ShowPanelByUseMode()`，切换使用方式时先隐藏右侧同级参数页面，再按本地录制/视频推流显示目标页面；增加兜底互斥校验，防止场景旧 Toggle 事件重新打开多个页面；同步 README/ChangeLog 说明。
- 风险点：当前命令行不直接编辑新场景复杂 YAML，只通过脚本约束场景中已绑定的 `goStreamSettingsRoot` 所在页面列表；如果后续新增新的使用方式，需要扩展 `ShowPanelByUseMode()` 的分支。

### 任务修正：2026-05-25 15:45

- 问题：正确逻辑不是按使用方式强制切换到某个右侧页面，而是使用方式控制左侧菜单项可见性；右侧页面必须由当前左侧菜单选中项决定。`视频设置` 和 `音频参数` 在 `存储本地` 与 `视频推流` 两种模式下都应存在，只有 `推流设置` 在 `存储本地` 时隐藏。
- 计划：改成菜单驱动页面刷新：自动读取左侧 Toggle 与右侧页面的绑定关系；切换使用方式时先刷新菜单可见性，再保持仍可见的当前菜单，只有当前菜单被隐藏时才切到第一个可见菜单；最后按选中菜单唯一显示对应页面。去掉使用方式切换时强制打开推流页面的逻辑。

### 修改记录：2026-05-25 15:55

- 为什么这么做：左侧分类菜单和右侧参数页是一组 Tab 关系，使用方式只应该影响 `推流设置` 这个 Tab 是否可见，不能直接决定右侧显示哪个页面。
- 做了什么：`UIRecorderParamsSettings` 自动从左侧 Toggle 的持久化 `SetActive` 事件中建立菜单和页面绑定；新增 `RefreshVisibleMenuItems()`、`SelectFirstVisibleMenuIfCurrentHidden()`、`RefreshVisiblePanels()`，按“菜单可见性 -> 当前选中合法性 -> 页面唯一显示”的顺序刷新；`RefreshUseModeUI()` 不再强制打开 `goStreamSettingsRoot`；`LateUpdate()` 继续兜底处理多个页面同时显示或隐藏菜单页面仍显示的异常状态。
- 验证结果：代码层逻辑满足 `存储本地` 隐藏 `推流设置` 菜单与页面，`视频推流` 显示 `视频设置`、`音频参数`、`推流设置`；切换使用方式时当前菜单仍有效则保持选中，当前菜单失效才切到第一个可见菜单；本次未修改 `Demo/LegacyScenes/`。

### 修改记录：2026-05-25 15:25

- 为什么这么做：`录制器_设置中心` 的右侧参数区实际是同一父节点下的多个页面，切换到视频推流时如果不先关闭音频/视频页面，就会出现推流页面和旧页面重叠。
- 做了什么：`UIRecorderParamsSettings` 新增 `HideAllModePanels()`、`ShowPanelByUseMode()`、`GetActiveLocalModePanelRoot()`、`GetFirstLocalModePanelRoot()` 和 `EnforceModePanelExclusivity()`；`RefreshUseModeUI()` 改为先隐藏右侧同级页面，再按使用方式显示本地页面或推流页面；`LateUpdate()` 做兜底互斥校验，防止场景旧 Toggle 的 `SetActive` 事件重新打开多个页面；README 补充设置中心使用方式切换互斥说明。
- 验证结果：代码层已保证 `存储本地` 与 `视频推流` 切换时右侧参数页面互斥；本次没有修改 `Demo/LegacyScenes/` 下旧场景；真实 Unity UI 仍需在 Editor 中打开 `录制器_设置中心` 点击切换验证。

### 任务计划：2026-05-25 14:45

- 问题：`录制器_屏幕录制` Demo UI 中仍有英文按钮和状态字段文案，运行后会显示 `Enable Audio`、`Start Recording`、`Current State` 等英文，不符合当前中文场景交付要求。
- 计划：只修改 Demo UI 默认显示文案，不改 Recorder 核心逻辑和 JSON 字段；将 `RecorderDemoBasicController` 的运行时状态刷新文本改为中文；将 `RecorderDemoBasicUIBuilderEditor` 生成 UI 时写入的按钮、Toggle、Dropdown 和状态文本改为中文；README 和 UnityPackageValidation 中涉及 Demo UI 的说明同步使用中文按钮名称。
- 风险点：如果场景中已经手动保存过旧英文 Text，需要在 Unity 中执行 `ByTools/🔴Recorder SDK/UI创建/屏幕录制` 或手动修改场景 Text 后保存；本次不会直接重写复杂 Scene YAML。

### 修改记录：2026-05-25 14:55

- 为什么这么做：Demo 场景面向使用者演示 SDK 最小接入方式，按钮和状态字段应保持中文一致，JSON 字段名则继续保留英文以保证配置兼容。
- 做了什么：`RecorderDemoBasicController` 的状态刷新改为 `当前状态`、`最近结果`、`当前配置ID`、`输出路径`、`最近错误码`、`会话数量`；`RecorderDemoBasicUIBuilderEditor` 生成的按钮、Toggle、Dropdown、状态文本和 Hierarchy 对象名改为中文；README 与 RecorderSdkUnityPackageValidation 的 Demo UI 描述改为中文按钮名称。
- 验证结果：rg 检查 Controller、Editor 生成工具、README、UnityPackageValidation 和 `录制器_屏幕录制.unity` 中已无指定英文 UI 文案；`0 dB` 保持不翻译；针对本次改动文件的 `git diff --check` 通过。

### 任务计划：2026-05-25 14:10

- 问题：Unity 新版本中 `LegacyRuntime.ttf` 才是有效内置 UI 字体，运行时动态创建 Text 若使用旧字体名会报错；同时屏幕录制 Demo Controller 把 UI 创建和业务绑定混在运行时脚本里，导致 Play 后动态生成 UI，不符合 Demo 场景应预先布好 UI 的要求。
- 计划：统一把动态 Text 字体改为 `LegacyRuntime.ttf`；重写 `RecorderDemoBasicController`，默认只绑定场景中已有 UI 并在缺失时给 Warning，不再运行时创建 UI；新增 `RecorderDemoBasicUIBuilderEditor` 到 Editor 目录，通过 `ByTools/🔴Recorder SDK/UI创建/屏幕录制` 一次性在当前场景创建 Canvas、EventSystem、基础按钮、状态文本和简单配置控件，并自动挂载与绑定 Controller；README 补充 Demo UI 是场景预制 UI 以及重建菜单。
- 风险点：当前命令行环境不能可靠打开 Unity Editor 保存场景，因此不直接手写复杂 Scene YAML；用户可在 Unity 内执行菜单生成并保存 `录制器_屏幕录制.unity`，生成后 UI 会真实存在于 Hierarchy。

### 修改记录：2026-05-25 14:35

- 为什么这么做：字体错误会阻塞运行时 UI 显示，Demo Controller 运行时创建 UI 会让场景职责不清晰，也不利于用户在 Hierarchy 中查看和调整演示界面。
- 做了什么：`UIRecorderParamsSettings.RuntimeUI` 的默认字体只读取 `LegacyRuntime.ttf`；`RecorderDemoBasicController` 删除运行时 Canvas/Button/Text/Dropdown/Toggle/Slider 创建逻辑，改为只绑定 public UI 字段、订阅 Recorder 事件、调用 Start/Stop、保存简单配置并刷新状态；新增 `RecorderDemoBasicUIBuilderEditor`，提供 `ByTools/🔴Recorder SDK/UI创建/屏幕录制` 菜单，在当前场景一次性创建并绑定 Demo UI；README 和 UnityPackageValidation 补充 Demo UI 是场景预制 UI，以及可用菜单重建。
- 验证结果：rg 检查 SDK 中已统一使用 `LegacyRuntime.ttf` 运行时字体加载；`RecorderDemoBasicController` 中已无 `CreateButton`、`CreateText`、`CreateDropdown`、`CreateToggle`、`CreateSlider`、`new GameObject` UI 创建逻辑；针对本次改动文件的 `git diff --check` 通过。

### 任务计划：2026-05-25 13:20

- 问题：Recorder SDK 目录和场景名称已由人工整理为新的结构与中文命名，文档、验证说明和少量脚本中仍残留旧英文场景名、旧路径和旧综合场景引用，后续导出 unitypackage 时容易误导使用者。
- 计划：不移动目录、不重新生成 `.meta`，只基于当前磁盘结构修正硬编码场景名与 README、UnityPackageValidation、RuntimeValidation、StabilityAcceptance、Changelog 中的旧引用；保留 `Assets/StreamingAssets/FFmpegTools/` 不变；后续新增 Demo 场景统一使用 `录制器_XXX` 命名。
- 风险点：SDK 路径如果后续再次移动，需要重新做一轮路径文本收口；本次不主动移动文件以避免破坏 GUID。

### 修改记录：2026-05-25 13:35

- 为什么这么做：新目录和中文场景名已经成为后续交付基准，旧英文场景名和旧 `Scripts/Core`、`Scripts/UISettings` 路径会让 README、验证文档和 unitypackage 导入说明与实际工程不一致。
- 做了什么：将全局快捷键加载场景和设置面板关闭卸载场景改为 `录制器_设置中心`；README 的 Scene Guide、关键脚本路径、验收文档路径、SmokeTest/UsageExample/Validation 文档路径统一到 `RecorderSdk` 新目录；RecorderSdkUnityPackageValidation 改为包含 `录制器_屏幕录制`、`录制器_设置中心`、`录制器_旧版综合场景`，并标注旧 `录制视频Recorder`、`录制视频Init` 为 Legacy；RuntimeValidation 的默认测试场景改为 `录制器_屏幕录制`；StabilityAcceptance 的 Core 目录说明改为 `RecorderSdk/Core`。
- 验证结果：rg 检查文档和脚本中已无 `Recorder_Demo_Basic`、`Recorder_Settings`、`Recorder_AllInOne_Legacy`、旧 `Scripts/Core`、旧 `Scripts/UISettings` 主引用；保留的 `录制视频Recorder.unity`、`录制视频Init.unity` 均在 Legacy 说明中；没有移动目录，没有重新生成 `.meta`。

### 任务计划：2026-05-25 12:30

- 问题：Recorder 示例当前把完整配置 UI 和 SDK 使用演示混在同一个场景中，新用户不知道应该打开哪个场景，也不利于开发者学习最小集成方式。
- 计划：保留原场景不破坏，新建 `录制器_设置中心.unity` 用作完整配置管理工具；新增 `录制器_屏幕录制.unity` 只包含 RecorderManager、基础按钮、状态文本和少量配置入口；新增 `RecorderDemoBasicController.cs` 订阅 Recorder 事件并调用 Start/Stop Async、展示结果、打开输出目录、清空历史、切换 configId；README 和 UnityPackageValidation 增加 Scene Guide，并重新生成 unitypackage 包含两个新场景。
- 风险点：Unity Editor 当前仍受许可证环境影响，场景创建以文本序列化方式完成，最终打开/按钮点击验证需要在许可证恢复后进入 Editor 实测。

### 修改记录：2026-05-25 12:55

- 做了什么：新增 `RecorderDemoBasicController.cs`；新增 `录制器_屏幕录制.unity`，场景内包含 `RecorderManager` 并挂载 `CrossPlatformScreenRecorder` 与 `RecorderDemoBasicController`，运行时自动创建基础按钮、状态文本、ConfigId 下拉、音频与音量增益入口；复制原完整 UI 场景为 `录制器_设置中心.unity`；复制旧一体化场景为 `录制器_旧版综合场景.unity`，同时保留原 `录制视频Recorder.unity` 不破坏已有引用；README 新增 Scene Guide；RecorderSdkUnityPackageValidation 更新导入后优先打开 Demo 场景；重新生成 `RecorderSDK_v0.3.0-beta.unitypackage`。
- 验证结果：新场景文件与 meta 已生成；unitypackage 构建路径清单包含 `录制器_屏幕录制.unity`、`录制器_设置中心.unity`、`录制器_旧版综合场景.unity` 和 `RecorderDemoBasicController.cs`；`git diff --check` 通过，仅有 CRLF 提示。Unity Editor 打开、Console、Start/Stop 实录仍需许可证环境恢复后验证。

### 任务计划：2026-05-25 11:10

- 问题：用户电脑播放声音正常或很大，但录制输出音量偏小，需要在 Recorder SDK 中支持录制音量增益，并且本地录制、合并和推流音频链路都要覆盖，同时避免增益过大导致爆音。
- 计划：新增 enableAudioGain、audioGainDb、audioLimiterEnabled 配置字段；UISettings 增加音量增强开关和增益输入/滑动字段读取写入；FFmpegCommandBuilder 统一构建音频 filter，启用时追加 volume 和 alimiter；合并阶段优先加音频滤镜；Validator 限制增益范围并给 warning；新增 AudioFilterInvalid 错误码；SessionHistory message 记录本次音量增强参数；默认 JSON、说明 JSON、README、SmokeTest 同步更新。
- 风险点：当前场景 UI 可能还未实际摆放对应控件，因此代码先支持 public 字段挂载，未挂载时保留配置默认值；alimiter 支持通过启动前 `ffmpeg -filters` 检测，检测失败会降级为只使用 volume。

### 修改记录：2026-05-25 11:45

- 做了什么：RecorderParamsConfig 新增 `enableAudioGain`、`audioGainDb`、`audioLimiterEnabled`；FFmpegCommandBuilder 新增统一音频滤镜构建，本地 Linux、Windows 推流、Linux 推流和合并阶段会在有音频时拼接 `-af "volume=XdB"`，开启限幅器时追加 `alimiter=limit=0.95`；RecorderConfigValidator 增加 -20 到 20dB 修正、无音频输入 warning、alimiter 支持检测与降级；新增 `AudioFilterInvalid` warning 错误码；Session/SessionInfo/SessionHistory 记录音量增强状态；UIRecorderParamsSettings 增加可挂载开关、输入框、滑动条和限幅器开关；默认模板 JSON、说明 JSON、README、SmokeTest 同步更新。
- 影响范围：Core 下的 RecorderConfigValidator、FFmpegCommandBuilder、CrossPlatformScreenRecorder、RecorderSession/Info、RecorderErrorCode、RecorderSdkSmokeTest；UISettings 下的 RecorderParamsConfig、UIRecorderParamsSettings、Descriptions、ConfigFiles；StreamingAssets/FFmpegTools/Configs 下模板和说明 JSON；README。
- 验证结果：Configs 下 JSON 使用 UTF-8 解析通过；rg 检查 `enableAudioGain`、`audioGainDb`、`audioLimiterEnabled`、`AudioFilterInvalid`、`volume=6dB`、`alimiter=limit=0.95` 均已覆盖；SmokeTest 已新增命令构建检查。Unity Editor 编译仍需等许可证环境恢复后实测。

### 收口检查：2026-05-25 12:05

- 做了什么：RecorderConfigMigrator 补齐旧 JSON 缺少 `enableAudioGain`、`audioGainDb`、`audioLimiterEnabled` 时的默认值；FFmpegCommandBuilder 的音频滤镜构建支持传入已有滤镜，并合并成同一个 `-af` 表达式；SmokeTest 增加旧配置默认值和已有滤镜合并检查；README 补充旧场景未挂新增 UI 控件时可通过 JSON 启用音量增益。
- 检查结论：新增 UI 字段绑定、显示、读取、修改颜色和交互状态均有 null 保护；当前命令构建入口每条音频链路只调用一次 `BuildAudioFilterArgs`，不会生成多个 `-af`。

### 任务计划：2026-05-25 10:30

- 问题：v0.3.0-beta 已封版并完成运行验证文档，但还没有整理成真正 Unity 可导入的 `.unitypackage` 验证包；README/ReleaseNotes 中的旧字段需要明确标记为 Legacy 兼容，RuntimeValidation 中的本机路径需要标注为验证环境记录。
- 计划：确认 FFmpegApp 是否内置 FFmpeg；新增 RecorderSdkUnityPackageValidation.md；更新 README、ReleaseNotes、RuntimeValidation 说明；使用 UnityPackage 标准结构生成 RecorderSDK_v0.3.0-beta.unitypackage；校验包内 pathname 清单、FFmpeg 是否包含、旧路径说明和空白格式。
- 风险点：当前 Unity License 仍阻塞 Editor 导出菜单，因此使用 UnityPackage 的 tar.gz 标准结构在命令行生成包；生成后通过解包检查 pathname/asset/meta 验证可导入结构，不做核心逻辑修改。

### 修改记录：2026-05-25 10:30

- 为什么这么做：交付给新 Unity 项目验证时需要真实 `.unitypackage`，并且必须让导入者明确包内包含哪些资源、是否内置 FFmpeg、如何运行 SmokeTest，以及旧字段只是 Legacy 兼容。
- 做了什么：新增 RecorderSdkUnityPackageValidation.md；README 增加 UnityPackage 验证文档索引并把 `configName`、`fileName`、`Creat_` 标记为 Legacy 兼容；ReleaseNotes 的旧配置说明改为 Legacy 兼容说明；RuntimeValidation 标注其中本机路径只属于本次验证环境；使用 UnityPackage 标准 `guid/asset + asset.meta + pathname` 结构生成 `RecorderSDK_v0.3.0-beta.unitypackage`；解包检查 pathname 清单。
- 影响范围：README、ReleaseNotes、RecorderSdkRuntimeValidation、RecorderSdkUnityPackageValidation、RecorderSDK_v0.3.0-beta.unitypackage；没有修改核心逻辑。
- 验证结果：生成 `RecorderSDK_v0.3.0-beta.unitypackage`，大小 135933679 bytes，包含 80 个 pathname；解包确认包含 RecorderSdk 场景、`RecorderSdkUnityPackageValidation.md`、`Assets/StreamingAssets/FFmpegTools/FFmpegApp/ffmpeg.exe` 和 `Assets/StreamingAssets/FFmpegTools/FFmpegApp/ffmpeg`；rg 检查 README/ReleaseNotes 中旧字段均有 Legacy 说明；git diff --check 通过，仅有 CRLF 提示。

### 任务计划：2026-05-25 09:30

- 问题：上一轮只完成运行验证文档和 FFmpeg 控制组，还没有完成真实 Unity SDK 运行验证；当前阻塞包括 Unity License Client IPC 超时、FFmpegApp 目录缺少 ffmpeg.exe、Windows 推流音频直接假设 wasapi 可用、README 仍有旧配置命名，以及说明 JSON 中可能残留本机路径文本。
- 计划：复制系统 FFmpeg 到 StreamingAssets/FFmpegTools/FFmpegApp；重试 Unity BatchMode 并记录是否进入脚本编译；为 Windows streamIncludeAudio 增加启动前 wasapi 设备能力检测，缺失时返回明确错误码/Warning，不等命令运行后失败；修正 README 配置命名；检查并清理 OptionDescriptions/RecorderOptionDescriptions 本机路径；更新 RecorderSdkRuntimeValidation.md。
- 风险点：Unity License 修复可能需要 Unity Hub 交互登录，当前命令行环境未必能完成；真实 Editor 场景运行和手动录制如果无法启动 Unity，只能继续标记为环境阻塞。

### 修改记录：2026-05-25 09:30

- 为什么这么做：真实 SDK 运行验证需要先清掉可控阻塞；FFmpegApp 缺失会导致默认配置必失败，Windows 推流音频如果不提前检测 wasapi，会让错误延迟到 FFmpeg 命令运行后才暴露。
- 做了什么：复制系统 FFmpeg 到 `Assets/StreamingAssets/FFmpegTools/FFmpegApp/ffmpeg.exe`；重跑 BatchMode 和无代理 BatchMode；检查 Unity Licensing Client 日志；RecorderConfigValidator 增加 `ffmpeg -devices` 的 wasapi 启动前检测，缺失时返回 `AudioStartFailed`；README 模板/用户配置命名改成 `template_*` 和 `create_*`；README 与 OptionDescriptions 补充 Windows 音频依赖 wasapi 及后续 dshow 可配置输入源说明；更新 RecorderSdkRuntimeValidation.md。
- 影响范围：RecorderConfigValidator、README、OptionDescriptions、RecorderSdkRuntimeValidation、FFmpegApp 目录；没有做 BackendFactory，没有拆架构。
- 验证结果：`FFmpegApp/ffmpeg.exe -version` 与 `-devices` 可执行；当前 FFmpeg 仍无 wasapi；BatchMode 与无代理 BatchMode 仍停在 License Client IPC 超时，未进入脚本编译；GUI 打开 Unity Editor 的审批请求超时，未能启动交互式 Editor；rg 检查 README/OptionDescriptions/RecorderOptionDescriptions 已无旧 Template_/Creat_ 命名和本机路径残留；git diff --check 通过，仅有 CRLF 提示。

### 任务计划：2026-05-25 00:00

- 问题：v0.3.0-beta 已完成封版文档，但还需要进入真实 Unity + FFmpeg 环境验证，确认本地录制、WebM、连续 Start/Stop、长时间录制、推流、音频、配置切换、状态机、事件、SessionHistory、Merge、压力测试等能力是否真正稳定可运行。
- 计划：先检查当前 Windows 环境中的 Unity、FFmpeg、配置文件、日志和输出目录；能直接执行的 FFmpeg/配置/文档级验证先执行；Unity 内运行、30 分钟长录制、RTMP 推流、WASAPI 设备插拔、多显示器和 Linux 项目按环境可用性记录结果；新增 RecorderSdkRuntimeValidation.md 记录测试环境、成功项、失败项、已知问题、崩溃日志、性能数据、CPU/内存、长时间录制结果。
- 风险点：当前对话环境可能无法启动 Unity Editor 或缺少 License；如果 FFmpeg 可执行文件不存在或没有 RTMP 服务、多显示器、Linux 桌面环境，则对应实机项只能记录为未执行/环境阻塞，不伪造通过结果。

### 修改记录：2026-05-25 00:00

- 为什么这么做：实机验证阶段要先确认当前机器是否具备真实 Unity + FFmpeg 测试条件，避免把环境阻塞误判为 SDK 通过或失败。
- 做了什么：检查 Unity 版本、BatchMode 日志、FFmpeg 版本、FFmpeg 设备能力、当前 UseWin 指针和模板配置；执行 FFmpeg MP4/WebM 编码落盘控制组、gdigrab 桌面采集尝试、WASAPI 输入尝试、RTMP 无服务端失败路径、FFmpeg 进程生命周期和 10 秒编码性能采样；新增 RecorderSdkRuntimeValidation.md；README 增加运行时验证文档索引。
- 影响范围：新增运行时验证文档和 README 链接；没有修改核心逻辑。
- 验证结果：FFmpeg MP4/WebM 编码控制组通过；RTMP 无服务端失败路径可复现；gdigrab 在当前会话失败 error 5；当前 FFmpeg 不支持 wasapi；Unity BatchMode 仍因 License Client IPC 超时未进入脚本编译；FFmpegApp 目录未发现 ffmpeg.exe，默认模板直接运行会缺 FFmpeg。

## 记录规则

每次修改录制参数设置相关功能前，先在本文档新增“任务计划”。

每完成一个具体问题后，补充以下内容：

- 为什么这么做
- 做了什么
- 影响范围
- 验证结果

## 待执行任务计划模板

### 任务计划：YYYY-MM-DD HH:mm

- 问题：
- 计划：
- 风险点：

### 修改记录：YYYY-MM-DD HH:mm

- 为什么这么做：
- 做了什么：
- 影响范围：
- 验证结果：

### 任务计划：2026-05-20 14:40

- 问题：视频推流模式目前只保留了 useMode 选项，缺少推流地址配置、推流参数保存、录制核心推流输出逻辑和操作前校验。
- 计划：新增推流地址配置字段；UI 支持“视频推流”模式下显示推流地址并隐藏本地保存路径；保存、另存为、使用前校验推流地址；录制核心在推流模式下直接输出到推流地址，暂按通用 RTMP/FLV 推流实现。
- 风险点：如果实际要支持的协议不是 RTMP/FLV，需要再按目标平台协议补充 ffmpeg muxer 和参数；场景中若还没有推流地址输入框，需要把新增 public 字段绑定到 UI，或按自动节点名查找。

### 修改记录：2026-05-20 14:40

- 为什么这么做：推流是实时链路，不能复用本地录制结束后的音视频合并流程，所以需要独立的推流地址配置和 ffmpeg 直播输出参数。
- 做了什么：新增 streamUrl 配置字段；UI 增加推流地址输入框引用、自动查找和兜底创建；推流模式下隐藏本地保存路径并显示推流地址；保存、另存为、使用前校验 rtmp:// 或 rtmps:// 地址；CrossPlatformScreenRecorder 在推流模式下输出 FLV/RTMP，停止时跳过本地文件等待和合并。
- 影响范围：RecorderParamsConfig、RecordConfigProvider、UIRecorderParamsSettings、CrossPlatformScreenRecorder、默认 JSON 配置和选项说明 JSON。
- 验证结果：git diff --check 通过，仅有 CRLF 提示；dotnet build 仍为当前工程环境的“生成失败，0 个警告，0 个错误”，未给出可定位脚本错误。

### 任务计划：2026-05-20 14:52

- 问题：推流需要明确保证沿用当前屏幕选择、不阻塞主程序，并在 Win/Linux 以及国产硬件环境下尽量稳定流畅。
- 计划：确认推流参数使用当前选中显示器；为 ffmpeg 进程设置较低优先级，避免抢占主程序；补充推流专用编码参数，采用低延迟、固定 GOP、队列缓冲和 CPU 友好的默认值；增加推流码率/帧率/缩放的安全兜底。
- 风险点：不同国产 CPU/GPU/系统发行版的硬件编码器差异很大，默认先走 libx264 CPU 编码保证兼容；后续如果要硬编码，需要按实际设备提供 h264_qsv、h264_vaapi、h264_nvenc、厂商 SDK 或系统 FFmpeg 编译能力做可选策略。

### 修改记录：2026-05-20 14:52

- 为什么这么做：推流必须继续复用当前显示器选择，同时 ffmpeg 实时编码不能抢占 Unity 主程序资源；国产硬件环境差异较大，默认参数应优先保证跨平台兼容和稳定。
- 做了什么：推流仍使用当前配置的 displayIndex/displayName 选择目标屏幕；ffmpeg 进程支持设置优先级，推流时降为 BelowNormal；推流参数增加 thread_queue_size、rtbufsize、zerolatency、固定 GOP、maxrate、bufsize、threads 0、flush_packets；对推流帧率、码率、编码预设增加安全兜底，过慢预设自动回落到 veryfast。
- 影响范围：CrossPlatformScreenRecorder 推流参数生成与 FFmpegProcessRunner 进程启动。
- 验证结果：git diff --check 通过，仅有 CRLF 提示；dotnet build 仍为当前工程环境的“生成失败，0 个警告，0 个错误”。

### 任务计划：2026-05-20 17:08

- 问题：本地录制和视频推流共用过多 UI 参数，MP4/WebM 选项混杂，推流性能参数没有独立暴露，Windows/Linux 推流是否带系统音频也缺少配置。
- 计划：新增推流专用 JSON 字段；按使用方式隐藏本地/推流专属控件；按 MP4/WebM 动态过滤视频和音频编码器选项；推流模式显示码率、GOP、缓冲区、低延迟、重连、是否带系统音频等控件；同步更新选项说明 JSON 和 README。
- 风险点：Windows 实时系统音频依赖 ffmpeg 是否编译 WASAPI 输入，若目标机 FFmpeg 不支持，需要后续接入可选音频设备列表或虚拟声卡方案。

### 修改记录：2026-05-20 17:08

- 为什么这么做：本地录制和 RTMP 推流的参数目标不同，继续混用会让使用者看到无效选项，也容易把 MP4/WebM 编码器选错。
- 做了什么：新增 streamVideoBitrate、streamGop、streamBufferSize、streamLowLatency、streamAutoReconnect、streamReconnectCount、streamReconnectIntervalMs、streamIncludeAudio 字段；推流模式下显示推流专用控件并隐藏本地保存、格式、CRF、合并等本地专属控件；MP4/WebM 下动态过滤视频和音频编码器；推流命令改用推流专用码率、GOP、缓冲区和低延迟参数；Windows/Linux 可通过 streamIncludeAudio 控制是否推系统音频；更新 OptionDescriptions.json 与 README。
- 影响范围：RecorderParamsConfig、UIRecorderParamsSettings、UIRecorderParamsSettings.RuntimeUI、UIRecorderParamsSettings.ConfigFiles、UIRecorderParamsSettings.Descriptions、CrossPlatformScreenRecorder、默认/用户 JSON 配置、README。
- 验证结果：git diff --check 通过，仅有 CRLF 提示；dotnet build 仍为当前工程环境的“生成失败，0 个警告，0 个错误”。

### 任务计划：2026-05-22 16:30

- 问题：用户已重新调整 UI 布局，左上角状态列表的详细信息需要补全；视频推流模式下文件前缀属于本地文件命名参数，应隐藏；首次运行时无关参数未及时按使用方式和格式刷新。
- 计划：新增状态文本自动收集与刷新逻辑，优先复用场景中 TextStatus 节点；推流模式隐藏文件前缀所在行；在初始化、配置应用、参数变更和使用方式切换后统一刷新动态 UI 状态；同步更新 README。
- 风险点：状态列表具体文本节点可能未绑定到脚本，自动查找会按节点名 TextStatus 处理，若场景命名不一致需要在 Inspector 手动绑定状态文本数组。

### 修改记录：2026-05-22 16:30

- 为什么这么做：状态列表需要跟随当前配置和参数变化实时说明，否则用户只能从散落控件判断当前状态；文件前缀只影响本地文件命名，推流时显示会造成误解；初始化后统一刷新可以避免首次打开时看到无关参数。
- 做了什么：新增 txtStatusDetails 状态文本数组并自动收集 TextStatus 节点；新增动态 UI 刷新入口，初始化、配置应用、参数变更后统一刷新显隐、可编辑状态、修改颜色和状态列表，并避免程序化刷新触发重复变更事件；推流模式隐藏文件前缀所在对象；更新选项说明 JSON 与 README。
- 影响范围：UIRecorderParamsSettings、RecorderOptionDescriptions.json、OptionDescriptions.json、README。
- 验证结果：git diff --check 通过，仅有 CRLF 提示；dotnet build Assembly-CSharp.csproj --no-restore 仍为当前工程环境的“生成失败，0 个警告，0 个错误”。

### 任务计划：2026-05-22 17:05

- 问题：左上角状态列表应按固定行显示具体参数值，而不是显示综合描述；当前使用状态需要同步控制图标颜色，正在使用为绿色，未使用为灰色。
- 计划：状态列表改为固定顺序：当前状态、模板名称、输出格式、分辨率比例、帧率、编码器；自动从 TextStatus 所在行查找 Icon 图形并刷新第一行图标颜色；保留手动绑定入口，方便场景布局继续调整。
- 风险点：用户第 7 条需求尚未补全，本次只实现已明确的 1-6 项；如果状态行顺序在场景中与文字描述不同，需要在 Inspector 中调整 txtStatusDetails 数组顺序。

### 修改记录：2026-05-22 17:05

- 为什么这么做：状态列表是固定参数值展示区，综合状态描述会与场景中左侧标题不匹配；使用状态应该只反映当前配置文件是否已被使用，不应该混入是否修改等其它信息。
- 做了什么：状态列表改为依次显示正在使用/未使用、配置名称、webm/mp4/视频流、分辨率比例、帧率、编码器；自动查找同级存在 Icon 的 TextStatus，减少误抓其它文本；新增当前使用状态图标颜色配置，正在使用为绿色，未使用为灰色；README 同步更新。
- 影响范围：UIRecorderParamsSettings、README。
- 验证结果：git diff --check 通过，仅有 CRLF 提示；dotnet build Assembly-CSharp.csproj --no-restore 仍为当前工程环境的“生成失败，0 个警告，0 个错误”。

### 任务计划：2026-05-22 17:25

- 问题：整体检查发现几个容易出问题的点：运行时脚本直接 using UnityEditor 会影响 Player 编译；控件显隐方法默认控件一定有父节点，场景布局变化时可能空引用；状态列表自动收集没有排序，场景层级调整后行顺序可能错；录制端读取旧配置时没有补全推流默认字段。
- 计划：把 UnityEditor 引用限制在编辑器环境；控件显隐增加父节点兜底；状态文本自动收集后按屏幕位置从上到下排序；RecordConfigProvider 同步补全推流默认值和空字段；更新 README 与验证记录。
- 风险点：状态列表如果后续不是纵向排列，仅靠坐标排序可能不符合视觉顺序，此时仍可用 txtStatusDetails 手动绑定顺序兜底。

### 修改记录：2026-05-22 17:25

- 为什么这么做：这些点都不是新功能，但会在打包、场景重排、旧配置推流或异常刷新时放大成难查的问题。
- 做了什么：UnityEditor 引用改为只在编辑器编译；状态文本自动收集后按界面坐标排序；控件显隐在没有父节点时回退隐藏自身；动态刷新用 try/finally 恢复 _isRefreshingUI；RecordConfigProvider 补全旧配置的推流默认字段；README 更新对应说明。
- 影响范围：UIRecorderParamsSettings、RecordConfigProvider、README。
- 验证结果：git diff --check 通过，仅有 CRLF 提示；dotnet build Assembly-CSharp.csproj --no-restore 仍为当前工程环境的“生成失败，0 个警告，0 个错误”。

### 任务计划：2026-05-23 09:45

- 问题：左上角状态列表仍未完全按 8 个固定字段显示，缺少音频编码和当前配置文件最后保存日期；编码器应显示当前配置中“视频编码器选项”的值，不能在推流时写死。
- 计划：状态列表固定输出 8 行：当前状态、模板名称、输出格式、分辨率比例、帧率、视频编码器、音频编码器、文件最后更新时间；时间从当前配置文件的 LastWriteTime 读取；README 与记录同步更新。
- 风险点：如果 txtStatusDetails 只绑定了 6 个文本，只会显示前 6 项；需要场景中补足 8 个 TextStatus 或手动绑定数组。

### 修改记录：2026-05-23 09:45

- 为什么这么做：状态列表每一行都有明确标题，必须只显示对应参数值；音频编码和文件更新时间也属于用户判断当前配置的重要信息。
- 做了什么：状态列表补齐 8 行输出；视频编码器和音频编码器均按当前配置格式读取对应字段；推流输出格式显示为“视频流”；最后更新时间读取当前配置文件 LastWriteTime；README 同步说明。
- 影响范围：UIRecorderParamsSettings、README。
- 验证结果：git diff --check 通过，仅有 CRLF 提示；dotnet build Assembly-CSharp.csproj --no-restore 仍为当前工程环境的“生成失败，0 个警告，0 个错误”。

### 任务计划：2026-05-23 10:05

- 问题：左上角状态列表的输出格式显示逻辑不准确，推流模式应显示“推流”，本地模式应显示 UI 中“视频格式”参数当前选项。
- 计划：调整状态列表输出格式获取逻辑：检测 useMode 为推流时返回“推流”，否则优先读取 drVideoFormat 当前显示值，读取不到时再回退到配置字段；同步更新 README 和修改记录。
- 风险点：如果场景未绑定 drVideoFormat，则仍会使用配置字段兜底显示 webm/mp4。

### 修改记录：2026-05-23 10:05

- 为什么这么做：输出格式这一行应表达当前使用方式和视频格式参数值；“视频流”不是 UI 约定显示值，容易与“视频推流”模式混淆。
- 做了什么：推流模式输出格式改为“推流”；本地存储模式优先读取 drVideoFormat 当前选项值，未绑定时回退到 outputAsWebm 字段；README 同步说明。
- 影响范围：UIRecorderParamsSettings、README。
- 验证结果：git diff --check 通过，仅有 CRLF 提示；dotnet build Assembly-CSharp.csproj --no-restore 仍为当前工程环境的“生成失败，0 个警告，0 个错误”。

### 任务计划：2026-05-23 10:20

- 问题：左上角状态列表是固定 8 项，用 public Text[] 依赖数组顺序，Inspector 中不直观也容易挂错。
- 计划：把 txtStatusDetails 数组拆成 8 个明确 Text 字段：当前状态、模板名称、输出格式、分辨率比例、帧率、视频编码器、音频编码器、最后更新时间；刷新逻辑按字段直接赋值；保留按名称自动兜底查找，README 同步说明。
- 风险点：场景需要重新把 8 个 Text 字段逐个挂到脚本上；自动兜底只能按 TextStatus 的界面顺序尝试填充，最终仍建议手动绑定。

### 修改记录：2026-05-23 10:20

- 为什么这么做：状态项固定且语义明确，使用数组会把正确性藏在 Inspector 顺序里，不如每一项独立字段直观。
- 做了什么：移除 public Text[] txtStatusDetails；新增 8 个具名状态 Text 字段；刷新逻辑按固定字段写入；自动兜底查找仍保留但只用于未手动绑定时补位；README 同步更新字段绑定方式。
- 影响范围：UIRecorderParamsSettings、README。
- 验证结果：git diff --check 通过，仅有 CRLF 提示；代码中已无 txtStatusDetails 字段引用；dotnet build Assembly-CSharp.csproj --no-restore 仍为当前工程环境的“生成失败，0 个警告，0 个错误”。

### 任务计划：2026-05-23 10:35

- 问题：状态列表已改为 Inspector 逐字段挂载，之前用于 TextStatus 自动查找、排序和兜底补位的代码已经没有必要，继续保留会增加理解成本并可能误绑定错误节点。
- 计划：删除状态列表自动查找与排序逻辑，只保留 8 个具名 Text 字段刷新和当前状态 Icon 颜色刷新；README 更新为必须手动绑定字段。
- 风险点：删除自动兜底后，未在 Inspector 绑定的状态文本不会自动显示，需要场景中显式挂载。

### 修改记录：2026-05-23 10:35

- 为什么这么做：状态列表现在已经是显式字段挂载，继续保留 TextStatus 查找、排序和补位会让真实绑定来源变得不明确。
- 做了什么：删除 NeedAutoResolveStatusTexts、ApplyAutoResolvedStatusTexts、FindStatusIconGraphic、SortStatusTexts 以及 ResolveOptionalUIReferences 中的状态文本查找逻辑；状态文本只通过 8 个 public Text 字段刷新；README 改为必须手动绑定。
- 影响范围：UIRecorderParamsSettings、README。
- 验证结果：git diff --check 通过，仅有 CRLF 提示；代码中已无 TextStatus 自动查找、排序、补位和 txtStatusDetails 残留；dotnet build Assembly-CSharp.csproj --no-restore 仍为当前工程环境的“生成失败，0 个警告，0 个错误”。

### 任务计划：2026-05-23 10:55

- 问题：进一步检查发现仍有部分旧兜底逻辑和未使用代码：UI 设置类中保留了未被调用的当前配置读取方法；推流地址与推流参数控件已经在场景中布局并通过字段挂载，运行时代码创建这些控件会让 UI 来源不清晰。
- 计划：删除 UIRecorderParamsSettings.ConfigFiles 中未使用的 LoadCurrentRecordConfig；删除运行时创建推流地址和推流参数控件的 EnsureStreamUrlInput/EnsureStreamSettingsControls 及其仅服务于这两者的创建方法；删除按节点名自动查找 UI 引用的 ResolveOptionalUIReferences；初始化阶段不再调用这些兜底方法；README 说明推流相关控件需要在场景中挂载。
- 风险点：删除运行时创建和自动查找后，场景中对应字段必须绑定，否则相关 UI 不会自动生成或自动补引用。

### 修改记录：2026-05-23 10:55

- 为什么这么做：当前 UI 已转为场景中显式布局和字段挂载，保留运行时创建、按名字查找和未使用读取方法会让代码路径重复且难判断真实来源。
- 做了什么：删除 UIRecorderParamsSettings.ConfigFiles 中未使用的 LoadCurrentRecordConfig；删除 InitUI 中推流地址和推流参数运行时创建调用；删除 EnsureStreamUrlInput、EnsureStreamSettingsControls、CreateLabeledDropdown、CreateToggle；删除 ResolveOptionalUIReferences 及其调用；状态刷新改为直接写 8 个具名字段，不再创建临时列表。
- 影响范围：UIRecorderParamsSettings、UIRecorderParamsSettings.RuntimeUI、UIRecorderParamsSettings.ConfigFiles、README。
- 验证结果：git diff --check 通过，仅有 CRLF 提示；代码中无上述删除方法残留，只有历史变更记录文本提及；dotnet build Assembly-CSharp.csproj --no-restore 仍为当前工程环境的“生成失败，0 个警告，0 个错误”。
### 任务计划：2026-05-23 11:06

- 问题：前面提出的 Recorder 第一阶段系统改造尚未完成，当前仍存在 GetPlatformDefaultFFmpegPath 中 Application.Quit、旧 Creat_ 新建规则、configName/fileName 双身份、Start/Stop async void 主流程、缺少 RecorderState 状态机，以及 FFmpeg 参数拼接仍集中在 CrossPlatformScreenRecorder 中。
- 计划：先修复明确 bug 与 Create_ 前缀；新增 schemaVersion=2、configId、displayName，并把旧 displayName 录屏屏幕字段迁移为 captureDisplayName，保存新版 JSON 时不再写 fileName；UseWinRecordConfig/UseLinuxRecordConfig 优先保存 currentConfigId 并兼容旧 fileName；新增 RecorderResult 和 RecorderState，保留 StartRecording/StopRecording 作为 Unity 按钮入口，新增可 await 的 StartRecordingAsync/StopRecordingAsync；抽出 FFmpegCommand/FFmpegCommandBuilder 承担录制、推流、合并参数构建；同步 README。
- 风险点：这是跨 UI、配置和录制核心的兼容改造，旧 JSON 的 displayName 曾表示屏幕名称，新版 displayName 表示配置显示名，需要在加载旧配置时迁移到 captureDisplayName；场景 Inspector 中旧字段序列化可能需要 Unity 刷新后重新确认。

### 修改记录：2026-05-23 11:06

- 为什么这么做：Recorder 第一阶段改造的目标是先稳定配置身份、开始/停止调用方式、状态流转和 FFmpeg 参数构建边界，避免继续依赖 fileName/configName 双身份和 async void 主流程。
- 做了什么：删除 GetPlatformDefaultFFmpegPath 中的 Application.Quit；新增 RecorderState、RecorderResult、FFmpegCommand、FFmpegCommandBuilder；StartRecording/StopRecording 保留为按钮入口并改为调用 StartRecordingAsync/StopRecordingAsync；FFmpeg 本地录制、推流和合并参数迁移到 FFmpegCommandBuilder；RecorderParamsConfig 新增 schemaVersion、configId、displayName、description、captureDisplayName，并把 fileName/configName 变为运行时兼容字段；UseWinRecordConfig/UseLinuxRecordConfig 新版写 currentConfigId；新建配置 ID 使用 create_ 前缀，旧 Creat_ 继续兼容读取；README 同步说明新版配置结构和 Async API。
- 影响范围：CrossPlatformScreenRecorder、RecorderParamsConfig、RecordConfigProvider、UIRecorderParamsSettings.ConfigFiles、README，以及新增 Core 下的 RecorderState、RecorderResult、FFmpegCommand、FFmpegCommandBuilder。
- 验证结果：rg 检查已无 Application.Quit 和旧 BuildFFmpegCaptureArguments；git diff --check 通过，仅有 CRLF 提示；dotnet build Assembly-CSharp.csproj --no-restore 仍为当前工程环境的“生成失败，0 个警告，0 个错误”。

### 任务计划：2026-05-23 11:34

- 阶段：第 2 阶段 / 共 3 阶段。
- 问题：第一阶段已稳定基础结构，但 CrossPlatformScreenRecorder 仍承担路径生成、配置校验、会话信息和错误事件等职责，后续继续扩展录制/推流/插件能力会让主类再次变大。
- 计划：新增 RecorderPathService 处理默认目录和本次输出路径；新增 RecorderConfigValidator/RecorderValidationResult 统一检查 FFmpeg、输出目录、推流地址、显示器、帧率和缩放比例；新增 RecorderSession/RecorderSessionInfo，将私有 CaptureSession 替换为可转换成只读信息的会话对象；CrossPlatformScreenRecorder 开始/停止流程调用 Validator 和 PathService，减少主流程中散落的校验和路径逻辑。
- 风险点：本阶段仍保持旧 API 和现有行为不变，不引入 IRecorderBackend/IAudioCaptureBackend 等插件接口；这些放入第 3 阶段的事件和扩展层一起处理。

### 修改记录：2026-05-23 11:34

- 阶段：第 2 阶段 / 共 3 阶段。
- 为什么这么做：路径生成、配置校验和会话摘要属于录制基础服务，不应该继续散落在 MonoBehaviour 主流程里，否则后续接上传、日志、插件或 UI 状态都会重复判断。
- 做了什么：新增 RecorderPathService 统一生成默认 FFmpeg 路径、默认视频目录和本次录制会话路径；新增 RecorderConfigValidator/RecorderValidationResult 统一校验 FFmpeg、输出目录、RTMP/RTMPS 推流地址、显示器、帧率和缩放比例；新增 RecorderSession/RecorderSessionInfo，并把 CrossPlatformScreenRecorder 的私有 CaptureSession 替换为 RecorderSession；对外新增 CurrentSession 只读摘要。
- 影响范围：CrossPlatformScreenRecorder 以及新增 Core 下的 RecorderPathService、RecorderConfigValidator、RecorderValidationResult、RecorderSession、RecorderSessionInfo。
- 验证结果：rg 检查主流程已调用 RecorderConfigValidator 和 RecorderPathService；git diff --check 通过，仅有 CRLF 提示；dotnet build Assembly-CSharp.csproj --no-restore 仍为当前工程环境的“生成失败，0 个警告，0 个错误”。

### 任务计划：2026-05-23 11:34

- 阶段：第 3 阶段 / 共 3 阶段。
- 问题：Recorder 已有基本状态机和会话信息，但对外事件仍偏旧，只能拿到 bool 或 string；配置迁移逻辑分散在 UI 和 Provider；插件化接口还没有基础边界。
- 计划：新增统一 RecorderEventArgs；扩展 CrossPlatformScreenRecorder 事件，包括 OnRecorderStateChanged、OnRecorderError、OnRecorderWarning、OnMergeStarted、OnMergeCompleted、OnConfigChanged；新增 RecorderConfigMigrator 集中处理旧 JSON 的字符串字段读取、configId 规整和显示名迁移辅助；新增 IRecorderBackend、IAudioCaptureBackend、IMergeService 接口作为后续插件化边界，暂不替换现有实现；同步 README。
- 风险点：第 3 阶段只建立稳定对外边界和事件，不把现有 WindowsLoopback/FFmpegProcessRunner 立刻替换为接口实现，避免一次性改动过大影响当前功能。

### 修改记录：2026-05-23 11:34

- 阶段：第 3 阶段 / 共 3 阶段。
- 为什么这么做：旧事件只能传 bool 或输出路径，外部系统难以判断会话、状态、错误和后台合并进度；配置迁移逻辑集中后更适合继续演进 JSON schema；插件化接口先建立边界，后续替换后端不会再冲击 UI 和主流程。
- 做了什么：新增 RecorderEventArgs、RecorderConfigMigrator、IRecorderBackend、IAudioCaptureBackend、IMergeService；CrossPlatformScreenRecorder 新增 OnRecorderStateChanged、OnRecorderError、OnRecorderWarning、OnMergeStarted、OnMergeCompleted、OnConfigChanged；README 补充 CurrentSession、统一事件和插件接口说明。
- 影响范围：CrossPlatformScreenRecorder、README，以及新增 Core 下的事件、迁移和接口类。
- 验证结果：rg 检查统一事件、CurrentSession、RecorderConfigMigrator 和插件接口均已存在；Core 下已无 Application.Quit 和旧 BuildFFmpegCaptureArguments；git diff --check 通过，仅有 CRLF 提示；dotnet build Assembly-CSharp.csproj --no-restore 仍为当前工程环境的“生成失败，0 个警告，0 个错误”。
### 任务计划：2026-05-23 12:11

- 问题：上一版代码已支持新版配置字段，但实际 Configs 目录 JSON 仍是旧结构；RecorderParamsConfig.schemaVersion 默认值为 2 会误判无 schemaVersion 的旧 JSON；displayName/captureDisplayName、configName/fileName/currentConfigId 的职责还没有彻底收干净。
- 计划：把 RecorderParamsConfig.schemaVersion 和 RecordConfigReference.schemaVersion 默认值改为 0；迁移逻辑按 schemaVersion <= 1 处理旧 displayName/configName/fileName；保存配置和 UseRecordConfig 时只写新版字段；配置下拉、排序、使用状态、最后更新时间改为基于 configId/displayName 和运行时 fileName 缓存；新增 LastSession/CurrentProcessingSession 解决 Merging 阶段 CurrentSession 为空；升级 StreamingAssets/FFmpegTools/Configs 下全部模板、用户配置和 UseWin/UseLinux JSON 为 schemaVersion=2 的实际文件。
- 风险点：真实 JSON 文件名会统一为 configId.json，旧 Template_/Creat_/Create_ 文件可能需要删除或迁移，必须保证旧引用仍可通过 currentConfigId 或旧 fileName 回退到新版文件。
### 任务计划：2026-05-23 12:11

- 问题：配置 JSON 已开始升级但未补齐 Linux 模板；模板中仍写死本机绝对路径；交付压缩包未包含 Core/UISettings 等核心 C# 脚本，无法独立验证 Recorder 系统代码。
- 计划：补齐 template_linux_low/medium/high.json；将模板配置中的 videoSaveDirectory 和 customFFmpegPath 置空，让运行时回退 Application.streamingAssetsPath 下默认目录；同步检查用户配置中的本机路径并尽量清理；重新生成包含 README、Configs、RecorderSdk/Core、RecorderSdk/UI/Settings、关键入口脚本的 Recorder 系统压缩包。
- 风险点：旧压缩包位置未在工程根目录发现，本次会在 Recorder 示例目录下生成新的 RecorderSystemPackage.zip，便于直接取用。

### 修改记录：2026-05-23 12:11

- 为什么这么做：UseLinuxRecordConfig 已指向 template_linux_medium，缺少 Linux 模板会导致 Linux 平台兜底失败；模板写死本机绝对路径会让其它机器无法直接使用；压缩包缺少核心脚本则无法验证 Recorder 系统逻辑。
- 做了什么：新增 template_linux_low、template_linux_medium、template_linux_high；将 Win 模板和用户配置中的 videoSaveDirectory/customFFmpegPath 清空，由运行时回退到 StreamingAssets/FFmpegTools/Videos 与 FFmpegApp；重新生成 RecorderSystemPackage_20260523_155638.zip，包内包含 README、Configs、RecorderSdk/Core、RecorderSdk/UI/Settings、CrossPlatformScreenRecorder、RecorderParamsConfig、FFmpegCommandBuilder、RecorderState、RecorderResult、RecorderSessionInfo 等核心脚本。
- 影响范围：StreamingAssets/FFmpegTools/Configs、RecorderSystemPackage_20260523_155638.zip。
- 验证结果：Configs 下已存在 win/linux 高中低模板和 UseWin/UseLinux 指针；rg 检查 Configs 中无 fileName、configName、本机 E:/UnityProjectsE 路径残留；压缩包清单确认包含 Core/UISettings 和关键核心脚本；git diff --check 通过，仅有 CRLF 提示；dotnet build Assembly-CSharp.csproj --no-restore 仍为当前工程环境的“生成失败，0 个警告，0 个错误”。
### 任务计划：2026-05-23 16:15

- 问题：当前 Recorder SDK 已有状态、结果和事件结构，但错误来源仍是字符串散落，状态流转缺少统一表，事件触发顺序没有集中约束，Session 只保留当前/最近一次，调用方难以稳定判断“当前发生了什么、为什么失败、处于什么状态”。
- 计划：新增 RecorderErrorCode 枚举并接入 RecorderResult/RecorderEventArgs；新增 RecorderStateTransition 表和状态切换校验；统一 CrossPlatformScreenRecorder 中 Start/Stop/Merge/Error 的错误码和事件创建入口；明确事件触发顺序为状态变化事件优先、再业务事件；新增 Session 历史列表，保留最近若干次会话摘要；同步 README 和验证记录。
- 风险点：本阶段只做 SDK 稳定性闭环，不继续拆架构，不实现 RecorderConfigRegistry、BackendFactory、Profile 导入导出，避免范围扩散。

### 修改记录：2026-05-23 16:15

- 阶段：SDK 稳定性第 1 阶段 / 共 2 阶段。
- 为什么这么做：调用方最需要先拿到稳定错误码、明确状态、固定事件顺序和可追踪会话历史，这比继续拆配置注册表或后端工厂更能解决当前 SDK 使用时“不知道为什么失败”的问题。
- 做了什么：新增 RecorderErrorCode 和 RecorderStateTransition；RecorderResult 与 RecorderEventArgs 改为统一携带 errorCode/errorCodeText；CrossPlatformScreenRecorder 的 Start/Stop/Merge/Error 路径统一使用 RecorderErrorCode；SetState 增加合法流转校验并保持状态事件先触发；新增 maxSessionHistoryCount 与 SessionHistory，录制失败、停止失败、合并完成、推流结束和后台处理失败都会记录会话摘要；调整后台合并计数到主线程完成事件前更新，避免 State=Merging 但 IsMerging=false 的短暂不一致；RecorderConfigValidator 改为校验实际输出目录，模板 videoSaveDirectory 为空时会回退默认 Videos 路径；RecorderValidationResult 增加首个错误码，FFmpeg 缺失、推流地址为空/非法、显示器不存在等错误会返回具体 RecorderErrorCode；README 补充 SDK 稳定性说明。
- 影响范围：CrossPlatformScreenRecorder、RecorderConfigValidator、RecorderValidationResult、RecorderResult、RecorderEventArgs、RecorderErrorCode、RecorderStateTransition、README。
- 验证结果：rg 检查 Core 下已无字符串版 RecorderResult.Failed、PublishError 和 Application.Quit 残留；ValidateForStart 已接入实际输出目录和具体错误码；git diff --check 通过，仅有 CRLF 提示；dotnet build Assembly-CSharp.csproj --no-restore 仍为当前工程环境的“生成失败，0 个警告，0 个错误”。

### 任务计划：2026-05-23 16:40

- 阶段：SDK 稳定性第 1 阶段验收闭环 / 共 2 阶段。
- 问题：第 1 阶段代码方向已完成，但缺少可验收材料：状态流转表、事件触发顺序、最小 Unity 运行测试脚本，以及 dotnet build 无错误失败的原因定位。
- 计划：新增阶段验收文档，列出允许/禁止状态流转和非法流转错误码；补充 Start/Stop/Merge/非法调用等场景的事件顺序、RecorderResult 和 SessionHistory 行为；新增 RecorderSdkSmokeTest MonoBehaviour，覆盖连续 Start、未录制 Stop、正常 Start->Stop、FFmpeg 路径错误、输出目录为空、合并失败模拟、SessionHistory 记录检查；定位 dotnet build 对 Unity 工程不适合的原因，并补充 Unity BatchMode 编译验证建议到 README。
- 风险点：SmokeTest 需要在 Unity Editor 内挂载并执行，真实录制依赖本机 FFmpeg、显示器枚举和平台音频能力；合并失败模拟如果不开放测试钩子，只能通过反射或测试模式最小侵入实现。

### 修改记录：2026-05-23 16:40

- 阶段：SDK 稳定性第 1 阶段验收闭环 / 共 2 阶段。
- 为什么这么做：第 1 阶段需要从“代码已改”变成“可验收、可复查、可在 Unity 内最小运行验证”，否则后续进入事件流和 SessionHistory 深化时缺少基准。
- 做了什么：新增 RecorderSdkPhase1Acceptance.md，整理状态流转表、非法流转错误码、Start/Stop/Merge/非法调用的事件顺序、RecorderResult 和 SessionHistory 行为；新增 RecorderSdkSmokeTest MonoBehaviour，覆盖未录制 Stop、FFmpeg 路径错误、输出目录为空回退、连续 Start、正常 Start->Stop、合并失败模拟和 SessionHistory 记录；修正 Merging->Starting 为合法流转，并让旧合并完成/失败在新录制进行中时不覆盖全局 State；README 补充验收文档、SmokeTest 和 Unity BatchMode 编译方式。
- 影响范围：CrossPlatformScreenRecorder、RecorderStateTransition、RecorderSdkSmokeTest、RecorderSdkPhase1Acceptance、README。
- 验证结果：rg 检查验收文档、SmokeTest、状态流转、BatchMode 说明均已存在；git diff --check 通过，仅有 CRLF 提示；dotnet build Assembly-CSharp.csproj --no-restore 仍失败且无错误明细，已定位该 csproj 为 Unity/Rider 设计期工程，不适合作为真实编译入口；Unity BatchMode 已执行，但当前机器 License Client IPC 超时，日志返回码 199，未进入脚本编译阶段。

### 任务计划：2026-05-23 17:10

- 阶段：SDK 稳定性第 2 阶段 / 共 2 阶段。
- 问题：第 1 阶段已经完成错误码、状态流转和基本历史记录，但事件流还没有完全收口，缺少 OnRecorderProgress；旧事件和新事件仍有分散触发点；SessionHistory 只记录摘要，无法表达 StartSucceeded、StopRequested、MergeFailed 等业务事件，也缺少结束时间、耗时、错误码和清理接口。
- 计划：新增 RecorderSessionEventType，扩展 RecorderSessionInfo 字段；统一事件发布入口，保证 Warning/Error 均带 RecorderErrorCode 和 sessionId；增加 OnRecorderProgress；用统一历史记录方法覆盖 StartSucceeded、StartFailed、StopRequested、StopSucceeded、StopFailed、MergeStarted、MergeSucceeded、MergeFailed、StreamEnded、ErrorOccurred、WarningOccurred；新增 GetSessionHistory() 和 ClearSessionHistory()；扩展 RecorderSdkSmokeTest 的事件顺序与历史测试；新增 RecorderSdkPhase2Acceptance.md 并更新 README。
- 风险点：本阶段只做事件和历史稳定，不引入 ConfigRegistry、BackendFactory、Profile 导入导出，也不做大规模服务层拆分；真实 Start/Stop 和 Merge 仍依赖本机 FFmpeg 与 Unity 运行环境。

### 修改记录：2026-05-23 17:10

- 阶段：SDK 稳定性第 2 阶段 / 共 2 阶段。
- 为什么这么做：SDK 调用方需要一个稳定的新事件流来监听业务进度、错误和警告，同时需要一份可查询、可清空、有事件类型和错误码的 SessionHistory 来定位问题。
- 做了什么：新增 RecorderSessionEventType；RecorderEventArgs 增加 eventType；RecorderSession/RecorderSessionInfo 增加 configId、startedAt、endedAt、durationMs、eventType、errorCode、message、isMerged、mergeOutputPath 等历史字段；CrossPlatformScreenRecorder 新增 OnRecorderProgress、GetSessionHistory、ClearSessionHistory；Start/Stop/Merge/Stream 里程碑统一写入历史并触发 Progress；Warning/Error 统一写入 WarningOccurred/ErrorOccurred 且带 RecorderErrorCode；旧 OnRecordStarted/OnRecordStopped 保留签名并由统一发布函数驱动；RecorderSdkSmokeTest 扩展事件顺序、非法 Stop、Merging->Starting、旧 Merge 不覆盖新 Recording、maxSessionHistoryCount、ClearSessionHistory 等检查；新增 RecorderSdkPhase2Acceptance.md；README 补充事件和历史读取示例。
- 影响范围：CrossPlatformScreenRecorder、RecorderEventArgs、RecorderSession、RecorderSessionInfo、RecorderSessionEventType、RecorderSdkSmokeTest、RecorderSdkPhase2Acceptance、README。
- 验证结果：rg 检查 OnRecorderProgress、GetSessionHistory、ClearSessionHistory、RecorderSessionEventType、Phase2 验收文档和 SmokeTest 扩展均已存在；rg 检查 Core 下无字符串版 PublishError/PublishWarning 调用残留；git diff --check 通过，仅有 CRLF 提示；Unity BatchMode 仍受当前机器 License Client IPC 超时限制，未进入脚本编译阶段。

### 任务计划：2026-05-23 18:00

- 阶段：SDK 稳定性第 3 阶段：配置管理稳定化。
- 问题：配置加载、迁移、保存、当前配置指针、用户配置创建/删除/重命名等能力仍分散在 Provider 和 UI 中，缺少统一结果对象、统一 AutoFix warnings、configId 索引和重复检测，SDK 调用方无法稳定管理配置。
- 计划：新增 RecorderConfigRegistry 只负责配置扫描/索引/当前配置/创建/克隆/删除/重命名/保存/回退默认模板，不启动录制、不拼 FFmpeg、不改 RecorderState；新增 RecorderConfigResult；补充 RecorderErrorCode 的配置错误码；完善旧配置迁移和 schemaVersion=2 保存；增加 AutoFix 并返回 warnings；统一 UseWin/UseLinux 只保存 schemaVersion/platform/currentConfigId；新增 RecorderSdkPhase3Acceptance.md；扩展 RecorderSdkSmokeTest 配置行为测试；README 同步说明。
- 风险点：现有 UI 仍有一部分 fileName 运行时缓存逻辑，本阶段目标是让 Registry 与 Provider 的主身份使用 configId，保留 fileName 仅作为旧配置兼容和运行时路径缓存，避免一次性重写 UI。

### 修改记录：2026-05-23 18:00

- 阶段：SDK 稳定性第 3 阶段：配置管理稳定化。
- 为什么这么做：配置管理需要有稳定主键、统一返回结果和可追踪 warnings，否则 SDK 调用方只能依赖文件名和零散 bool，后续迁移、克隆、删除、回退都会容易产生身份不同步。
- 做了什么：新增 RecorderConfigRegistry 与 RecorderConfigResult；补充 ConfigNotFound、ConfigDuplicateId、ConfigMigrationFailed、ConfigSaveFailed、ConfigDeleteFailed、ConfigInvalidId、ConfigAutoFixed、ConfigDirectoryMissing 等配置错误码；Registry 支持扫描、configId 索引、重复 ID 修复、当前配置读取/设置、创建、克隆、删除用户配置、重命名 displayName、保存、回退默认模板；AutoFix 统一修复空/非法 configId、空 displayName、非法帧率/缩放/重连参数，并返回 warnings；UseWinRecordConfig/UseLinuxRecordConfig 保存时只写 schemaVersion、platform、currentConfigId；RecordConfigProvider 改为通过 Registry 读取配置；UI 下拉、保存、另存为、使用、删除和正在使用状态改为以 configId 判断，删除旧 Provider 中不再使用的 fileName 指针解析逻辑。
- 影响范围：RecorderConfigRegistry、RecorderConfigResult、RecorderErrorCode、RecorderConfigMigrator、RecordConfigProvider、UIRecorderParamsSettings 配置文件逻辑、RecorderSdkSmokeTest、RecorderSdkPhase3Acceptance、README。
- 验证结果：rg 检查 Configs 下 JSON 已无 configName/fileName 和本机绝对路径；UseWin/UseLinux 当前为 schemaVersion/platform/currentConfigId；git diff --check 通过，仅有 CRLF 提示；dotnet build Assembly-CSharp.csproj --no-restore 仍为 Unity 设计期工程环境问题，失败但无 C# 错误明细，真实编译继续按 README 中 Unity BatchMode/Editor 验证。

### 任务计划：2026-05-23 18:35

- 阶段：Recorder SDK 稳定性闭环总体验收整理。
- 问题：三个稳定性阶段已经通过，但还缺少一份面向交付的总体验收文档，以及一个展示 SDK 调用方式的最小示例脚本。
- 计划：新增 RecorderSdkStabilityAcceptance.md，汇总架构、三阶段成果、公开 API、事件、JSON 配置规范、错误码、SessionHistory 字段、验证状态和已知风险；新增 RecorderSdkUsageExample.cs，演示订阅事件、切换 configId、StartRecordingAsync、StopRecordingAsync、读取 RecorderResult 和 SessionHistory；同步 README 验收索引。
- 风险点：本次只做文档和示例，不继续改核心逻辑；如果静态检查发现示例脚本编译风险，只做最小修正。

### 修改记录：2026-05-23 18:35

- 阶段：Recorder SDK 稳定性闭环总体验收整理。
- 为什么这么做：三阶段已经验收通过，需要沉淀一份可交付的总览文档，让后续接入 SDK 的调用方能快速了解架构、API、事件、配置规范、错误码、历史记录和当前风险。
- 做了什么：新增 RecorderSdkStabilityAcceptance.md；新增 RecorderSdkUsageExample.cs；README 增加总体验收文档和最小使用示例索引；RecorderConfigRegistry 增加 DeleteConfig 包装方法，保持 DeleteUserConfig 原逻辑不变；示例脚本未填写 configId 时自动选择当前平台 Medium 模板。
- 影响范围：RecorderSdkStabilityAcceptance、RecorderSdkUsageExample、RecorderConfigRegistry、README。
- 验证结果：rg 检查 RecorderSdkStabilityAcceptance、RecorderSdkUsageExample、DeleteConfig、StartRecordingAsync、StopRecordingAsync、SessionHistory 和统一事件均已存在；rg 检查 Configs 下无 configName/fileName/本机绝对路径残留；git diff --check 通过，仅有 CRLF 提示；dotnet build Assembly-CSharp.csproj --no-restore 仍为 Unity 设计期工程环境问题，失败但无 C# 错误明细。




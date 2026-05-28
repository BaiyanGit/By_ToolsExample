# CrossPlatformScreenRecorder

当前版本：Recorder SDK v0.3.0-beta

## 项目简介

`CrossPlatformScreenRecorder` 是一套基于 Unity 与 FFmpeg 的跨平台桌面录屏和 RTMP/RTMPS 推流方案，支持在 **Windows** 与 **Linux/X11** 下选择指定显示器进行本地录制或实时推流。

录制目标是物理显示器区域，因此可以录到 Unity 场景、`Screen Space - Overlay` UI，以及该显示器上的其它桌面内容。

## 功能特点

- 支持 Windows 与 Linux。
- 支持枚举并选择当前电脑显示器。
- 支持本地录制和视频推流两种使用方式。
- 支持 MP4 与 WebM 本地录制参数分离。
- 支持 RTMP/RTMPS 推流地址。
- 支持推流码率、GOP、缓冲区、低延迟、重连参数、是否包含系统音频配置。
- 支持自定义 FFmpeg 可执行文件路径。
- 支持按平台区分模板和用户配置。
- 支持左上角状态列表显示当前状态、模板名称、输出格式、分辨率比例、帧率、视频编码器、音频编码器和最后更新时间。
- 默认使用 FFmpeg 外部进程，启动和停止放到后台线程，避免阻塞 Unity 主程序。

## 维护规则

后续如果录制器功能有新增、删除或行为变化，必须同步更新本文档，包含：

- UI 上新增或删除的控件。
- JSON 配置字段变化。
- 默认配置文件命名或目录变化。
- Windows/Linux 平台支持差异。
- 本地录制和推流流程变化。

## 目录结构

配置和输出目录位于：

```txt
Assets/StreamingAssets/FFmpegTools/
```

主要子目录：

```txt
FFmpegTools/
  Configs/       配置文件
  FFmpegApp/     FFmpeg 可执行文件
  Videos/        默认本地视频输出目录
```

## 配置文件命名

所有录制配置都放在：

```txt
Assets/StreamingAssets/FFmpegTools/Configs/
```

模板配置：

```txt
template_win_low.json
template_win_medium.json
template_win_high.json
template_linux_low.json
template_linux_medium.json
template_linux_high.json
```

当前使用配置引用：

```txt
UseWinRecordConfig.json
UseLinuxRecordConfig.json
```

用户另存配置：

```txt
create_win_***.json
create_linux_***.json
```

`UseWinRecordConfig.json` 和 `UseLinuxRecordConfig.json` 只记录当前正在使用哪个源配置文件，不复制完整参数。录制端读取旧配置时会补全推流码率、GOP、缓冲区、重连次数和重连间隔等默认值，避免旧 JSON 缺字段导致推流参数为空。

新版配置结构使用 `schemaVersion = 2`。配置身份由 `configId` 表示，界面显示名由 `displayName` 表示；配置文件名由 `configId` 自动推导为 `configId.json`。`configName`、`fileName`、`Creat_` 前缀均为 Legacy 兼容字段/命名，只用于读取旧配置；新版保存不再写入 `configName` / `fileName`，用户新建配置统一生成 `create_` 前缀的 `configId`。

`UseWinRecordConfig.json` 和 `UseLinuxRecordConfig.json` 新版优先保存 `currentConfigId`：

```json
{
  "schemaVersion": 2,
  "platform": "Win",
  "currentConfigId": "template_win_medium"
}
```

旧版本中 `displayName` 曾表示录制屏幕名称。新版中 `displayName` 表示配置显示名称，录制屏幕名称迁移为 `captureDisplayName`。

## Scene Guide

推荐新用户先打开：

```txt
Assets/Demos/RecorderSdk/Demo/Scenes/录制器_屏幕录制.unity
```

场景职责：

- `录制器_屏幕录制.unity`：SDK 最小使用 Demo。只包含 `RecorderManager`、基础 `开始录制` / `停止录制` / `打开输出目录` / `清空历史` 按钮、状态文本，以及少量配置入口，适合开发者学习如何集成 `StartRecordingAsync()` / `StopRecordingAsync()`。
- `录制器_设置中心.unity`：完整配置管理工具。用于编辑 Recorder 配置、切换 `configId`、保存/另存为/删除配置、调整 FFmpeg、音频、推流、音量增益等参数。
- `录制器_推流演示.unity`：独立 RTMP/RTMPS 推流 Demo。包含简单 3D 测试画面、旋转 Cube、推流地址输入框、配置选择、开始/停止推流按钮和状态/错误文本，适合单独验证推流链路。
- `录制器_旧版综合场景.unity`：旧一体化场景备份，仅用于兼容和对照；新集成不建议从该场景开始。
- `录制视频Recorder.unity`、`录制视频Init.unity`：保留原始 Legacy 场景文件，避免破坏已有引用。

Demo UI 说明：

- 推荐先打开统一入口：`ByTools/Recorder SDK控制台`。控制台集中提供 UI 创建、布局导出/应用、API 参数参考、SDK API、环境检查、打包发布和文档查看。
- API 参数参考已内置在控制台的 `API 参数` 页，可在 `FFmpeg 参数` 和 `视频编码参数` 两个页签之间切换。
- 开发者调用说明已内置在控制台的 `SDK API` 页，包含 `StartRecordingAsync()`、`StopRecordingAsync()`、事件、错误码、SessionHistory 和配置系统说明。
- 文档中心使用中文分类和中文文档按钮，文档内容按点击懒加载并缓存；API 参数页也会按首次进入的页签初始化内容，避免控制台打开时一次性读取或构建全部内容。
- 打包发布页默认只包含 SDK 必要目录、`Configs`、可选 `FFmpegApp` 和 `Videos/README.md` 空目录说明，不会把 `Assets/StreamingAssets/FFmpegTools/Videos` 中的实际录制视频打进 unitypackage。
- `录制器_屏幕录制.unity` 的 UI 应提前存在于场景 Hierarchy 中，`RecorderDemoBasicController` 只负责绑定按钮、文本、Dropdown、Toggle、Slider 并调用 SDK。
- Play 后不会默认动态创建 Demo UI；如果 UI 字段未绑定，Controller 会输出 Warning，避免直接 NullReferenceException。
- 如需在当前场景重建屏幕录制 Demo UI，请进入控制台的 `UI 工具` 页点击 `创建屏幕录制 UI`。该工具只在 Editor 中一次性创建 UI、RecorderManager、CrossPlatformScreenRecorder 和 RecorderDemoBasicController，并自动绑定字段；生成后请保存场景。
- 手动调整 Demo UI 后，可在控制台的 `UI 工具` 页导出为 `DemoUILayoutProfile.json`；后续应用布局或重新创建 Demo UI 时会复用该布局。

## UI 使用说明

场景：

```txt
Assets/Demos/RecorderSdk/Demo/Scenes/录制器_设置中心.unity
```

主要对象：

```txt
RecorderParamsSettings
```

推流地址、推流参数、状态列表等 UI 控件需要在场景中完成布局并挂载到 `UIRecorderParamsSettings` 对应字段。脚本不会在运行时自动创建推流地址或推流参数控件。

设置中心 UI 生成与布局：

- 推荐在 `ByTools/Recorder SDK控制台` 的 `UI 工具` 页执行创建、导出和应用布局操作。
- 如需重建设置中心 UI，请在控制台 `UI 工具` 页点击 `创建设置中心 UI`。
- 该工具只用于 `录制器_设置中心.unity`，会在场景 Hierarchy 中创建真实 UI 对象，并自动挂载和绑定 `UIRecorderParamsSettings`。
- 手动调整设置中心 UI 后，可在控制台 `UI 工具` 页导出为 `SettingsUILayoutProfile.json`；后续应用布局或重新创建设置中心 UI 时会复用该布局。
- Play 时 Runtime 脚本只负责 UI 引用绑定、配置读写、状态刷新和事件响应，不再动态创建整套设置界面。

使用方式与左侧菜单联动说明：

- `存储本地`：左侧显示 `视频设置`、`音频参数`，隐藏 `推流设置`。
- `视频推流`：左侧显示 `视频设置`、`音频参数`、`推流设置`。
- 切换使用方式时，脚本会先刷新左侧菜单可见性，再根据当前仍有效的菜单刷新右侧页面。
- 如果当前选中的菜单仍然可见，会保持当前选中；只有当前菜单被隐藏时，才自动切换到第一个可见菜单。
- 右侧页面始终和左侧菜单一一对应，不允许多个参数页面叠加。
- 旧场景已移动到 `RecorderSdk/Demo/LegacyScenes/`，后续扩展只维护 `录制器_屏幕录制.unity` 与 `录制器_设置中心.unity`。

基础流程：

1. 选择当前平台下的配置文件。
2. 选择录制屏幕。
3. 配置 FFmpeg 可执行文件路径。
4. 选择使用方式：`存储本地` 或 `视频推流`。
5. 根据使用方式填写本地保存路径或推流地址。
6. 点击 `使用`，录制器会读取当前配置。
7. 调用 `CrossPlatformScreenRecorder.ins.StartRecording()` 开始。
8. 调用 `CrossPlatformScreenRecorder.ins.StopRecording()` 停止。

## 推流演示

场景：

```txt
Assets/Demos/RecorderSdk/Demo/Scenes/录制器_推流演示.unity
```

使用步骤：

1. 打开 `ByTools/Recorder SDK控制台`，在首页点击 `打开推流演示场景`，或直接打开上面的场景路径。
2. Play 场景，确认画面中有旋转 Cube 和 `Recorder SDK 推流演示` UI。
3. 在 `RTMP / RTMPS 地址` 输入框填写有效地址，例如 `rtmp://server/app/streamKey` 或 `rtmps://server/app/streamKey`。
4. 在配置 Dropdown 中选择一个现有 `configId`。Demo 会复用该配置的视频、FFmpeg、推流码率等参数。
5. Beta 阶段默认建议先验证视频推流链路；Windows 推流 Demo 创建临时配置时会保持 `streamIncludeAudio=false`，不会把输入框内容或临时音频策略写回正式配置。Linux/国产系统保留所选配置中的推流音频设置，并继续按现有 Linux 音频能力判断。
6. 点击 `开始推流`，Demo 会创建临时推流配置并调用 `StartRecordingAsync()`。
7. 点击 `停止推流`，Demo 会调用 `StopRecordingAsync()`。

安全说明：

- 推流地址通常包含 stream key，不建议提交到 Git。
- Demo 不内置真实推流密钥。
- Demo 不会把输入框里的临时推流地址永久保存到正式配置文件；启动推流时会创建临时配置，启动请求结束后恢复原配置并删除临时配置。
- Windows 推流音频依赖 FFmpeg `wasapi` 输入。本地录屏能录到声音，不代表 Windows 推流一定能带声音。
- Linux/国产系统推流音频不使用 `wasapi` 判断，依赖 `pulse` / `alsa` / `pipewire` 或当前系统 FFmpeg 编译能力。
- Windows 当前 FFmpeg 不支持 `wasapi` 时，请保持 `streamIncludeAudio=false`，先只测试视频推流链路。

常见失败原因：

- 地址不是 `rtmp://` 或 `rtmps://`。
- 推流服务器不可达、网络被防火墙拦截，或服务器拒绝 stream key。
- FFmpeg 路径无效，或当前 FFmpeg 不支持对应协议。
- 当前配置不是有效录制配置，例如显示器不可用、帧率/缩放参数非法。
- 推流包含系统音频时，当前平台或 FFmpeg 不支持所需音频输入。Windows 下常见原因是当前 FFmpeg 不支持 `wasapi`；Linux/国产系统请检查 `pulse` / `alsa` / `pipewire` 与 FFmpeg 编译能力。

代码调用推荐使用可等待接口：

```csharp
RecorderResult startResult = await CrossPlatformScreenRecorder.ins.StartRecordingAsync();
RecorderResult stopResult = await CrossPlatformScreenRecorder.ins.StopRecordingAsync();
```

`StartRecording()` 和 `StopRecording()` 仍保留给 Unity Button 或旧代码使用，内部会调用对应的 Async 方法。录制器状态可通过 `CrossPlatformScreenRecorder.ins.State` 读取，状态包括 `Uninitialized`、`Ready`、`Starting`、`Recording`、`Stopping`、`Merging`、`Completed`、`Error`。

`CrossPlatformScreenRecorder.ins.CurrentSession` 提供当前会话只读摘要，包含 `sessionId`、开始时间、输出路径、临时路径、是否推流、状态、配置名和显示器名称。

统一事件：

- `OnRecorderStateChanged`
- `OnRecorderError`
- `OnRecorderWarning`
- `OnMergeStarted`
- `OnMergeCompleted`
- `OnConfigChanged`

这些事件使用 `RecorderEventArgs`，包含 `sessionId`、`state`、`outputPath`、`message`、`exception`。旧事件 `OnRecordingStateChanged`、`OnRecordStarted`、`OnRecordStopped` 仍保留兼容。

Core 中新增了插件化接口边界：`IRecorderBackend`、`IAudioCaptureBackend`、`IMergeService`。当前版本先保留现有 FFmpeg/WASAPI 实现，后续替换或扩展后端时可逐步接入这些接口。

左上角状态列表按固定顺序显示：当前状态、模板名称、输出格式、分辨率比例、帧率、视频编码器、音频编码器、当前配置最后更新时间。当前状态只判断选中的配置文件是否已经被使用：正在使用时第一行 Icon 为绿色且文字为 `正在使用`，否则 Icon 为灰色且文字为 `未使用`。输出格式在视频推流模式显示 `推流`，在本地存储模式显示“视频格式”参数当前选项。状态文本必须在脚本上使用 8 个独立字段逐个绑定：`txtStatusUseState`、`txtStatusTemplateName`、`txtStatusOutputFormat`、`txtStatusOutputScale`、`txtStatusFrameRate`、`txtStatusVideoCodec`、`txtStatusAudioCodec`、`txtStatusLastUpdateTime`。

## 本地录制

选择 `存储本地` 时显示本地录制相关参数：

- 视频文件保存路径
- 视频文件前缀
- 视频格式
- 音频参数
- 视频编码参数
- 停止录制等待
- 临时文件释放等待
- 合并超时
- 合并后删除临时文件

本地录制会保存到配置中的 `videoSaveDirectory`。如果未配置，默认使用：

```txt
Assets/StreamingAssets/FFmpegTools/Videos/
```

### MP4 参数

选择 `mp4` 时，UI 只显示 MP4 相关编码选项：

- 视频编码器：`libx264`、`libx265`
- 音频编码器：`aac`
- CRF 画质
- 像素格式
- 编码预设

### WebM 参数

选择 `webm` 时，UI 只显示 WebM 相关编码选项：

- 视频编码器：`libvpx`、`libvpx-vp9`
- 音频编码器：`libvorbis`、`libopus`
- WebM 视频码率
- WebM deadline
- WebM cpu-used

## 视频推流

选择 `视频推流` 时显示推流相关参数，并隐藏本地保存路径、视频文件前缀、视频格式、CRF、合并超时等本地文件专用参数。首次打开设置界面时会立即按当前使用方式刷新显隐状态，避免显示无关参数。

当前仅支持：

```txt
rtmp://
rtmps://
```

推流参数：

- `streamUrl`：推流地址。
- `streamVideoBitrate`：推流视频码率。
- `streamGop`：关键帧间隔。
- `streamBufferSize`：推流缓冲区。
- `streamLowLatency`：是否启用低延迟模式。
- `streamAutoReconnect`：是否启用自动重连配置。
- `streamReconnectCount`：重连次数。
- `streamReconnectIntervalMs`：重连间隔。
- `streamIncludeAudio`：推流是否包含系统音频。默认模板保持 `false`，优先保证首次推流只验证视频链路。

推流默认使用 `libx264 + flv` 输出，优先保证 Windows/Linux 和国产硬件环境下的兼容性。推流时 FFmpeg 进程会降低优先级，减少对 Unity 主程序的影响。

### 推流音频

- Linux/国产系统：开启 `streamIncludeAudio` 且音频模式为系统音频时，会沿用当前 Linux 音频源解析逻辑，优先按 `pulse`，也受 `alsa` / `pipewire` 和当前 FFmpeg 编译能力影响；不会检查或依赖 `wasapi`。
- Windows：开启 `streamIncludeAudio` 且音频模式为系统音频时，会在启动前执行 `ffmpeg -hide_banner -devices` 检测是否支持 `wasapi`。如果当前 FFmpeg 不支持 WASAPI，会返回明确错误并阻止启动。
- Windows 本地 MP4/WebM 录屏声音和推流声音不是同一路径。本地录屏声音由 `WASAPILoopbackRecorder.dll` 录制临时 WAV，停止后再由 FFmpeg Merge；推流声音由 FFmpeg 实时 `wasapi` 输入直接进入 FLV/RTMP 管线。
- 如果 Windows 当前 FFmpeg 不支持 `wasapi`，推流请先走视频；如需 Windows 推流带音频，需要替换为支持 `wasapi` 的 Windows FFmpeg。Linux/国产系统不要用 `wasapi` 结论判断音频可用性。
- 如果只需要画面推流，关闭 `streamIncludeAudio` 可降低性能压力，并避免 wasapi 能力缺失导致启动失败。

## JSON 字段保留规则

JSON 中会保留本地录制和推流的全部字段。UI 隐藏某些控件只代表当前使用方式或格式下不需要编辑，不代表字段被删除。

保存和使用时只校验当前模式需要的字段：

- 本地录制校验 FFmpeg 路径和视频保存路径。
- 视频推流校验 FFmpeg 路径和 RTMP/RTMPS 推流地址。
- MP4 只显示 MP4 编码器选项。
- WebM 只显示 WebM 编码器选项。

这样可以避免用户切换格式或使用方式后丢失原来的参数。

## 关键脚本

- `RecorderSdk/Core/Runtime/CrossPlatformScreenRecorder.cs`：开始录制、停止录制、本地录制和推流主入口。
- `RecorderSdk/Core/Commands/FFmpegCommandBuilder.cs`：FFmpeg 本地录制、推流和合并命令构建。
- `RecorderSdk/Core/Commands/FFmpegProcessRunner.cs`：FFmpeg 进程启动、停止和优先级控制。
- `RecorderSdk/Core/Runtime/RecorderDisplayProvider.cs`：Windows/Linux 显示器枚举。
- `RecorderSdk/UI/Settings/UIRecorderParamsSettings.cs`：配置 UI 逻辑。
- `RecorderSdk/UI/Settings/UIRecorderParamsSettings.ConfigFiles.cs`：配置文件读取、保存、默认模板生成。
- `RecorderSdk/UI/Settings/UIRecorderParamsSettings.Descriptions.cs`：参数说明提示。
- `RecorderSdk/UI/Settings/RecorderParamsConfig.cs`：JSON 配置字段定义。
- `RecorderSdk/UI/Settings/RecordConfigProvider.cs`：录制器读取当前使用配置。

## 平台说明

### Windows

- 屏幕采集使用 FFmpeg `gdigrab`。
- 本地录制系统音频使用项目内 WASAPI Loopback 插件录为临时 WAV，再与视频合并。
- 推流系统音频使用 FFmpeg WASAPI 输入，启动前会检测当前 FFmpeg 是否编译支持；不支持时不会继续拼接推流命令。

### Linux

- 屏幕采集使用 `x11grab`。
- 显示器枚举依赖 `xrandr`。
- 系统音频依赖 PulseAudio 源解析。
- 当前实现面向 X11 环境。

## Recorder SDK 稳定性

当前稳定性阶段优先保证调用方能判断“发生了什么、为什么失败、现在处于什么状态”：

- `RecorderErrorCode`：统一错误码来源，`RecorderResult` 与 `RecorderEventArgs` 都会携带 `errorCode` 和 `errorCodeText`。
- `RecorderStateTransition`：集中定义状态流转表。非法流转不会静默吞掉，会通过 `OnRecorderWarning` 抛出 `StateTransitionInvalid`。
- 事件顺序：状态变化先触发 `OnRecorderStateChanged` 与旧兼容事件 `OnRecordingStateChanged`，随后再触发 `OnRecordStarted`、`OnMergeStarted`、`OnMergeCompleted`、`OnRecordStopped` 等业务事件。
- `SessionHistory`：`CrossPlatformScreenRecorder` 会保留最近若干次会话摘要，数量由 `maxSessionHistoryCount` 控制。失败、停止、合并完成和推流结束都会写入历史。
- `CurrentSession` 在录制和后台处理阶段都可读取；`LastSession` 保留最近一次结束或失败的会话。

稳定性验收摘要见：

```txt
Assets/Demos/RecorderSdk/Documentation/Acceptance/RecorderSdkAcceptanceSummary.md
```

历史阶段文档已归档到：

```txt
Assets/Demos/RecorderSdk/Documentation/Archive/
```

项目结构说明见：

```txt
Assets/Demos/RecorderSdk/Documentation/ProjectStructure.md
```

配置管理由 `RecorderConfigRegistry` 负责，主身份为 `configId`。`fileName` 是 Legacy 兼容字段，只作为旧配置兼容和运行时路径缓存，不作为新版配置身份。配置操作统一返回 `RecorderConfigResult`，包含 `success`、`errorCode`、`warnings`、`configId`、`config`、`message`。

```csharp
var registry = new RecorderConfigRegistry();
RecorderConfigResult current = registry.GetCurrentConfig();
RecorderConfigResult clone = registry.CloneConfig("template_win_medium", "我的录制配置");
RecorderConfigResult setCurrent = registry.SetCurrentConfig(clone.configId);
```

事件订阅建议：

```csharp
CrossPlatformScreenRecorder.ins.OnRecorderProgress += args =>
{
    Debug.Log($"{args.sessionId} {args.eventType} {args.message}");
};

CrossPlatformScreenRecorder.ins.OnRecorderError += args =>
{
    Debug.LogError($"{args.sessionId} {args.errorCodeText} {args.message}");
};
```

会话历史读取：

```csharp
IReadOnlyList<RecorderSessionInfo> history = CrossPlatformScreenRecorder.ins.GetSessionHistory();
CrossPlatformScreenRecorder.ins.ClearSessionHistory();
```

`SessionHistory` 会记录 `StartSucceeded`、`StartFailed`、`StopRequested`、`StopSucceeded`、`StopFailed`、`MergeStarted`、`MergeSucceeded`、`MergeFailed`、`StreamEnded`、`ErrorOccurred`、`WarningOccurred` 等事件。`maxSessionHistoryCount` 控制最多保留条数。

Unity 内最小冒烟测试脚本：

```txt
Assets/Demos/RecorderSdk/Demo/Scripts/RecorderSdkSmokeTest.cs
```

使用方式：把 `RecorderSdkSmokeTest` 挂到任意场景对象，勾选 `runOnStart`，或在 Inspector 右键执行 `Run Smoke Test`。测试会覆盖未录制 Stop、FFmpeg 路径错误、输出目录为空回退默认目录、连续 Start、正常 Start->Stop、合并失败模拟和 `SessionHistory` 记录。

SDK 最小使用示例：

```txt
Assets/Demos/RecorderSdk/Demo/Scripts/RecorderSdkUsageExample.cs
```

使用方式：把 `RecorderSdkUsageExample` 挂到场景对象，绑定 `CrossPlatformScreenRecorder`，设置 `configId`，再调用 `StartExample()` / `StopExample()` 或直接 await `StartExampleAsync()` / `StopExampleAsync()`。

运行时实机验证记录：

```txt
Assets/Demos/RecorderSdk/Documentation/Validation/RecorderSdkRuntimeValidation.md
```

UnityPackage 导入验证说明：

```txt
Assets/Demos/RecorderSdk/Documentation/Validation/RecorderSdkUnityPackageValidation.md
```

### 编译验证

不建议使用 `dotnet build Assembly-CSharp.csproj` 作为 Unity 工程验收入口。当前 `Assembly-CSharp.csproj` 是 Unity/Rider 生成的设计期工程，包含 `NoStdLib=true`、Unity/Rider 占位引用路径和 Unity SourceGenerator/Library 依赖，可能出现“生成失败，0 个警告，0 个错误”的无效结果。

推荐使用 Unity BatchMode：

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.0.48f1\Editor\Unity.exe" -batchmode -quit -projectPath "<项目根目录>" -logFile "Logs\UnityBatchmodeCompile.log"
```

后续接入 Unity Test Framework 后，可使用：

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.0.48f1\Editor\Unity.exe" -batchmode -quit -projectPath "<项目根目录>" -runTests -testPlatform editmode -testResults "Logs\RecorderEditModeTestResults.xml" -logFile "Logs\RecorderEditModeTests.log"
```

## 注意事项

- 推流对 CPU、网络和目标服务器稳定性更敏感，建议先从 `2M` 或 `3M` 码率测试。
- 国产硬件环境优先使用默认 `libx264` 软件编码保证兼容；如果后续需要硬件编码，需要按实际机器和 FFmpeg 编译能力增加独立策略。
- Windows 推流带系统音频依赖 FFmpeg WASAPI 输入；本地录屏声音由原生 DLL + Merge 实现，两者能力不要混淆。Linux/国产系统推流音频依赖 `pulse` / `alsa` / `pipewire` 或当前 FFmpeg 编译能力，不受 Windows `wasapi` 检测限制。当前 Beta 阶段默认只保证视频推流稳定。

## FAQ

### 本地录屏有声音，为什么推流开启音频会失败？

本地 MP4/WebM 录屏声音由 `WASAPILoopbackRecorder.dll` 录制为临时 WAV，停止录制后再与视频文件 Merge。Windows 推流声音走的是 FFmpeg 实时 `wasapi` 输入，也就是 `-f wasapi -i default`。如果当前 FFmpeg 没有编译 `wasapi` 输入，本地录屏仍可能有声音，但推流音频会不可用。

### Windows 当前 FFmpeg 不支持 wasapi 时怎么办？

在 Windows 上保持 `streamIncludeAudio=false`，先验证视频推流链路；如果业务必须推流带系统音频，请替换为支持 `wasapi` 的 Windows FFmpeg 构建，再在环境检查页确认“FFmpeg wasapi 输入支持”为支持。Linux/国产系统不检查 `wasapi`，请检查 `pulse` / `alsa` / `pipewire` 或系统 FFmpeg 编译能力。

## 录制声音太小怎么办？

音频配置区域新增：

```json
"enableAudioGain": false,
"audioGainDb": 0.0,
"audioLimiterEnabled": true
```

`enableAudioGain` 开启后会在 FFmpeg 音频链路中追加 `volume={audioGainDb}dB`；`audioLimiterEnabled` 开启时会继续追加 `alimiter=limit=0.95`，降低爆音和削波风险。如果当前 FFmpeg 不支持 `alimiter`，启动校验会返回 `AudioFilterInvalid` warning，并自动降级为只使用 `volume`。

建议：

- 先尝试 `audioGainDb = 6`，通常会明显增大音量。
- 如果仍然偏小，可以尝试 `9` 或 `12`。
- 如果出现破音、爆音或声音发糊，降低到 `3`，或关闭增强。
- 不建议超过 `12`，除非确认源声音不会削波。
- 没有音频输入时不会拼接 `-af`，关闭 `enableAudioGain` 时也不会改变 FFmpeg 音频滤镜。
- 旧场景如果还没有挂载新增的音量增强 UI 控件，运行时仍然安全；可以直接在 JSON 配置中设置 `enableAudioGain`、`audioGainDb`、`audioLimiterEnabled` 来启用该功能。




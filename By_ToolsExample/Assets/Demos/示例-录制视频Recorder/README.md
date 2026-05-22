# CrossPlatformScreenRecorder

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
Template_Win_Low.json
Template_Win_Medium.json
Template_Win_High.json
Template_Linux_Low.json
Template_Linux_Medium.json
Template_Linux_High.json
```

当前使用配置引用：

```txt
UseWinRecordConfig.json
UseLinuxRecordConfig.json
```

用户另存配置：

```txt
Creat_Win_***.json
Creat_Linux_***.json
```

`UseWinRecordConfig.json` 和 `UseLinuxRecordConfig.json` 只记录当前正在使用哪个源配置文件，不复制完整参数。

## UI 使用说明

场景：

```txt
Assets/Demos/示例-录制视频Recorder/录制视频Recorder.unity
```

主要对象：

```txt
RecorderParamsSettings
```

基础流程：

1. 选择当前平台下的配置文件。
2. 选择录制屏幕。
3. 配置 FFmpeg 可执行文件路径。
4. 选择使用方式：`存储本地` 或 `视频推流`。
5. 根据使用方式填写本地保存路径或推流地址。
6. 点击 `使用`，录制器会读取当前配置。
7. 调用 `CrossPlatformScreenRecorder.ins.StartRecording()` 开始。
8. 调用 `CrossPlatformScreenRecorder.ins.StopRecording()` 停止。

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

选择 `视频推流` 时显示推流相关参数，并隐藏本地保存路径、视频格式、CRF、合并超时等本地文件专用参数。

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
- `streamIncludeAudio`：推流是否包含系统音频。

推流默认使用 `libx264 + flv` 输出，优先保证 Windows/Linux 和国产硬件环境下的兼容性。推流时 FFmpeg 进程会降低优先级，减少对 Unity 主程序的影响。

### 推流音频

- Linux：开启 `streamIncludeAudio` 且音频模式为系统音频时，会尝试解析 Pulse 音频源并推送音频。
- Windows：开启 `streamIncludeAudio` 且音频模式为系统音频时，会尝试使用 FFmpeg WASAPI 输入 `default`。目标机器上的 FFmpeg 必须支持 WASAPI 输入，否则推流启动可能失败。
- 如果只需要画面推流，关闭 `streamIncludeAudio` 可降低性能压力。

## JSON 字段保留规则

JSON 中会保留本地录制和推流的全部字段。UI 隐藏某些控件只代表当前使用方式或格式下不需要编辑，不代表字段被删除。

保存和使用时只校验当前模式需要的字段：

- 本地录制校验 FFmpeg 路径和视频保存路径。
- 视频推流校验 FFmpeg 路径和 RTMP/RTMPS 推流地址。
- MP4 只显示 MP4 编码器选项。
- WebM 只显示 WebM 编码器选项。

这样可以避免用户切换格式或使用方式后丢失原来的参数。

## 关键脚本

- `Scripts/Core/CrossPlatformScreenRecorder.cs`：开始录制、停止录制、本地录制和推流命令生成。
- `Scripts/Core/FFmpegProcessRunner.cs`：FFmpeg 进程启动、停止和优先级控制。
- `Scripts/Core/RecorderDisplayProvider.cs`：Windows/Linux 显示器枚举。
- `Scripts/UISettings/UIRecorderParamsSettings.cs`：配置 UI 逻辑。
- `Scripts/UISettings/UIRecorderParamsSettings.ConfigFiles.cs`：配置文件读取、保存、默认模板生成。
- `Scripts/UISettings/UIRecorderParamsSettings.Descriptions.cs`：参数说明提示。
- `Scripts/UISettings/RecorderParamsConfig.cs`：JSON 配置字段定义。
- `Scripts/UISettings/RecordConfigProvider.cs`：录制器读取当前使用配置。

## 平台说明

### Windows

- 屏幕采集使用 FFmpeg `gdigrab`。
- 本地录制系统音频使用项目内 WASAPI Loopback 插件录为临时 WAV，再与视频合并。
- 推流系统音频使用 FFmpeg WASAPI 输入，依赖当前 FFmpeg 是否编译支持。

### Linux

- 屏幕采集使用 `x11grab`。
- 显示器枚举依赖 `xrandr`。
- 系统音频依赖 PulseAudio 源解析。
- 当前实现面向 X11 环境。

## 注意事项

- 推流对 CPU、网络和目标服务器稳定性更敏感，建议先从 `2M` 或 `3M` 码率测试。
- 国产硬件环境优先使用默认 `libx264` 软件编码保证兼容；如果后续需要硬件编码，需要按实际机器和 FFmpeg 编译能力增加独立策略。
- Windows 推流带系统音频前，请确认 FFmpeg 支持 WASAPI 输入。

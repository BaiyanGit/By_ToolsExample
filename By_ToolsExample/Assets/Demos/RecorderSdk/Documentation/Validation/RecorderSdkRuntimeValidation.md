# Recorder SDK Runtime Validation

## 验证目标

本阶段目标是验证 `Recorder SDK v0.3.0-beta` 在真实 Unity + FFmpeg 环境下是否稳定可运行。

本阶段不做：

- BackendFactory
- 新插件系统
- 新配置体系
- 大规模服务层拆分

本阶段允许：

- 修真实运行 bug
- 修状态机 bug
- 修事件顺序 bug
- 修 FFmpeg 命令 bug

## 测试环境

本节中的本机路径仅用于记录本次验证环境，不属于 SDK 默认配置，也不要求导入包使用相同路径。

- 验证日期：2026-05-25
- 操作系统：Windows 10 Pro
- Unity 版本：6000.0.48f1
- 项目路径：`E:\UnityProjectsE\Example\By_ToolsExample\By_ToolsExample`
- 示例场景：`Assets/Demos/RecorderSdk/Demo/Scenes/录制器_基础演示.unity`
- FFmpeg 路径：`C:\Program Files\ffmpeg\bin\ffmpeg.exe`
- SDK 内置 FFmpeg 路径：`Assets/StreamingAssets/FFmpegTools/FFmpegApp/ffmpeg.exe`
- FFmpeg 版本：`N-121090-g9e4ff4732c-20250916`
- FFmpeg 重要编译能力：
  - `gdigrab`：存在
  - `dshow`：存在
  - `libx264`：存在
  - `libvpx`：存在
  - `flv` / RTMP 输出：存在
  - `wasapi`：不存在

## 当前配置状态

- 配置目录：`Assets/StreamingAssets/FFmpegTools/Configs`
- 当前 Windows Use 指针：`UseWinRecordConfig.json`
- 当前 Windows `currentConfigId`：`template_win_medium`
- 当前模板：`template_win_medium.json`
- 模板 `customFFmpegPath`：空
- 模板 `videoSaveDirectory`：空
- SDK 默认 FFmpeg 目录：`Assets/StreamingAssets/FFmpegTools/FFmpegApp`
- 当前 `FFmpegApp` 目录状态：已复制 `ffmpeg.exe`

结论：

- 配置体系符合 v0.3.0-beta 规范。
- 当前 SDK 如果直接使用默认模板，会回退到 `StreamingAssets/FFmpegTools/FFmpegApp/ffmpeg.exe`。
- 当前该路径已存在可执行文件，`FFmpegPathMissing` 阻塞已处理。

## 已执行验证

### 1. Unity BatchMode

命令：

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.0.48f1\Editor\Unity.exe" -batchmode -quit -projectPath "E:\UnityProjectsE\Example\By_ToolsExample\By_ToolsExample" -logFile "Logs\UnityRuntimeValidation_Batchmode.log"
```

结果：

- 未进入脚本编译阶段。
- 日志显示 License Client IPC 超时。
- 返回码历史记录为 199。

关键日志：

```txt
Timed-out after 60.14s, waiting for channel: "LicenseClient-baiya"
IPC channel to LicensingClient doesn't exist; aborting
Application will terminate with return code 199
```

状态：环境阻塞。

影响：

- 不能在当前会话中完成 Unity Editor 内真实 SDK Start / Stop 验证。
- 不能在当前会话中完成状态机、事件、SessionHistory 的运行时实测。

补充尝试：

- 已确认 Unity Hub 进程存在。
- 已确认 `Unity.Licensing.Client.exe` 进程存在。
- Unity Licensing Client 日志中发现连接 `127.0.0.1:10808` 被拒绝。
- 已在当前 PowerShell 进程中清空 `HTTP_PROXY`、`HTTPS_PROXY`、`ALL_PROXY` 后重试 BatchMode。
- 重试日志：`Logs/UnityRuntimeValidation_Batchmode_NoProxy_20260525.log`
- 结果仍为 License Client IPC 超时，未进入脚本编译阶段。
- 已尝试请求打开 Unity Editor 图形界面以便交互式处理许可证，但当前工具授权请求超时，未能打开 Editor。

### 2. FFmpeg 版本和设备能力

命令：

```powershell
& "C:\Program Files\ffmpeg\bin\ffmpeg.exe" -version
& "C:\Program Files\ffmpeg\bin\ffmpeg.exe" -hide_banner -devices
```

结果：

- FFmpeg 可执行文件存在。
- `gdigrab`、`dshow`、`lavfi`、`openal` 可用。
- `wasapi` 不在设备列表中。

状态：部分通过。

影响：

- Windows 屏幕采集理论上可走 `gdigrab`。
- 当前 FFmpeg 构建不支持 SDK 中 Windows 系统音频推流/录制可能使用的 `wasapi`。
- 已复制系统 FFmpeg 到 `Assets/StreamingAssets/FFmpegTools/FFmpegApp/ffmpeg.exe`，并通过 `-version` 与 `-devices` 验证可执行。

### 3. DirectShow 音频设备枚举

命令：

```powershell
& "C:\Program Files\ffmpeg\bin\ffmpeg.exe" -hide_banner -list_devices true -f dshow -i dummy
```

结果：

- 检测到音频设备：`耳机 (LinkPods)`
- 检测到 `立体声混音 (Realtek(R) Audio)`，但标记为 `(none)`
- 未检测到 video dshow 设备。

状态：部分通过。

影响：

- dshow 音频设备可枚举。
- WASAPI 验证无法通过当前 FFmpeg 构建完成。

### 4. WASAPI 验证

命令：

```powershell
& "C:\Program Files\ffmpeg\bin\ffmpeg.exe" -hide_banner -f wasapi -i default -t 1 -f null -
```

结果：

```txt
Unknown input format: 'wasapi'
```

状态：失败。

结论：

- 当前 FFmpeg 不支持 `wasapi`。
- Windows 音频验证不能通过当前 FFmpeg 完成。
- SDK 已增加启动前 `ffmpeg -devices` 检测；如果 `streamIncludeAudio=true` 且当前 FFmpeg 不支持 `wasapi`，会在配置校验阶段返回 `RecorderErrorCode.AudioStartFailed`，不会继续拼接推流命令。

### 5. MP4 编码与落盘控制组

命令：

```powershell
& "C:\Program Files\ffmpeg\bin\ffmpeg.exe" -y -hide_banner -f lavfi -t 3 -i testsrc=size=1280x720:rate=10 -c:v libx264 -preset ultrafast -crf 30 -pix_fmt yuv420p "Assets/StreamingAssets/FFmpegTools/Videos/RuntimeValidation/runtime_lavfi_mp4_control.mp4"
```

结果：

- 输出文件：`runtime_lavfi_mp4_control.mp4`
- 文件大小：56138 bytes
- 时长：3.000000 秒
- 编码：H.264 / libx264
- 分辨率：1280x720
- 帧率：10 fps
- 像素格式：yuv420p

状态：通过。

结论：

- 当前 FFmpeg 的 MP4 / libx264 编码和输出目录写入能力正常。
- 该测试不是桌面录制，只是编码与落盘控制组。

### 6. WebM 编码与落盘控制组

命令：

```powershell
& "C:\Program Files\ffmpeg\bin\ffmpeg.exe" -y -hide_banner -f lavfi -t 3 -i testsrc=size=1280x720:rate=10 -c:v libvpx -b:v 2M -deadline realtime -cpu-used 8 "Assets/StreamingAssets/FFmpegTools/Videos/RuntimeValidation/runtime_lavfi_webm_control.webm"
```

结果：

- 输出文件：`runtime_lavfi_webm_control.webm`
- 文件大小：244880 bytes
- 时长：3.000000 秒
- 编码：VP8 / libvpx
- 分辨率：1280x720
- 帧率：10 fps

状态：通过。

结论：

- 当前 FFmpeg 的 WebM / libvpx 编码和输出目录写入能力正常。
- 该测试不是桌面录制，只是编码与落盘控制组。

### 7. Windows 桌面采集 gdigrab

命令：

```powershell
& "C:\Program Files\ffmpeg\bin\ffmpeg.exe" -y -hide_banner -f gdigrab -framerate 10 -t 5 -i desktop -vf scale=1280:-2 -c:v libx264 -preset ultrafast -crf 28 -pix_fmt yuv420p "Assets/StreamingAssets/FFmpegTools/Videos/RuntimeValidation/runtime_mp4_smoke.mp4"
```

结果：

```txt
[gdigrab] Capturing whole desktop as 3640x1920x32 at (0,-415)
[gdigrab] Failed to capture image (error 5)
Output file does not contain any stream
```

固定区域重试：

```powershell
& "C:\Program Files\ffmpeg\bin\ffmpeg.exe" -y -hide_banner -f gdigrab -framerate 5 -offset_x 0 -offset_y 0 -video_size 1280x720 -t 3 -i desktop -c:v libx264 -preset ultrafast -crf 30 -pix_fmt yuv420p "Assets/StreamingAssets/FFmpegTools/Videos/RuntimeValidation/runtime_mp4_region_smoke.mp4"
```

结果：

```txt
[gdigrab] Failed to capture image (error 5)
```

状态：失败。

结论：

- 当前命令运行会话没有桌面抓帧权限，或被 Windows 桌面会话权限限制。
- 已排除负坐标多显示器为唯一原因，因为固定 `0,0` 区域也失败。
- 需要在真实交互式 Unity Editor 进程中复测。

### 8. RTMP 推流失败场景

命令：

```powershell
& "C:\Program Files\ffmpeg\bin\ffmpeg.exe" -y -hide_banner -f lavfi -re -t 3 -i testsrc=size=640x360:rate=10 -c:v libx264 -preset ultrafast -tune zerolatency -f flv "rtmp://127.0.0.1:1935/live/recorder_validation"
```

结果：

```txt
Connection to tcp://127.0.0.1:1935 failed
Cannot open connection
Error opening output file
```

状态：通过失败路径控制组。

结论：

- 无 RTMP 服务端时 FFmpeg 会快速失败。
- SDK 中应映射为推流启动失败相关错误。
- 推流成功、推流中 Stop、自动重连、推流事件顺序仍需真实 RTMP 服务端验证。

### 9. FFmpeg 进程生命周期

结果：

- 控制组编码结束后，未发现残留 `ffmpeg` 进程。
- RTMP 失败测试结束后，短暂观察到 `ffmpeg` 进程，随后消失。

状态：部分通过。

结论：

- 本次命令行控制组未留下稳定可见的 FFmpeg 僵尸进程。
- SDK 级 Stop / Merge 队列 / 崩溃恢复仍需 Unity 运行时验证。

### 10. 基础性能控制组

命令：

```powershell
ffmpeg lavfi testsrc 1280x720 25fps -> libx264 ultrafast 10 秒输出
```

结果：

- 输出文件：`runtime_lavfi_perf_10s.mp4`
- 文件大小：248921 bytes
- FFmpeg 退出码：0
- 采样内存峰值：约 166.05 MB
- 采样 CPU 累计值：约 2.359375 秒

状态：通过控制组。

结论：

- 编码控制组可正常启动、输出、退出。
- 该数据不代表真实桌面录制 CPU/内存占用。

## 请求验证项结果总表

### Windows 实机验证

| 项目 | 状态 | 说明 |
| --- | --- | --- |
| 本地 MP4 录制 | 未通过 | Unity 未启动，gdigrab 在当前会话 error 5；FFmpegApp 缺失已修复。 |
| WebM 录制 | 未通过 | Unity 未启动，gdigrab 在当前会话 error 5；FFmpegApp 缺失已修复。 |
| 连续 Start / Stop | 未执行 | 需要 Unity Editor 运行 SDK。 |
| 长时间录制 30 分钟 | 未执行 | 需要 Unity Editor 和可抓屏环境。 |
| SessionHistory 是否正常 | 未执行 | 需要 Unity Editor 运行 SDK。 |
| 状态机是否正确 | 未执行 | 需要 Unity Editor 运行 SDK。 |
| Merge 是否稳定 | 未执行 | 需要 Unity Editor 运行 SDK。 |
| FFmpeg 崩溃后是否能恢复 | 未执行 | 需要 Unity Editor 运行 SDK。 |
| 非法状态调用只产生 Warning | 未执行 | 需要 Unity Editor 运行 SDK。 |
| 多显示器切换 | 未执行 | 当前 FFmpeg 显示桌面坐标含负 y，但抓帧失败。 |
| configId 切换 | 未执行 | 需要 Unity Editor 或测试脚本运行。 |
| 配置保存后重新加载 | 未执行 | 需要 Unity Editor 或测试脚本运行。 |

### 推流验证

| 项目 | 状态 | 说明 |
| --- | --- | --- |
| RTMP 推流 | 未执行 | 缺少可用 RTMP 服务端。 |
| 推流中 Stop | 未执行 | 需要 Unity Editor 和 RTMP 服务端。 |
| 推流失败 | 部分通过 | FFmpeg 控制组连接本地无服务端 RTMP，快速失败。 |
| 自动重连 | 未执行 | 需要可控 RTMP 服务端断开/恢复。 |
| streamIncludeAudio | 阻止启动 | 当前 FFmpeg 不支持 wasapi，SDK 已改为启动前返回 `AudioStartFailed`。 |
| 推流状态事件 | 未执行 | 需要 Unity Editor 运行 SDK。 |

### Windows 音频验证

| 项目 | 状态 | 说明 |
| --- | --- | --- |
| WASAPI | 未通过 | 当前 FFmpeg 报 `Unknown input format: wasapi`。 |
| 无音频设备 | 未执行 | 需要可控设备环境。 |
| 音频设备被拔掉 | 未执行 | 需要人工插拔设备。 |
| 音频录制失败后的错误码 | 静态修复 | Windows 推流音频缺 wasapi 时，配置校验返回 `AudioStartFailed`；Unity 事件仍需运行时验证。 |

### Linux 验证

| 项目 | 状态 | 说明 |
| --- | --- | --- |
| x11grab | 未执行 | 当前环境为 Windows。 |
| pulse | 未执行 | 当前环境为 Windows。 |
| 配置加载 | 未执行 | 当前环境为 Windows。 |
| Merge | 未执行 | 当前环境为 Windows。 |

### 压力测试

| 项目 | 状态 | 说明 |
| --- | --- | --- |
| 连续录制 50 次 | 未执行 | 需要 Unity Editor 运行 SDK。 |
| Start / Stop 高频调用 | 未执行 | 需要 Unity Editor 运行 SDK。 |
| Merge 队列 | 未执行 | 需要 Unity Editor 运行 SDK。 |
| SessionHistory 最大容量 | 未执行 | 需要 Unity Editor 运行 SDK。 |
| 临时文件清理 | 未执行 | 需要 Unity Editor 运行 SDK。 |
| FFmpeg 僵尸进程 | 部分通过 | 控制组未留下稳定可见的 ffmpeg 僵尸进程。 |

## 成功项

- FFmpeg 可执行文件存在于系统路径。
- FFmpeg MP4 / libx264 编码与落盘控制组通过。
- FFmpeg WebM / libvpx 编码与落盘控制组通过。
- FFmpeg RTMP 无服务端失败路径可复现。
- FFmpeg 控制组未留下稳定可见僵尸进程。
- 配置 JSON 当前符合 v0.3.0-beta 新版结构。

## 失败项

- Unity BatchMode 未能进入脚本编译阶段，License Client IPC 超时。
- FFmpegApp 目录已补入 `ffmpeg.exe`。
- 当前会话 `gdigrab` 桌面抓帧失败，错误码 error 5。
- 当前 FFmpeg 不支持 `wasapi`。

## 已知问题

1. 当前 SDK 默认 FFmpeg 路径已补齐。
   - 结果：`Assets/StreamingAssets/FFmpegTools/FFmpegApp/ffmpeg.exe` 已存在并能执行。
   - 仍需：Unity Editor 内确认 `RecorderPathService.GetPlatformDefaultFFmpegPath()` 能正确命中该路径。

2. 当前 FFmpeg 不支持 WASAPI。
   - 影响：Windows 系统音频相关功能无法通过 `wasapi` 验证。
   - 已处理：SDK 启动前检测 `wasapi`，缺失时返回明确错误。
   - 建议：使用支持 `wasapi` 的 FFmpeg 构建，或后续调整 Windows 音频策略为 dshow 可配置输入源。

3. 当前命令会话无法通过 `gdigrab` 抓屏。
   - 影响：无法在本会话中确认屏幕录制。
   - 建议：在真实交互式 Unity Editor 中运行 RecorderSdkSmokeTest 和手动录制验证。

4. Unity License Client IPC 超时。
   - 影响：BatchMode 编译和自动化测试无法推进。
   - 建议：先在 Unity Hub 中确认许可证登录与激活，再重跑 BatchMode。

## 崩溃日志

- 本次没有捕获到 Unity 崩溃日志。
- 本次没有捕获到 FFmpeg 崩溃日志。
- Unity BatchMode 日志路径：`Logs/UnityRuntimeValidation_Batchmode.log`

## 性能数据

本次只采集到 FFmpeg lavfi 编码控制组数据：

- 分辨率：1280x720
- 帧率：25 fps
- 编码：libx264 ultrafast
- 输出：MP4
- 输出文件：`runtime_lavfi_perf_10s.mp4`
- 输出大小：248921 bytes
- 采样内存峰值：约 166.05 MB
- 采样 CPU 累计值：约 2.359375 秒

未采集到真实桌面录制数据。

## CPU 占用

- 控制组 FFmpeg CPU 累计值约 2.359375 秒。
- 未采集 Unity 主进程 CPU。
- 未采集真实桌面录制 CPU。

## 内存增长

- 控制组 FFmpeg 工作集采样峰值约 166.05 MB。
- 未采集 Unity 主进程内存增长。
- 未执行长时间录制，因此没有长期内存趋势。

## 长时间录制结果

- 30 分钟录制：未执行。
- 原因：Unity BatchMode License 阻塞，当前会话 gdigrab 抓屏失败，无法进行真实 SDK 录制。

## 下一步实机验证清单

1. 在 Unity Hub 中修复 License Client IPC 问题。
2. 打开 `录制器_基础演示.unity`；需要完整配置 UI 时打开 `录制器_设置中心.unity`。
3. 挂载或执行 `RecorderSdkSmokeTest`。
4. 手动验证 MP4/WebM 本地录制。
5. 手动验证连续 Start / Stop。
6. 手动执行 30 分钟录制并记录 CPU/内存。
7. 准备 RTMP 服务端，验证推流成功、Stop、失败、重连、事件。
8. 更换支持 WASAPI 的 FFmpeg，验证 Windows 系统音频。
9. 在 Linux/X11 环境验证 x11grab、pulse、配置加载和 Merge。




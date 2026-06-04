# Recorder SDK UnityPackage 导入验证说明

## 包名称

`RecorderSDK_v0.3.0-beta.unitypackage`

这是 Unity 可导入的 `.unitypackage` 包，不是普通 zip。

当前生成位置：

```txt
RecorderSDK_v0.3.0-beta.unitypackage
```

## 包含目录

导入包包含：

- `Assets/Demos/RecorderSdk/`
  - `Demo/Scenes/录制器_屏幕录制.unity`
  - `Demo/Scenes/录制器_设置中心.unity`
  - `Demo/Scenes/录制器_旧版综合场景.unity`
  - 原始兼容场景
  - Recorder Core 脚本
  - UISettings 脚本
  - SmokeTest
  - SDK 使用示例
  - README
  - Release Notes
  - 阶段验收文档
  - 运行时验证文档
- `Assets/StreamingAssets/RecorderSDK/`
  - `Configs/`
  - `FFmpeg/`
  - `Videos/`

## 不包含目录

导入包不主动包含以下运行产物：

- Unity `Library/`
- Unity `Temp/`
- Unity `Logs/`
- Unity `Obj/`
- 用户本地录制生成的视频文件
- 运行验证过程中生成的临时输出文件

## 是否内置 FFmpeg

本包内置 FFmpeg：

- Windows：`Assets/StreamingAssets/RecorderSDK/FFmpeg/ffmpeg.exe`
- Linux：`Assets/StreamingAssets/RecorderSDK/FFmpeg/ffmpeg`

导入后默认模板中的 `customFFmpegPath` 为空，Recorder 会回退到当前平台默认路径：

- Windows：`Application.streamingAssetsPath/RecorderSDK/FFmpeg/ffmpeg.exe`
- Linux：`Application.streamingAssetsPath/RecorderSDK/FFmpeg/ffmpeg`

如果导入后不希望使用内置 FFmpeg，也可以在 UI 中选择自定义 FFmpeg 可执行文件路径。

## 导入新 Unity 项目的步骤

1. 打开目标 Unity 项目。
2. 菜单选择 `Assets > Import Package > Custom Package...`。
3. 选择 `RecorderSDK_v0.3.0-beta.unitypackage`。
4. 勾选全部导入项。
5. 点击 `Import`。
6. 等待 Unity 完成资源导入和脚本编译。

## 打开测试场景

导入后建议先打开 Demo 场景：

```txt
Assets/Demos/RecorderSdk/Demo/Scenes/录制器_屏幕录制.unity
```

`录制器_屏幕录制.unity` 用于验证 SDK 最小使用方式。场景中包含 `RecorderManager`，挂载 `CrossPlatformScreenRecorder` 和 `RecorderDemoBasicController`；UI 应提前存在于场景 Hierarchy 中，运行时脚本只负责绑定和业务逻辑，不默认动态创建 Demo UI。

如果导入后需要重建屏幕录制 Demo UI，打开目标场景后执行：

```txt
ByTools/Recorder SDK控制台
```

在控制台的 `UI 工具` 页点击 `创建屏幕录制 UI`。该按钮会一次性创建 Canvas、EventSystem、`开始录制`、`停止录制`、`打开输出目录`、`清空历史` 按钮，`当前状态`、`最近结果`、`当前配置ID`、`输出路径`、`最近错误码`、`会话数量` 文本，以及 Dropdown、Toggle、Slider，并自动绑定到 `RecorderDemoBasicController`。生成完成后请保存场景。

需要编辑完整配置时打开：

```txt
Assets/Demos/RecorderSdk/Demo/Scenes/录制器_设置中心.unity
```

`录制器_设置中心.unity` 用于展示完整配置 UI，可编辑配置、切换 `configId`、保存/另存为/删除配置、调整 FFmpeg、音频、推流和音量增益参数。

旧一体化场景保留为：

```txt
Assets/Demos/RecorderSdk/Demo/Scenes/录制器_旧版综合场景.unity
Assets/Demos/RecorderSdk/Demo/Scenes/录制视频Recorder.unity
Assets/Demos/RecorderSdk/Demo/Scenes/录制视频Init.unity
```

这些场景仅用于兼容和对照，新用户优先从 `录制器_屏幕录制.unity` 开始。

## 运行 RecorderSdkSmokeTest

`RecorderSdkSmokeTest` 路径：

```txt
Assets/Demos/RecorderSdk/Demo/Scripts/RecorderSdkSmokeTest.cs
```

运行方式：

1. 打开 `录制器_屏幕录制.unity` 或 `录制器_设置中心.unity`。
2. 在场景中创建一个空对象。
3. 挂载 `RecorderSdkSmokeTest`。
4. 绑定或确认场景中存在 `CrossPlatformScreenRecorder`。
5. 勾选 `runOnStart`，或在 Inspector 右键执行 `Run Smoke Test`。
6. 查看 Console 日志中的 `[PASS]` / `[FAIL]`。

重点检查：

- `StartRecordingAsync`
- `StopRecordingAsync`
- 非法 Stop 是否产生 Warning
- 连续 Start 是否返回 `AlreadyRunning`
- SessionHistory 是否写入
- ClearSessionHistory 是否生效

## 检查 MP4 / WebM 输出

默认输出目录：

```txt
Assets/StreamingAssets/RecorderSDK/Videos/
```

验证步骤：

1. 在 UI 中选择本地存储。
2. 选择 MP4 或 WebM。
3. 点击使用配置。
4. 开始录制。
5. 停止录制。
6. 检查 `Videos/` 下是否生成视频。
7. 使用播放器或 `ffprobe` 检查视频是否可播放。

Windows 本地录制需要当前环境允许 FFmpeg `gdigrab` 抓屏。

## 常见错误和处理方式

### FFmpegPathMissing

现象：

- 点击开始后提示 FFmpeg 不存在。

处理：

- 确认 `Assets/StreamingAssets/RecorderSDK/FFmpeg/ffmpeg.exe` 存在。
- 或在 UI 中选择自定义 FFmpeg 路径。

### AudioStartFailed

现象：

- Windows 推流开启系统音频时无法启动。

原因：

- 当前 FFmpeg 不支持 `wasapi` 输入。

处理：

- 关闭 `streamIncludeAudio`。
- 或更换支持 WASAPI 的 FFmpeg。
- 后续版本可扩展为 dshow 可配置输入源。

### DisplayNotFound

现象：

- 当前配置中的录制屏幕不存在。

处理：

- 打开配置 UI，重新选择当前机器的显示器。
- 保存或另存为配置。

### RTMP 推流失败

现象：

- 推流启动失败或快速断开。

处理：

- 确认推流地址以 `rtmp://` 或 `rtmps://` 开头。
- 确认 RTMP 服务端已启动。
- 确认防火墙允许连接。

### Unity License Client IPC 超时

现象：

- BatchMode 未进入脚本编译阶段。
- 日志出现 `IPC channel to LicensingClient doesn't exist`。

处理：

- 打开 Unity Hub。
- 确认账号已登录。
- 确认许可证已激活。
- 重启 Unity Hub 和 Unity Editor 后重试。





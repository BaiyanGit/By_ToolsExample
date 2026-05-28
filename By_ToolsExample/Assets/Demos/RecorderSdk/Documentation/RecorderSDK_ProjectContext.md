# Recorder SDK Project Context

本文档用于在新会话中快速恢复 Recorder SDK 当前上下文。它记录当前版本、目录结构、已完成阶段、验证状态、禁止修改范围和后续建议。

## 当前版本

- SDK 名称：Recorder SDK
- 当前版本：v0.3.0-beta
- 当前阶段：Beta 封版前验证与收口
- 当前日期：2026-05-27
- 当前正式 SDK 根目录：`Assets/Demos/RecorderSdk`
- StreamingAssets 固定路径：`Assets/StreamingAssets/FFmpegTools`
- 控制台入口：`ByTools/Recorder SDK控制台`

## 当前目录规则

正式 SDK 主路径以 `Assets/Demos/RecorderSdk` 为准。

不要再使用旧路径：

- `Assets/示例-录制视频Recorder/RecorderSdk`

StreamingAssets 路径保持固定：

- `Assets/StreamingAssets/FFmpegTools/Configs`
- `Assets/StreamingAssets/FFmpegTools/FFmpegApp`
- `Assets/StreamingAssets/FFmpegTools/Videos`

正式 unitypackage 检查清单应包含：

- `Assets/Demos/RecorderSdk/Core`
- `Assets/Demos/RecorderSdk/UI`
- `Assets/Demos/RecorderSdk/Editor`
- `Assets/Demos/RecorderSdk/Demo`
- `Assets/Demos/RecorderSdk/Documentation`
- `Assets/Demos/RecorderSdk/Plugins`
- `Assets/StreamingAssets/FFmpegTools/Configs`
- `Assets/StreamingAssets/FFmpegTools/FFmpegApp`
- `Assets/StreamingAssets/FFmpegTools/Videos/README.md`

正式 unitypackage 不应包含：

- `Assets/StreamingAssets/FFmpegTools/Videos` 下的实际 mp4、webm 或临时录制文件
- `Assets/示例-录制视频Recorder/RecorderSdk`
- `Library`
- `Temp`
- `obj`
- `.vs`
- 临时打包目录

## 推荐入口

新用户推荐先打开：

- `Assets/Demos/RecorderSdk/Demo/Scenes/录制器_屏幕录制.unity`

配置管理推荐打开：

- `Assets/Demos/RecorderSdk/Demo/Scenes/录制器_设置中心.unity`

Editor 工具统一入口：

- `ByTools/Recorder SDK控制台`

旧快捷菜单入口已经隐藏，功能保留在 SDK 控制台中。

## 当前控制台状态

`RecorderSdkHubWindow` 已完成以下收口：

- 首页改为 SDK 工作台式纵向阅读流。
- 首页顺序为：当前环境状态、快速开始、常用工具、最近状态、最近更新。
- 左侧导航已减重，使用细条表示当前选中页。
- UI 工具页使用清晰的创建和布局管理区域。
- 文档中心使用中文分类，点击文档时懒加载并缓存。
- 更新日志页默认显示摘要文档。
- 验证记录页默认显示验证摘要，并可切换完整运行时验证和 UnityPackage 验证。
- API 参数页内置 FFmpeg 参数和视频编码参数，不再使用旧分页系统。
- SDK API 页左侧分类已中文化，右侧内容改为开发者文档式单页滚动。
- 打包发布页只负责检查清单和必要文件，不再自动导出 unitypackage。

控制台已修复的问题：

- 更新日志页面卡死风险。
- 文档中心和验证记录滚动不到底。
- API 参数页 `DrawPagination()` 触发 Unity IMGUI Assertion。
- SDK API 右侧内容无法滚动。
- SDK API 分类标题英文暴露和标题重复。
- `ProjectStructure.md` 乱码。

## 当前 SDK 稳定性阶段

已完成阶段：

1. 状态与错误稳定
   - `RecorderState`
   - `RecorderErrorCode`
   - `RecorderResult`
   - 状态流转验收文档

2. 事件流与 SessionHistory 稳定
   - `OnRecorderStateChanged`
   - `OnRecorderWarning`
   - `OnRecorderError`
   - `OnRecorderProgress`
   - `OnRecordStarted`
   - `OnRecordStopped`
   - `OnMergeStarted`
   - `OnMergeCompleted`
   - `SessionHistory`

3. 配置管理稳定
   - `RecorderConfigRegistry`
   - `configId` 配置体系
   - `displayName`
   - `captureDisplayName`
   - `UseWinRecordConfig.json`
   - `UseLinuxRecordConfig.json`
   - 旧配置迁移
   - 配置合法性自动修复

4. 控制台和文档收口
   - `RecorderSdkHubWindow`
   - `ProjectStructure.md`
   - `RecorderSdkAcceptanceSummary.md`
   - `RecorderSdkChangeLogSummary.md`
   - `RecorderSdkValidationSummary.md`
   - `ReleaseNotes_v0.3.0-beta.md`

## 当前公开 API 重点

Recorder 入口类：

- `CrossPlatformScreenRecorder`

常用方法：

- `StartRecordingAsync()`
- `StopRecordingAsync()`
- `StartRecording()`
- `StopRecording()`
- `GetSessionHistory()`
- `ClearSessionHistory()`

常用结果类型：

- `RecorderResult`
- `RecorderEventArgs`
- `RecorderSessionInfo`

常用配置类型：

- `RecorderParamsConfig`
- `RecorderConfigRegistry`
- `RecordConfigProvider`

命令构建：

- `FFmpegCommandBuilder`
- `FFmpegCommand`

## 当前 JSON 配置规范

新版配置使用：

- `schemaVersion = 2`
- `configId`
- `displayName`
- `captureDisplayName`

新版当前配置引用使用：

- `currentConfigId`

新版配置不再写入：

- `configName`
- `fileName`

Legacy 兼容仍需保留：

- 旧 `configName`
- 旧 `fileName`
- 旧 `Creat_` 前缀

新建用户配置应使用：

- `Create_` 兼容逻辑已保留，但当前主要身份应以 `configId` 为准。

## 当前音量增强状态

已增加录制音量增强配置：

- `enableAudioGain`
- `audioGainDb`
- `audioLimiterEnabled`

行为：

- 未启用音频增强时不拼接 `volume` filter。
- 启用时使用 `volume=XdB`。
- limiter 开启时追加 `alimiter=limit=0.95`。
- 如果 `alimiter` 不可用，应降级并给 Warning。

常用建议：

- 3dB：轻微增大
- 6dB：明显增大
- 9dB：较大
- 12dB：很大，可能失真

## 当前验证状态

已完成：

- 控制台显示效果验证通过。
- 首页信息架构验证通过。
- UI 工具页显示验证通过。
- 文档中心滚动验证通过。
- 更新日志滚动验证通过。
- 验证记录滚动验证通过。
- SDK API 分类和滚动验证通过。
- API 参数页 IMGUI Assertion 已修复。
- `ProjectStructure.md` 中文显示修复。

未完成：

- Windows 实机 MP4 录制验证。
- Windows 实机 WebM 录制验证。
- 连续 Start / Stop 压力验证。
- 长时间录制验证。
- RTMP / RTMPS 推流验证。
- Windows WASAPI 实机音频验证。
- Linux x11grab / pulse 实机验证。
- FFmpeg 崩溃恢复验证。
- UnityPackage 手动导出和新项目导入验证。

环境限制记录：

- 自动启动 Unity Editor / BatchMode 曾受 GUI 权限或 License Client IPC 影响，不作为 SDK 代码阻塞。
- 后续 unitypackage 由用户在 Unity Editor 中手动导出。
- Codex 只负责检查路径、清单、必要文件和文档。

## 当前已知风险

- Runtime 真机验证尚未开始。
- 长时间录制和多轮 Start / Stop 压力测试尚未完成。
- 推流功能需要真实 RTMP / RTMPS 环境验证。
- Windows 系统音频依赖 WASAPI 能力和原生 DLL。
- Linux 录制能力依赖具体桌面环境、x11grab、pulse 或系统 FFmpeg 编译能力。
- 历史归档文档中可能仍存在编码异常，之前已决定暂不处理。
- `UIRecorderParamsSettings.RuntimeUI.cs` 中仍保留 Legacy 动态 UI 创建逻辑，默认不作为新工作流入口。

## 禁止随意修改范围

除非用户明确要求，不要修改：

- `CrossPlatformScreenRecorder`
- `RecorderState`
- `RecorderErrorCode`
- `RecorderResult`
- `RecorderConfigRegistry`
- `FFmpegCommandBuilder`
- `SessionHistory`
- 音量增强 Runtime 逻辑
- JSON 配置结构
- 实际录制流程

控制台显示问题只允许优先修改：

- `Assets/Demos/RecorderSdk/Editor/RecorderSdkHubWindow.cs`
- Editor 参数说明窗口
- Editor 布局工具
- Documentation 文档

## 后续建议阶段

建议下一阶段按以下顺序推进：

1. UnityPackage 手动导出前检查
   - 检查 `WASAPILoopbackRecorder.dll`
   - 检查 `Configs`
   - 检查 `FFmpegApp`
   - 检查 `Videos` 是否只包含 README
   - 检查导出清单不含旧路径

2. Windows 实机录制验证
   - MP4 本地录制
   - WebM 本地录制
   - 连续 Start / Stop 10 次
   - SessionHistory
   - 非法 Start / Stop Warning
   - Merge

3. Windows 音频验证
   - WASAPI 支持检测
   - 无音频设备
   - 音频设备异常
   - `AudioStartFailed`

4. 推流验证
   - RTMP
   - RTMPS
   - Stop during stream
   - 自动重连
   - `streamIncludeAudio`

5. Linux 验证
   - x11grab
   - pulse
   - 配置加载
   - Merge

6. 压力测试
   - 连续录制 50 次
   - 高频 Start / Stop
   - Merge 队列
   - SessionHistory 最大容量
   - 临时文件清理
   - FFmpeg 僵尸进程

## 新会话接入提示

新会话开始后建议先读取：

1. `Assets/Demos/RecorderSdk/Documentation/RecorderSDK_ProjectContext.md`
2. `Assets/Demos/RecorderSdk/Documentation/README.md`
3. `Assets/Demos/RecorderSdk/Documentation/ProjectStructure.md`
4. `Assets/Demos/RecorderSdk/Documentation/Changelog/RecorderSettingsChangeLog.md`
5. `Assets/Demos/RecorderSdk/Editor/RecorderSdkHubWindow.cs`

新会话默认规则：

- 先确认任务是否允许修改 Runtime。
- 如果只是控制台显示问题，只改 Editor。
- 如果是录制行为问题，再读 Core。
- 不要恢复旧路径。
- 不要恢复旧带图标菜单。
- 不要自动导出 unitypackage。
- 不要自动启动 Unity Editor，除非用户明确允许。


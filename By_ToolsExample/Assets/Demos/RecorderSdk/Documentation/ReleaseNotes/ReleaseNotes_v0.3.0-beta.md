# Release Notes - Recorder SDK v0.3.0-beta

## 版本定位

`Recorder SDK v0.3.0-beta` 是 Recorder SDK 的稳定性基线版本。

本版本目标不是继续扩展新功能，而是把录制器的核心调用链整理到可被外部代码稳定接入的状态：

- 调用方能知道当前处于什么状态。
- 调用方能拿到明确错误码。
- 调用方能订阅统一事件流。
- 调用方能查询 SessionHistory。
- 配置系统以 `configId` 作为稳定身份。

## 新增能力

- 新增 `RecorderState` 状态机。
- 新增 `RecorderErrorCode` 错误码体系。
- 新增 `RecorderResult`，`StartRecordingAsync()` / `StopRecordingAsync()` 可 await。
- 新增统一事件参数 `RecorderEventArgs`。
- 新增 `OnRecorderStateChanged`、`OnRecorderWarning`、`OnRecorderError`、`OnRecorderProgress`、`OnMergeStarted`、`OnMergeCompleted`。
- 新增 `SessionHistory`、`GetSessionHistory()`、`ClearSessionHistory()`。
- 新增 `RecorderConfigRegistry`。
- 新增 `RecorderConfigResult`。
- 新增 `configId` 配置体系。
- 新增配置迁移与 AutoFix warnings。
- 新增 `FFmpegCommandBuilder`，将 FFmpeg 参数构建从 Recorder 主流程中拆出。
- 新增 `RecorderSdkSmokeTest.cs`。
- 新增 `RecorderSdkUsageExample.cs`。
- 新增阶段验收文档和总体验收文档。

## Breaking Changes

- 新版配置主身份改为 `configId`。
- UI 显示名称改为 `displayName`。
- 显示器名称改为 `captureDisplayName`。
- 新版配置 JSON 不再写入 `configName`。
- 新版配置 JSON 不再写入 `fileName`。
- `UseWinRecordConfig.json` / `UseLinuxRecordConfig.json` 新版只保存 `schemaVersion`、`platform`、`currentConfigId`。
- 新生成用户配置文件使用 `create_` 风格的 `configId.json` 文件名。

## 兼容旧配置说明

以下字段和命名均为 Legacy 兼容内容，只用于读取历史配置，不代表新版配置仍然使用这些字段作为主身份。

仍兼容读取旧配置：

- `schemaVersion < 2` 会按 Legacy 配置处理。
- Legacy `configName` 会迁移到 `displayName`。
- Legacy `displayName` 会迁移到 `captureDisplayName`。
- Legacy `fileName` 只用于推导 `configId`。
- Legacy `Creat_` 前缀仍可读取。
- Legacy Use 指针中的 `fileName` 仍可回退解析。

保存新版配置时会统一升级：

- `schemaVersion = 2`
- 写入 `configId`
- 写入 `displayName`
- 不写入 `configName`
- 不写入 `fileName`

## 已知限制

- 尚未在真实 Unity Editor 内完成一次完整编译验证。
- 尚未做 Windows 实机 FFmpeg 录制验证。
- 尚未做 Linux 实机 FFmpeg 录制验证。
- 尚未做 RTMP / RTMPS 推流实机稳定性验证。
- 尚未做长时间录制压力测试。
- 尚未做多次 Start / Stop 压力测试。
- Unity BatchMode 当前在本机因 License Client IPC 超时未进入脚本编译阶段。
- `dotnet build Assembly-CSharp.csproj` 不作为 Unity 工程真实编译入口。
- BackendFactory 暂未实现。
- Profile 导入导出暂未实现。

## 后续计划

- 在真实 Unity Editor 中完成编译验证。
- 在 Windows 实机验证本地录制和推流。
- 在 Linux 实机验证本地录制和推流。
- 增加长时间录制压力测试。
- 增加多次 Start / Stop 压力测试。
- 完善 Unity Test Framework 自动化测试。
- 后续再进入 BackendFactory。
- 后续再进入 Profile 克隆、导出、导入能力。

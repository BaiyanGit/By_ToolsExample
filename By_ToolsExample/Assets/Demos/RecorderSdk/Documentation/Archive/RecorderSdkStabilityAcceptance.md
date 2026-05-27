# Recorder SDK 稳定性总体验收说明

## 1. 当前 SDK 架构总览

### Core

`RecorderSdk/Core` 是 Recorder SDK 的核心目录，当前包含录制入口、命令构建、状态机、错误码、事件、SessionHistory 和配置校验；SmokeTest 位于 `RecorderSdk/Demo/Scripts`。

主要入口：

- `CrossPlatformScreenRecorder`：Unity 层录制入口，负责开始录制、停止录制、状态流转、事件发布和 SessionHistory。
- `RecordConfigProvider`：录制前读取当前平台正在使用的配置。
- `RecorderConfigValidator`：录制前校验配置。
- `RecorderPathService`：输出路径与默认目录处理。

### ConfigRegistry

`RecorderConfigRegistry` 负责配置管理，不启动录制、不拼 FFmpeg、不修改 `RecorderState`。

职责：

- 扫描配置目录。
- 按 `configId` 建立索引。
- 读取和设置当前配置。
- 创建、克隆、删除用户配置。
- 重命名 `displayName`。
- 保存配置并升级到 `schemaVersion = 2`。
- 旧配置迁移和 AutoFix warnings。
- 当前配置丢失时回退平台 Medium 模板。

### CommandBuilder

`FFmpegCommandBuilder` 只负责构建 FFmpeg 命令，不负责进程执行和状态修改。

职责：

- Windows 本地录制命令。
- Windows 推流命令。
- Linux 本地录制命令。
- Linux 推流命令。
- 音视频合并命令。

返回 `FFmpegCommand`，包含：

- `executablePath`
- `arguments`
- `rawCommandLine`

### StateMachine

`RecorderState` 描述主流程状态：

- `Uninitialized`
- `Ready`
- `Starting`
- `Recording`
- `Stopping`
- `Merging`
- `Completed`
- `Error`

`RecorderStateTransition` 负责合法流转判断，非法流转返回 `RecorderErrorCode.StateTransitionInvalid` 并触发 warning。

### Event System

新事件统一使用 `RecorderEventArgs`，包含：

- `sessionId`
- `state`
- `outputPath`
- `message`
- `eventType`
- `errorCode`
- `errorCodeText`
- `exception`

旧事件 `OnRecordStarted`、`OnRecordStopped`、`OnRecordingStateChanged` 保留兼容，但由新流程统一驱动。

### SessionHistory

`CrossPlatformScreenRecorder` 维护 SessionHistory，用于记录开始、停止、合并、推流、错误和警告等会话历史。

公开接口：

- `GetSessionHistory()`
- `ClearSessionHistory()`
- `SessionHistory`
- `CurrentSession`
- `LastSession`

### SmokeTest

`RecorderSdkSmokeTest` 是 Unity 内可挂载运行的最小测试脚本，覆盖状态、事件、SessionHistory 和配置管理行为。

## 2. 三个阶段完成内容

### Phase1：状态与错误稳定

- 新增 `RecorderState`。
- 新增 `RecorderErrorCode`。
- 新增 `RecorderResult`。
- 新增 `RecorderStateTransition`。
- `StartRecordingAsync()` / `StopRecordingAsync()` 可 await。
- `StartRecording()` / `StopRecording()` 保留为 Unity Button 兼容入口。
- 明确非法 Start / Stop / 状态流转的错误码。
- 删除 `GetPlatformDefaultFFmpegPath()` 中错误的 `Application.Quit()`。

### Phase2：事件流与 SessionHistory 稳定

- 新增统一 `RecorderEventArgs`。
- 新增 `RecorderSessionEventType`。
- 新增 `OnRecorderProgress`。
- 统一 Warning / Error 事件来源。
- 错误事件和 Warning 均携带 `RecorderErrorCode`。
- 后台 Merge 完成时，如果已有新录制，不覆盖全局状态。
- SessionHistory 记录 Start / Stop / Merge / Stream / Error / Warning。
- 提供 `GetSessionHistory()` 和 `ClearSessionHistory()`。

### Phase3：配置管理稳定

- 新增 `RecorderConfigRegistry`。
- 新增 `RecorderConfigResult`。
- 配置主身份统一为 `configId`。
- UI 显示名统一为 `displayName`。
- 显示器名称统一为 `captureDisplayName`。
- 新配置保存时不再写 `configName` / `fileName`。
- `UseWinRecordConfig.json` / `UseLinuxRecordConfig.json` 只保存 `schemaVersion`、`platform`、`currentConfigId`。
- 旧 `Creat_`、旧 `fileName` 指向、旧 `configName` 字段保持兼容读取。
- AutoFix 统一返回 warnings。

## 3. 当前公开 API 列表

### CrossPlatformScreenRecorder

- `Task<RecorderResult> StartRecordingAsync()`
  开始录制，调用方可 await，返回成功状态、错误码、输出路径和 sessionId。

- `Task<RecorderResult> StopRecordingAsync()`
  停止录制，调用方可 await，返回成功状态、错误码、输出路径和 sessionId。

- `void StartRecording()`
  Unity Button 兼容入口，内部调用 `StartRecordingAsync()`。

- `void StopRecording()`
  Unity Button 兼容入口，内部调用 `StopRecordingAsync()`。

- `IReadOnlyList<RecorderSessionInfo> GetSessionHistory()`
  获取只读 SessionHistory。

- `void ClearSessionHistory()`
  清空 SessionHistory。

- `RecorderState State`
  当前录制状态。

- `RecorderSessionInfo CurrentSession`
  当前录制或后台处理中会话信息。

- `RecorderSessionInfo LastSession`
  最近一次会话信息。

### RecorderConfigRegistry

- `RecorderConfigResult Scan()`
  扫描配置目录并建立索引。

- `RecorderConfigResult GetCurrentConfig()`
  获取当前平台正在使用的配置。

- `RecorderConfigResult SetCurrentConfig(string configId)`
  切换当前配置。

- `RecorderConfigResult CreateUserConfig(string displayName)`
  创建用户配置。

- `RecorderConfigResult CloneConfig(string sourceConfigId, string displayName)`
  克隆模板或用户配置为新的用户配置。

- `RecorderConfigResult SaveConfig(RecorderParamsConfig config)`
  保存配置，自动升级到新版结构。

- `RecorderConfigResult DeleteUserConfig(string configId)`
  删除用户配置，禁止删除模板。

- `RecorderConfigResult DeleteConfig(string configId)`
  删除配置的对外兼容入口，内部仍禁止删除模板。

- `RecorderConfigResult RenameDisplayName(string configId, string displayName)`
  修改配置显示名称。

- `RecorderConfigResult FallbackToDefaultTemplate()`
  回退当前平台 Medium 模板。

## 4. 当前事件列表

- `OnRecorderStateChanged`
  状态变化事件，状态切换后触发。

- `OnRecorderWarning`
  警告事件，用于非法状态调用、非法状态流转、非致命配置或流程问题。

- `OnRecorderError`
  错误事件，用于 Start / Stop / Merge / FFmpeg / 配置校验等失败场景。

- `OnRecorderProgress`
  业务进度事件，用于 StartSucceeded、StopRequested、StopSucceeded、MergeStarted、MergeSucceeded、StreamEnded 等流程节点。

- `OnRecordStarted`
  旧兼容事件，开始录制成功后触发，只传输出路径。

- `OnRecordStopped`
  旧兼容事件，停止录制成功后触发，只传输出路径。

- `OnMergeStarted`
  后台合并开始事件。

- `OnMergeCompleted`
  后台合并完成事件，成功和失败均通过 `RecorderEventArgs.errorCode` 判断。

- `OnRecordingStateChanged`
  旧兼容事件，传 `bool IsRecording`。

- `OnConfigChanged`
  配置变化事件，当前预留给配置切换或后续配置刷新流程。

## 5. 当前 JSON 配置规范

新版配置文件统一使用 `schemaVersion = 2`。

核心身份字段：

- `schemaVersion`
  配置结构版本，当前为 2。

- `configId`
  稳定唯一 ID，用于内部引用和文件名推导，只允许英文小写、数字和下划线。

- `displayName`
  UI 显示名称，可以中文，可以修改，不参与文件路径。

- `captureDisplayName`
  录制显示器名称，例如 `\\.\DISPLAY1`。

- `currentConfigId`
  UseWin / UseLinux 当前使用配置指针。

不再使用：

- `configName`
  只作为旧配置兼容读取，不写入新版 JSON。

- `fileName`
  只作为运行时路径缓存或旧配置兼容，不写入新版 JSON。

Use 指针格式：

```json
{
  "schemaVersion": 2,
  "platform": "Win",
  "currentConfigId": "template_win_medium"
}
```

配置字段顺序遵循：

身份信息 -> 平台模式 -> 输出 -> 画面 -> 视频编码 -> WebM 视频 -> 音频 -> 推流 -> FFmpeg -> 高级参数。

## 6. 当前错误码列表

- `None`
  无错误。

- `ConfigNull`
  配置为空。

- `ConfigInvalid`
  配置校验失败。

- `ConfigNotFound`
  指定 `configId` 不存在。

- `ConfigDuplicateId`
  扫描或保存时发现重复 `configId`。

- `ConfigMigrationFailed`
  旧配置迁移失败。

- `ConfigSaveFailed`
  配置保存失败。

- `ConfigDeleteFailed`
  配置删除失败，或尝试删除模板配置。

- `ConfigInvalidId`
  `configId` 非法。

- `ConfigAutoFixed`
  配置被 AutoFix 修复，通常作为 warning。

- `ConfigDirectoryMissing`
  配置目录不存在且创建失败，或扫描失败。

- `InitializeFailed`
  初始化失败。

- `AlreadyRunning`
  当前已在启动、录制、停止或 FFmpeg 进程运行中。

- `NotRecording`
  未处于录制状态却调用 Stop。

- `StateBusy`
  正在启动或停止，暂不能重复调用。

- `StateTransitionInvalid`
  状态机非法流转。

- `FFmpegPathMissing`
  FFmpeg 路径为空或文件不存在。

- `FFmpegStartFailed`
  FFmpeg 进程启动失败。

- `AudioStartFailed`
  音频采集启动失败。

- `AudioStopFailed`
  音频采集停止失败。

- `DisplayNotFound`
  配置中的显示器索引或名称不可用。

- `StreamUrlEmpty`
  推流模式下推流地址为空。

- `StreamUrlInvalid`
  推流地址格式非法，当前只支持 RTMP / RTMPS。

- `StopFailed`
  停止采集或停止 FFmpeg 失败。

- `MergeFailed`
  音视频合并失败。

- `TempFileNotReady`
  临时文件未在超时时间内准备好。

- `Unknown`
  未分类错误。

## 7. 当前 SessionHistory 字段说明

- `sessionId`
  本次录制会话唯一 ID。

- `startTime`
  兼容字段，表示会话开始时间。

- `startedAt`
  会话开始时间。

- `endedAt`
  当前历史事件结束时间或记录时间。

- `durationMs`
  从开始到当前历史节点的持续毫秒数。

- `outputPath`
  输出文件路径或推流地址相关输出路径。

- `videoTempPath`
  视频临时文件路径。

- `audioTempPath`
  音频临时文件路径。

- `isStreaming`
  是否为推流模式。

- `isMerged`
  是否已经完成音视频合并。

- `mergeOutputPath`
  合并完成后的输出路径。

- `state`
  记录该历史时的 RecorderState。

- `eventType`
  记录该历史时的业务事件类型。

- `errorCode`
  错误码或 warning 码。

- `errorCodeText`
  错误码字符串。

- `message`
  历史说明文本。

- `configId`
  本次录制使用的配置 ID。

- `configName`
  兼容字段，当前使用配置显示名称。

- `displayName`
  本次录制使用的配置显示名称。

## 8. 当前验证状态

### rg 检查通过项

- Core 下存在 `StartRecordingAsync`、`StopRecordingAsync`、`RecorderResult`、`RecorderState`、`RecorderErrorCode`。
- Core 下存在统一事件 `OnRecorderStateChanged`、`OnRecorderWarning`、`OnRecorderError`、`OnRecorderProgress`、`OnMergeStarted`、`OnMergeCompleted`。
- Core 下存在 `GetSessionHistory()` 和 `ClearSessionHistory()`。
- Core 下存在 `RecorderConfigRegistry`、`RecorderConfigResult`、`RecorderConfigMigrator`。
- `Assets/StreamingAssets/FFmpegTools/Configs` 下 JSON 已无 `configName` / `fileName` / 本机绝对路径残留。
- `UseWinRecordConfig.json` / `UseLinuxRecordConfig.json` 使用 `currentConfigId`。

### git diff --check

`git diff --check` 已通过，仅有 Git 提示未来可能将 LF 替换为 CRLF，没有空白错误。

### dotnet build 说明

`dotnet build Assembly-CSharp.csproj --no-restore` 不作为 Unity 项目的真实编译入口。

当前工程的 `Assembly-CSharp.csproj` 属于 Unity / Rider 设计期工程，曾出现：

- 生成失败
- 0 个警告
- 0 个错误

因此不再把该命令作为验收阻塞项。

### Unity BatchMode 说明

Unity BatchMode 编译命令已记录到 README。当前机器执行时因 License Client IPC 超时，未进入脚本编译阶段，属于环境限制。

## 9. 当前已知风险

- 还没在真实 Unity Editor 内完成一次完整脚本编译验证。
- 还没做 Windows 实机 FFmpeg 录制验证。
- 还没做 Linux 实机验证。
- 还没做长时间录制压力测试。
- 还没做多次 Start / Stop 压力测试。
- 还没做推流 RTMP / RTMPS 实机稳定性验证。
- BackendFactory 暂未做。
- Profile 导入导出暂未做。
- Backend 插件化接口已有基础文件，但当前阶段没有替换现有后端实现。

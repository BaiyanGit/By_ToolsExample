# Recorder SDK 第 2 阶段验收说明

## 阶段目标

第 2 阶段只做事件流稳定与 SessionHistory 完善，不做：

- ConfigRegistry
- BackendFactory
- Profile 导入导出
- 大规模服务层拆分

目标是让 SDK 调用方可以稳定监听事件、追踪会话、定位问题。

## 本阶段改动

- 新增 `RecorderSessionEventType`，用于标记历史和进度事件。
- 新增 `OnRecorderProgress`，承载 Start/Stop/Merge/Stream 业务里程碑。
- `RecorderEventArgs` 新增 `eventType`。
- `RecorderSessionInfo` 扩展为会话历史条目，包含错误码、事件类型、开始/结束时间、耗时、合并状态等字段。
- `CrossPlatformScreenRecorder` 新增 `GetSessionHistory()` 和 `ClearSessionHistory()`。
- Warning/Error 事件统一携带 `RecorderErrorCode`。
- 非法状态调用、配置失败、启动失败、停止失败、合并失败都会写入历史。
- 后台 Merge 完成/失败时，如果当前已有新录制，不覆盖全局 `State`，但写入旧 session 的历史。

## 事件列表

新事件：

- `OnRecorderStateChanged`
- `OnRecorderWarning`
- `OnRecorderError`
- `OnRecorderProgress`
- `OnMergeStarted`
- `OnMergeCompleted`
- `OnConfigChanged`

旧兼容事件：

- `OnRecordingStateChanged`
- `OnRecordStarted`
- `OnRecordStopped`

说明：

- 新事件都使用 `RecorderEventArgs`，包含 `sessionId`。
- `OnRecorderWarning` 和 `OnRecorderError` 必须包含具体 `RecorderErrorCode`。
- 旧兼容事件只保留旧签名，仍由统一发布函数驱动；需要 sessionId 的调用方应订阅 `OnRecorderProgress` 或 `OnMergeCompleted`。

## 事件触发顺序

### Start 成功

1. `OnConfigChanged`
2. 可选：`OnRecorderStateChanged(Ready)` -> `OnRecordingStateChanged(false)`
3. `OnRecorderStateChanged(Starting)` -> `OnRecordingStateChanged(true)`
4. `OnRecorderStateChanged(Recording)` -> `OnRecordingStateChanged(true)`
5. `OnRecorderProgress(StartSucceeded)`
6. `OnRecordStarted(outputPath)`
7. 返回 `RecorderResult.success = true`

历史记录：

- `StartSucceeded`

### Start 失败

配置或非法状态失败：

1. `OnConfigChanged`
2. 可选：`OnRecorderStateChanged(Error)` -> `OnRecordingStateChanged(false)`
3. `OnRecorderError`
4. 返回 `RecorderResult.success = false`

历史记录：

- `StartFailed`
- `ErrorOccurred`

启动 FFmpeg/音频失败：

1. `OnConfigChanged`
2. `OnRecorderStateChanged(Starting)` -> `OnRecordingStateChanged(true)`
3. `OnRecorderStateChanged(Error)` -> `OnRecordingStateChanged(false)`
4. `OnRecorderError`
5. 返回 `RecorderResult.success = false`

历史记录：

- `StartFailed`
- `ErrorOccurred`

### Stop 成功

1. `OnRecorderStateChanged(Stopping)` -> `OnRecordingStateChanged(true)`
2. `OnRecorderProgress(StopRequested)`
3. 采集停止成功
4. `OnRecorderProgress(StopSucceeded)`
5. `OnRecorderStateChanged(Merging)` -> `OnRecordingStateChanged(false)`
6. `OnRecorderProgress(MergeStarted)`
7. `OnMergeStarted`
8. 返回 `RecorderResult.success = true`

历史记录：

- `StopRequested`
- `StopSucceeded`
- `MergeStarted`

### Stop 失败

未录制时 Stop：

1. `OnRecorderWarning`
2. 返回 `RecorderResult.success = false`
3. `RecorderResult.errorCode = NotRecording`

历史记录：

- `StopFailed`
- `WarningOccurred`

停止采集异常：

1. `OnRecorderStateChanged(Stopping)` -> `OnRecordingStateChanged(true)`
2. `OnRecorderProgress(StopRequested)`
3. `OnRecorderStateChanged(Error)` -> `OnRecordingStateChanged(false)`
4. `OnRecorderError`
5. 返回 `RecorderResult.success = false`

历史记录：

- `StopRequested`
- `StopFailed`
- `ErrorOccurred`

### Merge 成功

1. 后台线程完成处理，回到主线程
2. 如果当前没有新录制：`OnRecorderStateChanged(Completed)` -> `OnRecordingStateChanged(false)`
3. `OnRecorderProgress(MergeSucceeded)`
4. `OnMergeCompleted`
5. `OnRecordStopped(outputPath)`

历史记录：

- `MergeSucceeded`

如果后台合并期间已经开始新录制：

- 不触发旧 session 的全局 `Completed/Error` 状态覆盖。
- 仍触发 `OnRecorderProgress(MergeSucceeded)`、`OnMergeCompleted`、`OnRecordStopped`。
- 仍写入旧 session 的 `MergeSucceeded` 历史。

### Merge 失败

1. 后台线程失败，回到主线程
2. 如果当前没有新录制：`OnRecorderStateChanged(Error)` -> `OnRecordingStateChanged(false)`
3. `OnRecorderError`

历史记录：

- `MergeFailed`
- `ErrorOccurred`

如果后台合并期间已经开始新录制：

- 不覆盖当前新录制状态。
- 仍写入旧 session 的 `MergeFailed` 和 `ErrorOccurred`。

### Stream 结束

1. 推流停止后回到主线程
2. 如果当前没有新录制：`OnRecorderStateChanged(Completed)` -> `OnRecordingStateChanged(false)`
3. `OnRecorderProgress(StreamEnded)`
4. `OnMergeCompleted`
5. `OnRecordStopped(streamUrl)`

历史记录：

- `StreamEnded`

## SessionHistory 字段说明

`GetSessionHistory()` 返回 `IReadOnlyList<RecorderSessionInfo>`。

字段：

- `sessionId`：会话或操作 ID。
- `configId`：配置 ID。
- `configName`：配置显示名。
- `displayName`：显示器名称。
- `state`：记录该事件时的状态。
- `eventType`：历史事件类型。
- `errorCode` / `errorCodeText`：错误或警告原因。
- `message`：事件说明。
- `outputPath`：输出文件路径或推流地址。
- `startedAt` / `startTime`：会话开始时间。
- `endedAt`：该历史事件写入时间。
- `durationMs`：从会话开始到该历史事件的耗时。
- `isStreaming`：是否推流。
- `isMerged`：是否已经完成本地合并。
- `mergeOutputPath`：合并输出路径。

## SDK 调用示例

```csharp
CrossPlatformScreenRecorder.ins.OnRecorderProgress += args =>
{
    Debug.Log($"{args.sessionId} {args.eventType} {args.message}");
};

IReadOnlyList<RecorderSessionInfo> history = CrossPlatformScreenRecorder.ins.GetSessionHistory();
CrossPlatformScreenRecorder.ins.ClearSessionHistory();
```

## 已验证

- `RecorderSessionEventType` 已存在。
- `OnRecorderProgress` 已接入 Start/Stop/Merge/Stream 里程碑。
- Warning/Error 事件统一使用 `RecorderErrorCode`。
- `GetSessionHistory()` 和 `ClearSessionHistory()` 已提供。
- `maxSessionHistoryCount` 在统一历史写入方法中生效。
- 后台 Merge 完成/失败不会覆盖新录制状态。
- README 已说明事件和历史读取方式。
- `RecorderSdkSmokeTest` 已扩展为可记录事件和历史检查的 MonoBehaviour。

## 仍需 Unity 真机验证

- 真实 FFmpeg Start/Stop/Merge 的完整事件顺序。
- Windows WASAPI 音频录制和推流带音频。
- Linux X11/PulseAudio 录制和推流。
- 多次快速 Start/Stop 下的事件顺序。
- Unity Editor Inspector 中手动运行 `RecorderSdkSmokeTest` 的结果。

## 环境限制

- 当前不再使用 `dotnet build Assembly-CSharp.csproj` 作为 Unity 编译验收入口。
- Unity BatchMode 是推荐验证方式。
- 当前机器 BatchMode 因 License Client IPC 超时返回 199，未进入脚本编译阶段，属于环境问题。
